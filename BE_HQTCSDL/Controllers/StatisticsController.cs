using System.Data;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace BE_HQTCSDL.Controllers
{
    [ApiController]
    [Route("api/admin/statistics")]
    [Authorize(Roles = "ADMIN")]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public StatisticsController(ApplicationDbContext db)
        {
            _db = db;
        }

     
        [HttpGet("revenue")]
        public async Task<ActionResult<RevenueStatsDto>> GetRevenueStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SP_GET_REVENUE_STATS";
                command.CommandType = CommandType.StoredProcedure;

                var pStartDate = new OracleParameter("p_start_date", OracleDbType.Date) { Value = start };
                var pEndDate = new OracleParameter("p_end_date", OracleDbType.Date) { Value = end };
                var pTotalRevenue = new OracleParameter("p_total_revenue", OracleDbType.Decimal) { Direction = ParameterDirection.Output };
                var pTotalOrders = new OracleParameter("p_total_orders", OracleDbType.Int32) { Direction = ParameterDirection.Output };
                var pTotalItems = new OracleParameter("p_total_items", OracleDbType.Int32) { Direction = ParameterDirection.Output };
                var pAvgOrderValue = new OracleParameter("p_avg_order_value", OracleDbType.Decimal) { Direction = ParameterDirection.Output };

                command.Parameters.Add(pStartDate);
                command.Parameters.Add(pEndDate);
                command.Parameters.Add(pTotalRevenue);
                command.Parameters.Add(pTotalOrders);
                command.Parameters.Add(pTotalItems);
                command.Parameters.Add(pAvgOrderValue);

                await command.ExecuteNonQueryAsync();

                var totalRevenue = (OracleDecimal)pTotalRevenue.Value;
                var totalOrders = (OracleDecimal)pTotalOrders.Value;
                var totalItems = (OracleDecimal)pTotalItems.Value;
                var avgOrderValue = (OracleDecimal)pAvgOrderValue.Value;

                return Ok(new RevenueStatsDto
                {
                    StartDate = start,
                    EndDate = end,
                    TotalRevenue = totalRevenue.IsNull ? 0 : totalRevenue.ToInt64(),
                    TotalOrders = totalOrders.IsNull ? 0 : totalOrders.ToInt32(),
                    TotalItems = totalItems.IsNull ? 0 : totalItems.ToInt32(),
                    AvgOrderValue = avgOrderValue.IsNull ? 0 : avgOrderValue.ToInt64()
                });
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

    
        [HttpGet("inventory")]
        public async Task<ActionResult<InventoryStatsResponseDto>> GetInventoryStats(
            [FromQuery] int lowStockThreshold = 10,
            [FromQuery] bool onlyLowStock = false)
        {
            var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SP_GET_INVENTORY_STATS";
                command.CommandType = CommandType.StoredProcedure;

                var pThreshold = new OracleParameter("p_low_stock_threshold", OracleDbType.Int32) { Value = lowStockThreshold };
                var pOnlyLowStock = new OracleParameter("p_only_low_stock", OracleDbType.Int32) { Value = onlyLowStock ? 1 : 0 };
                var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };

                command.Parameters.Add(pThreshold);
                command.Parameters.Add(pOnlyLowStock);
                command.Parameters.Add(pCursor);

                using var reader = await command.ExecuteReaderAsync();
                var items = new List<InventoryStatsItemDto>();

                while (await reader.ReadAsync())
                {
                    items.Add(new InventoryStatsItemDto
                    {
                        ProductId = reader.GetInt64(reader.GetOrdinal("ProductId")),
                        Sku = reader.IsDBNull(reader.GetOrdinal("SKU")) ? null : reader.GetString(reader.GetOrdinal("SKU")),
                        ProductName = reader.IsDBNull(reader.GetOrdinal("ProductName")) ? null : reader.GetString(reader.GetOrdinal("ProductName")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        ReservedQuantity = reader.GetInt32(reader.GetOrdinal("ReservedQuantity")),
                        AvailableQuantity = reader.GetInt32(reader.GetOrdinal("AvailableQuantity")),
                        IsLowStock = reader.GetInt32(reader.GetOrdinal("IsLowStock")) == 1
                    });
                }

                return Ok(new InventoryStatsResponseDto
                {
                    LowStockThreshold = lowStockThreshold,
                    Items = items
                });
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        [HttpGet("calculate-discount")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> CalculateDiscount(
            [FromQuery] decimal amount,
            [FromQuery] string? voucherCode)
        {
            if (string.IsNullOrWhiteSpace(voucherCode))
            {
                return Ok(new { originalAmount = amount, discountedAmount = amount, discount = 0 });
            }

            var result = await _db.Database
                .SqlQueryRaw<decimal>(
                    "SELECT FN_CALC_DISCOUNTED_PRICE({0}, {1}) AS \"Value\" FROM DUAL",
                    amount, voucherCode.ToUpper())
                .FirstOrDefaultAsync();

            return Ok(new
            {
                originalAmount = amount,
                discountedAmount = result,
                discount = amount - result,
                voucherCode = voucherCode.ToUpper()
            });
        }
    }
}

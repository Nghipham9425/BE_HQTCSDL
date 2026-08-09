using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace BE_HQTCSDL.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _db;

        public InventoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<InventoryPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize)
        {
            var query = _db.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim().ToLower();
                query = query.Where(i =>
                    i.Product != null && (
                        i.Product.Name.ToLower().Contains(keyword) ||
                        i.Product.Sku.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : null,
                    ProductSku = i.Product != null ? i.Product.Sku : null,
                    Quantity = i.Quantity,
                    ReservedQuantity = i.ReservedQuantity,
                    AvailableQuantity = i.Quantity - i.ReservedQuantity,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            return new InventoryPagedResponseDto
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<InventoryDto?> GetByIdAsync(long id)
        {
            var inventory = await _db.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            return inventory != null ? MapToDto(inventory) : null;
        }

        public async Task<InventoryDto?> GetByProductIdAsync(long productId)
        {
            var inventory = await _db.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            return inventory != null ? MapToDto(inventory) : null;
        }

        public async Task<InventoryDto> CreateAsync(long productId, int quantity)
        {
            // Check if inventory already exists (trigger may have created it)
            var existing = await _db.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (existing != null)
            {
                // Update existing inventory instead of creating new
                existing.Quantity = quantity;
                await _db.SaveChangesAsync();
                return MapToDto(existing);
            }

            // Create new inventory if not exists
            var inventory = new Inventory
            {
                ProductId = productId,
                Quantity = quantity,
                ReservedQuantity = 0
            };

            _db.Inventories.Add(inventory);
            await _db.SaveChangesAsync();

            await _db.Entry(inventory).Reference(i => i.Product).LoadAsync();
            return MapToDto(inventory);
        }

        public async Task<InventoryDto?> UpdateAsync(long id, InventoryUpdateDto dto)
        {
            var inventory = await _db.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null) return null;

            inventory.Quantity = dto.Quantity;
            inventory.ReservedQuantity = dto.ReservedQuantity;
            inventory.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return MapToDto(inventory);
        }

        public async Task<bool> AdjustQuantityAsync(long productId, int adjustment)
        {
            // Use stored procedure SP_ADJUST_INVENTORY
            // This handles: locking, validation, prevents negative quantity
            var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();
            
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SP_ADJUST_INVENTORY";
                command.CommandType = CommandType.StoredProcedure;

                var pProductId = new OracleParameter("p_product_id", OracleDbType.Int64) { Value = productId };
                var pAdjustment = new OracleParameter("p_adjustment", OracleDbType.Int32) { Value = adjustment };
                var pSuccess = new OracleParameter("p_success", OracleDbType.Int32) { Direction = ParameterDirection.Output };
                var pErrorMessage = new OracleParameter("p_error_message", OracleDbType.Varchar2, 500) { Direction = ParameterDirection.Output };

                command.Parameters.Add(pProductId);
                command.Parameters.Add(pAdjustment);
                command.Parameters.Add(pSuccess);
                command.Parameters.Add(pErrorMessage);

                await command.ExecuteNonQueryAsync();

                var successValue = (OracleDecimal)pSuccess.Value;
                var success = successValue.IsNull ? 0 : successValue.ToInt32();
                if (success != 1)
                {
                    var errorValue = pErrorMessage.Value;
                    var errorMsg = errorValue is OracleString oracleStr && !oracleStr.IsNull 
                        ? oracleStr.Value 
                        : "Adjust inventory failed";
                    throw new InvalidOperationException(errorMsg);
                }

                return true;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<bool> ReserveAsync(long productId, int quantity)
        {
            var inventory = await _db.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory == null) return false;

            var available = inventory.Quantity - inventory.ReservedQuantity;
            if (available < quantity) return false;

            inventory.ReservedQuantity += quantity;
            inventory.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReleaseReservedAsync(long productId, int quantity)
        {
            var inventory = await _db.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory == null) return false;

            inventory.ReservedQuantity -= quantity;
            if (inventory.ReservedQuantity < 0) inventory.ReservedQuantity = 0;
            inventory.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmReservedAsync(long productId, int quantity)
        {
            var inventory = await _db.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory == null) return false;

            inventory.Quantity -= quantity;
            inventory.ReservedQuantity -= quantity;
            if (inventory.Quantity < 0) inventory.Quantity = 0;
            if (inventory.ReservedQuantity < 0) inventory.ReservedQuantity = 0;
            inventory.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return true;
        }

        private static InventoryDto MapToDto(Inventory inventory)
        {
            return new InventoryDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = inventory.Product?.Name,
                ProductSku = inventory.Product?.Sku,
                Quantity = inventory.Quantity,
                ReservedQuantity = inventory.ReservedQuantity,
                AvailableQuantity = inventory.Quantity - inventory.ReservedQuantity,
                CreatedAt = inventory.CreatedAt,
                UpdatedAt = inventory.UpdatedAt
            };
        }
    }
}

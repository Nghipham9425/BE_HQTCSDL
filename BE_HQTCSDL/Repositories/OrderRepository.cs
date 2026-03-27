using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<User?> GetCustomerByIdAsync(long customerId)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Id == customerId);
        }

        public Task<PaymentMethod?> GetPaymentMethodByIdAsync(long paymentMethodId)
        {
            return _db.PaymentMethods.FirstOrDefaultAsync(pm => pm.Id == paymentMethodId);
        }

        public Task<PaymentMethod?> GetActiveCodPaymentMethodAsync()
        {
            return _db.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.IsActive == 1 && pm.MethodName.ToUpper() == "COD");
        }

        public Task<Voucher?> GetValidVoucherByCodeAsync(string normalizedCode, DateTime now)
        {
            return _db.Vouchers
                .FirstOrDefaultAsync(v =>
                    v.IsActive == 1
                    && v.Code.ToUpper() == normalizedCode
                    && v.StartDate <= now
                    && v.EndDate >= now);
        }

        public Task<int> CountOrdersByVoucherIdAsync(long voucherId)
        {
            return _db.Orders.CountAsync(o => o.VoucherId == voucherId);
        }

        public Task<List<Product>> GetProductsByIdsForUpdateAsync(List<long> productIds)
        {
            return _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<long> CreateOrderAsync(Order order, List<OrderDetail> details, Payment payment)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                foreach (var detail in details)
                {
                    detail.OrderId = order.Id;
                }

                payment.OrderId = order.Id;

                _db.OrderDetails.AddRange(details);
                _db.Payments.Add(payment);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return order.Id;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public Task<List<Order>> GetOrdersByCustomerAsync(long customerId)
        {
            return _db.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderDetails)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .ToListAsync();
        }

        public Task<Order?> GetOrderByIdAsync(long customerId, long orderId)
        {
            return _db.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId && o.Id == orderId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Voucher)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync();
        }

        public async Task<(int Total, List<Order> Items)> GetOrdersPagedForAdminAsync(string? q, string? status, int page, int pageSize)
        {
            var query = _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToUpper();
                query = query.Where(o => o.OrderStatus == normalizedStatus);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim().ToLower();
                query = query.Where(o =>
                    o.Id.ToString().Contains(keyword) ||
                    (o.Customer.FullName != null && o.Customer.FullName.ToLower().Contains(keyword)) ||
                    (o.Customer.Email != null && o.Customer.Email.ToLower().Contains(keyword)) ||
                    (o.Customer.Phone != null && o.Customer.Phone.ToLower().Contains(keyword)) ||
                    (o.ShippingAddress != null && o.ShippingAddress.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (total, items);
        }

        public Task<Order?> GetOrderByIdForAdminAsync(long orderId)
        {
            return _db.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Voucher)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(long orderId, string status)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return false;

            order.OrderStatus = status;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
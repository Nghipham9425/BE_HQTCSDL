using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BE_HQTCSDL.Models;

namespace BE_HQTCSDL.Repositories.Interfaces
{
	public interface IOrderRepository
	{
		Task<User?> GetCustomerByIdAsync(long customerId);
		Task<PaymentMethod?> GetPaymentMethodByIdAsync(long paymentMethodId);
		Task<PaymentMethod?> GetActiveCodPaymentMethodAsync();
		Task<Voucher?> GetValidVoucherByCodeAsync(string normalizedCode, DateTime now);
		Task<int> CountOrdersByVoucherIdAsync(long voucherId);
		Task<List<Product>> GetProductsByIdsForUpdateAsync(List<long> productIds);
		Task<long> CreateOrderAsync(Order order, List<OrderDetail> details, Payment payment);
		Task<List<Order>> GetOrdersByCustomerAsync(long customerId);
		Task<Order?> GetOrderByIdAsync(long customerId, long orderId);
		Task<(int Total, List<Order> Items)> GetOrdersPagedForAdminAsync(string? q, string? status, int page, int pageSize);
		Task<Order?> GetOrderByIdForAdminAsync(long orderId);
		Task<bool> UpdateOrderStatusAsync(long orderId, string status);
	}
}


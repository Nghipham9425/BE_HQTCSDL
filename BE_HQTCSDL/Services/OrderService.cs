using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;

namespace BE_HQTCSDL.Services
{
    public class OrderService : IOrderService
    {
        private static readonly HashSet<string> AllowedOrderStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "PENDING",
            "CONFIRMED",
            "SHIPPED",
            "DONE",
            "CANCELLED"
        };

        private readonly IOrderRepository _repo;

        public OrderService(IOrderRepository repo)
        {
            _repo = repo;
        }

        public async Task<OrderDetailDto> PlaceOrderAsync(long customerId, OrderPlaceRequestDto dto)
        {
            if (customerId <= 0) throw new ArgumentException("Invalid customer");
            if (dto == null) throw new ArgumentException("Payload is required");
            if (string.IsNullOrWhiteSpace(dto.ShippingAddress)) throw new ArgumentException("ShippingAddress is required");
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber)) throw new ArgumentException("PhoneNumber is required");
            if (dto.Items == null || dto.Items.Count == 0) throw new ArgumentException("Order must contain at least one item");

            dto.ShippingAddress = dto.ShippingAddress.Trim();
            dto.PhoneNumber = dto.PhoneNumber.Trim();
            dto.OrderEmail = string.IsNullOrWhiteSpace(dto.OrderEmail) ? null : dto.OrderEmail.Trim();
            dto.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
            dto.VoucherCode = string.IsNullOrWhiteSpace(dto.VoucherCode) ? null : dto.VoucherCode.Trim().ToUpper();

            if (dto.ShippingAddress.Length > 300) throw new ArgumentException("ShippingAddress too long");
            if (dto.PhoneNumber.Length > 20) throw new ArgumentException("PhoneNumber too long");
            if (dto.OrderEmail != null && dto.OrderEmail.Length > 200) throw new ArgumentException("OrderEmail too long");
            if (dto.Note != null && dto.Note.Length > 500) throw new ArgumentException("Note too long");

            var customer = await _repo.GetCustomerByIdAsync(customerId);
            if (customer == null) throw new ArgumentException("Customer not found");

            customer.Phone = dto.PhoneNumber;
            if (string.IsNullOrWhiteSpace(dto.OrderEmail))
            {
                dto.OrderEmail = customer.Email;
            }

            var itemsByProduct = dto.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            if (itemsByProduct.Any(i => i.ProductId <= 0 || i.Quantity <= 0))
            {
                throw new ArgumentException("Invalid product or quantity");
            }

            var productIds = itemsByProduct.Select(i => i.ProductId).ToList();
            var products = await _repo.GetProductsByIdsForUpdateAsync(productIds);

            if (products.Count != productIds.Count)
            {
                throw new ArgumentException("Some products do not exist");
            }

            long subTotal = 0;
            var details = new List<OrderDetail>();

            foreach (var line in itemsByProduct)
            {
                var product = products.First(p => p.Id == line.ProductId);

                if (product.IsActive != 1) throw new ArgumentException($"Product {product.Id} is inactive");
                if (!product.Price.HasValue) throw new ArgumentException($"Product {product.Id} has no price");

                var inventory = product.Inventory;
                var availableStock = inventory != null ? (inventory.Quantity - inventory.ReservedQuantity) : 0;
                if (availableStock < line.Quantity) throw new ArgumentException($"Product {product.Id} does not have enough stock");

                var unitPrice = product.Price.Value;
                subTotal += unitPrice * line.Quantity;

                // Reserve the quantity instead of deducting directly
                // Quantity will be deducted when order status changes to DONE
                if (inventory != null)
                {
                    inventory.ReservedQuantity += line.Quantity;
                }

                details.Add(new OrderDetail
                {
                    ProductId = product.Id,
                    Sku = product.Sku,
                    Price = unitPrice,
                    Quantity = line.Quantity
                });
            }

            long discount = 0;
            long? voucherId = null;

            if (!string.IsNullOrWhiteSpace(dto.VoucherCode))
            {
                var now = DateTime.Now;
                var voucher = await _repo.GetValidVoucherByCodeAsync(dto.VoucherCode, now);
                if (voucher == null) throw new ArgumentException("Invalid or expired voucher");

                if (subTotal < voucher.MinOrderValue)
                {
                    throw new ArgumentException("Order does not meet voucher minimum value");
                }

                if (voucher.UsageLimit.HasValue)
                {
                    var usedCount = await _repo.CountOrdersByVoucherIdAsync(voucher.Id);
                    if (usedCount >= voucher.UsageLimit.Value)
                    {
                        throw new ArgumentException("Voucher usage limit reached");
                    }
                }

                if (voucher.DiscountType == "PERCENT")
                {
                    discount = subTotal * voucher.DiscountValue / 100;
                    if (voucher.MaxDiscount.HasValue && discount > voucher.MaxDiscount.Value)
                    {
                        discount = voucher.MaxDiscount.Value;
                    }
                }
                else if (voucher.DiscountType == "FIXED")
                {
                    discount = voucher.DiscountValue;
                }
                else
                {
                    throw new ArgumentException("Unsupported voucher discount type");
                }

                if (discount > subTotal) discount = subTotal;
                voucherId = voucher.Id;
            }

            var finalAmount = subTotal - discount;

            PaymentMethod? paymentMethod;
            if (dto.PaymentMethodId.HasValue)
            {
                paymentMethod = await _repo.GetPaymentMethodByIdAsync(dto.PaymentMethodId.Value);
                if (paymentMethod == null || paymentMethod.IsActive != 1)
                {
                    throw new ArgumentException("Invalid payment method");
                }
            }
            else
            {
                paymentMethod = await _repo.GetActiveCodPaymentMethodAsync();
                if (paymentMethod == null)
                {
                    throw new ArgumentException("COD payment method is not configured");
                }
            }

            if (!string.Equals(paymentMethod.MethodName, "COD", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only COD payment is supported currently");
            }

            var order = new Order
            {
                CustomerId = customerId,
                VoucherId = voucherId,
                Amount = subTotal,
                DiscountAmount = discount,
                ShippingAddress = dto.ShippingAddress,
                OrderEmail = dto.OrderEmail,
                Note = dto.Note,
                PaymentMethodId = paymentMethod.Id,
                OrderDate = DateTime.Now,
                OrderStatus = "PENDING"
            };

            var payment = new Payment
            {
                PaymentMethodId = paymentMethod.Id,
                Amount = finalAmount,
                Status = "PENDING",
                CreatedAt = DateTime.Now,
                PaidAt = null,
                TransactionId = null
            };

            var orderId = await _repo.CreateOrderAsync(order, details, payment);

            var createdOrder = await _repo.GetOrderByIdAsync(customerId, orderId);
            if (createdOrder == null) throw new InvalidOperationException("Cannot load created order");

            return ToOrderDetailDto(createdOrder);
        }

        public async Task<List<OrderListItemDto>> GetMyOrdersAsync(long customerId)
        {
            if (customerId <= 0) throw new ArgumentException("Invalid customer");

            var orders = await _repo.GetOrdersByCustomerAsync(customerId);

            return orders.Select(o => new OrderListItemDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.FullName,
                CustomerEmail = o.Customer?.Email,
                CustomerPhone = o.Customer?.Phone,
                Amount = o.Amount,
                DiscountAmount = o.DiscountAmount,
                FinalAmount = o.Amount - o.DiscountAmount,
                OrderStatus = o.OrderStatus,
                OrderDate = o.OrderDate,
                ShippingAddress = o.ShippingAddress,
                PaymentMethodName = o.PaymentMethod?.MethodName,
                ItemCount = o.OrderDetails.Sum(i => i.Quantity)
            }).ToList();
        }

        public async Task<OrderDetailDto?> GetMyOrderByIdAsync(long customerId, long orderId)
        {
            if (customerId <= 0) throw new ArgumentException("Invalid customer");
            if (orderId <= 0) throw new ArgumentException("Invalid order id");

            var order = await _repo.GetOrderByIdAsync(customerId, orderId);
            if (order == null) return null;

            return ToOrderDetailDto(order);
        }

        public async Task<OrderPagedResponseDto> GetAdminOrdersAsync(string? q, string? status, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToUpper();
                if (!AllowedOrderStatuses.Contains(normalizedStatus))
                {
                    throw new ArgumentException("Invalid order status");
                }

                status = normalizedStatus;
            }

            var (total, items) = await _repo.GetOrdersPagedForAdminAsync(q, status, page, pageSize);

            return new OrderPagedResponseDto
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items.Select(o => new OrderListItemDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer?.FullName,
                    CustomerEmail = o.Customer?.Email,
                    CustomerPhone = o.Customer?.Phone,
                    Amount = o.Amount,
                    DiscountAmount = o.DiscountAmount,
                    FinalAmount = o.Amount - o.DiscountAmount,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate,
                    ShippingAddress = o.ShippingAddress,
                    PaymentMethodName = o.PaymentMethod?.MethodName,
                    ItemCount = o.OrderDetails.Sum(i => i.Quantity)
                }).ToList()
            };
        }

        public async Task<OrderDetailDto?> GetAdminOrderByIdAsync(long orderId)
        {
            if (orderId <= 0) throw new ArgumentException("Invalid order id");

            var order = await _repo.GetOrderByIdForAdminAsync(orderId);
            if (order == null) return null;

            return ToOrderDetailDto(order);
        }

        public async Task<bool> UpdateOrderStatusAsync(long orderId, string status)
        {
            if (orderId <= 0) throw new ArgumentException("Invalid order id");
            if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Status is required");

            var normalizedStatus = status.Trim().ToUpper();
            if (!AllowedOrderStatuses.Contains(normalizedStatus))
            {
                throw new ArgumentException("Invalid order status");
            }

            return await _repo.UpdateOrderStatusAsync(orderId, normalizedStatus);
        }

        private static OrderDetailDto ToOrderDetailDto(Order order)
        {
            return new OrderDetailDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName,
                CustomerEmail = order.Customer?.Email,
                CustomerPhone = order.Customer?.Phone,
                Amount = order.Amount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.Amount - order.DiscountAmount,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                OrderEmail = order.OrderEmail,
                Note = order.Note,
                PaymentMethodId = order.PaymentMethodId,
                PaymentMethodName = order.PaymentMethod?.MethodName,
                VoucherCode = order.Voucher?.Code,
                Items = order.OrderDetails.Select(d => new OrderLineItemDto
                {
                    ProductId = d.ProductId,
                    Sku = d.Sku,
                    ProductName = d.Product?.Name ?? string.Empty,
                    Thumbnail = d.Product?.Thumbnail,
                    UnitPrice = d.Price,
                    Quantity = d.Quantity,
                    LineTotal = d.Price * d.Quantity
                }).ToList()
            };
        }
    }
}
using System;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;

namespace BE_HQTCSDL.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repo;

        public InventoryService(IInventoryRepository repo)
        {
            _repo = repo;
        }

        public Task<InventoryPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            return _repo.GetPagedAsync(q, page, pageSize);
        }

        public Task<InventoryDto?> GetByIdAsync(long id)
        {
            return _repo.GetByIdAsync(id);
        }

        public Task<InventoryDto?> GetByProductIdAsync(long productId)
        {
            return _repo.GetByProductIdAsync(productId);
        }

        public async Task<InventoryDto> CreateOrUpdateAsync(long productId, int quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            var existing = await _repo.GetByProductIdAsync(productId);
            if (existing != null)
            {
                var updated = await _repo.UpdateAsync(existing.Id, new InventoryUpdateDto
                {
                    Quantity = quantity,
                    ReservedQuantity = existing.ReservedQuantity
                });
                return updated ?? throw new InvalidOperationException("Failed to update inventory");
            }

            return await _repo.CreateAsync(productId, quantity);
        }

        public Task<InventoryDto?> UpdateAsync(long id, InventoryUpdateDto dto)
        {
            if (dto.Quantity < 0)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }
            if (dto.ReservedQuantity < 0)
            {
                throw new ArgumentException("Reserved quantity cannot be negative");
            }
            if (dto.ReservedQuantity > dto.Quantity)
            {
                throw new ArgumentException("Reserved quantity cannot exceed total quantity");
            }

            return _repo.UpdateAsync(id, dto);
        }

        public Task<bool> AdjustQuantityAsync(long productId, int adjustment)
        {
            return _repo.AdjustQuantityAsync(productId, adjustment);
        }
    }
}

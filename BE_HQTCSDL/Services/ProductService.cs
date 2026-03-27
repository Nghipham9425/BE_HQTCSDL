using System;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Services
{
    public class ProductService : IProductService
    {
        private static readonly string[] AllowedProductTypes = { "NORMAL", "TCG_CARD", "CONSOLE", "ACCESSORY" };

        private readonly IProductRepository _repo;
        private readonly ApplicationDbContext _db;

        public ProductService(IProductRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public Task<ProductPagedResponseDto> GetPagedAsync(
            string? q,
            string? productType,
            bool? isActive,
            long? categoryId,
            string? categoryName,
            int page,
            int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            if (categoryId.HasValue && categoryId.Value <= 0)
            {
                throw new ArgumentException("Invalid CategoryId");
            }

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                categoryName = categoryName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(productType))
            {
                var normalizedType = productType.Trim().ToUpper();
                if (!AllowedProductTypes.Contains(normalizedType))
                {
                    throw new ArgumentException("Invalid ProductType");
                }
                productType = normalizedType;
            }

            return _repo.GetPagedAsync(q, productType, isActive, categoryId, categoryName, page, pageSize);
        }

        public Task<ProductDetailDto?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);

        public async Task<ProductDetailDto> CreateAsync(ProductUpsertRequestDto dto)
        {
            await ValidateAsync(dto, null);
            return await _repo.CreateAsync(dto);
        }

        public async Task<ProductDetailDto?> UpdateAsync(long id, ProductUpsertRequestDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            await ValidateAsync(dto, id);
            return await _repo.UpdateAsync(id, dto);
        }

        public Task<bool> DeleteAsync(long id) => _repo.DeleteAsync(id);

        private async Task ValidateAsync(ProductUpsertRequestDto dto, long? productId)
        {
            if (string.IsNullOrWhiteSpace(dto.Sku)) throw new ArgumentException("Sku is required");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required");

            dto.Sku = dto.Sku.Trim();
            dto.Name = dto.Name.Trim();
            dto.ProductType = dto.ProductType?.Trim().ToUpper() ?? string.Empty;

            if (!AllowedProductTypes.Contains(dto.ProductType))
            {
                throw new ArgumentException("Invalid ProductType");
            }

            var query = _db.Products.Where(p => p.Sku == dto.Sku);
            if (productId.HasValue)
            {
                query = query.Where(p => p.Id != productId.Value);
            }

            var skuExists = await query.CountAsync() > 0;
            if (skuExists) throw new ArgumentException("Sku already exists");

            if (dto.ProductType == "TCG_CARD")
            {
                if (string.IsNullOrWhiteSpace(dto.CardId)) throw new ArgumentException("CardId is required for TCG_CARD");

                dto.CardId = dto.CardId.Trim();
                var cardExists = await _db.TcgCards.CountAsync(c => c.CardId == dto.CardId) > 0;
                if (!cardExists) throw new ArgumentException("Invalid CardId");
            }
            else
            {
                dto.CardId = null;
            }
        }
    }
}
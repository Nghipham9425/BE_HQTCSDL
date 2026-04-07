using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _db;

        public WishlistRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProductListItemDto>> GetWishlistItemsAsync(long userId)
        {
            var rows = await _db.Wishlists
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .Select(w => new
                {
                    Id = w.Product.Id,
                    Sku = w.Product.Sku,
                    Name = w.Product.Name,
                    ProductType = w.Product.ProductType,
                    Price = w.Product.Price,
                    Stock = w.Product.Inventory == null
                        ? (int?)null
                        : w.Product.Inventory.Quantity,
                    IsActiveRaw = w.Product.IsActive,
                    Thumbnail = w.Product.Thumbnail,
                    UpdatedAt = w.Product.UpdatedAt
                })
                .ToListAsync();

            return rows.Select(r => new ProductListItemDto
            {
                Id = r.Id,
                Sku = r.Sku,
                Name = r.Name,
                ProductType = r.ProductType,
                Price = r.Price,
                Stock = r.Stock ?? 0,
                IsActive = r.IsActiveRaw == 1,
                Thumbnail = r.Thumbnail,
                UpdatedAt = r.UpdatedAt
            }).ToList();
        }

        public async Task<bool> ProductExistsAsync(long productId)
        {
            return await _db.Products.CountAsync(p => p.Id == productId) > 0;
        }

        public async Task<bool> ExistsAsync(long userId, long productId)
        {
            return await _db.Wishlists.CountAsync(w => w.UserId == userId && w.ProductId == productId) > 0;
        }

        public async Task AddAsync(long userId, long productId)
        {
            _db.Wishlists.Add(new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = System.DateTime.Now
            });

            await _db.SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(long userId, long productId)
        {
            var entity = await _db.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (entity == null) return false;

            _db.Wishlists.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

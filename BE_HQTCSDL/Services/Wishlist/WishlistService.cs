using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;

namespace BE_HQTCSDL.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _repo;

        public WishlistService(IWishlistRepository repo)
        {
            _repo = repo;
        }

        public Task<List<ProductListItemDto>> GetItemsAsync(long userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user id");
            return _repo.GetWishlistItemsAsync(userId);
        }

        public async Task AddAsync(long userId, long productId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user id");
            if (productId <= 0) throw new ArgumentException("Invalid product id");

            var exists = await _repo.ProductExistsAsync(productId);
            if (!exists) throw new ArgumentException("Product not found");

            var alreadyInWishlist = await _repo.ExistsAsync(userId, productId);
            if (alreadyInWishlist) return;

            await _repo.AddAsync(userId, productId);
        }

        public Task<bool> RemoveAsync(long userId, long productId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user id");
            if (productId <= 0) throw new ArgumentException("Invalid product id");

            return _repo.RemoveAsync(userId, productId);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Repositories.Interfaces
{
    public interface IWishlistRepository
    {
        Task<List<ProductListItemDto>> GetWishlistItemsAsync(long userId);
        Task<bool> ProductExistsAsync(long productId);
        Task<bool> ExistsAsync(long userId, long productId);
        Task AddAsync(long userId, long productId);
        Task<bool> RemoveAsync(long userId, long productId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<List<ProductListItemDto>> GetItemsAsync(long userId);
        Task AddAsync(long userId, long productId);
        Task<bool> RemoveAsync(long userId, long productId);
    }
}

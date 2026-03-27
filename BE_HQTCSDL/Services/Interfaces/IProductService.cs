using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductPagedResponseDto> GetPagedAsync(
            string? q,
            string? productType,
            bool? isActive,
            long? categoryId,
            string? categoryName,
            int page,
            int pageSize);
        Task<ProductDetailDto?> GetByIdAsync(long id);
        Task<ProductDetailDto> CreateAsync(ProductUpsertRequestDto dto);
        Task<ProductDetailDto?> UpdateAsync(long id, ProductUpsertRequestDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
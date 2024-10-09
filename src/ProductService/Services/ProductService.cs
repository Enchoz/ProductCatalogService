using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using ProductService.Services.Interfaces;

namespace ProductService.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            var createdProduct = await _unitOfWork.ProductRepository.AddAsync(product);
            await _unitOfWork.CommitAsync();

            return createdProduct;
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _unitOfWork.ProductRepository.GetAllAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }
            return product;
        }

        public async Task UpdateProductAsync(Product product)
        {
            var existingProduct = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == product.Id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.Description = product.Description;

            await _unitOfWork.ProductRepository.UpdateAsync(existingProduct);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            await _unitOfWork.ProductRepository.DeleteAsync(product);
            await _unitOfWork.CommitAsync();
        }
    }
}

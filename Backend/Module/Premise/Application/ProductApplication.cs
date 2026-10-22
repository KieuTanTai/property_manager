using Premise.Interfaces.IRepository.BusinessType;
using Premise.Interfaces.IRepository.Product;
using Premise.Interfaces.IApplication;
using Premise.Models.Business;
using Premise.Models.Product;
using Shared.Interfaces;
using Shared.Logging;

namespace Premise.Application
{
    public class ProductApplication(
        ILogger<ProductApplication> logger,
        ILogPool logPool,
        IWhitelistProductRepository productRepository,
        IBusinessTypeRepository businessTypeRepository,
        IProductBusinessTypeRepository productBusinessTypeRepository,
        IUnitOfWork unitOfWork) : IProductApplication
    {
        private const string Module = "Premise";
        private const string Layer = "Application";

        private readonly ILogger<ProductApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;
        private readonly IWhitelistProductRepository _productRepository = productRepository;
        private readonly IBusinessTypeRepository _businessTypeRepository = businessTypeRepository;
        private readonly IProductBusinessTypeRepository _productBusinessTypeRepository = productBusinessTypeRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IReadOnlyList<WhitelistProductModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all products.");
            var products = await _productRepository.GetAllAsync(cancellationToken);
            if (products.Count != 0)
            {
                return products;
            }

            _logger.LogLayerWarning(_logPool, Module, Layer, "No products found.");
            throw new InvalidOperationException("No products found.");
        }

        public async Task<WhitelistProductModel> GetByIdAsync(
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            if (productId == Guid.Empty)
            {
                throw new ArgumentException("Product id is required.", nameof(productId));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading product by id.");
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product is not null)
            {
                return product;
            }

            _logger.LogLayerWarning(_logPool, Module, Layer, "Product not found by id.");
            throw new KeyNotFoundException($"Product not found: {productId}.");
        }

        public async Task<WhitelistProductModel> AddAsync(
            string productName,
            string? productDescription,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException("Product name is required.", nameof(productName));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Adding product.");
            var product = new WhitelistProductModel(productName);
            if (!string.IsNullOrWhiteSpace(productDescription))
            {
                product.SetWhitelistProductDescription(productDescription);
            }

            await _productRepository.AddAsync(product, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to add product.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Product creation failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Product created.");
            return product;
        }

        public async Task<WhitelistProductModel> AddForBusinessTypeAsync(
            string productName,
            string? productDescription,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                var exception = new ArgumentException(
                    "Product name is required.", nameof(productName));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product creation rejected because its name is missing.");
                throw exception;
            }

            if (businessTypeId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "Business type id is required.", nameof(businessTypeId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product creation rejected because its business type id is missing.");
                throw exception;
            }

            if (!await _businessTypeRepository.ExistsAsync(businessTypeId, cancellationToken))
            {
                var exception = new KeyNotFoundException(
                    $"Business type not found: {businessTypeId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product creation failed because the business type was not found.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Creating product and assigning it to a business type.");
            var product = new WhitelistProductModel(productName);
            if (!string.IsNullOrWhiteSpace(productDescription))
            {
                product.SetWhitelistProductDescription(productDescription);
            }

            await _productRepository.AddAsync(product, cancellationToken);
            await _productBusinessTypeRepository.AddAsync(
                new ProductBusinessTypeModel(product.WhitelistProductId, businessTypeId),
                cancellationToken);

            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException(
                    "Failed to create and assign product.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product creation and business type assignment failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Product created and assigned to business type.");
            return product;
        }

        public async Task<WhitelistProductModel> UpdateAsync(
            Guid productId,
            string productName,
            string? productDescription,
            CancellationToken cancellationToken = default)
        {
            if (productId == Guid.Empty)
            {
                throw new ArgumentException("Product id is required.", nameof(productId));
            }

            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException("Product name is required.", nameof(productName));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating product.");
            var product = await _productRepository.GetTrackedByIdAsync(productId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product not found: {productId}.");

            product.SetWhitelistProductName(productName);
            if (string.IsNullOrWhiteSpace(productDescription))
            {
                product.ClearWhitelistProductDescription();
            }
            else
            {
                product.SetWhitelistProductDescription(productDescription);
            }

            await _productRepository.UpdateAsync(product, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException("Failed to update product.");
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Product update failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Product updated.");
            return product;
        }
        public async Task<IReadOnlyList<BusinessTypeModel>> GetBusinessTypesAsync(
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            await EnsureProductExistsAsync(productId, cancellationToken);

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading product business types.");
            var associations = await _productBusinessTypeRepository.GetByProductIdAsync(
                productId, cancellationToken);
            if (associations.Count == 0)
            {
                return [];
            }

            return await _businessTypeRepository.GetByIdsAsync(
                associations.Select(association => association.BusinessTypeId),
                cancellationToken);
        }

        public async Task<IReadOnlyList<WhitelistProductModel>> GetByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            if (businessTypeId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "Business type id is required.", nameof(businessTypeId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product query rejected because the business type id is missing.");
                throw exception;
            }

            if (!await _businessTypeRepository.ExistsAsync(businessTypeId, cancellationToken))
            {
                var exception = new KeyNotFoundException(
                    $"Business type not found: {businessTypeId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product query failed because the business type was not found.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading products by business type.");
            var associations = await _productBusinessTypeRepository.GetByBusinessTypeIdAsync(
                businessTypeId, cancellationToken);
            if (associations.Count == 0)
            {
                return [];
            }

            return await _productRepository.GetByIdsAsync(
                associations.Select(association => association.ProductId),
                cancellationToken);
        }

        public async Task<ProductBusinessTypeModel> AddBusinessTypeAsync(
            Guid productId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            if (businessTypeId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "Business type id is required.", nameof(businessTypeId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product business type assignment rejected because the business type id is missing.");
                throw exception;
            }

            await EnsureProductExistsAsync(productId, cancellationToken);
            if (!await _businessTypeRepository.ExistsAsync(businessTypeId, cancellationToken))
            {
                var exception = new KeyNotFoundException(
                    $"Business type not found: {businessTypeId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product business type assignment failed because the business type was not found.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Assigning business type to product.");
            var association = new ProductBusinessTypeModel(productId, businessTypeId);
            await _productBusinessTypeRepository.AddAsync(association, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException(
                    "Failed to assign business type to product.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product business type assignment failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Business type assigned to product.");
            return association;
        }

        public async Task RemoveBusinessTypeAsync(
            Guid productId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default)
        {
            if (businessTypeId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "Business type id is required.", nameof(businessTypeId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product business type removal rejected because the business type id is missing.");
                throw exception;
            }

            await EnsureProductExistsAsync(productId, cancellationToken);
            if (!await _productBusinessTypeRepository.ExistsAsync(
                    productId, businessTypeId, cancellationToken))
            {
                var exception = new KeyNotFoundException(
                    $"Business type {businessTypeId} is not assigned to product {productId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product business type removal failed because the association was not found.");
                throw exception;
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Removing business type from product.");
            await _productBusinessTypeRepository.DeleteAsync(
                productId, businessTypeId, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException(
                    "Failed to remove business type from product.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product business type removal failed.");
                throw exception;
            }

            _logger.LogLayerInformation(_logPool, Module, Layer, "Business type removed from product.");
        }

        private async Task EnsureProductExistsAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            if (productId == Guid.Empty)
            {
                var exception = new ArgumentException("Product id is required.", nameof(productId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product lookup rejected because the product id is missing.");
                throw exception;
            }

            if (!await _productRepository.ExistsAsync(productId, cancellationToken))
            {
                var exception = new KeyNotFoundException($"Product not found: {productId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Product lookup failed because the product was not found.");
                throw exception;
            }
        }
    }
}

using System.ComponentModel.DataAnnotations;
using Premise.Models.Premise;
using Premise.Models.Product;
using Shared.ModelHelper;

namespace Premise.Models.Business
{
    public class BusinessTypeModel
    {
        public BusinessTypeModel(string businessTypeName)
        {
            BusinessTypeName = ModelFieldGuard.Required(businessTypeName, 50, nameof(businessTypeName));
        }

        public BusinessTypeModel(
            Guid businessTypeId,
            string businessTypeName,
            string? businessTypeDescription,
            bool businessTypeIsActive)
        {
            BusinessTypeId = businessTypeId;
            BusinessTypeName = ModelFieldGuard.Required(businessTypeName, 50, nameof(businessTypeName));
            BusinessTypeDescription = businessTypeDescription;
            BusinessTypeIsActive = businessTypeIsActive;
        }

        public BusinessTypeModel() {}

        public Guid BusinessTypeId { get; init; }

        [MaxLength(50)] public string BusinessTypeName { get; private set; } = string.Empty;

        [MaxLength(255)] public string? BusinessTypeDescription { get; private set; }

        public bool BusinessTypeIsActive { get; private set; } = true;

        public DateTime BusinessTypeCreatedAt { get; init; } = DateTime.Now;

        public DateTime BusinessTypeUpdatedAt { get; private set; } = DateTime.Now;

        public IReadOnlyList<PremiseModel> Premises { get; private set; } = new List<PremiseModel>();
        
        public IReadOnlyList<WhitelistProductModel> WhitelistProducts { get; private set; } = new List<WhitelistProductModel>();
        
        #region Setter

        public void SetBusinessTypeName(string name)
        {
            BusinessTypeName = ModelFieldGuard.Required(name, 50, nameof(name));
        }

        public void ClearBusinessTypeDescription()
        {
            if (BusinessTypeDescription is null)
            {
                return;
            }
            BusinessTypeDescription = null;
            BusinessTypeDescription = null;
        }

        public void SetBusinessTypeDescription(string description)
        {
            BusinessTypeDescription = ModelFieldGuard.Required(description, 255, nameof(description));
        }

        public void SetBusinessTypeIsActive(bool isActive)
        {
            if (BusinessTypeIsActive == isActive)
            {
                return;
            }
            BusinessTypeIsActive = isActive;
            BusinessTypeIsActive = isActive;
        }

        public void SetPremises(IReadOnlyList<PremiseModel> premises)
        {
            Premises = premises;
        }

        public void SetWhitelistProducts(IReadOnlyList<WhitelistProductModel> whitelistProducts)
        {
            WhitelistProducts = whitelistProducts;
        }

        #endregion
    }
}
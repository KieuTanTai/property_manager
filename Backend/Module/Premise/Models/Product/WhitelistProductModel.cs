using System.ComponentModel.DataAnnotations;
using Shared.ModelHelper;

namespace Premise.Models.Product
{
    public class WhitelistProductModel
    {
        public WhitelistProductModel(string whitelistProductName)
        {
            WhitelistProductName = ModelFieldGuard.Required(
                whitelistProductName,
                100,
                nameof(whitelistProductName));
        }

        public WhitelistProductModel(
            Guid whitelistProductId,
            string whitelistProductName,
            string? whitelistProductDescription)
        {
            WhitelistProductId = whitelistProductId;
            WhitelistProductName = ModelFieldGuard.Required(
                whitelistProductName,
                100,
                nameof(whitelistProductName));
            WhitelistProductDescription = whitelistProductDescription;
        }

        public WhitelistProductModel() {}

        public Guid WhitelistProductId { get; init; }

        [MaxLength(100)] public string WhitelistProductName { get; private set; } = string.Empty;

        [MaxLength(255)] public string? WhitelistProductDescription { get; private set; }

        public DateTime WhitelistProductCreatedAt { get; init; } = DateTime.Now;

        public DateTime WhitelistProductUpdatedAt { get; private set; } = DateTime.Now;

        #region Setter

        public void SetWhitelistProductName(string name)
        {
            WhitelistProductName = ModelFieldGuard.Required(name, 100, nameof(name));
        }

        public void ClearWhitelistProductDescription()
        {
            if (WhitelistProductDescription is null)
            {
                return;
            }
            WhitelistProductDescription = null;
            WhitelistProductDescription = null;
        }

        public void SetWhitelistProductDescription(string description)
        {
            WhitelistProductDescription = ModelFieldGuard.Required(description, 255, nameof(description));
        }

        #endregion
    }
}
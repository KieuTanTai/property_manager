using System.ComponentModel.DataAnnotations;
using Shared.ModelHelper;

namespace Premise.Models.Premise
{
    public class PremiseMediaModel
    {
        public PremiseMediaModel(Guid premiseMediaPremiseId, string premiseMediaImageUrl)
        {
            PremiseMediaPremiseId = premiseMediaPremiseId;
            PremiseMediaImageUrl = ImageDataUriGuard.Required(
                premiseMediaImageUrl,
                300 * 1024,
                nameof(premiseMediaImageUrl));
        }

        public PremiseMediaModel(
            int premiseMediaId,
            Guid premiseMediaPremiseId,
            string premiseMediaImageUrl)
        {
            PremiseMediaId = premiseMediaId;
            PremiseMediaPremiseId = premiseMediaPremiseId;
            PremiseMediaImageUrl = ImageDataUriGuard.Required(
                premiseMediaImageUrl,
                300 * 1024,
                nameof(premiseMediaImageUrl));
        }

        public PremiseMediaModel() {}

        public int PremiseMediaId { get; init; }

        public Guid PremiseMediaPremiseId { get; private set; }

        [MaxLength(ImageDataUriGuard.StandardMaxEncodedCharacters)]
        public string PremiseMediaImageUrl { get; private set; } = string.Empty;

        public DateTime PremiseMediaCreatedAt { get; init; } = DateTime.Now;

        public DateTime PremiseMediaUpdatedAt { get; private set; } = DateTime.Now;

        #region Setter

        public void SetPremiseMediaPremiseId(Guid premiseId)
        {
            PremiseMediaPremiseId = premiseId;
        }

        public void SetPremiseMediaImageUrl(string imageUrl)
        {
            PremiseMediaImageUrl = ImageDataUriGuard.Required(imageUrl, 300 * 1024, nameof(imageUrl));
        }

        #endregion
    }
}

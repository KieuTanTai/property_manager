using System.ComponentModel.DataAnnotations;
using Shared.ModelHelper;

namespace Premise.Models.Premise
{
    public class PremiseMediaModel
    {
        public PremiseMediaModel(Guid premiseMediaPremiseId, string premiseMediaImageUrl)
        {
            PremiseMediaPremiseId = premiseMediaPremiseId;
            PremiseMediaImageUrl = ModelFieldGuard.Required(
                premiseMediaImageUrl,
                255,
                nameof(premiseMediaImageUrl));
        }

        public PremiseMediaModel(
            int premiseMediaId,
            Guid premiseMediaPremiseId,
            string premiseMediaImageUrl)
        {
            PremiseMediaId = premiseMediaId;
            PremiseMediaPremiseId = premiseMediaPremiseId;
            PremiseMediaImageUrl = ModelFieldGuard.Required(
                premiseMediaImageUrl,
                255,
                nameof(premiseMediaImageUrl));
        }

        public PremiseMediaModel() {}

        public int PremiseMediaId { get; init; }

        public Guid PremiseMediaPremiseId { get; private set; }

        [MaxLength(255)] public string PremiseMediaImageUrl { get; private set; } = string.Empty;
        
        public DateTime PremiseMediaCreatedAt { get; init; } = DateTime.Now;
        
        public DateTime PremiseMediaUpdatedAt { get; private set; } = DateTime.Now;

        #region Setter

        public void SetPremiseMediaPremiseId(Guid premiseId)
        {
            PremiseMediaPremiseId = premiseId;
        }

        public void SetPremiseMediaImageUrl(string imageUrl)
        {
            PremiseMediaImageUrl = ModelFieldGuard.Required(imageUrl, 255, nameof(imageUrl));
        }

        #endregion
    }
}
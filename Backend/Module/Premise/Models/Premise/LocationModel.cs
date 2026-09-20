using System.ComponentModel.DataAnnotations;
using Shared.ModelHelper;

namespace Premise.Models.Premise
{
    public class LocationModel
    {
        public LocationModel(string locationAddress)
        {
            LocationAddress = ModelFieldGuard.Required(locationAddress, 255, nameof(locationAddress));
        }

        public LocationModel(Guid locationId, string? locationAddress)
        {
            LocationId = locationId;
            LocationAddress = ModelFieldGuard.Required(locationAddress, 255, nameof(locationAddress));
        }

        public LocationModel() {}

        public Guid LocationId { get; init; }

        [MaxLength(255)] public string LocationAddress { get; private set; } = string.Empty;

        public DateTime LocationCreatedAt { get; init; } = DateTime.Now;

        public DateTime LocationUpdatedAt { get; private set; } = DateTime.Now;

        #region Setter

        public void SetLocationAddress(string address)
        {
            LocationAddress = ModelFieldGuard.Required(address, 255, nameof(address));
        }

        #endregion
    }
}
using System.ComponentModel.DataAnnotations;
using Premise.Models.Business;
using Shared.Enum;
using Shared.ModelHelper;

namespace Premise.Models.Premise
{
    public class PremiseModel
    {
        public PremiseModel(string premiseName, Guid premiseLocationId)
        {
            PremiseName = ModelFieldGuard.Required(premiseName, 50, nameof(premiseName));
            PremiseLocationId = premiseLocationId;
        }

        public PremiseModel(
            string premiseName,
            Guid premiseLocationId,
            EPremiseStatus premiseStatus,
            int premisePosition,
            int premiseFloor,
            string premiseArea,
            string? premiseDescription)
        {
            PremiseName = ModelFieldGuard.Required(premiseName, 50, nameof(premiseName));
            PremiseLocationId = premiseLocationId;
            PremiseStatus = premiseStatus;
            PremisePosition = premisePosition;
            PremiseFloor = premiseFloor;
            PremiseArea = premiseArea;
            PremiseDescription = premiseDescription;
        }

        public PremiseModel(
            Guid premiseId,
            string? premiseName,
            Guid premiseLocationId,
            EPremiseStatus premiseStatus,
            int premisePosition,
            int premiseFloor,
            string premiseArea,
            string? premiseDescription)
        {
            PremiseId = premiseId;
            PremiseName = ModelFieldGuard.Required(premiseName, 50, nameof(premiseName));
            PremiseLocationId = premiseLocationId;
            PremiseStatus = premiseStatus;
            PremisePosition = premisePosition;
            PremiseFloor = premiseFloor;
            PremiseArea = premiseArea;
            PremiseDescription = premiseDescription;
        }

        public PremiseModel() {}

        public Guid PremiseId { get; init; }

        [MaxLength(50)] public string PremiseName { get; private set; } = string.Empty;

        public Guid PremiseLocationId { get; private set; }

        public EPremiseStatus PremiseStatus { get; private set; } = EPremiseStatus.Available;

        public int PremisePosition { get; private set; }

        public int PremiseFloor { get; private set; }

        [MaxLength(10)] public string PremiseArea { get; private set; } = string.Empty;

        [MaxLength(100)] public string? PremiseDescription { get; private set; }

        public DateTime PremiseCreatedAt { get; init; } = DateTime.Now;

        public DateTime PremiseUpdatedAt { get; private set; } = DateTime.Now;
        
        public LocationModel? PremiseLocation { get; private set; }
        
        public IReadOnlyList<PremiseMediaModel> PremiseMedia { get; private set; } = new List<PremiseMediaModel>();
        
        public IReadOnlyList<BusinessTypeModel> PremiseBusinessTypes { get; private set; } = new List<BusinessTypeModel>();
        

        #region Setter

        public void SetPremiseName(string name)
        {
            PremiseName = ModelFieldGuard.Required(name, 50, nameof(name));
        }

        public void SetPremiseLocationId(Guid locationId)
        {
            if (PremiseLocationId == locationId)
            {
                return;
            }

            PremiseLocationId = locationId;
        }

        public void SetPremiseStatus(EPremiseStatus status)
        {
            if (PremiseStatus == status)
            {
                return;
            }

            PremiseStatus = status;
        }

        public void SetPremisePosition(int premisePosition)
        {
            if (PremisePosition == premisePosition)
            {
                return;
            }

            PremisePosition = premisePosition;
        }

        public void SetPremiseFloor(int floor)
        {
            if (PremiseFloor == floor)
            {
                return;
            }

            PremiseFloor = floor;
        }

        public void ClearPremiseArea()
        {
            PremiseArea = string.Empty;
        }

        public void SetPremiseArea(string area)
        {
            PremiseArea = ModelFieldGuard.Required(area, 10, nameof(area));
        }

        public void ClearPremiseDescription()
        {
            if (PremiseDescription is null)
            {
                return;
            }
            PremiseDescription = null;
            PremiseDescription = null;
        }

        public void SetPremiseDescription(string description)
        {
            PremiseDescription = ModelFieldGuard.Required(description, 100, nameof(description));
        }

        public void SetPremiseBusinessTypes(IReadOnlyList<BusinessTypeModel> businessTypes)
        {
            PremiseBusinessTypes = businessTypes;
        }

        public void SetPremiseMedia(IReadOnlyList<PremiseMediaModel> premiseMedia)
        {
            PremiseMedia = premiseMedia;
        }

        public void SetPremiseLocation(LocationModel location)
        {
            PremiseLocation = location;
        }
        
        #endregion
    }
}
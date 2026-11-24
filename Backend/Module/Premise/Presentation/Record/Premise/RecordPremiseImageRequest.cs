using System.ComponentModel.DataAnnotations;
using Shared.ModelHelper;

namespace Premise.Presentation.Record.Premise
{
    public record RecordPremiseImageRequest(
        [Required]
        [MaxLength(ImageDataUriGuard.StandardMaxEncodedCharacters)]
        string ImageUrl
    );
}
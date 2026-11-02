using System.ComponentModel.DataAnnotations;
using Shared.ModelHelper;

namespace Premise.Presentations.Record.Premise
{
    public record RecordPremiseImageRequest(
        [Required]
        [MaxLength(ImageDataUriGuard.StandardMaxEncodedCharacters)]
        string ImageUrl
    );
}
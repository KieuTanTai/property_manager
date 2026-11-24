using System.ComponentModel.DataAnnotations;

namespace Premise.Presentation.Record.Product
{
    public record RecordAddProductRequest(
        [Required]
        [StringLength(100)]
        string ProductName,
        [StringLength(255)]
        string? ProductDescription
    );
}
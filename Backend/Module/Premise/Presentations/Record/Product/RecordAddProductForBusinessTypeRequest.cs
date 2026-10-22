using System.ComponentModel.DataAnnotations;

namespace Premise.Presentations.Record.Product;

public record RecordAddProductForBusinessTypeRequest(
    [Required]
    [StringLength(100)]
    string ProductName,

    [StringLength(255)]
    string? ProductDescription,

    [Required]
    Guid BusinessTypeId
);

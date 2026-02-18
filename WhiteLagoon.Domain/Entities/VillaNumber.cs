using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhiteLagoon.Domain.Entities;

public class VillaNumber
{
    /*
     * •	DatabaseGenerated(DatabaseGeneratedOption.None)
     * •	Instructs EF Core that the database will NOT generate a value for this column.
     * •	The application must supply a value for Villa_Number before calling SaveChanges().
     */
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Villa_Number { get; set; }
    [ForeignKey("Villa")]
    public int VillaId { get; set; }

    [ValidateNever]
    public Villa? Villa { get; set; }
    public string? SpecialDetails { get; set; }
}

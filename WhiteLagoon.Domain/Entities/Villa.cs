using System.ComponentModel.DataAnnotations;

namespace WhiteLagoon.Domain.Entities
{
    public class Villa
    {
        public int Id { get; set; }
        [MaxLength(50, ErrorMessage = "Name cannot be longer than 50 characters")]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Display(Name = "Price per Night")]
        [Range(1, 100000, ErrorMessage = "Price must be between 1 and 100000")]
        public double Price { get; set; }
        public int Sqft { get; set; }
        [Range(1, 20, ErrorMessage = "Occupancy must be between 1 and 20")]
        public int Occupancy { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}

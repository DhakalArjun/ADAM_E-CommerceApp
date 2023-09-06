using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ADAM_Razor_Temp.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Category Name is mandatory !")]
        [MaxLength(40)]
        [DisplayName("Category Name *")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display order must be between 1 and 100")] //custom error message
        public int DisplayOrder { get; set; }
    }
}

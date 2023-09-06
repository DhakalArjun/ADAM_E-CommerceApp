using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ADAM_E_CommerceApp.Models
{
    public class Category
    {
        /* Actually if we use property name as Id or CategoryId, then Entity Framework
        automatically know it's an primary key & we don't need to use [Key] annotation */
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

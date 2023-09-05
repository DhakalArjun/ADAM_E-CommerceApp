using System.ComponentModel.DataAnnotations;

namespace ADAM_E_CommerceApp.Models
{
    public class Category
    {
        /* Actually if we use property name as Id or CategoryId, then Entity Framework
        automatically know it's an primary key & we don't need to use [Key] annotation */
        [Key]            
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
    }
}

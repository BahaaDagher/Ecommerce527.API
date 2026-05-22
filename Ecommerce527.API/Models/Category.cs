
using System.ComponentModel.DataAnnotations;

namespace ECommerce527.API.Models
{
    public class Category
    {
        public int Id { get; set; }
        //[MinLength(2)]
        //[MaxLength(20)]
        [MinMaxCustom(2,20)]
        public string Name { get; set; }  = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
    }
}

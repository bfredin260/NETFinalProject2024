using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWConsole.Model
{
    public partial class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }

        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Enter Category Name!")]
        [MaxLength(15, ErrorMessage = "Category Name can't be longer than 15 characters!")]
        public string CategoryName { get; set; }
        
        public string Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
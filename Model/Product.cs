using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWConsole.Model
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int ProductId { get; set; }
        [Required(ErrorMessage = "Needs a name")]
        public string ProductName { get; set; }
        public int? SupplierId { get; set; }
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Enter quantity per unit")]
        public string QuantityPerUnit { get; set; }

        [Required(ErrorMessage = "Enter unit price")]
        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }

        [Required(ErrorMessage = "Is this product discontinued or not?")]
        public bool Discontinued { get; set; }

        [Required(ErrorMessage = "Product needs Category")]
        public virtual Category Category { get; set; }

        [Required(ErrorMessage = "Needs a Supplier")]
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

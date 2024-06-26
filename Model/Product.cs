﻿using System;
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
        [MaxLength(40, ErrorMessage = "Product Name can't be longer than 40 characters!")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Needs a Supplier")]
        public int? SupplierId { get; set; }

        [Required(ErrorMessage = "Product needs Category")]
        public int? CategoryId { get; set; }

        [MaxLength(40, ErrorMessage = "Quantity Per Unit can't be longer than 40 characters!")]
        public string QuantityPerUnit { get; set; }

        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }

        [Required(ErrorMessage = "Is this product discontinued or not?")]
        public bool Discontinued { get; set; }

        public virtual Category Category { get; set; }

        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

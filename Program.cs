using NLog;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.CompilerServices;

Console.WriteLine("Hello World!");
// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "/nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    var db = new NWContext();
    string choice;
    do
    {
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Display all Categories and their related products");
        Console.WriteLine("5) Add new Product");
        Console.WriteLine("6) Edit Product");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");
        if (choice == "1")
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (choice == "2")
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.Categories.Add(category);
                    db.SaveChanges();
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else if (choice == "3")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");            
            }
        }
        else if (choice == "4")
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        else if (choice == "5")
        {
            Product product = new Product();
            Console.WriteLine("Enter Product Name:");
            product.ProductName = Console.ReadLine();
            Console.WriteLine("What category is this product in?");
            foreach(Category c in db.Categories) {
                Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
            }
            int catId = int.Parse(Console.ReadLine());
            try{
                product.Category = db.Categories.FirstOrDefault(c => c.CategoryId == catId);
                product.CategoryId = catId;
            } catch(Exception e) {
                logger.Error(e.Message);
            }
            Console.WriteLine("Who supplies this product?");
            foreach(Supplier s in db.Suppliers) {
                Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
            }
            int supId = int.Parse(Console.ReadLine());
            try{
                product.Supplier = db.Suppliers.FirstOrDefault(s => s.SupplierId == supId);
                product.SupplierId = supId;
            } catch(Exception e) {
                logger.Error(e.Message);
            }
            Console.WriteLine("Enter Quantity Per Unit:");
            product.QuantityPerUnit = Console.ReadLine();
            Console.WriteLine("Enter Price Per Unit:");
            try {
                product.UnitPrice = decimal.Parse(Console.ReadLine());
            } catch(Exception e) {
                logger.Error(e.Message);
            }
            Console.WriteLine("Is this product discontinued? (y/n):");
            string discontinued = Console.ReadLine();
            if(discontinued == "y") {
                product.Discontinued = true;
            } else if(discontinued == "n") {
                product.Discontinued = false;
            }

            product.UnitsInStock = 0;
            product.UnitsOnOrder = 0;
            product.ReorderLevel = 0;

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Products.Any(p => p.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.Products.Add(product);
                    db.SaveChanges();
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else if(choice == "6") {
            Console.WriteLine("Which product do you want to edit?");
            foreach(Product pr in db.Products) {
                Console.WriteLine($"{pr.ProductId}) {pr.ProductName}");
            }
            string productChoice = Console.ReadLine();
            Product product = new Product();
            try {
                if(db.Products.Any(p => p.ProductId == int.Parse(productChoice))){
                    product = db.Products.FirstOrDefault(p => p.ProductId == int.Parse(productChoice));
                    Console.WriteLine("Which field do you want to change?");
                    Console.WriteLine("1) ProductName");
                    Console.WriteLine("2) QuantityPerUnit");
                    Console.WriteLine("3) UnitPrice");
                    Console.WriteLine("4) UnitsInStock");
                    Console.WriteLine("5) Discontinued?");
                    Console.WriteLine("6) Category");
                    Console.WriteLine("7) Supplier");
                    string editProductChoice = Console.ReadLine();
                    if(editProductChoice == "1") {
                        string oldName = product.ProductName;
                        Console.WriteLine("Enter a new Product Name:");
                        string pn = Console.ReadLine();
                        if(db.Products.Any(p => p.ProductName == pn)) {
                            logger.Error("Name exists");
                        } else {
                            ValidationContext context = new ValidationContext(product, null, null);
                            List<ValidationResult> results = new List<ValidationResult>();

                            var isValid = Validator.TryValidateObject(product, context, results, true);
                            if (isValid)
                            {
                                db.SaveChanges();
                                logger.Info("Product name updated!");

                            } else {
                                product.ProductName = oldName;
                                db.SaveChanges();
                                Console.WriteLine("Name cannot be empty");
                            }
                        }
                    } else if(editProductChoice == "2") {
                        Console.WriteLine("Enter a new Quantity Per Unit:");
                        string oldQpu = product.QuantityPerUnit;
                        product.QuantityPerUnit = Console.ReadLine();

                        ValidationContext context = new ValidationContext(product, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(product, context, results, true);
                        if (isValid)
                        {
                            db.SaveChanges();
                            logger.Info("Quantity per unit updated!");
                        } else {
                            product.QuantityPerUnit = oldQpu;
                            db.SaveChanges();
                            Console.WriteLine("Quantity per unit cannot be empty");
                        }
                    } else if(editProductChoice == "3") {
                        Console.WriteLine("Enter a new Unit Price:");
                        try {
                            decimal? oldUp = product.UnitPrice;
                            product.UnitPrice = decimal.Parse(Console.ReadLine());
                            
                            ValidationContext context = new ValidationContext(product, null, null);
                            List<ValidationResult> results = new List<ValidationResult>();

                            var isValid = Validator.TryValidateObject(product, context, results, true);
                            if (isValid)
                            {
                                db.SaveChanges();
                                logger.Info("Unit price updated!");
                            } else {
                                product.UnitPrice = oldUp;
                                db.SaveChanges();
                                Console.WriteLine("Unit price is not valid");
                            }
                        } catch(Exception e) {
                            logger.Error(e.Message);
                        }
                    } else if(editProductChoice == "4") {
                        Console.WriteLine("Enter new amount of Units in Stock:");
                        try {
                            short? oldUis = product.UnitsInStock;
                            product.UnitsInStock = short.Parse(Console.ReadLine());
                            
                            ValidationContext context = new ValidationContext(product, null, null);
                            List<ValidationResult> results = new List<ValidationResult>();

                            var isValid = Validator.TryValidateObject(product, context, results, true);
                            if (isValid)
                            {
                                db.SaveChanges();
                                logger.Info("Number of units in stock updated!");
                            } else {
                                product.UnitsInStock = oldUis;
                                db.SaveChanges();
                                Console.WriteLine("Number of units in stock is not valid");
                            }
                        } catch(Exception e) {
                            logger.Error(e.Message);
                        }
                    } else if(editProductChoice == "5") {
                        Console.WriteLine("Is this product discontinued? (y/n):");
                        string disc = Console.ReadLine();
                        if(disc == "y") {
                            product.Discontinued = true;
                            db.SaveChanges();
                            logger.Info("Discontinued status updated!");
                        } else if(disc == "n") {
                            product.Discontinued = false;
                            db.SaveChanges();
                            logger.Info("Discontinued status updated!");
                        } else {
                            Console.WriteLine("Nothing selected! Nothing was changed.");
                        }
                    } else if(editProductChoice == "6") {
                        Console.WriteLine("Which category does this product belong in?");
                        foreach(Category c in db.Categories) {
                            Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
                        }
                        int newCat = 0;
                        try {
                            newCat = int.Parse(Console.ReadLine());
                        } catch(Exception e) {
                            logger.Error(e.Message);
                        }
                        if(db.Categories.Any(c => c.CategoryId == newCat)) {
                            product.Category = db.Categories.FirstOrDefault(c => c.CategoryId == newCat);
                            product.CategoryId = newCat;
                            db.SaveChanges();
                            logger.Info("Category updated!");
                        } else {
                            logger.Error("Invalid option selected.");
                        }
                    } else if(editProductChoice == "7") {
                        Console.WriteLine("Who supplies this product?");
                        foreach(Supplier s in db.Suppliers) {
                            Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
                        }
                        int newSup = 0;
                        try {
                            newSup = int.Parse(Console.ReadLine());
                        } catch(Exception e) {
                            logger.Error(e.Message);
                        }
                        if(newSup > 0 && newSup < db.Suppliers.Count()) {
                            product.Supplier = db.Suppliers.FirstOrDefault(s => s.SupplierId == newSup);
                            product.SupplierId = newSup;
                            db.SaveChanges();
                            logger.Info("Supplier updated!");
                        } else {
                            logger.Error("Invalid option selected.");
                        }
                    } else {
                        logger.Error("Nothing selected!");
                    }
                } else {
                    logger.Error("There are no products with that Id");
                }
                    
            } catch(Exception e) {
                logger.Error(e.Message);
            }
        }
        Console.WriteLine();
    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");
﻿using NLog;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

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
        Console.WriteLine("7) Display Products");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");
        if (choice == "1")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{query.Count()} categories returned");

            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query) {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (choice == "2")
        {
            Category category = new Category();
            Console.WriteLine("\nEnter Category Name:");
            category.CategoryName = Console.ReadLine();

            Console.Clear();
            logger.Info($"Category Name set to \"{category.CategoryName}\"");

            Console.WriteLine("\nEnter the Category Description:");
            category.Description = Console.ReadLine();

            Console.Clear();
            logger.Info($"Category Description set to \"{category.Description}\"");

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
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    logger.Info("Category added!");

                    Console.ForegroundColor = ConsoleColor.White;
                    db.Categories.Add(category);
                    db.SaveChanges();
                }
            }
            else if (!isValid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if (choice == "3")
        {
            try {
                var query = db.Categories.OrderBy(p => p.CategoryId);

                Console.WriteLine("\nSelect the category whose products you want to display:");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{query.Count()} categories returned");

                Console.ForegroundColor = ConsoleColor.Magenta;
                foreach (var item in query) {
                    Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                }

                Console.ForegroundColor = ConsoleColor.White;
                
                int id = int.Parse(Console.ReadLine());
                Console.Clear();
                logger.Info($"CategoryId {id} selected");

                Console.ForegroundColor = ConsoleColor.Magenta;
                Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                Console.WriteLine($"\n{category.CategoryName} - {category.Description} {{");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\t{category.Products.Count} products returned");

                Console.ForegroundColor = ConsoleColor.DarkRed;
                foreach (Product p in category.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");            
                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("}");

                Console.ForegroundColor = ConsoleColor.White;

            } catch(FormatException) {

                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error("Invalid selection");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if (choice == "4")
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{query.Count()} categories returned");

            foreach (var item in query) {

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{item.CategoryName} - {item.Description} {{");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\t{item.Products.Count()} products returned");

                Console.ForegroundColor = ConsoleColor.DarkRed;
                foreach (Product p in item.Products) {
                    Console.WriteLine($"\t{p.ProductName}");
                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("}");
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (choice == "5")
        {
            try {
                Product product = new Product();
                Console.WriteLine("\nEnter Product Name:");
                product.ProductName = Console.ReadLine();

                Console.Clear();
                logger.Info($"Product Name set to \"{product.ProductName}\"");

                Console.WriteLine("\nWhat category is this product in?");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{db.Categories.Count()} categories returned");

                Console.ForegroundColor = ConsoleColor.DarkRed;
                foreach(Category c in db.Categories) {
                    Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
                }

                Console.ForegroundColor = ConsoleColor.White;
                int catId = int.Parse(Console.ReadLine());

                Console.Clear();

                try{
                    product.CategoryId = catId;

                    logger.Info($"Product's Category set to \"{db.Categories.FirstOrDefault(c => c.CategoryId == catId).CategoryName}\"");
                } catch(Exception e) {

                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error(e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine("\nWho supplies this product?");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{db.Suppliers.Count()} suppliers returned");

                Console.ForegroundColor = ConsoleColor.DarkRed;
                foreach(Supplier s in db.Suppliers) {
                    Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
                }

                Console.ForegroundColor = ConsoleColor.White;
                int supId = int.Parse(Console.ReadLine());

                Console.Clear();

                try{
                    product.SupplierId = supId;

                    logger.Info($"Product's Supplier set to \"{db.Suppliers.FirstOrDefault(s => s.SupplierId == supId).CompanyName}\"");
                } catch(Exception e) {

                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error(e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine("\nEnter Quantity Per Unit:");
                product.QuantityPerUnit = Console.ReadLine();

                Console.Clear();

                logger.Info($"Product's Quantity Per Unit set to \"{product.QuantityPerUnit}\"");

                Console.WriteLine("\nEnter Price Per Unit:");
                decimal pPU = decimal.Parse(Console.ReadLine());

                Console.Clear();

                try {
                    product.UnitPrice = pPU;

                    logger.Info($"Product's Unit Price set to \"{product.UnitPrice}\"");
                } catch(Exception e) {

                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error(e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.WriteLine("\nIs this product discontinued? (y/n):");
                string discontinued = Console.ReadLine();

                Console.Clear();

                if(discontinued == "y") {
                    product.Discontinued = true;
                } else if(discontinued == "n") {
                    product.Discontinued = false;
                }

                logger.Info($"Product's Discontinued Status set to \"{product.Discontinued}\"");

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
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        logger.Info("Product added!");
                        Console.ForegroundColor = ConsoleColor.White;

                        db.Products.Add(product);
                        db.SaveChanges();
                    }
                }
                if (!isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (var result in results){
                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
            } catch(FormatException) {

                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error("Invalid selection");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if(choice == "6") {
            try {
                Console.WriteLine("\nWhich product do you want to edit?");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{db.Products.Count()} products returned");

                Console.ForegroundColor = ConsoleColor.DarkRed;
                foreach(Product pr in db.Products) {
                    Console.WriteLine($"{pr.ProductId}) {pr.ProductName}");
                }

                Console.ForegroundColor = ConsoleColor.White;

                string productChoice = Console.ReadLine();

                Console.Clear();

                logger.Info($"ProductId {productChoice} selected");

                int pId = int.Parse(productChoice);

                Product product;
                try {
                    if(db.Products.Any(p => p.ProductId == pId)){
                        product = db.Products.FirstOrDefault(p => p.ProductId == int.Parse(productChoice));
                        Console.WriteLine("\nWhich field do you want to change?\n");
                        Console.WriteLine("1) ProductName");
                        Console.WriteLine("2) QuantityPerUnit");
                        Console.WriteLine("3) UnitPrice");
                        Console.WriteLine("4) UnitsInStock");
                        Console.WriteLine("5) Discontinued?");
                        Console.WriteLine("6) Category");
                        Console.WriteLine("7) Supplier");
                        string editProductChoice = Console.ReadLine();

                        Console.Clear();
                        logger.Info($"Option {editProductChoice} selected");

                        if(editProductChoice == "1") {
                            string oldName = product.ProductName;

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"\nCurrent value: {oldName}");

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("\nEnter a new Product Name:");
                            string pn = Console.ReadLine();

                            Console.Clear();

                            if(db.Products.Any(p => p.ProductName == pn)) {

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error("Name exists!");
                                Console.ForegroundColor = ConsoleColor.White;

                            } else {
                                ValidationContext context = new ValidationContext(product, null, null);
                                List<ValidationResult> results = new List<ValidationResult>();

                                var isValid = Validator.TryValidateObject(product, context, results, true);
                                if (isValid)
                                {
                                    product.ProductName = pn;

                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    logger.Info("Product name updated!");
                                    Console.ForegroundColor = ConsoleColor.White;

                                } else {
                                    product.ProductName = oldName;
                                    db.SaveChanges();

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    foreach (var result in results)
                                    {
                                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                    }
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }
                        } else if(editProductChoice == "2") {
                            string oldQpu = product.QuantityPerUnit;

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"\nCurrent value: {oldQpu}");

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("\nEnter a new Quantity Per Unit:");

                            product.QuantityPerUnit = Console.ReadLine();

                            db.SaveChanges();

                            ValidationContext context = new ValidationContext(product, null, null);
                            List<ValidationResult> results = new List<ValidationResult>();

                            var isValid = Validator.TryValidateObject(product, context, results, true);
                            if (isValid)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                logger.Info("Quantity Per Unit updated");
                                Console.ForegroundColor = ConsoleColor.White;

                            } else {
                                product.QuantityPerUnit = oldQpu;
                                db.SaveChanges();

                                Console.ForegroundColor = ConsoleColor.Red;
                                foreach (var result in results)
                                {
                                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } else if(editProductChoice == "3") {
                            decimal? oldUp = product.UnitPrice;

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"\nCurrent value: {oldUp:0.00}");

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("\nEnter a new Unit Price:");

                            decimal uP = decimal.Parse(Console.ReadLine());

                            Console.Clear();

                            try {
                                product.UnitPrice = uP;

                                db.SaveChanges();
                                
                                ValidationContext context = new ValidationContext(product, null, null);
                                List<ValidationResult> results = new List<ValidationResult>();

                                var isValid = Validator.TryValidateObject(product, context, results, true);
                                if (isValid)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    logger.Info("Unit price updated!");
                                    Console.ForegroundColor = ConsoleColor.White;

                                } else {
                                    product.UnitPrice = oldUp;
                                    db.SaveChanges();

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    foreach (var result in results)
                                    {
                                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                    }
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            } catch(Exception e) {

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error(e.Message);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } else if(editProductChoice == "4") {
                            short? oldUis = product.UnitsInStock;

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"\nCurrent value: {oldUis}");

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("\nEnter new amount of Units in Stock:");

                            try {
                                short? uIS = short.Parse(Console.ReadLine());

                                Console.Clear();

                                product.UnitsInStock = uIS;

                                db.SaveChanges();
                                
                                ValidationContext context = new ValidationContext(product, null, null);
                                List<ValidationResult> results = new List<ValidationResult>();

                                var isValid = Validator.TryValidateObject(product, context, results, true);
                                if (isValid)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    logger.Info("Number of units in stock updated!");
                                    Console.ForegroundColor = ConsoleColor.White;

                                } else {
                                    product.UnitsInStock = oldUis;
                                    db.SaveChanges();

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    foreach (var result in results)
                                    {
                                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                    }
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            } catch(Exception e) {

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error(e.Message);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } else if(editProductChoice == "5") {

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"\nCurrent value: {product.Discontinued}");

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("\nIs this product discontinued? (y/n):");

                            string disc = Console.ReadLine();
                            if(disc == "y") {
                                product.Discontinued = true;
                                db.SaveChanges();

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                logger.Info("Discontinued status updated!");
                                Console.ForegroundColor = ConsoleColor.White;

                            } else if(disc == "n") {
                                product.Discontinued = false;
                                db.SaveChanges();

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                logger.Info("Discontinued status updated!");
                                Console.ForegroundColor = ConsoleColor.White;

                            } else {

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Info("Nothing selected! Nothing was changed.");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } else if(editProductChoice == "6") {

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"\nCurrent value: {db.Categories.FirstOrDefault(c => c.CategoryId == product.CategoryId).CategoryName}");

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("\nWhich category does this product belong in?");

                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            foreach(Category c in db.Categories) {
                                Console.WriteLine($"\t{c.CategoryId}) {c.CategoryName}");
                            }

                            Console.ForegroundColor = ConsoleColor.White;
                            int newCat = 0;
                            try {
                                newCat = int.Parse(Console.ReadLine());
                            } catch(Exception e) {

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error(e.Message);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            if(db.Categories.Any(c => c.CategoryId == newCat)) {
                                product.Category = db.Categories.FirstOrDefault(c => c.CategoryId == newCat);
                                product.CategoryId = newCat;
                                db.SaveChanges();

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                logger.Info("Category updated!");
                                Console.ForegroundColor = ConsoleColor.White;

                            } else {

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error("Invalid option selected.");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } else if(editProductChoice == "7") {

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"\nCurrent value: {db.Suppliers.FirstOrDefault(s => s.SupplierId == product.SupplierId).CompanyName}");

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("\nWho supplies this product?");

                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            foreach(Supplier s in db.Suppliers) {
                                Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
                            }

                            Console.ForegroundColor = ConsoleColor.White;
                            int newSup = 0;
                            try {
                                newSup = int.Parse(Console.ReadLine());
                            } catch(Exception e) {

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error(e.Message);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            if(db.Suppliers.Any(s => s.SupplierId == newSup)) {
                                product.Supplier = db.Suppliers.FirstOrDefault(s => s.SupplierId == newSup);
                                product.SupplierId = newSup;
                                db.SaveChanges();

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                logger.Info("Supplier updated!");
                                Console.ForegroundColor = ConsoleColor.White;

                            } else {

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error("Invalid option selected.");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } else {

                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error("Nothing selected!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    } else {

                        Console.ForegroundColor = ConsoleColor.Red;
                        logger.Error("There are no products with that Id");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                        
                } catch(Exception e) {

                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error(e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                db.SaveChanges();
            } catch(FormatException) {

                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error("Invalid selection");
                Console.ForegroundColor = ConsoleColor.White;
            }
        } else if(choice == "7") {
            Console.WriteLine("\nWhich products would you like to display?\n");
            Console.WriteLine("1) All products");
            Console.WriteLine("2) Discontinued products");
            Console.WriteLine("3) Active products (not discontinued)");

            string dispProdChoice = Console.ReadLine();

            Console.Clear();

            logger.Info($"Option {dispProdChoice} selected");

            IQueryable<Product> query = null;

            if(dispProdChoice == "1") {
                query = db.Products;
            } else if(dispProdChoice == "2") {
                query =
                    from p in db.Products 
                    where p.Discontinued 
                    select p;
            } else if(dispProdChoice == "3") {
                query =
                    from p in db.Products 
                    where !p.Discontinued
                    select p;
            } else {

                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error("Invalid selection, no products displayed.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if(dispProdChoice == "1" || dispProdChoice == "2" || dispProdChoice == "3") {
                Console.WriteLine("\nSelect a product to view or any key to return to main prompt:");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{query.Count()} products returned");

                foreach(Product p in query) {
                    string discontinuedStatus = p.Discontinued ? "Discontinued" : "Active";

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"{p.ProductId}) {p.ProductName}");

                    Console.ForegroundColor = p.Discontinued ? ConsoleColor.Red : ConsoleColor.Green;
                    Console.WriteLine($" ({discontinuedStatus})");
                }

                Console.ForegroundColor = ConsoleColor.White;

                string specProductChoice = Console.ReadLine();

                Console.Clear();

                logger.Info($"ProductId {specProductChoice} selected");

                try {
                    int idComparison = int.Parse(specProductChoice);

                    Product pr;
                    pr = query.FirstOrDefault(p => p.ProductId == idComparison);

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"\n{pr.ProductName} {{");

                    Console.ForegroundColor = ConsoleColor.White;
                    try {
                        Console.WriteLine($"\tProductId: {pr.ProductId}\n");
                        try {
                            Console.WriteLine($"\tCategory: {db.Categories.FirstOrDefault(c => c.CategoryId == pr.CategoryId).CategoryName}");
                        } catch(Exception) {

                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error("Error finding product's Category.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        try {
                            Console.WriteLine($"\tSupplier: {db.Suppliers.FirstOrDefault(s => s.SupplierId == pr.SupplierId).CompanyName}");
                        } catch(Exception) {

                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error("Error finding product's Supplier.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        Console.WriteLine($"\n\tQuantity Per Unit: {pr.QuantityPerUnit}");
                        Console.WriteLine($"\tUnit Price: ${pr.UnitPrice:0.00}");
                        Console.WriteLine($"\tUnits in Stock: {pr.UnitsInStock}");
                        Console.WriteLine($"\tUnits on Order: {pr.UnitsOnOrder}");
                        Console.WriteLine($"\tReorder Level: {pr.ReorderLevel}");

                        string discontinuedString = pr.Discontinued ? "Discontinued" : "Active";

                        Console.ForegroundColor = pr.Discontinued ? ConsoleColor.Red : ConsoleColor.Green;
                        Console.WriteLine($"\n\t{discontinuedString}");

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("}");

                        Console.ForegroundColor = ConsoleColor.White;

                    } catch(Exception e) {

                        Console.ForegroundColor = ConsoleColor.Red;
                        logger.Error(e.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                } catch(Exception) {

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    logger.Info("No product selected.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        } else if(choice == "q") {
            logger.Info("Closing program...");
        } else {

            Console.ForegroundColor = ConsoleColor.Red;
            logger.Error($"\"{choice}\" is not a valid option!");
            Console.ForegroundColor = ConsoleColor.White;
        }
        Console.WriteLine();
    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    logger.Error(ex.Message);
    Console.ForegroundColor = ConsoleColor.White;
}

logger.Info("Program ended");
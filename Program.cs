using NLog;
using NWConsole.Model;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

Console.Clear();
Console.WriteLine("Hello World!");

// -- NLOG SETUP --

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "/nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();

// -- PROGAM START --

logger.Info("Program started\n");

try
{
    var db = new NWContext();
    string choice;
    do
    {
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Display all Categories and their related products");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Add new Category");
        Console.WriteLine("5) Edit Category");
        Console.WriteLine("6) Display Products");
        Console.WriteLine("7) Add new Product");
        Console.WriteLine("8) Edit Product");
        Console.WriteLine("\"q\" to quit");

        choice = Console.ReadLine();

        Console.Clear();

        logger.Info($"Option {choice} selected");

        if(choice == "1")
        {
            // -- DISPLAY ALL CATEGORIES --
            try 
            {
                // query gets all categories
                var query = db.Categories.OrderBy(c => c.CategoryId);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{query.Count()} categories returned");

                Console.ForegroundColor = ConsoleColor.Magenta;
                foreach(Category c in query) 
                {
                    Console.WriteLine($"{c.CategoryName} - {c.Description}");
                }

                Console.ForegroundColor = ConsoleColor.White;
            } 
            catch(Exception e) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error($"Error displaying categories: {e.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if(choice == "2")
        {
            // -- DISPLAY ALL CATEGORIES AND THEIR RELATED PRODUCTS --
            try 
            {
                // query gets all categories and their Products column
                var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{query.Count()} categories returned");

                // displays each category along with its Products list
                foreach (Category c in query) 
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"{c.CategoryName} - {c.Description} {{");

                    // query selets all active products in this category
                    var pQuery = from p in c.Products where !p.Discontinued orderby p.ProductId select p;

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine($"\t{pQuery.Count()} products returned");

                    foreach (Product p in pQuery)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write($"\t{p.ProductName}");

                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(" (Active)");
                    }
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("}");
                }

                Console.ForegroundColor = ConsoleColor.White;
            }
            catch(Exception e) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error($"Error displaying categories or products: {e.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if(choice == "3")
        {
            // -- DISPLAY CATEGORY AND RELATED PRODUCTS --
            try 
            {
                // query gets all categories and their Products column
                var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);

                Console.WriteLine("\nSelect the category whose products you want to display:");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{query.Count()} categories returned");

                Console.ForegroundColor = ConsoleColor.Magenta;

                // displays all categories
                foreach (Category c in query) 
                {
                    Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
                }
                Console.ForegroundColor = ConsoleColor.White;
                
                // user selects which category to view a product from
                int id = int.Parse(Console.ReadLine());

                Console.Clear();

                logger.Info($"CategoryId {id} selected");

                Console.ForegroundColor = ConsoleColor.Magenta;

                // selects the category that the user chose
                Category category = query.FirstOrDefault(c => c.CategoryId == id);

                // displays the category and all its Products
                Console.WriteLine($"\n{category.CategoryName} - {category.Description} {{");

                // query gets all active products in this category
                var pQuery = from p in category.Products where !p.Discontinued orderby p.ProductId select p;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\t{pQuery.Count()} products returned");

                Console.ForegroundColor = ConsoleColor.DarkRed;

                foreach (Product p in pQuery)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"\t{p.ProductName}");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(" (Active)");          
                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("}");

                Console.ForegroundColor = ConsoleColor.White;

            } 
            catch(FormatException) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error("Invalid selection");
                Console.ForegroundColor = ConsoleColor.White;

            } 
            catch(Exception e) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error($"Error displaying categories or products: {e.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if(choice == "4")
        {
            // -- ADD NEW CATEGORY --
            try
            {
                Category category = new();

                Console.WriteLine("\nEnter Category Name:");

                // user input for category name
                category.CategoryName = Console.ReadLine();

                Console.Clear();

                logger.Info($"Category Name set to \"{category.CategoryName}\"");

                Console.WriteLine("\nEnter the Category Description:");

                // user input for category description
                category.Description = Console.ReadLine();

                Console.Clear();

                logger.Info($"Category Description set to \"{category.Description}\"");

                // validate category
                ValidationContext context = new(category, null, null);
                List<ValidationResult> results = new();

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
                        try
                        {
                            // add category to database and save changes
                            db.Categories.Add(category);
                            db.SaveChanges();

                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            logger.Info("Category added!");

                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        catch(Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error($"Error saving category to database: {e.Message}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                if (!isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (var result in results)
                    {
                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error($"Error creating new category: {e.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if(choice == "5")
        {
            // -- EDIT CATEGORY --
            try 
            {
                // query gets all categories
                var query = db.Categories.OrderBy(c => c.CategoryId);

                Console.WriteLine("\nWhich category do you want to edit?");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{db.Categories.Count()} categories returned");

                Console.ForegroundColor = ConsoleColor.DarkRed;

                // display all categories
                foreach(Category c in db.Categories) 
                {
                    Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
                }
                Console.ForegroundColor = ConsoleColor.White;

                // user selects which category to edit
                int cId = int.Parse(Console.ReadLine());

                Console.Clear();

                logger.Info($"CategoryId {cId} selected");

                // selects the category that the user chose
                if(query.Any(c => c.CategoryId == cId))
                {
                    Category category = query.FirstOrDefault(c => c.CategoryId == cId);

                    Console.WriteLine("\nWhich field do you want to change?\n");
                    Console.WriteLine("1) CategoryName");
                    Console.WriteLine("2) Description");

                    // user selects which field of the selected category they want to edit 
                    string editCatField = Console.ReadLine();

                    Console.Clear();

                    logger.Info($"Option {editCatField} selected");

                    if(editCatField == "1") 
                    {
                        // -- CATEGORY NAME --

                        // holds on to the current CategoryName in case the new one is invalid
                        string oldName = category.CategoryName;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nCurrent value: {oldName}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nEnter a new Category Name:");

                        // user selects a new name for the selected category
                        string cn = Console.ReadLine();

                        Console.Clear();

                        // checks if name exists
                        if(query.Any(c => c.CategoryName == cn)) 
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error("Name exists!");
                            Console.ForegroundColor = ConsoleColor.White;
                        } 
                        else 
                        {
                            // if name doesn't exist, set CategoryName to new name
                            category.CategoryName = cn;

                            // validate length of CategoryName
                            ValidationContext context = new(category, null, null);
                            List<ValidationResult> results = new();

                            var isValid = Validator.TryValidateObject(category, context, results, true);
                            if (isValid)
                            {
                                try
                                {
                                    // save changes to database 
                                    db.SaveChanges();

                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    logger.Info("Category Name updated!");

                                    Console.ForegroundColor = ConsoleColor.White;
                                } 
                                catch(Exception e)
                                {
                                    category.CategoryName = oldName;

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    logger.Error($"Error saving category to database: {e.Message}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            } else {
                                // if category is invalid, set CategoryName back to original
                                category.CategoryName = oldName;

                                Console.ForegroundColor = ConsoleColor.Red;
                                foreach (var result in results)
                                {
                                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                    } 
                    else if(editCatField == "2") 
                    {
                        // -- CATEGORY DESCRIPTION --

                        // holds on to the current Description in case the new one is invalid
                        string oldDesc = category.Description;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nCurrent value: {oldDesc}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nEnter a new Description:");

                        // user selects a new description for the selected category
                        category.Description = Console.ReadLine();

                        Console.Clear();

                        // validate category
                        ValidationContext context = new(category, null, null);
                        List<ValidationResult> results = new();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            try
                            {
                                // save changes to database
                                db.SaveChanges();

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                logger.Info("Category Description updated!");

                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            catch(Exception e) 
                            {
                                category.Description = oldDesc;

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error($"Error saving category to database: {e.Message}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } 
                        else 
                        {
                            // if category is invalid, set Description back to original
                            category.Description = oldDesc;

                            Console.ForegroundColor = ConsoleColor.Red;
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    } 
                    else 
                    {
                        // -- INVALID SELECTION --
                        Console.ForegroundColor = ConsoleColor.Red;
                        logger.Error("Nothing selected!");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                } 
                else 
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error("There are no products with that Id");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            } 
            catch(FormatException) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error("Invalid selection");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if(choice == "6") 
        {
            // -- DISPLAY PRODUCTS --
            try
            {
                // query gets all products
                var query = db.Products.OrderBy(p => p.ProductId);

                Console.WriteLine("\nWhich products would you like to display?\n");
                Console.WriteLine("1) All products");
                Console.WriteLine("2) Discontinued products");
                Console.WriteLine("3) Active products (not discontinued)");

                // user selects the product list that they would like to display (all, discontinued or active)
                string dChoice = Console.ReadLine();

                Console.Clear();

                logger.Info($"Option {dChoice} selected");

                // updates query to only contain the necessary products
                if(dChoice == "1") 
                {
                    // nothing needs to be done since the query is already defined above
                }

                // -- DISCONTINUED PRODUCTS --
                else if(dChoice == "2") query = from p in query where p.Discontinued orderby p.ProductId select p;
                
                // -- ACTIVE PRODUCTS --
                else if(dChoice == "3") query = from p in query where !p.Discontinued orderby p.ProductId select p;

                // makes sure selection is valid before running this section
                if(dChoice == "1" || dChoice == "2" || dChoice == "3") 
                {
                    Console.WriteLine("\nSelect a product to view or any key to return to main prompt:");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n{query.Count()} products returned");

                    // displays each product in the query along with its Discontinued status
                    foreach(Product p in query) 
                    {
                        string discontinuedStatus = p.Discontinued ? "Discontinued" : "Active";

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write($"{p.ProductId}) {p.ProductName}");

                        Console.ForegroundColor = p.Discontinued ? ConsoleColor.Red : ConsoleColor.DarkGreen;
                        Console.WriteLine($" ({discontinuedStatus})");
                    }
                    Console.ForegroundColor = ConsoleColor.White;

                    // user selects which product they want to view
                    int pId = int.Parse(Console.ReadLine());

                    Console.Clear();

                    logger.Info($"ProductId {pId} selected");

                    // checks to make sure the product chosen exists before continuing
                    if(query.Any(p => p.ProductId == pId)) 
                    {
                        // selects the product that the user chose
                        Product product = query.FirstOrDefault(p => p.ProductId == pId);

                        Console.ForegroundColor = ConsoleColor.DarkRed;

                        // displays the product
                        Console.WriteLine($"\n{product.ProductName} {{");

                        Console.ForegroundColor = ConsoleColor.White;
                    
                        Console.WriteLine($"\tProductId: {product.ProductId}\n");
                        try 
                        {
                            Console.WriteLine($"\tCategory: {db.Categories.FirstOrDefault(c => c.CategoryId == product.CategoryId).CategoryName}");
                        } 
                        // here to ensure there are no crashes when retrieving the Category field
                        catch(Exception e) 
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error($"Error finding product's Category: {e.Message}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        try 
                        {
                            Console.WriteLine($"\tSupplier: {db.Suppliers.FirstOrDefault(s => s.SupplierId == product.SupplierId).CompanyName}");
                        } 
                        // here to ensure there are no crashes when retrieving the Supplier field
                        catch(Exception) 
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error("Error finding product's Supplier.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        Console.WriteLine($"\n\tQuantity Per Unit: {product.QuantityPerUnit}");
                        Console.WriteLine($"\tUnit Price: ${product.UnitPrice:0.00}");
                        Console.WriteLine($"\tUnits in Stock: {product.UnitsInStock}");
                        Console.WriteLine($"\tUnits on Order: {product.UnitsOnOrder}");
                        Console.WriteLine($"\tReorder Level: {product.ReorderLevel}");

                        string discontinuedString = product.Discontinued ? "Discontinued" : "Active";

                        Console.ForegroundColor = product.Discontinued ? ConsoleColor.Red : ConsoleColor.DarkGreen;
                        Console.WriteLine($"\n\t{discontinuedString}");

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("}");

                        Console.ForegroundColor = ConsoleColor.White;
                    } 
                    else 
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        logger.Info("No product selected");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else 
                {
                    // -- INVALID SELECTION --
                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error("Invalid selection, no products displayed");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch(FormatException) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error("Invalid selection");
                Console.ForegroundColor = ConsoleColor.White;
            }
        } 
        else if(choice == "7")
        {
            // -- ADD NEW PRODUCT --
            try 
            {
                Product product = new();

                Console.WriteLine("\nEnter Product Name:");

                // user input for Product Name
                product.ProductName = Console.ReadLine();

                Console.Clear();

                logger.Info($"Product Name set to \"{product.ProductName}\"");

                Console.WriteLine("\nWhat category is this product in?");

                Console.ForegroundColor = ConsoleColor.Green;

                // query gets all categories
                var cQuery = db.Categories.OrderBy(c => c.CategoryId);

                Console.WriteLine($"\n{cQuery.Count()} categories returned");

                Console.ForegroundColor = ConsoleColor.DarkRed;

                // displays all categories
                foreach(Category c in cQuery) 
                {
                    Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
                }

                Console.ForegroundColor = ConsoleColor.White;

                // user selects the category they want to relate their new product to
                int catId = int.Parse(Console.ReadLine());

                Console.Clear();

                // checks if the category exists with the given value before continuing
                if(cQuery.Any(c => c.CategoryId == catId))
                {
                    // set CategoryId to the value the user selected
                    product.Category = cQuery.FirstOrDefault(c => c.CategoryId == catId);
                    product.CategoryId = catId;

                    logger.Info($"Product's Category set to \"{product.Category.CategoryName}\"");
                    
                    Console.WriteLine("\nWho supplies this product?");

                    Console.ForegroundColor = ConsoleColor.Green;

                    // query gets all suppliers
                    var sQuery = db.Suppliers.OrderBy(s => s.SupplierId);

                    Console.WriteLine($"\n{sQuery.Count()} suppliers returned");

                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    // displays all suppliers
                    foreach(Supplier s in sQuery) 
                    {
                        Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
                    }

                    Console.ForegroundColor = ConsoleColor.White;

                    // user selects the supplier they want to relate their new product to
                    int supId = int.Parse(Console.ReadLine());

                    Console.Clear();

                    if(sQuery.Any(s => s.SupplierId == supId))
                    {
                        // set Supplier to the value the user selected
                        product.Supplier = sQuery.FirstOrDefault(s => s.SupplierId == supId);
                        product.SupplierId = supId;

                        logger.Info($"Product's Supplier set to \"{sQuery.FirstOrDefault(s => s.SupplierId == product.SupplierId).CompanyName}\"");
                        
                        Console.WriteLine("\nEnter Quantity Per Unit:");

                        // user selects the Quantity per Unit for the new product
                        // (optional according to NWContext.cs) - no validation needed
                        product.QuantityPerUnit = Console.ReadLine();

                        Console.Clear();

                        logger.Info($"Product's Quantity Per Unit set to \"{product.QuantityPerUnit}\"");

                        Console.WriteLine("\nEnter Price Per Unit:");

                        // user selects the unit price of the new product
                        decimal uP = decimal.Parse(Console.ReadLine());

                        Console.Clear();

                        product.UnitPrice = uP;

                        logger.Info($"Product's Unit Price set to \"{product.UnitPrice}\"");

                        Console.WriteLine("\nIs this product discontinued? (y/n):");

                        // user selects if the new product is discontinued or not
                        string discontinued = Console.ReadLine();

                        Console.Clear();

                        if(discontinued == "y") product.Discontinued = true;
                        else if(discontinued == "n") product.Discontinued = false;

                        logger.Info($"Product's Discontinued Status set to \"{product.Discontinued}\"");

                        // validate the new product
                        ValidationContext context = new(product, null, null);
                        List<ValidationResult> results = new();

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
                                try
                                {
                                    // save product to database
                                    db.Products.Add(product);
                                    db.SaveChanges();

                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    logger.Info("Product added!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                catch(Exception e)
                                {
                                    try
                                    {
                                        // if an exception is thrown, try to remove product, assuming it was added.
                                        db.Remove(product);
                                    }
                                    catch(Exception) {}

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    logger.Error($"Error saving product to database: {e.Message}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }
                        }
                        if (!isValid)
                        {
                            try
                            {
                                // if product is invalid, try to remove product, assuming it was added.
                                db.Remove(product);
                            }
                            catch(Exception) {}

                            Console.ForegroundColor = ConsoleColor.Red;
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                } 
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error($"There is no Category with Id \"{catId}\"");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            } 
            catch(FormatException) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error("Invalid selection");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if(choice == "8") 
        {
            // -- EDIT PRODUCT --
            try 
            {
                // query gets all products
                var query = db.Products.OrderBy(p => p.ProductId);

                Console.WriteLine("\nWhich product do you want to edit?");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{db.Products.Count()} products returned");

                Console.ForegroundColor = ConsoleColor.DarkRed;

                // displays all products
                foreach(Product p in query) 
                {
                    Console.WriteLine($"{p.ProductId}) {p.ProductName}");
                }
                Console.ForegroundColor = ConsoleColor.White;

                // user selects the product they want to edit
                int pId = int.Parse(Console.ReadLine());

                Console.Clear();

                logger.Info($"ProductId {pId} selected");

                // checks if the product exists at the given id before continuing
                if(query.Any(p => p.ProductId == pId))
                {
                    // selects the product that the user chose
                    Product product = query.FirstOrDefault(p => p.ProductId == pId);

                    Console.WriteLine("\nWhich field do you want to change?\n");
                    Console.WriteLine("1) ProductName");
                    Console.WriteLine("2) QuantityPerUnit");
                    Console.WriteLine("3) UnitPrice");
                    Console.WriteLine("4) UnitsInStock");
                    Console.WriteLine("5) Discontinued?");
                    Console.WriteLine("6) Category");
                    Console.WriteLine("7) Supplier");

                    // user selects which field they want to edit
                    string pField = Console.ReadLine();

                    Console.Clear();

                    logger.Info($"Option {pField} selected");

                    if(pField == "1") 
                    {
                        // -- PRODUCT NAME --

                        // holds on to the current name in case the new one is invalid
                        string oldName = product.ProductName;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nCurrent value: {oldName}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nEnter a new Product Name:");

                        // user selects a new ProductName
                        string pn = Console.ReadLine();

                        Console.Clear();

                        // checks if the name exists
                        if(query.Any(p => p.ProductName == pn)) 
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error("Name exists!");
                            Console.ForegroundColor = ConsoleColor.White;
                        } 
                        else 
                        {
                            // if name doesn't exist, set ProductName to the new name
                            product.ProductName = pn;

                            //validate product
                            ValidationContext context = new(product, null, null);
                            List<ValidationResult> results = new();

                            var isValid = Validator.TryValidateObject(product, context, results, true);
                            if (isValid)
                            {
                                try
                                {
                                    // save changes to database
                                    db.SaveChanges();

                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    logger.Info("Product name updated!");

                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                catch(Exception e)
                                {
                                    product.ProductName = oldName;

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    logger.Error($"Error saving product to database: {e.Message}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            } 
                            else 
                            {
                                // if product is invalid, set ProductName back to old name
                                product.ProductName = oldName;

                                Console.ForegroundColor = ConsoleColor.Red;
                                foreach (var result in results)
                                {
                                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                    } 
                    else if(pField == "2") 
                    {
                        // -- QUANTITY PER UNIT --

                        // stores current Quantity per Unit in case the new value is invalid
                        string oldQpu = product.QuantityPerUnit;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nCurrent value: {oldQpu}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nEnter a new Quantity Per Unit:");

                        // user selects a new Quantity per Unit for the product
                        product.QuantityPerUnit = Console.ReadLine();

                        Console.Clear();

                        // validate Quantity per Unit
                        ValidationContext context = new(product, null, null);
                        List<ValidationResult> results = new();

                        var isValid = Validator.TryValidateObject(product, context, results, true);
                        if (isValid)
                        {
                            try
                            {
                                // save changes to database
                                db.SaveChanges();

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                logger.Info("Quantity Per Unit updated!");

                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            catch(Exception e)
                            {
                                product.QuantityPerUnit = oldQpu;

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error($"Error saving product to database: {e.Message}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } 
                        else 
                        {
                            // if product is invalid, set Quantity per Unit back to its old value
                            product.QuantityPerUnit = oldQpu;

                            Console.ForegroundColor = ConsoleColor.Red;
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    } 
                    else if(pField == "3") 
                    {
                        // -- UNIT PRICE --

                        // stores current Unit Price in case the new value is invalid
                        decimal? oldUp = product.UnitPrice;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nCurrent value: {oldUp:0.00}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nEnter a new Unit Price:");

                        // user selects a new Unit Price for the product
                        product.UnitPrice = decimal.Parse(Console.ReadLine());

                        Console.Clear();
                        
                        // validate product
                        ValidationContext context = new(product, null, null);
                        List<ValidationResult> results = new();

                        var isValid = Validator.TryValidateObject(product, context, results, true);
                        if (isValid)
                        {
                            try
                            {
                                // save changes to database
                                db.SaveChanges();

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                logger.Info("Unit price updated!");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            catch(Exception e)
                            {
                                product.UnitPrice = oldUp;

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error($"Error saving product to database: {e.Message}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } 
                        else 
                        {
                            // if product is invalid, set UnitPrice back to its old value
                            product.UnitPrice = oldUp;

                            Console.ForegroundColor = ConsoleColor.Red;
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    } 
                    else if(pField == "4") 
                    {
                        // -- UNITS IN STOCK --

                        // stores the current UnitsInStock in case the new value is invalid
                        short? oldUis = product.UnitsInStock;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nCurrent value: {oldUis}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nEnter new amount of Units in Stock:");

                        // user selects a new UnitsInStock value for the product
                        product.UnitsInStock = short.Parse(Console.ReadLine());

                        Console.Clear();

                        // validate product                        
                        ValidationContext context = new(product, null, null);
                        List<ValidationResult> results = new();

                        var isValid = Validator.TryValidateObject(product, context, results, true);
                        if (isValid)
                        {
                            try
                            {   
                                // save changes to database
                                db.SaveChanges();
                            
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                logger.Info("Number of units in stock updated!");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            catch(Exception e)
                            {
                                product.UnitsInStock = oldUis;

                                Console.ForegroundColor = ConsoleColor.Red;
                                logger.Error($"Error saving product to database: {e.Message}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } 
                        else 
                        {   
                            // if product is invalid, set UnitsInStock back to its old value
                            product.UnitsInStock = oldUis;

                            Console.ForegroundColor = ConsoleColor.Red;
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    } 
                    else if(pField == "5") 
                    {
                        // -- DISCONTINUED STATUS --

                        // stores the current discontinued status in case the new one is invalid (somehow)
                        bool oldDisc = product.Discontinued;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nCurrent value: {product.Discontinued}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nIs this product discontinued? (y/n):");

                        // user selects if the product is discontinued or not
                        string disc = Console.ReadLine();

                        Console.Clear();

                        // set discontinued status to true or false depending on what the user selected
                        if(disc == "y") product.Discontinued = true;
                        else if(disc == "n") product.Discontinued = false;
                        
                        // checks if input was any valid value before continuing
                        if(disc == "y" || disc == "n")
                        {
                            // validate product                        
                            ValidationContext context = new(product, null, null);
                            List<ValidationResult> results = new();

                            var isValid = Validator.TryValidateObject(product, context, results, true);
                            if (isValid)
                            {
                                try
                                {   
                                    // save changes to database
                                    db.SaveChanges();
                                
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    logger.Info("Discontinued status updated!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                catch(Exception e)
                                {
                                    product.Discontinued = oldDisc;

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    logger.Error($"Error saving product to database: {e.Message}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            } 
                            else 
                            {   
                                // if product is invalid, set Discontinued status back to its old value
                                product.Discontinued = oldDisc;

                                Console.ForegroundColor = ConsoleColor.Red;
                                foreach (var result in results)
                                {
                                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                        else 
                        {
                            // -- INVALID SELECTION --
                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Info("Nothing selected! Nothing was changed.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    } 
                    else if(pField == "6") 
                    {
                        // -- CATEGORY --

                        // query gets all categories
                        var cQuery = db.Categories.OrderBy(c => c.CategoryId);

                        // stores current CategoryId value in case the new one is invalid
                        int? oldCId = product.CategoryId;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nCurrent value: {product.Category.CategoryName}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nWhich category does this product belong in?");

                        Console.ForegroundColor = ConsoleColor.DarkRed;

                        // displays all categories
                        foreach(Category c in cQuery) 
                        {
                            Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
                        }

                        Console.ForegroundColor = ConsoleColor.White;

                        // user selects a new category for the product
                        int newCat = int.Parse(Console.ReadLine());

                        Console.Clear();
                        
                        // checks to make sure that a category exists at the given Id before continuing
                        if(cQuery.Any(c => c.CategoryId == newCat))
                        {
                            // set Category to the value the user selected
                            product.Category = cQuery.FirstOrDefault(c => c.CategoryId == newCat);
                            product.CategoryId = newCat;

                            // validate product                        
                            ValidationContext context = new(product, null, null);
                            List<ValidationResult> results = new();

                            var isValid = Validator.TryValidateObject(product, context, results, true);
                            if (isValid)
                            {
                                try
                                {   
                                    // save changes to database
                                    db.SaveChanges();
                                
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    logger.Info("Category updated!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                catch(Exception e)
                                {
                                    product.Category = cQuery.FirstOrDefault(c => c.CategoryId == oldCId);
                                    product.CategoryId = oldCId;

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    logger.Error($"Error saving product to database: {e.Message}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            } 
                            else 
                            {   
                                // if product is invalid, set Category back to its old value
                                product.Category = cQuery.FirstOrDefault(c => c.CategoryId == oldCId);
                                product.CategoryId = oldCId;

                                Console.ForegroundColor = ConsoleColor.Red;
                                foreach (var result in results)
                                {
                                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } 
                        else 
                        {
                            // -- INVALID SELECTION --
                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error("Invalid option selected.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    } 
                    else if(pField == "7") 
                    {
                        // -- SUPPLIER --

                        // query gets all suppliers
                        var sQuery = db.Suppliers.OrderBy(s => s.SupplierId);

                        // stores current SupplierId in case the new one is invalid
                        int? oldSId = product.SupplierId;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nCurrent value: {sQuery.FirstOrDefault(s => s.SupplierId == product.SupplierId).CompanyName}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nWho supplies this product?");

                        Console.ForegroundColor = ConsoleColor.DarkRed;

                        // displays all suppliers
                        foreach(Supplier s in sQuery) 
                        {
                            Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
                        }

                        Console.ForegroundColor = ConsoleColor.White;

                        // user selects a new Supplier for the product
                        int newSup = int.Parse(Console.ReadLine());

                        Console.Clear();
                       
                        // checks to make sure that a supplier exists at the given Id before continuing
                        if(sQuery.Any(s => s.SupplierId == newSup)) 
                        {
                            // set Supplier to the new value
                            product.Supplier = sQuery.FirstOrDefault(s => s.SupplierId == newSup);
                            product.SupplierId = newSup;
                            
                            // validate product                        
                            ValidationContext context = new(product, null, null);
                            List<ValidationResult> results = new();

                            var isValid = Validator.TryValidateObject(product, context, results, true);
                            if (isValid)
                            {
                                try
                                {   
                                    // save changes to database
                                    db.SaveChanges();
                                
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    logger.Info("Supplier updated!");

                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                catch(Exception e)
                                {
                                    product.Supplier = sQuery.FirstOrDefault(s => s.SupplierId == oldSId);
                                    product.SupplierId = oldSId;

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    logger.Error($"Error saving product to database: {e.Message}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            } 
                            else 
                            {   
                                // if product is invalid, set SupplierId back to its old value
                                product.Supplier = sQuery.FirstOrDefault(s => s.SupplierId == oldSId);
                                product.SupplierId = oldSId;

                                Console.ForegroundColor = ConsoleColor.Red;
                                foreach (var result in results)
                                {
                                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } 
                        else 
                        {
                            // -- INVALID SELECTION --
                            Console.ForegroundColor = ConsoleColor.Red;
                            logger.Error("There is no Supplier with that Id!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    } 
                    else 
                    {
                        // -- INVALID SELECTION --
                        Console.ForegroundColor = ConsoleColor.Red;
                        logger.Error("Nothing selected!");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                } 
                else 
                {
                    // -- INVALID SELECTION --
                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error("There is no Product with that Id!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                try
                {
                    // save changes before exiting the method
                    db.SaveChanges();
                }
                catch(Exception e) 
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error($"Error saving product to database: {e.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            } 
            catch(FormatException) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                logger.Error("Invalid selection");
                Console.ForegroundColor = ConsoleColor.White;
            }
        } 
        else if(choice == "q") 
        {
            // -- QUIT --
            logger.Info("Closing program...");
        } 
        else 
        {
            // -- INVALID SELECTION --
            Console.ForegroundColor = ConsoleColor.Red;
            logger.Error($"\"{choice}\" is not a valid option!");
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        Console.WriteLine();

    } 
    while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    // -- ANY UNCAUGHT EXCEPTIONS (THERE SHOULDN'T BE ANY) --
    Console.ForegroundColor = ConsoleColor.Red;
    logger.Error("Unhandled exception caught: " + ex.Message);
    Console.ForegroundColor = ConsoleColor.White;
}

logger.Info("Program ended");
namespace Ownorent.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Ownorent.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Ownorent.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            /* Separator Separator Separator Separator Separator Separator Separator */

            if (!context.Warehouses.Any(c => c.WarehouseName == "Makati Main"))
            {
                context.Warehouses.Add(new Warehouse()
                {
                    WarehouseName = "Makati Main",
                    Location = "333 Sen. Gil J. Puyat Ave, Makati, 1200 Metro Manila",
                    IsDefault = true
                });
            }

            /* Separator Separator Separator Separator Separator Separator Separator */

            if (!context.Settings.Any(c => c.Code == "PLATFORM_TAX_BUY"))
            {
                context.Settings.Add(new Setting()
                {
                    Code = "PLATFORM_TAX_BUY",
                    Value = "2",
                    Description = "Percentage Taxed to Seller earnings when a Customer buys a product.",
                    IsActive = true
                });
            }

            if (!context.Settings.Any(c => c.Code == "PLATFORM_TAX_DAILY_RENT"))
            {
                context.Settings.Add(new Setting()
                {
                    Code = "PLATFORM_TAX_DAILY_RENT",
                    Value = "2",
                    Description = "Percentage Taxed to Seller earnings when a Customer pays for rent.",
                    IsActive = true
                });
            }

            if (!context.Settings.Any(c => c.Code == "PLATFORM_TAX_RENT_TO_OWN"))
            {
                context.Settings.Add(new Setting()
                {
                    Code = "PLATFORM_TAX_RENT_TO_OWN",
                    Value = "2",
                    Description = "Percentage Taxed to Seller earnings when a Customer pays rent to own bill.",
                    IsActive = true
                });
            }

            if (!context.Settings.Any(c => c.Code == "PLATFORM_TAX_CASHOUT"))
            {
                context.Settings.Add(new Setting()
                {
                    Code = "PLATFORM_TAX_CASHOUT",
                    Value = "0.8",
                    Description = "Percentage Taxed to Seller cashout amount whenever withdraws money.",
                    IsActive = true
                });
            }

            if (!context.Settings.Any(c => c.Code == "COMPUTE_DAILY_RENT_PERCENTAGE"))
            {
                context.Settings.Add(new Setting()
                {
                    Code = "COMPUTE_DAILY_RENT_PERCENTAGE",
                    Value = "0.2",
                    Description = "Used in computation, Computed Daily Rent Price = PRICE * COMPUTE_DAILY_RENT_PERCENTAGE.",
                    IsActive = true
                });
            }

            if (!context.Settings.Any(c => c.Code == "COMPUTE_MAXIMUM_DEPRECIATION_PRICE_PERCENTAGE"))
            {
                context.Settings.Add(new Setting()
                {
                    Code = "COMPUTE_MAXIMUM_DEPRECIATION_PRICE_PERCENTAGE",
                    Value = "9",
                    Description = "Used in computation, if a product is over its lifespan, then Computed Price = PRICE * COMPUTE_MAXIMUM_DEPRECIATION_PRICE_PERCENTAGE",
                    IsActive = true
                });
            }

            if (!context.Settings.Any(c => c.Code == "COMPUTE_MAXIMUM_DEPRECIATION_RENT_PRICE_PERCENTAGE"))
            {
                context.Settings.Add(new Setting()
                {
                    Code = "COMPUTE_MAXIMUM_DEPRECIATION_RENT_PRICE_PERCENTAGE",
                    Value = "0.09",
                    Description = "Used in computation, if a product is over its lifespan, then Computed Daily Rent Price = PRICE * COMPUTE_MAXIMUM_DEPRECIATION_RENT_PRICE_PERCENTAGE.",
                    IsActive = true
                });
            }

            /* Separator Separator Separator Separator Separator Separator Separator */

            if (!context.RentToOwnPaymentTerms.Any(c => c.Months == 3))
            {
                context.RentToOwnPaymentTerms.Add(new RentToOwnPaymentTerm()
                {
                    Months = 3,
                    InterestRate = 6.0014F
                });
            }
            if (!context.RentToOwnPaymentTerms.Any(c => c.Months == 6))
            {
                context.RentToOwnPaymentTerms.Add(new RentToOwnPaymentTerm()
                {
                    Months = 6,
                    InterestRate = 10.8014F
                });
            }
            if (!context.RentToOwnPaymentTerms.Any(c => c.Months == 9))
            {
                context.RentToOwnPaymentTerms.Add(new RentToOwnPaymentTerm()
                {
                    Months = 9,
                    InterestRate = 15.9326F
                });
            }
            if (!context.RentToOwnPaymentTerms.Any(c => c.Months == 12))
            {
                context.RentToOwnPaymentTerms.Add(new RentToOwnPaymentTerm()
                {
                    Months = 12,
                    InterestRate = 21.0056F
                });
            }
            if (!context.RentToOwnPaymentTerms.Any(c => c.Months == 18))
            {
                context.RentToOwnPaymentTerms.Add(new RentToOwnPaymentTerm()
                {
                    Months = 18,
                    InterestRate = 36.0098F
                });
            }
            if (!context.RentToOwnPaymentTerms.Any(c => c.Months == 24))
            {
                context.RentToOwnPaymentTerms.Add(new RentToOwnPaymentTerm()
                {
                    Months = 24,
                    InterestRate = 51.608F
                });
            }
            /* Separator Separator Separator Separator Separator Separator Separator */

            if (!context.Categories.Any(c => c.CategoryName == "ELECTRONICS - Smartphone - Android")) {
                context.Categories.Add(new Category()
                {
                    CategoryName = "ELECTRONICS - Smartphone - Android",
                    LastModifiedBy = "oadmin@mailinator.com",
                    UsefulLifeSpan = 3
                });
            }
            if (!context.Categories.Any(c => c.CategoryName == "ELECTRONICS - Smartphone - iPhone"))
            {
                context.Categories.Add(new Category()
                {
                    CategoryName = "ELECTRONICS - Smartphone - iPhone",
                    LastModifiedBy = "oadmin@mailinator.com",
                    UsefulLifeSpan = 5
                });
            }
            if (!context.Categories.Any(c => c.CategoryName == "ELECTRONICS - Home Appliance - Refrigerator"))
            {
                context.Categories.Add(new Category()
                {
                    CategoryName = "ELECTRONICS - Home Appliance - Refrigerator",
                    LastModifiedBy = "oadmin@mailinator.com",
                    UsefulLifeSpan = 9
                });
            }
            if (!context.Categories.Any(c => c.CategoryName == "ELECTRONICS - Home Appliance - Airconditioner"))
            {
                context.Categories.Add(new Category()
                {
                    CategoryName = "ELECTRONICS - Home Appliance - Airconditioner",
                    LastModifiedBy = "oadmin@mailinator.com",
                    UsefulLifeSpan = 7
                });
            }
            if (!context.Categories.Any(c => c.CategoryName == "Vehicles - Private Car"))
            {
                context.Categories.Add(new Category()
                {
                    CategoryName = "Vehicles - Private Car",
                    LastModifiedBy = "oadmin@mailinator.com",
                    UsefulLifeSpan = 6
                });
            }

            /* Separator Separator Separator Separator Separator Separator Separator */

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false,
            };

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

            if (!roleManager.RoleExists("Admin"))
            {
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Seller"))
            {
                var role = new IdentityRole();
                role.Name = "Seller";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Customer"))
            {
                var role = new IdentityRole();
                role.Name = "Customer";
                roleManager.Create(role);
            }

            if (!context.Users.Any(u => u.UserName == "oadmin@mailinator.com")) {
                var user = new ApplicationUser
                {
                    FirstName = "Administrator",
                    UserName = "oadmin@mailinator.com",
                    Email = "oadmin@mailinator.com",
                    EmailConfirmed = true,
                };
                userManager.Create(user, "ownorent@123");
                userManager.AddToRole(user.Id, "Admin");
            }

            if (!context.Users.Any(u => u.UserName == "s1@mailinator.com")) {
                var user = new ApplicationUser
                {
                    FirstName = "Seller 1",
                    UserName = "s1@mailinator.com",
                    Email = "s1@mailinator.com",
                    EmailConfirmed = true,
                };
                userManager.Create(user, "ownorent@123");
                userManager.AddToRole(user.Id, "Seller");
            }

            if (!context.Users.Any(u => u.UserName == "s2@mailinator.com")) {
                var user = new ApplicationUser
                {
                    FirstName = "Seller 2",
                    UserName = "s2@mailinator.com",
                    Email = "s2@mailinator.com",
                    EmailConfirmed = true,
                };
                userManager.Create(user, "ownorent@123");
                userManager.AddToRole(user.Id, "Seller");
            }

            if (!context.Users.Any(u => u.UserName == "c1@mailinator.com")) {
                var user = new ApplicationUser
                {
                    FirstName = "Customer 1",
                    UserName = "c1@mailinator.com",
                    Email = "c1@mailinator.com",
                    EmailConfirmed = true,
                };
                userManager.Create(user, "ownorent@123");
                userManager.AddToRole(user.Id, "Customer");
            }

            if (!context.Users.Any(u => u.UserName == "c2@mailinator.com")) {
                var user = new ApplicationUser
                {
                    FirstName = "Customer 2",
                    UserName = "c2@mailinator.com",
                    Email = "c2@mailinator.com",
                    EmailConfirmed = true,
                };
                userManager.Create(user, "ownorent@123");
                userManager.AddToRole(user.Id, "Customer");
            }
        }
    }
}

using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Ownorent.Models
{
    // SERVER INFO / DATABASE INFO
    // own123456 / ownorent$123456
    // ownorent-server.database.windows.net

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public string MobileNumber { get; set; }
        public string MobileNumberCode { get; set; }

        public int AccountType { get; set; }
        public int AccountStatus { get; set; }

        public string ProfilePictureLocation { get; set; } // upload profile pic

        public virtual List<UserAttachment> Attachments { get; set; }
        public virtual List<ProductTemplate> Products { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
        public virtual List<Address> Addresses { get; set; }

        public string ConfirmationCode { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<UserAttachment> UserAttachments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductTemplate> ProductTemplates { get; set; }
        public DbSet<ProductTemplateNote> ProductTemplateNotes { get; set; }
        public DbSet<ProductTemplateAttachment> ProductTemplateAttachments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductNote> ProductNotes { get; set; }
        public DbSet<ProductAttachment> ProductAttachments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RentToOwnPaymentTerm> RentToOwnPaymentTerms { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<TransactionGroup> TransactionGroups { get; set; }
        public DbSet<TransactionGroupPaymentAttempt> TransactionGroupPaymentAttempts { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
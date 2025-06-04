

using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context
{
    public class RioContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<IdentityUser>
    {
        public RioContext(DbContextOptions<RioContext> options) : base(options)
        {
        }

        public virtual DbSet<BranchMaster> BranchMasters { get; set; }
        public virtual DbSet<BranchMaster_CustomerMapping> BranchMaster_CustomerMappings { get; set; }
        public virtual DbSet<BranchMaster_SalesmanMapping> BranchMaster_SalesmanMappings { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CompanyMaster> CompanyMasters { get; set; }
        public virtual DbSet<CompanyMaster_BranchMasterMapping> CompanyMaster_BranchMasterMappings { get; set; }
        public virtual DbSet<CompanyMaster_DepartmentMapping> CompanyMaster_DepartmentMapping { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Item_CategoryMapping> Item_CategoryMappings { get; set; }
        public virtual DbSet<SalesInvoice> SalesInvoices { get; set; }
        public virtual DbSet<Salesman> Salesmans { get; set; }
        public virtual DbSet<SalesReturn> SalesReturns { get; set; }
        public virtual DbSet<BranchMaster_CategoryMapping> BranchMaster_CategoryMappings { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CartItemsMapping> CartItemsMappings { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }
        public virtual DbSet<Log> Logs { get;set; }
    }
}

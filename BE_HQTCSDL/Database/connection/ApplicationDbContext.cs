using BE_HQTCSDL.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<ProductPromotion> ProductPromotions => Set<ProductPromotion>();
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<SealedProduct> SealedProducts => Set<SealedProduct>();
        public DbSet<TcgCard> TcgCards => Set<TcgCard>();
        public DbSet<TcgSet> TcgSets => Set<TcgSet>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Voucher> Vouchers => Set<Voucher>();
        public DbSet<Wishlist> Wishlists => Set<Wishlist>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductCategory>()
                .HasIndex(x => new { x.ProductId, x.CategoryId })
                .IsUnique();

            modelBuilder.Entity<Wishlist>()
                .HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(x => new { x.UserId, x.ProductId, x.OrderId })
                .IsUnique();
        }
    }
}
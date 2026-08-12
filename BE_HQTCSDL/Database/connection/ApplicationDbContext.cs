using BE_HQTCSDL.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Database
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
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<TcgCard> TcgCards => Set<TcgCard>();
        public DbSet<TcgSet> TcgSets => Set<TcgSet>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
        public DbSet<Voucher> Vouchers => Set<Voucher>();
        public DbSet<Wishlist> Wishlists => Set<Wishlist>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

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

            modelBuilder.Entity<Inventory>()
                .HasIndex(x => x.ProductId)
                .IsUnique();

            modelBuilder.Entity<UserAddress>()
                .HasIndex(x => x.UserId);

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.ToTable("CONVERSATIONS", table =>
                {
                    table.HasCheckConstraint(
                        "CK_CONVERSATIONS_STATUS",
                        "\"STATUS\" IN ('AI_ACTIVE', 'WAITING_STAFF', 'STAFF_ACTIVE', 'CLOSED')");
                    table.HasCheckConstraint(
                        "CK_CONVERSATIONS_TYPE",
                        "\"TYPE\" IN ('GENERAL_SUPPORT', 'ORDER_SUPPORT')");
                });

                entity.HasIndex(x => x.CustomerId);
                entity.HasIndex(x => x.OrderId);
                entity.HasIndex(x => x.UpdatedAt);
                entity.HasIndex(x => x.Status);

                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.CustomerConversations)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.AssignedStaff)
                    .WithMany(x => x.AssignedConversations)
                    .HasForeignKey(x => x.AssignedStaffId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Order)
                    .WithMany(x => x.SupportConversations)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("CHAT_MESSAGES", table =>
                {
                    table.HasCheckConstraint(
                        "CK_CHAT_MESSAGES_SENDER_TYPE",
                        "\"SENDER_TYPE\" IN ('CUSTOMER', 'STAFF', 'AI', 'SYSTEM')");
                    table.HasCheckConstraint(
                        "CK_CHAT_MESSAGES_SENDER_ID",
                        "((\"SENDER_TYPE\" IN ('CUSTOMER', 'STAFF') AND \"SENDER_ID\" IS NOT NULL) OR " +
                        "(\"SENDER_TYPE\" IN ('AI', 'SYSTEM') AND \"SENDER_ID\" IS NULL))");
                });

                entity.HasIndex(x => x.ConversationId);
                entity.HasIndex(x => new { x.ConversationId, x.CreatedAt });

                entity.HasOne(x => x.Conversation)
                    .WithMany(x => x.Messages)
                    .HasForeignKey(x => x.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Sender)
                    .WithMany(x => x.ChatMessages)
                    .HasForeignKey(x => x.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

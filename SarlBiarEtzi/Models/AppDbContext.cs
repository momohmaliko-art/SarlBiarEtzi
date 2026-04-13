namespace SarlBiarEtzi.Models;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<UserLoginTemp> UserLoginTemps { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessage>().ToTable("chat_messages");
        modelBuilder.Entity<Contact>().ToTable("contacts");
        modelBuilder.Entity<Review>().ToTable("reviews");
        modelBuilder.Entity<UserLoginTemp>().ToTable("user_login_temp");

        // 👇 الجديد
        modelBuilder.Entity<ChatRoom>().ToTable("chat_rooms");
    }
}

using Microsoft.EntityFrameworkCore;
using TestSystem.Core.Entity;

namespace TestSystem.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opt) : base(opt)
    {
        
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<ClassRoom> ClassRooms { get; set; }
    public DbSet<UserClassRoom> UserClassRooms { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<Package> Packages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserClassRoom>()
            .HasKey(uc => new { uc.UserId, uc.ClassRoomId });

        modelBuilder.Entity<Package>()
            .HasKey(p => new { p.Id, p.UserId, p.TaskId });
    
        modelBuilder.Entity<UserClassRoom>()
            .Property(uc => uc.UserId)
            .IsRequired();
    
        modelBuilder.Entity<UserClassRoom>()
            .Property(uc => uc.ClassRoomId)
            .IsRequired();
    
        modelBuilder.Entity<User>()
            .HasMany(u => u.UserClassRooms)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);
    
        modelBuilder.Entity<ClassRoom>()
            .HasMany(cr => cr.UserClassRooms)
            .WithOne(x => x.ClassRoom)
            .HasForeignKey(x => x.ClassRoomId);
    
        modelBuilder.Entity<TaskEntity>()
            .Property(t => t.ClassRoomId)
            .IsRequired();
    
        modelBuilder.Entity<ClassRoom>()
            .HasMany(cr => cr.Tasks)
            .WithOne(x => x.ClassRoom)
            .HasForeignKey(x => x.ClassRoomId);
    
        modelBuilder.Entity<Package>()
            .Property(p => p.TaskId)
            .IsRequired();
    
        modelBuilder.Entity<Package>()
            .Property(p => p.UserId)
            .IsRequired();
    
        modelBuilder.Entity<Package>()
            .HasOne<TaskEntity>(p => p.Task)
            .WithMany(x => x.Packages)
            .HasForeignKey(p => p.TaskId);
    
        modelBuilder.Entity<Package>()
            .HasOne<User>(p => p.User)
            .WithMany(x => x.Packages)
            .HasForeignKey(p => p.UserId);
    
        base.OnModelCreating(modelBuilder);
    }
}
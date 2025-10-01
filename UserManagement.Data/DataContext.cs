using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;

namespace UserManagement.Data;

public class DataContext : DbContext, IDataContext
{
    public DataContext() => Database.EnsureCreated();
    public DataContext(DbContextOptions<DataContext> options) : base(options)
        => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseInMemoryDatabase("UserManagement.Data.DataContext");
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>().HasData(
        [
            new User { Id = 1, Forename = "Peter", Surname = "Loew", Email = "ploew@example.com", IsActive = true, DateOfBirth = new DateTime(1985, 3, 15) },
            new User { Id = 2, Forename = "Benjamin Franklin", Surname = "Gates", Email = "bfgates@example.com", IsActive = true, DateOfBirth = new DateTime(1976, 7, 22) },
            new User { Id = 3, Forename = "Castor", Surname = "Troy", Email = "ctroy@example.com", IsActive = false, DateOfBirth = new DateTime(1962, 11, 8) },
            new User { Id = 4, Forename = "Memphis", Surname = "Raines", Email = "mraines@example.com", IsActive = true, DateOfBirth = new DateTime(1974, 9, 12) },
            new User { Id = 5, Forename = "Stanley", Surname = "Goodspeed", Email = "sgodspeed@example.com", IsActive = true, DateOfBirth = new DateTime(1969, 1, 30) },
            new User { Id = 6, Forename = "H.I.", Surname = "McDunnough", Email = "himcdunnough@example.com", IsActive = true, DateOfBirth = new DateTime(1957, 4, 18) },
            new User { Id = 7, Forename = "Cameron", Surname = "Poe", Email = "cpoe@example.com", IsActive = false, DateOfBirth = new DateTime(1964, 12, 5) },
            new User { Id = 8, Forename = "Edward", Surname = "Malus", Email = "emalus@example.com", IsActive = false, DateOfBirth = new DateTime(1983, 6, 27) },
            new User { Id = 9, Forename = "Damon", Surname = "Macready", Email = "dmacready@example.com", IsActive = false, DateOfBirth = new DateTime(1958, 10, 14) },
            new User { Id = 10, Forename = "Johnny", Surname = "Blaze", Email = "jblaze@example.com", IsActive = true, DateOfBirth = new DateTime(1972, 2, 19) },
            new User { Id = 11, Forename = "Robin", Surname = "Feld", Email = "rfeld@example.com", IsActive = true, DateOfBirth = new DateTime(1966, 8, 3) },
        ]);

        // Configure ChangeLogEntry to NOT have a foreign key relationship
        // This allows logs to persist even when the referenced user is deleted
        model.Entity<ChangeLogEntry>().Ignore(c => c.User);
    }

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<ChangeLogEntry> ChangeLogs { get; set; } = default!;

    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        => base.Set<TEntity>();

    public void Create<TEntity>(TEntity entity) where TEntity : class
    {
        base.Add(entity);
        SaveChanges();
    }

    public void UpdateAndSave<TEntity>(TEntity entity) where TEntity : class
    {
        base.Update(entity);
        SaveChanges();
    }

    public void Delete<TEntity>(TEntity entity) where TEntity : class
    {
        base.Remove(entity);
        SaveChanges();
    }
    public TEntity? GetById<TEntity>(long id) where TEntity : class
        => base.Set<TEntity>().Find(id);
}

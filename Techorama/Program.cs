using Microsoft.EntityFrameworkCore;

namespace Techorama;

internal class Program
{
    static void Main(string[] args)
    {
        using (var context = new MyContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}

public class Dog
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Owner Owner { get; set; }
}
public class Owner
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ICollection<Dog> Dogs { get; set; }
}
class MyContext : DbContext
{
    public DbSet<Dog> Dogs { get; set; }
    public DbSet<Owner> Owners { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer(@"Data Source=.\;Initial Catalog=techorama;Integrated Security=SSPI;ConnectRetryCount=0;Trust Server Certificate=True;");
        optionsBuilder.LogTo(Console.WriteLine);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
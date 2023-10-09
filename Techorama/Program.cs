using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Options;

namespace Techorama;

internal class Program
{
    static void Main(string[] args)
    {
        using (var context = new MyContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            //context.Dogs.ToList();
            ////context.Set<DogOwner>().select
            //context.Add(new Owner() { FirstName = "T", LastName = "T", SSN = "T" });
            //context.SaveChanges();
            //context.ChangeTracker.Clear();
            //context.Owners.ToList();

            var ssn = "";
            var data = context.Owners.Where(x => EF.Property<string>(x, "SSN") == ssn).ToList();
            context.Entry(data[0]).Property<string>("SSN").CurrentValue = ssn;
        }
    }
}

public class Dog
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public Owner Owner { get; set; }
    //public int? OwnerId { get; set; }
    //public ICollection<Owner> Owners { get; set; }
    //public DateOnly OwnerAssigned { get; set; }
}
public class Owner
{
    private string _ssn;

    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string SSN
    {
        get => _ssn;
        set
        {
            Validate(value);
            _ssn = value;

            static void Validate(string value) { }
        }
    }
    public ICollection<Dog> Dogs { get; set; }
}
//public class DogOwner
//{
//    public int DogId { get; set; }
//    public int OwnerId { get; set; }
//    public DateOnly Assigned { get; set; }
//}
class MyContext : DbContext
{
    public DbSet<Dog> Dogs => Set<Dog>();
    public DbSet<Owner> Owners => Set<Owner>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(@"Data Source=.\;Initial Catalog=techorama;Integrated Security=SSPI;ConnectRetryCount=0;Trust Server Certificate=True;");
            optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Owner>(builder =>
        {
            builder.Property(x => x.Id)
                .UseHiLo();
            builder.Property<string>("SSN")
                .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                .HasField("_ssn");
        });

        modelBuilder.SharedTypeEntity<Dictionary<string, object>>("Cat", builder =>
        {
            builder.ToTable("Cats");
            builder.Property<int>("Id");
            builder.Property<string>("Name");
            builder.Property<DateTimeOffset>("DOB");
        });
        //modelBuilder.Entity<Cat>(builder =>
        //{
        //    builder.ToTable("Cats");
        //    builder.IndexerProperty()
        //    builder.Property<int>("Id");
        //    builder.Property<string>("Name");
        //    builder.Property<DateOnly>("DOB");
        //});

        modelBuilder.ApplyConfiguration(new DogConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        //configurationBuilder.Conventions.(x => cndsjkasd)
    }
}

class DogConfiguration : IEntityTypeConfiguration<Dog>
{
    public void Configure(EntityTypeBuilder<Dog> builder)
    {
        builder.ToTable("T_Dogs");
        builder.HasKey(x => x.Id);
        builder.HasAlternateKey(x => x.Id);
        builder.Property(x => x.Id)
            .UseHiLo();
        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired()
            .IsFixedLength(false)
            .IsUnicode()
            .HasColumnName("Name");
        //builder.HasMany(x => x.Owners)
        //    .WithMany(x => x.Dogs)
        //    .UsingEntity<DogOwner>();
    }
}

class StringConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entity in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var prop in entity.GetProperties())
            {
                if (prop.ClrType == typeof(string))
                {
                    prop.SetMaxLength(50);
                }
            }
        }
    }
}

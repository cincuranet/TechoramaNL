using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

            //var ssn = "";
            //var data = context.Owners.Where(x => EF.Property<string>(x, "SSN") == ssn).ToList();
            //context.Entry(data[0]).Property<string>("SSN").CurrentValue = ssn;

            //context.Dogs
            //    .ToList();
            //context.Owners.Where(x => x.ShippingAddress.Street == "vndsfjkvlf").ToList();
            context.Owners.Add(new Owner()
            {
                FirstName = "T",
                LastName = "T",
                ShippingAddress = new Address(),
                Duration = new Duration(10),
            });
            context.SaveChanges();
            var owner = context.Owners/*.Where(x => x.Duration == new Duration(11))*/.First();
            owner.Duration = new Duration(10);
            //owner.FirstName = "T";
            context.SaveChanges();
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
    public bool Deleted { get; set; }
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
    public Address InvoiceAddress { get; set; }
    public Address ShippingAddress { get; set; }
    public Duration Duration { get; set; }
}
public class Duration
{
    int _seconds;

    public Duration(int seconds)
    {
        _seconds = seconds;
    }

    public int Seconds => _seconds;
    public TimeSpan TimeSpan => TimeSpan.FromSeconds(_seconds);
}
public class DurationConverter : ValueConverter<Duration, int>
{
    public DurationConverter()
        : base(d => d.Seconds * 1000, x => new Duration(x / 1000), null)
    { }
}
public class DurationComparer : ValueComparer<Duration>
{
    public DurationComparer()
        : base((lhs, rhs) => lhs.Seconds == rhs.Seconds, x => x.Seconds.GetHashCode())
    { }
}
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
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

            //builder.OwnsOne(x => x.InvoiceAddress);
            //builder.OwnsOne(x => x.ShippingAddress);
            builder.OwnsOne(x => x.InvoiceAddress).ToJson();
            builder.OwnsOne(x => x.ShippingAddress).ToJson();

            builder.Property(x => x.Duration)
                .HasConversion(new DurationConverter(), new DurationComparer());
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
        builder.HasQueryFilter(x => !x.Deleted);
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

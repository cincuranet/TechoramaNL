using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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
            //context.Owners.Add(new Owner()
            //{
            //    FirstName = "T",
            //    LastName = "T",
            //    ShippingAddress = new Address(),
            //    Duration = new Duration(10),
            //});
            //context.SaveChanges();
            //var owner = context.Owners/*.Where(x => x.Duration == new Duration(11))*/
            //    .TagWithCallSite()
            //    .First();
            //owner.Duration = new Duration(10);
            ////owner.FirstName = "T";
            //context.SaveChanges();

            //context.Dogs
            //    .Where(x => context.FooBar(x.Id).Any() && EF.Functions.Like(x.Name, "A%Foo"))
            //    /*.FromSql($"select Name, 1 as Count from T_Dogs where 1=1")*/
            //    //.OrderBy(x => x.Id)
            //    .ToList();

            //var dogs = context.Dogs
            //    //.Include(x => x.Owner)
            //    .Where(x => !EF.Functions.Like(x.Name, "A%Foo"))
            //    .ToList();
            //var dogs = context.Dogs
            //    .AsSplitQuery()
            //    .Select(x => new
            //    {
            //        DogName = x.Name,
            //        OwnerName = x.Owner.FirstName + " " + x.Owner.LastName,
            //    })
            //    .ToList();
            //foreach (var dog in dogs)
            //{
            //    //context.Entry(dog).Reference(x => x.Owner).Load();
            //    Console.WriteLine(dog.OwnerName);
            //}
            //context.Set<Vehicle>().ToList();
            //context.Set<Car>().ToList();
            context.Dogs.TemporalAsOf(DateTime.UtcNow) /*TemporalFromTo(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow)*/
                .Where(x => x.Name.Contains("A"))
                .ToList();
        }
    }
}

public class Dog
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public virtual Owner Owner { get; set; }
    //public int? OwnerId { get; set; }
    //public ICollection<Owner> Owners { get; set; }
    //public DateOnly OwnerAssigned { get; set; }
    public bool Deleted { get; set; }

    public Dog()
    { }
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
    public virtual ICollection<Dog> Dogs { get; set; }
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
//class FooBar
//{
//    public string Name { get; set; }
//    public int Count { get; set; }
//}
public abstract class Vehicle
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Speed { get; set; }
}
public class Car : Vehicle
{
    public int EngineSize { get; set; }
}
public class Plane : Vehicle
{
    public int MTOW { get; set; }
    public int NumberOfPassengers { get; set; }
}
class MyContext : DbContext
{
    [DbFunction]
    public IQueryable<Dog> FooBar(int id) =>
        FromExpression(() => FooBar(id));

    public DbSet<Dog> Dogs => Set<Dog>();
    public DbSet<Owner> Owners => Set<Owner>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(@"Data Source=.\;Initial Catalog=techorama;Integrated Security=SSPI;ConnectRetryCount=0;Trust Server Certificate=True;",
                options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.EnableSensitiveDataLogging();
            //optionsBuilder.UseLazyLoadingProxies();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region -
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
            //builder.HasOne<Owner>("Owner")
            //.WithMany("Cats")
            //.HasForeignKey()
        });
        //modelBuilder.Entity<Cat>(builder =>
        //{
        //    builder.ToTable("Cats");
        //    builder.IndexerProperty()
        //    builder.Property<int>("Id");
        //    builder.Property<string>("Name");
        //    builder.Property<DateOnly>("DOB");
        //});

        //modelBuilder.Entity<FooBar>()
        //    .ToView("MyView")
        //    .HasNoKey();

        //modelBuilder.HasDbFunction()
        #endregion

        //modelBuilder
        //    .Entity<Vehicle>(builder =>
        //    {
        //        builder.UseTphMappingStrategy();
        //        builder.ToTable("TPH_Vehicles");
        //    })
        //    .Entity<Car>(builder =>
        //    {
        //        builder.HasDiscriminator<string>("D").HasValue("CA");
        //    })
        //    .Entity<Plane>(builder =>
        //    {
        //        builder.HasDiscriminator<string>("D").HasValue("PL");
        //    });
        //modelBuilder
        //    .Entity<Vehicle>(builder =>
        //    {
        //        builder.UseTptMappingStrategy();
        //        builder.ToTable("TPT_Vehicles");
        //    })
        //    .Entity<Car>(builder =>
        //    {
        //        builder.ToTable("TPT_Cars");
        //    })
        //    .Entity<Plane>(builder =>
        //    {
        //        builder.ToTable("TPT_Planes");
        //    });
        //modelBuilder
        //    .Entity<Vehicle>(builder =>
        //    {
        //        builder.UseTpcMappingStrategy();
        //    })
        //    .Entity<Car>(builder =>
        //    {
        //        builder.ToTable("TPC_Cars");
        //    })
        //    .Entity<Plane>(builder =>
        //    {
        //        builder.ToTable("TPC_Planes");
        //    });

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
        builder.ToTable("T_Dogs", b => b.IsTemporal());
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

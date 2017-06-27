using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace DataCollector.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("DataCollector")
        {
        }

        public DbSet<DataEntity> Data { get; set; }
        public DbSet<CollectionType> CollectionType { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
        }

        public static AppDbContext Create()
        {
            return new AppDbContext();
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var newException = new FormattedDbEntityValidationException(e);
                throw newException;
            }
        }
    }

    public class DataEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string JsonObject { get; set; }
        [Required]
        public DateTime CreatedOn { get; set; }
        [Required]
        public int IdCollectionType { get; set; }
        [ForeignKey("IdCollectionType")]
        public CollectionType CollectionType { get; set; }
    }

    public class CollectionType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
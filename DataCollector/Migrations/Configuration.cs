using DataCollector.Data;

namespace DataCollector.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DataCollector.Data.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DataCollector.Data.AppDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.CollectionType.AddOrUpdate(
                c =>c.Name, new CollectionType()
                {
                    Name = "Wiki"
                },
                new CollectionType()
                {
                    Name = "PDF"
                },
                new CollectionType()
                {
                    Name = "Sheet"
                },
                new CollectionType()
                {
                    Name = "Nav"
                }
                );
        }
    }
}

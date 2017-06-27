namespace DataCollector.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CollectionTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DataEntities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JsonObject = c.String(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IdCollectionType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CollectionTypes", t => t.IdCollectionType)
                .Index(t => t.IdCollectionType);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataEntities", "IdCollectionType", "dbo.CollectionTypes");
            DropIndex("dbo.DataEntities", new[] { "IdCollectionType" });
            DropTable("dbo.DataEntities");
            DropTable("dbo.CollectionTypes");
        }
    }
}

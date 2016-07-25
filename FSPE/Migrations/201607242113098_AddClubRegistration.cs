namespace FSPE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClubRegistration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClubRegistrations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentName = c.String(),
                        EmailAddress = c.String(),
                        PhoneNumber = c.String(),
                        ChildName = c.String(),
                        Grade = c.String(),
                        Teacher = c.String(),
                        ClubId = c.Int(nullable: false),
                        ChildDispositionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChildDispositions", t => t.ChildDispositionId, cascadeDelete: true)
                .ForeignKey("dbo.Clubs", t => t.ClubId, cascadeDelete: true)
                .Index(t => t.ClubId)
                .Index(t => t.ChildDispositionId);
            
            CreateTable(
                "dbo.ChildDispositions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            Sql("INSERT INTO ChildDispositions (Name) Values ('After Care')");
            Sql("INSERT INTO ChildDispositions (Name) Values ('Parent Pickup')");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClubRegistrations", "ClubId", "dbo.Clubs");
            DropForeignKey("dbo.ClubRegistrations", "ChildDispositionId", "dbo.ChildDispositions");
            DropIndex("dbo.ClubRegistrations", new[] { "ChildDispositionId" });
            DropIndex("dbo.ClubRegistrations", new[] { "ClubId" });
            DropTable("dbo.ChildDispositions");
            DropTable("dbo.ClubRegistrations");
        }
    }
}

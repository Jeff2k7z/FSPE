namespace FSPE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRegistrationLocks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RegistrationLocks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClubID = c.Int(nullable: false),
                        LockExpiration = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RegistrationLocks");
        }
    }
}

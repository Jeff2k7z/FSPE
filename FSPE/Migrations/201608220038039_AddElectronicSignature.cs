namespace FSPE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddElectronicSignature : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClubRegistrations", "ElectronicSignature", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ClubRegistrations", "ElectronicSignature");
        }
    }
}

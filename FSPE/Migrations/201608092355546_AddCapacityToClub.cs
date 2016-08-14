namespace FSPE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCapacityToClub : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clubs", "Capacity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clubs", "Capacity");
        }
    }
}

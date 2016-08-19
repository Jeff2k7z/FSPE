namespace FSPE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRegistrationDateandIsPaidtoClubRegistrations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClubRegistrations", "RegistrationDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.ClubRegistrations", "IsPaid", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ClubRegistrations", "IsPaid");
            DropColumn("dbo.ClubRegistrations", "RegistrationDate");
        }
    }
}

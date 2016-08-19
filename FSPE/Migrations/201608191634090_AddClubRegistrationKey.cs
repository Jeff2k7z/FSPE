namespace FSPE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClubRegistrationKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClubRegistrations", "RegistrationKey", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ClubRegistrations", "RegistrationKey");
        }
    }
}

namespace FSPE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClubRegistrationChange : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ClubRegistrations", "ParentName", c => c.String(nullable: false));
            AlterColumn("dbo.ClubRegistrations", "EmailAddress", c => c.String(nullable: false));
            AlterColumn("dbo.ClubRegistrations", "PhoneNumber", c => c.String(nullable: false));
            AlterColumn("dbo.ClubRegistrations", "ChildName", c => c.String(nullable: false));
            AlterColumn("dbo.ClubRegistrations", "Grade", c => c.String(nullable: false));
            AlterColumn("dbo.ClubRegistrations", "Teacher", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ClubRegistrations", "Teacher", c => c.String());
            AlterColumn("dbo.ClubRegistrations", "Grade", c => c.String());
            AlterColumn("dbo.ClubRegistrations", "ChildName", c => c.String());
            AlterColumn("dbo.ClubRegistrations", "PhoneNumber", c => c.String());
            AlterColumn("dbo.ClubRegistrations", "EmailAddress", c => c.String());
            AlterColumn("dbo.ClubRegistrations", "ParentName", c => c.String());
        }
    }
}

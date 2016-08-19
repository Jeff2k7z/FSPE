namespace FSPE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCouponCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClubRegistrations", "CouponCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ClubRegistrations", "CouponCode");
        }
    }
}

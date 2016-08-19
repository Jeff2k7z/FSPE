namespace FSPE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLockRegistrationKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegistrationLocks", "LockKey", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegistrationLocks", "LockKey");
        }
    }
}

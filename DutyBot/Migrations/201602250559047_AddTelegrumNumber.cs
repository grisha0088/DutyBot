namespace DutyBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTelegrumNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "TlgNumber", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "TlgNumber");
        }
    }
}

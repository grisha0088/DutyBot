namespace DutyBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changetipe : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "State", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "State", c => c.Int());
        }
    }
}

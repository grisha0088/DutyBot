namespace DutyBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LogsAndParametrs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        MessageTipe = c.String(),
                        UserId = c.Int(nullable: false),
                        Operation = c.String(),
                        Exception = c.String(),
                        AddInfo = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Parametrs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Parametrs");
            DropTable("dbo.Logs");
        }
    }
}

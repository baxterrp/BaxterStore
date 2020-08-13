using FluentMigrator;

namespace BaxterStore.Data.Migrations
{
    [Migration(1)]
    public class Version1 : Migration
    {
        private static readonly string _userTableName = "Users";

        public override void Down()
        {
            Delete.Table(_userTableName);
        }

        public override void Up()
        {
            Create.Table(_userTableName)
                .WithColumn("Id").AsString().NotNullable().PrimaryKey()
                .WithColumn("Email").AsString().NotNullable()
                .WithColumn("FirstName").AsString().NotNullable()
                .WithColumn("LastName").AsString().NotNullable()
                .WithColumn("Password").AsString().NotNullable();
        }
    }
}

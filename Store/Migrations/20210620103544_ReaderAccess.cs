using Microsoft.EntityFrameworkCore.Migrations;

namespace Store.Migrations
{
    public partial class ReaderAccess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE LOGIN xaporeader WITH PASSWORD = N'XAPOreader123#@!'");
            migrationBuilder.Sql("CREATE USER xaporeader  FOR LOGIN xaporeader WITH DEFAULT_SCHEMA=[dbo];");
            migrationBuilder.Sql("exec sp_addrolemember N'db_datareader', N'xaporeader'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

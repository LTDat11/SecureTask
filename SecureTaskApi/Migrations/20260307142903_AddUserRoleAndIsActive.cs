using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureTaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleAndIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""IsActive"" boolean NOT NULL DEFAULT false;
                ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""Role"" character varying(20) NOT NULL DEFAULT '';
            ");

            // Backfill existing rows so they are treated as active regular users
            migrationBuilder.Sql(@"
                UPDATE ""Users"" SET ""IsActive"" = true WHERE ""IsActive"" = false;
                UPDATE ""Users"" SET ""Role"" = 'User' WHERE ""Role"" = '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}

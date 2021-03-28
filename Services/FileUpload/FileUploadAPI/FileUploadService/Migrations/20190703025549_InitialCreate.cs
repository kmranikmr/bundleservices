using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileUploadService.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Project_File",
                columns: table => new
                {
                    Project_File_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Project_Id = table.Column<int>(nullable: true),
                    User_Id = table.Column<int>(nullable: false),
                    Source_Type_Id = table.Column<int>(nullable: false),
                    File_Name = table.Column<string>(nullable: true),
                    File_Path = table.Column<string>(nullable: true),
                    Source_Configuration = table.Column<string>(nullable: true),
                    Upload_Date = table.Column<DateTime>(nullable: false),
                    Is_Active = table.Column<bool>(nullable: true),
                    Is_Deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_File", x => x.Project_File_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_File");
        }
    }
}

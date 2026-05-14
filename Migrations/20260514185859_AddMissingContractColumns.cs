using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechMove.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingContractColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "ServiceRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "ServiceRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RequestNumber",
                table: "ServiceRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SignedAgreementPath",
                table: "Contracts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SignedAgreementFileName",
                table: "Contracts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AgreementUploadDate",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractNumber",
                table: "Contracts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ContractValueUSD",
                table: "Contracts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ContractValueZAR",
                table: "Contracts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Contracts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Contracts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "RequestNumber",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "AgreementUploadDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ContractNumber",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ContractValueUSD",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ContractValueZAR",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Contracts");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "SignedAgreementPath",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SignedAgreementFileName",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}

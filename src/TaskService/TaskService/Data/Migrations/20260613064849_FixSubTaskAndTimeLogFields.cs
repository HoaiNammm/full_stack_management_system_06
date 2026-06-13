using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskService.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSubTaskAndTimeLogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "SubTasks");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TaskTimeLogs",
                newName: "LoggedBy");

            migrationBuilder.RenameColumn(
                name: "LoggedAt",
                table: "TaskTimeLogs",
                newName: "LoggedDate");

            // CreatedAt may already exist if migration was partially applied — add only if missing
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'TaskTimeLogs') AND name = N'CreatedAt'
                )
                    ALTER TABLE [TaskTimeLogs] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
            ");

            // SprintId already exists in the DB — skip AddColumn, only create the index if missing
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Tasks_SprintId' AND object_id = OBJECT_ID(N'Tasks')
                )
                    CREATE INDEX [IX_Tasks_SprintId] ON [Tasks] ([SprintId]);
            ");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedTo",
                table: "SubTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SubTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SubTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SubTasks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Tasks_SprintId' AND object_id = OBJECT_ID(N'Tasks')
                )
                    DROP INDEX [IX_Tasks_SprintId] ON [Tasks];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'TaskTimeLogs') AND name = N'CreatedAt'
                )
                    ALTER TABLE [TaskTimeLogs] DROP COLUMN [CreatedAt];
            ");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "SubTasks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SubTasks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SubTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SubTasks");

            migrationBuilder.RenameColumn(
                name: "LoggedDate",
                table: "TaskTimeLogs",
                newName: "LoggedAt");

            migrationBuilder.RenameColumn(
                name: "LoggedBy",
                table: "TaskTimeLogs",
                newName: "UserId");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "SubTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

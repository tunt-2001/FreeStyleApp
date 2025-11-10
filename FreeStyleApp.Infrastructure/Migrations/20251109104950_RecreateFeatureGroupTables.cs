using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeStyleApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecreateFeatureGroupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạo lại FeatureGroups table nếu chưa có
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FeatureGroups')
                BEGIN
                    CREATE TABLE [FeatureGroups] (
                        [Id] uniqueidentifier NOT NULL,
                        [Name] nvarchar(200) NOT NULL,
                        [Description] nvarchar(500) NULL,
                        [Icon] nvarchar(50) NULL,
                        [DisplayOrder] int NOT NULL,
                        [IsActive] bit NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_FeatureGroups] PRIMARY KEY ([Id])
                    );
                END
            ");

            // Tạo lại Features table nếu chưa có
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Features')
                BEGIN
                    CREATE TABLE [Features] (
                        [Id] uniqueidentifier NOT NULL,
                        [Name] nvarchar(200) NOT NULL,
                        [Controller] nvarchar(200) NULL,
                        [Action] nvarchar(200) NULL,
                        [Icon] nvarchar(50) NULL,
                        [FeatureGroupId] uniqueidentifier NOT NULL,
                        [DisplayOrder] int NOT NULL,
                        [IsActive] bit NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_Features] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Features_FeatureGroups_FeatureGroupId] FOREIGN KEY ([FeatureGroupId]) REFERENCES [FeatureGroups] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_Features_FeatureGroupId] ON [Features] ([FeatureGroupId]);
                END
            ");

            // Tạo lại UserFeatureGroups table nếu chưa có
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserFeatureGroups')
                BEGIN
                    CREATE TABLE [UserFeatureGroups] (
                        [UserId] uniqueidentifier NOT NULL,
                        [FeatureGroupId] uniqueidentifier NOT NULL,
                        [AssignedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_UserFeatureGroups] PRIMARY KEY ([UserId], [FeatureGroupId]),
                        CONSTRAINT [FK_UserFeatureGroups_FeatureGroups_FeatureGroupId] FOREIGN KEY ([FeatureGroupId]) REFERENCES [FeatureGroups] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_UserFeatureGroups_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_UserFeatureGroups_FeatureGroupId] ON [UserFeatureGroups] ([FeatureGroupId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFeatureGroups");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "FeatureGroups");
        }
    }
}

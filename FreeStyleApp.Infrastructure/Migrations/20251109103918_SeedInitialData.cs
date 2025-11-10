using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeStyleApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Seed Permissions
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Code = 'Admin')
                BEGIN
                    INSERT INTO Permissions (Id, Code, Name)
                    VALUES (NEWID(), 'Admin', N'Quan tri vien');
                END
            ");

            // 2. Seed Admin User (Password: Admin@123 - SHA256 hash: E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7)
            migrationBuilder.Sql(@"
                DECLARE @AdminUserId uniqueidentifier;
                DECLARE @AdminPermissionId uniqueidentifier;
                
                -- Tao user admin neu chua co
                IF NOT EXISTS (SELECT 1 FROM Users WHERE UserName = 'admin')
                BEGIN
                    SET @AdminUserId = NEWID();
                    INSERT INTO Users (Id, UserName, PasswordHash, FullName, Email, IsActive)
                    VALUES (@AdminUserId, 'admin', 'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', N'Quan tri vien', 'admin@freestyleapp.com', 1);
                END
                ELSE
                BEGIN
                    SELECT @AdminUserId = Id FROM Users WHERE UserName = 'admin';
                END
                
                -- Gan Admin permission cho user admin
                SELECT @AdminPermissionId = Id FROM Permissions WHERE Code = 'Admin';
                IF @AdminPermissionId IS NOT NULL AND @AdminUserId IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM UserPermissions WHERE UserId = @AdminUserId AND PermissionId = @AdminPermissionId)
                    BEGIN
                        INSERT INTO UserPermissions (UserId, PermissionId)
                        VALUES (@AdminUserId, @AdminPermissionId);
                    END
                    
                    -- Gan tat ca permissions cho admin
                    INSERT INTO UserPermissions (UserId, PermissionId)
                    SELECT @AdminUserId, Id
                    FROM Permissions
                    WHERE Id NOT IN (SELECT PermissionId FROM UserPermissions WHERE UserId = @AdminUserId);
                END
            ");

            // 3. Seed FeatureGroup 'Quan tri he thong'
            migrationBuilder.Sql(@"
                DECLARE @FeatureGroupId uniqueidentifier;
                
                IF NOT EXISTS (SELECT 1 FROM FeatureGroups WHERE Name = N'Quan tri he thong')
                BEGIN
                    SET @FeatureGroupId = NEWID();
                    INSERT INTO FeatureGroups (Id, Name, Description, Icon, DisplayOrder, IsActive, CreatedAt)
                    VALUES (@FeatureGroupId, N'Quan tri he thong', N'Nhom chuc nang quan tri he thong', 'fas fa-cog', 1, 1, GETUTCDATE());
                    
                    -- Seed Features trong nhom 'Quan tri he thong'
                    INSERT INTO Features (Id, Name, Controller, Action, Icon, FeatureGroupId, DisplayOrder, IsActive, CreatedAt)
                    VALUES 
                        (NEWID(), N'Quan ly Nguoi dung', 'User', 'Index', 'fas fa-users', @FeatureGroupId, 1, 1, GETUTCDATE()),
                        (NEWID(), N'Quan ly vai tro', 'Permission', 'Index', 'fas fa-shield-alt', @FeatureGroupId, 2, 1, GETUTCDATE()),
                        (NEWID(), N'Cau hinh Menu', 'FeatureGroup', 'Index', 'fas fa-list', @FeatureGroupId, 3, 1, GETUTCDATE()),
                        (NEWID(), N'Nhat ky hoat dong', 'AuditLog', 'Index', 'fas fa-history', @FeatureGroupId, 4, 1, GETUTCDATE()),
                        (NEWID(), N'Bao cao va thong ke', 'Reports', 'Index', 'fas fa-chart-bar', @FeatureGroupId, 5, 1, GETUTCDATE()),
                        (NEWID(), N'Quan ly he thong', 'SystemManagement', 'Index', 'fas fa-server', @FeatureGroupId, 6, 1, GETUTCDATE());
                END
            ");

            // 4. Gan tat ca FeatureGroups cho user admin
            migrationBuilder.Sql(@"
                DECLARE @AdminUserId uniqueidentifier;
                SELECT @AdminUserId = Id FROM Users WHERE UserName = 'admin';
                
                IF @AdminUserId IS NOT NULL
                BEGIN
                    INSERT INTO UserFeatureGroups (UserId, FeatureGroupId, AssignedAt)
                    SELECT @AdminUserId, Id, GETUTCDATE()
                    FROM FeatureGroups
                    WHERE Id NOT IN (SELECT FeatureGroupId FROM UserFeatureGroups WHERE UserId = @AdminUserId);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xoa data seed (optional - co the bo qua)
            migrationBuilder.Sql(@"
                -- Xoa user admin (neu muon rollback)
                -- DELETE FROM UserPermissions WHERE UserId IN (SELECT Id FROM Users WHERE UserName = 'admin');
                -- DELETE FROM UserFeatureGroups WHERE UserId IN (SELECT Id FROM Users WHERE UserName = 'admin');
                -- DELETE FROM Users WHERE UserName = 'admin';
                
                -- Xoa FeatureGroup 'Quan tri he thong' (Features se tu dong bi xoa do CASCADE)
                -- DELETE FROM FeatureGroups WHERE Name = N'Quan tri he thong';
                
                -- Xoa Permission Admin
                -- DELETE FROM Permissions WHERE Code = 'Admin';
            ");
        }
    }
}

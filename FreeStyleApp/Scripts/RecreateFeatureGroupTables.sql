-- Script để tạo lại các tables FeatureGroups, Features, UserFeatureGroups

-- Tạo lại FeatureGroups table nếu chưa có
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
    PRINT 'Table FeatureGroups created successfully';
END
ELSE
BEGIN
    PRINT 'Table FeatureGroups already exists';
END

-- Tạo lại Features table nếu chưa có
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
    PRINT 'Table Features created successfully';
END
ELSE
BEGIN
    PRINT 'Table Features already exists';
END

-- Tạo lại UserFeatureGroups table nếu chưa có
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
    PRINT 'Table UserFeatureGroups created successfully';
END
ELSE
BEGIN
    PRINT 'Table UserFeatureGroups already exists';
END

-- Seed data
-- 1. Seed Permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Code = 'Admin')
BEGIN
    INSERT INTO Permissions (Id, Code, Name)
    VALUES (NEWID(), 'Admin', N'Quan tri vien');
    PRINT 'Admin permission created';
END

-- 2. Seed Admin User và gán permissions
DECLARE @AdminUserId uniqueidentifier;
DECLARE @AdminPermissionId uniqueidentifier;

IF NOT EXISTS (SELECT 1 FROM Users WHERE UserName = 'admin')
BEGIN
    SET @AdminUserId = NEWID();
    INSERT INTO Users (Id, UserName, PasswordHash, FullName, Email, IsActive)
    VALUES (@AdminUserId, 'admin', 'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', N'Quan tri vien', 'admin@freestyleapp.com', 1);
    PRINT 'Admin user created';
END
ELSE
BEGIN
    SELECT @AdminUserId = Id FROM Users WHERE UserName = 'admin';
END

SELECT @AdminPermissionId = Id FROM Permissions WHERE Code = 'Admin';
IF @AdminPermissionId IS NOT NULL AND @AdminUserId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserPermissions WHERE UserId = @AdminUserId AND PermissionId = @AdminPermissionId)
    BEGIN
        INSERT INTO UserPermissions (UserId, PermissionId)
        VALUES (@AdminUserId, @AdminPermissionId);
    END
    
    INSERT INTO UserPermissions (UserId, PermissionId)
    SELECT @AdminUserId, Id
    FROM Permissions
    WHERE Id NOT IN (SELECT PermissionId FROM UserPermissions WHERE UserId = @AdminUserId);
    PRINT 'Admin permissions assigned';
END

-- 3. Seed FeatureGroup 'Quan tri he thong'
DECLARE @FeatureGroupId uniqueidentifier;

IF NOT EXISTS (SELECT 1 FROM FeatureGroups WHERE Name = N'Quan tri he thong')
BEGIN
    SET @FeatureGroupId = NEWID();
    INSERT INTO FeatureGroups (Id, Name, Description, Icon, DisplayOrder, IsActive, CreatedAt)
    VALUES (@FeatureGroupId, N'Quan tri he thong', N'Nhom chuc nang quan tri he thong', 'fas fa-cog', 1, 1, GETUTCDATE());
    
    INSERT INTO Features (Id, Name, Controller, Action, Icon, FeatureGroupId, DisplayOrder, IsActive, CreatedAt)
    VALUES 
        (NEWID(), N'Quan ly Nguoi dung', 'User', 'Index', 'fas fa-users', @FeatureGroupId, 1, 1, GETUTCDATE()),
        (NEWID(), N'Quan ly vai tro', 'Permission', 'Index', 'fas fa-shield-alt', @FeatureGroupId, 2, 1, GETUTCDATE()),
        (NEWID(), N'Cau hinh Menu', 'FeatureGroup', 'Index', 'fas fa-list', @FeatureGroupId, 3, 1, GETUTCDATE()),
        (NEWID(), N'Nhat ky hoat dong', 'AuditLog', 'Index', 'fas fa-history', @FeatureGroupId, 4, 1, GETUTCDATE()),
        (NEWID(), N'Bao cao va thong ke', 'Reports', 'Index', 'fas fa-chart-bar', @FeatureGroupId, 5, 1, GETUTCDATE()),
        (NEWID(), N'Quan ly he thong', 'SystemManagement', 'Index', 'fas fa-server', @FeatureGroupId, 6, 1, GETUTCDATE());
    PRINT 'FeatureGroup and Features created';
END

-- 4. Gán tất cả FeatureGroups cho user admin
IF @AdminUserId IS NOT NULL
BEGIN
    INSERT INTO UserFeatureGroups (UserId, FeatureGroupId, AssignedAt)
    SELECT @AdminUserId, Id, GETUTCDATE()
    FROM FeatureGroups
    WHERE Id NOT IN (SELECT FeatureGroupId FROM UserFeatureGroups WHERE UserId = @AdminUserId);
    PRINT 'FeatureGroups assigned to admin';
END

PRINT 'Script completed successfully!';



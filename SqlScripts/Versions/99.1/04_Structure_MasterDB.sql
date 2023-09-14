USE MasterDB
GO
/*
Run this script on:

        172.16.16.102\STD.MasterDB    -  This database will be modified

to synchronize it with:

        103.21.149.93.MasterDB

You are recommended to back up your database before running this script

Script created by SQL Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/6/2023 9:22:56 AM

*/
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL Serializable
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[PrintConfigCustom]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[PrintConfigCustom] ADD
[PrintConfigHeaderCustomId] [int] NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[PrintConfigStandard]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[PrintConfigStandard] ADD
[PrintConfigHeaderStandardId] [int] NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[_TC_PB]'
GO
CREATE TABLE [dbo].[_TC_PB]
(
[F_Id] [int] NOT NULL IDENTITY(1, 1),
[CreatedByUserId] [int] NOT NULL,
[UpdatedByUserId] [int] NOT NULL,
[CreatedDatetimeUtc] [datetime] NOT NULL,
[UpdatedDatetimeUtc] [datetime] NOT NULL,
[IsDeleted] [bit] NOT NULL CONSTRAINT [DF___TC_PB__IsDelete__3D93F553] DEFAULT ((0)),
[DeletedDatetimeUtc] [datetime] NULL,
[Title] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Value] [int] NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK___TC_PB__2C6EC7235327E1DE] on [dbo].[_TC_PB]'
GO
ALTER TABLE [dbo].[_TC_PB] ADD CONSTRAINT [PK___TC_PB__2C6EC7235327E1DE] PRIMARY KEY CLUSTERED ([F_Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[_ProductionProcessStatus]'
GO
CREATE TABLE [dbo].[_ProductionProcessStatus]
(
[F_Id] [int] NOT NULL IDENTITY(1, 1),
[CreatedByUserId] [int] NOT NULL,
[UpdatedByUserId] [int] NOT NULL,
[CreatedDatetimeUtc] [datetime] NOT NULL,
[UpdatedDatetimeUtc] [datetime] NOT NULL,
[IsDeleted] [bit] NOT NULL CONSTRAINT [DF___Producti__IsDel__678A2F1F] DEFAULT ((0)),
[DeletedDatetimeUtc] [datetime] NULL,
[Value] [int] NULL,
[Title] [nvarchar] (120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK___Product__2C6EC72377FB5F08] on [dbo].[_ProductionProcessStatus]'
GO
ALTER TABLE [dbo].[_ProductionProcessStatus] ADD CONSTRAINT [PK___Product__2C6EC72377FB5F08] PRIMARY KEY CLUSTERED ([F_Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[PrintConfigHeaderCustom]'
GO
CREATE TABLE [dbo].[PrintConfigHeaderCustom]
(
[PrintConfigHeaderCustomId] [int] NOT NULL IDENTITY(1, 1),
[PrintConfigHeaderStandardId] [int] NULL,
[Title] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PrintConfigHeaderCustomCode] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[JsAction] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IsShow] [bit] NOT NULL,
[SortOrder] [int] NOT NULL,
[CreatedByUserId] [int] NOT NULL,
[CreatedDatetimeUtc] [datetime2] NOT NULL,
[UpdatedByUserId] [int] NOT NULL,
[UpdatedDatetimeUtc] [datetime2] NOT NULL,
[IsDeleted] [bit] NOT NULL,
[DeletedDatetimeUtc] [datetime2] NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_PrintConfigHeaderCustom] on [dbo].[PrintConfigHeaderCustom]'
GO
ALTER TABLE [dbo].[PrintConfigHeaderCustom] ADD CONSTRAINT [PK_PrintConfigHeaderCustom] PRIMARY KEY CLUSTERED ([PrintConfigHeaderCustomId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[PrintConfigHeaderStandard]'
GO
CREATE TABLE [dbo].[PrintConfigHeaderStandard]
(
[PrintConfigHeaderStandardId] [int] NOT NULL IDENTITY(1, 1),
[Title] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PrintConfigHeaderStandardCode] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[JsAction] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IsShow] [bit] NOT NULL,
[SortOrder] [int] NOT NULL,
[CreatedByUserId] [int] NOT NULL,
[CreatedDatetimeUtc] [datetime2] NOT NULL,
[UpdatedByUserId] [int] NOT NULL,
[UpdatedDatetimeUtc] [datetime2] NOT NULL,
[IsDeleted] [bit] NOT NULL,
[DeletedDatetimeUtc] [datetime2] NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_PrintConfigHeaderStandard] on [dbo].[PrintConfigHeaderStandard]'
GO
ALTER TABLE [dbo].[PrintConfigHeaderStandard] ADD CONSTRAINT [PK_PrintConfigHeaderStandard] PRIMARY KEY CLUSTERED ([PrintConfigHeaderStandardId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
-- This statement writes to the SQL Server Log so SQL Monitor can show this deployment.
IF HAS_PERMS_BY_NAME(N'sys.xp_logevent', N'OBJECT', N'EXECUTE') = 1
BEGIN
    DECLARE @databaseName AS nvarchar(2048), @eventMessage AS nvarchar(2048)
    SET @databaseName = REPLACE(REPLACE(DB_NAME(), N'\', N'\\'), N'"', N'\"')
    SET @eventMessage = N'Redgate SQL Compare: { "deployment": { "description": "Redgate SQL Compare deployed to ' + @databaseName + N'", "database": "' + @databaseName + N'" }}'
    EXECUTE sys.xp_logevent 55000, @eventMessage
END
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO

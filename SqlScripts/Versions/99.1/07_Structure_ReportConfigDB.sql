USE ReportConfigDB
GO
/*
Run this script on:

        172.16.16.102\STD.ReportConfigDB    -  This database will be modified

to synchronize it with:

        103.21.149.93.ReportConfigDB

You are recommended to back up your database before running this script

Script created by SQL Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/6/2023 9:27:06 AM

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
PRINT N'Creating [dbo].[ReportTypeCustom]'
GO
CREATE TABLE [dbo].[ReportTypeCustom]
(
[ReportTypeCustomId] [int] NOT NULL IDENTITY(1, 1),
[ReportTypeId] [int] NOT NULL,
[SubsidiaryId] [int] NOT NULL,
[IsDeleted] [bit] NOT NULL,
[HeadSql] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[BodySql] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[FooterSql] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK__ReportTy__8A5B8027D56A757C] on [dbo].[ReportTypeCustom]'
GO
ALTER TABLE [dbo].[ReportTypeCustom] ADD CONSTRAINT [PK__ReportTy__8A5B8027D56A757C] PRIMARY KEY CLUSTERED ([ReportTypeCustomId])
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

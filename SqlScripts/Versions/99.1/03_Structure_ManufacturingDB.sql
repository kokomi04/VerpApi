USE ManufacturingDB
GO
/*
Run this script on:

        172.16.16.102\STD.ManufacturingDB    -  This database will be modified

to synchronize it with:

        103.21.149.93.ManufacturingDB

You are recommended to back up your database before running this script

Script created by SQL Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/6/2023 9:20:37 AM

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
PRINT N'Altering [dbo].[ProductionAssignment]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[ProductionAssignment] ADD
[AssignedInputStatus] [int] NOT NULL CONSTRAINT [DF_ProductionAssignment_AssignedInputStatus] DEFAULT ((0))
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [dbo].[ProductionAssignment]'
GO
ALTER TABLE [dbo].[ProductionAssignment] ADD CONSTRAINT [DF_ProductionAssignment_AssignedProgressStatus] DEFAULT ((0)) FOR [AssignedProgressStatus]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[ProductionStep]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[ProductionStep] ADD
[ProductionStepAssignmentStatusId] [int] NOT NULL CONSTRAINT [DF_ProductionStep_IsCompletedAssignment] DEFAULT ((0))
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[ProductionOrder]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[ProductionOrder] ADD
[IsFinished] [bit] NOT NULL CONSTRAINT [DF_ProductionOrder_IsFinished] DEFAULT ((0))
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[ProductionHandover]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[ProductionHandover] ADD
[ProductionStepLinkDataId] [bigint] NULL,
[InventoryRequirementDetailId] [bigint] NULL,
[InventoryDetailId] [bigint] NULL,
[InventoryProductId] [int] NULL,
[IsAuto] [bit] NOT NULL CONSTRAINT [DF_ProductionHandover_IsAuto] DEFAULT ((0)),
[InventoryId] [bigint] NULL,
[InventoryCode] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[InventoryQuantity] [decimal] (32, 12) NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[ProductionOrderInventoryConflict]'
GO
CREATE TABLE [dbo].[ProductionOrderInventoryConflict]
(
[ProductionOrderId] [bigint] NOT NULL,
[InventoryDetailId] [bigint] NOT NULL,
[ProductId] [int] NOT NULL,
[InventoryTypeId] [int] NOT NULL,
[InventoryId] [bigint] NOT NULL,
[InventoryDate] [datetime2] NOT NULL,
[InventoryCode] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[InventoryQuantity] [decimal] (32, 12) NOT NULL,
[InventoryRequirementDetailId] [bigint] NULL,
[InventoryRequirementId] [bigint] NULL,
[RequireQuantity] [decimal] (32, 12) NULL,
[InventoryRequirementCode] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Content] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[HandoverInventoryQuantitySum] [decimal] (32, 12) NOT NULL,
[ConflictAllowcationStatusId] [int] NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_ProductionOrderInventoryConflict] on [dbo].[ProductionOrderInventoryConflict]'
GO
ALTER TABLE [dbo].[ProductionOrderInventoryConflict] ADD CONSTRAINT [PK_ProductionOrderInventoryConflict] PRIMARY KEY CLUSTERED ([ProductionOrderId], [InventoryDetailId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating extended properties'
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'Product id in production process', 'SCHEMA', N'dbo', 'TABLE', N'ProductionHandover', 'COLUMN', N'InventoryProductId'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
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

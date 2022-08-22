USE ManufacturingDB
GO
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.WeekPlan SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.MonthPlan SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductionOrder ADD
	MonthPlanId int NULL,
	FromWeekPlanId int NULL,
	ToWeekPlanId int NULL
GO
ALTER TABLE dbo.ProductionOrder ADD CONSTRAINT
	FK_ProductionOrder_MonthPlan FOREIGN KEY
	(
	MonthPlanId
	) REFERENCES dbo.MonthPlan
	(
	MonthPlanId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductionOrder ADD CONSTRAINT
	FK_ProductionOrder_FromWeekPlan FOREIGN KEY
	(
	FromWeekPlanId
	) REFERENCES dbo.WeekPlan
	(
	WeekPlanId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductionOrder ADD CONSTRAINT
	FK_ProductionOrder_ToWeekPlan FOREIGN KEY
	(
	ToWeekPlanId
	) REFERENCES dbo.WeekPlan
	(
	WeekPlanId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductionOrder SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


GO

USE ManufacturingDB
GO
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductionOrderConfiguration ADD
	IsWeekPlanSplitByWeekOfYear bit NOT NULL CONSTRAINT DF_ProductionOrderConfiguration_IsWeekPlanSplitByWeekOfYear DEFAULT 0
GO
ALTER TABLE dbo.ProductionOrderConfiguration SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
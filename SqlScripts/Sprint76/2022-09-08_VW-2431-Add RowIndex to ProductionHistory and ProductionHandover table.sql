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
ALTER TABLE dbo.ProductionHandover ADD
	RowIndex int NOT NULL CONSTRAINT DF_ProductionHandover_RowIndex DEFAULT 0
GO
ALTER TABLE dbo.ProductionHandover SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

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
ALTER TABLE dbo.ProductionHistory ADD
	RowIndex int NOT NULL CONSTRAINT DF_ProductionHistory_RowIndex DEFAULT 0
GO
ALTER TABLE dbo.ProductionHistory SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

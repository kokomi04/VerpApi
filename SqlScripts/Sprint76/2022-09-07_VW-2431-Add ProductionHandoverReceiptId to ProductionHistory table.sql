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
ALTER TABLE dbo.ProductionHandoverReceipt SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductionHistory ADD
	ProductionHandoverReceiptId bigint NULL
GO

ALTER TABLE dbo.ProductionHistory ADD CONSTRAINT
	FK_ProductionHistory_ProductionHandoverReceipt FOREIGN KEY
	(
	ProductionHandoverReceiptId
	) REFERENCES dbo.ProductionHandoverReceipt
	(
	ProductionHandoverReceiptId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	 
GO
ALTER TABLE dbo.ProductionHistory SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

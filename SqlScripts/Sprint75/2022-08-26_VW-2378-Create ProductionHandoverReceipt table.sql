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
ALTER TABLE dbo.ProductionOrder SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.ProductionHandoverReceipt
	(
	ProductionHandoverReceiptId bigint NOT NULL IDENTITY (1, 1),
	ProductionHandoverReceiptCode nvarchar(128) NULL,
	HandoverStatusId int NOT NULL,
	AcceptByUserId int NULL,
	SubsidiaryId int NOT NULL,
	CreatedByUserId int NOT NULL,
	CreatedDatetimeUtc datetime2(7) NOT NULL,
	UpdatedByUserId int NOT NULL,
	UpdatedDatetimeUtc datetime2(7) NOT NULL,
	IsDeleted bit NOT NULL,
	DeletedDatetimeUtc datetime2(7) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ProductionHandoverReceipt ADD CONSTRAINT
	PK_ProductionHandoverReceipt PRIMARY KEY CLUSTERED 
	(
	ProductionHandoverReceiptId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.ProductionHandoverReceipt SET (LOCK_ESCALATION = TABLE)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_ProductionHandoverReceipt_ProductionHandoverReceiptCode ON dbo.ProductionHandoverReceipt
	(
	SubsidiaryId,
	ProductionHandoverReceiptCode
	) WHERE ([IsDeleted]=(0))
	WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT


GO


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
ALTER TABLE dbo.ProductionHandover ADD
	ProductionHandoverReceiptId bigint NULL
GO
ALTER TABLE dbo.ProductionHandover ADD CONSTRAINT
	FK_ProductionHandover_ProductionHandoverReceipt FOREIGN KEY
	(
	ProductionHandoverReceiptId
	) REFERENCES dbo.ProductionHandoverReceipt
	(
	ProductionHandoverReceiptId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
CREATE UNIQUE NONCLUSTERED INDEX IX_ProductionHandoverReceipt ON dbo.ProductionHandoverReceipt
	(
	ProductionHandoverReceiptCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.ProductionHandoverReceipt SET (LOCK_ESCALATION = TABLE)
GO
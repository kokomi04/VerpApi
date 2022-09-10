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
CREATE TABLE dbo.ProductionOrderMaterialSet
	(
	ProductionOrderMaterialSetId bigint NOT NULL IDENTITY (1, 1),
	Title nvarchar(128) NULL,
	ProductionOrderId bigint NOT NULL,
	IsMultipleConsumptionGroupId bit NOT NULL,
	CreatedByUserId int NOT NULL,
	UpdatedByUserId int NOT NULL,
	CreatedDatetimeUtc datetime2(7) NOT NULL,
	UpdatedDatetimeUtc datetime2(7) NOT NULL,
	IsDeleted bit NOT NULL,
	DeletedDatetimeUtc datetime2(7) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ProductionOrderMaterialSet ADD CONSTRAINT
	DF_ProductionOrderMaterialSet_CreatedDatetimeUtc DEFAULT (getdate()) FOR CreatedDatetimeUtc
GO
ALTER TABLE dbo.ProductionOrderMaterialSet ADD CONSTRAINT
	DF_ProductionOrderMaterialSet_UpdatedDatetimeUtc DEFAULT (getdate()) FOR UpdatedDatetimeUtc
GO
ALTER TABLE dbo.ProductionOrderMaterialSet ADD CONSTRAINT
	DF_ProductionOrderMaterialSet_IsDeleted DEFAULT ((0)) FOR IsDeleted
GO
ALTER TABLE dbo.ProductionOrderMaterialSet ADD CONSTRAINT
	PK_ProductionOrderMaterialSet PRIMARY KEY CLUSTERED 
	(
	ProductionOrderMaterialSetId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.ProductionOrderMaterialSet ADD CONSTRAINT
	FK_ProductionOrderMaterialSet_ProductionOrder FOREIGN KEY
	(
	ProductionOrderId
	) REFERENCES dbo.ProductionOrder
	(
	ProductionOrderId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductionOrderMaterialSet SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE [dbo].[ProductionOrderMaterialSetConsumptionGroup](
	[ProductionOrderMaterialSetId] [BIGINT] NOT NULL,
	[ProductMaterialsConsumptionGroupId] [INT] NOT NULL,
 CONSTRAINT [PK_ProductionOrderMaterialSetConsumptionGroup] PRIMARY KEY CLUSTERED 
(
	[ProductionOrderMaterialSetId] ASC,
	[ProductMaterialsConsumptionGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE dbo.ProductionOrderMaterialSetConsumptionGroup ADD CONSTRAINT
	FK_ProductionOrderMaterialSetConsumptionGroup_ProductionOrderMaterialSet FOREIGN KEY
	(
	ProductionOrderMaterialSetId
	) REFERENCES dbo.ProductionOrderMaterialSet
	(
	ProductionOrderMaterialSetId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductionOrderMaterialSetConsumptionGroup SET (LOCK_ESCALATION = TABLE)
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
ALTER TABLE dbo.ProductionOrderMaterialSet SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductionOrderMaterials ADD
	ProductionOrderMaterialSetId bigint NULL,
	ProductMaterialsConsumptionGroupId int NULL
GO
ALTER TABLE dbo.ProductionOrderMaterials ADD CONSTRAINT
	FK_ProductionOrderMaterials_ProductionOrderMaterialSet FOREIGN KEY
	(
	ProductionOrderMaterialSetId
	) REFERENCES dbo.ProductionOrderMaterialSet
	(
	ProductionOrderMaterialSetId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductionOrderMaterials ALTER COLUMN ProductionStepLinkDataId BIGINT NULL
GO
ALTER TABLE dbo.ProductionOrderMaterials SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

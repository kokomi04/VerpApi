USE [PurchaseOrderDB]
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
ALTER TABLE dbo.PurchasingSuggestDetail ADD
	IsSubCalculation bit NOT NULL CONSTRAINT DF_PurchasingSuggestDetail_IsSubCalculation DEFAULT ((0))
GO
ALTER TABLE dbo.PurchasingSuggestDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO

CREATE TABLE [dbo].[PurchasingSuggestDetailSubCalculation](
	[PurchasingSuggestDetailSubCalculationId] [INT] IDENTITY(1,1) NOT NULL,
	[PurchasingSuggestDetailId] [BIGINT] NOT NULL,
	[ProductBomId] [BIGINT] NOT NULL,
	[UnitConversionId] [INT] NULL,
	[PrimaryUnitPrice] [DECIMAL](18, 5) NOT NULL,
	[PrimaryQuantity] [DECIMAL](32, 12) NOT NULL,
	[CreatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[UpdatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[IsDeleted] [BIT] NOT NULL,
	[DeletedDatetimeUtc] [DATETIME2](7) NULL,
 CONSTRAINT [PK_PurchasingSuggestDetailSubCalculation] PRIMARY KEY CLUSTERED 
(
	[PurchasingSuggestDetailSubCalculationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PurchasingSuggestDetailSubCalculation]  WITH CHECK ADD  CONSTRAINT [FK_PurchasingSuggestDetailSubCalculation_PurchasingSuggestDetail] FOREIGN KEY([PurchasingSuggestDetailId])
REFERENCES [dbo].[PurchasingSuggestDetail] ([PurchasingSuggestDetailId])
GO

ALTER TABLE [dbo].[PurchasingSuggestDetailSubCalculation] CHECK CONSTRAINT [FK_PurchasingSuggestDetailSubCalculation_PurchasingSuggestDetail]
GO
COMMIT


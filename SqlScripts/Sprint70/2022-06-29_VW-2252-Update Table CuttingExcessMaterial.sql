USE [PurchaseOrderDB]
GO
ALTER TABLE [dbo].[CuttingExcessMaterial] ADD [CuttingExcessMaterialId] [bigint] IDENTITY(1,1) NOT NULL
GO
ALTER TABLE [dbo].[CuttingExcessMaterial] DROP CONSTRAINT [PK_CuttingExcessMaterial]
GO
ALTER TABLE [dbo].[CuttingExcessMaterial] ADD CONSTRAINT [PK_CuttingExcessMaterial] PRIMARY KEY NONCLUSTERED ([CuttingExcessMaterialId] ASC)
GO
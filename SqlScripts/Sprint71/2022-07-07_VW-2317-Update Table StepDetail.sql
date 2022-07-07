USE [ManufacturingDB]
GO

ALTER TABLE [StepDetail] ADD [EstimateHandoverTime] [decimal](18, 5) NOT NULL CONSTRAINT [DF_StepDetail_EstimateHandoverTime] DEFAULT ((0))
GO



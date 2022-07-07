USE [ManufacturingDB]
GO

ALTER TABLE [StepDetail] ADD [EstimateHandoverTime] [int] NOT NULL CONSTRAINT [DF_StepDetail_EstimateHandoverTime] DEFAULT ((0))
GO

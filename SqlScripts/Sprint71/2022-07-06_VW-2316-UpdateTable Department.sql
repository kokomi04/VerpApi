USE [OrganizationDB]
GO

ALTER TABLE [Department] ADD [NumberOfMachine] [int] NOT NULL CONSTRAINT [DF_Department_NumberOfMachine] DEFAULT ((0))
GO
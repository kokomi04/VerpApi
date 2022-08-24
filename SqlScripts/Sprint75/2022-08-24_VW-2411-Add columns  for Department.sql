USE [OrganizationDB]
GO

ALTER TABLE [Department] ADD [IsFactory] BIT NOT NULL DEFAULT 0
GO

USE MasterDB;
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'PrintConfig')
BEGIN
  DROP TABLE PrintConfig  
END

CREATE TABLE PrintConfig(
	[PrintConfigId] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[ActiveForId] [int] NULL,
	[PrintConfigName] [nvarchar](255) NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[BodyTable] [nvarchar](max) NULL,
	[GenerateCode] [nvarchar](max) NULL,
	[PaperSize] [int] NULL,
	[Layout] [nvarchar](max) NULL,
	[HeadTable] [nvarchar](max) NULL,
	[FootTable] [nvarchar](max) NULL,
	[StickyFootTable] [bit] NULL,
	[StickyHeadTable] [bit] NULL,
	[CreatedByUserId] [int] NOT NULL,
	[CreatedDatetimeUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[UpdatedDatetimeUtc] [datetime2](7) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedDatetimeUtc] [datetime2](7) NULL,
	[HasTable] [bit] NULL,
	[Background] [nvarchar](max) NULL,
	[TemplateFileId] [bigint] NULL,
	[GenerateToString] [nvarchar](max) NULL,
	[ModuleTypeId] [int] NOT NULL)
GO

WITH TEMP AS (SELECT * FROM [AccountancyDB].[dbo].[PrintConfig])
INSERT INTO [MasterDB].[dbo].[PrintConfig] 
	SELECT ActiveForId, PrintConfigName, Title, BodyTable, GenerateCode, PaperSize, 
	Layout, HeadTable, FootTable, StickyFootTable, StickyHeadTable, CreatedByUserId, 
	CreatedDatetimeUtc, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted, 
	DeletedDatetimeUtc, HasTable, Background, TemplateFileId, GenerateToString, ModuleTypeId   
	FROM TEMP;
GO

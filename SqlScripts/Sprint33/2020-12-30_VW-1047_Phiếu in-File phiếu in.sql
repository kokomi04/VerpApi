USE MasterDB;
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'PrintConfigDetail')
BEGIN
  DROP TABLE PrintConfigDetail  
END

CREATE TABLE [dbo].[PrintConfigDetail](
	[PrintConfigDetailId] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[PrintConfigId] [int] NOT NULL,
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
	[TemplateFilePath] [nvarchar](max) NULL,
	[IsOrigin] [bit] NOT NULL,
	CONSTRAINT FK_PrintConfigDetail_PrintConfig FOREIGN KEY (PrintConfigId) REFERENCES PrintConfig(PrintConfigId))

INSERT INTO PrintConfigDetail (PrintConfigId, BodyTable, GenerateCode, PaperSize, Layout, HeadTable, FootTable, StickyFootTable, StickyHeadTable, CreatedByUserId,
CreatedDatetimeUtc, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted, DeletedDatetimeUtc, HasTable, Background, GenerateToString, TemplateFileId, IsOrigin)
SELECT PrintConfigId, BodyTable, GenerateCode, PaperSize, Layout, HeadTable, FootTable, StickyFootTable, StickyHeadTable, CreatedByUserId,
CreatedDatetimeUtc, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted, DeletedDatetimeUtc, HasTable, Background, GenerateToString, TemplateFileId, 1   
	FROM PrintConfig where IsDeleted = 0 
INSERT INTO PrintConfigDetail (PrintConfigId, BodyTable, GenerateCode, PaperSize, Layout, HeadTable, FootTable, StickyFootTable, StickyHeadTable, CreatedByUserId,
CreatedDatetimeUtc, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted, DeletedDatetimeUtc, HasTable, Background, GenerateToString, TemplateFileId, IsOrigin)
SELECT PrintConfigId, BodyTable, GenerateCode, PaperSize, Layout, HeadTable, FootTable, StickyFootTable, StickyHeadTable, CreatedByUserId,
CreatedDatetimeUtc, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted, DeletedDatetimeUtc, HasTable, Background, GenerateToString, TemplateFileId, 0   
	FROM PrintConfig where IsDeleted = 0 
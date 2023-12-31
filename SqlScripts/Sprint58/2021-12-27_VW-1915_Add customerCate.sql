USE [OrganizationDB]
GO

/****** Object:  Table [dbo].[CustomerCate]    Script Date: 12/27/2021 3:10:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CustomerCate](
	[CustomerCateId] [INT] IDENTITY(1,1) NOT NULL,
	[CustomerCateCode] [NVARCHAR](128) NULL,
	[Name] [NVARCHAR](256) NULL,
	[Description] [NVARCHAR](1024) NULL,
	[SortOrder]	[INT] NULL,
	[IsDeleted] [BIT] NOT NULL,
	[CreatedByUserId] [INT] NOT NULL,
	[CreatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[UpdatedByUserId] [INT] NOT NULL,
	[UpdatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[DeletedDatetimeUtc] [DATETIME2](7) NULL,
 CONSTRAINT [PK_CustomerCate] PRIMARY KEY CLUSTERED 
(
	[CustomerCateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/*
   Monday, December 27, 20213:07:39 PM
   User: VErpAdmin
   Server: 103.21.149.106
   Database: OrganizationDB
   Application: 
*/

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
ALTER TABLE dbo.CustomerCate SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Customer ADD
	CustomerCateId int NULL
GO
ALTER TABLE dbo.Customer ADD CONSTRAINT
	FK_Customer_CustomerCate FOREIGN KEY
	(
	CustomerCateId
	) REFERENCES dbo.CustomerCate
	(
	CustomerCateId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

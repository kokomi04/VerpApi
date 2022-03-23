USE [MasterDB]
GO

/****** Object:  Table [dbo].[ActionButtonBillType]    Script Date: 3/12/2022 5:35:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE dbo.ActionButton ALTER COLUMN ObjectTypeId INT NULL
ALTER TABLE dbo.ActionButton ALTER COLUMN ObjectId INT NULL
ALTER TABLE dbo.ActionButton ADD BillTypeObjectTypeId INT NULL

GO

CREATE TABLE [dbo].[ActionButtonBillType](
	[ActionButtonId] [INT] NOT NULL,
	[BillTypeObjectId] [BIGINT] NOT NULL,
	[CreatedByUserId] [INT] NOT NULL,
	[CreatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[UpdatedByUserId] [INT] NOT NULL,
	[UpdatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[IsDeleted] [BIT] NOT NULL,
	[DeletedDatetimeUtc] [DATETIME2](7) NULL,
 CONSTRAINT [PK_ActionButtonBillType] PRIMARY KEY CLUSTERED 
(
	[ActionButtonId] ASC,
	[BillTypeObjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ActionButtonBillType]  WITH CHECK ADD  CONSTRAINT [FK_ActionButtonBillType_ActionButton] FOREIGN KEY([ActionButtonId])
REFERENCES [dbo].[ActionButton] ([ActionButtonId])
GO

ALTER TABLE [dbo].[ActionButtonBillType] CHECK CONSTRAINT [FK_ActionButtonBillType_ActionButton]
GO



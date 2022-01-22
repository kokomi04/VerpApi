USE [StockDB]
GO
ALTER TABLE dbo.Product ADD ProductPurity DECIMAL(32,12) NULL
GO
CREATE TABLE [dbo].[ProductPurityCalc](
	[ProductPurityCalcId] [INT] IDENTITY(1,1) NOT NULL,
	[SubsidiaryId] [INT] NOT NULL,
	[Title] [NVARCHAR](128) NULL,
	[Description] [NVARCHAR](1024) NULL,
	[EvalSourceCodeJs] [NVARCHAR](MAX) NULL,
	[CreatedByUserId] [INT] NOT NULL,
	[CreatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[UpdatedByUserId] [INT] NOT NULL,
	[UpdatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[IsDeleted] [BIT] NOT NULL,
	[DeletedDatetimeUtc] [DATETIME2](7) NULL,
 CONSTRAINT [PK_ProductPurityCalc] PRIMARY KEY CLUSTERED 
(
	[ProductPurityCalcId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

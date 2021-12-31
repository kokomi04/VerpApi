
CREATE TABLE [dbo].[UserLoginLog] (
  [UserLoginLogId] bigint  IDENTITY(1,1) NOT NULL,
  [UserId] int  NULL,
  [UserName] nvarchar(128) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [IpAddress] nvarchar(128) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [UserAgent] nvarchar(128) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [Status] int NOT NULL,
  [MessageTypeId] int DEFAULT ((1)) NOT NULL,
  [MessageResourceName] nvarchar(512) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [MessageResourceFormatData] nvarchar(512) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [Message] nvarchar(512) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [CreatedDatetimeUtc] datetime2(7)  NOT NULL,
  [StrSubId] nvarchar(128) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[UserLoginLog] SET (LOCK_ESCALATION = TABLE)
GO


ALTER TABLE [dbo].[UserLoginLog] ADD CONSTRAINT [PK_UserLoginLog] PRIMARY KEY CLUSTERED ([UserLoginLogId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


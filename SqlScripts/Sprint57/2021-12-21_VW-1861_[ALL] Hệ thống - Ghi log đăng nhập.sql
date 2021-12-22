/*
 Navicat Premium Data Transfer

 Source Server         : 103.21.149.106
 Source Server Type    : SQL Server
 Source Server Version : 14001000
 Source Host           : 103.21.149.106:1433
 Source Catalog        : ActivityLogDB
 Source Schema         : dbo

 Target Server Type    : SQL Server
 Target Server Version : 14001000
 File Encoding         : 65001

 Date: 22/12/2021 14:00:48
*/


-- ----------------------------
-- Table structure for UserLoginLog
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[UserLoginLog]') AND type IN ('U'))
	DROP TABLE [dbo].[UserLoginLog]
GO

CREATE TABLE [dbo].[UserLoginLog] (
  [UserLoginLogId] bigint  IDENTITY(1,1) NOT NULL,
  [UserId] int  NULL,
  [UserName] nvarchar(128) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [IpAddress] nvarchar(128) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [UserAgent] nvarchar(128) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
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


-- ----------------------------
-- Primary Key structure for table UserLoginLog
-- ----------------------------
ALTER TABLE [dbo].[UserLoginLog] ADD CONSTRAINT [PK_UserLoginLog] PRIMARY KEY CLUSTERED ([UserLoginLogId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


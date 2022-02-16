USE [OrganizationDB]
GO

/****** Object:  Table [dbo].[LeaveConfig]    Script Date: 1/27/2022 3:55:43 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LeaveConfig](
	[LeaveConfigId] [INT] IDENTITY(1,1) NOT NULL,
	[Title] [NVARCHAR](128) NOT NULL,
	[Description] [NVARCHAR](512) NULL,
	[AdvanceDays] [INT] NOT NULL,
	[MonthRate] [DECIMAL](4, 1) NULL,
	[MaxAYear] [INT] NULL,
	[SeniorityMonthsStart] [INT] NULL,
	[SeniorityMonthOfYear] [INT] NULL,
	[OldYearTransferMax] [INT] NULL,
	[OldYearAppliedToDate] [DATETIME2](7) NULL,
	[IsDefault] [BIT] NOT NULL,
	[IsDeleted] [BIT] NOT NULL,
	[CreatedByUserId] [INT] NOT NULL,
	[CreatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[UpdatedByUserId] [INT] NOT NULL,
	[UpdatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[DeletedDatetimeUtc] [DATETIME2](7) NULL,
 CONSTRAINT [PK_LeaveConfig] PRIMARY KEY CLUSTERED 
(
	[LeaveConfigId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số ngày được ứng trước' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LeaveConfig', @level2type=N'COLUMN',@level2name=N'AdvanceDays'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 tháng làm việc được cho mấy ngày phép' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LeaveConfig', @level2type=N'COLUMN',@level2name=N'MonthRate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số ngày phép tối đa 1 năm' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LeaveConfig', @level2type=N'COLUMN',@level2name=N'MaxAYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Làm đến tháng thứ mấy thì bắt đầu tính thâm niên' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LeaveConfig', @level2type=N'COLUMN',@level2name=N'SeniorityMonthsStart'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Bắt đầu tính thâm niên từ tháng mấy của năm' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LeaveConfig', @level2type=N'COLUMN',@level2name=N'SeniorityMonthOfYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số phép tối đa mà năm cũ chuyển sang' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LeaveConfig', @level2type=N'COLUMN',@level2name=N'OldYearTransferMax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Phép năm cũ sẽ áp dụng đến ngày tháng nào' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LeaveConfig', @level2type=N'COLUMN',@level2name=N'OldYearAppliedToDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LeaveConfig'
GO


USE [OrganizationDB]
GO

/****** Object:  Table [dbo].[LeaveConfigValidation]    Script Date: 1/27/2022 3:55:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LeaveConfigValidation](
	[LeaveConfigId] [int] NOT NULL,
	[TotalDays] [int] NOT NULL,
	[MinDaysFromCreateToStart] [int] NULL,
	[IsWarning] [bit] NOT NULL,
 CONSTRAINT [PK_LeaveConfigValidation] PRIMARY KEY CLUSTERED 
(
	[LeaveConfigId] ASC,
	[TotalDays] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LeaveConfigValidation]  WITH CHECK ADD  CONSTRAINT [FK_LeaveConfigValidation_LeaveConfig] FOREIGN KEY([LeaveConfigId])
REFERENCES [dbo].[LeaveConfig] ([LeaveConfigId])
GO

ALTER TABLE [dbo].[LeaveConfigValidation] CHECK CONSTRAINT [FK_LeaveConfigValidation_LeaveConfig]
GO




USE [OrganizationDB]
GO

/****** Object:  Table [dbo].[LeaveConfigSeniority]    Script Date: 1/27/2022 3:56:14 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LeaveConfigSeniority](
	[LeaveConfigId] [int] NOT NULL,
	[Months] [int] NOT NULL,
	[AdditionDays] [int] NOT NULL,
 CONSTRAINT [PK_LeaveConfigSeniority] PRIMARY KEY CLUSTERED 
(
	[LeaveConfigId] ASC,
	[Months] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [OrganizationDB]
GO

/****** Object:  Table [dbo].[LeaveConfigRole]    Script Date: 1/27/2022 3:56:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LeaveConfigRole](
	[LeaveConfigRoleId] [int] IDENTITY(1,1) NOT NULL,
	[LeaveConfigId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[LeaveRoleTypeId] [int] NOT NULL,
 CONSTRAINT [PK_LeaveConfigRole] PRIMARY KEY CLUSTERED 
(
	[LeaveConfigRoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LeaveConfigRole]  WITH CHECK ADD  CONSTRAINT [FK_LeaveConfigRole_LeaveConfig] FOREIGN KEY([LeaveConfigId])
REFERENCES [dbo].[LeaveConfig] ([LeaveConfigId])
GO

ALTER TABLE [dbo].[LeaveConfigRole] CHECK CONSTRAINT [FK_LeaveConfigRole_LeaveConfig]
GO
USE [OrganizationDB]
GO

ALTER TABLE dbo.Employee ADD LeaveConfigId INT NULL
GO
ALTER TABLE dbo.Employee ADD CONSTRAINT
	FK_Employee_LeaveConfig FOREIGN KEY
	(
	LeaveConfigId
	) REFERENCES dbo.LeaveConfig
	(
	LeaveConfigId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO



USE [OrganizationDB]
GO

/****** Object:  Table [dbo].[Leave]    Script Date: 1/27/2022 3:57:09 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Leave](
	[LeaveId] [bigint] NOT NULL,
	[LeaveConfigId] [int] NOT NULL,
	[UserId] [int] NULL,
	[Title] [nvarchar](128) NULL,
	[Description] [nvarchar](1024) NULL,
	[DateStart] [datetime2](7) NOT NULL,
	[DateStartIsHalf] [bit] NOT NULL,
	[DateEnd] [datetime2](7) NOT NULL,
	[DateEndIsHalf] [bit] NOT NULL,
	[TotalDays] [decimal](4, 1) NOT NULL,
	[TotalDaysLastYearUsed] [decimal](4, 1) NOT NULL,	
	[FileId] [bigint] NULL,
	[AbsenceTypeSymbolId] [int] NOT NULL,
	[LeaveStatusId] [int] NOT NULL,
	[CheckedByUserId] [int] NULL,
	[CensoredByUserId] [int] NULL,
	[CheckedDatetimeUtc] [datetime2](7) NULL,
	[CensoredDatetimeUtc] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[CreatedDatetimeUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[UpdatedDatetimeUtc] [datetime2](7) NOT NULL,
	[DeletedDatetimeUtc] [datetime2](7) NULL,
 CONSTRAINT [PK_Leave] PRIMARY KEY CLUSTERED 
(
	[LeaveId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Leave]  WITH CHECK ADD  CONSTRAINT [FK_Leave_AbsenceTypeSymbol] FOREIGN KEY([AbsenceTypeSymbolId])
REFERENCES [dbo].[AbsenceTypeSymbol] ([AbsenceTypeSymbolId])
GO

ALTER TABLE [dbo].[Leave] CHECK CONSTRAINT [FK_Leave_AbsenceTypeSymbol]
GO

ALTER TABLE [dbo].[Leave]  WITH CHECK ADD  CONSTRAINT [FK_Leave_LeaveConfig] FOREIGN KEY([LeaveConfigId])
REFERENCES [dbo].[LeaveConfig] ([LeaveConfigId])
GO

ALTER TABLE [dbo].[Leave] CHECK CONSTRAINT [FK_Leave_LeaveConfig]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đơn xin nghỉ phép' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Leave'
GO


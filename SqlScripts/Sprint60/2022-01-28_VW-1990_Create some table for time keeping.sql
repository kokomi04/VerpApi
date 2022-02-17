use OrganizationDB;

CREATE TABLE [dbo].[WorkSchedule](
	[WorkScheduleId] [int] IDENTITY(1,1) NOT NULL,
	[WorkScheduleTitle] [varchar](256) NOT NULL,
	[IsAbsenceForSaturday] [bit] NOT NULL,
	[isAbsenceForSunday] [bit] NOT NULL,
	[isAbsenceForHoliday] [bit] NOT NULL,
	[IsCountWorkForHoliday] [bit] NOT NULL,
	[IsDayOfTimeOut] [bit] NOT NULL,
	[TimeSortConfigurationId] [int] NOT NULL,
	[FirstDayForCountedWork] [int] NOT NULL,
	[IsRoundBackForTimeOutAfterWork] [bit] NOT NULL,
	[RoundLevelForTimeOutAfterWork] [int] NOT NULL,
	[DecimalPlace] [int] NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[CreatedDatetimeUtc] [datetime2](7) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[UpdatedDatetimeUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[DeletedDatetimeUtc] [datetime2](7) NULL,
	CONSTRAINT [PK_WorkSchedule] PRIMARY KEY CLUSTERED 
	(
		[WorkScheduleId] ASC
	)
);

CREATE TABLE [dbo].[TimeSortConfiguration](
	[TimeSortConfigurationId] [int] IDENTITY(1,1) NOT NULL,
	[TimeSortCode] [nvarchar](50) NOT NULL,
	[TimeSortDescription] [nvarchar](50) NOT NULL,
	[TimeSortType] [int] NOT NULL,
	[MinMinutes] [bigint] NOT NULL,
	[MaxMinutes] [bigint] NOT NULL,
	[BetweenMinutes] [bigint] NOT NULL,
	[NumberOfCycles] [int] NOT NULL,
	[TimeEndCycles] [time](7) NOT NULL,
	[IsIgnoreNightShift] [bit] NOT NULL,
	[StartTimeIgnoreTimeShift] [time](7) NOT NULL,
	[EndTimeIgnoreTimeShift] [time](7) NOT NULL,
	[IsApplySpecialCase] [bit] NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[CreatedDatetimeUtc] [datetime2](7) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[UpdatedDatetimeUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[DeletedDatetimeUtc] [datetime2](7) NULL,
	 CONSTRAINT [PK_TimeSortConfiguration] PRIMARY KEY CLUSTERED 
	(
		[TimeSortConfigurationId] ASC
	)
);

CREATE TABLE [dbo].[SplitHour](
	[SplitHourId] [int] IDENTITY(1,1) NOT NULL,
	[TimeSortConfigurationId] [int] NOT NULL,
	[StartTimeOn] [time](7) NOT NULL,
	[EndTimeOn] [time](7) NOT NULL,
	[StartTimeOut] [time](7) NOT NULL,
	[EndTimeOut] [time](7) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[UpdatedDatetimeUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[DeletedDatetimeUtc] [datetime2](7) NULL,
	CONSTRAINT [PK_SplitHour] PRIMARY KEY CLUSTERED 
	(
		[SplitHourId] ASC
	)
);

CREATE TABLE [dbo].[AbsenceTypeSymbol](
	[AbsenceTypeSymbolId] [int] IDENTITY(1,1) NOT NULL,
	[TypeSymbolCode] [nvarchar](50) NOT NULL,
	[TypeSymbolDescription] [nvarchar](256) NOT NULL,
	[SymbolCode] [nvarchar](50) NOT NULL,
	[IsUsed] [bit] NOT NULL,
	[IsCounted] [bit] NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[UpdatedDatetimeUtc] [datetime2](7) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedDatetimeUtc] [datetime2](7) NULL,
	CONSTRAINT [PK_AbsenceTypeSymbol] PRIMARY KEY CLUSTERED 
	(
		[AbsenceTypeSymbolId] ASC
	)
);

CREATE TABLE [dbo].[CountedSymbol](
	[CountedSymbolId] [int] IDENTITY(1,1) NOT NULL,
	[CountedSymbolType] [int] NOT NULL,
	[SymbolCode] [nvarchar](50) NOT NULL,
	[SymbolDescription] [nvarchar](256) NOT NULL,
	[IsHide] [bit] NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[UpdatedDatetimeUtc] [datetime2](7) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedDatetimeUtc] [datetime2](7) NULL,
	 CONSTRAINT [PK_CountedSymbol] PRIMARY KEY CLUSTERED 
	(
		[CountedSymbolId] ASC
	)
);

CREATE TABLE [dbo].[ShiftConfiguration](
	[ShiftConfigurationId] [int] IDENTITY(1,1) NOT NULL,
	[OvertimeConfigurationId] [int] NULL,
	[ShiftCode] [nvarchar](50) NOT NULL,
	[BeginDate] [time](7) NOT NULL,
	[EndDate] [time](7) NOT NULL,
	[NumberOfTransition] [int] NOT NULL,
	[LunchTimeStart] [time](7) NOT NULL,
	[LunchTimeFinish] [time](7) NOT NULL,
	[ConvertToMins] [bigint] NOT NULL,
	[ConfirmationUnit] [decimal](18, 3) NOT NULL,
	[StartTimeOnRecord] [time](7) NOT NULL,
	[EndTimeOnRecord] [time](7) NOT NULL,
	[StartTimeOutRecord] [time](7) NOT NULL,
	[EndTimeOutRecord] [time](7) NOT NULL,
	[MinsWithoutTimeOn] [bigint] NOT NULL,
	[MinsWithoutTimeOut] [bigint] NOT NULL,
	[PositionOnReport] [int] NOT NULL,
	[IsSubtractionForLate] [bit] NOT NULL,
	[IsSubtractionForEarly] [bit] NOT NULL,
	[MinsAllowToLate] [bigint] NOT NULL,
	[MinsAllowToEarly] [bigint] NOT NULL,
	[IsCalculationForLate] [bit] NOT NULL,
	[IsCalculationForEarly] [bit] NOT NULL,
	[MinsRoundForLate] [bigint] NOT NULL,
	[MinsRoundForEarly] [bigint] NOT NULL,
	[IsRoundBackForLate] [bit] NOT NULL,
	[IsRoundBackForEarly] [bit] NOT NULL,
	[CreatedDatetimeUtc] [datetime2](7) NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[UpdatedDatetimeUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[DeletedDatetimeUtc] [datetime2](7) NULL,
	CONSTRAINT [PK_Shift] PRIMARY KEY CLUSTERED 
	(
		[ShiftConfigurationId] ASC
	)
)


ALTER TABLE [dbo].[ShiftConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_ShiftConfiguration_OvertimeConfiguration] FOREIGN KEY([OvertimeConfigurationId])
REFERENCES [dbo].[OvertimeConfiguration] ([OvertimeConfigurationId]);

ALTER TABLE [dbo].[SplitHour]  WITH CHECK ADD  CONSTRAINT [FK_SplitHour_SplitHour] FOREIGN KEY([TimeSortConfigurationId])
REFERENCES [dbo].[TimeSortConfiguration] ([TimeSortConfigurationId]);

ALTER TABLE [dbo].[WorkSchedule]  WITH CHECK ADD  CONSTRAINT [FK_WorkSchedule_TimeSortConfiguration] FOREIGN KEY([TimeSortConfigurationId])
REFERENCES [dbo].[TimeSortConfiguration] ([TimeSortConfigurationId]);
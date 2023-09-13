USE OrganizationDB
GO
/*
Run this script on:

        172.16.16.102\STD.OrganizationDB    -  This database will be modified

to synchronize it with:

        103.21.149.93.OrganizationDB

You are recommended to back up your database before running this script

Script created by SQL Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/6/2023 9:24:46 AM

*/
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL Serializable
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[HrAreaField]'
GO
ALTER TABLE [dbo].[HrAreaField] DROP CONSTRAINT [FK_HRAreaField_HRArea]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[HrAreaField] DROP CONSTRAINT [FK_HRAreaField_HRField]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[HrAreaField] DROP CONSTRAINT [FK_HRAreaField_HRType]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[ShiftConfiguration]'
GO
ALTER TABLE [dbo].[ShiftConfiguration] DROP CONSTRAINT [FK_ShiftConfiguration_OvertimeConfiguration]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[TimeSheetOvertime]'
GO
ALTER TABLE [dbo].[TimeSheetOvertime] DROP CONSTRAINT [FK_TimeSheetOvertime_OvertimeLevel]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[SalaryEmployee]'
GO
ALTER TABLE [dbo].[SalaryEmployee] DROP CONSTRAINT [FK_SalaryEmployee_SalaryPeriod]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[SalaryEmployee] DROP CONSTRAINT [FK_SalaryEmployee_HrBill]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[OvertimeConfiguration]'
GO
ALTER TABLE [dbo].[OvertimeConfiguration] DROP CONSTRAINT [PK_OvertimeConfiguration]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[OvertimeLevel]'
GO
ALTER TABLE [dbo].[OvertimeLevel] DROP CONSTRAINT [PK_OvertimeLevel]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[SalaryField]'
GO
ALTER TABLE [dbo].[SalaryField] DROP CONSTRAINT [DF_SalaryField_DecimalPlace]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [IX_SalaryEmployee] from [dbo].[SalaryEmployee]'
GO
DROP INDEX [IX_SalaryEmployee] ON [dbo].[SalaryEmployee]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Rebuilding [dbo].[OvertimeConfiguration]'
GO
CREATE TABLE [dbo].[RG_Recovery_1_OvertimeConfiguration]
(
[OvertimeConfigurationId] [int] NOT NULL IDENTITY(1, 1),
[WeekdayLevel] [int] NULL,
[IsWeekdayLevel] [bit] NOT NULL,
[WeekendLevel] [int] NULL,
[IsWeekendLevel] [bit] NOT NULL,
[HolidayLevel] [int] NULL,
[IsHolidayLevel] [bit] NOT NULL,
[WeekdayOvertimeLevel] [int] NULL,
[IsWeekdayOvertimeLevel] [bit] NOT NULL CONSTRAINT [DF__OvertimeC__IsWee__3D7EE6BA] DEFAULT ((0)),
[WeekendOvertimeLevel] [int] NULL,
[IsWeekendOvertimeLevel] [bit] NOT NULL CONSTRAINT [DF__OvertimeC__IsWee__3E730AF3] DEFAULT ((0)),
[HolidayOvertimeLevel] [int] NULL,
[IsHolidayOvertimeLevel] [bit] NOT NULL CONSTRAINT [DF__OvertimeC__IsHol__3F672F2C] DEFAULT ((0)),
[RoundMinutes] [int] NULL,
[IsRoundBack] [bit] NOT NULL CONSTRAINT [DF__OvertimeC__IsRou__405B5365] DEFAULT ((0)),
[OvertimeCalculationMode] [int] NOT NULL CONSTRAINT [DF__OvertimeC__Overt__414F779E] DEFAULT ((0)),
[OvertimeThresholdMins] [int] NULL,
[IsOvertimeThresholdMins] [bit] NOT NULL CONSTRAINT [DF__OvertimeC__IsOve__42439BD7] DEFAULT ((0)),
[MinThresholdMinutesBeforeWork] [int] NULL,
[MinThresholdMinutesAfterWork] [int] NULL,
[IsMinThresholdMinutesBeforeWork] [bit] NOT NULL,
[IsMinThresholdMinutesAfterWork] [bit] NOT NULL,
[MinsLimitOvertimeBeforeWork] [bigint] NOT NULL,
[MinsLimitOvertimeAfterWork] [bigint] NOT NULL,
[MinsReachesBeforeWork] [bigint] NOT NULL,
[MinsReachesAfterWork] [bigint] NOT NULL,
[MinsBonusWhenMinsReachesBeforeWork] [bigint] NOT NULL,
[MinsBonusWhenMinsReachesAfterWork] [bigint] NOT NULL,
[UpdatedByUserId] [int] NOT NULL,
[UpdatedDatetimeUtc] [datetime2] NOT NULL,
[IsDeleted] [bit] NOT NULL,
[DeletedDatetimeUtc] [datetime2] NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [dbo].[RG_Recovery_1_OvertimeConfiguration] ON
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
INSERT INTO [dbo].[RG_Recovery_1_OvertimeConfiguration]([OvertimeConfigurationId], [IsWeekdayLevel], [WeekendLevel], [IsWeekendLevel], [HolidayLevel], [IsHolidayLevel], [IsWeekdayOvertimeLevel], [IsWeekendOvertimeLevel], [IsHolidayOvertimeLevel], [IsRoundBack], [OvertimeCalculationMode], [IsOvertimeThresholdMins], [MinsLimitOvertimeBeforeWork], [MinsLimitOvertimeAfterWork], [MinsReachesBeforeWork], [MinsReachesAfterWork], [MinsBonusWhenMinsReachesBeforeWork], [MinsBonusWhenMinsReachesAfterWork], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc]) SELECT [OvertimeConfigurationId], [IsOvertimeLevel], [WeekendLevel], [IsWeekendLevel], [HolidayLevel], [IsHolidayLevel], [IsDayShiftLevel], [IsMinsAfterWork], [IsTotalHourWillCountShift], [IsMinsBeforeWork], [OvertimeLevel], [IsNightShiftLevel], [MinsLimitOvertimeBeforeWork], [MinsLimitOvertimeAfterWork], [MinsReachesBeforeWork], [MinsReachesAfterWork], [MinsBonusWhenMinsReachesBeforeWork], [MinsBonusWhenMinsReachesAfterWork], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc] FROM [dbo].[OvertimeConfiguration]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [dbo].[RG_Recovery_1_OvertimeConfiguration] OFF
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @idVal BIGINT
SELECT @idVal = IDENT_CURRENT(N'[dbo].[OvertimeConfiguration]')
IF @idVal IS NOT NULL
    DBCC CHECKIDENT(N'[dbo].[RG_Recovery_1_OvertimeConfiguration]', RESEED, @idVal)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DROP TABLE [dbo].[OvertimeConfiguration]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[dbo].[RG_Recovery_1_OvertimeConfiguration]', N'OvertimeConfiguration', N'OBJECT'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_OvertimeConfiguration] on [dbo].[OvertimeConfiguration]'
GO
ALTER TABLE [dbo].[OvertimeConfiguration] ADD CONSTRAINT [PK_OvertimeConfiguration] PRIMARY KEY CLUSTERED ([OvertimeConfigurationId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[OvertimeConfigurationMapping]'
GO
CREATE TABLE [dbo].[OvertimeConfigurationMapping]
(
[OvertimeConfigurationId] [int] NOT NULL,
[OvertimeLevelId] [int] NOT NULL,
[MinsLimit] [int] NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK__Overtime__F94FAF4E88EEADFA] on [dbo].[OvertimeConfigurationMapping]'
GO
ALTER TABLE [dbo].[OvertimeConfigurationMapping] ADD CONSTRAINT [PK__Overtime__F94FAF4E88EEADFA] PRIMARY KEY CLUSTERED ([OvertimeConfigurationId], [OvertimeLevelId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Rebuilding [dbo].[OvertimeLevel]'
GO
CREATE TABLE [dbo].[RG_Recovery_2_OvertimeLevel]
(
[OvertimeLevelId] [int] NOT NULL IDENTITY(1, 1),
[OvertimeRate] [decimal] (5, 2) NOT NULL,
[OvertimeCode] [nvarchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Description] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[OvertimePriority] [int] NOT NULL CONSTRAINT [DF__OvertimeL__Overt__1EA559DF] DEFAULT ((2)),
[SortOrder] [int] NOT NULL CONSTRAINT [DF__OvertimeL__SortO__603E1312] DEFAULT ((0)),
[UpdatedByUserId] [int] NOT NULL,
[UpdatedDatetimeUtc] [datetime2] NOT NULL,
[CreatedDatetimeUtc] [datetime2] NOT NULL,
[IsDeleted] [bit] NOT NULL,
[DeletedDatetimeUtc] [datetime2] NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [dbo].[RG_Recovery_2_OvertimeLevel] ON
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
INSERT INTO [dbo].[RG_Recovery_2_OvertimeLevel]([OvertimeLevelId], [OvertimeRate], [OvertimePriority], [UpdatedByUserId], [UpdatedDatetimeUtc], [CreatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc]) SELECT [OvertimeLevelId], CAST([OvertimeRate] AS [decimal] (5, 2)), [OrdinalNumber], [UpdatedByUserId], [UpdatedDatetimeUtc], [CreatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc] FROM [dbo].[OvertimeLevel]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [dbo].[RG_Recovery_2_OvertimeLevel] OFF
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @idVal BIGINT
SELECT @idVal = IDENT_CURRENT(N'[dbo].[OvertimeLevel]')
IF @idVal IS NOT NULL
    DBCC CHECKIDENT(N'[dbo].[RG_Recovery_2_OvertimeLevel]', RESEED, @idVal)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DROP TABLE [dbo].[OvertimeLevel]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[dbo].[RG_Recovery_2_OvertimeLevel]', N'OvertimeLevel', N'OBJECT'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_OvertimeLevel] on [dbo].[OvertimeLevel]'
GO
ALTER TABLE [dbo].[OvertimeLevel] ADD CONSTRAINT [PK_OvertimeLevel] PRIMARY KEY CLUSTERED ([OvertimeLevelId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[ShiftConfiguration]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[ShiftConfiguration] ADD
[Description] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IsNightShift] [bit] NOT NULL CONSTRAINT [DF__ShiftConf__IsNig__7D6461A5] DEFAULT ((0)),
[IsCheckOutDateTimekeeping] [bit] NULL CONSTRAINT [DF__ShiftConf__IsChe__7E5885DE] DEFAULT ((0)),
[IsSkipSaturdayWithShift] [bit] NOT NULL CONSTRAINT [DF__ShiftConf__IsSki__74100195] DEFAULT ((0)),
[IsSkipSundayWithShift] [bit] NOT NULL CONSTRAINT [DF__ShiftConf__IsSki__750425CE] DEFAULT ((0)),
[IsSkipHolidayWithShift] [bit] NOT NULL CONSTRAINT [DF__ShiftConf__IsSki__75F84A07] DEFAULT ((0)),
[IsCountWorkForHoliday] [bit] NOT NULL CONSTRAINT [DF__ShiftConf__IsCou__76EC6E40] DEFAULT ((0)),
[MaxLateMins] [int] NULL,
[MaxEarlyMins] [int] NULL,
[IsExceededLateAbsenceType] [bit] NOT NULL CONSTRAINT [DF__ShiftConf__IsExc__2883C9D4] DEFAULT ((0)),
[IsExceededEarlyAbsenceType] [bit] NOT NULL CONSTRAINT [DF__ShiftConf__IsExc__2977EE0D] DEFAULT ((0)),
[ExceededLateAbsenceTypeId] [int] NULL,
[ExceededEarlyAbsenceTypeId] [int] NULL,
[NoEntryTimeAbsenceTypeId] [int] NULL,
[NoExitTimeAbsenceTypeId] [int] NULL,
[IsNoEntryTimeWorkMins] [bit] NOT NULL CONSTRAINT [DF__ShiftConf__IsNoE__19418644] DEFAULT ((0)),
[IsNoExitTimeWorkMins] [bit] NOT NULL CONSTRAINT [DF__ShiftConf__IsNoE__1A35AA7D] DEFAULT ((0)),
[NoEntryTimeWorkMins] [bigint] NULL,
[NoExitTimeWorkMins] [bigint] NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[ShiftConfiguration] DROP
COLUMN [MinsWithoutTimeOn],
COLUMN [MinsWithoutTimeOut]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[dbo].[ShiftConfiguration].[EndDate]', N'EntryTime', N'COLUMN'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[dbo].[ShiftConfiguration].[BeginDate]', N'ExitTime', N'COLUMN'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[dbo].[ShiftConfiguration].[PositionOnReport]', N'WorkScheduleId', N'COLUMN'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[dbo].[ShiftConfiguration].[NumberOfTransition]', N'PartialShiftCalculationMode', N'COLUMN'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[ShiftConfiguration] ALTER COLUMN [LunchTimeStart] [time] NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[ShiftConfiguration] ALTER COLUMN [LunchTimeFinish] [time] NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [dbo].[ShiftConfiguration]'
GO
ALTER TABLE [dbo].[ShiftConfiguration] ADD CONSTRAINT [DF__ShiftConf__WorkS__731BDD5C] DEFAULT ((0)) FOR [WorkScheduleId]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[ShiftConfiguration] ADD CONSTRAINT [DF__ShiftConf__Parti__77E09279] DEFAULT ((0)) FOR [PartialShiftCalculationMode]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[AbsenceTypeSymbol]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[AbsenceTypeSymbol] ADD
[MaxOfDaysOffPerMonth] [int] NOT NULL CONSTRAINT [DF_AbsenceTypeSymbol_MaxOfDaysOffPerMonth] DEFAULT ((1)),
[SalaryRate] [float] NOT NULL CONSTRAINT [DF_AbsenceTypeSymbol_SalaryRate] DEFAULT ((1)),
[IsAnnualLeave] [bit] NOT NULL CONSTRAINT [DF__AbsenceTy__IsDef__2922E852] DEFAULT ((0)),
[CreatedByUserId] [int] NOT NULL CONSTRAINT [DF__AbsenceTy__Creat__5007AB49] DEFAULT ((0)),
[CreatedDatetimeUtc] [datetime2] NOT NULL CONSTRAINT [DF__AbsenceTy__Creat__50FBCF82] DEFAULT (getdate())
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[AbsenceTypeSymbol] DROP
COLUMN [TypeSymbolCode]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[AbsenceTypeSymbol] ALTER COLUMN [SymbolCode] [nvarchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [dbo].[AbsenceTypeSymbol]'
GO
ALTER TABLE [dbo].[AbsenceTypeSymbol] ADD CONSTRAINT [DF_AbsenceTypeSymbol_IsUsed] DEFAULT ((1)) FOR [IsUsed]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[AbsenceTypeSymbol] ADD CONSTRAINT [DF_AbsenceTypeSymbol_IsCounted] DEFAULT ((1)) FOR [IsCounted]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[HrAreaField]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[HrAreaField] ADD
[FiltersName] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[RequireFiltersName] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[TimeSheetRaw]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[TimeSheetRaw] ADD
[TimeKeepingMethod] [int] NOT NULL CONSTRAINT [DF__TimeSheet__TimeS__10422BEF] DEFAULT ((0)),
[TimeKeepingRecorder] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF__TimeSheet__Atten__11365028] DEFAULT ('')
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_SalaryEmployee] on [dbo].[SalaryEmployee]'
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SalaryEmployee] ON [dbo].[SalaryEmployee] ([SalaryPeriodId], [EmployeeId]) INCLUDE ([IsDeleted]) WHERE ([IsDeleted]=(0))
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[OvertimeConfigurationMapping]'
GO
ALTER TABLE [dbo].[OvertimeConfigurationMapping] ADD CONSTRAINT [FK__OvertimeC__Overt__4DB54E83] FOREIGN KEY ([OvertimeConfigurationId]) REFERENCES [dbo].[OvertimeConfiguration] ([OvertimeConfigurationId])
GO
ALTER TABLE [dbo].[OvertimeConfigurationMapping] ADD CONSTRAINT [FK__OvertimeC__Overt__4EA972BC] FOREIGN KEY ([OvertimeLevelId]) REFERENCES [dbo].[OvertimeLevel] ([OvertimeLevelId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[ShiftConfiguration]'
GO
ALTER TABLE [dbo].[ShiftConfiguration] ADD CONSTRAINT [FK_ShiftConfiguration_OvertimeConfiguration] FOREIGN KEY ([OvertimeConfigurationId]) REFERENCES [dbo].[OvertimeConfiguration] ([OvertimeConfigurationId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[TimeSheetOvertime]'
GO
ALTER TABLE [dbo].[TimeSheetOvertime] ADD CONSTRAINT [FK_TimeSheetOvertime_OvertimeLevel] FOREIGN KEY ([OvertimeLevelId]) REFERENCES [dbo].[OvertimeLevel] ([OvertimeLevelId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[SalaryEmployee]'
GO
ALTER TABLE [dbo].[SalaryEmployee] ADD CONSTRAINT [FK_SalaryEmployee_SalaryPeriod] FOREIGN KEY ([SalaryPeriodId]) REFERENCES [dbo].[SalaryPeriod] ([SalaryPeriodId])
GO
ALTER TABLE [dbo].[SalaryEmployee] ADD CONSTRAINT [FK_SalaryEmployee_HrBill] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[HrBill] ([F_Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [dbo].[SalaryField]'
GO
ALTER TABLE [dbo].[SalaryField] ADD CONSTRAINT [DF_SalaryField_DecimalPlace] DEFAULT ((0)) FOR [DecimalPlace]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
-- This statement writes to the SQL Server Log so SQL Monitor can show this deployment.
IF HAS_PERMS_BY_NAME(N'sys.xp_logevent', N'OBJECT', N'EXECUTE') = 1
BEGIN
    DECLARE @databaseName AS nvarchar(2048), @eventMessage AS nvarchar(2048)
    SET @databaseName = REPLACE(REPLACE(DB_NAME(), N'\', N'\\'), N'"', N'\"')
    SET @eventMessage = N'Redgate SQL Compare: { "deployment": { "description": "Redgate SQL Compare deployed to ' + @databaseName + N'", "database": "' + @databaseName + N'" }}'
    EXECUTE sys.xp_logevent 55000, @eventMessage
END
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO

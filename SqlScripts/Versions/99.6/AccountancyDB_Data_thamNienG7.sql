USE AccountancyDB
GO
/*
Run this script on:

172.16.16.102\STD.AccountancyDB    -  This database will be modified

to synchronize it with:

103.21.149.93.AccountancyDB

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/12/2023 10:13:32 AM

*/
		
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
GO
SET DATEFORMAT YMD
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL Serializable
GO
BEGIN TRANSACTION

PRINT(N'Update row in [dbo].[ProgramingFunction]')
UPDATE [dbo].[ProgramingFunction] SET [FunctionBody]=N'DECLARE @FromDate DATETIME2 = @ngay_bat_dau_lam_viec
DECLARE @ToDate DATETIME2 = datefromparts(@nam, @thang, @ngay)

DECLARE @DayFrom INT = DATEPART(DAY, @FromDate)
DECLARE @DayTo INT = DATEPART(DAY, @ToDate)
DECLARE @DayEndMonthTo INT = DATEPART(DAY,EOMONTH(@ToDate))

DECLARE @TotalMonths INT =  DATEDIFF(MONTH, @FromDate, @ToDate)
--DECLARE @TotalMonths01 INT =  DATEDIFF(DAY, @FromDate, @ToDate)

IF @DayFrom > @DayTo AND @DayTo < @DayEndMonthTo --AND (@TotalMonths01%30) != 0
	SET @TotalMonths = @TotalMonths - 1

SELECT @TotalMonths' WHERE [ProgramingFunctionId] = 102

PRINT(N'Add row to [dbo].[ProgramingFunction]')
SET IDENTITY_INSERT [dbo].[ProgramingFunction] ON
INSERT INTO [dbo].[ProgramingFunction] ([ProgramingFunctionId], [ProgramingFunctionName], [FunctionBody], [ProgramingLangId], [ProgramingLevelId], [Description], [Params]) VALUES (1103, N'NgayThamNienG7', N'DECLARE @FromDate DATETIME2 = @ngay_bat_dau_lam_viec
DECLARE @ToDate DATETIME2 = datefromparts(@nam, @thang, @ngay)

DECLARE @DayFrom INT = DATEPART(DAY, @FromDate)
DECLARE @DayTo INT = DATEPART(DAY, @ToDate)
DECLARE @MonthTo1 INT = DATEPART(MONTH, @ToDate) - 1
DECLARE @ToDate1 DATETIME2 = datefromparts(@nam, @MonthTo1, @ngay)
DECLARE @DayEndMonthTo1 INT = DATEPART(DAY,EOMONTH(@ToDate1))
--DECLARE @DayEndMonthTo INT = DATEPART(DAY,EOMONTH(@ToDate))
Declare @countDayOfMonth int = 0

IF @DayFrom > @DayTo 
	SET @countDayOfMonth = @DayTo + @DayEndMonthTo1 - @DayFrom
ELSE 
	SET @countDayOfMonth = @DayTo - @DayFrom


Select @countDayOfMonth', 1, 1, N'Ngày thâm niên G7', N'{"ReturnType":"void","ParamsList":[{"Name":"ngay_bat_dau_vao_lam","Type":"datetime","Description":"Ngày vào làm"},{"Name":"nam","Type":"int","Description":"Năm tính thâm niên"},{"Name":"thang","Type":"int","Description":"Tháng tính thâm niên"},{"Name":"ngay","Type":"int","Description":"Ngày tính thâm niên"}]}')
SET IDENTITY_INSERT [dbo].[ProgramingFunction] OFF
COMMIT TRANSACTION
GO

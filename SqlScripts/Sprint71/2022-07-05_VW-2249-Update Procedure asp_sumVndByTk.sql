USE [AccountancyDB]
GO
/****** Object:  StoredProcedure [dbo].[asp_sumVndByTk]    Script Date: 7/4/2022 7:00:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[asp_sumVndByTk]
(
	@SubId int,
	@tk_no nvarchar(max),
	@tk_co nvarchar(max),	
	@FromDate datetime2,
	@ToDate datetime2,
	@Thue_suat_VAT INT = NULL,
	@IsNotVat BIT = NULL,
	@Exist_Tk_No NVARCHAR(128) = NULL,
	@Result decimal(18,5) OUTPUT
) WITH RECOMPILE
AS
BEGIN
		
	DECLARE @WhereTkNo nvarchar(max) = ''
	DECLARE @WhereTkCo nvarchar(max) = ''

	DECLARE @Sql nvarchar(max) = ''

	IF LEN(@tk_no)>0
	BEGIN
		IF @tk_no = '-'
		BEGIN
			SELECT @WhereTkNo += ' OR (tk_no IS NOT NULL AND tk_no <> '''')';
		END
		ELSE
		BEGIN
			SELECT @WhereTkNo += ' OR tk_no LIKE ''' + LTRIM(RTRIM([Value])) + '%''' FROM dbo.ufn_Split(@tk_no,',')
		END
	END

	IF LEN(@tk_co)>0
	BEGIN
		IF @tk_co = '-'
		BEGIN
			SELECT @WhereTkCo += ' OR (tk_co IS NOT NULL AND tk_co <> '''')';
		END
		ELSE
		BEGIN
			SELECT @WhereTkCo += ' OR tk_co LIKE ''' + LTRIM(RTRIM([Value])) + '%''' FROM dbo.ufn_Split(@tk_co,',')
		END
	END

	SET @Sql = '
	SELECT @Result = SUM(Vnd) 
	FROM dbo._rc t WITH(NOLOCK)
	WHERE t.SubsidiaryId = @SubId AND [Ngay_ct] BETWEEN @FromDate AND @ToDate	
	';

	IF (LEN(@WhereTkNo)>0)
	BEGIN		
		SET @Sql += ' AND (1=0 ' + @WhereTkNo + ')'
	END

	IF (LEN(@WhereTkCo)>0)
	BEGIN		
		SET @Sql += ' AND (1=0 ' + @WhereTkCo + ')'
	END

	IF (@Thue_suat_VAT IS NOT NULL)
	BEGIN
		IF (@Thue_suat_VAT = 0)
		BEGIN
			SET @Sql += ' AND (Thue_suat_VAT IS NULL OR Thue_suat_VAT = 0)'
		END
		ELSE
		BEGIN
			SET @Sql += CONCAT(' AND Thue_suat_VAT = ', @Thue_suat_VAT)
		END
	END

	IF (@IsNotVat IS NOT NULL)
	BEGIN
		SET @Sql += CONCAT(' AND Not_VAT = ', @IsNotVat)
	END
	
	IF @Exist_Tk_No IS NOT NULL
	BEGIN
		SET @Sql += CONCAT(' AND EXISTS(SELECT 0 FROM dbo._rc c WHERE t.InputBill_F_Id = c.InputBill_F_Id AND c.tk_no LIKE ''', @Exist_Tk_No,'%'') ')
	END

	SET @Sql += ' OPTION(RECOMPILE)'
	PRINT @Sql
	EXEC sp_executesql @Sql, 
	N'	@SubId int,
		@Result DECIMAL(18,5) OUT,
		@FromDate DATETIME2,
		@ToDate DATETIME2
	'
	, @SubId = @SubId
	, @Result = @Result OUT
	, @FromDate = @FromDate
	, @ToDate = @ToDate;

END

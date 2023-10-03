USE ReportConfigDB
GO
/*
Run this script on:

172.16.16.102\STD.ReportConfigDB    -  This database will be modified

to synchronize it with:

103.21.149.93.ReportConfigDB

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/6/2023 6:59:42 PM

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

PRINT(N'Drop constraints from [dbo].[ReportType]')
ALTER TABLE [dbo].[ReportType] NOCHECK CONSTRAINT [FK_ReportType_ReportGroup]

PRINT(N'Drop constraint FK_ReportTypeView_ReportType from [dbo].[ReportTypeView]')
ALTER TABLE [dbo].[ReportTypeView] NOCHECK CONSTRAINT [FK_ReportTypeView_ReportType]

PRINT(N'Update rows in [dbo].[ReportType]')
UPDATE [dbo].[ReportType] SET [UpdatedDatetimeUtc]='2023-09-06 07:19:38.4009499', [HeadSql]=N'DECLARE @sl_dk_co decimal(18,5)
DECLARE @sl_dk_no decimal(18,5)
DECLARE @sl_dvt2_dk_co decimal(18,5)
DECLARE @sl_dvt2_dk_no decimal(18,5)

DECLARE @vnd_dk_co decimal(18,5)
DECLARE @vnd_dk_no decimal(18,5)

DECLARE @tk_title NVARCHAR(255)

DECLARE @unit_name NVARCHAR(255)

DECLARE @json_data NVARCHAR(MAX);

SELECT @tk_title = CONCAT(tk.AccountNumber, '' - '', tk.AccountNameVi) FROM v_AccountingAccount tk WHERE tk.AccountNumber = @Tk;
;With detailHeader AS(
SELECT DISTINCT
d.so_luong,
d.IsDebt,
d.Vnd_no,
d.Vnd_co,
so_ct,
thueXnk.vnd vnd
FROM dbo._rc_detail d 
LEFT JOIN [dbo].[v_Partner] p ON d.kh = p.F_Id
LEFT JOIN [dbo].[v_Department] de ON d.bo_phan = d.F_Id
LEFT JOIN [dbo].[v_Product] prd ON d.vthhtp = prd.F_Id
LEFT JOIN [dbo].[v_ProductUnitConversion] u ON prd.F_Id = u.ProductId
CROSS APPLY
	(
		SELECT 
			SUM(d1.Vnd_no) vnd
		FROM dbo._rc_detail d1
		WHERE d1.InputBill_F_Id = d.InputBill_F_Id 
		AND d1.vthhtp = d.vthhtp 
		AND d1.F_Id = d.F_Id 
		AND d1.BUT_TOAN = 3 
		AND d1.IsDebt = 1
	) thueXnk
WHERE d.SubsidiaryId = @SubId 
AND (d.Tk LIKE CONCAT(@Tk,''%'')) 
AND (ISNULL(@StockId,0) <= 0 OR CASE WHEN d.IsDebt = 1 THEN ISNULL(d.kho_lc, d.kho) ELSE d.kho END = @StockId)
AND d.ngay_ct < @FromDate
AND d.vthhtp = @ProductId
AND d.BUT_TOAN <> 3
)
SELECT
@sl_dk_no = SUM(CASE WHEN d.IsDebt = 1 THEN ISNULL(d.so_luong,0) ELSE 0 END),
@sl_dk_co = SUM(CASE WHEN d.IsDebt = 0 THEN ISNULL(d.so_luong,0) ELSE 0 END),

@vnd_dk_no = SUM(CASE WHEN d.IsDebt = 1 THEN ISNULL(d.Vnd_no,0)+ ISNULL(d.vnd,0) ELSE 0 END),
@vnd_dk_co = SUM(CASE WHEN d.IsDebt = 0 THEN ISNULL(d.Vnd_co,0) ELSE 0 END)
FROM detailHeader d
/*
SELECT 
    @sl_dk_no = SUM(ISNULL(_rc.so_luong,0)), @vnd_dk_no = SUM(ISNULL(_rc.vnd,0))
FROM dbo._rc_detail d 
WHERE _rc.SubsidiaryId = @SubId AND _rc.tk_no LIKE CONCAT(@Tk,''%'') AND _rc.ngay_ct < @FromDate AND _rc.vthhtp = @ProductId
GROUP BY _rc.tk_no

SELECT 
    @sl_dk_co = SUM(ISNULL(_rc.so_luong,0)), @vnd_dk_co = SUM(ISNULL(_rc.vnd,0))
FROM _rc 
WHERE _rc.SubsidiaryId = @SubId AND _rc.tk_co LIKE CONCAT(@Tk,''%'') AND _rc.ngay_ct < @FromDate AND _rc.vthhtp = @ProductId
GROUP BY _rc.tk_co
*/

DECLARE @vthhtp_ProductCode NVARCHAR(512)
DECLARE @vthhtp_ProductName NVARCHAR(512)

SELECT @vthhtp_ProductCode = p.ProductCode, @vthhtp_ProductName = p.ProductName, @unit_name = u.UnitName FROM v_Product p INNER JOIN v_Unit u ON p.UnitId = u.F_Id WHERE p.F_Id = @ProductId


;WITH detailUnitDvt2 AS (
	SELECT 
			ROW_NUMBER() OVER (PARTITION BY d.vthhtp_dvt2 ORDER BY  d.ngay_ct, prd.F_Id, d.vthhtp_dvt2) as stt,
		     d.vthhtp_dvt2 unitId,-- d.vthhtp_dvt2 IS NULL THEN defaultUnit.F_Id ELSE d.vthhtp_dvt2 END AS  unitId,
			 CASE WHEN d.vthhtp_dvt2 IS NULL THEN defaultUnit.ProductUnitConversionName ELSE u.ProductUnitConversionName END AS unitName,
			d.so_luong_dv2 sl_dvt2
			,d.so_ct soct,
			d.IsDebt IsDebt
			, prd.F_Id productId
	FROM dbo._rc_detail d
	LEFT JOIN [dbo].[v_ProductUnitConversion] u ON d.vthhtp_dvt2 = u.F_Id 
	LEFT JOIN [dbo].[v_Product] prd ON d.vthhtp = prd.F_Id
	OUTER APPLY(
	SELECT * FROM dbo.v_ProductUnitConversion def WHERE def.IsDefault =1 AND def.ProductId = @ProductId
	) defaultUnit
	 WHERE d.SubsidiaryId = @SubId 
		AND (d.Tk LIKE CONCAT(@Tk,''%'')) 
		AND d.ngay_ct < @FromDate
		AND d.vthhtp = @ProductId
		AND d.BUT_TOAN <> 3
), tongDauKy AS(
SELECT 
*,
SUM(CASE WHEN d.IsDebt =1 THEN ISNULL(d.sl_dvt2,0) ELSE- ISNULL(d.sl_dvt2,0) END ) OVER (PARTITION BY d.unitId) AS tonDauKy
FROM detailUnitDvt2 d
)
SELECT  @json_data =
(SELECT DISTINCT
d.F_Id,
d.ProductUnitConversionName unitName, 
ISNULL (tonDauKy,0) AS tonDauKy
FROM dbo.v_ProductUnitConversion d
LEFT JOIN tongDauKy u ON u.unitId = d.F_Id
WHERE d.ProductId = @ProductId-- AND d.IsDefault = 0
FOR JSON PATH)


SELECT 
    (ISNULL(@sl_dk_no,0) - ISNULL(@sl_dk_co,0)) sl_dau_ky, 
	(ISNULL(@sl_dvt2_dk_no,0) - ISNULL(@sl_dvt2_dk_co,0)) sl_dvt2_dau_ky, 
    (ISNULL(@vnd_dk_no,0) - ISNULL(@vnd_dk_co,0)) vnd_dau_ky, 
    @FromDate tu_ngay, 
    @ToDate den_ngay,
    @tk_title Tk_Title,
    @unit_name unit_name,
    @vthhtp_ProductCode vthhtp_ProductCode,
    @vthhtp_ProductName vthhtp_ProductName,
	@json_data json_data





SELECT CONCAT( N''Đơn vị: '',unitName) AS unitName,
CONCAT(N''Đầu kỳ: '', ISNULL( tonDauKy,0)) AS tonDauKy
FROM OPENJSON(@json_data)
WITH(
  unitName NVARCHAR(50) ''$.unitName''
, tonDauKy decimal ''$.tonDauKy''
)


-- SELECT CONCAT( N''Đơn vị: '',d.ProductUnitConversionName),
-- CONCAT(N''Đầu kỳ: '', ISNULL( tonDauKy,0))
-- FROM dbo.v_ProductUnitConversion d
-- LEFT JOIN tongDauKy u ON u.unitId = d.F_Id
-- WHERE d.ProductId = @ProductId


--SELECT N''Đơn vị: m3'', N''Đầu kỳ: '', 1000' WHERE [ReportTypeId] = 67
UPDATE [dbo].[ReportType] SET [UpdatedDatetimeUtc]='2023-09-06 07:19:38.6355585', [HeadSql]=N'DECLARE @sl_dk_co decimal(18,5)
DECLARE @sl_dk_no decimal(18,5)
DECLARE @sl_dvt2_dk_co decimal(18,5)
DECLARE @sl_dvt2_dk_no decimal(18,5)

DECLARE @vnd_dk_co decimal(18,5)
DECLARE @vnd_dk_no decimal(18,5)

DECLARE @tk_title NVARCHAR(255)

DECLARE @unit_name NVARCHAR(255)

DECLARE @json_data NVARCHAR(MAX);

SELECT @tk_title = CONCAT(tk.AccountNumber, '' - '', tk.AccountNameVi) FROM v_AccountingAccount tk WHERE tk.AccountNumber = @Tk;
;With detailHeader AS(
SELECT DISTINCT
d.so_luong,
d.IsDebt,
d.Vnd_no,
d.Vnd_co,
so_ct,
thueXnk.vnd vnd
FROM dbo._rc_detail d 
LEFT JOIN [dbo].[v_Partner] p ON d.kh = p.F_Id
LEFT JOIN [dbo].[v_Department] de ON d.bo_phan = d.F_Id
LEFT JOIN [dbo].[v_Product] prd ON d.vthhtp = prd.F_Id
LEFT JOIN [dbo].[v_ProductUnitConversion] u ON prd.F_Id = u.ProductId
CROSS APPLY
	(
		SELECT 
			SUM(d1.Vnd_no) vnd
		FROM dbo._rc_detail d1
		WHERE d1.InputBill_F_Id = d.InputBill_F_Id 
		AND d1.vthhtp = d.vthhtp 
		AND d1.F_Id = d.F_Id 
		AND d1.BUT_TOAN = 3 
		AND d1.IsDebt = 1
	) thueXnk
WHERE d.SubsidiaryId = @SubId 
AND (d.Tk LIKE CONCAT(@Tk,''%'')) 
AND (ISNULL(@StockId,0) <= 0 OR CASE WHEN d.IsDebt = 1 THEN ISNULL(d.kho_lc, d.kho) ELSE d.kho END = @StockId)
AND d.ngay_ct < @FromDate
AND d.vthhtp = @ProductId
AND d.BUT_TOAN <> 3
)
SELECT
@sl_dk_no = SUM(CASE WHEN d.IsDebt = 1 THEN ISNULL(d.so_luong,0) ELSE 0 END),
@sl_dk_co = SUM(CASE WHEN d.IsDebt = 0 THEN ISNULL(d.so_luong,0) ELSE 0 END),

@vnd_dk_no = SUM(CASE WHEN d.IsDebt = 1 THEN ISNULL(d.Vnd_no,0)+ ISNULL(d.vnd,0) ELSE 0 END),
@vnd_dk_co = SUM(CASE WHEN d.IsDebt = 0 THEN ISNULL(d.Vnd_co,0) ELSE 0 END)
FROM detailHeader d
/*
SELECT 
    @sl_dk_no = SUM(ISNULL(_rc.so_luong,0)), @vnd_dk_no = SUM(ISNULL(_rc.vnd,0))
FROM dbo._rc_detail d 
WHERE _rc.SubsidiaryId = @SubId AND _rc.tk_no LIKE CONCAT(@Tk,''%'') AND _rc.ngay_ct < @FromDate AND _rc.vthhtp = @ProductId
GROUP BY _rc.tk_no

SELECT 
    @sl_dk_co = SUM(ISNULL(_rc.so_luong,0)), @vnd_dk_co = SUM(ISNULL(_rc.vnd,0))
FROM _rc 
WHERE _rc.SubsidiaryId = @SubId AND _rc.tk_co LIKE CONCAT(@Tk,''%'') AND _rc.ngay_ct < @FromDate AND _rc.vthhtp = @ProductId
GROUP BY _rc.tk_co
*/

DECLARE @vthhtp_ProductCode NVARCHAR(512)
DECLARE @vthhtp_ProductName NVARCHAR(512)

SELECT @vthhtp_ProductCode = p.ProductCode, @vthhtp_ProductName = p.ProductName, @unit_name = u.UnitName FROM v_Product p INNER JOIN v_Unit u ON p.UnitId = u.F_Id WHERE p.F_Id = @ProductId


;WITH detailUnitDvt2 AS (
	SELECT 
			ROW_NUMBER() OVER (PARTITION BY d.vthhtp_dvt2 ORDER BY  d.ngay_ct, prd.F_Id, d.vthhtp_dvt2) as stt,
		     d.vthhtp_dvt2 unitId,-- d.vthhtp_dvt2 IS NULL THEN defaultUnit.F_Id ELSE d.vthhtp_dvt2 END AS  unitId,
			 CASE WHEN d.vthhtp_dvt2 IS NULL THEN defaultUnit.ProductUnitConversionName ELSE u.ProductUnitConversionName END AS unitName,
			d.so_luong_dv2 sl_dvt2
			,d.so_ct soct,
			d.IsDebt IsDebt
			, prd.F_Id productId
	FROM dbo._rc_detail d
	LEFT JOIN [dbo].[v_ProductUnitConversion] u ON d.vthhtp_dvt2 = u.F_Id 
	LEFT JOIN [dbo].[v_Product] prd ON d.vthhtp = prd.F_Id
	OUTER APPLY(
	SELECT * FROM dbo.v_ProductUnitConversion def WHERE def.IsDefault =1 AND def.ProductId = @ProductId
	) defaultUnit
	 WHERE d.SubsidiaryId = @SubId 
		AND (d.Tk LIKE CONCAT(@Tk,''%'')) 
		AND d.ngay_ct < @FromDate
		AND d.vthhtp = @ProductId
		AND d.BUT_TOAN <> 3
), tongDauKy AS(
SELECT 
*,
SUM(CASE WHEN d.IsDebt =1 THEN ISNULL(d.sl_dvt2,0) ELSE- ISNULL(d.sl_dvt2,0) END ) OVER (PARTITION BY d.unitId) AS tonDauKy
FROM detailUnitDvt2 d
)
SELECT  @json_data =
(SELECT DISTINCT
d.F_Id,
d.ProductUnitConversionName unitName, 
ISNULL (tonDauKy,0) AS tonDauKy
FROM dbo.v_ProductUnitConversion d
LEFT JOIN tongDauKy u ON u.unitId = d.F_Id
WHERE d.ProductId = @ProductId-- AND d.IsDefault = 0
FOR JSON PATH)


SELECT 
    (ISNULL(@sl_dk_no,0) - ISNULL(@sl_dk_co,0)) sl_dau_ky, 
	(ISNULL(@sl_dvt2_dk_no,0) - ISNULL(@sl_dvt2_dk_co,0)) sl_dvt2_dau_ky, 
    (ISNULL(@vnd_dk_no,0) - ISNULL(@vnd_dk_co,0)) vnd_dau_ky, 
    @FromDate tu_ngay, 
    @ToDate den_ngay,
    @tk_title Tk_Title,
    @unit_name unit_name,
    @vthhtp_ProductCode vthhtp_ProductCode,
    @vthhtp_ProductName vthhtp_ProductName,
	@json_data json_data





SELECT CONCAT( N''Đơn vị: '',unitName) AS unitName,
CONCAT(N''Đầu kỳ: '', ISNULL( tonDauKy,0)) AS tonDauKy
FROM OPENJSON(@json_data)
WITH(
  unitName NVARCHAR(50) ''$.unitName''
, tonDauKy decimal ''$.tonDauKy''
)


-- SELECT CONCAT( N''Đơn vị: '',d.ProductUnitConversionName),
-- CONCAT(N''Đầu kỳ: '', ISNULL( tonDauKy,0))
-- FROM dbo.v_ProductUnitConversion d
-- LEFT JOIN tongDauKy u ON u.unitId = d.F_Id
-- WHERE d.ProductId = @ProductId


--SELECT N''Đơn vị: m3'', N''Đầu kỳ: '', 1000' WHERE [ReportTypeId] = 190
UPDATE [dbo].[ReportType] SET [UpdatedDatetimeUtc]='2023-09-06 11:23:18.3808751', [BodySql]=N'SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

/*
DECLARE @StepId INT;
DECLARE @DepartmentId INT;
DECLARE @FromDate DATETIME2 = NULL;
DECLARE @ToDate DATETIME2 = NULL;
*/

DECLARE @CONTAINER_TYPE_PRODUCTION_ORDER INT = 2;
DECLARE @PRODUCTION_STEP_LINK_DATA_ROLE_TYPE_IN INT = 1;
--DECLARE @PRODUCTION_STEP_LINK_DATA_ROLE_TYPE_OUT INT = 2;
DECLARE @ASSIGNED_PROGRESS_STATUS_FINISH INT = 3;
DECLARE @LINK_DATA_OBJECT_TYPE_PRODUCT INT = 1;
DECLARE @LINK_DATA_OBJECT_TYPE_SEMI INT = 2;

DECLARE @HANDOVER_STATUS_APPROVED INT = 1;

DROP TABLE IF EXISTS #assignment;
SELECT o.ProductionOrderId,
       a.ProductionStepLinkDataId,
       s.ProductionStepId,
       a.DepartmentId,
       a.AssignmentQuantity,
       d.Quantity,
       d.QuantityOrigin,
       DATEDIFF(DAY, a.StartDate, a.EndDate) + 1 AssignDays,
       a.StartDate
INTO #assignment
FROM dbo.ProductionOrder o
    JOIN dbo.ProductionStep s
        ON o.ProductionOrderId = s.ContainerId
           AND s.ContainerTypeId = @CONTAINER_TYPE_PRODUCTION_ORDER
    JOIN dbo.ProductionAssignment a
        ON s.ProductionStepId = a.ProductionStepId
    JOIN dbo.ProductionStepLinkData d
        ON d.ProductionStepLinkDataId = a.ProductionStepLinkDataId
WHERE o.IsDeleted = 0
      AND s.IsDeleted = 0
      AND d.IsDeleted = 0
      AND o.IsFinished = 0
      AND a.AssignedProgressStatus <> @ASSIGNED_PROGRESS_STATUS_FINISH
      AND
      (
          @FromDate IS NULL
          OR o.[Date] >= @FromDate
      )
      AND
      (
          @ToDate IS NULL
          OR o.[Date] <= @ToDate
      );

DROP TABLE IF EXISTS #productionOrderStep;
SELECT DISTINCT
       a.ProductionOrderId,
       a.ProductionStepId
INTO #productionOrderStep
FROM #assignment a;

DROP TABLE IF EXISTS #input;
SELECT s.ProductionOrderId,
       s.ProductionStepId,
       d.ProductionStepLinkDataId,
       d.LinkDataObjectTypeId,
       d.LinkDataObjectId,
       d.Quantity,
       d.QuantityOrigin
INTO #input
FROM #productionOrderStep s
    JOIN dbo.ProductionStepLinkDataRole r
        ON r.ProductionStepId = s.ProductionStepId
           AND r.ProductionStepLinkDataRoleTypeId = @PRODUCTION_STEP_LINK_DATA_ROLE_TYPE_IN
    JOIN dbo.ProductionStepLinkData d
        ON d.ProductionStepLinkDataId = r.ProductionStepLinkDataId
WHERE d.IsDeleted = 0;

DROP TABLE IF EXISTS #handover;
SELECT h.ToProductionStepId,
       h.ToDepartmentId,
       h.ObjectTypeId,
       h.ObjectId,
       MIN(h.HandoverDatetime) MinHandoverDatetime,
       SUM(h.HandoverQuantity) TotalHandoverQuantity
INTO #handover
FROM dbo.ProductionHandover h
    JOIN #productionOrderStep s
        ON h.ToProductionStepId = s.ProductionStepId
WHERE h.[Status] = @HANDOVER_STATUS_APPROVED
      AND h.IsDeleted = 0
GROUP BY h.ToProductionStepId,
         h.ToDepartmentId,
         h.ObjectTypeId,
         h.ObjectId;

DROP TABLE IF EXISTS #inpAssignment;

;WITH assignInpProductionStep
AS (SELECT a.ProductionOrderId,
           a.ProductionStepId,
           a.DepartmentId,
           a.ProductionStepLinkDataId,
           a.LinkDataObjectTypeId,
           a.LinkDataObjectId,
           a.AssignmentQuantity,
           a.AssignStartDate,
           a.AssignDays,
           a.AssignmentQuantity - ISNULL(h.TotalHandoverQuantity, 0) WaitingQuantity,
           h.MinHandoverDatetime,
           CASE
               WHEN h.TotalHandoverQuantity > 0 THEN
                   0
               ELSE
                   1
           END IsNotHandoverYet
    FROM
    (
        SELECT inp.ProductionOrderId,
               inp.ProductionStepId,
               a.DepartmentId,
               inp.ProductionStepLinkDataId,
               inp.LinkDataObjectTypeId,
               inp.LinkDataObjectId,
               inp.QuantityOrigin * a.AssignmentQuantity / a.QuantityOrigin AssignmentQuantity,
               a.StartDate AssignStartDate,
               a.AssignDays
        FROM #assignment a
            JOIN #input inp
                ON a.ProductionStepId = inp.ProductionStepId
    ) a
        LEFT JOIN #handover h
            ON a.ProductionStepId = h.ToProductionStepId
               AND a.DepartmentId = h.ToDepartmentId
               AND a.LinkDataObjectTypeId = h.ObjectTypeId
               AND a.LinkDataObjectId = h.ObjectId)
SELECT a.ProductionOrderId,
       o.ProductionOrderCode,
       a.StepId,
       a.DepartmentId,
       a.PairId,
       a.LinkDataObjectTypeId,
       a.LinkDataObjectId,
       p.ProductCode,
       ISNULL(p.ProductName, semi.Title) ProductName,
       a.AssignmentQuantity,
       a.AssignStartDate,
       a.AssignDays,
       a.WaitingQuantity,
       a.MinHandoverDatetime,
       a.IsNotHandoverYet,
       ROW_NUMBER() OVER (PARTITION BY a.StepId,
                                       a.DepartmentId,
                                       a.IsNotHandoverYet
                          ORDER BY a.AssignStartDate,
                                   o.ProductionOrderCode
                         ) GroupSortOrder
INTO #inpAssignment
FROM
(
    SELECT a.ProductionOrderId,
           parent.StepId,
           a.DepartmentId,
           CONCAT(parent.StepId, ''A'', a.DepartmentId) PairId,
           a.LinkDataObjectTypeId,
           a.LinkDataObjectId,
           SUM(a.AssignmentQuantity) AssignmentQuantity,
           MIN(a.AssignStartDate) AssignStartDate,
           MIN(a.MinHandoverDatetime) MinHandoverDatetime,
           MAX(a.AssignDays) AssignDays,
           SUM(a.WaitingQuantity) WaitingQuantity,
           a.IsNotHandoverYet
    FROM assignInpProductionStep a
        JOIN dbo.ProductionStep s
            ON s.ProductionStepId = a.ProductionStepId
        LEFT JOIN dbo.ProductionStep parent
            ON s.ParentId = parent.ProductionStepId
    GROUP BY a.ProductionOrderId,
             parent.StepId,
             a.DepartmentId,
             a.LinkDataObjectTypeId,
             a.LinkDataObjectId,
             a.IsNotHandoverYet
) a
    LEFT JOIN dbo.ProductionOrder o
        ON a.ProductionOrderId = o.ProductionOrderId
    LEFT JOIN dbo.v_Product p
        ON a.LinkDataObjectId = p.F_Id
           AND a.LinkDataObjectTypeId = @LINK_DATA_OBJECT_TYPE_PRODUCT
    LEFT JOIN dbo.ProductSemi semi
        ON a.LinkDataObjectId = semi.ProductSemiId
           AND a.LinkDataObjectTypeId = @LINK_DATA_OBJECT_TYPE_SEMI
WHERE (
          @StepId IS NULL
          OR a.StepId = @StepId
      )
      AND
      (
          @DepartmentId IS NULL
          OR a.DepartmentId = @DepartmentId
      );

DROP TABLE IF EXISTS #stepDepartnent;
CREATE TABLE #stepDepartnent
(
    PairId NVARCHAR(512) NOT NULL,
    StepId INT NOT NULL,
    StepName NVARCHAR(128) NOT NULL,
    DepartmentId INT NOT NULL,
    DepartmentName NVARCHAR(128) NOT NULL,
    Title NVARCHAR(512) NOT NULL
);
INSERT INTO #stepDepartnent
(
    PairId,
    StepId,
    StepName,
    DepartmentId,
    DepartmentName,
    Title
)
SELECT CONCAT(p.StepId, ''A'', p.DepartmentId),
       p.StepId,
       s.StepName,
       p.DepartmentId,
       d.DepartmentName,
       CONCAT(s.StepName, '' / '', d.DepartmentName)
FROM
(SELECT DISTINCT StepId, DepartmentId FROM #inpAssignment) p
    LEFT JOIN dbo.Step s
        ON p.StepId = s.StepId
    LEFT JOIN dbo.v_Department d
        ON p.DepartmentId = d.F_Id
    OUTER APPLY
(
    SELECT sd.StepDetailId
    FROM dbo.StepDetail sd
    WHERE p.StepId = sd.StepId
          AND p.DepartmentId = sd.DepartmentId
          AND sd.IsDeleted = 0
) sd
ORDER BY s.SortOrder,
         sd.StepDetailId;

;WITH v
AS (SELECT CONCAT(inp.StepId, ''A'', inp.DepartmentId) [$RepeatId],
           CONCAT(inp.IsNotHandoverYet, ''A'', inp.GroupSortOrder) [$MergeRowId],
           s.Title GroupTitle,
           inp.ProductionOrderId,
           inp.ProductionOrderCode,
           inp.StepId,
           inp.DepartmentId,
           inp.PairId,
           inp.LinkDataObjectTypeId,
           inp.LinkDataObjectId,
           inp.ProductCode,
           inp.ProductName,
           inp.AssignmentQuantity,
           inp.AssignStartDate,
           inp.AssignDays,
           inp.WaitingQuantity,
           inp.MinHandoverDatetime,
           inp.IsNotHandoverYet,
           inp.GroupSortOrder
    FROM #inpAssignment inp
        LEFT JOIN #stepDepartnent s
            ON s.StepId = inp.StepId
               AND s.DepartmentId = inp.DepartmentId)
SELECT *
FROM
(
    SELECT 1 Area,
           N''1. Các mặt hàng đang chờ tại tổ'' Title,
           CONCAT(s.StepId, ''A'', s.DepartmentId) [$RepeatId],
           ''1'' [$MergeRowId],
           s.Title GroupTitle,
           NULL ProductionOrderId,
           NULL ProductionOrderCode,
           s.StepId,
           s.DepartmentId,
           s.PairId,
           NULL LinkDataObjectTypeId,
           NULL LinkDataObjectId,
           NULL ProductCode,
           NULL ProductName,
           NULL AssignmentQuantity,
           NULL AssignStartDate,
           NULL AssignDays,
           NULL WaitingQuantity,
           NULL MinHandoverDatetime,
           0 IsNotHandoverYet,
           NULL GroupSortOrder,
           0 RowNumber,
           0 IsSum,
           ''{"background": "#fff9ce", "fontWeight":"bold"}'' [$ROW_CSS_STYLE],
           ''*=Title'' [$GROUP_COLUMN]
    FROM #stepDepartnent s
    UNION
    SELECT 4 Area,
           N''2. Các mặt hàng đang SX ở các công đoạn trước '' Title,
           NULL [$RepeatId],
           ''4'' [$MergeRowId],
           NULL GroupTitle,
           NULL ProductionOrderId,
           NULL ProductionOrderCode,
           NULL StepId,
           NULL DepartmentId,
           NULL PairId,
           NULL LinkDataObjectTypeId,
           NULL LinkDataObjectId,
           NULL ProductCode,
           NULL ProductName,
           NULL AssignmentQuantity,
           NULL AssignStartDate,
           NULL AssignDays,
           NULL WaitingQuantity,
           NULL MinHandoverDatetime,
           0 IsNotHandoverYet,
           NULL GroupSortOrder,
           0 RowNumber,
           0 IsSum,
           ''{"background": "#fff9ce", "fontWeight":"bold"}'' [$ROW_CSS_STYLE],
           ''*=Title'' [$GROUP_COLUMN]
    UNION
    SELECT CASE
               WHEN v.IsNotHandoverYet = 0 THEN
                   2
               ELSE
                   5
           END Area,
           NULL Title,
           [$RepeatId],
           [$MergeRowId],
           GroupTitle,
           ProductionOrderId,
           ProductionOrderCode,
           StepId,
           DepartmentId,
           PairId,
           LinkDataObjectTypeId,
           LinkDataObjectId,
           ProductCode,
           ProductName,
           AssignmentQuantity,
           AssignStartDate,
           AssignDays,
           WaitingQuantity,
           MinHandoverDatetime,
           IsNotHandoverYet,
           v.GroupSortOrder,
           ROW_NUMBER() OVER (ORDER BY
                                  $ORDERBY:GroupSortOrder:
                                  ,
                                  v.GroupSortOrder
                             ) RowNumber,
            0 IsSum,
           NULL [$ROW_CSS_STYLE],
           NULL [$GROUP_COLUMN]
    FROM v
    ($FILTER::WHERE)

    UNION
    SELECT CASE
               WHEN v.IsNotHandoverYet = 0 THEN
                   3
               ELSE
                   6
           END Area,
           N''Tổng'' Title,
           v.[$RepeatId],
           CASE
               WHEN v.IsNotHandoverYet = 0 THEN
                   ''3''
               ELSE
                   ''6''
           END [$MergeRowId],
           s.Title GroupTitle,
           NULL ProductionOrderId,
           NULL ProductionOrderCode,
           s.StepId,
           s.DepartmentId,
           s.PairId,
           NULL LinkDataObjectTypeId,
           NULL LinkDataObjectId,
           NULL ProductCode,
           NULL ProductName,
           SUM(AssignmentQuantity) AssignmentQuantity,
           NULL AssignStartDate,
           NULL AssignDays,
           SUM(CASE WHEN WaitingQuantity > 0 THEN WaitingQuantity ELSE 0 END) WaitingQuantity,
           NULL MinHandoverDatetime,
           0 IsNotHandoverYet,
           MAX(v.GroupSortOrder) + 1 RowNumber,
           MAX(v.GroupSortOrder) + 1 RowNumber,
           1 IsSum,
           ''{"background": "#f7efeb", "fontWeight":"bold"}'' [$ROW_CSS_STYLE],
           ''ProductionOrderCode,ProductCode,ProductName=Title'' [$GROUP_COLUMN]
    FROM #stepDepartnent s
        JOIN v
            ON v.DepartmentId = s.DepartmentId
               AND v.StepId = s.StepId
    GROUP BY s.DepartmentId,
             s.StepId,
             s.Title,
             s.PairId,
             v.IsNotHandoverYet,
             v.[$RepeatId]
    UNION
    SELECT CASE
               WHEN t.IsNotHandoverYet = 0 THEN
                   3
               ELSE
                   6
           END Area,
           N''<Không có mặt hàng nào>'' Title,
           NULL [$RepeatId],
           CASE
               WHEN t.IsNotHandoverYet = 0 THEN
                   ''3''
               ELSE
                   ''6''
           END [$MergeRowId],
           NULL GroupTitle,
           NULL ProductionOrderId,
           NULL ProductionOrderCode,
           NULL StepId,
           NULL DepartmentId,
           NULL PairId,
           NULL LinkDataObjectTypeId,
           NULL LinkDataObjectId,
           NULL ProductCode,
           NULL ProductName,
           NULL AssignmentQuantity,
           NULL AssignStartDate,
           NULL AssignDays,
           NULL WaitingQuantity,
           NULL MinHandoverDatetime,
           NULL IsNotHandoverYet,
           MAX(v.GroupSortOrder) + 1 GroupSortOrder,
           MAX(v.GroupSortOrder) + 1 RowNumber,
           0 IsSum,
           ''{"background": "#f7efeb", "fontWeight":"bold"}'' [$ROW_CSS_STYLE],
           ''*=Title'' [$GROUP_COLUMN]
    FROM
    (
        VALUES
            (0),
            (1)
    ) t (IsNotHandoverYet)
        LEFT JOIN v
            ON v.IsNotHandoverYet = t.IsNotHandoverYet
    GROUP BY t.IsNotHandoverYet
    HAVING COUNT(v.IsNotHandoverYet) = 0
) v
ORDER BY v.Area,
         v.RowNumber;



/*
DECLARE @CONTAINER_TYPE_PRODUCTION_ORDER INT = 2;
DECLARE @PRODUCTION_STEP_LINK_DATA_ROLE_TYPE_IN INT = 1;
--DECLARE @PRODUCTION_STEP_LINK_DATA_ROLE_TYPE_OUT INT = 2;
DECLARE @ASSIGNED_PROGRESS_STATUS_FINISH INT = 3;
DECLARE @LINK_DATA_OBJECT_TYPE_PRODUCT INT = 1;
DECLARE @LINK_DATA_OBJECT_TYPE_SEMI INT = 2;

DROP TABLE IF EXISTS #assignment;
SELECT o.ProductionOrderId,
       a.ProductionStepLinkDataId,
       s.ProductionStepId,
       a.DepartmentId,
       a.AssignmentQuantity,
       d.Quantity,
       d.QuantityOrigin,
       DATEDIFF(DAY, a.StartDate, a.EndDate) AssignDays,
       a.StartDate
INTO #assignment
FROM dbo.ProductionOrder o
    JOIN dbo.ProductionStep s
        ON o.ProductionOrderId = s.ContainerId
           AND s.ContainerTypeId = @CONTAINER_TYPE_PRODUCTION_ORDER
    JOIN dbo.ProductionAssignment a
        ON s.ProductionStepId = a.ProductionStepId
    JOIN dbo.ProductionStepLinkData d
        ON d.ProductionStepLinkDataId = a.ProductionStepLinkDataId
WHERE o.IsDeleted = 0
      AND s.IsDeleted = 0
      AND d.IsDeleted = 0
      AND o.IsFinished = 0
      AND a.AssignedProgressStatus <> @ASSIGNED_PROGRESS_STATUS_FINISH;

DROP TABLE IF EXISTS #productionOrderStep;
SELECT DISTINCT
       a.ProductionOrderId,
       a.ProductionStepId
INTO #productionOrderStep
FROM #assignment a;

DROP TABLE IF EXISTS #input;
SELECT s.ProductionOrderId,
       s.ProductionStepId,
       d.ProductionStepLinkDataId,
       d.LinkDataObjectTypeId,
       d.LinkDataObjectId,
       d.Quantity,
       d.QuantityOrigin
INTO #input
FROM #productionOrderStep s
    JOIN dbo.ProductionStepLinkDataRole r
        ON r.ProductionStepId = s.ProductionStepId
           AND r.ProductionStepLinkDataRoleTypeId = @PRODUCTION_STEP_LINK_DATA_ROLE_TYPE_IN
    JOIN dbo.ProductionStepLinkData d
        ON d.ProductionStepLinkDataId = r.ProductionStepLinkDataId
WHERE d.IsDeleted = 0;

DROP TABLE IF EXISTS #handover;
SELECT h.ToProductionStepId,
       h.ToDepartmentId,
       h.ObjectTypeId,
       h.ObjectId,
       MIN(h.HandoverDatetime) MinHandoverDatetime,
       SUM(h.HandoverQuantity) TotalHandoverQuantity
INTO #handover
FROM dbo.ProductionHandover h
    JOIN #productionOrderStep s
        ON h.ToProductionStepId = s.ProductionStepId
WHERE h.Status = 1
      AND h.IsDeleted = 0
GROUP BY h.ToProductionStepId,
         h.ToDepartmentId,
         h.ObjectTypeId,
         h.ObjectId;

DROP TABLE IF EXISTS #inpAssignment;

;WITH assignInpProductionStep
AS (SELECT a.ProductionOrderId,
           a.ProductionStepId,
           a.DepartmentId,
           a.ProductionStepLinkDataId,
           a.LinkDataObjectTypeId,
           a.LinkDataObjectId,
           a.AssignmentQuantity,
           a.StartDate,
           a.AssignDays,
           a.AssignmentQuantity - ISNULL(h.TotalHandoverQuantity, 0) WaitingQuantity,
           CASE
               WHEN h.TotalHandoverQuantity > 0 THEN
                   0
               ELSE
                   1
           END IsNotHandoverYet
    FROM
    (
        SELECT inp.ProductionOrderId,
               inp.ProductionStepId,
               a.DepartmentId,
               inp.ProductionStepLinkDataId,
               inp.LinkDataObjectTypeId,
               inp.LinkDataObjectId,
               inp.QuantityOrigin * a.AssignmentQuantity / a.QuantityOrigin AssignmentQuantity,
               a.StartDate,
               a.AssignDays
        FROM #assignment a
            JOIN #input inp
                ON a.ProductionStepId = inp.ProductionStepId
    ) a
        LEFT JOIN #handover h
            ON a.ProductionStepId = h.ToProductionStepId
               AND a.DepartmentId = h.ToDepartmentId
               AND a.LinkDataObjectTypeId = h.ObjectTypeId
               AND a.LinkDataObjectId = h.ObjectId)
SELECT a.ProductionOrderId,
       o.ProductionOrderCode,
       a.StepId,
       a.DepartmentId,
       a.PairId,
       a.LinkDataObjectTypeId,
       a.LinkDataObjectId,
       p.ProductCode,
       ISNULL(p.ProductName, semi.Title) ProductName,
       a.AssignmentQuantity,
       a.StartDate,
       a.AssignDays,
       a.WaitingQuantity,
       a.IsNotHandoverYet,
	   ROW_NUMBER() OVER(PARTITION BY a.StepId, a.DepartmentId ORDER BY a.StartDate, o.ProductionOrderCode) RowNumber
INTO #inpAssignment
FROM
(
    SELECT a.ProductionOrderId,
           parent.StepId,
           a.DepartmentId,
           CONCAT(parent.StepId, ''A'', a.DepartmentId) PairId,
           a.LinkDataObjectTypeId,
           a.LinkDataObjectId,
           SUM(a.AssignmentQuantity) AssignmentQuantity,
           MIN(a.StartDate) StartDate,
           MAX(a.AssignDays) AssignDays,
           SUM(a.WaitingQuantity) WaitingQuantity,
           a.IsNotHandoverYet
    FROM assignInpProductionStep a
        JOIN dbo.ProductionStep s
            ON s.ProductionStepId = a.ProductionStepId
        LEFT JOIN dbo.ProductionStep parent
            ON s.ParentId = parent.ProductionStepId
    GROUP BY a.ProductionOrderId,
             parent.StepId,
             a.DepartmentId,
             a.LinkDataObjectTypeId,
             a.LinkDataObjectId,
             a.IsNotHandoverYet
) a
    LEFT JOIN dbo.ProductionOrder o
        ON a.ProductionOrderId = o.ProductionOrderId
    LEFT JOIN dbo.v_Product p
        ON a.LinkDataObjectId = p.F_Id
           AND a.LinkDataObjectTypeId = @LINK_DATA_OBJECT_TYPE_PRODUCT
    LEFT JOIN dbo.ProductSemi semi
        ON a.LinkDataObjectId = semi.ProductSemiId
           AND a.LinkDataObjectTypeId = @LINK_DATA_OBJECT_TYPE_SEMI;

DROP TABLE IF EXISTS  #stepDepartnent
CREATE TABLE #stepDepartnent
(
    PairId NVARCHAR(512) NOT NULL,
    StepId INT NOT NULL,
    StepName NVARCHAR(128) NOT NULL,
    DepartmentId INT NOT NULL,
    DepartmentName NVARCHAR(128) NOT NULL,
    Title NVARCHAR(512) NOT NULL
);
INSERT INTO #stepDepartnent
(
    PairId,
    StepId,
    StepName,
    DepartmentId,
    DepartmentName,
    Title
)
SELECT CONCAT(p.StepId, ''A'', p.DepartmentId),
       p.StepId,
       s.StepName,
       p.DepartmentId,
       d.DepartmentName,
       CONCAT(s.StepName, '' / '', d.DepartmentName)
FROM
(SELECT DISTINCT StepId, DepartmentId FROM #inpAssignment) p
    LEFT JOIN dbo.Step s
        ON p.StepId = s.StepId
    LEFT JOIN dbo.v_Department d
        ON p.DepartmentId = d.F_Id
    OUTER APPLY
(
    SELECT sd.StepDetailId
    FROM dbo.StepDetail sd
    WHERE p.StepId = sd.StepId
          AND p.DepartmentId = sd.DepartmentId
          AND sd.IsDeleted = 0
) sd
ORDER BY s.SortOrder,
         sd.StepDetailId;


DECLARE @SqlSelect NVARCHAR(max) = ''SELECT r.RowId ''
DECLARE @SqlJoin NVARCHAR(max) = ''FROM vRows r''

SELECT 

	@SqlSelect = CONCAT(@SqlSelect,''
			,
		    CASE WHEN inp'',PairId,''.RowNumber = 1 THEN s'',PairId,''.Title ELSE NULL END AS [GroupTitle'', PairId, ''],
			inp'',PairId,''.StepId AS [StepId'', PairId, ''],
			inp'',PairId,''.ProductionOrderCode AS [ProductionOrderCode'', PairId, ''],
			inp'',PairId,''.ProductionOrderId AS [ProductionOrderId'', PairId, ''],
			inp'',PairId,''.LinkDataObjectTypeId AS [LinkDataObjectTypeId'', PairId, ''],
			inp'',PairId,''.LinkDataObjectId AS [LinkDataObjectId'', PairId, ''],
			inp'',PairId,''.ProductCode AS [ProductCode'', PairId, ''],
			inp'',PairId,''.ProductName AS [ProductName'', PairId, ''],
			inp'',PairId,''.AssignmentQuantity AS [AssignmentQuantity'', PairId, ''],
			inp'',PairId,''.StartDate AS [StartDate'', PairId, ''],
			inp'',PairId,''.AssignDays AS [AssignDays'', PairId, ''],
			inp'',PairId,''.WaitingQuantity AS [WaitingQuantity'', PairId, '']		
	''),
	@SqlJoin = CONCAT(@SqlJoin,''
			
			LEFT JOIN #inpAssignment inp'',PairId,'' ON r.RowId = inp'',PairId,''.RowNumber AND inp'',PairId,''.StepId = '', StepId, '' AND inp'',PairId,''.DepartmentId = '', DepartmentId ,''
			LEFT JOIN #stepDepartnent s'',PairId,'' ON s'',PairId,''.StepId = '', StepId, '' AND s'',PairId,''.DepartmentId = '', DepartmentId ,''
	'')

FROM #stepDepartnent;



SET @SqlSelect = CONCAT(''
;WITH vRows AS (
	SELECT DISTINCT RowNumber RowId FROM #inpAssignment
)
'', @SqlSelect, @SqlJoin)

--SELECT CAST(''<root><![CDATA['' + @SqlSelect + '']]></root>'' AS XML);

EXEC( @SqlSelect)*/', [Columns]=N'[{"SortOrder":1,"Name":"#","Value":"Title","Alias":"Title","DataTypeId":1,"IsCalcSum":false,"IsHidden":false,"IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":false,"ColGroupId":0,"ColGroupName":"#"},{"SortOrder":2,"Name":"Mã lệnh SX","Value":"ProductionOrderCode","Alias":"ProductionOrderCode","DataTypeId":1,"IsArray":false,"IsRepeat":true,"IsCalcSum":false,"IsHidden":false,"RowSpan":"1","ColSpan":"1","IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":true,"ColGroupId":2,"ColGroupName":"GroupTitle","DetailOpenTypeId":2,"DetailTargetId":1,"DetailReportId":147,"DetailReportParams":"StepId=#StepId&ProductionOrderId=#ProductionOrderId"},{"SortOrder":3,"Name":"Mã hàng","Value":"ProductCode","Alias":"ProductCode","DataTypeId":1,"IsRepeat":true,"IsCalcSum":false,"IsHidden":false,"IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":true,"ColGroupId":2,"ColGroupName":"Mã hàng","DetailOpenTypeId":1,"DetailReportParams":"/system/products/view/#LinkDataObjectId","DetailJsCodeCanOpenTarget":"let LINK_DATA_OBJECT_TYPE_PRODUCT = 1;\r\nreturn $item.LinkDataObjectTypeId == LINK_DATA_OBJECT_TYPE_PRODUCT;"},{"SortOrder":4,"Name":"Tên hàng","Value":"ProductName","Alias":"ProductName","DataTypeId":1,"IsArray":false,"IsRepeat":true,"IsCalcSum":false,"IsHidden":false,"RowSpan":"1","ColSpan":"1","IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":true,"ColGroupId":2,"ColGroupName":"GroupTitle","DetailReportId":147,"DetailReportParams":"StepId=#StepId&ProductionOrderId=#ProductionOrderId"},{"SortOrder":5,"Name":"Số lượng YC","Value":"AssignmentQuantity","Alias":"AssignmentQuantity","DataTypeId":9,"IsRepeat":true,"IsCalcSum":true,"CalcSumConditionCol":"IsSum","IsHidden":false,"RowSpan":"1","ColSpan":"1","IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":true,"ColGroupId":2,"ColGroupName":"GroupTitle","DetailReportId":147,"DetailReportParams":"StepId=#StepId&ProductionOrderId=#ProductionOrderId"},{"SortOrder":6,"Name":"Số lượng còn lại","Value":"WaitingQuantity","Alias":"WaitingQuantity","DataTypeId":9,"IsRepeat":true,"IsCalcSum":true,"CalcSumConditionCol":"IsSum","IsHidden":false,"IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":true,"ColGroupId":2,"ColGroupName":"Số lượng BSP"},{"SortOrder":7,"Name":"Ngày nhận bàn giao","Value":"MinHandoverDatetime","Alias":"MinHandoverDatetime","DataTypeId":3,"IsRepeat":true,"IsCalcSum":false,"IsHidden":false,"IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":true,"ColGroupId":2,"ColGroupName":"Ngày nhận bàn giao"},{"SortOrder":8,"Name":"TG SX dự kiến","Value":"AssignDays","Alias":"AssignDays","DataTypeId":9,"IsRepeat":true,"IsCalcSum":false,"IsHidden":false,"IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":true,"ColGroupId":2,"ColGroupName":"TG SX dự kiến"},{"SortOrder":9,"Name":"Thời gian đưa vào\n sx dự kiến","Value":"AssignStartDate","Alias":"AssignStartDate","DataTypeId":3,"IsRepeat":true,"IsCalcSum":false,"IsHidden":false,"IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":true,"ColGroupId":2,"ColGroupName":"Thời gian đưa vào\n sx dự kiến"},{"SortOrder":10,"Name":"StepId","Value":"StepId","Alias":"StepId","DataTypeId":2,"IsRepeat":true,"IsCalcSum":false,"IsHidden":true,"IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":false,"ColGroupId":0,"ColGroupName":"StepId"},{"SortOrder":11,"Name":"ProductionOrderId","Value":"ProductionOrderId","Alias":"ProductionOrderId","DataTypeId":2,"IsRepeat":true,"IsCalcSum":false,"IsHidden":true,"IsDockLeft":false,"IsGroup":false,"IsGroupRow":false,"IsGroupRowLevel2":false,"IsColGroup":false,"ColGroupId":0,"ColGroupName":"ProductionOrderId"}]' WHERE [ReportTypeId] = 271
PRINT(N'Operation applied to 3 rows out of 3')

PRINT(N'Add constraints to [dbo].[ReportType]')
ALTER TABLE [dbo].[ReportType] WITH CHECK CHECK CONSTRAINT [FK_ReportType_ReportGroup]
ALTER TABLE [dbo].[ReportTypeView] WITH CHECK CHECK CONSTRAINT [FK_ReportTypeView_ReportType]
COMMIT TRANSACTION
GO

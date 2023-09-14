USE AccountancyPublicDB
GO
/*
Run this script on:

172.16.16.102\STD.AccountancyPublicDB    -  This database will be modified

to synchronize it with:

103.21.149.93.AccountancyPublicDB

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/6/2023 9:42:57 AM

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

PRINT(N'Drop constraints from [dbo].[InputAreaField]')
ALTER TABLE [dbo].[InputAreaField] NOCHECK CONSTRAINT [FK_InputAreaField_InputArea]
ALTER TABLE [dbo].[InputAreaField] NOCHECK CONSTRAINT [FK_InputAreaField_InputField]
ALTER TABLE [dbo].[InputAreaField] NOCHECK CONSTRAINT [FK_InputAreaField_InputType]

PRINT(N'Drop constraints from [dbo].[InputArea]')
ALTER TABLE [dbo].[InputArea] NOCHECK CONSTRAINT [FK_InputArea_InputType]

PRINT(N'Drop constraints from [dbo].[InputType]')
ALTER TABLE [dbo].[InputType] NOCHECK CONSTRAINT [FK_InputType_InputType]
ALTER TABLE [dbo].[InputType] NOCHECK CONSTRAINT [FK_InputType_InputTypeGroup]

PRINT(N'Drop constraint FK_InputTypeView_InputType from [dbo].[InputTypeView]')
ALTER TABLE [dbo].[InputTypeView] NOCHECK CONSTRAINT [FK_InputTypeView_InputType]

PRINT(N'Drop constraint FK_InputValueBill_InputType from [dbo].[InputBill]')
ALTER TABLE [dbo].[InputBill] NOCHECK CONSTRAINT [FK_InputValueBill_InputType]

PRINT(N'Update rows in [dbo].[InputAreaField]')
UPDATE [dbo].[InputAreaField] SET [Filters]=N'{"condition":"AND","rules":[{"id":"79","field":"F_Id","type":"string","input":"text","operator":5,"value":null,"dataType":2,"fieldName":"F_Id"}],"not":false,"valid":true}', [UpdatedByUserId]=170, [UpdatedDatetimeUtc]='2023-07-17 07:36:06.2795161', [FiltersName]=N'TK có phải là tài khoản không chứa tài khoản con nào' WHERE [InputAreaFieldId] = 25
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=228, [UpdatedDatetimeUtc]='2023-07-11 02:07:26.9305752', [RequireFiltersName]=N'tét' WHERE [InputAreaFieldId] = 26
UPDATE [dbo].[InputAreaField] SET [Filters]=N'{"condition":"AND","rules":[{"id":"1","field":"AccountNumber","type":"string","input":"text","operator":5,"value":null,"dataType":1,"fieldName":"AccountNumber"}],"not":false,"valid":true}', [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-11 07:13:02.3259539', [RequireFilters]=NULL, [FiltersName]=N'Tài khoản kế toán phải là tài khoản cấp cuối cùng', [RequireFiltersName]=N'' WHERE [InputAreaFieldId] = 30
UPDATE [dbo].[InputAreaField] SET [Filters]=N'{"condition":"AND","rules":[{"id":"79","field":"F_Id","type":"string","input":"text","operator":5,"value":null,"dataType":2,"fieldName":"F_Id"}],"not":false,"valid":true}', [UpdatedByUserId]=228, [UpdatedDatetimeUtc]='2023-07-10 02:49:06.3844954', [FiltersName]=N'', [RequireFiltersName]=N'' WHERE [InputAreaFieldId] = 291
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=228, [UpdatedDatetimeUtc]='2023-07-10 02:49:06.3842203', [RequireFiltersName]=N'test' WHERE [InputAreaFieldId] = 294
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=228, [UpdatedDatetimeUtc]='2023-07-10 02:35:59.0043561' WHERE [InputAreaFieldId] = 298
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-10 09:19:09.0770539', [RequireFiltersName]=N'' WHERE [InputAreaFieldId] = 311
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-10 09:04:52.0004551' WHERE [InputAreaFieldId] = 312
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-08-25 07:20:55.5951730', [RequireFilters]=N'{"condition":"AND","rules":[{"id":"30","field":"tk_no0","type":"string","input":"text","operator":6,"value":"15","dataType":1,"fieldName":"tk_no0"}],"not":false,"valid":true}' WHERE [InputAreaFieldId] = 372
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=228, [UpdatedDatetimeUtc]='2023-07-06 08:58:55.6567953', [RequireFilters]=NULL WHERE [InputAreaFieldId] = 440
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=228, [UpdatedDatetimeUtc]='2023-07-06 08:58:55.6565262', [RequireFilters]=NULL, [RequireFiltersName]=N're' WHERE [InputAreaFieldId] = 443
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-11 03:40:53.3972140', [RequireFilters]=NULL WHERE [InputAreaFieldId] = 473
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=228, [UpdatedDatetimeUtc]='2023-07-06 09:04:28.2625518', [RequireFilters]=NULL WHERE [InputAreaFieldId] = 476
UPDATE [dbo].[InputAreaField] SET [Filters]=N'{"condition":"AND","rules":[{"id":"79","field":"F_Id","type":"string","input":"text","operator":5,"value":null,"dataType":2,"fieldName":"F_Id"}],"not":false,"valid":true}', [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-10 03:54:18.7016906', [FiltersName]=N'', [RequireFiltersName]=N'' WHERE [InputAreaFieldId] = 480
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=228, [UpdatedDatetimeUtc]='2023-07-04 10:13:08.3303601' WHERE [InputAreaFieldId] = 1417
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-10 09:03:45.5782123', [RequireFiltersName]=N'' WHERE [InputAreaFieldId] = 1421
UPDATE [dbo].[InputAreaField] SET [SortOrder]=25, [UpdatedDatetimeUtc]='2023-07-28 11:39:35.0159534' WHERE [InputAreaFieldId] = 1635
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-25 09:50:30.4909095' WHERE [InputAreaFieldId] = 1647
UPDATE [dbo].[InputAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-25 09:49:16.1749716' WHERE [InputAreaFieldId] = 1668
UPDATE [dbo].[InputAreaField] SET [SortOrder]=11, [IsRequire]=0, [DefaultValue]=N'', [OnBlur]=NULL, [AutoFocus]=NULL, [Column]=0, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:38:00.2592731', [IsDeleted]=0, [RequireFilters]=NULL WHERE [InputAreaFieldId] = 1712
UPDATE [dbo].[InputAreaField] SET [SortOrder]=10, [DefaultValue]=N'', [OnBlur]=NULL, [AutoFocus]=NULL, [Column]=0, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:38:00.2589963', [IsDeleted]=0 WHERE [InputAreaFieldId] = 1713
UPDATE [dbo].[InputAreaField] SET [SortOrder]=5, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:38:00.2587224' WHERE [InputAreaFieldId] = 1714
UPDATE [dbo].[InputAreaField] SET [SortOrder]=4, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:38:00.2573269' WHERE [InputAreaFieldId] = 1716
UPDATE [dbo].[InputAreaField] SET [SortOrder]=3, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:38:00.2570496' WHERE [InputAreaFieldId] = 1718
UPDATE [dbo].[InputAreaField] SET [SortOrder]=7, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:38:00.2578877' WHERE [InputAreaFieldId] = 1719
UPDATE [dbo].[InputAreaField] SET [SortOrder]=6, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:38:00.2576105' WHERE [InputAreaFieldId] = 1720
UPDATE [dbo].[InputAreaField] SET [SortOrder]=8, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:38:00.2581637' WHERE [InputAreaFieldId] = 1721
UPDATE [dbo].[InputAreaField] SET [SortOrder]=9, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:38:00.2584354' WHERE [InputAreaFieldId] = 1722
UPDATE [dbo].[InputAreaField] SET [OnChange]=N'/** merged*/
$currentRow.kh0_PartnerName.value = $currentRow.kh0.value ? $currentRow.kh0.refObject.PartnerName : '''';', [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:34:28.5989547' WHERE [InputAreaFieldId] = 2026
UPDATE [dbo].[InputAreaField] SET [OnBlur]=NULL, [Column]=0, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:34:28.5995225', [IsDeleted]=0 WHERE [InputAreaFieldId] = 2029
UPDATE [dbo].[InputAreaField] SET [OnBlur]=NULL, [Column]=0, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-28 11:34:28.5992372', [IsDeleted]=0 WHERE [InputAreaFieldId] = 3020
UPDATE [dbo].[InputAreaField] SET [TitleStyleJson]=N'{
    "width":"250px"
}', [UpdatedByUserId]=170, [UpdatedDatetimeUtc]='2023-08-02 09:00:35.8648583' WHERE [InputAreaFieldId] = 3259
UPDATE [dbo].[InputAreaField] SET [SortOrder]=17, [UpdatedDatetimeUtc]='2023-07-28 11:41:31.8506268' WHERE [InputAreaFieldId] = 3288
UPDATE [dbo].[InputAreaField] SET [TitleStyleJson]=N'{
    "width":"250px"
}', [UpdatedByUserId]=170, [UpdatedDatetimeUtc]='2023-08-02 09:00:35.8650023' WHERE [InputAreaFieldId] = 3557
PRINT(N'Operation applied to 34 rows out of 34')

PRINT(N'Update row in [dbo].[ProgramingFunction]')
UPDATE [dbo].[ProgramingFunction] SET [FunctionBody]=N'/**
 * Updated date : 2022-01-10
 * Updated by   : trungvt
 * Desc         : Round vnd, tong tien by currency decimal place
 */

/**
 * Updated date : 2022-03-28
 * Updated by   : locnt
 * Desc         : additional vnd4, tong tien hang = vnd0+vnd3+vnd4
 */

/**
 * Updated date : 2022-06-08
 * Updated by   : locnt
 * Desc         : update executes only on specified row if $index is declared;
 */

/**
 * Updated date : 2022-11-01
 * Updated by   : locnt
 * Desc         : add ngoai_te1, VAT ngoại tệ;
 */

/**
 *Trường loại tiền
 * @type {BasicField}
 */
var $loai_tien_default = null;

/**
 *Trường tỷ giá
 * @type {BasicField}
 */
var $ty_gia_default = null;

var $defaultRow = {};
for (const [_, area] of Object.entries($bill)) {
  if (!area.isMultiRow) {
    area.rows.forEach(r => {
      if (r.loai_tien) {
        $loai_tien_default = r.loai_tien;
        $defaultRow = r;
      }

      if (r.ty_gia) {
        $ty_gia_default = r.ty_gia;
      }
    });
  }
}

var isPrimaryCurrency = checkIsPrimaryCurrency($defaultRow, $loai_tien_default);
var tyGiaValue = getTyGiaValue($defaultRow, isPrimaryCurrency, $ty_gia_default);
var decimalPlaceCurrency = getLoaiTienDecimalPlace($defaultRow, $loai_tien_default);
var decimalPlaceDefault = window.PrimaryCurrency?.DecimalPlace;

var currentFieldName = $currentField?.fieldName;
var isOnTableRow = Object.values($bill).some(a => a.isMultiRow && a.rows.find(r => r == $currentRow));
var $isOnLoad = $data.$isOnLoad;
var $changeRows = $data.$changeRows;
var isFieldChangeInGeneral = (!isOnTableRow && (currentFieldName == ''loai_tien'' || currentFieldName == ''ty_gia''));
var isRecalcTax = false;
var isCurrentFieldsForCalc = checkCurrentFieldsForCalc();


/**
 * Tổng thành tiền
 */
var sumVnd0 = 0;
/**
 * Tổng thuế XNK
 */
var sumVnd3 = 0;
/**
 * Tổng thuế VAT
 */
var sumVnd1 = 0;
/**
 * Tổng chi phí khác
 */
var sumVnd4 = 0;

/**
 * Số tiền (chứng từ phân bổ)
 */
var sumSoTien = 0;

//Tính thuế XNK và VAT cho dòng dữ liệu trong bảng chi tiết
for (const [_, area] of Object.entries($bill)) {
  if (area.isMultiRow) {
    area.rows.forEach((row, indexRow) => {
      var fixedFieldName = $currentField ? $currentField.fieldName : null;
      if (!fixedFieldName || !row[fixedFieldName]) {
        fixedFieldName = row[''don_gia0''] && row[''don_gia0''].value ? ''don_gia0'' : ''vnd0'';
      }
      if ($fixedFieldName) { fixedFieldName = $fixedFieldName };

      /**
      * Updated date : 2022-11-22
      * Updated by   : locnt
      * Desc         : không chay calcPriceMoneyByRow_New khi mới vào trang lần đầu, thêm $isOnTable, $isOnLoad, $changeRows
      */
      // executes only on specified row having value change;
      var isRowCurrent = $currentRow == row;
      var isChangedRow = ($changeRows && $changeRows.some(r => r == row));

      if (!$isOnLoad
        && (
          (isRowCurrent && isCurrentFieldsForCalc)
          || (!$currentRow && isChangedRow)
        )) {
        calcPriceMoneyByRow_New($bill, row, fixedFieldName);
        calcTax(row, 0, 0, 0);
        isRecalcTax = true;
      }

      calcSumViewOnly(row, 0, 0, 0);

      for(let p in row){
        let ps =  p.split(''$'');
        if(ps[0]==''vnd0''){
           //if (row.vnd0) {
            sumVnd0 += getNumberValue(row[p].value);
          //}
        }
      }
     
      if (row.vnd1) {
        sumVnd1 += getNumberValue(row.vnd1.value);
      }
      if (row.tien_thue) {
        sumVnd1 += getNumberValue(row.tien_thue.value);
      }
      if (row.vnd3) {
        sumVnd3 += getNumberValue(row.vnd3.value);
      }
      if (row.vnd4) {
        sumVnd4 += getNumberValue(row.vnd4.value);
      }

      if (row.so_tien) {
        sumSoTien += getNumberValue(row.so_tien.value);
      }
    });
  }
}

//Tính thuế XNK và VAT cho dòng dữ liệu dòng ở vùng tổng cộng
for (const [_, area] of Object.entries($bill)) {
  if (!area.isMultiRow) {
    area.rows.forEach(row => {
      if (isRecalcTax || ($currentRow == row && isCurrentFieldsForCalc)) {
        calcTax(row, sumVnd0, sumVnd3, sumSoTien);
      }
      calcSumViewOnly(row, sumVnd0, sumVnd3, sumVnd1, sumVnd4, sumSoTien);
    });
  }
}

processWarningBill($data);

/**
 * Tính thuế XNK và VAT cho dòng dữ liệu
 * @param {BillRow} $row - Dòng dữ liệu
 * @param {number} vnd0Value - (Tổng) giá trị tiền hàng mặc định
 * @param {number} vnd3Value - (Tổng) giá trị thuế XNK mặc định
 * @param {number} vnd1Value - (Tổng) giá trị thuế VAT mặc định
 * @param {number} vnd4Value - (Tổng) giá trị chi phí khác mặc định

 */
function calcSumViewOnly($row, vnd0Value, vnd3Value, vnd1Value, vnd4Value, soTienValue) {
  if ($row.vnd0) {
    vnd0Value = getNumberValue($row.vnd0.value);
  }

  if ($row.tien_thue) {
    vnd1Value = getNumberValue($row.tien_thue.value);
  }

  if ($row.vnd1) {
    vnd1Value = getNumberValue($row.vnd1.value);
  }

  if ($row.vnd3) {
    vnd3Value = getNumberValue($row.vnd3.value);
  }

  if ($row.vnd4) {
    vnd4Value = getNumberValue($row.vnd4.value);
  }

  if ($row.soTienValue) {
    soTienValue = getNumberValue($row.so_tien.value);
  }

  var rowSum = vnd0Value + vnd3Value + soTienValue;
  var rowSumWithVnd4 = vnd0Value + vnd3Value + vnd4Value + soTienValue;
  if ($row.tong_tien_hang) {
    // TH có field vnd4 xuất hiện
    if ($row.vnd4) {
      $row.tong_tien_hang.value = roundNumber(rowSumWithVnd4, decimalPlaceDefault);
    } else {
      $row.tong_tien_hang.value = roundNumber(rowSum, decimalPlaceDefault);
    }
  }

  if ($row.tong_cong) {
    $row.tong_cong.value = roundNumber(rowSum + vnd1Value, decimalPlaceDefault);
  }
  if ($row.tong_vnd0) {
    $row.tong_vnd0.value = rowSum;
  }

  if ($row.einvoice_tong_cong) {
    $row.einvoice_tong_cong.value = roundNumber(rowSum + vnd1Value, decimalPlaceDefault);
  }
  if ($row.einvoice_tong_vnd0) {
    $row.einvoice_tong_vnd0.value = roundNumber(rowSum, decimalPlaceDefault);
  }

  if ($row.bang_chu) {
    $row.bang_chu.value = readMoney($row.tong_cong ? $row.tong_cong.value : $row.einvoice_tong_cong.value);
  }
}

function calcTax($row, vnd0Value, vnd3Value, sumSoTien) {

  if ($row.vnd0) {
    vnd0Value = getNumberValue($row.vnd0.value);
  }
  if ($row.vnd3) {
    vnd3Value = getNumberValue($row.vnd3.value);
  }

  if ($row.thue_suat_xnk) {
    var thueXnk = getNumberValue($row.thue_suat_xnk.value);
    if ($row.vnd3) {
      var vnd3 = vnd0Value * thueXnk / 100.0;
      $row.vnd3.value = roundNumber(vnd3, decimalPlaceDefault);
    }
  }

  if ($row.thue_suat_vat && $row.thue_suat_vat.value >= 0) {
    var vatValue = getNumberValue($row.thue_suat_vat.value);
    if ($row.vnd1 || $row.tien_thue) {
      var rowSum = vnd0Value + vnd3Value + sumSoTien;
      const vnd1Value = rowSum * vatValue / 100;
      if ($row.vnd1)
        $row.vnd1.value = roundNumber(vnd1Value, decimalPlaceDefault);
      if ($row.tien_thue)
        $row.tien_thue.value = roundNumber(vnd1Value, decimalPlaceDefault);
    }
    // don''t executes when delete row;
    if (!isPrimaryCurrency && $row.ngoai_te1 && $row.ngoai_te0 && $row.ngoai_te0.value && !$data.$isDeleteRow) {
      $row.ngoai_te1.value = roundNumber($row.ngoai_te0.value * vatValue / 100, $row.ngoai_te1.decimalPlace);
    }
  }
}

function checkCurrentFieldsForCalc() {
  if ($fixedFieldName) { currentFieldName = $fixedFieldName };

  if (currentFieldName == ''vnd1'' || currentFieldName == ''vnd3'') return false;
  // update thue_suat_xnk0 -> thue_suat_xnk
  if ([''ty_gia'', ''loai_tien'', ''so_luong'', ''so_luong_dv2'', ''vthhtp_dvt2'', ''gia_dinh_muc'', ''loi_nhuan'', ''thue_suat_vat'', ''thue_suat_xnk''].includes(currentFieldName)
    || (currentFieldName && currentFieldName.includes(''don_gia''))
    || (currentFieldName && currentFieldName.includes(''ngoai_te''))
    || (currentFieldName && currentFieldName.includes(''vnd''))) {
    return true;
  }
  return false;
}' WHERE [ProgramingFunctionId] = 62

PRINT(N'Update row in [dbo].[InputTypeGlobalSetting]')
UPDATE [dbo].[InputTypeGlobalSetting] SET [UpdatedDatetimeUtc]='2023-08-22 07:50:48.7571957', [BeforeSubmitAction]=N'let valid = window.validPairAccountant($data);
if(!valid) return valid;
valid = validPairCustomer_Accountant($data);
if(!valid) return valid;
return validBillEntryDate($data)


// const valid = window.validPairAccountant($data);
//   if(valid){
//       return validPairCustomer_Accountant($data);
//   }else {
//       return valid;
//   }', [AfterSaveAction]=N'--SQL action
--trungvt update 23/10/2021: Update code to upper and merge update CheckStatusId, CensorStatusId to update ty_gia

DECLARE @so_ct NVARCHAR(128)
DECLARE @BillId BIGINT
SELECT TOP 1 @so_ct = so_ct FROM @Rows
--DECLARE @LastestVersion INT
--SELECT @BillId = InputBill_F_Id FROM @Rows

--SELECT LastestVersion = LatestBillVersion FROM InputBill WHERE BillCode = @so_ct 

-- UPDATE InputValueRow 
--     SET 
--         CheckStatusId = CASE WHEN CheckStatusId>0 THEN 0 ELSE CheckStatusId END, 
--         CensorStatusId = CASE WHEN CensorStatusId>0 THEN 0 ELSE CensorStatusId END,

--     WHERE so_ct = @so_ct AND IsDeleted = 0


--SQL action
--luanpt update 28/01/2022: Update logic set CheckStatusId, CensorStatusId when create or update bill
-- Kiểm tra xem chứng từ có cấu hình duyệt hay không

DECLARE @ACTION_ADD int = 2;
DECLARE @ACTION_UPDATE int = 4;

IF (@Action = @ACTION_ADD OR @Action = @ACTION_UPDATE)
BEGIN 

    DECLARE @WAITING bigint = 0;
    DECLARE @APPROVE bigint = 1;
    DECLARE @REJECT bigint = 2;

    DECLARE @hasCensorConfig BIT = 0;
    DECLARE @hasCheckConfig BIT = 0;

    --SELECT @hasCensorConfig = 1
    --    FROM InputAreaField af 
    --    INNER JOIN InputField f ON af.InputFieldId = f.InputFieldId AND f.IsDeleted = 0
    --    WHERE af.InputTypeId = @InputTypeId AND af.IsDeleted = 0 AND f.FieldName = ''CensorStatusId'';

    --SELECT @hasCheckConfig = 1
    --    FROM InputAreaField af 
    --    INNER JOIN InputField f ON af.InputFieldId = f.InputFieldId AND f.IsDeleted = 0
    --    WHERE af.InputTypeId = @InputTypeId AND af.IsDeleted = 0 AND f.FieldName = ''CheckStatusId'';

--SQL action
--tuannm update 29/03/2022: Update logic set CheckStatusId, CensorStatusId follow ObjectApprovalStep
	DECLARE @ApprovalStepTypeId int = 1;
    DECLARE @CheckStepTypeId int = 2;

    DECLARE @OBJECT_TYPE_INPUT_TYPE INT = @InputTypeObjectTypeId;--34

	SELECT @hasCensorConfig = 1
        FROM dbo.RefObjectApprovalStep r 
        WHERE r.ObjectTypeId = @OBJECT_TYPE_INPUT_TYPE AND r.ObjectId = @InputTypeId AND r.ObjectApprovalStepTypeId = @ApprovalStepTypeId AND r.IsEnable = 1 AND r.SubsidiaryId = @SubId

    SELECT @hasCheckConfig = 1
        FROM dbo.RefObjectApprovalStep r 
        WHERE r.ObjectTypeId = @OBJECT_TYPE_INPUT_TYPE AND r.ObjectId = @InputTypeId AND r.ObjectApprovalStepTypeId = @CheckStepTypeId AND r.IsEnable = 1 AND r.SubsidiaryId = @SubId

    UPDATE r 
        SET 
            ty_gia = CASE WHEN c.IsPrimary = 1 THEN 1 ELSE r.ty_gia END,
            CheckStatusId = CASE WHEN @hasCheckConfig = 1 THEN @WAITING ELSE @APPROVE END, 
            CensorStatusId = CASE WHEN @hasCensorConfig = 1 THEN @WAITING ELSE @APPROVE END,
            po_code = UPPER(po_code),
            order_code = UPPER(order_code),
            ma_lsx = UPPER(ma_lsx)
        FROM dbo.InputValueRow r
        LEFT JOIN dbo.v_Currency c ON r.loai_tien = c.F_Id
        WHERE r.so_ct = @so_ct AND r.IsDeleted = 0

    --split sourceBillCodes to code
    DELETE s
    FROM dbo.InputValueRow r
        JOIN dbo.InputValueRowSourceBillCode s ON r.F_Id = s.InputValueRow_F_Id
    WHERE r.so_ct = @so_ct AND r.IsDeleted = 0;

    INSERT INTO dbo.InputValueRowSourceBillCode
    (
        InputValueRow_F_Id,
        SourceBillCode
    )
    SELECT
        r.F_Id,
        c.[value]
    FROM dbo.InputValueRow r
        OUTER APPLY(
            SELECT LTRIM(RTRIM([value])) [value] FROM dbo.ufn_Split(CASE WHEN LEN(r.sourceBillCodes)>0 THEN r.sourceBillCodes ELSE r.so_ct END,'','')
        ) c
    WHERE r.so_ct = @so_ct AND r.IsDeleted = 0 AND LEN([c].[value])>0

END' WHERE [InputTypeGlobalSettingId] = 1

PRINT(N'Update row in [dbo].[InputField]')
UPDATE [dbo].[InputField] SET [DataSize]=-1, [RefTableTitle]=N'ExpenseItemCode,ExpenseItemName', [UpdatedByUserId]=170, [UpdatedDatetimeUtc]='2023-08-09 04:21:52.3140214', [Structure]=N'', [OnChange]=N'$currentRow.khoan_muc_cp_ExpenseItemName.value = $currentRow.khoan_muc_cp.value ? $currentRow.khoan_muc_cp.refObject.ExpenseItemName : '''';' WHERE [InputFieldId] = 62

PRINT(N'Add rows to [dbo].[InputField]')
SET IDENTITY_INSERT [dbo].[InputField] ON
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (163, N'chi_phi_mua_hang', N'Chi phí', NULL, 0, 1, 2048, 0, 8, NULL, NULL, NULL, NULL, 170, '2023-08-03 05:08:24.1445822', 170, '2023-08-03 07:09:45.3502560', 1, '2023-08-04 03:00:49.6908823', N'{"ControlTitle":"","Areas":[{"AreaTitle":"Danh sách chi phí","AreaCode":"chi_tiet","IsMultiRow":true,"Columns":1,"SortOrder":0,"Fields":[{"FieldTitle":"STT","FieldCode":"stt","Column":1,"SortOrder":1,"FormTypeId":1,"DataTypeId":2,"RefTableCode":null,"RefTableField":null,"RefTableTitle":null,"IsRequired":false},{"FieldTitle":"Mã chi phí","FieldCode":"chi_phi","Column":1,"SortOrder":2,"FormTypeId":2,"DataTypeId":2,"RefTableCode":"_ExpenseItem","RefTableField":"F_Id","RefTableTitle":"ExpenseItemCode,ExpenseItemName","IsRequired":true},{"FieldTitle":"Tên chi phí","FieldCode":"chi_phi_ExpenseItemName","Column":1,"SortOrder":3,"FormTypeId":6,"DataTypeId":1,"RefTableCode":null,"RefTableField":null,"RefTableTitle":null,"IsRequired":false},{"FieldTitle":"Tiêu thức phân bổ","FieldCode":"tieu_thuc","Column":1,"SortOrder":4,"FormTypeId":1,"DataTypeId":2,"RefTableCode":null,"RefTableField":null,"RefTableTitle":null,"IsRequired":true},{"FieldTitle":"Ghi chú","FieldCode":"note","Column":1,"SortOrder":5,"FormTypeId":1,"DataTypeId":1,"RefTableCode":null,"RefTableField":null,"RefTableTitle":null,"IsRequired":false},{"FieldTitle":"Số tiền","FieldCode":"total_money","Column":1,"SortOrder":6,"FormTypeId":1,"DataTypeId":9,"RefTableCode":null,"RefTableField":null,"RefTableTitle":null,"IsRequired":true}]}],"Buttons":[{"Title":null,"ButtonCode":null,"SortOrder":0,"JsAction":null,"IconName":null}]}', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (164, N'tieu_thuc_pb', N'Tiêu thức phân bổ', NULL, 3, 2, -1, 0, 2, NULL, N'_TC_PB', N'Value', N'Title', 170, '2023-08-04 03:02:35.2375848', 170, '2023-08-04 03:02:35.2378152', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (165, N'khoan_muc_cp_ExpenseItemName', N'Tên khoản mục cp', N'Tên khoản mục cp', 4, 1, 128, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 03:04:30.9295281', 170, '2023-08-04 03:04:30.9295294', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (166, N'so_tien', N'Số tiền', N'Số tiền', 0, 9, 18, 0, 1, NULL, NULL, NULL, NULL, 170, '2023-08-04 03:07:56.2352772', 170, '2023-08-04 03:07:56.2352777', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (167, N'ref_row', N'PB_Chi tiết chứng từ', NULL, 0, 8, -1, 0, 4, NULL, N'_InputPublic_Row', N'F_Id', N'F_Id,so_ct,ngay_ct,seri_hd,vthhtp_UnitName,so_luong,so_luong_dv2,vthhtp_SumEstimatePrice,vthhtp_SumMeasurement,vthhtp_SumNetWeight,thanh_tien,BillTypeId,BillId', 170, '2023-08-04 08:31:05.6446350', 170, '2023-08-08 11:32:30.8940239', 0, NULL, N'', 1, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (168, N'ref_row_so_ct', N'PB_Số CT', NULL, 0, 1, 128, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:33:04.3998640', 170, '2023-08-08 12:46:28.1355508', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, N'if(!$currentRow?.ref_row_BillTypeId) return null;
if ($this.isPublicAccounting) {
    return ''/accountant-2/bill/'' + $currentRow.ref_row_BillTypeId.value + ''/view/'' + $currentRow.ref_row_BillId.value;
}else{
    return ''/accountant/bill/'' + $currentRow.ref_row_BillTypeId.value + ''/view/'' + $currentRow.ref_row_BillId.value;
}', NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (169, N'ref_row_ngay_ct', N'PB_Ngày CT', NULL, 0, 3, 0, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:34:06.5002881', 170, '2023-08-04 08:34:06.5002893', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (170, N'ref_row_stt', N'PB_Số TT', NULL, 0, 2, 0, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:34:52.9040078', 170, '2023-08-04 08:34:52.9040082', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (171, N'ref_row_seri_hd', N'PB_Số HĐ', NULL, 0, 2, 128, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:35:36.1127065', 170, '2023-08-04 08:35:36.1127071', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (172, N'ref_row_vthhtp_ProductCode', N'PB_Mã mặt hàng', NULL, 0, 1, 128, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:37:18.7030789', 170, '2023-08-04 08:37:18.7030795', 1, '2023-08-04 08:37:49.2265688', N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (173, N'ref_row_so_luong', N'PB_Số lượng', NULL, 0, 9, 32, 12, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:41:17.1887839', 170, '2023-08-04 08:41:17.1887846', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (174, N'ref_row_so_luong_dv2', N'PB_Số lượng ĐVCĐ', NULL, 0, 9, 32, 12, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:42:01.6433558', 170, '2023-08-04 08:42:01.6433568', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (175, N'sumMeasurement', N'Tổng thể tích (m3)', N'Tổng thể tích (m3)', 0, 9, 32, 12, 1, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:47:39.5118659', 170, '2023-08-04 08:47:39.5118663', 0, NULL, N'', 1, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (176, N'sumNetWeight', N'Tổng trọng lượng (g)', N'Tổng trọng lượng (g)', 0, 9, 32, 12, 1, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:48:32.6441297', 170, '2023-08-04 08:48:32.6441301', 0, NULL, N'', 1, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (177, N'ref_row_thanh_tien', N'PB_Thành tiền', NULL, 0, 9, 18, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 08:50:35.9612225', 170, '2023-08-04 08:50:35.9612230', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (178, N'khoan_muc_cp_ExpenseItemName', N'Tên khoản mục CP', NULL, 0, 1, 128, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-04 09:01:08.5857526', 170, '2023-08-04 09:01:08.5857530', 1, '2023-08-04 09:03:33.3821027', N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (179, N'khoan_muc_cp_ExpenseItemCode', N'Mã khoản mục chi phí', NULL, 0, 1, 128, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-07 04:13:20.3549853', 170, '2023-08-07 04:13:20.3551691', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (181, N'parent_so_ct', N'Chứng từ cha', NULL, 0, 1, 128, 0, 9, NULL, NULL, NULL, NULL, 170, '2023-08-08 09:57:41.0785320', 170, '2023-08-08 12:47:45.4622138', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, N'if(!$currentRow?.parent_BillTypeId?.value) return null;
if ($this.isPublicAccounting) {
    return ''/accountant-2/bill/'' + $currentRow.parent_BillTypeId.value + ''/view/'' + $currentRow.parent_Bill_F_Id.value;
}else{
    return ''/accountant/bill/'' + $currentRow.parent_BillTypeId.value + ''/view/'' + $currentRow.parent_Bill_F_Id.value;
}', NULL, NULL, NULL, NULL, NULL, NULL, N'parent.BillCode')
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (182, N'parent_Bill_F_Id', N'ID chứng từ cha', NULL, 0, 8, 0, 0, 9, NULL, NULL, NULL, NULL, 170, '2023-08-08 10:22:09.1376177', 170, '2023-08-08 10:22:09.1379048', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'parent.F_Id')
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (183, N'parent_BillTypeId', N'ID loại chứng từ cha', NULL, 0, 2, 0, 0, 9, NULL, NULL, NULL, NULL, 170, '2023-08-08 10:28:25.9037021', 170, '2023-08-08 10:28:25.9037025', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'parent.InputTypeId')
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (184, N'ref_row_BillTypeId', N'PB_ID loại chứng từ dữ liệu', NULL, 0, 2, 0, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-08 11:21:38.4975913', 170, '2023-08-08 11:21:38.4977806', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (185, N'ref_row_BillId', N'PB_ID chứng từ dữ liệu', NULL, 0, 8, 0, 0, 6, NULL, NULL, NULL, NULL, 170, '2023-08-08 11:22:42.0456566', 170, '2023-08-08 11:22:42.0456571', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (186, N'chi_phi', N'Chi phí', NULL, 0, 1, 4000, 0, 8, NULL, NULL, NULL, NULL, 170, '2023-08-09 05:35:47.7576364', 170, '2023-08-10 04:39:09.9954602', 0, NULL, N'{"ControlTitle":"","Areas":[{"AreaTitle":"","AreaCode":"info","IsMultiRow":true,"Columns":2,"SortOrder":0,"Fields":[{"FieldTitle":"STT","FieldCode":"stt","Column":1,"SortOrder":1,"FormTypeId":1,"DataTypeId":2,"RefTableCode":null,"RefTableField":null,"RefTableTitle":null,"IsRequired":true},{"FieldTitle":"Khoản mục","FieldCode":"khoan_muc_cp","Column":1,"SortOrder":2,"FormTypeId":4,"DataTypeId":2,"RefTableCode":"_ExpenseItem","RefTableField":"F_Id","RefTableTitle":"ExpenseItemCode,ExpenseItemName","IsRequired":true},{"FieldTitle":"Tiêu chí","FieldCode":"tieu_thuc_pb","Column":1,"SortOrder":3,"FormTypeId":2,"DataTypeId":2,"RefTableCode":"_TC_PB","RefTableField":"Value","RefTableTitle":"Title","IsRequired":true},{"FieldTitle":"Số tiền","FieldCode":"so_tien","Column":1,"SortOrder":4,"FormTypeId":1,"DataTypeId":9,"RefTableCode":null,"RefTableField":null,"RefTableTitle":null,"IsRequired":true},{"FieldTitle":"Ghi chú","FieldCode":"note","Column":1,"SortOrder":5,"FormTypeId":1,"DataTypeId":1,"RefTableCode":null,"RefTableField":null,"RefTableTitle":null,"IsRequired":true}]}],"Buttons":[{"Title":"Cập nhật","ButtonCode":null,"SortOrder":0,"JsAction":"return true","IconName":null}]}', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (187, N'chi_phi_tkno', N'CP_Tk_nợ', NULL, 0, 1, 512, 0, 4, NULL, N'_AccountingAccount', N'AccountNumber', N'AccountNumber', 170, '2023-08-09 11:26:45.0304726', 170, '2023-08-09 11:26:45.0310724', 1, '2023-08-09 11:28:11.3520831', N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (188, N'tai_khoan_no', N'CP_Tài khoản nợ', NULL, 0, 1, 512, 0, 4, NULL, N'_AccountingAccount', N'AccountNumber', N'AccountNumber', 170, '2023-08-09 11:27:57.3143491', 170, '2023-08-09 11:27:57.3143508', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (189, N'tien_thue', N'CP_Tiền thuế', NULL, 0, 9, 18, 0, 1, NULL, NULL, NULL, NULL, 170, '2023-08-09 11:29:03.7263435', 170, '2023-08-09 11:29:03.7263453', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (192, N'HasChildren', N'Đã tạo chứng từ phân bổ', NULL, 0, 6, 0, 0, 9, NULL, NULL, NULL, NULL, 170, '2023-08-15 04:00:07.8601161', 170, '2023-08-15 04:00:07.8604446', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'vInputBill.HasChildren')
INSERT INTO [dbo].[InputField] ([InputFieldId], [FieldName], [Title], [Placeholder], [SortOrder], [DataTypeId], [DataSize], [DecimalPlace], [FormTypeId], [DefaultValue], [RefTableCode], [RefTableField], [RefTableTitle], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [Structure], [IsReadOnly], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [OnClick], [ReferenceUrl], [IsImage], [MouseEnter], [MouseLeave], [CustomButtonHtml], [CustomButtonOnClick], [ObjectApprovalStepTypeId], [SqlValue]) VALUES (1184, N'sourceBillCodes', N'Chứng từ nguồn dữ liệu', NULL, 0, 1, 512, 0, 1, NULL, NULL, NULL, NULL, 170, '2023-08-22 07:21:17.3993036', 170, '2023-08-22 07:21:17.3998855', 0, NULL, N'', 0, NULL, NULL, NULL, N'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
SET IDENTITY_INSERT [dbo].[InputField] OFF
PRINT(N'Operation applied to 28 rows out of 28')

PRINT(N'Add rows to [dbo].[InputType]')
SET IDENTITY_INSERT [dbo].[InputType] ON
INSERT INTO [dbo].[InputType] ([InputTypeId], [InputTypeGroupId], [Title], [InputTypeCode], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [PreLoadAction], [PostLoadAction], [AfterLoadAction], [BeforeSubmitAction], [BeforeSaveAction], [AfterSaveAction], [AfterUpdateRowsJsAction], [IsOpenning], [IsHide], [IsParentAllowcation], [DataAllowcationInputTypeIds], [ResultAllowcationInputTypeId], [CalcResultAllowcationSqlQuery]) VALUES (1128, 5, N'Phiếu nhập chi phí mua hàng', N'CHI_PHI_MUA_HANG', 12, 170, '2023-08-03 04:56:46.9478670', 170, '2023-08-09 12:05:07.1274217', 0, NULL, NULL, NULL, N'calcPriceTotalAndTaxRowAndBill($data);', NULL, NULL, NULL, NULL, 0, 0, 1, N'[11,59,1123,70,86,71]', 1129, N'DECLARE @total TABLE(
	Measurement DECIMAL(32,12),
	NetWeight DECIMAL(32,12),
	SoLuong DECIMAL(32,12),
	Vnd DECIMAL(32,12)
)

DECLARE @billRow TABLE
(
	F_Id BIGINT,
	so_ct NVARCHAR(128),
	ngay_ct DATETIME2,
	seri_hd NVARCHAR(128),
	BIllId BIGINT,
	BillTypeId BIGINT,
	kh0 NVARCHAR(128),
	kh0_PartnerCode NVARCHAR(128),
	stt INT,	
	vthhtp INT,
	vthhtp_ProductCode NVARCHAR(128),
	vthhtp_ProductName NVARCHAR(128),
	vthhtp_UnitId INT,
	vthhtp_UnitName NVARCHAR(128),
	vthhtp_EstimatePrice DECIMAL(32,12),
	vthhtp_Measurement DECIMAL(32,12),
	vthhtp_NetWeight DECIMAL(32,12),
	vthhtp_dvt2 BIGINT,
	vthhtp_dvt2_ProductUnitConversionName NVARCHAR(128),
	so_luong DECIMAL(32,12),
	so_luong_dv2 DECIMAL(32,12),

	vthhtp_SumEstimatePrice DECIMAL(32,12),
	vthhtp_SumMeasurement DECIMAL(32,12),
	vthhtp_SumNetWeight DECIMAL(32,12),
	thanh_tien DECIMAL(32,12),
	tk_no0 NVARCHAR(128),
	po_code NVARCHAR(128),
	ma_lsx NVARCHAR(128),
	order_code NVARCHAR(128),
	RateMeasurement DECIMAL(32,12),
	RateNetWeight DECIMAL(32,12),
	RateSoLuong DECIMAL(32,12),
	RateVnd DECIMAL(32,12)
)



    INSERT @total
	(
		Measurement,
		NetWeight,
		SoLuong,
		Vnd
	)

    SELECT SUM(r.vthhtp_SumMeasurement), SUM(r.vthhtp_SumNetWeight), SUM(r.so_luong), SUM(r.thanh_tien)
	FROM v_InputPublic_Row r
		JOIN dbo.InputBillAllocation al ON r.so_ct = al.DataAllowcation_BillCode
	WHERE al.Parent_InputBill_F_Id = @ParentFId;


	INSERT INTO @billRow
	(
	    F_Id,
	    so_ct,
	    ngay_ct,
	    seri_hd,
	    BIllId,
	    BillTypeId,
	    kh0,
	    kh0_PartnerCode,
	    stt,
	    vthhtp,
	    vthhtp_ProductCode,
	    vthhtp_ProductName,
	    vthhtp_UnitId,
	    vthhtp_UnitName,
	    vthhtp_EstimatePrice,
	    vthhtp_Measurement,
	    vthhtp_NetWeight,
	    vthhtp_dvt2,
	    vthhtp_dvt2_ProductUnitConversionName,
	    so_luong,
	    so_luong_dv2,
	    vthhtp_SumEstimatePrice,
	    vthhtp_SumMeasurement,
	    vthhtp_SumNetWeight,
	    thanh_tien,
	    tk_no0,
	    po_code,
	    ma_lsx,
	    order_code,
	    RateMeasurement,
	    RateNetWeight,
	    RateSoLuong,
	    RateVnd
	)
	
	
	SELECT
		r.F_Id,
	    r.so_ct,
	    r.ngay_ct,
	    r.seri_hd,
	    r.BillId,
	    r.BillTypeId,
	    r.kh0,
	    r.kh0_PartnerCode,
	    r.stt,
	    r.vthhtp,
	    r.vthhtp_ProductCode,
	    r.vthhtp_ProductName,
	    r.vthhtp_UnitId,
	    r.vthhtp_UnitName,
	    r.vthhtp_EstimatePrice,
	    r.vthhtp_Measurement,
	    r.vthhtp_NetWeight,
	    r.vthhtp_dvt2,
	    r.vthhtp_dvt2_ProductUnitConversionName,
	    r.so_luong,
	    r.so_luong_dv2,
	    r.vthhtp_SumEstimatePrice,
	    r.vthhtp_SumMeasurement,
	    r.vthhtp_SumNetWeight,
	    r.thanh_tien,
	    r.tk_no0,
	    r.po_code,
	    r.ma_lsx,
	    r.order_code,
		r.vthhtp_SumMeasurement*1.0 / total.Measurement RateMeasurement,
		r.vthhtp_SumNetWeight*1.0 / total.NetWeight RateNetWeight,
		r.so_luong*1.0 / total.SoLuong RateSoLuong,
		r.thanh_tien*1.0 / total.Vnd RateVnd

	FROM v_InputPublic_Row r
		JOIN dbo.InputBillAllocation al ON r.so_ct = al.DataAllowcation_BillCode
		CROSS JOIN @total total
	WHERE al.Parent_InputBill_F_Id = @ParentFId

DROP TABLE IF EXISTS #result
SELECT 
	ROW_NUMBER() OVER(PARTITION BY cp.F_Id ORDER BY r.thanh_tien DESC, r.F_Id) RowNumber,
	pb.so_ct,
	pb.UpdatedDatetimeUtc,
	ISNULL(pb.ngay_ct, cp.ngay_ct) ngay_ct,
	cp.ong_ba,
	cp.kh0,
	pn.PartnerCode kh0_PartnerCode,
	pn.PartnerName kh0_PartnerName,
	cp.dia_chi,
	cp.noi_dung,
	cp.mau_hd,
	cp.ky_hieu_hd,
	cp.seri_hd,
	cp.ngay_hd,
	cp.ma_link_hd,
	cp.attachment,
	r.F_Id ref_row,
	r.so_ct ref_row_so_ct,
	r.ngay_ct ref_row_ngay_ct,
	r.seri_hd ref_row_seri_hd,
	--r.kh0,
	--r.kh0_PartnerCode,
	r.stt ref_row_stt,
	r.vthhtp,
	r.vthhtp_ProductCode,
	r.vthhtp_ProductName,
	r.vthhtp_UnitName,
	r.so_luong ref_row_so_luong,
	r.vthhtp_dvt2,
	r.vthhtp_dvt2_ProductUnitConversionName,
	r.so_luong_dv2 ref_row_so_luong_dv2,
	r.thanh_tien ref_row_thanh_tien,
	r.vthhtp_SumMeasurement sumMeasurement,
	r.vthhtp_SumNetWeight sumNetWeight,
	cp.khoan_muc_cp,
	kmcp.ExpenseItemCode khoan_muc_cp_ExpenseItemCode,
	kmcp.ExpenseItemName khoan_muc_cp_ExpenseItemName,
	cp.so_tien,
	CASE cp.tieu_thuc_pb 
		WHEN 1 THEN cp.so_tien * r.RateSoLuong
		WHEN 2 THEN cp.so_tien * r.RateMeasurement
		WHEN 3 THEN cp.so_tien * r.RateNetWeight
		WHEN 4 THEN cp.so_tien * r.RateVnd
		ELSE NULL
	END vnd0,
	cp.tk_co1 tk_co0,
	r.tk_no0,	
	r.po_code,
	r.order_code,
	r.ma_lsx,

	cp.tk_co1 tk_co1,
	cp.tai_khoan_no tk_no1,
	cp.tien_thue vnd1,
	cp.thue_suat_vat
INTO #result
FROM @billRow r
CROSS JOIN dbo.InputValueRow cp
LEFT JOIN dbo.v_ExpenseItem kmcp ON cp.khoan_muc_cp = kmcp.F_Id
LEFT JOIN dbo.v_Partner pn ON cp.kh0 = pn.F_Id
OUTER APPLY(
	SELECT TOP(1) 
			pb.so_ct,
			pb.ngay_ct,
			pb.UpdatedDatetimeUtc
		FROM dbo.InputValueRow pb
		JOIN dbo.InputBill b ON b.F_Id = pb.InputBill_F_Id 
	WHERE pb.IsDeleted = 0 AND b.IsDeleted = 0 AND b.ParentInputBill_F_Id = @ParentFId
) pb
--OUTER APPLY(
--	SELECT TOP(1) 
--			cp_info.tk_co1
--		FROM dbo.InputValueRow cp_info		
--	WHERE cp_info.IsDeleted = 0 AND cp_info.InputBill_F_Id = @ParentFId --AND cp_info.IsBillEntry = 1
--) cp_info
WHERE cp.InputBill_F_Id = @ParentFId AND cp.IsDeleted = 0 AND cp.IsBillEntry = 0

UPDATE #result
SET vnd0 = so_tien - (SELECT SUM(vnd0) FROM #result WHERE RowNumber <> 1)
WHERE RowNumber = 1

SELECT * FROM #result



DROP TABLE IF EXISTS #result')
INSERT INTO [dbo].[InputType] ([InputTypeId], [InputTypeGroupId], [Title], [InputTypeCode], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [PreLoadAction], [PostLoadAction], [AfterLoadAction], [BeforeSubmitAction], [BeforeSaveAction], [AfterSaveAction], [AfterUpdateRowsJsAction], [IsOpenning], [IsHide], [IsParentAllowcation], [DataAllowcationInputTypeIds], [ResultAllowcationInputTypeId], [CalcResultAllowcationSqlQuery]) VALUES (1129, 5, N'Phân bổ chi phí', N'PB_CP', 15, 170, '2023-08-04 04:30:44.3384616', 170, '2023-08-04 08:27:12.8256829', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0, N'null', NULL, NULL)
INSERT INTO [dbo].[InputType] ([InputTypeId], [InputTypeGroupId], [Title], [InputTypeCode], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [PreLoadAction], [PostLoadAction], [AfterLoadAction], [BeforeSubmitAction], [BeforeSaveAction], [AfterSaveAction], [AfterUpdateRowsJsAction], [IsOpenning], [IsHide], [IsParentAllowcation], [DataAllowcationInputTypeIds], [ResultAllowcationInputTypeId], [CalcResultAllowcationSqlQuery]) VALUES (1130, 5, N'Phân bổ chi phí mua hàng (Test - V2)', N'CHI_PHI_MUA_HANG_PHAN_BO', 15, 170, '2023-08-10 04:23:43.9845801', 170, '2023-08-10 04:34:18.5583310', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 1, N'[11,59,1123,70,71,86]', NULL, NULL)
SET IDENTITY_INSERT [dbo].[InputType] OFF
PRINT(N'Operation applied to 3 rows out of 3')

PRINT(N'Add rows to [dbo].[ProgramingFunction]')
SET IDENTITY_INSERT [dbo].[ProgramingFunction] ON
INSERT INTO [dbo].[ProgramingFunction] ([ProgramingFunctionId], [ProgramingFunctionName], [FunctionBody], [ProgramingLangId], [ProgramingLevelId], [Description], [Params]) VALUES (101, N'tinhThamNien', N'DECLARE @NgayTinh DATETIME2 = datefromparts(@nam, @thang, @ngay)


SELECT DATEDIFF(Month, @ngay_bat_dau_lam_viec, @NgayTinh)', 1, 1, N'Tính thâm niên', N'{"ReturnType":"int","ParamsList":[{"Name":"ngay_bat_dau_lam_viec","Type":"DateTime","Description":"Ngày vào làm"},{"Name":"nam","Type":"int","Description":"Năm tính thâm niên"},{"Name":"thang","Type":"int","Description":"Tháng tính thâm niên"},{"Name":"ngay","Type":"int","Description":"Ngày tính thâm niên"}]}')
INSERT INTO [dbo].[ProgramingFunction] ([ProgramingFunctionId], [ProgramingFunctionName], [FunctionBody], [ProgramingLangId], [ProgramingLevelId], [Description], [Params]) VALUES (102, N'thangthamnienG7', N'DECLARE @FromDate DATETIME2 = @ngay_bat_dau_lam_viec
DECLARE @ToDate DATETIME2 = datefromparts(@nam, @thang, @ngay)

DECLARE @DayFrom INT = DATEPART(DAY, @FromDate)
DECLARE @DayTo INT = DATEPART(DAY, @ToDate)
DECLARE @DayEndMonthTo INT = DATEPART(DAY,EOMONTH(@ToDate))

DECLARE @TotalMonths INT =  DATEDIFF(MONTH, @FromDate, @ToDate)
DECLARE @TotalMonths01 INT =  DATEDIFF(DAY, @FromDate, @ToDate)

IF @DayFrom > @DayTo AND @DayTo < @DayEndMonthTo AND (@TotalMonths01%30) != 0
	SET @TotalMonths = @TotalMonths - 1

SELECT @TotalMonths', 1, 1, N'Tháng thâm niên G7', N'{"ReturnType":"int","ParamsList":[{"Name":"ngay_bat_dau_lam_viec","Type":"DateTime","Description":"Ngày vào làm"},{"Name":"nam","Type":"int","Description":"Năm tính thâm niên"},{"Name":"thang","Type":"int","Description":"Tháng tính thâm niên"},{"Name":"ngay","Type":"int","Description":"Ngày tính thâm niên"}]}')
INSERT INTO [dbo].[ProgramingFunction] ([ProgramingFunctionId], [ProgramingFunctionName], [FunctionBody], [ProgramingLangId], [ProgramingLevelId], [Description], [Params]) VALUES (103, N'testT8', N'select 1', 1, 1, N'Hàm xử lý', N'{"ReturnType":"void","ParamsList":[{"Name":"so_ct","Type":"NVarChar (128)","Description":"Mã nhân viên"}]}')
INSERT INTO [dbo].[ProgramingFunction] ([ProgramingFunctionId], [ProgramingFunctionName], [FunctionBody], [ProgramingLangId], [ProgramingLevelId], [Description], [Params]) VALUES (1101, N'validBillEntryDate', N'return new Promise(async (resolve) => {
  let billEntryDate;
  const invoicesDate = [];
  for (const [key, value] of Object.entries($bill)) {
    for(let i =0; i < value.rows.length; i++){
      const currentObj = value.rows[i];
      if(typeof currentObj[''ngay_ct''] !== ''undefined'' && currentObj[''ngay_ct''].value){
        const entryDate = new Date(currentObj[''ngay_ct''].value * 1000);
        if(entryDate instanceof Date){
          billEntryDate = entryDate;
        }
      }
      if(typeof currentObj[''ngay_hd''] !== ''undefined'' && currentObj[''ngay_hd''].value){
        const invoiceDate = new Date(currentObj[''ngay_hd''].value * 1000);
        if(invoiceDate instanceof Date){
          invoicesDate.push(invoiceDate);
        }
      }
      
    }
  }
  let valid = true; 
  if(billEntryDate){
    invoicesDate.forEach(item => {
      if(item.getFullYear() !== billEntryDate.getFullYear() || item.getMonth() !== billEntryDate.getMonth() || billEntryDate.getDate() < item.getDate()){
        valid = false;
      }
    })
  }
  if(!valid){
    const isIgnore = await $this.confirm(''Cảnh báo  sai khác giữa ngày chứng từ và ngày hóa đơn!'', ''Bạn có muốn lưu dữ liệu không?'')
    resolve(isIgnore)
  }else{
    resolve(true)
  }
})', 3, 2, N'Nếu chứng từ có nhập ngày hóa đơn - Nếu  ngày chứng từ < ngày hóa đơn và tháng của ngày chứng từ khác với tháng của hóa đơn ⇒ Khi lưu Gửi cảnh báo  sai khác', N'{"ReturnType":"boolean","ParamsList":[{"Name":"$data","Type":"BillDataContext","Description":"Dữ liệu thực thi hiện hành"}]}')
INSERT INTO [dbo].[ProgramingFunction] ([ProgramingFunctionId], [ProgramingFunctionName], [FunctionBody], [ProgramingLangId], [ProgramingLevelId], [Description], [Params]) VALUES (1102, N'invProcessSourceDataBeForMapping', N'console.log(''arguments'', arguments);
let $sourceBill = $data.$sourceBill;
let $sourceFields = $data.$sourceFields;
let $this1 = $data.$this;
let $outsideMappingModel = $data.$outsideMappingModel;


if (Array.isArray($sourceBill)) {
    let new$sourceBill = {
        ...$sourceBill[0],
        inventoryCodes: ''''
    };

    new$sourceBill[$outsideMappingModel.sourceDetailsPropertyName] = [];
    let details = new$sourceBill[$outsideMappingModel.sourceDetailsPropertyName];

    for (let i = 0; i < $sourceBill.length; i++) {
        let inv = $sourceBill[i];

        if (new$sourceBill.inventoryCodes)
            new$sourceBill.inventoryCodes += '','';

        if (inv.inventoryCode)
            new$sourceBill.inventoryCodes += inv.inventoryCode;

        let generalFields = $outsideMappingModel.fieldMappings
            .filter(f => !f.isDetail);

        for (let j = 0; j < generalFields.length; j++) {
            let f = generalFields[j];

            const fieldName = f.sourceFieldName;
            if (!new$sourceBill[fieldName]) {
                new$sourceBill[fieldName] = inv[fieldName];
            } else {
                if (inv[fieldName]) {
                    if (typeof (inv[fieldName]) == ''object'') {
                        if (Array.isArray(new$sourceBill[fieldName]) || Array.isArray(inv[fieldName])) {
                            if (!new$sourceBill[fieldName]) new$sourceBill[fieldName] = [];

                            inv[fieldName].forEach(v => {
                                if (!new$sourceBill[fieldName].find(e => e == v)) {
                                    new$sourceBill[fieldName].push(v);
                                }
                            })
                        } else {
                            new$sourceBill[fieldName] = { ...new$sourceBill[fieldName], ...inv[fieldName] }
                        }
                    } else {
                        if (new$sourceBill[fieldName] != inv[fieldName] && fieldName != ''inventoryCode'' && fieldName != ''date'') {
                            let msg = ''Tìm thấy nhiều hơn 1 giá trị trường '' + $sourceFields?.find(f => f.value == fieldName)?.title;
                            $this1.toastError(msg);
                            return Promise.reject(msg);
                        }
                    }
                }
            }

        };

        inv[$outsideMappingModel.sourceDetailsPropertyName].forEach(d => {
            let existedItems = details.filter(p => p.productId == d.productId);

            if ($outsideMappingModel.fieldMappings.find(m => m.sourceFieldName == ''productUnitConversionId'')) {
                existedItems = existedItems.filter(p => p.productUnitConversionId == d.productUnitConversionId);
            }

            if ($outsideMappingModel.fieldMappings.find(m => m.sourceFieldName == ''orderCode'')) {
                existedItems = existedItems.filter(p => p.orderCode == d.orderCode);
            }
            if ($outsideMappingModel.fieldMappings.find(m => m.sourceFieldName == ''poCode'')) {
                existedItems = existedItems.filter(p => p.poCode == d.poCode);
            }

            if ($outsideMappingModel.fieldMappings.find(m => m.sourceFieldName == ''productionOrderCode'')) {
                existedItems = existedItems.filter(p => p.productionOrderCode == d.productionOrderCode);
            }

            let existedItem = existedItems.find(p => true);

            if (existedItem) {
                existedItem.primaryQuantity += d.primaryQuantity;
                existedItem.productUnitConversionQuantity += d.productUnitConversionQuantity;
            } else {
                details.push(d);
            }
        })

    }

    $sourceBill = new$sourceBill;
} else {
    $sourceBill.inventoryCodes = $sourceBill.inventoryCode;
}


let $fieldData = {
    $EnumObjectType: $this1.EnumObjectType, $objectTypeId: $outsideMappingModel.objectTypeId, $source: null, $val: null
};

$sourceBill[$outsideMappingModel.sourceDetailsPropertyName].forEach(d => {
    $outsideMappingModel.fieldMappings.forEach(map => {
        $fieldData.$source = map.sourceFieldName;
        if (!map.isDetail) {
            $fieldData.$val = $sourceBill[map.sourceFieldName];
            $sourceBill[map.sourceFieldName] = exeptionalSourceMappingName($fieldData);
        } else {
            $fieldData.$val = d[map.sourceFieldName];
            d[map.sourceFieldName] = exeptionalSourceMappingName($fieldData);
        }
    });
});

return $sourceBill;', 3, 2, N'Tiền xử lý dữ liệu chứng từ kho trước khi map sang chứng từ kế toán', N'{"ReturnType":"void","ParamsList":[{"Name":"$data","Type":"{         $this: $component,         $sourceBill: $sourceBill,         $destinyBill: $destinyBill,         $outsideMappingModel: $outsideMappingModel,     }"}]}')
SET IDENTITY_INSERT [dbo].[ProgramingFunction] OFF
PRINT(N'Operation applied to 5 rows out of 5')

PRINT(N'Add rows to [dbo].[InputArea]')
SET IDENTITY_INSERT [dbo].[InputArea] ON
INSERT INTO [dbo].[InputArea] ([InputAreaId], [InputTypeId], [InputAreaCode], [Title], [Description], [IsMultiRow], [IsAddition], [Columns], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ColumnStyles], [IsGeneratedArea]) VALUES (1278, 1128, N'pnk_chung', N'Thông tin phiếu', N'', 0, 0, 2, 0, 170, '2023-08-03 04:58:32.4153071', 170, '2023-08-06 13:18:01.8653871', 0, NULL, N'[{"class":"","style":"","titleStyle":"","valueStyle":""},{"class":"","style":"","titleStyle":"","valueStyle":""}]', 0)
INSERT INTO [dbo].[InputArea] ([InputAreaId], [InputTypeId], [InputAreaCode], [Title], [Description], [IsMultiRow], [IsAddition], [Columns], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ColumnStyles], [IsGeneratedArea]) VALUES (1279, 1128, N'CHITIETPHIEU', N'Phân bổ chi tiết', N'', 1, 0, 1, 0, 170, '2023-08-03 08:09:14.1817895', 170, '2023-08-03 08:09:23.9523415', 0, NULL, N'[{"class":"","style":"","titleStyle":"","valueStyle":""}]', 0)
INSERT INTO [dbo].[InputArea] ([InputAreaId], [InputTypeId], [InputAreaCode], [Title], [Description], [IsMultiRow], [IsAddition], [Columns], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ColumnStyles], [IsGeneratedArea]) VALUES (1280, 1129, N'pnk_chung', N'Thông tin chung', N'', 0, 0, 2, 0, 170, '2023-08-04 04:31:39.7796085', 170, '2023-08-07 12:06:06.4307301', 0, NULL, N'[{"class":"","style":"","titleStyle":"","valueStyle":""},{"class":"","style":"","titleStyle":"","valueStyle":""}]', 0)
INSERT INTO [dbo].[InputArea] ([InputAreaId], [InputTypeId], [InputAreaCode], [Title], [Description], [IsMultiRow], [IsAddition], [Columns], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ColumnStyles], [IsGeneratedArea]) VALUES (1281, 1129, N'CHITIETPHIEU', N'Chi tiết phân bổ', N'', 1, 0, 1, 0, 170, '2023-08-04 04:31:58.1566606', 170, '2023-08-04 08:54:42.8011166', 0, NULL, N'[{"class":"","style":"","titleStyle":"","valueStyle":""}]', 0)
INSERT INTO [dbo].[InputArea] ([InputAreaId], [InputTypeId], [InputAreaCode], [Title], [Description], [IsMultiRow], [IsAddition], [Columns], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ColumnStyles], [IsGeneratedArea]) VALUES (1282, 1128, N'VAT', N'VAT chi phí', N'', 0, 0, 4, 3, 170, '2023-08-08 07:36:41.9138082', 170, '2023-08-09 11:46:07.6254678', 0, NULL, N'[{"class":"","style":"","titleStyle":"","valueStyle":""},{"class":"","style":"","titleStyle":"","valueStyle":""},{"class":"","style":"","titleStyle":"","valueStyle":""},{"class":"","style":"","titleStyle":"","valueStyle":""}]', 0)
INSERT INTO [dbo].[InputArea] ([InputAreaId], [InputTypeId], [InputAreaCode], [Title], [Description], [IsMultiRow], [IsAddition], [Columns], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ColumnStyles], [IsGeneratedArea]) VALUES (1283, 1129, N'VAT', N'VAT Phân bổ', N'', 0, 0, 4, 3, 170, '2023-08-09 11:22:12.5073020', 170, '2023-08-09 11:46:25.3365771', 0, NULL, N'[{"class":"","style":"","titleStyle":"","valueStyle":""},{"class":"","style":"","titleStyle":"","valueStyle":""},{"class":"","style":"","titleStyle":"","valueStyle":""},{"class":"","style":"","titleStyle":"","valueStyle":""}]', 0)
INSERT INTO [dbo].[InputArea] ([InputAreaId], [InputTypeId], [InputAreaCode], [Title], [Description], [IsMultiRow], [IsAddition], [Columns], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ColumnStyles], [IsGeneratedArea]) VALUES (1284, 1130, N'pnk_chung', N'Thông tin chung', N'', 0, 0, 2, 0, 170, '2023-08-10 04:23:44.0257778', 170, '2023-08-10 04:23:44.0268710', 0, NULL, NULL, 0)
INSERT INTO [dbo].[InputArea] ([InputAreaId], [InputTypeId], [InputAreaCode], [Title], [Description], [IsMultiRow], [IsAddition], [Columns], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ColumnStyles], [IsGeneratedArea]) VALUES (1285, 1130, N'CHITIETPHIEU', N'Chi tiết phân bổ', N'', 1, 0, 1, 0, 170, '2023-08-10 04:23:44.0738334', 170, '2023-08-10 04:23:44.0738359', 0, NULL, NULL, 0)
INSERT INTO [dbo].[InputArea] ([InputAreaId], [InputTypeId], [InputAreaCode], [Title], [Description], [IsMultiRow], [IsAddition], [Columns], [SortOrder], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ColumnStyles], [IsGeneratedArea]) VALUES (1286, 1130, N'VAT', N'VAT Phân bổ', N'', 0, 0, 4, 3, 170, '2023-08-10 04:23:44.2707711', 170, '2023-08-10 04:23:44.2707750', 0, NULL, NULL, 0)
SET IDENTITY_INSERT [dbo].[InputArea] OFF
PRINT(N'Operation applied to 9 rows out of 9')

PRINT(N'Add rows to [dbo].[InputAreaField]')
SET IDENTITY_INSERT [dbo].[InputAreaField] ON
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3664, 53, 1120, 1256, N'Ghi chú', NULL, 13, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 88, '2023-07-28 11:36:31.3926211', 88, '2023-07-28 11:36:31.3926262', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3665, 62, 1120, 1256, N'Khoản mục chi phí', NULL, 12, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 88, '2023-07-28 11:36:31.3930850', 88, '2023-07-28 11:36:31.3930893', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3666, 62, 100, 200, N'Khoản mục chi phí', NULL, 23, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 88, '2023-07-28 11:39:35.0154049', 88, '2023-07-28 11:39:35.0154083', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3667, 63, 100, 200, N'Khoản mục thu chi', NULL, 24, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 88, '2023-07-28 11:39:35.0156847', 88, '2023-07-28 11:39:35.0156860', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3668, 62, 1121, 1259, N'Khoản mục chi phí', NULL, 15, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 88, '2023-07-28 11:41:31.8500779', 88, '2023-07-28 11:41:31.8500822', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3669, 63, 1121, 1259, N'Khoản mục thu chi', NULL, 16, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 88, '2023-07-28 11:41:31.8503573', 88, '2023-07-28 11:41:31.8503586', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3670, 163, 1128, 1278, N'Chi phí', NULL, 1, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-03 05:27:23.5962434', 170, '2023-08-03 05:27:23.5964386', 1, '2023-08-03 08:47:58.6403372', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3671, 11, 1128, 1278, N'Ngày chứng từ', N'Ngay_ct', 2, 0, 1, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-03 08:47:58.6396060', 170, '2023-08-08 08:25:50.0279360', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3672, 133, 1128, 1278, N'Duyệt', NULL, 17, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-03 08:47:58.6397212', 170, '2023-08-03 08:50:07.1082159', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3673, 135, 1128, 1278, N'Kiểm tra', NULL, 16, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-03 08:47:58.6397640', 170, '2023-08-03 08:50:07.1082605', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3674, 162, 1128, 1278, N'Đã tạo CT KT thuế', NULL, 18, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-03 08:47:58.6398057', 170, '2023-08-03 08:50:07.1083279', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3675, 12, 1128, 1278, N'Số chứng từ', N'So_ct', 1, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-03 08:47:58.6398450', 170, '2023-08-03 08:50:07.1077049', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3676, 19, 1128, 1278, N'Mẫu hóa đơn', N'Mẫu hóa đơn', 10, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-03 08:47:58.6398857', 170, '2023-08-03 08:50:07.1077467', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3677, 98, 1128, 1278, N'Ký hiệu hóa đơn', N'Ký hiệu', 11, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-03 08:47:58.6399257', 170, '2023-08-03 08:50:07.1081307', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3678, 25, 1128, 1278, N'Số hóa đơn', N'Seri_hd', 12, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-03 08:47:58.6399735', 170, '2023-08-03 08:50:07.1077902', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3679, 26, 1128, 1278, N'Ngày hóa đơn', N'Ngay_hd', 13, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-03 08:47:58.6400167', 170, '2023-08-03 08:50:07.1078323', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3680, 99, 1128, 1278, N'Mã và link tra cứu HĐ điện tử', NULL, 14, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-03 08:47:58.6400663', 170, '2023-08-03 08:50:07.1081741', 0, NULL, NULL, N'if($currentRow.ma_link_hd && $currentRow.ma_link_hd.value){
    return $currentRow.ma_link_hd.value;
}', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3681, 27, 1128, 1278, N'Đính kèm', NULL, 15, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-03 08:47:58.6401112', 170, '2023-08-03 08:50:07.1078762', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3682, 31, 1128, 1278, N'TK có', N'TK có', 8, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-03 08:47:58.6401506', 170, '2023-08-03 08:50:07.1079184', 1, '2023-08-08 07:35:39.2643223', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3683, 33, 1128, 1278, N'Địa chỉ', NULL, 6, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-03 08:47:58.6401962', 170, '2023-08-03 08:50:07.1079613', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3684, 35, 1128, 1278, N'Mã khách', N'Mã khách', 4, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', N'/** merged*/
$currentRow.kh0_PartnerName.value = $currentRow.kh0.value ? $currentRow.kh0.refObject.PartnerName : '''';
if($currentRow.dia_chi){
    $currentRow.dia_chi.value = $currentRow.kh0.refObject ? $currentRow.kh0.refObject.Address : null;
}', NULL, 1, 170, '2023-08-03 08:47:58.6402396', 170, '2023-08-03 08:50:07.1080029', 0, NULL, NULL, N'var kh0Value = $currentRow.kh0;
if ($currentRow.kh0 && typeof($currentRow.kh0) == ''object'') {
    kh0Value = $currentRow.kh0.value;
}
if (kh0Value) {
    if (kh0Value.indexOf(''KH'') == 0) {
        const id = kh0Value.substring(2);
        return ''/system/customer/edit/'' + id + ''?viewmode='';
    }

    if (kh0Value.indexOf(''NV'') == 0) {
        const id = kh0Value.substring(2);
        return ''/system/users/edit/'' + id + ''?viewmode='';
    }

}', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3685, 37, 1128, 1278, N'Tên Khách', N'Tên Khách', 5, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-03 08:47:58.6402919', 170, '2023-08-03 08:50:07.1080449', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3686, 42, 1128, 1278, N'Nội dung', N'Nội dung', 7, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-03 08:47:58.6403358', 170, '2023-08-03 08:50:07.1080886', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3687, 32, 1128, 1278, N'Ông/Bà', NULL, 3, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-03 08:50:07.1076159', 170, '2023-08-03 08:50:07.1076166', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3688, 62, 1128, 1279, N'Khoản mục chi phí', NULL, 2, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 03:08:17.7514523', 170, '2023-08-06 13:46:39.9329126', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3689, 41, 1128, 1279, N'STT', NULL, 1, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, N'', N'', N'', N'/** merged*/

for (const [areaKey, area] of Object.entries($bill)) {
    if (area.rows.indexOf($currentRow) >= 0) {
        let arr = area.rows;
        let inputIndex = $currentRow.stt.value;
        let replaceRow = arr[inputIndex - 1];
        let length = arr.length;

        if (replaceRow && inputIndex <= $index) {
            replaceRow.stt.value++;
        }

        if (replaceRow && inputIndex > $index) {
             replaceRow.stt.value--;
        }
        // bubble sort
        for (let i = 0; i < length; i++) {
            for (let j = 0; j < length - i - 1; j++) {
                if (arr[j].stt.value > arr[j + 1].stt.value) {
                    let temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                }
            }
        }

    }
}

reCalcStt($bill);', NULL, 1, 170, '2023-08-04 03:08:17.7514947', 170, '2023-08-04 03:08:17.7514949', 0, NULL, NULL, NULL, 0, NULL, N'<a class="sort-table-up" title="Đẩy lên trên" ><i class="fa fa-arrow-circle-up"></i></a>', N'for (const [areaKey, area] of Object.entries($bill)) {
    if (area.rows.indexOf($currentRow) >= 0) {
        let arr = area.rows;
        $currentRow.stt.value = 0;

        let length = arr.length;

        // bubble sort
        for (let i = 0; i < length; i++) {
            for (let j = 0; j < length - i - 1; j++) {
                if (arr[j].stt.value > arr[j + 1].stt.value) {
                    let temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                }
            }
        }

    }
}

reCalcStt($bill);', N'', N'', NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3690, 164, 1128, 1279, N'Tiêu thức phân bổ', NULL, 4, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 03:08:17.7515363', 170, '2023-08-04 11:33:38.5048030', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3691, 165, 1128, 1279, N'Tên khoản mục cp', N'Tên khoản mục cp', 3, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 03:08:17.7515768', 170, '2023-08-04 11:33:38.5048484', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3692, 53, 1128, 1279, N'Ghi chú', NULL, 6, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 03:08:17.7516189', 170, '2023-08-04 11:33:38.5047583', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3693, 166, 1128, 1279, N'Số tiền', N'Số tiền', 5, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', N'CalcSumAllRowByRow($data);
calcSumRowWithTax($data);', NULL, 1, 170, '2023-08-04 03:08:17.7516608', 170, '2023-08-08 07:53:22.8348094', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3694, 11, 1129, 1280, N'Ngày chứng từ', N'Ngay_ct', 2, 0, 1, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6140289', 170, '2023-08-07 09:02:30.1501919', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3695, 12, 1129, 1280, N'Số chứng từ', N'So_ct', 1, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6140753', 170, '2023-08-04 08:51:48.6140756', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3696, 168, 1129, 1281, N'Số CT', NULL, 1, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6141158', 170, '2023-08-04 08:52:59.9903151', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3697, 169, 1129, 1281, N'Ngày CT', NULL, 2, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6141572', 170, '2023-08-04 08:52:59.9903847', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3698, 170, 1129, 1281, N'Số TT', NULL, 5, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6141977', 170, '2023-08-07 04:32:21.6717033', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3699, 171, 1129, 1281, N'Số HĐ', NULL, 3, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6142386', 170, '2023-08-04 08:52:59.9905756', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3700, 45, 1129, 1281, N'Mã vthhtp', NULL, 6, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6142786', 170, '2023-08-07 04:32:21.6708257', 0, NULL, NULL, N'var vthhtpValue = $currentRow.vthhtp;
if ($currentRow.vthhtp && typeof($currentRow.vthhtp) == ''object'') {
    vthhtpValue = $currentRow.vthhtp.value;
}
if (vthhtpValue) {   
    return ''/system/products/view/'' + vthhtpValue;
}', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3701, 46, 1129, 1281, N'Tên vthhtp', NULL, 7, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6143186', 170, '2023-08-07 04:32:21.6709120', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3702, 70, 1129, 1281, N'ĐVT', NULL, 8, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6143604', 170, '2023-08-07 04:32:21.6714571', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3703, 101, 1129, 1281, N'ĐVT2', NULL, 10, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6144005', 170, '2023-08-07 04:32:21.6715383', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3704, 173, 1129, 1281, N'PB_Số lượng', NULL, 9, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6144428', 170, '2023-08-07 04:32:21.6717880', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3705, 174, 1129, 1281, N'PB_Số lượng ĐVCĐ', NULL, 11, 0, 0, 0, 0, 1, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6144847', 88, '2023-08-10 10:51:53.7843183', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3706, 51, 1129, 1281, N'Chi phí', NULL, 17, 0, 0, 0, 0, 1, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6145289', 170, '2023-08-07 08:05:08.9156538', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 1)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3707, 175, 1129, 1281, N'Tổng thể tích (m3)', N'Tổng thể tích (m3)', 13, 0, 0, 0, 0, 1, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6145702', 88, '2023-08-10 10:51:53.7846164', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3708, 176, 1129, 1281, N'Tổng trọng lượng (g)', N'Tổng trọng lượng (g)', 14, 0, 0, 0, 0, 1, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6146146', 88, '2023-08-10 10:51:53.7849044', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3709, 177, 1129, 1281, N'PB_Thành tiền', NULL, 12, 0, 0, 0, 0, 1, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6146554', 88, '2023-08-10 10:51:53.7851944', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3710, 31, 1129, 1281, N'TK có', N'TK có', 18, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6146993', 170, '2023-08-07 04:32:21.6707438', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3711, 30, 1129, 1281, N'TK nợ', N'TK nợ', 19, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:51:48.6147624', 170, '2023-08-07 04:32:21.6706581', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3712, 64, 1129, 1281, N'Mã PO', NULL, 20, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:53:58.0589216', 170, '2023-08-07 04:32:21.6712023', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3713, 65, 1129, 1281, N'Mã đơn hàng', NULL, 21, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:53:58.0589660', 170, '2023-08-07 04:32:21.6712848', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3714, 66, 1129, 1281, N'Mã lệnh sx', NULL, 22, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:53:58.0590095', 170, '2023-08-07 04:32:21.6713667', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3715, 62, 1129, 1281, N'Khoản mục chi phí', NULL, 15, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 08:57:35.5710865', 170, '2023-08-07 04:32:21.6711204', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3716, 165, 1129, 1281, N'Tên khoản mục cp', N'Tên khoản mục cp', 16, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 0, 170, '2023-08-04 09:01:36.1460036', 170, '2023-08-07 08:15:02.6627124', 0, '2023-08-04 09:02:54.0258005', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3717, 178, 1129, 1281, N'Tên khoản mục CP', NULL, 21, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-04 09:02:54.0257989', 170, '2023-08-04 09:02:54.0257994', 1, '2023-08-04 09:03:20.5702575', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3718, 167, 1129, 1281, N'PB_Chi tiết chứng từ', NULL, 0, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-07 04:01:28.8373536', 170, '2023-08-07 08:05:08.9157610', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3719, 35, 1129, 1280, N'Mã khách', N'Mã khách', 4, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, N'/** merged*/
$currentRow.kh0_PartnerName.value = $currentRow.kh0.value ? $currentRow.kh0.refObject.PartnerName : '''';
if($currentRow.dia_chi){
    $currentRow.dia_chi.value = $currentRow.kh0.refObject ? $currentRow.kh0.refObject.Address : null;
}', NULL, 1, 170, '2023-08-07 04:28:42.4024385', 170, '2023-08-07 12:13:02.3520133', 0, '2023-08-07 12:03:27.9906200', NULL, N'var kh0Value = $currentRow.kh0;
if ($currentRow.kh0 && typeof($currentRow.kh0) == ''object'') {
    kh0Value = $currentRow.kh0.value;
}
if (kh0Value) {
    if (kh0Value.indexOf(''KH'') == 0) {
        const id = kh0Value.substring(2);
        return ''/system/customer/edit/'' + id + ''?viewmode='';
    }

    if (kh0Value.indexOf(''NV'') == 0) {
        const id = kh0Value.substring(2);
        return ''/system/users/edit/'' + id + ''?viewmode='';
    }

}', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3720, 32, 1129, 1280, N'Ông/Bà', NULL, 3, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-07 12:07:08.3099378', 170, '2023-08-07 12:07:08.3099391', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3721, 37, 1129, 1280, N'Tên Khách', N'Tên Khách', 5, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-07 12:07:08.3100971', 170, '2023-08-07 12:07:08.3100980', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3722, 42, 1129, 1280, N'Nội dung', N'Nội dung', 6, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-07 12:07:08.3102458', 170, '2023-08-07 12:07:08.3102466', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3723, 19, 1129, 1280, N'Mẫu hóa đơn', N'Mẫu hóa đơn', 7, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-07 12:07:08.3104004', 170, '2023-08-07 12:13:02.3508582', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3724, 98, 1129, 1280, N'Ký hiệu hóa đơn', N'Ký hiệu', 8, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-07 12:07:08.3105461', 170, '2023-08-07 12:13:02.3522974', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3725, 25, 1129, 1280, N'Số hóa đơn', N'Seri_hd', 9, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-07 12:07:08.3106973', 170, '2023-08-07 12:13:02.3511487', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3726, 26, 1129, 1280, N'Ngày hóa đơn', N'Ngay_hd', 10, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-07 12:07:08.3108421', 170, '2023-08-07 12:13:02.3514392', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3727, 99, 1129, 1280, N'Mã và link tra cứu HĐ điện tử', NULL, 11, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-07 12:07:08.3109908', 170, '2023-08-07 12:13:02.3525845', 0, NULL, NULL, N'if($currentRow.ma_link_hd && $currentRow.ma_link_hd.value){
    return $currentRow.ma_link_hd.value;
}', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3728, 27, 1129, 1280, N'Đính kèm', NULL, 12, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-07 12:07:08.3111352', 170, '2023-08-07 12:13:02.3517263', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3729, 85, 1128, 1278, N'TK có', N'TK có [VAT]', 8, 0, 0, 0, 0, 0, NULL, N'', N'{"condition":"AND","rules":[{"id":"79","field":"F_Id","type":"string","input":"text","operator":5,"value":null,"dataType":2,"fieldName":"F_Id"}],"not":false,"valid":true}', 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-08 07:35:21.9373583', 170, '2023-08-08 07:35:39.2646367', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3730, 84, 1128, 1282, N'TK nợ [VAT]', N'TK nợ [VAT]', 1, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-08 07:37:59.7565989', 170, '2023-08-08 07:41:57.2506728', 1, '2023-08-09 11:29:54.1645351', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3731, 52, 1128, 1282, N'Thuế suất VAT', NULL, 2, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', N'calcSumRowWithTax($data);', NULL, 4, 170, '2023-08-08 07:37:59.7569137', 170, '2023-08-08 07:53:22.8342293', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3732, 73, 1128, 1282, N'Thuế VAT', N'Thuế VAT', 3, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', N'calcSumRowWithTax($data);', NULL, 4, 170, '2023-08-08 07:37:59.7572354', 170, '2023-08-08 07:53:22.8345202', 1, '2023-08-09 11:29:54.1645317', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3733, 80, 1128, 1282, N'Tổng cộng', N'Tổng cộng', 4, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-08 07:37:59.7575361', 170, '2023-08-08 07:41:57.2503737', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3734, 181, 1129, 1280, N'Chứng từ chi phí', NULL, 13, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-08 09:58:12.2189420', 170, '2023-08-08 09:58:12.2193871', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3735, 182, 1129, 1280, N'ID chứng từ cha', NULL, 14, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-08 10:22:36.3594777', 170, '2023-08-08 10:22:36.3596939', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3736, 183, 1129, 1280, N'ID loại chứng từ cha', NULL, 15, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-08 10:28:35.9454229', 170, '2023-08-08 10:28:35.9454253', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3737, 184, 1129, 1281, N'PB_ID loại chứng từ dữ liệu', NULL, 23, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-08 11:24:32.0025599', 170, '2023-08-08 11:24:32.0025610', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3738, 185, 1129, 1281, N'PB_ID chứng từ dữ liệu', NULL, 24, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-08 11:24:32.0026061', 170, '2023-08-08 11:24:32.0026064', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3739, 186, 1128, 1278, N'Chi phí', NULL, 8, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-09 05:38:33.1088956', 170, '2023-08-09 05:40:38.7019054', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3740, 84, 1129, 1283, N'TK nợ [VAT]', N'TK nợ [VAT]', 1, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-09 11:23:10.9513725', 170, '2023-08-09 11:44:59.0069583', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3741, 52, 1129, 1283, N'Thuế suất VAT', NULL, 3, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-09 11:23:10.9517079', 170, '2023-08-09 11:44:59.0060556', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3742, 73, 1129, 1283, N'Thuế VAT', N'Thuế VAT', 4, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-09 11:23:10.9520047', 170, '2023-08-09 11:44:59.0063615', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3743, 80, 1129, 1283, N'Tổng cộng', N'Tổng cộng', 5, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-09 11:23:10.9523059', 170, '2023-08-09 11:44:59.0066622', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3744, 188, 1128, 1282, N'Tài khoản nợ', NULL, 1, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-09 11:29:54.1642118', 170, '2023-08-09 11:33:36.6180095', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3745, 189, 1128, 1282, N'Tiền thuế', NULL, 3, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-09 11:29:54.1645266', 170, '2023-08-09 11:33:36.6183260', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3746, 85, 1129, 1283, N'TK có [VAT]', N'TK có [VAT]', 2, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-09 11:32:12.3057296', 170, '2023-08-09 11:44:59.0072564', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3747, 11, 1130, 1284, N'Ngày chứng từ', N'Ngay_ct', 2, 0, 1, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.0695547', 170, '2023-08-10 04:23:44.0695616', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3748, 12, 1130, 1284, N'Số chứng từ', N'So_ct', 1, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.0698640', 170, '2023-08-10 04:23:44.0698653', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3749, 35, 1130, 1284, N'Mã khách', N'Mã khách', 4, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', N'/** merged*/
$currentRow.kh0_PartnerName.value = $currentRow.kh0.value ? $currentRow.kh0.refObject.PartnerName : '''';
if($currentRow.dia_chi){
    $currentRow.dia_chi.value = $currentRow.kh0.refObject ? $currentRow.kh0.refObject.Address : null;
}', NULL, 1, 170, '2023-08-10 04:23:44.0701609', 170, '2023-08-10 04:23:44.0701622', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3750, 32, 1130, 1284, N'Ông/Bà', NULL, 3, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.0704518', 170, '2023-08-10 04:23:44.0704527', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3751, 37, 1130, 1284, N'Tên Khách', N'Tên Khách', 5, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.0707457', 170, '2023-08-10 04:23:44.0707466', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3752, 42, 1130, 1284, N'Nội dung', N'Nội dung', 6, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.0710336', 170, '2023-08-10 04:23:44.0710345', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3753, 19, 1130, 1284, N'Mẫu hóa đơn', N'Mẫu hóa đơn', 9, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-10 04:23:44.0713395', 170, '2023-08-10 04:31:49.8603436', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3754, 98, 1130, 1284, N'Ký hiệu hóa đơn', N'Ký hiệu', 10, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-10 04:23:44.0716283', 170, '2023-08-10 04:31:49.8614957', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3755, 25, 1130, 1284, N'Số hóa đơn', N'Seri_hd', 11, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-10 04:23:44.0719183', 170, '2023-08-10 04:31:49.8606320', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3756, 26, 1130, 1284, N'Ngày hóa đơn', N'Ngay_hd', 12, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-10 04:23:44.0722054', 170, '2023-08-10 04:31:49.8609220', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3757, 99, 1130, 1284, N'Mã và link tra cứu HĐ điện tử', NULL, 13, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-10 04:23:44.0724954', 170, '2023-08-10 04:31:49.8617807', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3758, 27, 1130, 1284, N'Đính kèm', NULL, 14, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-10 04:23:44.0727829', 170, '2023-08-10 04:31:49.8612070', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3759, 181, 1130, 1284, N'Chứng từ chi phí', NULL, 13, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-10 04:23:44.0730730', 170, '2023-08-10 04:23:44.0730738', 1, '2023-08-10 04:26:24.0632233', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3760, 182, 1130, 1284, N'ID chứng từ cha', NULL, 14, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.0733601', 170, '2023-08-10 04:23:44.0733609', 1, '2023-08-10 04:26:24.0632276', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3761, 183, 1130, 1284, N'ID loại chứng từ cha', NULL, 15, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.0736505', 170, '2023-08-10 04:23:44.0736514', 1, '2023-08-10 04:26:24.0632293', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3762, 168, 1130, 1285, N'Số CT', NULL, 1, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2682346', 170, '2023-08-10 04:23:44.2682358', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3763, 169, 1130, 1285, N'Ngày CT', NULL, 2, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2679424', 170, '2023-08-10 04:23:44.2679436', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3764, 170, 1130, 1285, N'Số TT', NULL, 5, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2676403', 170, '2023-08-10 04:23:44.2676421', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3765, 171, 1130, 1285, N'Số HĐ', NULL, 3, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2672550', 170, '2023-08-10 04:23:44.2672559', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3766, 45, 1130, 1285, N'Mã vthhtp', NULL, 6, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2669637', 170, '2023-08-10 04:23:44.2669650', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3767, 46, 1130, 1285, N'Tên vthhtp', NULL, 7, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2666762', 170, '2023-08-10 04:23:44.2666771', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3768, 70, 1130, 1285, N'ĐVT', NULL, 8, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2663857', 170, '2023-08-10 04:23:44.2663866', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3769, 101, 1130, 1285, N'ĐVT2', NULL, 10, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2660969', 170, '2023-08-10 04:23:44.2660978', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3770, 173, 1130, 1285, N'PB_Số lượng', NULL, 9, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2658052', 170, '2023-08-10 04:23:44.2658060', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3771, 174, 1130, 1285, N'PB_Số lượng ĐVCĐ', NULL, 11, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2655134', 170, '2023-08-10 04:23:44.2655147', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3772, 51, 1130, 1285, N'Chi phí', NULL, 17, 0, 0, 0, 0, 1, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2651909', 170, '2023-08-10 04:23:44.2651917', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3773, 175, 1130, 1285, N'Tổng thể tích (m3)', N'Tổng thể tích (m3)', 13, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2649021', 170, '2023-08-10 04:23:44.2649030', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3774, 176, 1130, 1285, N'Tổng trọng lượng (g)', N'Tổng trọng lượng (g)', 14, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2646091', 170, '2023-08-10 04:23:44.2646099', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3775, 177, 1130, 1285, N'PB_Thành tiền', NULL, 12, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2643160', 170, '2023-08-10 04:23:44.2643173', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3776, 31, 1130, 1284, N'TK có', N'TK có', 7, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 170, '2023-08-10 04:23:44.2640059', 170, '2023-08-10 04:27:54.5203064', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3777, 30, 1130, 1285, N'TK nợ', N'TK nợ', 19, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2637073', 170, '2023-08-10 04:23:44.2637103', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3778, 64, 1130, 1285, N'Mã PO', NULL, 20, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2685238', 170, '2023-08-10 04:23:44.2685246', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3779, 65, 1130, 1285, N'Mã đơn hàng', NULL, 21, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2688151', 170, '2023-08-10 04:23:44.2688164', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3780, 66, 1130, 1285, N'Mã lệnh sx', NULL, 22, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2691043', 170, '2023-08-10 04:23:44.2691051', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3781, 62, 1130, 1285, N'Khoản mục chi phí', NULL, 15, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2693948', 170, '2023-08-10 04:23:44.2693956', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3782, 165, 1130, 1285, N'Tên khoản mục cp', N'Tên khoản mục cp', 16, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2696827', 170, '2023-08-10 04:23:44.2696840', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3783, 167, 1130, 1285, N'PB_Chi tiết chứng từ', NULL, 0, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2699732', 170, '2023-08-10 04:23:44.2699740', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3784, 184, 1130, 1285, N'PB_ID loại chứng từ dữ liệu', NULL, 23, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2702602', 170, '2023-08-10 04:23:44.2702611', 1, '2023-08-10 04:27:54.5208840', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3785, 185, 1130, 1285, N'PB_ID chứng từ dữ liệu', NULL, 24, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:23:44.2705832', 170, '2023-08-10 04:23:44.2705845', 1, '2023-08-10 04:27:54.5208874', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3786, 84, 1130, 1286, N'TK nợ [VAT]', N'TK nợ [VAT]', 1, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-10 04:23:44.5149998', 170, '2023-08-10 04:23:44.5150007', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3787, 52, 1130, 1286, N'Thuế suất VAT', NULL, 3, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-10 04:23:44.5148563', 170, '2023-08-10 04:23:44.5148567', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3788, 73, 1130, 1286, N'Thuế VAT', N'Thuế VAT', 4, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-10 04:23:44.5147098', 170, '2023-08-10 04:23:44.5147102', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3789, 80, 1130, 1286, N'Tổng cộng', N'Tổng cộng', 5, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-10 04:23:44.5145620', 170, '2023-08-10 04:23:44.5145624', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3790, 85, 1130, 1286, N'TK có [VAT]', N'TK có [VAT]', 2, 0, 0, 0, 0, 0, NULL, N'', NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 4, 170, '2023-08-10 04:23:44.5144120', 170, '2023-08-10 04:23:44.5144142', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3791, 186, 1130, 1284, N'Chi phí', NULL, 8, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-10 04:31:49.8600399', 170, '2023-08-10 04:31:49.8600433', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (3792, 192, 1128, 1278, N'Đã phân bổ', NULL, 19, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-15 04:00:51.8130741', 170, '2023-08-15 04:00:51.8134094', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4737, 1184, 11, 15, N'Chứng từ kho', NULL, 21, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:36:36.2632514', 170, '2023-08-22 07:36:36.2636230', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4738, 1184, 59, 70, N'Chứng từ kho', NULL, 19, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:37:17.6166920', 170, '2023-08-22 07:37:17.6166946', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4739, 1184, 1123, 1264, N'Chứng từ kho', NULL, 18, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:37:47.8339806', 170, '2023-08-22 07:37:47.8339836', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4740, 1184, 70, 110, N'Chứng từ kho', NULL, 17, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:38:13.5139679', 170, '2023-08-22 07:38:13.5139705', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4741, 1184, 71, 114, N'Chứng từ kho', NULL, 21, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:38:52.0303892', 170, '2023-08-22 07:38:52.0303926', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4742, 1184, 72, 118, N'Chứng từ kho', NULL, 14, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:39:17.3364590', 170, '2023-08-22 07:39:17.3364607', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4743, 1184, 86, 162, N'Chứng từ kho', NULL, 19, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:39:41.3075240', 170, '2023-08-22 07:39:41.3075257', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4744, 1184, 61, 78, N'Chứng từ kho', NULL, 20, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:40:17.9461908', 170, '2023-08-22 07:40:17.9461929', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4745, 1184, 62, 82, N'Chứng từ kho', NULL, 20, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:40:39.9684273', 170, '2023-08-22 07:40:39.9684299', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4746, 1184, 74, 122, N'Chứng từ kho', NULL, 16, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:41:00.5551346', 170, '2023-08-22 07:41:00.5551372', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4747, 1184, 75, 126, N'Chứng từ kho', NULL, 17, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:41:21.3161322', 170, '2023-08-22 07:41:21.3161344', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4748, 1184, 76, 129, N'Chứng từ kho', NULL, 16, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:41:41.0477834', 170, '2023-08-22 07:41:41.0477855', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4749, 1184, 87, 166, N'Chứng từ kho', NULL, 20, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:42:12.4271773', 170, '2023-08-22 07:42:12.4271799', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4750, 1184, 99, 195, N'Chứng từ kho', NULL, 14, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 2, 170, '2023-08-22 07:42:37.1045923', 170, '2023-08-22 07:42:37.1045953', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4751, 1184, 65, 92, N'Chứng từ kho', NULL, 6, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-23 10:40:23.2242302', 170, '2023-08-23 10:40:23.2248030', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
INSERT INTO [dbo].[InputAreaField] ([InputAreaFieldId], [InputFieldId], [InputTypeId], [InputAreaId], [Title], [Placeholder], [SortOrder], [IsAutoIncrement], [IsRequire], [IsUnique], [IsHidden], [IsCalcSum], [RegularExpression], [DefaultValue], [Filters], [Width], [Height], [TitleStyleJson], [InputStyleJson], [OnFocus], [OnKeydown], [OnKeypress], [OnBlur], [OnChange], [AutoFocus], [Column], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [RequireFilters], [ReferenceUrl], [IsBatchSelect], [OnClick], [CustomButtonHtml], [CustomButtonOnClick], [MouseEnter], [MouseLeave], [FiltersName], [RequireFiltersName], [IsPivotAllowcation], [IsReadOnly], [IsPivotValue]) VALUES (4752, 1184, 88, 170, N'Chứng từ kho', NULL, 6, 0, 0, 0, 0, 0, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL, N'', NULL, NULL, 1, 170, '2023-08-23 10:42:59.6696364', 170, '2023-08-23 10:42:59.6696389', 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, 0)
SET IDENTITY_INSERT [dbo].[InputAreaField] OFF
PRINT(N'Operation applied to 145 rows out of 145')

PRINT(N'Add constraints to [dbo].[InputAreaField]')
ALTER TABLE [dbo].[InputAreaField] WITH CHECK CHECK CONSTRAINT [FK_InputAreaField_InputArea]
ALTER TABLE [dbo].[InputAreaField] WITH CHECK CHECK CONSTRAINT [FK_InputAreaField_InputField]
ALTER TABLE [dbo].[InputAreaField] WITH CHECK CHECK CONSTRAINT [FK_InputAreaField_InputType]

PRINT(N'Add constraints to [dbo].[InputArea]')
ALTER TABLE [dbo].[InputArea] WITH CHECK CHECK CONSTRAINT [FK_InputArea_InputType]

PRINT(N'Add constraints to [dbo].[InputType]')
ALTER TABLE [dbo].[InputType] WITH CHECK CHECK CONSTRAINT [FK_InputType_InputType]
ALTER TABLE [dbo].[InputType] WITH CHECK CHECK CONSTRAINT [FK_InputType_InputTypeGroup]
ALTER TABLE [dbo].[InputTypeView] WITH CHECK CHECK CONSTRAINT [FK_InputTypeView_InputType]
ALTER TABLE [dbo].[InputBill] WITH CHECK CHECK CONSTRAINT [FK_InputValueBill_InputType]
COMMIT TRANSACTION
GO

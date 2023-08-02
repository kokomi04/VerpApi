USE AccountancyDB
GO
/*
Run this script on:

172.16.16.102\STD.AccountancyDB    -  This database will be modified

to synchronize it with:

103.21.149.93.AccountancyDB

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 14.7.8.21163 from Red Gate Software Ltd at 7/4/2023 12:16:56 PM

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

PRINT(N'Update row in [dbo].[ProgramingFunction]')
UPDATE [dbo].[ProgramingFunction] SET [FunctionBody]=N'/**
	updated date: 2023-01-06
	updated by : trungvt
	desc: calc sl_ton for public accountancy
*/

DECLARE @MODULE_INPUT_PUBLIC INT = 8002

IF @moduleId = @MODULE_INPUT_PUBLIC
BEGIN
    SELECT ISNULL(SUM(
                      --CASE 
                        --WHEN d.InRowNumber = 1 THEN
                                        CASE  d.IsDebt
                              WHEN 1 THEN d.so_luong
                              ELSE -d.so_luong 
                            END
                        --ELSE 0
                      --END
                    ),
                0) sl_ton
    FROM (   SELECT d.so_luong,
                    d.IsDebt,
                    ROW_NUMBER() OVER (PARTITION BY d.F_Id ORDER BY d.ngay_ct) InRowNumber
              FROM AccountancyPublicDB.dbo._rc_detail d
              WHERE d.SubsidiaryId = @SubId
                AND d.vthhtp       = @vthhtp
                AND d.Tk LIKE CONCAT(@Tk, ''%'')
                AND d.BUT_TOAN <> 3
                AND d.ngay_ct      <= @Date) d;
END
ELSE
BEGIN
    SELECT ISNULL(SUM(
                      --CASE 
                      --  WHEN d.InRowNumber = 1 THEN
                                        CASE  d.IsDebt
                              WHEN 1 THEN d.so_luong
                              ELSE -d.so_luong 
                            END
                       -- ELSE 0
                      --END
                    ),
                0) sl_ton
    FROM (   SELECT d.so_luong,
                    d.IsDebt,
                    ROW_NUMBER() OVER (PARTITION BY d.F_Id ORDER BY d.ngay_ct) InRowNumber
              FROM dbo._rc_detail d
              WHERE d.SubsidiaryId = @SubId
                AND d.vthhtp       = @vthhtp
                AND d.Tk LIKE CONCAT(@Tk, ''%'')
                AND d.BUT_TOAN <> 3
                AND d.ngay_ct      <= @Date) d;
END

/**
	updated date: 2022-12-09
	updated by : trungvt
	desc: calc sl_ton cause by DIEU_CHUYEN_KHO
*/

/*
SELECT ISNULL(SUM(
                    CASE  d.IsDebt
                                                        WHEN 1 THEN d.so_luong
                                                        ELSE -d.so_luong END
                  ),
              0) sl_ton
  FROM (   SELECT d.so_luong,
                  d.IsDebt,
                  ROW_NUMBER() OVER (PARTITION BY d.F_Id ORDER BY d.ngay_ct) InRowNumber
             FROM dbo._rc_detail d
            WHERE d.SubsidiaryId = @SubId
              AND d.vthhtp       = @vthhtp
              AND d.Tk LIKE CONCAT(@Tk, ''%'')
              AND d.ngay_ct      <= @Date) d;
*/              


/**
	updated date: 2022-03-24
	updated by : trungvt
	desc: calc sl_ton
*/

/*
SELECT ISNULL(SUM(CASE
                       WHEN d.InRowNumber = 1 THEN CASE d.IsDebt
                                                        WHEN 1 THEN d.so_luong
                                                        ELSE -d.so_luong END
                       ELSE 0 END),
              0) sl_ton
  FROM (   SELECT d.so_luong,
                  d.IsDebt,
                  ROW_NUMBER() OVER (PARTITION BY d.F_Id ORDER BY d.ngay_ct) InRowNumber
             FROM dbo._rc_detail d
            WHERE d.SubsidiaryId = @SubId
              AND d.vthhtp       = @vthhtp
              AND d.Tk LIKE CONCAT(@Tk, ''%'')
              AND d.ngay_ct      <= @Date) d;
*/		  
-- SELECT 
-- 	ISNULL(SUM(CASE WHEN _rc.tk_no LIKE CONCAT(@Tk,''%'') THEN ISNULL(_rc.so_luong,0) ELSE 0 END), 0) -ISNULL(SUM(CASE WHEN _rc.tk_co LIKE CONCAT(@Tk,''%'') THEN ISNULL(_rc.so_luong,0) ELSE 0 END), 0) sl_ton 
-- FROM _rc
-- WHERE _rc.SubsidiaryId = @SubId 
-- AND _rc.vthhtp = @vthhtp
-- AND (_rc.tk_co LIKE CONCAT(@Tk,''%'') OR _rc.tk_no LIKE CONCAT(@Tk,''%''))
-- AND _rc.ngay_ct <= @Date' WHERE [ProgramingFunctionId] = 23

PRINT(N'Update row in [dbo].[InputTypeGlobalSetting]')
UPDATE [dbo].[InputTypeGlobalSetting] SET [UpdatedDatetimeUtc]='2023-06-30 04:33:45.3179357', [BeforeSaveAction]=N'-- @Rows - Thông tin dữ liệu thêm vào db
-- @ResStatus - Kết quả trả về 
-- @Message - Thông báo lỗi

SET @ResStatus = 0;


-- Kiểm tra điều kiện tồn kho nếu là phiếu xuất
DECLARE @isError BIT = 0;
DECLARE @productCode nvarchar(512);
DECLARE @remainQuantity decimal(32,12);
DECLARE @accountNumber nvarchar(25);
DECLARE @unitName nvarchar(25);
DECLARE @exportQuantity decimal(32,12);
-- Lấy ra số lượng xuất group theo sản phẩm
-- Lấy số lượng tồn kho trong kế toán
-- Kiểm tra nếu ko đủ báo lỗi
WITH prod AS (
    SELECT 
        v.vthhtp,
        SUM(ISNULL(v.so_luong,0.0)) so_luong,
        MAX(v.ngay_ct) ngay_ct,
        MAX(v.tk_co) tk_co
    FROM (
        SELECT
            r.*,
            CASE
                WHEN ac0.IsStock = 1 THEN r.tk_co0
                WHEN ac1.IsStock = 1 THEN ac1.AccountNumber
                WHEN ac2.IsStock = 1 THEN ac2.AccountNumber
                WHEN ac3.IsStock = 1 THEN ac3.AccountNumber
                ELSE ac4.AccountNumber
            END tk_co
        FROM @Rows r
        LEFT JOIN v_AccountingAccount ac0 ON ac0.AccountNumber = r.tk_co0
        LEFT JOIN v_AccountingAccount ac1 ON ac1.AccountNumber = r.tk_co1
        LEFT JOIN v_AccountingAccount ac2 ON ac2.AccountNumber = r.tk_co2
        LEFT JOIN v_AccountingAccount ac3 ON ac3.AccountNumber = r.tk_co3
        LEFT JOIN v_AccountingAccount ac4 ON ac4.AccountNumber = r.tk_co4
        WHERE ac0.IsStock = 1 OR ac1.IsStock = 1 OR ac2.IsStock = 1 OR ac3.IsStock = 1 OR ac4.IsStock = 1
    ) v
    GROUP BY v.vthhtp, v.tk_co
),
remain AS (
	-- SELECT 
	-- 	rc.vthhtp,
    --     MAX(p.so_luong) so_luong,
    --     MAX(p.tk_co) tk_co,
	-- 	ISNULL(SUM(CASE WHEN rc.tk_no LIKE CONCAT(p.tk_co,''%'') THEN ISNULL(rc.so_luong,0) ELSE 0 END), 0) - ISNULL(SUM(CASE WHEN rc.tk_co LIKE CONCAT(p.tk_co,''%'') THEN ISNULL(rc.so_luong,0) ELSE 0 END), 0) sl_ton 
	-- FROM prod p
	-- LEFT JOIN [AccountancyDB].dbo._rc rc ON p.vthhtp = rc.vthhtp
	-- WHERE rc.SubsidiaryId = @SubId
	-- 	AND (rc.tk_co LIKE CONCAT(p.tk_co,''%'') OR rc.tk_no LIKE CONCAT(p.tk_co,''%''))
	-- 	AND rc.ngay_ct <= p.ngay_ct
    --     AND rc.InputBill_F_Id != @BillF_Id
	-- GROUP BY rc.vthhtp, p.tk_co

    SELECT 
            d.vthhtp, 
            d.Tk,
            MAX(d.so_luong_xuat) so_luong_xuat,
            MAX(d.tk_co) tk_co,
            ISNULL(SUM(
                    --CASE WHEN d.InRowNumber = 1 THEN 
                        CASE d.IsDebt
                             WHEN 1 THEN d.so_luong
                             ELSE -d.so_luong 
                        END
                    --ELSE 0 END
                    ),
              0) sl_ton
  FROM (   SELECT d.so_luong,
                  d.IsDebt,
                  d.Tk,
                  p.vthhtp,
                  p.so_luong so_luong_xuat,
                  p.tk_co,
                  ROW_NUMBER() OVER (PARTITION BY d.F_Id ORDER BY d.ngay_ct) InRowNumber
             FROM prod p
                LEFT JOIN dbo._rc_detail d  ON p.vthhtp = d.vthhtp
            WHERE d.SubsidiaryId = @SubId
              AND d.Tk LIKE CONCAT(p.tk_co, ''%'')
              AND d.InputBill_F_Id <> @BillF_Id
              AND d.BUT_TOAN <> 3
              AND d.ngay_ct      <= p.ngay_ct) d
    GROUP BY d.vthhtp, d.Tk
)
SELECT TOP 1 
    @isError = 1, 
    @remainQuantity = r.sl_ton,
    @productCode = p.ProductCode,
    @accountNumber = r.tk_co,
    @unitName = u.UnitName,
    @exportQuantity = r.so_luong_xuat
FROM remain r 
LEFT JOIN [StockDB].[dbo].Product p ON r.vthhtp = p.ProductId AND p.IsDeleted = 0
LEFT JOIN [MasterDB].[dbo].Unit u ON p.UnitId = u.UnitId AND u.IsDeleted = 0
WHERE r.so_luong_xuat > r.sl_ton;

IF (@isError = 1) 
BEGIN
    SET @ResStatus = 1;
    SET @Message = CONCAT(N''Số lượng tồn kho '', @productCode, N'' trong tài khoản '', @accountNumber, N'' còn tồn '', CAST(@remainQuantity AS FLOAT), N'' '', @unitName, N'' không đủ để xuất '', CAST(@exportQuantity AS FLOAT), N'' '', @unitName);
    RETURN;
END



DECLARE @tk_co0 nvarchar(128);
DECLARE @tk_no0 nvarchar(128);
DECLARE @kh0 nvarchar(512);
DECLARE @kh_co0 nvarchar(512);
DECLARE @vnd0 decimal(32,16);

DECLARE @tk_co1 nvarchar(128);
DECLARE @tk_no1 nvarchar(128);
DECLARE @kh1 nvarchar(512);
DECLARE @vnd1 decimal(32,16);

DECLARE @tk_co3 nvarchar(128);
DECLARE @tk_no3 nvarchar(128);
DECLARE @kh3 nvarchar(512);
DECLARE @vnd3 decimal(32,16);

DECLARE @so_luong decimal(32,16);

IF (SELECT CURSOR_STATUS(''global'',''RowCursor'')) >= -1
BEGIN
    CLOSE RowCursor;  
    DEALLOCATE RowCursor;
END

DECLARE RowCursor CURSOR FOR 
    SELECT 
        r.tk_co0, r.tk_no0, r.kh0, r.kh_co0, r.vnd0,
        r.tk_co1, r.tk_no1, r.kh1, r.vnd1,
        r.tk_co3, r.tk_no3, r.kh3, r.vnd3,
        r.so_luong
    FROM @Rows r
OPEN RowCursor    
FETCH NEXT FROM RowCursor INTO 
    @tk_co0, @tk_no0, @kh0, @kh_co0, @vnd0,
    @tk_co1, @tk_no1, @kh1, @vnd1,
    @tk_co3, @tk_no3, @kh3, @vnd3,
    @so_luong
        
WHILE(@@FETCH_STATUS=0)  
BEGIN  

    -- Validate thông tin số lượng (Nếu có TK kho bắt buộc nhập số lượng #0)
    DECLARE @IsStock_co BIT = 0;
    DECLARE @IsStock_no BIT = 0;
    IF (@tk_co0 IS NOT NULL OR @tk_no0 IS NOT NULL) 
    BEGIN
      
        SELECT @IsStock_co = ac.IsStock FROM v_AccountingAccount ac WHERE ac.AccountNumber = @tk_co0;
        SELECT @IsStock_no = ac.IsStock FROM v_AccountingAccount ac WHERE ac.AccountNumber = @tk_no0;
    END

--tmp comment allow so_luong = 0 for other fee (Phu Tai)
    --IF ((@IsStock_co = 1 OR @IsStock_no = 1) AND (@so_luong IS NULL OR @so_luong = 0))
    --BEGIN
        --SET @ResStatus = 1;
       -- SET @Message = N''Thông tin số lượng không được để trống khi tồn tại tài khoản vật tư hàng hóa'';
       -- BREAK;
    --END


    -- Validate thông tin khách hàng
    DECLARE @IsLiability_co BIT = 0;
    DECLARE @IsLiability_no BIT = 0;

    IF ((@tk_co0 IS NOT NULL OR @tk_no0 IS NOT NULL) AND @vnd0 IS NOT NULL AND @vnd0 != 0) 
    BEGIN
        SELECT @IsLiability_co = ac.IsLiability FROM v_AccountingAccount ac WHERE ac.AccountNumber = @tk_co0;
        SELECT @IsLiability_no = ac.IsLiability FROM v_AccountingAccount ac WHERE ac.AccountNumber = @tk_no0;
        -- when tkn OR tkc is công nợ
        IF((@IsLiability_co = 1 OR @IsLiability_no = 1) AND @kh0 IS NULL)
        BEGIN
            SET @ResStatus = 1;
			DECLARE @tkcn NVARCHAR(250) = '''';
			IF @IsLiability_co = 1
			BEGIN
				SET @tkcn = @tk_co0
			END
			ELSE
			BEGIN
				SET @tkcn = @tk_no0
			END
			
            SET @Message = N''Thông tin khách hàng không được để trống đối với tài khoản công nợ '' + @tkcn;
            BREAK;
        END
        -- when tkn AND tkc is công nợ
        IF(@IsLiability_co = 1 AND @IsLiability_no = 1 AND @kh_co0 IS NULL)
        BEGIN
            SET @ResStatus = 1;
            SET @Message = N''Thông tin khách hàng có không được để trống'';
            BREAK;
        END
    END

    IF ((@tk_co1 IS NOT NULL OR @tk_no1 IS NOT NULL) AND @vnd1 IS NOT NULL AND @vnd1 != 0) 
    BEGIN
        SELECT @IsLiability_co = ac.IsLiability FROM v_AccountingAccount ac WHERE ac.AccountNumber = @tk_co1;
        SELECT @IsLiability_no = ac.IsLiability FROM v_AccountingAccount ac WHERE ac.AccountNumber = @tk_no1;
        -- when tkn OR tkc is công nợ
        IF((@IsLiability_co = 1 OR @IsLiability_no = 1) AND @kh1 IS NULL AND @kh0 IS NULL)
        BEGIN
            SET @ResStatus = 1;
            SET @Message = N''Thông tin khách hàng thuế không được để trống'';
            BREAK;
        END
    END


    IF ((@tk_co3 IS NOT NULL OR @tk_no3 IS NOT NULL) AND @vnd3 IS NOT NULL AND @vnd3 != 0) 
    BEGIN
      
        SELECT @IsLiability_co = ac.IsLiability FROM v_AccountingAccount ac WHERE ac.AccountNumber = @tk_co3;
        SELECT @IsLiability_no = ac.IsLiability FROM v_AccountingAccount ac WHERE ac.AccountNumber = @tk_no3;
        -- when tkn OR tkc is công nợ
        IF((@IsLiability_co = 1 OR @IsLiability_no = 1) AND @kh3 IS NULL)
        BEGIN
            SET @ResStatus = 1;
            SET @Message = N''Thông tin khách hàng thuế xuất nhập khẩu không được để trống'';
            BREAK;
        END
    END

    FETCH NEXT FROM RowCursor INTO 
        @tk_co0, @tk_no0, @kh0, @kh_co0, @vnd0,
        @tk_co1, @tk_no1, @kh1, @vnd1,
        @tk_co3, @tk_no3, @kh3, @vnd3,
        @so_luong
END  
    
CLOSE RowCursor;  
DEALLOCATE RowCursor;' WHERE [InputTypeGlobalSettingId] = 1

PRINT(N'Update rows in [dbo].[InputAreaField]')
UPDATE [dbo].[InputAreaField] SET [Filters]=N'{"condition":"AND","rules":[{"id":"79","field":"F_Id","type":"string","input":"text","operator":5,"value":null,"dataType":2,"fieldName":"F_Id"}],"not":false,"valid":true}', [UpdatedByUserId]=170, [UpdatedDatetimeUtc]='2023-07-04 04:21:09.2431348' WHERE [InputAreaFieldId] = 30
UPDATE [dbo].[InputAreaField] SET [IsHidden]=0, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 02:23:19.4665589' WHERE [InputAreaFieldId] = 468
UPDATE [dbo].[InputAreaField] SET [IsHidden]=0, [UpdatedDatetimeUtc]='2023-06-21 02:23:19.4670667' WHERE [InputAreaFieldId] = 474
UPDATE [dbo].[InputAreaField] SET [IsHidden]=0, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 02:22:19.8678005' WHERE [InputAreaFieldId] = 481
UPDATE [dbo].[InputAreaField] SET [IsHidden]=0, [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 02:22:19.8681114' WHERE [InputAreaFieldId] = 483
UPDATE [dbo].[InputAreaField] SET [DefaultValue]=N'', [UpdatedByUserId]=215, [UpdatedDatetimeUtc]='2023-06-22 04:32:41.1046065' WHERE [InputAreaFieldId] = 1649
UPDATE [dbo].[InputAreaField] SET [DefaultValue]=N'', [UpdatedDatetimeUtc]='2023-06-21 10:34:41.8408307' WHERE [InputAreaFieldId] = 1652
UPDATE [dbo].[InputAreaField] SET [DefaultValue]=N'', [UpdatedDatetimeUtc]='2023-06-21 11:01:50.7696179' WHERE [InputAreaFieldId] = 3248
UPDATE [dbo].[InputAreaField] SET [DefaultValue]=N'', [UpdatedDatetimeUtc]='2023-06-21 11:01:50.7699379' WHERE [InputAreaFieldId] = 3249
UPDATE [dbo].[InputAreaField] SET [DefaultValue]=N'', [UpdatedDatetimeUtc]='2023-06-21 11:02:38.3376672' WHERE [InputAreaFieldId] = 3554
UPDATE [dbo].[InputAreaField] SET [DefaultValue]=N'', [UpdatedDatetimeUtc]='2023-06-21 11:02:38.3373782' WHERE [InputAreaFieldId] = 3555
PRINT(N'Operation applied to 11 rows out of 11')

PRINT(N'Add row to [dbo].[ProgramingFunction]')
SET IDENTITY_INSERT [dbo].[ProgramingFunction] ON
INSERT INTO [dbo].[ProgramingFunction] ([ProgramingFunctionId], [ProgramingFunctionName], [FunctionBody], [ProgramingLangId], [ProgramingLevelId], [Description], [Params]) VALUES (100, N'openPopupSelectPricingToOrder', N'$this.openPopupCategoryData($PricingCategoryCode, null, (data) => {
    if (!data) return;
    const dataObj = data[0];

    if ($bill[''pnk_chung''] && $bill[''pnk_chung''].rows && $bill[''pnk_chung''].rows.length) {
        $bill[''pnk_chung''].rows[0].kh0.value = dataObj.kh0;
        $bill[''pnk_chung''].rows[0].kh0.titleValue = dataObj.kh0_PartnerCode;
        $bill[''pnk_chung''].rows[0].kh0_PartnerName.value = dataObj.kh0_PartnerName;
        if ($bill[''pnk_chung''].rows[0].kh0_Address)
            $bill[''pnk_chung''].rows[0].kh0_Address.value = dataObj.kh0_Address;
        if ($bill[''pnk_chung''].rows[0].ngay_gh)
            $bill[''pnk_chung''].rows[0].ngay_gh.value = dataObj.ngay_gh;
        if ($bill[''pnk_chung''].rows[0].kh_nguoi_lh)
            $bill[''pnk_chung''].rows[0].kh_nguoi_lh.value = dataObj.nguoi_phu_trach_FullName;
        if ($bill[''pnk_chung''].rows[0].dktt)
            $bill[''pnk_chung''].rows[0].dktt.value = dataObj.dktt;
        if ($bill[''pnk_chung''].rows[0].dkgh)
            $bill[''pnk_chung''].rows[0].dkgh.value = dataObj.dkgh;
    }

    var baoGiaVoucherTypeId = dataObj.VoucherTypeId;
    var baoGiaFId = dataObj.F_Id;


    const url = `/PurchasingOrder/data/VoucherBills/${baoGiaVoucherTypeId}/${baoGiaFId}`

    $this.httpGet(url).then(r => {

        const area = Object.entries($bill).find(([key, value]) => value.isMultiRow === true)

        const detailArea = area[1];
       
        detailArea.rows = []
        console.log(r.list)

        $this.createRowsFromArrayData(detailArea.areaId, r.list, null, (newRow, rowData) => {
            newRow.stt.value = detailArea.rows.length + 1
            newRow.vthhtp.value = rowData.vthhtp
            newRow.vthhtp.titleValue = rowData.vthhtp_ProductCode
            newRow.vthhtp_ProductName.value = rowData.vthhtp_ProductName
            newRow.vthhtp_UnitId_UnitName.value = rowData.vthhtp_UnitId_UnitName
            newRow.vthhtp_Specification.value = rowData.vthhtp_Specification
            newRow.so_luong.value = rowData.so_luong
            if (newRow.don_gia0)
                newRow.don_gia0.value = rowData.don_gia0
            if (newRow.vthhtp_dvt2)
                newRow.vthhtp_dvt2.value = rowData.vthhtp_dvt2
            if (newRow.vthhtp_dvt2)
                newRow.vthhtp_dvt2.titleValue = rowData.vthhtp_dvt2_SecondaryUnitId_UnitName
            if (newRow.so_luong_dv2)
                newRow.so_luong_dv2.value = rowData.so_luong_dv2
            if (newRow.don_gia_dv2_0)
                newRow.don_gia_dv2_0.value = rowData.don_gia_dv2_0
            if (newRow.ngoai_te0)
                newRow.ngoai_te0.value = rowData.ngoai_te0
            if (newRow.vnd0)
                newRow.vnd0.value = rowData.vnd0


            // obj[''thue_suat_vat''].value = item.thue_suat_vat
            // obj[''vnd1''].value = item.vnd1
            if ($bill.VAT && $bill.VAT.rows) {
                $bill.VAT.rows[0].thue_suat_vat.value = rowData.thue_suat_vat
                $bill.VAT.rows[0].vnd1.value = rowData.vnd1
            }
            newRow.ghi_chu.value = rowData.ghi_chu
        })
        
        // console.log($bill[area[0]].rows)
        //calcTotalMoney($data);
        //setLastestPriceForBill($data);
        //setRemainQuantityInStockForBill($data);
        //$this.updateTotalPage.emit()
        // hvh 6-4-2022 - replace updateTotalPage with reloadLastPage function
        // $this.updateTotalPage();
        //$this.reloadLastPage(detailArea.voucherAreaId ?? detailArea.areaId);
        //calcPriceTotalAndTaxRowAndBill($data);
    });

});


return;
$this.openPopupCategoryData($PricingCategoryCode, null, (data) => {
    if (!data) return;
    const dataObj = data[0];

    if ($bill[''pnk_chung''] && $bill[''pnk_chung''].rows && $bill[''pnk_chung''].rows.length) {
        $bill[''pnk_chung''].rows[0].kh0.value = dataObj.kh0;
        $bill[''pnk_chung''].rows[0].kh0.titleValue = dataObj.kh0_PartnerCode;
        $bill[''pnk_chung''].rows[0].kh0_PartnerName.value = dataObj.kh0_PartnerName;
        if ($bill[''pnk_chung''].rows[0].kh0_Address)
            $bill[''pnk_chung''].rows[0].kh0_Address.value = dataObj.kh0_Address;
        if ($bill[''pnk_chung''].rows[0].ngay_gh)
            $bill[''pnk_chung''].rows[0].ngay_gh.value = dataObj.ngay_gh;
        if ($bill[''pnk_chung''].rows[0].kh_nguoi_lh)
            $bill[''pnk_chung''].rows[0].kh_nguoi_lh.value = dataObj.nguoi_phu_trach_FullName;
        if ($bill[''pnk_chung''].rows[0].dktt)
            $bill[''pnk_chung''].rows[0].dktt.value = dataObj.dktt;
        if ($bill[''pnk_chung''].rows[0].dkgh)
            $bill[''pnk_chung''].rows[0].dkgh.value = dataObj.dkgh;
    }

    var baoGiaVoucherTypeId = dataObj.VoucherTypeId;
    var baoGiaFId = dataObj.F_Id;


    const url = `/PurchasingOrder/data/VoucherBills/${baoGiaVoucherTypeId}/${baoGiaFId}`

    $this.httpGet(url).then(r => {

        const area = Object.entries($bill).find(([key, value]) => value.isMultiRow === true)

        const detailArea = area[1];
        let row = detailArea.rows[0]

        Object.entries(row).forEach(([key, value]) => {
            row[key].value = ((value.dataTypeId == $this.EnumDataType.Number || value.dataTypeId == $this.EnumDataType.Decimal) && value.formTypeId != $this.FormDataType.SearchTable && value.formTypeId != $this.FormDataType.Select) ? 0 : null
            row[key].titleValue = null
        })

        detailArea.rows = []
        console.log(r.list)
        r.list.forEach(item => {
            const obj = JSON.parse(JSON.stringify(row));

            obj.stt.value = detailArea.rows.length + 1
            obj.vthhtp.value = item.vthhtp
            obj.vthhtp.titleValue = item.vthhtp_ProductCode
            obj.vthhtp_ProductName.value = item.vthhtp_ProductName
            obj.vthhtp_UnitId_UnitName.value = item.vthhtp_UnitId_UnitName
            obj.vthhtp_Specification.value = item.vthhtp_Specification
            obj.so_luong.value = item.so_luong
            if (obj.don_gia0)
                obj.don_gia0.value = item.don_gia0
            if (obj.vthhtp_dvt2)
                obj.vthhtp_dvt2.value = item.vthhtp_dvt2
            if (obj.vthhtp_dvt2)
                obj.vthhtp_dvt2.titleValue = item.vthhtp_dvt2_SecondaryUnitId_UnitName
            if (obj.so_luong_dv2)
                obj.so_luong_dv2.value = item.so_luong_dv2
            if (obj.don_gia_dv2_0)
                obj.don_gia_dv2_0.value = item.don_gia_dv2_0
            if (obj.ngoai_te0)
                obj.ngoai_te0.value = item.ngoai_te0
            if (obj.vnd0)
                obj.vnd0.value = item.vnd0


            // obj[''thue_suat_vat''].value = item.thue_suat_vat
            // obj[''vnd1''].value = item.vnd1
            if ($bill.VAT && $bill.VAT.rows) {
                $bill.VAT.rows[0].thue_suat_vat.value = item.thue_suat_vat
                $bill.VAT.rows[0].vnd1.value = item.vnd1
            }
            obj.ghi_chu.value = item.ghi_chu
            detailArea.rows.push(obj)
        })

        // console.log($bill[area[0]].rows)
        //calcTotalMoney($data);
        setLastestPriceForBill($data);
        setRemainQuantityInStockForBill($data);
        //$this.updateTotalPage.emit()
        // hvh 6-4-2022 - replace updateTotalPage with reloadLastPage function
        // $this.updateTotalPage();
        $this.reloadLastPage(detailArea.voucherAreaId ?? detailArea.areaId);
        calcPriceTotalAndTaxRowAndBill($data);
    });

});', 3, 2, N'Mở popup lựa chọn báo giá / báo giá xuất khẩu vào đơn hàng', N'{"returnType":"void","paramsList":[{"name":"$data","type":"BillDataContext"},{"name":"$PricingCategoryCode","type":"string"}]}')
SET IDENTITY_INSERT [dbo].[ProgramingFunction] OFF

PRINT(N'Add constraints to [dbo].[InputAreaField]')
ALTER TABLE [dbo].[InputAreaField] WITH CHECK CHECK CONSTRAINT [FK_InputAreaField_InputArea]
ALTER TABLE [dbo].[InputAreaField] WITH CHECK CHECK CONSTRAINT [FK_InputAreaField_InputField]
ALTER TABLE [dbo].[InputAreaField] WITH CHECK CHECK CONSTRAINT [FK_InputAreaField_InputType]
COMMIT TRANSACTION
GO

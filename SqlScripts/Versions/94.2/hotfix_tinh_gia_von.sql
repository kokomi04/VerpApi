USE [AccountancyDB]
GO
/****** Object:  StoredProcedure [dbo].[asp_CalcProduct_OutputPrice]    Script Date: 6/28/2023 1:56:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/**
* Tính giá vốn
*/
ALTER   PROCEDURE [dbo].[asp_CalcProduct_OutputPrice]
	@SubId			INT,
	@Tk				NVARCHAR(128),	
	@ProductId		INT,
	@FromDate		DATETIME2,
	@ToDate			DATETIME2,
	@IsIgnoreZeroPrice	BIT = 1,
	@IsUpdate		BIT = 0,
	@IsInvalid		BIT OUTPUT,
	@IsError		BIT OUTPUT
AS
BEGIN

	DECLARE @DIEU_CHUYEN_KHO_TYPE NVARCHAR(128) = N'DIEU_CHUYEN_KHO';

	BEGIN TRANSACTION;

	DECLARE @OpenningDept TABLE
	(
		tk nvarchar(128),
		vthhtp int,
		balance_quantity decimal(32,12),
		balance_money decimal(32,12)
	)

	INSERT INTO @OpenningDept
	(
		tk,
		vthhtp,
		balance_quantity,
		balance_money
	)
	SELECT 
		d.tk, 
		d.vthhtp,
		sum(CASE WHEN d.IsDebt = 1 THEN d.so_luong ELSE -d.so_luong END),
		sum(CASE WHEN d.IsDebt = 1 THEN d.Vnd_no ELSE -d.Vnd_co END)
	FROM dbo._rc_detail d
	WHERE d.SubsidiaryId = @SubId 
		AND d.ngay_ct < @FromDate
		AND d.vthhtp IS NOT NULL
		AND (@ProductId IS NULL OR d.vthhtp = @ProductId)
		AND (@Tk IS NULL OR d.Tk LIKE CONCAT(@Tk,'%'))
		AND
		(d.Tk LIKE '151%'
			OR d.Tk LIKE '152%'
			OR d.Tk LIKE '153%'
			OR d.Tk LIKE '155%'
			OR d.Tk LIKE '156%'
			OR d.Tk LIKE '157%'
			OR d.Tk LIKE '158%'
		)
		
	GROUP BY d.tk, d.vthhtp


	IF @IsIgnoreZeroPrice = 0 AND EXISTS (
			SELECT 
				0
			FROM dbo._rc_detail d	
	
			WHERE d.SubsidiaryId = @SubId
			AND d.ngay_ct BETWEEN @FromDate AND @ToDate
			AND d.vthhtp IS NOT NULL
			AND (@ProductId IS NULL OR d.vthhtp = @ProductId)
			AND (@Tk IS NULL OR d.Tk LIKE CONCAT(@Tk,'%'))
			AND
			(d.Tk LIKE '151%'
				OR d.Tk LIKE '152%'
				OR d.Tk LIKE '153%'
				OR d.Tk LIKE '155%'
				OR d.Tk LIKE '156%'
				OR d.Tk LIKE '157%'
				OR d.Tk LIKE '158%'
			)
			AND d.IsDebt = 1
			AND d.BUT_TOAN = 0
			AND ISNULL(d.don_gia,0)=0
			AND ISNULL(d.Vnd_no,0) = 0
			AND d.InputType_InputTypeCode <> @DIEU_CHUYEN_KHO_TYPE
	)
	BEGIN
	    SET @IsInvalid = 1
		 
			SELECT					
						ROW_NUMBER() OVER(ORDER BY(SELECT NULL)) stt,
						p.ProductCode vthhtp_ProductCode,
						p.ProductName vthhtp_ProductName,
						d.InputType_Title,
						d.InputTypeId,
						d.InputBill_F_Id,
						d.ngay_ct,
						d.so_ct,
						d.tk tk_no,
						d.Tk_du tk_co,
						d.don_gia,
						d.so_luong,
						d.Vnd_no									
						
			FROM dbo._rc_detail d	
				LEFT JOIN dbo.v_Product p ON d.vthhtp = p.F_Id
			WHERE d.SubsidiaryId = @SubId
			AND d.ngay_ct BETWEEN @FromDate AND @ToDate
			AND d.vthhtp IS NOT NULL
			AND (@ProductId IS NULL OR d.vthhtp = @ProductId)			
			AND (@Tk IS NULL OR d.Tk LIKE CONCAT(@Tk,'%'))
			AND
			(d.Tk LIKE '151%'
				OR d.Tk LIKE '152%'
				OR d.Tk LIKE '153%'
				OR d.Tk LIKE '155%'
				OR d.Tk LIKE '156%'
				OR d.Tk LIKE '157%'
				OR d.Tk LIKE '158%'
			)
			AND d.IsDebt = 1
			AND d.so_luong > 0
			AND ISNULL(d.don_gia,0)=0
			AND ISNULL(d.Vnd_no,0) = 0
			AND d.InputType_InputTypeCode <> @DIEU_CHUYEN_KHO_TYPE
			ORDER BY d.Tk, d.vthhtp, d.ngay_ct, d.IsDebt DESC, d.F_Id
		ROLLBACK
		RETURN
	END
	SET @IsInvalid = 0;
	SET @IsError = 0;

	CREATE TABLE #GiaVon
	(	
		InputBill_F_Id bigint,
		BillVersion int,
		f_Id bigint,
		tk nvarchar(128),
		vthhtp int,	
		current_quantity decimal(32,12),	
		current_money decimal(32,12),	
		don_gia decimal(32,12),
		don_gia_vnd decimal(32,12),
		vnd decimal(32,12),
		BUT_TOAN int
	)
	

	DECLARE @current_tk nvarchar(128) = null
	DECLARE @current_vthhtp int = null
	DECLARE @current_quantity decimal(32,12)
	DECLARE @current_money decimal(32,12)

	DECLARE @inputType_InputTypeCode NVARCHAR(128)
	DECLARE @gv_billId bigint
	DECLARE @gv_billVersion int
	DECLARE @gv_f_Id int
	DECLARE @gv_tk nvarchar(128)
	DECLARE @gv_vthhtp int
	DECLARE @gv_IsDebt int
	DECLARE @gv_so_luong decimal(32,12)
	DECLARE @gv_ty_gia decimal(32,12)
	DECLARE @gv_don_gia decimal(32,12)
	DECLARE @gv_vnd_no decimal(32,12)
	DECLARE @gv_vnd_co decimal(32,12)
	DECLARE @gv_BUT_TOAN INT
    DECLARE @gv_DecimalPlace INT

	
	DECLARE @VndDecimalPlace INT = 0;
	SELECT @VndDecimalPlace = DecimalPlace FROM dbo.InputField WHERE FieldName = 'vnd0'
	SET @VndDecimalPlace = ISNULL(@VndDecimalPlace,0)

	DECLARE @CurrencyDefaultDecimalPlace INT
	SELECT @CurrencyDefaultDecimalPlace = DecimalPlace FROM dbo.v_Currency WHERE IsPrimary = 1
	IF ISNULL(@CurrencyDefaultDecimalPlace,0) < @VndDecimalPlace
	BEGIN
	    SET @VndDecimalPlace = ISNULL(@CurrencyDefaultDecimalPlace,0)
	END

	DECLARE cursor_gia_von CURSOR LOCAL FORWARD_ONLY READ_ONLY FOR

	SELECT
		d.InputType_InputTypeCode,
		d.InputBill_F_Id,
		d.LatestBillVersion,
		d.F_Id, 
		d.Tk, 	
		d.vthhtp, 
		d.so_luong,
		d.IsDebt,
		d.ty_gia,
		d.don_gia don_gia,
		d.Vnd_no,
		d.Vnd_co,
		d.BUT_TOAN,
		c.DecimalPlace
	FROM dbo._rc_detail d	
		LEFT JOIN dbo.v_Currency c ON d.loai_tien = c.F_Id
	WHERE d.SubsidiaryId = @SubId
	AND d.ngay_ct BETWEEN @FromDate AND @ToDate
	AND d.vthhtp IS NOT NULL
	AND (@ProductId IS NULL OR d.vthhtp = @ProductId)
	AND (@Tk IS NULL OR d.Tk LIKE CONCAT(@Tk,'%'))
	AND
	(d.Tk LIKE '151%'
		OR d.Tk LIKE '152%'
		OR d.Tk LIKE '153%'
		OR d.Tk LIKE '155%'
		OR d.Tk LIKE '156%'
		OR d.Tk LIKE '157%'
		OR d.Tk LIKE '158%'
	)
	--AND d.so_luong > 0	//thue xnk nhap vao chung tu rieng biet
	ORDER BY d.Tk, d.vthhtp, d.ngay_ct, 
	CASE WHEN d.IsDebt = 1 AND d.InputType_InputTypeCode <> @DIEU_CHUYEN_KHO_TYPE THEN 0 ELSE 1 END,
	CASE WHEN d.InputType_InputTypeCode = @DIEU_CHUYEN_KHO_TYPE THEN d.IsDebt ELSE 2 END,
	d.F_Id

	OPEN cursor_gia_von

	FETCH NEXT FROM cursor_gia_von
	INTO
		@inputType_InputTypeCode,
		@gv_billId,
		@gv_billVersion,
		@gv_f_Id,
		@gv_tk,	
		@gv_vthhtp,		
		@gv_so_luong,
		@gv_IsDebt,		
		@gv_ty_gia,
		@gv_don_gia,
		@gv_vnd_no,
		@gv_vnd_co,
		@gv_BUT_TOAN,
		@gv_DecimalPlace

	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		--Chỉ tính số lượng đối với bút toán thành tiền và trường giá vốn
		--Nếu có thêm bút toán thuế (XNK) thì chỉ cộng thêm lượng tiền vào (tổng tiền = tiền hàng + thuế XNK), số lượng thì chỉ tính 1 lần
		IF @gv_BUT_TOAN <> 0 AND  @gv_BUT_TOAN <> 2--except tax, VND0 = Thanh tien, VND2= Gia von
		BEGIN
			SET @gv_so_luong = 0
		END

		DECLARE @gv_don_gia_vnd DECIMAL(32,12) = 0
		
		IF @inputType_InputTypeCode = @DIEU_CHUYEN_KHO_TYPE AND EXISTS(SELECT 0 FROM #GiaVon WHERE f_Id = @gv_f_Id) AND @gv_IsDebt = 1
		BEGIN
		    SELECT @gv_vnd_no = vnd FROM #GiaVon WHERE f_Id = @gv_f_Id
		END

		--IF @current_vthhtp =4849
		--	BEGIN
		--		SELECT @current_money, @current_vthhtp,1,@gv_vthhtp
		--		ROLLBACK
		--		RETURN
		--	END

		IF ISNULL(@current_tk,'') <> ISNULL(@gv_tk,'') OR ISNULL(@current_vthhtp,0) <> ISNULL(@gv_vthhtp,0)
		BEGIN					
			--important: reinforcement need to reset, because if not exists in OpenningDept it will be keep old value
			SET @current_quantity = 0;
			SET @current_money = 0

			SET @current_tk = @gv_tk
			SET @current_vthhtp = @gv_vthhtp
			SELECT @current_quantity =  balance_quantity, @current_money  = o.balance_money FROM  @OpenningDept o WHERE  o.tk = @gv_tk AND o.vthhtp = @gv_vthhtp
			SET @current_quantity = ISNULL( @current_quantity,0)
			SET @current_money = ISNULL(@current_money,0)

			SET @gv_don_gia_vnd = 0

			IF @current_quantity > 0
			BEGIN
				SET @gv_don_gia_vnd = @current_money/@current_quantity;
			END

			print CONCAT('tk-',@gv_tk,'vthh-',@gv_vthhtp,'current_quantity-',@current_quantity,'current_money-',@current_money,'-','-')
		END
			
		IF @current_money >200000000000
		BEGIN
			SELECT @current_money, @current_vthhtp, 'max ' max1
			ROLLBACK
			RETURN
		END
		IF @gv_IsDebt = 1
		BEGIN
			
				SET @current_quantity = ISNULL(@current_quantity,0) + ISNULL(@gv_so_luong,0)
				SET @current_money = ISNULL(@current_money,0) + ISNULL(@gv_vnd_no,0)		
				
				
		END
		ELSE
		BEGIN
			
			IF ISNULL(@current_quantity,0) - @gv_so_luong < 0
			BEGIN
			    SET @IsError = 1;

				SELECT					
						ROW_NUMBER() OVER(ORDER BY(SELECT NULL)) stt,
						p.ProductCode vthhtp_ProductCode,
						p.ProductName vthhtp_ProductName,
						d.InputType_Title,
						d.InputTypeId,
						d.InputBill_F_Id,
						d.ngay_ct,
						d.so_ct,
						d.tk_no,
						d.tk_co,
						d.don_gia,
						d.so_luong,
						
						@current_quantity current_quantity,
						@current_money current_money
						
				FROM dbo._rc d	
					LEFT JOIN dbo.v_Product p ON d.vthhtp = p.F_Id
				WHERE d.F_Id = @gv_f_Id
				AND (@Tk IS NULL OR d.tk_co LIKE CONCAT(@Tk,'%'))
					AND
					(d.tk_co LIKE '151%'
						OR d.tk_co LIKE '152%'
						OR d.tk_co LIKE '153%'
						OR d.tk_co LIKE '155%'
						OR d.tk_co LIKE '156%'
						OR d.tk_co LIKE '157%'
						OR d.tk_co LIKE '158%'
					)
				ROLLBACK
				RETURN
			END


			SET @gv_don_gia_vnd = 0

			IF @current_quantity > 0
			BEGIN
				SET @gv_don_gia_vnd = @current_money/@current_quantity;
			END

			SET @gv_don_gia = @gv_don_gia_vnd / CASE WHEN @gv_ty_gia >0 THEN @gv_ty_gia ELSE 1 END
			IF @gv_DecimalPlace >= 0
			BEGIN
				SET @gv_don_gia = ROUND(@gv_don_gia, @gv_DecimalPlace);  
			END

			DECLARE @gv_thanh_tien DECIMAL(32,12) = @gv_so_luong *  @gv_don_gia_vnd

			SET @current_quantity = ISNULL(@current_quantity,0) - @gv_so_luong

			IF @current_quantity = 0
			BEGIN
			    SET @gv_thanh_tien = ISNULL(@current_money,0)
			END

			SET @current_money = ISNULL(@current_money,0) - @gv_thanh_tien			
			SET @current_money = ROUND(@current_money, @VndDecimalPlace);
			SET @gv_don_gia_vnd = ROUND(@gv_don_gia_vnd, @VndDecimalPlace);
			SET @gv_thanh_tien = ROUND(@gv_thanh_tien, @VndDecimalPlace);
			
						
			INSERT INTO #GiaVon
					(
						InputBill_F_Id,
						BillVersion,
						f_Id,
						tk,
						vthhtp,
						current_quantity,
						current_money,
						don_gia,
						don_gia_vnd,
						vnd,
						BUT_TOAN
					)
					VALUES
					(
						@gv_billId,
						@gv_billVersion,
						@gv_f_Id, -- f_Id - bigint
						@gv_tk, -- tk - int
						@gv_vthhtp, -- vthhtp - int
						@current_quantity,
						@current_money,
						@gv_don_gia,
						@gv_don_gia_vnd,
						@gv_thanh_tien,
						@gv_BUT_TOAN
					)				
		END	

		FETCH NEXT FROM cursor_gia_von
		INTO
			@inputType_InputTypeCode,
			@gv_billId,
			@gv_billVersion,
			@gv_f_Id,
			@gv_tk,			
			@gv_vthhtp,		
			@gv_so_luong,
			@gv_IsDebt,		
			@gv_ty_gia,
			@gv_don_gia,
			@gv_vnd_no,
			@gv_vnd_co,
			@gv_BUT_TOAN,
			@gv_DecimalPlace
	END

	CLOSE cursor_gia_von
	DEALLOCATE cursor_gia_von

	IF @IsUpdate = 1
	BEGIN
			DECLARE @InputValueRowColumns nvarchar(max) = ''
			SELECT @InputValueRowColumns += ','+[COLUMN_NAME] 
				FROM [INFORMATION_SCHEMA].[COLUMNS] 
				WHERE [TABLE_NAME] = 'InputValueRow' 
					AND [COLUMN_NAME] NOT IN('F_Id','BillVersion','CreatedDatetimeUtc','UpdatedDatetimeUtc','SystemLog',
											'don_gia0','don_gia1','don_gia2','don_gia3','don_gia4',
											'vnd0','vnd1','vnd2','vnd3','vnd4','vnd5');

			DECLARE @Sql NVARCHAR(max) = N'
			INSERT INTO InputValueRow(
				BillVersion,CreatedDatetimeUtc,UpdatedDatetimeUtc,SystemLog,
				don_gia0, don_gia1, don_gia2, don_gia3, don_gia4,
				vnd0,vnd1,vnd2,vnd3,vnd4
			' + @InputValueRowColumns + N')

			SELECT 
				BillVersion + 1, GETUTCDATE(), GETUTCDATE(),N''Created automatic by UpdateOutputPrice'', 
				ISNULL(p.don_gia0, r.don_gia0), ISNULL(p.don_gia1, r.don_gia1), ISNULL(p.don_gia2, r.don_gia2), ISNULL(p.don_gia3, r.don_gia3), ISNULL(p.don_gia4, r.don_gia4),
				ISNULL(p.vnd0, r.vnd0), ISNULL(p.vnd1, r.vnd1), ISNULL(p.vnd2, r.vnd2), ISNULL(p.vnd3, r.vnd3), ISNULL(p.vnd4, r.vnd4)
				' + @InputValueRowColumns + N'

				FROM InputValueRow r
					LEFT JOIN (
						SELECT 
							p.f_Id, 
							MAX(CASE WHEN p.BUT_TOAN = 0 THEN p.don_gia ELSE NULL END) don_gia0,
							MAX(CASE WHEN p.BUT_TOAN = 1 THEN p.don_gia ELSE NULL END) don_gia1,
							MAX(CASE WHEN p.BUT_TOAN = 2 THEN p.don_gia ELSE NULL END) don_gia2,
							MAX(CASE WHEN p.BUT_TOAN = 3 THEN p.don_gia ELSE NULL END) don_gia3,
							MAX(CASE WHEN p.BUT_TOAN = 4 THEN p.don_gia ELSE NULL END) don_gia4,

							MAX(CASE WHEN p.BUT_TOAN = 0 THEN p.vnd ELSE NULL END) vnd0,
							MAX(CASE WHEN p.BUT_TOAN = 1 THEN p.vnd ELSE NULL END) vnd1,
							MAX(CASE WHEN p.BUT_TOAN = 2 THEN p.vnd ELSE NULL END) vnd2,
							MAX(CASE WHEN p.BUT_TOAN = 3 THEN p.vnd ELSE NULL END) vnd3,
							MAX(CASE WHEN p.BUT_TOAN = 4 THEN p.vnd ELSE NULL END) vnd4
						FROM #GiaVon p 
						GROUP BY p.f_Id
					) as p ON r.F_Id = p.f_Id
				WHERE r.IsDeleted=0 AND r.SubsidiaryId = @SubId AND r.InputBill_F_Id IN (SELECT DISTINCT InputBill_F_Id FROM #GiaVon)';
			EXECUTE dbo.sp_executesql @stmt = @Sql, @params = N'@SubId INT', @SubId = @SubId;

			UPDATE r 
				SET IsDeleted = 1,
					SystemLog = N'Deleted automatic by Update product cost Output Price',
					DeletedDatetimeUtc = GETUTCDATE()
				FROM dbo.InputValueRow r
				JOIN (
					SELECT DISTINCT InputBill_F_Id, BillVersion FROM #GiaVon
				) p ON r.InputBill_F_Id = p.InputBill_F_Id AND r.BillVersion <= p.BillVersion
				WHERE r.SubsidiaryId = @SubId AND r.IsDeleted=0;		

			UPDATE b 
				SET LatestBillVersion = b.LatestBillVersion + 1,
					UpdatedDatetimeUtc = GETUTCDATE()
				FROM dbo.InputBill b
				JOIN (
					SELECT DISTINCT InputBill_F_Id FROM #GiaVon
				) p ON b.F_Id = p.InputBill_F_Id
				WHERE b.SubsidiaryId = @SubId;

			UPDATE u
				SET sum_vnd0 = (SELECT SUM(r.vnd0) FROM dbo.InputValueRow r WHERE r.InputBill_F_Id = u.InputBill_F_Id AND r.IsBillEntry = 0 AND u.BillVersion = r.BillVersion),
					sum_vnd1 = (SELECT SUM(r.vnd1) FROM dbo.InputValueRow r WHERE r.InputBill_F_Id = u.InputBill_F_Id AND r.IsBillEntry = 0 AND u.BillVersion = r.BillVersion),
					sum_vnd2 = (SELECT SUM(r.vnd2) FROM dbo.InputValueRow r WHERE r.InputBill_F_Id = u.InputBill_F_Id AND r.IsBillEntry = 0 AND u.BillVersion = r.BillVersion),
					sum_vnd3 = (SELECT SUM(r.vnd3) FROM dbo.InputValueRow r WHERE r.InputBill_F_Id = u.InputBill_F_Id AND r.IsBillEntry = 0 AND u.BillVersion = r.BillVersion),
					sum_vnd4 = (SELECT SUM(r.vnd4) FROM dbo.InputValueRow r WHERE r.InputBill_F_Id = u.InputBill_F_Id AND r.IsBillEntry = 0 AND u.BillVersion = r.BillVersion)
			FROM dbo.InputValueRow u
			WHERE u.SubsidiaryId = @SubId AND u.IsBillEntry = 1 AND u.IsDeleted = 0 AND u.InputBill_F_Id IN (SELECT DISTINCT InputBill_F_Id FROM #GiaVon)
		
	END

	
	SELECT					
			ROW_NUMBER() OVER(ORDER BY(SELECT NULL)) stt,
			v.vthhtp,
			p.ProductCode vthhtp_ProductCode,
			p.ProductName vthhtp_ProductName,
			v.InputType_Title,
			v.InputTypeId,
			v.InputBill_F_Id,
			v.ngay_ct,
			v.so_ct,
			v.tk_no,
			v.tk_co,
			v.don_gia * CASE WHEN v.ty_gia>0 THEN v.ty_gia ELSE 1 END don_gia,
			v.so_luong,
			v.vnd,
			gv.don_gia * CASE WHEN v.ty_gia>0 THEN v.ty_gia ELSE 1 END AS don_gia_update,
			gv.vnd as vnd_update,
			v.BUT_TOAN

		FROM #GiaVon gv
			JOIN dbo._rc as v ON gv.f_Id = v.F_Id AND gv.BUT_TOAN = v.BUT_TOAN
			LEFT JOIN dbo.v_Product p ON v.vthhtp = p.F_Id					
		--WHERE gv.vnd > 0
		ORDER BY v.vthhtp, v.ngay_ct, v.InputTypeId, v.InputBill_F_Id, v.BUT_TOAN, v.F_Id
				
		
	DROP TABLE IF EXISTS #GiaVon
	COMMIT TRANSACTION
END


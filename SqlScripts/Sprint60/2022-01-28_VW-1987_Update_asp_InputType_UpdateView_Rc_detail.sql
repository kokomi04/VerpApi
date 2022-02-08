ALTER PROCEDURE [dbo].[asp_InputType_UpdateView_Rc_detail]
AS
BEGIN
    DECLARE @FieldNames nvarchar(max) = ''
		DECLARE @APPROVE int = 1;
		SELECT @FieldNames += ',' + [COLUMN_NAME]
		FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow'
			--AND CHARINDEX('kh', [COLUMN_NAME]) != 1
			--AND CHARINDEX('tk_co', [COLUMN_NAME]) != 1
			--AND CHARINDEX('tk_no', [COLUMN_NAME]) != 1
			--AND CHARINDEX('loai_tien', [COLUMN_NAME]) != 1
			--AND CHARINDEX('ty_gia', [COLUMN_NAME]) != 1
			--AND CHARINDEX('vnd', [COLUMN_NAME]) != 1
			--AND CHARINDEX('ngoai_te', [COLUMN_NAME]) != 1

    IF LEN(@FieldNames)>0
    BEGIN
        SET @FieldNames = SUBSTRING(@FieldNames,2,LEN(@FieldNames)-1)
    END
    
    DECLARE @TransactionUnion nvarchar(max) = ''
    DECLARE @coupleIndex int = 0
    
    WHILE @coupleIndex < 6
    BEGIN
        DECLARE @TkNoName nvarchar(50) = CONCAT('r.tk_no' , @coupleIndex)        
        DECLARE @TkCoName nvarchar(50) = CONCAT('r.tk_co' , @coupleIndex)        
				DECLARE @VndName nvarchar(50) = CONCAT('r.vnd' , @coupleIndex)
				DECLARE @NgoaiTeName nvarchar(50) = CONCAT('r.ngoai_te' , @coupleIndex)
        DECLARE @DonGia nvarchar(50) = CONCAT('r.don_gia' , @coupleIndex)
        DECLARE @Kh nvarchar(50) = CONCAT('r.kh' , @coupleIndex)
        DECLARE @Khc nvarchar(50) = CONCAT('r.kh_co' , @coupleIndex)	
        
        IF(NOT EXISTS (SELECT * FROM sys.columns WHERE Name = CONCAT('tk_no' , @coupleIndex) AND Object_ID = Object_ID('vInputBillRow'))) 
        BEGIN
            SET @TkNoName = 'NULL'
        END
        
        IF(NOT EXISTS (SELECT * FROM sys.columns WHERE Name = CONCAT('tk_co' , @coupleIndex) AND Object_ID = Object_ID('vInputBillRow')))
        BEGIN
            SET @TkCoName = 'NULL'
        END
        
        IF(NOT EXISTS (SELECT * FROM sys.columns WHERE Name = CONCAT('kh' , @coupleIndex) AND Object_ID = Object_ID('vInputBillRow')))
        BEGIN
            SET @Kh = 'NULL'
        END
        
        IF(NOT EXISTS (SELECT * FROM sys.columns WHERE Name = CONCAT('kh_co' , @coupleIndex) AND Object_ID = Object_ID('vInputBillRow')))
        BEGIN
            SET @Khc = 'NULL'
        END
				
				IF(NOT EXISTS (SELECT * FROM sys.columns WHERE Name = CONCAT('don_gia' , @coupleIndex) AND Object_ID = Object_ID('vInputBillRow')))
        BEGIN
            SET @DonGia = 'NULL'
        END

        IF(NOT EXISTS (SELECT * FROM sys.columns WHERE Name = CONCAT('ngoai_te' , @coupleIndex) AND Object_ID = Object_ID('vInputValueRow')))
        BEGIN
            SET @NgoaiTeName = 'NULL'
        END
				
        IF(EXISTS (SELECT * FROM sys.columns WHERE Name = CONCAT('vnd' , @coupleIndex) AND Object_ID = Object_ID('vInputBillRow'))) 
        BEGIN
					IF(@TkNoName != 'NULL')
					BEGIN
						SET @TransactionUnion += N'
						SELECT ' + @TkNoName +' Tk, ' + @TkCoName + ' Tk_du, 1 AS IsDebt, ' + @DonGia + ' don_gia, ' + @VndName + ' Vnd_no, NULL Vnd_co, ' + @NgoaiTeName + ' Ngoai_te_no, NULL Ngoai_te_co, ISNULL(' + @Kh + ', r.kh0) kh, ' + CONVERT(nvarchar(max),@coupleIndex) +' AS BUT_TOAN WHERE (('+ @VndName + '>0 OR InputType_IsOpenning =1) OR ' + @TkCoName + ' IS NOT NULL) AND ('  + @TkNoName + ' IS NOT NULL) 
						UNION ALL'
					END
					IF(@TkCoName != 'NULL')
					BEGIN
						SET @TransactionUnion += N'
						SELECT ' + @TkCoName +' Tk, ' + @TkNoName + ' Tk_du, 0 AS IsDebt, ' + @DonGia + ' don_gia, NULL Vnd_no, ' + @VndName + ' Vnd_co, NULL Ngoai_te_no,' + @NgoaiTeName + ' Ngoai_te_co, ISNULL('+ @Khc + ',ISNULL('+ @Kh + ',ISNULL(r.kh_co0, r.kh0))) kh, ' + CONVERT(nvarchar(max),@coupleIndex) +' AS BUT_TOAN WHERE (('+ @VndName + '>0 OR InputType_IsOpenning =1) OR ' + @TkNoName + ' IS NOT NULL) AND ('  + @TkCoName + ' IS NOT NULL)
						UNION ALL'
					END
        END
        SET @coupleIndex = @coupleIndex+1;
    END
    
    IF LEN(@TransactionUnion)>0
    BEGIN
        SET @TransactionUnion = SUBSTRING(@TransactionUnion,1,LEN(@TransactionUnion)-9)
    END
        
    IF OBJECT_ID('dbo._rc_detail') IS NOT NULL
    BEGIN
        DROP VIEW _rc_detail;
    END
    
		IF DATALENGTH(@TransactionUnion) = 0
			SET @TransactionUnion = 'SELECT NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL'
		
    DECLARE @Sql nvarchar(max) = '';
		SET @Sql = CONCAT(N'CREATE VIEW _rc_detail AS 
    SELECT ', @FieldNames, ', c.Tk, c.Tk_du, c.IsDebt, c.don_gia, c.Vnd_no, c.Vnd_co, c.Ngoai_te_no, c.Ngoai_te_co, c.kh, c.BUT_TOAN FROM vInputBillRow r WITH(NOLOCK)
    CROSS APPLY
    ( ', @TransactionUnion, ' ) c WHERE r.CensorStatusId = ', @APPROVE, ';');
		
    EXEC (@Sql)
    
    


END
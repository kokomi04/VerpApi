ALTER PROCEDURE [dbo].[asp_InputType_UpdateView_Rc]
AS
BEGIN
	--RETURN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @FieldNames nvarchar(max) = '';
	DECLARE @APPROVE int = 1;

	SELECT @FieldNames += ',d.' + [COLUMN_NAME]
		FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow'		

	IF LEN(@FieldNames)>0
	BEGIN
		SET @FieldNames = SUBSTRING(@FieldNames,2,LEN(@FieldNames)-1)
	END


	DECLARE @Sql nvarchar(max) = '';


	DECLARE @MissingFields TABLE(
		FieldName nvarchar(128)
	)

	DECLARE @KhachHang_Field nvarchar(max) = 'kh0'
	DECLARE @KhachHang_Co_Field nvarchar(max) = 'ISNULL(kh_co0,kh0)'
		
	DECLARE @TransactionUnion nvarchar(max) = ''
	
	DECLARE @coupleIndex INT = 0
	DECLARE @strCoupleIndex nvarchar(max)
	
	WHILE @coupleIndex < 6
	BEGIN
		SET @strCoupleIndex = convert(nvarchar(128), @coupleIndex);

		DECLARE @Couple nvarchar(max) = ''
		
		DECLARE @IsExistsTk BIT = 1

		DECLARE @IsExistsVnd BIT = 1

		DECLARE @FieldName nvarchar(max)

		

		--1.1. Khach hang no
		SET @FieldName = 'kh'+@strCoupleIndex		

		DECLARE @kh_no_expression nvarchar(max) = ''

		IF EXISTS (SELECT 0 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow' AND [COLUMN_NAME] = @FieldName)
		BEGIN
			IF LEN(@KhachHang_Field)>0
			BEGIN
				SET @kh_no_expression = 'ISNULL('+@FieldName+', ' + @KhachHang_Field +')'				
			END
			ELSE
			BEGIN
				SET @kh_no_expression = @FieldName				
			END
		END
		ELSE
		BEGIN
			INSERT INTO @MissingFields VALUES(@FieldName);

			IF LEN(@KhachHang_Field)>0
			BEGIN
				SET @kh_no_expression = @KhachHang_Field
			END
			ELSE
			BEGIN
				SET @kh_no_expression = 'NULL'				
			END
		END
		SET @Couple+= @kh_no_expression +' AS ' + @FieldName

		SET @Couple += ','

		--1.2. Khach hang co
		SET @FieldName = 'kh_co'+@strCoupleIndex
		
		DECLARE @kh_co_expression nvarchar(max) = ''

		IF EXISTS (SELECT 0 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow' AND [COLUMN_NAME] = @FieldName)
		BEGIN
			IF LEN(@KhachHang_Co_Field)>0
			BEGIN
				SET @kh_co_expression = 'ISNULL('+@FieldName+', ISNULL(' + @kh_no_expression + ',' +@KhachHang_Co_Field +'))'
			END
			ELSE
			BEGIN
				SET @kh_co_expression = 'ISNULL('+@FieldName+', ' + @kh_no_expression + ')'
			END
		END
		ELSE
		BEGIN
			INSERT INTO @MissingFields VALUES(@FieldName);

			IF LEN(@KhachHang_Co_Field)>0
			BEGIN
				SET @kh_co_expression = 'ISNULL(' + @kh_no_expression + ',' +@KhachHang_Co_Field +')'
			END
			ELSE
			BEGIN
				SET @kh_co_expression = @kh_no_expression
			END
		END
		SET @Couple+= @kh_co_expression +' AS ' + @FieldName
		

		SET @Couple += ','

		--2. loai tien
		SET @FieldName = 'loai_tien'+@strCoupleIndex
		
		IF EXISTS (SELECT 0 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow' AND [COLUMN_NAME] = @FieldName)
		BEGIN			
			SET @Couple += @FieldName		
		END
		ELSE
		BEGIN
			INSERT INTO @MissingFields VALUES(@FieldName);

			SET @Couple += 'NULL AS ' + @FieldName			
		END

		SET @Couple += ','

		--3. tk_co
		SET @FieldName = 'tk_co'+@strCoupleIndex
		
		IF EXISTS (SELECT 0 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow' AND [COLUMN_NAME] = @FieldName)
		BEGIN			
			SET @Couple += @FieldName
		END
		ELSE
		BEGIN
			INSERT INTO @MissingFields VALUES(@FieldName);

			SET @Couple += ' NULL AS ' + @FieldName
			SET @IsExistsTk = 0;
		END

		SET @Couple += ','

		--4. tk_no
		SET @FieldName = 'tk_no'+@strCoupleIndex
		
		IF EXISTS (SELECT 0 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow' AND [COLUMN_NAME] = @FieldName)
		BEGIN			
			SET @Couple += @FieldName
		END
		ELSE
		BEGIN
			INSERT INTO @MissingFields VALUES(@FieldName);

			SET @Couple += ' NULL AS ' + @FieldName
			SET @IsExistsTk = 0;
		END

		SET @Couple += ','

		--5. ty_gia
		SET @FieldName = 'ty_gia'+@strCoupleIndex
		
		IF EXISTS (SELECT 0 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow' AND [COLUMN_NAME] = @FieldName)
		BEGIN			
			SET @Couple += @FieldName		
		END
		ELSE
		BEGIN
			INSERT INTO @MissingFields VALUES(@FieldName);
			SET @Couple += ' NULL AS ' + @FieldName			
		END


		SET @Couple += ','

		--6. don_gia
		SET @FieldName = 'don_gia'+@strCoupleIndex
		
		IF EXISTS (SELECT 0 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow' AND [COLUMN_NAME] = @FieldName)
		BEGIN
			SET @Couple += @FieldName
		END
		ELSE
		BEGIN
			INSERT INTO @MissingFields VALUES(@FieldName);

			SET @Couple += ' NULL AS ' + @FieldName
		END

		SET @Couple += ','

		--6. vnd
		SET @FieldName = 'vnd'+@strCoupleIndex
		
		IF EXISTS (SELECT 0 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow' AND [COLUMN_NAME] = @FieldName)
		BEGIN
			SET @Couple += @FieldName
		END
		ELSE
		BEGIN
			INSERT INTO @MissingFields VALUES(@FieldName);

			SET @Couple += ' NULL AS ' + @FieldName
			SET @IsExistsVnd = 0
		END

		SET @Couple += ','

		--7. ngoai_te
		SET @FieldName = 'ngoai_te'+@strCoupleIndex
		
		IF EXISTS (SELECT 0 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = 'vInputBillRow' AND [COLUMN_NAME] = @FieldName)
		BEGIN			
			SET @Couple += @FieldName		
		END
		ELSE
		BEGIN
			INSERT INTO @MissingFields VALUES(@FieldName);

			SET @Couple += ' NULL AS ' + @FieldName			
		END

		SET @Couple += ',' + @strCoupleIndex + ' AS BUT_TOAN'
		IF @IsExistsTk = 1 AND @IsExistsVnd = 1
		BEGIN

		--WHERE Vnd' + @strCoupleIndex + '>0 OR (tk_co' + @strCoupleIndex + ' IS NOT NULL AND tk_no' + @strCoupleIndex + ' IS NOT NULL)

			SET @TransactionUnion += N'
			SELECT ' + @Couple + '			
			WHERE Vnd' + @strCoupleIndex + '>0 OR (tk_co' + @strCoupleIndex + ' IS NOT NULL AND tk_no' + @strCoupleIndex + ' IS NOT NULL)
				OR (InputType_IsOpenning = 1 AND (tk_co' + @strCoupleIndex + ' IS NOT NULL OR tk_no' + @strCoupleIndex + ' IS NOT NULL))
			UNION ALL'
		END

		SET @coupleIndex = @coupleIndex+1;
	END

	IF LEN(@TransactionUnion)>0
	BEGIN
		SET @TransactionUnion = SUBSTRING(@TransactionUnion,1,LEN(@TransactionUnion)-13)
	END

	PRINT @TransactionUnion
	
	DROP VIEW IF EXISTS _rc

	--c.loai_tien, c.ty_gia,
	
	SELECT @FieldNames = @FieldNames + ', NULL AS ' + FieldName FROM @MissingFields;
	
	IF DATALENGTH(@TransactionUnion) = 0
		SET @TransactionUnion = 'SELECT NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL'
	
	SET @Sql = CONCAT(N'
	CREATE VIEW _rc AS
	SELECT ', @FieldNames, '

	, c.kh_no
	, c.kh_co
	, c.loai_tien_nt
	, c.tk_co
	, c.tk_no
	, c.don_gia
	, c.vnd
	, c.ngoai_te
	, c.BUT_TOAN

	FROM dbo.vInputBillRow d WITH(NOLOCK)
	
	CROSS APPLY
	(
		', @TransactionUnion, '
	) c (kh_no, kh_co, loai_tien_nt, tk_co, tk_no, ty_gia, don_gia, vnd, ngoai_te, BUT_TOAN)
	WHERE d.CensorStatusId = ', @APPROVE, ' ;');
	PRINT @Sql
	EXEC (@Sql)

END
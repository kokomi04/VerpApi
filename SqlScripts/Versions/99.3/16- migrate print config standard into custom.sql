USE MasterDB
GO
BEGIN TRY
  BEGIN TRAN
	
	
	INSERT INTO PrintConfigHeaderCustom (
		PrintConfigHeaderStandardId, 
		Title, 
		PrintConfigHeaderCustomCode, 
		JsAction, 
		IsShow, 
		SortOrder, 
		CreatedByUserId, 
		CreatedDatetimeUtc, 
		UpdatedByUserId, 
		UpdatedDatetimeUtc, 
		IsDeleted, 
		DeletedDatetimeUtc
	)
	SELECT 
		s.PrintConfigHeaderStandardId, 
		s.Title, 
		s.PrintConfigHeaderStandardCode, 
		s.JsAction, 
		s.IsShow, 
		s.SortOrder, 
		s.CreatedByUserId, 
		s.CreatedDatetimeUtc, 
		s.UpdatedByUserId, 
		s.UpdatedDatetimeUtc, 
		s.IsDeleted, 
		s.DeletedDatetimeUtc
	FROM PrintConfigHeaderStandard s
	LEFT JOIN PrintConfigHeaderCustom c ON c.PrintConfigHeaderStandardId = s.PrintConfigHeaderStandardId AND c.IsDeleted = 0
	WHERE c.PrintConfigHeaderCustomId IS NULL AND s.IsDeleted = 0


	DECLARE @tblNewCustomId TABLE
	(
		PrintConfigCustomId INT
	);

    INSERT INTO MasterDB.dbo.PrintConfigCustom (
		PrintConfigStandardId,
		PrintConfigHeaderCustomId,
        PrintConfigName,
        Title,
        BodyTable,
        GenerateCode,
        PaperSize,
        Layout,
        HeadTable,
        FootTable,
        StickyFootTable,
        StickyHeadTable,
        HasTable,
        Background,
        GenerateToString,
        TemplateFilePath,
        TemplateFileName,
        ContentType,
        CreatedByUserId,
        CreatedDatetimeUtc,
        UpdatedByUserId,
        UpdatedDatetimeUtc,
        IsDeleted,
        DeletedDatetimeUtc,
        SubsidiaryId--,
        --ModuleTypeId
	 )
	 OUTPUT Inserted.PrintConfigCustomId
	 INTO @tblNewCustomId(PrintConfigCustomId)
     SELECT
        v.PrintConfigStandardId,
		v.PrintConfigHeaderCustomId,
        v.PrintConfigName,
        v.Title,
        v.BodyTable,
        v.GenerateCode,
        v.PaperSize,
        v.Layout,
        v.HeadTable,
        v.FootTable,
        v.StickyFootTable,
        v.StickyHeadTable,
        v.HasTable,
        v.Background,
        v.GenerateToString,
        v.TemplateFilePath,
        v.TemplateFileName,
        v.ContentType,
        v.CreatedByUserId,
        v.CreatedDatetimeUtc,
        v.UpdatedByUserId,
        v.UpdatedDatetimeUtc,
        v.IsDeleted,
        v.DeletedDatetimeUtc,
        v.SubsidiaryId--,
        --v.ModuleTypeId
      FROM (
		SELECT
			p.PrintConfigStandardId,
			c.PrintConfigHeaderCustomId,
			p.PrintConfigName,
			p.Title,
			p.BodyTable,
			p.GenerateCode,
			p.PaperSize,
			p.Layout,
			p.HeadTable,
			p.FootTable,
			p.StickyFootTable,
			p.StickyHeadTable,
			p.HasTable,
			p.Background,
			p.GenerateToString,
			p.TemplateFilePath,
			p.TemplateFileName,
			p.ContentType,
			p.CreatedByUserId,
			p.CreatedDatetimeUtc,
			p.UpdatedByUserId,
			p.UpdatedDatetimeUtc,
			p.IsDeleted,
			p.DeletedDatetimeUtc,
			s.SubsidiaryId--,
			--p.ModuleTypeId
		  FROM MasterDB.dbo.PrintConfigStandard p
				
			  CROSS APPLY (
				SELECT
					s.SubsidiaryId
				  FROM OrganizationDB.dbo.Subsidiary s
				  WHERE s.IsDeleted = 0
			  ) s

			   OUTER APPLY (
				SELECT
					TOP(1)
					c.PrintConfigHeaderCustomId
				  FROM MasterDB.dbo.PrintConfigHeaderCustom c
				  WHERE c.PrintConfigHeaderStandardId = p.PrintConfigHeaderStandardId and c.IsDeleted = 0
			  ) c
		  WHERE p.IsDeleted = 0
	  ) v
      LEFT JOIN MasterDB.dbo.PrintConfigCustom c
        ON c.PrintConfigStandardId = v.PrintConfigStandardId and v.SubsidiaryId = c.SubsidiaryId and c.IsDeleted = 0
      WHERE c.PrintConfigCustomId is null and v.IsDeleted = 0
	  

	  INSERT INTO dbo.PrintConfigCustomModuleType
	  (
	      PrintConfigCustomId,
	      ModuleTypeId
	  )
	 SELECT c.PrintConfigCustomId, m.ModuleTypeId 
	 FROM @tblNewCustomId n
	 JOIN dbo.PrintConfigCustom c ON c.PrintConfigCustomId = n.PrintConfigCustomId
	 JOIN dbo.PrintConfigStandard s ON c.PrintConfigStandardId = s.PrintConfigStandardId
	 JOIN dbo.PrintConfigStandardModuleType m ON m.PrintConfigStandardId = s.PrintConfigStandardId;
	 
	 
	 INSERT INTO dbo.PrintConfigCustomModuleType
		(
			PrintConfigCustomId,
			ModuleTypeId
		)

		SELECT
		c.PrintConfigCustomId,
		st.ModuleTypeId
		FROM dbo.PrintConfigCustom c
		JOIN dbo.PrintConfigStandardModuleType st ON st.PrintConfigStandardId = c.PrintConfigStandardId
		LEFT JOIN dbo.PrintConfigCustomModuleType ct ON c.PrintConfigCustomId = ct.PrintConfigCustomId
		WHERE ct.PrintConfigCustomId IS NULL

  COMMIT TRAN -- Transaction Success!
END TRY
BEGIN CATCH
  IF @@TRANCOUNT > 0
    ROLLBACK TRAN --RollBack in case of Error
END CATCH
BEGIN TRY
  BEGIN TRAN

    INSERT INTO MasterDB.dbo.PrintConfigCustom (PrintConfigStandardId,
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
		TemplateFileId,
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
		SubsidiaryId,
        ModuleTypeId)
      SELECT
        v.PrintConfigStandardId,
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
        v.TemplateFileId,
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
        v.SubsidiaryId,
        v.ModuleTypeId
      FROM (SELECT
        p.PrintConfigStandardId,
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
        p.TemplateFileId,
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
        s.SubsidiaryId,
        s.ModuleTypeId
      FROM MasterDB.dbo.PrintConfigStandard p
      CROSS APPLY (SELECT
        s.SubsidiaryId
      FROM OrganizationDB.dbo.Subsidiary s
      WHERE s.IsDeleted = 0) s
      WHERE p.IsDeleted = 0) v
      LEFT JOIN MasterDB.dbo.PrintConfigCustom c
        ON c.PrintConfigStandardId = v.PrintConfigStandardId and v.SubsidiaryId = c.SubsidiaryId and c.IsDeleted = 0
	  WHERE c.PrintConfigCustomId is null

  COMMIT TRAN -- Transaction Success!
END TRY
BEGIN CATCH
  IF @@TRANCOUNT > 0
    ROLLBACK TRAN --RollBack in case of Error
END CATCH
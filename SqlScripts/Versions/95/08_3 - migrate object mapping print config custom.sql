USE MasterDB
GO
BEGIN TRY
  BEGIN TRAN

    INSERT INTO dbo.ObjectPrintConfigMapping
	(
		PrintConfigCustomId,
		ObjectTypeId,
		ObjectId,
		UpdateByUserId,
		UpdatedDatetimeUtc
	)

	SELECT
		DISTINCT
		c.PrintConfigCustomId,	
		sm.ObjectTypeId,
		sm.ObjectId,
		0,
		GETUTCDATE()
	FROM dbo.PrintConfigCustom c
	JOIN dbo.ObjectPrintConfigStandardMapping sm ON c.PrintConfigStandardId = sm.PrintConfigStandardId
	LEFT JOIN dbo.ObjectPrintConfigMapping cm ON c.PrintConfigCustomId = cm.PrintConfigCustomId 
		AND sm.ObjectTypeId = cm.ObjectTypeId 
		AND sm.ObjectId = cm.ObjectId
	WHERE cm.PrintConfigCustomId IS NULL
	AND c.IsDeleted = 0


  COMMIT TRAN -- Transaction Success!
END TRY
BEGIN CATCH
  IF @@TRANCOUNT > 0
    ROLLBACK TRAN --RollBack in case of Error
END CATCH
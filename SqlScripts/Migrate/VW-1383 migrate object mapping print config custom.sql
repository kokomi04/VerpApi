BEGIN TRY
  BEGIN TRAN

    INSERT INTO MasterDB.dbo.ObjectPrintConfigMapping
      SELECT
        c.PrintConfigCustomId,
        sm.ObjectTypeId,
        sm.ObjectId,
        sm.UpdateByUserId,
        sm.UpdatedDatetimeUtc
      FROM MasterDB.dbo.PrintConfigCustom c
      INNER JOIN MasterDB.dbo.ObjectPrintConfigStandardMapping sm
        ON c.PrintConfigStandardId = sm.PrintConfigStandardId
        AND c.SubsidiaryId = sm.SubsidiaryId
      WHERE c.PrintConfigCustomId NOT IN (SELECT DISTINCT
        PrintConfigCustomId
      FROM MasterDB.dbo.ObjectPrintConfigMapping)

  COMMIT TRAN -- Transaction Success!
END TRY
BEGIN CATCH
  IF @@TRANCOUNT > 0
    ROLLBACK TRAN --RollBack in case of Error
END CATCH
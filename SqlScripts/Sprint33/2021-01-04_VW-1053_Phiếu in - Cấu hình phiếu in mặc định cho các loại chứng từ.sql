TRUNCATE TABLE ObjectPrintConfigMapping

INSERT INTO ObjectPrintConfigMapping (PrintConfigId, ObjectTypeId, ObjectId, SubsidiaryId, UpdateByUserId, UpdatedDatetimeUtc)
SELECT a.PrintConfigId,49 ObjectTypeId, a.ActiveForId ObjectId, 2 SubsidiaryId, 2 UpdateByUserId, GETDATE() UpdatedDatetimeUtc
 FROM PrintConfig a WHERE a.IsDeleted = 0 AND a.ModuleTypeId = 4;
INSERT INTO ObjectPrintConfigMapping (PrintConfigId, ObjectTypeId, ObjectId, SubsidiaryId, UpdateByUserId, UpdatedDatetimeUtc)
SELECT a.PrintConfigId,39 ObjectTypeId, a.ActiveForId ObjectId, 2 SubsidiaryId, 2 UpdateByUserId, GETDATE() UpdatedDatetimeUtc
 FROM PrintConfig a WHERE a.IsDeleted = 0 AND a.ModuleTypeId = 5;
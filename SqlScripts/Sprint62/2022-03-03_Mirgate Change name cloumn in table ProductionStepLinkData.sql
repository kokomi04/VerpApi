USE ManufacturingDB;

ALTER TABLE ProductionStepLinkData ADD LinkDataObjectId bigint NULL;
ALTER TABLE ProductionStepLinkData ADD LinkDataObjectTypeId int NULL;

UPDATE ProductionStepLinkData SET LinkDataObjectId = ObjectId WHERE 1=1;
UPDATE ProductionStepLinkData SET LinkDataObjectTypeId = ObjectTypeId WHERE 1=1;

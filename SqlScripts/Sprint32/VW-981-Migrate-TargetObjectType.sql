USE MasterDB
GO
DECLARE @Product INT =6 
DECLARE @ProductType INT = 5
UPDATE ObjectCustomGenCodeMapping SET TargetObjectTypeId = @Product, ConfigObjectTypeId = @ProductType, ConfigObjectId = ObjectId WHERE ObjectTypeId=@ProductType


DECLARE @Stock INT =9 
DECLARE @InventoryInput INT =30 
DECLARE @InventoryOutput INT =31

UPDATE ObjectCustomGenCodeMapping SET TargetObjectTypeId = @InventoryInput, ConfigObjectTypeId = @Stock, ConfigObjectId = ObjectId WHERE ObjectTypeId=@InventoryInput
UPDATE ObjectCustomGenCodeMapping SET TargetObjectTypeId = @InventoryOutput, ConfigObjectTypeId = @Stock, ConfigObjectId = ObjectId WHERE ObjectTypeId=@InventoryOutput

DECLARE @InputType INT = 34
DECLARE @InputTypeRow INT = 35
DECLARE @InputAreaField INT = 38

UPDATE ObjectCustomGenCodeMapping SET TargetObjectTypeId = @InputTypeRow, ConfigObjectTypeId = @InputAreaField, ConfigObjectId = ObjectId WHERE ObjectTypeId=@InputType

DECLARE @VoucherType INT = 53
DECLARE @VoucherTypeRow INT = 56
DECLARE @VoucherAreaField INT = 59

UPDATE ObjectCustomGenCodeMapping SET TargetObjectTypeId = @VoucherTypeRow, ConfigObjectTypeId = @VoucherAreaField, ConfigObjectId = ObjectId WHERE ObjectTypeId=@VoucherType



UPDATE ObjectCustomGenCodeMapping SET TargetObjectTypeId = ObjectTypeId, ConfigObjectTypeId = ObjectTypeId, ConfigObjectId = ObjectId WHERE ObjectTypeId NOT IN (@ProductType, @InventoryInput, @InventoryOutput,@InputType,@VoucherType)
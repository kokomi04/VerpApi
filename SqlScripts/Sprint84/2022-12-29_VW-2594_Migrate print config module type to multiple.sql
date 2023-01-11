USE MasterDB
GO
/*
INSERT INTO dbo.PrintConfigStandardModuleType
(
    PrintConfigStandardId,
    ModuleTypeId
)
SELECT PrintConfigStandardId, ModuleTypeId FROM dbo.PrintConfigStandard WHERE IsDeleted = 0;

INSERT INTO dbo.PrintConfigStandardModuleType
(
    PrintConfigStandardId,
    ModuleTypeId
)
SELECT PrintConfigStandardId, 7 --AccountantPublic
FROM dbo.PrintConfigStandard WHERE ModuleTypeId = 5--Accountant;


INSERT INTO dbo.PrintConfigCustomModuleType
(
    PrintConfigCustomId,
    ModuleTypeId
)
SELECT PrintConfigCustomId, ModuleTypeId FROM dbo.PrintConfigCustom WHERE IsDeleted = 0;


INSERT INTO dbo.PrintConfigCustomModuleType
(
    PrintConfigCustomId,
    ModuleTypeId
)
SELECT PrintConfigCustomId, 7 --AccountantPublic 
FROM dbo.PrintConfigCustom
WHERE ModuleTypeId = 5--Accountant;



INSERT INTO [dbo].[ObjectPrintConfigStandardMapping]
           ([PrintConfigStandardId]
           ,[ObjectTypeId]
           ,[ObjectId]
           ,[UpdateByUserId]
           ,[UpdatedDatetimeUtc])
SELECT [PrintConfigStandardId]
           ,34001--InputTypePublic
           ,[ObjectId]
           ,[UpdateByUserId]
           ,GETUTCDATE()
FROM [dbo].[ObjectPrintConfigStandardMapping]
WHERE ObjectTypeId = 34--InputType

*/
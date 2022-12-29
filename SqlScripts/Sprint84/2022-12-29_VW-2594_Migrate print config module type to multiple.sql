USE MasterDB
GO
INSERT INTO dbo.PrintConfigStandardModuleType
(
    PrintConfigStandardId,
    ModuleTypeId
)
SELECT PrintConfigStandardId, ModuleTypeId FROM dbo.PrintConfigStandard;


INSERT INTO dbo.PrintConfigCustomModuleType
(
    PrintConfigCustomId,
    ModuleTypeId
)
SELECT PrintConfigCustomId, ModuleTypeId FROM dbo.PrintConfigCustom;
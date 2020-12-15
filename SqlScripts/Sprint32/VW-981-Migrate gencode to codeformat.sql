USE MasterDB
GO
UPDATE CustomGenCode SET CodeFormat = CONCAT(Prefix,'%S_NUMBER%',Suffix)
GO
INSERT INTO dbo.CustomGenCodeValue
(
    CustomGenCodeId,
    BaseValue,
    LastValue,
    LastCode,
    TempValue,
    TempCode
)
SELECT CustomGenCodeId, '', LastValue, LastCode, TempValue, TempCode FROM dbo.CustomGenCode
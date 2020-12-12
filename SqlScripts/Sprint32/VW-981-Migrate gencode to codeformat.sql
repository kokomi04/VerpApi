USE MasterDB
GO
UPDATE CustomGenCode SET CodeFormat = CONCAT(Prefix,'%S_NUMBER%',Suffix)
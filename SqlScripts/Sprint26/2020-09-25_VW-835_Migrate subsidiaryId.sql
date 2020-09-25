DECLARE @SubsidiaryId INT
IF NOT EXISTS(SELECT 0 FROM OrganizationDB.dbo.Subsidiary)
BEGIN
	INSERT INTO OrganizationDB.dbo.Subsidiary
	(
		--SubsidiaryId - column value is auto-generated
		ParentSubsidiaryId,
		SubsidiaryCode,
		SubsidiaryName,
		[Address],
		TaxIdNo,
		PhoneNumber,
		Email,
		Fax,
		[Description],
		CreatedByUserId,
		CreatedDatetimeUtc,
		UpdatedByUserId,
		UpdatedDatetimeUtc,
		IsDeleted,
		DeletedDatetimeUtc
	)
	SELECT

		-- SubsidiaryId - int
		NULL, -- ParentSubsidiaryId - int
		N'', -- SubsidiaryCode - nvarchar
		CompanyName, -- SubsidiaryName - nvarchar
		Address, -- Address - nvarchar
		TaxIdNo, -- TaxIdNo - nvarchar
		PhoneNumber, -- PhoneNumber - nvarchar
		Email, -- Email - nvarchar
		N'', -- Fax - nvarchar
		N'', -- Description - nvarchar
		0, -- CreatedByUserId - int
		GETUTCDATE(), -- CreatedDatetimeUtc - datetime2
		0, -- UpdatedByUserId - int
		 GETUTCDATE(), -- UpdatedDatetimeUtc - datetime2
		0, -- IsDeleted - bit
		NULL -- DeletedDatetimeUtc - datetime2

	FROM OrganizationDB.dbo.BusinessInfo
END

SELECT TOP(1) @SubsidiaryId = SubsidiaryId FROM OrganizationDB.dbo.Subsidiary WHERE IsDeleted=0 ORDER BY OrganizationDB.dbo.Subsidiary.SubsidiaryId

SELECT @SubsidiaryId SubsidiaryId

UPDATE OrganizationDB.dbo.BusinessInfo SET SubsidiaryId = @SubsidiaryId

INSERT INTO OrganizationDB.dbo.EmployeeSubsidiary
(
    --EmployeeSubsidiaryId - column value is auto-generated
    UserId,
    SubsidiaryId,
    CreatedByUserId,
    CreatedDateTimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    DeletedDatetimeUtc,
    IsDeleted
)

SELECT
	e.UserId,
	@SubsidiaryId,
	0,
	GETUTCDATE(),
	0,
	GETUTCDATE(),
	NULL,
	0
FROM OrganizationDB.dbo.Employee e
WHERE NOT EXISTS(SELECT 0 FROM OrganizationDB.dbo.EmployeeSubsidiary s WHERE e.UserId = s.UserId AND s.SubsidiaryId = @SubsidiaryId)


--UPDATE AccountancyDB.dbo.AccountantConfig SET SubsidiaryId = @SubsidiaryId

UPDATE AccountancyDB.dbo.InputBill SET SubsidiaryId = @SubsidiaryId

UPDATE AccountancyDB.dbo.InputValueRow SET SubsidiaryId = @SubsidiaryId

UPDATE StockDB.dbo.InventoryConfig SET SubsidiaryId = @SubsidiaryId
UPDATE StockDB.dbo.Inventory SET SubsidiaryId = @SubsidiaryId
UPDATE StockDB.dbo.InventoryDetail SET SubsidiaryId = @SubsidiaryId
UPDATE StockDB.dbo.[Package] SET SubsidiaryId = @SubsidiaryId
UPDATE StockDB.dbo.Stock SET SubsidiaryId = @SubsidiaryId



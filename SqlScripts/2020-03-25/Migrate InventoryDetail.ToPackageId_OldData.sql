SELECT 
	id.InventoryDetailId, 
	id.ToPackageId OldToPackageId, 
	dp.ToPackageId NewToPackageId,
	p.PackageId OldPackageId
FROM dbo.InventoryDetailToPackage dp
	JOIN dbo.InventoryDetail id ON dp.InventoryDetailId = id.InventoryDetailId
	JOIN dbo.Package p ON id.ToPackageId = p.PackageId
WHERE id.ToPackageId <> dp.ToPackageId

UPDATE id
	SET id.ToPackageId = dp.ToPackageId
FROM dbo.InventoryDetailToPackage dp
	JOIN dbo.InventoryDetail id ON dp.InventoryDetailId = id.InventoryDetailId
	JOIN dbo.Package p ON id.ToPackageId = p.PackageId
WHERE id.ToPackageId <> dp.ToPackageId
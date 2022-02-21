USE StockDB;

--before update
SELECT
  p.ProductId,
  p.Coefficient,
  v.QuantityOrigin
FROM Product p
LEFT JOIN (SELECT
  s.ContainerId,
  s.ContainerTypeId,
  ld.ObjectId,
  ld.ObjectTypeId,
  ld.QuantityOrigin
FROM (SELECT
  r.*
FROM ManufacturingDB.dbo.ProductionStepLinkDataRole r
WHERE r.ProductionStepLinkDataId IN (SELECT
  r.ProductionStepLinkDataId
FROM ManufacturingDB.dbo.ProductionStepLinkDataRole r
GROUP BY r.ProductionStepLinkDataId
HAVING COUNT(*) = 1)
AND r.ProductionStepLinkDataRoleTypeId = 2) r
JOIN ManufacturingDB.dbo.ProductionStep s
  ON r.ProductionStepId = s.ProductionStepId
  AND s.ContainerTypeId = 1
  AND s.IsDeleted = 0
JOIN ManufacturingDB.dbo.ProductionStepLinkData ld
  ON r.ProductionStepLinkDataId = ld.ProductionStepLinkDataId
  AND ld.ObjectTypeId = 1
  AND ld.IsDeleted = 0
WHERE s.ContainerId = ld.ObjectId) v
  ON p.ProductId = v.ContainerId
WHERE p.IsDeleted = 0
AND v.QuantityOrigin IS NOT NULL
AND p.Coefficient != v.QuantityOrigin;

--mirgate coefficient
UPDATE p
SET p.Coefficient = v.QuantityOrigin
FROM Product p
LEFT JOIN (SELECT
  s.ContainerId,
  s.ContainerTypeId,
  ld.ObjectId,
  ld.ObjectTypeId,
  ld.QuantityOrigin
FROM (SELECT
  r.*
FROM ManufacturingDB.dbo.ProductionStepLinkDataRole r
WHERE r.ProductionStepLinkDataId IN (SELECT
  r.ProductionStepLinkDataId
FROM ManufacturingDB.dbo.ProductionStepLinkDataRole r
GROUP BY r.ProductionStepLinkDataId
HAVING COUNT(*) = 1)
AND r.ProductionStepLinkDataRoleTypeId = 2) r
JOIN ManufacturingDB.dbo.ProductionStep s
  ON r.ProductionStepId = s.ProductionStepId
  AND s.ContainerTypeId = 1
  AND s.IsDeleted = 0
JOIN ManufacturingDB.dbo.ProductionStepLinkData ld
  ON r.ProductionStepLinkDataId = ld.ProductionStepLinkDataId
  AND ld.ObjectTypeId = 1
  AND ld.IsDeleted = 0
WHERE s.ContainerId = ld.ObjectId) v
  ON p.ProductId = v.ContainerId
WHERE p.IsDeleted = 0
AND v.QuantityOrigin IS NOT NULL
AND p.Coefficient != v.QuantityOrigin;

--after update
SELECT
  p.ProductId,
  p.Coefficient,
  v.QuantityOrigin
FROM Product p
LEFT JOIN (SELECT
  s.ContainerId,
  s.ContainerTypeId,
  ld.ObjectId,
  ld.ObjectTypeId,
  ld.QuantityOrigin
FROM (SELECT
  r.*
FROM ManufacturingDB.dbo.ProductionStepLinkDataRole r
WHERE r.ProductionStepLinkDataId IN (SELECT
  r.ProductionStepLinkDataId
FROM ManufacturingDB.dbo.ProductionStepLinkDataRole r
GROUP BY r.ProductionStepLinkDataId
HAVING COUNT(*) = 1)
AND r.ProductionStepLinkDataRoleTypeId = 2) r
JOIN ManufacturingDB.dbo.ProductionStep s
  ON r.ProductionStepId = s.ProductionStepId
  AND s.ContainerTypeId = 1
  AND s.IsDeleted = 0
JOIN ManufacturingDB.dbo.ProductionStepLinkData ld
  ON r.ProductionStepLinkDataId = ld.ProductionStepLinkDataId
  AND ld.ObjectTypeId = 1
  AND ld.IsDeleted = 0
WHERE s.ContainerId = ld.ObjectId) v
  ON p.ProductId = v.ContainerId
WHERE p.IsDeleted = 0
AND v.QuantityOrigin IS NOT NULL
AND p.Coefficient != v.QuantityOrigin;
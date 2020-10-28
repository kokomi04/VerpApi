DECLARE @CategoryDataModuleId INT = 702;
DECLARE @InputTypeModuleId INT = 802;
DECLARE @SalesBillModuleId INT = 902;

DECLARE @CategoryDataObjectTypeId INT = 32;
DECLARE @InputTypeObjectTypeId INT = 34;
DECLARE @SalesBillObjectTypeId INT = 53;


DECLARE @StockObjectTypeId INT = 9

INSERT INTO dbo.RolePermission
(
    RoleId,
    ModuleId,
    Permission,
    CreatedDatetimeUtc
)
SELECT  r.RoleId,
		m.ModuleId,
		2147483647,
		GETDATE()
	FROM
	dbo.Module AS m,
	dbo.[Role] AS r
	WHERE 
	(r.RoleTypeId = 1 OR r.IsEditable = 0)
	AND m.ModuleId NOT IN
	(
		SELECT ModuleId FROM dbo.RolePermission WHERE RoleId = r.RoleId
	);
--Stock
INSERT INTO [dbo].[RoleDataPermission]
           ([RoleId]
           ,[ObjectTypeId]
           ,[ObjectId])
          
	SELECT  r.RoleId,
		@StockObjectTypeId,
		c.StockId
	FROM
	StockDB.dbo.[Stock] AS c,
	dbo.[Role] AS r
	WHERE 
	(r.RoleTypeId = 1 OR r.IsEditable = 0)
	AND c.IsDeleted = 0
	AND NOT EXISTS
	(
		SELECT 0 FROM dbo.RoleDataPermission WHERE RoleId = r.RoleId AND ObjectTypeId = @StockObjectTypeId AND ObjectId = c.StockId
	);
--category

;WITH tmp AS (
	SELECT t.CategoryId
		
		FROM Category t 
		WHERE t.IsDeleted = 0
)

INSERT INTO dbo.RolePermission
(
    RoleId,
    ModuleId,
	ObjectTypeId,
	ObjectId,
    Permission,
	JsonActionIds,
    CreatedDatetimeUtc
)
SELECT  r.RoleId,
		@CategoryDataModuleId,
		@CategoryDataObjectTypeId,
		m.CategoryId,
		2147483647,
		NULL,
		GETDATE()
	FROM
	tmp AS m,
	dbo.[Role] AS r
	WHERE 
	(r.RoleTypeId = 1 OR r.IsEditable = 0)
	AND NOT EXISTS
	(
		SELECT 0 FROM dbo.RolePermission p WHERE p.RoleId = r.RoleId AND p.ModuleId = @CategoryDataModuleId AND p.ObjectTypeId = @CategoryDataObjectTypeId AND p.ObjectId = m.CategoryId
	)

--INPUT type

;WITH tmp AS (
	SELECT 
	t.InputTypeId, 
	replace(replace (t.Actions, '{"InputActionId":',''),'}','') ActionIds

	FROM
	(
		SELECT t.InputTypeId, 
		(SELECT InputActionId FROM AccountancyDB.dbo.InputAction a WHERE  t.InputTypeId = a.InputTypeId AND a.IsDeleted = 0 FOR JSON PATH) Actions

		FROM AccountancyDB.dbo.InputType t 
		WHERE t.IsDeleted = 0		
	) t
)

INSERT INTO dbo.RolePermission
(
    RoleId,
    ModuleId,
	ObjectTypeId,
	ObjectId,
    Permission,
	JsonActionIds,
    CreatedDatetimeUtc
)
SELECT  r.RoleId,
		@InputTypeModuleId,
		@InputTypeObjectTypeId,
		m.InputTypeId,
		2147483647,
		m.ActionIds,
		GETDATE()
	FROM
	tmp AS m,
	dbo.[Role] AS r
	WHERE 
	(r.RoleTypeId = 1 OR r.IsEditable = 0)
	AND NOT EXISTS
	(
		SELECT 0 FROM dbo.RolePermission p WHERE p.RoleId = r.RoleId AND p.ModuleId = @InputTypeModuleId AND p.ObjectTypeId = @InputTypeObjectTypeId AND p.ObjectId = m.InputTypeId
	)
	
	
;WITH tmp AS (
	SELECT 
	t.VoucherTypeId, 
	replace(replace (t.Actions, '{"VoucherActionId":',''),'}','') ActionIds

	FROM
	(
		SELECT t.VoucherTypeId, 
		(SELECT VoucherActionId FROM PurchaseOrderDB.dbo.VoucherAction a WHERE  t.VoucherTypeId = a.VoucherTypeId AND a.IsDeleted = 0 FOR JSON PATH) Actions

		FROM PurchaseOrderDB.dbo.VoucherType t 
		WHERE t.IsDeleted = 0		
	) t
)

INSERT INTO dbo.RolePermission
(
    RoleId,
    ModuleId,
	ObjectTypeId,
	ObjectId,
    Permission,
	JsonActionIds,
    CreatedDatetimeUtc
)
SELECT  r.RoleId,
		@SalesBillModuleId,
		@SalesBillObjectTypeId,
		m.VoucherTypeId,
		2147483647,
		m.ActionIds,
		GETDATE()
	FROM
	tmp AS m,
	dbo.[Role] AS r
	WHERE 
	(r.RoleTypeId = 1 OR r.IsEditable = 0)
	AND NOT EXISTS
	(
		SELECT 0 FROM dbo.RolePermission p WHERE p.RoleId = r.RoleId AND p.ModuleId = @SalesBillModuleId AND p.ObjectTypeId = @SalesBillObjectTypeId AND p.ObjectId = m.VoucherTypeId
	)
	
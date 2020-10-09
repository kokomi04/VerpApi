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
	)
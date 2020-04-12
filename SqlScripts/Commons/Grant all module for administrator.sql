INSERT INTO dbo.RolePermission
(
    RoleId,
    ModuleId,
    Permission,
    CreatedDatetimeUtc
)
SELECT 1,
		m.ModuleId,
		2147483647,
		GETDATE()
	FROM
	dbo.Module AS m
	WHERE m.ModuleId NOT IN
	(
	SELECT ModuleId FROM dbo.RolePermission WHERE RoleId=1
	)
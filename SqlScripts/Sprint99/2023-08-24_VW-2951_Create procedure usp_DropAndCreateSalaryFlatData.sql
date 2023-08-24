USE OrganizationDB
GO

Alter PROCEDURE usp_DropAndCreateSalaryFlatData
	@SubsidiaryId INT
AS
BEGIN
	DECLARE @SubsidiaryCode NVARCHAR(128)
	SET @SubsidiaryCode = (SELECT SubsidiaryCode FROM Subsidiary where SubsidiaryId = @SubsidiaryId)
	-- Drop view
	IF OBJECT_ID('v_EmployeeSalary_'+ @SubsidiaryCode +'', 'V') IS NOT NULL
	BEGIN
		DECLARE @DropQuery NVARCHAR(MAX)
		SET @DropQuery = 'DROP VIEW v_EmployeeSalary_'+ @SubsidiaryCode +''
		EXEC sp_executesql @DropQuery;
	END
	-- Create view
	DECLARE @Columns NVARCHAR(MAX)
	SELECT @Columns = COALESCE(@Columns + ', ', '') + QUOTENAME(SalaryFieldName)
	FROM SalaryField Where IsDeleted = 0 AND SubsidiaryId = @SubsidiaryId ORDER BY SortOrder
	DECLARE @CreateQuery NVARCHAR(MAX)
	SET @CreateQuery = '
	CREATE VIEW v_EmployeeSalary_'+ @SubsidiaryCode +' AS
		SELECT *
		FROM
		(
			SELECT 
				se.SalaryEmployeeId,
				se.EmployeeId,
				se.SalaryPeriodId,
				se.SalaryGroupId,
				sf.SalaryFieldName,
				sev.Value
			FROM 
				SalaryEmployee se
			JOIN 
				SalaryEmployeeValue sev ON se.SalaryEmployeeId = sev.SalaryEmployeeId
			JOIN 
				SalaryField sf ON sev.SalaryFieldId = sf.SalaryFieldId
			WHERE se.IsDeleted = 0 AND sf.IsDeleted = 0 
		) as sfd
		PIVOT
		(
			MAX(sfd.Value)
			FOR sfd.SalaryFieldName IN (' + @Columns + ')
		) As SalaryFlatData'

	EXEC sp_executesql @CreateQuery;
END;
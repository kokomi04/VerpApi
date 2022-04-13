declare @InputTypeId int;
declare @FieldCheckStepString varchar(max);
declare @FieldApprovalStepString varchar(max);

declare @CheckStepTypeId int;
declare @ApprovalStepTypeId int;

SET @InputTypeId = 34;

SET @CheckStepTypeId = 2;
SET @ApprovalStepTypeId = 1;

--Lấy danh sách fieldId cho buoc kiem tra
Select @FieldCheckStepString = concat(@FieldCheckStepString+',',f.InputFieldId) from AccountancyDB.dbo.InputField f where (f.ObjectApprovalStepTypeId & @CheckStepTypeId) = @CheckStepTypeId;
SET @FieldCheckStepString = concat('[',@FieldCheckStepString,']');

--Lấy danh sách fieldId cho buoc duyet
Select @FieldApprovalStepString = concat(@FieldApprovalStepString+',',f.InputFieldId) from AccountancyDB.dbo.InputField f where (f.ObjectApprovalStepTypeId & @ApprovalStepTypeId) = @ApprovalStepTypeId;
SET @FieldApprovalStepString = concat('[',@FieldApprovalStepString,']');

UPDATE OrganizationDB.dbo.ObjectApprovalStep SET ObjectFieldEnable = @FieldCheckStepString 
where ObjectTypeId = @InputTypeId AND IsEnable = 0 AND ObjectApprovalStepTypeId = @CheckStepTypeId;

UPDATE OrganizationDB.dbo.ObjectApprovalStep SET ObjectFieldEnable = @FieldApprovalStepString 
where ObjectTypeId = @InputTypeId AND IsEnable = 0 AND ObjectApprovalStepTypeId = @ApprovalStepTypeId;


use MasterDB;
Go
declare @TempTable Table (CustomGenCodeId int, ObjectTypeId int)

insert into CustomGenCode (CustomGenCodeName, CodeLength, Prefix, Suffix, Seperator, DateFormat,
LastValue, LastCode, IsActived, IsDeleted, Description, TempValue, SortOrder)
output inserted.CustomGenCodeId, inserted.TempValue into @TempTable(CustomGenCodeId, ObjectTypeId)
select a.ObjectTypeName as CustomGenCodeName, a.CodeLength, a.Prefix, a.Suffix, a.Seperator, a.DateFormat,
a.LastValue, a.LastCode, a.IsActived, a.IsDeleted, a.ObjectTypeName as Description, a.ObjectTypeId as TempValue, 1 as SortOrder
from ObjectGenCode a WHERE a.IsDeleted != 1;

insert into ObjectCustomGenCodeMapping (CustomGenCodeId, ObjectTypeId, ObjectId, UpdatedUserId)
select b.CustomGenCodeId, b.ObjectTypeId, 0 as ObjectId, 0 UpdatedUserId from @TempTable b;
Go
USE PurchaseOrderDB
GO
/*
Run this script on:

172.16.16.102\STD.PurchaseOrderDB    -  This database will be modified

to synchronize it with:

103.21.149.93.PurchaseOrderDB

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/6/2023 9:50:59 AM

*/
		
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
GO
SET DATEFORMAT YMD
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL Serializable
GO
BEGIN TRANSACTION

PRINT(N'Drop constraints from [dbo].[VoucherAreaField]')
ALTER TABLE [dbo].[VoucherAreaField] NOCHECK CONSTRAINT [FK_VoucherAreaField_VoucherArea]
ALTER TABLE [dbo].[VoucherAreaField] NOCHECK CONSTRAINT [FK_VoucherAreaField_VoucherField]
ALTER TABLE [dbo].[VoucherAreaField] NOCHECK CONSTRAINT [FK_VoucherAreaField_VoucherType]

PRINT(N'Update rows in [dbo].[VoucherAreaField]')
UPDATE [dbo].[VoucherAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-06 07:02:39.8870627', [FiltersName]=N'', [RequireFiltersName]=N'' WHERE [VoucherAreaFieldId] = 59
UPDATE [dbo].[VoucherAreaField] SET [DefaultValue]=N'return ($bill[''HT''].rows[0].loai_tien.titleValue == ''VND'' ||
 $bill[''HT''].rows[0].loai_tien.titleValue == null || $bill[''HT''].rows[0].loai_tien.value == undefined) ?  ''1'' : ''23000'';
 console.log($bill[''HT''].rows[0].ty_gia);', [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-06 07:42:41.5890302' WHERE [VoucherAreaFieldId] = 174
UPDATE [dbo].[VoucherAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-07-06 03:45:18.4099125' WHERE [VoucherAreaFieldId] = 198
UPDATE [dbo].[VoucherAreaField] SET [DefaultValue]=N'return ($bill[''HT''].rows[0].loai_tien.titleValue == ''VND'' ||
 $bill[''HT''].rows[0].loai_tien.titleValue == null) ?  ''1'' : ''23000'';
 console.log($bill[''HT''].rows[0].ty_gia);', [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-06 07:46:11.9095042' WHERE [VoucherAreaFieldId] = 244
UPDATE [dbo].[VoucherAreaField] SET [DefaultValue]=N'return ($bill[''HT''].rows[0].loai_tien.titleValue == ''VND'' ||
 $bill[''HT''].rows[0].loai_tien.titleValue == null) ?  ''1'' : ''23000'';
 console.log($bill[''HT''].rows[0].ty_gia);', [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-07-06 07:46:37.8647000' WHERE [VoucherAreaFieldId] = 275
UPDATE [dbo].[VoucherAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-08-24 11:08:05.3200104' WHERE [VoucherAreaFieldId] = 502
PRINT(N'Operation applied to 6 rows out of 6')

PRINT(N'Add constraints to [dbo].[VoucherAreaField]')
ALTER TABLE [dbo].[VoucherAreaField] WITH CHECK CHECK CONSTRAINT [FK_VoucherAreaField_VoucherArea]
ALTER TABLE [dbo].[VoucherAreaField] WITH CHECK CHECK CONSTRAINT [FK_VoucherAreaField_VoucherField]
ALTER TABLE [dbo].[VoucherAreaField] WITH CHECK CHECK CONSTRAINT [FK_VoucherAreaField_VoucherType]
COMMIT TRANSACTION
GO

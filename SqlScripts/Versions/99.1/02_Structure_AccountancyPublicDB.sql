USE AccountancyPublicDB
GO
/*
Run this script on:

        172.16.16.102\STD.AccountancyPublicDB    -  This database will be modified

to synchronize it with:

        103.21.149.93.AccountancyPublicDB

You are recommended to back up your database before running this script

Script created by SQL Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/6/2023 9:17:59 AM

*/
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL Serializable
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping types'
GO
DROP TYPE [dbo].[InputTableType]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating types'
GO
CREATE TYPE [dbo].[InputTableType] AS TABLE
(
[ngay_ct] [datetime2] NULL,
[so_ct] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[mau_hd] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[seri_hd] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ngay_hd] [datetime2] NULL,
[attachment] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[loai_tien] [int] NULL,
[ty_gia] [decimal] (18, 5) NULL,
[tk_no0] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tk_co0] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ong_ba] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[dia_chi] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[bo_phan] [int] NULL,
[kh0] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[kh_co0] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[stt] [decimal] (18, 5) NULL,
[noi_dung] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[vthhtp] [int] NULL,
[so_luong] [decimal] (18, 5) NULL,
[don_gia0] [decimal] (18, 5) NULL,
[ngoai_te0] [decimal] (18, 3) NULL,
[vnd0] [decimal] (18, 0) NULL,
[thue_suat_vat] [int] NULL,
[ghi_chu] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[kho] [int] NULL,
[kho_lc] [int] NULL,
[khe_uoc_vay] [int] NULL,
[tk_thu_kbnn] [int] NULL,
[ma_chuong_nsnn] [int] NULL,
[ma_muc_nsnn] [int] NULL,
[cong_trinh] [int] NULL,
[phan_xuong] [int] NULL,
[khoan_muc_cp] [int] NULL,
[khoan_muc_tc] [int] NULL,
[po_code] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[order_code] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ma_lsx] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[dien_giai] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[thue_suat_xnk] [decimal] (18, 5) NULL,
[vnd3] [decimal] (18, 0) NULL,
[vnd1] [decimal] (18, 0) NULL,
[sl_po] [decimal] (18, 5) NULL,
[sl_od] [decimal] (18, 5) NULL,
[sl_ycvt] [decimal] (18, 5) NULL,
[tk_no1] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tk_co1] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tk_no2] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tk_co2] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tk_no3] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tk_co3] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tscd] [int] NULL,
[kh1] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[kh3] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tknh] [int] NULL,
[tknh_kh] [int] NULL,
[ky_hieu_hd] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ma_link_hd] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[constrain_ty_gia] [bit] NULL,
[vthhtp_dvt2] [int] NULL,
[so_luong_dv2] [decimal] (18, 5) NULL,
[don_gia_dv2_0] [decimal] (18, 5) NULL,
[dv_ql_hc] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[constrain_sl_dg] [bit] NULL,
[vnd2] [decimal] (18, 0) NULL,
[tk_no4] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tk_co4] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[vnd4] [int] NULL,
[sum_vnd0] [decimal] (18, 0) NULL,
[sum_vnd1] [decimal] (18, 0) NULL,
[sum_vnd2] [decimal] (18, 0) NULL,
[sum_vnd3] [decimal] (18, 0) NULL,
[sum_vnd4] [decimal] (18, 0) NULL,
[sum_vnd5] [decimal] (18, 0) NULL,
[test01] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[don_gia1] [decimal] (18, 0) NULL,
[don_gia2] [decimal] (18, 0) NULL,
[don_gia3] [decimal] (18, 0) NULL,
[don_gia4] [decimal] (18, 0) NULL,
[don_gia5] [decimal] (18, 0) NULL,
[don_gia_dvt2_1] [decimal] (18, 0) NULL,
[don_gia_dvt2_2] [decimal] (18, 0) NULL,
[don_gia_dvt2_3] [decimal] (18, 0) NULL,
[don_gia_dvt2_4] [decimal] (18, 0) NULL,
[Not_VAT] [bit] NULL,
[CensorStatusId] [int] NULL,
[CheckStatusId] [int] NULL,
[sl_lsx] [int] NULL,
[CensorUserId] [int] NULL,
[CensorDatetimeUtc] [datetime2] NULL,
[cptt] [int] NULL,
[kh4] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Mat_hang_VAT] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[kh_co1] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ngoai_te1] [decimal] (18, 4) NULL,
[So_ct_goc] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tieu_thuc_pb] [int] NULL,
[so_tien] [decimal] (18, 0) NULL,
[ref_row] [bigint] NULL,
[sumMeasurement] [decimal] (32, 12) NULL,
[sumNetWeight] [decimal] (32, 12) NULL,
[chi_phi] [nvarchar] (4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tai_khoan_no] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tien_thue] [decimal] (18, 0) NULL,
[sourceBillCodes] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[InputValueRow]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[InputValueRow] ADD
[IsGeneratedEntry] [bit] NOT NULL CONSTRAINT [DF_InputValueRow_IsGenerateEntry] DEFAULT ((0)),
[IsIgnoredEntry] [bit] NOT NULL CONSTRAINT [DF_InputValueRow_IsIgnoreEntry] DEFAULT ((0)),
[tieu_thuc_pb] [int] NULL,
[so_tien] [decimal] (18, 0) NULL,
[ref_row] [bigint] NULL,
[sumMeasurement] [decimal] (32, 12) NULL,
[sumNetWeight] [decimal] (32, 12) NULL,
[chi_phi] [nvarchar] (4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tai_khoan_no] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tien_thue] [decimal] (18, 0) NULL,
[sourceBillCodes] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[InputBill]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[InputBill] ADD
[ParentInputBill_F_Id] [bigint] NULL,
[HasChildren] [bit] NOT NULL CONSTRAINT [DF_InputBill_HasChildren] DEFAULT ((0))
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[InputType]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[InputType] ADD
[IsParentAllowcation] [bit] NOT NULL CONSTRAINT [DF_InputType_IsAllowcationBill] DEFAULT ((0)),
[DataAllowcationInputTypeIds] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ResultAllowcationInputTypeId] [int] NULL,
[CalcResultAllowcationSqlQuery] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[InputArea]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[InputArea] ADD
[IsGeneratedArea] [bit] NOT NULL CONSTRAINT [DF_InputArea_IsGeneratedArea] DEFAULT ((0))
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[InputAreaField]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[InputAreaField] ADD
[FiltersName] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[RequireFiltersName] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IsPivotAllowcation] [bit] NOT NULL CONSTRAINT [DF_InputAreaField_IsPivotAllowcation] DEFAULT ((0)),
[IsReadOnly] [bit] NOT NULL CONSTRAINT [DF_InputAreaField_IsReadOnly] DEFAULT ((0)),
[IsPivotValue] [bit] NOT NULL CONSTRAINT [DF_InputAreaField_IsPivotValue] DEFAULT ((0))
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[InputField]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[InputField] ADD
[SqlValue] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[InputBillAllocation]'
GO
CREATE TABLE [dbo].[InputBillAllocation]
(
[Parent_InputBill_F_Id] [bigint] NOT NULL,
[DataAllowcation_BillCode] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_InputBillAllocation] on [dbo].[InputBillAllocation]'
GO
ALTER TABLE [dbo].[InputBillAllocation] ADD CONSTRAINT [PK_InputBillAllocation] PRIMARY KEY CLUSTERED ([Parent_InputBill_F_Id], [DataAllowcation_BillCode])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[InputValueRowSourceBillCode]'
GO
CREATE TABLE [dbo].[InputValueRowSourceBillCode]
(
[InputValueRow_F_Id] [bigint] NOT NULL,
[SourceBillCode] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_InputValueRowSourceBillCode] on [dbo].[InputValueRowSourceBillCode]'
GO
ALTER TABLE [dbo].[InputValueRowSourceBillCode] ADD CONSTRAINT [PK_InputValueRowSourceBillCode] PRIMARY KEY CLUSTERED ([InputValueRow_F_Id], [SourceBillCode])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[InputBillAllocation]'
GO
ALTER TABLE [dbo].[InputBillAllocation] ADD CONSTRAINT [FK_InputBillAllocation_InputBill] FOREIGN KEY ([Parent_InputBill_F_Id]) REFERENCES [dbo].[InputBill] ([F_Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[InputBill]'
GO
ALTER TABLE [dbo].[InputBill] ADD CONSTRAINT [FK_InputBill_InputBill] FOREIGN KEY ([ParentInputBill_F_Id]) REFERENCES [dbo].[InputBill] ([F_Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[InputType]'
GO
ALTER TABLE [dbo].[InputType] ADD CONSTRAINT [FK_InputType_InputType] FOREIGN KEY ([ResultAllowcationInputTypeId]) REFERENCES [dbo].[InputType] ([InputTypeId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
-- This statement writes to the SQL Server Log so SQL Monitor can show this deployment.
IF HAS_PERMS_BY_NAME(N'sys.xp_logevent', N'OBJECT', N'EXECUTE') = 1
BEGIN
    DECLARE @databaseName AS nvarchar(2048), @eventMessage AS nvarchar(2048)
    SET @databaseName = REPLACE(REPLACE(DB_NAME(), N'\', N'\\'), N'"', N'\"')
    SET @eventMessage = N'Redgate SQL Compare: { "deployment": { "description": "Redgate SQL Compare deployed to ' + @databaseName + N'", "database": "' + @databaseName + N'" }}'
    EXECUTE sys.xp_logevent 55000, @eventMessage
END
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO

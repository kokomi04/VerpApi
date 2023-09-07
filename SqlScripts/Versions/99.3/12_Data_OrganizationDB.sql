USE OrganizationDB
GO
/*
Run this script on:

172.16.16.102\STD.OrganizationDB    -  This database will be modified

to synchronize it with:

103.21.149.93.OrganizationDB

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 14.7.8.21163 from Red Gate Software Ltd at 9/6/2023 9:49:55 AM

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

PRINT(N'Drop constraints from [dbo].[HrField]')
ALTER TABLE [dbo].[HrField] NOCHECK CONSTRAINT [FK_HrField_HrArea]

PRINT(N'Update row in [dbo].[HrField]')
UPDATE [dbo].[HrField] SET [DataSize]=32, [UpdatedDatetimeUtc]='2023-08-10 03:48:31.3107917', [SqlValue]=N'dbo.afn_MonthWorks(ngay_bat_dau_lam_viec)' WHERE [HrFieldId] = 939

PRINT(N'Update rows in [dbo].[HrAreaField]')
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-24 10:02:50.8468584' WHERE [HrAreaFieldId] = 6
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8857299' WHERE [HrAreaFieldId] = 9
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8857855' WHERE [HrAreaFieldId] = 10
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8858452' WHERE [HrAreaFieldId] = 11
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=208, [UpdatedDatetimeUtc]='2023-08-25 10:55:20.7003293' WHERE [HrAreaFieldId] = 12
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8859585' WHERE [HrAreaFieldId] = 13
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8860136' WHERE [HrAreaFieldId] = 14
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8860715' WHERE [HrAreaFieldId] = 15
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8861263' WHERE [HrAreaFieldId] = 16
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8861816' WHERE [HrAreaFieldId] = 17
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8862397' WHERE [HrAreaFieldId] = 18
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8862951' WHERE [HrAreaFieldId] = 19
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8863533' WHERE [HrAreaFieldId] = 20
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8864093' WHERE [HrAreaFieldId] = 22
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8864672' WHERE [HrAreaFieldId] = 23
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8865225' WHERE [HrAreaFieldId] = 24
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8865805' WHERE [HrAreaFieldId] = 25
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8866358' WHERE [HrAreaFieldId] = 26
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8866929' WHERE [HrAreaFieldId] = 27
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8867569' WHERE [HrAreaFieldId] = 28
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8868162' WHERE [HrAreaFieldId] = 29
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8868715' WHERE [HrAreaFieldId] = 30
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8869294' WHERE [HrAreaFieldId] = 31
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8869848' WHERE [HrAreaFieldId] = 32
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8870453' WHERE [HrAreaFieldId] = 33
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8871008' WHERE [HrAreaFieldId] = 34
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8873830' WHERE [HrAreaFieldId] = 46
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8873276' WHERE [HrAreaFieldId] = 47
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8872693' WHERE [HrAreaFieldId] = 50
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8872140' WHERE [HrAreaFieldId] = 51
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8871585' WHERE [HrAreaFieldId] = 52
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8874419' WHERE [HrAreaFieldId] = 55
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8874974' WHERE [HrAreaFieldId] = 56
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8875564' WHERE [HrAreaFieldId] = 583
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8876118' WHERE [HrAreaFieldId] = 584
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8876697' WHERE [HrAreaFieldId] = 585
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8877252', [FiltersName]=N'', [RequireFiltersName]=N'' WHERE [HrAreaFieldId] = 782
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8877833' WHERE [HrAreaFieldId] = 783
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8878385' WHERE [HrAreaFieldId] = 784
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8878965' WHERE [HrAreaFieldId] = 786
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8879519' WHERE [HrAreaFieldId] = 787
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8880100' WHERE [HrAreaFieldId] = 789
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8880654' WHERE [HrAreaFieldId] = 796
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8881336' WHERE [HrAreaFieldId] = 797
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8881900' WHERE [HrAreaFieldId] = 798
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8882491' WHERE [HrAreaFieldId] = 799
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8883051' WHERE [HrAreaFieldId] = 800
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8883639', [FiltersName]=N'HSNS_tham chiếu', [RequireFiltersName]=N'HSNS_BỘ phận bắt buộc' WHERE [HrAreaFieldId] = 804
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8884204' WHERE [HrAreaFieldId] = 809
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8884762' WHERE [HrAreaFieldId] = 810
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8886530' WHERE [HrAreaFieldId] = 840
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8885366' WHERE [HrAreaFieldId] = 848
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8885933' WHERE [HrAreaFieldId] = 849
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8887100' WHERE [HrAreaFieldId] = 852
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8887694' WHERE [HrAreaFieldId] = 868
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-24 10:02:50.8469102' WHERE [HrAreaFieldId] = 870
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8888857' WHERE [HrAreaFieldId] = 877
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8889421' WHERE [HrAreaFieldId] = 890
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8890037' WHERE [HrAreaFieldId] = 891
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8890601' WHERE [HrAreaFieldId] = 892
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8891212' WHERE [HrAreaFieldId] = 895
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8891770' WHERE [HrAreaFieldId] = 898
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8892358' WHERE [HrAreaFieldId] = 900
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8892930' WHERE [HrAreaFieldId] = 901
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8893528' WHERE [HrAreaFieldId] = 902
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8894102' WHERE [HrAreaFieldId] = 903
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8895306' WHERE [HrAreaFieldId] = 904
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8894720' WHERE [HrAreaFieldId] = 905
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8897750' WHERE [HrAreaFieldId] = 906
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8895893' WHERE [HrAreaFieldId] = 907
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8896537' WHERE [HrAreaFieldId] = 908
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8897116' WHERE [HrAreaFieldId] = 909
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8898337' WHERE [HrAreaFieldId] = 910
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8898973' WHERE [HrAreaFieldId] = 911
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8899531' WHERE [HrAreaFieldId] = 914
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8900695' WHERE [HrAreaFieldId] = 918
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8901282' WHERE [HrAreaFieldId] = 919
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8901840' WHERE [HrAreaFieldId] = 920
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8902427' WHERE [HrAreaFieldId] = 921
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8900131' WHERE [HrAreaFieldId] = 922
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8902982' WHERE [HrAreaFieldId] = 926
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8903569' WHERE [HrAreaFieldId] = 927
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8904132' WHERE [HrAreaFieldId] = 937
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8904730' WHERE [HrAreaFieldId] = 938
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8905299' WHERE [HrAreaFieldId] = 939
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8905901' WHERE [HrAreaFieldId] = 940
UPDATE [dbo].[HrAreaField] SET [UpdatedByUserId]=217, [UpdatedDatetimeUtc]='2023-08-23 08:09:57.8906458' WHERE [HrAreaFieldId] = 944
PRINT(N'Operation applied to 87 rows out of 87')

PRINT(N'Add constraints to [dbo].[HrField]')
ALTER TABLE [dbo].[HrField] WITH CHECK CHECK CONSTRAINT [FK_HrField_HrArea]
COMMIT TRANSACTION
GO

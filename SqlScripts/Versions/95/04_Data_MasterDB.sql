USE MasterDB
GO
/*
Run this script on:

172.16.16.102\STD.MasterDB    -  This database will be modified

to synchronize it with:

103.21.149.93.MasterDB

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 14.7.8.21163 from Red Gate Software Ltd at 7/4/2023 12:21:24 PM

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

PRINT(N'Drop constraints from [dbo].[PrintConfigStandardModuleType]')
ALTER TABLE [dbo].[PrintConfigStandardModuleType] NOCHECK CONSTRAINT [FK_PrintConfigStandardModuleType_PrintConfigStandard]

PRINT(N'Drop constraints from [dbo].[ModuleApiEndpointMapping]')
ALTER TABLE [dbo].[ModuleApiEndpointMapping] NOCHECK CONSTRAINT [FK_ModuleApiEndpointMapping_ApiEndpoint]
ALTER TABLE [dbo].[ModuleApiEndpointMapping] NOCHECK CONSTRAINT [FK_ModuleApiEndpointMapping_Module]

PRINT(N'Drop constraints from [dbo].[Category]')
ALTER TABLE [dbo].[Category] NOCHECK CONSTRAINT [FK_Category_CategoryGroup]

PRINT(N'Drop constraint FK_CategoryField_Category from [dbo].[CategoryField]')
ALTER TABLE [dbo].[CategoryField] NOCHECK CONSTRAINT [FK_CategoryField_Category]

PRINT(N'Drop constraint FK_FK_ReportTypeView_ReportTypeView_Category from [dbo].[CategoryView]')
ALTER TABLE [dbo].[CategoryView] NOCHECK CONSTRAINT [FK_FK_ReportTypeView_ReportTypeView_Category]

PRINT(N'Drop constraint FK_OutSideDataConfig_Category from [dbo].[OutSideDataConfig]')
ALTER TABLE [dbo].[OutSideDataConfig] NOCHECK CONSTRAINT [FK_OutSideDataConfig_Category]

PRINT(N'Drop constraints from [dbo].[ApiEndpoint]')
ALTER TABLE [dbo].[ApiEndpoint] NOCHECK CONSTRAINT [FK_ApiEndpoint_Action]
ALTER TABLE [dbo].[ApiEndpoint] NOCHECK CONSTRAINT [FK_ApiEndpoint_Method]

PRINT(N'Drop constraint FK_ActionButtonBillType_ActionButton from [dbo].[ActionButtonBillType]')
ALTER TABLE [dbo].[ActionButtonBillType] NOCHECK CONSTRAINT [FK_ActionButtonBillType_ActionButton]

PRINT(N'Delete rows from [dbo].[ModuleApiEndpointMapping]')
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 3
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 4
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 5
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 6
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 7
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 8
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 9
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 10
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 11
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 12
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 13
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 14
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 15
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 16
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 17
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 18
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 19
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 20
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 21
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 22
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 23
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 24
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 25
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 26
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 27
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 28
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 29
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 30
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 31
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 32
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 33
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 34
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 35
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 36
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 37
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 38
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 39
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 40
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 41
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 42
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 43
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 44
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 45
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 46
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 47
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 48
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 49
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 50
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 51
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 52
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 53
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 54
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 55
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 56
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 57
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 58
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 59
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 60
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 61
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 62
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 63
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 64
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 65
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 66
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 67
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 68
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 69
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 70
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 71
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 72
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 73
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 74
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 75
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 76
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 77
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 78
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 79
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 80
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 81
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 82
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 83
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 84
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 85
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 86
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 87
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 88
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 89
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 90
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 91
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 92
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 93
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 94
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 95
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 96
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 97
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 98
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 99
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 100
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 101
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 102
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 103
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 104
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 105
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 106
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 107
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 108
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 109
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 110
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 111
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 112
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 113
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 114
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 115
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 116
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 117
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 118
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 119
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 120
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 121
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 122
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 123
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 124
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 125
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 126
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 127
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 128
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 129
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 130
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 131
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 132
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 133
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 134
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 135
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 136
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 137
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 138
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 139
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 140
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 141
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 142
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 143
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 144
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 145
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 146
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 147
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 148
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 149
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 150
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 151
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 152
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 153
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 154
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 155
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 156
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 157
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 158
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 159
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 160
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 161
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 162
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 163
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 164
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 165
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 166
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 167
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 168
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 169
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 170
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 171
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 172
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 173
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 174
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 175
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 176
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 177
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 178
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 179
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 180
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 181
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 182
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 183
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 184
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 185
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 186
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 187
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 188
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 189
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 190
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 191
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 192
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 193
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 194
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 195
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 196
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 197
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 198
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 199
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 200
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 201
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 202
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 203
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 204
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 205
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 206
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 207
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 208
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 209
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 210
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 211
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 212
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 213
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 214
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 215
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 216
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 217
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 218
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 219
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 220
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 221
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 222
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 223
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 224
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 225
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 226
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 227
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 228
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 229
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 230
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 231
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 232
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 233
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 234
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 235
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 236
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 237
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 238
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 239
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 240
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 241
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 242
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 243
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 244
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 245
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 246
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 247
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 248
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 249
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 250
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 251
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 252
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 253
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 254
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 255
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 256
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 257
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 258
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 259
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 260
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 261
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 262
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 263
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 264
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 265
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 266
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 267
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 268
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 269
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 270
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 271
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 272
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 273
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 274
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 275
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 276
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 277
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 278
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 279
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 280
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 281
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 282
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 283
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 284
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 285
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 286
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 287
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 288
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 289
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 290
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 291
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 292
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 293
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 294
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 295
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 296
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 297
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 298
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 299
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 300
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 301
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 302
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 303
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 304
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 305
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 306
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 307
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 308
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 309
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 310
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 311
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 312
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 313
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 314
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 315
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 316
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 317
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 318
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 319
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 320
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 321
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 322
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 323
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 324
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 325
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 326
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 327
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 328
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 329
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 330
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 331
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 332
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 333
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 334
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 335
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 336
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 337
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 338
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 339
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 340
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 341
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 342
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 343
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 344
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 345
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 346
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 347
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 348
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 349
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 350
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 351
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 352
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 353
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 354
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 355
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 356
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 357
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 358
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 359
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 360
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 361
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 362
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 363
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 364
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 365
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 366
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 367
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 368
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 369
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 370
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 371
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 372
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 373
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 374
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 375
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 376
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 377
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 378
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 379
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 380
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 381
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 382
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 383
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 384
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 385
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 386
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 387
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 388
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 389
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 390
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 391
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 392
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 393
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 394
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 395
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 396
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 397
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 398
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 399
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 400
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 401
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 402
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 403
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 404
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 405
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 406
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 407
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 408
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 409
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 410
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 411
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 412
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 413
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 414
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 415
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 416
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 417
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 418
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 419
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 420
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 421
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 422
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 423
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 424
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 425
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 426
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 427
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 428
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 429
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 430
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 431
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 432
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 433
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 434
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 435
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 436
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 437
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 438
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 439
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 440
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 441
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 442
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 443
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 444
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 445
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 446
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 447
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 448
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 449
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 450
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 451
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 452
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 453
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 454
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 455
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 456
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 457
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 458
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 459
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 460
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 461
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 462
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 463
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 464
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 465
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 466
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 467
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 468
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 469
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 470
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 471
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 472
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 473
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 474
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 475
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 476
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 477
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 478
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 479
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 480
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 481
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 482
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 483
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 484
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 485
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 486
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 487
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 488
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 489
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 490
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 491
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 492
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 493
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 494
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 495
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 496
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 497
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 498
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 499
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 500
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 501
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 502
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 503
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 504
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 505
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 506
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 507
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 508
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 509
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 510
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 511
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 512
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 513
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 514
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 515
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 516
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 517
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 518
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 519
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 520
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 521
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 522
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 523
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 524
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 525
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 526
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 527
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 528
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 529
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 530
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 531
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 532
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 533
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 534
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 535
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 536
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 537
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 538
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 539
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 540
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 541
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 542
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 543
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 544
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 545
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 546
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 547
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 548
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 549
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 550
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 551
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 552
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 553
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 554
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 555
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 556
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 557
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 558
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 559
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 560
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 561
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 562
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 563
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 564
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 565
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 566
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 567
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 568
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 569
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 570
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 571
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 572
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 573
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 574
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 575
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 576
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 577
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 578
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 579
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 580
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 581
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 582
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 583
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 584
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 585
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 586
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 587
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 588
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 589
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 590
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 591
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 592
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 593
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 594
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 595
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 596
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 597
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 598
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 599
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 600
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 601
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 602
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 603
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 604
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 605
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 606
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 607
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 608
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 609
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 610
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 611
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 612
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 613
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 614
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 615
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 616
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 617
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 618
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 619
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 620
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 621
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 622
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 623
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 624
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 625
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 626
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 627
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 628
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 629
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 630
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 631
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 632
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 633
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 634
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 635
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 636
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 637
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 638
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 639
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 640
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 641
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 642
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 643
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 644
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 645
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 646
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 647
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 648
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 649
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 650
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 651
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 652
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 653
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 654
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 655
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 656
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 657
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 658
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 659
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 660
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 661
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 662
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 663
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 664
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 665
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 666
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 667
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 668
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 669
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 670
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 671
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 672
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 673
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 674
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 675
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 676
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 677
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 678
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 679
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 680
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 681
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 682
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 683
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 684
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 685
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 686
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 687
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 688
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 689
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 690
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 691
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 692
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 693
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 694
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 695
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 696
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 697
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 698
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 699
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 700
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 701
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 702
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 703
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 704
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 705
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 706
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 707
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 708
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 709
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 710
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 711
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 712
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 713
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 714
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 715
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 716
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 717
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 718
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 719
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 720
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 721
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 722
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 723
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 724
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 725
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 726
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 727
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 728
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 729
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 730
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 731
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 732
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 733
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 734
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 735
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 736
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 737
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 738
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 739
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 740
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 741
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 742
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 743
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 744
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 745
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 746
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 747
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 748
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 749
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 750
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 751
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 752
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 753
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 754
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 755
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 756
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 757
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 758
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 759
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 760
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 761
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 762
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 763
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 764
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 765
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 766
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 767
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 768
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 769
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 770
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 771
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 772
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 773
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 774
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 775
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 776
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 777
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 778
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 779
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 780
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 781
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 782
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 783
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 784
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 785
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 786
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 787
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 788
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 789
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 790
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 791
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 792
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 793
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 794
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 795
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 796
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 797
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 798
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 799
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 800
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 801
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 802
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 803
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 804
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 805
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 806
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 807
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 808
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 809
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 810
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 811
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 812
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 813
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 814
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 815
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 816
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 817
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 818
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 819
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 820
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 821
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 822
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 823
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 824
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 825
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 826
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 827
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 828
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 829
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 830
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 831
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 832
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 833
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 834
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 835
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 836
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 837
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 838
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 839
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 840
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 841
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 842
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 843
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 844
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 845
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 846
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 847
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 848
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 849
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 850
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 851
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 852
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 853
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 854
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 855
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 856
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 857
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 858
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 859
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 860
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 861
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 862
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 863
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 864
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 865
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 866
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 867
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 868
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 869
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 870
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 871
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 872
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 873
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 874
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 875
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 876
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 877
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 878
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 879
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 880
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 881
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 882
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 883
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 884
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 885
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 886
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 887
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 888
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 889
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 890
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 891
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 892
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 893
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 894
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 895
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 896
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 897
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 898
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 899
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 900
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 901
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 902
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 903
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 904
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 905
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 906
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 907
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 908
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 909
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 910
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 911
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 912
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 913
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 914
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 915
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 916
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 917
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 918
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 919
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 920
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 921
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 922
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 923
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 924
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 925
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 926
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 927
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 928
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 929
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 930
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 931
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 932
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 933
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 934
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 935
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 936
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 937
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 938
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 939
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 940
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 941
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 942
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 943
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 944
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 945
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 946
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 947
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 948
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 949
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 950
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 951
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 952
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 953
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 954
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 955
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 956
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 957
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 958
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 959
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 960
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 961
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 962
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 963
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 964
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 965
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 966
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 967
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 968
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 969
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 970
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 971
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 972
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 973
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 974
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 975
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 976
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 977
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 978
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 979
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 980
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 981
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 982
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 983
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 984
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 985
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 986
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 987
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 988
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 989
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 990
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 991
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 992
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 993
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 994
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 995
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 996
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 997
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 998
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 999
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1000
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1001
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1002
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1003
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1004
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1005
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1006
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1007
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1008
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1009
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1010
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1011
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1012
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1013
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1014
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1015
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1016
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1017
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1018
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1019
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1020
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1021
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1022
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1023
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1024
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1025
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1026
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1027
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1028
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1029
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1030
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1031
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1032
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1033
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1034
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1035
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1036
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1037
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1038
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1039
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1040
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1041
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1042
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1043
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1044
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1045
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1046
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1047
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1048
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1049
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1050
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1051
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1052
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1053
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1054
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1055
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1056
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1057
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1058
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1059
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1060
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1061
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1062
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1063
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1064
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1065
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1066
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1067
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1068
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1069
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1070
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1071
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1072
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1073
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1074
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1075
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1076
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1077
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1078
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1079
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1080
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1081
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1082
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1083
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1084
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1085
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1086
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1087
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1088
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1089
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1090
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1091
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1092
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1093
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1094
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1095
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1096
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1097
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1098
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1099
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1100
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1101
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1102
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1103
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1104
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1105
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1106
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1107
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1108
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1109
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1110
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1111
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1112
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1113
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1114
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1115
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1116
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1117
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1118
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1119
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1120
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1121
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1122
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1123
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1124
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1125
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1126
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1127
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1128
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1129
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1130
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1131
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1132
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1133
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1134
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1135
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1136
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1137
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1138
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1139
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1140
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1141
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1142
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1143
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1144
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1145
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1146
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1147
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1148
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1149
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1150
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1151
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1152
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1153
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1154
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1155
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1156
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1157
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1158
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1159
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1160
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1161
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1162
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1163
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1164
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1165
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1166
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1167
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1168
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1169
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1170
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1171
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1172
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1173
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1174
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1175
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1176
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1177
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1178
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1179
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1180
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1181
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1182
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1183
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1184
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1185
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1186
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1187
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1188
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1189
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1190
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1191
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1192
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1193
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1194
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1195
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1196
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1197
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1198
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1199
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1200
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1201
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1202
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1203
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1204
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1205
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1206
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1207
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1208
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1209
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1210
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1211
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1212
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1213
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1214
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1215
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1216
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1217
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1218
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1219
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1220
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1221
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1222
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1223
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1224
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1225
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1226
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1227
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1228
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1229
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1230
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1231
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1232
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1233
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1234
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1235
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1236
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1237
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1238
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1239
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1240
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1241
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1242
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1243
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1244
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1245
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1246
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1247
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1248
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1249
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1250
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1251
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1252
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1253
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1254
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1255
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1256
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1257
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1258
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1259
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1260
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1261
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1262
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1263
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1264
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1265
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1266
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1267
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1268
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1269
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1270
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1271
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1272
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1273
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1274
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1275
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1276
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1277
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1278
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1279
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1280
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1281
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1282
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1283
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1284
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1285
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1286
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1287
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1288
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1289
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1290
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1291
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1292
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1293
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1294
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1295
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1296
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1297
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1298
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1299
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1300
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1301
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1302
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1303
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1304
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1305
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1306
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1307
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1308
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1309
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1310
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1311
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1312
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1313
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1314
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1315
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1316
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1317
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1318
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1319
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1320
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1321
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1322
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1323
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1324
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1325
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1326
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1327
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1328
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1329
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1330
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1331
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1332
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1333
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1334
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1335
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1336
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1337
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1338
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1339
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1340
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1341
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1342
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1343
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1344
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1345
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1346
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1347
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1348
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1349
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1350
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1351
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1352
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1353
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1354
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1355
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1356
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1357
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1358
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1359
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1360
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1361
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1362
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1363
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1364
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1365
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1366
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1367
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1368
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1369
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1370
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1371
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1372
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1373
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1374
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1375
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1376
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1377
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1378
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1379
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1380
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1381
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1382
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1383
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1384
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1385
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1386
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1387
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1388
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1389
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1390
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1391
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1392
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1393
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1394
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1395
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1396
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1397
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1398
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1399
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1400
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1401
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1402
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1403
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1404
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1405
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1406
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1407
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1408
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1409
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1410
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1411
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1412
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1413
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1414
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1415
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1416
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1417
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1418
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1419
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1420
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1421
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1422
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1423
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1424
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1425
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1426
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1427
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1428
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1429
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1430
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1431
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1432
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1433
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1434
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1435
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1436
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1437
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1438
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1439
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1440
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1441
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1442
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1443
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1444
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1445
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1446
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1447
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1448
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1449
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1450
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1451
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1452
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1453
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1454
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1455
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1456
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1457
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1458
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1459
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1460
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1461
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1462
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1463
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1464
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1465
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1466
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1467
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1468
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1469
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1470
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1471
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1472
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1473
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1474
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1475
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1476
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1477
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1478
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1479
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1480
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1481
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1482
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1483
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1484
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1485
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1486
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1487
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1488
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1489
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1490
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1491
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1492
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1493
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1494
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1495
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1496
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1497
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1498
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1499
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1500
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1501
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1502
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1503
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1504
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1505
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1506
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1507
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1508
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1509
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1510
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1511
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1512
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1513
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1514
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1515
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1516
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1517
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1518
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1519
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1520
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1521
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1522
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1523
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1524
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1525
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1526
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1527
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1528
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1529
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1530
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1531
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1532
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1533
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1534
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1535
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1536
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1537
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1538
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1539
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1540
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1541
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1542
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1543
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1544
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1545
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1546
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1547
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1548
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1549
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1550
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1551
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1552
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1553
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1554
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1555
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1556
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1557
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1558
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1559
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1560
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1561
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1562
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1563
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1564
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1565
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1566
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1567
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1568
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1569
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1570
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1571
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1572
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1573
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1574
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1575
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1576
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1577
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1578
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1579
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1580
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1581
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1582
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1583
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1584
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1585
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1586
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1587
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1588
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1589
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1590
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1591
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1592
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1593
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1594
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1595
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1596
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1597
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1598
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1599
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1600
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1601
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1602
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1603
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1604
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1605
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1606
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1607
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1608
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1609
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1610
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1611
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1612
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1613
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1614
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1615
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1616
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1617
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1618
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1619
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1620
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1621
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1622
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1623
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1624
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1625
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1626
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1627
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1628
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1629
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1630
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1631
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1632
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1633
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1634
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1635
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1636
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1637
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1638
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1639
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1640
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1641
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1642
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1643
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1644
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1645
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1646
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1647
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1648
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1649
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1650
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1651
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1652
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1653
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1654
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1655
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1656
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1657
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1658
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1659
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1660
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1661
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1662
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1663
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1664
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1665
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1666
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1667
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1668
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1669
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1670
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1671
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1672
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1673
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1674
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1675
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1676
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1677
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1678
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1679
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1680
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1681
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1682
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1683
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1684
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1685
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1686
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1687
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1688
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1689
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1690
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1691
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1692
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1693
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1694
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1695
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1696
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1697
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1698
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1699
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1700
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1701
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1702
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1703
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1704
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1705
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1706
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1707
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1708
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1709
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1710
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1711
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1712
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1713
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1714
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1715
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1716
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1717
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1718
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1719
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1720
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1721
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1722
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1723
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1724
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1725
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1726
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1727
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1728
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1729
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1730
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1731
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1732
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1733
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1734
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1735
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1736
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1737
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1738
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1739
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1740
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1741
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1742
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1743
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1744
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1745
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1746
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1747
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1748
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1749
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1750
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1751
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1752
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1753
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1754
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1755
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1756
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1757
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1758
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1759
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1760
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1761
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1762
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1763
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1764
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1765
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1766
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1767
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1768
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1769
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1770
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1771
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1772
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1773
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1774
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1775
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1776
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1777
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1778
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1779
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1780
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1781
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1782
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1783
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1784
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1785
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1786
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1787
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1788
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1789
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1790
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1791
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1792
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1793
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1794
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1795
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1796
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1797
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1798
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1799
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1800
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1801
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1802
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1803
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1804
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1805
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1806
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1807
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1808
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1809
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1810
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1811
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1812
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1813
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1814
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1815
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1816
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1817
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1818
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1819
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1820
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1821
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1822
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1823
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1824
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1825
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1826
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1827
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1828
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1829
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1830
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1831
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1832
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1833
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1834
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1835
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1836
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1837
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1838
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1839
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1840
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1841
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1842
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1843
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1844
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1845
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1846
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1847
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1848
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1849
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1850
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1851
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1852
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1853
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1854
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1855
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1856
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1857
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1858
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1859
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1860
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1861
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1862
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1863
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1864
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1865
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1866
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1867
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1868
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1869
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1870
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1871
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1872
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1873
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1874
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1875
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1876
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1877
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1878
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1879
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1880
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1881
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1882
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1883
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1884
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1885
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1886
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1887
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1888
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1889
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1890
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1891
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1892
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1893
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1894
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1895
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1896
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1897
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1898
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1899
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1900
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1901
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1902
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1903
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1904
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1905
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1906
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1907
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1908
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1909
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1910
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1911
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1912
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1913
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1914
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1915
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1916
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1917
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1918
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1919
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1920
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1921
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1922
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1923
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1924
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1925
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1926
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1927
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1928
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1929
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1930
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1931
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1932
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1933
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1934
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1935
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1936
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1937
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1938
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1939
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1940
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1941
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1942
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1943
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1944
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1945
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1946
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1947
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1948
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1949
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1950
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1951
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1952
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1953
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1954
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1955
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1956
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1957
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1958
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1959
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1960
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1961
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1962
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1963
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1964
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1965
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1966
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1967
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1968
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1969
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1970
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1971
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1972
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1973
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1974
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1975
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1976
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1977
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1978
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1979
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1980
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1981
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1982
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1983
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1984
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1985
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1986
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1987
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1988
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1989
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1990
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1991
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1992
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1993
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1994
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1995
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1996
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1997
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1998
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 1999
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2000
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2001
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2002
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2003
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2004
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2005
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2006
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2007
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2008
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2009
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2010
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2011
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2012
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2013
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2014
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2015
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2016
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2017
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2018
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2019
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2020
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2021
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2022
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2023
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2024
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2025
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2026
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2027
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2028
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2029
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2030
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2031
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2032
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2033
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2034
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2035
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2036
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2037
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2038
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2039
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2040
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2041
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2042
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2043
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2044
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2045
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2046
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2047
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2048
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2049
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2050
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2051
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2052
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2053
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2054
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2055
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2056
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2057
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2058
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2059
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2060
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2061
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2062
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2063
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2064
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2065
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2066
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2067
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2068
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2069
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2070
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2071
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2072
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2073
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2074
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2075
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2076
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2077
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2078
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2079
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2080
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2081
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2082
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2083
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2084
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2085
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2086
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2087
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2088
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2089
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2090
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2091
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2092
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2093
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2094
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2095
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2096
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2097
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2098
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2099
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2100
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2101
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2102
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2103
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2104
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2105
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2106
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2107
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2108
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2109
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2110
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2111
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2112
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2113
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2114
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2115
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2116
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2117
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2118
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2119
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2120
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2121
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2122
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2123
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2124
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2125
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2126
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2127
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2128
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2129
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2130
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2131
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2132
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2133
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2134
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2135
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2136
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2137
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2138
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2139
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2140
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2141
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2142
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2143
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2144
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2145
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2146
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2147
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2148
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2149
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2150
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2151
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2152
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2153
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2154
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2155
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2156
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2157
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2158
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2159
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2160
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2161
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2162
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2163
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2164
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2165
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2166
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2167
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2168
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2169
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2170
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2171
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2172
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2173
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2174
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2175
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2176
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2177
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2178
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2179
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2180
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2181
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2182
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2183
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2184
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2185
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2186
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2187
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2188
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2189
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2190
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2191
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2192
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2193
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2194
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2195
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2196
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2197
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2198
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2199
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2200
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2201
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2202
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2203
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2204
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2205
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2206
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2207
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2208
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2209
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2210
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2211
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2212
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2213
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2214
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2215
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2216
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2217
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2218
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2219
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2220
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2221
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2222
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2223
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2224
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2225
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2226
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2227
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2228
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2229
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2230
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2231
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2232
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2233
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2234
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2235
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2236
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2237
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2238
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2239
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2240
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2241
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2242
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2243
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2244
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2245
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2246
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2247
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2248
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2249
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2250
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2251
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2252
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2253
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2254
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2255
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2256
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2257
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2258
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2259
DELETE FROM [dbo].[ModuleApiEndpointMapping] WHERE [ModuleApiEndpointMappingId] = 2260
PRINT(N'Operation applied to 2260 rows out of 2260')

PRINT(N'Update rows in [dbo].[PrintConfigStandard]')
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 04:16:56.8045248', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 2
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 04:17:03.5935003', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 3
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedDatetimeUtc]='2023-06-21 04:17:40.6314379', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 5
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedDatetimeUtc]='2023-06-21 04:17:46.1446735', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 6
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedDatetimeUtc]='2023-06-21 04:17:53.0271343', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 7
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedDatetimeUtc]='2023-06-21 04:17:58.5040219', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 8
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedDatetimeUtc]='2023-06-21 04:18:03.4659967', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 9
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedDatetimeUtc]='2023-06-21 04:18:08.3078148', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 10
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedDatetimeUtc]='2023-06-21 04:18:13.0935716', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 11
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 04:18:19.5431320', [MinimumTableRows]=7 WHERE [PrintConfigStandardId] = 12
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedDatetimeUtc]='2023-06-21 04:18:26.1880314', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 16
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedDatetimeUtc]='2023-06-21 04:18:31.7069086', [MinimumTableRows]=7 WHERE [PrintConfigStandardId] = 22
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 04:18:39.3521988', [MinimumTableRows]=7 WHERE [PrintConfigStandardId] = 27
UPDATE [dbo].[PrintConfigStandard] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 04:18:47.4418528', [MinimumTableRows]=5 WHERE [PrintConfigStandardId] = 52
PRINT(N'Operation applied to 14 rows out of 14')

PRINT(N'Update rows in [dbo].[Menu]')
UPDATE [dbo].[Menu] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 01:59:58.907', [SortOrder]=5 WHERE [MenuId] = 2679
UPDATE [dbo].[Menu] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 01:59:03.817', [SortOrder]=1 WHERE [MenuId] = 2699
UPDATE [dbo].[Menu] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 01:59:48.547', [SortOrder]=6 WHERE [MenuId] = 2700
UPDATE [dbo].[Menu] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 02:00:04.760', [SortOrder]=4 WHERE [MenuId] = 2702
UPDATE [dbo].[Menu] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 01:59:19.897', [SortOrder]=2 WHERE [MenuId] = 2750
UPDATE [dbo].[Menu] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 01:59:28.077', [SortOrder]=3 WHERE [MenuId] = 3012
UPDATE [dbo].[Menu] SET [UpdatedByUserId]=88, [UpdatedDatetimeUtc]='2023-06-21 02:00:14.747', [SortOrder]=7 WHERE [MenuId] = 3082
PRINT(N'Operation applied to 7 rows out of 7')

PRINT(N'Update rows in [dbo].[Category]')
UPDATE [dbo].[Category] SET [JoinSqlRaw]=N'WITH tmp
AS (SELECT DepartmentId F_Id,
           DepartmentName,
           DepartmentCode,
           [Description],
           IsActived,
           WorkingHoursPerDay,
           IsProduction,
           SubsidiaryId,
           ParentId,
           IsFactory,
		   CONVERT(NVARCHAR(max),'''') TitlePrefix,
		   CONVERT(NVARCHAR(max),CONCAT(DepartmentCode, '' - '', DepartmentName)) TreeTitle,
		   CONVERT(NVARCHAR(max), CONCAT(''/'',DepartmentCode,''/'')) PathCodes,
		   CONVERT(NVARCHAR(max), CONCAT(''/'',DepartmentName,''/'')) PathNames,
		   1 [Level],
		   DepartmentId ParentId_Level_1,
		   NULL ParentId_Level_2,
		   NULL ParentId_Level_3,
		   NULL ParentId_Level_4,
		   NULL ParentId_Level_5,
		   NULL ParentId_Level_6,
		   NULL ParentId_Level_7,
		   NULL ParentId_Level_8,
		   NULL ParentId_Level_9,
		   NULL ParentId_Level_10
    FROM [OrganizationDB].[dbo].Department
    WHERE IsDeleted = 0
          AND ParentId IS NULL
    UNION ALL
    SELECT d.DepartmentId F_Id,
           d.DepartmentName,
           d.DepartmentCode,
           d.[Description],
           d.IsActived,
           d.WorkingHoursPerDay,
           d.IsProduction,
           d.SubsidiaryId,
           d.ParentId,
           d.IsFactory,
		   CONVERT(NVARCHAR(max),CONCAT(''----'', tmp.TitlePrefix)) TitlePrefix,
		   CONVERT(NVARCHAR(max),CONCAT(tmp.TitlePrefix,'' '', d.DepartmentCode, '' - '', d.DepartmentName)) TreeTitle,
		   CONVERT(NVARCHAR(max),CONCAT(tmp.PathCodes, d.DepartmentCode,''/'')) PathCodes,
		   CONVERT(NVARCHAR(max),CONCAT(tmp.PathNames, d.DepartmentName,''/'')) PathNames,
		   tmp.[Level] + 1 [Level],
		   tmp.ParentId_Level_1,
		   CASE WHEN tmp.[Level] + 1 =2 THEN d.DepartmentId ELSE tmp.ParentId_Level_2 END ParentId_Level_2,
		   CASE WHEN tmp.[Level] + 1 =3 THEN d.DepartmentId ELSE tmp.ParentId_Level_3 END ParentId_Level_3,
		   CASE WHEN tmp.[Level] + 1 =4 THEN d.DepartmentId ELSE tmp.ParentId_Level_4 END ParentId_Level_4,
		   CASE WHEN tmp.[Level] + 1 =5 THEN d.DepartmentId ELSE tmp.ParentId_Level_5 END ParentId_Level_5,
		   CASE WHEN tmp.[Level] + 1 =6 THEN d.DepartmentId ELSE tmp.ParentId_Level_6 END ParentId_Level_6,
		   CASE WHEN tmp.[Level] + 1 =7 THEN d.DepartmentId ELSE tmp.ParentId_Level_7 END ParentId_Level_7,
		   CASE WHEN tmp.[Level] + 1 =8 THEN d.DepartmentId ELSE tmp.ParentId_Level_8 END ParentId_Level_8,
		   CASE WHEN tmp.[Level] + 1 =9 THEN d.DepartmentId ELSE tmp.ParentId_Level_9 END ParentId_Level_9,
		   CASE WHEN tmp.[Level] + 1 =10 THEN d.DepartmentId ELSE tmp.ParentId_Level_10 END ParentId_Level_10
    FROM [OrganizationDB].[dbo].Department d
        JOIN tmp
            ON d.ParentId = tmp.F_Id
    WHERE d.IsDeleted = 0)
SELECT *
FROM tmp' WHERE [CategoryId] = 8
UPDATE [dbo].[Category] SET [JoinSqlRaw]=N'SELECT        
	c.PartnerId F_Id,
	c.CustomerId,
	c.CustomerCode PartnerCode,
	c.CustomerName PartnerName, 
	c.Email, c.PhoneNumber, 
	c.TaxIdNo,
	c.Website,
	c.CustomerStatusId, 
	c.CustomerTypeId, 
	c.Address, 
	c.LegalRepresentative,
	c.DebtDays, 
	c.DebtLimitation, 
	c.LoanDays, 
	c.LoanLimitation,
	c.DebtManagerUserId,
	c.LogoFileId,
	--c.InformationContact,
	c.PayConditionsId,
	c.DeliveryConditionsId,
	pc.PayConditionName,
	dc.ConditionName,
	(
	SELECT TOP 1
		cc.FullName
	FROM [OrganizationDB].[dbo].CustomerContact cc
	WHERE cc.CustomerId = c.CustomerId and cc.IsDeleted = 0
	) AS FirstContactName
FROM [OrganizationDB].[dbo].Customer c
LEFT JOIN [MasterDB].[dbo]._DeliveryConditions dc ON c.DeliveryConditionsId = dc.F_Id
LEFT JOIN [MasterDB].[dbo]._PayConditions pc ON c.PayConditionsId = pc.F_Id
WHERE c.CustomerStatusId = 1 AND c.IsDeleted = 0
UNION ALL
SELECT 
	e.PartnerId F_Id,
	NULL CustomerId,
	e.EmployeeCode PartnerCode,
	e.FullName PartnerName,
	e.Email,
	e.Phone PhoneNumber,
	NULL TaxIdNo,
	NULL Website,
	1 CustomerStatusId, 
	2 CustomerTypeId,
	e.Address,
	NULL AS DebtDays,
	NULL AS DebtLimitation,
	NULL LoanDays,
	NULL LoanLimitation,
	NULL DebtManagerUserId,
	NULL LogoFileId,
	--NULL AS InformationContact,
	NULL AS PayConditionsId,
	NULL AS DeliveryConditionsId,
	NULL PayConditionName,
	NULL ConditionName,
	NULL FirstContactName,
	null LegalRepresentative
FROM [OrganizationDB].[dbo].Employee e
WHERE e.IsDeleted = 0' WHERE [CategoryId] = 48
PRINT(N'Operation applied to 2 rows out of 2')

PRINT(N'Update rows in [dbo].[ActionButton]')
UPDATE [dbo].[ActionButton] SET [UpdatedByUserId]=170, [UpdatedDatetimeUtc]='2023-06-23 08:14:52.8706159', [JsAction]=N'openPopupSelectPricingToOrder($data,''_CTBH_BAO_GIA_INFO'');
return;


//old code
$this.openPopupCategoryData(''_CTBH_BAO_GIA_INFO'', null, (data) => {
    if (!data) return;
    const dataObj = data[0];

    if ($bill[''pnk_chung''] && $bill[''pnk_chung''].rows && $bill[''pnk_chung''].rows.length) {
        $bill[''pnk_chung''].rows[0].kh0.value = dataObj.kh0;
        $bill[''pnk_chung''].rows[0].kh0.titleValue = dataObj.kh0_PartnerCode;
        $bill[''pnk_chung''].rows[0].kh0_PartnerName.value = dataObj.kh0_PartnerName;
        if ($bill[''pnk_chung''].rows[0].kh0_Address)
            $bill[''pnk_chung''].rows[0].kh0_Address.value = dataObj.kh0_Address;
        if ($bill[''pnk_chung''].rows[0].ngay_gh)
            $bill[''pnk_chung''].rows[0].ngay_gh.value = dataObj.ngay_gh;
        if ($bill[''pnk_chung''].rows[0].kh_nguoi_lh)
            $bill[''pnk_chung''].rows[0].kh_nguoi_lh.value = dataObj.nguoi_phu_trach_FullName;
        if ($bill[''pnk_chung''].rows[0].dktt)
            $bill[''pnk_chung''].rows[0].dktt.value = dataObj.dktt;
        if ($bill[''pnk_chung''].rows[0].dkgh)
            $bill[''pnk_chung''].rows[0].dkgh.value = dataObj.dkgh;
    }

    var baoGiaVoucherTypeId = dataObj.VoucherTypeId;
    var baoGiaFId = dataObj.F_Id;

    const url = `/PurchasingOrder/data/VoucherBills/${baoGiaVoucherTypeId}/${baoGiaFId}`

    $this.httpGet(url).then(r => {

        const area = Object.entries($bill).find(([key, value]) => value.isMultiRow === true)
        //const areaCode = area[0];
        const detailArea = area[1];
        let row = detailArea.rows[0]

        Object.entries(row).forEach(([key, value]) => {
            row[key].value = ((value.dataTypeId == $this.EnumDataType.Number || value.dataTypeId == $this.EnumDataType.Decimal) && value.formTypeId != $this.FormDataType.SearchTable && value.formTypeId != $this.FormDataType.Select) ? 0 : null
            row[key].titleValue = null
        })

        detailArea.rows = []
        console.log(r.list)
        r.list.forEach(item => {
            const obj = JSON.parse(JSON.stringify(row));

            obj.stt.value = detailArea.rows.length + 1
            obj.vthhtp.value = item.vthhtp
            obj.vthhtp.titleValue = item.vthhtp_ProductCode
            obj.vthhtp_ProductName.value = item.vthhtp_ProductName
            obj.vthhtp_UnitId_UnitName.value = item.vthhtp_UnitId_UnitName
            obj.vthhtp_Specification.value = item.vthhtp_Specification
            obj.so_luong.value = item.so_luong
            if (obj.don_gia0)
                obj.don_gia0.value = item.don_gia0
            if (obj.vthhtp_dvt2)
                obj.vthhtp_dvt2.value = item.vthhtp_dvt2
            if (obj.vthhtp_dvt2)
                obj.vthhtp_dvt2.titleValue = item.vthhtp_dvt2_SecondaryUnitId_UnitName
            if (obj.so_luong_dv2)
                obj.so_luong_dv2.value = item.so_luong_dv2
            if (obj.don_gia_dv2_0)
                obj.don_gia_dv2_0.value = item.don_gia_dv2_0
            if (obj.ngoai_te0)
                obj.don_gia_dv2_0.value = item.ngoai_te0
            if (obj.don_gia_dv2_0)
                obj.vnd0.value = item.vnd0
            if (obj.vthhtp_Specification)
                obj.vthhtp_Specification.value = item.vthhtp_Specification
            // obj[''thue_suat_vat''].value = item.thue_suat_vat
            // obj[''vnd1''].value = item.vnd1
            if ($bill.VAT && $bill.VAT.rows) {
                $bill.VAT.rows[0].thue_suat_vat.value = item.thue_suat_vat
                $bill.VAT.rows[0].vnd1.value = item.vnd1
            }
            obj.ghi_chu.value = item.ghi_chu
            detailArea.rows.push(obj)
        })

        // console.log($bill[area[0]].rows)
        //calcTotalMoney($data);
        setLastestPriceForBill($data);
        setRemainQuantityInStockForBill($data);
        //$this.updateTotalPage.emit()
        // hvh 6-4-2022 - replace updateTotalPage with reloadLastPage function
        // $this.updateTotalPage();
        $this.reloadLastPage(areaData.voucherAreaId ?? areaData.areaId);
        calcPriceTotalAndTaxRowAndBill($data);
    });

});' WHERE [ActionButtonId] = 85
UPDATE [dbo].[ActionButton] SET [UpdatedByUserId]=170, [UpdatedDatetimeUtc]='2023-06-23 08:14:37.8430136', [JsAction]=N'openPopupSelectPricingToOrder($data,''_CTBH_BAO_GIA_XK_INFO'');
return;

$this.openPopupCategoryData(''_CTBH_BAO_GIA_XK_INFO'', null, (data) => {
    if (!data) return;
    const dataObj = data[0];

    if ($bill[''pnk_chung''] && $bill[''pnk_chung''].rows && $bill[''pnk_chung''].rows.length) {
        $bill[''pnk_chung''].rows[0].kh0.value = dataObj.kh0;
        $bill[''pnk_chung''].rows[0].kh0.titleValue = dataObj.kh0_PartnerCode;
        $bill[''pnk_chung''].rows[0].kh0_PartnerName.value = dataObj.kh0_PartnerName;
        if ($bill[''pnk_chung''].rows[0].kh0_Address)
            $bill[''pnk_chung''].rows[0].kh0_Address.value = dataObj.kh0_Address;
        if ($bill[''pnk_chung''].rows[0].ngay_gh)
            $bill[''pnk_chung''].rows[0].ngay_gh.value = dataObj.ngay_gh;
        if ($bill[''pnk_chung''].rows[0].kh_nguoi_lh)
            $bill[''pnk_chung''].rows[0].kh_nguoi_lh.value = dataObj.nguoi_phu_trach_FullName;
        if ($bill[''pnk_chung''].rows[0].dktt)
            $bill[''pnk_chung''].rows[0].dktt.value = dataObj.dktt;
        if ($bill[''pnk_chung''].rows[0].dkgh)
            $bill[''pnk_chung''].rows[0].dkgh.value = dataObj.dkgh;
    }

    var baoGiaVoucherTypeId = dataObj.VoucherTypeId;
    var baoGiaFId = dataObj.F_Id;


    const url = `/PurchasingOrder/data/VoucherBills/${baoGiaVoucherTypeId}/${baoGiaFId}`

    $this.httpGet(url).then(r => {

        const area = Object.entries($bill).find(([key, value]) => value.isMultiRow === true)
        
        const detailArea = area[1];
        let row = detailArea.rows[0]

        Object.entries(row).forEach(([key, value]) => {
            row[key].value = ((value.dataTypeId == $this.EnumDataType.Number || value.dataTypeId == $this.EnumDataType.Decimal) && value.formTypeId != $this.FormDataType.SearchTable && value.formTypeId != $this.FormDataType.Select) ? 0 : null
            row[key].titleValue = null
        })

        detailArea.rows = []
        console.log(r.list)
        r.list.forEach(item => {
            const obj = JSON.parse(JSON.stringify(row));

            obj.stt.value = detailArea.rows.length + 1
            obj.vthhtp.value = item.vthhtp
            obj.vthhtp.titleValue = item.vthhtp_ProductCode
            obj.vthhtp_ProductName.value = item.vthhtp_ProductName
            obj.vthhtp_UnitId_UnitName.value = item.vthhtp_UnitId_UnitName
            obj.vthhtp_Specification.value = item.vthhtp_Specification
            obj.so_luong.value = item.so_luong
            if (obj.don_gia0)
                obj.don_gia0.value = item.don_gia0
            if (obj.vthhtp_dvt2)
                obj.vthhtp_dvt2.value = item.vthhtp_dvt2
            if (obj.vthhtp_dvt2)
                obj.vthhtp_dvt2.titleValue = item.vthhtp_dvt2_SecondaryUnitId_UnitName
            if (obj.so_luong_dv2)
                obj.so_luong_dv2.value = item.so_luong_dv2
            if (obj.don_gia_dv2_0)
                obj.don_gia_dv2_0.value = item.don_gia_dv2_0
            if (obj.ngoai_te0)
                obj.ngoai_te0.value = item.ngoai_te0
            if (obj.vnd0)
                obj.vnd0.value = item.vnd0
            // obj[''thue_suat_vat''].value = item.thue_suat_vat
            // obj[''vnd1''].value = item.vnd1
            if ($bill.VAT && $bill.VAT.rows) {
                $bill.VAT.rows[0].thue_suat_vat.value = item.thue_suat_vat
                $bill.VAT.rows[0].vnd1.value = item.vnd1
            }
            obj.ghi_chu.value = item.ghi_chu
            detailArea.rows.push(obj)
        })

        // console.log($bill[area[0]].rows)
        //calcTotalMoney($data);
        setLastestPriceForBill($data);
        setRemainQuantityInStockForBill($data);
        //$this.updateTotalPage.emit()
        // hvh 6-4-2022 - replace updateTotalPage with reloadLastPage function
        // $this.updateTotalPage();
        $this.reloadLastPage(detailArea.voucherAreaId ?? detailArea.areaId);
        calcPriceTotalAndTaxRowAndBill($data);
    });

});' WHERE [ActionButtonId] = 86
PRINT(N'Operation applied to 2 rows out of 2')

PRINT(N'Add rows to [dbo].[ApiEndpoint]')
INSERT INTO [dbo].[ApiEndpoint] ([ApiEndpointId], [ServiceId], [Route], [MethodId], [ActionId]) VALUES ('3c233041-3e37-7426-f2ed-880faf6880bf', 0, N'api/organization/salary/addition/data/{salaryPeriodAdditionTypeId}/bills/Search', 2, 1)
INSERT INTO [dbo].[ApiEndpoint] ([ApiEndpointId], [ServiceId], [Route], [MethodId], [ActionId]) VALUES ('4df5539f-004f-0e9f-5827-92b5ccd4649e', 0, N'api/organization/salary/addition/data/{salaryPeriodAdditionTypeId}/bills/export', 2, 1)
INSERT INTO [dbo].[ApiEndpoint] ([ApiEndpointId], [ServiceId], [Route], [MethodId], [ActionId]) VALUES ('2e6b9dd7-bb9f-3ccf-32e5-ec525736af76', 0, N'api/help', 1, 1)
PRINT(N'Operation applied to 3 rows out of 3')

PRINT(N'Add row to [dbo].[PrintConfigStandard]')
SET IDENTITY_INSERT [dbo].[PrintConfigStandard] ON
INSERT INTO [dbo].[PrintConfigStandard] ([PrintConfigStandardId], [PrintConfigName], [Title], [BodyTable], [GenerateCode], [PaperSize], [Layout], [HeadTable], [FootTable], [StickyFootTable], [StickyHeadTable], [HasTable], [Background], [GenerateToString], [TemplateFilePath], [TemplateFileName], [ContentType], [CreatedByUserId], [CreatedDatetimeUtc], [UpdatedByUserId], [UpdatedDatetimeUtc], [IsDeleted], [DeletedDatetimeUtc], [ModuleTypeId], [MinimumTableRows]) VALUES (64, N'Bao_gia_ky_he thong', N'Báo giá có ký hệ thống', N'["stt","vthhtp","vthhtp_ProductName","vthhtp_UnitId_UnitName","vthhtp_Specification","so_luong","don_gia0","vnd0","ghi_chu"]', N'//  add backgroud

 $this.taoDauPhieuInCoLogo();

 $this.themdongVaCanGiua({
     text: ''BÁO GIÁ'',
     value: '''',
     font: ''bold'',
     fontSize: 20,
     y: 80
 },
 {
     spacingBefore: 100, spacingAfter: 300
 });

 $this.themDong({
        text: ''Mã báo giá: '',
        value: ''#{so_ct}'',
        font: ''normal'',
        fontSize: 11,
        x: 20,
        y: 105
 });
  $this.themDong({
        text: ''Ngày chứng từ: '',
        value: ''#{ngay_ct}'',
        font: ''normal'',
        fontSize: 11,
        x: 20,
        y: 120
 });
   $this.themDong({
        text: ''Khách hàng: '',
        value: '''',
        font: ''normal'',
        fontSize: 11,
        x: 20,
        y: 135
 });
  $this.themDongCatdoan({
        text: '''',
        value: ''#{kh0_PartnerName}'',
        font: ''normal'',
        fontSize: 11,
        x: 64,
        y: 135,
        split: 166
 });
   $this.themDong({
        text: ''Người phụ trách: '',
        value: ''#{nguoi_phu_trach}'',
        font: ''normal'',
        fontSize: 11,
        x: 20,
        y: 155
 });
 $this.themDong({
        text: ''Nội dung: '',
        value: ''#{noi_dung}'',
        font: ''normal'',
        fontSize: 11,
        x: 20,
        y: 170
 });

 $this.themDong({
        text: ''Điều kiện thanh toán: '',
        value: ''#{dkgh}'',
        font: ''normal'',
        fontSize: 11,
        x: 230,
        y: 105
},
 {
     isFrameText: true
 });

   $this.themDong({
        text: ''Điều khoản thanh toán: '',
        value: ''#{dktt}'',
        font: ''normal'',
        fontSize: 11,
        x: 230,
        y: 120
},
 {
     isFrameText: true
 });

    $this.themDong({
        text: ''Thời gian giao hàng: '',
        value: ''#{ngay_gh}'',
        font: ''normal'',
        fontSize: 11,
        x: 230,
        y: 135
},
 {
     isFrameText: true
 });

   $this.themDong({
        text: ''Thòi gian hiệu lực: '',
        value: ''#{thhl}'',
        font: ''normal'',
        fontSize: 11,
        x: 230,
        y: 150
},
 {
     isFrameText: true
 });

    $this.taoBang({
        y: 182, width: 20, height: 20
    });

    $this.themDongSauBang({
        text: ''Tổng số tiền bằng chữ: '',
         value: $this.docTienRaChu(''#{tong_cong}''),
        font: ''italics'',
        fontSize: 10,
        x: 20,
        y: 15
    },
    {
        spacingBefore: 150, spacingAfter: 50
    });

     $this.themDongSauBang({
        text: ''Rất mong được hợp tác cùng quý khách                                                                                   ĐẠI DIỆN CÔNG TY'',
         value: '''',
        font: ''bold'',
        fontSize: 10,
        x: 20,
        y: 42
    });
    // $this.themDongSauBang(''Đơn giá trên chưa bao gồm thuế GTGT'', '''', ''normal'', 11, 20, 30
// );


$this.taoHinhVuong({
    x: 300,
    y: 48 + $this.doDaiCuaBangCuoiCung,
    width: 135,
    height: 55,
    color: ''#ff0000''
})


$this.themDongCatdoan({
    text: '''',
    value: ''Signed by: '',
    font: ''normal'',
    fontSize: 9,
    x: 305,
    y: 56 + $this.doDaiCuaBangCuoiCung,
    color: ''red'',
    split: 125
});
$this.themDongCatdoan({
    text: '''',
    value: ''#{companyName}'',
    font: ''normal'',
    fontSize: 9,
    x: 305,
    y: 66 + $this.doDaiCuaBangCuoiCung,
    color: ''red'',
    split: 125
});

$this.themDongSauBang({
    text: '''',
    value: ''Date: '',
    font: ''normal'',
    fontSize: 9,
    x: 305,
    y: 86,
    color: ''red''
});
$this.themDongSauBang({
    text: '''',
    value: ''#{ngay_ct}'',
    font: ''normal'',
    fontSize: 9,
    x: 322,
    y: 86,
    color: ''red''
});

$this.themDongSauBang({
    text: '''',
    value: ''Signee: '',
    font: ''normal'',
    fontSize: 9,
    x: 305,
    y: 96,
    color: ''red''
});
$this.themDongSauBang({
    text: '''',
    value: ''#{CensorUserId_FullName}'',
    font: ''normal'',
    fontSize: 9,
    x: 327,
    y: 96,
    color: ''red''
});
$this.themHinhAnh({
    x: 335,
    y: 56 + $this.doDaiCuaBangCuoiCung,
    width: 30,
    height: 25,
    alpha: 0.66,
    value: ''data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGQAAABRCAYAAAAkVQNKAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAxBpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6RkI0MTRCQTE2MzA4MTFFQ0JFNEVGQjkxRTQ2QjUwRkEiIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6RkI0MTRCQTA2MzA4MTFFQ0JFNEVGQjkxRTQ2QjUwRkEiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNiBXaW5kb3dzIj4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9IjExQkVBNERCNkFFQTY1NzVGQUY5MkE5NDJFRDc1RUVEIiBzdFJlZjpkb2N1bWVudElEPSIxMUJFQTREQjZBRUE2NTc1RkFGOTJBOTQyRUQ3NUVFRCIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PjvthZ8AAB8hSURBVHja7F0HfFTF1p9bt6Zn0zshIZCEkFCkCYIUBZGqQYrIQwFFVB4oYANFHwj87IUPpSoIAg9ERCyPJi1AgJBCQioppLftt34zd5OQhGySDUt9b/JbQpLduXPnP+ec/zlz5lxMFEVwF5ui1lTlU6IrCC/XZcWW6ovCK81lXrWmMj8Dr3PmBJaCw8MwDHA4RnIK0rnCmXYqc5d5FWkcg5I0Kq98jdo/yUmuyYB9ceABaNidBsTAaH1zq1MezapIGphdk9q3nCnwMZhqnDnRDATAAxyNB8OBiGNAhEhgCJG6MYroS0TfMUAAEtA4AZSkq1aj9M8KcohICHHvfiTQOfykinbK+x8grTSWY5yvlCeOulj817TMqksPVZgLnThggvNOAZIg4OTi0iSjhqPhiDgQMAQM+r9o+V2TUSNQLBAJ8O+CwANe4ACByYC7zFsX4hx9OtZryJZwTc9fKFJW9T9A6lqVoSwiofC3F88V/Tn2ujHDj4cyQOEknDgCTf2N2QW2jUH6BPxHwNANWF6gDiAeai6zyEEQcRAg71TYw2f4j738RmxyU3gm/9cCAoHocix395KEokOTKtnrCozCoXoh4cVIOIsCnFCkfBAoQodBaRMx+OJ5Br544EB5m3v7DN89JGjSB85KTep/DSBINR3P3vPm4Wu75pTx+WpakgbKMtd2nvP2iRH6BhkBlBiWNwM32ks/yH/iukHBT/2LpuTlDzQgaaUJz/xyZcOyLH1iZ5qmLEAI9SAIN5atDRMqNgzy1saG1JsIO0HAcAwHOikjc57o8sLSCK8+Pz5wgDCcSfNrxuaPjuZtn8HiRkATsrqJxCwz0eYyRm8TpJcEmUAAHuclayAJFuyDqDPyGLQ7mPQFARYJm8QOAYLBftB3hjcBWqDBw/6Tto+KeH6ejJBXPhCAlGrzHtp2adXmVO3ZMBktBySaJJu6gwwJfSGWJLLwRwgKRsMXCWSYApDw//AHSKREXIC0mBV5wIgGCSxktHFIewkccTTcNumrM/863gC6qQakPNN9wT88HQPP3O7JFkRenVZ6dsyF4qPxNYZSH5JUakNdo4/39nt0k4PMJfuWAEkrPjvt+5QPv65iC1U0qQAWbW1RL6JVecCkFYomn+M5OEACOJDOwEvum++tDk71cgg956L2znGgHcsdSdd8ipCx0CvkIWwEfD+hZ6v9qs1lvuXGYp9ibU7c9drsbmXG/CAdXwNx4yGFlgEcMjjkt4htASTpQRGYOAa44Z6GydFvvtDNq/cPt4X282aXxKJjU05c2/NSTm1SFxZj4TgxaZ4EngU+8vDiGd3fnRbo1uXPDgFy+tqvb+xI+2SlmdIBuUjBhY21a02aAQMETgRupBvbybXX8W6a/j+HuEaccFd5p2EYobd9xQmKSn1RRFZl0tC0kpNPZFSf71fFlREYSQEaSVprKrNe28HvjGgEFK8CT0f8852+gaPetx8QjOvZ/N9nH83/95x8fUoADh1eEvpeWMNiqVsUghl4UgHVr/X9qpfNgBzL2vWvHRmfLCagzUbqQnKshUZ318JKZASojuA691dHXOvj+/jWGK+B212Unin2XomVhqKoi0XH408XHpyRb7zig+OIblNWbFlj+wWHB+AYWRFMCH1lzSOh8YtuTTUJygsFh2f+lbNtUTYEgiBwIMPlkoKuZ35iHRiWoWDAyOnACL9Zm2wC5Gj27tV7rqxZCGjk1sluQCC2rBKgiwag6ge+zt3yhwY+9Ums9+DNUK1U3G5dzfBGzYXCY88cydm5IN+QHEBQKBoAx9vKvaIwDUdAUIwCmBg+/6PBneLf6Mi1r5Scn/R75qbl6TXnIwApwAUhb5ddhcQc+MrC8toNSMK13xdtSX7/I1wGJE8b2t8b8iA21QISk2GNwJn0YEYET/+4X/CYNRRx53m/mTO5H8/d988/c394rZYrlclImVXmJ/0aShQiDyIjgsld33qrX+DjH7T3WiXa3D4HM7esulB6ZBCLmQAF1SYUPkAIdWGgtigGfLMj5V7TLkDSS89N/erCkq08qYV0UWEZfAufk5wwaFwZTg96uz52/ImIFxZqHAIS7jaVLNbm9t+T8tUnyZXHetIUJYUmWzMuSH0RHA1m9lgxI8qr3+a2aP9f2dvfPpy7c46eq6QoGV1HyW3znxj4FazokdUmIMW6gphPzsw/XisWqGlMDnABl8S7OSCY1KkZkLwcPBH6wqqhofHvwN8y9wq/h/cp+/3qD6sOZq9/hcc5SKmpViDBgEGsBRosjFky6NsAJa0qael9qSWnp+1LX78iT3cxQE7SkDnJ6+JrttllpFHMrAlM7PTKWrJVvcYzDtsvrdxexearVYQS3pSFqjUBA0NuGgZYQQ+cCDfTlOilcyM9B2y65zxgDDOPCJv6qrc6JPH71BXrdEKlXI7JbzixwBKlFHABsIwJaKgg3fiI2QvllOImp1Fnrgn49cqmNX9f3z2Jg2tOQTvUg26TVFgCpAIwsnoQrnoop2/wE2tblZD9aeu37M/5ZpqKUlt1vNC+BQO5vAfpo5sZ997TAc4Rv97rEdXcqrTh6xPf/KmaK3KEXnrd+oKqCmcAZzaDrs4DL8VHL5zprvZNbP7Zy8UnZ+zO+HJlsT7DU0HCz0InFm/Yr2n36oDXhMALJkg+BdDVqW/S1OilU1xVXslWAckuTxr72dlX/i1QLPTAyWYXa4gywU6NwI3y186J++hJb6eQw+A+aYU1mYPWnXvj50ruuiMJDTDaT4E0CwwJmPTjmC5zXiQIqsk+ioGp9TuQvnH1sYI98SLJAApRzXZGBxpvF6BNOJ6DRl9QgjCXHhf7+4//orvPgG0YhhuteupQVTl+fHLepWzDpSA5oWgBeot4c/AmlLjGOC/uo7H+LuG/g/us5VWljvjy3KL9NVwx5U74myZ0fWVRrN8jXzR/X2ZZYvzulM/W5BqTfGWkClJ+wubAtWQnoAMoE2jQza3fyUFBEz8O0/TcC5ptPbcIyOHsXR9sv7JyqZxWSbt5LTEqSGyhNyUDs3usnhzh2fNHcJ+2pKLj/zies2fO+Oj5r3o7BJ9oSgSA7M+MbR8ezNmwwIzppMBp2wHTlhxjE8B5CnRz6Z/4aKfJ74dqYvZZ03A3AQINlt/qv19IqRIKHBETEaUBCM3EE8WAjGBi59cgm5q8GNz/DfFUvqnXXxz1U/Ln3yRW/NVPRtNSxNk2sYA2Cfo0PCeCAKeueY+HTF8e5T3oB4mMttJuYll/Z+9fUMbkOMppZcNqwBrvK0CJMUM/I1oz9OyQ0Pi3wYPRmoBxCRrunWlrPy1nCuE8QKovYi1qCauhEygVLGRObrR/7fDQZ9f0DRz5VXsjFGRT6aj2P1G0exZFUJIn3th413/nIMBOpJt5YsS82ZCvs+ABaoLIKw+lb115MGfjyyJpBEpChX7ZLv5kIc6QcQoGKE0yMNBn4v7HwmYsdlZ42rRl3ASQswWH5pUxBQ5KStlkQYiNlCoPKe6w8Gc/1qh8LzxIYOjMVUE7Lq3dcq7i0EAZpLMYJgeWVYm1CYS0UDG4VBkzCFRH5YztMvvNCI/e2zsyjgZAIO1TnC/482mcIhqyOZpf1CyaUMT2+sDAJ1c/SGDkVaYP+/7y+5vzjVe8FaTaEottp4ZC2swMqb9cUINhwdM2De887Q0ZqSjt6FgaAMmsuDwyX381kJbTABPwJmJq8ZsEiaANC3pmNX0PbXneajtX8MfCHamf/EsPykgEBqKn0pZCO9gUCqcjLztIHZM7qevLC0PdYnbf6ngaADlXfHgKixkgT1bepDNR7IoVGeCn7lIY6zNk/YMBhSj/JX3j54eyN88CJAdkQHkjkiWCVuNc6A1IW1AcCYYHTt81Kuz5l+SkstQeo5IAMfEGt6yK84NJZMxb0JooNMDzZtDH57Gt0IPV3e9QmFi9947La7eeLvllKE3JIG+k6jfvGrIkRasqSoCU3wA0sqCqCVHz3+juNdCuC1QCpLA6q0+ZKdeNIMkWVwfKBnQivNgY78Fb7ncwqvRlEVsvLN+dpj0dAVd1XahcvJnAtGC90TywDAtiPR479lS3l2c7KTRX7D0+CZCsiqRBjMgCpUi3OCSWZ0G0a9wZV6VH2v0MxrXKK8M3X1zxQzGT7i6jVZLT255QOQZXqQEzAppzAOM7z107rPOUJWhabscYESBYbk1Sfwyvy3PCbshsPbdGjnqEZ5+f72cwUq+fnrn58vtf68RyGrIgKUnbkl9shdqiiCxmyfQzQTrrKQupeabH63PCPWJva5iINLI6zzJDXhiJ4RKTsiSnNWYSPHSQnMUQ18ij9ysYp3IPvLXzyqfvmyktUIgKydfDGhQU1gqHAoBhDSDa+eG0yd0XTXFRet9234ss1Rd2rWZr3CRAkDyIWKO9cch0BQZ4qTrneaj9kjpyAQi4b25lah+tsUajkKtqAl3CLzjK3NLvFBiH0n/44pfsb17CaAGCQTcCoWnWY+PwkLStAMzQwSDAiMCZPz4RMetFEqfvyLEGslSXF8XwBhylzNTHrsSGNQLlQ+CBtyo4E8cIk62dJ1z7fcGBrO+WVBoK3QWMBzzUim6Ep2Fw4MRvh3WeuqB5DMnOjd6d/OXGw3k/PEPJUPIcZYmCWDHfjX8yiUZoTx2FCd3mv9svcPQKaxfQmqtCGNZMuam97LbA8HJ9YSdB5FoUXbEuu85XFZBha8fHcnav2Jy8bG0Fl+1OwAlBm/9Kgga1YrHyp6ufzd+V/NVWcOOQiF0bK7AO319Yueuva5ueIWVYXVIDZjX0Ub+BJELOa+aMwAsPLp8bu3Zca2Bcr8kZ8Mnf80+dLzg8y64SUmEsCbYerrEc8NOogs/b0mmlvjjyt4xvl2C0CEhMafF6LTlikEXIAUHz4Mi1zZOdKMeKYeFTX7bnDTGc0W3rhZV7zpb99rACbT2LoGlSWjMlJf0N2k4BgcEYQYSqV9a02HfHuam8Llu7Rkrxmek7Lq36ppDJUHTl+3jYFRAtW62xBggaLNoTUcnVNiW3JZWceKqar8BpFKQT8JuUAg65P0nRYF/W1/PUCpeivgGj/mWPmzEwWp+N55f9nFp1Ik4hbR+06uI1AIOMt9nMgj6ax/+e3H3hFAXleM3Kh6i/rm5btT/7u9d4Ug9oXAm0bJWrXQExsNUe1sQZR6edMBmAAyyzpdNSbUEn+OGG9P/mzqbFrEInlGbAj6mfrXBReGd20cT+dCs3Umsq77zx3PK9V7Rnulr2csQ6SmtNKwpSGIQHDGChRzHSf8aWJ7s9PxfaSkOL3j1n8NiV8vnmU4V7RxI0DUjI1njBgPaG3O0JCA5FnLaW+YoWN9o/JnHKaEunFEnwHFYvDdbVIUrvNJNafMulFRuKdbkPddj7NpZGrju7+I803emuckrRvp09qIrNGAMEngQTw15ZMy5yzrPWwCg15Pddd3rRsRNFu0ZSULIp0bKtjVQxzzOUXQERm+SENl3FddkxyI4ItnQa7Bp9DBfoBp8GsxqNgFQUglLNF6m3XFyxw8TqvG0Gw1AS9V3C4oPZ+qRAOWXZ5cRu2lhrdmfwdhjRBGhRBaZFvbNgSKenrSZXZ5RemPTZ6QV/pBrOhMtoRV2itNBYrdv1oB7eKDe+9TizDa2bR98d4Q49rpo5HRAkpoZZV+Mok4CQg2ztpYCdlz/fCECreZ7NyENRzLqExb9mG1P8FIQSacl6KmIVEJR0aUC7nsDbOLf7qqd7+z76sVU1aKwK3npx2fZKNlelIFTS6a6m/aMr4JhdAaFwpclabhZSN5Yz4LxNYkkRlDa+++tT3Al/rRnePtY24NBOKcGpkr0j/rz6Y7sMfIX+esz6hCW/5JmS/SikpqxKxI0/oaVh5E3AEw+omRu38qkwj7idrUaFOa2TAashaIKCYLe8cgkct6svhatItzLr+gwqFYEFBlbnZWvH3g6BZ5/r8d40hejCm1GRgLYOfcJr0SQNDlz9elFGaeKk1vqGvlPsurNLf8szX/FVkCpACAC0vfONDsbogZ+sc/FLvdYM93ft8ktb92AWzA6Wk490Xf/iTYpDTqrterwCd5ZprltP0cekcwu1TKV3RzoPcY/aNz3yzRdRJjmLcW2mgyMCwRFmsD1p9Tc1xsrOLduM4qj1CW//nG9K8UR73+06WIqhtCU9CFZF583tvXqkZzsz8g1MrTsjMNBhFFqUP6RY5LQja19AlC75Vm8FnX4VeVCpK+jc0Qt09xn4f+M7z/uI41gpsbhNHg7tSQmX5bozae12UeQVTWyGsTh63bm3fyswpfnKSUWD0ye2AgaaTCOnBeGOA1Nm91o93FXpfam9Y682lQVB89/kAFrz5iRzL7QrIG4qj1ysoaqC2NKyBUW6zOhbucjg0EmLh3pP3MeYTaDNfFi44mWUHFys/CPur6ztKxsMrLki9Nuz7xzINST5UJQlV6ot2RChejdytSDS+dHE53u+P8JR5mZTCKhMXxwgwS3i1pgz8FDaN/sG91QGJcswhWCJ/WM3+yE4BfIN17rxzVarjU0cE/nSrAiX/lcYqMcxXGg17I0uTNAy8Ev2xvn5NelDIVvz+/bssoM5+ot+KOMcbSoJzdhO85kSCBFt1YJeTiNPv9DzvREqWm3rSsYKtVf6WFJphRaJJyREEBB/u0aucXcHv8tKmXMl2rDBmjFSNG8UlJ4KQ05AmeFazC2FXglF+ZSY15/2oPx0LM838hWsDAyjAIMbwE/JH2/49vzbxzO1CaEWP6PteKQonbnQgt5uY44+22vZKJq0/TididN5FOkzowjpuMHNf0cnxRxIN72L0iPLroCoKcdSD5V/Dspkb5G3Q0AgywI5FWl9b/VibgrvpPiopbMIgUJJ+a26QJggQJdRBvJ0yQFp1QlBMkhtsVZjU3WjR0kITC3o4zr6+PTYpWMoQtahlKWimuy4KlOpGsdIKX32hl21XIgXGaBR+2eoaKci+zqGsO9OjpF/i0Ir5hGKSnLZ8XH2uGC4pueOxzrN+NrA8XXlMlqJ/aEsEJyEqoGqM+BtS4YZLp4+mrHHno19awxJULUdHWdG+bnRrGiUynk0M3HSQHiRAyEOkQnAziV1pKuFukYdpzDaas9yXAauVl3sXWks6WKPiw4Lnbaol/sjJxg4eZJlvJWGIvu45SgaSlqL04w+NrXHEggGXd3RLgVBkCeVnRmBE4TV1UKJMhDm2sPuZ2IkQPydw0+4yn1qkBi2/C4S1HAl9KWCo1PtclGM0MdHLnjOVRagRWcnbK1T0tIEGVkj6Ok26uS0HkvHUgRdcyu95VWmDcnXp4SQ6Ax4Cw1JhxsdUBPg2uXv2wKIjFSVhrn0PMpCr9xKOE7yD04XH/oHKzCO9riwo8LtanzX118C0qlezmapqIt6WjxwCEYPl6Hnp8UuGUsT8lve+z5ZsH822lNvMQqOTo7xDAh3jTmmoFSltwUQyYHzGrCHEukW7QgyZDQ0bgW6ZK+LhUdm2OvikV4PbX3Yd9JPRs5km5CI9SE+5IHrQKTTwMvPxb4zSkYqym51TKW6vLhLpYdHodNSWEvqEX7hgATRXgP3gtvQGgAJc4/92VfVqQTl8GJWJgGD3P5I7rbFLG+026bM6PB/vOoj71zKCMa2412N/AzkgRugB95Z3TdzZuyy0ZASl9hjPEez9iyuEasIVPW0cVimPkeFherKVxFWGu7ee89tBYSCoh7nPWILxwlWwkM4lBIlyNRd9j6e++ur9hqAknYoGt/l5YU4T0rlJUBb4RXJiEMDDh3MQEX09Zlxy8aoZFa3XG1q+dWZg05dPzBBLp0lbDEQI52g7ekz5HsoQdW3FRDUevsN/9ZF5s5Br9xaVANQ0FP+LWfTPyv017vaaxBRXn239vMafdDMGdr1fjOvA75EeMXzcR+Odla42yW9Fc4/sS/9q88NWA1G1DufWNN7F0QWONM+pt7+j68Dt6k1AcRJocl4yOeJbRzLWB01DX13LVci3536+TogiqS9BjIyfOZCd8LLZG0xYHWDhaQCuGJ+xpmxyye5q70T7XX9o9l7l6ZUnoxS4MobIZkGKRGkFwOlo4/XqN1onu4IIFIgMGjiB660j0EquWfFnsoIBbhQ/seA/2TtfM9eA3FReqYOC56xmmEYKTHiZunEgAkzATXvIszsvnyGn3NnuxUpKKjKGHwg88t3KZKwUpjGUtnUhQo0PRIycQW4je0mQJwU7hmDg5760gypnYBbU+MYQKWO9matX5JScspuiWIDQp5c1cN1UJLBrIX2hLPkGkv1R0TLYUpWBp6OWfhqqCZmp72uqTXXBG6+vGqLAegIStqIEloUT5ZjwMCg8RucFR5X7iggqA0MGrcyxKHHNa4Vpw3VNhRIA/j+8oqv8qvTR9hjMARO6qfHvT3qEe/4nwlOLmWd63kDMEMV6kWFlcyKWfFCD+9HPrfXzZt5g8eWCx/uyzem+culaqpCi+4OI5iBrzqyeEjQ+OXgNjertU4yShMnfJb48i6SwKQTRi35JzgK5EHQXCn/mpfi1ozycmxaCeFWWlFNbr/C6qvRDGd0dVF5ZIe4RR+2F7WVvG2BV286v+JAQvmBh1WUoo5DgRvHMOpOZnA4lFQGAy/0XD0jUvPQ5rsGCGq7L3+z/VDRd/HqxoaumUVB/gA60ONBBlXPjH3vaX/nsHu+5glkcx5bL6zac7b8YH9U3pYQrG1AiUDPGsBwv+d2TIh6Mf5OjK3VzYVRETPmdVLF5ZoFg5VQOSZlY8hwGShn85y/PrdoX2rJ6efuZTBqTOXh6869/Xti+YH+CqlKQ9Oav1jDC5II3gRC1JFFoyOem3enxtcqIHJSXjE96o2pDpi3iUXVLFuUEVw6GkZBUGqwUvm6i29vOHp17zJgQ37VnWpZ5UlPfprw2tHk2mPdFZSDlHdsOdJ2cyI2LxqAA+7OTIleOk1GKu5Yvch21Vy8dP3Ec99dWroBEKKlOpAVpxGd+REEVF+dBXGeI//zZMTc+W5Kr5S7DQTHM85/ZO1Ydih34yuoUiotRXFFqYhYUw9QlMIyqKgY/BCY1f3DudE+g765k2Ntd1XS4zn73tme9tFyiqov7d2a1ysiBgOcZB76x4NnrekfMPpTAqfuyoNVMsouTNh/df2KzOrzXaQj0Kiian1hP7HpQwKkgv04C0RILidFLFw1OHj8Ha90ZFPd3j/St67dm/XFAnSUgGijiBfiLMiZQqd7O6tjrg4Jiv8crrYNRAcqWHek5VQmjz6c9dOrFyuPDkUFc1BBHULE6kYmNvWqRItkcDgDWDMGxoTO+fKxsKnz7sYCsrmy9e8Zmz/5OfObVwhJUshGVZqtNQFIkVyRBIGO0Zl9fZ7Y0N1rwHYHuXOu/dmT0T297OKoU4X7Z6WVnxpgFrXQgVXWVYCzXpEeyTsqZsmwHHiy00tfjAyb/vLdUq8dqv1+OPun9/+d/ulbIiEAaVet1T7EhsL30vM7IK13pb114a49T0R7PLQn0KVLoovSJxEA2xK6b3jaVSEF1Rl9U8vOjEgtPzv8ujHbUyDNQE5AOgsXgSBYxaHuoAoO5ccICBYH48JeXflIp0lL7qa96/DTES4WHp79Y8qaz2pBJU2RMilq3laJVAt1FgCHElSh74L4vyPlDjzVnS77qgNTfBxCrrgp/VPUMqdSyGx0BEkJUMVpURoWy5udGc6s0Jlr3SuNhWHFurzuxdr0yEJdflgVU6rkgQmQBA1I9HwrDGvYW7F2fyg2JpWE5RigxJzZyV3/ubCn76Of3W0CckvPD8mvSh++/fKq77KNyX4ozxYXbD/DKUDDikrhIXaGnhuCXjSQSTXTCZyEjidhhmxIZAWTHAU8UfVQlAAupaUSlkQ+EqlOoXH4U2yD0WPStrGU7yuPyXsmeukMf9fQI/cCNb/lJ+wYmVr//akb1x67vnuSSJjhKlU0nNOwpUnPLKzP1BXFhoLNDZtlUsoQdsORw5o9XeemI8/YzXEpYNncYnkG4BwJ+vmM3fNkt9nzlZRDIbhHmt2eQXW56OSze69+taLQkOaHHvBCABLcaw0Vj2E4I/BRRhSPDZ37ZrRv/w332hjt+pQ2I6P3/k/WtneP5/97Vi1XTqDzeFLpoyZHzO7wo17RkWcRnT9ngQPpwg/yHbdhSOjk5Ur63pGK2wZIfSvTFvQ8kr3z9XMlBydo+QqcgIzHUvweu6M3h/wPVjAAR9yD7+75+O6hIePWeqkDE8A93G7rkz5LtNd6JuQfmpVY8ue4UmOeB6qUgNJC0cO8Gsp4YB1/LJ5Yd0ytcV/oPAuyEShO5aHwr4z1fGRvH//RX3s6+J8D90G7I8/CRQf6U0pOPXWx5PCE7Jrk3lVsCS09U4GwPGUN+QuY2AGGVl9LXWJfInqgDXCivMxBTtFn47wGb4vw7LNPSauLwH3U7vjToquNpV3zKpMfzqxMHpSlS42rMhT5GlmdkhONdcewMYlAYY18iYbTfZAii6KFiSEA0ZarknQyuyi98wMcIxLCXGOOBLt2O+KkcL8K7tOG3c3nqUMfRF5lLAuETl6/Cn1BYLm+qLPWXOGrY6sdTYKBYnnOEXr3CggOR2EyrZykgAPlWuQo1+S7K/2zNSrva56qgDMuSs8sHMNN4AFo/y/AABOGAq+z5wBBAAAAAElFTkSuQmCC''
})


const pageCount = doc.internal.getNumberOfPages()

for (var i = 1; i <= pageCount; i++) {
    doc.setTextColor(''blue'');
    doc.setPage(i)
    $this.themDong({
        text: ''Phiếu được in từ hệ thống quản trị doanh nghiệp                       CÔNG TY  CP GIẢI PHÁP CÔNG NGHỆ VERP - https://verp.vn/'',
        value: '''',
        font: ''normal'',
        fontSize: 9,
        color: ''blue'',
        x: 45,
        y: doc.internal.pageSize.height - 10
    });

    $this.themHinhAnh({
        x: 178,
        y: doc.internal.pageSize.height - 18,
        width: 33,
        height: 15,
        alpha: 1,
        value: ''data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEYAAAAXCAYAAAC2/DnWAAAABGdBTUEAALGOfPtRkwAAACBjSFJNAACHDwAAjA8AAP1SAACBQAAAfXkAAOmLAAA85QAAGcxzPIV3AAAKOWlDQ1BQaG90b3Nob3AgSUNDIHByb2ZpbGUAAEjHnZZ3VFTXFofPvXd6oc0wAlKG3rvAANJ7k15FYZgZYCgDDjM0sSGiAhFFRJoiSFDEgNFQJFZEsRAUVLAHJAgoMRhFVCxvRtaLrqy89/Ly++Osb+2z97n77L3PWhcAkqcvl5cGSwGQyhPwgzyc6RGRUXTsAIABHmCAKQBMVka6X7B7CBDJy82FniFyAl8EAfB6WLwCcNPQM4BOB/+fpFnpfIHomAARm7M5GSwRF4g4JUuQLrbPipgalyxmGCVmvihBEcuJOWGRDT77LLKjmNmpPLaIxTmns1PZYu4V8bZMIUfEiK+ICzO5nCwR3xKxRoowlSviN+LYVA4zAwAUSWwXcFiJIjYRMYkfEuQi4uUA4EgJX3HcVyzgZAvEl3JJS8/hcxMSBXQdli7d1NqaQffkZKVwBALDACYrmcln013SUtOZvBwAFu/8WTLi2tJFRbY0tba0NDQzMv2qUP91829K3NtFehn4uWcQrf+L7a/80hoAYMyJarPziy2uCoDOLQDI3fti0zgAgKSobx3Xv7oPTTwviQJBuo2xcVZWlhGXwzISF/QP/U+Hv6GvvmckPu6P8tBdOfFMYYqALq4bKy0lTcinZ6QzWRy64Z+H+B8H/nUeBkGceA6fwxNFhImmjMtLELWbx+YKuGk8Opf3n5r4D8P+pMW5FonS+BFQY4yA1HUqQH7tBygKESDR+8Vd/6NvvvgwIH554SqTi3P/7zf9Z8Gl4iWDm/A5ziUohM4S8jMX98TPEqABAUgCKpAHykAd6ABDYAasgC1wBG7AG/iDEBAJVgMWSASpgA+yQB7YBApBMdgJ9oBqUAcaQTNoBcdBJzgFzoNL4Bq4AW6D+2AUTIBnYBa8BgsQBGEhMkSB5CEVSBPSh8wgBmQPuUG+UBAUCcVCCRAPEkJ50GaoGCqDqqF6qBn6HjoJnYeuQIPQXWgMmoZ+h97BCEyCqbASrAUbwwzYCfaBQ+BVcAK8Bs6FC+AdcCXcAB+FO+Dz8DX4NjwKP4PnEIAQERqiihgiDMQF8UeikHiEj6xHipAKpAFpRbqRPuQmMorMIG9RGBQFRUcZomxRnqhQFAu1BrUeVYKqRh1GdaB6UTdRY6hZ1Ec0Ga2I1kfboL3QEegEdBa6EF2BbkK3oy+ib6Mn0K8xGAwNo42xwnhiIjFJmLWYEsw+TBvmHGYQM46Zw2Kx8lh9rB3WH8vECrCF2CrsUexZ7BB2AvsGR8Sp4Mxw7rgoHA+Xj6vAHcGdwQ3hJnELeCm8Jt4G749n43PwpfhGfDf+On4Cv0CQJmgT7AghhCTCJkIloZVwkfCA8JJIJKoRrYmBRC5xI7GSeIx4mThGfEuSIemRXEjRJCFpB+kQ6RzpLuklmUzWIjuSo8gC8g5yM/kC+RH5jQRFwkjCS4ItsUGiRqJDYkjiuSReUlPSSXK1ZK5kheQJyeuSM1J4KS0pFymm1HqpGqmTUiNSc9IUaVNpf+lU6RLpI9JXpKdksDJaMm4ybJkCmYMyF2TGKQhFneJCYVE2UxopFykTVAxVm+pFTaIWU7+jDlBnZWVkl8mGyWbL1sielh2lITQtmhcthVZKO04bpr1borTEaQlnyfYlrUuGlszLLZVzlOPIFcm1yd2WeydPl3eTT5bfJd8p/1ABpaCnEKiQpbBf4aLCzFLqUtulrKVFS48vvacIK+opBimuVTyo2K84p6Ss5KGUrlSldEFpRpmm7KicpFyufEZ5WoWiYq/CVSlXOavylC5Ld6Kn0CvpvfRZVUVVT1Whar3qgOqCmrZaqFq+WpvaQ3WCOkM9Xr1cvUd9VkNFw08jT6NF454mXpOhmai5V7NPc15LWytca6tWp9aUtpy2l3audov2Ax2yjoPOGp0GnVu6GF2GbrLuPt0berCehV6iXo3edX1Y31Kfq79Pf9AAbWBtwDNoMBgxJBk6GWYathiOGdGMfI3yjTqNnhtrGEcZ7zLuM/5oYmGSYtJoct9UxtTbNN+02/R3Mz0zllmN2S1zsrm7+QbzLvMXy/SXcZbtX3bHgmLhZ7HVosfig6WVJd+y1XLaSsMq1qrWaoRBZQQwShiXrdHWztYbrE9Zv7WxtBHYHLf5zdbQNtn2iO3Ucu3lnOWNy8ft1OyYdvV2o/Z0+1j7A/ajDqoOTIcGh8eO6o5sxybHSSddpySno07PnU2c+c7tzvMuNi7rXM65Iq4erkWuA24ybqFu1W6P3NXcE9xb3Gc9LDzWepzzRHv6eO7yHPFS8mJ5NXvNelt5r/Pu9SH5BPtU+zz21fPl+3b7wX7efrv9HqzQXMFb0ekP/L38d/s/DNAOWBPwYyAmMCCwJvBJkGlQXlBfMCU4JvhI8OsQ55DSkPuhOqHC0J4wybDosOaw+XDX8LLw0QjjiHUR1yIVIrmRXVHYqLCopqi5lW4r96yciLaILoweXqW9KnvVldUKq1NWn46RjGHGnIhFx4bHHol9z/RnNjDn4rziauNmWS6svaxnbEd2OXuaY8cp40zG28WXxU8l2CXsTphOdEisSJzhunCruS+SPJPqkuaT/ZMPJX9KCU9pS8Wlxqae5Mnwknm9acpp2WmD6frphemja2zW7Fkzy/fhN2VAGasyugRU0c9Uv1BHuEU4lmmfWZP5Jiss60S2dDYvuz9HL2d7zmSue+63a1FrWWt78lTzNuWNrXNaV78eWh+3vmeD+oaCDRMbPTYe3kTYlLzpp3yT/LL8V5vDN3cXKBVsLBjf4rGlpVCikF84stV2a9021DbutoHt5turtn8sYhddLTYprih+X8IqufqN6TeV33zaEb9joNSydP9OzE7ezuFdDrsOl0mX5ZaN7/bb3VFOLy8qf7UnZs+VimUVdXsJe4V7Ryt9K7uqNKp2Vr2vTqy+XeNc01arWLu9dn4fe9/Qfsf9rXVKdcV17w5wD9yp96jvaNBqqDiIOZh58EljWGPft4xvm5sUmoqbPhziHRo9HHS4t9mqufmI4pHSFrhF2DJ9NProje9cv+tqNWytb6O1FR8Dx4THnn4f+/3wcZ/jPScYJ1p/0Pyhtp3SXtQBdeR0zHYmdo52RXYNnvQ+2dNt293+o9GPh06pnqo5LXu69AzhTMGZT2dzz86dSz83cz7h/HhPTM/9CxEXbvUG9g5c9Ll4+ZL7pQt9Tn1nL9tdPnXF5srJq4yrndcsr3X0W/S3/2TxU/uA5UDHdavrXTesb3QPLh88M+QwdP6m681Lt7xuXbu94vbgcOjwnZHokdE77DtTd1PuvriXeW/h/sYH6AdFD6UeVjxSfNTws+7PbaOWo6fHXMf6Hwc/vj/OGn/2S8Yv7ycKnpCfVEyqTDZPmU2dmnafvvF05dOJZ+nPFmYKf5X+tfa5zvMffnP8rX82YnbiBf/Fp99LXsq/PPRq2aueuYC5R69TXy/MF72Rf3P4LeNt37vwd5MLWe+x7ys/6H7o/ujz8cGn1E+f/gUDmPP8usTo0wAAAAlwSFlzAAAOxAAADsQBlSsOGwAACYVJREFUWEftV3twVNUd/u6+N1lIyANnYIBACO8gOh2QQnwU0GJErFhBhQCCUEcqyhQQR0oRFTuAgmiptahVsFT7mKKgFLSAjuURVFopgRgSJA8CyW6S3c2+7+l3zt4lJNnQxP7RGdtv5jfnnHue9zu/10Hg9TeFb+VqkUBo/wHReE+R0RIidr5WeK6fYLSSw7tkqQgfOiKi5RUiWlkl/Os2iNCevUZv16BHo0btvwvNv/kloVdWw/XsU5AIvbcbwa2vIe1P76h27KsyeO9fgLR9H0CzWdW3tqjvNwiOmTNgzh8BU1o6fEsfg/Phh+CYNweea8cAugBMmjG6DSzsMgHCTGlyo/uOP8Ay8hoEf/cGml94GqbuLoB94BiYjXVknaJWlH2agAh6kbryVVhHFcivCv6nZ/L8nwM2GzQ5R05IlIm6hCp1aBY7XD8/zD6NSxKq8xuiedOLcNx1J3zLH0f3ba8jvP8A9AsX4bj7Lgh/MzwjroGIxvgDiVNcBnk4/pgmSQl4YZ8+A651z6uuwNvb0Lx6CUw90lqIsfCoxpxWxMg+EYWWkoq0356QXxX8T/wA0VNHAYe9hZgEIYlSwmhrIS+sN86Go2jdpS6ISAS6x2O04tAbG41aHHpDg1GLQ6+9AL26BqbeveDatAHNGzcjvHOXIkVBbm7hySmaxaIEwRBJCMSl2Sj9fi6mXyJFQuOttZonEQxQpSksRaIeDvKnOc7m4HnrEHxzbXyshFRFM/c3cw0KYlFjTmKuUer8LsekpCP84SvQ68+1aIz3x48i9NpvkOVzqzUDL/0S/hUr4Zg/l4cPkjkdgZe3IMtdx1vsAeHzoa57TzgXLSSpMZgyMxD4xctIeWwpUpYtUWuIZmrM1d9RGqPxgMLnh+OhH0Gz23gY4/bl1URCsN50I6xjx6l5EsF3tsP/1E9g7pEOQW0w9eoNeyG1UP5IYh7VQKM2BLc/p8xcaDrQVIe0P1fyeyr8P52G6OliaowDWjQIy+hbYe4zCCIWia9B0awktKoEkc93wURyRdgHy/CbpIXH4Vr/LFKfWGG0ACd/IFZZhZQlD8OUnY3Qu7sAqwXRo8dgu3kiov/4Eqnr1/JANpLzIGjl6uYTpCSDJDN1ZcsenQZv2tS7L/3YIuNDCwIvrwaiYfoREhPw0awXKVLagdphHT8N1msnGh9a4Fs+lkRTs6gjcg37jDUtpqTZ7TBd1dNoxeFauwa+R5eqenjne+i2eSMCr7yq2sFfv0bSFkM/X8v1BLzTZ8H1/DrV1xE03n7T9Jloum8Omu4tisusInhnz0Zj4S10lKXGyCSIUt3bQHdfoOlsoEakUqGptQ4XnA+sMXqTINRsVFoQencjYjVf0ZRIbNAH6w2zYerZv4WYBMIHP4F34UNGi3sWzYR76NVwkoTY6VKkrFiG+pzBsN95B2JlZWT3h2gYPR72ovuMGR1Dkh/59G+I7D+IyIFPEOFeUsJ/2aMih3lgnjEyCaztI6Jv+b3Q0jKVVciI5lz8XLyjIzhSjEocgpoWfOtn0FzpZFkGCCucc9arvnbECK9XaUECpgED6HQboZedQbTkFPSKCgi3G9GzX7N9muSUQ6+rh3nQFX7qcsjwIJ1iQqSds+z+5g5jQBLY7Ige+xSe7/WHZ0IOPJMoN/eFXllGH2GjjwvDnDsCtoIpxoT20FLTENi4AE2z+6FpDmVuDrzzc6B1z1KOXvgbYL97Fcmh/yPaEWMvnEzn9XujxcAwcAAyTn+J8IGPYb/9NoR270Gm5zxiJ06osaKOzm4nc4+8gcaMjiHCEZjzcmEeMhjmwYOUmHL6wblqFW8tiV8woCKU9AEWag2JUEJHqUhhv/B64Hpye3xwR5AXoiIU51qkcC2ansZcQUQjDB59YJtwvzE4CTHJYOrmgiV3AAIvboF96hQVYex3TEV41/uIfHEclvzhxsgrQ7jrkf7hB0jfuwvp+3az3I0e+/8K54KFxogrIEZVp2NEgKGdImQpA2qwGc65K+gf+xgDr4BwiOO5xiXxcp0miIbzSHlkmzEojk4neFGaUv3AXHTbsBGC+Y5MpryPL0MW/Y65A21pF65pps6Vj8fzksS28mqYuUqTEn4vb20iiR7VPlz3ZrieMj0erqWeMEKG3qBPsVkYrR6hOQXhmPYgNCczZQOtwzWj0nench2avMxnpGOSwhxGy+wF2w0z1ZwEupT5Bn61Feb+ObBNmoDI0WI60kNIWdw+hCbQlhj1jeQImcNIyIORo3ikjNCRdkPGCUYIohUxMgcZU8BLeUP1JRDcxij56jMMzzSpkJ/huBCu1S0334qYZg9zrLdgSRKuk6FTppSAc8E85g2vqHpQJnNXIEVB/r/knSKY2UpBaio0muYlccVFBIN8J/0xPi8BNZdz5Pwk4VpqipbBFIM+Q0vPQuTQ+/R9h4xeifjel9YgeZ1Fl4iR6LbpOVy0uuB4oMVRdQRBldUbacOXSxM1po3oFy/CdmshTWikMZNzpR+R2uXlHF8TU/PWz5EEXKu2cl1m69LEmNb7n5ln9HCNINdo5nw/RZbyCdBJdP0RyVtvvKcIri0vwJyRYXz89uE/fl1/W9FlU/pfwf+J6QBdIkbQux8//nejFcfJkyVGrTXcfDY0B+LO7uLFOlW2RX19PXQZMZLg66/PqbL2wgXU8clxJTTSqbdFQ2Mjztech48v+m+CLvmYHTvexrBhw/DF8eO4bsxoFQlP8GmQmZmBercHd0y9HU+ueRq33DyJh21EQcE4HC3+DCUlJRiUl0e/rSOVqb/OB1v5mQr0H9AfR5kPFc2aieqaGnz+2RcYMWIYys6cIRluDOH7q+dV2ThypBhDhg7BqVOn1R579+5Dds8sRVjfvn1xjiTmcf1Dhw9j/ry5+Oij/Sg5VYLx48ahtLQUWdnZKC+vwA3XF2AA9+wMuqQxPZhsneGhs7OzUHzsGH+6WN2Mna/mKPMMk0nDMP5AVXUlsrIyEWRu4m1qwsDcXBJXj4aGBlSUn6WWncLY665DGbPpKbcVoqqqGpWVlRg1aiSJ/ifS09IwfPhQPomsXKsa0ZiutMvCjLmG7fyR+QgFw1RhKNK9zJhlOXny91FbW0vC6pCbO5B7uuH2NMDv9yM/fwTOkJxOQ2pMV3CypEREIhERDIVUW9YvR6JdVVUlqMbM6XTmZlFVSoTDrce3xcGDH4vtb+0wWnFUnD0rSku/4prVxhe5TliEQ2FVDxlnuRyJfc6dq2x3xn8PIf4FbOMqisII8gQAAAAASUVORK5CYII=''
    })

}', 1, N'portrait', N'[{"content":"STT","value":"stt","row":1,"colSpan":1,"rowSpan":1,"halign":"center","valign":"middle","width":"0","dataType":"text"},{"content":"Mã VTHHTP","value":"vthhtp","row":1,"colSpan":1,"rowSpan":1,"halign":"center","valign":"middle","width":"0","dataType":"text"},{"content":"Tên VTHHTP","value":"vthhtp_ProductName","row":1,"colSpan":1,"rowSpan":1,"halign":"center","valign":"middle","width":"0","dataType":"text"},{"content":"ĐVT","value":"vthhtp_UnitId_UnitName","row":1,"colSpan":1,"rowSpan":1,"halign":"center","valign":"middle","width":"0","dataType":"text"},{"content":"Quy cách","value":"vthhtp_Specification","row":1,"colSpan":1,"rowSpan":1,"halign":"center","valign":"middle","width":"0","dataType":"text"},{"content":"Số lượng","value":"so_luong","row":1,"colSpan":1,"rowSpan":1,"halign":"right","valign":"middle","width":"0","dataType":"number"},{"content":"Đơn giá","value":"don_gia0","row":1,"colSpan":1,"rowSpan":1,"halign":"right","valign":"middle","width":"0","dataType":"number"},{"content":"Thành tiền","value":"vnd2","row":1,"colSpan":1,"rowSpan":1,"halign":"right","valign":"middle","width":"0","dataType":"number"},{"content":"Ghi chú","value":"ghi_chu","row":1,"colSpan":1,"rowSpan":1,"halign":"center","valign":"middle","width":"35","dataType":"text"}]', N'[{"content":"Thuế GTGT","value":"","row":"2","colSpan":7,"rowSpan":1,"halign":"center","valign":"middle","width":0,"dataType":"text"},{"content":"{vnd1}","value":"","row":"2","colSpan":1,"rowSpan":1,"halign":"right","valign":"middle","width":0,"dataType":"number"},{"content":"Cộng","value":"","row":"3","colSpan":7,"rowSpan":1,"halign":"center","valign":"middle","width":0,"dataType":"text"},{"content":"{tong_cong}","value":"","row":"3","colSpan":1,"rowSpan":1,"halign":"right","valign":"middle","width":0,"dataType":"number"},{"content":"Thuế suất","value":"","row":1,"colSpan":7,"rowSpan":1,"halign":"center","valign":"middle","width":0,"dataType":"text"},{"content":"{thue_suat_vat}","value":"","row":1,"colSpan":1,"rowSpan":1,"halign":"right","valign":"middle","width":0,"dataType":"number"},{"content":"","value":"","row":1,"colSpan":1,"rowSpan":1,"halign":"center","valign":"middle","width":0,"dataType":"text","gruopRow":false},{"content":"","value":"","row":"2","colSpan":1,"rowSpan":1,"halign":"center","valign":"middle","width":0,"dataType":"text","gruopRow":false},{"content":"","value":"","row":"3","colSpan":1,"rowSpan":1,"halign":"center","valign":"middle","width":0,"dataType":"text","gruopRow":false}]', 0, 1, 1, NULL, N'tong_cong', NULL, NULL, NULL, 88, '2023-06-21 04:14:26.9783768', 88, '2023-06-21 04:14:26.9795092', 0, NULL, NULL, 10)
SET IDENTITY_INSERT [dbo].[PrintConfigStandard] OFF

PRINT(N'Add rows to [dbo].[ModuleApiEndpointMapping]')
SET IDENTITY_INSERT [dbo].[ModuleApiEndpointMapping] ON
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13561, 101, 'a1830c4c-fb7f-6d5a-2bdd-1a5af17dc992')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13562, 101, '38da8203-1840-5302-da4f-3615b32808bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13563, 101, '34cfb8e3-a764-809b-2b48-52cd8ab68549')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13564, 101, 'f34b737d-6604-d7db-7a59-557ff4dd9999')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13565, 101, 'de120643-418f-7b19-3841-5864e058dbad')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13566, 101, '3fdb18e6-1bfb-a5ba-a44b-6a07e7c7d631')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13567, 101, '9aae4a5d-033e-46a5-49b8-72feceae691e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13568, 101, '0f83dcc2-3a6e-b046-3985-739bc72c7cf2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13569, 101, '9ed93f20-634e-cf38-d2b8-792a97678135')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13570, 101, 'b67c6e8a-f775-6919-0b6f-7f1bd3b38dc0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13571, 101, '3a7d046f-b536-c536-d93f-a68742128aa8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13572, 101, 'bf59c0bd-a6d8-a50d-7211-af21d24db18f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13573, 101, 'a96633ef-c2ca-c960-ce22-baaf15502522')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13574, 201, 'a1830c4c-fb7f-6d5a-2bdd-1a5af17dc992')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13575, 201, 'e83205b7-fe24-f2a0-5e03-278c053e62c5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13576, 201, '457e6c28-9853-cbda-4004-37c9a11ea8f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13577, 201, 'e22ca1dd-84b2-bd96-346d-3e77b36bf5c6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13578, 201, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13579, 201, '9e7965fc-b030-b9c1-3668-4ce97fa37f99')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13580, 201, '1deb7fa8-8172-e16f-41f5-50178822494a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13581, 201, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13582, 201, '05f2a8fb-1844-dc8d-52a3-55c00ecfb51c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13583, 201, 'ccdca087-21ff-6a83-1c98-672d8c2a06cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13584, 201, 'fe8e1ebf-6a98-1de1-a70f-70bfea23b3bd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13585, 201, '9ed93f20-634e-cf38-d2b8-792a97678135')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13586, 201, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13587, 201, '7af59ef9-708a-7648-5136-7f908d2e7c4b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13588, 201, '2ebb58b9-090d-a511-3951-9c3b2639db4e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13589, 201, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13590, 201, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13591, 202, 'a1830c4c-fb7f-6d5a-2bdd-1a5af17dc992')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13592, 202, 'f164a3e5-63d2-24df-ded8-2757b072e2d4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13593, 202, 'e68660dc-5464-674a-a147-2890a4c1a5bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13594, 202, '181ff160-2293-21ec-c441-32d0bd6b3549')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13595, 202, '8a850dad-0354-7d29-1ab8-4c7a6ebdca8e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13596, 202, '34cfb8e3-a764-809b-2b48-52cd8ab68549')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13597, 202, 'de120643-418f-7b19-3841-5864e058dbad')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13598, 202, 'f79412da-668e-b3d6-e645-59821bfbfd43')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13599, 202, '86065638-1a9d-40e0-edd2-5ddbfe647041')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13600, 202, '8d4fc2c3-4160-48a4-85c5-5f8461c1942d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13601, 202, '8d78dfeb-06a2-e865-fbd1-69044f2f267f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13602, 202, 'c8c8bf0c-3dc8-f9c9-344b-6fee89f6a615')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13603, 202, '0f83dcc2-3a6e-b046-3985-739bc72c7cf2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13604, 202, '5ea125bd-ca15-ab73-cf7f-7b9319f3eb64')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13605, 202, 'b67c6e8a-f775-6919-0b6f-7f1bd3b38dc0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13606, 202, '3773e371-a7fb-adaf-3e07-8a35bcada0f7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13607, 202, '274223b8-7958-e1fe-071f-9ba04c3129f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13608, 202, '6889aea2-a4d6-5179-5a00-cc49c8696a88')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13609, 202, 'a5c3b934-5ab2-711e-f05b-cd197de3522f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13610, 203, '80a5dea1-9033-1b00-0320-03f600ef40fa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13611, 203, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13612, 203, 'c46e5565-62d6-90ce-32e9-361eb723d53e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13613, 203, '08ac0b32-5770-cd32-d82d-57475ea70898')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13614, 203, 'ee83ac7a-88b2-29e3-0a2c-fa22b296df1b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13615, 204, '10e1b95a-8505-0f42-0e07-39d3d4f32177')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13616, 204, '776a3147-338e-1342-96ef-7947b903411f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13617, 204, 'ae5148dc-9827-ae65-d639-7e771e26d5e4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13618, 204, '177b2907-c68b-6090-b4ff-9c33dabdbd27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13619, 204, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13620, 205, '419edfc9-397f-585e-899f-2b41ca9598ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13621, 205, 'aef5419f-5076-7179-8dd4-38d2af9836e2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13622, 205, '1deb7fa8-8172-e16f-41f5-50178822494a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13623, 205, 'd32272ff-d800-a201-3614-6126e9883ddf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13624, 205, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13625, 205, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13626, 205, '91ebeacc-271e-9d89-ee88-dee121e69bd2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13627, 205, '0b4f8e4c-5dd0-fab2-25ad-ed6c67142ad0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13628, 205, 'c32bc170-7397-f950-1ead-fc907ebffcf3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13629, 205, 'c0d4e20e-0473-8f5a-c1e0-fdd0efdd96de')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13630, 206, '82cabde4-e579-0875-6b19-04059f44a643')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13631, 206, '3bd41858-f83f-ca79-8969-092cabc967ca')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13632, 206, 'cde3139d-a080-43b2-2051-0b55f96e8855')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13633, 206, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13634, 206, 'dda5ce9e-58e9-7032-f2c9-0ef33bad32f7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13635, 206, '5b2ec155-5ce8-72b3-4b9d-11bbff0a14c0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13636, 206, '45a042bb-0af8-e229-f4a5-13d946c98ccc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13637, 206, '269441f7-136d-1dee-5da9-171d057eeff4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13638, 206, '657d91b6-a78f-836c-4eb7-20c1d3083cf4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13639, 206, '1f06492b-f1bd-0914-b32d-23719ac7f969')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13640, 206, 'c7de07b0-a8b9-92b8-d7b1-245092f5295f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13641, 206, '3ec51946-cdfb-07e6-7892-254bfa16c9a4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13642, 206, '92db7ba4-2135-ecff-4c94-277c3733ef0d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13643, 206, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13644, 206, '0355e35c-f518-907c-8595-37d41c9761e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13645, 206, 'c40f6db4-cda1-6172-8919-3bfae20f4cb4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13646, 206, '3b877e49-c7a7-6a43-3ab5-3ee70a8ff318')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13647, 206, '2ba02b05-b91a-5033-12aa-44ee85d51bb3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13648, 206, '1deb7fa8-8172-e16f-41f5-50178822494a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13649, 206, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13650, 206, 'ccc3f045-d042-00c0-93fa-55817ad4141d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13651, 206, '618ab866-579d-9aec-ed3a-5a48ed92ae31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13652, 206, '8e305d86-a84a-3359-8801-5bec74e48ba9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13653, 206, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13654, 206, '4f2e8e19-62e7-731e-a2b7-60f9f32d73df')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13655, 206, '3d8b5c92-9630-6715-76e0-618f1c0d82d2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13656, 206, '761e8aac-4ba8-77ef-4dcd-6a3dd8321b25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13657, 206, 'c73556c2-0514-d67f-8f35-7584e834fbee')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13658, 206, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13659, 206, '7d9bde30-dda1-79d2-49ca-77513c8b8c46')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13660, 206, '6deb8018-42b7-4934-0442-776c7090cf6d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13661, 206, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13662, 206, 'b9dff692-9331-c84b-08d4-8ce726d03f37')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13663, 206, 'eb3629ab-bd2d-e47a-9856-94bf3b7c3296')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13664, 206, 'c1509851-00cb-03ea-5381-94eed3c1ff5b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13665, 206, 'b4e15d25-b21e-6d1c-2680-96de52887aa6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13666, 206, '68428694-eae1-e424-e203-972160b383a1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13667, 206, '491a587a-a49d-1743-39ef-99595937a87e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13668, 206, 'f4d94902-5489-fb68-0af8-9a8cedeaf61b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13669, 206, '5244e475-500b-3a5b-b8eb-9bcfc5fb5ef8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13670, 206, '36a26e26-972c-a80c-5ad6-9eadb0be95ea')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13671, 206, '55e396ff-71c9-e9dd-9561-a1d52f3ecef1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13672, 206, '039aab51-27ab-6106-66e4-a2a2a6e785e2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13673, 206, '19812772-6ef8-8936-f1b5-b236ad6e071b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13674, 206, '9d411636-6f22-8c31-948b-b73d7afd50c6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13675, 206, '39e1636f-e2b2-3800-f459-b7e8e08a9d90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13676, 206, 'e58c5e2f-40c7-e8aa-2fa0-b89305f1c59b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13677, 206, '3a4edcdb-8598-c022-1b7b-b95db5b1e17d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13678, 206, '9b2c1609-8fe8-2cab-3021-c216c206afbe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13679, 206, '29c2a358-2f38-67f0-127f-c2214d0355a7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13680, 206, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13681, 206, '9284338e-79e0-5bca-d8fa-dbd305d9d24c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13682, 206, '05272a9d-9106-b201-aff4-decf1253be2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13683, 206, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13684, 206, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13685, 206, '928dc3dc-bf62-8ed2-7ca6-e90ac42a7383')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13686, 206, 'ffebc1d4-4bda-c5ee-2669-eab3c9693639')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13687, 206, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13688, 206, '743d9a82-7ded-4a06-7852-eecec9d53db2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13689, 206, 'bb544002-e927-22c0-6750-f2efc4704a0b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13690, 206, 'ee83ac7a-88b2-29e3-0a2c-fa22b296df1b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13691, 206, '63bb9079-ab38-e008-e836-fc87c24dfb53')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13692, 206, 'c32bc170-7397-f950-1ead-fc907ebffcf3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13693, 207, '645739a4-2381-9990-af99-070773f3597b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13694, 207, '1cfb6a17-2002-d704-0c0f-191286b3a7ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13695, 207, 'a1830c4c-fb7f-6d5a-2bdd-1a5af17dc992')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13696, 207, '657d91b6-a78f-836c-4eb7-20c1d3083cf4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13697, 207, '419edfc9-397f-585e-899f-2b41ca9598ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13698, 207, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13699, 207, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13700, 207, '41791b1d-d3b6-6fa2-f724-3e3b8dcf6b99')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13701, 207, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13702, 207, 'd39689de-8203-b6da-19a9-452c213ba6f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13703, 207, '6625857d-c5e6-2507-2bb5-51f0b00fa9d0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13704, 207, '69e419bf-ed25-e07f-07fe-5bdbbe00cabf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13705, 207, '00cfe926-098a-04f4-abd4-5e4d912715f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13706, 207, 'b48b84b8-0135-7001-fac2-60e564a66f8f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13707, 207, '75fa201c-f28d-1a69-26fe-6ece2b18c50c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13708, 207, '9a5cedba-20df-2207-a59a-74d91dcc066c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13709, 207, '7d9bde30-dda1-79d2-49ca-77513c8b8c46')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13710, 207, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13711, 207, '1051edcf-77ea-df08-e4a4-7ed5d5f7a085')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13712, 207, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13713, 207, 'a519d12c-0083-96e3-4047-91c766339587')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13714, 207, 'adec3c14-69d2-3f1a-62bc-9c549efa7fb1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13715, 207, '02415db3-b0bd-9c93-6512-aab3cdf990e2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13716, 207, 'e10a5241-6a30-1eb1-5e90-b06c4c323cfe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13717, 207, '7c81c5dc-c437-7ced-e982-b26dbec5edcc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13718, 207, 'eb6fb1fc-86fa-dce6-9d06-b340452ddb97')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13719, 207, '79de5dfa-bee6-f034-e1c1-b3550dcf2759')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13720, 207, 'a5784fca-53b9-101b-664a-bedbb4bada81')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13721, 207, 'b5a1e16b-3010-3c7e-c8f2-d1122baa76fe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13722, 207, '05272a9d-9106-b201-aff4-decf1253be2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13723, 207, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13724, 207, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13725, 207, 'cfd75a84-863c-1d7f-22aa-f3ef39a9f5c6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13726, 208, 'c5244088-10e2-97ab-41a6-01d46a476de9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13727, 208, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13728, 208, '24a1861e-a9e2-6fcc-4d8d-326944a2d785')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13729, 208, '32ace0fc-7c42-9f69-b477-35c414b33d4d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13730, 208, '1deb7fa8-8172-e16f-41f5-50178822494a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13731, 208, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13732, 208, '7b104afb-3a6e-3a8c-8dc8-5b3a593730d0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13733, 208, '10872b16-896f-a345-40c9-65fea2849e37')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13734, 208, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13735, 208, '5e4c14d1-b55e-ca72-b05c-893dd7a63ada')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13736, 208, '48554b34-16a9-c1df-5510-93e0ac91873a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13737, 208, 'bffaf927-4720-9f8f-4f95-c4a08b7edb25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13738, 208, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13739, 208, '7ad9fac7-b49a-ca46-9dd6-f03f2aa46dcf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13740, 210, '1deb7fa8-8172-e16f-41f5-50178822494a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13741, 210, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13742, 210, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13743, 210, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13744, 211, 'ef81f188-8081-53e1-b901-4881df88d66f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13745, 211, 'fdaf8699-f636-845f-eda3-5805a83d5007')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13746, 211, '9ab56802-7601-b00d-b3f0-6f46dc445708')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13747, 211, 'c73556c2-0514-d67f-8f35-7584e834fbee')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13748, 211, '7e9ce0da-eeaf-763c-acdf-9411745ba4bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13749, 214, 'ed2eacef-505c-ea04-90e9-786215116606')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13750, 214, '10ff3607-d3b1-3f89-8bbc-94ebf2224b1c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13751, 216, '419edfc9-397f-585e-899f-2b41ca9598ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13752, 216, '1deb7fa8-8172-e16f-41f5-50178822494a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13753, 216, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13754, 216, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13755, 216, 'c0d4e20e-0473-8f5a-c1e0-fdd0efdd96de')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13756, 217, '746f30fa-4010-7dde-b778-0eca78f9e97b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13757, 217, '5f3fd1f9-7048-efce-8d1f-22fab3cc4da7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13758, 217, '07ca6181-bc81-4a73-832e-64baff1ea8ea')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13759, 217, '947cbe09-78cc-2a43-7c8c-699899db33a6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13760, 217, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13761, 217, '37645333-5a5e-68f7-293c-a8ef00e8b788')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13762, 217, 'da2d29e9-31a6-6ef0-0b2f-c9dc1a07b2e9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13763, 217, '23b7ec7b-1909-9ac7-053a-ee155ccba6fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13764, 218, 'b6d82e57-a01d-2f2b-ff30-64bd94b1e4f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13765, 218, '9aae4a5d-033e-46a5-49b8-72feceae691e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13766, 218, 'a96633ef-c2ca-c960-ce22-baaf15502522')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13767, 219, 'a28fc284-693b-1ba3-164e-030357fb3218')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13768, 219, '42fab4d7-d847-3239-bac2-09b01c6175d8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13769, 219, '348dbb92-ffad-e2cf-f73f-258f83a3b1e1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13770, 219, 'd3d9a07b-e32b-4b22-9240-27eeaefffab7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13771, 219, '87297a65-2af2-79af-d869-36c6edd76d87')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13772, 219, '50f26f98-f576-8b8c-e5d5-44d3b7796032')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13773, 219, '1deb7fa8-8172-e16f-41f5-50178822494a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13774, 219, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13775, 219, '24243cbe-e764-0bd4-4672-534ef681b239')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13776, 219, '49b666d2-c67e-4a8e-3f40-5392215ae76d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13777, 219, 'ca685a5b-d711-449c-c23d-645487b6cc7d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13778, 219, 'c6942cd1-ced3-f844-587c-6a7ec1f19185')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13779, 219, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13780, 219, 'c88815ee-a01a-4140-0ed9-799b4a31ffa0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13781, 219, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13782, 219, '7c3812ed-0ec0-14a1-8341-7be34d0bf84a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13783, 219, '3a31e095-67d8-fcd5-8e7f-8c8c108a21da')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13784, 219, '00a8fbd3-3b4d-c119-8d0e-910e8dcb682b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13785, 219, '68eb2e92-0043-675f-51c7-941bb40c2a5f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13786, 219, '52da5d85-66cf-7f6f-5a14-974f520afce7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13787, 219, '44f215c8-0d96-dfd4-ceaa-99261a2cc486')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13788, 219, '08adec44-1592-83d1-a41f-9c96a15afd84')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13789, 219, '34fc5611-9e08-ad65-38dd-aafdfe5d7724')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13790, 219, '53a56208-12d5-e2bd-cc89-b0b651a1627b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13791, 219, 'e144df1e-e702-7e7a-1dd4-b3efb302efc9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13792, 219, '200de375-d8c3-5a14-62f4-b518b1daf901')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13793, 219, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13794, 219, 'b8b19d5f-7479-ab90-f681-cc72c19a2a35')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13795, 219, '1da229d7-5d53-9e62-5b1f-cc891cd4d3be')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13796, 219, '6153f3b6-3075-494a-06a0-ccb534e848bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13797, 219, '2f7a399d-d0f6-7375-c60b-d6bf76cdbef5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13798, 219, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13799, 219, '49719f37-e9d0-9670-ad15-f4267f3ee5c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13800, 220, '075d5d2c-d312-8e69-ac79-13a27c9c998e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13801, 220, '1ed4a3e5-313a-1877-8acf-65c9fcb74294')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13802, 220, '82b36bba-75fc-9ce6-e0c1-76c032abd3bd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13803, 220, '5d0ed1e3-586a-c040-e556-813f93a49f41')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13804, 220, '804ca71a-9e9d-18a4-9104-dfea1540bf58')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13805, 220, 'f9a371b0-0973-620e-0a08-f5ea5a252beb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13806, 221, '4e1b15dd-da74-0025-8cb7-0f566dbbf8da')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13807, 221, '2e63f7db-31f8-71e0-d327-233f7a654fca')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13808, 221, 'f3adbbda-b2bd-c8a9-0145-3ef1b2bff22f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13809, 221, '2cb9dca6-2a0c-19b4-54b0-4aff685be219')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13810, 221, 'f2c42a50-8944-1dd9-2431-4b24b8a31071')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13811, 221, '8a850dad-0354-7d29-1ab8-4c7a6ebdca8e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13812, 221, '91d28f13-aae2-3de6-6c1b-4d976b9f9289')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13813, 221, 'f61727b3-12ca-cf54-b12d-719f4637ab67')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13814, 221, '82e9b4d7-ee8e-ce0d-95dc-7bc1857a44fc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13815, 221, 'ebf06e14-6634-43f0-c871-7e13f91be688')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13816, 221, '0525fc12-130a-b914-e1f3-83c0d5d324df')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13817, 221, '7ca6b956-c2ab-1d2a-fe68-94605212ccd9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13818, 221, '8d66e47e-6ee0-7b8f-43da-9827fc1c3e2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13819, 221, '28520fa1-2f60-780d-8684-9d1153bb0644')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13820, 221, '864ac9ae-b5db-3fc1-e5d0-a2ba2198590c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13821, 221, 'ee442aa8-4992-e500-0036-a2dea487382f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13822, 221, 'a5c3b934-5ab2-711e-f05b-cd197de3522f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13823, 221, '437d94fa-5d65-2d8a-760f-ce25c876ca08')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13824, 221, '3037ec80-f675-f357-35f8-d0417a0dccbf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13825, 221, '03685f01-edd7-73a2-e791-d15c70b8666b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13826, 221, 'c10ca7e0-43fd-bddf-d7f1-d5accbb7111c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13827, 221, 'a019095d-9fc3-37f0-dad1-d91f726d4593')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13828, 221, '61a88a8e-5e6f-f389-6d90-e0c54b90bdde')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13829, 221, '9e4b00ba-7ed8-9857-f735-e315586f5330')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13830, 222, 'c6c0aed4-1c49-c46c-8717-0be9ec4c974c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13831, 222, '1468d185-382f-bcf6-f2e8-5fc355e6b746')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13832, 222, '90e2f938-3f86-92ec-c517-a5b0697e056e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13833, 222, '7e3dff45-15f8-5ceb-7e46-b019e12b02b0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13834, 223, '3fec3eb2-e34d-159f-6b99-01bd1bbfc633')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13835, 223, '5fc35140-e561-d522-666a-49e17376cd7c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13836, 223, 'f57a7e4d-530c-b6f2-c181-4d014cafecec')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13837, 223, 'ccdca087-21ff-6a83-1c98-672d8c2a06cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13838, 223, 'c6230f3d-e06a-9db3-9dbc-7c81b864c1ab')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13839, 223, '21780e71-63fe-8a65-5d44-7e998ca04ad4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13840, 223, 'd46aadf9-e0f5-9885-a895-be49bbbdce6e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13841, 224, '928ac568-ddc1-b125-3805-14b70fc68eb8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13842, 224, '38da8203-1840-5302-da4f-3615b32808bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13843, 224, '891a663a-bdd5-bdf1-2738-3ae8b6f1492f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13844, 224, '0b84431b-6cbd-594a-fdce-6eb56141b889')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13845, 224, '0479c0b7-a32d-0510-daf6-a02b3c8cf771')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13846, 224, '3a7d046f-b536-c536-d93f-a68742128aa8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13847, 224, 'fbb3f5a5-86c8-e438-72ef-afe745b370f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13848, 224, 'd189e09a-3c0e-8c84-8265-b38d852ecd3d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13849, 224, 'b5ace500-4c8d-8ca8-8fd6-cb64d541d9d5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13850, 224, 'fd008ce0-2042-e7c3-a650-e1070b58c675')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13851, 224, '8c2561b3-f6f4-5f29-40e7-e206a185d2d6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13852, 224, 'e702a910-9ba7-5896-fd71-e987568abdef')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13853, 226, '018172a5-33e5-d95d-07c4-0f9cb043e8a4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13854, 226, 'b1579df7-21d4-ffa3-6efb-6744bd5ec5f4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13855, 226, 'ca0252d6-0932-fe2b-99d0-7dbc204ed214')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13856, 226, '8c5f7427-c71a-4e5e-c504-90cc53a0e9c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13857, 226, '20596813-1740-fe4f-f401-952b3185e305')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13858, 226, '36f54324-aa54-3d49-a9ec-b9b5eb365f92')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13859, 227, 'dd54cb65-a57d-4f6b-9b83-29b3a6ddfd0b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13860, 227, '419edfc9-397f-585e-899f-2b41ca9598ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13861, 227, 'b0ff1f4a-7735-a27f-4eb6-3a74ec704a1b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13862, 227, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13863, 227, 'bffaf927-4720-9f8f-4f95-c4a08b7edb25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13864, 227, '703edae0-a31f-02fc-5ac6-e4f45309e867')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13865, 227, '477e67ca-4cee-9536-3b68-ee350a53af24')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13866, 230, '41ff090a-e00b-486d-9f9a-812d93d14f4b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13867, 230, '57ed647a-162b-2b94-fbf0-c995e654d1a7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13868, 231, '8ad1c21b-9df2-eef9-b47d-08a009edb8d5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13869, 231, 'cce21a8e-93ae-7062-c801-398104f37f22')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13870, 231, 'c868a586-e5d8-21b2-111f-3b0df515396e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13871, 231, '618cbc00-263b-bbd7-b3da-4e42e269eb2f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13872, 231, '71e564ff-ef65-1370-19c6-54e09d897e27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13873, 231, '7c0a7955-cc7f-1654-b6cf-57b37f3bc4a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13874, 231, '5c5a0f99-0f12-19e6-575b-627fe1a80a5c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13875, 231, '4a93fce9-7c4f-c1e2-6134-956d76f909bc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13876, 231, '783bc12f-5b88-d2d5-b54f-ab99ca273943')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13877, 231, 'ebf3d209-4304-be58-23e4-ad2cbe639cdb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13878, 231, '414a030b-812d-ad12-6c18-cb844925c577')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13879, 231, '350cce3c-e086-3cee-90ce-e1c68f28f75c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13880, 231, '1b5f79cd-4c2d-8b93-7f5a-f5a3f2664f81')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13881, 240, 'b8759390-ca05-7ead-4258-0f60d6d7f216')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13882, 240, '1873c28c-2d9b-395c-9f05-2e0d59d6a9ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13883, 240, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13884, 240, '42b3aa20-92a5-dbad-673e-698ef6cda850')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13885, 240, '45ae71b0-7a5d-3c79-c414-77ddd8792c73')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13886, 240, 'c29c8caf-3012-606b-5d5b-94f45d690f47')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13887, 240, 'e23939d4-9517-2697-e28d-9a4f98d5ba40')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13888, 240, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13889, 240, '747e6181-bd93-0648-199f-ec8f23c11c86')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13890, 240, 'c4683230-bce3-7eeb-e121-faabfc8b529a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13891, 241, 'ded59ca9-0829-7874-72dd-6fe9349d7ebb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13892, 241, '8080af47-5656-6aca-1360-9124f40e3177')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13893, 241, '193a2f93-0919-be19-c0d2-b34269d9c54f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13894, 241, '4aba7101-e13b-3611-7b93-f112dc30e0cb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13895, 242, '49b8c01e-4279-b995-eb00-0675b3e4b85f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13896, 242, '48be3fb2-6eb9-9880-dcab-1e4538a7ea8c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13897, 242, '9f9580cf-effc-e634-812e-ad4c738f1a1c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13898, 242, 'e62065a2-8fc7-7d88-53c5-c47474266e19')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13899, 242, '743d9a82-7ded-4a06-7852-eecec9d53db2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13900, 250, '70c46bff-42f0-40c7-417c-0f56214f70a6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13901, 250, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13902, 250, '57250a49-cb2a-2f6f-35b2-3f0af03abaa8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13903, 250, '46438258-4610-6b8f-557a-3ff99b3eec90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13904, 250, '154e3808-90cc-10a5-19a2-6d733b5d8cd1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13905, 250, '3166cb16-0c8c-4114-4d74-73a1a5edd501')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13906, 250, 'c238fbac-4f5b-a6f5-abd3-74fc27335e27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13907, 250, '1e7621e0-ec78-3c91-2305-772cb30606b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13908, 250, '9ed93f20-634e-cf38-d2b8-792a97678135')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13909, 250, 'fa807f19-dda9-88f0-210c-7c8ed288372d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13910, 250, '093a1ffd-ce57-7832-db74-7cecaca5cace')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13911, 250, '895fc889-097e-179f-3bca-845cb47bcae3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13912, 250, '9bdccbd6-217f-fc46-2512-96596eff6037')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13913, 250, 'f6693773-cb68-d5e8-7879-a845140bba55')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13914, 250, '69c4c8bd-ae39-83eb-2dbb-abdf55c55a70')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13915, 250, '2e0a9e33-ce98-8b60-7ee2-b0a2db320fcc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13916, 250, 'cdb0fbbe-6066-c62f-bd69-b1355c9ddcfc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13917, 250, '4d92afae-baa4-df2a-c45e-b73831eea5d0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13918, 250, 'fdb5b29a-82da-71f2-b855-bfbed458fa96')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13919, 250, '152b8115-a885-ebe0-3a46-c6cf37f54b22')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13920, 250, 'adbdf3a0-de19-dad0-d93c-d6e6878c4a9f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13921, 250, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13922, 251, 'a28fc284-693b-1ba3-164e-030357fb3218')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13923, 251, '348dbb92-ffad-e2cf-f73f-258f83a3b1e1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13924, 251, '87297a65-2af2-79af-d869-36c6edd76d87')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13925, 251, 'a3f3700f-4cec-f090-1ad3-467a3e363f88')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13926, 251, '24243cbe-e764-0bd4-4672-534ef681b239')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13927, 251, 'df46f0cd-e79f-15a8-4bef-5967e925eb65')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13928, 251, 'c88815ee-a01a-4140-0ed9-799b4a31ffa0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13929, 251, '3a31e095-67d8-fcd5-8e7f-8c8c108a21da')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13930, 251, 'e719e5bc-b22a-81ce-dfa5-90dbc18d9725')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13931, 251, '52da5d85-66cf-7f6f-5a14-974f520afce7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13932, 251, '08adec44-1592-83d1-a41f-9c96a15afd84')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13933, 251, '53a56208-12d5-e2bd-cc89-b0b651a1627b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13934, 251, '2cd7b730-d47e-4cbf-012e-e8010c8e013a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13935, 251, '34283a7b-416d-088e-d6e9-f1e9bb0f5bd8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13936, 252, '7e987e5c-f056-f010-5a1f-6c8fa94df3b5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13937, 252, '36f9ad46-1957-8fb1-ffd5-94f7859607f4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13938, 252, 'd189e09a-3c0e-8c84-8265-b38d852ecd3d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13939, 252, 'ff122662-7dce-5e27-8c50-c1ace2185a05')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13940, 252, 'fc99a240-638a-7ab0-9054-c80f28934b5b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13941, 252, 'b0d4505a-26c5-e857-95ac-f61b5fd5cb7b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13942, 253, '9aae4a5d-033e-46a5-49b8-72feceae691e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13943, 253, '85202d1b-ba66-6414-4251-a349a0da2f7a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13944, 254, '8125d95d-006b-912d-5bda-c670d962f33c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13945, 254, 'a6011672-9621-5fb9-dd6a-ea8e846b4da0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13946, 255, 'c166e3cc-e7f3-deea-656a-2e8c70d0bd35')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13947, 255, '3fe99ba4-a784-9164-3fc1-300420463367')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13948, 255, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13949, 255, '204701c5-51c7-e8b1-8fe2-63bc803b2fa7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13950, 255, 'd2eb6fea-4f87-2700-5877-7d45f029af22')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13951, 255, '0cdb2315-a3a9-551a-fee2-86c028440479')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13952, 255, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13953, 255, '2cdf1f0f-c7fa-5223-67aa-90f3445f3f29')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13954, 255, '3cd561ec-de72-7702-e4a8-994f5ee1b635')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13955, 255, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13956, 255, 'be5333cb-2b6a-4239-591e-adc0fa49ed96')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13957, 255, '2865d439-7ac4-bd28-b15f-b6d0c9143402')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13958, 255, '5e20a257-942b-ca0e-68ed-cc5affdf10f7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13959, 255, '1f3d4b19-5101-4c4c-0a5d-d4c428ca3b08')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13960, 255, '72535c4e-cea1-fc65-7961-f4e2112a0bfc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13961, 301, '3dcd8f73-9ba4-bc19-a69d-042a83cf27c9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13962, 301, 'ad0d9f69-1ea2-fe8c-953b-0c81259c9df3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13963, 301, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13964, 301, '5375fe3c-d068-923b-f3f9-18eb45d45b6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13965, 301, '1cfb6a17-2002-d704-0c0f-191286b3a7ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13966, 301, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13967, 301, '23e92047-babf-094c-8fe1-1efa798a09aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13968, 301, 'b9d5efe6-08a2-dd4c-2ea7-20ed3e83cd28')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13969, 301, '1f06492b-f1bd-0914-b32d-23719ac7f969')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13970, 301, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13971, 301, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13972, 301, '457e6c28-9853-cbda-4004-37c9a11ea8f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13973, 301, '0355e35c-f518-907c-8595-37d41c9761e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13974, 301, 'd39689de-8203-b6da-19a9-452c213ba6f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13975, 301, 'b96dbb3e-4729-b77a-9909-47d64a63ccb7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13976, 301, '62e64ead-4fe2-9499-24f8-48df068b0a08')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13977, 301, '9928f2f3-a353-15fa-613a-4dc6af384bbc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13978, 301, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13979, 301, '6625857d-c5e6-2507-2bb5-51f0b00fa9d0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13980, 301, 'bf46459d-341c-6392-e263-5aab5ca69799')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13981, 301, 'ccce0cc0-1ac4-207c-903c-5bee7666d229')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13982, 301, 'b5696f5f-5536-1f89-6d54-5c6757602b38')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13983, 301, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13984, 301, '00cfe926-098a-04f4-abd4-5e4d912715f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13985, 301, '3ca0ae16-7501-4657-720a-64da09be962f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13986, 301, '8a07f59f-ac86-5bca-24a0-70a5363cd1e5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13987, 301, '3166cb16-0c8c-4114-4d74-73a1a5edd501')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13988, 301, '9a5cedba-20df-2207-a59a-74d91dcc066c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13989, 301, '9ed93f20-634e-cf38-d2b8-792a97678135')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13990, 301, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13991, 301, '7c3812ed-0ec0-14a1-8341-7be34d0bf84a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13992, 301, '1051edcf-77ea-df08-e4a4-7ed5d5f7a085')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13993, 301, '41f0ec72-2077-5414-7d03-896cffb03068')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13994, 301, 'e86ad0fa-b426-4656-77c5-8c1726b7723c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13995, 301, '68eb2e92-0043-675f-51c7-941bb40c2a5f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13996, 301, '6d74e250-d74f-a8f4-8a6b-97234dc13056')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13997, 301, '654318f5-e18f-fffc-1afe-98d6196ab4d4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13998, 301, '34fc5611-9e08-ad65-38dd-aafdfe5d7724')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (13999, 301, 'e10a5241-6a30-1eb1-5e90-b06c4c323cfe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14000, 301, 'f69902b8-63e0-4417-db47-b1301949159e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14001, 301, 'cdb0fbbe-6066-c62f-bd69-b1355c9ddcfc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14002, 301, 'e73acb68-2f00-2436-c354-b15302013ad8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14003, 301, 'eb6fb1fc-86fa-dce6-9d06-b340452ddb97')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14004, 301, '79de5dfa-bee6-f034-e1c1-b3550dcf2759')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14005, 301, '32d8d104-2524-9402-a452-bf1f2d95b1e8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14006, 301, 'd95c9e95-bd00-4214-5783-bf8531668b62')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14007, 301, '005a566b-1052-1f18-7030-bffa8ad84a71')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14008, 301, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14009, 301, 'a4ed8260-257e-860d-4dd5-d815a3d7dc5c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14010, 301, '024e6ead-0bc1-a738-cc82-dba2dca51818')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14011, 301, '05272a9d-9106-b201-aff4-decf1253be2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14012, 301, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14013, 301, '2b0de723-db75-8f05-e0d6-e7283643e166')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14014, 301, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14015, 301, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14016, 301, '49719f37-e9d0-9670-ad15-f4267f3ee5c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14017, 301, '0c9d303c-294b-67dd-ebda-f556552c1951')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14018, 301, '5c0a26c5-e588-c838-59ba-f5bf11c09f6d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14019, 301, 'cfde05d2-9814-257b-c6e7-fbbc706bb41e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14020, 302, '3dcd8f73-9ba4-bc19-a69d-042a83cf27c9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14021, 302, '645739a4-2381-9990-af99-070773f3597b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14022, 302, 'ad0d9f69-1ea2-fe8c-953b-0c81259c9df3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14023, 302, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14024, 302, '5a15c78b-93e4-374a-8ce8-0ec520513e5d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14025, 302, '1f06492b-f1bd-0914-b32d-23719ac7f969')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14026, 302, '7f9a0dba-214c-b383-8942-26f13ce4199d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14027, 302, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14028, 302, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14029, 302, 'fad05772-da89-bb59-41f4-3304ea722306')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14030, 302, 'e09ee3d0-6464-09e0-2953-35c4e95ff7cd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14031, 302, '457e6c28-9853-cbda-4004-37c9a11ea8f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14032, 302, '0355e35c-f518-907c-8595-37d41c9761e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14033, 302, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14034, 302, '62e64ead-4fe2-9499-24f8-48df068b0a08')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14035, 302, '9928f2f3-a353-15fa-613a-4dc6af384bbc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14036, 302, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14037, 302, '6625857d-c5e6-2507-2bb5-51f0b00fa9d0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14038, 302, '9e218664-e335-369b-d5c6-5575182015f2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14039, 302, '05d775da-4272-6a9e-8362-57c783b71e6e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14040, 302, '43b3325e-c930-507d-5248-58fe592cca5f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14041, 302, 'bf46459d-341c-6392-e263-5aab5ca69799')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14042, 302, '69e419bf-ed25-e07f-07fe-5bdbbe00cabf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14043, 302, 'ccce0cc0-1ac4-207c-903c-5bee7666d229')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14044, 302, 'b5696f5f-5536-1f89-6d54-5c6757602b38')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14045, 302, '3ca0ae16-7501-4657-720a-64da09be962f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14046, 302, 'f5b2125a-1322-b6df-06a6-6b597a1ca552')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14047, 302, '8a07f59f-ac86-5bca-24a0-70a5363cd1e5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14048, 302, '3166cb16-0c8c-4114-4d74-73a1a5edd501')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14049, 302, '9a5cedba-20df-2207-a59a-74d91dcc066c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14050, 302, '9ed93f20-634e-cf38-d2b8-792a97678135')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14051, 302, '451f5423-a70c-3d57-7217-7a5001804755')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14052, 302, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14053, 302, '7c3812ed-0ec0-14a1-8341-7be34d0bf84a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14054, 302, '1051edcf-77ea-df08-e4a4-7ed5d5f7a085')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14055, 302, '41f0ec72-2077-5414-7d03-896cffb03068')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14056, 302, '68eb2e92-0043-675f-51c7-941bb40c2a5f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14057, 302, '6d74e250-d74f-a8f4-8a6b-97234dc13056')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14058, 302, 'adec3c14-69d2-3f1a-62bc-9c549efa7fb1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14059, 302, 'e0bd6193-6e6f-0690-ed6f-9f78783aba45')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14060, 302, '34fc5611-9e08-ad65-38dd-aafdfe5d7724')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14061, 302, 'f69902b8-63e0-4417-db47-b1301949159e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14062, 302, 'cdb0fbbe-6066-c62f-bd69-b1355c9ddcfc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14063, 302, 'e73acb68-2f00-2436-c354-b15302013ad8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14064, 302, 'eb6fb1fc-86fa-dce6-9d06-b340452ddb97')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14065, 302, 'd2d26bac-406f-ef69-af6e-bf14741db767')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14066, 302, '32d8d104-2524-9402-a452-bf1f2d95b1e8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14067, 302, '005a566b-1052-1f18-7030-bffa8ad84a71')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14068, 302, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14069, 302, 'a4ed8260-257e-860d-4dd5-d815a3d7dc5c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14070, 302, '024e6ead-0bc1-a738-cc82-dba2dca51818')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14071, 302, '7c49ede3-bded-0aff-954c-e0fec873c969')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14072, 302, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14073, 302, '2b0de723-db75-8f05-e0d6-e7283643e166')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14074, 302, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14075, 302, '7d39180e-5b1c-a960-05f4-e9a32166cb8c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14076, 302, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14077, 302, '49719f37-e9d0-9670-ad15-f4267f3ee5c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14078, 302, 'cfde05d2-9814-257b-c6e7-fbbc706bb41e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14079, 303, '6f1687ef-1991-b9e8-f490-053982caf5a8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14080, 303, '657d91b6-a78f-836c-4eb7-20c1d3083cf4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14081, 303, 'a8a1813b-3e49-b38c-376f-2d236a9a98ff')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14082, 303, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14083, 303, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14084, 303, '0355e35c-f518-907c-8595-37d41c9761e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14085, 303, '618ab866-579d-9aec-ed3a-5a48ed92ae31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14086, 303, '9ed93f20-634e-cf38-d2b8-792a97678135')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14087, 303, '1051edcf-77ea-df08-e4a4-7ed5d5f7a085')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14088, 303, 'a519d12c-0083-96e3-4047-91c766339587')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14089, 303, 'c79e8ae1-aa62-57f2-ba89-d8c57b882b77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14090, 303, '05272a9d-9106-b201-aff4-decf1253be2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14091, 303, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14092, 303, 'c32bc170-7397-f950-1ead-fc907ebffcf3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14093, 304, '645739a4-2381-9990-af99-070773f3597b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14094, 304, '1cfb6a17-2002-d704-0c0f-191286b3a7ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14095, 304, 'a1830c4c-fb7f-6d5a-2bdd-1a5af17dc992')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14096, 304, '657d91b6-a78f-836c-4eb7-20c1d3083cf4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14097, 304, '419edfc9-397f-585e-899f-2b41ca9598ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14098, 304, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14099, 304, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14100, 304, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14101, 304, '00cfe926-098a-04f4-abd4-5e4d912715f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14102, 304, 'b48b84b8-0135-7001-fac2-60e564a66f8f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14103, 304, '75fa201c-f28d-1a69-26fe-6ece2b18c50c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14104, 304, '9a5cedba-20df-2207-a59a-74d91dcc066c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14105, 304, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14106, 304, '1051edcf-77ea-df08-e4a4-7ed5d5f7a085')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14107, 304, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14108, 304, 'a519d12c-0083-96e3-4047-91c766339587')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14109, 304, '02415db3-b0bd-9c93-6512-aab3cdf990e2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14110, 304, 'e10a5241-6a30-1eb1-5e90-b06c4c323cfe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14111, 304, '7c81c5dc-c437-7ced-e982-b26dbec5edcc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14112, 304, 'eb6fb1fc-86fa-dce6-9d06-b340452ddb97')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14113, 304, 'a5784fca-53b9-101b-664a-bedbb4bada81')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14114, 304, 'b5a1e16b-3010-3c7e-c8f2-d1122baa76fe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14115, 304, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14116, 304, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14117, 304, 'cfd75a84-863c-1d7f-22aa-f3ef39a9f5c6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14118, 304, 'ee83ac7a-88b2-29e3-0a2c-fa22b296df1b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14119, 304, 'c0d4e20e-0473-8f5a-c1e0-fdd0efdd96de')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14120, 305, '1f06492b-f1bd-0914-b32d-23719ac7f969')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14121, 305, '0355e35c-f518-907c-8595-37d41c9761e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14122, 305, '10e1b95a-8505-0f42-0e07-39d3d4f32177')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14123, 305, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14124, 306, '457e6c28-9853-cbda-4004-37c9a11ea8f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14125, 306, '6625857d-c5e6-2507-2bb5-51f0b00fa9d0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14126, 307, '4695fd4b-9328-fae7-c91f-3043125f276e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14127, 307, '7d9bde30-dda1-79d2-49ca-77513c8b8c46')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14128, 308, '3bd41858-f83f-ca79-8969-092cabc967ca')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14129, 308, '5a15c78b-93e4-374a-8ce8-0ec520513e5d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14130, 308, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14131, 308, 'b96dbb3e-4729-b77a-9909-47d64a63ccb7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14132, 308, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14133, 308, '9e218664-e335-369b-d5c6-5575182015f2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14134, 308, '00cfe926-098a-04f4-abd4-5e4d912715f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14135, 308, '50535e74-814f-6e82-5c19-6c5fd644803e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14136, 308, '5c82c12d-eced-1347-ca25-72fbffcc60d5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14137, 308, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14138, 308, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14139, 308, 'e10a5241-6a30-1eb1-5e90-b06c4c323cfe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14140, 308, 'cdb0fbbe-6066-c62f-bd69-b1355c9ddcfc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14141, 308, 'ad1371fc-5a80-2e7c-1488-d5d2b8a34b0a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14142, 308, '83209255-8a24-ab2e-1770-d9388f3e8716')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14143, 308, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14144, 309, '3dcd8f73-9ba4-bc19-a69d-042a83cf27c9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14145, 309, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14146, 309, '5d9ebcd6-7f49-83c8-2cf1-10997ae670fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14147, 309, '5375fe3c-d068-923b-f3f9-18eb45d45b6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14148, 309, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14149, 309, '1f06492b-f1bd-0914-b32d-23719ac7f969')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14150, 309, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14151, 309, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14152, 309, '457e6c28-9853-cbda-4004-37c9a11ea8f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14153, 309, '0355e35c-f518-907c-8595-37d41c9761e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14154, 309, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14155, 309, '62e64ead-4fe2-9499-24f8-48df068b0a08')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14156, 309, '9928f2f3-a353-15fa-613a-4dc6af384bbc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14157, 309, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14158, 309, '3166cb16-0c8c-4114-4d74-73a1a5edd501')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14159, 309, '9a5cedba-20df-2207-a59a-74d91dcc066c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14160, 309, '9ed93f20-634e-cf38-d2b8-792a97678135')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14161, 309, '34fc5611-9e08-ad65-38dd-aafdfe5d7724')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14162, 309, 'f69902b8-63e0-4417-db47-b1301949159e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14163, 309, 'cdb0fbbe-6066-c62f-bd69-b1355c9ddcfc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14164, 309, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14165, 309, 'a4ed8260-257e-860d-4dd5-d815a3d7dc5c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14166, 309, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14167, 309, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14168, 309, 'ee83ac7a-88b2-29e3-0a2c-fa22b296df1b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14169, 309, 'cfde05d2-9814-257b-c6e7-fbbc706bb41e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14170, 309, '953479a9-f064-d4e0-6d11-fc77fa487e92')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14171, 309, '7c5758bc-0b1c-230d-fea5-fed4c237e08a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14172, 400, 'fbb7a716-c135-0a3c-723d-0729327e6a4a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14173, 400, '5d9ebcd6-7f49-83c8-2cf1-10997ae670fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14174, 400, 'fa04aaaa-15f8-16ad-b72f-1addb026c6a9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14175, 400, '9143fee4-cf82-6274-3bbc-21980b01e872')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14176, 400, '557f512f-3d32-f8d1-d452-2d15ab048742')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14177, 400, '0e7d25a0-51f5-bae5-c016-38cc88cb7d2d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14178, 400, '50d7f4de-5172-8c5a-6283-4777b674fda9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14179, 400, '9173545c-aad1-1856-0fea-55b40a6f54eb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14180, 400, '235e28b9-4925-9aa0-dc75-5af0c2d27871')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14181, 400, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14182, 400, '6f47aa33-349f-87b6-205a-814d76924d11')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14183, 400, '6e727409-5cea-ba44-aa9c-859621295c9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14184, 400, '288bfacc-6523-79c9-907a-875af8b805aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14185, 400, '34fc5611-9e08-ad65-38dd-aafdfe5d7724')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14186, 400, '5f0357eb-00a6-d7a7-2a5c-b540633b7d71')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14187, 400, '3fd2be5c-3622-3187-875f-bcef2e0f0243')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14188, 400, 'f2ebb64a-cb9c-7c64-2485-cb08b0931ae1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14189, 400, '6153f3b6-3075-494a-06a0-ccb534e848bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14190, 400, '1bf9d829-a078-8ab8-6d08-db2044f46647')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14191, 400, '242f455f-7dc0-c593-4824-e05fd0297222')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14192, 400, '2701d847-01e9-8c41-4c15-e9e9a0223d30')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14193, 400, '6b118d3a-e841-1785-5476-ec9eb5299c00')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14194, 401, 'fd22136a-8b15-2831-f8e6-03d713c8c921')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14195, 401, 'd165535c-837b-d514-d4eb-05c0c843ecf3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14196, 401, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14197, 401, '6ceaf363-01f2-ac5d-ef7b-25fbcfd31977')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14198, 401, '6ed139ab-fe67-303d-dc5a-274a79acb1c9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14199, 401, 'fe9ce181-2d87-6cc5-0355-311e836f32e9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14200, 401, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14201, 401, 'b3a02f94-d26c-15e3-0509-3742d6ecf4a9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14202, 401, 'b4e65a63-89f9-e462-5450-3a6bfeecb6cc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14203, 401, '60d32abc-f4fc-3889-6155-41c8b1247e03')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14204, 401, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14205, 401, '25ac67cc-b9e9-8a0e-0c7e-43833c4faea6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14206, 401, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14207, 401, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14208, 401, '805e6661-074e-48d7-1a6d-723c13eecf81')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14209, 401, 'a1dd4771-9347-587f-aeb1-73bc5c036c09')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14210, 401, '151300a5-96c3-6edb-7995-7482bcf41c46')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14211, 401, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14212, 401, '288bfacc-6523-79c9-907a-875af8b805aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14213, 401, '07340343-e94b-64de-6221-8cf80215edd1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14214, 401, '181ff7f2-a43c-82ca-bf2c-9472631035f9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14215, 401, 'eede4248-4895-77c9-63f0-9741f812817d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14216, 401, '088fa085-21aa-846a-b5c6-a027c7d66f54')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14217, 401, '32d8d104-2524-9402-a452-bf1f2d95b1e8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14218, 401, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14219, 401, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14220, 401, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14221, 401, 'd4a7842a-67c9-8606-3046-f2890a66edf0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14222, 401, '131f746b-8956-6e9d-6090-fac997c1d827')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14223, 401, '99054e31-ae9a-0784-6a61-fd817d40aa69')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14224, 402, '10ecd666-17eb-3608-c0d4-07abe028718b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14225, 402, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14226, 402, 'd6a87723-73d4-158b-68a1-182dfda2be9a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14227, 402, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14228, 402, '7e4db51b-657b-1029-3e05-2140fb56a7c3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14229, 402, 'fe9ce181-2d87-6cc5-0355-311e836f32e9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14230, 402, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14231, 402, 'b4e65a63-89f9-e462-5450-3a6bfeecb6cc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14232, 402, '57a4cbb2-859c-4bfe-85d7-3f739c2a3454')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14233, 402, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14234, 402, '5c63510b-7711-5be5-df03-49867ac9da68')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14235, 402, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14236, 402, '750e0bfb-f404-0a1e-61f3-50e316c1877c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14237, 402, '6625857d-c5e6-2507-2bb5-51f0b00fa9d0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14238, 402, 'dcc00166-7016-10d0-172b-5471c2714276')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14239, 402, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14240, 402, '73d108f7-d950-7eef-9fe7-62cbe8e6bd40')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14241, 402, 'ecdc2404-31a5-54b5-8fd1-67fa394df556')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14242, 402, '549ef4e0-0e95-379c-17a6-6e1ad7873ee0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14243, 402, 'e07a756b-ca5c-dc19-0488-6ea624cf055e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14244, 402, '9aae4a5d-033e-46a5-49b8-72feceae691e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14245, 402, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14246, 402, '288bfacc-6523-79c9-907a-875af8b805aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14247, 402, '07340343-e94b-64de-6221-8cf80215edd1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14248, 402, '0ce49209-a616-e24a-0072-9288b6b4d8a4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14249, 402, '088820fc-25c7-5685-1d56-92dbe33db584')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14250, 402, '181ff7f2-a43c-82ca-bf2c-9472631035f9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14251, 402, 'eede4248-4895-77c9-63f0-9741f812817d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14252, 402, '0d8f4d79-9624-ef5c-c35f-9d9b5e1e5c34')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14253, 402, '3b2a039a-ff0d-c48c-e4b6-a1d4187478eb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14254, 402, '49ad95a2-453b-fc94-18f5-a85da0e0c398')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14255, 402, '73df63fb-a107-83da-19a6-a9484d7cd01a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14256, 402, '07695bba-dd6c-8f0b-10ef-a9a05fbda060')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14257, 402, 'a96633ef-c2ca-c960-ce22-baaf15502522')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14258, 402, '351f4371-f79e-8b9c-1d3b-bd656858a3a1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14259, 402, '32d8d104-2524-9402-a452-bf1f2d95b1e8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14260, 402, '2bb08aed-430d-9ea6-47bd-c11f6b0834eb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14261, 402, '227ff458-1c9e-8fe6-6be2-c439eb2b4b87')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14262, 402, '0429ff87-c576-91dd-f14d-c8507bfa3405')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14263, 402, '91ce9d81-2856-f0db-f066-d1c210028e0a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14264, 402, '30b523a5-fb65-f508-14eb-df23e98b4980')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14265, 402, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14266, 402, 'ee978abd-c8d7-7d1e-aec2-e87f6f66ec05')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14267, 402, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14268, 402, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14269, 402, 'd4a7842a-67c9-8606-3046-f2890a66edf0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14270, 402, '99054e31-ae9a-0784-6a61-fd817d40aa69')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14271, 403, 'ce9db009-dde6-1986-e24f-18f07c85210d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14272, 403, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14273, 403, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14274, 403, 'a96633ef-c2ca-c960-ce22-baaf15502522')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14275, 403, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14276, 403, '50155b54-4a87-e43c-8f7a-e8513bbc8b4c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14277, 403, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14278, 404, '71c5f5b4-c63f-336c-4860-04ce7a5abff8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14279, 404, 'd682e971-e52b-d3ed-d0bf-088cb1a01973')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14280, 404, '13e8efc7-17f1-bc90-e3f5-0d0eda35eb1e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14281, 404, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14282, 404, '5d9ebcd6-7f49-83c8-2cf1-10997ae670fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14283, 404, '8ec32df7-b478-015e-b06b-10ce7d3d6a5b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14284, 404, 'ff32f234-fc17-bffc-0f2b-113950cec1ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14285, 404, 'eaf0c90f-0fc7-bab6-b8a5-15710d0c945e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14286, 404, 'ce9db009-dde6-1986-e24f-18f07c85210d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14287, 404, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14288, 404, 'f186b9dc-56cd-8958-50f3-1aa1b717ace4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14289, 404, 'f8468cea-2f1f-3d5c-dc5b-1d5f18fe39a6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14290, 404, '6e7d82b3-293b-78e4-fade-1e7de5de7f01')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14291, 404, '6ed139ab-fe67-303d-dc5a-274a79acb1c9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14292, 404, '01e37c76-8e1f-978d-ba4b-2bc25f2fceec')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14293, 404, 'a946f84f-6ab7-9936-55bd-2e9af6a0ea6d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14294, 404, 'fe9ce181-2d87-6cc5-0355-311e836f32e9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14295, 404, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14296, 404, 'ec98b5bc-c33d-04d7-3d0c-34e3938a168e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14297, 404, '7db9dfac-f46b-09ab-ea61-36a7af465da6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14298, 404, '457e6c28-9853-cbda-4004-37c9a11ea8f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14299, 404, 'b4e65a63-89f9-e462-5450-3a6bfeecb6cc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14300, 404, '41791b1d-d3b6-6fa2-f724-3e3b8dcf6b99')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14301, 404, '9cff8a24-3780-0a60-1dbd-40ac83d89da2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14302, 404, '60d32abc-f4fc-3889-6155-41c8b1247e03')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14303, 404, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14304, 404, 'ee761f5d-ce70-f517-0933-434b312a8d8c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14305, 404, '25ac67cc-b9e9-8a0e-0c7e-43833c4faea6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14306, 404, '5ce9c557-b219-8df1-7eb5-4447982e17fd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14307, 404, 'ca7fc392-43b4-9c3f-d201-47295ea7ee32')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14308, 404, '19617c4b-709c-866d-844f-4ded7da5f769')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14309, 404, '809b4d4a-0193-7974-45e4-4df59b08719b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14310, 404, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14311, 404, 'dfb2be2e-f4d4-81d5-21b2-540c948a7ad1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14312, 404, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14313, 404, 'e4473efe-9df4-cf78-3d95-5d695bcf96fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14314, 404, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14315, 404, '73d108f7-d950-7eef-9fe7-62cbe8e6bd40')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14316, 404, '5de6fd00-df0a-0899-e6a9-655d0ef9a07e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14317, 404, 'ecdc2404-31a5-54b5-8fd1-67fa394df556')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14318, 404, '549ef4e0-0e95-379c-17a6-6e1ad7873ee0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14319, 404, 'e07a756b-ca5c-dc19-0488-6ea624cf055e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14320, 404, 'f9cbb761-0154-d87f-f6e4-715d29cc1ded')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14321, 404, '138d5013-0f26-d332-dfe4-72100e071d5c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14322, 404, '9aae4a5d-033e-46a5-49b8-72feceae691e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14323, 404, '1d8953df-6793-0019-0072-74d7384c093d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14324, 404, '89a6de0b-ca97-c9c1-4461-75150738d1ea')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14325, 404, '49a81537-e051-8904-74c0-786f3a40df75')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14326, 404, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14327, 404, 'a6235a1f-4082-7438-b284-7bec6bb1ce23')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14328, 404, '68e15dfa-6ac1-8802-4772-7c19b933bacf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14329, 404, '4fe1282f-7584-c764-a613-81969e51cfef')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14330, 404, '75d0c3cc-8f3c-14ad-b856-81f89b6b9110')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14331, 404, '09f7c844-8139-054b-291b-81fbf10d5e27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14332, 404, '288bfacc-6523-79c9-907a-875af8b805aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14333, 404, '8674e233-02c7-3d58-fc15-8a909b411c3b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14334, 404, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14335, 404, 'e86ad0fa-b426-4656-77c5-8c1726b7723c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14336, 404, '07340343-e94b-64de-6221-8cf80215edd1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14337, 404, '89ebaba6-7165-d248-ca8e-915ac84366be')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14338, 404, 'c8b8c768-c1fb-ef51-333f-92a77f0adcf7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14339, 404, '1f0993b1-6e0e-26d7-509a-9413ec91fd6e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14340, 404, 'a4befc85-22fd-3f9a-45f0-96500a70fcf2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14341, 404, 'eede4248-4895-77c9-63f0-9741f812817d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14342, 404, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14343, 404, 'aca5d5c1-1afd-ea34-2113-a0f939cb1cd2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14344, 404, '3b2a039a-ff0d-c48c-e4b6-a1d4187478eb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14345, 404, '3ad66478-469a-d08b-b960-a77f48b39bca')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14346, 404, '49ad95a2-453b-fc94-18f5-a85da0e0c398')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14347, 404, '07695bba-dd6c-8f0b-10ef-a9a05fbda060')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14348, 404, '969d4b74-c218-0354-4eea-ab4199727dd4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14349, 404, 'bf130afa-4054-9798-404c-ae2a53f0a118')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14350, 404, 'cdb0fbbe-6066-c62f-bd69-b1355c9ddcfc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14351, 404, '36f54324-aa54-3d49-a9ec-b9b5eb365f92')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14352, 404, 'a96633ef-c2ca-c960-ce22-baaf15502522')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14353, 404, '32d8d104-2524-9402-a452-bf1f2d95b1e8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14354, 404, '5463f8c1-345a-5e8c-2970-c10afd213d5a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14355, 404, '227ff458-1c9e-8fe6-6be2-c439eb2b4b87')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14356, 404, 'fdf83571-b954-55b1-86cb-c52886b862d7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14357, 404, 'f43b2a35-a935-891f-a09c-d339bf933701')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14358, 404, '2e7937cb-6667-765c-475b-d5b62859499b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14359, 404, '0457a5ed-2867-32c3-b5a9-d9adda351d8f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14360, 404, '024e6ead-0bc1-a738-cc82-dba2dca51818')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14361, 404, 'd59af2db-56c5-d24d-539b-dd59da8775f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14362, 404, '727865ef-6dfd-ee4f-1d9a-dd92f014f543')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14363, 404, '30b523a5-fb65-f508-14eb-df23e98b4980')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14364, 404, '2e085fd4-3d57-117f-91cc-df259c1a3bf5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14365, 404, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14366, 404, '3a364733-6b8f-565c-cebd-e742bb5cbe1b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14367, 404, '50155b54-4a87-e43c-8f7a-e8513bbc8b4c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14368, 404, 'ee978abd-c8d7-7d1e-aec2-e87f6f66ec05')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14369, 404, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14370, 404, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14371, 404, '71b1c049-8bfb-6b75-b538-ecccc7f5e010')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14372, 404, 'd4a7842a-67c9-8606-3046-f2890a66edf0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14373, 404, '20edcc0f-8efd-2e31-5cbb-f4aee89b0057')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14374, 404, 'aa5c8748-db68-e663-935e-f55dca9f9b59')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14375, 404, '627f896e-d5a9-409b-8f92-f5c3825691a7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14376, 404, 'ae1bd754-cbe4-214c-c664-f8bfcdf7240b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14377, 404, '131f746b-8956-6e9d-6090-fac997c1d827')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14378, 404, '1399bc72-3146-79cb-2cc6-fc87d13b5d10')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14379, 404, '99054e31-ae9a-0784-6a61-fd817d40aa69')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14380, 405, '8d556b61-2bb2-6d3d-3672-08c8070c8470')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14381, 405, '45a042bb-0af8-e229-f4a5-13d946c98ccc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14382, 405, '0bbe2ac3-bba8-efb2-74d3-141204eafcc1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14383, 405, '5375fe3c-d068-923b-f3f9-18eb45d45b6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14384, 405, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14385, 405, '5c211b38-a7d3-f72f-0806-29bd49b693f4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14386, 405, '70e45891-db1b-2776-1c5b-2e7404aae3cb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14387, 405, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14388, 405, '692ee583-0b39-8178-2538-326bd4d240a3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14389, 405, '83bc0079-52e6-6108-7458-436fb1e981f5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14390, 405, '053d1689-d0ea-a201-70fb-50572da9307e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14391, 405, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14392, 405, '855f9f41-c8d5-f78d-e3a1-65f46c522545')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14393, 405, '3fd77cad-0e55-19d7-21dc-6a3da7643dec')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14394, 405, '1f0993b1-6e0e-26d7-509a-9413ec91fd6e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14395, 405, '7021851a-3f74-30e8-e860-a0ea57d7c283')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14396, 405, '592e2c97-e767-8270-25b0-ab3f2e91d120')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14397, 405, 'bf130afa-4054-9798-404c-ae2a53f0a118')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14398, 405, '9b2c1609-8fe8-2cab-3021-c216c206afbe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14399, 405, '80ecb954-c725-6f9e-1065-c3b56a8460a6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14400, 405, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14401, 405, '3f75c050-ebb2-3c8f-b8d8-e6c0cf6a24d1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14402, 405, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14403, 405, 'd4a7842a-67c9-8606-3046-f2890a66edf0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14404, 405, 'aa5c8748-db68-e663-935e-f55dca9f9b59')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14405, 406, '49b8c01e-4279-b995-eb00-0675b3e4b85f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14406, 406, 'ff32f234-fc17-bffc-0f2b-113950cec1ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14407, 406, '5375fe3c-d068-923b-f3f9-18eb45d45b6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14408, 406, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14409, 406, 'dff705f1-5c59-748c-1d78-2593dcd3c287')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14410, 406, 'fe9ce181-2d87-6cc5-0355-311e836f32e9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14411, 406, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14412, 406, 'd75ffc98-8868-5507-6586-486d8191a2c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14413, 406, 'bd556c3d-ea1c-99d2-d223-4ca877a0152a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14414, 406, '9d39b546-f9a0-1358-e6b6-5bddec2b2c05')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14415, 406, 'bf130afa-4054-9798-404c-ae2a53f0a118')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14416, 406, 'e58c5e2f-40c7-e8aa-2fa0-b89305f1c59b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14417, 406, 'b7ecd363-a7a6-65db-c6c2-b90cda3c735c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14418, 406, '717d5258-a770-163b-7046-c49af071c8a4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14419, 406, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14420, 406, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14421, 406, '743d9a82-7ded-4a06-7852-eecec9d53db2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14422, 406, 'aa5c8748-db68-e663-935e-f55dca9f9b59')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14423, 550, 'ccc5639f-8b5b-99de-7c65-19c88f72ceac')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14424, 550, '9b2d7f3e-cbd4-8379-9f06-63dc528c4b45')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14425, 550, 'f8a896f2-77e7-ce12-ca78-a17118fce014')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14426, 550, '45aee6b5-b0f3-2dd9-bf1c-b130b6e7f72e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14427, 550, 'b555cd85-4597-fb8c-6250-b83f86568427')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14428, 550, '777f217a-cb5c-66d5-053a-efcffb183ec5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14429, 552, '585ebfc2-338a-343b-e84c-048e934f1dd1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14430, 552, '3e449169-667f-8c96-ddfb-132143443977')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14431, 552, '9b2d7f3e-cbd4-8379-9f06-63dc528c4b45')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14432, 552, '777f217a-cb5c-66d5-053a-efcffb183ec5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14433, 555, '3e449169-667f-8c96-ddfb-132143443977')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14434, 555, '45a042bb-0af8-e229-f4a5-13d946c98ccc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14435, 555, '269441f7-136d-1dee-5da9-171d057eeff4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14436, 555, '46438258-4610-6b8f-557a-3ff99b3eec90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14437, 555, '45aee6b5-b0f3-2dd9-bf1c-b130b6e7f72e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14438, 555, '777f217a-cb5c-66d5-053a-efcffb183ec5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14439, 601, 'f57a7e4d-530c-b6f2-c181-4d014cafecec')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14440, 601, '8d4fc2c3-4160-48a4-85c5-5f8461c1942d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14441, 601, '6607069b-27fa-864f-0c45-a400a95d167f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14442, 601, 'e46da351-5ee2-74bb-24ee-c73bc304759e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14443, 601, 'b8269c09-9375-db11-9de3-f255f5bf771c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14444, 602, 'b6f3d30c-43ab-aaf2-48e7-0623e97db49a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14445, 602, 'a62cf213-d827-9811-18bf-21a6f28d6ab3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14446, 602, 'ad43aa81-3ed9-738b-c555-2859cd204238')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14447, 602, '701e03fa-f712-881a-add5-2f6762f9098d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14448, 602, '181ff160-2293-21ec-c441-32d0bd6b3549')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14449, 602, 'ff2bb37d-4c8a-bbc7-6fac-38f90d8e599b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14450, 602, '8d4fc2c3-4160-48a4-85c5-5f8461c1942d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14451, 602, 'ccdca087-21ff-6a83-1c98-672d8c2a06cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14452, 602, 'b5bbcb31-c0a1-c8be-100f-6d5572f1638e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14453, 602, '04d6756e-9079-7590-b328-7b4d81795da3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14454, 602, 'e0ee8ebf-2e1d-64b1-b8f2-84d4cbf3157a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14455, 602, '4f26fdd2-78c3-ce69-740d-8962f41a74b8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14456, 602, '1013e05c-3ac4-257b-4ed0-8a8afc0daa25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14457, 602, '93e7a041-1d13-cef0-207c-8f8658691606')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14458, 602, '5cde7aa1-cfa0-96fa-a374-90051667ecf6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14459, 602, 'ceebce90-213f-9cf3-e710-a058d1076600')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14460, 602, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14461, 602, 'e46da351-5ee2-74bb-24ee-c73bc304759e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14462, 602, '2cdd7787-9a51-61c9-e12c-d75dc253fb8f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14463, 602, '4f14751d-862c-9b20-12ce-e0741e92fc4e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14464, 602, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14465, 602, '80f76099-854b-87c4-03d7-f5eff6847ce0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14466, 602, '670ef3dc-5362-c2f0-5995-faf4173b1874')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14467, 603, '1767aef2-a3c6-c59a-ea37-1c996d29a200')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14468, 603, 'adea0c6e-5e7e-7c71-d53f-34bfad15a083')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14469, 603, '65960a51-4a8c-4344-772e-4b068ec27f79')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14470, 603, '8e91a26c-be5b-08c5-59d6-723aa206c881')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14471, 603, '70402105-8ba1-2fe3-a7ed-816621fc3a85')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14472, 603, '87c2b2af-3d0e-c09e-95ea-be6d39f83ec9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14473, 603, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14474, 605, '4d66fa41-29d3-cfe2-05e0-0f539b7af27c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14475, 605, 'f4079c80-fa34-2908-6bd3-2196c1043619')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14476, 605, 'b61f97f8-74d9-559e-b56b-57fda32ec496')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14477, 605, 'b5bbcb31-c0a1-c8be-100f-6d5572f1638e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14478, 605, '04d6756e-9079-7590-b328-7b4d81795da3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14479, 605, '4f26fdd2-78c3-ce69-740d-8962f41a74b8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14480, 605, '1013e05c-3ac4-257b-4ed0-8a8afc0daa25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14481, 605, '5cde7aa1-cfa0-96fa-a374-90051667ecf6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14482, 605, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14483, 605, 'e10a5241-6a30-1eb1-5e90-b06c4c323cfe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14484, 605, '1c61c803-b15a-d758-a6e6-b9e744f7282e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14485, 605, 'a96633ef-c2ca-c960-ce22-baaf15502522')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14486, 606, 'a62cf213-d827-9811-18bf-21a6f28d6ab3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14487, 606, '181ff160-2293-21ec-c441-32d0bd6b3549')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14488, 606, 'ff2bb37d-4c8a-bbc7-6fac-38f90d8e599b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14489, 606, 'b61f97f8-74d9-559e-b56b-57fda32ec496')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14490, 606, '8d4fc2c3-4160-48a4-85c5-5f8461c1942d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14491, 606, 'ccdca087-21ff-6a83-1c98-672d8c2a06cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14492, 606, 'b5bbcb31-c0a1-c8be-100f-6d5572f1638e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14493, 606, '04d6756e-9079-7590-b328-7b4d81795da3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14494, 606, '1013e05c-3ac4-257b-4ed0-8a8afc0daa25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14495, 606, '93e7a041-1d13-cef0-207c-8f8658691606')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14496, 606, '6607069b-27fa-864f-0c45-a400a95d167f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14497, 606, 'e46da351-5ee2-74bb-24ee-c73bc304759e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14498, 606, '2cdd7787-9a51-61c9-e12c-d75dc253fb8f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14499, 606, 'b8269c09-9375-db11-9de3-f255f5bf771c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14500, 606, '80f76099-854b-87c4-03d7-f5eff6847ce0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14501, 607, '4d66fa41-29d3-cfe2-05e0-0f539b7af27c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14502, 607, 'b5bbcb31-c0a1-c8be-100f-6d5572f1638e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14503, 607, '1013e05c-3ac4-257b-4ed0-8a8afc0daa25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14504, 607, '1c61c803-b15a-d758-a6e6-b9e744f7282e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14505, 701, 'b6f3d30c-43ab-aaf2-48e7-0623e97db49a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14506, 701, '5a46f9c2-f5e0-3da4-a88a-0faa89f145b8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14507, 701, 'dae2b455-cf83-c5bd-a005-15ec9ad082ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14508, 701, 'a1830c4c-fb7f-6d5a-2bdd-1a5af17dc992')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14509, 701, '154818e4-b027-9dda-281b-241bb8e4d355')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14510, 701, '3c0b0576-4cb7-1dbe-fb7c-2726faf69ef6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14511, 701, 'ad43aa81-3ed9-738b-c555-2859cd204238')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14512, 701, '0927c33d-802d-28f1-2957-28ff90c37bba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14513, 701, '701e03fa-f712-881a-add5-2f6762f9098d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14514, 701, 'b0bf300d-f598-6db0-b264-38a3f7a864f0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14515, 701, '52f465af-aa5a-e748-d54c-4934d6e1ea4b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14516, 701, '6270b9d1-a28c-803c-a5fe-6574d18f0e51')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14517, 701, '32d4f303-7abd-15ac-f8e2-6cf0ac99a961')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14518, 701, '74affc4a-3c2a-3fd1-421b-784e3571d7dd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14519, 701, 'a6b8afb5-7349-bb22-2897-79a28454b7c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14520, 701, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14521, 701, 'f1095e92-c73b-02fa-fc6c-8246bff73ce8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14522, 701, 'e0ee8ebf-2e1d-64b1-b8f2-84d4cbf3157a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14523, 701, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14524, 701, '4f26fdd2-78c3-ce69-740d-8962f41a74b8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14525, 701, 'd73b0f8f-6a1d-048a-f061-89c5898b3d60')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14526, 701, '7531c6a4-020f-3375-6a7f-8cc3883fb468')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14527, 701, '5cde7aa1-cfa0-96fa-a374-90051667ecf6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14528, 701, 'ceebce90-213f-9cf3-e710-a058d1076600')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14529, 701, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14530, 701, '2a035098-b17e-0e9a-3d5b-a5a1ef710354')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14531, 701, '193a2f93-0919-be19-c0d2-b34269d9c54f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14532, 701, '8ed5dc9d-a88a-d95a-8da3-b7a9ace1dda8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14533, 701, '483999e9-08b2-dde8-a5f1-b7dea0329861')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14534, 701, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14535, 701, '0e2367af-34ac-e3b7-7772-d1457d0d0db2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14536, 701, '4f32b311-00bb-9e02-da0d-d1afaff864b4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14537, 701, '7333dfa7-b5aa-a6e8-7791-dc11b881a7a3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14538, 701, '4f14751d-862c-9b20-12ce-e0741e92fc4e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14539, 701, 'b03fa376-e3e0-74a8-f329-eace8a68325e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14540, 701, '8a5ab0f0-5678-e25d-9838-f46f4a454385')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14541, 701, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14542, 701, 'd945cb5d-ea6f-0e53-dc57-fd5b5cbd5876')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14543, 702, 'f6a708e0-e10a-3806-838c-17978b0aa390')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14544, 702, '5ae46788-46f5-12ac-2bb5-1b073d419778')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14545, 702, 'cc3ea4ba-9c2e-06d4-b873-21f5598a312a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14546, 702, '154818e4-b027-9dda-281b-241bb8e4d355')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14547, 702, '3c0b0576-4cb7-1dbe-fb7c-2726faf69ef6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14548, 702, 'ad43aa81-3ed9-738b-c555-2859cd204238')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14549, 702, '701e03fa-f712-881a-add5-2f6762f9098d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14550, 702, '60d32abc-f4fc-3889-6155-41c8b1247e03')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14551, 702, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14552, 702, '32d4f303-7abd-15ac-f8e2-6cf0ac99a961')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14553, 702, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14554, 702, 'f1095e92-c73b-02fa-fc6c-8246bff73ce8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14555, 702, '4f26fdd2-78c3-ce69-740d-8962f41a74b8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14556, 702, 'd73b0f8f-6a1d-048a-f061-89c5898b3d60')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14557, 702, '5cde7aa1-cfa0-96fa-a374-90051667ecf6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14558, 702, 'fa71f033-6c03-7965-854f-99926556f6be')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14559, 702, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14560, 702, '08132ae2-14d9-a78b-f00d-a807da4f040f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14561, 702, '64abf93e-f461-1e9c-2975-adba03c30b82')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14562, 702, '627da1cf-e5d2-5379-4ee8-ade453d3900c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14563, 702, 'c5c126c6-3358-d8a7-6120-bf2d2434acab')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14564, 702, '4f32b311-00bb-9e02-da0d-d1afaff864b4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14565, 702, 'babe47c3-0ecc-3e00-78b3-dccf404a206f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14566, 702, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14567, 702, 'b03fa376-e3e0-74a8-f329-eace8a68325e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14568, 702, '131f746b-8956-6e9d-6090-fac997c1d827')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14569, 801, 'e1364041-cd01-191d-fd95-009a8816b3c7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14570, 801, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14571, 801, 'b6f3d30c-43ab-aaf2-48e7-0623e97db49a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14572, 801, '259cfca3-b68f-a18f-7867-0ab66c7a5630')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14573, 801, 'e48751d3-abf2-e8ef-cfa3-0bb2a06c54a2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14574, 801, 'af9df58d-16e0-4729-94aa-0d668f509316')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14575, 801, '9c645d7c-9905-a07c-2418-0e6c25809523')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14576, 801, '6ddcffbb-69b0-f390-6722-103a7f2fa85d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14577, 801, 'dae2b455-cf83-c5bd-a005-15ec9ad082ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14578, 801, '5c19632f-d520-10c7-ff36-1689d77b0e57')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14579, 801, '5b5426e9-dc45-af10-515e-179f3225a2de')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14580, 801, '5ae46788-46f5-12ac-2bb5-1b073d419778')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14581, 801, '1767aef2-a3c6-c59a-ea37-1c996d29a200')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14582, 801, 'cc3ea4ba-9c2e-06d4-b873-21f5598a312a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14583, 801, '142e79c9-e8c4-c47d-d8c9-240f8f6d4709')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14584, 801, '154818e4-b027-9dda-281b-241bb8e4d355')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14585, 801, 'ad43aa81-3ed9-738b-c555-2859cd204238')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14586, 801, '419edfc9-397f-585e-899f-2b41ca9598ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14587, 801, '701e03fa-f712-881a-add5-2f6762f9098d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14588, 801, 'c35fb626-7d22-9c17-796c-378f28cea207')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14589, 801, '5ee9b39d-98fb-78f3-67ba-3999154e8b99')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14590, 801, '8b95573a-c77d-46c5-88e5-3d549eb0a1c1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14591, 801, '52f465af-aa5a-e748-d54c-4934d6e1ea4b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14592, 801, '8a850dad-0354-7d29-1ab8-4c7a6ebdca8e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14593, 801, 'e6cb628c-673a-c40f-1168-4ccac2b06a94')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14594, 801, '022a5a0a-2aee-1251-50f7-4d604d5552bd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14595, 801, '7593859f-90a3-9b8f-cbab-4de3b760a057')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14596, 801, 'f4d9fba7-1d45-2e75-f9a5-5338e14d8dbf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14597, 801, 'cca7002b-907b-100c-aff0-543c84e6958f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14598, 801, 'd05ae428-73f3-a119-5142-6714289568e6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14599, 801, '27f9dced-93ee-9a2b-8dca-6beb52afae98')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14600, 801, '32d4f303-7abd-15ac-f8e2-6cf0ac99a961')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14601, 801, 'b2687321-49ba-47fe-9a30-6ee1a4e1fcc4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14602, 801, '272dd5b4-3c09-a920-7f8a-72e5f8c13008')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14603, 801, '311a783b-1e40-1778-d1e4-75830adabd37')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14604, 801, '06a8eb2f-62de-b544-08b2-794ae50cf741')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14605, 801, 'efde4739-5ab5-bdd3-3654-79abeb874fb5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14606, 801, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14607, 801, '472fed04-32b0-b203-4470-7c80e8c2ffb6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14608, 801, 'bd1a2d5d-2da0-be69-3d76-84693205523c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14609, 801, 'e0ee8ebf-2e1d-64b1-b8f2-84d4cbf3157a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14610, 801, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14611, 801, '4f26fdd2-78c3-ce69-740d-8962f41a74b8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14612, 801, 'd73b0f8f-6a1d-048a-f061-89c5898b3d60')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14613, 801, '7531c6a4-020f-3375-6a7f-8cc3883fb468')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14614, 801, '5cde7aa1-cfa0-96fa-a374-90051667ecf6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14615, 801, '8d66e47e-6ee0-7b8f-43da-9827fc1c3e2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14616, 801, 'fa71f033-6c03-7965-854f-99926556f6be')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14617, 801, 'ceebce90-213f-9cf3-e710-a058d1076600')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14618, 801, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14619, 801, '2a035098-b17e-0e9a-3d5b-a5a1ef710354')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14620, 801, '08132ae2-14d9-a78b-f00d-a807da4f040f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14621, 801, 'e91d7a99-16b3-266b-0e56-a9227da71dac')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14622, 801, '5831e5d2-ebd1-0a57-1726-aafed1dfd899')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14623, 801, '627da1cf-e5d2-5379-4ee8-ade453d3900c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14624, 801, '8ed5dc9d-a88a-d95a-8da3-b7a9ace1dda8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14625, 801, 'c5c126c6-3358-d8a7-6120-bf2d2434acab')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14626, 801, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14627, 801, '78d4d008-163b-fe25-c342-c977e059a08d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14628, 801, 'ce636556-3219-a3ac-30c7-d6e354254ef2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14629, 801, '7333dfa7-b5aa-a6e8-7791-dc11b881a7a3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14630, 801, 'babe47c3-0ecc-3e00-78b3-dccf404a206f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14631, 801, '4f14751d-862c-9b20-12ce-e0741e92fc4e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14632, 801, 'bd7dc7a0-43dc-1a4a-19a7-e83bed667668')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14633, 801, 'b03fa376-e3e0-74a8-f329-eace8a68325e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14634, 801, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14635, 801, '00376e37-8d45-7f8a-3c98-fa91d1fd6fd6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14636, 801, 'bfe62a22-a57f-cb1e-a299-fcd5f0b8a465')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14637, 801, '5135a3cc-aac1-961a-0080-fd0b2fbb7859')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14638, 801, 'c0d4e20e-0473-8f5a-c1e0-fdd0efdd96de')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14639, 802, 'e1364041-cd01-191d-fd95-009a8816b3c7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14640, 802, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14641, 802, 'e61a6455-e284-e0ab-1587-04a4ba845a3d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14642, 802, 'e48751d3-abf2-e8ef-cfa3-0bb2a06c54a2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14643, 802, 'af9df58d-16e0-4729-94aa-0d668f509316')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14644, 802, 'bb3553f4-bb03-f3c3-60a8-1853e8739a35')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14645, 802, '142e79c9-e8c4-c47d-d8c9-240f8f6d4709')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14646, 802, '154818e4-b027-9dda-281b-241bb8e4d355')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14647, 802, '5ee9b39d-98fb-78f3-67ba-3999154e8b99')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14648, 802, '98b1e24c-5779-3455-4390-3b1315c27587')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14649, 802, '46438258-4610-6b8f-557a-3ff99b3eec90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14650, 802, 'ae4bd6ef-b286-2d83-792b-472151e6af3c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14651, 802, '720cc7eb-f739-9a8a-d955-4955fb162b50')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14652, 802, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14653, 802, 'a2cce1f2-cab7-631e-ead0-58e6e13ca134')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14654, 802, '43b3325e-c930-507d-5248-58fe592cca5f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14655, 802, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14656, 802, 'ccdca087-21ff-6a83-1c98-672d8c2a06cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14657, 802, 'ecdc2404-31a5-54b5-8fd1-67fa394df556')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14658, 802, '9aae4a5d-033e-46a5-49b8-72feceae691e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14659, 802, '06a8eb2f-62de-b544-08b2-794ae50cf741')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14660, 802, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14661, 802, '472fed04-32b0-b203-4470-7c80e8c2ffb6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14662, 802, 'ebf06e14-6634-43f0-c871-7e13f91be688')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14663, 802, '10581633-24ad-48f6-0e4b-7fb2370ab8e9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14664, 802, '0525fc12-130a-b914-e1f3-83c0d5d324df')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14665, 802, '3c16fdf5-6c3c-5a0e-84ab-84ea1281e796')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14666, 802, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14667, 802, '4f26fdd2-78c3-ce69-740d-8962f41a74b8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14668, 802, '41f0ec72-2077-5414-7d03-896cffb03068')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14669, 802, 'd73b0f8f-6a1d-048a-f061-89c5898b3d60')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14670, 802, 'e2dbb286-7ebe-186a-42d2-8aa6c2b9501e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14671, 802, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14672, 802, '5cde7aa1-cfa0-96fa-a374-90051667ecf6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14673, 802, '006bece0-fad1-deb5-bd8f-9049bde026e2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14674, 802, '8d66e47e-6ee0-7b8f-43da-9827fc1c3e2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14675, 802, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14676, 802, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14677, 802, '7b409460-3200-a54b-673a-a9535c7c6741')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14678, 802, '5831e5d2-ebd1-0a57-1726-aafed1dfd899')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14679, 802, '627da1cf-e5d2-5379-4ee8-ade453d3900c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14680, 802, '6d379e4d-5b2c-06be-a695-af6e6378b67a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14681, 802, '87c2b2af-3d0e-c09e-95ea-be6d39f83ec9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14682, 802, 'c5c126c6-3358-d8a7-6120-bf2d2434acab')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14683, 802, 'c51bad2e-e402-306d-cfa0-c539e22c0fcb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14684, 802, '83395326-8640-df5e-1abe-cf90232c05d8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14685, 802, 'cd03b464-507e-42c8-779f-d58061957de5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14686, 802, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14687, 802, 'cc50fb1b-afae-5bfa-72fe-f7becc286de3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14688, 802, '5135a3cc-aac1-961a-0080-fd0b2fbb7859')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14689, 804, '142e79c9-e8c4-c47d-d8c9-240f8f6d4709')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14690, 804, '22c81e6d-4efe-b28b-c33c-43f7a6a21104')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14691, 804, 'e53bbe6a-43a6-2701-5341-503cdda22d6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14692, 804, 'd779b8ab-13a6-9780-6efd-61fc221819db')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14693, 804, '7fb53b5b-7668-e86f-c5b5-7d0ea864eb7e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14694, 804, '7aba1627-eea6-92b2-197c-7fc807c471db')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14695, 804, '41f0ec72-2077-5414-7d03-896cffb03068')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14696, 804, '3aa02ab2-e527-3a40-e93b-9af6a2001dca')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14697, 804, '19593eb8-446e-061a-50ef-9c9624d520cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14698, 804, '9792b2c6-fef0-3f96-3943-a5e303db9eac')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14699, 804, '5831e5d2-ebd1-0a57-1726-aafed1dfd899')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14700, 804, '55823a3e-b446-b297-dcd1-d426d6878bb4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14701, 804, 'd9a822e8-3d77-e7f6-b026-f86a68cfa3d9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14702, 805, '72094827-082a-cda7-65af-00a8c9c6d68d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14703, 805, 'af9df58d-16e0-4729-94aa-0d668f509316')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14704, 805, '142e79c9-e8c4-c47d-d8c9-240f8f6d4709')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14705, 805, 'c166e3cc-e7f3-deea-656a-2e8c70d0bd35')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14706, 805, '3fe99ba4-a784-9164-3fc1-300420463367')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14707, 805, '3dedc96c-0641-e2f4-659e-3eb676d01a42')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14708, 805, '22c81e6d-4efe-b28b-c33c-43f7a6a21104')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14709, 805, '2cb9dca6-2a0c-19b4-54b0-4aff685be219')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14710, 805, 'f57a7e4d-530c-b6f2-c181-4d014cafecec')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14711, 805, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14712, 805, '204701c5-51c7-e8b1-8fe2-63bc803b2fa7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14713, 805, 'ccdca087-21ff-6a83-1c98-672d8c2a06cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14714, 805, '4e7ea947-d8bf-a965-1387-6885b7e7dde4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14715, 805, 'd2eb6fea-4f87-2700-5877-7d45f029af22')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14716, 805, '47c46daf-bbc5-41eb-504e-7e47a5f2130c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14717, 805, '0cdb2315-a3a9-551a-fee2-86c028440479')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14718, 805, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14719, 805, '2cdf1f0f-c7fa-5223-67aa-90f3445f3f29')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14720, 805, '21884262-3211-8c56-1ca0-927b21a06595')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14721, 805, '77b0c55a-c546-ab10-c12f-979ecea5dc3f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14722, 805, '8d66e47e-6ee0-7b8f-43da-9827fc1c3e2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14723, 805, '3cd561ec-de72-7702-e4a8-994f5ee1b635')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14724, 805, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14725, 805, '6d379e4d-5b2c-06be-a695-af6e6378b67a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14726, 805, '2865d439-7ac4-bd28-b15f-b6d0c9143402')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14727, 805, '5e20a257-942b-ca0e-68ed-cc5affdf10f7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14728, 805, '13f4dbe2-2158-2861-1e7b-ef6bee1c0145')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14729, 805, '72535c4e-cea1-fc65-7961-f4e2112a0bfc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14730, 806, '07750a0e-7d31-2221-d4c0-1d18ca08ac6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14731, 806, 'f57a7e4d-530c-b6f2-c181-4d014cafecec')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14732, 806, '9520cdbb-7503-a58b-3441-5a8134d09703')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14733, 806, '5560f084-b012-41c5-3970-649bf52b57e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14734, 806, '33309c0b-bf03-b36c-b5f0-eba655052202')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14735, 810, '1b688cd2-382e-758d-92ca-55c18cc25853')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14736, 810, 'aa260fcd-4ba9-f3d9-9e44-bd595a390c05')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14737, 810, '4ef8c4b5-a26b-95e5-9a06-bd5c9e6607db')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14738, 810, '774c3737-6c7f-87f7-b52d-d20f982f3623')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14739, 811, '6457a4d2-0633-223d-6c85-8f8462a64f31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14740, 812, '5d6ef3f4-2ca0-28e8-ec89-10d2a4c27a6c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14741, 812, '84616d3c-db47-0657-2f45-9c24ed1f6b0a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14742, 812, '879ebc90-09f3-a96f-9fb8-9da3bebd0003')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14743, 812, 'f559fac7-1f4e-4e56-c51d-ce72d4e86548')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14744, 813, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14745, 813, 'fb9f9da8-5b83-9b73-3de9-20e6d28b1fbd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14746, 813, 'a1655c38-4b4d-aaf9-6fc9-b67a222eb973')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14747, 813, 'c5c126c6-3358-d8a7-6120-bf2d2434acab')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14748, 813, '38947fe6-0ea4-ed4a-f322-d048905aefb2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14749, 813, '9eb36c54-33a9-7c8b-b089-ed2680d7b32e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14750, 814, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14751, 814, 'a4e03c19-99c7-c8cb-2a85-1530ccbba17e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14752, 814, '8bfa7bc4-a784-e8be-f553-1edcfab3dd91')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14753, 814, 'e662b9f7-1a4d-3331-b562-42a3cae90b28')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14754, 815, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14755, 815, 'd4669ea0-d562-0c07-7c26-8262f37833c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14756, 815, 'df222454-b0a3-76ee-0f3b-d354ca3ed73a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14757, 815, '9e048027-6960-f035-9273-f6051225cc11')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14758, 816, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14759, 816, '861ce7eb-cc40-f182-4b8e-05f40dad2ceb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14760, 816, '2905a64e-e441-a606-9933-120c3bd2c3d9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14761, 816, '4b1c5ebb-cbd6-3b07-de12-2de8974e979c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14762, 816, 'ee95073f-0e1a-7060-4bb6-701dcf67e82e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14763, 816, 'c5c126c6-3358-d8a7-6120-bf2d2434acab')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14764, 816, 'e94060d4-105f-d0f7-6756-f1fda5f62171')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14765, 817, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14766, 817, '4782c94a-92f0-3e2d-4e7e-7b2048e78162')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14767, 817, 'f1e833cc-678f-110d-77ab-e5397a2b2fcb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14768, 817, 'c38f3e6d-0a63-884d-e4b2-e659020dd5cd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14769, 901, '6a1d86c4-fbd7-9228-22b8-090bddbd27fd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14770, 901, 'ddce996f-3b56-5b0d-4d9a-09a5d9e1ca5b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14771, 901, 'b6339c12-009e-8e3e-f6aa-0f839fd3e109')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14772, 901, '2fa20314-2c7c-42e4-94f1-20ae3be7e81a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14773, 901, '154818e4-b027-9dda-281b-241bb8e4d355')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14774, 901, 'ad43aa81-3ed9-738b-c555-2859cd204238')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14775, 901, 'cd5324fb-7cfa-081e-dde9-2938adbc5bf7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14776, 901, '419edfc9-397f-585e-899f-2b41ca9598ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14777, 901, '701e03fa-f712-881a-add5-2f6762f9098d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14778, 901, 'f5e8f593-71d4-9fc6-1f34-3286f0b76463')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14779, 901, 'f3e8e177-8f2c-6cbf-bb78-3ae9e3936032')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14780, 901, '97ea488d-66b1-6aa3-3ed8-3efb030850ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14781, 901, '986d197e-e765-6c94-a4f0-433907814eb7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14782, 901, '22c81e6d-4efe-b28b-c33c-43f7a6a21104')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14783, 901, '1f177b36-3cbc-c933-53f8-49571f0feb7f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14784, 901, '2cb9dca6-2a0c-19b4-54b0-4aff685be219')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14785, 901, 'b3ab8e55-a476-aeeb-8501-4ee124d7535d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14786, 901, 'ef0315ac-b357-efc0-edcd-524f13d651a6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14787, 901, 'bbb67558-85e1-8796-f9d1-53b4f7568b45')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14788, 901, '316212cf-d0ce-8f6b-40e6-5a041f366d06')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14789, 901, '4449f644-ff65-2ca9-4d7d-5e25a5d1d50a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14790, 901, 'fa26c5b9-8ba3-abd1-cac9-5f12cdbe9885')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14791, 901, '264d0480-0974-dafb-77a6-70df82b8b142')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14792, 901, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14793, 901, 'e8d2344f-1e62-a1f1-a248-7ca7db8bc177')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14794, 901, '133fe350-194c-298b-a7f5-804049167890')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14795, 901, '6a692f66-8b55-128a-3e11-82cd2f575734')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14796, 901, 'e0ee8ebf-2e1d-64b1-b8f2-84d4cbf3157a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14797, 901, '4b1c5fce-3cae-aafc-6157-885059c5dbe5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14798, 901, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14799, 901, 'b889a433-517d-b57d-a16e-88b4bde51cd6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14800, 901, '0aa2d60f-5859-4b1f-9868-8cd464ae4475')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14801, 901, '581cf92f-2f40-b0ad-1a8e-92660a54ab04')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14802, 901, 'a6bbdb6b-e11b-b979-fd78-95654b3444ed')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14803, 901, '19593eb8-446e-061a-50ef-9c9624d520cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14804, 901, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14805, 901, '396abf5f-5f81-205a-6d3f-b2f60a0adbf8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14806, 901, 'b89466e7-9dba-24e9-5b70-b43dc882e346')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14807, 901, 'e7eaee00-dd03-1b71-08e0-b483414e27a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14808, 901, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14809, 901, 'b872c54d-3414-aa27-39a6-c3bb90e885c7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14810, 901, 'a5c3b934-5ab2-711e-f05b-cd197de3522f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14811, 901, 'b42d03f1-6211-8b1b-b059-d4c347fb5844')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14812, 901, 'bd2fce46-2c7f-5044-4676-d58c0b972e49')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14813, 901, '04c915f5-3132-5126-3082-d964874a028f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14814, 901, '4f14751d-862c-9b20-12ce-e0741e92fc4e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14815, 901, 'b03fa376-e3e0-74a8-f329-eace8a68325e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14816, 901, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14817, 901, '5d58e641-3835-6ffd-7058-f840b26d66de')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14818, 901, 'c0d4e20e-0473-8f5a-c1e0-fdd0efdd96de')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14819, 902, 'ca87f98b-9b26-052d-255a-02ff39683154')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14820, 902, 'af9df58d-16e0-4729-94aa-0d668f509316')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14821, 902, '32f2b250-7125-8ea6-8bd8-14007d4c6167')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14822, 902, '4973b0ef-f903-2370-774e-1edd2d8bb4ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14823, 902, '154818e4-b027-9dda-281b-241bb8e4d355')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14824, 902, 'f412370c-e696-1589-42e4-24f1c16aa6b3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14825, 902, '3c5305fb-8048-b4e2-45c0-25d3baf32773')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14826, 902, 'cd5324fb-7cfa-081e-dde9-2938adbc5bf7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14827, 902, 'e1f43303-4770-221f-841b-338fc8c58a8e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14828, 902, 'c5eb9582-e548-741b-598b-3ef870ad5104')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14829, 902, '46438258-4610-6b8f-557a-3ff99b3eec90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14830, 902, 'a40c28aa-36cb-1a6e-3f19-46499cdb4ffc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14831, 902, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14832, 902, '3ce268b6-35b3-bd4f-06e1-57cf64745a9b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14833, 902, '4449f644-ff65-2ca9-4d7d-5e25a5d1d50a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14834, 902, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14835, 902, 'ccdca087-21ff-6a83-1c98-672d8c2a06cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14836, 902, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14837, 902, 'e8d2344f-1e62-a1f1-a248-7ca7db8bc177')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14838, 902, 'ebf06e14-6634-43f0-c871-7e13f91be688')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14839, 902, '2e437bb0-d6c0-1cbc-0603-7e47109ee338')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14840, 902, '133fe350-194c-298b-a7f5-804049167890')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14841, 902, '70402105-8ba1-2fe3-a7ed-816621fc3a85')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14842, 902, '41f0ec72-2077-5414-7d03-896cffb03068')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14843, 902, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14844, 902, '5cde7aa1-cfa0-96fa-a374-90051667ecf6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14845, 902, 'b50ae320-2081-522f-27c0-96f8cfc734f7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14846, 902, '19593eb8-446e-061a-50ef-9c9624d520cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14847, 902, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14848, 902, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14849, 902, 'af7d80ab-301a-c511-4348-a5e8d3d3f644')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14850, 902, '0ed55ab5-d1bd-48ec-f73d-a85e8a9dcf13')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14851, 902, '6d379e4d-5b2c-06be-a695-af6e6378b67a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14852, 902, 'da3f9688-2e9a-52fb-653a-b02d97768e39')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14853, 902, '87c2b2af-3d0e-c09e-95ea-be6d39f83ec9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14854, 902, 'c5c126c6-3358-d8a7-6120-bf2d2434acab')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14855, 902, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14856, 902, '2de79d92-73de-db48-d1b0-c80e4f111ede')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14857, 902, '0eeddac6-c675-77dc-c228-cc2475a70178')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14858, 902, 'b4bea114-56e2-604d-9c8e-d0fd632d0a18')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14859, 902, '3a2e5c63-cc7a-6c22-8682-d6e59e0cc34e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14860, 902, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14861, 902, 'ddcbac40-c9ae-238c-b8f8-f5a5f07454b9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14862, 902, 'bde9d070-0465-fcf4-3c52-faba75fe7a1f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14863, 902, 'b7c9c99f-6386-22d5-1558-fd26dc2730b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14864, 1001, '216d6df4-2b07-6284-10ec-00c53cb5ddf5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14865, 1001, '3dcd8f73-9ba4-bc19-a69d-042a83cf27c9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14866, 1001, 'd165535c-837b-d514-d4eb-05c0c843ecf3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14867, 1001, '0351dca6-0c1c-d374-b2c5-0c4601483d9e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14868, 1001, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14869, 1001, '5d9ebcd6-7f49-83c8-2cf1-10997ae670fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14870, 1001, '45a042bb-0af8-e229-f4a5-13d946c98ccc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14871, 1001, 'f6a708e0-e10a-3806-838c-17978b0aa390')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14872, 1001, 'd4cce8bc-5514-d67a-01d4-183c751e49e6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14873, 1001, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14874, 1001, 'ae6cd5e6-fc43-dc75-7892-1b026608006f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14875, 1001, '40d911b6-e6f1-a5fd-1289-1ceb886cec79')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14876, 1001, '9143fee4-cf82-6274-3bbc-21980b01e872')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14877, 1001, '3c5305fb-8048-b4e2-45c0-25d3baf32773')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14878, 1001, 'cd5324fb-7cfa-081e-dde9-2938adbc5bf7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14879, 1001, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14880, 1001, '953e7424-e754-0c60-753e-33ab2cd94545')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14881, 1001, '0f89c62a-001d-e00a-8f55-354813dead6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14882, 1001, '9291c89d-0aef-d53c-3ca8-3619fb3aa0d3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14883, 1001, '457e6c28-9853-cbda-4004-37c9a11ea8f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14884, 1001, 'cd04cc23-00ea-6cd7-bbfc-38d930f64d07')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14885, 1001, '17b60340-c727-7d21-edda-394c82edc75e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14886, 1001, 'd2c1cda6-d4ca-1235-7850-3b49ec17ec46')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14887, 1001, '1009090d-6080-3395-2cb3-3e83f0567d1a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14888, 1001, '46438258-4610-6b8f-557a-3ff99b3eec90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14889, 1001, 'fba0a774-b9d1-2ae5-97bb-40aaa61d6208')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14890, 1001, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14891, 1001, '9928f2f3-a353-15fa-613a-4dc6af384bbc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14892, 1001, '3584e503-c02a-82c8-326c-4e715ef9e8bd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14893, 1001, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14894, 1001, '496176ac-e69c-f23c-9c33-57a799f9179e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14895, 1001, '2c95d955-f99a-12ca-ad7a-5873178e6404')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14896, 1001, '235e28b9-4925-9aa0-dc75-5af0c2d27871')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14897, 1001, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14898, 1001, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14899, 1001, 'ae68a9b7-5d95-9ad0-9eb8-6623747fc232')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14900, 1001, 'fffcf67a-6c2e-5b3b-33e9-68240a4a1de7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14901, 1001, '9aae4a5d-033e-46a5-49b8-72feceae691e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14902, 1001, '85678d14-5f0a-4e33-95c1-7303cd1111a1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14903, 1001, '151300a5-96c3-6edb-7995-7482bcf41c46')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14904, 1001, '1d8953df-6793-0019-0072-74d7384c093d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14905, 1001, '4ab50f6a-c535-c8df-5590-751d08b68d54')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14906, 1001, '45ae71b0-7a5d-3c79-c414-77ddd8792c73')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14907, 1001, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14908, 1001, 'd5babbd6-fc2f-48d6-5e93-7c54b67be00e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14909, 1001, '4fe1282f-7584-c764-a613-81969e51cfef')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14910, 1001, '6e727409-5cea-ba44-aa9c-859621295c9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14911, 1001, '288bfacc-6523-79c9-907a-875af8b805aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14912, 1001, '3c303b22-cd6d-a62f-b111-87989ff6b6cd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14913, 1001, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14914, 1001, '6c06a88a-e292-9721-39a9-8c17888f368d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14915, 1001, '2f0a3224-2b3d-5ab3-5fd2-913f02dcb5fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14916, 1001, '102040e2-4488-62ce-e3e8-92a2f26ab42e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14917, 1001, '92f8baae-2686-1361-afb0-93bd29c93807')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14918, 1001, 'd95c54ae-9766-d0f9-41d6-9d3a2e804807')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14919, 1001, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14920, 1001, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14921, 1001, '438811ab-10bd-e865-fed0-a63c5c9c274d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14922, 1001, '004522d7-f35d-d7f6-7326-a6b1f37ca6b1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14923, 1001, '34fc5611-9e08-ad65-38dd-aafdfe5d7724')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14924, 1001, '6d379e4d-5b2c-06be-a695-af6e6378b67a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14925, 1001, 'f9c65e8c-49ea-6cf4-71f1-b5d182952e41')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14926, 1001, '4f1602dc-c106-dcad-2166-b8123de7e9d4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14927, 1001, 'bfb98549-d0aa-5cca-556e-b81b51b234dd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14928, 1001, '9b2c1609-8fe8-2cab-3021-c216c206afbe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14929, 1001, '8125d95d-006b-912d-5bda-c670d962f33c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14930, 1001, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14931, 1001, '92872849-0540-a879-8fdc-d9fb0037ab66')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14932, 1001, 'd59af2db-56c5-d24d-539b-dd59da8775f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14933, 1001, '9c942a7e-4571-8a1f-618c-e3ae658b677b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14934, 1001, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14935, 1001, '7d8525c1-4924-a9c6-f130-e623196a9b1c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14936, 1001, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14937, 1001, '6512526d-9842-4c6c-45f5-ec15bf26aef3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14938, 1001, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14939, 1001, '4723ab1d-2dad-c8bd-e34c-f197324790ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14940, 1001, '34283a7b-416d-088e-d6e9-f1e9bb0f5bd8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14941, 1001, '8adfd9b1-8af1-31d8-4da3-f3aecf9eab89')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14942, 1001, '20edcc0f-8efd-2e31-5cbb-f4aee89b0057')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14943, 1001, 'aa5c8748-db68-e663-935e-f55dca9f9b59')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14944, 1001, '879ce42a-2984-a7dd-571b-f5b0847a920a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14945, 1001, '95aeacb9-e410-7a51-e061-f6417b3a30a1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14946, 1001, 'aa176a8f-58b4-cf00-0fd6-f9aa7e366431')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14947, 1001, '1161e686-7d20-0798-bebf-fd6029304bda')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14948, 1002, '2be02ded-c434-9595-3886-0484e34a22a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14949, 1002, '71c5f5b4-c63f-336c-4860-04ce7a5abff8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14950, 1002, 'f8468cea-2f1f-3d5c-dc5b-1d5f18fe39a6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14951, 1002, '6e7d82b3-293b-78e4-fade-1e7de5de7f01')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14952, 1002, '229c62aa-e123-9687-10e8-2a7152a3367c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14953, 1002, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14954, 1002, 'a946f84f-6ab7-9936-55bd-2e9af6a0ea6d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14955, 1002, 'b47595b9-e5d5-a14e-6ca5-303b4556889b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14956, 1002, '483fa458-8ccd-6cdd-9660-3e3d16696df0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14957, 1002, 'ee761f5d-ce70-f517-0933-434b312a8d8c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14958, 1002, 'e3e2d200-f5b9-ac59-45c9-44cd3744d095')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14959, 1002, '809b4d4a-0193-7974-45e4-4df59b08719b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14960, 1002, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14961, 1002, '571c8760-70b7-ad11-8da4-52052877429c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14962, 1002, 'dfb2be2e-f4d4-81d5-21b2-540c948a7ad1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14963, 1002, '85347d1b-6fdf-8165-ff97-56c62edd0406')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14964, 1002, '138d5013-0f26-d332-dfe4-72100e071d5c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14965, 1002, '1d8953df-6793-0019-0072-74d7384c093d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14966, 1002, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14967, 1002, '1ad61a47-c4f5-45ec-53e5-76cd9cae0598')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14968, 1002, '06dd5f50-fe7f-9132-c154-7a1549b6c546')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14969, 1002, '4fe1282f-7584-c764-a613-81969e51cfef')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14970, 1002, '288bfacc-6523-79c9-907a-875af8b805aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14971, 1002, 'aca5d5c1-1afd-ea34-2113-a0f939cb1cd2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14972, 1002, '67f70e38-aba0-d27a-9573-b3b8cb43e9b4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14973, 1002, '9b2c1609-8fe8-2cab-3021-c216c206afbe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14974, 1002, 'b4a87cfb-32c0-9791-1318-ca27704bddad')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14975, 1002, '14a6e7dc-5849-9a44-f151-cdb9bcbf8b4d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14976, 1002, '7aa9e0fa-3df9-d5ef-3e1d-d5f2f771a9d1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14977, 1002, 'cd3dd5e8-7fe1-1587-7094-e53c9eacd156')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14978, 1002, '7d8525c1-4924-a9c6-f130-e623196a9b1c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14979, 1002, '20d6dcc4-6273-5565-e3fb-e80e3848486f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14980, 1002, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14981, 1002, '20edcc0f-8efd-2e31-5cbb-f4aee89b0057')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14982, 1002, '95aeacb9-e410-7a51-e061-f6417b3a30a1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14983, 1002, 'aa176a8f-58b4-cf00-0fd6-f9aa7e366431')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14984, 1002, '1161e686-7d20-0798-bebf-fd6029304bda')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14985, 1003, 'b8759390-ca05-7ead-4258-0f60d6d7f216')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14986, 1003, '5d9ebcd6-7f49-83c8-2cf1-10997ae670fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14987, 1003, '3ce2dcf0-05e3-9475-1bee-11120eaabb9f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14988, 1003, '2d687246-c07b-2c59-3e97-162094c6d49a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14989, 1003, 'd4cce8bc-5514-d67a-01d4-183c751e49e6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14990, 1003, '5d9af576-6d0c-5cfc-f909-1ea8ee504149')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14991, 1003, '4405c6a8-9727-349f-d737-25e31855248c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14992, 1003, '769ae22b-c98d-b5cb-7722-27637ac691d6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14993, 1003, '244fff7c-da0a-3d49-b29f-2d15133b5dac')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14994, 1003, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14995, 1003, '0f89c62a-001d-e00a-8f55-354813dead6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14996, 1003, '2ebad782-5d31-0bce-f6da-360722c55dd1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14997, 1003, '1009090d-6080-3395-2cb3-3e83f0567d1a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14998, 1003, 'fb3eabcb-8dc6-a86c-2240-3e9e60640c52')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (14999, 1003, '3f1fb3a9-9688-0415-c450-42eb8ab48beb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15000, 1003, '35a36834-009c-8016-de0f-43923b219e31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15001, 1003, 'e3e2d200-f5b9-ac59-45c9-44cd3744d095')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15002, 1003, 'bad8ef2b-8a82-ff1e-e67d-478c785f9f5e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15003, 1003, 'e3ff0dbd-4360-061a-eda5-4942a662fea9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15004, 1003, '3584e503-c02a-82c8-326c-4e715ef9e8bd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15005, 1003, '2085f15b-201c-227a-b8a9-554c1b161913')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15006, 1003, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15007, 1003, '8ac6e6ae-c671-df31-7975-6577185d5d9e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15008, 1003, '42b3aa20-92a5-dbad-673e-698ef6cda850')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15009, 1003, 'b4c85d1f-2dae-dafb-5f70-6dc044ca03c6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15010, 1003, '2e4ea0e9-bb47-2141-0fe9-71e2aa291d70')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15011, 1003, '4ab50f6a-c535-c8df-5590-751d08b68d54')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15012, 1003, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15013, 1003, '45ae71b0-7a5d-3c79-c414-77ddd8792c73')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15014, 1003, 'dfb7a595-4a55-1589-8d7d-7d7930e6bf76')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15015, 1003, '6459377e-02dd-e5a7-184d-7e6c8450f257')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15016, 1003, '93531860-bb0d-f7a3-cc2b-8394d6ffaac0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15017, 1003, '6e727409-5cea-ba44-aa9c-859621295c9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15018, 1003, '2f0a3224-2b3d-5ab3-5fd2-913f02dcb5fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15019, 1003, 'f4f5b0f0-9ff8-891f-5c7a-94270cb83eaa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15020, 1003, 'eb3629ab-bd2d-e47a-9856-94bf3b7c3296')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15021, 1003, 'c29c8caf-3012-606b-5d5b-94f45d690f47')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15022, 1003, 'e6a49350-cd3e-e77e-01a0-9cf5a11dd55a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15023, 1003, 'c11e2c1d-076d-06aa-3477-9ffd0cc15dfc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15024, 1003, '1f3f2fae-930e-c374-0412-a14267359d27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15025, 1003, 'da268926-b3a7-6142-513d-a6c0ab8fa5e2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15026, 1003, '6abd0883-7229-1f66-7bf4-aa9b37862c3e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15027, 1003, 'b34b4aec-a17a-3fbb-60f4-b0a71563a7f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15028, 1003, 'ea2c5c65-4b64-d301-0134-b3f7f4adc668')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15029, 1003, '3c6f7468-aa94-3f06-f009-b41c6710e69c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15030, 1003, '12d577ab-9fef-3d5c-6879-b536dfbfa1f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15031, 1003, '0bbe3e14-4457-acf6-468e-bb12e11b212d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15032, 1003, '37ddd6ac-cfcb-193c-b394-bb2bf3a24424')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15033, 1003, '108cde9c-7527-44b5-f60b-bd0f070877a6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15034, 1003, '9b2c1609-8fe8-2cab-3021-c216c206afbe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15035, 1003, '6188fd0e-fc21-de15-1b80-cb1afedc9d96')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15036, 1003, '6153f3b6-3075-494a-06a0-ccb534e848bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15037, 1003, 'ea6a1c4a-ff6e-6cd8-8641-dd3940918714')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15038, 1003, '05272a9d-9106-b201-aff4-decf1253be2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15039, 1003, 'b911db3a-edad-5c58-5095-e1831be38ea5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15040, 1003, 'd8e66e68-06ff-8efa-8b2c-eb14baa00fc1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15041, 1003, '747e6181-bd93-0648-199f-ec8f23c11c86')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15042, 1003, '4d84e52c-bd54-ba64-efa7-ecf1bcb3a0d1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15043, 1003, '2c225364-fd40-d7b5-5296-f052949dfc95')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15044, 1003, '3bccad28-52d2-188c-cb2f-f171f14c1ac2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15045, 1003, '95aeacb9-e410-7a51-e061-f6417b3a30a1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15046, 1003, '2287d58c-2c29-8078-005e-f8d826113e3c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15047, 1003, 'c4683230-bce3-7eeb-e121-faabfc8b529a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15048, 1003, 'b9907c16-670a-81bb-d4ea-fc15b65dd798')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15049, 1003, '953479a9-f064-d4e0-6d11-fc77fa487e92')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15050, 1003, '1161e686-7d20-0798-bebf-fd6029304bda')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15051, 1004, '244fff7c-da0a-3d49-b29f-2d15133b5dac')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15052, 1004, '2ebad782-5d31-0bce-f6da-360722c55dd1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15053, 1004, '1009090d-6080-3395-2cb3-3e83f0567d1a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15054, 1004, '35a36834-009c-8016-de0f-43923b219e31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15055, 1004, 'd187e535-fb28-77c0-7624-6be5c2f2d2ff')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15056, 1004, '285eaf91-9459-64a8-d457-72cf3844aca5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15057, 1004, 'dfb7a595-4a55-1589-8d7d-7d7930e6bf76')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15058, 1004, 'f4f5b0f0-9ff8-891f-5c7a-94270cb83eaa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15059, 1004, '6188fd0e-fc21-de15-1b80-cb1afedc9d96')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15060, 1004, '05272a9d-9106-b201-aff4-decf1253be2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15061, 1005, '8c4dc756-49ba-983f-8ecd-057e5fbfc4bd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15062, 1005, 'fbb7a716-c135-0a3c-723d-0729327e6a4a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15063, 1005, '5d9ebcd6-7f49-83c8-2cf1-10997ae670fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15064, 1005, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15065, 1005, '40d911b6-e6f1-a5fd-1289-1ceb886cec79')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15066, 1005, '92992468-120f-6a38-974b-20ecefb66004')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15067, 1005, '4405c6a8-9727-349f-d737-25e31855248c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15068, 1005, '557f512f-3d32-f8d1-d452-2d15ab048742')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15069, 1005, '3c8b7da8-433d-ba31-a5be-2ebec7239ac8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15070, 1005, '2df5d41b-8710-95a1-29ae-3512803fbf3a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15071, 1005, '0f89c62a-001d-e00a-8f55-354813dead6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15072, 1005, 'cd04cc23-00ea-6cd7-bbfc-38d930f64d07')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15073, 1005, '46438258-4610-6b8f-557a-3ff99b3eec90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15074, 1005, 'c8fff3c6-382e-a57b-8954-5750209b12ac')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15075, 1005, '496176ac-e69c-f23c-9c33-57a799f9179e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15076, 1005, '7a67d334-9d24-1133-20c6-595e6f6e92b7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15077, 1005, '235e28b9-4925-9aa0-dc75-5af0c2d27871')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15078, 1005, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15079, 1005, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15080, 1005, '6f62020d-791f-6c7e-bf2d-630bb5fd14e0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15081, 1005, '42b3aa20-92a5-dbad-673e-698ef6cda850')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15082, 1005, '2e4ea0e9-bb47-2141-0fe9-71e2aa291d70')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15083, 1005, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15084, 1005, '45ae71b0-7a5d-3c79-c414-77ddd8792c73')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15085, 1005, 'bb9b4044-189a-3f2f-6bd2-79cfa15b898f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15086, 1005, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15087, 1005, '6e727409-5cea-ba44-aa9c-859621295c9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15088, 1005, '288bfacc-6523-79c9-907a-875af8b805aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15089, 1005, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15090, 1005, 'dcb96b40-a2d0-8756-4e71-948c441110e0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15091, 1005, 'd95c54ae-9766-d0f9-41d6-9d3a2e804807')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15092, 1005, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15093, 1005, '06869b20-da08-19a7-f792-a0bdb5ab880a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15094, 1005, '1f3f2fae-930e-c374-0412-a14267359d27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15095, 1005, '3f86ce3d-bf3c-e0d8-7096-a71463956fbb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15096, 1005, '1cdea31a-f09a-bf55-239b-a80e553099a3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15097, 1005, '7592d652-61cb-e6b6-c024-b104c5035859')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15098, 1005, '12d577ab-9fef-3d5c-6879-b536dfbfa1f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15099, 1005, '5f0357eb-00a6-d7a7-2a5c-b540633b7d71')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15100, 1005, '8125d95d-006b-912d-5bda-c670d962f33c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15101, 1005, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15102, 1005, 'b79fcf82-1033-e8c7-0360-ca12e6969e26')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15103, 1005, '6153f3b6-3075-494a-06a0-ccb534e848bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15104, 1005, 'aadf35d9-1ada-18f7-6ba3-d77bb1e821cb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15105, 1005, 'e5f0a669-8104-167a-dac6-d78c666ff81c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15106, 1005, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15107, 1005, 'd03a0bae-011e-3ddd-0a43-ea1496337e05')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15108, 1005, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15109, 1005, '34283a7b-416d-088e-d6e9-f1e9bb0f5bd8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15110, 1005, '645fc7c0-1ec9-b5a9-b290-f236d583d8e4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15111, 1005, 'aa5c8748-db68-e663-935e-f55dca9f9b59')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15112, 1006, 'b2d3bb4b-74f2-e60d-1515-06090f207eda')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15113, 1006, 'fbb7a716-c135-0a3c-723d-0729327e6a4a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15114, 1006, '2fcd5ff6-1abb-146e-e898-0cfb2d579cb3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15115, 1006, '7c77f76c-1c8e-4ff7-e502-0f2cddf36a48')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15116, 1006, 'fa04aaaa-15f8-16ad-b72f-1addb026c6a9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15117, 1006, 'bc244091-cebd-d0ec-d741-200e6dc906c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15118, 1006, '9143fee4-cf82-6274-3bbc-21980b01e872')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15119, 1006, '4405c6a8-9727-349f-d737-25e31855248c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15120, 1006, '8d029866-6bd3-0954-d957-29e32d547f3c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15121, 1006, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15122, 1006, '0943c0ad-481e-2c55-b299-34ea311e55af')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15123, 1006, 'f4e21902-330e-3d54-d2a7-389e98df21ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15124, 1006, '0e7d25a0-51f5-bae5-c016-38cc88cb7d2d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15125, 1006, 'd101cc5d-c4e9-ab34-ac46-399cd89c1f10')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15126, 1006, '1009090d-6080-3395-2cb3-3e83f0567d1a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15127, 1006, 'fba0a774-b9d1-2ae5-97bb-40aaa61d6208')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15128, 1006, 'fa9f4b6e-a1a8-c06c-2bd9-464e2a2eb690')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15129, 1006, '62e64ead-4fe2-9499-24f8-48df068b0a08')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15130, 1006, 'd53821b6-ddae-cce0-9193-4a2cf5314d5a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15131, 1006, '9928f2f3-a353-15fa-613a-4dc6af384bbc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15132, 1006, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15133, 1006, '9173545c-aad1-1856-0fea-55b40a6f54eb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15134, 1006, 'caf7e870-23cb-cf9d-271c-56461dc6cc6a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15135, 1006, '3cc379ba-a764-1fd5-bc1c-58101846567e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15136, 1006, '235e28b9-4925-9aa0-dc75-5af0c2d27871')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15137, 1006, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15138, 1006, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15139, 1006, '3d8b5c92-9630-6715-76e0-618f1c0d82d2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15140, 1006, '2e4ea0e9-bb47-2141-0fe9-71e2aa291d70')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15141, 1006, 'e741dd51-5520-9d70-0daf-71f1bab1edf9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15142, 1006, '3166cb16-0c8c-4114-4d74-73a1a5edd501')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15143, 1006, '049515f0-2e60-4ace-0e49-756908ea332c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15144, 1006, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15145, 1006, 'fe98461f-fc56-673b-265b-77fc00a2aeab')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15146, 1006, '9ed93f20-634e-cf38-d2b8-792a97678135')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15147, 1006, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15148, 1006, 'd5babbd6-fc2f-48d6-5e93-7c54b67be00e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15149, 1006, 'd2ae9f59-1277-a230-1e19-7d202000a933')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15150, 1006, 'ac8d5b79-e6ca-7528-bd48-7e7c04d7ffed')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15151, 1006, '7e1be9c1-860c-c3a5-eb6d-8362316ce153')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15152, 1006, 'b17d67bf-a15b-4085-a855-854ca5de19cd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15153, 1006, '6e727409-5cea-ba44-aa9c-859621295c9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15154, 1006, '288bfacc-6523-79c9-907a-875af8b805aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15155, 1006, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15156, 1006, '22380625-9597-3c96-6b27-902671791ef4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15157, 1006, '1dd1c49c-1c1d-a88f-47ec-97543d6caba5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15158, 1006, '3ad2659f-ba90-5b2f-1470-999574fedad2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15159, 1006, '7018cebb-31af-3eff-b342-999d54a88ebc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15160, 1006, 'd95c54ae-9766-d0f9-41d6-9d3a2e804807')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15161, 1006, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15162, 1006, '1f3f2fae-930e-c374-0412-a14267359d27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15163, 1006, 'd10cf7cd-de8c-367f-189d-a6f4523075aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15164, 1006, '62766cd9-af63-de6f-b1a8-ac9fe87e829d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15165, 1006, '5ba1d09b-daac-7469-3f14-acb615845e0a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15166, 1006, 'b34b4aec-a17a-3fbb-60f4-b0a71563a7f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15167, 1006, '7f4573de-69ab-5457-ce5b-b3af6ac45bb8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15168, 1006, '12d577ab-9fef-3d5c-6879-b536dfbfa1f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15169, 1006, 'f9c65e8c-49ea-6cf4-71f1-b5d182952e41')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15170, 1006, '751d9941-39a9-6a3b-71f8-b83f16ff1ebc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15171, 1006, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15172, 1006, 'f2ebb64a-cb9c-7c64-2485-cb08b0931ae1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15173, 1006, '6153f3b6-3075-494a-06a0-ccb534e848bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15174, 1006, '1f165faf-4a86-08cb-1aad-d4df115ecb8d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15175, 1006, '1992c5dc-58de-a941-97e3-d630e178d100')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15176, 1006, '3f6789d1-1687-7095-eb55-d7c238d57b51')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15177, 1006, '1bf9d829-a078-8ab8-6d08-db2044f46647')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15178, 1006, 'ba4d42cf-557f-7d3d-1ce7-df095888acfd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15179, 1006, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15180, 1006, '645fc7c0-1ec9-b5a9-b290-f236d583d8e4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15181, 1006, 'aa5c8748-db68-e663-935e-f55dca9f9b59')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15182, 1007, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15183, 1007, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15184, 1007, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15185, 1007, 'fc11ee34-9417-c058-919b-320991684e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15186, 1007, '2b8e081d-743e-3933-b34f-5036fe3199a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15187, 1007, '85347d1b-6fdf-8165-ff97-56c62edd0406')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15188, 1007, '08ac0b32-5770-cd32-d82d-57475ea70898')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15189, 1007, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15190, 1007, 'b4a87cfb-32c0-9791-1318-ca27704bddad')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15191, 1007, '721fba72-5d58-8cd3-d15c-ce59b3cd65cc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15192, 1007, '7aa9e0fa-3df9-d5ef-3e1d-d5f2f771a9d1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15193, 1007, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15194, 1007, '3a364733-6b8f-565c-cebd-e742bb5cbe1b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15195, 1007, '20d6dcc4-6273-5565-e3fb-e80e3848486f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15196, 1007, '96bb5af0-b160-0e74-061b-e8879997d04b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15197, 1007, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15198, 1008, 'fbb7a716-c135-0a3c-723d-0729327e6a4a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15199, 1008, '40d911b6-e6f1-a5fd-1289-1ceb886cec79')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15200, 1008, '4405c6a8-9727-349f-d737-25e31855248c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15201, 1008, '557f512f-3d32-f8d1-d452-2d15ab048742')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15202, 1008, '0f89c62a-001d-e00a-8f55-354813dead6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15203, 1008, 'cd04cc23-00ea-6cd7-bbfc-38d930f64d07')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15204, 1008, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15205, 1008, '2e4ea0e9-bb47-2141-0fe9-71e2aa291d70')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15206, 1008, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15207, 1008, '6e727409-5cea-ba44-aa9c-859621295c9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15208, 1008, '22380625-9597-3c96-6b27-902671791ef4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15209, 1008, 'f78f2b45-7a40-d214-62c6-9ae82edcbc77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15210, 1008, 'd95c54ae-9766-d0f9-41d6-9d3a2e804807')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15211, 1008, '1f3f2fae-930e-c374-0412-a14267359d27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15212, 1008, '34fc5611-9e08-ad65-38dd-aafdfe5d7724')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15213, 1008, '12d577ab-9fef-3d5c-6879-b536dfbfa1f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15214, 1008, '5f0357eb-00a6-d7a7-2a5c-b540633b7d71')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15215, 1008, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15216, 1008, '6153f3b6-3075-494a-06a0-ccb534e848bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15217, 1008, '34283a7b-416d-088e-d6e9-f1e9bb0f5bd8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15218, 1008, 'aa5c8748-db68-e663-935e-f55dca9f9b59')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15219, 1009, '1d39c469-2791-a4dd-cbfd-1dc31ce6c23c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15220, 1009, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15221, 1009, '94372a8d-3059-aa6d-fdeb-314b0b3ea5f2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15222, 1009, '8d3a5e9f-d29a-4126-871b-3326a324e070')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15223, 1009, '0f89c62a-001d-e00a-8f55-354813dead6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15224, 1009, 'cd04cc23-00ea-6cd7-bbfc-38d930f64d07')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15225, 1009, 'ff0c83be-7292-b627-36ac-3d0b020c4920')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15226, 1009, '496176ac-e69c-f23c-9c33-57a799f9179e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15227, 1009, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15228, 1009, '6f62020d-791f-6c7e-bf2d-630bb5fd14e0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15229, 1009, '42b3aa20-92a5-dbad-673e-698ef6cda850')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15230, 1009, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15231, 1009, 'a0de7a41-29e5-e357-9b06-7846104d8387')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15232, 1009, 'd243a252-9bdc-f128-f331-7d07fabc3495')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15233, 1009, '3c303b22-cd6d-a62f-b111-87989ff6b6cd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15234, 1009, '1f3f2fae-930e-c374-0412-a14267359d27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15235, 1009, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15236, 1009, 'a7d2ba56-85d1-605d-d11d-a542072b5678')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15237, 1009, '8b034c62-bcdc-d6db-4fdb-b4191f7caafb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15238, 1009, '12d577ab-9fef-3d5c-6879-b536dfbfa1f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15239, 1009, '5f0357eb-00a6-d7a7-2a5c-b540633b7d71')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15240, 1009, '3098785e-cd9c-f119-31ba-c297abe48bc3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15241, 1009, '6c3e2550-74e2-690f-f4f8-c2d2c3773b1b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15242, 1009, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15243, 1009, '6a843238-7092-94fb-41c1-cfb6aea516c9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15244, 1009, '242f455f-7dc0-c593-4824-e05fd0297222')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15245, 1009, 'aa5c8748-db68-e663-935e-f55dca9f9b59')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15246, 1009, '05e81d83-e9e7-610e-f5d5-f55ff2fa8d22')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15247, 1010, '5d9ebcd6-7f49-83c8-2cf1-10997ae670fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15248, 1010, 'cc389039-c56c-fe33-4748-18687ed07249')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15249, 1010, '5ed3a647-193a-6234-39cc-2797e117a44a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15250, 1010, 'c1c35470-5c83-3020-bf9c-2af74cc63d29')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15251, 1010, 'f4e21902-330e-3d54-d2a7-389e98df21ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15252, 1010, 'd101cc5d-c4e9-ab34-ac46-399cd89c1f10')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15253, 1010, '1009090d-6080-3395-2cb3-3e83f0567d1a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15254, 1010, '8dd369cf-d2cc-7be3-b74d-45e008f02536')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15255, 1010, 'fa9f4b6e-a1a8-c06c-2bd9-464e2a2eb690')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15256, 1010, 'fb6bfc9c-4d84-0088-4bfc-4a9a37fe3238')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15257, 1010, '79df559f-3f58-3d24-801a-5137f39732b8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15258, 1010, 'e960b62d-a6bc-f288-f47f-5c6f75589a2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15259, 1010, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15260, 1010, '3d8b5c92-9630-6715-76e0-618f1c0d82d2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15261, 1010, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15262, 1010, 'cb112a90-8f12-8adf-7615-7f12a2743baf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15263, 1010, '288bfacc-6523-79c9-907a-875af8b805aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15264, 1010, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15265, 1010, 'd95c54ae-9766-d0f9-41d6-9d3a2e804807')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15266, 1010, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15267, 1010, 'd10cf7cd-de8c-367f-189d-a6f4523075aa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15268, 1010, '34fc5611-9e08-ad65-38dd-aafdfe5d7724')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15269, 1010, '5ba1d09b-daac-7469-3f14-acb615845e0a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15270, 1010, '7f4573de-69ab-5457-ce5b-b3af6ac45bb8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15271, 1010, '12d577ab-9fef-3d5c-6879-b536dfbfa1f8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15272, 1010, '2ea831f4-4e6f-ade5-b6b0-c98b6a672728')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15273, 1011, 'fe9ce181-2d87-6cc5-0355-311e836f32e9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15274, 1011, '6057be52-c479-96d7-4a93-e3f85041c7a5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15275, 1012, 'e9597b5b-f076-76f0-8eb3-1912e5169f0f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15276, 1012, '98a659fb-c3f1-0862-8a40-1ce140378a66')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15277, 1012, '04743e9c-32ed-fb75-72f7-341e635f1bd5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15278, 1012, '597f4a4f-e73d-4555-f2aa-7059df61e4c8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15279, 1012, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15280, 1012, '3d46372e-138b-f3ee-b62f-8118b77523bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15281, 1012, 'b331d46b-2395-a920-1e66-956869d7988d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15282, 1012, '1f3f2fae-930e-c374-0412-a14267359d27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15283, 2000, 'a91cb00f-3df5-c06e-81cc-162f970e3a42')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15284, 2061, 'de99d1f2-2c4b-01a4-d3cc-0e1fa56fa574')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15285, 2061, 'b4e15d25-b21e-6d1c-2680-96de52887aa6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15286, 2061, '19812772-6ef8-8936-f1b5-b236ad6e071b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15287, 2061, '32d8d104-2524-9402-a452-bf1f2d95b1e8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15288, 2062, '82cabde4-e579-0875-6b19-04059f44a643')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15289, 2062, 'caa69aba-1bba-74e4-4c30-087671e74dc1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15290, 2062, 'cde3139d-a080-43b2-2051-0b55f96e8855')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15291, 2062, '45a042bb-0af8-e229-f4a5-13d946c98ccc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15292, 2062, '269441f7-136d-1dee-5da9-171d057eeff4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15293, 2062, '92db7ba4-2135-ecff-4c94-277c3733ef0d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15294, 2062, '1c472a20-9b80-2d25-e76e-2e1a2611b483')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15295, 2062, '3b877e49-c7a7-6a43-3ab5-3ee70a8ff318')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15296, 2062, 'ccc3f045-d042-00c0-93fa-55817ad4141d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15297, 2062, '4f2e8e19-62e7-731e-a2b7-60f9f32d73df')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15298, 2062, '761e8aac-4ba8-77ef-4dcd-6a3dd8321b25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15299, 2062, 'bb8825c7-b7cf-9e4a-d6bb-767f1cb01e77')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15300, 2062, 'eb3629ab-bd2d-e47a-9856-94bf3b7c3296')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15301, 2062, '5244e475-500b-3a5b-b8eb-9bcfc5fb5ef8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15302, 2062, '36a26e26-972c-a80c-5ad6-9eadb0be95ea')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15303, 2062, '55e396ff-71c9-e9dd-9561-a1d52f3ecef1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15304, 2062, '39e1636f-e2b2-3800-f459-b7e8e08a9d90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15305, 2062, '9b2c1609-8fe8-2cab-3021-c216c206afbe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15306, 2062, '29c2a358-2f38-67f0-127f-c2214d0355a7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15307, 2062, '9284338e-79e0-5bca-d8fa-dbd305d9d24c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15308, 2062, '24188eb7-9808-74c9-4797-e97118314cce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15309, 2062, '743d9a82-7ded-4a06-7852-eecec9d53db2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15310, 2062, 'f590d8d7-1f80-9cdb-b14b-f1c4b4693ab6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15311, 2062, 'ee83ac7a-88b2-29e3-0a2c-fa22b296df1b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15312, 2062, '63bb9079-ab38-e008-e836-fc87c24dfb53')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15313, 2063, '8e305d86-a84a-3359-8801-5bec74e48ba9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15314, 2063, '3d8b5c92-9630-6715-76e0-618f1c0d82d2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15315, 2063, 'e58c5e2f-40c7-e8aa-2fa0-b89305f1c59b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15316, 2064, '798f86b0-9f0f-0d97-fcca-16a66ecaa474')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15317, 2064, '3ec51946-cdfb-07e6-7892-254bfa16c9a4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15318, 2064, 'c73556c2-0514-d67f-8f35-7584e834fbee')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15319, 2064, '7d9bde30-dda1-79d2-49ca-77513c8b8c46')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15320, 2064, 'eb3629ab-bd2d-e47a-9856-94bf3b7c3296')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15321, 2064, '1f3f2fae-930e-c374-0412-a14267359d27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15322, 2064, 'e58c5e2f-40c7-e8aa-2fa0-b89305f1c59b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15323, 2064, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15324, 2064, 'c32bc170-7397-f950-1ead-fc907ebffcf3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15325, 2065, 'c73556c2-0514-d67f-8f35-7584e834fbee')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15326, 2065, '7d9bde30-dda1-79d2-49ca-77513c8b8c46')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15327, 2065, '9d411636-6f22-8c31-948b-b73d7afd50c6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15328, 2065, '6889aea2-a4d6-5179-5a00-cc49c8696a88')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15329, 2065, '928dc3dc-bf62-8ed2-7ca6-e90ac42a7383')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15330, 2065, 'bdbe7e22-b5fc-df8b-dbfc-ec43e8355fa9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15331, 2065, 'c32bc170-7397-f950-1ead-fc907ebffcf3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15332, 2069, '798f86b0-9f0f-0d97-fcca-16a66ecaa474')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15333, 2069, '599457de-31f6-59a4-6139-21d97bdffecd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15334, 2069, 'c3a9ecf1-0bf0-b18d-e550-738ac0cc0ba4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15335, 2069, '8371d073-fc84-8f2b-43f2-d05b3bfb7f79')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15336, 2069, '48debb0e-fc57-554d-eae5-eefa69b6ed19')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15337, 8001, 'e1364041-cd01-191d-fd95-009a8816b3c7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15338, 8001, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15339, 8001, '69bfb1ae-16f3-e5fc-e68e-05084ab405db')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15340, 8001, 'b6f3d30c-43ab-aaf2-48e7-0623e97db49a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15341, 8001, '2b4efafe-1209-c08e-7cf2-07731b92d7a6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15342, 8001, '259cfca3-b68f-a18f-7867-0ab66c7a5630')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15343, 8001, 'e48751d3-abf2-e8ef-cfa3-0bb2a06c54a2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15344, 8001, '9c645d7c-9905-a07c-2418-0e6c25809523')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15345, 8001, '6ddcffbb-69b0-f390-6722-103a7f2fa85d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15346, 8001, '5c19632f-d520-10c7-ff36-1689d77b0e57')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15347, 8001, '5b5426e9-dc45-af10-515e-179f3225a2de')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15348, 8001, '28367da9-ca4e-5f94-5ad9-199cc98965fa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15349, 8001, '1767aef2-a3c6-c59a-ea37-1c996d29a200')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15350, 8001, '9edacd18-0d76-f740-113c-20015b9a256c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15351, 8001, '7dd9c053-ab6e-0c2c-3dd4-21477bb3a5b5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15352, 8001, '4429092b-37a1-f967-cd20-234d1327032f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15353, 8001, '142e79c9-e8c4-c47d-d8c9-240f8f6d4709')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15354, 8001, '60e70074-a64c-09d7-0b86-255e3354a27e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15355, 8001, 'ad43aa81-3ed9-738b-c555-2859cd204238')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15356, 8001, '701e03fa-f712-881a-add5-2f6762f9098d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15357, 8001, 'cb98e75c-1d6a-27bf-8edc-311b01c1674d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15358, 8001, '20b4e685-4847-fb2e-e80f-35a0479d5ae9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15359, 8001, 'c35fb626-7d22-9c17-796c-378f28cea207')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15360, 8001, '5ee9b39d-98fb-78f3-67ba-3999154e8b99')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15361, 8001, '8b95573a-c77d-46c5-88e5-3d549eb0a1c1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15362, 8001, 'f3adbbda-b2bd-c8a9-0145-3ef1b2bff22f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15363, 8001, 'a2811700-c8ef-d726-4ff0-3f864d06bebc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15364, 8001, '1dc7c93f-87b4-90ed-aba7-450aa1d3afc7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15365, 8001, '04822a70-a4a6-906f-0241-4576afe0148d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15366, 8001, '52f465af-aa5a-e748-d54c-4934d6e1ea4b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15367, 8001, 'f2c42a50-8944-1dd9-2431-4b24b8a31071')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15368, 8001, '8a850dad-0354-7d29-1ab8-4c7a6ebdca8e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15369, 8001, 'cca7002b-907b-100c-aff0-543c84e6958f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15370, 8001, 'f8ec9318-124b-e4af-8ff7-585618daf4bc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15371, 8001, 'd33a8c0c-ef3c-660b-7874-595c33aaa16b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15372, 8001, '78dac62c-ebcb-39b3-2bc0-5c15a4faaff8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15373, 8001, '6c7a41c4-4aac-8d18-f04c-6351a77603c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15374, 8001, 'f382557c-3a42-7b30-5f1b-66ceab53f6cb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15375, 8001, '1ac6fdeb-4321-33f9-a9f4-67aae6956368')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15376, 8001, '27f9dced-93ee-9a2b-8dca-6beb52afae98')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15377, 8001, '91ea5654-699f-5255-f9b6-6e63acba80e4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15378, 8001, 'b2687321-49ba-47fe-9a30-6ee1a4e1fcc4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15379, 8001, '272dd5b4-3c09-a920-7f8a-72e5f8c13008')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15380, 8001, '311a783b-1e40-1778-d1e4-75830adabd37')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15381, 8001, '06a8eb2f-62de-b544-08b2-794ae50cf741')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15382, 8001, '472fed04-32b0-b203-4470-7c80e8c2ffb6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15383, 8001, 'bd1a2d5d-2da0-be69-3d76-84693205523c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15384, 8001, 'e0ee8ebf-2e1d-64b1-b8f2-84d4cbf3157a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15385, 8001, 'b274f4bd-a793-7642-428c-8688f24f94a2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15386, 8001, '7b6270b1-7b51-602c-3837-871ae4c61a02')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15387, 8001, 'd73b0f8f-6a1d-048a-f061-89c5898b3d60')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15388, 8001, '5740231f-718a-0d3f-b6c6-8e10d71690f1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15389, 8001, '8d66e47e-6ee0-7b8f-43da-9827fc1c3e2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15390, 8001, 'b26013e2-fbbb-f820-ac54-9e627363f269')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15391, 8001, '3e3adaf3-4432-0251-b12f-a339d0396278')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15392, 8001, 'e91d7a99-16b3-266b-0e56-a9227da71dac')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15393, 8001, '5831e5d2-ebd1-0a57-1726-aafed1dfd899')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15394, 8001, 'd6706ecd-e138-b578-1326-ab5bf39ed355')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15395, 8001, '358eed6a-b6ed-dcf3-b7d7-b3e427298d50')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15396, 8001, '0829322a-05a1-ac32-c66b-beae46ed3651')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15397, 8001, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15398, 8001, 'c1e70577-1a92-224a-499d-c3d9329ba740')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15399, 8001, '4d468906-0dc1-6a3e-3552-c3da9d3a1174')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15400, 8001, '92c8354f-4994-1358-a39a-c4c43fdc02f0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15401, 8001, '6889aea2-a4d6-5179-5a00-cc49c8696a88')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15402, 8001, 'c8cba6c6-162f-98c0-35f1-d1061f34990e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15403, 8001, '22b7d0b9-30b0-9dd3-4e64-d20ad3949e27')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15404, 8001, '573269c9-09b1-7ff3-6772-d4ee0abf4578')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15405, 8001, '243ded1e-5a21-10fe-bc29-d6b8af6f6723')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15406, 8001, 'ce636556-3219-a3ac-30c7-d6e354254ef2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15407, 8001, 'a061309b-ea83-bba2-92ba-dac4e6bdbb1f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15408, 8001, 'ca271e6e-28ee-e5aa-791e-df3617f0d713')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15409, 8001, '4f14751d-862c-9b20-12ce-e0741e92fc4e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15410, 8001, 'bd7dc7a0-43dc-1a4a-19a7-e83bed667668')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15411, 8001, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15412, 8001, '00376e37-8d45-7f8a-3c98-fa91d1fd6fd6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15413, 8001, 'bfe62a22-a57f-cb1e-a299-fcd5f0b8a465')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15414, 8001, '4e3b14f6-c4bd-61b3-4481-fdbf7e07f0ab')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15415, 8002, 'b0cfec47-c18e-e4cc-cf2a-135c92539542')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15416, 8002, 'f93dbfc1-b55c-07fa-d8d1-1be8dcecc00a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15417, 8002, '60e70074-a64c-09d7-0b86-255e3354a27e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15418, 8002, 'a6630799-4b87-7dbc-e5f2-316b36cf4b56')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15419, 8002, '399ce002-5c50-c646-2dbb-353b9856b7e6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15420, 8002, 'a2811700-c8ef-d726-4ff0-3f864d06bebc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15421, 8002, '46438258-4610-6b8f-557a-3ff99b3eec90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15422, 8002, 'e5c84105-63c9-75ff-e276-41fe8907d03c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15423, 8002, 'd33a8c0c-ef3c-660b-7874-595c33aaa16b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15424, 8002, '08125fc6-97f1-ffd5-145f-5d81c47d0cc2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15425, 8002, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15426, 8002, '6c7a41c4-4aac-8d18-f04c-6351a77603c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15427, 8002, 'c5c5782f-ca4f-6ddc-25b2-69c32982688e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15428, 8002, '14000c5b-8c77-2038-cd3b-6a1cf6bda6e8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15429, 8002, '9d4a1827-9143-c1a5-fd0d-730e02558ea7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15430, 8002, 'ebf06e14-6634-43f0-c871-7e13f91be688')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15431, 8002, '94b6d179-62b5-a5d8-0673-853e3883d79b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15432, 8002, '7b6270b1-7b51-602c-3837-871ae4c61a02')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15433, 8002, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15434, 8002, 'f9091a19-32c7-d668-5ac2-9d0edefcf125')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15435, 8002, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15436, 8002, '5831e5d2-ebd1-0a57-1726-aafed1dfd899')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15437, 8002, '089969a7-e0bb-7bd4-0c7f-ab3597a22713')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15438, 8002, '4aa70940-467c-0064-7531-ba829dd3feaa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15439, 8002, '39a4f008-2300-1f57-6406-c4f269378a1c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15440, 8002, 'c51bad2e-e402-306d-cfa0-c539e22c0fcb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15441, 8002, 'c10ca7e0-43fd-bddf-d7f1-d5accbb7111c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15442, 8002, '61a88a8e-5e6f-f389-6d90-e0c54b90bdde')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15443, 8131, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15444, 8131, '9f483be0-b817-6292-9d5e-1de97dae0313')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15445, 8131, '95fc0f25-087d-1fab-cc55-45bcf6e84ed1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15446, 8131, '13218603-f29b-411c-a8a1-f8fbe14e1bbc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15447, 8132, '356a68e8-599f-4754-8489-0130876c6de7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15448, 8132, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15449, 8132, '7356e9aa-44cf-b642-36cd-adbd79d15179')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15450, 8132, 'e11a0701-31e2-b9e3-947a-bef6978a8f8e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15451, 8132, '13218603-f29b-411c-a8a1-f8fbe14e1bbc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15452, 9003, 'ddce996f-3b56-5b0d-4d9a-09a5d9e1ca5b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15453, 9003, '4973b0ef-f903-2370-774e-1edd2d8bb4ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15454, 9003, 'c5eb9582-e548-741b-598b-3ef870ad5104')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15455, 9003, '196a1249-18dc-52b2-657b-4b2503ac7c79')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15456, 9003, 'f413144d-38f8-1289-bc87-62289ee1a31f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15457, 9003, '8dbaf7c1-a92d-3989-78d1-814273a55c01')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15458, 9003, '19593eb8-446e-061a-50ef-9c9624d520cf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15459, 9003, '1bf0285b-f898-7db4-83a8-a2ee155abf1a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15460, 9003, 'f9eaa403-4879-b84c-cf96-b3faf352a962')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15461, 9003, 'bde9d070-0465-fcf4-3c52-faba75fe7a1f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15462, 10007, '701e03fa-f712-881a-add5-2f6762f9098d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15463, 10007, '8d78dfeb-06a2-e865-fbd1-69044f2f267f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15464, 10007, 'c02a55d1-dd17-2e24-6be8-8f58e6cb0bb5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15465, 10007, '6889aea2-a4d6-5179-5a00-cc49c8696a88')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15466, 40401, '6101d587-9b42-b850-c56c-156394842fe8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15467, 40401, 'e8577104-e2c0-cec3-b159-1a065cd3da25')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15468, 40401, '0a484799-c98f-444f-684c-2d38957a357d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15469, 40401, '7866bced-88dc-e43b-e31b-341e8eb14c6a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15470, 40401, 'b8ad64bb-fba7-935e-f803-4b961111a553')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15471, 40401, '809b4d4a-0193-7974-45e4-4df59b08719b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15472, 40401, '677077f4-028d-ab57-a4b5-6362379ab960')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15473, 40401, 'd4ae0381-b12c-81a6-011d-679923ff4b7d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15474, 40401, '0e38ba0e-211b-332a-a8b3-72013306b1db')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15475, 40401, '9aae4a5d-033e-46a5-49b8-72feceae691e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15476, 40401, '8a1b1920-d1ea-3a86-ff77-8173370741a0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15477, 40401, '07695bba-dd6c-8f0b-10ef-a9a05fbda060')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15478, 40401, '4216cbdd-7a4c-99fb-957c-bf846572005e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15479, 40401, '009e99fd-8bf5-a10c-0770-c79d34d4cfa3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15480, 40401, 'eab74e5a-92b8-a493-565d-cccef4f3e7a3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15481, 40401, '9939c7b4-c38d-331b-0fe8-ddc89996d74a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15482, 40401, '1e83a163-6030-faae-8b5d-ec09eec1cae9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15483, 82010, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15484, 82010, '8c369404-41b6-6c35-6ad2-0fccd1e688ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15485, 82010, 'a5bb735c-8a6e-6919-99fb-38f6ec06ccc6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15486, 82010, 'f26410fa-ddeb-420a-5cae-66edca4417a7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15487, 82010, '37d6c9e6-ceea-78ef-94d6-6a92cc45aec0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15488, 82011, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15489, 82011, 'a829f8dc-782d-f686-d6e0-dde9cb667df2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15490, 82012, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15491, 82012, '584dd075-eda6-ddc7-3f7c-8e77cf2ae806')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15492, 82012, 'a40a2dc6-26e9-7aef-7500-8f39e756b661')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15493, 82012, '7190f2a6-73c4-2fdd-ec06-b420680e17c9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15494, 82012, 'd98737af-028e-8567-315d-c6f0e16b0a29')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15495, 82013, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15496, 82013, '146043e2-06d6-5e02-82d9-0825fcf34146')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15497, 82013, '17d0a2d5-682c-2dce-359b-5a43cd28d8b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15498, 82013, '17c7a743-6a66-c65f-f6ba-79bd2c4cad3a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15499, 82013, 'ab34038e-7786-4c11-ff59-b1ff5e986af8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15500, 82013, 'dfafb8ae-4676-0c3e-d887-caca8acd6045')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15501, 82014, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15502, 82014, '146043e2-06d6-5e02-82d9-0825fcf34146')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15503, 82014, '6be69779-7d8d-bc55-7bc0-1e9e34cc9290')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15504, 82014, '825ea17c-7bb1-8bcc-e482-342b7cc7488f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15505, 82014, 'b5cbbca6-1c9a-3c77-4293-65df9fe86ffd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15506, 82015, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15507, 82015, '146043e2-06d6-5e02-82d9-0825fcf34146')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15508, 82015, '4174a389-e5c6-7acc-9ea4-11bb02e0fe8e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15509, 82015, 'c71505a7-3a8f-dd0e-32b5-12444b6793c6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15510, 82015, 'eb76178e-9f5a-288b-ff13-7f6b20933c2a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15511, 82016, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15512, 82016, '146043e2-06d6-5e02-82d9-0825fcf34146')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15513, 82016, '93614c0a-e4b6-2f9b-c785-a8e4da4b961b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15514, 82016, '40bc223a-36bd-4ac8-e1f3-b2d5ebdcf74f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15515, 82016, '47896dd1-8c7c-ac0a-54c4-f287c06296eb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15516, 82016, 'bae2e179-68f3-e5cf-4c7f-f84fe6445323')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15517, 82016, '7fa24fc3-a3fe-3a42-d864-f9df629d1cd0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15518, 82017, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15519, 82017, '146043e2-06d6-5e02-82d9-0825fcf34146')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15520, 82017, 'd2d97fc4-c983-7b2a-1814-0de5e75f9025')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15521, 82017, '5d82c00c-943e-45c6-2b2f-971ddf896ecb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15522, 82017, '4114e6df-9439-c58f-cba6-b35a6670ce68')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15523, 82131, '367650b9-219a-7304-8e6c-02247ade9b75')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15524, 82131, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15525, 82131, '146043e2-06d6-5e02-82d9-0825fcf34146')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15526, 82131, 'e88f19f6-13b8-d46d-873b-190501a89cb2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15527, 82131, '13f8cfd4-75d5-b246-647b-29c3e1e2714b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15528, 82132, '367650b9-219a-7304-8e6c-02247ade9b75')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15529, 82132, '92fd8e87-20a8-37de-68a0-0446a6cb31b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15530, 82132, 'a87e9476-53d7-456a-9020-06ed1b03c6d4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15531, 82132, '146043e2-06d6-5e02-82d9-0825fcf34146')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15532, 82132, '444488a4-3017-3ef2-0663-a52490530d2f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15533, 82132, 'e62e0cad-3fd7-00f9-9286-d88c09715e17')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15534, 111001, '0f740c19-a121-de5f-f991-081977862670')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15535, 111001, '483702ba-4f97-16aa-67d0-0e894d8fbeee')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15536, 111001, 'd9ba5435-72af-33ae-b812-1217ba6cbc22')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15537, 111001, '0e6bbd8b-519a-62fb-f616-13f3c38df4ba')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15538, 111001, 'a9111e87-f085-7d73-a4dc-1a5c5c3cdb14')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15539, 111001, '43ab23ba-8ee7-b49f-a174-1ef4f5e90b64')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15540, 111001, '839daa42-90e2-239b-dbb9-24a69e0bf4b8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15541, 111001, 'e68660dc-5464-674a-a147-2890a4c1a5bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15542, 111001, 'c6e2c025-85d1-fed8-ecec-2cb463a1d2ff')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15543, 111001, '701e03fa-f712-881a-add5-2f6762f9098d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15544, 111001, 'e28d52f1-ba66-95fb-66a9-3237fdfe3751')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15545, 111001, '848c4754-dd2d-381d-2ef9-3b179dfbd02b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15546, 111001, '71910b7d-b710-619e-6e65-40c8e8f61b3a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15547, 111001, 'e6179ba2-c073-9958-f221-4868e95f051b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15548, 111001, '4106c8a3-f4ce-6267-42cb-48d04d24a9a3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15549, 111001, '52f465af-aa5a-e748-d54c-4934d6e1ea4b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15550, 111001, 'c04d49d2-d022-4633-8ec8-4b23692b6c2b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15551, 111001, 'f79412da-668e-b3d6-e645-59821bfbfd43')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15552, 111001, '384fa1c3-7eb2-d436-63a3-5b514d27c0b3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15553, 111001, '1c48da3c-56a6-93f3-8d4d-6b4ba1969185')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15554, 111001, '04b0fee9-323c-df1b-0490-72c3450eee47')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15555, 111001, '2dc6f170-87db-bdf7-1aa8-7b47f376b3e6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15556, 111001, 'df91f329-3197-3579-74ca-7b4db7db1c31')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15557, 111001, 'e0ee8ebf-2e1d-64b1-b8f2-84d4cbf3157a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15558, 111001, 'cd066b16-3fb3-77f7-d198-8860877f6b74')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15559, 111001, 'b4408073-4330-6a20-28da-8c9415214983')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15560, 111001, '29d72840-80bb-d253-189a-96a6bb41cc7f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15561, 111001, '3aa02ab2-e527-3a40-e93b-9af6a2001dca')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15562, 111001, 'cadd0d3d-ff4d-ce12-e645-b29c602c3386')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15563, 111001, '8178bd26-7160-65ce-6fd9-b70cf36bbf7d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15564, 111001, '71006233-b65f-b679-b9af-c177ba0b61b9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15565, 111001, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15566, 111001, '7674c692-74fe-b517-322d-c7f44ccec410')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15567, 111001, 'b1982128-07ed-ec9a-744c-ca191b02b15a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15568, 111001, '0c20024c-7447-f0c3-abbd-d259d52ac393')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15569, 111001, 'bbb72d54-10e1-e089-feaa-da8666c11125')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15570, 111001, 'd353dd8f-77a3-51ce-36a6-dac7755a85cc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15571, 111001, 'd7a735ba-36f4-4a02-8a01-e052b0a644e4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15572, 111001, '4f14751d-862c-9b20-12ce-e0741e92fc4e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15573, 111001, '4bc9b45f-3b71-e1f1-7584-e221f2468a71')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15574, 111001, 'ea3be831-a542-004d-1ce8-e31b0843fc42')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15575, 111001, 'f12e9a9d-c10f-34d6-192f-ece8839995f9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15576, 111001, 'f679c18e-efcb-7b11-8a16-effbd484053a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15577, 111001, 'efb03a91-497a-8dfb-99f7-f438cf763e8c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15578, 111001, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15579, 111001, 'd9a822e8-3d77-e7f6-b026-f86a68cfa3d9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15580, 111001, '4160e683-9e01-6d84-c1ef-f992c439edd5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15581, 111002, 'fe469b82-e16c-2c30-c580-0b44822e6587')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15582, 111002, '81084451-817a-f837-d446-3708c02c2cc2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15583, 111002, '3771dd39-6dea-c4a3-46b4-5e792f601408')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15584, 111002, '204701c5-51c7-e8b1-8fe2-63bc803b2fa7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15585, 111002, '10a8c247-e225-fc20-a242-702fbcca7c4d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15586, 111002, 'a31cf10b-c808-f8cb-32a6-7b204b5c43e6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15587, 111002, 'ebf06e14-6634-43f0-c871-7e13f91be688')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15588, 111002, 'bdaf235b-32c2-c2c6-92d5-8c0a76676749')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15589, 111002, '29d72840-80bb-d253-189a-96a6bb41cc7f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15590, 111002, '4974c9e4-135d-4648-4320-9ed9a368f2e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15591, 111002, '3e556dbb-b984-d15c-6058-a4e799d9d5d5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15592, 111002, 'eb2a305b-33f2-5599-babb-a660bdfce4a8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15593, 111002, '0c95d5e4-2c6e-9c2e-6b27-b7d278c747e1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15594, 111002, '0c20024c-7447-f0c3-abbd-d259d52ac393')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15595, 111002, 'd353dd8f-77a3-51ce-36a6-dac7755a85cc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15596, 111002, 'ea3be831-a542-004d-1ce8-e31b0843fc42')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15597, 111002, '1ceb0cf6-f0b5-98bc-8674-e6601e65f8ee')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15598, 111002, 'f679c18e-efcb-7b11-8a16-effbd484053a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15599, 111002, 'd9a822e8-3d77-e7f6-b026-f86a68cfa3d9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15600, 111010, '000524d1-897c-7cc5-d8a1-11af3a7e4ac4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15601, 111010, '181e72f6-cabe-f1a8-1b6b-4333ee6209c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15602, 111010, '8c1e1147-3a2e-6cfc-2218-43e8d872ed8b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15603, 111010, 'c2712ea6-dbd4-876a-5527-4b454934a422')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15604, 111010, '0a2f6e10-0cd9-204e-336e-4e30953d94e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15605, 111010, '89cb5f05-3849-498f-d5e1-4feeb19f56f3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15606, 111010, '5c03b2b5-9f73-6181-74a2-6aff4ce4df21')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15607, 111010, '0bb8be81-9261-6f79-5ea0-88d7a54b1c8d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15608, 111010, 'd596c03b-a6b2-b05e-d66d-8c615e913803')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15609, 111010, '6f051758-ff87-105c-493d-acd866ad7646')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15610, 111010, '2a6f00f6-9fdc-70f7-aeeb-c569e2cac343')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15611, 111010, '6153f3b6-3075-494a-06a0-ccb534e848bb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15612, 111010, 'bca232fa-8927-ad21-899e-d272666f07c5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15613, 111010, '4a7174f0-1d99-451c-97e8-de49e6cfda3a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15614, 111011, '2ef6e08e-def3-869b-d714-470303184bfb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15615, 111011, 'be7fac29-6b22-b4f0-c638-47fc8e1b4a83')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15616, 111011, 'a0966d46-6ba4-32f4-6e0b-68fab11de99d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15617, 111011, 'a2d4f1dc-13f7-3161-0dd6-828dc62ea3c3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15618, 111011, '16b0d9a7-8f35-eeec-02d9-f2e77fe5e2f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15619, 111012, 'f85fbd03-f7a4-ec51-eb43-0442989313a7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15620, 111012, '3476b098-a2ca-a7d5-d43b-05c55e655ea7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15621, 111012, '04fde87c-91b4-2aa5-a9d1-2b4a113bb9fc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15622, 111012, '2ef6e08e-def3-869b-d714-470303184bfb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15623, 111012, 'e63cf3a6-9012-ca2c-df9b-4e2c1a0c12fd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15624, 111012, 'c630ff65-80cd-3635-4d71-6bd19fcbfa9a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15625, 111012, '78f4f0b5-1220-ad01-eefd-778249e8e970')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15626, 111012, '8a165a98-3b72-c21f-7b9b-8d291fbaf547')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15627, 111012, '32e36394-1bf2-1e5d-5043-9a5871f95b39')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15628, 111012, '576e6578-1bd8-9f1c-0101-9bccd0526106')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15629, 111012, '04e214c7-4ed0-9ffd-c049-a92d1565e32e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15630, 111012, '3eda09ba-b08e-c0f8-bf06-b4355072bd8d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15631, 111012, 'b7100077-37c9-1116-3079-c94b9c8fd3b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15632, 111012, 'cac892b0-ab55-6d57-197f-d684364461a4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15633, 111012, 'f915d872-387d-e34e-da96-dc5092552912')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15634, 111012, '8b1d0eff-62b4-0bb9-791e-fa1fd332be54')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15635, 111013, 'a783d51a-acd5-f231-a678-168b4dbb53f6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15636, 111013, '2701758d-9c64-e5d1-ae3f-1bf4010124ee')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15637, 111013, '0d45ad83-2d82-4a88-9648-1fcb3996de06')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15638, 111013, '10feada8-4325-7c92-4eff-553871453eee')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15639, 111013, '4e31edfc-3ad6-dd2d-3324-7d704b5b9c51')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15640, 111013, '462c5edd-dfe4-1046-79ea-8e7762fab32a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15641, 111013, '7f95b101-a38a-d6ed-2ff4-db8905612c1f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15642, 111020, 'e31544a7-106f-dc52-9ec0-a39ea807125f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15643, 111020, '6f051758-ff87-105c-493d-acd866ad7646')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15644, 111020, '8a9b1273-c5c8-a3fa-5df8-bdc40f4fc8fe')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15645, 111020, '2f245968-3f0a-99ab-bb21-cd53e8fdda90')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15646, 111020, 'f3c6f9f3-0ed9-c30c-a8ae-f6d18a29938d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15647, 111021, '89cb5f05-3849-498f-d5e1-4feeb19f56f3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15648, 111021, '3d0167f1-4184-3607-49ab-66948f8e7cfa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15649, 111021, 'cd8c0c2a-5fcd-25ff-e150-ad0d6f59dfaa')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15650, 111021, '580650c5-1c9a-8e56-cbbd-afced91e85cd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15651, 111021, 'ebb6065d-7d6c-e9f5-995b-e357807f7e1a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15652, 111022, '33f7a8a9-5dae-3211-4470-047c648d55a3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15653, 111022, '000524d1-897c-7cc5-d8a1-11af3a7e4ac4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15654, 111022, '8be9330e-fe7d-9fd0-ea62-9e03f909de19')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15655, 111022, '1ada977e-a64d-4b85-032b-b07d9a6e86ef')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15656, 111022, 'd5f9feaf-99b2-07e6-fd64-fb1e935c37ce')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15657, 111030, 'b4bbbe08-069e-ed3f-aa90-0913eb90565b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15658, 111030, 'd9ba5435-72af-33ae-b812-1217ba6cbc22')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15659, 111030, '8a04c353-58da-fb56-6e9f-35de36695597')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15660, 111030, '1f411963-87d5-1629-d088-4087652540eb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15661, 111030, 'f385ba34-2716-9a20-2578-494558425ef3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15662, 111030, '1a389195-043e-c807-69e0-6fce4e13c7e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15663, 111030, '2592251c-754e-0962-4d6d-ac214a847005')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15664, 111030, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15665, 111030, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15666, 111031, 'b4bbbe08-069e-ed3f-aa90-0913eb90565b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15667, 111031, 'd9ba5435-72af-33ae-b812-1217ba6cbc22')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15668, 111031, '6cadbdee-95ca-8049-6caa-2a46227ef3ee')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15669, 111031, '4b61068c-21b5-ab70-2da2-40b3b0ea6a1d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15670, 111031, '884b6eb7-6228-efc0-1677-4a154aff88b2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15671, 111031, '1a389195-043e-c807-69e0-6fce4e13c7e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15672, 111031, 'c84ec08f-f06e-f93f-5751-af23a6553721')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15673, 111031, 'e153964d-9d25-f4c2-6664-bdcc39f76c6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15674, 111031, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15675, 111031, 'b03fa376-e3e0-74a8-f329-eace8a68325e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15676, 111031, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15677, 111032, 'b4bbbe08-069e-ed3f-aa90-0913eb90565b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15678, 111032, 'd9ba5435-72af-33ae-b812-1217ba6cbc22')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15679, 111032, '8a04c353-58da-fb56-6e9f-35de36695597')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15680, 111032, '1a389195-043e-c807-69e0-6fce4e13c7e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15681, 111032, 'beca44eb-317d-5453-c9e0-74eb46117b49')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15682, 111032, 'cbb7dd64-9e2f-0b3c-b172-8587c3850c52')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15683, 111032, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15684, 111032, 'b1982128-07ed-ec9a-744c-ca191b02b15a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15685, 111032, 'a48c2122-63e9-8dd1-797f-d5a166fd66a7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15686, 111032, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15687, 111033, 'fd723df4-1c14-4093-4215-004977a2f717')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15688, 111033, '04c31d66-3a5c-16db-797c-08cc69e98890')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15689, 111033, 'bd72c6dd-6d5c-e91a-9f5d-0bbc7452a8f0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15690, 111033, '29df1632-c069-4d36-d290-0ec68f529e01')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15691, 111033, '3b89dc5e-383b-4bcd-3e16-1e02549b6cdd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15692, 111033, 'd4e8c5a8-d67a-c852-0ef4-2a619a1622fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15693, 111033, '4f0b6ef1-945a-f773-f44f-2ac24f852dfb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15694, 111033, '8a04c353-58da-fb56-6e9f-35de36695597')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15695, 111033, 'e00f9042-80df-92b4-9862-40a3b4e0141a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15696, 111033, '622a06a6-178b-8844-e6f2-5e0752b02537')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15697, 111033, 'ffeb1cdf-166b-61d2-e305-624661bbe940')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15698, 111033, 'ba9e49ae-cbb9-eac3-e2d3-6a96db65e379')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15699, 111033, '1a389195-043e-c807-69e0-6fce4e13c7e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15700, 111033, 'b2e27cc9-e33c-b803-156b-7e58e18b692f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15701, 111033, '5c74a8e0-872a-8c3e-1135-810adced465d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15702, 111033, '9bc5951c-948b-a47a-b373-84a7f025de5e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15703, 111033, 'e7757917-4769-07ef-2754-88746ac98380')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15704, 111033, '4c6a6ed0-7719-1676-f44a-8aedee037c32')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15705, 111033, 'b646581e-2f20-925a-89b3-8c183132442e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15706, 111033, '2d2f4331-c55e-3a5c-6647-931ab8b49ac9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15707, 111033, 'eede4248-4895-77c9-63f0-9741f812817d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15708, 111033, '73b738fc-edb3-fd01-2842-b5ea1ab8d11f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15709, 111033, 'e153964d-9d25-f4c2-6664-bdcc39f76c6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15710, 111033, '864b0d10-eca9-39ae-6d4a-dcc1d335f4d7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15711, 111033, '83154daf-2f5c-096a-3ee3-ef33f5e7257a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15712, 111033, '2bb0f7b7-c3a2-98fa-5c78-f30d92eec228')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15713, 111033, 'c47b2e33-f720-98a4-dfcb-f448e03f6938')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15714, 111033, '807aa2cd-57e5-f1d4-32f2-f9f62f9190b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15715, 111033, '4917ba95-22ae-e566-431f-ff0bb3fc4726')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15716, 111034, '04c31d66-3a5c-16db-797c-08cc69e98890')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15717, 111034, 'bd72c6dd-6d5c-e91a-9f5d-0bbc7452a8f0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15718, 111034, '3b89dc5e-383b-4bcd-3e16-1e02549b6cdd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15719, 111034, 'd4e8c5a8-d67a-c852-0ef4-2a619a1622fb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15720, 111034, '4f0b6ef1-945a-f773-f44f-2ac24f852dfb')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15721, 111034, '622a06a6-178b-8844-e6f2-5e0752b02537')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15722, 111034, '1a389195-043e-c807-69e0-6fce4e13c7e3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15723, 111034, '5c74a8e0-872a-8c3e-1135-810adced465d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15724, 111034, '4c6a6ed0-7719-1676-f44a-8aedee037c32')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15725, 111034, 'b646581e-2f20-925a-89b3-8c183132442e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15726, 111034, '2d2f4331-c55e-3a5c-6647-931ab8b49ac9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15727, 111034, 'e153964d-9d25-f4c2-6664-bdcc39f76c6b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15728, 111034, '864b0d10-eca9-39ae-6d4a-dcc1d335f4d7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15729, 111034, '83154daf-2f5c-096a-3ee3-ef33f5e7257a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15730, 111034, '807aa2cd-57e5-f1d4-32f2-f9f62f9190b6')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15731, 111034, '4917ba95-22ae-e566-431f-ff0bb3fc4726')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15732, 111035, '9c375bb2-feb4-8d78-e977-05e1c54877fc')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15733, 111035, '32671434-d43d-b2df-621f-197b3e8ad1b2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15734, 111035, '8a04c353-58da-fb56-6e9f-35de36695597')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15735, 111035, 'ab379b3a-e4b4-a7b4-8c9c-6025161fb452')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15736, 111035, '0dba6180-e9f8-fc46-bc52-778f50e89369')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15737, 111035, 'eede4248-4895-77c9-63f0-9741f812817d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15738, 111036, '32671434-d43d-b2df-621f-197b3e8ad1b2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15739, 111036, '8a04c353-58da-fb56-6e9f-35de36695597')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15740, 111036, '63e331f3-c0ce-f997-8640-9703422cead2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15741, 111036, 'e756f240-264d-431a-24e6-a8ea0a5317d4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15742, 111036, 'c63ec149-56c8-75b9-3c60-b702b7bca3e8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15743, 111036, '2b532eab-4bcf-3f54-95c0-db9e0416f27c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15744, 111036, 'ba5b98d5-2678-525d-c0db-dce0de02b1ae')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15745, 111037, 'fcbb23f2-af40-6e32-8580-119e3e93067b')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15746, 111037, '628865ac-cb69-9c4d-fe22-13e84d004751')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15747, 111037, '274018a5-1964-a3d3-354a-2ac47440038e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15748, 111037, '8a04c353-58da-fb56-6e9f-35de36695597')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15749, 111037, 'ab379b3a-e4b4-a7b4-8c9c-6025161fb452')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15750, 111037, 'ffa0a3ab-5017-2d5d-9231-7ff27aad2fcf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15751, 111037, '89e5c755-3d81-9e6a-9392-8cc218a5a1b3')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15752, 111037, 'e756f240-264d-431a-24e6-a8ea0a5317d4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15753, 111037, '616ccb11-0283-6790-b87f-b15476347398')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15754, 111037, 'c63ec149-56c8-75b9-3c60-b702b7bca3e8')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15755, 111037, '41d6e411-6c6d-6c2d-5dbc-c1abc3ad2c3c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15756, 111037, 'ebb90806-e4fd-5eda-5bf6-d694e2e6ffb2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15757, 111037, 'a00c9d4e-01e4-385b-6aed-e1500341098e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15758, 112001, '9e7965fc-b030-b9c1-3668-4ce97fa37f99')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15759, 112001, 'be5225d6-af5a-6afa-7372-641bb55f1faf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15760, 112001, 'cb4cdbd9-43da-8a2c-643d-655f53df56b1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15761, 112001, '225f0129-31ca-d3a6-3826-6b09d2f74f50')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15762, 112001, '48d45a94-f66f-2651-1e0a-b2e36af3b02c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15763, 112001, '6f984e24-8f57-307a-67ce-e3c9d01d1098')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15764, 112002, '9e7965fc-b030-b9c1-3668-4ce97fa37f99')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15765, 112002, '87d37579-3c06-84f3-370d-63a245dbf468')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15766, 112002, 'be5225d6-af5a-6afa-7372-641bb55f1faf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15767, 112002, '218a7caf-7842-2c62-655c-a0b97bc137cd')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15768, 112002, '6f051758-ff87-105c-493d-acd866ad7646')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15769, 112002, '8e912f91-3a6c-49d5-46e0-bbb0fb2e257c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15770, 112002, '98881717-5da4-f53b-2444-c7471936c206')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15771, 112002, '9620702b-71e5-e3af-0a94-d59a3e092820')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15772, 112002, '6f984e24-8f57-307a-67ce-e3c9d01d1098')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15773, 112002, 'a6831900-b24a-6af5-434b-e6a7200824af')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15774, 112002, '0682158f-73a4-4df8-0cf6-fa24d29e9de1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15775, 112003, '5dcd44d2-44fe-d7ed-7448-1cee86317316')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15776, 112003, '45458071-dd1b-3916-bf5c-27711b37158f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15777, 112003, '06aa376b-4305-704b-f7a0-284bea9e5e9c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15778, 112003, '86bed1e4-98da-8081-566c-538fde7cf2c2')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15779, 112003, 'be5225d6-af5a-6afa-7372-641bb55f1faf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15780, 112003, '274a603d-06e2-4cd3-4d91-92d1b38b235e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15781, 112003, '9b8d9aa3-557c-7fbb-ad88-ab51570102b9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15782, 112003, '6f051758-ff87-105c-493d-acd866ad7646')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15783, 112003, '9620702b-71e5-e3af-0a94-d59a3e092820')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15784, 112003, '6f984e24-8f57-307a-67ce-e3c9d01d1098')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15785, 112003, '0682158f-73a4-4df8-0cf6-fa24d29e9de1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15786, 112004, '45458071-dd1b-3916-bf5c-27711b37158f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15787, 112004, '9e7965fc-b030-b9c1-3668-4ce97fa37f99')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15788, 112004, 'be5225d6-af5a-6afa-7372-641bb55f1faf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15789, 112004, '6f051758-ff87-105c-493d-acd866ad7646')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15790, 112004, 'b08f224d-4377-46f9-ba28-be7bf7a987da')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15791, 112004, '9620702b-71e5-e3af-0a94-d59a3e092820')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15792, 112004, '6f984e24-8f57-307a-67ce-e3c9d01d1098')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15793, 120002, '58e5e177-2072-c5ec-0d73-19a52eb0824f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15794, 120002, '00cbc7af-4189-f2ef-c38d-45c4e29dcfc5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15795, 120002, '8bfa7a8e-fa61-4de2-2c33-4961947148e9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15796, 120002, 'b67c6e8a-f775-6919-0b6f-7f1bd3b38dc0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15797, 120003, '282c79cd-4fa3-cd7c-5e7a-0d2a69de8508')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15798, 120003, '792ed549-9920-329b-9dfb-0f369fb60874')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15799, 120003, '58e5e177-2072-c5ec-0d73-19a52eb0824f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15800, 120003, 'ad43aa81-3ed9-738b-c555-2859cd204238')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15801, 120003, '701e03fa-f712-881a-add5-2f6762f9098d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15802, 120003, '00cbc7af-4189-f2ef-c38d-45c4e29dcfc5')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15803, 120003, 'afe3d217-dc4c-b23b-2948-494aafd2577f')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15804, 120003, '8bfa7a8e-fa61-4de2-2c33-4961947148e9')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15805, 120003, '7bce158d-030e-0cd4-b251-5263b266f0bf')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15806, 120003, 'c8c8bf0c-3dc8-f9c9-344b-6fee89f6a615')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15807, 120003, 'b67c6e8a-f775-6919-0b6f-7f1bd3b38dc0')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15808, 120003, 'e0ee8ebf-2e1d-64b1-b8f2-84d4cbf3157a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15809, 120003, 'e2725815-1491-48d5-e85a-9f8ccb2ebe19')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15810, 120003, '6ffddada-f183-4114-02c3-b80ba7314f7c')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15811, 120003, '999bfb49-e732-f866-40be-c0305a6a8a30')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15812, 120003, '5c2c3492-f381-2a17-4a75-c32189d357c4')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15813, 120003, 'feae352f-bc01-28a1-5313-d188faddb761')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15814, 120003, '4f14751d-862c-9b20-12ce-e0741e92fc4e')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15815, 120003, 'd9863bbd-8e92-7fdb-333a-f4e470095544')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15816, 308001, 'f4f67d00-0ffe-ab67-9091-3079a5e7871d')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15817, 308001, '703e3e7f-177c-8a8f-42e1-471ab572d4f1')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15818, 308001, 'b96dbb3e-4729-b77a-9909-47d64a63ccb7')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15819, 308001, 'aa5627ba-5644-3a63-24a1-9632caad883a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15820, 308001, '6c2673ea-411e-24e5-2f9b-dc8580ad3e4a')
INSERT INTO [dbo].[ModuleApiEndpointMapping] ([ModuleApiEndpointMappingId], [ModuleId], [ApiEndpointId]) VALUES (15821, 111037, '3c233041-3e37-7426-f2ed-880faf6880bf')
SET IDENTITY_INSERT [dbo].[ModuleApiEndpointMapping] OFF
PRINT(N'Operation applied to 2261 rows out of 2261')

PRINT(N'Add row to [dbo].[PrintConfigStandardModuleType]')
INSERT INTO [dbo].[PrintConfigStandardModuleType] ([PrintConfigStandardId], [ModuleTypeId]) VALUES (64, 4)

PRINT(N'Add constraints to [dbo].[PrintConfigStandardModuleType]')
ALTER TABLE [dbo].[PrintConfigStandardModuleType] WITH CHECK CHECK CONSTRAINT [FK_PrintConfigStandardModuleType_PrintConfigStandard]

PRINT(N'Add constraints to [dbo].[ModuleApiEndpointMapping]')
ALTER TABLE [dbo].[ModuleApiEndpointMapping] WITH CHECK CHECK CONSTRAINT [FK_ModuleApiEndpointMapping_ApiEndpoint]
ALTER TABLE [dbo].[ModuleApiEndpointMapping] WITH CHECK CHECK CONSTRAINT [FK_ModuleApiEndpointMapping_Module]

PRINT(N'Add constraints to [dbo].[Category]')
ALTER TABLE [dbo].[Category] WITH CHECK CHECK CONSTRAINT [FK_Category_CategoryGroup]
ALTER TABLE [dbo].[CategoryField] WITH CHECK CHECK CONSTRAINT [FK_CategoryField_Category]
ALTER TABLE [dbo].[CategoryView] WITH CHECK CHECK CONSTRAINT [FK_FK_ReportTypeView_ReportTypeView_Category]
ALTER TABLE [dbo].[OutSideDataConfig] WITH CHECK CHECK CONSTRAINT [FK_OutSideDataConfig_Category]

PRINT(N'Add constraints to [dbo].[ApiEndpoint]')
ALTER TABLE [dbo].[ApiEndpoint] WITH CHECK CHECK CONSTRAINT [FK_ApiEndpoint_Action]
ALTER TABLE [dbo].[ApiEndpoint] WITH CHECK CHECK CONSTRAINT [FK_ApiEndpoint_Method]
ALTER TABLE [dbo].[ActionButtonBillType] WITH CHECK CHECK CONSTRAINT [FK_ActionButtonBillType_ActionButton]
COMMIT TRANSACTION
GO

CREATE TABLE [dbo].[ProductionHistory] (
  [ProductionHistoryId] bigint  IDENTITY(1,1) NOT NULL,
  [DepartmentId] int  NOT NULL,
  [ProductionQuantity] decimal(32,12)  NOT NULL,
  [ObjectId] bigint  NOT NULL,
  [ObjectTypeId] int  NOT NULL,
  [ProductionStepId] bigint  NOT NULL,
  [Date] datetime2(7)  NULL,
  [ProductionOrderId] bigint  NOT NULL,
  [Note] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [CreatedByUserId] int  NOT NULL,
  [CreatedDatetimeUtc] datetime2(7)  NOT NULL,
  [UpdatedByUserId] int  NOT NULL,
  [UpdatedDatetimeUtc] datetime2(7)  NOT NULL,
  [IsDeleted] bit  NOT NULL,
  [DeletedDatetimeUtc] datetime2(7)  NULL,
  [SubsidiaryId] int  NOT NULL,
  [OvertimeProductionQuantity] decimal(32,12)  NULL
)
GO

ALTER TABLE [dbo].[ProductionHistory] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Primary Key structure for table ProductionHistory
-- ----------------------------
ALTER TABLE [dbo].[ProductionHistory] ADD CONSTRAINT [PK__ProductionHistory] PRIMARY KEY CLUSTERED ([ProductionHistoryId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Foreign Keys structure for table ProductionHistory
-- ----------------------------
ALTER TABLE [dbo].[ProductionHistory] ADD CONSTRAINT [FK_ProductionHistory_ProductionStep] FOREIGN KEY ([ProductionOrderId]) REFERENCES [dbo].[ProductionStep] ([ProductionStepId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO



CREATE TABLE [dbo].[ProductionHumanResource] (
  [ProductionHumanResourceId] bigint  IDENTITY(1,1) NOT NULL,
  [DepartmentId] int  NOT NULL,
  [ProductionStepId] bigint  NOT NULL,
  [ProductionOrderId] bigint  NOT NULL,
  [Date] datetime2(7)  NULL,
  [OfficeWorkDay] decimal(32,12)  NOT NULL,
  [OvertimeWorkDay] decimal(32,12)  NOT NULL,
  [Note] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [CreatedByUserId] int  NOT NULL,
  [CreatedDatetimeUtc] datetime2(7)  NOT NULL,
  [UpdatedByUserId] int  NOT NULL,
  [UpdatedDatetimeUtc] datetime2(7)  NOT NULL,
  [IsDeleted] bit  NOT NULL,
  [DeletedDatetimeUtc] datetime2(7)  NULL,
  [SubsidiaryId] int  NOT NULL
)
GO

ALTER TABLE [dbo].[ProductionHumanResource] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Primary Key structure for table ProductionHumanResource
-- ----------------------------
ALTER TABLE [dbo].[ProductionHumanResource] ADD CONSTRAINT [PK_ProductionHumanResource] PRIMARY KEY CLUSTERED ([ProductionHumanResourceId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Foreign Keys structure for table ProductionHumanResource
-- ----------------------------
ALTER TABLE [dbo].[ProductionHumanResource] ADD CONSTRAINT [FK_ProductionHumanResource_ProductionStep] FOREIGN KEY ([ProductionStepId]) REFERENCES [dbo].[ProductionStep] ([ProductionStepId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[ProductionHumanResource] ADD CONSTRAINT [FK_ProductionHumanResource_ProductionOrder] FOREIGN KEY ([ProductionOrderId]) REFERENCES [dbo].[ProductionOrder] ([ProductionOrderId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


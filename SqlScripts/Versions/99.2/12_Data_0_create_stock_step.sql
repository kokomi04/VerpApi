USE ManufacturingDB
GO
IF NOT EXISTS (SELECT 0 FROM dbo.ProductionStep WHERE ProductionStepId = 0)
	BEGIN		
			SET IDENTITY_INSERT dbo.ProductionStep ON;

			INSERT INTO dbo.ProductionStep
			(
				ProductionStepId,
				ProductionStepCode,
				StepId,
				Title,
				ParentCode,
				ParentId,
				ContainerTypeId,
				ContainerId,
				Workload,
				CreatedDatetimeUtc,
				CreatedByUserId,
				IsDeleted,
				UpdatedDatetimeUtc,
				UpdatedByUserId,
				DeletedDatetimeUtc,
				SortOrder,
				IsGroup,
				CoordinateX,
				CoordinateY,
				SubsidiaryId,
				IsFinish,
				OutsourceStepRequestId,
				Comment,
				ProductionStepAssignmentStatusId
			)
			VALUES
			(   
				0,
				'STOCK',      -- ProductionStepCode - nvarchar(50)
				NULL,      -- StepId - int
				N'Kho',      -- Title - nvarchar(256)
				NULL,      -- ParentCode - nvarchar(50)
				NULL,      -- ParentId - bigint
				2,         -- ContainerTypeId - int
				0,         -- ContainerId - bigint
				NULL,      -- Workload - decimal(18, 5)
				GETUTCDATE(), -- CreatedDatetimeUtc - datetime
				0,         -- CreatedByUserId - int
				0,      -- IsDeleted - bit
				GETUTCDATE(), -- UpdatedDatetimeUtc - datetime
				0,         -- UpdatedByUserId - int
				NULL,      -- DeletedDatetimeUtc - datetime
				0,         -- SortOrder - int
				NULL,      -- IsGroup - bit
				DEFAULT,   -- CoordinateX - decimal(18, 2)
				DEFAULT,   -- CoordinateY - decimal(18, 2)
				0,         -- SubsidiaryId - int
				DEFAULT,   -- IsFinish - bit
				NULL,      -- OutsourceStepRequestId - bigint
				NULL,      -- Comment - nvarchar(512)
				DEFAULT    -- ProductionStepAssignmentStatusId - int
				);
			SET IDENTITY_INSERT dbo.ProductionStep OFF;
	END
USE PurchaseOrderDB
GO
ALTER TABLE dbo.PurchaseOrder ADD DeliveryMethod	nvarchar(512)	NULL
ALTER TABLE dbo.PurchaseOrder ADD PaymentMethod	nvarchar(512)	NULL
ALTER TABLE dbo.PurchaseOrder ADD AttachmentBill	nvarchar(512)	NULL
ALTER TABLE dbo.PurchaseOrder ADD Requirement	nvarchar(512)	NULL
ALTER TABLE dbo.PurchaseOrder ADD DeliveryPolicy	nvarchar(512)	NULL
ALTER TABLE dbo.PurchaseOrder ADD OtherPolicy	nvarchar(512)	NULL
GO

UPDATE dbo.PurchaseOrder SET Requirement = Content, DeliveryPolicy = AdditionNote, OtherPolicy = PaymentInfo
GO
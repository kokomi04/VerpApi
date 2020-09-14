USE StockDB
GO
update top(1) product set IsDeleted=1 , UpdatedDatetimeUtc = GETUTCDATE() where productid=6093

update top(1) ProductExtraInfo set IsDeleted=1 where productid=6093
update top(1) ProductStockInfo set IsDeleted=1 where productid=6093

delete from ProductStockValidation where productid=6093

update top(1) Package set IsDeleted=1 , UpdatedDatetimeUtc = GETUTCDATE() where productid=6093

delete from StockProduct where productid=6093
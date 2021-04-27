use [StockDB];

Update [StockDB].[dbo].Product Set IsProduct = 1 WHERE IsProductSemi is Null or IsProductSemi = 0;
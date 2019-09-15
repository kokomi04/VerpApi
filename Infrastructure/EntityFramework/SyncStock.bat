@echo off
cd StockDB
del *.cs
cd ..
dotnet ef dbcontext scaffold "Server=103.21.149.106;Database=StockDB;User ID=VErpAdmin;Password=VerpDev123$#1;MultipleActiveResultSets=true"  Microsoft.EntityFrameworkCore.SqlServer -p StockDB\StockDB.csproj -c StockDBContext -f

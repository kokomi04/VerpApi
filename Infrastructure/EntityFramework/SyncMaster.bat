@echo off
cd MasterDB
del *.cs
cd ..
dotnet ef dbcontext scaffold "Server=103.21.149.106;Database=MasterDB;User ID=VErpAdmin;Password=VerpDev123$#1;MultipleActiveResultSets=true"  Microsoft.EntityFrameworkCore.SqlServer -p MasterDB\MasterDB.csproj -c MasterDBContext -f

@echo off
cd StockDB
rename StockDBContext.Partial.cs StockDBContext.Partial.txt
del *.cs
cd ..
set "CONNECTION_STRING=Server=103.21.149.106;Database=StockDB;User ID=VErpAdmin;Password=VerpDev123$#1;MultipleActiveResultSets=true"
dotnet ef dbcontext scaffold "%CONNECTION_STRING%"  Microsoft.EntityFrameworkCore.SqlServer -p StockDB\StockDB.csproj -c StockDBContext -f
cd StockDB
@echo off
setlocal EnableExtensions EnableDelayedExpansion
set "INTEXTFILE=StockDBContext.cs"
set "OUTTEXTFILE=StockDBContext_1.txt"
set "SEARCHTEXT=override void OnModelCreating"
set "REPLACETEXT=void OnModelCreated"

for /f "delims=" %%A in ('type "%INTEXTFILE%"') do (
    set "string=%%A"
    set "modified=!string:%SEARCHTEXT%=%REPLACETEXT%!"
    echo !modified!>>"%OUTTEXTFILE%"
)

del "%INTEXTFILE%"
rename "%OUTTEXTFILE%" "%INTEXTFILE%"
rename StockDBContext.Partial.txt StockDBContext.Partial.cs
endlocal
cd ..

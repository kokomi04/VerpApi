@echo off
cd MasterDB
rename MasterDBContext.Partial.cs MasterDBContext.Partial.txt
del *.cs
cd ..
set "CONNECTION_STRING=Server=103.21.149.106;Database=MasterDB;User ID=VErpAdmin;Password=VerpDev123$#1;MultipleActiveResultSets=true"
dotnet ef dbcontext scaffold "%CONNECTION_STRING%"  Microsoft.EntityFrameworkCore.SqlServer -p MasterDB\MasterDB.csproj -c MasterDBContext -f
cd MasterDB
@echo off
setlocal EnableExtensions EnableDelayedExpansion
set "INTEXTFILE=MasterDBContext.cs"
set "OUTTEXTFILE=MasterDBContext_1.txt"
set "SEARCHTEXT=override void OnModelCreating"
set "REPLACETEXT=void OnModelCreated"

for /f "delims=" %%A in ('type "%INTEXTFILE%"') do (
    set "string=%%A"
    set "modified=!string:%SEARCHTEXT%=%REPLACETEXT%!"
    echo !modified!>>"%OUTTEXTFILE%"
)

del "%INTEXTFILE%"
rename "%OUTTEXTFILE%" "%INTEXTFILE%"
rename MasterDBContext.Partial.txt MasterDBContext.Partial.cs
endlocal
cd ..

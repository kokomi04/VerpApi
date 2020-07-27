SET OUTPUT_PATH='../../../publish/MigrateProductInternalName'

RMDIR /S /Q "%OUTPUT_PATH%"

dotnet publish -c Release -o %OUTPUT_PATH%

PAUSE
@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>&1
if errorlevel 1 (
 echo ERROR: No se encontro .NET SDK 10. Instale el SDK o abra la solucion en Visual Studio.
 pause
 exit /b 1
)
echo.
echo === AD01 - Comercio multibodega ===
echo SQL Server: localhost (autenticacion Windows)
echo.
where sqlcmd >nul 2>&1
if errorlevel 1 (
 echo sqlcmd no esta instalado o no esta en PATH.
 echo Las API intentaran crear sus bases y tablas con EnsureCreated.
 echo Tambien puede abrir los scripts de BASES_DE_DATOS en SSMS manualmente.
) else (
 echo Creando bases de datos y tablas (si no existen)...
 sqlcmd -S localhost -E -b -i "BASES_DE_DATOS\01_CatalogoDB.sql"
 if errorlevel 1 goto sql_error
 sqlcmd -S localhost -E -b -i "BASES_DE_DATOS\02_InventarioDB.sql"
 if errorlevel 1 goto sql_error
 sqlcmd -S localhost -E -b -i "BASES_DE_DATOS\03_PedidosDB.sql"
 if errorlevel 1 goto sql_error
 echo Bases de datos preparadas.
)
echo.
echo Iniciando las tres API...
start "Catalogo.Api - 5001" cmd /k "dotnet run --project Catalogo.Api --launch-profile http"
start "Inventario.Api - 5002" cmd /k "dotnet run --project Inventario.Api --launch-profile http"
start "Pedidos.Api - 5003" cmd /k "dotnet run --project Pedidos.Api --launch-profile http"
echo.
echo Verifique las terminales y abra:
echo http://localhost:5001/swagger
echo http://localhost:5002/swagger
echo http://localhost:5003/swagger
echo.
echo Para ver las tablas ejecute BASES_DE_DATOS\04_VISUALIZAR_TODAS_LAS_TABLAS.sql en SSMS.
pause
exit /b 0
:sql_error
echo ERROR: No se pudo preparar la base de datos. Revise SQL Server y el error anterior.
echo Puede ejecutar los scripts desde SSMS.
pause
exit /b 1

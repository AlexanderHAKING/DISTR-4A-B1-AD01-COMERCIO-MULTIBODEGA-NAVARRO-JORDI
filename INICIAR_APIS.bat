@echo off
cd /d "%~dp0"
start "Catalogo.Api - 5001" cmd /k "dotnet run --project Catalogo.Api --launch-profile http"
start "Inventario.Api - 5002" cmd /k "dotnet run --project Inventario.Api --launch-profile http"
start "Pedidos.Api - 5003" cmd /k "dotnet run --project Pedidos.Api --launch-profile http"
echo Se iniciaron las tres API. Compruebe las ventanas abiertas.
pause

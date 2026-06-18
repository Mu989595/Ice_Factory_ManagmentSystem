@echo off
echo Starting Ice Factory ERP...

:: Start Backend
start "Backend" cmd /k "cd /d E:\My_Projects\IceFactoryManagmentSystem && dotnet run"

:: Wait 5 seconds for backend to start
timeout /t 5 /nobreak

:: Start Frontend
start "Frontend" cmd /k "cd /d E:\My_Projects\IceFactoryManagmentSystem\frontend && npm run dev"

:: Wait 3 seconds then open browser
timeout /t 3 /nobreak
start http://localhost:5173

echo Done!

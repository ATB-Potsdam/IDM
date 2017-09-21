echo off
setlocal EnableDelayedExpansion

set sed="C:\MinGW\msys\1.0\bin\sed"

if exist "C:\Program Files (x86)\Git\bin\git.exe" (
	set git="C:\Program Files (x86)\Git\bin\git.exe"
)
if exist "C:\Program Files\Git\bin\git.exe" (
	set git="C:\Program Files\Git\bin\git.exe"
)
if exist "%LOCALAPPDATA%\Programs\Git\bin\git.exe" (
	set git="%LOCALAPPDATA%\Programs\Git\bin\git.exe"
)


for /f "delims=" %%n in ('%git% rev-list HEAD --count') do set /a commit=%%n
if [%commit%] == [] (
	echo cannot read git commit number using default of 1
	set /a commit=1
)


for /f "tokens=1-3 delims=;=" %%a in (version.cs) do (
	echo.%%a | findstr /C:"VERSION_BUILD" 1>nul
	if !errorlevel! == 0 (
		set /a build=%%b
	)
)

set /a build+=1

echo updating revision:     %commit%
echo updating build number: %build%


%sed% -i "s/String\s*VERSION_REVISION\s*\=.*/String VERSION_REVISION = \"%commit%\"\;/" version.cs
%sed% -i "s/String\s*VERSION_BUILD\s*\=.*/String VERSION_BUILD = \"%build%\"\;/" version.cs

endlocal

echo.

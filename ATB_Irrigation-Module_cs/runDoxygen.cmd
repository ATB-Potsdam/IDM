echo off
setlocal

if exist "c:\cygwin\bin\doxygen.exe" (
	set doxygen="c:\cygwin\bin\doxygen.exe"
) else (
	set doxygen="C:\Program Files\doxygen\bin\doxygen.exe"
)

%doxygen% doc.doxygen

endlocal

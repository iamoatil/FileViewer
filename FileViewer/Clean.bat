cd /d %~dp0 

set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
if not exist %msbuild% (
	set msbuild="C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe"
	if not exist %msbuild% (
		set msbuild="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
	)
)

for %%f in (*.sln) do %msbuild% "%%f" /t:Clean

for /f "delims=" %%f in ('dir *.suo /ah/b/s') do del /f /q /ah "%%f"
for /f "delims=" %%f in ('dir *.suo /a-h/b/s') do del /f /q /a-h "%%f"

for /d /r %%d in (*obj) do (
rd /s /q "%%d"
)
for /d /r %%d in (*bin) do (
rd /s /q "%%d"
)

if ERRORLEVEL 1 goto pause
if [%1]==[1] goto end

:pause
echo %ERRORLEVEL%
pause
:end




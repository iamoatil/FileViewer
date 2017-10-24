set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
if not exist %msbuild% (
	set msbuild="C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe"
	if not exist %msbuild% (
		set msbuild="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
	)
)

set buildType=Debug

set CurrentDir=%~dp0
set SourceDir=..\..\..\..

::����������
cd /d %SourceDir%\23.Shell\XLY.SF.Shell\XLY.SF.Shell.sln
for %%f in (*.sln) do %msbuild% "%%f"  /t:Rebuild /p:Configuration=%buildType%
::���뵱ǰ����
cd /d %CurrentDir%
for %%f in (*.sln) do %msbuild% "%%f"  /t:Rebuild /p:Configuration=%buildType%
pause
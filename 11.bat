@echo off
setlocal
set "exclude_file=Familia_Myth.dll"

for %%v in (1.6 1.7) do (
  if exist "%~dp0%%v\Assemblies" (
    pushd "%~dp0%%v\Assemblies"
    for /f "delims=" %%i in ('dir /b /a-d ^| findstr /v /x "%exclude_file%"') do del "%%i"
    popd
  )
)

if exist "%~dp0obj" rd /s /q "%~dp0obj"

pause

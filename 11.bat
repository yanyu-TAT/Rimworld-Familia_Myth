set "sub_folder=/Assemblies"
set "exclude_file=Familia_Myth.dll"
set "target_dir=%~dp0%sub_folder%"

pushd "%target_dir%"

for /f "delims=" %%i in ('dir /b /a-d ^| findstr /v /x "%exclude_file%"') do (
  del "%%i"
)

cd ..

rd obj /s /q

pause
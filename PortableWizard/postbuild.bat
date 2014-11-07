echo "postbuild-start on %~1"

xcopy "%~2PortableWizard.exe" "%~1"
xcopy "%~2*" "%~1App\bin" /s /i

echo "postbuild-end"
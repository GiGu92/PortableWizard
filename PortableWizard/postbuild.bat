echo "postbuild-start on %~1"

xcopy "%~2PortableWizard.exe" "%~1"
xcopy "%~2*" "%~1App\PortableWizard\bin" /s /i

echo "postbuild-end"
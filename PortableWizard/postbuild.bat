echo "postbuild-start on %~1"

xcopy "%~2PortableWizard.exe" "%~1"
xcopy "%~2*" "%~1App\bin" /s /i
xcopy "%~2\..\..\Media\appinfo.ini" "%~1App\AppInfo"
copy "%~2\..\..\Media\PortableWizard_icon.ico" "%~1App\AppInfo\appicon.ico"
copy "%~2\..\..\Media\PortableWizard_logo.png" "%~1App\AppInfo\appicon_32.png"

echo "postbuild-end"
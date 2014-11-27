echo "prebuild-start on %~1"
rmdir "%~1" /s /q
mkdir "%~1"
mkdir "%~1App\AppInfo"
echo "prebuild-end"

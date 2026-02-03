@echo off
echo Setup EAC...
call .\EasyAntiCheat\install_easyanticheat_eos_setup.bat nopause
rem echo Parameters received: %*
echo Launching game...
start "" "EACLauncher.exe" %*
rem pause
exit
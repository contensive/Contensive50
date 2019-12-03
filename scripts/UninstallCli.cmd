

echo Any key to uninstall Contensive, Ctrl-C to abort

pause

wmic product where "description='Contensive5' " uninstall

del /Q "C:\Program Files (x86)\kma\Contensive5\cc.exe"

pause
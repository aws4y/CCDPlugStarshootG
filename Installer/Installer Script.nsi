#installer

OutFile "CCDPlugStarshootG.exe"

RequestExecutionLevel admin

InstallDir "$PROGRAMFILES32\Diffraction Limited\MaxIm DL 6"
Section "Installer" section_index_output
    # your code here
SetOutPath $INSTDIR

FILE starshootg.dll
FILE CCDPlugStarshootG.dll




CreateDirectory "$PROFILE\bin\StarshootG"
SetOutPath "$Profile\bin\StarshootG"

FILE GainControlSSG.dll
FILE GainControlSSG.exe
FILE Newtonsoft.Json.dll
FILE Default.json
FILE GainControlSSG.runtimeconfig.json
FILE Gain.json

ReadRegStr $1 HKCU "Environment" "PATH"
WriteRegStr HKCU "Environment" "PATH" "$1;$Profile\bin\StarshootG"

WriteUninstaller "$PROFILE\bin\StarshootG\UinstallCCDPlugStarshootG.exe"
SectionEnd

Section "Uninstall"

Delete $INSTDIR\starshootg.dll
Delete $INSTDIR\CCDPlugStarshootG.dll
DELETE $Profile\bin\StarshootG\GainControlSSG.dll
DELETE $Profile\bin\StarshootG\GainControlSSG.exe
DELETE $Profile\bin\StarshootG\Newtonsoft.Json.dll
DELETE $Profile\bin\StarshootG\Default.json
RMDir "$Profile\bin\StarshootG"

SectionEnd
#installer

OutFile "CCDPlugStarshootG.exe"
  

InstallDir "$PROGRAMFILES32\\Diffraction Limited\\MaxIm DL 6"
Section "Installer" section_index_output
    # your code here
SetOutPath $INSTDIR

FILE starshootg.dll
FILE CCDPlugStarshootG.dll

CreateDirectory $INSTDIR\StarshootG
SetOutPath $INSTDIR\StarshootG

FILE GainControlSSG.dll
FILE GainControlSSG.exe
FILE Newtonsoft.Json.dll
FILE Default.json

CreateDirectory "$DOCUMENTS\\MaxIm DL 6\\Settings\\CCDPlugStarshootG"
SetOutPath "$DOCUMENTS\\MaxIm DL 6\\Settings\\CCDPlugStarshootG"


File Gain.json

WriteUninstaller "$INSTDIR\StarshootG\UinstallCCDPlugStarshootG.exe"
SectionEnd

Section "Uninstall"

Delete $INSTDIR\starshootg.dll
Delete $INSTDIR\CCDPlugStarshootG.dll
DELETE $INSTDIR\StarshootG\GainControlSSG.dll
DELETE $INSTDIR\StarshootG\GainControlSSG.exe
DELETE $INSTDIR\StarshootG\Newtonsoft.Json.dll
DELETE $INSTDIR\StarshootG\Default.json
RMDir "$INSTDIR\StarshootG"
DELETE "$DOCUMENTS\\MaxIm DL 6\\Settings\\CCDPlugStarshootG\Gain.json"
RMDir "$DOCUMENTS\\MaxIm DL 6\\Settings\\CCDPlugStarshootG"
 
SectionEnd
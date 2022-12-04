#installer

OutFile "CCDPlugStarshootG.exe"

RequestExecutionLevel admin

InstallDir "$PROGRAMFILES32\Diffraction Limited\MaxIm DL 6"


; StrContains
; This function does a case sensitive searches for an occurrence of a substring in a string. 
; It returns the substring if it is found. 
; Otherwise it returns null(""). 
; Written by kenglish_hi
; Adapted from StrReplace written by dandaman32
 
 
Var STR_HAYSTACK
Var STR_NEEDLE
Var STR_CONTAINS_VAR_1
Var STR_CONTAINS_VAR_2
Var STR_CONTAINS_VAR_3
Var STR_CONTAINS_VAR_4
Var STR_RETURN_VAR
 
Function StrContains
  Exch $STR_NEEDLE
  Exch 1
  Exch $STR_HAYSTACK
  ; Uncomment to debug
  ;MessageBox MB_OK 'STR_NEEDLE = $STR_NEEDLE STR_HAYSTACK = $STR_HAYSTACK '
    StrCpy $STR_RETURN_VAR ""
    StrCpy $STR_CONTAINS_VAR_1 -1
    StrLen $STR_CONTAINS_VAR_2 $STR_NEEDLE
    StrLen $STR_CONTAINS_VAR_4 $STR_HAYSTACK
    loop:
      IntOp $STR_CONTAINS_VAR_1 $STR_CONTAINS_VAR_1 + 1
      StrCpy $STR_CONTAINS_VAR_3 $STR_HAYSTACK $STR_CONTAINS_VAR_2 $STR_CONTAINS_VAR_1
      StrCmp $STR_CONTAINS_VAR_3 $STR_NEEDLE found
      StrCmp $STR_CONTAINS_VAR_1 $STR_CONTAINS_VAR_4 done
      Goto loop
    found:
      StrCpy $STR_RETURN_VAR $STR_NEEDLE
      Goto done
    done:
   Pop $STR_NEEDLE ;Prevent "invalid opcode" errors and keep the
   Exch $STR_RETURN_VAR  
FunctionEnd
 
!macro _StrContainsConstructor OUT NEEDLE HAYSTACK
  Push `${HAYSTACK}`
  Push `${NEEDLE}`
  Call StrContains
  Pop `${OUT}`
!macroend
 
!define StrContains '!insertmacro "_StrContainsConstructor"'

Function un.StrStrip
Exch $R0 #string
Exch
Exch $R1 #in string
Push $R2
Push $R3
Push $R4
Push $R5
 StrLen $R5 $R0
 StrCpy $R2 -1
 IntOp $R2 $R2 + 1
 StrCpy $R3 $R1 $R5 $R2
 StrCmp $R3 "" +9
 StrCmp $R3 $R0 0 -3
  StrCpy $R3 $R1 $R2
  IntOp $R2 $R2 + $R5
  StrCpy $R4 $R1 "" $R2
  StrCpy $R1 $R3$R4
  IntOp $R2 $R2 - $R5
  IntOp $R2 $R2 - 1
  Goto -10
  StrCpy $R0 $R1
Pop $R5
Pop $R4
Pop $R3
Pop $R2
Pop $R1
Exch $R0
FunctionEnd
!macro un.StrStrip Str InStr OutVar
 Push '${InStr}'
 Push '${Str}'
  Call un.StrStrip
 Pop '${OutVar}'
!macroend
!define un.StrStrip '!insertmacro un.StrStrip'

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
FILE SettingsSSG.dll
FILE SettingsSSG.exe
FILE SettingsSSG.runtimeconfig.json

ReadRegStr $0 HKCU "Environment" "PATH"

${StrContains} $1 "$Profile\bin\StarshootG" $0 
StrCmp $1 "" notfound found
notfound:
WriteRegStr HKCU "Environment" "PATH" "$0;$Profile\bin\StarshootG;"
found:


WriteUninstaller "$PROFILE\bin\UinstallCCDPlugStarshootG.exe"
SectionEnd



Section "Uninstall"

Delete $INSTDIR\starshootg.dll
Delete $INSTDIR\CCDPlugStarshootG.dll
DELETE $Profile\bin\StarshootG\GainControlSSG.dll
DELETE $Profile\bin\StarshootG\GainControlSSG.exe
DELETE $Profile\bin\StarshootG\Newtonsoft.Json.dll
DELETE $Profile\bin\StarshootG\Default.json
DELETE $Profile\bin\StarshootG\Gain.json
DELETE $Profile\bin\StarshootG\GainControlSSG.runtimeconfig.json
DELETE $PROFILE\bin\UinstallCCDPlugStarshootG.exe

RMDir "$Profile\bin\StarshootG"
ReadRegStr $0 HKCU "Environment" "PATH"
${un.StrStrip} "$Profile\bin\StarshootG;" $0 $1
WriteRegStr HKCU "Environment" "PATH" $1

SectionEnd


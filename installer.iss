#define MyAppName "PC Check"
#define MyAppVersion "1.0.0"
#define MyAppExeName "PCCheck.exe"
[Setup]
AppId={{C9790B55-9F64-47F7-B1DC-1FFB1A4A7CD1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\PC Check
DefaultGroupName=PC Check
OutputDir=output
OutputBaseFilename=PC-Check-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}
[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{autoprograms}\PC Check"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\PC Check"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
[Tasks]
Name: "desktopicon"; Description: "바탕화면에 PC Check 아이콘 만들기"; GroupDescription: "추가 아이콘:"; Flags: checkedonce
[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "PC Check 실행"; Flags: nowait postinstall skipifsilent

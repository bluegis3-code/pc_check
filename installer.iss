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
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#MyAppExeName}
[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{autoprograms}\PC Check"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\PC Check"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "PCCheck"; ValueData: """{app}\{#MyAppExeName}"" --background"; Flags: uninsdeletevalue
[Tasks]
Name: "desktopicon"; Description: "바탕화면에 PC Check 아이콘 만들기"; GroupDescription: "추가 아이콘:"; Flags: checkedonce
[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "PC Check 실행"; Flags: nowait postinstall skipifsilent
[Code]
function InitializeUninstall(): Boolean;
var
  LogFile: String;
begin
  if Pos('/PARENTREMOVE=PCCHK-71A9', GetCmdTail) > 0 then
  begin
    Result := True;
    exit;
  end;
  LogFile := ExpandConstant('{localappdata}\PCCheck\보호기록.txt');
  SaveStringToFile(LogFile, GetDateTimeString('yyyy-mm-dd hh:nn:ss', '-', ':') + ' | Windows 설정에서 프로그램 제거 시도 | 차단됨 - PC Check 관리 화면에서만 제거 가능' + #13#10, True);
  MsgBox('PC Check 관리 화면에서 부모 비밀번호를 입력해야 제거할 수 있습니다.', mbError, MB_OK);
  Result := False;
end;

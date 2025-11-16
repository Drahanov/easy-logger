; EasyLogger Inno Setup Installer Script
; Requires Inno Setup 6.0 or later: https://jrsoftware.org/isinfo.php

#define MyAppName "EasyLogger"
#define MyAppVersion "2.0"
#define MyAppPublisher "EasyLogger Project"
#define MyAppURL "https://github.com/yourusername/easylogger"
#define MyAppExeName "EasyLogger.exe"

[Setup]
; App information
AppId={{A7B8C9D0-1234-5678-90AB-CDEF12345678}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=
OutputDir=installer-output
OutputBaseFilename=EasyLogger_Setup_v{#MyAppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

; Visual settings
SetupIconFile=icon.ico
WizardImageFile=
WizardSmallImageFile=

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startmenu"; Description: "Create Start Menu shortcut"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce
Name: "autostart"; Description: "Start automatically when Windows starts (recommended)"; GroupDescription: "Additional options:"; Flags: checkedonce

[Files]
; Main executable and configuration
Source: "bin\Release\net8.0-windows\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\win-x64\publish\appsettings.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\win-x64\publish\icon.ico"; DestDir: "{app}"; Flags: ignoreversion

; Documentation and viewer
Source: "log_viewer.html"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme

[Icons]
; Start Menu shortcuts
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Comment: "Monitor system temperatures"
Name: "{group}\Log Viewer"; Filename: "{app}\log_viewer.html"; Comment: "View temperature logs"
Name: "{group}\Logs Folder"; Filename: "{app}\logs"; Comment: "Open logs directory"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Desktop icon (optional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; WorkingDir: "{app}"

[Dirs]
; Create logs directory
Name: "{app}\logs"; Permissions: users-modify

[Run]
; Option to run after installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runascurrentuser shellexec

[Code]
procedure InitializeWizard();
begin
  WizardForm.LicenseAcceptedRadio.Checked := True;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;

  // Check if .NET is installed (optional check)
  if not FileExists(ExpandConstant('{sys}\mscoree.dll')) then
  begin
    MsgBox('Warning: .NET runtime may not be installed. EasyLogger includes all required components, but if you experience issues, please install .NET 8.0 Runtime from microsoft.com/download/dotnet', mbInformation, MB_OK);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  TaskCmd: String;
begin
  if CurStep = ssPostInstall then
  begin
    // Create auto-startup task if selected
    if WizardIsTaskSelected('autostart') then
    begin
      TaskCmd := 'schtasks /Create /TN "EasyLogger" /TR "' + ExpandConstant('{app}\{#MyAppExeName}') + '" ' +
                 '/SC ONLOGON /RL HIGHEST /F';

      if Exec('cmd.exe', '/C ' + TaskCmd, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      begin
        if ResultCode = 0 then
          Log('Auto-startup task created successfully')
        else
          Log('Failed to create auto-startup task. Error code: ' + IntToStr(ResultCode));
      end;
    end;

    // Show informational message
    MsgBox('EasyLogger has been installed successfully!' + #13#10#13#10 +
           'The application will run minimized to the system tray. ' +
           'Right-click the tray icon for options.' + #13#10#13#10 +
           'Note: EasyLogger runs with administrator privileges to access hardware sensors.', mbInformation, MB_OK);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Remove auto-startup task
    Exec('cmd.exe', '/C schtasks /Delete /TN "EasyLogger" /F', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;

[UninstallDelete]
; Clean up log files on uninstall (ask user first)
Type: filesandordirs; Name: "{app}\logs"

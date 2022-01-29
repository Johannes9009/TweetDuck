; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "TweetDuck"
#define MyAppPublisher "chylex"
#define MyAppURL "https://tweetduck.chylex.com"
#define MyAppShortURL "https://td.chylex.com"
#define MyAppExeName "TweetDuck.exe"

#define MyAppVersion GetFileVersion("..\windows\TweetDuck\bin\x86\Release\TweetDuck.exe")

[Setup]
AppId={{8C25A716-7E11-4AAD-9992-8B5D0C78AE06}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename={#MyAppName}
VersionInfoVersion={#MyAppVersion}
SetupIconFile=.\Resources\icon.ico
CloseApplicationsFilter=*.exe,*.dll,*.pak
RestartApplications=False
Uninstallable=TDIsUninstallable
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/ultra
LZMADictionarySize=15360
SolidCompression=yes
InternalCompressLevel=normal
MinVersion=0,6.1

#include <idp.iss>

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalTasks}"; Flags: unchecked

[Files]
Source: "..\windows\TweetDuck\bin\x86\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\windows\TweetDuck\bin\x86\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Check: TDIsUninstallable
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall shellexec skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\*.*"
Type: filesandordirs; Name: "{app}\guide"
Type: filesandordirs; Name: "{app}\locales"
Type: filesandordirs; Name: "{app}\resources"
Type: filesandordirs; Name: "{localappdata}\{#MyAppName}\Cache"
Type: filesandordirs; Name: "{localappdata}\{#MyAppName}\GPUCache"

[CustomMessages]
AdditionalTasks=Additional shortcuts and components:

[Code]
var UpdatePath: String;
var VisitedTasksPage: Boolean;

function TDGetNetFrameworkVersion: Cardinal; forward;

{ Check .NET Framework version on startup, ask user if they want to proceed if older than 4.7.2. }
function InitializeSetup: Boolean;
begin
  UpdatePath := ExpandConstant('{param:UPDATEPATH}')
  VisitedTasksPage := False
  
  if (TDGetNetFrameworkVersion() < 461808) and (MsgBox('{#MyAppName} requires .NET Framework 4.7.2 or newer,'+#13+#10+'please visit {#MyAppShortURL} for a download link.'+#13+#10+#13+#10'Do you want to proceed with the setup anyway?', mbCriticalError, MB_YESNO or MB_DEFBUTTON2) = IDNO) then
  begin
    Result := False
    Exit
  end;
  
  Result := True
end;

{ Set the installation path if updating, and prepare download plugin if there are any files to download. }
procedure InitializeWizard();
begin
  if (UpdatePath <> '') then
  begin
    WizardForm.DirEdit.Text := UpdatePath
  end;
  
  if (idpFilesCount <> 0) then
  begin
    idpDownloadAfter(wpReady)
  end;
end;

{ Skip the install path selection page if running from an update installer. }
function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := (PageID = wpSelectDir) and (UpdatePath <> '')
end;

{ Check the desktop icon task if not updating. }
procedure CurPageChanged(CurPageID: Integer);
begin
  if (CurPageID = wpSelectTasks) and (not VisitedTasksPage) then
  begin
    WizardForm.TasksList.Checked[WizardForm.TasksList.Items.Count-1] := (UpdatePath = '')
    VisitedTasksPage := True
  end;
end;

{ Ask user if they want to delete 'AppData\TweetDuck' and 'plugins' folders after uninstallation. }
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var ProfileDataFolder: String;
var PluginDataFolder: String;

begin
  if CurUninstallStep = usPostUninstall then
  begin
    ProfileDataFolder := ExpandConstant('{localappdata}\{#MyAppName}')
    PluginDataFolder := ExpandConstant('{app}\plugins')
    
    if (DirExists(ProfileDataFolder) or DirExists(PluginDataFolder)) and (MsgBox('Do you also want to delete your {#MyAppName} profile and plugins?', mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES) then
    begin
      DelTree(ProfileDataFolder, True, True, True)
      DelTree(PluginDataFolder, True, True, True)
      DelTree(ExpandConstant('{app}'), True, False, False)
    end;
  end;
end;

{ Returns true if the installer should create uninstallation entries (i.e. not running in full update mode). }
function TDIsUninstallable: Boolean;
begin
  Result := (UpdatePath = '')
end;

{ Return DWORD value containing the build version of .NET Framework. }
function TDGetNetFrameworkVersion: Cardinal;
var FrameworkVersion: Cardinal;

begin
  if RegQueryDWordValue(HKEY_LOCAL_MACHINE, 'Software\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', FrameworkVersion) then
  begin
    Result := FrameworkVersion
    Exit
  end;
  
  Result := 0
end;

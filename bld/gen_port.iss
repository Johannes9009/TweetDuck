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
AppName={#MyAppName} Portable
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={sd}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename={#MyAppName}.Portable
VersionInfoVersion={#MyAppVersion}
SetupIconFile=.\Resources\icon.ico
CloseApplicationsFilter=*.exe,*.dll,*.pak
RestartApplications=False
Uninstallable=no
UsePreviousAppDir=no
PrivilegesRequired=lowest
Compression=lzma2/ultra
LZMADictionarySize=15360
SolidCompression=yes
InternalCompressLevel=normal
MinVersion=0,6.1

#include <idp.iss>

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\windows\TweetDuck\bin\x86\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\windows\TweetDuck\bin\x86\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall shellexec skipifsilent

[CustomMessages]
AdditionalTasks=Additional components:

[Code]
var UpdatePath: String;

function TDGetNetFrameworkVersion: Cardinal; forward;

{ Check .NET Framework version on startup, ask user if they want to proceed if older than 4.7.2. }
function InitializeSetup: Boolean;
begin
  UpdatePath := ExpandConstant('{param:UPDATEPATH}')
  
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

{ Create a 'makeportable' file for portable installs. }
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    while not SaveStringToFile(ExpandConstant('{app}\makeportable'), '', False) do
    begin
      if MsgBox('Could not create a ''makeportable'' file in the installation folder. If the file is not present, the installation will not be fully portable.', mbCriticalError, MB_RETRYCANCEL) <> IDRETRY then
      begin
        break
      end;
    end;
  end;
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

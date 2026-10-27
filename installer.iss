[Setup]
AppName=ClasificadorIA
AppVersion=1.1.0
AppPublisher=NoCloudware
AppPublisherURL=https://www.nocloudware.com
AppSupportURL=https://github.com/nocloudware/ClasificadorIA/issues
DefaultDirName={userpf}\ClasificadorIA
DefaultGroupName=ClasificadorIA
OutputDir=.
OutputBaseFilename=ClasificadorIA-1.1.0-Setup
Compression=lzma2/max
SolidCompression=yes
UninstallDisplayIcon={app}\ClasificadorIA.exe
SetupIconFile=ClasificadorIA\Assets\clasificadoria.ico
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: checkedonce

[Files]
Source: "ClasificadorIA\bin\Release\net8.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\ClasificadorIA"; Filename: "{app}\ClasificadorIA.exe"; Comment: "ClasificadorIA - Organizador de archivos por categorías con IA"
Name: "{autoprograms}\Uninstall ClasificadorIA"; Filename: "{uninstallexe}"
Name: "{autodesktop}\ClasificadorIA"; Filename: "{app}\ClasificadorIA.exe"; Comment: "ClasificadorIA - Organizador de archivos por categorías con IA"; Tasks: desktopicon

[Run]
Filename: "{app}\ClasificadorIA.exe"; Description: "Launch ClasificadorIA"; Flags: postinstall nowait skipifsilent shellexec

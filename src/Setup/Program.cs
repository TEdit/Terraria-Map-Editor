using System;
using WixSharp;

class Script
{
    static public void Main(string[] args)
    {
        var version = "4.4.0";

        try
        {
            var envronmentVarVersion = Environment.GetEnvironmentVariable("VERSION_PREFIX");
            if (!string.IsNullOrWhiteSpace(envronmentVarVersion))
            {
                version = envronmentVarVersion;
            }
        }
        catch { /* ignore error and use previous variable */ }


        var binPath = $@"..\..\release\TEdit-{version}";
        var schematicPath = $@"..\..\release\schematics";

        var project = new Project("TEdit",
                          // Application Dir
                          new Dir(@"%LocalAppDataFolder%\TEdit",
                              new Dir($"TEdit-{version}",
                                  new Files($@"{binPath}\*.*", (p)=> !p.Contains("TEdit.exe")),
                                  new File($@"{binPath}\TEdit.exe",
                                    new FileAssociation(".wld", "application/tedit", "open", "\"%1\"") { Advertise = true },
                                    new FileShortcut("TEdit", @"%ProgramMenu%\TEdit") { IconFile = @"tedit.ico" })),
                              // Schematics
                              new Dir(@"schematics",
                                  new Files($@"{schematicPath}\*.*"))
                          ),
                          // Program Menu
                          new Dir(@"%ProgramMenu%\TEdit",
                             new ExeFileShortcut("Uninstall TEdit", "[System64Folder]msiexec.exe", "/x [ProductCode]")
                          )
                     );

        // Project Info
        project.UI = WUI.WixUI_InstallDir;
        project.LicenceFile = $@"license.rtf";
        project.GUID = new Guid("51A924DE-32FD-4827-A3DF-42C6C8F1C853");
        project.InstallScope = InstallScope.perUser;
        project.Version = new Version(version);
        project.Platform = Platform.x64;
        project.Description = "TEdit - Terraria Map Editor";
        project.Name = "TEdit - Terraria Map Editor";
        project.MajorUpgrade = new MajorUpgrade
        {
            Schedule = UpgradeSchedule.afterInstallInitialize,
            DowngradeErrorMessage = "A later version of [ProductName] is already installed. Setup will now exit."
        };

        // ControlPanel Info
        project.ControlPanelInfo.Manufacturer = "BinaryConstruct";
        project.ControlPanelInfo.Comments = "TEdit - Terraria Map Editor";
        project.ControlPanelInfo.UrlUpdateInfo = "https://www.binaryconstruct.com/downloads/";
        project.ControlPanelInfo.UrlInfoAbout = "https://www.binaryconstruct.com/tedit/";
        project.ControlPanelInfo.HelpLink = "https://docs.binaryconstruct.com";
        project.ControlPanelInfo.ProductIcon = "tedit.ico";
        project.ControlPanelInfo.InstallLocation = "[INSTALLDIR]";
        project.ControlPanelInfo.NoModify = true;

        Compiler.BuildMsi(project, $"TEdit-{version}.msi");
    }
}
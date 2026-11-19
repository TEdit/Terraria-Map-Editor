using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TEdit.Input;
using TEdit.ViewModel;
using Wpf.Ui.Controls;
using Button = System.Windows.Controls.Button;

namespace TEdit.View.Popups;

public partial class SettingsWindow : FluentWindow
{
    private readonly Mcp.McpServerService _mcp;
    internal SettingsWindow(WorldViewModel wvm, Mcp.McpServerService mcp)
    {
        InitializeComponent();
        DataContext = new SettingsViewModel(wvm);
        _mcp = mcp;
        var settings = Configuration.UserSettingsService.Current.McpServer;
        McpEnabled.IsChecked = settings.Enabled;
        McpEdit.IsChecked = settings.AllowEdits;
        McpScripts.IsChecked = settings.AllowScripts;
        McpShared.IsChecked = settings.SharedLocalAccess;
        McpPort.Text = settings.Port.ToString();
        AgentClient.ItemsSource = System.Enum.GetValues<Mcp.AgentProduct>()
            .Select(p => KeyValuePair.Create(p, Mcp.AgentSetup.Name(p))).ToArray();
        AgentClient.SelectedIndex = 0;
        RefreshHost(null, null);
    }

    private void BrowsePath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SettingItem setting }) return;

        var currentPath = setting.Value as string;
        if (string.IsNullOrWhiteSpace(currentPath))
            currentPath = DependencyChecker.PathToContent ?? "";

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = setting.Description,
            SelectedPath = currentPath
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            setting.Value = dialog.SelectedPath;
        }
    }

    private void AddKeybinding_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SettingItem setting }) return;

        var captureWindow = new InputCaptureWindow { Owner = this };
        if (captureWindow.ShowDialog() == true && captureWindow.CapturedInput.HasValue)
        {
            var binding = captureWindow.CapturedInput.Value;
            if (binding.IsValid && !setting.Bindings.Contains(binding))
            {
                setting.Bindings.Add(binding);
                App.Input.Registry.SetUserBindings(setting.ActionId, new List<InputBinding>(setting.Bindings));
                App.Input.SaveUserCustomizations();
            }
        }
    }

    private void RemoveKeybinding_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is not SettingItem setting) return;
        if (button.CommandParameter is not InputBinding binding) return;

        setting.Bindings.Remove(binding);
        App.Input.Registry.SetUserBindings(setting.ActionId, new List<InputBinding>(setting.Bindings));
        App.Input.SaveUserCustomizations();
    }

    private void ResetKeybinding_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SettingItem setting }) return;

        App.Input.Registry.ResetToDefaults(setting.ActionId);
        App.Input.SaveUserCustomizations();

        // Refresh the bindings in the UI
        setting.Bindings.Clear();
        foreach (var binding in App.Input.Registry.GetBindings(setting.ActionId))
        {
            setting.Bindings.Add(binding);
        }
    }

    private void CategoryExpander_Expanded(object sender, RoutedEventArgs e)
    {
        if (sender is Expander { DataContext: CollectionViewGroup group } &&
            DataContext is SettingsViewModel vm)
        {
            vm.SetCategoryExpanded(group.Name?.ToString() ?? "", true);
        }
    }

    private void CategoryExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        if (sender is Expander { DataContext: CollectionViewGroup group } &&
            DataContext is SettingsViewModel vm)
        {
            vm.SetCategoryExpanded(group.Name?.ToString() ?? "", false);
        }
    }

    private void RefreshHost(object sender, RoutedEventArgs e)
    {
        McpStatus.Text = Properties.Language.ResourceManager.GetString("mcp_host_" + _mcp.State);
        var running = _mcp.State == Mcp.HostState.Running;
        if (running && _mcp.Preferences.AllowEdits) McpStatus.Text = Properties.Language.mcp_drawing_enabled;
        if (running && _mcp.Preferences.AllowEdits && _mcp.Preferences.AllowScripts)
            McpStatus.Text = Properties.Language.mcp_scripts_enabled;
        McpManagement.IsEnabled = running;
        McpEndpoint.Text = running ? _mcp.Endpoint : "";
        bool shared = running ? _mcp.Preferences.SharedLocalAccess : McpShared.IsChecked == true;
        McpProfilesPanel.Visibility = shared ? Visibility.Collapsed : Visibility.Visible;
        McpSetup.Text = running ? string.Format(_mcp.Preferences.SharedLocalAccess ? Properties.Language.mcp_shared_setup : Properties.Language.mcp_setup_prompt, _mcp.Endpoint) : "";
        var selectedProfile = McpClients.SelectedValue;
        McpClients.ItemsSource = running ? _mcp.Profiles.List() : null;
        if (selectedProfile != null) McpClients.SelectedValue = selectedProfile;
        UpdateAgentSetup(sender, e);
        McpHistory.ItemsSource = running ? _mcp.Operations.History().Select(
            op => KeyValuePair.Create(op.Id, op.Id + " · " + Properties.Language.ResourceManager.GetString("mcp_" + op.Status))).ToArray() : null;
    }

    private Mcp.AgentSetup? _agentSetup;
    private static string AgentExecutable => System.IO.Path.Combine(System.AppContext.BaseDirectory, "TEdit.exe");

    private void UpdateAgentSetup(object sender, RoutedEventArgs e)
    {
        if (AgentClient?.SelectedValue is not Mcp.AgentProduct product || _mcp == null) return;
        bool shared = _mcp.Preferences.SharedLocalAccess;
        bool mustUseBridge = !shared || product == Mcp.AgentProduct.ClaudeDesktop;
        AgentWorkspace.IsEnabled = Mcp.AgentSetup.SupportsWorkspace(product);
        if (!AgentWorkspace.IsEnabled) AgentWorkspace.IsChecked = false;
        if (mustUseBridge || !AgentStdio.IsEnabled) AgentStdio.IsChecked = mustUseBridge;
        AgentStdio.IsEnabled = !mustUseBridge;
        var profile = shared ? null : McpClients.SelectedValue as System.Guid?;
        _agentSetup = _mcp.State == Mcp.HostState.Running && (shared || profile.HasValue)
            ? Mcp.AgentSetup.Create(product, AgentWorkspace.IsChecked == true, AgentStdio.IsChecked == true, _mcp.Endpoint, AgentExecutable, profile)
            : null;
        AgentActions.IsEnabled = _agentSetup != null;
        AgentTarget.Text = _agentSetup?.Target ?? "";
        AgentPreview.Text = _agentSetup?.Preview ?? "";
        AgentResult.Text = !shared && profile == null ? Properties.Language.agent_profile : "";
    }

    private void CopyAgent(object sender, RoutedEventArgs e)
    {
        AgentPreview.Focus();
        AgentPreview.SelectAll();
        try { AgentPreview.Copy(); }
        catch (System.Runtime.InteropServices.ExternalException) { /* Text remains selected. */ }
    }

    private void ExportAgent(object sender, RoutedEventArgs e)
    {
        if (_agentSetup is not {} setup) return;
        var dialog = new Microsoft.Win32.SaveFileDialog { FileName = "tedit-agent." + setup.Extension, DefaultExt = setup.Extension, OverwritePrompt = false };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            setup.ExportNewFile(dialog.FileName);
        }
        catch (System.Exception ex) when (ex is System.IO.IOException or System.UnauthorizedAccessException)
        { AgentResult.Text = Properties.Language.agent_export_error; }
    }

    private async void CheckAgent(object sender, RoutedEventArgs e)
    {
        if (_agentSetup is not {} setup || sender is not Button button) return;
        button.IsEnabled = false;
        try
        {
            using var timeout = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(12));
            var profile = _mcp.Preferences.SharedLocalAccess ? null : McpClients.SelectedValue as System.Guid?;
            var endpoint = _mcp.Endpoint;
            var instance = _mcp.Instance;
            await using var client = AgentStdio.IsChecked == true
                ? await ModelContextProtocol.Client.McpClient.CreateAsync(new ModelContextProtocol.Client.StdioClientTransport(new()
                {
                    Command = AgentExecutable, Arguments = profile is System.Guid id
                        ? [Mcp.AgentBridge.Argument, endpoint, "--profile", id.ToString("N")] : [Mcp.AgentBridge.Argument, endpoint]
                }), cancellationToken: timeout.Token)
                : await Mcp.AgentBridge.ConnectAsync(endpoint, null, Mcp.McpServerService.UserDirectory, timeout.Token);
            var status = await client.CallToolAsync("get_status", cancellationToken: timeout.Token);
            bool ok = status.StructuredContent?.GetProperty("data").GetProperty("instanceId").GetGuid() == instance;
            if (setup == _agentSetup) AgentResult.Text = ok ? Properties.Language.agent_ok : Properties.Language.agent_failed;
        }
        catch (System.Exception) { if (setup == _agentSetup) AgentResult.Text = Properties.Language.agent_failed; }
        finally { button.IsEnabled = true; }
    }

    private void CopyMcpSetup(object sender, RoutedEventArgs e)
    {
        McpSetup.Focus();
        McpSetup.SelectAll();
        try { McpSetup.Copy(); }
        catch (System.Runtime.InteropServices.ExternalException) { /* Keep the text selected for manual copying. */ }
    }

    private void IssueProfile(object sender, RoutedEventArgs e)
    {
        try
        {
            var credential = _mcp.Profiles.Issue(McpName.Text);
            McpCredential.Text = "Bearer " + credential;
            RefreshHost(sender, e);
            McpClients.SelectedValue = System.Guid.ParseExact(credential[..32], "N");
        }
        catch (System.Exception) { McpStatus.Text = Properties.Language.mcp_profile_error; }
    }

    private async void RevokeProfile(object sender, RoutedEventArgs e)
    {
        if (McpClients.SelectedValue is not System.Guid id) return;
        try
        {
            McpCredential.Clear();
            await _mcp.RevokeAsync(id);
            RefreshHost(sender, e);
        }
        catch (System.Exception) { McpStatus.Text = Properties.Language.mcp_profile_error; }
    }

    private void CancelOperation(object sender, RoutedEventArgs e)
    {
        if (McpHistory.SelectedValue is System.Guid id) _mcp.Operations.Find(null, id, true);
        RefreshHost(sender, e);
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(McpPort.Text, out var port) || port is < 1024 or > 65535)
        { McpStatus.Text = Properties.Language.mcp_port; return; }
        var preferences = new Mcp.McpPreferences(McpEnabled.IsChecked == true, port, McpEdit.IsChecked == true, McpShared.IsChecked == true, McpScripts.IsChecked == true);
        Configuration.UserSettingsService.Current.McpServer = preferences;
        IsEnabled = false;
        try { await _mcp.ApplyAsync(preferences); }
        finally { IsEnabled = true; }
        RefreshHost(sender, e);
        if (_mcp.State is Mcp.HostState.Running or Mcp.HostState.Disabled) Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.RevertChanges();
        }
        Close();
    }
}

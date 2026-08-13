using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using TEdit.Terraria.Player;
using TEdit.UI.Xaml.Dialog;
using TEdit.ViewModel;
using Xunit;

namespace TEdit.Terraria.Tests.ViewModel;

public class PlayerEditorExternalChangeTests
{
    [Fact]
    public async Task SavePlayer_WhenFileChangedExternallyAndCanceled_DoesNotOverwrite()
    {
        string path = Path.Combine(Path.GetTempPath(), $"tedit-player-{Guid.NewGuid():N}.plr");
        try
        {
            PlayerFile.Save(path, new PlayerCharacter { Name = "Loaded" });
            var dialogs = new TestDialogService(DialogResponse.Cancel);
            var viewModel = new PlayerEditorViewModel(dialogs);
            viewModel.LoadPlayerFromFile(path);

            PlayerFile.Save(path, new PlayerCharacter { Name = "External" });
            byte[] externalBytes = File.ReadAllBytes(path);
            viewModel.Player!.Name = "TEdit Edit";

            await viewModel.SavePlayerCommand.Execute().FirstAsync();

            File.ReadAllBytes(path).ShouldBe(externalBytes);
            dialogs.MessageCount.ShouldBe(1);
            viewModel.StatusText.ShouldContain("canceled");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task SavePlayer_WhenExternalOverwriteAccepted_RefreshesTrackedVersion()
    {
        string path = Path.Combine(Path.GetTempPath(), $"tedit-player-{Guid.NewGuid():N}.plr");
        try
        {
            PlayerFile.Save(path, new PlayerCharacter { Name = "Loaded" });
            var dialogs = new TestDialogService(DialogResponse.OK);
            var viewModel = new PlayerEditorViewModel(dialogs);
            viewModel.LoadPlayerFromFile(path);

            PlayerFile.Save(path, new PlayerCharacter { Name = "External" });
            viewModel.Player!.Name = "TEdit Edit";

            await viewModel.SavePlayerCommand.Execute().FirstAsync();
            await viewModel.SavePlayerCommand.Execute().FirstAsync();

            PlayerFile.Load(path).Name.ShouldBe("TEdit Edit");
            dialogs.MessageCount.ShouldBe(1, "the successful save must become the new comparison baseline");
        }
        finally
        {
            File.Delete(path);
        }
    }

    private sealed class TestDialogService(DialogResponse response) : IDialogService
    {
        public int MessageCount { get; private set; }

        public Task<DialogResponse> ShowMessageAsync(string message, string caption,
            DialogButton button = DialogButton.OK, DialogImage image = DialogImage.Information,
            CancellationToken cancellationToken = default)
        {
            MessageCount++;
            return Task.FromResult(response);
        }

        public Task<DialogResponse> ShowExceptionAsync(string message,
            DialogImage image = DialogImage.Error, CancellationToken cancellationToken = default) =>
            Task.FromResult(DialogResponse.OK);

        public Task<bool> ShowConfirmationAsync(string title, string message,
            CancellationToken cancellationToken = default) => Task.FromResult(response == DialogResponse.Yes);

        public Task ShowAlertAsync(string title, string message,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<DialogResponse> ShowWarningAsync(string title, string message,
            DialogButton button = DialogButton.OK, CancellationToken cancellationToken = default) =>
            Task.FromResult(response);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEdit.Desktop.Services;

public interface IDialogService
{
    Task<string> OpenFileDialogAsync();
}

public class DialogService : IDialogService
{
    public async Task<string> OpenFileDialogAsync() => await Task.FromResult("test");
}

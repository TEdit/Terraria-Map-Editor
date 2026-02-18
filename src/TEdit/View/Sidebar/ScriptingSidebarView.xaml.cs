using System;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ReactiveUI;
using TEdit.Scripting;
using TEdit.ViewModel;

namespace TEdit.View.Sidebar;

public partial class ScriptingSidebarView : UserControl
{
    private readonly ScriptingSidebarViewModel _vm;
    private bool _suppressBinding;
    private CompletionWindow? _completionWindow;
    private IHighlightingDefinition? _jsHighlighting;
    private IHighlightingDefinition? _luaHighlighting;

    public ScriptingSidebarView()
    {
        InitializeComponent();
        _vm = ViewModelLocator.GetScriptingSidebarViewModel();
        DataContext = _vm;

        InitializeSyntaxHighlighting();
        WireBinding();
        WireCompletion();

        _vm.EnsureExampleScripts();
    }

    private void InitializeSyntaxHighlighting()
    {
        var assembly = Assembly.GetExecutingAssembly();

        _jsHighlighting = LoadHighlighting(assembly, "TEdit.Resources.JavaScript.xshd");
        _luaHighlighting = LoadHighlighting(assembly, "TEdit.Resources.Lua.xshd");

        ApplySyntaxHighlighting();

        _vm.WhenAnyValue(x => x.SelectedEngineIndex)
            .Subscribe(_ => ApplySyntaxHighlighting());
    }

    private static IHighlightingDefinition? LoadHighlighting(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;
        using var reader = new XmlTextReader(stream);
        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    private void ApplySyntaxHighlighting()
    {
        CodeEditor.SyntaxHighlighting = _vm.SelectedEngineIndex switch
        {
            0 => _jsHighlighting,
            1 => _luaHighlighting,
            _ => null
        };
    }

    private void WireBinding()
    {
        // VM → Editor: update editor text when VM changes
        _vm.WhenAnyValue(x => x.ScriptCode)
            .Subscribe(code =>
            {
                if (_suppressBinding) return;
                _suppressBinding = true;
                if (CodeEditor.Document.Text != code)
                    CodeEditor.Document.Text = code ?? "";
                _suppressBinding = false;
            });

        // Editor → VM: update VM when editor text changes
        CodeEditor.TextChanged += (_, _) =>
        {
            if (_suppressBinding) return;
            _suppressBinding = true;
            _vm.ScriptCode = CodeEditor.Document.Text;
            _suppressBinding = false;
        };
    }

    private void WireCompletion()
    {
        CodeEditor.TextArea.TextEntering += OnTextEntering;
        CodeEditor.TextArea.TextEntered += OnTextEntered;

        // Ctrl+Space for top-level module completion
        CodeEditor.TextArea.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                ShowModuleCompletion();
            }
        };
    }

    private void OnTextEntering(object sender, TextCompositionEventArgs e)
    {
        if (_completionWindow != null && e.Text.Length > 0)
        {
            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                _completionWindow.CompletionList.RequestInsertion(e);
            }
        }
    }

    private void OnTextEntered(object sender, TextCompositionEventArgs e)
    {
        if (e.Text != ".") return;

        // Find the word before the dot
        var offset = CodeEditor.CaretOffset - 1; // position of the dot
        var document = CodeEditor.Document;
        var wordStart = offset - 1;

        while (wordStart >= 0 && char.IsLetterOrDigit(document.GetCharAt(wordStart)))
            wordStart--;
        wordStart++;

        if (wordStart >= offset) return;

        var moduleName = document.GetText(wordStart, offset - wordStart);
        var module = ScriptApiMetadata.Modules.FirstOrDefault(m =>
            m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

        if (module == null) return;

        ShowMethodCompletion(module);
    }

    private void ShowModuleCompletion()
    {
        _completionWindow = new CompletionWindow(CodeEditor.TextArea);
        var data = _completionWindow.CompletionList.CompletionData;

        foreach (var module in ScriptApiMetadata.Modules)
        {
            data.Add(new ScriptCompletionData(module.Name, module.Description));
        }

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) => _completionWindow = null;
    }

    private void ShowMethodCompletion(ScriptApiMetadata.ApiModule module)
    {
        _completionWindow = new CompletionWindow(CodeEditor.TextArea);
        var data = _completionWindow.CompletionList.CompletionData;

        foreach (var method in module.Methods)
        {
            data.Add(new ScriptCompletionData(method.Name, $"{method.Signature}\n{method.Description}"));
        }

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) => _completionWindow = null;
    }
}

internal class ScriptCompletionData : ICompletionData
{
    public ScriptCompletionData(string text, string description)
    {
        Text = text;
        Description = description;
    }

    public ImageSource? Image => null;
    public string Text { get; }
    public object Content => Text;
    public object Description { get; }
    public double Priority => 1;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Text);
    }
}

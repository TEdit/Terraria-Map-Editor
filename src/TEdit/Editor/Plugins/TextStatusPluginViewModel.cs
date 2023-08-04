using TEdit.Common.Reactive;
using Microsoft.Xna.Framework;
using System.Windows;

namespace TEdit.Editor.Plugins;

public class TextStatusPluginViewModel : ObservableObject
{
    private string textValue;
    private int lineSpacing = 1;
    private int letterSpacing = 0;
    private int lineLength = 64;
    private TextAlignment justification = TextAlignment.Left;

    public string TextValue
    {
        get => textValue;
        set
        {
            textValue = value;
            RaisePropertyChanged(nameof(TextValue));
        }
    }
    public int LineSpacing
    {
        get => lineSpacing;
        set
        {

            lineSpacing = MathHelper.Clamp(value, 0, 50);
            RaisePropertyChanged(nameof(LineSpacing));
        }
    }
    public int LetterSpacing
    {
        get => letterSpacing;
        set
        {
            letterSpacing = MathHelper.Clamp(value, 0, 50); ;
            RaisePropertyChanged(nameof(LetterSpacing));
        }
    }
    public int LineLength
    {
        get => lineLength;
        set
        {
            lineLength = MathHelper.Clamp(value, 1, 400); ;
            RaisePropertyChanged(nameof(LineLength));
        }
    }
    public TextAlignment Justification
    {
        get => justification;
        set
        {
            justification = value;
            RaisePropertyChanged(nameof(Justification));
        }
    }
}

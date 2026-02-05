using ReactiveUI;
using Microsoft.Xna.Framework;
using System.Windows;

namespace TEdit.Editor.Plugins;

public partial class TextStatusPluginViewModel : ReactiveObject
{
    private string textValue;
    public string TextValue
    {
        get => textValue;
        set
        {
            textValue = value;
            this.RaisePropertyChanged(nameof(TextValue));
        }
    }

    private int lineSpacing = 1;
    public int LineSpacing
    {
        get => lineSpacing;
        set
        {

            lineSpacing = MathHelper.Clamp(value, 0, 50);
            this.RaisePropertyChanged(nameof(LineSpacing));
        }
    }

    private int letterSpacing = 0;
    public int LetterSpacing
    {
        get => letterSpacing;
        set
        {
            letterSpacing = MathHelper.Clamp(value, 0, 50); ;
            this.RaisePropertyChanged(nameof(LetterSpacing));
        }
    }

    private int lineLength = 64;
    public int LineLength
    {
        get => lineLength;
        set
        {
            lineLength = MathHelper.Clamp(value, 1, 400); ;
            this.RaisePropertyChanged(nameof(LineLength));
        }
    }

    private TextAlignment justification = TextAlignment.Left;
    public TextAlignment Justification
    {
        get => justification;
        set
        {
            justification = value;
            this.RaisePropertyChanged(nameof(Justification));
        }
    }
}

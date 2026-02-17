using TEdit.Common;

namespace TEdit.Terraria.Player;

public partial class PlayerAppearance : ReactiveObject
{
    [Reactive] private int _hair;
    [Reactive] private byte _hairDye;
    [Reactive] private byte _skinVariant;

    [Reactive] private TEditColor _hairColor = new(151, 100, 69);
    [Reactive] private TEditColor _skinColor = new(255, 125, 90);
    [Reactive] private TEditColor _eyeColor = new(105, 90, 75);
    [Reactive] private TEditColor _shirtColor = new(175, 165, 140);
    [Reactive] private TEditColor _underShirtColor = new(160, 180, 215);
    [Reactive] private TEditColor _pantsColor = new(255, 230, 175);
    [Reactive] private TEditColor _shoeColor = new(160, 105, 60);

    public bool Male => SkinVariant < 4;
}

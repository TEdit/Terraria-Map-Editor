using System.Windows.Input;
using TEdit.Input;
using Xunit;
using InputBinding = TEdit.Input.InputBinding;
using InputType = TEdit.Input.InputType;

namespace TEdit.Tests.Input;

public class InputBindingTests
{
    [Fact]
    public void Keyboard_CreatesValidBinding()
    {
        var binding = InputBinding.Keyboard(Key.C, ModifierKeys.Control);

        Assert.Equal(InputType.Keyboard, binding.Type);
        Assert.Equal(Key.C, binding.Key);
        Assert.Equal(ModifierKeys.Control, binding.Modifiers);
        Assert.True(binding.IsValid);
    }

    [Fact]
    public void Mouse_CreatesValidBinding()
    {
        var binding = InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Shift);

        Assert.Equal(InputType.Mouse, binding.Type);
        Assert.Equal(TEditMouseButton.Left, binding.MouseButton);
        Assert.Equal(ModifierKeys.Shift, binding.Modifiers);
        Assert.True(binding.IsValid);
    }

    [Fact]
    public void Wheel_CreatesValidBinding()
    {
        var binding = InputBinding.Wheel(MouseWheelDirection.Up, ModifierKeys.Control);

        Assert.Equal(InputType.MouseWheel, binding.Type);
        Assert.Equal(MouseWheelDirection.Up, binding.MouseWheel);
        Assert.Equal(ModifierKeys.Control, binding.Modifiers);
        Assert.True(binding.IsValid);
    }

    [Fact]
    public void Default_IsNotValid()
    {
        var binding = default(InputBinding);

        Assert.Equal(InputType.None, binding.Type);
        Assert.False(binding.IsValid);
    }

    [Theory]
    [InlineData("Ctrl+C", Key.C, ModifierKeys.Control)]
    [InlineData("Shift+A", Key.A, ModifierKeys.Shift)]
    [InlineData("Alt+F4", Key.F4, ModifierKeys.Alt)]
    [InlineData("Ctrl+Shift+S", Key.S, ModifierKeys.Control | ModifierKeys.Shift)]
    [InlineData("Z", Key.Z, ModifierKeys.None)]
    public void Parse_KeyboardBindings(string input, Key expectedKey, ModifierKeys expectedModifiers)
    {
        var binding = InputBinding.Parse(input);

        Assert.Equal(InputType.Keyboard, binding.Type);
        Assert.Equal(expectedKey, binding.Key);
        Assert.Equal(expectedModifiers, binding.Modifiers);
    }

    [Theory]
    [InlineData("LeftClick", TEditMouseButton.Left, ModifierKeys.None)]
    [InlineData("RightClick", TEditMouseButton.Right, ModifierKeys.None)]
    [InlineData("MiddleClick", TEditMouseButton.Middle, ModifierKeys.None)]
    [InlineData("Ctrl+LeftClick", TEditMouseButton.Left, ModifierKeys.Control)]
    [InlineData("Shift+RightClick", TEditMouseButton.Right, ModifierKeys.Shift)]
    [InlineData("Mouse4", TEditMouseButton.XButton1, ModifierKeys.None)]
    [InlineData("Mouse5", TEditMouseButton.XButton2, ModifierKeys.None)]
    public void Parse_MouseBindings(string input, TEditMouseButton expectedButton, ModifierKeys expectedModifiers)
    {
        var binding = InputBinding.Parse(input);

        Assert.Equal(InputType.Mouse, binding.Type);
        Assert.Equal(expectedButton, binding.MouseButton);
        Assert.Equal(expectedModifiers, binding.Modifiers);
    }

    [Theory]
    [InlineData("WheelUp", MouseWheelDirection.Up, ModifierKeys.None)]
    [InlineData("WheelDown", MouseWheelDirection.Down, ModifierKeys.None)]
    [InlineData("Ctrl+WheelUp", MouseWheelDirection.Up, ModifierKeys.Control)]
    [InlineData("Shift+WheelDown", MouseWheelDirection.Down, ModifierKeys.Shift)]
    public void Parse_WheelBindings(string input, MouseWheelDirection expectedDirection, ModifierKeys expectedModifiers)
    {
        var binding = InputBinding.Parse(input);

        Assert.Equal(InputType.MouseWheel, binding.Type);
        Assert.Equal(expectedDirection, binding.MouseWheel);
        Assert.Equal(expectedModifiers, binding.Modifiers);
    }

    [Theory]
    [InlineData("")]
    [InlineData("None")]
    [InlineData(null)]
    public void Parse_InvalidReturnsDefault(string? input)
    {
        var binding = InputBinding.Parse(input!);

        Assert.False(binding.IsValid);
    }

    [Fact]
    public void ToString_Keyboard_FormatsCorrectly()
    {
        var binding = InputBinding.Keyboard(Key.C, ModifierKeys.Control);
        Assert.Equal("Ctrl+C", binding.ToString());
    }

    [Fact]
    public void ToString_KeyboardMultipleModifiers_FormatsCorrectly()
    {
        var binding = InputBinding.Keyboard(Key.S, ModifierKeys.Control | ModifierKeys.Shift);
        Assert.Equal("Ctrl+Shift+S", binding.ToString());
    }

    [Fact]
    public void ToString_Mouse_FormatsCorrectly()
    {
        var binding = InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Control);
        Assert.Equal("Ctrl+LeftClick", binding.ToString());
    }

    [Fact]
    public void ToString_Wheel_FormatsCorrectly()
    {
        var binding = InputBinding.Wheel(MouseWheelDirection.Up, ModifierKeys.None);
        Assert.Equal("WheelUp", binding.ToString());
    }

    [Fact]
    public void RoundTrip_PreservesBinding()
    {
        var original = InputBinding.Keyboard(Key.Z, ModifierKeys.Control);
        var serialized = original.ToString();
        var parsed = InputBinding.Parse(serialized);

        Assert.Equal(original, parsed);
    }

    [Fact]
    public void Equals_SameBindings_AreEqual()
    {
        var a = InputBinding.Keyboard(Key.A, ModifierKeys.Control);
        var b = InputBinding.Keyboard(Key.A, ModifierKeys.Control);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equals_DifferentBindings_AreNotEqual()
    {
        var a = InputBinding.Keyboard(Key.A, ModifierKeys.Control);
        var b = InputBinding.Keyboard(Key.B, ModifierKeys.Control);

        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void GetHashCode_SameBindings_SameHash()
    {
        var a = InputBinding.Keyboard(Key.A, ModifierKeys.Control);
        var b = InputBinding.Keyboard(Key.A, ModifierKeys.Control);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}

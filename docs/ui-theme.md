# UI Theme Options for TEdit

## Current Theme

TEdit WPF uses a custom **Expression Dark** theme implemented entirely with XAML ResourceDictionaries (no external packages). The theme files are located in `src/TEdit/Themes/`:

- **ExpressionDark.xaml** (~3,820 lines) - Main theme with control templates
- **GlobalStyles.xaml** - Global app styles and custom components
- **Templates.xaml** - Application-specific data templates

The theme is licensed under the Microsoft Public License (Ms-PL) and uses dynamic resource binding throughout.

## Modern Dark Theme Alternatives

### 1. Built-in Fluent Theme (.NET 9+)

Microsoft added a native Fluent theme in .NET 9 with Windows 11 aesthetics, integrated light/dark modes, and system accent color support.

**Availability:** Both TEdit WPF (net10.0-windows) and TEdit5/Avalonia (net10.0) can use modern Fluent theming.

**Reference:** [WPF .NET 9 Fluent Theme](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net90)

### 2. Material Design in XAML Toolkit (MDIX)

Modern Material Design 3 implementation for WPF.

- Comprehensive control library with additional controls (dialogs, cards, snackbars, etc.)
- Very active community
- NuGet: `MaterialDesignThemes`

**Reference:** [Material Design in XAML Toolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)

### 3. MahApps.Metro

Modern Windows 10/11 look and feel for WPF applications.

- Well-established, stable library
- Modern window chrome and controls
- Can integrate with Material Design via `MaterialDesignThemes.MahApps`
- NuGet: `MahApps.Metro`

**Reference:** [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)

### 4. MDIX + MahApps Combined

Many projects use both together for the best of both worlds:
- MahApps window chrome and flyouts
- Material Design styling and controls

**Reference:** [MahApps.Metro Integration](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/MahApps.Metro-integration)

## Recommendations

| Target | Recommended Option |
|--------|-------------------|
| TEdit WPF (net10.0-windows) | Built-in Fluent Theme, MahApps.Metro, or Material Design |
| TEdit5 (Avalonia, net10.0) | Built-in Avalonia Fluent themes |

## Additional Resources

- [The State of WPF in 2025](https://www.edandersen.com/p/the-state-of-wpf-in-2025)
- [Getting Started with Material Design](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/Getting-Started)

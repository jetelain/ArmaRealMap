# GameRealisticMap.Studio Instructions

## Project Overview
GameRealisticMap.Studio is the main WPF desktop application of the GameRealisticMap toolset, which generates realistic Arma 3 terrains from real-world geographic data and allows Arma 3 terrain editing.

## UI Framework
- **WPF** application using the **Gemini** IDE framework and **Caliburn.Micro** MVVM toolkit.
- XAML views follow the Caliburn.Micro conventions:
  - Namespace: `xmlns:cal="http://www.caliburnproject.org"`
  - Button actions: `cal:Message.Attach="MethodName"`
  - ViewModels inherit `PropertyChangedBase` or `Screen`.
  - Property change notifications: `NotifyOfPropertyChange(nameof(Property))` or `NotifyOfPropertyChange(string.Empty)` to refresh all bindings.
- Views are located in `Modules/<ModuleName>/Views/` and ViewModels in `Modules/<ModuleName>/ViewModels/`.
- MEF (`[Export]`, `[ImportingConstructor]`) is used for dependency injection throughout the Studio.

## Localization
- All user-visible strings in XAML views must use `Labels.resx` (at `GameRealisticMap.Studio/Labels.resx`).
- Add new entries to `Labels.resx` for every label, button, or message in a view.
- Reference strings in XAML using `{x:Static r:Labels.KeyName}` or `<x:Static Member="r:Labels.KeyName"/>` inside content elements.
- The `r` namespace is declared as: `xmlns:r="clr-namespace:GameRealisticMap.Studio"`
- The generated accessor class is `Labels` in the `GameRealisticMap.Studio` namespace (`Labels.Designer.cs`).

#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Mosaic.UI.Wpf;
using Mosaic.UI.Wpf.Common;
using Mosaic.UI.Wpf.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Avalon.Common
{
    /// <summary>
    /// Stores Mosaic-specific application settings independently from MUD profiles.
    /// </summary>
    public partial class AppSettings : ObservableObject, IAppSettings
    {
        [property: Browsable(false)]
        [ObservableProperty]
        private string? _applicationDataFolder;

        [property: Browsable(false)]
        [System.Text.Json.Serialization.JsonIgnore]
        [ObservableProperty]
        private LocalSettings? _clientSettings = new();

        [property: Category("Appearance")]
        [property: DisplayName("Theme")]
        [ObservableProperty]
        private MosaicThemeMode _theme = MosaicThemeMode.Dark;

        [property: Browsable(false)]
        [ObservableProperty]
        private ObservableCollection<WindowViewState> _windowViewStates = new();
    }
}

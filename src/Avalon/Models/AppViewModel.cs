using System.Windows.Markup;
using Avalon.Controls;
using Argus.Memory;
using System.IO;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Mosaic.UI.Wpf;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalon.Models
{
    /// <summary>
    /// A static instance application view model that can be used for observable
    /// app settings. The AppViewModel is generally not persisted beyond the execution
    /// run of the app.
    /// </summary>
    public partial class AppViewModel : ObservableObject
    {
        [ObservableProperty]
        private AppSettings _appSettings = new();

        /// <summary>
        /// The title of the application
        /// </summary>
        [ObservableProperty]
        private string _title = "Avalon Mud Client";

        /// <summary>
        /// The text displayed in the main window status bar.
        /// </summary>
        [ObservableProperty]
        private string _statusText = "Ready";

        /// <summary>
        /// The game terminal whose document is currently showing, used by the status bar. Null when no
        /// game is open.
        /// </summary>
        [ObservableProperty]
        private MudTerminal? _activeTerminal;

        /// <summary>
        /// Gets or sets the number of editor scripts running without a game terminal.
        /// </summary>
        [ObservableProperty]
        private int _standaloneScriptsActive;

        /// <summary>
        /// The title of the alert shown in the ribbon footer.
        /// </summary>
        [ObservableProperty]
        private string _alertTitle = "";

        /// <summary>
        /// The message of the alert shown in the ribbon footer.
        /// </summary>
        [ObservableProperty]
        private string _alertMessage = "";

        /// <summary>
        /// The severity of the alert shown in the ribbon footer.
        /// </summary>
        [ObservableProperty]
        private InfoBarSeverity _alertSeverity = InfoBarSeverity.Information;

        /// <summary>
        /// Whether the ribbon footer alert is showing.
        /// </summary>
        [ObservableProperty]
        private bool _alertVisible;

        /// <summary>
        /// Shows an alert in the ribbon footer.
        /// </summary>
        public void ShowAlert(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Information)
        {
            AlertTitle = title;
            AlertMessage = message;
            AlertSeverity = severity;
            AlertVisible = true;
        }

        /// <summary>
        /// Saves the current application settings to their normal location.
        /// </summary>
        [RelayCommand]
        private void Save() => App.SaveAppSettings();

        /// <summary>
        /// Prompts for a destination and saves a copy of the current application settings.
        /// </summary>
        [RelayCommand]
        private void SaveAs()
        {
            var path = WpfUtilities.SaveFileRequest("Save a copy of AppSettings", "JSON settings (*.json)|*.json");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(AppSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
            {
                Mosaic.UI.Wpf.Controls.MessageBox.Show(ex.Message, "Save Settings Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }

    /// <summary>
    /// A markup extension that provides an instance of <see cref="AppViewModel"/>.
    /// </summary>
    /// <remarks>
    /// <example>  
    /// <![CDATA[  
    /// <Window xmlns:vm="clr-namespace:Continuum.Common"  
    ///         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">  
    ///     <Window.Resources>  
    ///         <vm:AppViewModelLocator x:Key="AppViewModel" />  
    ///     </Window.Resources>  
    ///     <Grid DataContext="{StaticResource AppViewModel}">  
    ///         <!-- Your UI elements here -->  
    ///     </Grid>  
    /// </Window>  
    /// ]]>  
    /// </example>  
    /// </remarks>
    public class AppViewModelLocator : MarkupExtension
    {
        /// <summary>
        /// Returns an instance of <see cref="AppViewModel"/> from the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>An instance of <see cref="AppViewModel"/>.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return AppServices.GetRequiredService<AppViewModel>();
        }
    }
}

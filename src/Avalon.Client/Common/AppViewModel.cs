using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalon.Common
{
    /// <summary>
    /// Application-level view model used by Mosaic services.
    /// </summary>
    public partial class AppViewModel : ObservableObject
    {
        [ObservableProperty]
        private AppSettings _appSettings = new();

        [ObservableProperty]
        private string _title = "Avalon Mud Client";
    }
}

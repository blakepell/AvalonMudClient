using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalon.Models;

/// <summary>
/// The severity of an <see cref="Alert"/> or info bar.
/// </summary>
public enum InfoBarSeverity
{
    /// <summary>
    /// General information.
    /// </summary>
    Information,

    /// <summary>
    /// Something completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// Something needs attention.
    /// </summary>
    Warning,

    /// <summary>
    /// Something failed.
    /// </summary>
    Error
}

/// <summary>
/// A message shown over a terminal until it expires or is closed.
/// </summary>
public partial class Alert : ObservableObject
{
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [ObservableProperty]
    public partial string Title { get; set; } = "Alert";

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    [ObservableProperty]
    public partial string Message { get; set; } = "";

    /// <summary>
    /// Gets or sets the text of the optional action link.
    /// </summary>
    [ObservableProperty]
    public partial string? ActionMessage { get; set; }

    /// <summary>
    /// Gets or sets how many seconds the alert shows, or 0 to use the default.
    /// </summary>
    [ObservableProperty]
    public partial int Duration { get; set; }

    /// <summary>
    /// Gets or sets whether the alert stays until it is closed.
    /// </summary>
    [ObservableProperty]
    public partial bool Permanent { get; set; }

    /// <summary>
    /// Gets or sets the severity.
    /// </summary>
    [ObservableProperty]
    public partial InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Information;

    /// <summary>
    /// Gets or sets the command sent when the action link is clicked.
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// Gets or sets the JavaScript run when the action link is clicked.
    /// </summary>
    public string? JavaScript { get; set; }

    /// <summary>
    /// Gets or sets a .NET action run when the action link is clicked.
    /// </summary>
    public Action<Alert>? Action { get; set; }
}

# PullToRefreshWrapper

**Base class:** `ContentControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/PullToRefreshWrapper/PullToRefreshWrapper.cs`

Decorates scrollable content with a mouse and touch pull-to-refresh gesture.

**Demo:** `src/MosaicWpfDemo/Views/Examples/PullToRefreshWrapperExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `bool IsRefreshing` | Gets or sets whether refresh is running. Set to false when the consumer's task finishes. |
| `ICommand? RefreshCommand` | Gets or sets the command executed after a qualifying pull or keyboard refresh request. |
| `object? RefreshCommandParameter` | Gets or sets the parameter passed to the refresh command. |
| `double PullThreshold` | Gets or sets the required translated pull distance in device-independent pixels. Defaults to 100. |
| `double PullFriction` | Gets or sets the drag multiplier, greater than zero and at most one. Defaults to 0.5. |
| `double PullProgress` | Gets the pull percentage, clamped to the range 0–100. |
| `bool IsPullReady` | Gets whether the current pull has reached the refresh threshold. |
| `bool RequestRefresh(…)` | Requests refresh when enabled, idle, and the command can execute. |


## Usage notes

Wrap scrollable content and bind `RefreshCommand`. Keep `IsRefreshing` true while refreshing, then reset it when the refresh completes.

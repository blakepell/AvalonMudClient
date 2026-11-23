using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Avalon.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Controls;

/// <summary>
/// Displays the game resources tracked by a named dock panel.
/// </summary>
public partial class VitalsPanel : UserControl
{
    private readonly ObservableCollection<VitalRow> _rows = [];
    private readonly Action<List<string>> _saveLabels;

    internal VitalsPanel(IEnumerable<string> labels, Action<List<string>> saveLabels)
    {
        InitializeComponent();
        _saveLabels = saveLabels;
        VitalRows.ItemsSource = _rows;

        foreach (var label in labels.Where(label => !string.IsNullOrWhiteSpace(label))
                     .Select(label => label.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _rows.Add(new VitalRow(label));
        }
    }

    internal bool SetVital(string? label, double current, double maximum)
    {
        if (string.IsNullOrWhiteSpace(label) || !double.IsFinite(current) || !double.IsFinite(maximum) || maximum <= 0)
        {
            return false;
        }

        label = label.Trim();
        if (FindRow(label) is { } row)
        {
            row.SetReading(current, maximum);
        }
        else
        {
            row = new VitalRow(label);
            row.SetReading(current, maximum);
            _rows.Add(row);
        }

        return true;
    }

    internal bool RemoveVital(string? label) => FindRow(label?.Trim()) is { } row && _rows.Remove(row);

    private VitalRow? FindRow(string? label) => _rows.FirstOrDefault(row =>
        string.Equals(row.Label, label, StringComparison.OrdinalIgnoreCase));

    private void ConfigureVitals_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new VitalsEditor(_rows.Select(row => row.Label));
        if (Window.GetWindow(this) is { } owner)
        {
            dialog.Owner = owner;
        }

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var rows = dialog.Labels.Select(label => FindRow(label) ?? new VitalRow(label)).ToList();
        _rows.Clear();
        foreach (var (row, label) in rows.Zip(dialog.Labels))
        {
            row.Label = label;
            _rows.Add(row);
        }

        _saveLabels(dialog.Labels.ToList());
    }

    private sealed class VitalsEditor : DialogBase
    {
        public ObservableCollection<string> Labels { get; }

        public VitalsEditor(IEnumerable<string> labels)
        {
            Title = "Configure Vitals";
            Width = 380;
            Height = 330;
            Labels = new ObservableCollection<string>(labels);

            var editor = new StringListEditor
            {
                Margin = new Thickness(8),
                Items = Labels,
                Validate = label => !Labels.Contains(label, StringComparer.OrdinalIgnoreCase)
            };
            SetBody(editor);
            Loaded += (_, _) => editor.InputTextBox?.Focus();
        }
    }

    private enum VitalCondition { Healthy, Warning, Critical }

    private sealed class VitalRow(string label) : ObservableObject
    {
        private double _current;
        private double _maximum;

        public string Label { get; set; } = label;
        public double Fraction => _maximum > 0 ? Math.Clamp(_current / _maximum, 0, 1) : 0;
        public string Reading => _maximum > 0 ? $"{_current:0.##}/{_maximum:0.##}" : "—";
        public VitalCondition Condition => Fraction switch
        {
            >= 0.60 => VitalCondition.Healthy,
            >= 0.30 => VitalCondition.Warning,
            _ => VitalCondition.Critical
        };

        public void SetReading(double current, double maximum)
        {
            if (_current == current && _maximum == maximum)
            {
                return;
            }

            _current = current;
            _maximum = maximum;
            OnPropertyChanged(nameof(Fraction));
            OnPropertyChanged(nameof(Reading));
            OnPropertyChanged(nameof(Condition));
        }
    }
}

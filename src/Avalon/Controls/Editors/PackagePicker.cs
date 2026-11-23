using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Avalon.Models;

namespace Avalon.Controls.Editors;

/// <summary>
/// Selects an item's package from the packages registered in its profile.
/// </summary>
internal sealed class PackagePicker : ComboBox
{
    public static readonly DependencyProperty ProfileProperty = DependencyProperty.Register(
        nameof(Profile), typeof(Game), typeof(PackagePicker), new PropertyMetadata(null, UpdateChoices));

    public static readonly DependencyProperty PackageNameProperty = DependencyProperty.Register(
        nameof(PackageName), typeof(string), typeof(PackagePicker), new PropertyMetadata(null, UpdateChoices));

    private bool _updatingChoices;

    public PackagePicker()
    {
        SetResourceReference(StyleProperty, typeof(ComboBox));
        AutomationProperties.SetName(this, "Package");
        DisplayMemberPath = nameof(Choice.Label);
        SelectedValuePath = nameof(Choice.Name);
        RefreshChoices();
    }

    public Game? Profile
    {
        get => (Game?)GetValue(ProfileProperty);
        set => SetValue(ProfileProperty, value);
    }

    public string? PackageName
    {
        get => (string?)GetValue(PackageNameProperty);
        set => SetValue(PackageNameProperty, value);
    }

    protected override void OnDropDownOpened(EventArgs e)
    {
        RefreshChoices();
        base.OnDropDownOpened(e);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        if (!_updatingChoices && SelectedItem is Choice choice)
        {
            SetCurrentValue(PackageNameProperty, choice.Name);
        }
    }

    private static void UpdateChoices(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var picker = (PackagePicker)sender;
        if (e.Property == ProfileProperty || !Equals(picker.SelectedValue, e.NewValue))
        {
            picker.RefreshChoices();
        }
    }

    private void RefreshChoices()
    {
        var choices = new List<Choice> { new("", "(None)") };
        if (Profile is { } profile)
        {
            choices.AddRange(profile.Packages.Select(package => package.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                .Select(name => new Choice(name, name)));
        }

        string currentName = PackageName ?? "";
        var selected = choices.Find(choice => string.Equals(choice.Name, currentName, StringComparison.OrdinalIgnoreCase));
        if (selected is null)
        {
            // Keep the existing assignment visible if its package was renamed, removed or never imported.
            selected = new Choice(currentName, $"{currentName} (Unavailable)");
            choices.Add(selected);
        }

        _updatingChoices = true;
        try
        {
            ItemsSource = choices;
            SelectedItem = selected;
        }
        finally
        {
            _updatingChoices = false;
        }
    }

    private sealed record Choice(string Name, string Label);
}

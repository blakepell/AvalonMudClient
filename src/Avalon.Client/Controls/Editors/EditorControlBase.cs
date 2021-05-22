/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using ModernWpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Avalon.Common.Utilities;

namespace Avalon.Controls
{
    /// <summary>
    /// Base class with shared functionality for Editor controls that implement a <see cref="DataGrid"/>
    /// and some kind of entry panel for each item.
    /// </summary>
    /// <remarks>
    /// In order to hide the Generic nature of this from the XAML which does not support Generics we'll have
    /// to have a class inherit from this specifying the generic type and then a class that inherits from that
    /// one that will inherit it and not specify the type, effectively making the XAML think it's a regular
    /// old class with a generic declaration.  Seems hacky, but works. ¯\_(ツ)_/¯
    /// </remarks>
    public class EditorControlBase<T> : UserControl, IShellControl
    {
        /// <summary>
        /// Timer that sets the delay on your filtering TextBox.
        /// </summary>
        private DispatcherTimer _typingTimer;

        /// <summary>
        /// A reference to the parent window.
        /// </summary>
        public Shell ParentWindow { get; set; }

        /// <summary>
        /// A reference to the <see cref="DataGrid"/> for the editor control.
        /// </summary>
        public DataGrid DataList { get; set; }

        /// <summary>
        /// A reference to the <see cref="SearchBox"/> control if one exits on the form.  If one does
        /// not this will be null.
        /// </summary>
        public SearchBox SearchBox { get; set; }

        public static readonly DependencyProperty SourceListProperty = DependencyProperty.Register(
            "PropertyType", typeof(FullyObservableCollection<T>), typeof(EditorControlBase<T>), new PropertyMetadata(new FullyObservableCollection<T>()));

        /// <summary>
        /// The source generic FullyObservableCollection list.
        /// </summary>
        public FullyObservableCollection<T> SourceList
        {
            get => (FullyObservableCollection<T>)GetValue(SourceListProperty);
            set => SetValue(SourceListProperty, value);
        }

        public EditorControlBase(FullyObservableCollection<T> sourceList)
        {
            this.SourceList = sourceList;
            this.Loaded += EditorControlBase_Loaded;
            this.Unloaded += EditorControlBase_Unloaded;

            _typingTimer = new DispatcherTimer();
            _typingTimer.Tick += this._typingTimer_Tick;
        }

        private void EditorControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            // Find all of the controls we need to exist from the child control.
            this.ParentWindow = this.FindAscendant<Shell>();
            this.DataList = this.FindDescendant<DataGrid>();
            this.SearchBox = this.FindDescendant<SearchBox>();

            // We had to find this before wiring it up.
            this.SearchBox.TextChanged += SearchBoxFilter_TextChanged;

            this.FocusFilter();

            // Load the variable list the first time that it's requested.
            DataList.ItemsSource = new ListCollectionView(this.SourceList)
            {
                Filter = Filter
            };

            DataList.SelectedItem = null;

            var win = this.FindAscendant<Shell>();

            if (win != null)
            {
                win.StatusBarRightText = $"{this.SourceList.Count.ToString()} items listed.";
            }
        }

        /// <summary>
        /// Unwire events, cleanup resources and release references.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorControlBase_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unbind();

            if (typeof(T).GetInterfaces().Contains(typeof(IModelInfo)))
            {
                // Cleanup any items from the source list if they are empty (and that object implements ModelInfo).
                // This will allow the model to tell us if it's considered empty (as an example, if a Variable doesn't
                // have a key I consider it empty (even if other properties are there).
                for (int i = this.SourceList.Count - 1; i >= 0; i--)
                {
                    if (((IModelInfo)this.SourceList[i]).IsEmpty())
                    {
                        this.SourceList.RemoveAt(i);
                    }
                }
            }

            // Unsubscribe all events we manually subscribed to.
            _typingTimer.Stop();
            _typingTimer.Tick -= this._typingTimer_Tick;
            this.Loaded -= EditorControlBase_Loaded;
            this.Unloaded -= EditorControlBase_Unloaded;
            this.SearchBox.TextChanged -= SearchBoxFilter_TextChanged;

            this.ParentWindow = null;
            this.DataList = null;
            this.SearchBox = null;
            this.SourceList = null;
            _typingTimer = null;
        }

        /// <summary>
        /// The typing delay timer's tick that will refresh the filter after 300ms.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// It's important to stop this timer when this fires so that it doesn't continue
        /// to fire until it's needed again.
        /// </remarks>
        private void _typingTimer_Tick(object sender, EventArgs e)
        {
            _typingTimer.Stop();
            ((ListCollectionView)DataList?.ItemsSource)?.Refresh();
        }

        /// <summary>
        /// The filter's text changed event that will setup the delay timer and effective callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _typingTimer.Stop();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(300);
            _typingTimer.Start();
        }

        /// <summary>
        /// Removes and cleans up the binding if one exists on the DataGrid.
        /// </summary>
        public void Unbind()
        {
            if (this.DataList == null)
            {
                return;
            }

            if (DataList.ItemsSource is ListCollectionView lcv)
            {
                // Detach, clear the value and remove the reference.  I've read in a few places doing so
                // helps with the garbage collector cleaning this stuff up.
                lcv.DetachFromSourceCollection();
                DataList.ClearValue(ItemsControl.ItemsSourceProperty);
                DataList.ItemsSource = null;
            }
        }

        /// <summary>
        /// Reloads the DataList's ItemSource if it's changed.
        /// </summary>
        public void Reload()
        {
            this.Unbind();

            DataList.ItemsSource = new ListCollectionView(this.SourceList)
            {
                Filter = Filter
            };

            DataList.Items.Refresh();
        }

        /// <summary>
        /// Sets the focus onto the filter text box.
        /// </summary>
        public void FocusFilter()
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(() => this.SearchBox.Focus()));
        }

        /// <summary>
        /// The filter that runs when someone types in the search box.  This should be overriden to
        /// meet the requirements of each control.  If not overriden true is always returned.
        /// </summary>
        /// <param name="item"></param>
        public virtual bool Filter(object item)
        {
            return true;
        }

        public virtual void PrimaryButtonClick()
        {
        }

        public virtual void SecondaryButtonClick()
        {
        }
    }

}

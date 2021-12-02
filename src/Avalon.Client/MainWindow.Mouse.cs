/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Avalon
{
    /// <summary>
    /// Partial class of the MainWindow that handles mouse and scroll events.
    /// </summary>
    public partial class MainWindow
    {

        /// <summary>
        /// Handles the scroll for terminals that auto scroll, but stop auto scrolling
        /// when the user manually scrolls up.  When the scroll goes all the way to the
        /// bottom auto scroll is re-enabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommTerminal_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sv = e.OriginalSource as ScrollViewer;

            if (sv == null)
            {
                return;
            }

            var term = sender as AvalonTerminal;

            if (term == null)
            {
                return;
            }

            if (e.ExtentHeightChange == 0)
            {
                // Content unchanged: User Scroll Event
                // If the scroll bar isn't on the bottom or within our threshold we won't
                // auto-scroll, otherwise, we will.
                term.IsAutoScrollEnabled = Math.Abs(e.VerticalOffset - sv.ScrollableHeight) < 20;
            }
        }

        /// <summary>
        /// Pass mouse scrolling off to the back buffer.  Probably.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameTerminal_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // If no back buffer, don't bother.
            if (!App.Settings.AvalonSettings.BackBufferEnabled || !App.Settings.AvalonSettings.MouseWheelScrollReroutesToBackBuffer)
            {
                return;
            }

            // Allowing scrolling down in the main terminal when the back buffer is collapsed.
            if (e.Delta < 0 && GameBackBufferTerminal.Visibility == Visibility.Collapsed)
            {
                e.Handled = false;
                return;
            }

            // Back buffer is collapsed, show it, scroll to the bottom of it.
            if (GameBackBufferTerminal.Visibility == Visibility.Collapsed)
            {
                GameBackBufferTerminal.Visibility = Visibility.Visible;

                // Scroll via the vertical offset, if not done for some reason the first time the window is shown
                // it will be at the top.
                GameBackBufferTerminal.ScrollToLastLine(true);

                // Since the height of this changed scroll it to the bottom.
                GameTerminal.ScrollToLastLine();
            }
            else if (GameBackBufferTerminal.Visibility == Visibility.Visible)
            {
                if (e.Delta > 0)
                {
                    this.GameBackBufferTerminal.PageUp();

                    // This mitigates scrolling up.
                    e.Handled = true;
                }
                else
                {
                    this.GameBackBufferTerminal.PageDown();

                    // Now, if the last line in the back buffer is visible then we can just collapse the
                    // back buffer because the main terminal shows everything at the end.
                    if (GameBackBufferTerminal.IsLastLineVisible())
                    {
                        GameBackBufferTerminal.Visibility = Visibility.Collapsed;
                        TextInput.Editor.Focus();
                    }
                }
            }
        }

        /// <summary>
        /// Mouse wheel scroll handling for the BackBuffer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameBackBufferTerminal_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (GameBackBufferTerminal.Visibility == Visibility.Visible)
            {
                if (e.Delta < 0)
                {
                    // Now, if the last line in the back buffer is visible then we can just collapse the
                    // back buffer because the main terminal shows everything at the end.
                    if (GameBackBufferTerminal.IsLastLineVisible())
                    {
                        GameBackBufferTerminal.Visibility = Visibility.Collapsed;
                        TextInput.Editor.Focus();
                    }
                }
            }
        }

    }
}

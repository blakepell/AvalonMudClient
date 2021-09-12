/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Avalon
{
    /// <summary>
    /// ViewModel for IntroWindow.
    /// </summary>
    public class IntroWindowViewModel : DependencyObject
    {
        public static readonly DependencyProperty ProfilesProperty = DependencyProperty.Register(
            "Profiles", typeof(ObservableCollection<ProfileViewModel>), typeof(IntroWindowViewModel), new PropertyMetadata(default(ObservableCollection<ProfileViewModel>)));

        public ObservableCollection<ProfileViewModel> Profiles
        {
            get => (ObservableCollection<ProfileViewModel>) GetValue(ProfilesProperty);
            set => SetValue(ProfilesProperty, value);
        }

        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            nameof(Version), typeof(string), typeof(IntroWindowViewModel), new PropertyMetadata(default(string)));

        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        public static readonly DependencyProperty SelectedProfileProperty = DependencyProperty.Register(
            nameof(SelectedProfile), typeof(ProfileViewModel), typeof(IntroWindowViewModel), new PropertyMetadata(default(ProfileViewModel)));

        public ProfileViewModel SelectedProfile
        {
            get => (ProfileViewModel) GetValue(SelectedProfileProperty);
            set => SetValue(SelectedProfileProperty, value);
        }

        /// <summary>
        /// A minimal snapshot of existing Profile's for use with binding on the IntroWindow.
        /// </summary>
        public class ProfileViewModel : DependencyObject
        {
            public static readonly DependencyProperty GameAddressProperty = DependencyProperty.Register(
                nameof(GameAddress), typeof(string), typeof(ProfileViewModel), new PropertyMetadata(default(string)));

            public string GameAddress
            {
                get => (string) GetValue(GameAddressProperty);
                set => SetValue(GameAddressProperty, value);
            }

            public static readonly DependencyProperty GamePortProperty = DependencyProperty.Register(
                nameof(GamePort), typeof(int), typeof(ProfileViewModel), new PropertyMetadata(default(int)));

            public int GamePort
            {
                get => (int) GetValue(GamePortProperty);
                set => SetValue(GamePortProperty, value);
            }

            public static readonly DependencyProperty GameDescriptionProperty = DependencyProperty.Register(
                nameof(GameDescription), typeof(string), typeof(ProfileViewModel), new PropertyMetadata(default(string)));

            public string GameDescription
            {
                get => (string)GetValue(GameDescriptionProperty);
                set => SetValue(GameDescriptionProperty, value);
            }

            public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register(
                nameof(Filename), typeof(string), typeof(ProfileViewModel), new PropertyMetadata(default(string)));

            public string Filename
            {
                get => (string) GetValue(FilenameProperty);
                set => SetValue(FilenameProperty, value);
            }

            public static readonly DependencyProperty LastSaveDateProperty = DependencyProperty.Register(
                "LastSaveDate", typeof(DateTime), typeof(ProfileViewModel), new PropertyMetadata(default(DateTime)));

            public DateTime LastSaveDate
            {
                get => (DateTime) GetValue(LastSaveDateProperty);
                set => SetValue(LastSaveDateProperty, value);
            }

            public static readonly DependencyProperty FullPathProperty = DependencyProperty.Register(
                nameof(FullPath), typeof(string), typeof(ProfileViewModel), new PropertyMetadata(default(string)));

            public string FullPath
            {
                get => (string)GetValue(FullPathProperty);
                set => SetValue(FullPathProperty, value);
            }

            public static readonly DependencyProperty ProfileSizeProperty = DependencyProperty.Register(
                nameof(ProfileSize), typeof(string), typeof(ProfileViewModel), new PropertyMetadata(default(string)));

            public string ProfileSize
            {
                get => (string) GetValue(ProfileSizeProperty);
                set => SetValue(ProfileSizeProperty, value);
            }
        }
    }
}
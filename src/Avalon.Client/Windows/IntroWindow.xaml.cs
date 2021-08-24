using System.Collections.ObjectModel;
using System.Windows;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class IntroWindow : Window
    {
        //public ObservableCollection<Profile> Profiles { get; set; }

        public IntroWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            //this.Profiles = new();
            //this.Profiles.Add(new Profile() {GameAddress = "dsl-mud.org", GamePort = 4000, GameName = "Dark and Shattered Lands", CustomDescription = "Immortal" });
            //this.Profiles.Add(new Profile() { GameAddress = "dsl-mud.org", GamePort = 4000, GameName = "Dark and Shattered Lands", CustomDescription = "Mortal" });
            //this.Profiles.Add(new Profile() { GameAddress = "dsl-mud.org", GamePort = 8000, GameName = "Dark and Shattered Lands", CustomDescription = "Immortal" });
        }
    }
}

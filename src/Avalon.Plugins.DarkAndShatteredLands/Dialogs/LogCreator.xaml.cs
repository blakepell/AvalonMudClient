using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace Avalon
{
    /// <summary>
    /// An immortal utility for restrings.
    /// </summary>
    public partial class LogCreatorWindow : Window
    {

        /// <summary>
        /// The text for the status bar.
        /// </summary>
        public string StatusText
        {
            get => TextBlockStatus.Text;
            set => TextBlockStatus.Text = value;
        }

        private StringTransformer LineManager { get; set; }

        /// <summary>
        /// The text in the log editor.
        /// </summary>
        public string Text
        {
            get => LogEditor.Text;
        }

        /// <summary>
        /// The default status bar color.
        /// </summary>
        private SolidColorBrush _defaultStatusColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#007ACC"));

        /// <summary>
        /// A reference to the clients interpreter.
        /// </summary>
        private IInterpreter _interp;

        /// <summary>
        /// A list of the AnsiColors so that we can remove them from the strings for easy viewing.
        /// </summary>
        private List<AnsiColor> _colors = AnsiColors.ToList();

        /// <summary>
        /// An indicator that the text may need to be rebuilt in the LineManager.
        /// </summary>
        private bool _keyPressed = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogCreatorWindow(IInterpreter interp, string text)
        {
            InitializeComponent();
            _interp = interp;
            this.LogEditor.TextArea.TextView.Options.EnableEmailHyperlinks = false;
            this.LogEditor.TextArea.TextView.Options.EnableHyperlinks = false;
            this.LogEditor.TextArea.TextView.Options.ShowBoxForControlCharacters = false;

            // Get rid of the auto indenting.
            this.LogEditor.TextArea.IndentationStrategy = null;

            // Set the font from the client settings.
            this.LogEditor.FontSize = _interp.Conveyor.ClientSettings.TerminalFontSize;

            LineManager = new StringTransformer(text);
            LineManager.RemoveLineIfContains("Password", StringComparison.CurrentCultureIgnoreCase);
            LineManager.Replace("\a", "");
            LogEditor.Text = LineManager.ToString();
        }

        /// <summary>
        /// Event that executes when the window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogCreatorWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Code that is executed for the Cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            SetIdle();
            this.Close();
        }

        /// <summary>
        /// Code that is executed for the Save button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            SetProcessing("Status: Please choose where you would like to save your file.");

            var sfd = new SaveFileDialog
            {
                Title = "Save Log File",
                ValidateNames = true,
                AutoUpgradeEnabled = true,
            };

            sfd.ShowDialog();

            if (string.IsNullOrWhiteSpace(sfd.FileName))
            {
                SetStatus("Status: Log Save Cancelled.");
                return;
            }

            try
            {
                SetProcessing($"Status: Saving to {sfd.FileName}");
                System.IO.File.WriteAllText(sfd.FileName, LogEditor.Text, Encoding.ASCII);
                this.Close();
            }
            catch (Exception ex)
            {
                // On error show them the error but don't close the log.
                SetError($"Error: {ex.Message}");
                return;
            }

            this.Close();
        }

        /// <summary>
        /// Sets the form to error colors.
        /// </summary>
        /// <param name="text"></param>
        public void SetError(string text)
        {
            StatusText = text;
            this.BorderBrush = Brushes.Red;
            TextBlockStatus.Background = Brushes.Red;
            StatusBarWindow.Background = Brushes.Red;
        }

        /// <summary>
        /// Settings the window as processing.
        /// </summary>
        /// <param name="text"></param>
        public void SetProcessing(string text)
        {
            StatusText = text;
            this.BorderBrush = Brushes.Orange;
            TextBlockStatus.Background = Brushes.Orange;
            StatusBarWindow.Background = Brushes.Orange;
        }

        /// <summary>
        /// Resets the color of the form and the text on the status bar to the default.
        /// </summary>
        public void SetIdle()
        {
            StatusText = "Status: Idle";
            this.BorderBrush = _defaultStatusColor;
            TextBlockStatus.Background = _defaultStatusColor;
            StatusBarWindow.Background = _defaultStatusColor;
        }

        /// <summary>
        /// Resets the color of the form and the text on the status bar to the default.
        /// </summary>
        public void SetStatus(string text)
        {
            StatusText = $"Status: {text}";
            this.BorderBrush = _defaultStatusColor;
            TextBlockStatus.Background = _defaultStatusColor;
            StatusBarWindow.Background = _defaultStatusColor;
        }

        /// <summary>
        /// Shared button handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonHandler_Click(object sender, RoutedEventArgs e)
        {
            var fe = e.Source as FrameworkElement;
            string desc = fe.Tag as string;

            // Reset the line manager.
            if (_keyPressed)
            {
                _keyPressed = false;
                LineManager.Lines.Clear();
                LineManager = null;
                LineManager = new StringTransformer(LogEditor.Text);
            }

            // Get any parameters from the user that are needed.
            string argOne = "";
            string argTwo = "";

            switch (desc)
            {
                case "Remove Lines that Contain":
                case "Remove Lines that Start With":
                case "Remove Lines that End With":
                    argOne = await _interp.Conveyor.InputBox("Enter text:", desc);

                    if (string.IsNullOrWhiteSpace(argOne))
                    {
                        return;
                    }

                    break;
                case "Find and Replace":
                    argOne = await _interp.Conveyor.InputBox("Enter to find:", desc);

                    if (string.IsNullOrWhiteSpace(argOne))
                    {
                        return;
                    }

                    argTwo = await _interp.Conveyor.InputBox("Enter replacement text:", desc);

                    if (string.IsNullOrWhiteSpace(argTwo))
                    {
                        return;
                    }

                    break;
            }

            SetProcessing($"Status: Executing {desc}");

            try
            {
                switch (desc)
                {
                    case "Remove Prompts":
                        RemovePrompts();
                        break;
                    case "Remove Channels":
                        RemoveChannels();
                        break;
                    case "Remove Toasts":
                        RemoveToasts();
                        break;
                    case "Remove Double Blank Lines":
                        break;
                    case "Remove Lines that Start With":
                        this.LineManager.RemoveLineIfStartsWith(argOne, StringComparison.OrdinalIgnoreCase);
                        break;
                    case "Remove Lines that End With":
                        this.LineManager.RemoveLineIfEndsWith(argOne, StringComparison.OrdinalIgnoreCase);
                        break;
                    case "Remove Lines that Contain":
                        this.LineManager.RemoveLineIfContains(argOne, StringComparison.OrdinalIgnoreCase);
                        break;
                    case "Remove Notes":
                        this.RemoveNotes();
                        break;
                    case "Remove Battle":
                        RemoveBattle();
                        break;
                    case "Remove Score":
                        RemoveScore();
                        break;
                    case "Remove Affects":
                        RemoveAffects();
                        break;
                    case "Remove Clan Info":
                        RemoveClanInfo();
                        break;
                    case "Remove Single Word Lines":
                        this.LineManager.RemoveLineIfWordCountEquals(1);
                        break;
                    case "Remove Spell Cast Commands":
                        RemoveSpells();
                        break;
                    case "Remove Directions":
                        RemoveDirections();
                        break;
                    case "Find and Replace":
                        this.LineManager.Replace(argOne, argTwo);
                        break;
                    case "RP Log":
                        CreateRpLog();
                        break;
                    case "Remove Maccus":
                        this.LineManager.RemoveLineIfContains("Maccus");
                        break;
                    default:
                        SetError($"Status: {desc} was not found.");
                        return;
                }

                this.LineManager.RemoveDoubleBlankLines();
                this.LogEditor.Text = this.LineManager.ToString();
            }
            catch (Exception ex)
            {
                SetError($"Status: Error - {ex.Message}");
                return;
            }

            SetStatus($"Status: {desc} completed.");
            e.Handled = true;
        }

        /// <summary>
        /// Removes prompts from the output.
        /// </summary>
        public void RemovePrompts()
        {
            this.LineManager.RemoveLineIfRegexMatches(@"\<(\d+)/(\d+)hp (\d+)/(\d+)m (\d+)/(\d+)mv \((\d+)\|(\w+)\) \((.*?)\) \((.*?)\) (.*?) (.*?)\>");
        }

        /// <summary>
        /// Removes all channels.
        /// </summary>
        private void RemoveChannels()
        {
            var list = new string[]
            {
                @"^[\a]?(\[ .* \] )?([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) (\bclan gossip(s?)\b|\bclan(s?)\b|\bgossip(s?)\b|\bask(s?)\b|\banswers(s?)\b|\btell(s?)\b|\bBloodbath(s?)\b|\bpray(s?)\b|\bgrats\b|\bauction(s?)\b|\bquest(s?)\b|\bradio(s?)\b|\bimm(s?)\b).*'",
                @"^[\a]?(\[ .* \] )?(?!.*OOC).*Kingdom: .*",
                @"\((Admin|Coder)\) \(Imm\) [\w'-]+:",
                @"^[\a]?(\(.*\)?)?([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+) (OOC|\[Newbie\]).*",
                @"^\((Shalonesti|OOC Shalonesti|Clave|OOC Clave)\).*"

            };

            this.LineManager.RemoveLineIfRegexMatches(list);
        }

        private void RemoveDirections()
        {
            var list = new string[]
            {
                "u",
                "up",
                "d",
                "down",
                "n",
                "north",
                "s",
                "south",
                "e",
                "east",
                "w",
                "west",
                "northeast",
                "ne",
                "northwest",
                "nw",
                "southeast",
                "se",
                "southwest",
                "southwest",
                "Alas, you cannot go that way."
            };

            this.LineManager.RemoveLineIfEquals(list, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Remove toasts from the output.
        /// </summary>
        private void RemoveToasts()
        {
            this.LineManager.RemoveLineIfRegexMatches(@"^[\a]?([\[\(](.*?)[\]\)])?[ ]{0,}([\w'-]+) got (.*?) by (.*?) ([\[\(] (.*?) [\]\)])?[ ]{0,}([\(]Arena[\)])?");
        }

        /// <summary>
        /// Removes battle echos.
        /// </summary>
        private void RemoveBattle()
        {
            var filterList = new string[]
            {
                "You dodge",
                "You parry",
                "dodges your attack",
                "parries your attack",
                "draws life from",
                "draws energy from",
                "is knocked to the ground",
                "fumes and dissolves",
                "is pitted and etched",
                "is corroded into scrap",
                "corrodes and breaks",
                "is burned into waste",
                "turns blue and shivers",
                "freezes and shatters",
                "is blinded by smoke",
                "Your eyes tear up from smoke",
                "ignites and burns!",
                "bubbles and boils!",
                "crackles and burns!",
                "smokes and chars!",
                "sparks and sputters!",
                "blackens and crisps!",
                "melts and drips!",
                "You feel poison coursing through your veins.",
                "looks very ill.",
                "Your muscles stop responding.",
                "overloads and explodes.",
                "is fused into a worthless lump.",
                "is blinded by the intense heat",
                "You shut your eyes to prevent them from melting",
                "is exhausted by the extreme heat",
                "You are exhausted from the extreme heat",
                "You feel so very tired and sleepy",
                "falls to the floor in a deep sleep.",
                "You feel slow and lethargic",
                "begins to move very slowly.",
                "You are paralyzed",
                "windpipe right out of his throat",
                "is DEAD!!",
                "You have almost completed your QUEST!",
                "Return to the questmaster before your time runs out!",
                "experience points.",
                "coins from the corpse",
                "coins for your sacrifice",
                "death cry.",
                " in half with one mighty swing ",
                " from the corpse of "
            };

            this.LineManager.RemoveLineIfContains(filterList, StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfRegexMatches(@"^(You|Your)(\s)?(.*?)?(miss(es)?|scratch(es)?|graze(s)?|hit(s)?|injure(s)?|wound(s)?|maul(s)?|decimate(s)?|devastate(s)?|maim(s)?|MUTILATE(S)?|DISEMBOWEL(S)?|DISMEMBER(S)?|MASSACRE(S)?|MANGLE(S)?|\*\*\* DEMOLISH(ES)? \*\*\*|\*\*\* DEVASTATES \*\*\*|=== OBLITERATE(S)? ===|>>> ANNIHILATE(S)? <<<|<<< ERADICATE(S)? >>>|(does|do) HIDEOUS things to|(does|do) GHASTLY things to|(does|do) UNSPEAKABLE things to) (.*?)(\.|\!)");
            this.LineManager.RemoveLineIfRegexMatches(@"^(.*?)?(miss(es)?|scratch(es)?|graze(s)?|hit(s)?|injure(s)?|wound(s)?|maul(s)?|decimate(s)?|devastate(s)?|maim(s)?|MUTILATE(S)?|DISEMBOWEL(S)?|DISMEMBER(S)?|MASSACRE(S)?|MANGLE(S)?|\*\*\* DEMOLISH(ES)? \*\*\*|\*\*\* DEVASTATES \*\*\*|=== OBLITERATE(S)? ===|>>> ANNIHILATE(S)? <<<|<<< ERADICATE(S)? >>>|(does|do) HIDEOUS things to|(does|do) GHASTLY things to|(does|do) UNSPEAKABLE things to) (.*?)(\.|\!)");
            this.LineManager.RemoveLineIfRegexMatches(@"^(.*?)? (is in excellent condition|has a few scratches|has some small wounds and bruises|has quite a few wounds|has some big nasty wounds and scratches|looks pretty hurt|is in awful condition|is bleeding to death).");
        }

        private void RemoveSpells()
        {
            var cmds = new string[]
            {
                "c ",
                "cast "
            };

            this.LineManager.RemoveLineIfStartsWith(cmds, StringComparison.OrdinalIgnoreCase);
        }

        public void RemoveLoginScreen()
        {
            this.LineManager.RemoveLineIfStartsWith(@"[ ", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Do you want color", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Original DikuMUD by: Hans Staerfelt, Katja Nyboe, Tom Madsen, Michael", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Seifert, Sebastian Hammer.", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Original MERC 2.1 by Hatchet, Furey, and Kahn.", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"DSL Owned by Allen Games. (which is owned by Scorn)", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"DSL Web Site: http://www.dsl-mud.org", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Various Snippets from SMAUG", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"                                   /   \", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@" _                         )      ((   ))     (", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"(@)                       /|\      ))_((     /|\                         _", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"|-|`\                    / | \    (/\|/\)   / | \                       (@)", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"| |---------------------/--|-voV---\`|'/--Vov-|--\----------------------|-|", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"|-|                          '^`   (o o)  '^`                           | |", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"| |                                `\Y/'                                |-|", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"|-|                                                                     | |", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"|-|                    DARK & SHATTERED LANDS (DSL)                     | |", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"| |                                                                     |-|", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"|-|                           [Implementor]                             | |", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"| |                               Scorn                                 |-|", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"|-|                        (scorn@dsl-mud.org)                          | |", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"|_|_____________________________________________________________________|-|", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"(@)                l   /\ /         ( (       \ /\   l                `\|_|", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"                   l /   V           \ \       V   \ l                  (@)", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"                   l/                _) )_          \I", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"                                     `\ /'                         ", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"                                       ,", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"             Code:  DSL ", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"           Based on ROM ", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"[DSL] (Push Enter to Continue)", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@" ------===+*<==(  Dark and Shattered Lands: Main Login Menu )==>*+===------", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (C)reate a New Character", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (L)imited Race Creation", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (P)lay Existing Character", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (M)aster Account Login     ", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (F)orm a New Master Account", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (W)ho is on now?", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (H)elpfiles", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (Q)uit", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    Your selection? ->", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"(Existing Master Account)", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"What is your Master Account's name?", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Password:", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@" ------===+*<==(  Dark and Shattered Lands: Master Login Menu )==>*+===------", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Master account:", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (C)reate a New Character", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (V)iew Characters and Personal information", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (M)aster Account Password Change", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (E)mail and Personal Information Change", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (A)dd existing Character to Master  System Time", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (R)ewards menu", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"    (Q)uit to Main Menu", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@" ------===+*<==(  Dark and Shattered Lands: Master Character Logon )==>*+===------", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"   You can only log on a character attached to this master account.", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"   To see that list hit enter then from the main menu hit: ", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"   (V) to View Characters and Personal information.", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Player name:", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"*", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"WELCOME TO DARK & SHATTERED LANDS", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"-", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"When approaching a Red Dragon, be sure to bring your wand of marshmallow.", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Welcome to DSL! DSL Loves You! Other muds think you are ugly, they said so!  ", StringComparison.OrdinalIgnoreCase);

            this.LineManager.RemoveLineIfStartsWith(@"whoami", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"score", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"prompt ", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"score", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"You are currently improving ", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfStartsWith(@"Syntax: improve <skillname> / improve none", StringComparison.OrdinalIgnoreCase);

        }

        /// <summary>
        /// Removes the score.
        /// </summary>
        private void RemoveScore()
        {
            string pattern = @"Score for (?<Player>.*?)
----------------------------------------------------------------------------
(.*?)
(?=<(\d+)/(\d+)hp)";

            this.LineManager.ReplaceRegex(pattern, "");
        }

        /// <summary>
        /// Removes notes.
        /// </summary>
        private void RemoveNotes()
        {
            this.LineManager.RemoveLineIfContains("new news article waiting", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("changes waiting to be read", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("change waiting to be read", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("new ooc note waiting", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("new ooc notes waiting", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("new notes waiting", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("new note waiting", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("new quest note waiting", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("new quest notes waiting", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("unread auctions", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("unread auction", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("story notes have been added", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("story note have been added", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("bloodbath notes have been added", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("bloodbath note have been added", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("imm note has been added", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("imm notes have been added", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("prayer has been added", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("prayers have been added", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("nameauth has been added", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("nameauths have been added", StringComparison.OrdinalIgnoreCase);

            string pattern = @"--------------------------------------------------------------------------------
\[(\s?\s?\d+)\] (?<From>.*?): (?<Subject>.*?)
(?<Timestamp>.*?)
To: (?<To>.*?)
--------------------------------------------------------------------------------
(?<NoteText>.*?)
(?=<(\d+)/(\d+)hp)";
            this.LineManager.ReplaceRegex(pattern, "");

        }

        /// <summary>
        /// Removes the cinfo command.
        /// </summary>
        private void RemoveClanInfo()
        {
            string pattern = @"----------------------------------------------------------------------------
Clan Information for: (.*?)
----------------------------------------------------------------------------
(.*?)
Your current war\(s\): (.*?)
----------------------------------------------------------------------------
(.*?)
(?=<(\d+)/(\d+)hp)";

            this.LineManager.ReplaceRegex(pattern, "");
        }

        private void RemoveAffects()
        {
            string pattern = @"You are affected by the following spells:
(.*?)
(?=<(\d+)/(\d+)hp)";
            this.LineManager.ReplaceRegex(pattern, "");
        }

        private void CreateRpLog()
        {
            RemoveNotes();
            RemoveScore();
            RemoveClanInfo();
            RemoveAffects();
            RemoveToasts();
            RemovePrompts();
            RemoveLoginScreen();
            RemoveSpells();
            RemoveBattle();

            this.LineManager.RemoveLineIfWordCountEquals(1);
            this.LineManager.RemoveLineIfStartsWith(@"-->");
            this.LineManager.RemoveLineIfStartsWith(@"[ (C)ontinue, (R)efresh, (B)ack, (H)elp, (E)nd, (T)op, (Q)uit, or RETURN ]");
            this.LineManager.RemoveLineIfStartsWith(@"[Hit Return to continue]");
            this.LineManager.RemoveLineIfStartsWith(@"You have become better at");
            this.LineManager.RemoveLineIfStartsWith(@"You are logged in as:");
            this.LineManager.RemoveLineIfStartsWith(@"<");
            this.LineManager.RemoveLineIfStartsWith(@"You are using:");
            this.LineManager.RemoveLineIfStartsWith(@"You are affected by the following spells:");
            this.LineManager.RemoveLineIfStartsWith(@"You aren't currently on a quest.");
            this.LineManager.RemoveLineIfStartsWith(@"There is less than a hour remaining until you can go on another quest.");
            this.LineManager.RemoveLineIfStartsWith(@"Master account:");
            this.LineManager.RemoveLineIfStartsWith(@"Personal Name: ");
            this.LineManager.RemoveLineIfStartsWith(@"E-mail: ");
            this.LineManager.RemoveLineIfStartsWith(@"Please press enter to get back to the master menu or enter character name to log");
            this.LineManager.RemoveLineIfStartsWith(@"Player name:");
            this.LineManager.RemoveLineIfContains("practice sessions left.", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains("You focus your training on", StringComparison.OrdinalIgnoreCase);
            this.LineManager.RemoveLineIfContains(" : modifies");
            this.LineManager.RemoveLineIfContains("Dark and Shattered Lands: ");

            for (int i = 1; i < 250; i++)
            {
                this.LineManager.RemoveLineIfStartsWith($"{i}. ");
            }
        }

        /// <summary>
        /// Handles the KeyDown event for the LogEditor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            _keyPressed = true;
        }
    }
}

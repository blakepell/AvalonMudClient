using Avalon.Windows.MobProgEditor;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class MobProgEditorWindow : Window
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MobProgEditorWindow()
        {
            InitializeComponent();

            AvalonLuaEditor.TextArea.TextEntering += AvalonLuaEditor_TextEntering;
            AvalonLuaEditor.TextArea.TextEntered += AvalonLuaEditor_TextEntered;
            AvalonLuaEditor.Options.ConvertTabsToSpaces = true;

            this.Title = "Mob Prog";

            var asmMobProg = Assembly.GetExecutingAssembly();
            string resourceNameMobProg = $"{asmMobProg.GetName().Name}.Dialogs.MobProgEditor.MobProgDarkTheme.xshd";

            using var s = asmMobProg.GetManifestResourceStream(resourceNameMobProg);
            using var reader = new XmlTextReader(s);

            AvalonLuaEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }


        /// <summary>
        /// Fires when the Window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StringEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AvalonLuaEditor.Focus();            
        }

        /// <summary>
        /// Code that is executed for the Cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Code that is executed for the Save button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        CompletionWindow _completionWindow;

        void AvalonLuaEditor_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Text was a space.. see if the previous word was a command that has sub commands.
            if (e.Text == " ")
            {
                string word = GetWordBeforeSpace(AvalonLuaEditor);

                if (word == "mob")
                {
                    // Open code completion after the user has pressed dot:
                    _completionWindow = new CompletionWindow(AvalonLuaEditor.TextArea);
                    var data = _completionWindow.CompletionList.CompletionData;

                    data.Add(new MobProgCompletionData("asound", "Usage: mob asound <string>\r\nASOUND prints the text string to the rooms around the mobile in the same manner as a death cry."));
                    data.Add(new MobProgCompletionData("gecho", "Usage: mob gecho <string>\r\nGECHO prints the text string to all players in the game."));
                    data.Add(new MobProgCompletionData("zecho", "Usage: mob zecho <string>\r\nZECHO prints the text string to all players in the same area as the mobile."));
                    data.Add(new MobProgCompletionData("echo", "Usage: mob echo <string>\r\nECHO prints the text string to all players in the same room."));
                    data.Add(new MobProgCompletionData("echoaround", "Usage: mob echoaround <victim> <string>\r\nECHOAROUND displays the string to everyone except the victim."));
                    data.Add(new MobProgCompletionData("echoat", "Usage: mob echoat <victim> <string>\r\nECHOAT displays the string to the victim only."));
                    data.Add(new MobProgCompletionData("kill", "Usage: mob kill <victim>\r\nLets a mobile kill a player without having to murder. Lots of MOBprograms end up with mpkill $n commands floating around. It works on both mobiles and players."));
                    data.Add(new MobProgCompletionData("assist", "Usage: mob assist <victim>\r\nThe mobile will assist another mob or player."));
                    data.Add(new MobProgCompletionData("junk", "Usage: mob junk <object>\r\nDestroys the object refered to in the mobile's inventory. It prints no message to the world and you can do things like junk all.bread or junk all. This is nice for having janitor mobiles clean out their inventory if they are carrying too much (have a MOBprogram trigger on the 'full inventory')"));
                    data.Add(new MobProgCompletionData("mload", "Usage: mload <vnum>\r\nCreates a mob and places it in the same room with the mobile."));
                    data.Add(new MobProgCompletionData("oload", "Usage: oload <vnum> <level> <room|wear>\r\nOLOAD loads the object into the inventory of the mobile. Even if the item is non-takeable, the mobile will receive it in the inventory. This lets a mobile distribute a quest item or load a key or something. The optional 3rd parameter can be specified; 'room' means to load the object to the room, 'wear' means to force the mobile to wear the object loaded (useful for equipping mobiles on the fly). Currently loading items that are level 51 doesnt work."));
                    data.Add(new MobProgCompletionData("purge", "Usage: mob purge <argument>\r\nDestroys the argument from the room of the mobile. Without an argument the result is the cleansing of all NPC's and items from the room with the exception of the mobile itself.  However, mppurge $i will indeedpurge the mobile, but it MUST be the last command the mobile tries todo, otherwise the mud cant reference the acting mobile trying to do thecommands and bad things happen."));
                    data.Add(new MobProgCompletionData("goto", "Usage: mob goto <location>\r\nMoves the mobile to the room or mobile or object requested. It makes no message of its departure or of its entrance, so these must be supplied with echo commands if they are desired."));
                    data.Add(new MobProgCompletionData("at", "Usage: mob at <location> <command>\r\nPerfoms the command at the designated location. Very useful for doing magic slight of hand tricks that leave players dumbfounded.. such as metamorphing mobiles, or guard summoning, or corpse vanishing.  Use a room vnum, and not a target such as $n."));
                    data.Add(new MobProgCompletionData("transfer", "Usage: mob transfer <victim|'all'> <location>\r\nSends the victim to the destination or to the room of the mobile as a default.  if the victim is 'all' then all the characters in the room of the mobile are transferred to the destination.  Good for starting quests or things like that.  There is no message given to the player that it has been transfered and the player doesnt do a look at the new room unless the mob forces them to."));
                    data.Add(new MobProgCompletionData("gtransfer", "Usage: mob gtransfer <victim> <location>\r\nGtransfer works like transfer, except that the group the victim belongs to is transferred with the victim."));
                    data.Add(new MobProgCompletionData("otransfer", "Usage: mob otransfer <object> <location>\r\nOtransfer transfers an object in the room."));
                    data.Add(new MobProgCompletionData("force", "Usage: mob force <victim> <command>\r\nForces the victim to do the designated command. The victim is not told that they are forced, they just do the command so usually some mpecho message is nice.  You can force players to remove belongings and give them to you, etc.  The player sees the normal command messages (such as removing the item and giving it away in the above example)  Again, if the victim is 'all' then everyone in the mobiles room does the command. Gforce works like force except that it affects the group the victim belongs to."));
                    data.Add(new MobProgCompletionData("gforce", "Usage mob gforce <victim> <command>\r\nBehaves like force but affects all members of the group the victim belongs to."));
                    data.Add(new MobProgCompletionData("vforce", "Usage: mob vforce <vnum> <command>\r\nVFORCE affects all mobiles with the specified vnum in the game world.  This is useful for purging a certain type of NPC from the game (by forcing them to purge themselves)."));
                    data.Add(new MobProgCompletionData("cast", "Usage: mob cast <spell> <victim>\r\nLets the mobile to cast spells. Beware, this does only crude validity checking and does not use up any mana. All spells are available regardless of the race or other abilities of the mobile. Casting the spell occurs silently, but spell effects are displayed normally."));
                    data.Add(new MobProgCompletionData("damage", "Usage: mob damage <victim|'all'> <min> <max> <'lethal'>\r\nCauses unconditional damage to the victim. Specifying 'all' as victim causes damage to all characters in the room except the mobile. Min and max parameters define the minimum and maximum amounts of damage caused. By default, the damage is non-lethal, but by supplying the optional 'lethal' parameter, the damage can kill the victim. This command is silent, you must echo all messages yourself in the program. Useful for implementing special attacks for mobiles."));
                    data.Add(new MobProgCompletionData("remember", "Usage: mob remember <victim>\r\nThis command enables the mobile to remember a player for future reference in a MOBprogram. The player can subsequently be referred as '$q' in programs activated by the mobile. MOB FORGET clears the target. Note that if the first time the mobile runs a program, $q is automatically set to the player who triggered the event."));
                    data.Add(new MobProgCompletionData("forget", "Usage: mob forget\r\nThis command clears the player who remembered using 'mob remember'."));
                    data.Add(new MobProgCompletionData("delay", "Usage: mob delay <pulses>\r\nMOB DELAY sets the time in PULSE_MOBILE after which the mobile's  delay trigger is activated. If the mobile has a program defined for delay trigger, the program is executed when the timer expires."));
                    data.Add(new MobProgCompletionData("cancel", "Usage: mob cancel\r\nMOB CANCEL resets the delay timer."));
                    data.Add(new MobProgCompletionData("call", "Usage: mob call <vnum> <victim> <target1> <target2>\r\nThis command lets you call MOBprograms from within a running one, i.e. to call a MOBprogram subroutine. The first parameter is the vnum of the program to execute, the second is the victim's name (for example $n), and the third and fourth are optional object names. All other parameters except vnum can be replaced with word 'null' indicating ignored parameter. MOBprograms can be called recursively, but as a safety measure, parser allows only 5 recursions."));
                    data.Add(new MobProgCompletionData("flee", "Usage: mob flee\r\nCauses a mobile to unconditionally flee from combat. Can be used for example with the hit point percentage trigger to simulate 'wimpy' behavior."));
                    data.Add(new MobProgCompletionData("remove", "Usage: mob remove <victim> <count> <vnum|all>\r\nLets the mobile to strip an object of given vnum from the victim. Objects removed are destroyed. If the vnum is replaced with 'all', the whole inventory of the victim is destroyed. This command is probably most useful for extracting quest items from a player after a quest has been completed.  If count is omitted then it takes all objects of that vnum from the char, so as not to break any mprogs out there."));
                    data.Add(new MobProgCompletionData("nodisp", "Usage: nodisp <victim>\r\nDisables some act messages for mobs set as likeobj."));
                    data.Add(new MobProgCompletionData("echotoship", "Usage: echotoship <ship in room> <message>\r\nEchos a message to a ship if it's in the room."));
                    data.Add(new MobProgCompletionData("immecho", "Usage: immecho <message>\r\nEchos a message to all immortals in the game.  This includes the '-->' prefix."));
                    data.Add(new MobProgCompletionData("timer", "Usage: mob timer <pulses> <program vnum>\r\nMOB TIMER sets the time till the next defined program is activated."));
                    data.Add(new MobProgCompletionData("hunt", "Usage: mob hunt <victim\r\nThis command causes the mob to hunt down the victim in the current area. This is an alias for AHUNT."));
                    data.Add(new MobProgCompletionData("ahunt", "Usage: mob ahunt <victim\r\nThis command causes the mob to hunt down the victim in the current area."));
                    data.Add(new MobProgCompletionData("ghunt", "Usage: mob ghunt <victim\r\nThis command causes the mob to hunt down the victim and attack them anywhere in the world."));
                    data.Add(new MobProgCompletionData("chunt", "Usage: mob chunt <victim\r\nThis command causes the mob to hunt down the victim and attack them anywhere in the continent."));

                    _completionWindow.Show();
                    _completionWindow.Closed += delegate
                    {
                        _completionWindow = null;
                    };
                }
                else if (word == "if" || word == "or" || word == "and")
                {
                    // Open code completion after the user has pressed dot:
                    _completionWindow = new CompletionWindow(AvalonLuaEditor.TextArea);
                    var data = _completionWindow.CompletionList.CompletionData;

                    data.Add(new MobProgCompletionData("rand", "Usage: if rand 50\r\nIs random percentage less than or equal to random number."));
                    data.Add(new MobProgCompletionData("mobhere", "Usage:if mobhere 2500\r\nUsage: if mobhere guard\r\nIs an NPC with this vnum/name in the room."));
                    data.Add(new MobProgCompletionData("objhere", "Usage:if objhere 2500\r\nUsage: if objhere gauntlet\r\nIs an object with this vnum/name in the room."));
                    data.Add(new MobProgCompletionData("mobexists", "Usage: if mobexists thorn keep lord\r\nDoes NPC 'name' exist somewhere in the world."));
                    data.Add(new MobProgCompletionData("objexists", "Usage: if objexists keep key\r\nDoes object 'name' exist somewhere in the world."));
                    data.Add(new MobProgCompletionData("people", "Usage: if people == 1\r\nIs the number of people in the room equal to integer."));
                    data.Add(new MobProgCompletionData("players", "Usage: if people == 1\r\nIs the number of PCs in the room equal to integer."));
                    data.Add(new MobProgCompletionData("mobs", "Usage: if mobs == 1\r\nIs the number of NPCs in the room equal to integer."));
                    data.Add(new MobProgCompletionData("clones", "Usage: if clones == 1\r\nIs the number of NPCs in the room with the same vnum as the NPC who activated the program equal to integer."));
                    data.Add(new MobProgCompletionData("order", "Usage: if order == 1\r\nIs the order (of several similar NPCs) of the NPC who activated the trigger equal to integer."));
                    data.Add(new MobProgCompletionData("hour", "Usage: if hour == 1\r\nIs the hour/game time equal to integer."));
                    data.Add(new MobProgCompletionData("ispc", "Usage: if ispc $n\r\nIs the $* a PC"));
                    data.Add(new MobProgCompletionData("isnpc", "Usage: if isnpc $n\r\nIs the $* an NPC"));
                    data.Add(new MobProgCompletionData("isgood", "Usage: if isgood $n\r\nIs the $* of a good alignment."));
                    data.Add(new MobProgCompletionData("isevil", "Usage: if isevil $n\r\nIs the $* of an evil alignment."));
                    data.Add(new MobProgCompletionData("isneutral", "Usage: if isneutral $n\r\nIs the $* of a neutral alignment."));
                    data.Add(new MobProgCompletionData("isimmort", "Usage: if isimmort $n\r\nIs the $* an immortal."));
                    data.Add(new MobProgCompletionData("ischarm", "Usage: if ischarm $n\r\nIs $* affected by charm."));
                    data.Add(new MobProgCompletionData("isfollow", "Usage: if isfollow $n\r\nIs $* a follower with their master in the room."));
                    data.Add(new MobProgCompletionData("isactive", "Usage: if isactive $n\r\nIs $*'s position > POS_SLEEPING"));
                    data.Add(new MobProgCompletionData("isdelay", "Usage: if isdelay $n\r\nDoes $* have a delayed mob prog pending."));
                    data.Add(new MobProgCompletionData("isvisible", "Usage: if isvisible $n\r\nIs $* visible to NPC who activated the program."));
                    data.Add(new MobProgCompletionData("hastarget", "Usage: if hastarget $n\r\nDoes $* have a mob prog target in the room."));
                    data.Add(new MobProgCompletionData("istarget", "Usage: if istarget $n\r\nIs $* the target of NPC who activated the program."));
                    data.Add(new MobProgCompletionData("exists", "Usage: if exists $n\r\nDoes $* exist somewhere in the world."));
                    data.Add(new MobProgCompletionData("affected", "Usage: if affected $n deafen\r\nIf $* affected by a specific affect."));
                    data.Add(new MobProgCompletionData("act", "Usage: if act $n <act flag>\r\nIf $* is affected by an act flag."));
                    data.Add(new MobProgCompletionData("off", "Usage: if off $n <offensive flag>\r\nIf $* is affected by an offensive flag."));
                    data.Add(new MobProgCompletionData("imm", "Usage: if act $n <immune flag>\r\nIf $* is affected by an immune flag."));
                    data.Add(new MobProgCompletionData("carries", "Usage: if carries $n <vnum|object name>\r\nDoes $* have an object."));
                    data.Add(new MobProgCompletionData("wears", "Usage: if wears $n <vnum|object name>\r\nDoes $* wear an object."));
                    data.Add(new MobProgCompletionData("has", "Usage: if has $n <type>\r\nDoes $* have an object type (of something like weapon)."));
                    data.Add(new MobProgCompletionData("uses", "Usage: if uses $n <type>\r\nDoes $* wearing obj type (of something like armor)."));
                    data.Add(new MobProgCompletionData("name", "Usage: if name $n <player name>\r\nIs $*'s name a specified player name."));
                    data.Add(new MobProgCompletionData("pos", "Usage: if pos $n <position>\r\nIs $*'s position a value."));
                    data.Add(new MobProgCompletionData("clan", "Usage: if clan $n <clan name>\r\nIs $* in the specified clan."));
                    data.Add(new MobProgCompletionData("race", "Usage: if race $n <race>\r\nIs $* in the specified clan."));
                    data.Add(new MobProgCompletionData("class", "Usage: if class $n <class>\r\nIs $* a specified class."));
                    data.Add(new MobProgCompletionData("objtype", "Usage: if objtype $p scroll\r\nIs $p a scroll."));
                    data.Add(new MobProgCompletionData("vnum", "Usage: if $i == 1200\r\nIs $* equal to the specified vnum."));
                    data.Add(new MobProgCompletionData("hpcnt", "Usage: if hpcnt $i > 30\r\nHit point percent check."));
                    data.Add(new MobProgCompletionData("room", "Usage: if room $i == 1233\r\nIs the room a specified vnum."));
                    data.Add(new MobProgCompletionData("sex", "Usage: if sex $n == <sex: 0=neutral, 1=male, 2=female>\r\nIs $* a given gender."));
                    data.Add(new MobProgCompletionData("level", "Usage: if level $n == 51\r\nIs $*'s level equal to integer."));
                    data.Add(new MobProgCompletionData("align", "Usage: if align $n == <alignment: 1=evil, 2=neutral, 3=good>\r\nIs $* of a specified alignment."));
                    data.Add(new MobProgCompletionData("money", "Usage: if money $n > 5000\r\nDoes $* have money (in silver) equal to integer."));
                    data.Add(new MobProgCompletionData("objval0", "Usage: if objval0 > 1000\r\nIf objval0 is a given value."));
                    data.Add(new MobProgCompletionData("objval1", "Usage: if objval1 > 1000\r\nIf objval1 is a given value."));
                    data.Add(new MobProgCompletionData("objval2", "Usage: if objval2 > 1000\r\nIf objval2 is a given value."));
                    data.Add(new MobProgCompletionData("objval3", "Usage: if objval3 > 1000\r\nIf objval3 is a given value."));
                    data.Add(new MobProgCompletionData("objval4", "Usage: if objval4 > 1000\r\nIf objval4 is a given value."));
                    data.Add(new MobProgCompletionData("grpsize", "Usage: if grpsize $n > 6\r\nIf $*'s group size is a given value."));
                    data.Add(new MobProgCompletionData("ismounted", "Usage: if ismounted $n\r\nIs $* riding a mount."));
                    data.Add(new MobProgCompletionData("comm", "Usage: if comm $i NOSHOUT\r\nIf $* has a given comm value."));
                    data.Add(new MobProgCompletionData("isship", "Usage: if isship $n\r\nIf $* is a ship."));
                    data.Add(new MobProgCompletionData("rank", "Usage: if act $n <rank>\r\nIf $* is ranked with the exact name of a rank."));
                    data.Add(new MobProgCompletionData("sunlight", "Usage: if sunlight <number, 0=dark, 1=sunrise, 2=day>\r\nThe position of the sun.\r\n"));

                    _completionWindow.Show();
                    _completionWindow.Closed += delegate
                    {
                        _completionWindow = null;
                    };
                }
                else
                {
                    return;
                }
            }
            if (e.Text == "$")
            {
                // Open code completion after the user has pressed dot:
                _completionWindow = new CompletionWindow(AvalonLuaEditor.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;

                // Trigger Variables
                data.Add(new MobProgCompletionData("i", "The first of the names of the mobile itself.", "$"));
                data.Add(new MobProgCompletionData("I", "The short description of the mobile itself.", "$"));
                data.Add(new MobProgCompletionData("n", "The name of whomever caused the trigger to happen.", "$"));
                data.Add(new MobProgCompletionData("N", "The name and title of whomever caused the trigger to happen.", "$"));
                data.Add(new MobProgCompletionData("t", "The name of a secondary character target (i.e A smiles at B)", "$"));
                data.Add(new MobProgCompletionData("T", "The short description, or name and title of target (NPC vs PC)", "$"));
                data.Add(new MobProgCompletionData("r", "The name of a random PC in the room with the mobile", "$"));
                data.Add(new MobProgCompletionData("R", "The short description, or name and title of the random PC", "$"));
                data.Add(new MobProgCompletionData("q", "The name of the MOBprogram target (see MOB REMEMBER)", "$"));
                data.Add(new MobProgCompletionData("Q", "The short description of the MOBprogram target", "$"));
                data.Add(new MobProgCompletionData("j", "he, she, it based on sex of $i.", "$"));
                data.Add(new MobProgCompletionData("e", "he, she, it based on sex of $n.", "$"));
                data.Add(new MobProgCompletionData("E", "he, she, it based on sex of $t.", "$"));
                data.Add(new MobProgCompletionData("J", "he, she, it based on sex of $r.", "$"));
                data.Add(new MobProgCompletionData("k", "him, her, it based on sex of $i.", "$"));
                data.Add(new MobProgCompletionData("m", "him, her, it based on sex of $n.", "$"));
                data.Add(new MobProgCompletionData("M", "him, her, it based on sex of $t.", "$"));
                data.Add(new MobProgCompletionData("K", "him, her, it based on sex of $r.", "$"));
                data.Add(new MobProgCompletionData("l", "his, hers, its based on sex of $i.", "$"));
                data.Add(new MobProgCompletionData("s", "his, hers, its based on sex of $n.", "$"));
                data.Add(new MobProgCompletionData("S", "his, hers, its based on sex of $t.", "$"));
                data.Add(new MobProgCompletionData("L", "his, hers, its based on sex of $r.", "$"));
                data.Add(new MobProgCompletionData("o", "the first of the names of the primary object (i.e A drops B)", "$"));
                data.Add(new MobProgCompletionData("O", "the short description of the primary object", "$"));
                data.Add(new MobProgCompletionData("p", "the first of the names of the secondary object (i.e A puts B in C)", "$"));
                data.Add(new MobProgCompletionData("P", "the short description of the secondary object", "$"));

                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }
        }

        public static string GetWordBeforeSpace(TextEditor textEditor)
        {
            var wordBeforeDot = string.Empty;
            var caretPosition = textEditor.CaretOffset - 2;
            var lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(caretPosition));
            string text = textEditor.Document.GetText(lineOffset, 1);

            while (true)
            {
                if (text == null && text.CompareTo(" ") > 0)
                {
                    break;
                }
                if (Regex.IsMatch(text, @".*[^A-Za-z\. ]"))
                {
                    break;
                }

                if (text != " ")
                {
                    wordBeforeDot = text + wordBeforeDot;
                }

                if (caretPosition == 0)
                {
                    break;
                }

                lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(--caretPosition));

                text = textEditor.Document.GetText(lineOffset, 1);
            }

            return wordBeforeDot;
        }

        void AvalonLuaEditor_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        /// <summary>
        /// Gets or sets the text of the action button.
        /// </summary>
        public string ActionButtonText
        {
            get
            {
                return ButtonSave.Content.ToString();
            }
            set
            {
                ButtonSave.Content = value;
            }
        }

        /// <summary>
        /// The value of the Lua text editor.
        /// </summary>
        public string Text
        {
            get => AvalonLuaEditor.Text;
            set => AvalonLuaEditor.Text = value;
        }

        /// <summary>
        /// The text for the status bar.
        /// </summary>
        public string StatusText
        {
            get => TextBlockStatus.Text;
            set => TextBlockStatus.Text = value;
        }

        /// <summary>
        /// Opens a file from the file system.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Open File",
                ValidateNames = true,
                AutoUpgradeEnabled = true,
                Multiselect = false
            };

            ofd.ShowDialog();

            if (string.IsNullOrWhiteSpace(ofd.FileName))
            {
                return;
            }

            try
            {
                AvalonLuaEditor.Text = System.IO.File.ReadAllText(ofd.FileName, System.Text.Encoding.ASCII);
                this.StatusText = ofd.SafeFileName;
            }
            catch (Exception ex)
            {
                this.StatusText = $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Saves a file to the file system.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSaveFile_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Title = "Save File",
                ValidateNames = true,
                AutoUpgradeEnabled = true,
            };

            sfd.ShowDialog();

            if (string.IsNullOrWhiteSpace(sfd.FileName))
            {
                return;
            }

            try
            {
                System.IO.File.WriteAllText(sfd.FileName, AvalonLuaEditor.Text, System.Text.Encoding.ASCII);
            }
            catch (Exception ex)
            {
                this.StatusText = $"Error: {ex.Message}";
            }
        }
    }
}
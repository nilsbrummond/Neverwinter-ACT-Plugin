using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net;


[assembly: AssemblyTitle("Neverwinter Parsing Plugin")]
[assembly: AssemblyDescription("A basic parser that reads the combat logs in Neverwinter.")]
[assembly: AssemblyCopyright("nils.brummond@gmail.com based on: Antday <Unique> based on STO Plugin from Hilbert@mancom, Pirye@ucalegon")]
[assembly: AssemblyVersion("1.2.2.0")]


/* Version History - npb
 * 1.2.2.0 - 2014/01/10
 *  - Minor code cleanup
 *  - Filtered out all injuries from showing in outgoing damage.
 * 1.2.1.0 - 2013/10/13
 *  - Fix healing keeping encounters from ending.
 *  - Filtered out minor injuries from showing up.
 * 1.2.0.0 - 2013/10/2
 *  - Handle round off error for small numbers better.  ACT int damage vs NW floating point damage issues.
 *    Damage rounding down to zero has some odd effects.
 *  - Better startup to have clean plugin startup so to avoid plugin failure to load.
 * 1.1.0.0 - 2013/9/29
 *  - Improvements to shield tracking.  Matches shield to damage and adds extra info.
 *  - Fix to prevent plugin to fail to load.
 * 1.0.0.0 - 2013/9/26
 *  - Fixes to shield tracking.
 *  - Fixes to Chaotic Growth tracking.
 *  - Looks good for a 1.0 release at this point.
 * 0.0.9.1 - 2013/8/10
 *  - Change the fixed point damage so the graphs will not be off by 10x.  Round-offs errors are increased.
 *  - Keep exact damage number as well for direct display.
 * 0.0.9.0 - 2013/8/08
 *  - Added options to add player character names to help detect the player and allies.
 * 0.0.8.0 - 2013/7/20
 *  - Reworked the processing model.  Less special cases.  Better support.
 * 0.0.7.0 - 2013/7/16
 *  - Added Flank as a column type
 *  - Added Effectiveness column ( actual damage / base damage )
 * 0.0.6.0 - 2013/7/9
 *  - Combat log color coding
 *  - Pet name hash tables
 *  - encounter and unit clean names
 *  - pets filter as owner for selective parsing
 *  - Evaluating some of the delay parsing to see if needed
 *  - other minor improvements
 *  - improved combat start detection
 *  - Chaotic Growth tracking
 */

/* Version History - Antday <Unique>
 * 0.0.5.1 - 2013/04/24
 *   - add Neverwinter options to ACT option panel
 * 0.0.4.6 - 2013/04/21
 *   - actions assigned to attacker; some actions not yet assignable -> Unknown attacker
 * 0.0.3.0 - 2013/04/18
 *   - handle "Cleanse" (Internal Event = Pn.Vklp251) from Cleric as non damage
 * 0.0.2.0 - 2013/04/11
 *   - cleanup some things from orig STO Parser
 * 0.0.1.0 - 2013/04/05
 *   - initial alpha version
*/


namespace NWParsing_Plugin
{
    public class NW_Parser : UserControl, IActPluginV1
    {

        #region Designer Created Code (Avoid editing)

        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_mergeNPC = new System.Windows.Forms.CheckBox();
            this.checkBox_mergePets = new System.Windows.Forms.CheckBox();
            this.checkBox_flankSkill = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_clearAll = new System.Windows.Forms.Button();
            this.button_remove = new System.Windows.Forms.Button();
            this.button_add = new System.Windows.Forms.Button();
            this.textBox_player = new System.Windows.Forms.TextBox();
            this.listBox_players = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Neverwinter parser plugin Options";
            // 
            // checkBox_mergeNPC
            // 
            this.checkBox_mergeNPC.AutoSize = true;
            this.checkBox_mergeNPC.Location = new System.Drawing.Point(6, 21);
            this.checkBox_mergeNPC.Name = "checkBox_mergeNPC";
            this.checkBox_mergeNPC.Size = new System.Drawing.Size(291, 17);
            this.checkBox_mergeNPC.TabIndex = 2;
            this.checkBox_mergeNPC.Text = "Merge all NPC combatants by removing NPC unique IDs";
            this.checkBox_mergeNPC.UseVisualStyleBackColor = true;
            this.checkBox_mergeNPC.MouseEnter += new System.EventHandler(this.checkBox_mergeNPC_MouseEnter);
            this.checkBox_mergeNPC.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // checkBox_mergePets
            // 
            this.checkBox_mergePets.AutoSize = true;
            this.checkBox_mergePets.Location = new System.Drawing.Point(6, 44);
            this.checkBox_mergePets.Name = "checkBox_mergePets";
            this.checkBox_mergePets.Size = new System.Drawing.Size(284, 17);
            this.checkBox_mergePets.TabIndex = 3;
            this.checkBox_mergePets.Text = "Merge all pet data to owner and remove pet from listing";
            this.checkBox_mergePets.UseVisualStyleBackColor = true;
            this.checkBox_mergePets.MouseEnter += new System.EventHandler(this.checkBox_mergePets_MouseEnter);
            this.checkBox_mergePets.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // checkBox_flankSkill
            // 
            this.checkBox_flankSkill.AutoSize = true;
            this.checkBox_flankSkill.Location = new System.Drawing.Point(6, 67);
            this.checkBox_flankSkill.Name = "checkBox_flankSkill";
            this.checkBox_flankSkill.Size = new System.Drawing.Size(213, 17);
            this.checkBox_flankSkill.TabIndex = 4;
            this.checkBox_flankSkill.Text = "Split skills in to flank and non-flank skills";
            this.checkBox_flankSkill.UseVisualStyleBackColor = true;
            this.checkBox_flankSkill.MouseEnter += new System.EventHandler(this.checkBox_flankSkill_MouseEnter);
            this.checkBox_flankSkill.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.button_clearAll);
            this.groupBox1.Controls.Add(this.button_remove);
            this.groupBox1.Controls.Add(this.button_add);
            this.groupBox1.Controls.Add(this.textBox_player);
            this.groupBox1.Controls.Add(this.listBox_players);
            this.groupBox1.Location = new System.Drawing.Point(15, 147);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(362, 188);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Player Detection";
            this.groupBox1.MouseEnter += new System.EventHandler(this.playerNameControls_MouseEnter);
            this.groupBox1.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(184, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Add / Remove Player";
            this.label2.MouseEnter += new System.EventHandler(this.playerNameControls_MouseEnter);
            this.label2.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // button_clearAll
            // 
            this.button_clearAll.Location = new System.Drawing.Point(296, 64);
            this.button_clearAll.Name = "button_clearAll";
            this.button_clearAll.Size = new System.Drawing.Size(60, 23);
            this.button_clearAll.TabIndex = 9;
            this.button_clearAll.Text = "Clear All";
            this.button_clearAll.UseVisualStyleBackColor = true;
            this.button_clearAll.Click += new System.EventHandler(this.button_clearAll_Click);
            this.button_clearAll.MouseEnter += new System.EventHandler(this.playerNameControls_MouseEnter);
            this.button_clearAll.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // button_remove
            // 
            this.button_remove.Location = new System.Drawing.Point(225, 64);
            this.button_remove.Name = "button_remove";
            this.button_remove.Size = new System.Drawing.Size(65, 23);
            this.button_remove.TabIndex = 8;
            this.button_remove.Text = "Remove";
            this.button_remove.UseVisualStyleBackColor = true;
            this.button_remove.Click += new System.EventHandler(this.button_remove_Click);
            this.button_remove.MouseEnter += new System.EventHandler(this.playerNameControls_MouseEnter);
            this.button_remove.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // button_add
            // 
            this.button_add.Location = new System.Drawing.Point(185, 64);
            this.button_add.Name = "button_add";
            this.button_add.Size = new System.Drawing.Size(34, 23);
            this.button_add.TabIndex = 7;
            this.button_add.Text = "Add";
            this.button_add.UseVisualStyleBackColor = true;
            this.button_add.Click += new System.EventHandler(this.button_add_Click);
            this.button_add.MouseEnter += new System.EventHandler(this.playerNameControls_MouseEnter);
            this.button_add.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // textBox_player
            // 
            this.textBox_player.Location = new System.Drawing.Point(185, 38);
            this.textBox_player.Name = "textBox_player";
            this.textBox_player.Size = new System.Drawing.Size(171, 20);
            this.textBox_player.TabIndex = 6;
            this.textBox_player.TextChanged += new System.EventHandler(this.textBox_player_TextChanged);
            this.textBox_player.MouseEnter += new System.EventHandler(this.playerNameControls_MouseEnter);
            this.textBox_player.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // listBox_players
            // 
            this.listBox_players.FormattingEnabled = true;
            this.listBox_players.Location = new System.Drawing.Point(6, 19);
            this.listBox_players.Name = "listBox_players";
            this.listBox_players.Size = new System.Drawing.Size(172, 160);
            this.listBox_players.TabIndex = 5;
            this.listBox_players.SelectedIndexChanged += new System.EventHandler(this.listBox_players_SelectedIndexChanged);
            this.listBox_players.MouseEnter += new System.EventHandler(this.playerNameControls_MouseEnter);
            this.listBox_players.MouseLeave += new System.EventHandler(this.control_MouseLeave);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox_mergeNPC);
            this.groupBox2.Controls.Add(this.checkBox_mergePets);
            this.groupBox2.Controls.Add(this.checkBox_flankSkill);
            this.groupBox2.Location = new System.Drawing.Point(15, 41);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(362, 100);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // NW_Parser
            // 
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Name = "NW_Parser";
            this.Size = new System.Drawing.Size(399, 380);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_mergeNPC;
        private System.Windows.Forms.CheckBox checkBox_mergePets;
        private System.Windows.Forms.CheckBox checkBox_flankSkill;
        private GroupBox groupBox1;
        private TextBox textBox_player;
        private ListBox listBox_players;
        private GroupBox groupBox2;
        private Button button_clearAll;
        private Button button_remove;
        private Button button_add;
        private Label label2;

        #endregion

        public NW_Parser()
        {
            InitializeComponent();
        }

        internal static string[] separatorLog = new string[] { "::", "," };

        // NOTE: The values of "Unknown" and "UNKNOWN" short-circuit the ally determination code.  Must use one of these two names.
        //       Information from EQAditu.
        internal static string unk = "UNKNOWN", unkInt = "C[0 Unknown]", pet = "<PET> ", unkAbility = "Unknown Ability";

        internal static CultureInfo cultureLog = new CultureInfo("en-US");

        // This is for SQL syntax; do not change
        internal static CultureInfo usCulture = new CultureInfo("en-US");

        private PetOwnerRegistery petOwnerRegistery = new PetOwnerRegistery();
        private EntityOwnerRegistery entityOwnerRegistery = new EntityOwnerRegistery();

        private UnmatchedShieldLines unmatchedShieldLines = new UnmatchedShieldLines();

        // For tracking source of Chaotic Growth heals.
        private Dictionary<string, ChaoticGrowthInfo> magicMissileLastHit = new Dictionary<string, ChaoticGrowthInfo>();

        private Dictionary<string, bool> playerCharacterNames = new Dictionary<string, bool>();
        private bool playersCharacterFound = false;

        private static readonly Dictionary<string, bool> injuryTypes = new Dictionary<string, bool>()
        {
            // Minor Body Injury
            // 13:10:13:14:39:55.0::,,,,Lady Shiva,P[401878470@8454371 Lady Shiva@pombetitha],Minor Body Injury,Pn.Snckuc1,HitPointsMax,ShowPowerDisplayName,0.3,0
            {"Pn.Snckuc1", true},

            // Minor Head Injury
            // 13:12:26:13:02:18.0::Pelaios Babausong,P[501470872@9346055 Pelaios Babausong@defacto12232],,*,,*,Minor Head Injury,Pn.3t0w251,PowerMode,ShowPowerDisplayName,0,0
            {"Pn.3t0w251", true},

            // Minor Arm Injury
            // 13:12:26:13:25:23.9::Dragonfly,P[501350239@7290941 Dragonfly@traenenengel],,*,,*,Minor Arm Injury,Pn.Wuki8e1,PowerMode,ShowPowerDisplayName,0,0
            {"Pn.Wuki8e1", true},

            // Minor Leg Injury
            // 14:01:01:10:28:01.7::speedflash,P[201405998@7734429 speedflash@speedflash1911],,*,,*,Minor Leg Injury,Pn.Kxil0c1,PowerMode,ShowPowerDisplayName,0,0
            {"Pn.Kxil0c1", true},

            // Severe Body Injury
            // 14:01:01:10:44:20.5::Drakon,P[500426320@6327016 Drakon@larrybusby],,*,,*,Severe Body Injury,Pn.Fmwcu5,PowerMode,ShowPowerDisplayName,0,0
            {"Pn.Fmwcu5", true},

            // Severe Head Injury
            // 14:01:01:10:54:10.4::Drakon,P[500426320@6327016 Drakon@larrybusby],,*,,*,Severe Head Injury,Pn.An2r3x1,PowerMode,ShowPowerDisplayName,0,0
            {"Pn.An2r3x1", true},

            // Severe Arms Injury
            // 14:01:01:11:56:14.8::Drakon,P[500426320@6327016 Drakon@larrybusby],,*,,*,Severe Arms Injury,Pn.3t5b87,PowerMode,ShowPowerDisplayName,0,0
            {"Pn.3t5b87", true},

            // Severe Legs Injury
            // 14:01:01:12:03:33.6::Lodur,P[201093074@7545190 Lodur@lodur42],,*,,*,Severe Legs Injury,Pn.Va2e05,PowerMode,ShowPowerDisplayName,0,0
            {"Pn.Va2e05", true},
        };

        // Instant when the current combat action took place
        private DateTime curActionTime = DateTime.MinValue;

        Label lblStatus = null;

        TreeNode optionsNode = null;

        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "neverwinter.config.xml");
        SettingsSerializer xmlSettings;

        private int parsedLineCount = 0;
        private int errorLineCount = 0;

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            // Push the option screen into the option tab
            int dcIndex = -1;
            for (int i = 0; i < ActGlobals.oFormActMain.OptionsTreeView.Nodes.Count; i++)
            {
                if (ActGlobals.oFormActMain.OptionsTreeView.Nodes[i].Text == "Neverwinter")
                    dcIndex = i;
            }
            if (dcIndex != -1)
            {
                optionsNode = ActGlobals.oFormActMain.OptionsTreeView.Nodes[dcIndex].Nodes.Add("General");
                ActGlobals.oFormActMain.OptionsControlSets.Add(@"Neverwinter\General", new List<Control> { this });
                Label lblConfig = new Label();
                lblConfig.AutoSize = true;
                lblConfig.Text = "Find the applicable options in the Options tab, Neverwinter section.";
                pluginScreenSpace.Controls.Add(lblConfig);
            }
            else
            {
                ActGlobals.oFormActMain.OptionsTreeView.Nodes.Add("Neverwinter");
                dcIndex = ActGlobals.oFormActMain.OptionsTreeView.Nodes.Count - 1;
                optionsNode = ActGlobals.oFormActMain.OptionsTreeView.Nodes[dcIndex].Nodes.Add("General");
                ActGlobals.oFormActMain.OptionsControlSets.Add(@"Neverwinter\General", new List<Control> { this });
                Label lblConfig = new Label();
                lblConfig.AutoSize = true;
                lblConfig.Text = "Find the applicable options in the Options tab, Neverwinter section.";
                pluginScreenSpace.Controls.Add(lblConfig);
            }
            ActGlobals.oFormActMain.OptionsTreeView.Nodes[dcIndex].Expand();

            // Neverwinter settings file
            xmlSettings = new SettingsSerializer(this);
            LoadSettings();

            // Setting this Regex will allow ACT to extract the character's name from the file name as the first capture group
            // when opening new log files. We'll say the log file name may look like "20080706-Player.log"
            ActGlobals.oFormActMain.LogPathHasCharName = false;

            // A windows file system filter to search updated log files with.
            ActGlobals.oFormActMain.LogFileFilter = "Combat*.log";

            // If all log files are in a single folder, this isn't an issue. If log files are split into different folders,
            // enter the parent folder name here. This way ACT will monitor that base folder and all sub-folders for updated files.
            ActGlobals.oFormActMain.LogFileParentFolderName = "GameClient";

            // Then to apply the settings and restart the log checking thread
            try
            {
                ActGlobals.oFormActMain.ResetCheckLogs();
            }
            catch
            {
                // Ignore when no log file is currently open
            }

            // This is the absolute path of where you wish ACT generated macro exports to be put. I'll leave it up to you
            // to determine this path programatically. If left blank, ACT will attempt to find EQ2 by registry or log file parents.
            // ActGlobals.oFormActMain.GameMacroFolder = @"C:\Program Files\Game Company\Game Folder";

            // Lets say that the log file time stamp is like: "[13:42:57]"
            // ACT needs to know the length of the timestamp and spacing at the beginning of the log line
            ActGlobals.oFormActMain.TimeStampLen = 19; // Remember to include spaces after the time stamp

            // Replace ACT's default DateTime parser with your own implementation matching your format
            ActGlobals.oFormActMain.GetDateTimeFromLog = new FormActMain.DateTimeLogParser(ParseDateTime);

            // This Regex is only used by a quick parsing method to find the current zone name based on a file position
            // If you do not define this correctly, the quick parser will fail and take a while to do so.
            // You still need to implement a zone change parser in your engine regardless of this
            // ActGlobals.oFormActMain.ZoneChangeRegex = new Regex(@"You have entered: (.+)\.", RegexOptions.Compiled);

            // All of your parsing engine will be based off of this event
            // You should not use Before/AfterCombatAction as you may enter infinite loops. AfterLogLineRead is okay, but not recommended
            ActGlobals.oFormActMain.BeforeLogLineRead += new LogLineEventDelegate(oFormActMain_BeforeLogLineRead);

            // Hooks for periodic pet cache purge
            ActGlobals.oFormActMain.OnCombatEnd += new CombatToggleEventDelegate(oFormActMain_OnCombatEnd);
            ActGlobals.oFormActMain.LogFileChanged += new LogFileChangedDelegate(oFormActMain_LogFileChanged);

            FixupCombatDataStructures();

            // Set status text to successfully loaded
            lblStatus = pluginStatusText;
            lblStatus.Text = "Neverwinter ACT plugin loaded";
        }

        
        
        private string GetIntCommas()
        {
            return ActGlobals.mainTableShowCommas ? "#,0" : "0";
        }

        private string GetFloatCommas()
        {
            return ActGlobals.mainTableShowCommas ? "#,0.00" : "0.00";
        }

        private string GetCellDataFlank(MasterSwing Data)
        {
            object val;
            bool flank = false;

            if (Data.Tags.TryGetValue("Flank", out val))
            {
                flank = (bool)val;
            }

            return flank.ToString();
        }

        private string GetSqlDataFlank(MasterSwing Data)
        {
            object val;
            bool flank = false;

            if (Data.Tags.TryGetValue("Flank", out val))
            {
                flank = (bool)val;
            }

            return flank.ToString(usCulture)[0].ToString();
        }

        private int MasterSwingCompareFlank(MasterSwing Left, MasterSwing Right)
        {
            object val;
            bool leftFlank = false;
            bool rightFlank = false;

            if (Left.Tags.TryGetValue("Flank", out val))
            {
                leftFlank = (bool)val;
            }

            if (Right.Tags.TryGetValue("Flank", out val))
            {
                rightFlank = (bool)val;
            }

            return leftFlank.CompareTo(rightFlank);
        }

        private string GetCellDataBaseDamage(MasterSwing Data)
        {
            object damBaseObj;

            if (Data.Tags.TryGetValue("BaseDamage", out damBaseObj))
            {
                float d = (float)damBaseObj;
                if (d == 0) return "";
                return d.ToString("F1");
            }

            return "";
        }

        private string GetSqlDataBaseDamage(MasterSwing Data)
        {
            object damBaseObj;

            if (Data.Tags.TryGetValue("BaseDamage", out damBaseObj))
            {
                float d = (float)damBaseObj;
                return d.ToString();
            }

            return "0";
        }

        private int MasterSwingCompareBaseDamage(MasterSwing Left, MasterSwing Right)
        {
            object l;
            object r;

            bool lvalid = Left.Tags.TryGetValue("BaseDamage", out l);
            bool rvalid = Right.Tags.TryGetValue("BaseDamage", out r);

            if (lvalid && rvalid)
            {
                float dl = (float)l;
                float dr = (float)r;

                return dl.CompareTo(dr);
            }
            else
            {
                if (lvalid) { return 1; }
                else if (rvalid) { return -1; }
                else { return 0; }
            }
        }

        private string GetCellDataEffectiveness(MasterSwing Data)
        {
            object effObj;

            if (Data.Tags.TryGetValue("Effectiveness", out effObj))
            {
                float d = (float)effObj;
                return d.ToString("P1");
            }

            return "";
        }

        private string GetSqlDataEffectiveness(MasterSwing Data)
        {
            object effObj;

            if (Data.Tags.TryGetValue("Effectiveness", out effObj))
            {
                double d = (float)effObj;
                return d.ToString();
            }

            return "0";
        }

        private int MasterSwingCompareEffectiveness(MasterSwing Left, MasterSwing Right)
        {
            object l;
            object r;

            bool lvalid = Left.Tags.TryGetValue("Effectiveness", out l);
            bool rvalid = Right.Tags.TryGetValue("Effectiveness", out r);

            if (lvalid && rvalid)
            {
                float dl = (float)l;
                float dr = (float)r;

                return dl.CompareTo(dr);
            }
            else
            {
                if (lvalid) { return 1; }
                else if (rvalid) { return -1; }
                else { return 0; }
            }
        }

        private string GetCellDataDamage(MasterSwing Data)
        {
            object d;
            if (Data.Tags.TryGetValue("DamageF", out d))
            {
                float df = (float)d;
                if (df > 0.0)
                {
                    return df.ToString("F1");
                }
            }

            return Data.Damage.ToString();
        }

        private float GetDmgToShieldValue(MasterSwing Data)
        {
            object d;
            if (Data.Tags.TryGetValue("ShieldDmgF", out d))
            {
                float df = (float)d;
                return df;
            }
            else
            {
                return 0;
            }
        }

        private string GetCellDataDmgToShield(MasterSwing Data)
        {
            return GetDmgToShieldValue(Data).ToString("F1");
        }

        private string GetSqlDataDmgToShield(MasterSwing Data)
        {
            return GetDmgToShieldValue(Data).ToString("F1");
        }

        private int MasterSwingCompareDmgToShield(MasterSwing Left, MasterSwing Right)
        {
            return GetDmgToShieldValue(Left).CompareTo(GetDmgToShieldValue(Right));
        }

        private float GetShieldPValue(MasterSwing Data)
        {
            object d;
            if (Data.Tags.TryGetValue("ShieldP", out d))
            {
                float df = (float)d;
                return df;
            }
            else
            {
                return 0;
            }
        }

        private string GetCellDataShieldP(MasterSwing Data)
        {
            return GetShieldPValue(Data).ToString("P1");
        }

        private string GetSqlDataShieldP(MasterSwing Data)
        {
            return GetShieldPValue(Data).ToString("P1");
        }

        private int MasterSwingCompareShieldP(MasterSwing Left, MasterSwing Right)
        {
            return GetShieldPValue(Left).CompareTo(GetShieldPValue(Right));
        }

        private int GetDTFlankValue(DamageTypeData Data)
        {
            if (Data.Items.Count == 0) return 0;

            AttackType at = Data.Items["All"];
            return GetAAFlankValue(at);
        }

        private string GetCellDataFlankHits(DamageTypeData Data)
        {
            return GetDTFlankValue(Data).ToString(GetIntCommas());
        }

        private string GetSqlDataFlankHits(DamageTypeData Data)
        {
            return GetDTFlankValue(Data).ToString();
        }

        private double GetDTFlankPrecValue(DamageTypeData Data)
        {
            if (Data.Hits == 0) return 0;

            double fv = (double)GetDTFlankValue(Data);
            fv *= 100.0;
            fv /= Data.Hits;

            return fv;
        }

        private string GetCellDataFlankPrec(DamageTypeData Data)
        {
            return GetDTFlankPrecValue(Data).ToString("0'%");
        }
        
        private string GetSqlDataFlankPrec(DamageTypeData Data)
        {
            return GetDTFlankPrecValue(Data).ToString("0'%");
        }

        private double GetDTEffectivenessValue(DamageTypeData Data)
        {
            if (Data.Items.Count == 0) return Double.NaN;

            AttackType at = Data.Items["All"];
            return GetAAEffectiveness(at);
        }

        private string GetCellDataEffectiveness(DamageTypeData Data)
        {
            return GetDTEffectivenessValue(Data).ToString("P1");
        }

        private string GetSqlDataEffectiveness(DamageTypeData Data)
        {
            return GetDTEffectivenessValue(Data).ToString("P1");
        }

        private int GetAAFlankValue(AttackType Data)
        {
            int flankCount = 0;

            if (Data.Items.Count == 0) return 0;

            if (Data.Tags.ContainsKey("flankPrecCacheCount"))
            {
                int flankPrecCacheCount = (int)Data.Tags["flankPrecCacheCount"];
                if (flankPrecCacheCount == Data.Items.Count)
                {
                    flankCount = (int)Data.Tags["flankPrecCacheValue"];
                    return flankCount;
                }
            }

            int count = Data.Items.Count;

            for (int i=0; i<count; i++)
            {
                MasterSwing ms = Data.Items[i];

                object fv;
                if ( ( ms.Damage > 0 ) && (ms.Tags.TryGetValue("Flank", out fv) ) )
                {
                    bool flank = (bool)fv;
                    if (flank) flankCount++;
                }
            }

            Data.Tags["flankPrecCacheCount"] = count;
            Data.Tags["flankPrecCacheValue"] = flankCount;

            return flankCount;
        }

        private string GetCellDataFlankHits(AttackType Data) 
        {
            return GetAAFlankValue(Data).ToString(GetIntCommas());
        }

        private string GetSqlDataFlankHits(AttackType Data)
        {
            return GetAAFlankValue(Data).ToString();
        }

        private int AttackTypeCompareFlankHits(AttackType Left, AttackType Right)
        {
            return GetAAFlankValue(Left).CompareTo(GetAAFlankValue(Right));
        }

        private string GetCellDataFlankPrec(AttackType Data)
        {
            double flankPrec = (double) GetAAFlankValue(Data);
            flankPrec *= 100.0;
            flankPrec /= (double)Data.Hits;

            return flankPrec.ToString("0'%");
        }

        private string GetSqlDataFlankPrec(AttackType Data)
        {
            double flankPrec = (double)GetAAFlankValue(Data);
            flankPrec *= 100.0;
            flankPrec /= (double)Data.Hits;

            return flankPrec.ToString("0'%");
        }

        private int AttackTypeCompareFlankPrec(AttackType Left, AttackType Right)
        {
            double flankPrecLeft = (double)GetAAFlankValue(Left);
            flankPrecLeft /= (double)Left.Items.Count;

            double flankPrecRight = (double)GetAAFlankValue(Right);
            flankPrecRight /= (double)Right.Items.Count;

            return flankPrecLeft.CompareTo(flankPrecRight);
        }

        private double GetAAEffectiveness(AttackType Data)
        {
            double dmgTotal = 0;
            double baseDmgTotal = 0;
            double effectiveness = 0.0;

            if (Data.Items.Count == 0) return Double.NaN;

            if (Data.Tags.ContainsKey("effectivenessCacheCount"))
            {
                int flankPrecCacheCount = (int)Data.Tags["effectivenessCacheCount"];
                if (flankPrecCacheCount == Data.Items.Count)
                {
                    effectiveness = (double)Data.Tags["effectivenessCacheValue"];
                    return effectiveness;
                }
            }

            
            int count = Data.Items.Count;

            for (int i=0; i<count; i++)
            {
                MasterSwing ms = Data.Items[i];

                object damBaseObj;
                object damObj;

                if (ms.Tags.TryGetValue("BaseDamage", out damBaseObj))
                {
                    ms.Tags.TryGetValue("DamageF", out damObj);

                    float bd = (float)damBaseObj;
                    float d = (float)damObj;

                    if (bd > 0)
                    {
                        dmgTotal += d;
                        baseDmgTotal += bd;
                    }
                }
            }

            effectiveness = (double) dmgTotal / (double) baseDmgTotal;

            Data.Tags["effectivenessCacheCount"] = count;
            Data.Tags["effectivenessCacheValue"] = effectiveness;

            return effectiveness;
        }

        private string GetCellDataEffectiveness(AttackType Data)
        {
            return GetAAEffectiveness(Data).ToString("P1");
        }

        private string GetSqlDataEffectiveness(AttackType Data)
        {
            return GetAAEffectiveness(Data).ToString("P1");
        }

        private int AttackTypeCompareEffectiveness(AttackType Left, AttackType Right)
        {
            return GetAAEffectiveness(Left).CompareTo(GetAAEffectiveness(Right));
        }

        private string GetCellDataFlankDamPrec(CombatantData Data)
        {
            return GetCellDataFlankPrec(Data.Items["Outgoing Damage"]);
        }

        private string GetSqlDataFlankDamPrec(CombatantData Data)
        {
            return GetSqlDataFlankPrec(Data.Items["Outgoing Damage"]);
        }

        private int CDCompareFlankDamPrec(CombatantData Left, CombatantData Right)
        {
            return GetDTFlankPrecValue(Left.Items["Outgoing Damage"]).CompareTo(GetDTFlankPrecValue(Right.Items["Outgoing Damage"]));
        }

        private string GetCellDataDmgEffectPrec(CombatantData Data)
        {
            return GetCellDataEffectiveness(Data.Items["Outgoing Damage"]);
        }

        private string GetSqlDataDmgEffectPrec(CombatantData Data)
        {
            return GetSqlDataEffectiveness(Data.Items["Outgoing Damage"]);
        }

        private int CDCompareDmgEffectPrec(CombatantData Left, CombatantData Right)
        {
            return GetDTEffectivenessValue(Left.Items["Outgoing Damage"]).CompareTo(GetDTEffectivenessValue(Right.Items["Outgoing Damage"]));
        }

        private string GetCellDataDmgTakenEffectPrec(CombatantData Data)
        {
            return GetCellDataEffectiveness(Data.Items["Incoming Damage"]);
        }

        private string GetSqlDataDmgTakenEffectPrec(CombatantData Data)
        {
            return GetSqlDataEffectiveness(Data.Items["Incoming Damage"]);
        }

        private int CDCompareDmgTakenEffectPrec(CombatantData Left, CombatantData Right)
        {
            return GetDTEffectivenessValue(Left.Items["Incoming Damage"]).CompareTo(GetDTEffectivenessValue(Right.Items["Incoming Damage"]));
        }


        private void FixupCombatDataStructures()
        {
            // - Remove data types that do not apply to Neverwinter combat logs.
            // - Add new data types just for Neverwinter.

            EncounterData.ColumnDefs.Clear();
            EncounterData.ColumnDefs.Add("EncId", new EncounterData.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.EncId; }));
            EncounterData.ColumnDefs.Add("Title", new EncounterData.ColumnDef("Title", true, "VARCHAR(64)", "Title", (Data) => { return Data.Title; }, (Data) => { return Data.Title; }));
            EncounterData.ColumnDefs.Add("StartTime", new EncounterData.ColumnDef("StartTime", true, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : String.Format("{0} {1}", Data.StartTime.ToShortDateString(), Data.StartTime.ToLongTimeString()); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            EncounterData.ColumnDefs.Add("EndTime", new EncounterData.ColumnDef("EndTime", true, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.EndTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            EncounterData.ColumnDefs.Add("Duration", new EncounterData.ColumnDef("Duration", true, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }));
            EncounterData.ColumnDefs.Add("Damage", new EncounterData.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return (Data.Damage / 10).ToString(GetIntCommas()); }, (Data) => { return Data.Damage.ToString(); }));
            EncounterData.ColumnDefs.Add("EncDPS", new EncounterData.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.DPS.ToString(GetFloatCommas()); }, (Data) => { return Data.DPS.ToString(usCulture); }));
            EncounterData.ColumnDefs.Add("Zone", new EncounterData.ColumnDef("Zone", false, "VARCHAR(64)", "Zone", (Data) => { return Data.ZoneName; }, (Data) => { return Data.ZoneName; }));
            EncounterData.ColumnDefs.Add("Kills", new EncounterData.ColumnDef("Kills", true, "INT", "Kills", (Data) => { return Data.AlliedKills.ToString(GetIntCommas()); }, (Data) => { return Data.AlliedKills.ToString(); }));
            EncounterData.ColumnDefs.Add("Deaths", new EncounterData.ColumnDef("Deaths", true, "INT", "Deaths", (Data) => { return Data.AlliedDeaths.ToString(); }, (Data) => { return Data.AlliedDeaths.ToString(); }));

            EncounterData.ExportVariables.Clear();
            EncounterData.ExportVariables.Add("n", new EncounterData.TextExportFormatter("n", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-newline"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-newline"].DisplayedText, (Data, SelectiveAllies, Extra) => { return "\n"; }));
            EncounterData.ExportVariables.Add("t", new EncounterData.TextExportFormatter("t", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-tab"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-tab"].DisplayedText, (Data, SelectiveAllies, Extra) => { return "\t"; }));
            EncounterData.ExportVariables.Add("title", new EncounterData.TextExportFormatter("title", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-title"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-title"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "title", Extra); }));
            EncounterData.ExportVariables.Add("duration", new EncounterData.TextExportFormatter("duration", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-duration"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-duration"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "duration", Extra); }));
            EncounterData.ExportVariables.Add("DURATION", new EncounterData.TextExportFormatter("DURATION", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-DURATION"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-DURATION"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DURATION", Extra); }));
            EncounterData.ExportVariables.Add("damage", new EncounterData.TextExportFormatter("damage", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-damage"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-damage"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "damage", Extra); }));
            EncounterData.ExportVariables.Add("damage-m", new EncounterData.TextExportFormatter("damage-m", "Damage M", "Damage divided by 1,000,000 (with two decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "damage-m", Extra); }));
            EncounterData.ExportVariables.Add("DAMAGE-k", new EncounterData.TextExportFormatter("DAMAGE-k", "Short Damage K", "Damage divided by 1,000 (with no decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DAMAGE-k", Extra); }));
            EncounterData.ExportVariables.Add("DAMAGE-m", new EncounterData.TextExportFormatter("DAMAGE-m", "Short Damage M", "Damage divided by 1,000,000 (with no decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DAMAGE-m", Extra); }));
            EncounterData.ExportVariables.Add("dps", new EncounterData.TextExportFormatter("dps", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-dps"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-dps"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "dps", Extra); }));
            EncounterData.ExportVariables.Add("DPS", new EncounterData.TextExportFormatter("DPS", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-DPS"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-DPS"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DPS", Extra); }));
            EncounterData.ExportVariables.Add("DPS-k", new EncounterData.TextExportFormatter("DPS-k", "DPS K", "DPS divided by 1,000 (with no decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "DPS-k", Extra); }));
            EncounterData.ExportVariables.Add("encdps", new EncounterData.TextExportFormatter("encdps", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-extdps"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-extdps"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "encdps", Extra); }));
            EncounterData.ExportVariables.Add("ENCDPS", new EncounterData.TextExportFormatter("ENCDPS", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-EXTDPS"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-EXTDPS"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "ENCDPS", Extra); }));
            EncounterData.ExportVariables.Add("ENCDPS-k", new EncounterData.TextExportFormatter("ENCDPS-k", "Short DPS K", "ENCDPS divided by 1,000 (with no decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "ENCDPS-k", Extra); }));
            EncounterData.ExportVariables.Add("hits", new EncounterData.TextExportFormatter("hits", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-hits"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-hits"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "hits", Extra); }));
            EncounterData.ExportVariables.Add("crithits", new EncounterData.TextExportFormatter("crithits", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-crithits"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-crithits"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "crithits", Extra); }));
            EncounterData.ExportVariables.Add("crithit%", new EncounterData.TextExportFormatter("crithit%", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-crithit%"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-crithit%"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "crithit%", Extra); }));
            EncounterData.ExportVariables.Add("misses", new EncounterData.TextExportFormatter("misses", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-misses"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-misses"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "misses", Extra); }));
            EncounterData.ExportVariables.Add("hitfailed", new EncounterData.TextExportFormatter("hitfailed", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-hitfailed"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-hitfailed"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "hitfailed", Extra); }));
            EncounterData.ExportVariables.Add("swings", new EncounterData.TextExportFormatter("swings", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-swings"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-swings"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "swings", Extra); }));
            EncounterData.ExportVariables.Add("tohit", new EncounterData.TextExportFormatter("tohit", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-tohit"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-tohit"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "tohit", Extra); }));
            EncounterData.ExportVariables.Add("TOHIT", new EncounterData.TextExportFormatter("TOHIT", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-TOHIT"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-TOHIT"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "TOHIT", Extra); }));
            EncounterData.ExportVariables.Add("maxhit", new EncounterData.TextExportFormatter("maxhit", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-maxhit"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-maxhit"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "maxhit", Extra); }));
            EncounterData.ExportVariables.Add("MAXHIT", new EncounterData.TextExportFormatter("MAXHIT", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-MAXHIT"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-MAXHIT"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "MAXHIT", Extra); }));
            EncounterData.ExportVariables.Add("healed", new EncounterData.TextExportFormatter("healed", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-healed"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-healed"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "healed", Extra); }));
            EncounterData.ExportVariables.Add("enchps", new EncounterData.TextExportFormatter("enchps", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-exthps"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-exthps"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "enchps", Extra); }));
            EncounterData.ExportVariables.Add("ENCHPS", new EncounterData.TextExportFormatter("ENCHPS", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-EXTHPS"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-EXTHPS"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "ENCHPS", Extra); }));
            EncounterData.ExportVariables.Add("ENCHPS-k", new EncounterData.TextExportFormatter("ENCHPS", "Short ENCHPS K", "ENCHPS divided by 1,000 (with no decimal places)", (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "ENCHPS-k", Extra); }));
            EncounterData.ExportVariables.Add("critheals", new EncounterData.TextExportFormatter("critheals", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-critheals"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-critheals"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "critheals", Extra); }));
            EncounterData.ExportVariables.Add("critheal%", new EncounterData.TextExportFormatter("critheal%", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-critheal%"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-critheal%"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "critheal%", Extra); }));
            EncounterData.ExportVariables.Add("heals", new EncounterData.TextExportFormatter("heals", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-heals"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-heals"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "heals", Extra); }));
            EncounterData.ExportVariables.Add("cures", new EncounterData.TextExportFormatter("cures", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-cures"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-cures"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "cures", Extra); }));
            EncounterData.ExportVariables.Add("maxheal", new EncounterData.TextExportFormatter("maxheal", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-maxheal"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-maxheal"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "maxheal", Extra); }));
            EncounterData.ExportVariables.Add("MAXHEAL", new EncounterData.TextExportFormatter("MAXHEAL", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-MAXHEAL"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-MAXHEAL"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "MAXHEAL", Extra); }));
            //EncounterData.ExportVariables.Add("maxhealward", new EncounterData.TextExportFormatter("maxhealward", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-maxhealward"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-maxhealward"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "maxhealward", Extra); }));
            //EncounterData.ExportVariables.Add("MAXHEALWARD", new EncounterData.TextExportFormatter("MAXHEALWARD", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-MAXHEALWARD"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-MAXHEALWARD"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "MAXHEALWARD", Extra); }));
            EncounterData.ExportVariables.Add("damagetaken", new EncounterData.TextExportFormatter("damagetaken", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-damagetaken"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-damagetaken"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "damagetaken", Extra); }));
            EncounterData.ExportVariables.Add("healstaken", new EncounterData.TextExportFormatter("healstaken", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-healstaken"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-healstaken"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "healstaken", Extra); }));
            //EncounterData.ExportVariables.Add("powerdrain", new EncounterData.TextExportFormatter("powerdrain", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-powerdrain"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-powerdrain"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "powerdrain", Extra); }));
            EncounterData.ExportVariables.Add("powerheal", new EncounterData.TextExportFormatter("powerheal", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-powerheal"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-powerheal"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "powerheal", Extra); }));
            EncounterData.ExportVariables.Add("kills", new EncounterData.TextExportFormatter("kills", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-kills"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-kills"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "kills", Extra); }));
            EncounterData.ExportVariables.Add("deaths", new EncounterData.TextExportFormatter("deaths", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-deaths"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-deaths"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "deaths", Extra); }));


            CombatantData.ColumnDefs.Clear();
            CombatantData.ColumnDefs.Add("EncId", new CombatantData.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.Parent.EncId; }, (Left, Right) => { return 0; }));
            CombatantData.ColumnDefs.Add("Ally", new CombatantData.ColumnDef("Ally", false, "CHAR(1)", "Ally", (Data) => { return Data.Parent.GetAllies().Contains(Data).ToString(); }, (Data) => { return Data.Parent.GetAllies().Contains(Data) ? "T" : "F"; }, (Left, Right) => { return Left.Parent.GetAllies().Contains(Left).CompareTo(Right.Parent.GetAllies().Contains(Right)); }));
            CombatantData.ColumnDefs.Add("Name", new CombatantData.ColumnDef("Name", true, "VARCHAR(64)", "Name", (Data) => { return Data.Name; }, (Data) => { return Data.Name; }, (Left, Right) => { return Left.Name.CompareTo(Right.Name); }));
            CombatantData.ColumnDefs.Add("StartTime", new CombatantData.ColumnDef("StartTime", true, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.StartTime.CompareTo(Right.StartTime); }));
            CombatantData.ColumnDefs.Add("EndTime", new CombatantData.ColumnDef("EndTime", false, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.EndTime.CompareTo(Right.EndTime); }));
            CombatantData.ColumnDefs.Add("Duration", new CombatantData.ColumnDef("Duration", true, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }, (Left, Right) => { return Left.Duration.CompareTo(Right.Duration); }));
            CombatantData.ColumnDefs.Add("Damage", new CombatantData.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return Data.Damage.ToString(GetIntCommas()); }, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            CombatantData.ColumnDefs.Add("Damage%", new CombatantData.ColumnDef("Damage%", true, "VARCHAR(4)", "DamagePerc", (Data) => { return Data.DamagePercent; }, (Data) => { return Data.DamagePercent; }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            CombatantData.ColumnDefs.Add("Kills", new CombatantData.ColumnDef("Kills", false, "INT", "Kills", (Data) => { return Data.Kills.ToString(GetIntCommas()); }, (Data) => { return Data.Kills.ToString(); }, (Left, Right) => { return Left.Kills.CompareTo(Right.Kills); }));
            CombatantData.ColumnDefs.Add("Healed", new CombatantData.ColumnDef("Healed", false, "BIGINT", "Healed", (Data) => { return Data.Healed.ToString(GetIntCommas()); }, (Data) => { return Data.Healed.ToString(); }, (Left, Right) => { return Left.Healed.CompareTo(Right.Healed); }));
            CombatantData.ColumnDefs.Add("Healed%", new CombatantData.ColumnDef("Healed%", false, "VARCHAR(4)", "HealedPerc", (Data) => { return Data.HealedPercent; }, (Data) => { return Data.HealedPercent; }, (Left, Right) => { return Left.Healed.CompareTo(Right.Healed); }));
            CombatantData.ColumnDefs.Add("CritHeals", new CombatantData.ColumnDef("CritHeals", false, "INT", "CritHeals", (Data) => { return Data.CritHeals.ToString(GetIntCommas()); }, (Data) => { return Data.CritHeals.ToString(); }, (Left, Right) => { return Left.CritHeals.CompareTo(Right.CritHeals); }));
            CombatantData.ColumnDefs.Add("Heals", new CombatantData.ColumnDef("Heals", false, "INT", "Heals", (Data) => { return Data.Heals.ToString(GetIntCommas()); }, (Data) => { return Data.Heals.ToString(); }, (Left, Right) => { return Left.Heals.CompareTo(Right.Heals); }));
            CombatantData.ColumnDefs.Add("Cures", new CombatantData.ColumnDef("Cures", false, "INT", "CureDispels", (Data) => { return Data.CureDispels.ToString(GetIntCommas()); }, (Data) => { return Data.CureDispels.ToString(); }, (Left, Right) => { return Left.CureDispels.CompareTo(Right.CureDispels); }));
            //CombatantData.ColumnDefs.Add("PowerDrain", new CombatantData.ColumnDef("PowerDrain", true, "BIGINT", "PowerDrain", (Data) => { return Data.PowerDamage.ToString(GetIntCommas()); }, (Data) => { return Data.PowerDamage.ToString(); }, (Left, Right) => { return Left.PowerDamage.CompareTo(Right.PowerDamage); }));
            CombatantData.ColumnDefs.Add("PowerReplenish", new CombatantData.ColumnDef("PowerReplenish", false, "BIGINT", "PowerReplenish", (Data) => { return Data.PowerReplenish.ToString(GetIntCommas()); }, (Data) => { return Data.PowerReplenish.ToString(); }, (Left, Right) => { return Left.PowerReplenish.CompareTo(Right.PowerReplenish); }));
            CombatantData.ColumnDefs.Add("DPS", new CombatantData.ColumnDef("DPS", false, "DOUBLE", "DPS", (Data) => { return Data.DPS.ToString(GetFloatCommas()); }, (Data) => { return Data.DPS.ToString(usCulture); }, (Left, Right) => { return Left.DPS.CompareTo(Right.DPS); }));
            CombatantData.ColumnDefs.Add("EncDPS", new CombatantData.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.EncDPS.ToString(GetFloatCommas()); }, (Data) => { return Data.EncDPS.ToString(usCulture); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            CombatantData.ColumnDefs.Add("EncHPS", new CombatantData.ColumnDef("EncHPS", true, "DOUBLE", "EncHPS", (Data) => { return Data.EncHPS.ToString(GetFloatCommas()); }, (Data) => { return Data.EncHPS.ToString(usCulture); }, (Left, Right) => { return Left.Healed.CompareTo(Right.Healed); }));
            CombatantData.ColumnDefs.Add("Hits", new CombatantData.ColumnDef("Hits", false, "INT", "Hits", (Data) => { return Data.Hits.ToString(GetIntCommas()); }, (Data) => { return Data.Hits.ToString(); }, (Left, Right) => { return Left.Hits.CompareTo(Right.Hits); }));
            CombatantData.ColumnDefs.Add("CritHits", new CombatantData.ColumnDef("CritHits", false, "INT", "CritHits", (Data) => { return Data.CritHits.ToString(GetIntCommas()); }, (Data) => { return Data.CritHits.ToString(); }, (Left, Right) => { return Left.CritHits.CompareTo(Right.CritHits); }));
            CombatantData.ColumnDefs.Add("Avoids", new CombatantData.ColumnDef("Avoids", false, "INT", "Blocked", (Data) => { return Data.Blocked.ToString(GetIntCommas()); }, (Data) => { return Data.Blocked.ToString(); }, (Left, Right) => { return Left.Blocked.CompareTo(Right.Blocked); }));
            CombatantData.ColumnDefs.Add("Misses", new CombatantData.ColumnDef("Misses", false, "INT", "Misses", (Data) => { return Data.Misses.ToString(GetIntCommas()); }, (Data) => { return Data.Misses.ToString(); }, (Left, Right) => { return Left.Misses.CompareTo(Right.Misses); }));
            CombatantData.ColumnDefs.Add("Swings", new CombatantData.ColumnDef("Swings", false, "INT", "Swings", (Data) => { return Data.Swings.ToString(GetIntCommas()); }, (Data) => { return Data.Swings.ToString(); }, (Left, Right) => { return Left.Swings.CompareTo(Right.Swings); }));
            CombatantData.ColumnDefs.Add("HealingTaken", new CombatantData.ColumnDef("HealingTaken", false, "BIGINT", "HealsTaken", (Data) => { return Data.HealsTaken.ToString(GetIntCommas()); }, (Data) => { return Data.HealsTaken.ToString(); }, (Left, Right) => { return Left.HealsTaken.CompareTo(Right.HealsTaken); }));
            CombatantData.ColumnDefs.Add("DamageTaken", new CombatantData.ColumnDef("DamageTaken", true, "BIGINT", "DamageTaken", (Data) => { return Data.DamageTaken.ToString(GetIntCommas()); }, (Data) => { return Data.DamageTaken.ToString(); }, (Left, Right) => { return Left.DamageTaken.CompareTo(Right.DamageTaken); }));
            CombatantData.ColumnDefs.Add("Deaths", new CombatantData.ColumnDef("Deaths", true, "INT", "Deaths", (Data) => { return Data.Deaths.ToString(GetIntCommas()); }, (Data) => { return Data.Deaths.ToString(); }, (Left, Right) => { return Left.Deaths.CompareTo(Right.Deaths); }));
            CombatantData.ColumnDefs.Add("ToHit%", new CombatantData.ColumnDef("ToHit%", false, "FLOAT", "ToHit", (Data) => { return Data.ToHit.ToString(GetFloatCommas()); }, (Data) => { return Data.ToHit.ToString(usCulture); }, (Left, Right) => { return Left.ToHit.CompareTo(Right.ToHit); }));
            CombatantData.ColumnDefs.Add("FCritHit%", new CombatantData.ColumnDef("FCritHit%", true, "VARCHAR(8)", "FCritHitPerc", (Data) => { return GetFilteredCritChance(Data).ToString("0'%"); }, (Data) => { return GetFilteredCritChance(Data).ToString("0'%"); }, (Left, Right) => { return GetFilteredCritChance(Left).CompareTo(GetFilteredCritChance(Right)); }));
            CombatantData.ColumnDefs.Add("CritDam%", new CombatantData.ColumnDef("CritDam%", false, "VARCHAR(8)", "CritDamPerc", (Data) => { return Data.CritDamPerc.ToString("0'%"); }, (Data) => { return Data.CritDamPerc.ToString("0'%"); }, (Left, Right) => { return Left.CritDamPerc.CompareTo(Right.CritDamPerc); }));
            CombatantData.ColumnDefs.Add("CritHeal%", new CombatantData.ColumnDef("CritHeal%", false, "VARCHAR(8)", "CritHealPerc", (Data) => { return Data.CritHealPerc.ToString("0'%"); }, (Data) => { return Data.CritHealPerc.ToString("0'%"); }, (Left, Right) => { return Left.CritHealPerc.CompareTo(Right.CritHealPerc); }));
            //CombatantData.ColumnDefs.Add("Threat +/-", new CombatantData.ColumnDef("Threat +/-", false, "VARCHAR(32)", "ThreatStr", (Data) => { return Data.GetThreatStr("Threat (Out)"); }, (Data) => { return Data.GetThreatStr("Threat (Out)"); }, (Left, Right) => { return Left.GetThreatDelta("Threat (Out)").CompareTo(Right.GetThreatDelta("Threat (Out)")); }));
            //CombatantData.ColumnDefs.Add("ThreatDelta", new CombatantData.ColumnDef("ThreatDelta", false, "INT", "ThreatDelta", (Data) => { return Data.GetThreatDelta("Threat (Out)").ToString(GetIntCommas()); }, (Data) => { return Data.GetThreatDelta("Threat (Out)").ToString(); }, (Left, Right) => { return Left.GetThreatDelta("Threat (Out)").CompareTo(Right.GetThreatDelta("Threat (Out)")); }));
            
            CombatantData.ColumnDefs.Add("FlankDam%",
                new CombatantData.ColumnDef("FlankDam%", false, "VARCHAR(8)", "FlankDamPrec", GetCellDataFlankDamPrec, GetSqlDataFlankDamPrec, CDCompareFlankDamPrec));
            CombatantData.ColumnDefs.Add("DmgEffect%",
                new CombatantData.ColumnDef("DmgEffect%", false, "VARCHAR(8)", "DmgEffectPrec", GetCellDataDmgEffectPrec, GetSqlDataDmgEffectPrec, CDCompareDmgEffectPrec));
            CombatantData.ColumnDefs.Add("DmgTakenEffect%",
                new CombatantData.ColumnDef("DmgTakenEffect%", false, "VARCHAR(8)", "DmgTakenEffectPrec", GetCellDataDmgTakenEffectPrec, GetSqlDataDmgTakenEffectPrec, CDCompareDmgTakenEffectPrec));


            CombatantData.OutgoingDamageTypeDataObjects = new Dictionary<string, CombatantData.DamageTypeDef>
		{
			//{"Auto-Attack (Out)", new CombatantData.DamageTypeDef("Auto-Attack (Out)", -1, Color.DarkGoldenrod)},
			//{"Skill/Ability (Out)", new CombatantData.DamageTypeDef("Skill/Ability (Out)", -1, Color.DarkOrange)},
			{"Outgoing Damage", new CombatantData.DamageTypeDef("Outgoing Damage", 0, Color.Orange)},
			{"Healed (Out)", new CombatantData.DamageTypeDef("Healed (Out)", 1, Color.Blue)},
			//{"Power Drain (Out)", new CombatantData.DamageTypeDef("Power Drain (Out)", -1, Color.Purple)},
			{"Power Replenish (Out)", new CombatantData.DamageTypeDef("Power Replenish (Out)", 1, Color.Violet)},
			{"Cure/Dispel (Out)", new CombatantData.DamageTypeDef("Cure/Dispel (Out)", 0, Color.Wheat)},
			//{"Threat (Out)", new CombatantData.DamageTypeDef("Threat (Out)", -1, Color.Yellow)},
			{"All Outgoing (Ref)", new CombatantData.DamageTypeDef("All Outgoing (Ref)", 0, Color.Black)}
		};
            CombatantData.IncomingDamageTypeDataObjects = new Dictionary<string, CombatantData.DamageTypeDef>
		{
			{"Incoming Damage", new CombatantData.DamageTypeDef("Incoming Damage", -1, Color.Red)},
			{"Healed (Inc)",new CombatantData.DamageTypeDef("Healed (Inc)", 1, Color.LimeGreen)},
			//{"Power Drain (Inc)",new CombatantData.DamageTypeDef("Power Drain (Inc)", -1, Color.Magenta)},
			{"Power Replenish (Inc)",new CombatantData.DamageTypeDef("Power Replenish (Inc)", 1, Color.MediumPurple)},
			{"Cure/Dispel (Inc)", new CombatantData.DamageTypeDef("Cure/Dispel (Inc)", 0, Color.Wheat)},
			//{"Threat (Inc)",new CombatantData.DamageTypeDef("Threat (Inc)", -1, Color.Yellow)},
			{"All Incoming (Ref)",new CombatantData.DamageTypeDef("All Incoming (Ref)", 0, Color.Black)}
		};
            CombatantData.SwingTypeToDamageTypeDataLinksOutgoing = new SortedDictionary<int, List<string>>
		{ 
			{1, new List<string> { "Outgoing Damage" } },
			{2, new List<string> { "Outgoing Damage" } },
			{3, new List<string> { "Healed (Out)" } },
			{13, new List<string> { "Power Replenish (Out)" } },
			{20, new List<string> { "Cure/Dispel (Out)" } }
		};
            CombatantData.SwingTypeToDamageTypeDataLinksIncoming = new SortedDictionary<int, List<string>>
		{ 
			{1, new List<string> { "Incoming Damage" } },
			{2, new List<string> { "Incoming Damage" } },
			{3, new List<string> { "Healed (Inc)" } },
			{13, new List<string> { "Power Replenish (Inc)" } },
			{20, new List<string> { "Cure/Dispel (Inc)" } }
		};

            CombatantData.DamageSwingTypes = new List<int> { 1, 2 };
            CombatantData.HealingSwingTypes = new List<int> { 3 };

            CombatantData.DamageTypeDataNonSkillDamage = "Auto-Attack (Out)";
            CombatantData.DamageTypeDataOutgoingDamage = "Outgoing Damage";
            CombatantData.DamageTypeDataOutgoingHealing = "Healed (Out)";
            CombatantData.DamageTypeDataIncomingDamage = "Incoming Damage";
            CombatantData.DamageTypeDataIncomingHealing = "Healed (Inc)";


            CombatantData.ExportVariables.Clear();
            CombatantData.ExportVariables.Add("n", new CombatantData.TextExportFormatter("n", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-newline"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-newline"].DisplayedText, (Data, Extra) => { return "\n"; }));
            CombatantData.ExportVariables.Add("t", new CombatantData.TextExportFormatter("t", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-tab"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-tab"].DisplayedText, (Data, Extra) => { return "\t"; }));
            CombatantData.ExportVariables.Add("name", new CombatantData.TextExportFormatter("name", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-name"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-name"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "name", Extra); }));
            CombatantData.ExportVariables.Add("NAME", new CombatantData.TextExportFormatter("NAME", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME", Extra); }));
            CombatantData.ExportVariables.Add("duration", new CombatantData.TextExportFormatter("duration", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-duration"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-duration"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "duration", Extra); }));
            CombatantData.ExportVariables.Add("DURATION", new CombatantData.TextExportFormatter("DURATION", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-DURATION"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-DURATION"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "DURATION", Extra); }));
            CombatantData.ExportVariables.Add("damage", new CombatantData.TextExportFormatter("damage", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-damage"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-damage"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "damage", Extra); }));
            CombatantData.ExportVariables.Add("damage-m", new CombatantData.TextExportFormatter("damage-m", "Damage M", "Damage divided by 1,000,000 (with two decimal places)", (Data, Extra) => { return CombatantFormatSwitch(Data, "damage-m", Extra); }));
            CombatantData.ExportVariables.Add("DAMAGE-k", new CombatantData.TextExportFormatter("DAMAGE-k", "Short Damage K", "Damage divided by 1,000 (with no decimal places)", (Data, Extra) => { return CombatantFormatSwitch(Data, "DAMAGE-k", Extra); }));
            CombatantData.ExportVariables.Add("DAMAGE-m", new CombatantData.TextExportFormatter("DAMAGE-m", "Short Damage M", "Damage divided by 1,000,000 (with no decimal places)", (Data, Extra) => { return CombatantFormatSwitch(Data, "DAMAGE-m", Extra); }));
            CombatantData.ExportVariables.Add("damage%", new CombatantData.TextExportFormatter("damage%", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-damage%"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-damage%"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "damage%", Extra); }));
            CombatantData.ExportVariables.Add("dps", new CombatantData.TextExportFormatter("dps", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-dps"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-dps"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "dps", Extra); }));
            CombatantData.ExportVariables.Add("DPS", new CombatantData.TextExportFormatter("DPS", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-DPS"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-DPS"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "DPS", Extra); }));
            CombatantData.ExportVariables.Add("DPS-k", new CombatantData.TextExportFormatter("DPS-k", "Short DPS K", "Short DPS divided by 1,000 (with no decimal places)", (Data, Extra) => { return CombatantFormatSwitch(Data, "DPS-k", Extra); }));
            CombatantData.ExportVariables.Add("encdps", new CombatantData.TextExportFormatter("encdps", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-extdps"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-extdps"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "encdps", Extra); }));
            CombatantData.ExportVariables.Add("ENCDPS", new CombatantData.TextExportFormatter("ENCDPS", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-EXTDPS"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-EXTDPS"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "ENCDPS", Extra); }));
            CombatantData.ExportVariables.Add("ENCDPS-k", new CombatantData.TextExportFormatter("ENCDPS-k", "Short Encounter DPS K", "Short Encounter DPS divided by 1,000 (with no decimal places)", (Data, Extra) => { return CombatantFormatSwitch(Data, "ENCDPS-k", Extra); }));
            CombatantData.ExportVariables.Add("hits", new CombatantData.TextExportFormatter("hits", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-hits"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-hits"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "hits", Extra); }));
            CombatantData.ExportVariables.Add("crithits", new CombatantData.TextExportFormatter("crithits", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-crithits"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-crithits"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "crithits", Extra); }));
            CombatantData.ExportVariables.Add("crithit%", new CombatantData.TextExportFormatter("crithit%", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-crithit%"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-crithit%"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "crithit%", Extra); }));
            CombatantData.ExportVariables.Add("fcrithit%", new CombatantData.TextExportFormatter("fcrithit%", "Filtered Critical Hit Chance", "Critical Hit Chance filtered against AttackTypes that have the ability to critically hit.", (Data, Extra) => { return CombatantFormatSwitch(Data, "fcrithit%", Extra); }));
            CombatantData.ExportVariables.Add("misses", new CombatantData.TextExportFormatter("misses", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-misses"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-misses"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "misses", Extra); }));
            CombatantData.ExportVariables.Add("hitfailed", new CombatantData.TextExportFormatter("hitfailed", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-hitfailed"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-hitfailed"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "hitfailed", Extra); }));
            CombatantData.ExportVariables.Add("swings", new CombatantData.TextExportFormatter("swings", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-swings"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-swings"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "swings", Extra); }));
            CombatantData.ExportVariables.Add("tohit", new CombatantData.TextExportFormatter("tohit", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-tohit"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-tohit"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "tohit", Extra); }));
            CombatantData.ExportVariables.Add("TOHIT", new CombatantData.TextExportFormatter("TOHIT", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-TOHIT"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-TOHIT"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "TOHIT", Extra); }));
            CombatantData.ExportVariables.Add("maxhit", new CombatantData.TextExportFormatter("maxhit", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-maxhit"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-maxhit"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "maxhit", Extra); }));
            CombatantData.ExportVariables.Add("MAXHIT", new CombatantData.TextExportFormatter("MAXHIT", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-MAXHIT"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-MAXHIT"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "MAXHIT", Extra); }));
            CombatantData.ExportVariables.Add("healed", new CombatantData.TextExportFormatter("healed", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-healed"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-healed"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "healed", Extra); }));
            CombatantData.ExportVariables.Add("healed%", new CombatantData.TextExportFormatter("healed%", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-healed%"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-healed%"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "healed%", Extra); }));
            CombatantData.ExportVariables.Add("enchps", new CombatantData.TextExportFormatter("enchps", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-exthps"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-exthps"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "enchps", Extra); }));
            CombatantData.ExportVariables.Add("ENCHPS", new CombatantData.TextExportFormatter("ENCHPS", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-EXTHPS"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-EXTHPS"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "ENCHPS", Extra); }));
            CombatantData.ExportVariables.Add("ENCHPS-k", new CombatantData.TextExportFormatter("ENCHPS-k", "Short Encounter HPS K", "Short Encounter HPS divided by 1,000 (with no decimal places)", (Data, Extra) => { return CombatantFormatSwitch(Data, "ENCHPS-k", Extra); }));
            CombatantData.ExportVariables.Add("critheals", new CombatantData.TextExportFormatter("critheals", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-critheals"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-critheals"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "critheals", Extra); }));
            CombatantData.ExportVariables.Add("critheal%", new CombatantData.TextExportFormatter("critheal%", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-critheal%"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-critheal%"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "critheal%", Extra); }));
            CombatantData.ExportVariables.Add("heals", new CombatantData.TextExportFormatter("heals", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-heals"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-heals"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "heals", Extra); }));
            CombatantData.ExportVariables.Add("cures", new CombatantData.TextExportFormatter("cures", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-cures"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-cures"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "cures", Extra); }));
            CombatantData.ExportVariables.Add("maxheal", new CombatantData.TextExportFormatter("maxheal", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-maxheal"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-maxheal"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "maxheal", Extra); }));
            CombatantData.ExportVariables.Add("MAXHEAL", new CombatantData.TextExportFormatter("MAXHEAL", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-MAXHEAL"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-MAXHEAL"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "MAXHEAL", Extra); }));
            //CombatantData.ExportVariables.Add("maxhealward", new CombatantData.TextExportFormatter("maxhealward", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-maxhealward"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-maxhealward"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "maxhealward", Extra); }));
            //CombatantData.ExportVariables.Add("MAXHEALWARD", new CombatantData.TextExportFormatter("MAXHEALWARD", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-MAXHEALWARD"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-MAXHEALWARD"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "MAXHEALWARD", Extra); }));
            CombatantData.ExportVariables.Add("damagetaken", new CombatantData.TextExportFormatter("damagetaken", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-damagetaken"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-damagetaken"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "damagetaken", Extra); }));
            CombatantData.ExportVariables.Add("healstaken", new CombatantData.TextExportFormatter("healstaken", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-healstaken"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-healstaken"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "healstaken", Extra); }));
            //CombatantData.ExportVariables.Add("powerdrain", new CombatantData.TextExportFormatter("powerdrain", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-powerdrain"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-powerdrain"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "powerdrain", Extra); }));
            CombatantData.ExportVariables.Add("powerheal", new CombatantData.TextExportFormatter("powerheal", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-powerheal"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-powerheal"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "powerheal", Extra); }));
            CombatantData.ExportVariables.Add("kills", new CombatantData.TextExportFormatter("kills", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-kills"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-kills"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "kills", Extra); }));
            CombatantData.ExportVariables.Add("deaths", new CombatantData.TextExportFormatter("deaths", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-deaths"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-deaths"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "deaths", Extra); }));
            //CombatantData.ExportVariables.Add("threatstr", new CombatantData.TextExportFormatter("threatstr", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-threatstr"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-threatstr"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "threatstr", Extra); }));
            //CombatantData.ExportVariables.Add("threatdelta", new CombatantData.TextExportFormatter("threatdelta", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-threatdelta"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-threatdelta"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "threatdelta", Extra); }));
            CombatantData.ExportVariables.Add("NAME3", new CombatantData.TextExportFormatter("NAME3", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME3"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME3"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME3", Extra); }));
            CombatantData.ExportVariables.Add("NAME4", new CombatantData.TextExportFormatter("NAME4", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME4"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME4"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME4", Extra); }));
            CombatantData.ExportVariables.Add("NAME5", new CombatantData.TextExportFormatter("NAME5", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME5"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME5"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME5", Extra); }));
            CombatantData.ExportVariables.Add("NAME6", new CombatantData.TextExportFormatter("NAME6", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME6"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME6"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME6", Extra); }));
            CombatantData.ExportVariables.Add("NAME7", new CombatantData.TextExportFormatter("NAME7", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME7"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME7"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME7", Extra); }));
            CombatantData.ExportVariables.Add("NAME8", new CombatantData.TextExportFormatter("NAME8", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME8"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME8"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME8", Extra); }));
            CombatantData.ExportVariables.Add("NAME9", new CombatantData.TextExportFormatter("NAME9", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME9"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME9"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME9", Extra); }));
            CombatantData.ExportVariables.Add("NAME10", new CombatantData.TextExportFormatter("NAME10", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME10"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME10"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME10", Extra); }));
            CombatantData.ExportVariables.Add("NAME11", new CombatantData.TextExportFormatter("NAME11", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME11"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME11"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME11", Extra); }));
            CombatantData.ExportVariables.Add("NAME12", new CombatantData.TextExportFormatter("NAME12", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME12"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME12"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME12", Extra); }));
            CombatantData.ExportVariables.Add("NAME13", new CombatantData.TextExportFormatter("NAME13", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME13"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME13"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME13", Extra); }));
            CombatantData.ExportVariables.Add("NAME14", new CombatantData.TextExportFormatter("NAME14", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME14"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME14"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME14", Extra); }));
            CombatantData.ExportVariables.Add("NAME15", new CombatantData.TextExportFormatter("NAME15", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME15"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME15"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME15", Extra); }));


            DamageTypeData.ColumnDefs.Clear();
            DamageTypeData.ColumnDefs.Add("EncId", new DamageTypeData.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.Parent.Parent.EncId; }));
            DamageTypeData.ColumnDefs.Add("Combatant", new DamageTypeData.ColumnDef("Combatant", false, "VARCHAR(64)", "Combatant", (Data) => { return Data.Parent.Name; }, (Data) => { return Data.Parent.Name; }));
            DamageTypeData.ColumnDefs.Add("Grouping", new DamageTypeData.ColumnDef("Grouping", false, "VARCHAR(92)", "Grouping", (Data) => { return string.Empty; }, GetDamageTypeGrouping));
            DamageTypeData.ColumnDefs.Add("Type", new DamageTypeData.ColumnDef("Type", true, "VARCHAR(64)", "Type", (Data) => { return Data.Type; }, (Data) => { return Data.Type; }));
            DamageTypeData.ColumnDefs.Add("StartTime", new DamageTypeData.ColumnDef("StartTime", false, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            DamageTypeData.ColumnDefs.Add("EndTime", new DamageTypeData.ColumnDef("EndTime", false, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            DamageTypeData.ColumnDefs.Add("Duration", new DamageTypeData.ColumnDef("Duration", false, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }));
            DamageTypeData.ColumnDefs.Add("Damage", new DamageTypeData.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return Data.Damage.ToString(GetIntCommas()); }, (Data) => { return Data.Damage.ToString(); }));
            DamageTypeData.ColumnDefs.Add("EncDPS", new DamageTypeData.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.EncDPS.ToString(GetFloatCommas()); }, (Data) => { return Data.EncDPS.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("CharDPS", new DamageTypeData.ColumnDef("CharDPS", false, "DOUBLE", "CharDPS", (Data) => { return Data.CharDPS.ToString(GetFloatCommas()); }, (Data) => { return Data.CharDPS.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("DPS", new DamageTypeData.ColumnDef("DPS", false, "DOUBLE", "DPS", (Data) => { return Data.DPS.ToString(GetFloatCommas()); }, (Data) => { return Data.DPS.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("Average", new DamageTypeData.ColumnDef("Average", true, "FLOAT", "Average", (Data) => { return Data.Average.ToString(GetFloatCommas()); }, (Data) => { return Data.Average.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("Median", new DamageTypeData.ColumnDef("Median", false, "INT", "Median", (Data) => { return Data.Median.ToString(GetIntCommas()); }, (Data) => { return Data.Median.ToString(); }));
            DamageTypeData.ColumnDefs.Add("MinHit", new DamageTypeData.ColumnDef("MinHit", true, "INT", "MinHit", (Data) => { return Data.MinHit.ToString(GetIntCommas()); }, (Data) => { return Data.MinHit.ToString(); }));
            DamageTypeData.ColumnDefs.Add("MaxHit", new DamageTypeData.ColumnDef("MaxHit", true, "INT", "MaxHit", (Data) => { return Data.MaxHit.ToString(GetIntCommas()); }, (Data) => { return Data.MaxHit.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Hits", new DamageTypeData.ColumnDef("Hits", true, "INT", "Hits", (Data) => { return Data.Hits.ToString(GetIntCommas()); }, (Data) => { return Data.Hits.ToString(); }));
            DamageTypeData.ColumnDefs.Add("CritHits", new DamageTypeData.ColumnDef("CritHits", false, "INT", "CritHits", (Data) => { return Data.CritHits.ToString(GetIntCommas()); }, (Data) => { return Data.CritHits.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Avoids", new DamageTypeData.ColumnDef("Avoids", false, "INT", "Blocked", (Data) => { return Data.Blocked.ToString(GetIntCommas()); }, (Data) => { return Data.Blocked.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Misses", new DamageTypeData.ColumnDef("Misses", false, "INT", "Misses", (Data) => { return Data.Misses.ToString(GetIntCommas()); }, (Data) => { return Data.Misses.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Swings", new DamageTypeData.ColumnDef("Swings", true, "INT", "Swings", (Data) => { return Data.Swings.ToString(GetIntCommas()); }, (Data) => { return Data.Swings.ToString(); }));
            DamageTypeData.ColumnDefs.Add("ToHit", new DamageTypeData.ColumnDef("ToHit", false, "FLOAT", "ToHit", (Data) => { return Data.ToHit.ToString(GetFloatCommas()); }, (Data) => { return Data.ToHit.ToString(); }));
            DamageTypeData.ColumnDefs.Add("AvgDelay", new DamageTypeData.ColumnDef("AvgDelay", false, "FLOAT", "AverageDelay", (Data) => { return Data.AverageDelay.ToString(GetFloatCommas()); }, (Data) => { return Data.AverageDelay.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Crit%", new DamageTypeData.ColumnDef("Crit%", true, "VARCHAR(8)", "CritPerc", (Data) => { return Data.CritPerc.ToString("0'%"); }, (Data) => { return Data.CritPerc.ToString("0'%"); }));

            DamageTypeData.ColumnDefs.Add("FlankHits",
                new DamageTypeData.ColumnDef("FlankHits", false, "INT", "FlankHits", GetCellDataFlankHits, GetSqlDataFlankHits));
            DamageTypeData.ColumnDefs.Add("Flank%",
                new DamageTypeData.ColumnDef("Flank%", true, "VARCHAR(8)", "FlankPerc", GetCellDataFlankPrec, GetSqlDataFlankPrec));
            DamageTypeData.ColumnDefs.Add("Effectiveness",
                new DamageTypeData.ColumnDef("Effectiveness", true, "VARCHAR(8)", "Effectiveness", GetCellDataEffectiveness, GetSqlDataEffectiveness));


            AttackType.ColumnDefs.Clear();
            AttackType.ColumnDefs.Add("EncId", new AttackType.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.Parent.Parent.Parent.EncId; }, (Left, Right) => { return 0; }));
            AttackType.ColumnDefs.Add("Attacker", new AttackType.ColumnDef("Attacker", false, "VARCHAR(64)", "Attacker", (Data) => { return Data.Parent.Outgoing ? Data.Parent.Parent.Name : string.Empty; }, (Data) => { return Data.Parent.Outgoing ? Data.Parent.Parent.Name : string.Empty; }, (Left, Right) => { return 0; }));
            AttackType.ColumnDefs.Add("Victim", new AttackType.ColumnDef("Victim", false, "VARCHAR(64)", "Victim", (Data) => { return Data.Parent.Outgoing ? string.Empty : Data.Parent.Parent.Name; }, (Data) => { return Data.Parent.Outgoing ? string.Empty : Data.Parent.Parent.Name; }, (Left, Right) => { return 0; }));
            AttackType.ColumnDefs.Add("SwingType", new AttackType.ColumnDef("SwingType", false, "TINYINT", "SwingType", GetAttackTypeSwingType, GetAttackTypeSwingType, (Left, Right) => { return 0; }));
            AttackType.ColumnDefs.Add("Type", new AttackType.ColumnDef("Type", true, "VARCHAR(64)", "Type", (Data) => { return Data.Type; }, (Data) => { return Data.Type; }, (Left, Right) => { return Left.Type.CompareTo(Right.Type); }));
            AttackType.ColumnDefs.Add("StartTime", new AttackType.ColumnDef("StartTime", false, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.StartTime.CompareTo(Right.StartTime); }));
            AttackType.ColumnDefs.Add("EndTime", new AttackType.ColumnDef("EndTime", false, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.EndTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.EndTime.CompareTo(Right.EndTime); }));
            AttackType.ColumnDefs.Add("Duration", new AttackType.ColumnDef("Duration", false, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }, (Left, Right) => { return Left.Duration.CompareTo(Right.Duration); }));
            AttackType.ColumnDefs.Add("Damage", new AttackType.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return Data.Damage.ToString(GetIntCommas()); }, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            AttackType.ColumnDefs.Add("EncDPS", new AttackType.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.EncDPS.ToString(GetFloatCommas()); }, (Data) => { return Data.EncDPS.ToString(usCulture); }, (Left, Right) => { return Left.EncDPS.CompareTo(Right.EncDPS); }));
            AttackType.ColumnDefs.Add("CharDPS", new AttackType.ColumnDef("CharDPS", false, "DOUBLE", "CharDPS", (Data) => { return Data.CharDPS.ToString(GetFloatCommas()); }, (Data) => { return Data.CharDPS.ToString(usCulture); }, (Left, Right) => { return Left.CharDPS.CompareTo(Right.CharDPS); }));
            AttackType.ColumnDefs.Add("DPS", new AttackType.ColumnDef("DPS", false, "DOUBLE", "DPS", (Data) => { return Data.DPS.ToString(GetFloatCommas()); }, (Data) => { return Data.DPS.ToString(usCulture); }, (Left, Right) => { return Left.DPS.CompareTo(Right.DPS); }));
            AttackType.ColumnDefs.Add("Average", new AttackType.ColumnDef("Average", true, "FLOAT", "Average", (Data) => { return Data.Average.ToString(GetFloatCommas()); }, (Data) => { return Data.Average.ToString(usCulture); }, (Left, Right) => { return Left.Average.CompareTo(Right.Average); }));
            AttackType.ColumnDefs.Add("Median", new AttackType.ColumnDef("Median", true, "INT", "Median", (Data) => { return Data.Median.ToString(GetIntCommas()); }, (Data) => { return Data.Median.ToString(); }, (Left, Right) => { return Left.Median.CompareTo(Right.Median); }));
            AttackType.ColumnDefs.Add("MinHit", new AttackType.ColumnDef("MinHit", true, "INT", "MinHit", (Data) => { return Data.MinHit.ToString(GetIntCommas()); }, (Data) => { return Data.MinHit.ToString(); }, (Left, Right) => { return Left.MinHit.CompareTo(Right.MinHit); }));
            AttackType.ColumnDefs.Add("MaxHit", new AttackType.ColumnDef("MaxHit", true, "INT", "MaxHit", (Data) => { return Data.MaxHit.ToString(GetIntCommas()); }, (Data) => { return Data.MaxHit.ToString(); }, (Left, Right) => { return Left.MaxHit.CompareTo(Right.MaxHit); }));
            AttackType.ColumnDefs.Add("Resist", new AttackType.ColumnDef("Resist", true, "VARCHAR(64)", "Resist", (Data) => { return Data.Resist; }, (Data) => { return Data.Resist; }, (Left, Right) => { return Left.Resist.CompareTo(Right.Resist); }));
            AttackType.ColumnDefs.Add("Hits", new AttackType.ColumnDef("Hits", true, "INT", "Hits", (Data) => { return Data.Hits.ToString(GetIntCommas()); }, (Data) => { return Data.Hits.ToString(); }, (Left, Right) => { return Left.Hits.CompareTo(Right.Hits); }));
            AttackType.ColumnDefs.Add("CritHits", new AttackType.ColumnDef("CritHits", false, "INT", "CritHits", (Data) => { return Data.CritHits.ToString(GetIntCommas()); }, (Data) => { return Data.CritHits.ToString(); }, (Left, Right) => { return Left.CritHits.CompareTo(Right.CritHits); }));
            AttackType.ColumnDefs.Add("Avoids", new AttackType.ColumnDef("Avoids", false, "INT", "Blocked", (Data) => { return Data.Blocked.ToString(GetIntCommas()); }, (Data) => { return Data.Blocked.ToString(); }, (Left, Right) => { return Left.Blocked.CompareTo(Right.Blocked); }));
            AttackType.ColumnDefs.Add("Misses", new AttackType.ColumnDef("Misses", false, "INT", "Misses", (Data) => { return Data.Misses.ToString(GetIntCommas()); }, (Data) => { return Data.Misses.ToString(); }, (Left, Right) => { return Left.Misses.CompareTo(Right.Misses); }));
            AttackType.ColumnDefs.Add("Swings", new AttackType.ColumnDef("Swings", true, "INT", "Swings", (Data) => { return Data.Swings.ToString(GetIntCommas()); }, (Data) => { return Data.Swings.ToString(); }, (Left, Right) => { return Left.Swings.CompareTo(Right.Swings); }));
            AttackType.ColumnDefs.Add("ToHit", new AttackType.ColumnDef("ToHit", true, "FLOAT", "ToHit", (Data) => { return Data.ToHit.ToString(GetFloatCommas()); }, (Data) => { return Data.ToHit.ToString(usCulture); }, (Left, Right) => { return Left.ToHit.CompareTo(Right.ToHit); }));
            AttackType.ColumnDefs.Add("AvgDelay", new AttackType.ColumnDef("AvgDelay", false, "FLOAT", "AverageDelay", (Data) => { return Data.AverageDelay.ToString(GetFloatCommas()); }, (Data) => { return Data.AverageDelay.ToString(usCulture); }, (Left, Right) => { return Left.AverageDelay.CompareTo(Right.AverageDelay); }));
            AttackType.ColumnDefs.Add("Crit%", new AttackType.ColumnDef("Crit%", true, "VARCHAR(8)", "CritPerc", (Data) => { return Data.CritPerc.ToString("0'%"); }, (Data) => { return Data.CritPerc.ToString("0'%"); }, (Left, Right) => { return Left.CritPerc.CompareTo(Right.CritPerc); }));

            AttackType.ColumnDefs.Add("FlankHits",
                new AttackType.ColumnDef("FlankHits", false, "INT", "FlankHits", GetCellDataFlankHits, GetSqlDataFlankHits, AttackTypeCompareFlankHits));
            AttackType.ColumnDefs.Add("Flank%",
                new AttackType.ColumnDef("Flank%", true, "VARCHAR(8)", "FlankPerc", GetCellDataFlankPrec, GetSqlDataFlankPrec, AttackTypeCompareFlankPrec));
            AttackType.ColumnDefs.Add("Effectiveness",
                new AttackType.ColumnDef("Effectiveness", true, "VARCHAR(8)", "Effectiveness", GetCellDataEffectiveness, GetSqlDataEffectiveness, AttackTypeCompareEffectiveness));


            MasterSwing.ColumnDefs.Clear();
            MasterSwing.ColumnDefs.Add("EncId", new MasterSwing.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.ParentEncounter.EncId; }, (Left, Right) => { return 0; }));
            MasterSwing.ColumnDefs.Add("Time", new MasterSwing.ColumnDef("Time", true, "TIMESTAMP", "STime", (Data) => { return Data.Time.ToString("T"); }, (Data) => { return Data.Time.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.Time.CompareTo(Right.Time); }));
            MasterSwing.ColumnDefs.Add("Attacker", new MasterSwing.ColumnDef("Attacker", true, "VARCHAR(64)", "Attacker", (Data) => { return Data.Attacker; }, (Data) => { return Data.Attacker; }, (Left, Right) => { return Left.Attacker.CompareTo(Right.Attacker); }));
            MasterSwing.ColumnDefs.Add("SwingType", new MasterSwing.ColumnDef("SwingType", false, "TINYINT", "SwingType", (Data) => { return Data.SwingType.ToString(); }, (Data) => { return Data.SwingType.ToString(); }, (Left, Right) => { return Left.SwingType.CompareTo(Right.SwingType); }));
            MasterSwing.ColumnDefs.Add("AttackType", new MasterSwing.ColumnDef("AttackType", true, "VARCHAR(64)", "AttackType", (Data) => { return Data.AttackType; }, (Data) => { return Data.AttackType; }, (Left, Right) => { return Left.AttackType.CompareTo(Right.AttackType); }));
            MasterSwing.ColumnDefs.Add("DamageType", new MasterSwing.ColumnDef("DamageType", true, "VARCHAR(64)", "DamageType", (Data) => { return Data.DamageType; }, (Data) => { return Data.DamageType; }, (Left, Right) => { return Left.DamageType.CompareTo(Right.DamageType); }));
            MasterSwing.ColumnDefs.Add("Victim", new MasterSwing.ColumnDef("Victim", true, "VARCHAR(64)", "Victim", (Data) => { return Data.Victim; }, (Data) => { return Data.Victim; }, (Left, Right) => { return Left.Victim.CompareTo(Right.Victim); }));
            MasterSwing.ColumnDefs.Add("DamageNum", new MasterSwing.ColumnDef("DamageNum", false, "INT", "Damage", (Data) => { return ((int)Data.Damage).ToString(); }, (Data) => { return ((int)Data.Damage).ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            //MasterSwing.ColumnDefs.Add("Damage", new MasterSwing.ColumnDef("Damage", true, "VARCHAR(128)", "DamageString", /* lambda */ (Data) => { return Data.Damage.ToString(); }, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            // As a C# lesson, the above lines(lambda expressions) can also be written as(anonymous methods):
            
            MasterSwing.ColumnDefs.Add("Damage",
                new MasterSwing.ColumnDef("Damage", true, "VARCHAR(128)", "DamageString", GetCellDataDamage, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));

            MasterSwing.ColumnDefs.Add("Critical", new MasterSwing.ColumnDef("Critical", true, "CHAR(1)", "Critical", /* anonymous */ delegate(MasterSwing Data) { return Data.Critical.ToString(); }, delegate(MasterSwing Data) { return Data.Critical.ToString(usCulture)[0].ToString(); }, delegate(MasterSwing Left, MasterSwing Right) { return Left.Critical.CompareTo(Right.Critical); }));
            // Or also written as(delegated methods):
            MasterSwing.ColumnDefs.Add("Special", new MasterSwing.ColumnDef("Special", true, "VARCHAR(64)", "Special", /* delegate */ GetCellDataSpecial, GetSqlDataSpecial, MasterSwingCompareSpecial));
            
            MasterSwing.ColumnDefs.Add("Flank",
                new MasterSwing.ColumnDef("Flank", true, "CHAR(1)", "Flank", GetCellDataFlank, GetSqlDataFlank, MasterSwingCompareFlank));
            MasterSwing.ColumnDefs.Add("BaseDamage",
                new MasterSwing.ColumnDef("BaseDamage", true, "INT", "BaseDamageString", GetCellDataBaseDamage, GetSqlDataBaseDamage, MasterSwingCompareBaseDamage));
            MasterSwing.ColumnDefs.Add("Effectiveness",
                new MasterSwing.ColumnDef("Effectiveness", true, "VARCHAR(8)", "EffectivenessString", GetCellDataEffectiveness, GetSqlDataEffectiveness, MasterSwingCompareEffectiveness));
            MasterSwing.ColumnDefs.Add("DmgToShield",
                new MasterSwing.ColumnDef("DmgToShield", false, "VARCHAR(128)", "DmgToShieldstring", GetCellDataDmgToShield, GetSqlDataDmgToShield, MasterSwingCompareDmgToShield));
            MasterSwing.ColumnDefs.Add("ShieldP",
                new MasterSwing.ColumnDef("ShieldP", false, "VARCHAR(8)", "ShieldPDtring", GetCellDataShieldP, GetSqlDataShieldP, MasterSwingCompareShieldP));


            ActGlobals.oFormActMain.ValidateLists();
            ActGlobals.oFormActMain.ValidateTableSetup();
        }

        #region ImportedFromEQ2Plugin

        // Needed to import this code to allow a setup from a blank state instead of the default state.
        // Blank state setup is the only way to enable the plugin when switching from some other plugin
        // that changed the default state.

        private string EncounterFormatSwitch(EncounterData Data, List<CombatantData> SelectiveAllies, string VarName, string Extra)
        {
            long damage = 0;
            long healed = 0;
            int swings = 0;
            int hits = 0;
            int crits = 0;
            int heals = 0;
            int critheals = 0;
            int cures = 0;
            int misses = 0;
            int hitfail = 0;
            float tohit = 0;
            double dps = 0;
            double hps = 0;
            long healstaken = 0;
            long damagetaken = 0;
            long powerdrain = 0;
            long powerheal = 0;
            int kills = 0;
            int deaths = 0;

            switch (VarName)
            {
                case "maxheal":
                    return Data.GetMaxHeal(true, false);
                case "MAXHEAL":
                    return Data.GetMaxHeal(false, false);
                case "maxhealward":
                    return Data.GetMaxHeal(true, true);
                case "MAXHEALWARD":
                    return Data.GetMaxHeal(false, true);
                case "maxhit":
                    return Data.GetMaxHit(true);
                case "MAXHIT":
                    return Data.GetMaxHit(false);
                case "duration":
                    return Data.DurationS;
                case "DURATION":
                    return Data.Duration.TotalSeconds.ToString("0");
                case "damage":
                    foreach (CombatantData cd in SelectiveAllies)
                        damage += cd.Damage;
                    return damage.ToString();
                case "damage-m":
                    foreach (CombatantData cd in SelectiveAllies)
                        damage += cd.Damage;
                    return (damage / 1000000.0).ToString("0.00");
                case "DAMAGE-k":
                    foreach (CombatantData cd in SelectiveAllies)
                        damage += cd.Damage;
                    return (damage / 1000.0).ToString("0");
                case "DAMAGE-m":
                    foreach (CombatantData cd in SelectiveAllies)
                        damage += cd.Damage;
                    return (damage / 1000000.0).ToString("0");
                case "healed":
                    foreach (CombatantData cd in SelectiveAllies)
                        healed += cd.Healed;
                    return healed.ToString();
                case "swings":
                    foreach (CombatantData cd in SelectiveAllies)
                        swings += cd.Swings;
                    return swings.ToString();
                case "hits":
                    foreach (CombatantData cd in SelectiveAllies)
                        hits += cd.Hits;
                    return hits.ToString();
                case "crithits":
                    foreach (CombatantData cd in SelectiveAllies)
                        crits += cd.CritHits;
                    return crits.ToString();
                case "crithit%":
                    foreach (CombatantData cd in SelectiveAllies)
                        crits += cd.CritHits;
                    foreach (CombatantData cd in SelectiveAllies)
                        hits += cd.Hits;
                    float critdamperc = (float)crits / (float)hits;
                    return critdamperc.ToString("0'%");
                case "heals":
                    foreach (CombatantData cd in SelectiveAllies)
                        heals += cd.Heals;
                    return heals.ToString();
                case "critheals":
                    foreach (CombatantData cd in SelectiveAllies)
                        critheals += cd.CritHits;
                    return critheals.ToString();
                case "critheal%":
                    foreach (CombatantData cd in SelectiveAllies)
                        critheals += cd.CritHeals;
                    foreach (CombatantData cd in SelectiveAllies)
                        heals += cd.Heals;
                    float crithealperc = (float)critheals / (float)heals;
                    return crithealperc.ToString("0'%");
                case "cures":
                    foreach (CombatantData cd in SelectiveAllies)
                        cures += cd.CureDispels;
                    return cures.ToString();
                case "misses":
                    foreach (CombatantData cd in SelectiveAllies)
                        misses += cd.Misses;
                    return misses.ToString();
                case "hitfailed":
                    foreach (CombatantData cd in SelectiveAllies)
                        hitfail += cd.Blocked;
                    return hitfail.ToString();
                case "TOHIT":
                    foreach (CombatantData cd in SelectiveAllies)
                        tohit += cd.ToHit;
                    tohit /= SelectiveAllies.Count;
                    return tohit.ToString("0");
                case "DPS":
                case "ENCDPS":
                    foreach (CombatantData cd in SelectiveAllies)
                        damage += cd.Damage;
                    dps = damage / Data.Duration.TotalSeconds;
                    return dps.ToString("0");
                case "DPS-k":
                case "ENCDPS-k":
                    foreach (CombatantData cd in SelectiveAllies)
                        damage += cd.Damage;
                    dps = damage / Data.Duration.TotalSeconds;
                    return (dps / 1000.0).ToString("0");
                case "ENCHPS":
                    foreach (CombatantData cd in SelectiveAllies)
                        healed += cd.Healed;
                    hps = healed / Data.Duration.TotalSeconds;
                    return hps.ToString("0");
                case "ENCHPS-k":
                    foreach (CombatantData cd in SelectiveAllies)
                        healed += cd.Healed;
                    hps = healed / Data.Duration.TotalSeconds;
                    return (hps / 1000.0).ToString("0");
                case "tohit":
                    foreach (CombatantData cd in SelectiveAllies)
                        tohit += cd.ToHit;
                    tohit /= SelectiveAllies.Count;
                    return tohit.ToString("F");
                case "dps":
                case "encdps":
                    foreach (CombatantData cd in SelectiveAllies)
                        damage += cd.Damage;
                    dps = damage / Data.Duration.TotalSeconds;
                    return dps.ToString("F");
                case "dps-k":
                case "encdps-k":
                    foreach (CombatantData cd in SelectiveAllies)
                        damage += cd.Damage;
                    dps = damage / Data.Duration.TotalSeconds;
                    return (dps / 1000.0).ToString("F");
                case "enchps":
                    foreach (CombatantData cd in SelectiveAllies)
                        healed += cd.Healed;
                    hps = healed / Data.Duration.TotalSeconds;
                    return hps.ToString("F");
                case "enchps-k":
                    foreach (CombatantData cd in SelectiveAllies)
                        healed += cd.Healed;
                    hps = healed / Data.Duration.TotalSeconds;
                    return (hps / 1000.0).ToString("F");
                case "healstaken":
                    foreach (CombatantData cd in SelectiveAllies)
                        healstaken += cd.HealsTaken;
                    return healstaken.ToString();
                case "damagetaken":
                    foreach (CombatantData cd in SelectiveAllies)
                        damagetaken += cd.DamageTaken;
                    return damagetaken.ToString();
                case "powerdrain":
                    foreach (CombatantData cd in SelectiveAllies)
                        powerdrain += cd.PowerDamage;
                    return powerdrain.ToString();
                case "powerheal":
                    foreach (CombatantData cd in SelectiveAllies)
                        powerheal += cd.PowerReplenish;
                    return powerheal.ToString();
                case "kills":
                    foreach (CombatantData cd in SelectiveAllies)
                        kills += cd.Kills;
                    return kills.ToString();
                case "deaths":
                    foreach (CombatantData cd in SelectiveAllies)
                        deaths += cd.Deaths;
                    return deaths.ToString();
                case "title":
                    return Data.Title;

                default:
                    return VarName;
            }
        }

        private string CombatantFormatSwitch(CombatantData Data, string VarName, string Extra)
        {
            int len = 0;
            switch (VarName)
            {
                case "name":
                    return Data.Name;
                case "NAME":
                    len = Int32.Parse(Extra);
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME3":
                    len = 3;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME4":
                    len = 4;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME5":
                    len = 5;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME6":
                    len = 6;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME7":
                    len = 7;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME8":
                    len = 8;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME9":
                    len = 9;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME10":
                    len = 10;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME11":
                    len = 11;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME12":
                    len = 12;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME13":
                    len = 13;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME14":
                    len = 14;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "NAME15":
                    len = 15;
                    return Data.Name.Length - len > 0 ? Data.Name.Remove(len, Data.Name.Length - len).Trim() : Data.Name;
                case "DURATION":
                    return Data.Duration.TotalSeconds.ToString("0");
                case "duration":
                    return Data.DurationS;
                case "maxhit":
                    return Data.GetMaxHit(true);
                case "MAXHIT":
                    return Data.GetMaxHit(false);
                case "maxheal":
                    return Data.GetMaxHeal(true, false);
                case "MAXHEAL":
                    return Data.GetMaxHeal(false, false);
                case "maxhealward":
                    return Data.GetMaxHeal(true, true);
                case "MAXHEALWARD":
                    return Data.GetMaxHeal(false, true);
                case "damage":
                    return Data.Damage.ToString();
                case "damage-k":
                    return (Data.Damage / 1000.0).ToString("0.00");
                case "damage-m":
                    return (Data.Damage / 1000000.0).ToString("0.00");
                case "DAMAGE-k":
                    return (Data.Damage / 1000.0).ToString("0");
                case "DAMAGE-m":
                    return (Data.Damage / 1000000.0).ToString("0");
                case "healed":
                    return Data.Healed.ToString();
                case "swings":
                    return Data.Swings.ToString();
                case "hits":
                    return Data.Hits.ToString();
                case "crithits":
                    return Data.CritHits.ToString();
                case "critheals":
                    return Data.CritHeals.ToString();
                case "crithit%":
                    return Data.CritDamPerc.ToString("0'%");
                case "fcrithit%":
                    return GetFilteredCritChance(Data).ToString("0'%");
                case "critheal%":
                    return Data.CritHealPerc.ToString("0'%");
                case "heals":
                    return Data.Heals.ToString();
                case "cures":
                    return Data.CureDispels.ToString();
                case "misses":
                    return Data.Misses.ToString();
                case "hitfailed":
                    return Data.Blocked.ToString();
                case "TOHIT":
                    return Data.ToHit.ToString("0");
                case "DPS":
                    return Data.DPS.ToString("0");
                case "DPS-k":
                    return (Data.DPS / 1000.0).ToString("0");
                case "ENCDPS":
                    return Data.EncDPS.ToString("0");
                case "ENCDPS-k":
                    return (Data.EncDPS / 1000.0).ToString("0");
                case "ENCHPS":
                    return Data.EncHPS.ToString("0");
                case "ENCHPS-k":
                    return (Data.EncHPS / 1000.0).ToString("0");
                case "tohit":
                    return Data.ToHit.ToString("F");
                case "dps":
                    return Data.DPS.ToString("F");
                case "dps-k":
                    return (Data.DPS / 1000.0).ToString("F");
                case "encdps":
                    return Data.EncDPS.ToString("F");
                case "encdps-k":
                    return (Data.EncDPS / 1000.0).ToString("F");
                case "enchps":
                    return Data.EncHPS.ToString("F");
                case "enchps-k":
                    return (Data.EncHPS / 1000.0).ToString("F");
                case "healstaken":
                    return Data.HealsTaken.ToString();
                case "damagetaken":
                    return Data.DamageTaken.ToString();
                case "powerdrain":
                    return Data.PowerDamage.ToString();
                case "powerheal":
                    return Data.PowerReplenish.ToString();
                case "kills":
                    return Data.Kills.ToString();
                case "deaths":
                    return Data.Deaths.ToString();
                case "damage%":
                    return Data.DamagePercent;
                case "healed%":
                    return Data.HealedPercent;
                case "threatstr":
                    return Data.GetThreatStr("Threat (Out)");
                case "threatdelta":
                    return Data.GetThreatDelta("Threat (Out)").ToString();
                case "n":
                    return "\n";
                case "t":
                    return "\t";

                default:
                    return VarName;
            }
        }

        private string GetCellDataSpecial(MasterSwing Data)
        {
            return Data.Special;
        }

        private string GetSqlDataSpecial(MasterSwing Data)
        {
            return Data.Special;
        }

        private int MasterSwingCompareSpecial(MasterSwing Left, MasterSwing Right)
        {
            return Left.Special.CompareTo(Right.Special);
        }

        private string GetAttackTypeSwingType(AttackType Data)
        {
            int swingType = 100;
            List<int> swingTypes = new List<int>();
            List<MasterSwing> cachedItems = new List<MasterSwing>(Data.Items);
            for (int i = 0; i < cachedItems.Count; i++)
            {
                MasterSwing s = cachedItems[i];
                if (swingTypes.Contains(s.SwingType) == false)
                    swingTypes.Add(s.SwingType);
            }
            if (swingTypes.Count == 1)
                swingType = swingTypes[0];

            return swingType.ToString();
        }

        private string GetDamageTypeGrouping(DamageTypeData Data)
        {
            string grouping = string.Empty;

            int swingTypeIndex = 0;
            if (Data.Outgoing)
            {
                grouping += "attacker=" + Data.Parent.Name;
                foreach (KeyValuePair<int, List<string>> links in CombatantData.SwingTypeToDamageTypeDataLinksOutgoing)
                {
                    foreach (string damageTypeLabel in links.Value)
                    {
                        if (Data.Type == damageTypeLabel)
                        {
                            grouping += String.Format("&swingtype{0}={1}", swingTypeIndex++ == 0 ? string.Empty : swingTypeIndex.ToString(), links.Key);
                        }
                    }
                }
            }
            else
            {
                grouping += "victim=" + Data.Parent.Name;
                foreach (KeyValuePair<int, List<string>> links in CombatantData.SwingTypeToDamageTypeDataLinksIncoming)
                {
                    foreach (string damageTypeLabel in links.Value)
                    {
                        if (Data.Type == damageTypeLabel)
                        {
                            grouping += String.Format("&swingtype{0}={1}", swingTypeIndex++ == 0 ? string.Empty : swingTypeIndex.ToString(), links.Key);
                        }
                    }
                }
            }

            return grouping;
        }

        private float GetFilteredCritChance(CombatantData Data)
        {
            List<AttackType> allAttackTypes = new List<AttackType>();
            List<AttackType> filteredAttackTypes = new List<AttackType>();

            foreach (KeyValuePair<string, AttackType> item in Data.Items["Outgoing Damage"].Items)
                allAttackTypes.Add(item.Value);
            foreach (KeyValuePair<string, AttackType> item in Data.Items["Healed (Out)"].Items)
                allAttackTypes.Add(item.Value);

            foreach (AttackType item in allAttackTypes)
            {
                if (item.Type == ActGlobals.ActLocalization.LocalizationStrings["attackTypeTerm-all"].DisplayedText)
                    continue;
                if (item.CritPerc == 0.0f)
                    continue;

                string damageType = string.Empty;
                bool cont = false;
                for (int i = 0; i < item.Items.Count; i++)
                {
                    string itemDamageType = item.Items[i].DamageType;
                    if (String.IsNullOrEmpty(damageType))
                    {
                        damageType = itemDamageType;
                    }
                    else
                    {
                        if (itemDamageType == "melee")
                            continue;
                        if (itemDamageType == "non-melee")
                            continue;
                        if (itemDamageType != damageType)
                        {
                            cont = true;
                            break;
                        }
                    }
                }
                if (cont)
                    continue;
                filteredAttackTypes.Add(item);
            }

            if (filteredAttackTypes.Count == 0)
                return float.NaN;
            else
            {
                float hits = 0;
                float critHits = 0;
                for (int i = 0; i < filteredAttackTypes.Count; i++)
                {
                    AttackType item = filteredAttackTypes[i];
                    hits += item.Hits;
                    critHits += item.CritHits;
                }
                float perc = critHits / hits;
                float ratio = hits / (float)Data.AllOut[ActGlobals.ActLocalization.LocalizationStrings["attackTypeTerm-all"].DisplayedText].Hits;
                //ActGlobals.oFormActMain.WriteDebugLog(String.Format("FCrit: {0} -> {1} / {2} = {3:0%} [{4:0%} data used]", Data.Name, critHits, hits, perc, ratio));
                if (perc == 1)
                {
                    if (ratio > 0.25f)
                        return 100;
                    else
                        return float.NaN;
                }
                if (ratio > 0.25f)
                    return (int)(perc * 100f);
                else
                    return float.NaN;
            }
        }

        #endregion ImportedFromEQ2Plugin

        void oFormActMain_LogFileChanged(bool IsImport, string NewLogFileName)
        {
            curActionTime = DateTime.MinValue;
            //purgePetCache();
            petOwnerRegistery.Clear();
            entityOwnerRegistery.Clear();
            magicMissileLastHit.Clear();
            unmatchedShieldLines.Clear();

            playersCharacterFound = false;
        }

        void oFormActMain_OnCombatEnd(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            curActionTime = DateTime.MinValue;

            // Don't actually want this.  Maybe on zone changes.
            // purgePetCache();

            // Don't clear this.  PvP encounters can be split while Chaotic Growth is active.
            // magicMissileLastHit.Clear();

            unmatchedShieldLines.Clear();

            entityOwnerRegistery.Clear();
            playersCharacterFound = false;
        }

        // Must match LogLineEventDelegate signature
        void oFormActMain_BeforeLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            parsedLineCount++;

            if (logInfo.logLine.Length < 30 || logInfo.logLine[19] != ':' || logInfo.logLine[20] != ':')
            {
                logInfo.detectedType = Color.DarkGray.ToArgb();
                errorLineCount++;
                return;
            }

            if (logInfo.detectedTime != curActionTime)
            {
                // Different times mean new action block, any pending actions won't be related to those of the new block
                curActionTime = logInfo.detectedTime;
            }

            ParsedLine pl = new ParsedLine(logInfo);

            if (pl.error)
            {
                logInfo.detectedType = Color.DarkGray.ToArgb();
                errorLineCount++;
                return;
            }

            // Fix up the ParsedLine to be easy to process.
            ProcessBasic(pl);

            // Detect Player names..
            if ( ! (playersCharacterFound || isImport ) )
            {
                if (pl.ownEntityType == EntityType.Player)
                {
                    if (playerCharacterNames.ContainsKey(pl.ownDsp))
                    {
                        ActGlobals.charName = pl.ownDsp;
                        playersCharacterFound = true;
                    }
                }
            }

            // Do the real stuff..
            ProcessAction(pl);
        }

        private void ProcessNamesOST(ParsedLine line)
        {
            // Owner, Source (belongs to owner), Target
            petOwnerRegistery.Register(line);
            entityOwnerRegistery.Register(line);

            ProcessOwnerSourceNames(line);
            ProcessTargetNames(line);
        }

        private void ProcessNamesST(ParsedLine line)
        {
            // Source, Target: All independant
            ProcessSourceNames(line);
            ProcessTargetNames(line);
        }

        private void ProcessNamesTargetOnly(ParsedLine line)
        {
            // Target only
            ProcessTargetNames(line);
        }

        private void ProcessBasic(ParsedLine line)
        {
            //
            // Fix up the ParsedLine.
            // Add calculated data fields to the ParsedLine.
            //


            if (line.ownDsp == "" && line.ownInt == "")
            {
                // Ugly fix for lines without an owner
                line.ownDsp = NW_Parser.unk;
                line.ownInt = NW_Parser.unkInt;
            }
            else if (line.ownInt[0] == 'P') { line.ownEntityType = EntityType.Player; }
            else if (line.ownInt[0] == 'C') 
            {
                // There should never be a Pet or Entity in this possition??
                line.ownEntityType = EntityType.Creature;
            }


            if (line.srcInt == "*")
            {
                line.srcDsp = line.ownDsp;
                line.srcInt = line.ownInt;
                line.srcEntityType = line.ownEntityType;
            }
            else if ((line.srcInt == "") && (line.srcDsp == ""))
            {
                //
                // // Ugly fix for lines without a source
                // // srcDsp = NW_Parser.unk;
                // // srcInt = NW_Parser.unkInt;
                // 
                // Ugly fix does not work.  See this valid example from a log:
                // "13:07:02:13:48:18.1::Kallista Hellbourne,P[200674407@288107 Kallista Hellbourne@tonyleon],,,Sentry,C[1150404 Frost_Goblin_Sentry],Storm Spell,Pn.Zh5vu,Lightning,ShowPowerDisplayName,580.333,0"
                // The Control Wizard effect Storm Spell seems to not have a source.  Should just use the owner in this case.
                // If the owner is unknown this will work as before.

                line.srcDsp = line.ownDsp;
                line.srcInt = line.ownInt;
                line.srcEntityType = line.ownEntityType;
            }
            else if (line.srcInt[0] == 'P')
            {
                line.srcEntityType = EntityType.Player;
            }
            else if (line.srcInt[0] == 'C')
            {
                // Basic Pet and Entity detection..

                if (line.srcInt.Contains(" Pet_"))
                {
                    line.srcEntityType = EntityType.Pet;
                }
                else if (line.srcInt.Contains(" Entity_"))
                {
                    line.srcEntityType = EntityType.Entity;
                }
                else
                {
                    line.srcEntityType = EntityType.Creature;
                }
            }

            if (line.tgtInt == "*")
            {
                line.tgtDsp = line.srcDsp;
                line.tgtInt = line.srcInt;
                line.tgtEntityType = line.srcEntityType;

                // If it is a Pet then the pet owner info needs to get set.
                // But first we can not do it here in case this is the first time we saw the owner.
                // Need to register owner of pet first...
            }
            else if ((line.tgtInt == "") && (line.tgtDsp == ""))
            {
                // Ugly fix for lines without a target
                line.tgtDsp = NW_Parser.unk;
                line.tgtInt = NW_Parser.unkInt;
            }
            else if (line.tgtInt[0] == 'P') { line.tgtEntityType = EntityType.Player; }
            else if (line.tgtInt[0] == 'C') 
            {
                // Basic Pet and Entity detection..

                if (line.tgtInt.Contains(" Pet_"))
                {
                    line.tgtEntityType = EntityType.Pet;
                }
                else if (line.tgtInt.Contains(" Entity_"))
                {
                    line.tgtEntityType = EntityType.Entity;
                }
                else
                {
                    line.tgtEntityType = EntityType.Creature;
                }
            }

            // Defaults for the clean names.
            line.encAttackerName = line.ownDsp;
            line.encTargetName = line.tgtDsp;
            line.unitAttackerName = line.ownDsp;
            line.unitTargetName = line.tgtDsp;
        }

        private void ProcessOwnerSourceNames(ParsedLine line)
        {
            // Owner default:
            line.encAttackerName = line.ownDsp;
            line.unitAttackerName = line.ownDsp;

            // We assume the owner is the owner of the source for this processing.

            if (line.srcEntityType == EntityType.Pet)
            {
                // Use the pet owner name for encounter name and filtering.
                line.encAttackerName = line.ownDsp;

                // Pet name:
                line.unitAttackerName = line.srcDsp + " [" + line.ownDsp + "'s Pet]";
                if (this.checkBox_mergePets.Checked)
                {
                    line.unitAttackerName = line.ownDsp;
                }
            }
            else if (line.ownEntityType == EntityType.Creature)
            {
                line.encAttackerName = line.ownDsp;
                String creatureId = line.ownInt.Split()[0].Substring(2);

                if (checkBox_mergeNPC.Checked)
                {
                    // Merge all NPCs to a single name.
                    line.unitAttackerName = line.ownDsp;
                }
                else
                {
                    // Separate each NPC with its unique creature ID added.
                    line.unitAttackerName = line.ownDsp + " [" + creatureId + "]";
                }
            }
        }

        private void ProcessSourceNames(ParsedLine line)
        {
            switch (line.srcEntityType)
            {
                case EntityType.Player:
                    {
                        line.encAttackerName = line.srcDsp;
                        line.unitAttackerName = line.srcDsp;
                        break;
                    }

                case EntityType.Pet:
                    {
                        OwnerInfo owner = petOwnerRegistery.Resolve(line.srcInt);

                        if (owner != null)
                        {

                            // Use the pet owner name for encounter name and filtering.
                            line.encAttackerName = owner.ownerDsp;

                            // Pet name:
                            line.unitAttackerName = line.srcDsp + " [" + owner.ownerDsp + "'s Pet]";
                            if (this.checkBox_mergePets.Checked)
                            {
                                line.unitAttackerName = owner.ownerDsp;
                            }
                        }
                        else
                        {
                            // Pet with unknown owner.
                            // Register it under UNKNOWN until it resolves.
                            line.encAttackerName = unk;
                            line.unitAttackerName = unk;
                        }
                        break;
                    }
                case EntityType.Entity:
                    {
                        OwnerInfo owner = entityOwnerRegistery.Resolve(line.srcInt);
                        if (owner != null)
                        {
                            if (owner.ownerEntityType == EntityType.Creature)
                            {
                                line.encAttackerName = owner.ownerDsp;

                                if (checkBox_mergeNPC.Checked)
                                {
                                    // Merge all NPCs to a single name.
                                    line.unitAttackerName = owner.ownerDsp;
                                }
                                else
                                {
                                    // Separate each NPC with its unique creature ID added.
                                    String creatureId = owner.ownerInt.Split()[0].Substring(2);
                                    line.unitAttackerName = owner.ownerDsp + " [" + creatureId + "]";
                                }
              
                            }
                            else
                            {
                                line.encAttackerName = owner.ownerDsp;
                                line.unitAttackerName = owner.ownerDsp;
                            }
                        }
                        else
                        {
                            line.encAttackerName = line.srcDsp;
                            line.unitAttackerName = line.srcDsp;
                        }
                        break;
                    }
                case EntityType.Creature:
                    {
                        line.encAttackerName = line.srcDsp;
                        
                        if (checkBox_mergeNPC.Checked)
                        {
                            // Merge all NPCs to a single name.
                            line.unitAttackerName = line.srcDsp;
                        }
                        else
                        {
                            // Separate each NPC with its unique creature ID added.
                            String creatureId = line.srcInt.Split()[0].Substring(2);
                            line.unitAttackerName = line.srcDsp + " [" + creatureId + "]";
                        }
              
                        break;
                    }

                // case ParsedLine.EntityType.Unknown:
                default:
                    {
                        // Use the defaults.
                        line.encAttackerName = line.srcDsp;
                        line.unitAttackerName = line.srcDsp;
                        break;
                    }
            }
        }

        private void ProcessTargetNames(ParsedLine line)
        {
            switch (line.tgtEntityType)
            {
                case EntityType.Player:
                    {
                        line.encTargetName = line.tgtDsp;
                        line.unitTargetName = line.tgtDsp;
                        break;
                    }

                case EntityType.Pet:
                    {
                        line.tgtOwnerInfo = petOwnerRegistery.Resolve(line.tgtInt);

                        if (line.tgtOwnerInfo != null)
                        {

                            // Use the pet owner name for encounter name and filtering.
                            line.encTargetName = line.tgtOwnerInfo.ownerDsp;

                            // Pet name:
                            line.unitTargetName = line.tgtDsp + " [" + line.tgtOwnerInfo.ownerDsp + "'s Pet]";
                            if (this.checkBox_mergePets.Checked)
                            {
                                line.unitTargetName = line.tgtOwnerInfo.ownerDsp;
                            }
                        }
                        else
                        {
                            // Pet with unknown owner.
                            // Register it under UNKNOWN until it resolves.
                            line.encTargetName = unk;
                            line.unitTargetName = unk;
                        }
                        break;
                    }
                case EntityType.Entity:
                    {
                        line.tgtOwnerInfo = entityOwnerRegistery.Resolve(line.tgtInt);
                        if (line.tgtOwnerInfo != null)
                        {
                            // What does this mean???
                        }
                        break;
                    }
                case EntityType.Creature:
                    {
                        if (line.tgtInt.Contains(" Trickster_Baitandswitch"))
                        {
                            // Bait and Switch
                            // 13:07:09:21:57:26.9::Dracnia,P[200787912@7184553 Dracnia@tminhtran],,*,Lodur,C[215 Trickster_Baitandswitch],Lashing Blade,Pn.Gji3ar1,Physical,Critical|Flank|Kill,14778.6,15481.4
                            // Not a pet...

                            line.encTargetName = line.tgtDsp;
                            line.unitTargetName = "Trickster [" + line.tgtDsp + "]";
                        }
                        else
                        {
                            line.encTargetName = line.tgtDsp;
                            String creatureId = line.tgtInt.Split()[0].Substring(2);

                            if (checkBox_mergeNPC.Checked)
                            {
                                // Merge all NPCs to a single name.
                                line.unitTargetName = line.tgtDsp;
                            }
                            else
                            {
                                // Separate each NPC with its unique creature ID added.
                                line.unitTargetName = line.tgtDsp + " [" + creatureId + "]";
                            }
                        }
                        break;
                    }

                // case ParsedLine.EntityType.Unknown:
                default:
                    {
                        // Use the defaults.
                        break;
                    }
            }
        }

        private void ProcessActionHeals(ParsedLine l)
        {
            int magAdj = (int)Math.Round(l.mag);
            int magBaseAdj = (int)Math.Round(l.magBase);

            l.logInfo.detectedType = l.critical ? Color.Green.ToArgb() : Color.DarkGreen.ToArgb();

            // NOTE: Do NOT use SetEncounter() on heals (i.e Non-Hostile Actions)

            // Heals can not start an encounter.
            if (ActGlobals.oFormActMain.InCombat)
            {
                ProcessNamesOST(l);

                // PVP Rune Heal - Needs some cleanup.  Use the player as the source since they grabbed it.
                // Does 'Pn.R0jdk' == PVP RUNE HEAL???
                // 13:07:09:14:00:23.2::Rune,C[317 Pvp_Rune_Heal],,*,Mus'Mugen Uhlaalaa,P[201045055@5998737 Mus'Mugen Uhlaalaa@bupfen],Heal,Pn.R0jdk,HitPoints,,-1136.92,0

                if (l.evtInt == "Pn.R0jdk") // Assume this is PVP Rune Heal for now...
                {   
                    AddCombatActionNW(
                        (int)SwingTypeEnum.Healing, l.critical, false, l.special, l.unitTargetName,
                        "PVP Heal Rune", new Dnum(-magAdj), -l.mag, -l.magBase, l.logInfo.detectedTime,
                        l.ts, l.unitTargetName, l.type);
                }
                else if (l.evtInt == "Pn.Hemuxg") // PvP Kill downed player
                {
                    // PVP finish off
                    // 13:07:10:09:13:09.2::CamierDerWeisse,P[200083978@5783571 CamierDerWeisse@faru2],,*,FIVEFINGERZ,P[200862049@7260841 FIVEFINGERZ@fivefingerz],Kill,Pn.Hemuxg,HitPoints,,0,0

                    // TODO:  Should this be recorded or ignored...
                }
                else if (l.evtInt == "Pn.Qiwkdx1") // Pretty sure this is end of pvp auto heal.
                {
                    // TODO: Make sure this is really only an end of pvp match auto heal.
                    // 13:07:10:11:03:42.1::Nephylia Necromon,P[201238857@7793332 Nephylia Necromon@nephodin],,*,,*,,Pn.Qiwkdx1,HitPoints,,-7240.66,0
                    // Ignore it.
                }
                else if (l.evtInt == "Pn.Dbm4um1") // Campfire
                {
                    // Camp fire.
                    // Give credit to the player for standing in it.
                    // Note: Trying to eliminate the [unknown] source.
                    // 13:07:10:11:02:20.6::,,,,Brandeor,P[201267923@5148411 Brandeor@brandeor],Campfire,Pn.Dbm4um1,HitPoints,,-525.321,0

                    AddCombatActionNW(
                        (int)SwingTypeEnum.Healing, l.critical, false, l.special, l.unitTargetName,
                        l.evtDsp, new Dnum(-magAdj), -l.mag, -l.magBase, l.logInfo.detectedTime,
                        l.ts, l.unitTargetName, l.type);                    
                }
                else if (l.evtInt == "Pn.Zrqjy1") // Chaotic Growth
                {
                    // Chaotic Growth - Proc debuff from CW Magic Missile.  Debuffed target AOE heals casters allies.
                    // But the log shows the debuffed target as the healer...
                    // Credit should go to the CW that casted the MM, but that is not clear in the logs.

                    // 13:07:09:20:52:51.5::Rassler,P[200973822@6215544 Rassler@lendal4],,*,Rhiyan Torr,P[200010914@5686857 Rhiyan Torr@wyvernonenine],Chaotic Growth,Pn.Zrqjy1,HitPoints,,-215,0

                    // NOTE:  Track the last person to hit each target with magic missile.  Give healing credit to that person.
                    //        IF that fails then it is a self heal...  Keeps it on the same team in pvp at least.

                    bool handled = false;
                    ChaoticGrowthInfo cgi = null;
                    if (magicMissileLastHit.TryGetValue(l.srcInt, out cgi))
                    {
                        if (!cgi.triggered)
                        {
                            cgi.triggered = true;
                            cgi.ts = l.logInfo.detectedTime;
                        }

                        // Use encounter names attacker and target here.  This allows filtering
                        // NOTE: Use SetEncounter() as this heal is part of a hostile action.
                        if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, cgi.encName, l.encTargetName))
                        {
                            AddCombatActionNW(
                                (int)SwingTypeEnum.Healing, l.critical, l.flank, l.unitAttackerName, cgi.unitName,
                                l.evtDsp, new Dnum(-magAdj), -l.mag, -l.magBase, l.logInfo.detectedTime,
                                l.ts, l.unitTargetName, l.type);
                        }

                        handled = true;
                    }

                    if (!handled)
                    {
                        // Use encounter names attacker and target here.  This allows filtering
                        // NOTE: Use SetEncounter() as this heal is part of a hostile action.
                        if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encTargetName, l.encTargetName))
                        {
                            AddCombatActionNW(
                                (int)SwingTypeEnum.Healing, l.critical, l.flank, l.unitAttackerName, unk,
                                l.evtDsp, new Dnum(-magAdj), -l.mag, -l.magBase, l.logInfo.detectedTime,
                                l.ts, l.unitTargetName, l.type);
                        }
                    }
                }
                else if (l.evtInt == "Pn.R1tsg4")
                {
                    // Shocking execution
                    // There is a HitPoints of value zero that is assioatied with shocking execution.
                    // Note that the <EvtInt> is different from the actual damaging log entry.
                    // Just ignore it...
                    // 13:07:17:10:33:02.1::Lodur,P[201093074@7545190 Lodur@lodur42],,*,KingOfSwordsx2,P[201247997@5290133 KingOfSwordsx2@sepherosrox],Shocking Execution,Pn.R1tsg4,HitPoints,,0,0

                }
                else
                {
                    // Default heal.

                    AddCombatActionNW(
                        (int)SwingTypeEnum.Healing, l.critical, l.flank, l.special, l.unitAttackerName,
                        l.attackType, new Dnum(-magAdj), -l.mag, -l.magBase, l.logInfo.detectedTime,
                        l.ts, l.unitTargetName, l.type);
                }
            }
        }

        private void ProcessActionShields(ParsedLine l)
        {
            int magAdj = (int)Math.Round(l.mag);
            int magBaseAdj = (int)Math.Round(l.magBase);

            // Shielding goes first and acts like a heal to cancel coming damage.  Attacker has his own damage line.  example:

            // 13:07:02:10:48:49.1::Neston,P[200243656@6371989 Neston@adamtech],,*,Flemming Fedtgebis,P[201082649@7532407 Flemming Fedtgebis@feehavregroed],Forgemaster's Flame,Pn.Lbf9ic,Shield,,-349.348,-154.608
            // 13:07:02:10:48:49.1::SorXian,P[201063397@7511146 SorXian@sorxian],,*,Flemming Fedtgebis,P[201082649@7532407 Flemming Fedtgebis@feehavregroed],Entangling Force,Pn.Oonws91,Shield,,-559.613,-247.663
            // 13:07:02:10:48:49.1::Neston,P[200243656@6371989 Neston@adamtech],,*,Flemming Fedtgebis,P[201082649@7532407 Flemming Fedtgebis@feehavregroed],Forgemaster's Flame,Pn.Lbf9ic,Radiant,,154.608,349.348
            // 13:07:02:10:48:49.1::SorXian,P[201063397@7511146 SorXian@sorxian],,*,Flemming Fedtgebis,P[201082649@7532407 Flemming Fedtgebis@feehavregroed],Entangling Force,Pn.Oonws91,Arcane,,247.663,559.613
            
            // NOTE:
            // Notice that the mag and magBase numbers are swap in the shield line verse the damage line.
            // Therefore the amount shield == magBase ???
            // The mag is meaningless ???
            // If mag > magBase on the attack is all damage not shielded ???  (ie high armor pen)

            // NOTE:
            // NW Patch on 7/17/2013 changed shield to report blocked damage in the mag field.
            // 13:07:18:10:25:54.2::Miner,C[1445 Mindflayer_Duergarminerthrall],,*,Largoevo,P[201228983@6531604 Largoevo@largoevo],Melee Attack,Pn.M7kie6,Shield,,-242.837,0
            // Actuall not sure on this....
            
            //
            // Target prevented damage.
            //

            l.logInfo.detectedType = l.critical ? Color.Green.ToArgb() : Color.DarkGreen.ToArgb();

            ProcessNamesOST(l);

            // Use encounter names attacker and target here.  This allows filtering
            // Hostile action triggered.  Use SetEncounter().
            if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encAttackerName, l.encTargetName))
            {
                // Put the attacker and the attack type in the special field.
                string special = l.unitAttackerName + " : " + l.attackType;

                Dnum shielded = null;
                float mag = 0;
                float magBase = 0;

                // This is just weird...
                if (l.magBase == 0) // Don't use magBaseAdj here.  Rounded to zero is not zero.
                {
                    shielded = new Dnum( -magAdj );
                    mag = -l.mag;
                    magBase = -l.magBase;
                }
                else
                {
                    shielded = new Dnum(-magBaseAdj);
                    mag = -l.magBase;
                    magBase = -l.mag;
                }

                // SwingType = Heal
                // special = attacker
                // attacker & victim = target
                MasterSwing ms = new MasterSwing(
                    (int)SwingTypeEnum.Healing,
                    l.critical, special, shielded, l.logInfo.detectedTime, l.ts, l.type, l.unitTargetName, l.type, l.unitTargetName);

                ms.Tags.Add("DamageF", mag);
                ms.Tags.Add("Flank", l.flank);

                ActGlobals.oFormActMain.AddCombatAction(ms);

                unmatchedShieldLines.AddShield(ms, l);
            }
        }

        private void ProcessActionCleanse(ParsedLine l)
        {
            l.logInfo.detectedType = Color.Blue.ToArgb();

            // Cleanse
            // 13:07:17:10:37:53.5::righteous,P[201081445@5908801 righteous@r1ghteousg],,*,KingOfSwordsx2,P[201247997@5290133 KingOfSwordsx2@sepherosrox],Cleanse,Pn.H8hm3x1,AttribModExpire,ShowPowerDisplayName,0,0

            if (ActGlobals.oFormActMain.InCombat)
            {
                ProcessNamesOST(l);

                AddCombatActionNW(
                    (int)SwingTypeEnum.CureDispel, l.critical, l.flank, l.special, 
                    l.unitAttackerName, l.attackType, Dnum.NoDamage, l.mag, l.magBase, 
                    l.logInfo.detectedTime, l.ts, l.unitTargetName, l.type );
            }
        }

        private void ProcessActionPower(ParsedLine l)
        {
            int magAdj = (int)Math.Round(l.mag);
            //int magBaseAdj = (int)Math.Round(l.magBase * 10);

            l.logInfo.detectedType = Color.Black.ToArgb();

            // NOTE: Do NOT use SetEncounter() on power (i.e Non-Hostile Actions)

            if (ActGlobals.oFormActMain.InCombat)
            {
                if (l.evtInt == "Pn.Ygyxld") // Critical Power
                {
                    // Critical Power
                    // 13:07:18:10:40:48.3::Tifa,P[200500793@6707245 Tifa@liliiith],Shard,C[2006 Entity_Shardoftheendlessavalanche],,*,Critical Power,Pn.Ygyxld,Power,,-0,0
                    // This power can trigger on CW's entities... These triggers should be ignored as they are zero effect.

                    if (l.ownInt != l.srcInt)
                    {
                        l.logInfo.detectedType = Color.Gray.ToArgb();
                        return;
                    }
                }

                if (l.evtInt == "Pn.He9xu") // Bait and Switch
                {
                    // TR - Bait and Switch Trigger
                    // special case: Bait and Switch
                    // 13:07:09:20:53:00.9::Lodur,C[835 Trickster_Baitandswitch],,*,Lodur,P[201093074@7545190 Lodur@lodur42],Trigger,Pn.He9xu,Power,,-0.521139,0
                    // 13:07:09:21:43:30.3::Lodur,C[152 Trickster_Baitandswitch],,*,Lodur,P[201093074@7545190 Lodur@lodur42],Trigger,Pn.He9xu,Power,Immune,0,0
                    // 13:07:10:09:11:08.8::Lodur,C[178 Trickster_Baitandswitch],,*,Lodur,P[201093074@7545190 Lodur@lodur42],Trigger,Pn.He9xu,Power,Immune,0,0

                    ProcessNamesTargetOnly(l);

                    // Target is the source as well.

                    AddCombatActionNW(
                        (int)SwingTypeEnum.PowerHealing, l.critical, false, "", "Trickster [" + l.tgtDsp + "]",
                        "Bait and Switch", new Dnum(-magAdj), -l.mag, 0, l.logInfo.detectedTime,
                        l.ts, l.tgtDsp, l.type);

                }
                else if (l.evtInt == "Pn.Jy04um1") // Guard Break
                {
                    // Guard Break
                    // 13:07:18:10:50:08.7::Largoevo,P[201228983@6531604 Largoevo@largoevo],Bodyguard,C[2175 Mindflayer_Thoonhulk_Eventbodyguard],Largoevo,P[201228983@6531604 Largoevo@largoevo],Guard Break,Pn.Jy04um1,Power,,-28.8571,0
                    // Owner    = Guardian Fighter  [Attacker]
                    // source   = Target enemy      [Special]
                    // target   = Guardian Fighter  [Victim]
                    // 
                    // NOTE: Do not assume source is a pet of owner.  Source could be a pet or fake pet.  Resolve fake pet to owner.

                    ProcessNamesST(l);

                    AddCombatActionNW(
                        (int)SwingTypeEnum.PowerHealing, l.critical, false, l.unitAttackerName, l.unitTargetName,
                        l.evtDsp, new Dnum(-magAdj), -l.mag, 0, l.logInfo.detectedTime,
                        l.ts, l.unitTargetName, l.type);
                }
                else if (l.evtInt == "Pn.Wxao05") // Maelstrom of Chaos
                {
                    // Maelstrom of Chaos
                    // 13:07:18:10:37:50.5::Tifa,P[200500793@6707245 Tifa@liliiith],,*,,*,Maelstrom of Chaos,Pn.Wxao05,Power,,500,0
                    // Canceling this power early will cost half of your Action Points.

                    // Ignore this for now.
                }
                else
                {
                    // Normal Power case...
                    ProcessNamesOST(l);

                    AddCombatActionNW(
                        (int)SwingTypeEnum.PowerHealing, l.critical, l.flank, l.special,
                        l.unitAttackerName, l.attackType, new Dnum(-magAdj), -l.mag, -l.magBase,
                        l.logInfo.detectedTime, l.ts, l.unitTargetName, l.type);
                }
            }
        }

        private void ProcessActionSPDN(ParsedLine l)
        {
            // Handle all the buff and proc buffs/debuffs
            // type: PowerRecharge, Null, Alacrity, CombatAdvantage, Lightning(Storm Spell), CritSeverity, ...

            l.logInfo.detectedType = Color.DarkTurquoise.ToArgb();

            if (l.evtInt == "Pn.Fwolu") // Chaotic Growth
            {
                // Chaotic Growth (Fixed in latest NW patch)
                // 13:07:18:10:51:58.2::Tifa,P[200500793@6707245 Tifa@liliiith],,*,Guard,C[2205 Mindflayer_Duergarguardthrall],Chaotic Growth,Pn.Fwolu,Null,ShowPowerDisplayName,0,0

                l.logInfo.detectedType = Color.DarkOliveGreen.ToArgb();

                ProcessNamesOST(l);

                ChaoticGrowthInfo cgi = null;
                if (magicMissileLastHit.TryGetValue(l.tgtInt, out cgi))
                {
                    cgi.triggered = true;
                    cgi.ts = l.logInfo.detectedTime;
                    cgi.encName = l.encAttackerName;
                    cgi.unitName = l.unitAttackerName;
                }

                if (ActGlobals.oFormActMain.InCombat)
                {
                    AddCombatActionHostile(l, (int)SwingTypeEnum.NonMelee, l.critical, l.special, l.attackType, Dnum.NoDamage, 0, l.type);
                }
            }
            else if (l.evtInt == "Pn.Zh5vu")
            {
                // Storm Spell
                // 13:07:18:10:49:10.1::Tifa,P[200500793@6707245 Tifa@liliiith],,*,Scourge,C[2143 Mindflayer_Scourge],Storm Spell,Pn.Zh5vu,Lightning,ShowPowerDisplayName,583.917,0

                // Ignore this as there is a damage log line to go with it.
            }
            else if (injuryTypes.ContainsKey(l.evtInt))
            {
                // Injure...
                
                // Ignore this as it is not reall part of combat.
            }
            else
            {
                // Default

                if (ActGlobals.oFormActMain.InCombat)
                {
                    ProcessNamesOST(l);
                    AddCombatActionHostile(l, (int)SwingTypeEnum.NonMelee, l.critical, l.special, l.attackType, Dnum.NoDamage, 0, l.type);
                }
            }
        }

        private void ProcessActionDamage(ParsedLine l)
        {
            int magAdj = (int)Math.Round(l.mag);
            int magBaseAdj = (int)Math.Round(l.magBase);

            l.logInfo.detectedType = l.critical ? Color.Red.ToArgb() : Color.DarkRed.ToArgb();

            string special = l.special;

            MasterSwing msShielded = unmatchedShieldLines.MatchDamage(l);
            if (msShielded != null)
            {
                // Fix up the shield line.
                // Tags are about the only thing that can be altered on MS that is already added via AddCombatAction.
                // So do most of it with adding Tags.

                // Shield line:  add column for attack damage and % blocked
                // Attack line:  add amount shielded to the 'special' column.

                object val;
                if (msShielded.Tags.TryGetValue("DamageF", out val))
                {
                    float df = (float) val;
                    string shieldSpecialText = "Shield(" + df.ToString("F1") + ")";

                    if (special == "None")
                    {
                        special = shieldSpecialText;
                    }
                    else
                    {
                        special = l.special + " | " + shieldSpecialText;
                    }

                    float shielded = df / l.mag;
                    msShielded.Tags.Add("ShieldDmgF", l.mag);
                    msShielded.Tags.Add("ShieldP", shielded);
                }
            }

            if (l.evtInt == "Autodesc.Combatevent.Falling")
            {
                // Falling damage does not start combat...
                if (ActGlobals.oFormActMain.InCombat)
                {
                    ProcessNamesOST(l);
                    AddCombatActionHostile(l, l.swingType, l.critical, special, l.attackType, magAdj, l.mag, l.type, l.magBase);
                }
            }
            else if (l.evtInt == "Pn.Wypyjw1") // Knight's Valor,
            {
                // "13:07:18:10:30:48.3::Largoevo,P[201228983@6531604 Largoevo@largoevo],Ugan the Abominable,C[1469 Mindflayer_Miniboss_Ugan],Largoevo,P[201228983@6531604 Largoevo@largoevo],Knight's Valor,Pn.Wypyjw1,Physical,,449.42,1195.48
                // Attack goes SRC -> TRG and ignore the owner.  The SRC is not the owner's pet.

                ProcessNamesST(l);
                AddCombatActionHostile(l, l.swingType, l.critical, special, l.attackType, magAdj, l.mag, l.type, l.magBase);
            }
            else
            {
                ProcessNamesOST(l);

                if ((l.evtInt == "Pn.3t6cw8") && (magAdj > 0)) // Magic Missile
                {
                    ChaoticGrowthInfo cgi = null;
                    if (magicMissileLastHit.TryGetValue(l.tgtInt, out cgi))
                    {
                        if (cgi.triggered)
                        {
                            TimeSpan t = l.logInfo.detectedTime - cgi.ts;
                            if (t.TotalSeconds > 10.0)
                            {
                                cgi.triggered = false;
                            }
                        }

                        if (!cgi.triggered)
                        {
                            cgi.encName = l.encAttackerName;
                            cgi.unitName = l.unitAttackerName;
                            cgi.ts = l.logInfo.detectedTime;
                        }
                    }
                    else
                    {
                        cgi = new ChaoticGrowthInfo();
                        cgi.encName = l.encAttackerName;
                        cgi.unitName = l.unitAttackerName;
                        cgi.triggered = false;
                        cgi.ts = l.logInfo.detectedTime;

                        magicMissileLastHit.Add(l.tgtInt, cgi);
                    }
                }

                //
                // Note:  There seems to be many cases where dmgBase == 0 while damage is applied.
                //

                /*
                if (l.flags.Contains("Miss"))
                {
                    // TODO:  Not sure I have ever seen a "miss" in a log.  This actually valid?
                    AddCombatActionHostile(l, l.swingType, l.critical, l.special, l.attackType, Dnum.Miss, l.type, magBaseAdj);
                }
                else 
                */

                if (l.immune)
                {
                    if ((magAdj == 0) && (magBaseAdj == 0))
                    {
                        // 13:07:18:10:49:21.6::Tristan,C[2120 Pet_Dog],,*,Oll'noth the Dominator,C[1997 Mindflayer_Eventboss],Takedown,Pn.Ebxsjf,KnockBack,Immune,0,0

                        // Ignore these for now...
                        l.logInfo.detectedType = Color.Gray.ToArgb();
                    }
                    else
                    {
                        // Generally damaging attacks have mag=0 and magBase > 0 when Immune.
                        l.logInfo.detectedType = Color.Maroon.ToArgb();
                        AddCombatActionHostile(l, l.swingType, l.critical, special, l.attackType, Dnum.NoDamage, l.mag, l.type, l.magBase);
                    }
                }
                else if (l.dodge)
                {
                    // It really looks like Dodge does not stop all damage - just reduces it by about 80%...
                    // I have seen damaging attacks that are both Dodge and Kill in the flags.
                    // So the target dodged but still died.
                    l.logInfo.detectedType = Color.Maroon.ToArgb();
                    AddCombatActionHostile(l, l.swingType, l.critical, special, l.attackType, magAdj, l.mag, l.type, l.magBase);
                }
                else
                {
                    if ((magAdj == 0) && (magBaseAdj == 0))
                    {
                        // Ignore it...  This is generally a Non-Target entity getting AOE'd...
                        l.logInfo.detectedType = Color.Gray.ToArgb();
                    }
                    else
                    {
                        // NOT All attacks have a magBase (anymore).
                        AddCombatActionHostile(l, l.swingType, l.critical, special, l.attackType, magAdj, l.mag, l.type, l.magBase);
                    }
                }
            }
        }

        private void ProcessAction(ParsedLine l)
        {
            l.logInfo.detectedType = Color.Gray.ToArgb();

            if (l.type == "HitPoints")
            {
                ProcessActionHeals(l);
            }
            else if (l.type == "Shield")
            {
                ProcessActionShields(l);
            }
            else if (l.type == "AttribModExpire") // Cleanse
            {
                ProcessActionCleanse(l);
            }
            else if (l.type == "Power")
            {
                ProcessActionPower(l);
            }
            else if (l.showPowerDisplayName)
            {
                // Non-damaging effects.
                ProcessActionSPDN(l);
            }
            else
            {
                // What is left should all be damage.
                ProcessActionDamage(l);
            }

            // add action Killing
            if (l.kill)
            {
                l.logInfo.detectedType = Color.Fuchsia.ToArgb();

                // Clean from last MM hit.
                // The Kill can come right before a proc.  Ordering isssue.
                // magicMissileLastHit.Remove(l.tgtInt);

                // TODO: use tgtDsp or unitTargetName?
                ActGlobals.oFormSpellTimers.RemoveTimerMods(l.tgtDsp);
                ActGlobals.oFormSpellTimers.DispellTimerMods(l.tgtDsp);

                // No "Killing : Flank" ever.  Doesn't make sense since there is no damage in the kill tracking.
                // And it messes up the kill counts.
                // AddCombatActionHostile(l, l.swingType, l.critical, l.special, "Killing", Dnum.Death, l.type);

                // Use encounter names attacker and target here.  This allows filtering
                if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encAttackerName, l.encTargetName))
                {
                    MasterSwing ms = 
                        new MasterSwing(l.swingType, l.critical, l.special, Dnum.Death, l.logInfo.detectedTime, l.ts, 
                            "Killing", l.unitAttackerName, "Death", l.unitTargetName);
                    ms.Tags.Add("Flank", l.flank);
                    ActGlobals.oFormActMain.AddCombatAction(ms);
                }
            }
        }

        // For hostile actions only.  Handles the SetEncounter().
        private void AddCombatActionHostile(
            ParsedLine line, int swingType, bool critical, string special, string theAttackType, Dnum Damage, float realDamage, string theDamageType, float baseDamage=0)
        {
            // Use encounter names attacker and target here.  This allows filtering
            if (ActGlobals.oFormActMain.SetEncounter(line.logInfo.detectedTime, line.encAttackerName, line.encTargetName))
            {
                // add Flank to AttackType if setting is set
                string tempAttack = theAttackType;
                if (line.flank && this.checkBox_flankSkill.Checked) tempAttack = theAttackType + ": Flank";

                AddCombatActionNW(
                    swingType, line.critical, line.flank, special, line.unitAttackerName,
                    tempAttack, Damage, realDamage, baseDamage, line.logInfo.detectedTime,
                    line.ts, line.unitTargetName, theDamageType);
            }
        }

        // Wrapper around AddCombatAction to add extra Tags that are used in the NW plugin.
        private void AddCombatActionNW(
            int swingType, bool critical, bool flank, string special, string attacker, string theAttackType, 
            Dnum damage, float realDamage, float baseDamage,
            DateTime time, int timeSorter, string victim, string theDamageType)
        {
            MasterSwing ms = new MasterSwing(swingType, critical, special, damage, time, timeSorter, theAttackType, attacker, theDamageType, victim);

            ms.Tags.Add("DamageF", realDamage);
            ms.Tags.Add("BaseDamage", baseDamage);
            ms.Tags.Add("Flank", flank);

            if (baseDamage > 0)
            {
                float eff = realDamage / baseDamage;
                ms.Tags.Add("Effectiveness", eff);
            }

            ActGlobals.oFormActMain.AddCombatAction(ms);
        }

        // Must match the DateTimeLogParser delegate signature
        private DateTime ParseDateTime(string FullLogLine)
        {
            if (FullLogLine.Length >= 21 && FullLogLine[19] == ':' && FullLogLine[20] == ':')
            {
                return DateTime.ParseExact(FullLogLine.Substring(0, 19), "yy':'MM':'dd':'HH':'mm':'ss'.'f", System.Globalization.CultureInfo.InvariantCulture); ;
            }
            return ActGlobals.oFormActMain.LastKnownTime;
        }

        public void DeInitPlugin()
        {
            ActGlobals.oFormActMain.BeforeLogLineRead -= oFormActMain_BeforeLogLineRead;
            ActGlobals.oFormActMain.OnCombatEnd -= oFormActMain_OnCombatEnd;
            ActGlobals.oFormActMain.LogFileChanged -= oFormActMain_LogFileChanged;

            if (optionsNode != null)
            {
                optionsNode.Remove();
                ActGlobals.oFormActMain.OptionsControlSets.Remove(@"Neverwinter\General");
            }

            for (int i = 0; i < ActGlobals.oFormActMain.OptionsTreeView.Nodes.Count; i++)
            {
                if (ActGlobals.oFormActMain.OptionsTreeView.Nodes[i].Text == "Neverwinter")
                    ActGlobals.oFormActMain.OptionsTreeView.Nodes[i].Remove();
            }

            SaveSettings();

            unmatchedShieldLines.Clear();
            entityOwnerRegistery.Clear();
            petOwnerRegistery.Clear();
            magicMissileLastHit.Clear();

            lblStatus.Text = "Neverwinter ACT plugin unloaded";
        }

        // Load option settings from file
        void LoadSettings()
        {

            xmlSettings.AddControlSetting(checkBox_mergeNPC.Name, checkBox_mergeNPC);
            xmlSettings.AddControlSetting(checkBox_mergePets.Name, checkBox_mergePets);
            xmlSettings.AddControlSetting(checkBox_flankSkill.Name, checkBox_flankSkill);
            xmlSettings.AddControlSetting(listBox_players.Name, listBox_players);

            if (File.Exists(settingsFile))
            {
                FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlTextReader xReader = new XmlTextReader(fs);

                try
                {
                    while (xReader.Read())
                    {
                        if (xReader.NodeType == XmlNodeType.Element)
                        {
                            if (xReader.LocalName == "SettingsSerializer")
                            {
                                xmlSettings.ImportFromXml(xReader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error loading settings: " + ex.Message;
                }
                xReader.Close();
            }

            foreach (string i in listBox_players.Items)
            {
                playerCharacterNames.Add(i.ToString(), true);
            }
        }

        void SaveSettings()
        {
            FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8);
            xWriter.Formatting = Formatting.Indented;
            xWriter.Indentation = 1;
            xWriter.IndentChar = '\t';
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");	// <Config>
            xWriter.WriteStartElement("SettingsSerializer");	// <Config><SettingsSerializer>
            xmlSettings.ExportToXml(xWriter);	// Fill the SettingsSerializer XML
            xWriter.WriteEndElement();	// </SettingsSerializer>
            xWriter.WriteEndElement();	// </Config>
            xWriter.WriteEndDocument();	// Tie up loose ends (shouldn't be any)
            xWriter.Flush();	// Flush the file buffer to disk
            xWriter.Close();
        }

        private void button_add_Click(object sender, EventArgs e)
        {
            string name = textBox_player.Text;
            if ( ! listBox_players.Items.Contains(name) )
            {
                listBox_players.Items.Add(name);
                playerCharacterNames.Add(name, true);
                textBox_player.Clear();
            }
        }

        private void button_remove_Click(object sender, EventArgs e)
        {
            string name = textBox_player.Text;
            if (listBox_players.Items.Contains(name))
            {
                listBox_players.Items.Remove(name);
                playerCharacterNames.Remove(name);
                textBox_player.Clear();
            }
        }

        private void button_clearAll_Click(object sender, EventArgs e)
        {
            listBox_players.Items.Clear();
            playerCharacterNames.Clear();
            textBox_player.Clear();
        }

        private void listBox_players_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_players.SelectedIndex != -1)
            {
                textBox_player.Text = listBox_players.SelectedItem.ToString();
            }
        }

        private void control_MouseLeave(object sender, EventArgs e)
        {
            ActGlobals.oFormActMain.SetOptionsHelpText(String.Empty);
        }

        private void textBox_player_TextChanged(object sender, EventArgs e)
        {
            bool not_empty = (this.textBox_player.Text.Length > 0);
            this.button_remove.Enabled = this.button_add.Enabled = not_empty;

            if (!not_empty)
            {
                listBox_players.SelectedIndex = -1;
            }
        }

        private void playerNameControls_MouseEnter(object sender, EventArgs e)
        {
            ActGlobals.oFormActMain.SetOptionsHelpText("Add the names of your player characters.  This allows ACT to detect which player character is yours.  Spelling and capitalization must be exact.");
        }

        private void checkBox_mergeNPC_MouseEnter(object sender, EventArgs e)
        {
            ActGlobals.oFormActMain.SetOptionsHelpText("Select this option to merge NPC combatants by name.  This removes the instance number from the combatant name.  For example Orc [1], Orc [2], ... Orc [n] are all merged in Orc");
        }

        private void checkBox_mergePets_MouseEnter(object sender, EventArgs e)
        {
            ActGlobals.oFormActMain.SetOptionsHelpText("Merge a player's pet with the player and remove the pet as a combatant.");
        }

        private void checkBox_flankSkill_MouseEnter(object sender, EventArgs e)
        {
            ActGlobals.oFormActMain.SetOptionsHelpText("Separate flank hits in to separate abilities named \"<ability-name> : Flank\"");
        }
    }

    internal class OwnerInfo
    {
        public string ownerDsp;
        public string ownerInt;
        public EntityType ownerEntityType;
        public string petDsp;
        public string petInt;
    }

    internal interface OwnerRegistery
    {
        void Clear();
        void Register(ParsedLine line);
        OwnerInfo Resolve(string nameInt);
    }

    internal class PetOwnerRegistery : OwnerRegistery
    {
        private Dictionary<string, OwnerInfo> petPlayerCache = new Dictionary<string, OwnerInfo>();
        private Dictionary<string, OwnerInfo> playerPetCache = new Dictionary<string, OwnerInfo>();

        public void Clear()
        {
            petPlayerCache.Clear();
            playerPetCache.Clear();
        }

        public void Register(ParsedLine line)
        {
            // Problem lines:
            // "13:07:02:13:48:18.1::Kallista Hellbourne,P[200674407@288107 Kallista Hellbourne@tonyleon],,,Sentry,C[1150404 Frost_Goblin_Sentry],Storm Spell,Pn.Zh5vu,Lightning,ShowPowerDisplayName,580.333,0"
            // "13:07:03:11:47:12.6::Grizzard,P[200743305@6022049 Grizzard@shamedy],Cutter,C[9395 Winterforge_Frost_Goblin_Cutter],Grizzard,P[200743305@6022049 Grizzard@shamedy],Guard Break,Pn.Jy04um1,Power,,-23.1135,0"
            // "13:07:09:11:01:08.4::Correk,P[201028460@1546238 Correk@Gleyvien],Target Dummy,C[265291 Entity_Targetdummy],,*,Doom!,Pn.F1j0yx1,Radiant,Critical,10557.2,8445.78"
            // "13:07:09:11:01:59.5::Nasus king,P[201132249@7587600 Nasus king@portazorras],SerGay,C[265715 Pet_Clericdisciple],Target Dummy,C[265291 Entity_Targetdummy],Sacred Flame,Pn.Tegils,Physical,Flank,59.7605,0"
            
            bool add = false;
            OwnerInfo OwnerInfo = null;

            // Record owner of all pets we see.
            if (line.srcEntityType == EntityType.Pet)
            {
                if (petPlayerCache.TryGetValue(line.srcInt, out OwnerInfo))
                {
                    if (OwnerInfo.ownerInt != line.ownInt)
                    {
                        // Pet Owner changed...  Not sure if possible... but just in case.
                        petPlayerCache.Remove(OwnerInfo.petInt);
                        playerPetCache.Remove(OwnerInfo.ownerInt);
                        add = true;
                    }
                }
                else
                {
                    // Check if this player had another pet registered and clean up.
                    // Only one pet is allowed.
                    if (playerPetCache.TryGetValue(line.ownInt, out OwnerInfo))
                    {
                        playerPetCache.Remove(line.ownInt);
                        petPlayerCache.Remove(OwnerInfo.petInt);
                    }

                    add = true;
                }

                if (add)
                {
                    OwnerInfo = new OwnerInfo();
                    OwnerInfo.ownerDsp = line.ownDsp;
                    OwnerInfo.ownerInt = line.ownInt;
                    OwnerInfo.ownerEntityType = line.ownEntityType;
                    OwnerInfo.petDsp = line.srcDsp;
                    OwnerInfo.petInt = line.srcInt;

                    petPlayerCache.Add(line.srcInt, OwnerInfo);
                    playerPetCache.Add(line.ownInt, OwnerInfo);
                }
            }
        }

        public OwnerInfo Resolve(string nameInt)
        {
            // Lookup the creature to see if it is a pet.

            OwnerInfo petOwner = null;
            if (petPlayerCache.TryGetValue(nameInt, out petOwner))
            {
                return petOwner;
            }
            
            return null;
        }

    }

    internal class EntityOwnerRegistery : OwnerRegistery
    {
        private Dictionary<string, OwnerInfo> entityPlayerCache = new Dictionary<string, OwnerInfo>();

        public void Clear()
        {
            entityPlayerCache.Clear();
        }

        public void Register(ParsedLine line)
        {
            bool add = false;
            OwnerInfo OwnerInfo = null;

            // Record owner of all entities we see.
            if (line.srcEntityType == EntityType.Entity)
            {
                if (entityPlayerCache.TryGetValue(line.srcInt, out OwnerInfo))
                {
                    if (OwnerInfo.ownerInt != line.ownInt)
                    {
                        // Pet Owner changed...  Not sure if possible... but just in case.
                        entityPlayerCache.Remove(OwnerInfo.petInt);
                        add = true;
                    }
                }
                else
                {
                    // Multiple entities may be owned by same owner.
                    add = true;
                }

                if (add)
                {
                    OwnerInfo = new OwnerInfo();
                    OwnerInfo.ownerDsp = line.ownDsp;
                    OwnerInfo.ownerInt = line.ownInt;
                    OwnerInfo.ownerEntityType = line.ownEntityType;
                    OwnerInfo.petDsp = line.srcDsp;
                    OwnerInfo.petInt = line.srcInt;

                    entityPlayerCache.Add(line.srcInt, OwnerInfo);
                }
            }
        }

        public OwnerInfo Resolve(string nameInt)
        {
            // Lookup the creature to see if it is a pet.

            OwnerInfo petOwner = null;
            if (entityPlayerCache.TryGetValue(nameInt, out petOwner))
            {
                return petOwner;
            }

            return null;
        }
    }

    internal class ChaoticGrowthInfo
    {
        public string encName;
        public string unitName;
        public bool triggered;
        public DateTime ts;
    }

    internal class ShieldLine
    {
        public MasterSwing ms;
        public ParsedLine line;
    }

    internal class UnmatchedShieldLines
    {
        // Added new lines to end to maintain FIFO/Time ordering.
        // Should act as a FIFO if 100% matches.
        private LinkedList<ShieldLine> active = new LinkedList<ShieldLine>();

        public UnmatchedShieldLines()
        {
        }

        public void Clear()
        {
            active.Clear();
        }

        public void AddShield(MasterSwing ms, ParsedLine line)
        {
            ShieldLine sl = new ShieldLine();
            sl.ms = ms;
            sl.line = line;

            active.AddLast(sl);
        }

        public MasterSwing MatchDamage(ParsedLine line)
        {
            LinkedListNode<ShieldLine> slnNext = active.First;

            while (slnNext != null)
            {
                LinkedListNode<ShieldLine> cur = slnNext;
                ShieldLine sl = cur.Value;
                slnNext = slnNext.Next;

                // Examaple:
                // 13:09:26:09:31:31.2::Lorne Fellbane,P[201332730@6101294 Lorne Fellbane@todesfaelle],,*,HeLLCaT,P[201327748@7398668 HeLLCaT@phantom3535],Bilethorn Weapon,Pn.Nhw1351,Shield,,-1.3,0
                // 13:09:26:09:31:31.2::Lorne Fellbane,P[201332730@6101294 Lorne Fellbane@todesfaelle],,*,HeLLCaT,P[201327748@7398668 HeLLCaT@phantom3535],Bilethorn Weapon,Pn.Nhw1351,Shield,,-10.9585,0
                // 13:09:26:09:31:31.2::Lorne Fellbane,P[201332730@6101294 Lorne Fellbane@todesfaelle],HeLLCaT,P[201327748@7398668 HeLLCaT@phantom3535],,*,Bilethorn Weapon,Pn.Nhw1351,Poison,,1.3,6.5
                // 13:09:26:09:31:31.2::Lorne Fellbane,P[201332730@6101294 Lorne Fellbane@todesfaelle],HeLLCaT,P[201327748@7398668 HeLLCaT@phantom3535],,*,Bilethorn Weapon,Pn.Nhw1351,Poison,,10.9585,54.7925

                // Notice that the Source field doesn't always match...

                /* Odd case of zero damage:
13:09:26:09:36:11.8::briserus,P[201401849@8271148 briserus@briserus],,*,Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],Flaming Weapon,Pn.Mzftfj,Shield,,-2,-0.484209
13:09:26:09:36:11.8::briserus,P[201401849@8271148 briserus@briserus],,*,Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],Flaming Weapon,Pn.Mzftfj,Shield,,-2,-0.242104
13:09:26:09:36:11.8::briserus,P[201401849@8271148 briserus@briserus],,*,Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],Flaming Weapon,Pn.Mzftfj,Fire,Dodge,0,0
13:09:26:09:36:11.8::briserus,P[201401849@8271148 briserus@briserus],,*,Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],Flaming Weapon,Pn.Mzftfj,Fire,,0,0
13:09:26:09:36:11.8::briserus,P[201401849@8271148 briserus@briserus],,*,Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],Flaming Weapon,Pn.Mzftfj,Fire,,0.484209,2
13:09:26:09:36:11.8::briserus,P[201401849@8271148 briserus@briserus],,*,Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],Flaming Weapon,Pn.Mzftfj,Fire,Dodge,0.242104,2
                */

                /* Two Shield lines for one damage line:  TODO: Handle this...
13:09:26:09:36:20.8::DRUGnROLL,P[200404637@6548010 DRUGnROLL@drugnroll],,*,Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],Entangling Force,Pn.Oonws91,Shield,,-17.2139,-3.44278
13:09:26:09:36:20.8::DRUGnROLL,P[200404637@6548010 DRUGnROLL@drugnroll],,*,Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],Entangling Force,Pn.Oonws91,Shield,,-340.835,0
13:09:26:09:36:20.8::Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],,*,,*,Health Steal,Pn.S6b30w1,HitPoints,,-0.0190611,0
13:09:26:09:36:20.8::Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],,*,,*,Health Steal,Pn.S6b30w1,HitPoints,,-0.117442,0
13:09:26:09:36:20.8::DRUGnROLL,P[200404637@6548010 DRUGnROLL@drugnroll],,*,Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],Entangling Force,Pn.Oonws91,Arcane,Critical,344.278,1721.39
13:09:26:09:36:20.9::Lord DopeVIII,P[200441364@6420568 Lord DopeVIII@lorddopeviii],,*,,*,Shielded Resurgence,Pn.Mrczs41,Null,ShowPowerDisplayName,0,0
                */

                // Compare
                if ((sl.line.evtInt == line.evtInt) &&
                        (sl.line.ownInt == line.ownInt) &&
                        // (sl.line.srcInt == line.srcInt) &&
                        (sl.line.tgtInt == line.tgtInt) &&
                        (line.type != "Shield") &&
                        (line.mag > 0.0)  // <- Skip zero damage lines.
                    )
                {
                    // Matched
                    active.Remove(cur);
                    return sl.ms;
                }
                else
                {
                    // Check expired.
                    TimeSpan diff = line.logInfo.detectedTime - sl.ms.Time;

                    if (diff.TotalMilliseconds > 500)
                    {
                        // Drop old and unmatch shield lines.
                        // Generally shield line should match in <= 100ms.
                        active.Remove(cur);
                    }
                }
            }

            return null;
        }
    }

    internal enum EntityType
    {
        Player,
        Pet,
        Creature,
        Entity,
        Unknown
    }

    // Pre-parsed line
    internal class ParsedLine
    {

        public LogLineEventArgs logInfo;

        //
        // Parsed from the line.
        //

        public String ownDsp, ownInt, srcDsp, srcInt, tgtDsp, tgtInt, evtDsp, evtInt;
        public String type, attackType, special, flags;
        public int swingType, ts;
        public bool critical, flank, dodge, immune, kill, showPowerDisplayName;
        public float mag, magBase;
        public bool error;

        //
        // Computed extra data.
        //

        public EntityType ownEntityType, srcEntityType, tgtEntityType;
        public OwnerInfo tgtOwnerInfo = null;

        // The attacker name for encounters
        public String encAttackerName;

        // The target name for encounters
        public String encTargetName;

        // The attacker name for the combat action.
        public String unitAttackerName;

        // The target name for the combat action.
        public String unitTargetName;



        public ParsedLine(LogLineEventArgs logInfo)
        {
            //    Time            ::   ownDsp  ,                ownInt                   ,       srcDsp     ,      srcInt          ,   tgtDsp  ,           tgtInt                        ,evtDsp,  evtInt  ,type ,flags,mag,magBase
            // 13:03:22:20:16:32.6::Ameise-22  ,P[100083146@5846877 Ameise@antday]       ,Morsches Skelett-1,C[6261 Skeleton_Basic],Ameise-22  ,P[100083146@5846877 Ameise@antday]       ,Wache ,Pn.Jy04um1,Power,Kill ,-0 ,0
            // 13:07:08:14:57:31.4::Wolf       ,C[42358 Monster_Wolf]                    ,                  ,*                     ,Fiolnir    ,P[201259732@7545190 Fiolnir@lodur42]     ,Bite  ,Pn.Lp6b6g1,Physical,Flank,43.4474,47.6017

            this.logInfo = logInfo;
            this.ts = ++ActGlobals.oFormActMain.GlobalTimeSorter;
            string[] split = logInfo.logLine.Split(NW_Parser.separatorLog, StringSplitOptions.None);
            ownDsp = split[1];
            ownInt = split[2];
            srcDsp = split[3];
            srcInt = split[4];
            tgtDsp = split[5];
            tgtInt = split[6];
            evtDsp = split[7];
            evtInt = split[8];
            type = split[9];
            flags = split[10];
            mag = float.Parse(split[11], NW_Parser.cultureLog);
            magBase = float.Parse(split[12], NW_Parser.cultureLog);
            if (split.Length != 13)
            {
                this.error = true;
            }

            ownEntityType = EntityType.Unknown;
            srcEntityType = EntityType.Unknown;
            tgtEntityType = EntityType.Unknown;


            // Defaults for the clean names.
            encAttackerName = srcDsp;
            encTargetName = tgtDsp;
            unitAttackerName = srcDsp;
            unitTargetName = tgtDsp;


            kill = critical = flank = dodge = immune = false;
            special = "None";
            if (flags.Length > 0)
            {
                int extraFlagCount = 0;
                string[] sflags = flags.Split('|');
                foreach (string sflag in sflags)
                {
                    switch (sflag)
                    {
                        case "Flank":
                            flank = true;
                            break;
                        case "Critical":
                            critical = true;
                            break;
                        case "Dodge":
                            dodge = true;
                            special = (extraFlagCount++ > 0) ? (special + " | " + sflag) : sflag;
                            break;
                        case "Immune":
                            immune = true;
                            special = (extraFlagCount++ > 0) ? (special + " | " + sflag) : sflag;
                            break;
                        case "Kill":
                            kill = true;
                            break;
                        case "ShowPowerDisplayName":
                            showPowerDisplayName = true;
                            break;

                        default:
                            special = (extraFlagCount++ > 0) ? (special + " | " + sflag) : sflag;
                            break;
                    }
                }
            }

            swingType = (int)SwingTypeEnum.NonMelee;

            attackType = evtDsp;
            if (attackType.Trim().Length == 0)
            {
                // Uggly fix for missing attack type
                attackType = NW_Parser.unkAbility;
            }
        }
    }
}
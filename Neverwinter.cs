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
[assembly: AssemblyCopyright("doublecarlos, oracleNW, jicama, dragonsbite, designedbyrng, nils.brummond@gmail.com based on: Antday <Unique> based on STO Plugin from Hilbert@mancom, Pirye@ucalegon")]
[assembly: AssemblyVersion("1.2.8.2")]

/*
 * Version History - doublecarlos
 * 1.2.8.2 2025-05-07
 * - Damage from entities created by Blue Fire Eye and Tutor are correctly added to their DamageType views instead of the player's when merging is disabled.
 */

/*
 * Version History - doublecarlos
 * 1.2.8.1 2025-05-03
 * - Simplify OwnerRegistery code to avoid "key already exists" exceptions on Alt-Tab.
 */

/*
 * Version History - oracleNW
 * 1.2.8.0 2023-07-06
 * 
 */

/* Version History - jicama (@valyana in NW)
 * 1.2.7.0 - 6-11-2020
 * - Greatly improve shield tracking for mod 17/18.  Complicated because Oathkeeper and tank shields look identical in the logs, but behave differently.
 * - Fix Watcher boss parsing.
 * - Hopefully fix exceptions on alt-tab.
 * No version bump - 5-22-2019
 * - Improve shield tracking for mod 16.
 * - Add deflect tracking.
 * - Reduce UNKNOWNs for pets.
 */

/* Version History - dragonsbite
 * 1.2.6.0 - 2-11-2018
 *  - Changed processing order to fix some things. Removed the previous Wheel of Elements : Earth fix as this fix fixes it and as below and other stuff.
 *  - Fixed Cleanse to show the correct amount of inc Cleanse and out Cleanse
 *  - Fixed Fall Damage to not start combat
 *  - Fixed Spike Trap, Poison Spike Trap, and Arrow Trap to not start combat.
 * 1.2.5.0 - 2-9-2018
 *   -Fixed Falling Damage to not start combat. 
 *   -Fixed Wheel of Elements : Earth to not start combat.
 * 1.2.4.0 - 2-9-2018
 *  - Fixed Pet Damage "In and "Out" and "Deaths". This affects your damage in and out by less then 0.05% and your deaths.
 *  - Fixed Cure/Dispel In and Out to only show "Cleanse" and ignored others that did NOT show a cleanse and instead just showed messages of expiration.
 *  - Fixed "Start" and "Stop" of combat to now start and stop via actual damage as it properly should. Encounter times are decreased as a result. 
 *  - Consequently dps increases as it should. The amount of encounters increases as there is more time between encounters now. 
 */

/* Version History - designedbyrng
 * 1.2.3.0 - 2-8-2018
 *  - Non-damaging procs are in their own category "Non-Damage (Out)" and "Non-Damage (Inc)"
 *  - Fixed Slayer marks now are parsed properly
 *  - Several stats have been renamed to fit neverwinter better 
 */

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

        private UnmatchedShieldLines unmatchedShieldLines = null;

        // For tracking source of Chaotic Growth heals.
        private Dictionary<string, ChaoticGrowthInfo> magicMissileLastHit = new Dictionary<string, ChaoticGrowthInfo>();

        private Dictionary<string, bool> playerCharacterNames = new Dictionary<string, bool>();
        private bool playersCharacterFound = false;

        // FIXME changing some of these Inc/Out___Name strings breaks columns in Combatant list in Encounter view for some reason
        private const string IncDamageName = "Damage (Inc)";
        private const string OutDamageName = "Damage (Out)";
        private const string IncHealedName = "Heals (Inc)";
        private const string OutHealedName = "Heals (Out)";
        private const string IncPowerRName = "Power Replenish (Inc)";
        private const string OutPowerRName = "Power Replenish (Out)";
        private const string IncSkillsName = "Non-Damage (Inc)";
        private const string OutSkillsName = "Non-Damage (Out)";
        private const string IncCleanseName = "Cure/Dispel (Inc)";
        private const string OutCleanseName = "Cure/Dispel (Out)";

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

        public static readonly string[] companionEntityPowers = {
            "Pn.Prookc1", // "Hexed earth" from Blue Fire Eye
            "Pn.Bdzbls", // "Instructional Aid" from Tutor
            "Pn.3kzn9w1", // "Instructional Aid" from Tutor
        };

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

            unmatchedShieldLines = new UnmatchedShieldLines(this);
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

        private string GetCellDataDeflect(MasterSwing Data)
        {
            object val;
            bool Deflect = false;

            if (Data.Tags.TryGetValue("Deflect", out val))
            {
                Deflect = (bool)val;
            }

            return Deflect.ToString();
        }

        private string GetSqlDataDeflect(MasterSwing Data)
        {
            object val;
            bool Deflect = false;

            if (Data.Tags.TryGetValue("Deflect", out val))
            {
                Deflect = (bool)val;
            }

            return Deflect.ToString(usCulture)[0].ToString();
        }

        private int MasterSwingCompareDeflect(MasterSwing Left, MasterSwing Right)
        {
            object val;
            bool leftDeflect = false;
            bool rightDeflect = false;

            if (Left.Tags.TryGetValue("Deflect", out val))
            {
                leftDeflect = (bool)val;
            }

            if (Right.Tags.TryGetValue("Deflect", out val))
            {
                rightDeflect = (bool)val;
            }

            return leftDeflect.CompareTo(rightDeflect);
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

        private int GetDTDeflectValue(DamageTypeData Data)
        {
            if (Data.Items.Count == 0) return 0;

            AttackType at = Data.Items["All"];
            return GetAADeflectValue(at);
        }

        private string GetCellDataDeflectHits(DamageTypeData Data)
        {
            return GetDTDeflectValue(Data).ToString(GetIntCommas());
        }

        private string GetSqlDataDeflectHits(DamageTypeData Data)
        {
            return GetDTDeflectValue(Data).ToString();
        }

        private double GetDTDeflectPrecValue(DamageTypeData Data)
        {
            if (Data.Hits == 0) return 0;

            double fv = (double)GetDTDeflectValue(Data);
            fv *= 100.0;
            fv /= Data.Hits;

            return fv;
        }

        private string GetCellDataDeflectPrec(DamageTypeData Data)
        {
            return GetDTDeflectPrecValue(Data).ToString("0'%");
        }

        private string GetSqlDataDeflectPrec(DamageTypeData Data)
        {
            return GetDTDeflectPrecValue(Data).ToString("0'%");
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

            for (int i = 0; i < count; i++)
            {
                MasterSwing ms = Data.Items[i];

                object fv;
                if ((ms.Damage > 0) && (ms.Tags.TryGetValue("Flank", out fv)))
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
            double flankPrec = (double)GetAAFlankValue(Data);
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

        private int GetAADeflectValue(AttackType Data)
        {
            int DeflectCount = 0;

            if (Data.Items.Count == 0) return 0;

            if (Data.Tags.ContainsKey("DeflectPrecCacheCount"))
            {
                int DeflectPrecCacheCount = (int)Data.Tags["DeflectPrecCacheCount"];
                if (DeflectPrecCacheCount == Data.Items.Count)
                {
                    DeflectCount = (int)Data.Tags["DeflectPrecCacheValue"];
                    return DeflectCount;
                }
            }

            int count = Data.Items.Count;

            for (int i = 0; i < count; i++)
            {
                MasterSwing ms = Data.Items[i];

                object fv;
                if ((ms.Damage > 0) && (ms.Tags.TryGetValue("Deflect", out fv)))
                {
                    bool Deflect = (bool)fv;
                    if (Deflect) DeflectCount++;
                }
            }

            Data.Tags["DeflectPrecCacheCount"] = count;
            Data.Tags["DeflectPrecCacheValue"] = DeflectCount;

            return DeflectCount;
        }

        private string GetCellDataDeflectHits(AttackType Data)
        {
            return GetAADeflectValue(Data).ToString(GetIntCommas());
        }

        private string GetSqlDataDeflectHits(AttackType Data)
        {
            return GetAADeflectValue(Data).ToString();
        }

        private int AttackTypeCompareDeflectHits(AttackType Left, AttackType Right)
        {
            return GetAADeflectValue(Left).CompareTo(GetAADeflectValue(Right));
        }

        private string GetCellDataDeflectPrec(AttackType Data)
        {
            double DeflectPrec = (double)GetAADeflectValue(Data);
            DeflectPrec *= 100.0;
            DeflectPrec /= (double)Data.Hits;

            return DeflectPrec.ToString("0'%");
        }

        private string GetSqlDataDeflectPrec(AttackType Data)
        {
            double DeflectPrec = (double)GetAADeflectValue(Data);
            DeflectPrec *= 100.0;
            DeflectPrec /= (double)Data.Hits;

            return DeflectPrec.ToString("0'%");
        }

        private int AttackTypeCompareDeflectPrec(AttackType Left, AttackType Right)
        {
            double DeflectPrecLeft = (double)GetAADeflectValue(Left);
            DeflectPrecLeft /= (double)Left.Items.Count;

            double DeflectPrecRight = (double)GetAADeflectValue(Right);
            DeflectPrecRight /= (double)Right.Items.Count;

            return DeflectPrecLeft.CompareTo(DeflectPrecRight);
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

            for (int i = 0; i < count; i++)
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

            effectiveness = (double)dmgTotal / (double)baseDmgTotal;

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
            return GetCellDataFlankPrec(Data.Items[OutDamageName]);
        }

        private string GetSqlDataFlankDamPrec(CombatantData Data)
        {
            return GetSqlDataFlankPrec(Data.Items[OutDamageName]);
        }

        private int CDCompareFlankDamPrec(CombatantData Left, CombatantData Right)
        {
            return GetDTFlankPrecValue(Left.Items[OutDamageName]).CompareTo(GetDTFlankPrecValue(Right.Items[OutDamageName]));
        }

        private string GetCellDataDeflectDamPrec(CombatantData Data)
        {
            return GetCellDataDeflectPrec(Data.Items[OutDamageName]);
        }

        private string GetSqlDataDeflectDamPrec(CombatantData Data)
        {
            return GetSqlDataDeflectPrec(Data.Items[OutDamageName]);
        }

        private int CDCompareDeflectDamPrec(CombatantData Left, CombatantData Right)
        {
            return GetDTDeflectPrecValue(Left.Items[OutDamageName]).CompareTo(GetDTDeflectPrecValue(Right.Items[OutDamageName]));
        }

        private string GetCellDataDmgEffectPrec(CombatantData Data)
        {
            return GetCellDataEffectiveness(Data.Items[OutDamageName]);
        }

        private string GetSqlDataDmgEffectPrec(CombatantData Data)
        {
            return GetSqlDataEffectiveness(Data.Items[OutDamageName]);
        }

        private int CDCompareDmgEffectPrec(CombatantData Left, CombatantData Right)
        {
            return GetDTEffectivenessValue(Left.Items[OutDamageName]).CompareTo(GetDTEffectivenessValue(Right.Items[OutDamageName]));
        }

        private string GetCellDataDmgTakenEffectPrec(CombatantData Data)
        {
            return GetCellDataEffectiveness(Data.Items[IncDamageName]);
        }

        private string GetSqlDataDmgTakenEffectPrec(CombatantData Data)
        {
            return GetSqlDataEffectiveness(Data.Items[IncDamageName]);
        }

        private int CDCompareDmgTakenEffectPrec(CombatantData Left, CombatantData Right)
        {
            return GetDTEffectivenessValue(Left.Items[IncDamageName]).CompareTo(GetDTEffectivenessValue(Right.Items[IncDamageName]));
        }


        private void FixupEncounterData()
        {
            EncounterData.ColumnDefs.Clear();
            EncounterData.ColumnDefs.Add("EncId", new EncounterData.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.EncId; }));
            EncounterData.ColumnDefs.Add("Title", new EncounterData.ColumnDef("Title", true, "VARCHAR(64)", "Title", (Data) => { return Data.Title; }, (Data) => { return Data.Title; }));
            EncounterData.ColumnDefs.Add("StartTime", new EncounterData.ColumnDef("StartTime", true, "TIMESTAMP", "StartTime",
                    (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("G"); },
                    (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            EncounterData.ColumnDefs.Add("EndTime", new EncounterData.ColumnDef("EndTime", true, "TIMESTAMP", "EndTime",
                    (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.EndTime.ToString("G"); },
                    (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            EncounterData.ColumnDefs.Add("Duration", new EncounterData.ColumnDef("Duration", true, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }));
            EncounterData.ColumnDefs.Add("Damage", new EncounterData.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return (Data.Damage / 10).ToString(GetIntCommas()); }, (Data) => { return Data.Damage.ToString(); })); // TODO does not work as expected
            EncounterData.ColumnDefs.Add("EncDPS", new EncounterData.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.DPS.ToString(GetFloatCommas()); }, (Data) => { return Data.DPS.ToString(usCulture); })); // TODO does not work as expected
            EncounterData.ColumnDefs.Add("Kills", new EncounterData.ColumnDef("Kills", true, "INT", "Kills", (Data) => { return Data.AlliedKills.ToString(GetIntCommas()); }, (Data) => { return Data.AlliedKills.ToString(); })); // TODO does not work as expected
            EncounterData.ColumnDefs.Add("Deaths", new EncounterData.ColumnDef("Deaths", true, "INT", "Deaths", (Data) => { return Data.AlliedDeaths.ToString(); }, (Data) => { return Data.AlliedDeaths.ToString(); })); // TODO does not work as expected

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
            EncounterData.ExportVariables.Add("damagetaken", new EncounterData.TextExportFormatter("damagetaken", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-damagetaken"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-damagetaken"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "damagetaken", Extra); }));
            EncounterData.ExportVariables.Add("healstaken", new EncounterData.TextExportFormatter("healstaken", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-healstaken"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-healstaken"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "healstaken", Extra); }));
            EncounterData.ExportVariables.Add("powerheal", new EncounterData.TextExportFormatter("powerheal", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-powerheal"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-powerheal"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "powerheal", Extra); }));
            EncounterData.ExportVariables.Add("kills", new EncounterData.TextExportFormatter("kills", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-kills"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-kills"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "kills", Extra); }));
            EncounterData.ExportVariables.Add("deaths", new EncounterData.TextExportFormatter("deaths", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-deaths"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-deaths"].DisplayedText, (Data, SelectiveAllies, Extra) => { return EncounterFormatSwitch(Data, SelectiveAllies, "deaths", Extra); }));
        }
        private void FixupCombatantData()
        {
            CombatantData.ColumnDefs.Clear();
            CombatantData.ColumnDefs.Add("EncId", new CombatantData.ColumnDef("EncId", false, "CHAR(8)", "EncId",
                    (Data) => { return string.Empty; },
                    (Data) => { return Data.Parent.EncId; },
                    (Left, Right) => { return 0; })); // TODO shows nothing
            CombatantData.ColumnDefs.Add("Ally", new CombatantData.ColumnDef("Ally", false, "CHAR(1)", "Ally",
                    (Data) => { return Data.Parent.GetAllies().Contains(Data).ToString(); },
                    (Data) => { return Data.Parent.GetAllies().Contains(Data) ? "T" : "F"; },
                    (Left, Right) => { return Left.Parent.GetAllies().Contains(Left).CompareTo(Right.Parent.GetAllies().Contains(Right)); })); // TODO does not work
            CombatantData.ColumnDefs.Add("Name", new CombatantData.ColumnDef("Name", true, "VARCHAR(64)", "Name",
                    (Data) => { return Data.Name; },
                    (Data) => { return Data.Name; },
                    (Left, Right) => { return Left.Name.CompareTo(Right.Name); }));
            CombatantData.ColumnDefs.Add("StartTime", new CombatantData.ColumnDef("StartTime", true, "TIMESTAMP", "StartTime",
                    (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("T"); },
                    (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); },
                    (Left, Right) => { return Left.StartTime.CompareTo(Right.StartTime); }));
            CombatantData.ColumnDefs.Add("EndTime", new CombatantData.ColumnDef("EndTime", false, "TIMESTAMP", "EndTime",
                    (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.StartTime.ToString("T"); },
                    (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); },
                    (Left, Right) => { return Left.EndTime.CompareTo(Right.EndTime); }));
            CombatantData.ColumnDefs.Add("Duration", new CombatantData.ColumnDef("Duration", true, "INT", "Duration",
                    (Data) => { return Data.DurationS; },
                    (Data) => { return Data.Duration.TotalSeconds.ToString("0"); },
                    (Left, Right) => { return Left.Duration.CompareTo(Right.Duration); }));
            CombatantData.ColumnDefs.Add("Damage", new CombatantData.ColumnDef("Damage", true, "BIGINT", "Damage",
                    (Data) => { return Data.Damage.ToString(GetIntCommas()); },
                    (Data) => { return Data.Damage.ToString(); },
                    (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            CombatantData.ColumnDefs.Add("Damage%", new CombatantData.ColumnDef("Damage%", true, "VARCHAR(4)", "DamagePerc",
                    (Data) => { return Data.DamagePercent; },
                    (Data) => { return Data.DamagePercent; },
                    (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); })); // TODO does not work
            CombatantData.ColumnDefs.Add("Kills", new CombatantData.ColumnDef("Kills", false, "INT", "Kills",
                    (Data) => { return Data.Kills.ToString(GetIntCommas()); },
                    (Data) => { return Data.Kills.ToString(); },
                    (Left, Right) => { return Left.Kills.CompareTo(Right.Kills); }));
            // TODO add PlayerKills column (Kills currently include companion kills)
            CombatantData.ColumnDefs.Add("Healed", new CombatantData.ColumnDef("Healed", false, "BIGINT", "Healed",
                    (Data) => { return Data.Healed.ToString(GetIntCommas()); },
                    (Data) => { return Data.Healed.ToString(); },
                    (Left, Right) => { return Left.Healed.CompareTo(Right.Healed); }));
            CombatantData.ColumnDefs.Add("Healed%", new CombatantData.ColumnDef("Healed%", false, "VARCHAR(4)", "HealedPerc",
                    (Data) => { return Data.HealedPercent; },
                    (Data) => { return Data.HealedPercent; },
                    (Left, Right) => { return Left.Healed.CompareTo(Right.Healed); })); // TODO does not work
            CombatantData.ColumnDefs.Add("CritHeals", new CombatantData.ColumnDef("CritHeals", false, "INT", "CritHeals",
                    (Data) => { return Data.CritHeals.ToString(GetIntCommas()); },
                    (Data) => { return Data.CritHeals.ToString(); },
                    (Left, Right) => { return Left.CritHeals.CompareTo(Right.CritHeals); }));
            CombatantData.ColumnDefs.Add("Heals", new CombatantData.ColumnDef("Heals", false, "INT", "Heals",
                    (Data) => { return Data.Heals.ToString(GetIntCommas()); },
                    (Data) => { return Data.Heals.ToString(); },
                    (Left, Right) => { return Left.Heals.CompareTo(Right.Heals); }));
            CombatantData.ColumnDefs.Add("Cleanses", new CombatantData.ColumnDef("Cleanses", false, "INT", "CureDispels",
                    (Data) => { return Data.CureDispels.ToString(GetIntCommas()); },
                    (Data) => { return Data.CureDispels.ToString(); },
                    (Left, Right) => { return Left.CureDispels.CompareTo(Right.CureDispels); }));
            CombatantData.ColumnDefs.Add("AP Gain", new CombatantData.ColumnDef("AP Gain", false, "BIGINT", "PowerReplenish",
                    (Data) => { return Data.PowerReplenish.ToString(GetIntCommas()); },
                    (Data) => { return Data.PowerReplenish.ToString(); },
                    (Left, Right) => { return Left.PowerReplenish.CompareTo(Right.PowerReplenish); }));
            CombatantData.ColumnDefs.Add("DPS", new CombatantData.ColumnDef("DPS", false, "DOUBLE", "DPS",
                    (Data) => { return Data.DPS.ToString(GetFloatCommas()); },
                    (Data) => { return Data.DPS.ToString(usCulture); },
                    (Left, Right) => { return Left.DPS.CompareTo(Right.DPS); })); // TODO calculate with duration floor 1 second, unless damage is zero (creating NaN when duration also zero), then just list zero
            CombatantData.ColumnDefs.Add("EncDPS", new CombatantData.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS",
                    (Data) => { return Data.EncDPS.ToString(GetFloatCommas()); },
                    (Data) => { return Data.EncDPS.ToString(usCulture); },
                    (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            CombatantData.ColumnDefs.Add("EncHPS", new CombatantData.ColumnDef("EncHPS", true, "DOUBLE", "EncHPS",
                    (Data) => { return Data.EncHPS.ToString(GetFloatCommas()); },
                    (Data) => { return Data.EncHPS.ToString(usCulture); },
                    (Left, Right) => { return Left.Healed.CompareTo(Right.Healed); }));
            CombatantData.ColumnDefs.Add("CritHits", new CombatantData.ColumnDef("CritHits", false, "INT", "CritHits",
                    (Data) => { return Data.CritHits.ToString(GetIntCommas()); },
                    (Data) => { return Data.CritHits.ToString(); },
                    (Left, Right) => { return Left.CritHits.CompareTo(Right.CritHits); }));
            CombatantData.ColumnDefs.Add("Swings", new CombatantData.ColumnDef("Swings", false, "INT", "Swings",
                    (Data) => { return Data.Swings.ToString(GetIntCommas()); },
                    (Data) => { return Data.Swings.ToString(); },
                    (Left, Right) => { return Left.Swings.CompareTo(Right.Swings); }));
            CombatantData.ColumnDefs.Add("HealingTaken", new CombatantData.ColumnDef("HealingTaken", false, "BIGINT", "HealsTaken",
                    (Data) => { return Data.HealsTaken.ToString(GetIntCommas()); },
                    (Data) => { return Data.HealsTaken.ToString(); },
                    (Left, Right) => { return Left.HealsTaken.CompareTo(Right.HealsTaken); }));
            CombatantData.ColumnDefs.Add("DamageTaken", new CombatantData.ColumnDef("DamageTaken", true, "BIGINT", "DamageTaken",
                    (Data) => { return Data.DamageTaken.ToString(GetIntCommas()); },
                    (Data) => { return Data.DamageTaken.ToString(); },
                    (Left, Right) => { return Left.DamageTaken.CompareTo(Right.DamageTaken); }));
            CombatantData.ColumnDefs.Add("Deaths", new CombatantData.ColumnDef("Deaths", true, "INT", "Deaths",
                    (Data) => { return Data.Deaths.ToString(GetIntCommas()); },
                    (Data) => { return Data.Deaths.ToString(); },
                    (Left, Right) => { return Left.Deaths.CompareTo(Right.Deaths); }));
            CombatantData.ColumnDefs.Add("FCritHit%", new CombatantData.ColumnDef("FCritHit%", true, "VARCHAR(8)", "FCritHitPerc",
                    (Data) => { return GetFilteredCritChance(Data).ToString("0'%"); },
                    (Data) => { return GetFilteredCritChance(Data).ToString("0'%"); },
                    (Left, Right) => { return GetFilteredCritChance(Left).CompareTo(GetFilteredCritChance(Right)); })); // TODO verify if this works for NW
            CombatantData.ColumnDefs.Add("CritDam%", new CombatantData.ColumnDef("CritDam%", false, "VARCHAR(8)", "CritDamPerc",
                    (Data) => { return Data.CritDamPerc.ToString("0'%"); },
                    (Data) => { return Data.CritDamPerc.ToString("0'%"); },
                    (Left, Right) => { return Left.CritDamPerc.CompareTo(Right.CritDamPerc); })); // TODO verify if this works for NW
            CombatantData.ColumnDefs.Add("CritHeal%", new CombatantData.ColumnDef("CritHeal%", false, "VARCHAR(8)", "CritHealPerc",
                    (Data) => { return Data.CritHealPerc.ToString("0'%"); },
                    (Data) => { return Data.CritHealPerc.ToString("0'%"); },
                    (Left, Right) => { return Left.CritHealPerc.CompareTo(Right.CritHealPerc); }));

            CombatantData.ColumnDefs.Add("FlankDam%", // TODO verify if this is % of damage or % of hits, and consider having both columns
                new CombatantData.ColumnDef("FlankDam%", false, "VARCHAR(8)", "FlankDamPrec", GetCellDataFlankDamPrec, GetSqlDataFlankDamPrec, CDCompareFlankDamPrec));
            CombatantData.ColumnDefs.Add("DeflectDam%", // TODO verify if this is % of damage or % of hits, and consider having both columns
                new CombatantData.ColumnDef("DeflectDam%", false, "VARCHAR(8)", "DeflectDamPrec", GetCellDataDeflectDamPrec, GetSqlDataDeflectDamPrec, CDCompareDeflectDamPrec));
            CombatantData.ColumnDefs.Add("DmgEffect%",
                new CombatantData.ColumnDef("DmgEffect%", false, "VARCHAR(8)", "DmgEffectPrec", GetCellDataDmgEffectPrec, GetSqlDataDmgEffectPrec, CDCompareDmgEffectPrec));
            CombatantData.ColumnDefs.Add("DmgTakenEffect%",
                new CombatantData.ColumnDef("DmgTakenEffect%", false, "VARCHAR(8)", "DmgTakenEffectPrec", GetCellDataDmgTakenEffectPrec, GetSqlDataDmgTakenEffectPrec, CDCompareDmgTakenEffectPrec));


            CombatantData.OutgoingDamageTypeDataObjects = new Dictionary<string, CombatantData.DamageTypeDef>
        {
            {OutDamageName, new CombatantData.DamageTypeDef("Damage", 0, Color.Orange)},
            {OutHealedName, new CombatantData.DamageTypeDef(OutHealedName, 1, Color.Blue)},
            {OutPowerRName, new CombatantData.DamageTypeDef(OutPowerRName, 1, Color.Violet)},
            {OutCleanseName, new CombatantData.DamageTypeDef(OutCleanseName, 0, Color.Wheat)},
            {OutSkillsName, new CombatantData.DamageTypeDef(OutSkillsName, -1, Color.DarkOrange)},
            {"All Outgoing (Ref)", new CombatantData.DamageTypeDef("All Outgoing (Ref)", 0, Color.Black)}
        };
            CombatantData.IncomingDamageTypeDataObjects = new Dictionary<string, CombatantData.DamageTypeDef>
        {
            {IncDamageName, new CombatantData.DamageTypeDef(IncDamageName, -1, Color.Red)},
            {IncHealedName,new CombatantData.DamageTypeDef(IncHealedName, 1, Color.LimeGreen)},
            {IncPowerRName,new CombatantData.DamageTypeDef(IncPowerRName, 1, Color.MediumPurple)},
            {IncCleanseName, new CombatantData.DamageTypeDef(IncCleanseName, 0, Color.Wheat)},
            {IncSkillsName, new CombatantData.DamageTypeDef(IncSkillsName, -1, Color.Yellow)},
            {"All Incoming (Ref)",new CombatantData.DamageTypeDef("All Incoming (Ref)", 0, Color.Black)}
        };
            CombatantData.SwingTypeToDamageTypeDataLinksOutgoing = new SortedDictionary<int, List<string>>
        {
            {1, new List<string> { OutDamageName } },
            {2, new List<string> { OutSkillsName } },
            {3, new List<string> { OutHealedName } },
            {13, new List<string> { OutPowerRName } },
            {20, new List<string> { OutCleanseName } }
        };
            CombatantData.SwingTypeToDamageTypeDataLinksIncoming = new SortedDictionary<int, List<string>>
        {
            {1, new List<string> { IncDamageName } },
            {2, new List<string> { IncSkillsName } },
            {3, new List<string> { IncHealedName } },
            {13, new List<string> { IncPowerRName } },
            {20, new List<string> { IncCleanseName } }
        };

            CombatantData.DamageSwingTypes = new List<int> { 1, 2 };
            CombatantData.HealingSwingTypes = new List<int> { 3 };

            CombatantData.DamageTypeDataNonSkillDamage = "Auto-Attack (Out)";
            CombatantData.DamageTypeDataOutgoingDamage = OutDamageName;
            CombatantData.DamageTypeDataOutgoingHealing = OutHealedName;
            CombatantData.DamageTypeDataIncomingDamage = IncDamageName;
            CombatantData.DamageTypeDataIncomingHealing = IncHealedName;

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
            CombatantData.ExportVariables.Add("damagetaken", new CombatantData.TextExportFormatter("damagetaken", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-damagetaken"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-damagetaken"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "damagetaken", Extra); }));
            CombatantData.ExportVariables.Add("healstaken", new CombatantData.TextExportFormatter("healstaken", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-healstaken"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-healstaken"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "healstaken", Extra); }));
            CombatantData.ExportVariables.Add("powerheal", new CombatantData.TextExportFormatter("powerheal", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-powerheal"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-powerheal"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "powerheal", Extra); }));
            CombatantData.ExportVariables.Add("kills", new CombatantData.TextExportFormatter("kills", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-kills"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-kills"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "kills", Extra); }));
            CombatantData.ExportVariables.Add("deaths", new CombatantData.TextExportFormatter("deaths", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-deaths"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-deaths"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "deaths", Extra); }));
            CombatantData.ExportVariables.Add("NAME3", new CombatantData.TextExportFormatter("NAME3", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME3"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME3"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME3", Extra); }));
            CombatantData.ExportVariables.Add("NAME4", new CombatantData.TextExportFormatter("NAME4", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME4"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME4"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME4", Extra); }));
            CombatantData.ExportVariables.Add("NAME8", new CombatantData.TextExportFormatter("NAME8", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME8"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME8"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME8", Extra); }));
            CombatantData.ExportVariables.Add("NAME12", new CombatantData.TextExportFormatter("NAME12", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME12"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME12"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME12", Extra); }));
            CombatantData.ExportVariables.Add("NAME15", new CombatantData.TextExportFormatter("NAME15", ActGlobals.ActLocalization.LocalizationStrings["exportFormattingLabel-NAME15"].DisplayedText, ActGlobals.ActLocalization.LocalizationStrings["exportFormattingDesc-NAME15"].DisplayedText, (Data, Extra) => { return CombatantFormatSwitch(Data, "NAME15", Extra); }));
        }
        private void FixupDamageTypeData()
        {
            DamageTypeData.ColumnDefs.Clear();
            DamageTypeData.ColumnDefs.Add("EncId", new DamageTypeData.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.Parent.Parent.EncId; })); // TODO shows nothing
            DamageTypeData.ColumnDefs.Add("Combatant", new DamageTypeData.ColumnDef("Combatant", false, "VARCHAR(64)", "Combatant", (Data) => { return Data.Parent.Name; }, (Data) => { return Data.Parent.Name; }));
            DamageTypeData.ColumnDefs.Add("Grouping", new DamageTypeData.ColumnDef("Grouping", false, "VARCHAR(92)", "Grouping", (Data) => { return string.Empty; }, GetDamageTypeGrouping)); // TODO shows nothing, may be irrelevant to NW
            DamageTypeData.ColumnDefs.Add("Type", new DamageTypeData.ColumnDef("Type", true, "VARCHAR(64)", "Type", (Data) => { return Data.Type; }, (Data) => { return Data.Type; }));
            DamageTypeData.ColumnDefs.Add("StartTime", new DamageTypeData.ColumnDef("StartTime", false, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            DamageTypeData.ColumnDefs.Add("EndTime", new DamageTypeData.ColumnDef("EndTime", false, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }));
            DamageTypeData.ColumnDefs.Add("Duration", new DamageTypeData.ColumnDef("Duration", false, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }));
            DamageTypeData.ColumnDefs.Add("Damage", new DamageTypeData.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return Data.Damage.ToString(GetIntCommas()); }, (Data) => { return Data.Damage.ToString(); }));
            DamageTypeData.ColumnDefs.Add("EncDPS", new DamageTypeData.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.EncDPS.ToString(GetFloatCommas()); }, (Data) => { return Data.EncDPS.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("CharDPS", new DamageTypeData.ColumnDef("CharDPS", false, "DOUBLE", "CharDPS", (Data) => { return Data.CharDPS.ToString(GetFloatCommas()); }, (Data) => { return Data.CharDPS.ToString(usCulture); })); // TODO what is this?
            DamageTypeData.ColumnDefs.Add("DPS", new DamageTypeData.ColumnDef("DPS", false, "DOUBLE", "DPS", (Data) => { return Data.DPS.ToString(GetFloatCommas()); }, (Data) => { return Data.DPS.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("AverageHit", new DamageTypeData.ColumnDef("AverageHit", true, "FLOAT", "Average", (Data) => { return Data.Average.ToString(GetFloatCommas()); }, (Data) => { return Data.Average.ToString(usCulture); }));
            DamageTypeData.ColumnDefs.Add("MedianHit", new DamageTypeData.ColumnDef("MedianHit", false, "INT", "Median", (Data) => { return Data.Median.ToString(GetIntCommas()); }, (Data) => { return Data.Median.ToString(); }));
            DamageTypeData.ColumnDefs.Add("MinHit", new DamageTypeData.ColumnDef("MinHit", true, "INT", "MinHit", (Data) => { return Data.MinHit.ToString(GetIntCommas()); }, (Data) => { return Data.MinHit.ToString(); }));
            DamageTypeData.ColumnDefs.Add("MaxHit", new DamageTypeData.ColumnDef("MaxHit", true, "INT", "MaxHit", (Data) => { return Data.MaxHit.ToString(GetIntCommas()); }, (Data) => { return Data.MaxHit.ToString(); }));
            DamageTypeData.ColumnDefs.Add("CritHits", new DamageTypeData.ColumnDef("CritHits", false, "INT", "CritHits", (Data) => { return Data.CritHits.ToString(GetIntCommas()); }, (Data) => { return Data.CritHits.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Swings", new DamageTypeData.ColumnDef("Swings", true, "INT", "Swings", (Data) => { return Data.Swings.ToString(GetIntCommas()); }, (Data) => { return Data.Swings.ToString(); })); // TODO same as Hits for all intents and purposes, irrelevant for NW
            DamageTypeData.ColumnDefs.Add("AvgDelay", new DamageTypeData.ColumnDef("AvgDelay", false, "FLOAT", "AverageDelay", (Data) => { return Data.AverageDelay.ToString(GetFloatCommas()); }, (Data) => { return Data.AverageDelay.ToString(); }));
            DamageTypeData.ColumnDefs.Add("Crit%", new DamageTypeData.ColumnDef("Crit%", true, "VARCHAR(8)", "CritPerc", (Data) => { return Data.CritPerc.ToString("0'%"); }, (Data) => { return Data.CritPerc.ToString("0'%"); }));
            // TODO consider filtering Crit/Flank/Deflect to consider when powers can't be affected by them, and/or adding a dmg% column in addition to the hit chance% column

            DamageTypeData.ColumnDefs.Add("FlankHits",
                new DamageTypeData.ColumnDef("FlankHits", false, "INT", "FlankHits", GetCellDataFlankHits, GetSqlDataFlankHits));
            DamageTypeData.ColumnDefs.Add("Flank%",
                new DamageTypeData.ColumnDef("Flank%", true, "VARCHAR(8)", "FlankPerc", GetCellDataFlankPrec, GetSqlDataFlankPrec));
            DamageTypeData.ColumnDefs.Add("DeflectHits",
                new DamageTypeData.ColumnDef("DeflectHits", false, "INT", "DeflectHits", GetCellDataDeflectHits, GetSqlDataDeflectHits));
            DamageTypeData.ColumnDefs.Add("Deflect%",
                new DamageTypeData.ColumnDef("Deflect%", true, "VARCHAR(8)", "DeflectPerc", GetCellDataDeflectPrec, GetSqlDataDeflectPrec));
            DamageTypeData.ColumnDefs.Add("Effectiveness", // TODO just show zero when it's NaN
                new DamageTypeData.ColumnDef("Effectiveness", true, "VARCHAR(8)", "Effectiveness", GetCellDataEffectiveness, GetSqlDataEffectiveness));
        }
        private void FixupAttackTypeData()
        {
            AttackType.ColumnDefs.Clear();
            AttackType.ColumnDefs.Add("EncId", new AttackType.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.Parent.Parent.Parent.EncId; }, (Left, Right) => { return 0; })); // TODO shows nothing in ACT window
            AttackType.ColumnDefs.Add("Attacker", new AttackType.ColumnDef("Attacker", false, "VARCHAR(64)", "Attacker", (Data) => { return Data.Parent.Outgoing ? Data.Parent.Parent.Name : string.Empty; }, (Data) => { return Data.Parent.Outgoing ? Data.Parent.Parent.Name : string.Empty; }, (Left, Right) => { return 0; }));
            AttackType.ColumnDefs.Add("Victim", new AttackType.ColumnDef("Victim", false, "VARCHAR(64)", "Victim", (Data) => { return Data.Parent.Outgoing ? string.Empty : Data.Parent.Parent.Name; }, (Data) => { return Data.Parent.Outgoing ? string.Empty : Data.Parent.Parent.Name; }, (Left, Right) => { return 0; }));
            AttackType.ColumnDefs.Add("SwingType", new AttackType.ColumnDef("SwingType", false, "TINYINT", "SwingType", GetAttackTypeSwingType, GetAttackTypeSwingType, (Left, Right) => { return 0; })); // TODO make human-readable
            AttackType.ColumnDefs.Add("Type", new AttackType.ColumnDef("Type", true, "VARCHAR(64)", "Type", (Data) => { return Data.Type; }, (Data) => { return Data.Type; }, (Left, Right) => { return Left.Type.CompareTo(Right.Type); }));
            AttackType.ColumnDefs.Add("StartTime", new AttackType.ColumnDef("StartTime", false, "TIMESTAMP", "StartTime", (Data) => { return Data.StartTime == DateTime.MaxValue ? "--:--:--" : Data.StartTime.ToString("T"); }, (Data) => { return Data.StartTime == DateTime.MaxValue ? "0000-00-00 00:00:00" : Data.StartTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.StartTime.CompareTo(Right.StartTime); }));
            AttackType.ColumnDefs.Add("EndTime", new AttackType.ColumnDef("EndTime", false, "TIMESTAMP", "EndTime", (Data) => { return Data.EndTime == DateTime.MinValue ? "--:--:--" : Data.EndTime.ToString("T"); }, (Data) => { return Data.EndTime == DateTime.MinValue ? "0000-00-00 00:00:00" : Data.EndTime.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.EndTime.CompareTo(Right.EndTime); }));
            AttackType.ColumnDefs.Add("Duration", new AttackType.ColumnDef("Duration", false, "INT", "Duration", (Data) => { return Data.DurationS; }, (Data) => { return Data.Duration.TotalSeconds.ToString("0"); }, (Left, Right) => { return Left.Duration.CompareTo(Right.Duration); }));
            AttackType.ColumnDefs.Add("Damage", new AttackType.ColumnDef("Damage", true, "BIGINT", "Damage", (Data) => { return Data.Damage.ToString(GetIntCommas()); }, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            AttackType.ColumnDefs.Add("EncDPS", new AttackType.ColumnDef("EncDPS", true, "DOUBLE", "EncDPS", (Data) => { return Data.EncDPS.ToString(GetFloatCommas()); }, (Data) => { return Data.EncDPS.ToString(usCulture); }, (Left, Right) => { return Left.EncDPS.CompareTo(Right.EncDPS); }));
            AttackType.ColumnDefs.Add("CharDPS", new AttackType.ColumnDef("CharDPS", false, "DOUBLE", "CharDPS", (Data) => { return Data.CharDPS.ToString(GetFloatCommas()); }, (Data) => { return Data.CharDPS.ToString(usCulture); }, (Left, Right) => { return Left.CharDPS.CompareTo(Right.CharDPS); })); // TODO what is this?
            AttackType.ColumnDefs.Add("DPS", new AttackType.ColumnDef("DPS", false, "DOUBLE", "DPS", (Data) => { return Data.DPS.ToString(GetFloatCommas()); }, (Data) => { return Data.DPS.ToString(usCulture); }, (Left, Right) => { return Left.DPS.CompareTo(Right.DPS); }));
            AttackType.ColumnDefs.Add("Average", new AttackType.ColumnDef("Average", true, "FLOAT", "Average", (Data) => { return Data.Average.ToString(GetFloatCommas()); }, (Data) => { return Data.Average.ToString(usCulture); }, (Left, Right) => { return Left.Average.CompareTo(Right.Average); }));
            AttackType.ColumnDefs.Add("Median", new AttackType.ColumnDef("Median", true, "INT", "Median", (Data) => { return Data.Median.ToString(GetIntCommas()); }, (Data) => { return Data.Median.ToString(); }, (Left, Right) => { return Left.Median.CompareTo(Right.Median); }));
            AttackType.ColumnDefs.Add("MinHit", new AttackType.ColumnDef("MinHit", true, "INT", "MinHit", (Data) => { return Data.MinHit.ToString(GetIntCommas()); }, (Data) => { return Data.MinHit.ToString(); }, (Left, Right) => { return Left.MinHit.CompareTo(Right.MinHit); }));
            AttackType.ColumnDefs.Add("MaxHit", new AttackType.ColumnDef("MaxHit", true, "INT", "MaxHit", (Data) => { return Data.MaxHit.ToString(GetIntCommas()); }, (Data) => { return Data.MaxHit.ToString(); }, (Left, Right) => { return Left.MaxHit.CompareTo(Right.MaxHit); }));
            AttackType.ColumnDefs.Add("Resist", new AttackType.ColumnDef("Resist", true, "VARCHAR(64)", "Resist", (Data) => { return Data.Resist; }, (Data) => { return Data.Resist; }, (Left, Right) => { return Left.Resist.CompareTo(Right.Resist); }));
            AttackType.ColumnDefs.Add("CritHits", new AttackType.ColumnDef("CritHits", false, "INT", "CritHits", (Data) => { return Data.CritHits.ToString(GetIntCommas()); }, (Data) => { return Data.CritHits.ToString(); }, (Left, Right) => { return Left.CritHits.CompareTo(Right.CritHits); }));
            AttackType.ColumnDefs.Add("Swings", new AttackType.ColumnDef("Swings", true, "INT", "Swings", (Data) => { return Data.Swings.ToString(GetIntCommas()); }, (Data) => { return Data.Swings.ToString(); }, (Left, Right) => { return Left.Swings.CompareTo(Right.Swings); }));
            AttackType.ColumnDefs.Add("AvgDelay", new AttackType.ColumnDef("AvgDelay", false, "FLOAT", "AverageDelay", (Data) => { return Data.AverageDelay.ToString(GetFloatCommas()); }, (Data) => { return Data.AverageDelay.ToString(usCulture); }, (Left, Right) => { return Left.AverageDelay.CompareTo(Right.AverageDelay); }));
            AttackType.ColumnDefs.Add("Crit%", new AttackType.ColumnDef("Crit%", true, "VARCHAR(8)", "CritPerc", (Data) => { return Data.CritPerc.ToString("0'%"); }, (Data) => { return Data.CritPerc.ToString("0'%"); }, (Left, Right) => { return Left.CritPerc.CompareTo(Right.CritPerc); }));

            AttackType.ColumnDefs.Add("FlankHits",
                new AttackType.ColumnDef("FlankHits", false, "INT", "FlankHits", GetCellDataFlankHits, GetSqlDataFlankHits, AttackTypeCompareFlankHits));
            AttackType.ColumnDefs.Add("Flank%",
                new AttackType.ColumnDef("Flank%", true, "VARCHAR(8)", "FlankPerc", GetCellDataFlankPrec, GetSqlDataFlankPrec, AttackTypeCompareFlankPrec));
            AttackType.ColumnDefs.Add("DeflectHits",
                new AttackType.ColumnDef("DeflectHits", false, "INT", "DeflectHits", GetCellDataDeflectHits, GetSqlDataDeflectHits, AttackTypeCompareDeflectHits));
            AttackType.ColumnDefs.Add("Deflect%",
                new AttackType.ColumnDef("Deflect%", true, "VARCHAR(8)", "DeflectPerc", GetCellDataDeflectPrec, GetSqlDataDeflectPrec, AttackTypeCompareDeflectPrec));
            AttackType.ColumnDefs.Add("Effectiveness",
                new AttackType.ColumnDef("Effectiveness", true, "VARCHAR(8)", "Effectiveness", GetCellDataEffectiveness, GetSqlDataEffectiveness, AttackTypeCompareEffectiveness));
        }
        private void FixupMasterSwingData()
        {
            MasterSwing.ColumnDefs.Clear();
            MasterSwing.ColumnDefs.Add("EncId", new MasterSwing.ColumnDef("EncId", false, "CHAR(8)", "EncId", (Data) => { return string.Empty; }, (Data) => { return Data.ParentEncounter.EncId; }, (Left, Right) => { return 0; }));
            MasterSwing.ColumnDefs.Add("Time", new MasterSwing.ColumnDef("Time", true, "TIMESTAMP", "STime", (Data) => { return Data.Time.ToString("T"); }, (Data) => { return Data.Time.ToString("u").TrimEnd(new char[] { 'Z' }); }, (Left, Right) => { return Left.Time.CompareTo(Right.Time); }));
            MasterSwing.ColumnDefs.Add("Attacker", new MasterSwing.ColumnDef("Attacker", true, "VARCHAR(64)", "Attacker", (Data) => { return Data.Attacker; }, (Data) => { return Data.Attacker; }, (Left, Right) => { return Left.Attacker.CompareTo(Right.Attacker); }));
            MasterSwing.ColumnDefs.Add("SwingType", new MasterSwing.ColumnDef("SwingType", false, "TINYINT", "SwingType", (Data) => { return Data.SwingType.ToString(); }, (Data) => { return Data.SwingType.ToString(); }, (Left, Right) => { return Left.SwingType.CompareTo(Right.SwingType); })); // TODO make human-readable
            MasterSwing.ColumnDefs.Add("AttackType", new MasterSwing.ColumnDef("AttackType", true, "VARCHAR(64)", "AttackType", (Data) => { return Data.AttackType; }, (Data) => { return Data.AttackType; }, (Left, Right) => { return Left.AttackType.CompareTo(Right.AttackType); }));
            MasterSwing.ColumnDefs.Add("DamageType", new MasterSwing.ColumnDef("DamageType", true, "VARCHAR(64)", "DamageType", (Data) => { return Data.DamageType; }, (Data) => { return Data.DamageType; }, (Left, Right) => { return Left.DamageType.CompareTo(Right.DamageType); }));
            MasterSwing.ColumnDefs.Add("Victim", new MasterSwing.ColumnDef("Victim", true, "VARCHAR(64)", "Victim", (Data) => { return Data.Victim; }, (Data) => { return Data.Victim; }, (Left, Right) => { return Left.Victim.CompareTo(Right.Victim); }));
            MasterSwing.ColumnDefs.Add("DamageNum", new MasterSwing.ColumnDef("DamageNum", false, "INT", "Damage", (Data) => { return ((int)Data.Damage).ToString(); }, (Data) => { return ((int)Data.Damage).ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            //MasterSwing.ColumnDefs.Add("Damage", new MasterSwing.ColumnDef("Damage", true, "VARCHAR(128)", "DamageString", /* lambda */ (Data) => { return Data.Damage.ToString(); }, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));
            // As a C# lesson, the above lines(lambda expressions) can also be written as(anonymous methods):

            MasterSwing.ColumnDefs.Add("Damage",
                new MasterSwing.ColumnDef("Damage", true, "VARCHAR(128)", "DamageString", GetCellDataDamage, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); }));

            MasterSwing.ColumnDefs.Add("Critical", new MasterSwing.ColumnDef("Critical", true, "CHAR(1)", "Critical", /* anonymous */ delegate (MasterSwing Data) { return Data.Critical.ToString(); }, delegate (MasterSwing Data) { return Data.Critical.ToString(usCulture)[0].ToString(); }, delegate (MasterSwing Left, MasterSwing Right) { return Left.Critical.CompareTo(Right.Critical); }));
            // Or also written as(delegated methods):
            MasterSwing.ColumnDefs.Add("Special", new MasterSwing.ColumnDef("Special", true, "VARCHAR(64)", "Special", /* delegate */ GetCellDataSpecial, GetSqlDataSpecial, MasterSwingCompareSpecial));

            MasterSwing.ColumnDefs.Add("Flank",
                new MasterSwing.ColumnDef("Flank", true, "CHAR(1)", "Flank", GetCellDataFlank, GetSqlDataFlank, MasterSwingCompareFlank));
            MasterSwing.ColumnDefs.Add("Deflect",
                new MasterSwing.ColumnDef("Deflect", true, "CHAR(1)", "Deflect", GetCellDataDeflect, GetSqlDataDeflect, MasterSwingCompareDeflect));
            MasterSwing.ColumnDefs.Add("BaseDamage",
                new MasterSwing.ColumnDef("BaseDamage", true, "INT", "BaseDamageString", GetCellDataBaseDamage, GetSqlDataBaseDamage, MasterSwingCompareBaseDamage));
            MasterSwing.ColumnDefs.Add("Effectiveness",
                new MasterSwing.ColumnDef("Effectiveness", true, "VARCHAR(8)", "EffectivenessString", GetCellDataEffectiveness, GetSqlDataEffectiveness, MasterSwingCompareEffectiveness));
            MasterSwing.ColumnDefs.Add("DmgToShield",
                new MasterSwing.ColumnDef("DmgToShield", false, "VARCHAR(128)", "DmgToShieldstring", GetCellDataDmgToShield, GetSqlDataDmgToShield, MasterSwingCompareDmgToShield));
            MasterSwing.ColumnDefs.Add("ShieldP",
                new MasterSwing.ColumnDef("ShieldP", false, "VARCHAR(8)", "ShieldPDtring", GetCellDataShieldP, GetSqlDataShieldP, MasterSwingCompareShieldP));
        }
        private void FixupCombatDataStructures()
        {
            // - Remove data types that do not apply to Neverwinter combat logs.
            // - Add new data types just for Neverwinter.
            FixupEncounterData();
            FixupCombatantData();
            FixupDamageTypeData();
            FixupAttackTypeData();
            FixupMasterSwingData();
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

            foreach (KeyValuePair<string, AttackType> item in Data.Items[OutDamageName].Items)
                allAttackTypes.Add(item.Value);
            foreach (KeyValuePair<string, AttackType> item in Data.Items[OutHealedName].Items)
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

            // TODO ally detect v1.0
            // mark all players as allies unless they have engaged in a hostile action against a player or companion
            // mark companion with same ally status as their owners
            // mark all NPCs as non-allies unless they are healing a player
            // in a later version, try to filter out players that are aiding enemies as non-allies (they might be considered ally if not directly hostile towards other player)

            // Detect Player names..
            if (!(playersCharacterFound || isImport))
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
                if (line.srcDsp != "")
                {
                    // If there's a source, use that.
                    line.ownDsp = line.srcDsp;
                    line.ownInt = line.srcInt;
                }
                else
                {
                    line.ownDsp = NW_Parser.unk;
                    line.ownInt = NW_Parser.unkInt;
                }
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
                // "13:07:02:13:48:18.1::Kallista Hellbourne,P[200674407@288107 Kallista Hellbourne@tonyleon],,,Sentry,C[1150404 Frost_Goblin_Sentry],Storm Spell,Pn.Zh5vu,Lightning,ShowPowerDisplayName,580.333,0"
                // The Control Wizard effect Storm Spell seems to not have a source.  Should just use the owner in this case.

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
                else if (line.tgtInt.Contains(" Entity_") || line.tgtInt.Contains(" Artifact_Weapon_Illusion_Clone"))
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

            if (Array.IndexOf(NW_Parser.companionEntityPowers, line.evtInt) != -1)
            {
                OwnerInfo info = petOwnerRegistery.ResolveByPlayer(line.ownInt);
                string attackerName = unk;

                if (info != null) {
                    attackerName = info.petDsp + " [" + info.ownerDsp + "'s Pet]";
                }

                line.encAttackerName = line.ownDsp;
                line.unitAttackerName = attackerName;
                if (this.checkBox_mergePets.Checked)
                {
                    line.unitAttackerName = line.ownDsp;
                }
            }
            // We assume the owner is the owner of the source for this processing.
            else if (line.srcEntityType == EntityType.Pet)
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
            else if (line.ownEntityType == EntityType.Player && line.srcInt.Contains("Artifact_Weapon_Illusion_Clone"))
            {
                // if not merging mirage weapons with player
                // line.unitAttackerName = "Mirage Weapon" + " [" + line.ownDsp + "'s Entity]";
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
                        (int)SwingTypeEnum.Healing, l.critical, false, false, l.special, l.unitTargetName,
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
                        (int)SwingTypeEnum.Healing, l.critical, false, false, l.special, l.unitTargetName,
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
                                (int)SwingTypeEnum.Healing, l.critical, l.flank, l.dodge, l.unitAttackerName, cgi.unitName,
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
                                (int)SwingTypeEnum.Healing, l.critical, l.flank, l.dodge, l.unitAttackerName, unk,
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
                        (int)SwingTypeEnum.Healing, l.critical, l.flank, l.dodge, l.special, l.unitAttackerName,
                        l.attackType, new Dnum(-magAdj), -l.mag, -l.magBase, l.logInfo.detectedTime,
                        l.ts, l.unitTargetName, l.type);
                }
            }
        }

        private void ProcessActionShields(ParsedLine l)
        {
            // TODO try to improve shield detection (shield/overshield/temp HP), determine if it works on all shield-carrying paragons/songward/etc. Would be good if we can consistently detect overall effectiveness in all cases of shields

            // Blue overshields:
            // Fully shielded, Physical is damage absorbed:
            // 20:06:10:15:39:10.1::Halaster,C[64 M17_Trial_Boss_Halaster],,*,Character, P[513125329@28357045 Character@user], Arcane Blast,Pn.Zu6srm,Shield,,-190600,0
            // 20:06:10:15:39:10.1::Halaster,C[64 M17_Trial_Boss_Halaster],,*,Character, P[513125329@28357045 Character@user], Arcane Blast,Pn.Zu6srm,Physical,,190600,381200
            // Partially shielded, Physical is total damage:
            // 20:06:10:10:25:20.9::Halaster,C[10 M17_Trial_Boss_Halaster],,*,Character,P[513125329@28357045 Character@user],Duumvirate,Pn.T3r6sh,Shield,ShieldBreak,-388515,0
            // 20:06:10:10:25:20.9::Halaster,C[10 M17_Trial_Boss_Halaster],,*,Character, P[513125329@28357045 Character@user], Duumvirate, Pn.T3r6sh,Physical,ShieldBreak,668405,1.33681e+06

            // Tank shields:
            // Fully shielded, no way to determine base damage:
            // 20:03:13:22:14:10.7::Pillar Fire,C[24305 M18_Dungeon_Boss_2_Pillar_Fire],,*,Character,P[513125329@28357045 Character@user],Pool of Fire,Pn.Yg7mkc1,Shield,,-43312.5,0
            // Partially shielded, Physical is damage to HP:
            // 20:06:10:10:45:25.3::Halaster,C[539 M17_Trial_Boss_Halaster],,*,Character,P[513125329@28357045 Character@user],Duumvirate,Pn.T3r6sh,Shield,ShieldBreak,-540850,0
            // 20:06:10:10:45:25.3::Halaster,C[539 M17_Trial_Boss_Halaster],,*,Character, P[513125329@28357045 Character@user], Duumvirate, Pn.T3r6sh,Physical,Dodge | ShieldBreak,355223,3.98255e+06
            // Half shielded by Paladin Divine Champion:
            // 20:06:10:15:29:43.0::Halaster,C[40 M17_Trial_Boss_Halaster],,*,Character, P[513125329@28357045 Character@user], Arcane Blast,Pn.Zu6srm,Shield,,-49459.2,0
            // 20:06:10:15:29:43.0::Halaster,C[40 M17_Trial_Boss_Halaster],,*,Character, P[513125329@28357045 Character@user], Arcane Blast,Pn.Zu6srm,Physical,Dodge,49459.2,395673
            // Notice that this looks identical to the fully blue-shielded case above apart from the effectiveness.

            // Fully absorbed by both types of Shield, Physical is damage absorbed:
            // 20:03:13:22:14:53.8::Pillar Fire,C[24308 M18_Dungeon_Boss_2_Pillar_Fire],,*,Character, P[513125329@28357045 Character@user], Pool of Fire, Pn.Yg7mkc1,Shield,,-21284.9,0
            // 20:03:13:22:14:53.8::Pillar Fire,C[24308 M18_Dungeon_Boss_2_Pillar_Fire],,*,Character, P[513125329@28357045 Character@user], Pool of Fire, Pn.Yg7mkc1,Shield,,-21284.9,0
            // 20:03:13:22:14:53.8::Pillar Fire,C[24308 M18_Dungeon_Boss_2_Pillar_Fire],,*,Character, P[513125329@28357045 Character@user], Pool of Fire, Pn.Yg7mkc1,Physical,,42569.9,86625
            // There's no way to tell which line is which type of shield.

            // Fully absorbed, breaking tank shield but not overshield, Physical damage number is the damage absorbed:
            // 20:06:10:10:17:34.7::Halaster,C[10 M17_Trial_Boss_Halaster],,*,Character, P[513125329@28357045 Character@user], Duumvirate, Pn.6h4go6,Shield,ShieldBreak,-286873,0
            // 20:06:10:10:17:34.7::Halaster,C[10 M17_Trial_Boss_Halaster],,*,Character, P[513125329@28357045 Character@user], Duumvirate, Pn.6h4go6,Shield,ShieldBreak,-158627,0
            // 20:06:10:10:17:34.7::Halaster,C[10 M17_Trial_Boss_Halaster],,*,Character, P[513125329@28357045 Character@user], Duumvirate, Pn.6h4go6,Physical,ShieldBreak,445500,891000

            // Not quite fully absorbed, breaking both shields, so Physical damage number is the damage to HP:
            // 20:06:10:10:17:34.8::Halaster,C[10 M17_Trial_Boss_Halaster],,*,Barbarian,P[514242424@5215280 Barbarian@other],Duumvirate,Pn.T3r6sh,Shield,ShieldBreak,-402401,0
            // 20:06:10:10:17:34.8::Halaster,C[10 M17_Trial_Boss_Halaster],,*,Barbarian,P[514242424@5215280 Barbarian@other],Duumvirate,Pn.T3r6sh,Shield,ShieldBreak,-448137,0
            // 20:06:10:10:17:34.8::Halaster,C[10 M17_Trial_Boss_Halaster],,*,Barbarian,P[514242424@5215280 Barbarian@other],Duumvirate,Pn.T3r6sh,Physical,ShieldBreak,49144.1,1.79936e+06

            // So it seems that we need to collect up to two shield lines per Physical line.  If they add up to the same amount as Physical, it's fully shielded; otherwise it's partial.
            // There's no obvious way to distinguish the two partially shielded cases; I guess we need to distinguish them based on whether adding the absorbed damage would make effectiveness
            // improbably high.  See ProcessActionDamage for the implementation.

            // For some reason we sometimes get an extra line blocking 0 damage, ignore it:
            // 19:05:21:11:35:43.8::Arcturia,C[13942 M16_Boss_Arcturia_Dungeon],,*,Third,P[512369627@6638553 Third@third],Arcturia's Wail,Pn.Be1dij1,Shield,,-0,0
            // 19:05:21:11:35:43.8::Arcturia,C[13942 M16_Boss_Arcturia_Dungeon],,*,Third,P[512369627@6638553 Third@third],Arcturia's Wail,Pn.Be1dij1,Shield,ShieldBreak,-18267,0
            // 19:05:21:11:35:43.8::Arcturia,C[13942 M16_Boss_Arcturia_Dungeon],,*,Third,P[512369627@6638553 Third@third],Arcturia's Wail,Pn.Be1dij1,Physical,ShieldBreak,215063,333000
            if (l.mag == -0 && l.magBase == 0)
                return;

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

                if (l.magBase == 0) // Don't use magBaseAdj here.  Rounded to zero is not zero.
                {
                    mag = -l.mag;
                    magBase = -l.magBase;
                }
                else
                {
                    mag = -l.magBase;
                    magBase = -l.mag;
                }
                l.mag = mag;
                l.magBase = magBase;
                shielded = new Dnum((int)mag);

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
                if (l.evtDsp == "Cleanse")
                {
                    ProcessNamesOST(l);

                    AddCombatActionNW(
                        (int)SwingTypeEnum.CureDispel, l.critical, l.flank, l.dodge, l.special,
                        l.unitAttackerName, l.attackType, Dnum.NoDamage, l.mag, l.magBase,
                        l.logInfo.detectedTime, l.ts, l.unitTargetName, l.type);
                }
                else
                {
                    // ignore the rest as they're only expire messages
                }
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
                        (int)SwingTypeEnum.PowerHealing, l.critical, false, false, "", "Trickster [" + l.tgtDsp + "]",
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
                        (int)SwingTypeEnum.PowerHealing, l.critical, false, false, l.unitAttackerName, l.unitTargetName,
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
                    //  Trying to not end combat via power
                    AddCombatActionNW(
                    (int)SwingTypeEnum.PowerHealing, l.critical, false, l.dodge, l.special,
                    l.unitAttackerName, l.attackType, new Dnum(-magAdj), -l.mag, -l.magBase,
                    l.logInfo.detectedTime, l.ts, l.unitTargetName, l.type);
                }
            }
        }

        private void ProcessActionSPDN(ParsedLine l)
        {
            // Handle all the buff and proc buffs/debuffs
            int magAdj = (int)Math.Round(l.mag);
            int magBaseAdj = (int)Math.Round(l.magBase);
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
                    //   AddCombatActionHostile(l, (int)SwingTypeEnum.NonMelee, l.critical, l.special, l.attackType, Dnum.NoDamage, 0, l.type);
                    AddCombatActionNW(
                    (int)SwingTypeEnum.NonMelee, l.critical, l.flank, l.dodge, l.special,
                    l.unitAttackerName, l.attackType, new Dnum(-magAdj), -l.mag, -l.magBase,
                    l.logInfo.detectedTime, l.ts, l.unitTargetName, l.type);
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
                    float df = (float)val;
                    string shieldSpecialText = "Shield(" + df.ToString("F1") + ")";

                    if (special == "None")
                    {
                        special = shieldSpecialText;
                    }
                    else
                    {
                        special = l.special + " | " + shieldSpecialText;
                    }

                    // FIXME ~~Track whether a player is a tank?  We only ever need to add if they are.~~
                    // Disagree. Non-tanks have shields (dreadnought, both bard paragons with Song Ward class feature)
                    if ((int)df == (int)l.mag)
                    {
                        // If absorbed == magnitude, it was probably fully absorbed and we don't want to add them.
                        // But it might have been half-absorbed by Paladin Divine Champion, so check iff effectiveness is too low.
                        // Add if the effectiveness is lower than half what we expect (~50% normally, ~25% with deflect).
                        // This can wrongly add if DR is buffed to over 74% (max is 80%), but that's rare even in heal checks.
                        float dr = (float)(l.dodge ? 0.13 : 0.26);
                        if (l.mag < l.magBase * dr)
                            l.mag += df;
                    }
                    else
                    {
                        // If absorbed != magnitude, we want to add iff the attack broke a tank's shield.
                        // Add unless the effectiveness with it added is higher than we expect.
                        // This can wrongly add if the absorbed amount is small compared to the attack size, but that doesn't make much difference.
                        // This can wrongly fail to add if the player has uncapped defense, but that's unlikely for a tank.
                        float dr = (float)(l.dodge ? 0.26 : 0.51); // Expect at least 50% DR and 50% deflect severity.
                        float sum = l.mag + df;
                        if (sum < l.magBase * dr)
                            l.mag = sum;
                    }
                    magAdj = (int)l.mag;
                    float shielded = df / l.mag;
                    msShielded.Tags.Add("ShieldDmgF", l.mag);
                    msShielded.Tags.Add("ShieldP", shielded);
                }
            }

            if (l.evtInt == "Pn.Wypyjw1") // Knight's Valor,
            {
                // "13:07:18:10:30:48.3::Largoevo,P[201228983@6531604 Largoevo@largoevo],Ugan the Abominable,C[1469 Mindflayer_Miniboss_Ugan],Largoevo,P[201228983@6531604 Largoevo@largoevo],Knight's Valor,Pn.Wypyjw1,Physical,,449.42,1195.48
                // Attack goes SRC -> TRG and ignore the owner.  The SRC is not the owner's pet.

                ProcessNamesST(l);
                AddCombatActionHostile(l, (int)SwingTypeEnum.Melee, l.critical, special, l.attackType, magAdj, l.mag, l.type, l.magBase);
            }
            else if (l.evtInt == "Pn.Q3o7t91") // Bloodletter self-damage
            {
                // this needs special processing so that it doesn't process names in a way that tries to attribute someone else's companion to the player, when using Bloodletter on them
                // example: Barbarian taking damage from Bloodletter while attacking another player's companion. The player is not the owner of the companion.
                // Attack goes OWN -> TRG, and SRC is just informational
                // 23:07:07:17:11:40.0::Stof,P[509567510@19259169 Stof@stof#0000],Tutor,C[39688 Pet_Tutor],Stof,P[509567510@19259169 Stof@stof#0000],Bloodletter,Pn.Q3o7t91,Physical,,22160.4,29897
                ProcessNamesTargetOnly(l); // don't use OwnerSource or OST, only one entity is really relevant with this action (the hostile inter-target damage hit is recorded separately)
                AddCombatActionNW((int)SwingTypeEnum.Melee, l.critical, l.flank, l.dodge, l.special, l.unitTargetName, l.attackType, new Dnum(magAdj), l.mag, l.magBase, l.logInfo.detectedTime, l.ts, l.unitTargetName, l.type);
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
                    AddCombatActionHostile(l, (int)SwingTypeEnum.Melee, l.critical, l.special, l.attackType, Dnum.Miss, l.type, magBaseAdj);
                }
                else 
                */

                if (l.immune)
                {
                    if ((magAdj == 0) && (magBaseAdj == 0))
                    {
                        // 13:07:18:10:49:21.6::Tristan,C[2120 Pet_Dog],,*,Oll'noth the Dominator,C[1997 Mindflayer_Eventboss],Takedown,Pn.Ebxsjf,KnockBack,Immune,0,0

                        // Ignore CC immunity for now...
                        l.logInfo.detectedType = Color.Gray.ToArgb();
                    }
                    else
                    {
                        // Generally damaging attacks have mag=0 and magBase > 0 when Immune.
                        l.logInfo.detectedType = Color.Maroon.ToArgb();
                        AddCombatActionHostile(l, (int)SwingTypeEnum.Melee, l.critical, special, l.attackType, Dnum.NoDamage, l.mag, l.type, l.magBase);
                    }
                }
                else if (l.dodge)
                {
                    // "Dodge" in the log means that the attack was Deflected
                    l.logInfo.detectedType = Color.Maroon.ToArgb();
                    AddCombatActionHostile(l, (int)SwingTypeEnum.Melee, l.critical, special, l.attackType, magAdj, l.mag, l.type, l.magBase);
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
                        AddCombatActionHostile(l, (int)SwingTypeEnum.Melee, l.critical, special, l.attackType, magAdj, l.mag, l.type, l.magBase);
                    }
                }
            }
        }

        private void ProcessAction(ParsedLine l)
        {
            l.logInfo.detectedType = Color.Gray.ToArgb();

            if (!ActGlobals.oFormActMain.InCombat
                && (l.evtInt == "Autodesc.Combatevent.Falling"
                    || l.evtInt == "Pn.Mlg6n01" // Poison Spike Trap
                    || l.evtInt == "Pn.Sv2m0c1" // Spike Trap
                    || l.evtInt == "Pn.Rjmxw51" // Arrow Trap
                    || l.evtInt == "Pn.O4hc6g1")) // Fall damage
            {
                // Ignore, environmental damage shouldn't start an encounter.
                return;
            }

            if (l.type == "AttribModExpire") // Cleanse
            {
                ProcessActionCleanse(l);
            }
            else if (l.showPowerDisplayName)
            {
                // Non-damaging effects.
                ProcessActionSPDN(l);
            }
            else if (l.type == "Power")
            {
                ProcessActionPower(l);
            }
            else if (l.type == "HitPoints")
            {
                ProcessActionHeals(l);
            }
            else if (l.type == "Shield")
            {
                ProcessActionShields(l);
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
                // AddCombatActionHostile(l, (int)SwingTypeEnum.Melee, l.critical, l.special, "Killing", Dnum.Death, l.type);

                // Use encounter names attacker and target here.  This allows filtering
                if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encAttackerName, l.encTargetName))
                {
                    MasterSwing ms =
                        new MasterSwing((int)SwingTypeEnum.Melee, l.critical, l.special, Dnum.Death, l.logInfo.detectedTime, l.ts,
                            "Killing", l.unitAttackerName, "Death", l.unitTargetName);
                    ms.Tags.Add("Flank", l.flank);
                    ActGlobals.oFormActMain.AddCombatAction(ms);
                }
            }
        }

        // For hostile actions only.  Handles the SetEncounter().
        internal void AddCombatActionHostile(
            ParsedLine line, int swingType, bool critical, string special, string theAttackType, Dnum Damage, float realDamage, string theDamageType, float baseDamage = 0)
        {
            // Use encounter names attacker and target here.  This allows filtering
            if (ActGlobals.oFormActMain.SetEncounter(line.logInfo.detectedTime, line.encAttackerName, line.encTargetName))
            {
                // add Flank to AttackType if setting is set
                string tempAttack = theAttackType;
                if (line.flank && this.checkBox_flankSkill.Checked) tempAttack = theAttackType + ": Flank";

                if (line.srcInt.Contains("Artifact_Weapon_Illusion_Clone"))
                {
                    // if merging with player
                    line.unitAttackerName = line.srcDsp;
                    tempAttack = theAttackType + " (Mirage Weapon)";
                }
                AddCombatActionNW(
                    swingType, line.critical, line.flank, line.dodge, special, line.unitAttackerName,
                    tempAttack, Damage, realDamage, baseDamage, line.logInfo.detectedTime,
                    line.ts, line.unitTargetName, theDamageType);
            }
        }

        // Wrapper around AddCombatAction to add extra Tags that are used in the NW plugin.
        private void AddCombatActionNW(
            int swingType, bool critical, bool flank, bool deflect, string special, string attacker, string theAttackType,
            Dnum damage, float realDamage, float baseDamage,
            DateTime time, int timeSorter, string victim, string theDamageType)
        {
            MasterSwing ms = new MasterSwing(swingType, critical, special, damage, time, timeSorter, theAttackType, attacker, theDamageType, victim);

            ms.Tags.Add("DamageF", realDamage);
            ms.Tags.Add("BaseDamage", baseDamage);
            ms.Tags.Add("Flank", flank);
            ms.Tags.Add("Deflect", deflect);

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
            if (!listBox_players.Items.Contains(name))
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

    // TODO improve ownership code to handle mirage weapon clones, enemy spawners, and entity-spawning companions (tutor) properly
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

            OwnerInfo OwnerInfo = null;

            // Record owner of all pets we see.
            if (line.srcEntityType == EntityType.Pet && Array.IndexOf(NW_Parser.companionEntityPowers, line.evtInt) == -1)
            {
                OwnerInfo = new OwnerInfo();
                OwnerInfo.ownerDsp = line.ownDsp;
                OwnerInfo.ownerInt = line.ownInt;
                OwnerInfo.ownerEntityType = line.ownEntityType;
                OwnerInfo.petDsp = line.srcDsp;
                OwnerInfo.petInt = line.srcInt;

                petPlayerCache[line.srcInt] = OwnerInfo;
                playerPetCache[line.ownInt] = OwnerInfo;
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

        public OwnerInfo ResolveByPlayer(string nameInt)
        {
            OwnerInfo petOwner = null;
            if (playerPetCache.TryGetValue(nameInt, out petOwner))
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
            OwnerInfo OwnerInfo = null;

            // Record owner of all entities we see.
            if (line.srcEntityType == EntityType.Entity)
            {
                OwnerInfo = new OwnerInfo();
                OwnerInfo.ownerDsp = line.ownDsp;
                OwnerInfo.ownerInt = line.ownInt;
                OwnerInfo.ownerEntityType = line.ownEntityType;
                OwnerInfo.petDsp = line.srcDsp;
                OwnerInfo.petInt = line.srcInt;

                entityPlayerCache[line.srcInt] = OwnerInfo;
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

        // Keep a pointer to the parser so we can add damage events for attacks that are never matched.
        private NW_Parser parser;

        public UnmatchedShieldLines(NW_Parser p)
        {
            parser = p;
        }

        public void Clear()
        {
            LinkedListNode<ShieldLine> slnNext = active.First;
            if (slnNext != null)
            {
                LinkedListNode<ShieldLine> cur = slnNext;
                RemoveUnmatched(cur);
                slnNext = active.First;
            }
            //active.Clear();
        }

        public void AddShield(MasterSwing ms, ParsedLine line)
        {
            ShieldLine sl = new ShieldLine();
            sl.ms = ms;
            sl.line = line;

            active.AddLast(sl);
        }

        private void RemoveUnmatched(LinkedListNode<ShieldLine> cur)
        {
            // Drop old and unmatched shield line; the attack must have been fully absorbed.
            ShieldLine sl = cur.Value;
            ParsedLine l = sl.line;
            parser.AddCombatActionHostile(l, (int)SwingTypeEnum.Melee, l.critical, "Shield", l.attackType, new Dnum((int)l.mag), l.mag, "Physical", l.magBase);
            active.Remove(cur);
        }

        public MasterSwing MatchDamage(ParsedLine line)
        {
            LinkedListNode<ShieldLine> slnNext = active.First;

            MasterSwing found = null;

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

                // Check expired.
                TimeSpan diff = line.logInfo.detectedTime - sl.ms.Time;

                if (diff.TotalMilliseconds > 500)
                {
                    // Generally shield line should match in <= 100ms.
                    RemoveUnmatched(cur);
                }
                // Compare
                else if ((sl.line.evtInt == line.evtInt) &&
                        (sl.line.ownInt == line.ownInt) &&
                        // (sl.line.srcInt == line.srcInt) &&
                        (sl.line.tgtInt == line.tgtInt) &&
                        (line.type != "Shield") &&
                        (line.mag > 0.0)  // <- Skip zero damage lines.
                    )
                {
                    // Matched
                    active.Remove(cur);
                    if (found == null)
                        found = sl.ms;
                    else
                    {
                        // Add the damage from this line to the DamageF tag on 'found'.
                        float fd = (float)found.Tags["DamageF"];
                        fd += sl.line.mag;
                        found.Tags["DamageF"] = fd;
                    }
                }
            }

            return found;
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
            string line = logInfo.logLine;
            string[] split = line.Split(NW_Parser.separatorLog, StringSplitOptions.None);

            // TODO ouch. Implement this in a cleaner, more reliable way.
            if (split.Length > 13)
            {
                // Too many fields, probably one of the names has a comma in it.
                string overloadfix = line.Replace(", ", " ");
                split = overloadfix.Split(NW_Parser.separatorLog, StringSplitOptions.None);
            }
            if (split.Length != 13)
            {
                // Still not right, give up.
                this.error = true;
                return;
            }

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
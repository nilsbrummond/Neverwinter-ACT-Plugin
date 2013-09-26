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
[assembly: AssemblyVersion("0.0.9.1")]


/* Version History - npb
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


namespace Parsing_Plugin
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

        // For tracking source of Chaotic Growth heals.
        private Dictionary<string, ChaoticGrowthInfo> magicMissileLastHit = new Dictionary<string, ChaoticGrowthInfo>();

        private Dictionary<string, bool> playerCharacterNames = new Dictionary<string, bool>();
        private bool playersCharacterFound = false;

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

            // InitializeOwnerIsTheRealSource();

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
            if (Data.Damage > 0)
            {
                object d;
                Data.Tags.TryGetValue("DamageF", out d);
                float df = (float) d;
                return df.ToString("F1");
            }

            return Data.Damage.ToString();
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

            EncounterData.ExportVariables.Remove("maxhealward");
            EncounterData.ExportVariables.Remove("MAXHEALWARD");
            EncounterData.ExportVariables.Remove("powerdrain");

            CombatantData.ColumnDefs.Remove("PowerDrain");
            CombatantData.ColumnDefs.Remove("Threat +/-");
            CombatantData.ColumnDefs.Remove("ThreatDelta");

            CombatantData.ColumnDefs.Add("FlankDam%",
                new CombatantData.ColumnDef("FlankDam%", false, "VARCHAR(8)", "FlankDamPrec", GetCellDataFlankDamPrec, GetSqlDataFlankDamPrec, CDCompareFlankDamPrec));
            CombatantData.ColumnDefs.Add("DmgEffect%",
                new CombatantData.ColumnDef("DmgEffect%", false, "VARCHAR(8)", "DmgEffectPrec", GetCellDataDmgEffectPrec, GetSqlDataDmgEffectPrec, CDCompareDmgEffectPrec));
            CombatantData.ColumnDefs.Add("DmgTakenEffect%",
                new CombatantData.ColumnDef("DmgTakenEffect%", false, "VARCHAR(8)", "DmgTakenEffectPrec", GetCellDataDmgTakenEffectPrec, GetSqlDataDmgTakenEffectPrec, CDCompareDmgTakenEffectPrec));


            CombatantData.OutgoingDamageTypeDataObjects.Remove("Auto-Attack (Out)");
            CombatantData.OutgoingDamageTypeDataObjects.Remove("Skill/Ability (Out)");
            CombatantData.OutgoingDamageTypeDataObjects.Remove("Power Drain (Out)");
            CombatantData.OutgoingDamageTypeDataObjects.Remove("Threat (Out)");

            CombatantData.IncomingDamageTypeDataObjects.Remove("Power Drain (Inc)");
            CombatantData.IncomingDamageTypeDataObjects.Remove("Threat (Inc)");

            CombatantData.SwingTypeToDamageTypeDataLinksOutgoing[1].RemoveAt(0);
            CombatantData.SwingTypeToDamageTypeDataLinksOutgoing[2].RemoveAt(0);
            CombatantData.SwingTypeToDamageTypeDataLinksOutgoing.Remove(10);
            CombatantData.SwingTypeToDamageTypeDataLinksOutgoing.Remove(16);

            CombatantData.SwingTypeToDamageTypeDataLinksIncoming.Remove(10);
            CombatantData.SwingTypeToDamageTypeDataLinksIncoming.Remove(16);

            CombatantData.ExportVariables.Remove("threatstr");
            CombatantData.ExportVariables.Remove("threatdelta");
            CombatantData.ExportVariables.Remove("maxhealward");
            CombatantData.ExportVariables.Remove("MAXHEALWARD");

            DamageTypeData.ColumnDefs.Add("FlankHits", 
                new DamageTypeData.ColumnDef("FlankHits", false, "INT", "FlankHits", GetCellDataFlankHits, GetSqlDataFlankHits));
            DamageTypeData.ColumnDefs.Add("Flank%",
                new DamageTypeData.ColumnDef("Flank%", true, "VARCHAR(8)", "FlankPerc", GetCellDataFlankPrec, GetSqlDataFlankPrec)); 
            DamageTypeData.ColumnDefs.Add("Effectiveness",
                new DamageTypeData.ColumnDef("Effectiveness", true, "VARCHAR(8)", "Effectiveness", GetCellDataEffectiveness, GetSqlDataEffectiveness));

            AttackType.ColumnDefs.Add("FlankHits",
                new AttackType.ColumnDef("FlankHits", false, "INT", "FlankHits", GetCellDataFlankHits, GetSqlDataFlankHits, AttackTypeCompareFlankHits));
            AttackType.ColumnDefs.Add("Flank%",
                new AttackType.ColumnDef("Flank%", true, "VARCHAR(8)", "FlankPerc", GetCellDataFlankPrec, GetSqlDataFlankPrec, AttackTypeCompareFlankPrec));
            AttackType.ColumnDefs.Add("Effectiveness",
                new AttackType.ColumnDef("Effectiveness", true, "VARCHAR(8)", "Effectiveness", GetCellDataEffectiveness, GetSqlDataEffectiveness, AttackTypeCompareEffectiveness));

            MasterSwing.ColumnDefs["Damage"] =
                new MasterSwing.ColumnDef("Damage", true, "VARCHAR(128)", "DamageString", GetCellDataDamage, (Data) => { return Data.Damage.ToString(); }, (Left, Right) => { return Left.Damage.CompareTo(Right.Damage); });

            MasterSwing.ColumnDefs.Add("Flank", 
                new MasterSwing.ColumnDef("Flank", true, "CHAR(1)", "Flank", GetCellDataFlank, GetSqlDataFlank, MasterSwingCompareFlank));

            MasterSwing.ColumnDefs.Add("BaseDamage",
                new MasterSwing.ColumnDef("BaseDamage", true, "INT", "BaseDamageString", GetCellDataBaseDamage, GetSqlDataBaseDamage, MasterSwingCompareBaseDamage));

            MasterSwing.ColumnDefs.Add("Effectiveness",
                new MasterSwing.ColumnDef("Effectiveness", true, "VARCHAR(8)", "EffectivenessString", GetCellDataEffectiveness, GetSqlDataEffectiveness, MasterSwingCompareEffectiveness));

            ActGlobals.oFormActMain.ValidateLists();
            ActGlobals.oFormActMain.ValidateTableSetup();
        }

        void oFormActMain_LogFileChanged(bool IsImport, string NewLogFileName)
        {
            curActionTime = DateTime.MinValue;
            //purgePetCache();
            petOwnerRegistery.Clear();
            entityOwnerRegistery.Clear();
            magicMissileLastHit.Clear();

            playersCharacterFound = false;
        }

        void oFormActMain_OnCombatEnd(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            curActionTime = DateTime.MinValue;

            // Don't actually want this.  Maybe on zone changes.
            // purgePetCache();

            magicMissileLastHit.Clear();
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
            processBasic(pl);

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
            processAction(pl);
        }

        private void processNamesOST(ParsedLine line)
        {
            // Owner, Source (belongs to owner), Target
            petOwnerRegistery.Register(line);
            entityOwnerRegistery.Register(line);

            processOwnerSourceNames(line);
            processTargetNames(line);
        }

        private void processNamesST(ParsedLine line)
        {
            // Source, Target: All independant
            processSourceNames(line);
            processTargetNames(line);
        }

        private void processNamesTargetOnly(ParsedLine line)
        {
            // Target only
            processTargetNames(line);
        }

        private void processBasic(ParsedLine line)
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

        private void processOwnerSourceNames(ParsedLine line)
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

        private void processSourceNames(ParsedLine line)
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

        private void processTargetNames(ParsedLine line)
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

        private void processActionHeals(ParsedLine l)
        {
            int magAdj = (int)Math.Round(l.mag);
            int magBaseAdj = (int)Math.Round(l.magBase);

            l.logInfo.detectedType = l.critical ? Color.Green.ToArgb() : Color.DarkGreen.ToArgb();

            // Heals can not start an encounter.
            if (ActGlobals.oFormActMain.InCombat)
            {
                processNamesOST(l);

                // PVP Rune Heal - Needs some cleanup.  Use the player as the source since they grabbed it.
                // Does 'Pn.R0jdk' == PVP RUNE HEAL???
                // 13:07:09:14:00:23.2::Rune,C[317 Pvp_Rune_Heal],,*,Mus'Mugen Uhlaalaa,P[201045055@5998737 Mus'Mugen Uhlaalaa@bupfen],Heal,Pn.R0jdk,HitPoints,,-1136.92,0

                if (l.evtInt == "Pn.R0jdk") // Assume this is PVP Rune Heal for now...
                {
                    // Use encounter names attacker and target here.  This allows filtering
                    if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encTargetName, l.encTargetName))
                    {
                        AddCombatAction(
                            (int)SwingTypeEnum.Healing, l.critical, false, l.special, l.unitTargetName,
                            "PVP Heal Rune", new Dnum(-magAdj), -l.mag, -l.magBase, l.logInfo.detectedTime,
                            l.ts, l.unitTargetName, l.type);
                    }
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

                    // Use encounter names attacker and target here.  This allows filtering
                    if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encTargetName, l.encTargetName))
                    {
                        AddCombatAction(
                            (int)SwingTypeEnum.Healing, l.critical, false, l.special, l.unitTargetName,
                            l.evtDsp, new Dnum(-magAdj), -l.mag, -l.magBase, l.logInfo.detectedTime,
                            l.ts, l.unitTargetName, l.type);
                    }
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
                        if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, cgi.encName, l.encTargetName))
                        {
                            AddCombatAction(
                                (int)SwingTypeEnum.Healing, l.critical, l.flank, l.unitAttackerName, cgi.unitName,
                                l.evtDsp, new Dnum(-magAdj), -l.mag, -l.magBase, l.logInfo.detectedTime,
                                l.ts, l.unitTargetName, l.type);
                        }

                        handled = true;
                    }

                    if (!handled)
                    {
                        // Use encounter names attacker and target here.  This allows filtering
                        if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encTargetName, l.encTargetName))
                        {
                            AddCombatAction(
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
                    addCombatAction(l, (int)SwingTypeEnum.Healing, l.critical, l.special, l.attackType, new Dnum(-magAdj), -l.mag, l.type);
                }
            }
        }

        private void processActionShields(ParsedLine l)
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

            processNamesOST(l);

            // Use encounter names attacker and target here.  This allows filtering
            if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encAttackerName, l.encTargetName))
            {
                // Put the attacker and the attack type in the special field.
                string special = l.unitAttackerName + " : " + l.attackType;

                Dnum shielded = null;
                float mag = 0;
                float magBase = 0;

                // This is just weird...
                if (magBaseAdj == 0)
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
            }
        }

        private void processActionCleanse(ParsedLine l)
        {
            l.logInfo.detectedType = Color.Blue.ToArgb();

            // Cleanse
            // 13:07:17:10:37:53.5::righteous,P[201081445@5908801 righteous@r1ghteousg],,*,KingOfSwordsx2,P[201247997@5290133 KingOfSwordsx2@sepherosrox],Cleanse,Pn.H8hm3x1,AttribModExpire,ShowPowerDisplayName,0,0

            if (ActGlobals.oFormActMain.InCombat)
            {
                processNamesOST(l);
                addCombatAction(l, (int)SwingTypeEnum.CureDispel, l.critical, l.special, l.attackType, Dnum.NoDamage, 0, l.type);
            }
        }

        private void processActionPower(ParsedLine l)
        {
            int magAdj = (int)Math.Round(l.mag);
            //int magBaseAdj = (int)Math.Round(l.magBase * 10);

            l.logInfo.detectedType = Color.Black.ToArgb();

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

                    processNamesTargetOnly(l);

                    // Target is the source as well.
                    if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.tgtDsp, l.tgtDsp))
                    {
                        AddCombatAction(
                            (int)SwingTypeEnum.PowerHealing, l.critical, false, "", "Trickster [" + l.tgtDsp + "]",
                            "Bait and Switch", new Dnum(-magAdj), -l.mag, 0, l.logInfo.detectedTime,
                            l.ts, l.tgtDsp, l.type);
                    }
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

                    processNamesST(l);

                    if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.unitTargetName, l.unitTargetName))
                    {
//                        ActGlobals.oFormActMain.AddCombatAction(
//                            (int)SwingTypeEnum.PowerHealing, l.critical, l.unitAttackerName, l.unitTargetName,
//                            l.evtDsp, new Dnum(-magAdj), l.logInfo.detectedTime,
//                            l.ts, l.unitTargetName, l.type); 

                        AddCombatAction(
                            (int)SwingTypeEnum.PowerHealing, l.critical, false, l.unitAttackerName, l.unitTargetName,
                            l.evtDsp, new Dnum(-magAdj), -l.mag, 0, l.logInfo.detectedTime,
                            l.ts, l.unitTargetName, l.type);
                    }
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
                    processNamesOST(l);
                    addCombatAction(l, (int)SwingTypeEnum.PowerHealing, l.critical, l.special, l.attackType, new Dnum(-magAdj), -l.mag, l.type);
                }
            }
        }

        private void processActionSPDN(ParsedLine l)
        {
            // Handle all the buff and proc buffs/debuffs
            // type: PowerRecharge, Null, Alacrity, CombatAdvantage, Lightning(Storm Spell), CritSeverity, ...

            l.logInfo.detectedType = Color.DarkTurquoise.ToArgb();

            if (l.evtInt == "Pn.Fwolu") // Chaotic Growth
            {
                // Chaotic Growth (Fixed in latest NW patch)
                // 13:07:18:10:51:58.2::Tifa,P[200500793@6707245 Tifa@liliiith],,*,Guard,C[2205 Mindflayer_Duergarguardthrall],Chaotic Growth,Pn.Fwolu,Null,ShowPowerDisplayName,0,0

                l.logInfo.detectedType = Color.DarkOliveGreen.ToArgb();

                processNamesOST(l);

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
                    addCombatAction(l, (int)SwingTypeEnum.NonMelee, l.critical, l.special, l.attackType, Dnum.NoDamage, 0, l.type);
                }
            }
            else if (l.evtInt == "Pn.Zh5vu")
            {
                // Storm Spell
                // 13:07:18:10:49:10.1::Tifa,P[200500793@6707245 Tifa@liliiith],,*,Scourge,C[2143 Mindflayer_Scourge],Storm Spell,Pn.Zh5vu,Lightning,ShowPowerDisplayName,583.917,0

                // Ignore this as there is a damage log line to go with it.
            }
            else
            {
                // Default

                if (ActGlobals.oFormActMain.InCombat)
                {
                    processNamesOST(l);
                    addCombatAction(l, (int)SwingTypeEnum.NonMelee, l.critical, l.special, l.attackType, Dnum.NoDamage, 0, l.type);
                }
            }
        }

        private void processActionDamage(ParsedLine l)
        {
            int magAdj = (int)Math.Round(l.mag);
            int magBaseAdj = (int)Math.Round(l.magBase);

            l.logInfo.detectedType = l.critical ? Color.Red.ToArgb() : Color.DarkRed.ToArgb();

            
            if (l.evtInt == "Autodesc.Combatevent.Falling")
            {
                // Falling damage does not start combat...
                if (ActGlobals.oFormActMain.InCombat)
                {
                    processNamesOST(l);
                    addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, magAdj, l.mag, l.type, l.magBase);
                }
            }
            else if (l.evtInt == "Pn.Wypyjw1") // Knight's Valor,
            {
                // "13:07:18:10:30:48.3::Largoevo,P[201228983@6531604 Largoevo@largoevo],Ugan the Abominable,C[1469 Mindflayer_Miniboss_Ugan],Largoevo,P[201228983@6531604 Largoevo@largoevo],Knight's Valor,Pn.Wypyjw1,Physical,,449.42,1195.48
                // Attack goes SRC -> TRG and ignore the owner.  The SRC is not the owner's pet.

                processNamesST(l);
                addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, magAdj, l.mag, l.type, l.magBase);
            }
            else
            {
                processNamesOST(l);

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
                    addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, Dnum.Miss, l.type, magBaseAdj);
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
                        addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, Dnum.NoDamage, l.mag, l.type, l.magBase);
                    }
                }
                else if (l.dodge)
                {
                    // It really looks like Dodge does not stop all damage - just reduces it by about 80%...
                    // I have seen damaging attacks that are both Dodge and Kill in the flags.
                    // So the target dodged but still died.
                    l.logInfo.detectedType = Color.Maroon.ToArgb();
                    addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, magAdj, l.mag, l.type, l.magBase);
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
                        // All attacks have a magBase.
                        addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, magAdj, l.mag, l.type, l.magBase);
                    }
                }
            }
        }

        private void processAction(ParsedLine l)
        {
            l.logInfo.detectedType = Color.Gray.ToArgb();

            if (l.type == "HitPoints")
            {
                processActionHeals(l);
            }
            else if (l.type == "Shield")
            {
                processActionShields(l);
            }
            else if (l.type == "AttribModExpire") // Cleanse
            {
                processActionCleanse(l);
            }
            else if (l.type == "Power")
            {
                processActionPower(l);
            }
            else if (l.showPowerDisplayName)
            {
                // Non-damaging effects.
                processActionSPDN(l);
            }
            else
            {
                // What is left should all be damage.
                processActionDamage(l);
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
                // addCombatAction(l, l.swingType, l.critical, l.special, "Killing", Dnum.Death, l.type);

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

        private void addCombatAction(
            ParsedLine line, int swingType, bool critical, string special, string theAttackType, Dnum Damage, float realDamage, string theDamageType, float baseDamage=0)
        {
            // Use encounter names attacker and target here.  This allows filtering
            if (ActGlobals.oFormActMain.SetEncounter(line.logInfo.detectedTime, line.encAttackerName, line.encTargetName))
            {
                // add Flank to AttackType if setting is set
                string tempAttack = theAttackType;
                if (line.flank && this.checkBox_flankSkill.Checked) tempAttack = theAttackType + ": Flank";

                AddCombatAction(
                    swingType, line.critical, line.flank, special, line.unitAttackerName,
                    tempAttack, Damage, realDamage, baseDamage, line.logInfo.detectedTime,
                    line.ts, line.unitTargetName, theDamageType);
            }
        }

        private void AddCombatAction(
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

            //purgePetCache();
            SaveSettings();
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
            if (flags.Length > 0)
            {
                int extraFlagCount = 0;
                special = "None";
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
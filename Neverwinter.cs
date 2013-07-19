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

/*
TODO:
- set encounter name
- set character name
- power replenish shall not be inc damage
- overheal ?? prolly the diff of l.mag to l.magBase ??
- filter all non-player-chars check, how to handle ??
- activate help text in options window
- handle unknown activities, transfer into known activities
- remove threat as it is not in the logs.

*/

[assembly: AssemblyTitle("Neverwinter Parsing Plugin")]
[assembly: AssemblyDescription("A basic parser that reads the combat logs in Neverwinter.")]
[assembly: AssemblyCopyright("nils.brummond@gmail.com based on: Antday <Unique> based on STO Plugin from Hilbert@mancom, Pirye@ucalegon")]
[assembly: AssemblyVersion("0.0.7.3")]

/* Version History - npb
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
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_mergeNPC = new System.Windows.Forms.CheckBox();
            this.checkBox_mergePets = new System.Windows.Forms.CheckBox();
            this.checkBox_flankSkill = new System.Windows.Forms.CheckBox();
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Options";
            // 
            // checkBox_mergeNPC
            // 
            this.checkBox_mergeNPC.AutoSize = true;
            this.checkBox_mergeNPC.Location = new System.Drawing.Point(15, 56);
            this.checkBox_mergeNPC.Name = "checkBox_mergeNPC";
            this.checkBox_mergeNPC.Size = new System.Drawing.Size(291, 17);
            this.checkBox_mergeNPC.TabIndex = 2;
            this.checkBox_mergeNPC.Text = "Merge all NPC combatants by removing NPC unique IDs";
            this.checkBox_mergeNPC.UseVisualStyleBackColor = true;
            // 
            // checkBox_mergePets
            // 
            this.checkBox_mergePets.AutoSize = true;
            this.checkBox_mergePets.Location = new System.Drawing.Point(15, 72);
            this.checkBox_mergePets.Name = "checkBox_mergePets";
            this.checkBox_mergePets.Size = new System.Drawing.Size(284, 17);
            this.checkBox_mergePets.TabIndex = 3;
            this.checkBox_mergePets.Text = "Merge all pet data to owner and remove pet from listing";
            this.checkBox_mergePets.UseVisualStyleBackColor = true;
            // 
            // checkBox_flankSkill
            // 
            this.checkBox_flankSkill.AutoSize = true;
            this.checkBox_flankSkill.Location = new System.Drawing.Point(15, 88);
            this.checkBox_flankSkill.Name = "checkBox_flankSkill";
            this.checkBox_flankSkill.Size = new System.Drawing.Size(213, 17);
            this.checkBox_flankSkill.TabIndex = 4;
            this.checkBox_flankSkill.Text = "Split skills in to flank and non-flank skills";
            this.checkBox_flankSkill.UseVisualStyleBackColor = true;
            // 
            // NW_Parser
            // 
            this.Controls.Add(this.checkBox_flankSkill);
            this.Controls.Add(this.checkBox_mergePets);
            this.Controls.Add(this.checkBox_mergeNPC);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "NW_Parser";
            this.Size = new System.Drawing.Size(650, 150);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox_mergeNPC;
        private System.Windows.Forms.CheckBox checkBox_mergePets;
        private System.Windows.Forms.CheckBox checkBox_flankSkill;

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

        // Pet internal id to owner display name

        internal static Dictionary<string, PetInfo> petPlayerCache = new Dictionary<string, PetInfo>();
        internal Dictionary<string, PetInfo> playerPetCache = new Dictionary<string, PetInfo>();

        internal Dictionary<string, bool> ownerIsTheRealSource = null;

        // For tracking source of Chaotic Growth heals.
        internal Dictionary<string, ChaoticGrowthInfo> magicMissileLastHit = new Dictionary<string, ChaoticGrowthInfo>();

        // Instant when the current combat action took place
        private DateTime curActionTime = DateTime.MinValue;

        Label lblStatus = null;

        TreeNode optionsNode = null;

        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "neverwinter.config.xml");
        SettingsSerializer xmlSettings;

        private int parsedLineCount = 0;
        private int errorLineCount = 0;
        private int unknownLineCount = 0;

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
            //ActGlobals.oFormActMain.SetOptionsHelpText("testing");

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

            InitializeOwnerIsTheRealSource();

            FixupCombatDataStructures();

            // Set status text to successfully loaded
            lblStatus = pluginStatusText;
            lblStatus.Text = "Neverwinter ACT plugin loaded";
        }

        private void InitializeOwnerIsTheRealSource()
        {
            // TODO:  Should this be the default (owner gets credit) the exception be the src gets credit?
            //        Need to look at that appoach.  Would a Pet be the only exception to that rule?

            ownerIsTheRealSource = new Dictionary<string, bool>();

            // Doom!
            // "13:07:09:11:01:08.4::Correk,P[201028460@1546238 Correk@Gleyvien],Target Dummy,C[265291 Entity_Targetdummy],,*,Doom!,Pn.F1j0yx1,Radiant,Critical,10557.2,8445.78"
            ownerIsTheRealSource.Add("Pn.F1j0yx1", true);

            // Astral Shield
            // 13:07:08:10:04:36.5::Fangogermany,P[200678852@4820887 Fangogermany@fangogermany],Divine Astral Shield,C[635 Entity_Cleric_Astralshield_Divine],Thorin Demoneye,P[201207170@6240948 Thorin Demoneye@mugen714],Astral Shield,Pn.Do58x01,HitPoints,,-442.911,0
            ownerIsTheRealSource.Add("Pn.Do58x01", true);

            // Guard Break..??
            // "13:07:03:11:47:12.6::Grizzard,P[200743305@6022049 Grizzard@shamedy],Cutter,C[9395 Winterforge_Frost_Goblin_Cutter],Grizzard,P[200743305@6022049 Grizzard@shamedy],Guard Break,Pn.Jy04um1,Power,,-23.1135,0
            ownerIsTheRealSource.Add("Pn.Jy04um1", true);

            // Flame Strike
            // 13:07:09:12:30:28.4::Rolen Taletreader,P[201260846@7855942 Rolen Taletreader@hehe13],Flame Strike,C[268743 Entity_Flamestrike],Target Dummy,C[268573 Entity_Targetdummy],Flame Strike,Pn.1pwldl,Fire,,424.395,0
            ownerIsTheRealSource.Add("Pn.1pwldl", true);

            // Arcane Singularity
            // 13:07:09:14:00:37.8::Natalia Ratova,P[200724456@4597813 Natalia Ratova@candlewolf],Phantasm,C[324 Entity_Arcanesingularity],Chicho,P[201128876@7369799 Chicho@hutch90],Arcane Singularity,Pn.Zlm7uf,ConstantForce,Immune,0,0
            ownerIsTheRealSource.Add("Pn.Zlm7uf", true);

            // Daunting Light
            // 13:07:09:14:06:14.6::Deepshanx,P[200977150@5512302 Deepshanx@deepshanx],Daunting Light,C[354 Entity_Dauntinglight_Zone],Rampage,P[200247698@5976052 Rampage@Aggovain],Daunting Light,Pn.7qb24b,Radiant,,2100.64,3546.14
            ownerIsTheRealSource.Add("Pn.7qb24b", true);

            // Storm Pillar
            // 13:07:09:19:11:06.4::Devwin,P[200237307@5570467 Devwin@chronalsurge],Storm Pillar,C[245 Entity_Stormpillar_Weak],Orexion,P[200057573@5810203 Orexion@theace02],Storm Pillar,Pn.Vexgm3,Lightning,,175.989,393.029
            ownerIsTheRealSource.Add("Pn.Vexgm3", true);

            // Bilethorn Weapon
            // 13:07:10:00:22:49.4::Pristina,P[200701659@6066607 Pristina@alnana],Lodur,C[42 Trickster_Baitandswitch],,*,Bilethorn Weapon,Pn.Nhw1351,Poison,,5.447,6.5
            ownerIsTheRealSource.Add("Pn.Nhw1351", true);

            // Hallowed Ground
            // 13:07:08:09:37:04.7::Kaps,P[200709935@7009499 Kaps@kaps181],Hallowed Ground,C[386 Entity_Hallowedground_Zone],Dirty Horror,P[201149078@6317095 Dirty Horror@scooby1361],Moon Touched,Pn.Fb9e3q,HitPoints,,-1429.35,0
            ownerIsTheRealSource.Add("Pn.Fb9e3q", true);

            // Shard of the Endless Avalanche
            // 13:07:17:10:24:30.2::zoorhie,P[200847904@7204132 zoorhie@zeldanora13],Shard,C[620 Entity_Shardoftheendlessavalanche],Chloe Oralia,P[200802208@6132862 Chloe Oralia@alluriya],Shardplosion,Pn.7d13go,Arcane,,2283.86,3696.82
            ownerIsTheRealSource.Add("Pn.7d13go", true);

            // Sudden Storm
            // 13:07:17:15:28:58.7::Sharee,P[201240783@6333626 Sharee@baelzeden],Domination Point,C[131 Pvp_Capturepoint],Bishop,P[200705682@6899836 Bishop@daytak],Sudden Storm Arc,Pn.6c0tls,Lightning,,94.6794,202.771
            ownerIsTheRealSource.Add("Pn.6c0tls", true);

            // Sacred Flame
            // 13:07:13:16:29:33.6::Kratos,P[200351767@6314814 Kratos@symon189],Infiltrator,C[111210 Mindflayer_Infiltrator],Kratos,P[200351767@6314814 Kratos@symon189],Sacred Flame,Pn.Ke4quc1,HitPoints,,-1390.32,0
            ownerIsTheRealSource.Add("Pn.Ke4quc1", true);
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
            object duration;

            if (Data.Tags.TryGetValue("BaseDamage", out duration))
            {
                int d = (int)duration;
                if (d == 0) return "";

                double dd = (double)d;
                dd /= 10;
                return dd.ToString("F1");
            }

            return "";
        }

        private string GetSqlDataBaseDamage(MasterSwing Data)
        {
            object duration;

            if (Data.Tags.TryGetValue("BaseDamage", out duration))
            {
                int d = (int)duration;
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
                int dl = (int)l;
                int dr = (int)r;

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
            object duration;

            if (Data.Tags.TryGetValue("Effectiveness", out duration))
            {
                double d = (double)duration;
                return d.ToString("P1");
            }

            return "";
        }

        private string GetSqlDataEffectiveness(MasterSwing Data)
        {
            object duration;

            if (Data.Tags.TryGetValue("Effectiveness", out duration))
            {
                double d = (double)duration;
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
                double dl = (double)l;
                double dr = (double)r;

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
                int d = (int)Data.Damage;
                double dd = (double)d;
                dd /= 10;
                return dd.ToString("F1");
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
            /*  BUG
             *  	
                [External Code]	
	            Neverwinter.cs-635096821166741155.dll!Parsing_Plugin.NW_Parser.GetAAFlankValue(Advanced_Combat_Tracker.AttackType Data) Line 587 + 0xd8 bytes	C#
 	            Neverwinter.cs-635096821166741155.dll!Parsing_Plugin.NW_Parser.GetDTFlankValue(Advanced_Combat_Tracker.DamageTypeData Data) Line 519 + 0xb bytes	C#
 	            Neverwinter.cs-635096821166741155.dll!Parsing_Plugin.NW_Parser.GetDTFlankPrecValue(Advanced_Combat_Tracker.DamageTypeData Data) Line 536 + 0xb bytes	C#
 	            Neverwinter.cs-635096821166741155.dll!Parsing_Plugin.NW_Parser.CDCompareFlankDamPrec(Advanced_Combat_Tracker.CombatantData Left, Advanced_Combat_Tracker.CombatantData Right) Line 717 + 0x32 bytes	C#
 	            [External Code]	
             *
             */

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
            // return GetAAFlankValue(Data).ToString() + " / " + Data.Hits.ToString();

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
            int dmgTotal = 0;
            int baseDmgTotal = 0;
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

                object fv;
                if (ms.Tags.TryGetValue("BaseDamage", out fv))
                {
                    int bd = (int) fv;

                    if (bd > 0)
                    {
                        dmgTotal += ms.Damage.Number;
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
            // - Display fixed point int for damage since ACT using integer damage and Neverwinter uses floating.
            // - TODO: Finish export vars cleanups.

            EncounterData.ColumnDefs["Damage"].GetCellData = (Data) => { return (Data.Damage / 10).ToString(GetIntCommas()); };
            EncounterData.ColumnDefs["EncDPS"].GetCellData = (Data) => { return (Data.DPS / 10).ToString(GetFloatCommas()); };

            EncounterData.ExportVariables.Remove("maxhealward");
            EncounterData.ExportVariables.Remove("MAXHEALWARD");
            EncounterData.ExportVariables.Remove("powerdrain");

            CombatantData.ColumnDefs.Remove("PowerDrain");
            CombatantData.ColumnDefs.Remove("Threat +/-");
            CombatantData.ColumnDefs.Remove("ThreatDelta");

            CombatantData.ColumnDefs["Damage"].GetCellData = (Data) => { return (Data.Damage / 10).ToString(GetIntCommas()); };
            CombatantData.ColumnDefs["Healed"].GetCellData = (Data) => { return (Data.Healed / 10).ToString(GetIntCommas()); };
            CombatantData.ColumnDefs["DPS"].GetCellData = (Data) => { return (Data.DPS / 10).ToString(GetFloatCommas()); };
            CombatantData.ColumnDefs["EncDPS"].GetCellData = (Data) => { return (Data.EncDPS / 10).ToString(GetFloatCommas()); };
            CombatantData.ColumnDefs["EncHPS"].GetCellData = (Data) => { return (Data.EncHPS / 10).ToString(GetFloatCommas()); };
            CombatantData.ColumnDefs["HealingTaken"].GetCellData = (Data) => { return (Data.HealsTaken / 10).ToString(GetIntCommas()); };
            CombatantData.ColumnDefs["DamageTaken"].GetCellData = (Data) => { return (Data.DamageTaken / 10).ToString(GetIntCommas()); };

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

            DamageTypeData.ColumnDefs["Damage"].GetCellData = (Data) => { return (Data.Damage / 10).ToString(GetIntCommas()); };
            DamageTypeData.ColumnDefs["EncDPS"].GetCellData = (Data) => { return (Data.EncDPS / 10.0).ToString(GetFloatCommas()); };
            DamageTypeData.ColumnDefs["CharDPS"].GetCellData = (Data) => { return (Data.CharDPS / 10.0).ToString(GetFloatCommas()); };
            DamageTypeData.ColumnDefs["DPS"].GetCellData = (Data) => { return (Data.DPS / 10.0).ToString(GetFloatCommas()); };
            DamageTypeData.ColumnDefs["Average"].GetCellData = (Data) => { return (Data.Average / 10.0).ToString(GetFloatCommas()); };
            DamageTypeData.ColumnDefs["Median"].GetCellData = (Data) => { return (Data.Median / 10).ToString(GetIntCommas()); };
            DamageTypeData.ColumnDefs["MinHit"].GetCellData = (Data) => { return (Data.MinHit / 10).ToString(GetIntCommas()); };
            DamageTypeData.ColumnDefs["MaxHit"].GetCellData = (Data) => { return (Data.MaxHit / 10).ToString(GetIntCommas()); };
            DamageTypeData.ColumnDefs.Add("FlankHits", 
                new DamageTypeData.ColumnDef("FlankHits", false, "INT", "FlankHits", GetCellDataFlankHits, GetSqlDataFlankHits));
            DamageTypeData.ColumnDefs.Add("Flank%",
                new DamageTypeData.ColumnDef("Flank%", true, "VARCHAR(8)", "FlankPerc", GetCellDataFlankPrec, GetSqlDataFlankPrec)); 
            DamageTypeData.ColumnDefs.Add("Effectiveness",
                new DamageTypeData.ColumnDef("Effectiveness", true, "VARCHAR(8)", "Effectiveness", GetCellDataEffectiveness, GetSqlDataEffectiveness));

            AttackType.ColumnDefs["Damage"].GetCellData = (Data) => { return (Data.Damage / 10).ToString(GetIntCommas()); };
            AttackType.ColumnDefs["EncDPS"].GetCellData = (Data) => { return Data.EncDPS.ToString(GetFloatCommas()); };
            AttackType.ColumnDefs["CharDPS"].GetCellData = (Data) => { return Data.CharDPS.ToString(GetFloatCommas()); };
            AttackType.ColumnDefs["DPS"].GetCellData = (Data) => { return Data.DPS.ToString(GetFloatCommas()); };
            AttackType.ColumnDefs["Average"].GetCellData = (Data) => { return (Data.Average / 10).ToString(GetIntCommas()); };
            AttackType.ColumnDefs["Median"].GetCellData = (Data) => { return (Data.Median / 10).ToString(GetIntCommas()); };
            AttackType.ColumnDefs["MinHit"].GetCellData = (Data) => { return (Data.MinHit / 10).ToString(GetIntCommas()); };
            AttackType.ColumnDefs["MaxHit"].GetCellData = (Data) => { return (Data.MaxHit / 10).ToString(GetIntCommas()); };
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
            purgePetCache();
            magicMissileLastHit.Clear();
        }

        void oFormActMain_OnCombatEnd(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            curActionTime = DateTime.MinValue;

            // Don't actually want this.  Maybe on zone changes.
            // purgePetCache();

            magicMissileLastHit.Clear();
        }

        private void purgePetCache()
        {
            petPlayerCache.Clear();
            playerPetCache.Clear();
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
            processOwnerSourceTarget(pl);
            processPetNames(pl);
            processNames(pl);

            logInfo.detectedType = parseLineType(pl).ToArgb();

            // Do the real stuff..
            processAction(pl);
        }

        private void processOwnerSourceTarget(ParsedLine line)
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
            else if (line.ownInt[0] == 'C') { line.ownEntityType = EntityType.Creature; }


            // Some abilties log damage in the form of:
            // Owner  = owner
            // Source = target
            // Target = target
            //
            // i.e. the target is the source for standing in the effect and not moving out of it.  The damage source should be the owner for credit.
            //
            // we use ownerIsTheRealSource to handle this.
            //

            if (ownerIsTheRealSource.ContainsKey(line.evtInt) || (line.srcInt == "*"))
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
                // If the <target> is a 'C' type and not equal to the entity in <owner>
                // then the <target> is a pet of the <owner>.  Does as 'C' ever have
                // another 'C' type as a pet?  not sure but this should handle it.
                if (line.srcInt.CompareTo(line.ownInt) != 0)
                {
                    line.srcEntityType = EntityType.Pet;
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
            else if (line.tgtInt[0] == 'C') { line.tgtEntityType = EntityType.Creature; }
            // NOTE: C -> Pet lookup is later.


            // Defaults for the clean names.
            line.encAttackerName = line.srcDsp;
            line.encTargetName = line.tgtDsp;
            line.unitAttackerName = line.srcDsp;
            line.unitTargetName = line.tgtDsp;
        }

        private void processPetNames(ParsedLine line)
        {
            // Problem lines:
            // "13:07:02:13:48:18.1::Kallista Hellbourne,P[200674407@288107 Kallista Hellbourne@tonyleon],,,Sentry,C[1150404 Frost_Goblin_Sentry],Storm Spell,Pn.Zh5vu,Lightning,ShowPowerDisplayName,580.333,0"
            // "13:07:03:11:47:12.6::Grizzard,P[200743305@6022049 Grizzard@shamedy],Cutter,C[9395 Winterforge_Frost_Goblin_Cutter],Grizzard,P[200743305@6022049 Grizzard@shamedy],Guard Break,Pn.Jy04um1,Power,,-23.1135,0"
            // "13:07:09:11:01:08.4::Correk,P[201028460@1546238 Correk@Gleyvien],Target Dummy,C[265291 Entity_Targetdummy],,*,Doom!,Pn.F1j0yx1,Radiant,Critical,10557.2,8445.78"
            // "13:07:09:11:01:59.5::Nasus king,P[201132249@7587600 Nasus king@portazorras],SerGay,C[265715 Pet_Clericdisciple],Target Dummy,C[265291 Entity_Targetdummy],Sacred Flame,Pn.Tegils,Physical,Flank,59.7605,0"

            bool exception = false;
            bool add = false;
            PetInfo petInfo = null;

            if (line.srcInt.Contains("Trickster_Baitandswitch"))
            {
                // TODO: if this turns into a list then use a dictionary lookup...
                // Bait and Switch Trickster.
                // Not a pet.
                exception = true;
            }

            // Record owner of all pets we see.
            if (line.srcEntityType == EntityType.Pet && (!exception))
            {
                if (petPlayerCache.TryGetValue(line.srcInt, out petInfo))
                {
                    if (petInfo.ownerInt != line.ownInt)
                    {
                        // Pet Owner changed...  Not sure if possible... but just in case.
                        petPlayerCache.Remove(petInfo.petInt);
                        playerPetCache.Remove(petInfo.ownerInt);
                        add = true;
                    }
                }
                else
                {
                    // Check if this player had another pet registered and clean up.
                    if (playerPetCache.TryGetValue(line.ownInt, out petInfo))
                    {
                        playerPetCache.Remove(line.ownInt);
                        petPlayerCache.Remove(petInfo.petInt);
                    }

                    add = true;
                }

                if (add)
                {
                    petInfo = new PetInfo();
                    petInfo.ownerDsp = line.ownDsp;
                    petInfo.ownerInt = line.ownInt;
                    petInfo.petDsp = line.srcDsp;
                    petInfo.petInt = line.srcInt;

                    petPlayerCache.Add(line.srcInt, petInfo);
                    playerPetCache.Add(line.ownInt, petInfo);
                }
            }

            // Lookup the creature to see if it is a pet.
            if (line.tgtEntityType == EntityType.Creature)
            {
                PetInfo petOwner = null;
                if (petPlayerCache.TryGetValue(line.tgtInt, out petOwner))
                {
                    // Target is a pet.
                    line.tgtEntityType = EntityType.Pet;
                    line.tgtPetInfo = petOwner;
                }
            }
            else if (line.tgtEntityType == EntityType.Pet)
            {
                // If a Pet then get the owner info..
                // The pet status was set via a '*' in the target...
                // It should be registered now.

                PetInfo petOwner = null;
                if (petPlayerCache.TryGetValue(line.tgtInt, out petOwner))
                {
                    line.tgtPetInfo = petOwner;
                }
                else
                {
                    line.tgtEntityType = EntityType.Creature;
                }
            }
        }


        private void processNames(ParsedLine line)
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
                        // Use the pet owner name for encounter name and filtering.
                        line.encAttackerName = line.ownDsp;

                        // Pet name:
                        line.unitAttackerName = line.srcDsp + " [" + line.ownDsp + "'s Pet]";
                        if (this.checkBox_mergePets.Checked)
                        {
                            line.unitAttackerName = line.ownDsp;
                        }
                        break;
                    }

                case EntityType.Creature:
                    {
                        line.encAttackerName = line.srcDsp;
                        String creatureId = line.srcInt.Split()[0].Substring(2);

                        if (checkBox_mergeNPC.Checked)
                        {
                            // Merge all NPCs to a single name.
                            line.unitAttackerName = line.srcDsp;
                        }
                        else
                        {
                            // Separate each NPC with its unique creature ID added.
                            line.unitAttackerName = line.srcDsp + " [" + creatureId + "]";
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
                        // Use the pet owner name for encounter name and filtering.
                        line.encTargetName = line.tgtPetInfo.ownerDsp;

                        // Pet name:
                        line.unitTargetName = line.tgtDsp + " [" + line.tgtPetInfo.ownerDsp + "'s Pet]";
                        if (this.checkBox_mergePets.Checked)
                        {
                            line.unitTargetName = line.tgtPetInfo.ownerDsp;
                        }
                        break;
                    }

                case EntityType.Creature:
                    {

                        if (line.tgtInt.Contains("Trickster_Baitandswitch"))
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

        private Color parseLineType(ParsedLine l)
        {

            // add action Killing
            if (l.kill)
            {
                return Color.Fuchsia;
            }

            if (l.srcEntityType == EntityType.Pet)
            {
                return Color.Aqua;
            }

            if (l.flags.Contains("Miss") || l.dodge || l.immune)
            {
                return Color.Blue;
            }
            else if (l.evtInt == "Pn.Vklp251")
            {
                //handle cleanse   "Reinigen/Cleanse"
                return Color.Orange;
            }
            else if (l.type == "Power")
            {
                return Color.Black;
            }
            else if (l.type == "HitPoints" || l.type == "Shield")
            {
                if (l.critical)
                {
                    return Color.Green;
                }
                else
                {
                    return Color.DarkGreen;
                }

            }
            else if (l.evtInt == "Autodesc.Combatevent.Falling")
            {
                return Color.DarkRed;
            }
            else
            {
                int magBase = (int)Math.Round(l.magBase);

                if (magBase > 0)
                {
                    //handle the rest
                    if (l.critical)
                    {
                        return Color.Red;
                    }
                    else
                    {
                        return Color.DarkRed;
                    }
                }
            }

            return Color.Gray;
        }

        private void processAction(ParsedLine l)
        {
            int magAdj = (int)Math.Round(l.mag * 10);
            int magBaseAdj = (int)Math.Round(l.magBase * 10);

            // Basic attack, magnitude is actual damage dealt taking resists/buffs/debuffs/critical into account, magnitudeBase is damage without these 
            if (l.evtInt == "Pn.H8hm3x1")
            {
                // 13:07:17:10:37:53.5::righteous,P[201081445@5908801 righteous@r1ghteousg],,*,KingOfSwordsx2,P[201247997@5290133 KingOfSwordsx2@sepherosrox],Cleanse,Pn.H8hm3x1,AttribModExpire,ShowPowerDisplayName,0,0

                if (ActGlobals.oFormActMain.InCombat)
                {
                    //handle cleanse   "Reinigen/Cleanse"
                    addCombatAction(l, (int)SwingTypeEnum.CureDispel, l.critical, l.special, l.attackType, Dnum.NoDamage, l.type);
                }
            }
            else if (l.type == "HitPoints")
            {
                // Heals can not start an encounter.
                if (ActGlobals.oFormActMain.InCombat)
                {
                    // PVP Rune Heal - Needs some cleanup.  Use the player as the source since they grabbed it.
                    // Does 'Pn.R0jdk' == PVP RUNE HEAL???
                    // 13:07:09:14:00:23.2::Rune,C[317 Pvp_Rune_Heal],,*,Mus'Mugen Uhlaalaa,P[201045055@5998737 Mus'Mugen Uhlaalaa@bupfen],Heal,Pn.R0jdk,HitPoints,,-1136.92,0

                    if (l.evtInt == "Pn.R0jdk") // Assume this is PVP Rune Heal for now...
                    {
                        // Use encounter names attacker and target here.  This allows filtering
                        if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encTargetName, l.encTargetName))
                        {
                            ActGlobals.oFormActMain.AddCombatAction(
                                (int)SwingTypeEnum.Healing, l.critical, l.special, l.unitTargetName,
                                "PVP Heal Rune", new Dnum(-magAdj), l.logInfo.detectedTime,
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
                            ActGlobals.oFormActMain.AddCombatAction(
                                (int)SwingTypeEnum.Healing, l.critical, l.special, l.unitTargetName,
                                l.evtDsp, new Dnum(-magAdj), l.logInfo.detectedTime,
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
                        if (magicMissileLastHit.TryGetValue(l.unitAttackerName, out cgi))
                        {
                            if (!cgi.triggered)
                            {
                                cgi.triggered = true;
                                cgi.ts = l.logInfo.detectedTime;
                            }

                            // Use encounter names attacker and target here.  This allows filtering
                            if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, cgi.encName, l.encTargetName))
                            {
                                ActGlobals.oFormActMain.AddCombatAction(
                                    (int)SwingTypeEnum.Healing, l.critical, l.unitAttackerName, cgi.unitName,
                                    l.evtDsp, new Dnum(-magAdj), l.logInfo.detectedTime,
                                    l.ts, l.unitTargetName, l.type);
                            }

                            handled = true;
                        }

                        if (!handled)
                        {
                            // Use encounter names attacker and target here.  This allows filtering
                            if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encTargetName, l.encTargetName))
                            {
                                ActGlobals.oFormActMain.AddCombatAction(
                                    (int)SwingTypeEnum.Healing, l.critical, l.unitAttackerName, l.unitTargetName,
                                    l.evtDsp, new Dnum(-magAdj), l.logInfo.detectedTime,
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
                        addCombatAction(l, (int)SwingTypeEnum.Healing, l.critical, l.special, l.attackType, new Dnum(-magAdj), l.type);
                    }
                }
            }
            else if (l.type == "Shield")
            {
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

                //
                // Target prevented damage.
                //

                // Use encounter names attacker and target here.  This allows filtering
                if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encAttackerName, l.encTargetName))
                {
                    // Put the attacker and the attack type in the special field.
                    string special = l.unitAttackerName + " : " + l.attackType;

                    ActGlobals.oFormActMain.AddCombatAction(
                        (int)SwingTypeEnum.Healing, false, special, l.unitTargetName,
                        l.type, new Dnum(-magBaseAdj), l.logInfo.detectedTime,
                        l.ts, l.unitTargetName, "HitPoints");
                }
            }
            else if (l.type == "Power")
            {
                if (ActGlobals.oFormActMain.InCombat)
                {
                    // special case: Bait and Switch
                    // 13:07:09:20:53:00.9::Lodur,C[835 Trickster_Baitandswitch],,*,Lodur,P[201093074@7545190 Lodur@lodur42],Trigger,Pn.He9xu,Power,,-0.521139,0
                    // 13:07:09:21:43:30.3::Lodur,C[152 Trickster_Baitandswitch],,*,Lodur,P[201093074@7545190 Lodur@lodur42],Trigger,Pn.He9xu,Power,Immune,0,0
                    // 13:07:10:09:11:08.8::Lodur,C[178 Trickster_Baitandswitch],,*,Lodur,P[201093074@7545190 Lodur@lodur42],Trigger,Pn.He9xu,Power,Immune,0,0


                    if (l.evtInt == "Pn.He9xu")
                    {
                        // TR - Bait and Switch Trigger
                        // Target is the source as well.
                        if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.tgtDsp, l.tgtDsp))
                        {
                            ActGlobals.oFormActMain.AddCombatAction(
                                (int)SwingTypeEnum.PowerHealing, l.critical, "", "Trickster [" + l.tgtDsp + "]",
                                "Bait and Switch", new Dnum(-magAdj), l.logInfo.detectedTime,
                                l.ts, l.tgtDsp, l.type);
                        }
                    }
                    else
                    {
                        // Normal Power case...
                        addCombatAction(l, (int)SwingTypeEnum.PowerHealing, l.critical, l.special, l.attackType, new Dnum(-magAdj), l.type);
                    }
                }
            }
            else if (l.evtInt == "Autodesc.Combatevent.Falling")
            {
                if (ActGlobals.oFormActMain.InCombat)
                {
                    addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, magAdj, l.type, magBaseAdj);
                }
            }
            else
            {
                //handle the rest
                //addCombatAction(l.logInfo.detectedTime, tempOwnerDsp, tempTargetDsp, l.swingType, l.critical, l.special, l.attackType, (int)Math.Round(l.mag), l.type, l.ts);

                if ((l.evtInt == "Pn.3t6cw8") && (magBaseAdj > 0)) // Magic Missile
                {
                    ChaoticGrowthInfo cgi = null;
                    if (magicMissileLastHit.TryGetValue(l.unitTargetName, out cgi))
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

                        magicMissileLastHit.Add(l.unitTargetName, cgi);
                    }
                }

                if (magBaseAdj > 0)
                {

                    if (l.flags.Contains("Miss"))
                    {
                        // TODO:  Not sure I have ever seen a "miss" in a log.  This actually valid?
                        addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, Dnum.Miss, l.type, magBaseAdj);
                    }
                    else if (l.immune)
                    {
                        // Generally damaging attacks have mag=0 and magBase > 0 when Immune.
                        addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, Dnum.NoDamage, l.type, magBaseAdj);
                    }
                    else if (l.dodge)
                    {
                        // It really looks like Dodge does not stop all damage - just reduces it by about 80%...
                        // I have seen damaging attacks that are both Dodge and Kill in the flags.  
                        // So the target dodged but still died.
                        addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, magAdj, l.type, magBaseAdj);
                    }
                    else
                    {
                        // All attacks have a magBase.
                        addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, magAdj, l.type, magBaseAdj);
                    }
                }
                else
                {
                    if (l.evtInt == "Pn.Yswhgl1")
                    {
                        // Shocking Execution
                        // It bypasses all defenses.  This, it turns out, leads to a non-zero damage and a 0 base damage.
                        // 13:07:17:10:33:01.9::Lodur,P[201093074@7545190 Lodur@lodur42],,*,KingOfSwordsx2,P[201247997@5290133 KingOfSwordsx2@sepherosrox],Shocking Execution,Pn.Yswhgl1,Physical,Critical|Flank|Kill,11887.1,0

                        addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, magAdj, l.type);
                    }
                    else
                    {
                        unknownLineCount++;
                    }
                }
            }

            // add action Killing
            if (l.kill)
            {
                // Clean from last MM hit.
                magicMissileLastHit.Remove(l.unitTargetName);

                // TODO: use tgtDsp or unitTargetName?
                ActGlobals.oFormSpellTimers.RemoveTimerMods(l.tgtDsp);
                ActGlobals.oFormSpellTimers.DispellTimerMods(l.tgtDsp);

                // No "Killing : Flank" ever.  Doesn't make sense since there is no damage in the kill tracking.
                // And it messes up the kill counts.
                // addCombatAction(l, l.swingType, l.critical, l.special, "Killing", Dnum.Death, l.type);

                // Use encounter names attacker and target here.  This allows filtering
                if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encAttackerName, l.encTargetName))
                {
                    ActGlobals.oFormActMain.AddCombatAction(
                        l.swingType, l.critical, l.special, l.unitAttackerName,
                        "Killing", Dnum.Death, l.logInfo.detectedTime,
                        l.ts, l.unitTargetName, "Death");
                }
            }
        }

        private void addCombatAction(
            ParsedLine line, int swingType, bool critical, string special, string theAttackType, Dnum Damage, string theDamageType, int baseDamage=0)
        {
            // Use encounter names attacker and target here.  This allows filtering
            if (ActGlobals.oFormActMain.SetEncounter(line.logInfo.detectedTime, line.encAttackerName, line.encTargetName))
            {
                // add Flank to AttackType if setting is set
                string tempAttack = theAttackType;
                if (line.flank && this.checkBox_flankSkill.Checked) tempAttack = theAttackType + ": Flank";

                AddCombatAction(
                    swingType, line.critical, line.flank, special, line.unitAttackerName,
                    tempAttack, Damage, baseDamage, line.logInfo.detectedTime,
                    line.ts, line.unitTargetName, theDamageType);
            }
        }

        private void AddCombatAction(
            int swingType, bool critical, bool flank, string special, string attacker, string theAttackType, 
            Dnum damage, int baseDamage,
            DateTime time, int timeSorter, string victim, string theDamageType)
        {
            MasterSwing ms = new MasterSwing(swingType, critical, special, damage, time, timeSorter, theAttackType, attacker, theDamageType, victim);

            ms.Tags.Add("BaseDamage", baseDamage);
            ms.Tags.Add("Flank", flank);

            if (baseDamage > 0)
            {
                double eff = (double)damage / (double)baseDamage;
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

    }

    internal class PetInfo
    {
        public string ownerDsp;
        public string ownerInt;
        public string petDsp;
        public string petInt;
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
        public bool critical, flank, dodge, immune, kill;
        public float mag, magBase;
        public bool error;

        //
        // Computed extra data.
        //

        public EntityType ownEntityType, srcEntityType, tgtEntityType;
        public PetInfo tgtPetInfo = null;

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
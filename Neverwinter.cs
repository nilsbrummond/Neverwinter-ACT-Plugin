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

*/

[assembly: AssemblyTitle("Neverwinter Parsing Plugin")]
[assembly: AssemblyDescription("A basic parser that reads the combat logs in Neverwinter.")]
[assembly: AssemblyCopyright("nils.brummond@gmail.com based on: Antday <Unique> based on STO Plugin from Hilbert@mancom, Pirye@ucalegon")]
[assembly: AssemblyVersion("0.0.6.0")]

/* Version History - npb
 * 0.0.6.0 - 2013/7/9
 *  - Combat log color coding
 *  - Pet name hash tables
 *  - encounter and unit clean names
 *  - pets filter as owner for selective parsing
 *  - Evaluating some of the delay parsing to see if needed
 *  - other minor improvements
 *  - improved combat start detection
 *  TODO: remove threat as it is not in the logs.
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
namespace Parsing_Plugin {
    public class NW_Parser : UserControl, IActPluginV1 {
  
		#region Designer Created Code (Avoid editing)
		
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
		
		#region Windows Form Designer generated code
		
		private void InitializeComponent() {
		
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_filterAlly = new System.Windows.Forms.CheckBox();
            this.checkBox_mergePets = new System.Windows.Forms.CheckBox();
            this.checkBox_flankSkill = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			
			 // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(350, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Neverwinter parser plugin Options";
            //this.label1.MouseHover += new System.EventHandler(this.label1_MouseHover);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Options";
            // 
            // checkBox_filterAlly
            // 
            this.checkBox_filterAlly.AutoSize = true;
            this.checkBox_filterAlly.Checked = false;
			this.checkBox_filterAlly.Enabled = false;
            //this.checkBox_filterAlly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_filterAlly.Location = new System.Drawing.Point(15, 56);
            this.checkBox_filterAlly.Name = "checkBox_filterAlly";
            this.checkBox_filterAlly.Size = new System.Drawing.Size(204, 17);
            this.checkBox_filterAlly.TabIndex = 2;
            this.checkBox_filterAlly.Text = "Filter all Non-Player-Characters from ACT";
            this.checkBox_filterAlly.UseVisualStyleBackColor = true;
            //this.checkBox_filterAlly.MouseHover += new System.EventHandler(this.checkBox_filterAlly_MouseHover);
            // 
            // checkBox_mergePets
            // 
            this.checkBox_mergePets.AutoSize = true;
			this.checkBox_mergePets.Checked = false;
			//this.checkBox_mergePets.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_mergePets.Location = new System.Drawing.Point(15, 72);
            this.checkBox_mergePets.Name = "checkBox_mergePets";
            this.checkBox_mergePets.Size = new System.Drawing.Size(181, 17);
            this.checkBox_mergePets.TabIndex = 3;
            this.checkBox_mergePets.Text = "Merge all pet data to owner and remove pet from listing";
            this.checkBox_mergePets.UseVisualStyleBackColor = true;
            //this.checkBox_mergePets.MouseHover += new System.EventHandler(this.checkBox_mergePets_MouseHover);
            // 
            // checkBox_flankSkill
            // 
            this.checkBox_flankSkill.AutoSize = true;
            this.checkBox_flankSkill.Checked = false;
            //this.checkBox_flankSkill.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_flankSkill.Location = new System.Drawing.Point(15, 88);
            this.checkBox_flankSkill.Name = "checkBox_flankSkill";
            this.checkBox_flankSkill.Size = new System.Drawing.Size(350, 17);
            this.checkBox_flankSkill.TabIndex = 4;
            this.checkBox_flankSkill.Text = "Merge flank and non-flank skills together";
            this.checkBox_flankSkill.UseVisualStyleBackColor = true;
            //this.checkBox_flankSkill.MouseHover += new System.EventHandler(this.checkBox_flankSkill_MouseHover);
			
			//this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBox_flankSkill);
            this.Controls.Add(this.checkBox_mergePets);
            this.Controls.Add(this.checkBox_filterAlly);
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
        private System.Windows.Forms.CheckBox checkBox_filterAlly;
        private System.Windows.Forms.CheckBox checkBox_mergePets;
        private System.Windows.Forms.CheckBox checkBox_flankSkill;
		
		#endregion
		
		public NW_Parser() {
            InitializeComponent();
        }
		
		internal static string[] separatorLog = new string[] { "::", "," };
        internal static string unk = "[Unknown]", unkInt = "C[Unknown]", pet = "<PET> ", unkAbility = "Unknown Ability";
        internal static CultureInfo cultureLog = new CultureInfo("en-US"), cultureDisplay = new CultureInfo("de-DE");

        // Pet internal id to owner display name

        internal static  Dictionary<string, PetInfo> petPlayerCache = new Dictionary<string, PetInfo>();
        internal Dictionary<string, PetInfo> playerPetCache = new Dictionary<string, PetInfo>();

        internal Dictionary<string, bool> ownerIsTheRealSource = null;

        // For tracking source of Chaotic Growth heals.
        internal Dictionary<string, ChaoticGrowthInfo> magicMissileLastHit = new Dictionary<string, ChaoticGrowthInfo>();

        // Instant when the current combat action took place
        private DateTime curActionTime = DateTime.MinValue;

        // Lines of the current action
        private List<ParsedLine> delayedActions = new List<ParsedLine>(20);

        Label lblStatus = null;
		
		TreeNode optionsNode = null;
		
		string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "neverwinter.config.xml");
        SettingsSerializer xmlSettings;
		
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText) {
            			
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
                dcIndex = ActGlobals.oFormActMain.OptionsTreeView.Nodes.Count-1;
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
            try {
                ActGlobals.oFormActMain.ResetCheckLogs();
            } catch {
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

			// Set status text to successfully loaded
            lblStatus = pluginStatusText;
            lblStatus.Text = "Neverwinter ACT plugin loaded";
        }

        private void InitializeOwnerIsTheRealSource()
        {
            // TODO:  Should this be the default (owner gets credit) the exception be the src get credit.

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
        }

        void oFormActMain_LogFileChanged(bool IsImport, string NewLogFileName) {
            curActionTime = DateTime.MinValue;
            purgePetCache();
            magicMissileLastHit.Clear();
        }

        void oFormActMain_OnCombatEnd(bool isImport, CombatToggleEventArgs encounterInfo) {
            curActionTime = DateTime.MinValue;

            // Don't actually want this.  Maybe on zone changes.
            // purgePetCache();

            magicMissileLastHit.Clear();
        }

        private void purgePetCache() {
            petPlayerCache.Clear();
            playerPetCache.Clear();
        }

        // Must match LogLineEventDelegate signature
        void oFormActMain_BeforeLogLineRead(bool isImport, LogLineEventArgs logInfo) {
            if (logInfo.logLine.Length < 30 || logInfo.logLine[19] != ':' || logInfo.logLine[20] != ':') {
                return;
            }

            if (logInfo.detectedTime != curActionTime) {
                // Different times mean new action block, any pending actions won't be related to those of the new block
                // parseDelayedActions(true);
                curActionTime = logInfo.detectedTime;
            }

            ParsedLine pl = new ParsedLine(logInfo);

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
                bool add = false;
                PetInfo petInfo = null;

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
                    line.unitAttackerName = line.srcDsp + " [" + creatureId + "]";
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
                        line.unitTargetName = line.tgtDsp + " [" + creatureId + "]";
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
            if (l.flags.Contains("Kill"))
            {
                return Color.Fuchsia;
            }

            if (l.srcEntityType == EntityType.Pet)
            {
                return Color.Aqua;
            }

            if (l.flags.Contains("Miss") || l.flags.Contains("Dodge") || l.flags.Contains("Immune"))
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

                int mag = (int)Math.Round(l.mag);
                int magBase = (int)Math.Round(l.magBase);
                int mitigated = magBase = mag;

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
            // Basic attack, magnitude is actual damage dealt taking resists/buffs/debuffs/critical into account, magnitudeBase is damage without these 
            if (l.evtInt == "Pn.Vklp251")
            {
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
                                "PVP Heal Rune", new Dnum((int)Math.Round(-l.mag)), l.logInfo.detectedTime,
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
                                l.evtDsp, new Dnum((int)Math.Round(-l.mag)), l.logInfo.detectedTime,
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
                                    l.evtDsp, new Dnum((int)Math.Round(-l.mag)), l.logInfo.detectedTime,
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
                                    l.evtDsp, new Dnum((int)Math.Round(-l.mag)), l.logInfo.detectedTime,
                                    l.ts, l.unitTargetName, l.type);
                            }
                        }
                    }
                    else
                    {
                        // Default heal.
                        addCombatAction(l, (int)SwingTypeEnum.Healing, l.critical, l.special, l.attackType, new Dnum((int)Math.Round(-l.mag)), l.type);
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
                        l.type, new Dnum((int)Math.Round(-l.mag)), l.logInfo.detectedTime,
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
                                "Bait and Switch", new Dnum((int)Math.Round(-l.mag)), l.logInfo.detectedTime,
                                l.ts, l.tgtDsp, l.type);
                        }
                    }
                    else
                    {
                        // Normal Power case...
                        addCombatAction(l, (int)SwingTypeEnum.PowerHealing, l.critical, l.special, l.attackType, new Dnum((int)Math.Round(-l.mag)), l.type);
                    }
                }
            }
            else if (l.evtInt == "Autodesc.Combatevent.Falling")
            {
                if (ActGlobals.oFormActMain.InCombat)
                {
                    addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, (int)Math.Round(l.mag), l.type);
                }
            }
            else
            {
                //handle the rest
                //addCombatAction(l.logInfo.detectedTime, tempOwnerDsp, tempTargetDsp, l.swingType, l.critical, l.special, l.attackType, (int)Math.Round(l.mag), l.type, l.ts);
                
                int mag = (int)Math.Round(l.mag);
                int magBase = (int)Math.Round(l.magBase);
                int mitigated = magBase = mag;

                if ((l.evtInt == "Pn.3t6cw8") && (magBase > 0)) // Magic Missile
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

                if (l.flags.Contains("Miss") || l.flags.Contains("Dodge") || l.flags.Contains("Immune"))
                {
                    // Handle Miss and Dodge
                    addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, Dnum.Miss, l.type);
                }
                else if (magBase > 0)
                {
                    // All attacks have a magBase.
                    addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, mag, l.type);
                }
            }

            // add action Killing
            if (l.flags.Contains("Kill"))
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

        /*
        private void parseDelayedActions(bool parseUnmatched) {
            int i = 0;
            while (i < delayedActions.Count) {
                ParsedLine l = delayedActions[i];
                bool parsed = false;
                //bool isPetAction = l.srcInt.CompareTo(l.ownInt) != 0 && !l.npcOwner;
                // For attacks absorbed by Shields, the owner is the actual damage source, the logged source is the attack target.
                // We need to know which player spawned the pet in order to record his indirect damage, but we won't know that until the pet hits hull.
                // So for now all pet shield damage before first hull hit is attributed to the pet, which sucks for mines since they only have one shot.
                //string realSrcInt = l.srcInt, realSrcDsp = l.srcDsp, realOwnDsp = l.ownDsp;
                //bool ignorePet =  l.type.Equals("Radiation") || l.type.Equals("Electrical") || l.attackType.Equals("Hargh'Peng Torpedo Secondary Detonation") || l.attackType.Contains("Ramming Speed");
                //if (!realSrcInt.Equals(unkInt)) {
                //    if (!ignorePet && isPetAction) {
                        // When a player pet hits the hull of something, store pet-player relationship for later lookups
                //        if (!petPlayerCache.ContainsKey(realSrcInt)) {
                //            petPlayerCache.Add(realSrcInt, realOwnDsp);
                //        }
                //    }
				//	if (ignorePet || l.npcOwner) {
                        // Potential pet shield attack, try to find actual owner
                 //       if (petPlayerCache.ContainsKey(l.ownInt)) {
                 //           realSrcInt = l.ownInt;
                 //           realSrcDsp = l.ownDsp;
                 //           realOwnDsp = petPlayerCache[l.ownInt];
                 //           ignorePet = false;
                 //           isPetAction = true;
                 //       } else {
                            // Owner yet unknown. We look ahead in current action lines to see if we have a matching hull damage to guess owner
                  //          bool found = false;
                 //           for (int j = i+1; j < delayedActions.Count; j++) {
                 //               ParsedLine p = delayedActions[j];
                 //               if (!p.npcOwner && p.srcInt == l.ownInt && p.evtInt == l.evtInt && p.tgtInt == l.tgtInt) {
                                    // Match when owner is a player, same event type, same target, and match source is equal to current line owner
                  //                  realSrcInt = l.ownInt;
                 //                   realSrcDsp = l.ownDsp;
                  //                  realOwnDsp = p.ownDsp;
                  //                  ignorePet = false;
                  //                  isPetAction = true;
                  //                  petPlayerCache.Add(realSrcInt, p.ownDsp);
                  //                  found = true;
                  //                  break;
                  //              }
                  //          }
                  //          if (!found) {
                  //              matched = false;
                  //          }
                  //      }
                  //  }
                //}
				//if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, tempOwnerDsp, victim))
				
				
                if (parseUnmatched) {
					string tempTargetDsp = l.tgtDsp;
					string tempOwnerDsp = l.ownDsp;
					//int tempSwingType = l.SwingType;
					
	
					
                    
                    // Basic attack, magnitude is actual damage dealt taking resists/buffs/debuffs/critical into account, magnitudeBase is damage without these 
					if (l.flags.Contains("Miss") || l.flags.Contains("Dodge")) {
						//handle Miss and Dodge
						addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, Dnum.Miss, l.type);
					} else if (l.evtInt == "Pn.Vklp251") {
                        if (ActGlobals.oFormActMain.InCombat)
                        {
                            //handle cleanse   "Reinigen/Cleanse"
                            addCombatAction(l, (int)SwingTypeEnum.CureDispel, l.critical, l.special, l.attackType, Dnum.NoDamage, l.type);
                        }
					} else if (l.type == "HitPoints") {
                        // Heals can not start an encounter.
                        if (ActGlobals.oFormActMain.InCombat)
                        {
                            if (l.evtInt != "Pn.Dbm4um1" && l.evtInt != "Pn.Qiwkdx1")
                            { //not eq "Lagerfeuer" and "Aufsteigen"
                                addCombatAction(l, (int)SwingTypeEnum.Healing, l.critical, l.special, l.attackType, new Dnum((int)Math.Round(-l.mag)), l.type);
                            }
						}
                    } else if (l.type == "Shield")
                    {
                        // "13:07:02:10:47:32.3::Lodur,P[201093074@7545190 Lodur@lodur42],,*,Ashurbanipal,P[201073234@7521465 Ashurbanipal@kolkotel],Sly Flourish,Pn.Pcrgk5,Shield,,-256.14,-142.802"
                        
                        // 

                        //
                        // addCombatAction(l, (int)SwingTypeEnum.Healing, l.critical, l.special, l.attackType, new Dnum((int)Math.Round(-l.mag)), l.type);
                        
                        // Attacker did no damage.
                        string blocked = "Blocked: " + ((int)Math.Round(l.mag)).ToString();
                        addCombatAction(l, l.swingType, l.critical, blocked, l.attackType, Dnum.NoDamage, "Unknown");

                        //
                        // Target prevented damage.
                        //

                        // Use encounter names attacker and target here.  This allows filtering
                        if (ActGlobals.oFormActMain.SetEncounter(l.logInfo.detectedTime, l.encAttackerName, l.encTargetName))
                        {
                            // add Flank to AttackType if setting is set
                            string tempAttack = l.attackType;
                            if (l.special == "Flank" && !this.checkBox_flankSkill.Checked) tempAttack = l.attackType + ": Flank";

                            ActGlobals.oFormActMain.AddCombatAction(
                                (int)SwingTypeEnum.Healing, false, "", l.unitTargetName,
                                tempAttack, new Dnum((int)Math.Round(-l.mag)), l.logInfo.detectedTime,
                                l.ts, l.unitTargetName, l.type);
                        }


					} else if (l.type == "Power" ) {
                        if (ActGlobals.oFormActMain.InCombat)
                        {
                            // TODO: Power replenish; shall not be inc damage; check !!
                            addCombatAction(l, (int)SwingTypeEnum.PowerHealing, l.critical, l.special, l.attackType, new Dnum((int)Math.Round(-l.mag)), l.type);
                        }
					} else if (l.evtInt == "Autodesc.Combatevent.Falling") {
                        if (ActGlobals.oFormActMain.InCombat)
                        {
                            addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, (int)Math.Round(l.mag), l.type);
                        }
                    } else {
						//handle the rest
						//addCombatAction(l.logInfo.detectedTime, tempOwnerDsp, tempTargetDsp, l.swingType, l.critical, l.special, l.attackType, (int)Math.Round(l.mag), l.type, l.ts);
                        addCombatAction(l, l.swingType, l.critical, l.special, l.attackType, (int)Math.Round(l.mag), l.type);
					}
                 
					// add action Killing
					if (l.flags.Contains("Kill")) {
							addCombatAction(l, l.swingType, l.critical, l.special, "Killing", Dnum.Death, l.type);
                            
					}
                    parsed = true;
                }

                if (parsed) {
                    delayedActions.RemoveAt(i);
                } else {
                    i++;
                }
            }
            if (parseUnmatched) {
                delayedActions.Clear();
            }
        }
         * */

		/*
		private void addCombatAction(DateTime Time, string attacker, string victim, int swingType, bool critical, string special, string theAttackType, Dnum Damage, string theDamageType, int ts) {
            if (ActGlobals.oFormActMain.SetEncounter(Time, attacker, victim)) {
				
                // add Flank to AttackType if setting is set
                string tempAttack = theAttackType;
				if (special == "Flank" && !this.checkBox_flankSkill.Checked) tempAttack = theAttackType + ": " + special;

                ActGlobals.oFormActMain.AddCombatAction(swingType, critical, special, attacker, tempAttack, Damage, Time, ts, victim, theDamageType);
            }
        }
        */

        private void addCombatAction(
            ParsedLine line, int swingType, bool critical, string special, string theAttackType, Dnum Damage, string theDamageType)
        {
            // Use encounter names attacker and target here.  This allows filtering
            if (ActGlobals.oFormActMain.SetEncounter(line.logInfo.detectedTime, line.encAttackerName, line.encTargetName))
            {
                // add Flank to AttackType if setting is set
                string tempAttack = theAttackType;
                if (special == "Flank" && !this.checkBox_flankSkill.Checked) tempAttack = theAttackType + ": " + special;

                ActGlobals.oFormActMain.AddCombatAction(
                    swingType, line.critical, special, line.unitAttackerName, 
                    tempAttack, Damage, line.logInfo.detectedTime,
                    line.ts, line.unitTargetName, theDamageType);
            }
        }


        // Must match the DateTimeLogParser delegate signature
        private DateTime ParseDateTime(string FullLogLine) {
            if (FullLogLine.Length >= 21 && FullLogLine[19] == ':' && FullLogLine[20] == ':') {
                return DateTime.ParseExact(FullLogLine.Substring(0, 19), "yy':'MM':'dd':'HH':'mm':'ss'.'f", System.Globalization.CultureInfo.InvariantCulture); ;
            }
            return ActGlobals.oFormActMain.LastKnownTime;
        }

        public void DeInitPlugin() {
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
		void LoadSettings() {
            
            xmlSettings.AddControlSetting(checkBox_filterAlly.Name, checkBox_filterAlly);
            xmlSettings.AddControlSetting(checkBox_mergePets.Name, checkBox_mergePets);
            xmlSettings.AddControlSetting(checkBox_flankSkill.Name, checkBox_flankSkill);
 
            if (File.Exists(settingsFile)) {
                FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlTextReader xReader = new XmlTextReader(fs);

                try {
                    while (xReader.Read()) {
                        if (xReader.NodeType == XmlNodeType.Element) {
                            if (xReader.LocalName == "SettingsSerializer") {
                                xmlSettings.ImportFromXml(xReader);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    lblStatus.Text = "Error loading settings: " + ex.Message;
                }
                xReader.Close();
            }
        }
		
        void SaveSettings() {
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
    internal class ParsedLine {

        public LogLineEventArgs logInfo;

        //
        // Parsed from the line.
        //
        
        public String ownDsp, ownInt, srcDsp, srcInt, tgtDsp, tgtInt, evtDsp, evtInt;
        public String type, attackType, special, flags;
        public int swingType, ts;
        public bool critical;
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



        public ParsedLine(LogLineEventArgs logInfo) {
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
            if (split.Length > 13 && split[13].CompareTo("EOF") == 0) {
                // Ugly patch for last lines parsing, since there's no way to trigger parsing at end of file without NREs in ACT
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


            if (flags.Contains("Critical")) {
                critical = true;
            }
			// special attacks for Flank
			special = "None";
			if (flags.Contains("Flank")) {
                special = "Flank";
            }
			

			//temp all Melee
            swingType = (int)SwingTypeEnum.NonMelee;
			// Das war früher "||" - wieder zurückgeändert
            //if (npcOwner && npcSource) {
            //    swingType = (int)SwingTypeEnum.NonMelee;
            //}

            attackType = evtDsp;
            if (attackType.Trim().Length == 0) {
                // Uggly fix for missing attack type
                attackType = NW_Parser.unkAbility;
            }
			//Special now used with Flank
            //int pos = attackType.IndexOf(" - ");
            //if (pos > 0) {
            //    special = attackType.Substring(pos + 3, attackType.Length - pos - 3);
            //    attackType = attackType.Substring(0, pos);
            //}
			//if (npcOwner || (npcSource && (mag>0)))
			//{
				//Pets als nonMelee anzeigne
			//	swingType = (int)SwingTypeEnum.NonMelee;
			//}
        }

        internal bool shouldDelayParse() {
            // Currently we only need to delay parse shield damage when the damage owner is a pet whose owner is unknown
            // return "Shield".Equals(type) && mag < 0 && magBase < 0 && npcOwner && !NW_Parser.petPlayerCache.ContainsKey(ownInt);
            return false;
        }
    }
}

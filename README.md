Neverwinter-ACT-Plugin
======================

An plugin for ACT (Advanced Combat Tracker) to parse combat log file from Neverwinter by Perfect World.

- ACT is found at http://advancedcombattracker.com/
- D&D Neverwinter is found at http://nw.perfectworld.com/
- The released version of this plugin can be downloaded from [HERE](https://s3.amazonaws.com/nw-act-plugin/Neverwinter.cs)

This plugin is based on the version 0.0.5.1 plugin by Antday \<Unique\>, 
which is based on the STO plugin from Hilbert @ mancom, Pirye @ ucalegon.


State
=====
- Imporved GW shieldblock tracking.  See new optional columns (DmgToShield, ShieldP)
- Stable enough for a 1.0 release.  Please report any issues you have.
- Tracking of CW Chaotic Growth gives the healing credit to the last CW to MM the target.
- Most special cases should be covered now.
- Combat log colored.
- Companion Pet's owners tracked.
- Damage base recording and effectiveness (dmg / dmgBase)
- Added 'Flank' (Combat Advantage?) as it's own column the way Critical Hits are.
- Removed Column types and features of ACT that are not used by the NW plugin.


Known Issues
============
- Devoted Cleric power "Flame Strike" causes falling damage that can not be tracked.  Falling damage does not specify who/what caused the fall.
- There is currently no way to detect zone changes from the Combatlog.Log file.
- When updating your plugin you may get a "plugin initialization failed" error.  The workaround is to remove the old plugin, restart ACT, and add the new plugin.
- injuries are counted as outgoing damage.  (Feature?)
- Cleric Disciple companions Sacred Flame temp HP effect is credited to the owner of the companion.


Install
=======
- Download the Neverwinter.cs file to your computer from [HERE](https://s3.amazonaws.com/nw-act-plugin/Neverwinter.cs).
- In the Neverwinter game use the command "/Combatlog 1" to begin logging to a file.  
- From ACT goto the plugins tab and "browse..." to and then enable the Neverwinter.cs plugin.  
- Goto the option tab now.   Select the Miscellaneous category and use the "Open log" button.  Select the file "C:\Users\Public\Games\Cryptic Studios\Neverwinter\Live\Logs\GameClient\Combatlog.Log".


Non-standard Column Types
=========================
There are a number of column types, some included by default and some not, added for Neverwinter ACT.  They can be turned on and off in the options tab.
- Encounter View (Options -> Main Table / Encounters -> Encounter View Options)
  - DmgTakenEffect%: The overall effectiveness of a combatants damage reduction
  - DmgEffect%: The overall effectiveness of a combtants armor penetration.
  - FlankDam%: The overall percentage of hits that where flanking (have Combat Advantage).
- Combatant View (Options -> Main Table / Encounters -> Combatant View Options)
  - FlankHits: The number of hits that are flanking.
  - Flank%: The percentage of hits that are flanking.
  - Effectiveness: The average (damage / BaseDamge) of the combatant.
- Damage Type View (Options -> Main Table / Encounters -> Damage Type View Options)
  - FlankHits: The number of hits that are flanking.
  - Flank%: The percentage of hits that are flanking.
  - Effectiveness: The average (damage / BaseDamge) of the damage type.
- Attack Type View (Options -> Main Table / Encounters -> Attack Type View Options)
  - DmgToShield: The damage of the attack that was shielded (GW Shield Block). 
  - ShieldP: The percent of the damage of an attack that is shielded.
  - Flank: Boolean indicating if the attack has flank (Combat Advantage) status.
  - BaseDamage: The raw damage reported before armor penetration and defense are applied.
  - Effectiveness: Actual damage / BaseDamage as the percentage.

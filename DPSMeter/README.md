# Shape of Dreams DPS Meter

Steam Workshop mod for Shape of Dreams.

## Current build

- Total damage
- DPS
- Hit count and elapsed damage time
- Per-skill damage
- Per-Essence damage
- Percentage contribution
- Reset button
- Hide button
- dps_reset console command
- dps_meter console command

## Attribution

The meter uses the game's existing ClientEventManager.OnTakeDamage event. The event supplies EventInfoDamage, including the final damage amount and the actor that caused it.

The actor API exposes the original entity (firstEntity), originating skill trigger (firstTrigger), and actor-tree lookup. Ability instances expose their originating Essence through gem.

No damage calculation is modified.

## Building

The project follows the game's current ModTemplate-style layout.

The project defaults ShapeOfDreamsHome to:

C:\Program Files (x86)\Steam\steamapps\common\Shape of Dreams

If Shape of Dreams is installed somewhere else, override ShapeOfDreamsHome in Visual Studio/MSBuild.

Build Release.

For Workshop distribution, place the completed project under the game's Mods folder, enable Developer Mode, load it through the in-game Mod Manager, and use the Workshop Upload button when ready.

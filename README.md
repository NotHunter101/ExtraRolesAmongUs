![character infographic](./characterGraphic.png)
# Extra Roles
A BepInEx mod for Among Us that adds 4 new roles into the game.

- [Medic](#medic)
- [Officer](#officer)
- [Engineer](#engineer)
- [Joker](#joker)

# Notice
This mod will work on Innersloth servers, but everybody in the lobby has to have the same version of the mod. For help with installing the mod, getting it to work, or fixing an issue, join the [Discord](https://discord.gg/j2MVs4r6cc).

[![Discord](https://discord.com/assets/e4923594e694a21542a489471ecffa50.svg)](https://discord.gg/j2MVs4r6cc)

**This mod cannot be installed on Android/iOS/Epic Games/Microsoft Store**

# What does the mod add?

## Medic
### **Team: Crewmates**
The Medic can give any player a shield that will make them immortal. Although, if The Medic dies, the shield will break.
The only exception is The Officer; they will still die if they try to kill a Crewmate.
The Medic's other feature shows when they find a corpse: they can get a report that contains clues to the killer's identity.
The type of information they get is based on a timer that can be configured inside the lobby.

## Game Options:
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Snow Medic | This is the game setting that toggles whether the Medic's name is lit up green for everybody in the game, or just themselves | Toggle | Off |
| Show Shielded Player | When The Medic shield's somebody, their visor will change to the color cyan. If this setting is set to 1, everybody can see the color change. If not, only the shielded player can see | Options | Self |
| Murder Attempt Indicator For Shielded Player | If this setting is enabled, the shielded player will hear a *ting* noise when somebody tries (and fails) to murder them | Toggle | On |
| Snow Medic Reports | When the medic reports the body and if this function is desactived, it will not show the Medic who reported it | Options | Off |
| Time Where Medic Reports Will Have Name | The amount of time (in seconds) that The Medic will have to report the body since death to get the killer's name | Number | 5 |
| Time Where Medic Reports Will Have Color Type | The amount of time (in seconds) that The Medic will have to report the body since death to get the killer's color type. "color type" means either "lighter" or "darker", and a full list of colors and their types are included at the bottom of the page | Number | 20 |
| Medic Spawn Chance | The percentage chance that anybody in the game will become The Medic | Number | 100% |
-----------------------
  
## Officer
### **Team: Crewmates**
The Officer is a class of Crewmate that is allowed to kill people, similar to Impostors.
Their goal is to locate the Impostor and deliver vigilante justice, but if they accidentally shoot a Crewmate, they die instead.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Snow Officer | If this setting is enabled, The Officer's name will be lit up blue for everyone. If it isn't, it will only be lit for themselves | Toggle | Off |
| Officer Kill Cooldown | This is the kill cooldown length for The Officer. The first cooldown on the first round will be equal to ten no matter what, just like the Impostor | Number | 30 |
| Officer Kill Behaviour | This settings control what will happen when The Officer attempts to kill someone in the game | Options | Impostor |
| Officer Spawn Chance | The percentage chance that anybody in the game will become The Officer | Number | 100% |
-----------------------
  
## Engineer
### **Team: Crewmates**
The Engineer can repair one emergency per game from anywhere on the entire map.
The other ability of The Engineer is that they are able to use the vents that were previously exclusive to Impostors.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Show Engineer | If this setting is enabled, The Engineer's name will be lit up orange for everyone. If it isn't, it will only be lit for themselves | Toggle | Off |
| Engineer Spawn Chance | The percentage chance that anybody in the game will become The Officer | Number | 100% |
-----------------------

## Joker
### Team: Neutral
The Joker is interesting; they aren't part of the Crewmates **or** Impostors, and they can only win by being falsely convicted as an Impostor.
If The Joker get's voted off the ship, the game will end instantly.
The Joker also has no tasks.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Show Joker | If this setting is enabled, The Joker's name will be lit up grey for everyone. If it isn't, it will only be lit for themselves | Toggle | Off |
| Joker Spawn Chance | The percentage chance that anybody in the game will become The Joker | Number | 100% |
-----------------------

# Releases:
| Among Us - Version | Mod Version | Link |
|----------|-------------|-----------------|
| **2021.3.5s** | v1.3.1-AU3.5s | [Download](https://github.com/NotHunter101/ExtraRolesAmongUs/releases/download/v1.3.1(3.5s)/Extra.Roles.v.1.3.1-3.5s.zip) (latest version)
| **2020.12.9s** | v1.3.1 | [Download](https://github.com/NotHunter101/ExtraRolesAmongUs/releases/download/v1.3.1/Role.Mod.v.1.3.1.zip)
  
## Install Instructions

1) Download and unzip the latest release from the [releases](https://github.com/NotHunter101/ExtraRolesAmongUs/releases/latest) tab.
2) Go to the Among Us install directory. On Steam, right-click the game, hover over "Manage", and click "Browse Local Files"
3) Drag every single file inside the downloaded .zip into your Among Us directory. (The folder that contains Among Us.exe)
4) Run the game. The mod will take pretty long to start the first time, but after that, it will start at about the same speed as normal.
5) To verify the mod is installed, look at the text in the top left of the menu screen.
6) Make sure it says "Mods: 3" and "Extra Roles Mod vX.X.X Loaded." (X.X.X being the current version number)

Not working? You might want to install the dependency [vc_redist](https://aka.ms/vs/16/release/vc_redist.x86.exe).

For an easier understanding on how to use the mod, watch this video: https://youtu.be/gtuqYsdir_k

# Game Options

## Show Medic
*Default: false*<br/>
This is the game setting that toggles whether the Medic's name is lit up green for everybody in the game, or just themselves.
  
## Show Shielded Player
*Default: true*<br/>
When The Medic shield's somebody, their visor will change to the color cyan. If this setting is set to 1, everybody can see the color change. If not, only the shielded player can see.
  
## Murder Attempt Indicator For Shielded Player
*Default: true*<br/>
If this setting is enabled, the shielded player will hear a *ting* noise when somebody tries (and fails) to murder them.
  
## Show Officer
*Default: false*<br/>
If this setting is enabled, The Officer's name will be lit up blue for everyone. If it isn't, it will only be lit for themselves.
  
## Officer Kill Cooldown
*Default: 30*<br/>
This is the kill cooldown length for The Officer. The first cooldown on the first round will be equal to ten no matter what, just like the Impostor.
  
## Show Engineer
*Default: false*<br/>
If this setting is enabled, The Engineer's name will be lit up orange for everyone. If it isn't, it will only be lit for themselves.
  
## Show Joker
*Default: false*<br/>
If this setting is enabled, The Jokers's name will be lit up grey for everyone. If it isn't, it will only be lit for themselves.
  
## Officer Kill Behaviour
*Default: Impostor*<br/>
This settings control what will happen when The Officer attempts to kill someone in the game.
- Impostor: The Officer can only kill the impostor. If The Officer attacks a Crewmate or The Joker The Officer will die.
- Joker: The Officer can kill either Impostor(s) or the Joker. If The Officer attacks a Crewmate The Officer will die.
- Crew Die: The Officer can kill both The Joker And Impostors without consequences. If The Officer attacks a Crewmate both the officer and the Crewmate die.
- Anyone: The Officer can kill anyone without consequences.
  
### Time Where Medic Reports Will Have Name
*Default: 5*<br/>
The amount of time (in seconds) that The Medic will have to report the body since death to get the killer's name.

## Time Where Medic Reports Will Have Color Type
*Default: 20*<br/>
The amount of time (in seconds) that The Medic will have to report the body since death to get the killer's color type.
"color type" means either "lighter" or "darker", and a full list of colors and their types are included at the bottom of the page.
  
## Medic Spawn Chance
*Default: 100*<br/>
The percentage chance that anybody in the game will become The Medic.
  
## Officer Spawn Chance
*Default: 100*<br/>
The percentage chance that anybody in the game will become The Officer.
  
## Engineer Spawn Chance
*Default: 100*<br/>
The percentage chance that anybody in the game will become The Engineer.
  
## Joker Spawn Chance
*Default: 100*<br/>
The percentage chance that anybody in the game will become The Joker.

# Color Types
- Red is darker.
- Blue is darker.
- Green is darker.
- Pink is lighter.
- Orange is lighter.
- Yellow is lighter.
- Grey is darker.
- White is lighter.
- Purple is darker.
- Brown is darker.
- Cyan is lighter.
- Lime is lighter.

# Bugs or feature suggestions
If you ever need to talk to someone for help fixing an issue, want to report a bug, or suggest a feature, do not hesitate to join the mod [Discord Server](https://discord.gg/j2MVs4r6cc).
[![Discord](https://discord.com/assets/e4923594e694a21542a489471ecffa50.svg)](https://discord.gg/j2MVs4r6cc)
 
# Resources
- https://github.com/NuclearPowered/Reactor The framework the mod uses.

- https://github.com/BepInEx For hooking game functions.

- https://github.com/Impostor/Impostor For running a non-official server. (currently, the anti-cheat breaks some features of the mod)

- https://github.com/DorCoMaNdO/Reactor-Essentials For creating custom game options easily.

- https://github.com/Woodi-dev/Among-Us-Sheriff-Mod For code snippets.

- https://github.com/tomozbot/SweeperMod For code snippets.

# License
This software is distributed under the <a href="https://github.com/NotHunter101/ExtraRolesAmongUs/blob/main/LICENSE">`GNU GPLv3 License`</a>. BepInEx is distributed under <a href="https://github.com/BepInEx/BepInEx/blob/master/LICENSE">`LGPL-2.1 License`</a>.

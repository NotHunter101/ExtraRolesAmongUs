# Extra Roles
A BepInEx mod for Among Us that adds 4 new roles into the game.

![character infographic](./characterGraphic.png)

# Notice
This mod cannot be played on Innersloth servers. A modded server is hosted by me and connected to by default, but if you want to join your own server, you can build my fork of the Impostor server and switch the IP in the config.

# What does the mod add?

## Medic
The Medic can give any player a shield that will make them immortal.  
The only exception is The Officer; they will still die if they try to kill a Crewmate.  
Although, if The Medic dies, the shield will break.  
The Medic's other feature shows when they find a corpse: they can get a report that contains clues to the killer's identity.  
The type of information they get is based on a timer that can be configured inside the host's config file. 
  
## Officer
The Officer is a class of Crewmate that is allowed to kill people, similar to Impostors.  
Their goal is to locate the Impostor and deliver vigilante justice, but if they accidentally shoot a Crewmate, they die instead.  
  
## Engineer
The Engineer cab repair one emergency per game from anywhere on the entire map.  
The other ablity of The Engineer is that they are able to use the vents that were previously exclusive to Impostors.  

## Joker
The Joker is interesting; they aren't part of the Crewmates **or** Impostors, and they can only win by convincing being falsely convicted as an Impostor.  
If The Joker get's voted off the ship, the game will end instantly.  
The Joker also has no tasks.  

# Configuration

There are a lot of configurable game settings in this mod, but they aren't changable in game. If you want to change your settings, go to the directory *Among Us\BepInEx\config* and open the *gg.reactor.extraroles.cfg* file with any text editor. The game options of the lobby host will be the ones loaded by default. The settings may be a little hard to understand, so I've compiled a list of what every one does below.

## Custom Server
  
### IP
*Default: 24.57.85.224  *
This value is the IP address that the mod will try to connect to when creating a lobby. If you build the custom server yourself, you have to change this to your IP. If you are okay to use the default server, you can just use the default setting.  
  
### Port
*Default: 22023  *
This value is the port the mod will use when connecting to the IP above. The default IP in the Impostor server is 22023, so you will most likely not need to change this.  
  
## Game Options
  
### Show Medic
*Default: 0  *

# Credits
https://github.com/NuclearPowered/Reactor The framework the mod uses.  
https://github.com/BepInEx For hooking game functions.  
https://github.com/Woodi-dev/Among-Us-Sheriff-Mod For code snippets.  
https://github.com/tomozbot/SweeperMod For code snippets.  
https://github.com/NotHunter101/Impostor My fork of Impostor that the mod needs to run.  
https://github.com/Hiumee/Impostor The fork of Impostor that can repair sabotages programmatically.  

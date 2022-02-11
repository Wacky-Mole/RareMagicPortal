# RareMagicPortal
Magical Portal Fluid + JVLServerSync + CraftingStation + CraftingStationLVL


Tired of Portals be the end all, be all of Valheim? You don't want to unnecessarily restrict which items can be teleported or not? Do you want
to see more PVP or more cooperation between your buddies and their bases?
Well, I've a mod for you!

Set a starting amount of PortalMagicFluid per new character.  This mod changes the recipe for portals and requires a new unique item called PortalMagicFluid. One Magical Portal Fluid per Portal.

Has four Server configurable values which are sync to client:
1)Turn on and off the new portal requirements (true, false)
2) Starting quantity of PortalMagicFluid (default 3)( int 0-250) [ Only applies to brand new character on first spawn in]
3) CraftingStation required nearby - Default is $piece_workbench for workbench.
4) Level required for that craftingStation to be able to build this piece. - I think unique to this mod. - Default 1

Admin can spawn in more items with name 'PortalMagicFluid'

Mod was produced with the hope that multiplayer servers will require more teamwork or more PVP to capture the scare resource. 
Other mods can allow it to be bought at the trader for high prices, gambled on or become rare drops from bosses.
﻿
Could be combined with WayShrine https://www.nexusmods.com/valheim/mods/1298 to create interesting maps.

Requires JVL and its dependencies.

The server will send the server config to overwrite client config on connect. The mod looks for changes in the .cfg and with ConfigurationManager
You can change midgame as an admin in 1.3.0 now!

It is better to set the starting quantity amount to 0 for Dedicated servers and let ServerCharacters mod handle the first time spawn in amounts.
https://valheim.thunderstore.io/package/Smoothbrain/ServerCharacters/

First Mod: Download and enjoy.
No known conflicts.

ChangeLog:


        Version 1.1.1
            Fixed Server Sync. - Now with moar Server Sync!
        Version 1.00
            Mod Release



Thank you to OdinPlus Team for some useful information.

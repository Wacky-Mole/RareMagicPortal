# RareMagicPortal

Magical Portal Fluid + ServerSync + CraftingStation + CraftingStationLVL + PortalHealth + Crystal and Key Economy. 

Includes pictures

Allows you to limit the amount of portals per character by giving every player a certain amount of PortalFluid. This is a good tool for restricting the amount of Portals on a server and increasing their scarcity and therefore their value.
Increase PVP and multiply cooperation by making portals a scarce resource. Allow only teammates or special access portals with Keys and Crystals.
Make Crystal and Key trading a part of the valheim econmoy to get around the world. 

Tired of Portals being the end all, be all of Valheim? You don't want to unnecessarily restrict which items can be teleported or not? Do you want
to see more PVP or more cooperation between your buddies and their bases?
Well, I've a mod for you!

2.0 adds 4 Crystals and Keys Each, RGB+G Red, Green, Blue and Gold/Master.
4 Crystals, PortalCrystalMaster, PortalCrystalRed , PortalCrystalGreen, PortalCrystalBlue, 4 keys PortalKeyRed , PortalKeyGold, PortalKeyBlue , PortalKeyGreen PortalKeyGreen

You can set the default color and default consumption per portal and change it once someone has gone through it. Make a portal admin only or free passage. 

Odin makes a way. 

Set a starting amount of PortalMagicFluid per new character.  This mod changes the recipe for portals and can require a new unique item called PortalMagicFluid. One Magical Portal Fluid per Portal.

Has 12 Server configurable values which are sync to client as of 2.0

0) Server Sync

1)Turn on and off PortalJuice (true, false)

2) Starting quantity of PortalMagicFluid (default 3)[ Only applies to brand new character on first spawn in]

3) CraftingStation required nearby - Default is $piece_workbench for workbench.

4) Level required for that craftingStation to be able to build this piece. - Default 1

5) OnlyCreatorCanDeconstruct - Default is true; Can still be destoryed

6) Portal Health: Default 400

7) Only the Creator can change the Portal Name : Default false;

8) Portal_Crystal_Enable = false  - turn on/off Crystals and Keys

9) Crystal_Consume_Default = 1. Default Crystal Consumption for new Portals

10)  Portal_Crystal_Color_Default = Red - Default Color: Red,  Gold is automically enabled for all plus - Default

11) PortalJuiceValue = 300 - Sets value of Portal Juice, a value of 0 keeps it from being sold at trader


Admin can spawn in more items with name 'PortalMagicFluid'

Mod was produced with the hope that multiplayer servers will require more teamwork or more PVP to capture the scarce resource.
Other mods can allow the resources to be bought at the trader for high prices, gambled on or become rare drops from bosses.
﻿
Could be combined with WayShrine https://www.nexusmods.com/valheim/mods/1298 to create interesting maps.

It is better to set the starting quantity amount to 0 for Dedicated servers and let ServerCharacters mod handle the first time spawn in amounts.
https://valheim.thunderstore.io/package/Smoothbrain/ServerCharacters/

First Mod: Download and enjoy.
No known conflicts.
ChangeLog:
        

        Version 2.0.0
            Rewrite of mod: Added Crystals and Keys. YML configuration on entering portals. 4 Different types of crystals and keys. 
            Crystals are Consumable. Keys are not. TargetPortal compatibility. YML file for each world. ServerSynced admin control
        Version 1.6.0
            Added three New Configs: Portal Health, OnlyCreatorCanDeconstruct (  destroy possible), OnlyCreatorCanRename
        Version 1.5.1
            Fixed potential SinglePlayer Bug
        Version 1.5.0
            Fixed another ServerSync bug, cleaned up log outputs.
        Version 1.4.1
            Fixed Single Player bug. 1.4.0 Removed JVL requirements.
        Version 1.4.0
            ﻿Removed JVL requirements from the Mod. Fixed Station Lvl message to display ingame station name.
        Version 1.3.0
            Added CraftingStation and CraftingStation lvl to config! You can now change midgame by editing cfg or with ConfigurationManager
        Version 1.1.1
            Fixed Server Sync. - Now with moar Server Sync!
        Version 1.00
            Mod Release



Thank you to OdinPlus Team for some useful information.
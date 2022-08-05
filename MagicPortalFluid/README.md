# RareMagicPortal

<img src="https://wackymole.com/hosts/FireandIce.png" width="500"/>  <img src="https://wackymole.com/hosts/GoldPortal.png" width="500"/>

`Server and client side required for Server Sync enforcement.`

Tired of portals being the end all, be all of Valheim?<br>
You don't want to unnecessarily restrict which items can be teleported or not?<br>
Do you want to see more PVP or more cooperation between your buddies and their bases?<br>
Do you want more colors on your portals or more freedom or restrictions?<br>
Well, I've a mod for you!

### Main Feature:
* Magical Portal Fluid
  * Added 1 extra required item to the Portal building recipe
  * Controlling the distribution of fluid = Controlling the amount of portals can be built in the world
  * Increasing portals' scarcity and value on a multiplayer server

* Crystal and Key Economy
  * v2.0 adds 4 Crystals and 4 Keys, color-tiered:Red, Green, Blue and Gold/Master.
    * Crystals will be **consumed** upon entering the portal
    * Keys to be **possessed** before entering the portal
      * > *Simulate the economy progression from spending and upgrade to long-holding possession*
  * Allows you to make special portal for:
    * Special access portal by team members only
    * Special toll charges for custom locations/bosses portal
  * Helps your server build a more interactive economy in Valheim

* Portal Colors - You can manually edit or cyle through colors with "LeftControl-E" as Admin or Owner with EnableCrystal off
 * > Black Portal -With EnableCrystals- Admin Only Portal
 * > Yellow Portal -With EnableCrystals- Normal Portal, no crystal or key req
 * > Red Portal  -With EnableCrystals- Red Crystal, Red Key, Gold Crystal, Gold Key
 * > Green Portal  -With EnableCrystals- Green Crystal, Green Key, Gold Crystal, Gold Key
 * > Blue Portal  -With EnableCrystals- Blue Crystal, Blue Key, Gold Crystal, Gold Key
 * > Gold Portal  -With EnableCrystals-  Gold Crystal, Gold Key
 * > White Portal -With EnableCrystals-  Teleport Anything, Traverese with Metals or Ore

<img src="https://wackymole.com/hosts/White2.png" width="700"/>  <img src="https://wackymole.com/hosts/OdinsBlessing.png" width="200"/>

* Portal Drink
 * Allows you do drink and be able to Teleport Anything for a configurable amount of time. (Turns Any Portal White with base color behind)

YML files are synced on creation to the rest of the clients. Server always override client files on sync, except on finding a new portal or changing Portal color. 

<img src="https://wackymole.com/hosts/typesofcrystals.png" width="300"/> <img src="https://wackymole.com/hosts/nored.png" width="300"/> <img src="https://wackymole.com/hosts/goldPortal.png" width="300"/>

## Prefab IDs:
  * PortalMagicFluid
  * PortalCrystalMaster
  * PortalCrystalRed
  * PortalCrystalGreen
  * PortalCrystalBlue
  * PortalKeyGold
  * PortalKeyRed
  * PortalKeyGreen
  * PortalKeyBlue
  * PortalDrink

## Configuration Options:
### RareMagicPortal.cfg
  * [General]
    * Force Server Config: 
      * > Enable/Disable ServerSync enforcement
    * YML Logs
      * > Useful for seeing what a Portal Requirements are: Default: True
    * CraftingStation_Requirement: 
      * > Default required workbench
    * Level_of_CraftingStation_Req: 
      * > Default required lvl 1 of workbench
    * OnlyCreatorCanDeconstruct: 
      * > Allow/Disallow ONLY creator of the portal can demolish/destroy the portal
    * Portal_Health: 
      * > Configure the HP of portal
    * OnlyCreatorCanChange: 
      * > Allow/Disallow ONLY creator of the portal can rename the portal tag

  * [Portal Crystals]
    * Portal_Crystal_Enable: 
      * > Enable/Disable Crystals and Keys to be loaded and used in game
    * Crystal_Consume_Default: 
      * > Default required 1 crystal for each portal
    * Portal_Crystal_Color_Default: 
      * > Default required RED crystal for TP consumption

  * [PortalJuice]
    * EnablePortalJuice: 
      * > Enable/Disable Portal Fluid to be loaded and used in game:
    * PortalMagicFluidSpawn: 
      * > Default spawning 3 fluids upon *NEW CHARACTER* created into the world/server
    * PortalJuiceValue: 
      * > Default 0 coins 
        >> Set to value of 1 or more to sale at Haldor

  * [Portal Drink]
    * Portal_drink_timer = 120 : Seconds that PortalDrink lasts


### YML (config/Portal_Names/*.yml)
  * > *The mod will auto generate default data into each yml named after your current world **upon your teleportation via ANY portal.***
```
  Demo_Portal_Name:
    Portal_Crystal_Cost:
      Red: 1
      Green: 0
      Blue: 0
      Gold: 1
    Portal_Key:
      Red: true
      Green: false
      Blue: false
      Gold: true
    Free_Passage: false  - No Crystal or Key requirement
    TeleportAnything: false  - Portal allows you to Teleport Anything
    AdditionalProhibitItems: -- Additional items restricted at this portal
    - Stone
    - Wood
    Admin_only_Access: false -- Only admins

```

## Compatibility:
* WayShrine by Azumatt
  * https://www.nexusmods.com/valheim/mods/1298
  * https://valheim.thunderstore.io/package/Azumatt/Wayshrine/
* TargetPortal by Smoothbrain
  * https://valheim.thunderstore.io/package/Smoothbrain/TargetPortal/
* and most of the other mods.

## Author's Note:
* Mod was produced with the hope that multiplayer servers will require more teamwork or more PVP to capture the scarce resource.
* Other mods can allow the resources to be bought at the trader for high prices, gambled on or become rare drops from bosses.
* If you are using *ServerCharacters*,
  * It is suggested to set the starting quantity amount to 0 for dedicated server and let ServerCharacters mod handle the first time spawn in amounts.
  * > https://valheim.thunderstore.io/package/Smoothbrain/ServerCharacters/

## Change Log:
        
        Version 2.1.1
            Changed Default for EnablePortalJuice to false for everyone that just wants PortalColors.
            Fixed Spelling errors. 
        Version 2.1.0
            Added PortalDrink - Configurable Time that allows a player to Teleport anything.
            Added PortalColor Changing feature, can be used exclusively or with Crystals and Keys
            Added AdditionalProhibitItems, If you restrict additional items on specific portals
            Added TeleportAnything for individual Portals- White Portals allows anything thorugh
            Defaults changed on PortalJuiceValue
            Bug fixes for TargetPortal mod
            Fixed WackysDatabase comptability with Portals- Keep EnablePortalJuice = false and add PortalJuice with Wackysdb instead if you want unique reqs
        Version 2.0.0
            Rewrite of mod: Added Crystals and Keys. YML configuration on entering portals. 4 Different types of crystals and keys. 
            Crystals are Consumable. Keys are not. TargetPortal, AnyPortal, TeleportAnything compatibility. YML file for each world. ServerSynced admin control
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


## Credits:
* Thank you to OdinPlus Team for some useful information.
* Zeall for readme update
* Some code from https://github.com/redseiko/ComfyMods/tree/main/ColorfulPortals
* Assets from https://assetstore.unity.com/packages/3d/props/interior/free-alchemy-and-magic-pack-142991
* crystal assets from https://assetstore.unity.com/packages/3d/environments/fantasy/translucent-crystals-106274
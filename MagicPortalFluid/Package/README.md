# RareMagicPortal

<img src="https://wackymole.com/hosts/FireandIce.png" width="500"/>  <img src="https://wackymole.com/hosts/GoldPortal.png" width="500"/>

`Server and client side required for Server Sync enforcement.` V2.4.2

MinimumRequiredVersion = "2.4.2"

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
  * v2.0 adds 6 Crystals and 6 Keys, color-tiered:Red, Green, Blue, Purple, Tan and Gold/Master.
    * Crystals will be **consumed** upon entering the portal
    * Keys to be **possessed** before entering the portal
      * > *Simulate the economy progression from spending and upgrade to long-holding possession*
  * Allows you to make special portal for:
    * Special access portal by team members only
    * Special toll charges for custom locations/bosses portal
  * Helps your server build a more interactive economy in Valheim

* Restrict Additional Items
  * You can Restrict Wood or Stone or any other item
  * Editing IsTeleportable with wackysDatabase is better, but you can do it here

* Portal Colors
  * If you just want to see different Portal Colors, this mod is great for that.

#### How to Get Crystals/Keys/Fluid in your World
The Crystals/Keys/Fluid are mostly meant for Multiplayer Worlds<br>
The Admins can add them to the world in several different ways. <br>
Drop That mod : https://valheim.thunderstore.io/package/ASharpPen/Drop_That/ <br>
Sell That mod : https://valheim.thunderstore.io/package/ASharpPen/Sell_That/ <br>
Better Trader: https://valheim.thunderstore.io/package/OdinPlus/Better_Trader_Remake/ <br>
CLLC: https://valheim.thunderstore.io/package/Smoothbrain/CreatureLevelAndLootControl/ <br>
KG's Marketplace https://valheim.thunderstore.io/package/KGvalheim/Marketplace_And_Server_NPCs_Revamped/ <br>
KnarrTheTrader https://valheim.thunderstore.io/package/OdinPlus/KnarrTheTrader/  <br>
Make it a Bounty Reward in EpicLoot <br>
Server rewards for completing objectives, like in KG marketplace mod for bounties,  <br>
https://valheim.thunderstore.io/package/NewHaven/Server_Rewards/ Server rewards, but I don't recommend

### Portal Colors
* Portal Colors - You can manually edit or cyle through colors with "LeftControl-E" as Admin or Owner with EnableCrystal off
 * > Black Portal -With EnableCrystals- Admin Only Portal
 * > Yellow Portal -With EnableCrystals- Normal Portal, no crystal or key req
 * > Red Portal  -With EnableCrystals- Red Crystal, Red Key, Gold Crystal, Gold Key
 * > Green Portal  -With EnableCrystals- Green Crystal, Green Key, Gold Crystal, Gold Key
 * > Blue Portal  -With EnableCrystals- Blue Crystal, Blue Key, Gold Crystal, Gold Key
 * > Purple Portal  -With EnableCrystals- Purple Crystal, Purple Key, Gold Crystal, Gold Key
 * > Tan Portal  -With EnableCrystals- Tan Crystal, Tan Key, Gold Crystal, Gold Key
 * > Gold Portal  -With EnableCrystals-  Gold Crystal, Gold Key
 * > White Portal -With EnableCrystals-  Teleport Anything, Traverese with Metals or Ore

<img src="https://wackymole.com/hosts/White2.png" width="700"/>  <img src="https://wackymole.com/hosts/OdinsBlessing.png" width="200"/>

* Portal Drink
 * Allows you do drink a potion and be able to Teleport Anything for a configurable amount of time. (Turns Any Portal White with base color behind)

YML files are synced on creation to the rest of the clients. Server always override client files on sync, except on finding a new portal or changing Portal color. 

<img src="https://wackymole.com/hosts/typesofcrystals.png" width="300"/> <img src="https://wackymole.com/hosts/nored.png" width="300"/> <img src="https://wackymole.com/hosts/goldPortal.png" width="300"/>

## Prefab IDs:
  * PortalMagicFluid
  * PortalCrystalMaster
  * PortalCrystalRed
  * PortalCrystalGreen
  * PortalCrystalBlue
  * PortalCrystalPurple
  * PortalCrystalTan
  * PortalKeyGold
  * PortalKeyRed
  * PortalKeyGreen
  * PortalKeyBlue
  * PortalKeyPurple
  * PortalKeyTan
  * PortalDrink

## Configuration Options:
### RareMagicPortal.cfg
  * [General]
    * Force Server Config: 
      * > Enable/Disable ServerSync enforcement
    * YML Logs
      * > Useful for seeing what a Portal Requirements are: Default: True
    * CraftingStation_Requirement = $piece_workbench
      * > Default required workbench
    * Level_of_CraftingStation_Req: 
      * > Default required lvl 1 of workbench
    * OnlyCreatorCanDeconstruct: 
      * > Allow/Disallow ONLY creator of the portal can demolish/destroy the portal
    * Portal_Health: 
      * > Configure the HP of portal = 400
    * OnlyCreatorCanChange: 
      * > Allow/Disallow ONLY creator of the portal can rename the portal tag
    * Portal_D_Restrict:
      * > Additional Items to Restrict by Default. 
      * > For Example you can add - Wood,Stone - And those items will restricted
    * Modifier key for toggle = LeftControl 
      * > Default Shortcut for admin and !EnabledCrystal && owner color cycling
      * > If Crystals and Keys is disabled than the owner can change coloring. 
      * > Shortcut is LeftControl + E on hovering
    * Force Portal Animation
      * > False
      * > Forces Portal Animation for Target Portal Mod, is not synced and only config only applies if mod is loaded

  * [Portal Crystals]
    * Enable Portal Crystals and Keys = false
      * > Enable/Disable Crystals and Keys to be loaded and used in game
    * Crystal_Consume_Default: 
      * > Default required 1 crystal for each portal
    * Portal_Crystal_Color_Default: 
      * > Default required RED crystal for TP consumption
      * > Options include Red, Green, Blue, Purple, Tan or None - None makes portals free passage by default
    * USE_GOLD_AS_PORTAL_MASTER = true
      * > Will Set Gold to always be true on regular colors - Making Gold Key the Master Key
      * > If you turn this off, you have to cycle through ALL Portals to get rid of Gold setting
    * UseTopLeftMessage
       *>  false, In case a mod is interfering with Center Messages for Portal tags, display on TopLeft instead.

  * [PortalFluid]
    * EnablePortalFluid: 
      * > Enable/Disable Portal Fluid to be loaded and used in game: 2.1.1 Default false
    * PortalMagicFluidSpawn: 
      * > Default spawning 3 fluids upon *NEW CHARACTER* created into the world/server
    * PortalFluidValue: 
      * > Default 0 coins 
        >> Set to value of 1 or more to sale at Haldor

  * [Portal Drink]
    * Portal_drink_timer = 120 : Seconds that PortalDrink lasts


### YML (config/Portal_Names/*.yml)
  * > *The mod will auto generate default data into each yml named after your current world **upon getting close to ANY portal.***
```
  Demo_Portal_Name:
    Portal_Crystal_Cost:
      Red: 1
      Green: 0
      Blue: 0
      Purple: 0
      Tan: 0
      Gold: 1
    Portal_Key:
      Red: true
      Green: false
      Blue: false
      Purple: false
      Tan: false
      Gold: true
    Free_Passage: false  - No Crystal or Key requirement
    TeleportAnything: false  - Portal allows you to Teleport Anything
    AdditionalProhibitItems: -- Additional items restricted at this portal or [Stone, Wood]
    - Stone
    - Wood
    Admin_only_Access: false -- Only admins

```

## Compatibility:
* WayShrine by Azumatt
  * https://www.nexusmods.com/valheim/mods/1298
  * https://valheim.thunderstore.io/package/Azumatt/Wayshrine/
* TargetPortal by Smoothbrain
<img src="https://wackymole.com/hosts/TargetPortalRMP2.png" width="600"/> 

  * https://valheim.thunderstore.io/package/Smoothbrain/TargetPortal/
  * This a recommended mod, it has good compatibility with RMP.  Shows Icon Color in 2.1.2
* and most of the other mods.

## Author's Note:
* Mod was produced with the hope that multiplayer servers will require more teamwork or more PVP to capture the scarce resource.
* Other mods can allow the resources to be bought at the trader for high prices, gambled on or become rare drops from bosses.
* If you are using *ServerCharacters*,
  * It is suggested to set the starting quantity amount to 0 for dedicated server and let ServerCharacters mod handle the first time spawn in amounts.
  * > https://valheim.thunderstore.io/package/Smoothbrain/ServerCharacters/

## Change Log:
        Version 2.4.2
            Added ConfigOption to force Force Portal Animation if TargetPortal is installed. - Not synced
            Fixed an Error if entered wrong password on joining server.  - Made Server more efficient
            Got rid of Empty tag, now just '' / Made it so '' should auto update to default color on server
            Changed the way things are loaded. 
        Version 2.4.1
            Fixed TargetPortal Default Icons loading when they arn't suppose to.
            Added ConfigOption for UseTopLeftMessage - Mostly for OdinsQOL or other announcing center mods
        Version 2.4.0
            Added the color Tan to the mix, after Purple
            Will work with old PortalName Files, but recommend to del - Portals go to Defaults
            Fix Portal Animation to OFF with TargetPortal(except during PortalDrink usage)
            Added a check for empty portal name, so that it remains default portal type ("" and Empty Tag)
            (Please delete or manually change "" and Empty Tag in yml)
            TanKey and CrystalTan, Fixed Purple Logic.
            Needs Updated TargetPortal update for icons. Now only TargetPortalIcons should be displayed
            Fix error for RMP on logout    
        Version 2.3.0
            Added the color Purple to the Mix, After Blue
            Fix for Fluid not changing value setting
            Fix for color Syncing(mostly)
            Changed config .cfg names- SERVER ADMINS - CHECK CONFIGS - some will go to defaults
            Will work with old PortalName Files, but recommend to del
            Fix for admin Portal
            Added config and fix For Shortcut for admin cycling
        Version 2.2.4
            Fix for Server YML continous writing loop. Change Crystal Stack size to 25.
            Rewrote how Ctrl-E is displayed.  Added some more log info. 
        Version 2.2.2
            Rewrote the way YML client updates are performed. Server now is the sole distributor for YAML changes.
            This should allow nonadmins to update portal colors and set names that everyone can see. 
            Version 2.2.1 Has a continous writing loop problem for servers, not sure if 2.2.2 fixed it. 
        Version 2.2.1
            Bug fixes for Server not saving admin changes.
            Known bug for nonAdmin Portal Creator trying to change Color, but not syncing with Portal_Crystal_Enable = false
        Version 2.2.0
            Changed Icons, Added a default Prohibited Items List, Add "None" to default color option for free passage ( yellow default).
            Now Syncs with portal color by just hoverring over name. 
        Version 2.1.3
            Bug fix for Admin status: I swear there are little gremlins messing up Serversync and admin permissions.
        Version 2.1.2
            Extra Compatibility with TargetPortal
            Now shows TargetPortal Icon colors based on color of Portal
            bug fixes
        Version 2.1.1
            Changed Default for EnablePortalJuice to false for everyone that just wants PortalColors.
            Changed ConfigEnableYMLLogs to allow for indivudal log settings. 
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
* GraveBear for Icon Update
* Some code from https://github.com/redseiko/ComfyMods/tree/main/ColorfulPortals
* Assets from https://assetstore.unity.com/packages/3d/props/interior/free-alchemy-and-magic-pack-142991
* crystal assets from https://assetstore.unity.com/packages/3d/environments/fantasy/translucent-crystals-106274
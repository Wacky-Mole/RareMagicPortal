# RareMagicPortal

Support me!

<a href="https://www.buymeacoffee.com/WackyMole" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" height='36' style="height: 36px;" ></a>  [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/H2H6LL5GA)

<img src="https://wackymole.com/hosts/FireandIce.png" width="500"/>  <img src="https://wackymole.com/hosts/GoldPortal.png" width="500"/>

`Server and client side required for Server Sync enforcement.` V2.6.8

MinimumRequiredVersion = "2.6.8"

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

 * Portal Drink
  * Allows you to teleport anywhere with anything on you.
  * Bypasses normal inventory checks - should be rare

* Portal Colors
  * If you just want to see different Portal Colors, this mod is great for that.

#### How to Get Crystals/Keys/Fluid in your World
Added Drop Config for Fluid,Drink and Crystals - 
By Default Fluid drops from Elder - 100% chance for 1-2 drops. 
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

    With TargetPortalMod users will see"^" with a number this is the internal value - this is normal, If you remove TargetPortal mod you might have to rename portals to get them to connect

    All Portal Colors can be focred to Use Gold Crystal and Gold Key with USE_GOLD_AS_PORTAL_MASTER

 
 * > Yellow Portal -With EnableCrystals- Yellow Portal, Yellow Key  - Value 1
 * > Red Portal  -With EnableCrystals- Red Crystal, Red Key,  - Value 2
 * > Green Portal  -With EnableCrystals- Green Crystal, Green Key  - Value 3
 * > Blue Portal  -With EnableCrystals- Blue Crystal, Blue Key - Value 4
 * > Purple Portal  -With EnableCrystals- Purple Crystal, Purple Key - Value 5
 * > Tan Portal  -With EnableCrystals- Tan Crystal, Tan Key, - Value 6
 * > Cyan Portal  -With EnableCrystals- Cyan Crystal, Cyan Key, - Value 7
 * > Orange Portal  -With EnableCrystals- Orange Crystal, Orange Key, - Value 8
 * > White Portal -With EnableCrystals-  White Crystal, White Key - Value 20
 * > Black Portal -With EnableCrystals- Black Crystal, Black Key  - Value 21
 * > Gold Portal  -With EnableCrystals-  PortalCrystalMaster (Gold) Crystal, Gold Key - Value 22
 

<img src="https://wackymole.com/hosts/White2.png" width="700"/>  <img src="https://wackymole.com/hosts/OdinsBlessing.png" width="200"/>



* Portal Drink

https://user-images.githubusercontent.com/25761125/212202220-c49d72c3-f2b8-4ee8-8e3c-81a1aa0dd6df.mp4


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
  * PortalCrystalYellow
  * PortalCrystalCyan
  * PortalCrystalOrange
  * PortalCrystalWhite
  * PortalCrystalBlack

  * PortalKeyGold
  * PortalKeyRed
  * PortalKeyGreen
  * PortalKeyBlue
  * PortalKeyPurple
  * PortalKeyTan
  * PortalKeyYellow
  * PortalKeyCyan
  * PortalKeyOrange
  * PortalKeyWhite
  * PortalKeyBlack

  * PortalDrink

## Configuration Options:
### RareMagicPortal.cfg
  * [1.General]
    * Force Server Config: 
      * > Enable/Disable ServerSync enforcement
    * YML Logs
      * > Useful for seeing what a Portal Requirements are: Default: True

  * [2.PortalFluid]
    * EnablePortalFluid: 
      * > Enable/Disable Portal Fluid to be loaded and used in game: 2.1.1 Default false
    * PortalMagicFluidSpawn: 
      * > Default spawning 3 fluids upon *NEW CHARACTER* created into the world/server
    * PortalFluidValue: Value at trader - 0 means won't sale
    * portalmagicfluid.DropsFrom.Add("gd_king", 1f, 1, 2); // Elder drop 100% 1-2 portalFluids
      * [Drops for Fluid,Drink and Crystals can be configured in Bepinex Manager, but recommend manually editing]
    * "gd_king", 1f, 1, 2); // Elder drop 100% 1-2 portalFluids for example

  * [3.Portal Config"]
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
      * > Will only work for New Portals. You need to edit existing Portals in .YML files
      * > Make sure '' has your restrictions in YML file
    * Modifier key for toggle = LeftControl 
      * > Default Shortcut for admin and !EnabledCrystal && owner color cycling
      * > If Crystals and Keys is disabled than the owner can change coloring. 
      * > Shortcut is LeftControl + E on hovering
    * Force Portal Animation
      * > False
      * > Forces Portal Animation for Target Portal Mod, is not synced and only config only applies if mod is loaded
    * ConfigMaxWeight
       * > Makes it so any player weight above this amount can not teleport. 
       * > Default is 0. Which disables checks
       * > Will apply checks to all portals

  * [4.Portal Crystals]
    * Careful of serverdevcommands mod - will not consume crystals with even devcommands enabled
    * Enable Portal Crystals and Keys = false
      * > Enable/Disable Crystals and Keys to be loaded and used in game - This controls all usage and restrictions
      * > If this is not enabled then, Free Passage Color, Admin Color and Teleport anything color do NOT WORK,  PortalDrink will still work.
    * Crystal_Consume_Default: 
      * > Default required 1 crystal for each portal
    * Portal_Crystal_Color_Default: 
      * > Default required RED crystal for TP consumption
      * > Options include Yellow,Red,Green,Blue,Purple,Tan,Cyan,Orange,White,Black,Gold,none,None - None makes portals free passage by default
    * USE_GOLD_AS_PORTAL_MASTER = true
      * > Will Set Gold to always be true on regular colors - Making Gold Key the Master Key
      * > If you turn this off, you have to cycle through ALL Portals to get rid of Gold setting
    * UseTopLeftMessage
       * >  false, In case a mod is interfering with Center Messages for Portal tags, display on TopLeft instead.

  * [5.Portal Drink]
    * Portal_drink_timer = 120 : Seconds that PortalDrink lasts

  * [6.Biome Colors]
    * Force Biome Colors for Default
      * > Overrides CrystalKeyDefaultColor and sets UNCHANGED portals to their BiomeColor
    * BiomeRepColors
      * > Biome Colors "Meadows:Tan,BlackForest:Blue,Swamp:Green,Mountain:Black,Plains:Orange,Mistlands:Purple,DeepNorth:Cyan,AshLands:Red,Ocean:Blue", "Biomes and their related Colors. - No spaces
    
  * [7.Colors]
     * Enabled Colors
        * > Any color not in list will not cycle through, reboot required
     * Free Passage Color
        * > Won't collect toll crystal or use key - Only 1 can be selected
        * > Recommend none
     * Admin only Color
        * > Only Admins can pass through this portal, - inventory checks still applied
        * > Recommended none
        * > Does nohting when Enable Portal Crystals and Keys = false
     * TelePortAnythingColor
        * > This color portal will allow anyone to come through, free of cost and free of inventory checks
        * > It might be bugged
        * > recommend none
        * > Does nothing when Enable Portal Crystals and Keys = false
     * Portal Drink Color
        * > Color to let the player know that PortalDrink is active on them. - Rainbow mode alternates every second 
        * > Rainbow or White recommended
        * > Doesn't change underlying color requirements
  * [8.CrystalSelector]
    * > Set Item usage for different colors - Defaults are entered but you can use JewelCrafting gems
    * > CrystalMaster is Gold, -fyi
    * > JC has around 7 type of base colors, currently RMP has 11. - JC has 5 base type of gems. $jc_shattered_color_crystal, $jc_uncut_color_stone, $jc_color_socket, $jc_adv_color_socket, $jc_perfect_color_socket
    * > You could use combination crystal names if you want to be very very restrictive. name has to be the ItemDrop.shared.m_name




     

### YML (config/Portal_Names/*.yml)
  * > *The mod will auto generate default data into each yml named after your current world **upon getting close to ANY portal.***
  * > Free_Passage, TeleportAnything, AdditionalProhibitItems, Admin_only_Access may have to be manually edited if you changed settings after setting up portals!
```
  Demo_Portal_Name:
    Portal_Crystal_Cost:
      Red: 0
      Green: 0
      Blue: 0
      Purple: 0
      Tan: 0
      Gold: 1
      Yellow: 1
      Cyan: 0
      Orange: 0
      White: 0
      Black: 0
    Portal_Key:
      Red: false
      Green: false
      Blue: false
      Purple: false
      Tan: false
      Gold: true
      Yellow: true
      Cyan: false
      Orange: false
      White: false
      Black: false
    Free_Passage: false  - No Crystal or Key requirement
    TeleportAnything: false  - Portal allows you to Teleport Anything
    AdditionalProhibitItems: -- Additional items restricted at this portal or [Stone, Wood]
    - Stone
    - Wood
    BiomeColor: skip - this doesn't do much
    SpecialMode: 0  - nothing
    AllowedUsers: [] - Only the players in this list will be allowed to go here - manual add only, [WackaMole, Player2] - Empty allows all
    Admin_only_Access: false -- Only admins

```

## Compatibility:
* WayShrine by Azumatt
  * https://www.nexusmods.com/valheim/mods/1298
  * https://valheim.thunderstore.io/package/Azumatt/Wayshrine/
* TargetPortal by Smoothbrain
<img src="https://wackymole.com/hosts/TargetPortalRMP2.png" width="600"/> 

  * https://valheim.thunderstore.io/package/Smoothbrain/TargetPortal/
  * This a recommended mod, it has good compatibility with RMP. 

  * BiomeColorForce is a tricky for TargetPortal - so Icons will have ^(num)(1-22) next to the name on map. This is to let the mod know what color the icon is for later use
  * Warning - Removing TargetPortal mod with ForcedBiomes, might make Portals unable to connect with each other. You might have to deconstruct and rebuild them or change names. - Debug Logs should show Portal Real name

  *  Most of the other mods.

## Author's Note:
* Mod was produced with the hope that multiplayer servers will require more teamwork or more PVP to capture the scarce resource.
* Other mods can allow the resources to be bought at the trader for high prices, gambled on or become rare drops from bosses.
* If you are using *ServerCharacters*,
  * It is suggested to set the starting quantity amount to 0 for dedicated server and let ServerCharacters mod handle the first time spawn in amounts.
  * > https://valheim.thunderstore.io/package/Smoothbrain/ServerCharacters/

<details><summary>Feedback</summary>

Wacky Git https://github.com/Wacky-Mole/RareMagicPortal

For questions or suggestions please join discord channel: [Odin Plus Team](https://discord.gg/odinplus) or my discord at [Wolf Den](https://discord.gg/yPj7xjs3Xf)

Support me at https://www.buymeacoffee.com/WackyMole  or https://ko-fi.com/wackymole

<a href="https://www.buymeacoffee.com/WackyMole" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>

<a href='https://ko-fi.com/H2H6LL5GA' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi3.png?v=3' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

<img src="https://wackymole.com/hosts/bmc_qr.png" width="100"/>

</details> 

## Change Log:
        Version 2.6.8
            Thunderstore really needs to let modders edit the readme without another upload.
        Version 2.6.7
            Bug Fixes, Fixed issue when not allowing all Colors on multiplayer. 
        Version 2.6.6
            Updated for 216.9, Updated ItemManager
        Version 2.6.5
            Update ItemManager, Added Portal Drink Wont Allow, for admins to restrict drink for some items. 
        Version 2.6.4
             Added Use Small Server Updates to replace the bulk YML file change sync. </br> Risky Server Save - ONLY SAVES YML on SERVER Exit (not recommended, except for large servers)
        Version 2.6.3
            Fix for 214.2, Upated ItemManager and Localiz Mananger. 
        Version 2.6.2
            Bug Fixes - A lot
        Version 2.6.1
            Fixed some log spam, set some defaults to mirror more closely vanilla.
            If you see ▲ 6 or numbers in a portalname that is just displaying for Forced Biome Color: lets other mods know the color
        Version 2.6.0
            Big update: Added BiomeColorForce, EnabledColors, FreePassageColor, adminColor, TelePortAnythingColor, PortalDrinkColor
            Was a pain to make compatitle with TargetPortal, you will notice ^(num) on Portals with ConfigUseBiomeColors, just passes info
            Individual configs for each Crystal Color used, can now set JC crystals to be consumed - which is pretty cool.
            Fixed - Only Owner can deconstruct, added AllowedUsers - If any added, only players in this list can go through
            More bugs, but hopefully you can live with those.
        Version 2.5.3
            I didn't do spanish correctly, fixed some spanish yml error.
        Version 2.5.2
            Fixed Color visual, added Spanish Translations <br/>
            Made it so if you have PortalFluid disabled then it doesn't spawn PortalFluid not matter the number. 
        Version 2.5.1
            Special ItemManager update for RMP
        Version 2.5.0
            Updated for Mistlands, ItemManager, Added ConfigMaxWeight for all portals (maybe individual in future)
            Added Drop configs for Fluid,Crystals and Drink. Fixed Crystal Size, adjusted CrystalTan
            Messed up All your configs, no need to thank me with a donation. 
        Version 2.4.8
            Removed SteamAPI for actual crossplay. Made it so Server doesn't get updated for no named Portal
            Fixed Icons
        Version 2.4.7
            Updated ServerSync For 211.11 and hopefully fixed another Portal Admin issue. 
        Version 2.4.6
            Updated ServerSync for crossplay
        Version 2.4.5
            Skip
        Version 2.4.4
            Bug Fix for Extra Inventory Mods
        Version 2.4.3
            Made Coop Compatible(no dedicated server)
            Bug fix for "" sending crazy amount of data and disconnecting
            Bug fixes
        Version 2.4.2
            Added ConfigOption to force Force Portal Animation if TargetPortal is installed. - Not synced
            Fixed an Error if entered wrong password on joining server.  - Made Server more efficent
            Got rid of Empty tag, now just '' / Made it so '' should auto update to default color on server
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
            Changed Default for EnablePortalFluid to false for everyone that just wants PortalColors.
            Changed ConfigEnableYMLLogs to allow for indivudal log settings. 
            Fixed Spelling errors. 
        Version 2.1.0
            Added PortalDrink - Configurable Time that allows a player to Teleport anything.
            Added PortalColor Changing feature, can be used exclusively or with Crystals and Keys
            Added AdditionalProhibitItems, If you restrict additional items on specific portals
            Added TeleportAnything for individual Portals- White Portals allows anything thorugh
            Defaults changed on PortalJuiceValue
            Bug fixes for TargetPortal mod
            Fixed WackysDatabase comptability with Portals- Keep EnablePortalFluid = false and add PortalFluid with Wackysdb instead if you want unique reqs
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

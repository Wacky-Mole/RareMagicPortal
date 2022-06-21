// MagicPortalFluid
// a Valheim mod created by WackyMole. Do whatever with it. - WM
// assets from https://assetstore.unity.com/packages/3d/props/interior/free-alchemy-and-magic-pack-142991
// 
// File:    MagicPortalFluid.cs
// Project: MagicPortalFluid
// create yml file for portal names, put example file in it and explain that it only does something with EnableCrystals and Consumable is true
// load portal names After every change

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HarmonyLib;
using RareMagicPortal;
//using PieceManager;
using ServerSync;
using ItemManager;
using BepInEx.Logging;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


namespace RareMagicPortal
{
	//extra
	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	//[BepInDependency(Jotunn.Main.ModGuid)]
	//[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
	internal class MagicPortalFluid : BaseUnityPlugin
	{
		public const string PluginGUID = "WackyMole.RareMagicPortal";
		public const string PluginName = "RareMagicPortal";
		public const string PluginVersion = "2.0.0";

		internal const string ModName = PluginName;
		internal const string ModVersion = PluginVersion;
		internal const string Author = "WackyMole";
		private const string ModGUID = Author + "." + ModName;
		private static string ConfigFileName = PluginGUID + ".cfg";
		private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "WackyMole.RareMagicPortal.cfg";
		private static string YMLFULL = YMLFULLFOLDER + "World1.yml";
		//private static string YMLFULLServer = Paths.ConfigPath + Path.DirectorySeparatorChar + "WackyMole" + ".PortalServerNames.yml";
		private static string YMLFULLFOLDER = Path.Combine(Path.GetDirectoryName(Paths.ConfigPath + Path.DirectorySeparatorChar), "Portal_Names");

		internal static string ConnectionError = "";

		private readonly Harmony _harmony = new(ModGUID);

		public static readonly ManualLogSource RareMagicPortal =
			BepInEx.Logging.Logger.CreateLogSource(ModName);

		private static readonly ConfigSync ConfigSync = new(ModGUID)
		{ DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = "2.0.0" };



		// Use this class to add your own localization to the game
		// https://valheim-modding.github.io/Jotunn/tutorials/localization.html
		//public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

		private AssetBundle portalmagicfluid;
		private static MagicPortalFluid context;
		public static ConfigEntry<bool> modEnabled;
		public static ConfigEntry<bool> isDebug;
		public static bool firstTime = false;
		public static ConfigEntry<int> nexusID;
		private static List<RecipeData> recipeDatas = new List<RecipeData>();
		private static string assetPath;
		private static string assetPathyml;
		public static string PiecetoLookFor = "portal_wood"; //name
		public static string PieceTokenLookFor = "$piece_portal"; //m_name
		public static Vector3 tempvalue;
		public static bool loadfilesonce = false;
		public static Dictionary<string, int> Ind_Portal_Consumption;
		public static int CurrentCrystalCount;
		public static bool isAdmin = true;
		public static bool isLocal = true;
		public static string Worldname = "demo_world";
		public static bool LoggingOntoServerFirst = true;

		public static int PortalMagicFluidSpawn = 3; // default
		public static bool DisablePortalJuice = false; // don't disable
		public static string TabletoAddTo;
		public static string DefaultTable = "$piece_workbench";
		public static bool piecehaslvl = false;
		public static bool CreatorOnly = true;
		public static bool CreatorLock = false;
		public static float PortalHealth = 400f;
		public static int CraftingStationlvl = 1;
		public static int MagicPortalFluidValue = 300;
		public static bool EnableCrystals = false;
		public static bool EnableKeys = false;
		public static int CrystalsConsumable = 1;
		public static bool AdminOnlyBuild = false;
		public static string DefaultPortalColor = "blue";

		private static string YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");
		private static bool JustWrote = false;


		private static ConfigEntry<bool>? ConfigFluid;
		private static ConfigEntry<int>? ConfigSpawn;
		private static ConfigEntry<string>? ConfigTable;
		private static ConfigEntry<int>? ConfigTableLvl;
		private static ConfigEntry<bool>? ConfigCreator;
		private static ConfigEntry<float>? ConfiglHealth;
		private static ConfigEntry<bool>? ConfigCreatorLock;
		private static ConfigEntry<int>? ConfigFluidValue;
		private static ConfigEntry<bool>? ConfigEnableCrystals;
		private static ConfigEntry<bool>? ConfigEnableKeys;
		private static ConfigEntry<int>? ConfigCrystalsConsumable;
		private static ConfigEntry<bool>? ConfigAdminOnly;
		private static ConfigEntry<string>? CrystalKeyDefaultColor;

		public static string crystalcolorre = ""; // need to reset everytime maybe?
		public string message_eng_NO_Portal = $"Portal Crystals/Key Required"; // Blue Portal Crystal
		public string message_eng_MasterCost = $", Rainbow Crystals Required"; // 3, Master Crystals Required
		public string message_eng_NotCreator = $""; 
		public string message_eng_Grants_Acess = $"";
		public string message_eng_Crystal_Consumed = $"";
		public string message_eng_Odins_Kin = $"Only Odin's Kin are Allowed";
		public string message_only_Owner_Can_Change = $"Only the Owner Can change Name";

		public const string CrystalMaster = "$item_PortalCrystalMaster";  // RGB
		public const string CrystalRed = "$item_PortalCrystalRed";
		public const string CrystalGreen = "$item_PortalCrystalGreen";
		public const string CrystalBlue = "$item_PortalCrystalBlue";

		public const string PortalKeyGold = "$item_PortalKeyGold";
		public const string PortalKeyRed = "$item_PortalKeyRed";
		public const string PortalKeyGreen = "$item_PortalKeyGreen";
		public const string PortalKeyBlue = "$item_PortalKeyBlue";



		public static string WelcomeString = "#Hello, this is the Portal yml file. It keeps track of all portals you enter";
		private static PortalName PortalN;

		public static ItemDrop.ItemData Crystal { get; private set; }



		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		[HarmonyPriority(0)]
		private static class ZNetScene_Awake_PatchWRare
		{
			private static void Postfix()
			{
				{
					Worldname = ZNet.instance.GetWorldName();// for singleplayer  // won't be ready for multiplayer

					RareMagicPortal.LogInfo("Setting MagicPortal Fluid Afterdelay");
					((MonoBehaviour)(object)context).StartCoroutine(DelayedLoad()); // important
																					//LoadAllRecipeData(reload: true); // while loading on world screen
				}
			}
		}

		[HarmonyPatch(typeof(Game), "SpawnPlayer")]
		private static class Game_OnNewCharacterDone_Patch
		{
			[HarmonyPostfix]
			private static void Postfix()
			{
				{
					StartingitemPrefab();
				}
			}
		}

		[HarmonyPatch(typeof(FejdStartup), "OnNewCharacterDone")]
		private static class FejdStartup_OnNewCharacterDone_Patch
		{
			private static void Postfix()
			{
				StartingFirsttime();

			}

		}

		[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Interact))]
		[HarmonyPrefix]
		private static bool PortalCheckOutside(TeleportWorld __instance, Humanoid human, bool hold)
		{
			if (hold)
				return false;
			if (__instance.m_nview.IsValid())
			{
				Player closestPlayer = Player.GetClosestPlayer(__instance.m_proximityRoot.position, 5f);
				Piece portal = null;
				var emptyPiece = new List<Piece>();
				Piece.GetAllPiecesInRadius(__instance.transform.position, 5f, emptyPiece);
				//long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
				foreach (var piece in emptyPiece)
				{
					if (piece.m_name == "portal_wood")
					{
						portal = piece;
					}
				}
				if (portal != null)
                {
					bool sameperson = false;
					if (portal.m_creator == closestPlayer.GetPlayerID())
						sameperson = true;

					if (sameperson || !sameperson && !CreatorLock  || closestPlayer.m_noPlacementCost)
                    {
						return true;
                    } // Only creator || not creator and not in lock mode || not in noplacementcost mode
					human.Message(MessageHud.MessageType.Center, "Only the Owner Can change Name");
					return false; // noncreator doesn't have permiss
				}		
				return true;				
            }
			return true;
		}

		[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.UpdatePortal))] //maybe, gets called a lot
		static class TeleportWorld_Teleport_CheckUPdate
		{
			[HarmonyPostfix]
			private static void Postfix(TeleportWorld __instance)
			{
				if (__instance.m_nview.IsValid() && __instance.m_hadTarget)
                {


				}

			}
		}

		[HarmonyPatch(typeof(TeleportWorldTrigger), "OnTriggerEnter")]
		[HarmonyPriority(1)]
		private class OpenBeforeTargetPortal // need a blax solution to active below patch before hers. 
		{
			private static bool Prefix(TeleportWorldTrigger __instance, Collider collider)
			{
				return true;
			}

		}

			[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]  // for Crystals
		static class TeleportWorld_Teleport_CheckforCrystal
		{
			[HarmonyPrefix]
			private static bool Prefix(TeleportWorld __instance, ref Player player)
			{
				//finding portal name
				string PortalName = __instance.GetHoverText();
				var found = PortalName.IndexOf(":") + 2;
				var end = PortalName.IndexOf("\" ");
				var le = end - found;
				PortalName = PortalName.Substring(found, le);
				// end finding portal name

				int CrystalForPortal = CrystalsConsumable;
				bool OdinsKin = false;



				RareMagicPortal.LogInfo($"Portal name  is {PortalName}");
				if (PortalN.Portals.ContainsKey(PortalName))
                {
					CrystalForPortal = PortalN.Portals[PortalName].Crystal_Cost_Master;
					OdinsKin = PortalN.Portals[PortalName].Admin_only_Access;

				} else
                {
					WritetoYML(PortalName, CrystalsConsumable);
				}
				if (OdinsKin && !isAdmin) // If requires admin, but not admin
                {
					player.Message(MessageHud.MessageType.Center, "Only Odin's Kin are Allowed");
					return false;
				}
				if (EnableCrystals || EnableKeys)
				{
					
					ItemDrop.ItemData Crystal = null;
					//ItemDrop.ItemData Crystal = ObjectDB.instance.GetItemPrefab("PortalCrystal").GetComponent<ItemDrop>().m_itemData; // IDK this wasn't working
					CurrentCrystalCount = player.m_inventory.CountItems(CrystalMaster);
					RareMagicPortal.LogInfo("Crystal Master Count before " + CurrentCrystalCount);

					/*
					List<ItemDrop.ItemData> GetItem = player.m_inventory.GetAllItems();
					CurrentCrystalCount = 0;
					foreach (ItemDrop.ItemData item in GetItem)
					{
						if (item.m_shared.m_name == CrystalMaster)
						{
							CurrentCrystalCount += item.m_stack;
							Crystal = item;
						}
					}
					*/

					if (CurrentCrystalCount > 0 && __instance.m_hadTarget)
					{
						if (CrystalsConsumable > 0) // check if consume mode is active
						{
							if (CurrentCrystalCount >= CrystalForPortal )
							{
								if (!player.IsTeleportable())
								{
									player.Message(MessageHud.MessageType.Center, "$msg_noteleport");
									return false;
								}

								player.m_inventory.RemoveItem(Crystal, CrystalForPortal);

								if (CrystalForPortal > 1)// formatting logic
									player.Message(MessageHud.MessageType.TopLeft, $"Consumed {CrystalForPortal} Portal Crystals");
								else player.Message(MessageHud.MessageType.TopLeft, $" One Portal Crystal Consumed");
								return true;

							} else
                            {
								player.Message(MessageHud.MessageType.Center, $"{CrystalForPortal} Crystals Require for Portal {PortalName}");
								return false;
							}
						}
						player.Message(MessageHud.MessageType.TopLeft, $"Portal Crystal Grants Access");
						return true;
					}
					else
					{
						player.Message(MessageHud.MessageType.Center, "No Portal Crystals");
						return false;
					}
				}
				else return true;
			}

		}
		[HarmonyPostfix]
		private static void Postfix(ref Player player)
        {

        }


		[HarmonyPatch(typeof(Player), "CheckCanRemovePiece")]
		private static class Player_CheckforOwnerP
		{
			[HarmonyPrefix]
			private static bool Prefix(ref Player __instance, ref Piece piece)
			{
				if (piece == null) 
					return true;

				if (piece.name == PiecetoLookFor && !__instance.m_noPlacementCost) // portal
                {
					bool bool2 = piece.IsCreator();// nice
					if (bool2 || !CreatorOnly)
                    { // can remove because is creator or creator only mode is foff
						return true;

                    }else
                    {
						__instance.Message(MessageHud.MessageType.Center, "You are not the portal Creator - Go axe a stump");
						return false;
					}
                }
				return true;
			}
		}

		[HarmonyPatch(typeof(Player), "PlacePiece")]
		private static class Player_MessageforPortal_Patch
        {
			[HarmonyPrefix]
			private static bool Prefix(ref Player __instance, ref Piece piece)

			{
				if (piece == null) return true;

				if (piece.name == PiecetoLookFor && !__instance.m_noPlacementCost) // portal
				{
					if (__instance.transform.position != null)
						tempvalue = __instance.transform.position; // save position //must be assigned
					else
						tempvalue = new Vector3(0, 0, 0); // shouldn't ever be called 

					var paulstation = CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, tempvalue);
					var paullvl = paulstation.GetLevel();

					if (paullvl + 1 > CraftingStationlvl) // just for testing
					{
						piecehaslvl = true;
					}
					else
					{
						string worktablename = piece.m_craftingStation.name;
						GameObject temp = GetPieces().Find(g => Utils.GetPrefabName(g) == worktablename);
						var name = temp.GetComponent<Piece>().m_name;
						__instance.Message(MessageHud.MessageType.Center, "Need a Level " + CraftingStationlvl + " " + name + " for placement");
						piecehaslvl = false;
						return false;
					}
				}
				return true;
			}
					
        }
		private void Awake()
		{
			CreateConfigValues();
			ReadAndWriteConfigValues();
			LoadAssets();

			context = this;

			assetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), typeof(MagicPortalFluid).Namespace);
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), (string)null);

			SetupWatcher();
			setupYMLFolderWatcher();
			
			YMLPortalData.ValueChanged += CustomSyncEventDetected;

			RareMagicPortal.LogInfo("MagicPortalFluid has loaded start assets");

		}


		private static void LoadIN()
        {
			LoggingOntoServerFirst = true;
			setupYMLFile();
			ReadYMLValuesBoring();//
			

		}


		// end startup

		private void LoadAssets()
		{
			Item portalmagicfluid = new("portalmagicfluid", "portalmagicfluid", "assetsEmbedded");
			portalmagicfluid.Name.English("Magical Portal Fluid");
			portalmagicfluid.Description.English("Once a mythical essence, now made real with Odin's blessing");
			
			Item PortalCrystalMaster = new("portalcrystal", "PortalCrystalMaster", "assetsEmbedded");
			PortalCrystalMaster.Name.English("Rainbow Portal Crystal");
			PortalCrystalMaster.Description.English("Odin's Traveling Crystals allow for any Portal Traveling");


			Item PortalCrystalRed = new("portalcrystal", "PortalCrystalRed", "assetsEmbedded");
			PortalCrystalRed.Name.English("Red Portal Crystal");
			PortalCrystalRed.Description.English("Odin's Traveling Crystals allow for Red Portal Traveling");


			Item PortalCrystalGreen = new("portalcrystal", "PortalCrystalGreen", "assetsEmbedded");
			PortalCrystalGreen.Name.English("Green Portal Crystal");
			PortalCrystalGreen.Description.English("Odin's Traveling Crystals allow for Green Portal Traveling");


			Item PortalCrystalBlue = new("portalcrystal", "PortalCrystalBlue", "assetsEmbedded");
			PortalCrystalBlue.Name.English("Blue Portal Crystal");
			PortalCrystalBlue.Description.English("Odin's Traveling Crystals allow for Blue Portal Traveling");


			Item PortalKeyRed = new("portalcrystal", "PortalKeyRed", "assetsEmbedded");
			PortalKeyRed.Name.English("Red Portal Key");
			PortalKeyRed.Description.English("Unlock Portals Requiring The Red Key");

			Item PortalKeyGold = new("portalcrystal", "PortalKeyGold", "assetsEmbedded");
			PortalKeyGold.Name.English("Gold Admin Portal Key");
			PortalKeyGold.Description.English("Unlock All Portals");

			Item PortalKeyBlue = new("portalcrystal", "PortalKeyBlue", "assetsEmbedded");
			PortalKeyBlue.Name.English("Blue Portal Key");
			PortalKeyBlue.Description.English("Unlock Portals Requiring The Blue Key");

			Item PortalKeyGreen = new("portalcrystal", "PortalKeyGreen", "assetsEmbedded");
			PortalKeyGreen.Name.English("Green Portal Key");
			PortalKeyGreen.Description.English("Unlock Portals Requiring The Green Key");



		}

		private void UnLoadAssets()
		{
			portalmagicfluid.Unload(false);
		}

		public void setupYMLFolderWatcher()
        {
			if (!Directory.Exists(YMLFULLFOLDER))
			{
				Directory.CreateDirectory(YMLFULLFOLDER);
				//File.WriteAllText(YMLFULL, WelcomeString + yaml); //overwrites
			}

			FileSystemWatcher watcherfolder = new(YMLFULLFOLDER);
			watcherfolder.Changed += ReadYMLValues;
			watcherfolder.Created += ReadYMLValues;
			watcherfolder.Renamed += ReadYMLValues;
			watcherfolder.SynchronizingObject = ThreadingHelper.SynchronizingObject;
			watcherfolder.IncludeSubdirectories = true;
			watcherfolder.EnableRaisingEvents = true;

		}

		private static void setupYMLFile()
        {
			YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");

			if (!File.Exists(YMLCurrentFile))
			{
				PortalN = new PortalName()  // kind of iffy in inside this
				{
				Portals = new Dictionary<string, PortalName.Portal>
					{
						{"Demo_Portal_Name", new PortalName.Portal() {
							Crystal_Cost_Master = 3,
						}},
					}
				};
				var serializer = new SerializerBuilder()
					.Build();
				var yaml = serializer.Serialize(PortalN);
				WelcomeString = WelcomeString + Environment.NewLine;

				File.WriteAllText(YMLCurrentFile, WelcomeString + yaml); //overwrites
				RareMagicPortal.LogInfo("Creating Portal_Name file " + Worldname);
				JustWrote = true;
			}
		}
		private void CustomSyncEventDetected()
        {
			Worldname = ZNet.instance.GetWorldName();
			YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");
			if (LoggingOntoServerFirst)
			{
				RareMagicPortal.LogInfo("You are now connected to Server World " + Worldname);
				LoggingOntoServerFirst = false;
			}

			isLocal = false;
			if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
			{
				//isDedServer = true;
			}
			// else
            {
				string SyncedString = YMLPortalData.Value;
				RareMagicPortal.LogInfo(SyncedString); // info SyncString might make debugmode for this
				var deserializer = new DeserializerBuilder()
					.Build();

				PortalN.Portals.Clear();
				PortalN = deserializer.Deserialize<PortalName>(SyncedString);
				JustWrote = true;
				//File.WriteAllText(YMLCurrentFile, WelcomeString + SyncedString); //overwrites
																		


			}

		}

		private void ReadYMLValues(object sender, FileSystemEventArgs e) // Thx Azumatt // This gets hit after writing
        {
			if (!File.Exists(YMLCurrentFile)) return;
			if (isAdmin && !JustWrote) // if local admin or ServerSync admin
			{
				var yml = File.ReadAllText(YMLCurrentFile);

				var deserializer = new DeserializerBuilder()
					.Build();

				PortalN.Portals.Clear();
				PortalN = deserializer.Deserialize<PortalName>(yml);
				YMLPortalData.Value = yml;
			}
			if (JustWrote)
					JustWrote = false;
			else
			{
				RareMagicPortal.LogInfo("Portal Cost Values Didn't change because you are not an admin");

			}

		}

		private void SetupWatcher() // Thx Azumatt
		{
			FileSystemWatcher watcher = new(BepInEx.Paths.ConfigPath, ConfigFileName);
			watcher.Changed += ReadConfigValues;
			watcher.Created += ReadConfigValues;
			watcher.Renamed += ReadConfigValues;
			watcher.IncludeSubdirectories = true;
			watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
			watcher.EnableRaisingEvents = true;
		}

		private static void ReadYMLValuesBoring() // Startup File 
		{
			if (!File.Exists(YMLCurrentFile)) return;
			var yml = File.ReadAllText(YMLCurrentFile);

			var deserializer = new DeserializerBuilder()
				.Build();
			//PortalN.Portals.Clear();
			PortalN = new PortalName(); // init
			PortalN = deserializer.Deserialize<PortalName>(yml);
			//YMLPortalData.Value = yml; // should only be one time and for server

		}

		private void ReadConfigValues(object sender, FileSystemEventArgs e) // Thx Azumatt
        {
            if (!File.Exists(ConfigFileFullPath)) return;


			//RareMagicPortal.LogInfo("ReadConfigValues called- checking admin status");
			//bool admin= !ConfigSync.IsLocked; // or locked?
			bool admin = ConfigSync.IsAdmin;
			bool locked = ConfigSync.IsLocked;
			//RareMagicPortal.LogInfo("admin " + admin);
			//RareMagicPortal.LogInfo("locked " + locked);
			if (admin) // Server Sync Admin Only
			{
				isAdmin = admin; // need to check this
				RareMagicPortal.LogInfo("ReadConfigValues loaded");
				try
				{
					if (ConfigSync.IsSourceOfTruth)
					{
						RareMagicPortal.LogInfo("ReadConfigValues loaded- you are an admin - on a server");
						Config.Reload();
						ReadAndWriteConfigValues();
						PortalChanger();
					}
					else
					{
						// false so remote config is being used
						RareMagicPortal.LogInfo("Server values loaded");
						ReadAndWriteConfigValues();
						PortalChanger();
					}


				}
				catch
				{
					RareMagicPortal.LogInfo($"There was an issue loading your {ConfigFileName}");
					RareMagicPortal.LogInfo("Please check your config entries for spelling and format!");
				}
			}
			else
			{
				RareMagicPortal.LogInfo("You are not ServerSync admin"); 
				try
				{
					

					if (ConfigSync.IsSourceOfTruth && !locked)
					{
						RareMagicPortal.LogInfo("ReadConfigValues loaded- You are an local admin"); // Server hits this on dedicated
						ReadAndWriteConfigValues(); 
						PortalChanger();
						isAdmin = true; // local admin as well
					}
					else
					{

						RareMagicPortal.LogInfo("You are not an Server admin - Server Sync Values loaded "); // regular non admin clients
						ReadAndWriteConfigValues(); 
						PortalChanger();
						
					}


				}
				catch
				{
					RareMagicPortal.LogInfo($"There was an issue loading your {ConfigFileName}");
					RareMagicPortal.LogInfo("Please check your config entries for spelling and format!");
				}


			}
		}
		

        // changing portals section
        private static void PortalChanger()
		{	
			var peter = GetPieces().Find((GameObject g) => Utils.GetPrefabName(g) == "portal_wood"); //item prefab loaded from hammer
			if (peter != null)
				{
					WearNTear por =  peter.GetComponent<WearNTear>();
					por.m_health = PortalHealth; // set New Portal Health

					List<Piece.Requirement> requirements = new List<Piece.Requirement>();
						requirements.Add(new Piece.Requirement
						{
							m_amount = 20,
							m_resItem = ObjectDB.instance.GetItemPrefab("FineWood").GetComponent<ItemDrop>(),
							m_recover = true
						});
						if (!DisablePortalJuice) { // make this more dynamic
							requirements.Add(new Piece.Requirement
							{
								m_amount = 1,
								m_resItem = ObjectDB.instance.GetItemPrefab("PortalMagicFluid").GetComponent<ItemDrop>(),
								m_recover = true
							});
						}
						requirements.Add(new Piece.Requirement
						{
							m_amount = 10,
							m_resItem = ObjectDB.instance.GetItemPrefab("GreydwarfEye").GetComponent<ItemDrop>(),
							m_recover = true
						});
						requirements.Add(new Piece.Requirement
						{
							m_amount = 2,
							m_resItem = ObjectDB.instance.GetItemPrefab("SurtlingCore").GetComponent<ItemDrop>(),
							m_recover = true
						});

					var CraftingStationforPaul = GetCraftingStation(TabletoAddTo);
					if (CraftingStationforPaul == null)
					{
						CraftingStationforPaul.m_name = DefaultTable;
					}

					Piece petercomponent = peter.GetComponent<Piece>();
					petercomponent.m_craftingStation = GetCraftingStation(CraftingStationforPaul.m_name); // sets crafting station workbench/forge /ect
					petercomponent.m_resources = requirements.ToArray();

            }		// if loop	
																			  			

		}

		private static void StartingFirsttime()
		{
			firstTime = true;

		}

		private static void StartingitemPrefab()
		{

			if (firstTime && PortalMagicFluidSpawn != 0)
			{
				RareMagicPortal.LogInfo("New Starting Item Set");
				Inventory inventory = ((Humanoid)Player.m_localPlayer).m_inventory;
				inventory.AddItem("PortalMagicFluid", PortalMagicFluidSpawn, 1, 0, 0L, "");
				firstTime = false;

			}
		}


		public static void Dbgl(string str = "", bool pref = true)
		{
			if (false) // debug
			{
				Debug.Log((pref ? (typeof(MagicPortalFluid).Namespace + " ") : "") + str);
			}
		}

		public static IEnumerator DelayedLoad()
		{
			yield return new WaitForSeconds(0.3f);
			LoadAllRecipeData(reload: true);
			yield break;
		}

		private static void LoadAllRecipeData(bool reload)
		{
			if (reload) // waits until the last seconds to reference and overwrite
			{

				PortalChanger();
				LoadIN();
				
			}
		}

		private static CraftingStation GetCraftingStation(string name)
		{
			if (name == "")
			{
				return null;
			}
			foreach (Recipe recipe in ObjectDB.instance.m_recipes)
			{
				if (recipe?.m_craftingStation?.m_name == name)
				{
					//Jotunn.Logger.LogMessage("got crafting station " + name);
					return recipe.m_craftingStation;
				}
			}
			return null;
		}

		private static List<GameObject> GetPieces()
		{
			List<GameObject> list = new List<GameObject>();
			if (!ObjectDB.instance)
			{
				return list;
			}
			ItemDrop itemDrop = ObjectDB.instance.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>();
			if ((bool)itemDrop)
			{
				list.AddRange(Traverse.Create((object)itemDrop.m_itemData.m_shared.m_buildPieces).Field("m_pieces").GetValue<List<GameObject>>());
			}
			ItemDrop itemDrop2 = ObjectDB.instance.GetItemPrefab("Hoe")?.GetComponent<ItemDrop>();
			if ((bool)itemDrop2)
			{
				list.AddRange(Traverse.Create((object)itemDrop2.m_itemData.m_shared.m_buildPieces).Field("m_pieces").GetValue<List<GameObject>>());
			}
			return list;
		}
		private static RecipeData GetPieceRecipeByName(string name)
		{
			GameObject gameObject = GetPieces().Find((GameObject g) => Utils.GetPrefabName(g) == name);
			if (gameObject == null)
			{
				Dbgl("Item " + name + " not found!");
				return null;
			}
			Piece component = gameObject.GetComponent<Piece>();
			if (component == null)
			{
				Dbgl("Item data not found!");
				return null;
			}
			RecipeData recipeData = new RecipeData
			{
				name = name,
				amount = 1,
				craftingStation = (component.m_craftingStation?.m_name ?? ""),
				minStationLevel = 1
			};
			Piece.Requirement[] resources = component.m_resources;
			foreach (Piece.Requirement requirement in resources)
			{
				recipeData.reqs.Add($"{Utils.GetPrefabName(requirement.m_resItem.gameObject)}:{requirement.m_amount}:{requirement.m_amountPerLevel}:{requirement.m_recover}");
			}
			return recipeData;
		}

		#region ConfigOptions

		private static ConfigEntry<bool>? _serverConfigLocked;


		private static readonly CustomSyncedValue<string> YMLPortalData = new(ConfigSync, "PortalYmlData", ""); // doesn't show up in config
		private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
			bool synchronizedSetting = true)
		{
			ConfigDescription extendedDescription =
				new(
					description.Description +
					(synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
					description.AcceptableValues, description.Tags);
			ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
			//var configEntry = Config.Bind(group, name, value, description);

			SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

			return configEntry;
		}

		private ConfigEntry<T> config<T>(string group, string name, T value, string description,
			bool synchronizedSetting = true)
		{
			return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
		}

		private class ConfigurationManagerAttributes
		{
			public bool? Browsable = false;
		}

		#endregion
	

	private void CreateConfigValues()
		{
		_serverConfigLocked = config("General", "Force Server Config", true, "Force Server Config");
		_ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

		// Add server config which gets pushed to all clients connecting and can only be edited by admins
		// In local/single player games the player is always considered the admin

			ConfigFluid = config("PortalJuice", "DisablePortalJuice", false,
							"Disable PortalFluid requirement?");

			ConfigSpawn = config("PortalJuice", "PortalMagicFluidSpawn", 3,
				"How much PortalMagicFluid to start with on a new character?");

			ConfigFluidValue = config("PortalJuice", "PortalJuiceValue", 300, "What is the value of MagicPortalJuice? " + System.Environment.NewLine + "A Value of 0, makes item not saleable to trader");

			ConfigTable = config("Portal Config", "CraftingStation_Requirement", DefaultTable,
				"Which CraftingStation is required nearby?" + System.Environment.NewLine + "Default is Workbench = $piece_workbench, forge = $piece_forge, Artisan station = $piece_artisanstation "  + System.Environment.NewLine + "Pick a valid table otherwise default is workbench"); // $piece_workbench , $piece_forge , $piece_artisanstation

			ConfigTableLvl = config("Portal Config", "Level_of_CraftingStation_Req", 1,
				"What level of CraftingStation is required for placing Portal?");

			ConfigCreator = config("Portal Config", "OnlyCreatorCanDeconstruct", true, "Only the Creator of the Portal can deconstruct it. It can still be destoryed");

			ConfiglHealth = config("Portal Config", "Portal_Health", 400f, "Health of Portal");

			ConfigCreatorLock = config("Portal Config", "OnlyCreatorCanChange", false, "Only Creator can change Portal name");

			ConfigEnableCrystals = config("Portal Crystals", "Portal_Crystal_Enable", false, "Enable Portal Crystals");

			ConfigEnableKeys = config("Portal Keys", "Portal_Keys_Enable", false, "Enable Portal Crystals");

			ConfigCrystalsConsumable = config("Portal Crystals", "Crystal_Consume_Default", 1, "How many Master and Primary Crystals to Consume by Default for New Portals?");

			ConfigAdminOnly = config("Portal Config", "Only_Admin_Can_Build", false, "Only The Admins Can Build Portals");

			CrystalKeyDefaultColor = config("Portal Crystals", "Portal_Crystal_Color_Default", "blue", "Default Color for New Portals");
		}

	private void ReadAndWriteConfigValues()
		{
			//RareMagicPortal.LogInfo("Reached Read and Write");
			DisablePortalJuice = (bool)Config["PortalJuice", "DisablePortalJuice"].BoxedValue;
			PortalMagicFluidSpawn = (int)Config["PortalJuice", "PortalMagicFluidSpawn"].BoxedValue;
			TabletoAddTo = (string)Config["Portal Config", "CraftingStation_Requirement"].BoxedValue;
			CraftingStationlvl = (int)Config["Portal Config", "Level_of_CraftingStation_Req"].BoxedValue;
			CreatorOnly = (bool)Config["Portal Config", "OnlyCreatorCanDeconstruct"].BoxedValue;
			PortalHealth = (float)Config["Portal Config", "Portal_Health"].BoxedValue;
			CreatorLock = (bool)Config["Portal Config", "OnlyCreatorCanChange"].BoxedValue;
			MagicPortalFluidValue = (int)Config["PortalJuice", "PortalJuiceValue"].BoxedValue;
			EnableCrystals = (bool)Config["Portal Crystals", "Portal_Crystal_Enable"].BoxedValue;
			EnableKeys = (bool)Config["Portal Keys", "Portal_Keys_Enable"].BoxedValue;
			DefaultPortalColor = (string)Config["Portal Crystals", "Portal_Crystal_Color_Default"].BoxedValue;
			CrystalsConsumable = (int)Config["Portal Crystals", "Crystal_Consume_Default"].BoxedValue;
			AdminOnlyBuild = (bool)Config["Portal Config", "Only_Admin_Can_Build"].BoxedValue;


			if (CraftingStationlvl > 10 || CraftingStationlvl < 1)
				CraftingStationlvl = 1;

		}

	private static void WritetoYML(string PortalName, int CrystalsConsumable)
        {
			if (isAdmin)  // I am not sure why a non admin would need to write.
			{
				// need to write soemthing for DefaultPortalColor and Key setup

				PortalName.Portal paulgo = new PortalName.Portal
				{
					Crystal_Cost_Master = CrystalsConsumable, // If only using master crystals
				};
				PortalN.Portals.Add(PortalName, paulgo);
				var serializer = new SerializerBuilder()
					.Build();
				var yaml = WelcomeString + Environment.NewLine + serializer.Serialize(PortalN); // build everytime

				File.WriteAllText(YMLCurrentFile, yaml); //overwrite
					

			}

		}

	}
	// end of namespace class

}

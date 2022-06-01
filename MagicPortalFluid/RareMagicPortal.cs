// MagicPortalFluid
// a Valheim mod created by WackyMole. Do whatever with it. - WM
// assets from https://assetstore.unity.com/packages/3d/props/interior/free-alchemy-and-magic-pack-142991
// 
// File:    MagicPortalFluid.cs
// Project: MagicPortalFluid

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
		public static string PiecetoLookFor = "portal_wood"; //name
		public static string PieceTokenLookFor = "$piece_portal"; //m_name
		public static Vector3 tempvalue;
		public static bool loadfilesonce = false;

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
		public static int CrystalsConsumable = 1;
		public static int CurrentCrystalCount;


		private static ConfigEntry<bool>? ConfigFluid;
		private static ConfigEntry<int>? ConfigSpawn;
		private static ConfigEntry<string>? ConfigTable;
		private static ConfigEntry<int>? ConfigTableLvl;
		private static ConfigEntry<bool>? ConfigCreator;
		private static ConfigEntry<float>? ConfiglHealth;
		private static ConfigEntry<bool>? ConfigCreatorLock;
		private static ConfigEntry<int>? ConfigFluidValue;
		private static ConfigEntry<bool>? ConfigEnableCrystals;
		private static ConfigEntry<int>? ConfigCrystalsConsumable;


		public static ItemDrop.ItemData Crystal { get; private set; }

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		[HarmonyPriority(0)]
		private static class ZNetScene_Awake_PatchWRare
		{
			private static void Postfix()
			{
				{
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


		[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]  // for Crystals
		static class TeleportWorld_Teleport_CheckforCrystal
		{
			[HarmonyPrefix]
			private static bool Prefix(TeleportWorld __instance, ref Player player)
			{
				if (EnableCrystals)
				{
					ItemDrop.ItemData Crystal = null;
					//ItemDrop.ItemData Crystal = ObjectDB.instance.GetItemPrefab("PortalCrystal").GetComponent<ItemDrop>().m_itemData; // IDK this wasn't working
					//CurrentCrystalCount = player.m_inventory.CountItems("$portalmagiccrystal"); 
					string nameC = "$portalmagiccrystal";
					List<ItemDrop.ItemData> GetItem = player.m_inventory.GetAllItems();
					CurrentCrystalCount = 0;
					foreach (ItemDrop.ItemData item in GetItem)
					{
						if (item.m_shared.m_name == nameC)
						{
							CurrentCrystalCount += item.m_stack;
							Crystal = item;
						}
					}
					if (CurrentCrystalCount > 0)
					{
						if (CrystalsConsumable > 0) // if enough for teleport
						{
							if (!player.IsTeleportable())
							{
								player.Message(MessageHud.MessageType.Center, "$msg_noteleport");
								return false;
							}
							Piece portal = null;
							var emptyPiece = new List<Piece>();
							Piece.GetAllPiecesInRadius(__instance.transform.position, 2f, emptyPiece);
							foreach (var piece in emptyPiece)
							{
								if (piece.m_name == "portal_wood")
								{
									portal = piece;
								}
							}
							if (portal != null)
                            {

                            }
							player.m_inventory.RemoveItem(Crystal, CrystalsConsumable);
							if (CrystalsConsumable > 1)
                            {
								player.Message(MessageHud.MessageType.TopLeft, $"Consumed {CrystalsConsumable} Portal Crystals");
							} else
                            {
								player.Message(MessageHud.MessageType.TopLeft, $" One Portal Crystal Consumed");
                            }
							
							return true;
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
			


			RareMagicPortal.LogInfo("MagicPortalFluid has loaded start assets");

		}


		// end startup

		private void LoadAssets()
		{
			Item portalmagicfluid = new("portalmagicfluid", "portalmagicfluid", "assetsEmbedded");
			portalmagicfluid.Name.English("Magical Portal Fluid");
			portalmagicfluid.Description.English("Once a mythical essence, now made real with Odin's blessing");
			
			Item PortalCrystal = new("portalcrystal", "PortalCrystal", "assetsEmbedded");
			PortalCrystal.Name.English("Portal Essence Crystal");
			PortalCrystal.Description.English("Unlock Traveling Odin's Crystals");


		}

		private void UnLoadAssets()
		{
			portalmagicfluid.Unload(false);
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

		private void ReadConfigValues(object sender, FileSystemEventArgs e) // Thx Azumatt
        {
            if (!File.Exists(ConfigFileFullPath)) return;
			//RareMagicPortal.LogInfo("ReadConfigValues called- checking admin status");
			bool admin= !ConfigSync.IsLocked; // or locked?
			if (admin)
			{
				RareMagicPortal.LogInfo("ReadConfigValues loaded");
				try
				{
					

					if (ConfigSync.IsSourceOfTruth)
					{
						RareMagicPortal.LogInfo("ReadConfigValues loaded- you are an admin-maybe");
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
					

					if (ConfigSync.IsSourceOfTruth)
					{
						RareMagicPortal.LogInfo("ReadConfigValues loaded- You are an local admin");
						Config.Reload(); // I have no idea why, but both of these have to run to update from file properly // this one for configmanger
						ReadAndWriteConfigValues(); // It could be a synchronizing issue. But it's not updating after I call it again. So weird // this one for local file edit
						PortalChanger();
					}
					else
					{
						// false so remote config is being used
						RareMagicPortal.LogInfo("You are not an admin - Server Sync Values loaded ");
						//Config.Reload(); // I have no idea why, but both of these have to run to update from file properly // this one for configmanger
						ReadAndWriteConfigValues(); // It could be a synchronizing issue. But it's not updating after I call it again. So weird // this one for local file edit
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

			ConfigCrystalsConsumable = config("Portal Crystals", "Crystal_Consume_Default", 1, "How many Crystals to Consume on Teleporting by Default, 0 is No Consumption");

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
			CrystalsConsumable = (int)Config["Portal Crystals", "Crystal_Consume_Default"].BoxedValue;



			if (CraftingStationlvl > 10 || CraftingStationlvl < 1)
				CraftingStationlvl = 1;

		}

	}
	// end of namespace class

}

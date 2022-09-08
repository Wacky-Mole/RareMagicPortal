// MagicPortalFluid
// a Valheim mod created by WackyMole. Do whatever with it. - WM
// assets from https://assetstore.unity.com/packages/3d/props/interior/free-alchemy-and-magic-pack-142991
// crystal assets from https://assetstore.unity.com/packages/3d/environments/fantasy/translucent-crystals-106274
//  Thank you https://github.com/redseiko/ComfyMods/tree/main/ColorfulPortals
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
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using HarmonyLib;
using RareMagicPortal;
//using PieceManager;
using ServerSync;
using ItemManager;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using YamlDotNet;
using YamlDotNet.Serialization;
using LocalizationManager;
using StatusEffectManager;



namespace RareMagicPortal
{
	//extra
	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	[BepInDependency("org.bepinex.plugins.targetportal", BepInDependency.DependencyFlags.SoftDependency)]  // it loads before this mod// not really required, but whatever
	internal class MagicPortalFluid : BaseUnityPlugin
	{
		public const string PluginGUID = "WackyMole.RareMagicPortal";
		public const string PluginName = "RareMagicPortal";
		public const string PluginVersion = "2.4.1";

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
		{ DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = "2.4.1" };

		private static MagicPortalFluid? plugin;
		private static MagicPortalFluid context;

		private AssetBundle portalmagicfluid;
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
		public static bool EnablePortalJuice = false; // it is true
		public static bool EnableExtraYMLLog = true;
		public static string TabletoAddTo;
		public static string DefaultTable = "$piece_workbench";
		public static bool piecehaslvl = false;
		public static bool CreatorOnly = true;
		public static bool CreatorLock = false;
		public static float PortalHealth = 400f;
		public static int CraftingStationlvl = 1;
		public static int MagicPortalFluidValue = 0;
		public static bool EnableCrystals = false;
		//public static bool EnableKeys = false;
		public static int CrystalsConsumable = 1;
		public static bool AdminOnlyBuild = false;
		public static string DefaultPortalColor = "blue";
		public static int DrinkDuration = 120;
		public static bool cyclewhite = false;
		public static string DefaultRestrictString = "";
		public static bool UseGoldASMaster = true;
		public static string ColorsEnabled = "";
		public static bool msgLeft = false;

		private static string YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");
		private static int JustWrote = 0;
		private static bool JustWait = false;
		private static bool JustRespawn = false;
		private static bool NoMoreLoading = false;
		private static bool Teleporting = false;
		private static string checkiftagisPortal = null;

		private static Player player = null; // need to keep it between patches
		private static bool m_hadTarget = false;
		private static List<Minimap.PinData> HoldPins;
		private static bool Globaliscreator = false;


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
		private static ConfigEntry<int>? PortalDrinkTimer;
		private static ConfigEntry<bool>? ConfigEnableYMLLogs;
		private static ConfigEntry<string>? ConfigAddRestricted;
		private static ConfigEntry<bool>? ConfigEnableGoldAsMaster;
		private static ConfigEntry<string>? ConfigEnableColorEnable;
		private static ConfigEntry<KeyboardShortcut>? portalRMPKEY = null!;
		private static ConfigEntry<bool>? ConfigMessageLeft;

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
		public const string CrystalPurple = "$item_PortalCrystalPurple";
		public const string CrystalTan = "$item_PortalCrystalTan";

		public const string PortalKeyGold = "$item_PortalKeyGold";
		public const string PortalKeyRed = "$item_PortalKeyRed";
		public const string PortalKeyGreen = "$item_PortalKeyGreen";
		public const string PortalKeyBlue = "$item_PortalKeyBlue";
		public const string PortalKeyPurple = "$item_PortalKeyPurple";
		public const string PortalKeyTan = "$item_PortalKeyTan";

		SpriteTools IconColor = new SpriteTools();

		public static Sprite IconBlack = null!;
		public static Sprite IconYellow = null!;
		public static Sprite IconRed = null!;
		public static Sprite IconGreen = null!;
		public static Sprite IconBlue = null!;
		public static Sprite IconGold = null!;
		public static Sprite IconWhite = null!;
		public static Sprite IconDefault = null!;
		public static Sprite IconPurple = null!;
		public static Sprite IconTan = null!;

		internal static Localization english = null!;

		public static CustomSE AllowTeleEverything = new CustomSE("yippeTele");
		public static List<StatusEffect> statusEffectactive;

		private static readonly List<string> portalPrefabs = new List<string>();


		public static string WelcomeString = "#Hello, this is the Portal yml file. It keeps track of all portals you enter";
		private static PortalName PortalN;

		static Coroutine myCoroutineRMP;
		public static ItemDrop.ItemData Crystal { get; private set; }

		static readonly int _teleportWorldColorHashCode = "TeleportWorldColor".GetStableHashCode(); // I should probably change this
		static readonly int _teleportWorldColorAlphaHashCode = "TeleportWorldColorAlpha".GetStableHashCode();
		static readonly int _portalLastColoredByHashCode = "PortalLastColoredBy".GetStableHashCode();
		private static string PortalFluidname;
		private static bool TargetPortalLoaded = false;

		static readonly Dictionary<TeleportWorld, TeleportWorldDataRMP> _teleportWorldDataCache = new();
		static readonly KeyboardShortcut _changePortalReq= new(KeyCode.E, KeyCode.LeftControl);

		static Color m_colorTargetfound = new Color(191f/255f, 150f/255f, 0, 25);
		static Color lightcolor = new Color (1f, 100f/255f , 0 , 1f);
		//Material PortalDefMaterial = originalMaterials["portal_small"];
		static Color flamesstart = new Color(1f, 194f/255f, 34f/255f, 1f);
		static Color flamesend = new Color(1f, 0, 0, 1f);
		static Color Gold = new Color(1f, 215f / 255f, 0, 1f);
		static Color Purple = new Color(107f / 255f, 63f / 255f, 160 / 255f, 1f);
		static Color Tan = new Color(210f / 255f, 180f / 255f, 140f / 255f, 1f);
		static Color Brown = new Color(193f/255f, 69f/255f, 19f/255f, 1f);

		static private Dictionary<string, bool> enaColor = new Dictionary<string, bool>(){
				{"Red", false },
				{"Blue", false },
				{"Green", false },
				{"Gold", false },
				{"Purple",false },
				{"Tan",false }
			};


		static IEnumerator RemovedDestroyedTeleportWorldsCoroutine()
		{
			WaitForSeconds waitThirtySeconds = new(seconds: 30f);
			List<KeyValuePair<TeleportWorld, TeleportWorldDataRMP>> existingPortals = new();
			int portalCount = 0;

			while (true)
			{
				yield return waitThirtySeconds;
				portalCount = _teleportWorldDataCache.Count;

				existingPortals.AddRange(_teleportWorldDataCache.Where(entry => entry.Key));
				_teleportWorldDataCache.Clear();

				foreach (KeyValuePair<TeleportWorld, TeleportWorldDataRMP> entry in existingPortals)
				{
					_teleportWorldDataCache[entry.Key] = entry.Value;
				}

				existingPortals.Clear();

				if ((portalCount - _teleportWorldDataCache.Count) > 0)
				{
					RareMagicPortal.LogInfo($"Removed {portalCount - _teleportWorldDataCache.Count}/{portalCount} portal references.");
				}
			}
		}


		[HarmonyPatch(typeof(TeleportWorld))] // thank you https://github.com/redseiko/ComfyMods/tree/main/ColorfulPortals
		class TeleportWorldPatchRMP
		{
			//static readonly KeyboardShortcut _changeColorActionShortcut = new(KeyCode.E, KeyCode.LeftShift);

			[HarmonyPostfix]
			[HarmonyPatch(nameof(TeleportWorld.Awake))]
			static void TeleportWorldAwakePostfixRMP(ref TeleportWorld __instance)
			{
				
				if (!__instance)
				{
					return;
				}

				// Stone 'portal' prefab does not set this property.
				if (!__instance.m_proximityRoot)
				{
					__instance.m_proximityRoot = __instance.transform;
				}
				
				// Stone 'portal' prefab does not set this property.
				if (!__instance.m_target_found ) 
				{
					// The prefab does not have '_target_found_red' but instead '_target_found'.
					GameObject targetFoundObject = __instance.gameObject.transform.Find("_target_found").gameObject;

					// Disable the GameObject first, as adding component EffectFade calls its Awake() before being attached.
					targetFoundObject.SetActive(false);
					__instance.m_target_found = targetFoundObject.AddComponent<EffectFade>();
					targetFoundObject.SetActive(true);
				}
				//RareMagicPortal.LogInfo("Adding Portal Awake for all Portals");

				_teleportWorldDataCache.Add(__instance, new TeleportWorldDataRMP(__instance));
			}

			[HarmonyPostfix]
			[HarmonyPriority(Priority.Low)]
			[HarmonyPatch(nameof(TeleportWorld.UpdatePortal))]
			static void TeleportWorldUpdatePortalPostfixRMP(ref TeleportWorld __instance)
			{
				if (//!ConfigEnableCrystals.Value
					   !__instance
					|| !__instance.m_nview
					|| __instance.m_nview.m_zdo == null
					|| __instance.m_nview.m_zdo.m_zdoMan == null
					|| __instance.m_nview.m_zdo.m_vec3 == null )
					//|| !__instance.m_nview.m_zdo.m_vec3.ContainsKey(_teleportWorldColorHashCode)) // going to ask for it below, so no reason to get a null
					//|| !_teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData)) // I don't think this will break anything
				{
					return;
				}
				try
				{
					_teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData);
					if (Player.m_localPlayer.m_seman.GetStatusEffect("yippeTele") != null)
					{
						// override color for white
						teleportWorldData.TargetColor = Color.white;
						SetTeleportWorldColors(teleportWorldData, false, false);
					}
					else
					{
						string PortalName = __instance.m_nview.m_zdo.GetString("tag", "Empty tag");
						int colorint = CrystalandKeyLogicColor(PortalName); // this should sync up portal colors
					
						//Color CurrentZDOColor = Utils.Vec3ToColor(__instance.m_nview.m_zdo.m_vec3.GetValueSafe(_teleportWorldColorHashCode));

						Color color;
						
						switch (colorint)
						{
							case 0:
								color = Color.black;
								break;
							case 1:
								color = Color.yellow;
								break;
							case 2:
								color = Color.red;
								break;
							case 3:
								color = Color.green;
								break;
							case 4:
								color = Color.cyan;
								break;
							case 5:
								color = Purple;
								break;
							case 6:
								color = Tan;
								break;
							case 7:
								color = Gold; // go with material change small_portal
								break;
							case 8:
								color = Color.white;
								break;
							default:
								color = Color.yellow;
								break;

						}
						//if (!__instance.m_nview.m_zdo.m_vec3.ContainsKey(_teleportWorldColorHashCode))
						//	__instance.m_nview.m_zdo.Set(_teleportWorldColorHashCode, Utils.ColorToVec3(color));

						if (color != teleportWorldData.OldColor)
						{  // don't waste resources
							teleportWorldData.TargetColor = color;
							SetTeleportWorldColors(teleportWorldData, true);
						}

						
					}
					//if (TargetPortalLoaded)
					//__instance.gameObject.transform.Find("_target_found").gameObject.SetActive(false);

					
				}
				catch { } // catches beginning errors

				
			}
		}

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		[HarmonyPriority(0)]
		private static class ZNetScene_Awake_PatchWRare
		{
			private static void Postfix()
			{
				{
					Worldname = ZNet.instance.GetWorldName();// for singleplayer  // won't be ready for multiplayer
					TargetPortalLoaded = Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal");

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

		[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.GetHoverText))]
		public static class TeleportWorldGetHoverTextPostfixRMP
		{
			static void Postfix(ref TeleportWorld __instance, ref string __result)
			{
				if (!__instance)
				{
					return;
				}
				Piece portal = __instance.GetComponent<Piece>();
				Player closestPlayer = Player.m_localPlayer;
				bool sameperson = false;
				if (portal.m_creator == closestPlayer.GetPlayerID())
					sameperson = true;

				string PortalName = __instance.m_nview.m_zdo.GetString("tag", "Empty tag");
			
				
				int colorint = CrystalandKeyLogicColor(PortalName);
				string currentcolor = "Default";
				string nextcolor;
				string text;
				Color color;
				switch (colorint)
				{
					case 0:
						currentcolor = "Black";
						nextcolor = "Yellow";
						text = "Admin Only";
						color = Color.black;
						break;
					case 1:
						currentcolor = "Yellow";
						nextcolor = "Red";
						text = "Normal Portal";
						color = Color.yellow;
						break;
					case 2:
						currentcolor = "Red";
						nextcolor = "Green";
						text = "Red Crystal Portal";
						color = Color.red;
						break;
					case 3:
						currentcolor = "Green";
						nextcolor = "Blue";
						text = "Green Crystal Portal";
						color = Color.green;
						break;
					case 4:
						currentcolor = "Blue";
						nextcolor = "Gold";
						text = "Blue Crystal Portal";
						color = Color.cyan;
						break;
					case 5:
						currentcolor = "Purple";
						nextcolor = "Tan";
						text = "Purple Crystal Portal";
						color = Purple;
						break;
					case 6:
						currentcolor = "Tan";
						nextcolor = "Gold";
						text = "Tan Crystal Portal";
						color = Tan;
						break;
					case 7:
						currentcolor = "Gold";
						nextcolor = "White";
						text = "Gold Crystal Portal";
						color = Gold;
						break;
					case 8:
						currentcolor = "White";
						nextcolor = "Black";
						text = "Any Teleportation Portal with No Crystal Cost";
						color = Color.white;
						break;
					default:
						currentcolor = "Yellow";
						text = "";
						nextcolor = currentcolor;
						color = Color.yellow;
						break;

				}
				if (
				!__instance.m_nview
				|| __instance.m_nview.m_zdo == null
				|| __instance.m_nview.m_zdo.m_zdoMan == null
				|| __instance.m_nview.m_zdo.m_vec3 == null)
				{
					RareMagicPortal.LogInfo("Setting Portal Color For First Time");
					if (_teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData))
					{
						teleportWorldData.TargetColor = color;
						SetTeleportWorldColors(teleportWorldData, true);
						__instance.m_nview.m_zdo.Set(_teleportWorldColorHashCode, Utils.ColorToVec3(color));
						__instance.m_nview.m_zdo.Set(_portalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);
					}
				}
				if (PortalName != "" && PortalName != "Empty tag")
				{ 
						if (isAdmin || sameperson && !EnableCrystals)
						{
							//string Shortcut

							if (portalRMPKEY.Value.MainKey is KeyCode.None)
							{
								//Shortcut = portalRMPKEY.Value;
								return; // because it would error out otherwise

							}
							// L-ctrl + E instead of _

							if (EnableCrystals)
							{
								__result =
									string.Format(
										"{0}\n<size={4}>[<color={5}>{2}</color>] Change <color={1}>Portal</color>[{1}] Crystal to: [<color={3}>{3}</color>]</size>\n<size={4}>{6}</size>",
										__result,
										currentcolor,
										portalRMPKEY.Value + " + " + "E",//_changePortalReq,
										nextcolor,
										15,
										"Yellow",
										text);
							}
							else
							{
								__result =
									string.Format(
										"{0}\n<size={4}>[<color={5}>{2}</color>] Change <color={1}>Portal</color>[{1}] Color to: [<color={3}>{3}</color>] </size>",
										__result,
										currentcolor,
										portalRMPKEY.Value + " + " + "E", //_changePortalReq,
										nextcolor,
										15,
										"Yellow"
										);
							}
						}
						else
						{
							if (EnableCrystals)
							{
								__result =
									string.Format(
										"{0}\n<size={2}><color={1}>{1} Portal</color></size>\n<size={2}>{4}</size>",
										__result,
										currentcolor,
										15,
										"Yellow",
										text
										);
							}
							else
							{
								__result =
									string.Format(
										"{0}\n<size={2}><color={1}>{1} Portal</color></size>",
										__result,
										currentcolor,
										15,
										"Yellow"
										);
							}

						}
				}else// name = ""
                {
					string jo = "Please name Portal to change from Default";
					__result =
								string.Format(
									"{0}\n<size={1}><color={3}>{2}</color></size>",
									__result,
									15,
									jo,
									"Yellow"
									);

				} 
			}
		}
		[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.TargetFound))]
		public static class DisabldHaveTarget
		{
			private static bool Prefix(ref bool __result)
            {
               if (Player.m_localPlayer.m_seman.GetStatusEffect("yippeTele") != null)
                {
					__result = true;
					return false;
				}
                if (TargetPortalLoaded ) // I should make a config for this instead
				{
					__result = false;
					return false;
				}
				return true;
			}
		}


		[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Interact))]
		public static class PortalCheckOutside
		{
			private static bool Prefix(TeleportWorld __instance, Humanoid human, bool hold)

			{
				if (hold)
					return false;

				if (Chainloader.PluginInfos.ContainsKey("com.sweetgiorni.anyportal"))
				{ // check to see if AnyPortal is loaded // don't touch when anyportal is loaded
					return true;
				}

				if (__instance.m_nview.IsValid())
				{
					//RareMagicPortal.LogInfo($"Made it to Map during Portal Interact");
					Piece portal = null;
					portal = __instance.GetComponent<Piece>();
					string PortalName = __instance.m_nview.m_zdo.GetString("tag", "Empty tag");
					

					if (portal != null && PortalName !="" && PortalName != "Empty tag")
					{
						Player closestPlayer = Player.m_localPlayer; //Player.GetClosestPlayer(__instance.m_proximityRoot.position, 5f);
						bool sameperson = false;
						if (portal.m_creator == closestPlayer.GetPlayerID())
							sameperson = true;

						//RareMagicPortal.LogInfo($"Made it to Map during Portal Interact Past portal check and is admin {isAdmin}");
						if (Input.GetKey(portalRMPKEY.Value.MainKey) && portalRMPKEY.Value.Modifiers.All(Input.GetKey) && (isAdmin || sameperson && !EnableCrystals)) // creator can change it if enable crystals is off
						{
							Globaliscreator = sameperson; // set this for yml permissions
						//	RareMagicPortal.LogInfo($"Made it to Map during teleworldcache");

							int colorint = CrystalandKeyLogicColor(PortalName);
							string currentcolor = "Default";
							string nextcolor;
							string text;
							Color color = Color.yellow;
							Color Gold = new Color(1f, 215f/255f, 0, 1f);

							// color needs to be the nextcolor color = Color.white;
							switch (colorint)
							{
								case 0:
									currentcolor = "Black";
									nextcolor = "Yellow";
									text = "Admin Only";
									color = Color.yellow;
									break;
								case 1:
									currentcolor = "Yellow";
									nextcolor = "Red";
									text = "Normal Portal";
									color = Color.red;
									break;
								case 2:
									currentcolor = "Red";
									nextcolor = "Green";
									text = "Red Crystal Portal";
									color = Color.green;
									break;
								case 3:
									currentcolor = "Green";
									nextcolor = "Blue";
									text = "Green Crystal Portal";
									color = Color.cyan;
									break;
								case 4:
									currentcolor = "Blue";
									nextcolor = "Purple";
									text = "Blue Crystal Portal";
									color = Purple;
									break;
								case 5:
									currentcolor = "Purple";
									nextcolor = "Tan";
									text = "Purple Crystal Portal";
									color = Tan;
									break;
								case 6:
									currentcolor = "Tan";
									nextcolor = "Gold";
									text = "Tan Crystal Portal";
									color = Gold;
									break;
								case 7:
									currentcolor = "Gold";
									nextcolor = "White";
									text = "Gold Crystal Portal";
									color = Color.white; // go with material change small_portal
									break;
								case 8:
									currentcolor = "White";
									nextcolor = "Black";
									text = "Any Teleportation Portal No Cost";
									color = Color.black;
									break;
								default:
									currentcolor = "Yellow";
									text = "";
									nextcolor = currentcolor;
									color = Color.yellow;
									break;

							}

							if (_teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData))
							{
								teleportWorldData.TargetColor = color;
								SetTeleportWorldColors(teleportWorldData, true);
							}
							__instance.m_nview.m_zdo.Set(_teleportWorldColorHashCode, Utils.ColorToVec3(color));
							//__instance.m_nview.m_zdo.Set(_teleportWorldColorAlphaHashCode, color);
							__instance.m_nview.m_zdo.Set(_portalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);

							// now need to set the yml file to update with these changes on interact
							if (colorint == 8) // interate one up
								colorint = 0;
							else colorint++;
					
							updateYmltoColorChange(PortalName, colorint);

							return false; // stop interaction on changing name

						}

						if (sameperson || !sameperson && !CreatorLock || closestPlayer.m_noPlacementCost)
						{
							return true;
						} // Only creator || not creator and not in lock mode || not in noplacementcost mode
						human.Message(MessageHud.MessageType.Center, "$rmp_onlyownercanchange");
						return false; // noncreator doesn't have permiss
					}
					return true;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Minimap), "SetMapMode")] // doesn't matter if targetportal is loaded or not
		public class LeavePortalModeOnMapCloseMagicPortal
		{
			private static void Postfix(Minimap.MapMode mode)
			{
				if (mode != Minimap.MapMode.Large)
				{
					Teleporting = false; 
				}
			}
		}
	

		[HarmonyPatch(typeof(Inventory), "IsTeleportable")]
		public static class InventoryIsTeleportablePatchRMP
		{
			[HarmonyPriority(Priority.LowerThanNormal)]
			private static bool Prefix(ref bool __result, ref Inventory __instance)
			{
				/*
				if (Game.instance.m_firstSpawn)
				{
					return __result;
				}
				*/


				//Player.m_localPlayer
				bool bo2 = false;
				if  ( Player.m_localPlayer.m_seman.GetStatusEffect("yippeTele") != null)
                {
					bo2 = true;
				}


				Piece portal = null;
				String name = null;
				Vector3 hi = Player.m_localPlayer.transform.position;
				List<Piece> piecesfound = new List<Piece>();
				Piece.GetAllPiecesInRadius(hi, 5f, piecesfound);
				/*
				foreach (Piece piece in piecesfound)
                {
					//RareMagicPortal.LogInfo($"Piece found {piece.name}");
					if (piece.name == "portal_wood(Clone)") // list of pieces that have teleport world
                    {
						portal = piece;
						//RareMagicPortal.LogInfo($"Inventory found Portal");
						break;
                    }
                }*/
				TeleportWorld portalW = null;
				foreach (Piece piece in piecesfound)
                {
					if (piece.TryGetComponent<TeleportWorld>(out portalW))
					{
						break;
					}	
                }

				if (portalW != null)
				{
					//portalW = portal.GetComponent<TeleportWorld>();
					name = portalW.GetHoverText();
					if (name != null)
                    {
						var found = name.IndexOf(":") + 2;
						var end = name.IndexOf("\" ");
						var le = end - found;
						name = name.Substring(found, le); // lol wish it was more efficent
						//RareMagicPortal.LogInfo($"Inventory Portal Check name is {name}");

						var PortalName = name;
						bool OdinsKin = false;
						bool Free_Passage = false;
						bool TeleportAny = false;
						List<string> AdditionalProhibitItems;

						if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
						{
							WritetoYML(PortalName);
						}
						OdinsKin = PortalN.Portals[PortalName].Admin_only_Access;
						Free_Passage = PortalN.Portals[PortalName].Free_Passage;
						TeleportAny = PortalN.Portals[PortalName].TeleportAnything;
						AdditionalProhibitItems = PortalN.Portals[PortalName].AdditionalProhibitItems;

						if (TeleportAny && EnableCrystals) // allows for teleport anything portal if EnableCrystals otherwise just white
							bo2 = true;

						if (!bo2 && AdditionalProhibitItems.Count > 0)
                        {
							var instan = ObjectDB.instance;
							foreach (ItemDrop.ItemData allItem in __instance.GetAllItems())
							{
								foreach (var item in AdditionalProhibitItems)
								{
									GameObject go = instan.GetItemPrefab(item);
									if (go != null)
                                    {
										ItemDrop.ItemData data = go.GetComponent<ItemDrop>().m_itemData;
										if (data != null)
                                        {
											if (data.m_shared.m_name == allItem.m_shared.m_name)
                                            {
												RareMagicPortal.LogInfo($"Found Prohibited item {data.m_shared.m_name} Teleport Not Allowed");
												Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Prohibited item " + data.m_shared.m_name);
												__result = false;
												return false;
                                            }
                                        }
									}
									

								}
							}
                        }// end !bo2

					}
				}


				if (bo2) // if status effect is active 
                {
					__result = true;
					return false;
                }
				return true;

			}
		}


		[HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapLeftClick))]
		private class MapLeftClickForRareMagic // for magic portal
		{
			private class SkipPortalException2 : Exception // skip all other mods if targetportal is installed and passes everything else
			{
			}

			[HarmonyPriority(Priority.HigherThanNormal)]
			private static bool Prefix()
			{


				if (!Teleporting)
				{
					return true;
				}
				if (!Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal")){ // check to see if targetportal is loaded
					return true;
				}
				//RareMagicPortal.LogInfo($"Made it to Map during Telecheck");
				string PortalName;
				Minimap Instancpass = Minimap.instance;
				HoldPins = Instancpass.m_pins;
				//return true;
				
				try
				{
					 PortalName = HandlePortalClick(); //my handleportal click
				} catch { PortalName = null; }
					if (PortalName == null)
                {
					throw new SkipPortalException2();//return false; and stop TargetPortals from executing

				}
				if (!Player.m_localPlayer.IsTeleportable())
				{
					Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noteleport");
					return false;
				}
				if (CrystalandKeyLogic(PortalName))
                {
					//RareMagicPortal.LogInfo($"True, so TargetPortalShould Take over");

					//HandleTeleport(Instancpass);
					return true; // allow TargetPortal to do it's checks
					//throw new SkipPortalException2();//return false; and stop TargetPortals from executing
				} else
                {
					//RareMagicPortal.LogInfo($"TargetPortal is forced to stop");
					throw new SkipPortalException2();//return false; and stop TargetPortals from executing
					
				}
				
									 
				
			}
			private static Exception? Finalizer(Exception __exception) => __exception is SkipPortalException2 ? null : __exception;
		}

		[HarmonyPatch(typeof(Minimap), nameof(Minimap.Start))]
		[HarmonyPriority(Priority.Low)]
		public class AddMinimapPortalIconRMP
		{

			private static void Postfix(Minimap __instance)
			{
				HoldPins = Minimap.instance.m_pins;
				//RareMagicPortal.LogWarning("Here is MinimapStart");
			}
		}

		[HarmonyPatch(typeof(TeleportWorldTrigger), nameof(TeleportWorldTrigger.OnTriggerEnter))]  // for Crystals and Keys
		private class TeleportWorld_Teleport_CheckforCrystal
		{ 
			private class SkipPortalException : Exception
			{
			}
			//throw new SkipPortalException(); This is used for return false instead/ keeps other mods from loading patches.

			static string OutsideP = null;
			[HarmonyPriority(Priority.HigherThanNormal)]
			private static bool Prefix(TeleportWorldTrigger __instance , Collider collider)
			{
				//finding portal name
				if (collider.GetComponent<Player>() != Player.m_localPlayer)
				{
					throw new SkipPortalException();
				}

				
				player = collider.GetComponent<Player>();
				string PortalName = "";
				if (!Chainloader.PluginInfos.ContainsKey("com.sweetgiorni.anyportal"))
				{ // check to see if AnyPortal is loaded // don't touch when anyportal is loaded

					PortalName = __instance.m_tp.GetText();
				} else // for anyportal
                {
					ZDOID zDOID =  __instance.m_tp.m_nview.GetZDO().GetZDOID("target");
					ZDO zDO = ZDOMan.instance.GetZDO(zDOID);
					if (zDO == null || !zDO.IsValid())
					{
					}
					else
					{
						PortalName = zDO.GetString("tag", "Empty tag");
					}
				}
				// end finding portal name

				m_hadTarget = __instance.m_tp.m_hadTarget;
				OutsideP = PortalName;
				// keep player and m_hadTarget for future patch for targetportal

				if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal"))
                {
					Teleporting = true;
					return true; // skip on checking because we don't know where this is going 
					// we will catch in map for tele check
                }
				if (!m_hadTarget) // if no target continuie on with logic
					return false;

				if (CrystalandKeyLogic(PortalName) )
				{
					Teleporting = true;
					return true;

                }else // false 
                {
					Teleporting = false;
					if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal")) // or any other mods that need to be skipped // this shoudn't be hit
						throw new SkipPortalException();  // stops other mods from executing  // little worried about betterwards and loveisward
					else return false;
				}

				//else return true;
			}
			private static Exception? Finalizer(Exception __exception) => __exception is SkipPortalException ? null : __exception;
		
		[HarmonyPostfix]
			[HarmonyPriority(Priority.Low)]
			private static void Postfix(TeleportWorldTrigger __instance)
			{
			 if (Teleporting && Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal"))
				{
					//RareMagicPortal.LogInfo($"Made it to Portal Trigger");
					
					int colorint;
					String PName;
					String PortalName;
					Minimap instance = Minimap.instance;
					List<Minimap.PinData> paul = instance.m_pins;

					//instance.ShowPointOnMap(__instance.transform.position);
					try
					{
						//PortalName = HandlePortalClick(); //This is making minimap instance correctly
					}
					catch { }
					//List<Minimap.PinData> paul = HoldPins;
					foreach (Minimap.PinData pin in paul)
					{
						PName = null;
						try
						{	
							if ( pin.m_icon.name == "TargetPortalIcon") // only selects correct icon now
							{

								PName = pin.m_name; // icons name - Portalname

								colorint = CrystalandKeyLogicColor(PName); // kindof expensive task to do this cpu wize for all portals
								switch (colorint)
								{
									case 0:
										pin.m_icon = IconBlack;
										break;
									case 1:
										pin.m_icon = IconDefault;
										break;
									case 2:
										pin.m_icon = IconRed;
										break;
									case 3:
										pin.m_icon = IconGreen;
										break;
									case 4:
										pin.m_icon = IconBlue;
										break;
									case 5:
										pin.m_icon = IconPurple;
										break;
									case 6:
										pin.m_icon = IconTan;
										break;
									case 7:
										pin.m_icon = IconGold;
										break;
									case 8:
										pin.m_icon = IconWhite;
										break;
									default:
										pin.m_icon = IconDefault;
										break;
								}
								pin.m_icon.name = "TargetPortalIcon"; // test after 2.4
							}
						} catch { }
					}// foreach
				}

			}
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
						__instance.Message(MessageHud.MessageType.Center, "$rmp_youarenotcreator");
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
						__instance.Message(MessageHud.MessageType.Center, "$rmp_needlvl " + CraftingStationlvl + " " + name + " $rmp_forplacement");
						piecehaslvl = false;
						return false;
					}
				}
				return true;
			}
					
        }

		[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
		private static class ZrouteMethodsClientRMP

		{
			private static void Prefix()
			{
				ZRoutedRpc.instance.Register("RequestServerAnnouncementRMP", new Action<long, ZPackage>(RPC_RequestServerAnnouncementRMP)); // Our Server Handler
				//ZRoutedRpc.instance.Register("EventServerAnnouncementRMP", new Action<long, ZPackage>(RPC_EventServerAnnouncementRMP)); // event handler
			}
		}

		[HarmonyPatch(typeof(ZNet), "Shutdown")]
		private class PatchZNetDisconnect
		{
			private static bool Prefix()
			{
				RareMagicPortal.LogInfo("Logoff? Save text file, don't delete");
				((MonoBehaviour)(object)context).StopCoroutine(RemovedDestroyedTeleportWorldsCoroutine());


				NoMoreLoading = true;
				return true;
			}
		}


		[HarmonyPatch(typeof(ZNet), "OnDestroy")]
		private class PatchZNetDestory
		{
			private static void Postfix()
			{ // The Server send once last config sync before destory, but after Shutdown which messes stuff up. 
				NoMoreLoading = false;
			}
		}

		private void Awake()
		{
			CreateConfigValues();
			ReadAndWriteConfigValues();
			Localizer.Load();
			english = new Localization();
			english.SetupLanguage("English");
			LoadAssets();

			context = this;

			assetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), typeof(MagicPortalFluid).Namespace);
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), (string)null);

			SetupWatcher();
			setupYMLFolderWatcher();
			PortalDrink();

			YMLPortalData.ValueChanged += CustomSyncEventDetected;

			StartCoroutine(RemovedDestroyedTeleportWorldsCoroutine());
			
			IconColors();


			RareMagicPortal.LogInfo($"MagicPortalFluid has loaded start assets");
			

		}
		private void IconColors()
        {
			Texture2D tex = IconColor.loadTexture("portal.png");
			Texture2D temp = IconColor.loadTexture("portaliconTarget.png");
			IconDefault = IconColor.CreateSprite(temp, false);
			//Color Gold = new Color(1f, 215f / 255f, 0, 1f);
			IconColor.setTint(Color.black);
			IconBlack = IconColor.CreateSprite(tex, true);
			IconColor.setTint(Color.yellow);
			IconYellow = IconColor.CreateSprite(tex, true);
			IconColor.setTint(Color.red);
			IconRed = IconColor.CreateSprite(tex, true);
			IconColor.setTint(Color.green);
			IconGreen = IconColor.CreateSprite(tex, true);
			IconColor.setTint(Color.blue);
			IconBlue = IconColor.CreateSprite(tex, true);
			IconColor.setTint(Gold);
			IconGold = IconColor.CreateSprite(tex, true);
			IconColor.setTint(Color.white);
			IconWhite = IconColor.CreateSprite(tex, true);
			IconColor.setTint(Purple);
			IconPurple = IconColor.CreateSprite(tex, true);
			IconColor.setTint(Brown);
			IconTan = IconColor.CreateSprite(tex, true);

		}

		private static void LoadIN()
        {
			LoggingOntoServerFirst = true;
			setupYMLFile();
			ReadYMLValuesBoring();
		}


		// end startup

		private void PortalDrink()
        {
			AllowTeleEverything.Name.English("Odin's Portal Blessing");
			AllowTeleEverything.Type = EffectType.Consume;
			AllowTeleEverything.Icon = "portaldrink.png";
			//AllowTeleEverything.IconSprite = portaldrind;
			AllowTeleEverything.Effect.m_startMessageType = MessageHud.MessageType.Center;
			AllowTeleEverything.Effect.m_startMessage = "$rmp_startmessage";
			AllowTeleEverything.Effect.m_stopMessageType = MessageHud.MessageType.Center; // Specify where the stop effect message shows
			AllowTeleEverything.Effect.m_stopMessage = "$rmp_stopmessage";
			AllowTeleEverything.Effect.m_tooltip = "$rmp_tooltip_drink";
			//AllowTeleEverything.Effect.m_stopEffects = 
			AllowTeleEverything.Effect.m_ttl = DrinkDuration; // 2min
			AllowTeleEverything.Effect.m_time = 0f;// starts at 0 
			AllowTeleEverything.Effect.m_flashIcon = true;
			//AllowTeleEverything.Effect.m_cooldown = DrinkDuration;
			AllowTeleEverything.Effect.IsDone();// well be true if done
			AllowTeleEverything.AddSEToPrefab(AllowTeleEverything, "PortalDrink");
				
		}

		private void LoadAssets()
		{
			Item portalmagicfluid = new("portalmagicfluid", "portalmagicfluid", "assetsEmbedded");
			portalmagicfluid.Name.English("Magical Portal Fluid");
			portalmagicfluid.Description.English("Once a mythical essence, now made real with Odin's blessing");

			PortalFluidname = portalmagicfluid.Prefab.name;

			Item PortalDrink = new("portalmagicfluid", "PortalDrink", "assetsEmbedded");
			PortalDrink.Name.English("Magical Portal Drink");
			PortalDrink.Description.English("Odin's Blood of Teleportation");

			Item PortalCrystalMaster = new("portalcrystal", "PortalCrystalMaster", "assetsEmbedded");
			PortalCrystalMaster.Name.English("Gold Portal Crystal");
			PortalCrystalMaster.Description.English("Odin's Golden Crystal allows for Golden Portal Traveling and maybe more Portals");

			Item PortalCrystalRed = new("portalcrystal", "PortalCrystalRed", "assetsEmbedded");
			PortalCrystalRed.Name.English("Red Portal Crystal");
			PortalCrystalRed.Description.English("Odin's Traveling Crystals allow for Red Portal Traveling");

			Item PortalCrystalGreen = new("portalcrystal", "PortalCrystalGreen", "assetsEmbedded");
			PortalCrystalGreen.Name.English("Green Portal Crystal");
			PortalCrystalGreen.Description.English("Odin's Traveling Crystals allow for Green Portal Traveling");

			Item PortalCrystalBlue = new("portalcrystal", "PortalCrystalBlue", "assetsEmbedded");
			PortalCrystalBlue.Name.English("Blue Portal Crystal");
			PortalCrystalBlue.Description.English("Odin's Traveling Crystals allow for Blue Portal Traveling");

			Item PortalCrystalPurple = new("portalcrystal", "PortalCrystalPurple", "assetsEmbedded");
			PortalCrystalPurple.Name.English("Purple Portal Crystal");
			PortalCrystalPurple.Description.English("Odin's Traveling Crystals allow for Purple Portal Traveling");

			Item PortalCrystalTan = new("portalcrystal", "PortalCrystalTan", "assetsEmbedded");
			PortalCrystalTan.Name.English("Tan Portal Crystal");
			PortalCrystalTan.Description.English("Odin's Traveling Crystals allow for Tan Portal Traveling");

			Item PortalKeyRed = new("portalcrystal", "PortalKeyRed", "assetsEmbedded");
			PortalKeyRed.Name.English("Red Portal Key");
			PortalKeyRed.Description.English("Unlock Portals Requiring The Red Key");

			Item PortalKeyGold = new("portalcrystal", "PortalKeyGold", "assetsEmbedded");
			PortalKeyGold.Name.English("Gold Portal Key");
			PortalKeyGold.Description.English("Unlock Gold Portals and perhaps more Portals");

			Item PortalKeyBlue = new("portalcrystal", "PortalKeyBlue", "assetsEmbedded");
			PortalKeyBlue.Name.English("Blue Portal Key");
			PortalKeyBlue.Description.English("Unlock Portals Requiring The Blue Key");

			Item PortalKeyGreen = new("portalcrystal", "PortalKeyGreen", "assetsEmbedded");
			PortalKeyGreen.Name.English("Green Portal Key");
			PortalKeyGreen.Description.English("Unlock Portals Requiring The Green Key");

			Item PortalKeyPurple = new("portalcrystal", "PortalKeyPurple", "assetsEmbedded");
			PortalKeyPurple.Name.English("Purple Portal Key");
			PortalKeyPurple.Description.English("Unlock Portals Requiring The Purple Key");

			Item PortalKeyTan = new("portalcrystal", "PortalKeyTan", "assetsEmbedded");
			PortalKeyTan.Name.English("Tan Portal Key");
			PortalKeyTan.Description.English("Unlock Portals Requiring The Tan Key");




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
			Worldname = ZNet.instance.GetWorldName();
			RareMagicPortal.LogInfo("WorldName " + Worldname);
			YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");
			GetAllMaterials();

			if (!File.Exists(YMLCurrentFile))
			{
				PortalN = new PortalName()  // kind of iffy in inside this
				{
				Portals = new Dictionary<string, PortalName.Portal>
					{
						{"Demo_Portal_Name", new PortalName.Portal() {
							//Crystal_Cost_Master = 3,
						}},

					}
				};
				PortalN.Portals["Demo_Portal_Name"].AdditionalProhibitItems.Add("Stone");
				PortalN.Portals["Demo_Portal_Name"].AdditionalProhibitItems.Add("Wood");

				var serializer = new SerializerBuilder()
					.Build();
				var yaml = serializer.Serialize(PortalN);
				WelcomeString = WelcomeString + Environment.NewLine;

				File.WriteAllText(YMLCurrentFile, WelcomeString + yaml); //overwrites
				RareMagicPortal.LogInfo("Creating Portal_Name file " + Worldname);
				JustWrote = 2;
			}
		}
		private void CustomSyncEventDetected()
        {
			//Worldname = ZNet.instance.GetWorldName();
			if (String.IsNullOrEmpty(ZNet.instance.GetWorldName()))
				JustWait = true;
			else JustWait = false;

			if (!JustWait && !NoMoreLoading)
			{
				YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");
				if (LoggingOntoServerFirst)
				{
					RareMagicPortal.LogInfo("You are now connected to Server World" + Worldname);
					LoggingOntoServerFirst = false;
				}

				isLocal = false;

				string SyncedString = YMLPortalData.Value;

				if (EnableExtraYMLLog)
					RareMagicPortal.LogInfo(SyncedString);

				var deserializer = new DeserializerBuilder()
					.Build();

				PortalN.Portals.Clear();
				PortalN = deserializer.Deserialize<PortalName>(SyncedString);
				if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
				{
					//RareMagicPortal.LogInfo("Server Portal UPdates Are being Saved " + Worldname);
					//File.WriteAllText(YMLCurrentFile, SyncedString);
				}
				JustWrote = 2;

			}

		}
		IEnumerator WaitforReadWrote()
        {
			yield return new WaitForSeconds(10f);

			JustWrote = 0; // lets manual update happen no problem
			//StopCoroutine(WaitforReadWrote()); not needed
		}

		private void ReadYMLValues(object sender, FileSystemEventArgs e) // Thx Azumatt // This gets hit after writing
        {
			if (!File.Exists(YMLCurrentFile)) return;
			if (isAdmin && JustWrote == 0) // if local admin or ServerSync admin
			{
				var yml = File.ReadAllText(YMLCurrentFile);

				var deserializer = new DeserializerBuilder()
					.Build();

				PortalN.Portals.Clear();
				PortalN = deserializer.Deserialize<PortalName>(yml);
				if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
				{
					RareMagicPortal.LogInfo("Server Portal YML Manual UPdate " + Worldname);
					YMLPortalData.Value = yml; 
				} else
                {
					RareMagicPortal.LogInfo("Client Admin Manual YML UPdate " + Worldname);
					if (EnableExtraYMLLog)
						RareMagicPortal.LogInfo(yml);
				}

			}
			if (JustWrote == 2)
				JustWrote = 0;

			if (JustWrote == 1)
			{
				JustWrote = 2;
				StartCoroutine(WaitforReadWrote());
			}

			

			if (!isAdmin)
			{
				//RareMagicPortal.LogInfo("Portal Values Didn't change because you are not an admin");

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
			if (JustWait)
			{
				Worldname = ZNet.instance.GetWorldName();
				JustWait = false;
				YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");
				if (LoggingOntoServerFirst)
				{
					RareMagicPortal.LogInfo("You are now connected to Server World" + Worldname);
					LoggingOntoServerFirst = false;
				}

				isLocal = false;
				if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
				{
					//isDedServer = true;
				}
				string SyncedString = YMLPortalData.Value;

				if (EnableExtraYMLLog)
					RareMagicPortal.LogInfo(SyncedString); 

				var deserializer2 = new DeserializerBuilder()
					.Build();

				//PortalN.Portals.Clear();
				PortalN = deserializer2.Deserialize<PortalName>(SyncedString);
				JustWrote = 2;
				File.WriteAllText(YMLCurrentFile, WelcomeString + SyncedString); //overwrites

			}
			else
			{
				if (!File.Exists(YMLCurrentFile)) return;
				var yml = File.ReadAllText(YMLCurrentFile);

				var deserializer = new DeserializerBuilder()
					.Build();
				//PortalN.Portals.Clear();
				PortalN = new PortalName(); // init
				PortalN = deserializer.Deserialize<PortalName>(yml);
				if (EnableExtraYMLLog)
					RareMagicPortal.LogInfo(yml);

				if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
				{
					YMLPortalData.Value = yml; // should only be one time and for server
				}
			}

			}

		private void ReadConfigValues(object sender, FileSystemEventArgs e) // Thx Azumatt
        {
            if (!File.Exists(ConfigFileFullPath)) return;


			bool admin = ConfigSync.IsAdmin;
			bool locked = ConfigSync.IsLocked;
			bool islocaladmin = false;
			bool isdedServer = false;
			if (ZNet.instance.IsServer() && !ZNet.instance.IsDedicated())
				islocaladmin = true;
			if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
				isdedServer = true;


			if (admin || islocaladmin || isdedServer) // Server Sync Admin Only
			{
				isAdmin = admin; // need to check this
				try
				{
					if (islocaladmin)
                    {
						isAdmin = true;
						RareMagicPortal.LogInfo("ReadConfigValues loaded- you are a local admin");
						Config.Reload();
						ReadAndWriteConfigValues();
						PortalChanger();
					} else if (isdedServer)
                    {
						RareMagicPortal.LogInfo("ReadConfigValues loaded- you the dedicated Server");
						Config.Reload();
						ReadAndWriteConfigValues();
						PortalChanger();
					}
					else if (ConfigSync.IsSourceOfTruth)
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
				isAdmin = false;
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
						if (EnablePortalJuice) { // make this more dynamic
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
					if (EnablePortalJuice)
						petercomponent.m_resources = requirements.ToArray(); // only updates if true

				//RareMagicPortal.LogInfo($"There changing fluid value {PortalFluidname}");
				ObjectDB.instance.GetItemPrefab(PortalFluidname).GetComponent<ItemDrop>().m_itemData.m_shared.m_value = MagicPortalFluidValue;

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

		public static IEnumerator DelayedLoad()
		{
			yield return new WaitForSeconds(0.3f);
			LoadAllRecipeData(reload: true);
			//yield break;

			// I need to keep checking until the world name is populated- probably at respawn
			while (String.IsNullOrEmpty(ZNet.instance.GetWorldName()))
            {
				yield return new WaitForSeconds(1);
			}
			LoadIN();
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

			ConfigEnableYMLLogs = config("General", "YMLPortalLogs", false, "Show YML Portal Logs after Every update", false);

			// Add server config which gets pushed to all clients connecting and can only be edited by admins
			// In local/single player games the player is always considered the admin

			ConfigFluid = config("PortalFluid", "EnablePortalFluid", false,
							"Enable PortalFluid requirement?");

			ConfigSpawn = config("PortalFluid", "PortalMagicFluidSpawn", 3,
				"How much PortalMagicFluid to start with on a new character?");

			ConfigFluidValue = config("PortalFluid", "PortalFluidValue", 0, "What is the value of MagicPortalFluid? " + System.Environment.NewLine + "A Value of 1 or more, makes the item saleable to trader");

			ConfigTable = config("Portal Config", "CraftingStation_Requirement", DefaultTable,
				"Which CraftingStation is required nearby?" + System.Environment.NewLine + "Default is Workbench = $piece_workbench, forge = $piece_forge, Artisan station = $piece_artisanstation "  + System.Environment.NewLine + "Pick a valid table otherwise default is workbench"); // $piece_workbench , $piece_forge , $piece_artisanstation

			ConfigTableLvl = config("Portal Config", "Level_of_CraftingStation_Req", 1,
				"What level of CraftingStation is required for placing Portal?");

			ConfigCreator = config("Portal Config", "OnlyCreatorCanDeconstruct", true, "Only the Creator of the Portal can deconstruct it. It can still be destoryed");

			ConfiglHealth = config("Portal Config", "Portal_Health", 400f, "Health of Portal");

			ConfigAddRestricted = config("Portal Config", "Portal_D_Restrict", "", "Additional Items to Restrict by Default - 'Wood,Stone'");

			ConfigCreatorLock = config("Portal Config", "OnlyCreatorCanChange", false, "Only Creator can change Portal name");

			ConfigEnableCrystals = config("Portal Crystals", "Enable Portal Crystals and Keys", false, "Enable Portal Crystals and Keys");

			ConfigEnableGoldAsMaster = config("Portal Crystals", "USE_GOLD_AS_PORTAL_MASTER", true, "Enabled Gold Key and Crystal as Master Key to all (Red,Green,Blue,Purple,Tan,Gold)");

			//ConfigEnableColorEnable = config("Portal Crystals", "ColorsEnabled", "Red,Green,Blue,Purple,Gold", "Whether anyone can use these Colored Portals, even if they have the requirements-  Red,Green,Blue,Purple,Gold");

			//ConfigEnableKeys = config("Portal Keys", "Portal_Keys_Enable", false, "Enable Portal Crystals");

			ConfigCrystalsConsumable = config("Portal Crystals", "Crystal_Consume_Default", 1, "What is the Default number of crystals to consume for each New Portal? - Depending on the default Color, all other colors will be 0 (no access)" + System.Environment.NewLine + " Gold/Master gets set to this regardless of Default Color" + System.Environment.NewLine + " 0 - Means that passage is denied for this color");

			//ConfigAdminOnly = config("Portal Config", "Only_Admin_Can_Build", false, "Only The Admins Can Build Portals");

			CrystalKeyDefaultColor = config("Portal Crystals", "Portal_Crystal_Color_Default", "Red", "Default Color for New Portals? " + System.Environment.NewLine + "Red,Green,Blue,Purple,Tan,None" + System.Environment.NewLine + " None - will set Portals to Free Passage by default");

			ConfigMessageLeft = config("Portal Crystals", "UseTopLeftMessage", false, "In case a mod is interfering with Center Messages for Portal tags, display on TopLeft instead.");

			PortalDrinkTimer = config("Portal Drink", "Portal_drink_timer", 120, "How Long Odin's Drink lasts");

			

			portalRMPKEY = config("Portal Config", "Modifier key for toggle", new KeyboardShortcut(KeyCode.LeftControl), "Modifier key that has to be pressed while hovering over Portal + E", false);

		}

        private void ReadAndWriteConfigValues()
		{
			//RareMagicPortal.LogInfo("Reached Read and Write");
			EnablePortalJuice = (bool)Config["PortalFluid", "EnablePortalFluid"].BoxedValue;
			PortalMagicFluidSpawn = (int)Config["PortalFluid", "PortalMagicFluidSpawn"].BoxedValue;
			TabletoAddTo = (string)Config["Portal Config", "CraftingStation_Requirement"].BoxedValue;
			CraftingStationlvl = (int)Config["Portal Config", "Level_of_CraftingStation_Req"].BoxedValue;
			CreatorOnly = (bool)Config["Portal Config", "OnlyCreatorCanDeconstruct"].BoxedValue;	
			PortalHealth = (float)Config["Portal Config", "Portal_Health"].BoxedValue;
			CreatorLock = (bool)Config["Portal Config", "OnlyCreatorCanChange"].BoxedValue;
			MagicPortalFluidValue = (int)Config["PortalFluid", "PortalFluidValue"].BoxedValue;
			EnableCrystals = (bool)Config["Portal Crystals", "Enable Portal Crystals and Keys"].BoxedValue;
			//EnableKeys = (bool)Config["Portal Keys", "Portal_Keys_Enable"].BoxedValue;
			DefaultPortalColor = (string)Config["Portal Crystals", "Portal_Crystal_Color_Default"].BoxedValue;
			CrystalsConsumable = (int)Config["Portal Crystals", "Crystal_Consume_Default"].BoxedValue;
			//AdminOnlyBuild = (bool)Config["Portal Config", "Only_Admin_Can_Build"].BoxedValue;
			DrinkDuration = (int)Config["Portal Drink", "Portal_drink_timer"].BoxedValue;
			EnableExtraYMLLog = (bool)Config["General", "YMLPortalLogs"].BoxedValue;
			DefaultRestrictString = (string)Config["Portal Config", "Portal_D_Restrict"].BoxedValue;
			UseGoldASMaster = (bool)Config["Portal Crystals", "USE_GOLD_AS_PORTAL_MASTER"].BoxedValue;
			msgLeft = (bool)Config["Portal Crystals", "UseTopLeftMessage"].BoxedValue;


			//ColorsEnabled = (string)Config["Portal Crystals", "ColorsEnabled"].BoxedValue;
			/*
			string[] colorstosearch = ColorsEnabled.Split(',');
			foreach (var col in enaColor)
            {
				//col.Value = false;
            }
			foreach (string color in colorstosearch)
			{
				enaColor[color] = true;
			}
			*/
			//

			AllowTeleEverything.Effect.m_cooldown = DrinkDuration;

			if (CraftingStationlvl > 10 || CraftingStationlvl < 1)
				CraftingStationlvl = 1;

		}

	private static void WritetoYML(string PortalName, string updateP=null) // this only happens if portal is not in yml file at all
        {

			int colorint = 1; // freepassage yellow = 1
			if (updateP == null)
			{

				PortalName.Portal paulgo = new PortalName.Portal
				{
					//Crystal_Cost_Master = CrystalsConsumable, // If only using master crystals
				};
				PortalN.Portals.Add(PortalName, paulgo); // adds
			}
			if (updateP != null)
            {
				if (updateP == "Purple")
				{
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Purple"] = false;
					colorint = 1;
					RareMagicPortal.LogWarning($"Your PortalName world has Portals that don't Contain Purple, this portal {PortalName} is being updated to default");
				}
				if (updateP == "Tan")
				{
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Tan"] = false;
					colorint = 1;
					RareMagicPortal.LogWarning($"Your PortalName world has Portals that don't Contain Tan, this portal {PortalName} is being updated to default");
				}
				else
				{
					return;
				}
			}

			if (DefaultPortalColor == "Red")
			{
				PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = CrystalsConsumable;
				PortalN.Portals[PortalName].Portal_Key["Red"] = true;
				colorint = 2;

			}
			else
			{
				PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = 0;
				PortalN.Portals[PortalName].Portal_Key["Red"] = false;
			}
			if (DefaultPortalColor == "Green")
			{
				PortalN.Portals[PortalName].Portal_Crystal_Cost["Green"] = CrystalsConsumable;
				PortalN.Portals[PortalName].Portal_Key["Green"] = true;
				colorint = 3;
			}
			if (DefaultPortalColor == "Blue")
			{
				PortalN.Portals[PortalName].Portal_Crystal_Cost["Blue"] = CrystalsConsumable;
				PortalN.Portals[PortalName].Portal_Key["Blue"] = true;
				colorint = 4;
			}

			if (DefaultPortalColor == "Purple")
			{
				PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = CrystalsConsumable;
				PortalN.Portals[PortalName].Portal_Key["Purple"] = true;
				colorint = 5;
			}

			if (DefaultPortalColor == "Tan")
			{
				PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = CrystalsConsumable;
				PortalN.Portals[PortalName].Portal_Key["Tan"] = true;
				colorint = 6;
			}

			if (DefaultPortalColor == "None")
			{
				PortalN.Portals[PortalName].Free_Passage = true;
				colorint = 1;
			}
			else
			{
				PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = CrystalsConsumable; // by default always unless true
			}

			if (DefaultRestrictString != "")
				PortalN.Portals[PortalName].AdditionalProhibitItems = DefaultRestrictString.Split(',').ToList(); // one time
		

				if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())// only for server 
				{
					RareMagicPortal.LogInfo("You are a dedicated Server");
					var serializer = new SerializerBuilder()
					.Build();
					var yamlfull = WelcomeString + Environment.NewLine + serializer.Serialize(PortalN); // build everytime

					JustWrote = 1;
					File.WriteAllText(YMLCurrentFile, yamlfull); //overwrite
					string lines = "";
					foreach (string line in System.IO.File.ReadLines(YMLCurrentFile)) // rethrough lines manually and add spaces, stupid
					{
						lines += line + Environment.NewLine;
						if (line.Contains("Admin_only_Access")) // three spaces for non main objects
						{ lines += Environment.NewLine; }
					}
					File.WriteAllText(YMLCurrentFile, lines); //overwrite with extra goodies
					JustWrote = 2;                                     
					YMLPortalData.Value = yamlfull; // send out to clients from server only
				}
				else
				{
					if (!ZNet.instance.IsServer())
					{
						RareMagicPortal.LogInfo("You are connect to a Server");
						ServerZDOymlUpdate(colorint, PortalName);// send to server to update and push yml
					}
					else // single client only or Server but not dedicated
					{
						RareMagicPortal.LogInfo("Single client only or Server but not dedicated");
						var serializer = new SerializerBuilder()
						.Build();
						var yamlfull = WelcomeString + Environment.NewLine + serializer.Serialize(PortalN); // build everytime

						JustWrote = 1;
						File.WriteAllText(YMLCurrentFile, yamlfull); //overwrite
						string lines = "";
						foreach (string line in System.IO.File.ReadLines(YMLCurrentFile)) // rethrough lines manually and add spaces, stupid
						{
							lines += line + Environment.NewLine;
							if (line.Contains("Admin_only_Access")) // three spaces for non main objects
							{ lines += Environment.NewLine; }
						}
						File.WriteAllText(YMLCurrentFile, lines); //overwrite with extra goodies
						if (EnableExtraYMLLog)
							RareMagicPortal.LogInfo(yamlfull);
						JustWrote = 2;
					}
				}

		}
		private static void HandleTeleport(Minimap Instancpass) // this is just for testing
        {

			Minimap instance = Instancpass;
			List<Minimap.PinData> paul = instance.m_pins;
			Vector3 pos = instance.ScreenToWorldPoint(Input.mousePosition);
			float radius = instance.m_removeRadius * (instance.m_largeZoom * 2f);

			Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);

			instance.SetMapMode(Minimap.MapMode.Small);

			Player.m_localPlayer.TeleportTo(pos + rotation * Vector3.forward + Vector3.up, rotation, true);
		}
		private static string HandlePortalClick()
        {
			Minimap instance = Minimap.instance;
			List<Minimap.PinData> paul = instance.m_pins;
			Vector3 pos = instance.ScreenToWorldPoint(Input.mousePosition);
			float radius = instance.m_removeRadius * (instance.m_largeZoom * 2f);

			checkiftagisPortal = "";
			Minimap.PinData  pinData = null;
			float num = 999999f;
			foreach (Minimap.PinData pin in paul)
			{
				//pin.m_save = true;
				float num2 = Utils.DistanceXZ(pos, pin.m_pos);
				if (num2 < radius && (num2 < num || pinData == null))
				{
					pinData = pin;
					num = num2;
					//pin.m_save = true;
				}
			}
			if (!string.IsNullOrEmpty(pinData.m_name))
				checkiftagisPortal = pinData.m_name; // icons name
			if (pinData.m_icon.name == "" || pinData.m_icon.name == "TargetPortalIcon") // only targetPortalIcon now, or not maybe new icons being set don't have name after messing with them
			{ // TargetPortals Icons have no name therefore this stupid check weeds out regular icons  // pull request for icon.name  TargetPortalIcon
			}
			else 
				checkiftagisPortal = null;

			if (checkiftagisPortal.Contains("$hud") || checkiftagisPortal.Contains("Day "))
				checkiftagisPortal = null;

			return checkiftagisPortal;
		}

		private static int CrystalandKeyLogicColor(string PortalName)
		{
			// 0 is black/admin only
			// 1 is normal // free passage
			// 2 red
			// 3 green
			// 4 blue
			// 5 Purple
			// 6 gold 
			// 7 white (Only allow free passage with PortalDrink or enablecrystals)

			int CrystalForPortal = CrystalsConsumable;
			bool OdinsKin = false;
			bool Free_Passage = false;

			if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
			{
				WritetoYML(PortalName);
			}
			OdinsKin = PortalN.Portals[PortalName].Admin_only_Access;
			Free_Passage = PortalN.Portals[PortalName].Free_Passage;
			var Portal_Crystal_Cost = PortalN.Portals[PortalName].Portal_Crystal_Cost; // rgbG  // 0 means it can't be used, (Keys only) anything greater means the cost. -1 means same as 0
			var Portal_Key = PortalN.Portals[PortalName].Portal_Key; // rgbG

			if (OdinsKin ) 
			{
				return 0;
			}

			if (Free_Passage)
				return 1;

			if (PortalN.Portals[PortalName].TeleportAnything ) 
			{
				//CycleWhite = false;
				return 8;
			}

			if (Portal_Crystal_Cost["Red"] > 0 || Portal_Key["Red"])
				return 2;

			if ((Portal_Crystal_Cost["Green"] > 0 || Portal_Key["Green"]))
				return 3;

			if ((Portal_Crystal_Cost["Blue"] > 0 || Portal_Key["Blue"]))
				return 4;
			try
			{
				if ((Portal_Crystal_Cost["Purple"] > 0 || Portal_Key["Purple"]))
					return 5;
			} catch {
				RareMagicPortal.LogInfo("No Purple in File for Portal, adding one");
				WritetoYML(PortalName, "Purple");
			}
			try
			{
				if ((Portal_Crystal_Cost["Tan"] > 0 || Portal_Key["Tan"]))
					return 6;
			}
			catch
			{
				RareMagicPortal.LogInfo("No Tan in File for Portal, adding one");
				WritetoYML(PortalName, "Tan");
			}

			//if ((Portal_Crystal_Cost["Gold"] > 0 || Portal_Key["Gold"]) && !PortalN.Portals[PortalName].TeleportAnything && EnableCrystals || (Portal_Crystal_Cost["Gold"] > 0 && Portal_Key["Gold"]) && !PortalN.Portals[PortalName].TeleportAnything)
			if ((Portal_Crystal_Cost["Gold"] > 0 || Portal_Key["Gold"]))
			{
				return 7;
			}
			//if ((Portal_Crystal_Cost["Gold"] == 0 && Portal_Key["Gold"]) && !EnableCrystals)
			if (PortalN.Portals[PortalName].TeleportAnything)
				return 8;

			return 0;


		}
		private static void updateYmltoColorChange(string PortalName, int colorint)
        {
			if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
			{
				WritetoYML(PortalName);
			}

			// now just do a straight update on switch results

			string currentcolor = "Default"; // for reference only

			switch (colorint)
			{
				case 0:
					currentcolor = "Black";
					if (EnableCrystals)
					{
						PortalN.Portals[PortalName].Admin_only_Access = true;
						PortalN.Portals[PortalName].TeleportAnything = true; // I guess
						
					}
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Gold"] = false;
					PortalN.Portals[PortalName].TeleportAnything = false;

					break;
				case 1:
					currentcolor = "Yellow";
					PortalN.Portals[PortalName].Free_Passage = true;
					PortalN.Portals[PortalName].Admin_only_Access = false;
					PortalN.Portals[PortalName].TeleportAnything = false;

					PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = 0; // making sure defautls are set
					PortalN.Portals[PortalName].Portal_Key["Purple"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Tan"] = false;

					break;
				case 2:
					currentcolor = "Red";
					PortalN.Portals[PortalName].Free_Passage = false;
					PortalN.Portals[PortalName].Admin_only_Access = false ;

					PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = 1;
					PortalN.Portals[PortalName].Portal_Key["Red"] = true;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Green"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Green"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Blue"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Blue"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Purple"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Tan"] = false;
					if (UseGoldASMaster)
					{
						PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = 1;
						PortalN.Portals[PortalName].Portal_Key["Gold"] = true;
					}

					break;
				case 3:
					currentcolor = "Green";
					PortalN.Portals[PortalName].Free_Passage = false;
					PortalN.Portals[PortalName].Admin_only_Access = false;

					PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Red"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Green"] = 1;
					PortalN.Portals[PortalName].Portal_Key["Green"] = true;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Blue"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Blue"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Purple"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Tan"] = false;
					if (UseGoldASMaster)
					{
						PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = 1;
						PortalN.Portals[PortalName].Portal_Key["Gold"] = true;
					}

					break;
				case 4:
					currentcolor = "Blue";
					PortalN.Portals[PortalName].Free_Passage = false;
					PortalN.Portals[PortalName].Admin_only_Access = false;

					PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Red"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Green"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Green"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Blue"] = 1;
					PortalN.Portals[PortalName].Portal_Key["Blue"] = true;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Purple"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Tan"] = false;
					if (UseGoldASMaster)
					{
						PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = 1;
						PortalN.Portals[PortalName].Portal_Key["Gold"] = true;
					}

					break;

				case 5:
					currentcolor = "Purple";
					PortalN.Portals[PortalName].Free_Passage = false;
					PortalN.Portals[PortalName].Admin_only_Access = false;

					PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Red"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Green"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Green"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Blue"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Blue"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = 1;
					PortalN.Portals[PortalName].Portal_Key["Purple"] = true;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Tan"] = false;
					if (UseGoldASMaster)
					{
						PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = 1;
						PortalN.Portals[PortalName].Portal_Key["Gold"] = true;
					}

					break;

				case 6:
					currentcolor = "Tan";
					PortalN.Portals[PortalName].Free_Passage = false;
					PortalN.Portals[PortalName].Admin_only_Access = false;

					PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Red"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Green"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Green"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Blue"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Blue"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Purple"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = 1;
					PortalN.Portals[PortalName].Portal_Key["Tan"] = true;
					if (UseGoldASMaster)
					{
						PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = 1;
						PortalN.Portals[PortalName].Portal_Key["Gold"] = true;
					}

					break;
				case 7:
					currentcolor = "Gold";
					PortalN.Portals[PortalName].Free_Passage = false;
					PortalN.Portals[PortalName].Admin_only_Access = false;

					PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Red"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Green"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Green"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Blue"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Blue"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Purple"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = 0;
					PortalN.Portals[PortalName].Portal_Key["Tan"] = false;
					PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = 1;
					PortalN.Portals[PortalName].Portal_Key["Gold"] = true;


					break;
				case 8:
					currentcolor = "White"; // only use for freee trans
					{ 
						PortalN.Portals[PortalName].TeleportAnything = true;
						PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = 0;
						PortalN.Portals[PortalName].Portal_Key["Gold"] = false;// 
					}
					// Only free passage if using enablecrystal is on. Otherwise just white color or PortalDrink


					break;
				default:
					currentcolor = "Yellow";
					PortalN.Portals[PortalName].Free_Passage = true;
					PortalN.Portals[PortalName].Admin_only_Access = false;
					PortalN.Portals[PortalName].TeleportAnything = false;
					break;

			}


			if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())// only for server 
			{
				RareMagicPortal.LogInfo("You are a dedicated Server");
				var serializer = new SerializerBuilder()
				.Build();
				var yamlfull = WelcomeString + Environment.NewLine + serializer.Serialize(PortalN); // build everytime

				JustWrote = 1;
				File.WriteAllText(YMLCurrentFile, yamlfull); //overwrite
				string lines = "";
				foreach (string line in System.IO.File.ReadLines(YMLCurrentFile)) // rethrough lines manually and add spaces, stupid
				{
					lines += line + Environment.NewLine;
					if (line.Contains("Admin_only_Access")) // three spaces for non main objects
					{ lines += Environment.NewLine; }
				}
				File.WriteAllText(YMLCurrentFile, lines); //overwrite with extra goodies
				JustWrote = 2;
				YMLPortalData.Value = yamlfull; // send out to clients from server only
			}
			else
			{
				if (!ZNet.instance.IsServer())
				{
					RareMagicPortal.LogInfo("You are connect to a Server");
					ServerZDOymlUpdate(colorint, PortalName);// send to server to update and push yml
				}
				else // single client only or Server but not dedicated
				{
					RareMagicPortal.LogInfo("Single client only or Server but not dedicated");
					var serializer = new SerializerBuilder()
					.Build();
					var yamlfull = WelcomeString + Environment.NewLine + serializer.Serialize(PortalN); // build everytime

					JustWrote = 1;
					File.WriteAllText(YMLCurrentFile, yamlfull); //overwrite
					string lines = "";
					foreach (string line in System.IO.File.ReadLines(YMLCurrentFile)) // rethrough lines manually and add spaces, stupid
					{
						lines += line + Environment.NewLine;
						if (line.Contains("Admin_only_Access")) // three spaces for non main objects
						{ lines += Environment.NewLine; }
					}
					File.WriteAllText(YMLCurrentFile, lines); //overwrite with extra goodies
					if (EnableExtraYMLLog)
						RareMagicPortal.LogInfo(yamlfull);
					JustWrote = 2;
				}
			}

		}

		private static bool CrystalandKeyLogic(string PortalName)
        {
			int CrystalForPortal = CrystalsConsumable;
			bool OdinsKin = false;
			bool Free_Passage = false;
			//Dictionary <string, int> Portal_Crystal_Cost = null; // rgbG
			//Dictionary <string, bool> Portal_Key; //rgbG

			RareMagicPortal.LogInfo($"Portal name is {PortalName}");
			if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
			{
				WritetoYML(PortalName);
			}


			//CrystalForPortal = PortalN.Portals[PortalName].Crystal_Cost_Master;
			OdinsKin = PortalN.Portals[PortalName].Admin_only_Access;
			Free_Passage = PortalN.Portals[PortalName].Free_Passage;
			var Portal_Crystal_Cost = PortalN.Portals[PortalName].Portal_Crystal_Cost; // rgbG  // 0 means it can't be used, (Keys only) anything greater means the cost. -1 means same as 0
			var Portal_Key = PortalN.Portals[PortalName].Portal_Key; // rgbG
			var TeleportEvery = PortalN.Portals[PortalName].TeleportAnything;
			// the admin can customize crystal cost or key usage, but master crystal and golden key always are automatic unless set to admin

			try// keep this for a while for people upgrading
			{
				if ((Portal_Crystal_Cost["Purple"] > 0 || Portal_Key["Purple"])) { } 
	
			}
			catch
			{
				RareMagicPortal.LogInfo("No Purple in File for Portal, adding one in CrystalKeyLogic");
				WritetoYML(PortalName, "Purple");
			}
			try// keep this for a while for people upgrading
			{
				if ((Portal_Crystal_Cost["Tan"] > 0 || Portal_Key["Tan"])) { }

			}
			catch
			{
				RareMagicPortal.LogInfo("No Tan in File for Portal, adding one in CrystalKeyLogic");
				WritetoYML(PortalName, "Tan");
			}

			//RareMagicPortal.LogInfo($"Portal IS ADMIN {OdinsKin} and is player admin? {isAdmin}");
			if (OdinsKin && isAdmin)
			{
				player.Message(MessageHud.MessageType.TopLeft, "$rmp_kin_welcome"); // forgot this one
				return true;
			}

			else if (OdinsKin && !isAdmin && EnableCrystals) // If requires admin, but not admin, but only with enable crystals otherwise just a normal portal
			{
				player.Message(MessageHud.MessageType.Center, "$rmp_kin_only");
				//Teleporting = false;
				return false;
				
			}

			if (TeleportEvery) // if no crystals, then just white, if crystals then free passage
			{
				player.Message(MessageHud.MessageType.TopLeft, "$rmp_freepassage");
				if (EnableCrystals)
                {
					player.Message(MessageHud.MessageType.Center, "$rmp_allowsEverything");
				}
				return true;
			}

			if (EnableCrystals)
			{
				if (!player.IsTeleportable())
				{
					player.Message(MessageHud.MessageType.Center, "$msg_noteleport");

					return false;
				}

				if (Free_Passage)
				{
					player.Message(MessageHud.MessageType.TopLeft, "$rmp_freepassage");
					return true;
				}


				int CrystalCountMaster = player.m_inventory.CountItems(CrystalMaster);
				int CrystalCountRed = player.m_inventory.CountItems(CrystalRed);
				int CrystalCountGreen = player.m_inventory.CountItems(CrystalGreen);
				int CrystalCountBlue = player.m_inventory.CountItems(CrystalBlue);
				int CrystalCountPurple = player.m_inventory.CountItems(CrystalPurple);
				int CrystalCountTan = player.m_inventory.CountItems(CrystalTan);

				int KeyCountGold = player.m_inventory.CountItems(PortalKeyGold);
				int KeyCountRed = player.m_inventory.CountItems(PortalKeyRed);
				int KeyCountGreen = player.m_inventory.CountItems(PortalKeyGreen);
				int KeyCountBlue = player.m_inventory.CountItems(PortalKeyBlue);
				int KeyCountPurple = player.m_inventory.CountItems(PortalKeyPurple);
				int KeyCountTan = player.m_inventory.CountItems(PortalKeyTan);

				int flagCarry = 0; // don't have any keys or crystals
				int crystalorkey = 0;// 0 is crystal, 1 is key, 2 is both

				bool foundAccess = false;
				int lowest = 0;

				if (Portal_Crystal_Cost["Red"] > 0 || Portal_Key["Red"])
				{
					if (CrystalCountRed == 0) // has none of required
						flagCarry = 1;
					else if (Portal_Crystal_Cost["Red"] > CrystalCountRed) // has less than required
						flagCarry = 11;
					else flagCarry = 21; // has more than required

					if (Portal_Key["Red"]){
						if (Portal_Crystal_Cost["Red"] == 0)
                        {
							crystalorkey = 1;
							if (KeyCountRed > 0)
								flagCarry = 111;
							else
								flagCarry = 1; // no crystal cost, but key cost with no key
						}
						else
                        {
							if (KeyCountRed > 0 && flagCarry < 20)
								flagCarry = 111;
							else
								crystalorkey = 2; // yes crystal cost, and key cost with no key, so let user know both is good
						}
					}
				}
				if (flagCarry > 20)
					foundAccess = true;
				if (flagCarry < 20 && lowest == 0)
					lowest = flagCarry;


				if (!foundAccess && (Portal_Crystal_Cost["Green"] > 0 || Portal_Key["Green"]))
				{
					if (CrystalCountGreen == 0) // has none of required
						flagCarry = 2;
					else if (Portal_Crystal_Cost["Green"] > CrystalCountGreen) // has less than required
						flagCarry = 12;
					else flagCarry = 22; // has more than required

					if (Portal_Key["Green"])
					{
						if (Portal_Crystal_Cost["Green"] == 0)
						{
							crystalorkey = 1;
							if (KeyCountGreen > 0)
								flagCarry = 222;
							else
								flagCarry = 2; // no crystal cost, but key cost with no key
						}
						else
						{
							if (KeyCountGreen > 0 && flagCarry < 20)
								flagCarry = 222;
							else
								crystalorkey = 2; // yes crystal cost, and key cost with no key, so let user know both is good
						}
					}
				}
				if (flagCarry > 20)
					foundAccess = true;

				if (flagCarry < 20 && lowest == 0)
					lowest = flagCarry;

				if (!foundAccess && (Portal_Crystal_Cost["Blue"] > 0 || Portal_Key["Blue"]))
				{
					if (CrystalCountBlue == 0) // has none of required
						flagCarry = 3;
					else if (Portal_Crystal_Cost["Blue"] > CrystalCountBlue) // has less than required
						flagCarry = 13;
					else flagCarry = 33; // has more than required

					if (Portal_Key["Blue"])
					{
						if (Portal_Crystal_Cost["Blue"] == 0)
						{
							crystalorkey = 1;
							if (KeyCountBlue > 0)
								flagCarry = 333;
							else
								flagCarry = 3; // no crystal cost, but key cost with no key
						}
						else
						{
							if (KeyCountBlue > 0 && flagCarry < 20)
								flagCarry = 333;
							else
								crystalorkey = 2; // yes crystal cost, and key cost with no key, so let user know both is good
						}
					}
				}
				if (flagCarry > 30)
					foundAccess = true;

				if (flagCarry < 20 && lowest == 0)
					lowest = flagCarry;


				if (!foundAccess && (Portal_Crystal_Cost["Purple"] > 0 || Portal_Key["Purple"]))
				{

					if (CrystalCountPurple == 0) // has none of required
						flagCarry = 4;
					else if (Portal_Crystal_Cost["Purple"] > CrystalCountPurple) // has less than required
						flagCarry = 14;
					else flagCarry = 44; // has more than required

					if (Portal_Key["Purple"])
					{
						if (Portal_Crystal_Cost["Purple"] == 0)
						{
							crystalorkey = 1;
							if (KeyCountPurple > 0)
								flagCarry = 444;
							else
								flagCarry = 4; // no crystal cost, but key cost with no key
						}
						else
						{
							if (KeyCountPurple > 0 && flagCarry < 20)
								flagCarry = 444;
							else
								crystalorkey = 2; // yes crystal cost, and key cost with no key, so let user know both is good
						}
					}
				}

				if (flagCarry > 40)
					foundAccess = true;

				if (flagCarry < 20 && lowest == 0)
					lowest = flagCarry;


				if (!foundAccess && (Portal_Crystal_Cost["Tan"] > 0 || Portal_Key["Tan"]))
				{
					if (CrystalCountTan == 0) // has none of required
						flagCarry = 5;
					else if (Portal_Crystal_Cost["Tan"] > CrystalCountTan) // has less than required
						flagCarry = 15;
					else flagCarry = 55; // has more than required

					if (Portal_Key["Tan"])
					{
						if (Portal_Crystal_Cost["Tan"] == 0)
						{
							crystalorkey = 1;
							if (KeyCountTan > 0)
								flagCarry = 555;
							else
								flagCarry = 5; // no crystal cost, but key cost with no key
						}
						else
						{
							if (KeyCountTan > 0 && flagCarry < 20)
								flagCarry = 555;
							else
								crystalorkey = 2; // yes crystal cost, and key cost with no key, so let user know both is good
						}
					}
				}
				if (flagCarry > 50)
					foundAccess = true;

				if (flagCarry < 20 && lowest == 0)
					lowest = flagCarry;


				if (!foundAccess && (Portal_Crystal_Cost["Gold"] > 0 || Portal_Key["Gold"]))
				{
					if (CrystalCountMaster == 0) // has none of required
						flagCarry = 9;
					else if (Portal_Crystal_Cost["Gold"] > CrystalCountMaster) // has less than required
						flagCarry = 19;
					else flagCarry = 99; // has more than required

					if (Portal_Key["Gold"])
					{
						if (Portal_Crystal_Cost["Gold"] == 0)
						{
							crystalorkey = 1;
							if (KeyCountGold > 0)
								flagCarry = 999;
							else
								flagCarry = 9; // no crystal cost, but key cost with no key
						}
						else
						{
							if (KeyCountGold > 0 && flagCarry < 20)
								flagCarry = 999;
							else
								crystalorkey = 2; // yes crystal cost, and key cost with no key, so let user know both is good
						}
					}
				}
				if (flagCarry < 20 && lowest == 0)
					lowest = flagCarry;

				if (flagCarry < 20 && lowest != 0)
					flagCarry = lowest;

				string CorK = "$rmp_crystals";
				if (crystalorkey == 1)
					CorK = "$rmp_key";
				if (crystalorkey == 2)
					CorK = "$rmp_crystalorkey";


				var hud = MessageHud.MessageType.Center;
				if (msgLeft)
					hud = MessageHud.MessageType.TopLeft;
					
				//Localizer.AddPlaceholder("rmp_no_red_portal", "No Red Portal");
				switch (flagCarry)
				{
					case 1:
						player.Message(hud, $"$rmp_no_red_portal {CorK}");
						return false;
					case 2:
						player.Message(hud, $"$rmp_no_green_portal {CorK}");
						return false;
					case 3:
						player.Message(hud, $"$rmp_no_blue_portal {CorK}");
						return false;
					case 4:
						player.Message(hud, $"$rmp_no_purple_portal {CorK}");
						return false;
					case 5:
						player.Message(hud, $"$rmp_no_tan_portal {CorK}");
						return false;
					case 9:
						player.Message(hud, $"$rmp_no_gold_portal {CorK}");
						return false;
					case 11:
						player.Message(hud, $"{Portal_Crystal_Cost["Red"]} $rmp_required_red {PortalName}");
						return false;
					case 12:
						player.Message(hud, $"{Portal_Crystal_Cost["Green"]} $rmp_required_green {PortalName}");
						return false;
					case 13:
						player.Message(hud, $"{Portal_Crystal_Cost["Blue"]} $rmp_required_blue {PortalName}");
						return false;
					case 14:
						player.Message(hud, $"{Portal_Crystal_Cost["Purple"]} $rmp_required_purple {PortalName}");
						return false;
					case 15:
						player.Message(hud, $"{Portal_Crystal_Cost["Tan"]} $rmp_required_tan {PortalName}");
						return false;
					case 19:
						player.Message(hud, $"{Portal_Crystal_Cost["Gold"]} $rmp_required_gold {PortalName}");
						return false;
					case 21:
						player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Red"]} $rmp_consumed_red");
						player.m_inventory.RemoveItem(CrystalRed, Portal_Crystal_Cost["Red"]);
						return true;
					case 22:
						player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Green"]} $rmp_consumed_green");
						player.m_inventory.RemoveItem(CrystalGreen, Portal_Crystal_Cost["Green"]);
						return true;
					case 33:
						player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Blue"]} $rmp_consumed_blue");
						player.m_inventory.RemoveItem(CrystalBlue, Portal_Crystal_Cost["Blue"]);
						return true;
					case 44:
						player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Purple"]} $rmp_consumed_purple");
						player.m_inventory.RemoveItem(CrystalPurple, Portal_Crystal_Cost["Purple"]);
						return true;
					case 55:
						player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Tan"]} $rmp_consumed_tan");
						player.m_inventory.RemoveItem(CrystalTan, Portal_Crystal_Cost["Tan"]);
						return true;
					case 99:
						player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Gold"]} $rmp_consumed_gold");
						player.m_inventory.RemoveItem(CrystalMaster, Portal_Crystal_Cost["Gold"]);
						return true;

					case 111:
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_redKey_access");
						return true;
					case 222:
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_greenKey_access");
						return true;
					case 333:
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_blueKey_access");
						return true;
					case 444:
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_purpleKey_access");
						return true;
					case 555:
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_tanKey_access");
						return true;
					case 999:
						player.Message(MessageHud.MessageType.TopLeft, $"$rmp_goldKey_access");
						return true;

					default:
						player.Message(hud, $"$rmp_noaccess");
						return false;


				}
			}
			return true;

		}

			static void SetParticleColors(
		   IEnumerable<Light> lights,
		   IEnumerable<ParticleSystem> systems,
		   IEnumerable<ParticleSystemRenderer> renderers,
		   Color targetColor)
			{
				var targetColorGradient = new ParticleSystem.MinMaxGradient(targetColor);

				foreach (ParticleSystem system in systems)
				{
					var colorOverLifetime = system.colorOverLifetime;

					if (colorOverLifetime.enabled)
					{
						colorOverLifetime.color = targetColorGradient;
					}

					var sizeOverLifetime = system.sizeOverLifetime;

					if (sizeOverLifetime.enabled)
					{
						var main = system.main;
						main.startColor = targetColor;
					}
				}

				foreach (ParticleSystemRenderer renderer in renderers)
				{
					renderer.material.color = targetColor;
				}

				foreach (Light light in lights)
				{
					light.color = targetColor;
				}
			}


		static bool TryGetTeleportWorld(TeleportWorld key, out TeleportWorldDataRMP value)
		{
			if (key)
			{
				return _teleportWorldDataCache.TryGetValue(key, out value);
			}

			value = default;
			return false;
		}


		static void SetTeleportWorldColors(TeleportWorldDataRMP teleportWorldData, bool SetcolorTarget=false, bool SetMaterial = false)
		{

			teleportWorldData.OldColor = teleportWorldData.TargetColor;
			//Color Gold = new Color(1f, 215f / 255f, 0, 1f);
			//Color Cyan = Color.cyan

			if (teleportWorldData.TargetColor == Gold)
			{
				try
				{
					Material mat = originalMaterials["shaman_prupleball"];
					foreach (Renderer red in teleportWorldData.MeshRend)
					{
						red.material = mat;
					}
				}
				catch { }
			}
			else if (teleportWorldData.TargetColor == Color.black)
			{
				try
				{
					Material mat = originalMaterials["silver_necklace"];
					foreach (Renderer red in teleportWorldData.MeshRend)
					{
						red.material = mat;
					}
				}
				catch { }
			}
			/*
			else if (teleportWorldData.TargetColor == Tan)
			{
				try
				{
					Material mat = originalMaterials["ball2"];
					foreach (Renderer red in teleportWorldData.MeshRend)
					{
						red.material = mat;
					}
				}
				catch { }
			}*/
			else
            {
				Material mat = originalMaterials["portal_small"];
				foreach (Renderer red in teleportWorldData.MeshRend)
				{
					red.material = mat;
				}
			}

			foreach (Light light in teleportWorldData.Lights)
			{
				if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
				{
					light.color = lightcolor;
				}
				else
					light.color = teleportWorldData.TargetColor;
			}

			Color FlamePurple = new Color(191f / 255f, 0f, 191f / 255f, 1);
			foreach (ParticleSystem system in teleportWorldData.Systems)
			{
				ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
				if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
				{
					colorOverLifetime.color = new ParticleSystem.MinMaxGradient(flamesstart, flamesend);
				}

				ParticleSystem.MainModule main = system.main;
				if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
				{
					main.startColor = flamesstart;
				}
				else
					main.startColor = teleportWorldData.TargetColor;
			}

			//teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor; // set color

			foreach (Material material in teleportWorldData.Materials)
			{
				if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
				{
					material.color = flamesstart;
				}
				else
					material.color = teleportWorldData.TargetColor;
			}
			
			if (SetcolorTarget)
            {
				if (teleportWorldData.TargetColor == Color.black)
				{
					teleportWorldData.TeleportW.m_colorTargetfound = Color.black * 10;
				}
				else if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
				{
					teleportWorldData.TeleportW.m_colorTargetfound = m_colorTargetfound * 7;
				}
				else if (teleportWorldData.TargetColor == Gold)
				{
					teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor;
				}
				else if (teleportWorldData.TargetColor == Tan)
				{
					teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor *3;
				}
				else if (teleportWorldData.TargetColor == Color.cyan) // cyan now
				{
					teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor*4;
				}
				else
					teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor * 7; // set color // set intensity very high
			}
			
		}
		private static Dictionary<string, Material> originalMaterials;

		public static void GetAllMaterials()
		{
			Material[] array = Resources.FindObjectsOfTypeAll<Material>();
			originalMaterials = new Dictionary<string, Material>();
			Material[] array2 = array;
			foreach (Material val in array2)
			{
				// Dbgl($"Material {val.name}" );
				originalMaterials[val.name] = val;
			}
		}

		/*
		void UpdateColorHexValue(object sender, EventArgs eventArgs)
		{
			_targetPortalColorHex.Value = $"#{GetColorHtmlString(_targetPortalColor.Value)}";
		}

		void UpdateColorValue(object sender, EventArgs eventArgs)
		{
			if (ColorUtility.TryParseHtmlString(_targetPortalColorHex.Value, out Color color))
			{
				_targetPortalColor.Value = color;
			}
		}
		*/


		private static void ServerZDOymlUpdate(int Colorint, string Portalname) // MESSAGE SENDER
        {
			if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
				return;

			ZPackage pkg = new ZPackage(); // Create ZPackage
			string textSplit = Portalname + "," + Colorint;
			string msg ="";
			for (int i = 1; i < textSplit.Length; i++)
			{
				msg += textSplit[i] + " ";
			}
			pkg.Write(Portalname + "," + Colorint);
			RareMagicPortal.LogInfo($"Sending the Server a update for {Portalname} with Color {Colorint}");
			ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestServerAnnouncementRMP", new object[] { pkg });
		}

		public static void RPC_RequestServerAnnouncementRMP(long sender, ZPackage pkg) // MESSAGE RECIEVER
		{
			if (ZNet.instance.IsDedicated() && ZNet.instance.IsServer())
			{
				if (pkg != null && pkg.Size() > 0)
				{ // Check that our Package is not null, and if it isn't check that it isn't empty.
					ZNetPeer peer = ZNet.instance.GetPeer(sender);
					if (peer != null)
					{ // Confirm the peer exists
						string peerSteamID = ((ZSteamSocket)peer.m_socket).GetPeerID().m_SteamID.ToString();
						string msg = pkg.ReadString();
						string[] msgArray = msg.Split(',');
						string PortalName = msgArray[0];
						int Colorint = Convert.ToInt32(msgArray[1]);
						RareMagicPortal.LogInfo($"Server has recieved a YML update from {peerSteamID} for {PortalName} with Color {Colorint}");

						updateYmltoColorChange(PortalName, Colorint);

						//YMLPortalData.Value has been updated
						return;

						//ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "EventServerAnnouncementRMP", new object[] { pkg }); // send to clients which is not needed will yml
					}
				}
			}
		}
		/*
		public static void RPC_EventServerAnnouncementRMP(long sender, ZPackage pkg)

			return;
		}
		*/


		static string GetColorHtmlString(Color color)
		{
			return color.a == 1.0f
				? ColorUtility.ToHtmlStringRGB(color)
				: ColorUtility.ToHtmlStringRGBA(color);
		}



	}// end of namespace class
	public static class ObjectExtensions
	{
		public static T Ref<T>(this T o) where T : UnityEngine.Object
		{
			return o ? o : null;
		}
	}

	

}

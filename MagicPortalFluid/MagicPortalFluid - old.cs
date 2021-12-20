// MagicPortalFluid
// a Valheim mod skeleton using Jötunn
// 
// File:    MagicPortalFluid.cs
// Project: MagicPortalFluid

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.GUI;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = Jotunn.Logger;
using HarmonyLib;
using MagicPortalFluid;


namespace MagicPortalFluid
{
	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	[BepInDependency(Jotunn.Main.ModGuid)]
	//[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
	internal class MagicPortalFluid : BaseUnityPlugin
	{
		public const string PluginGUID = "WackyMole.MagicPortalFluid";
		public const string PluginName = "MagicPortalFluid";
		public const string PluginVersion = "0.0.1";

		// Use this class to add your own localization to the game
		// https://valheim-modding.github.io/Jotunn/tutorials/localization.html
		//public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

		private AssetBundle portalmagicfluid;
		private CustomLocalization Localization;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		[HarmonyPriority(0)]
		private static class ZNetScene_Awake_Patch
		{
			private static void Postfix()
			{
				{
					((MonoBehaviour)(object)context).StartCoroutine(DelayedLoadRecipes());
					LoadAllRecipeData(reload: true); // while loading on world screen
				}
			}
		}


		private void Awake()
		{
			LoadAssets();
			itemModCreation();

			context = this;

			assetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), typeof(MagicPortalFluid).Namespace);
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), (string)null);



			// Jotunn comes with MonoMod Detours enabled for hooking Valheim's code
			// https://github.com/MonoMod/MonoMod
			On.FejdStartup.Awake += FejdStartup_Awake;

			// PrefabManager.OnVanillaPrefabsAvailable += PortalRecipeCreate;

			Jotunn.Logger.LogInfo("MagicPortalFluid has landed");

		}

		private void FejdStartup_Awake(On.FejdStartup.orig_Awake orig, FejdStartup self)
		{
			// This code runs before Valheim's FejdStartup.Awake
			Jotunn.Logger.LogInfo("FejdStartup is going to awake");

			// Call this method so the original game method is invoked
			orig(self);

			// This code runs after Valheim's FejdStartup.Awake
			//PortalRecipeCreate();
			// SetPieceRecipeData();
			//LoadAllRecipeData(reload: true);
			//PortalChanger();
			Jotunn.Logger.LogInfo("FejdStartup has awoken");
		}

		// end startup

		private void LoadAssets()
		{
			portalmagicfluid = AssetUtils.LoadAssetBundleFromResources("portalmagicfluid", typeof(MagicPortalFluid).Assembly);
			Jotunn.Logger.LogInfo($"Embedded resources: {string.Join(",", typeof(MagicPortalFluid).Assembly.GetManifestResourceNames())}");

		}

		private void UnLoadAssets()
		{
			portalmagicfluid.Unload(false);
		}

		private void itemModCreation()
		{
			var magicjuice_prefab = portalmagicfluid.LoadAsset<GameObject>("PortalMagicFluid");
			var portaljuice = new CustomItem(magicjuice_prefab, fixReference: false);
			ItemManager.Instance.AddItem(portaljuice);

			var magicjuice_prefab2 = portalmagicfluid.LoadAsset<GameObject>("PortalMagicFluid2");
			var portaljuice2 = new CustomItem(magicjuice_prefab2, fixReference: false);
			ItemManager.Instance.AddItem(portaljuice2);

		}

		// changing portals section

		// JVL changer
		private static void PortalChanger()
		{
			Jotunn.Logger.LogInfo("Wackymole has landed");
			var paul = PrefabManager.Instance.GetPrefab("portal_wood"); // this is iffy
			GameObject peter = GetPieces().Find((GameObject g) => Utils.GetPrefabName(g) == "portal_wood"); // better
																											//peter.GetComponent<Piece>().m_craftingStation = GetCraftingStation("Hammer");
			List<Piece.Requirement> requirements = new List<Piece.Requirement>();
			requirements.Add(new Piece.Requirement
			{
				m_amount = 1,
				m_resItem = ObjectDB.instance.GetItemPrefab("FineWood").GetComponent<ItemDrop>(),
				m_recover = true
			});
			requirements.Add(new Piece.Requirement
			{
				m_amount = 1,
				m_resItem = ObjectDB.instance.GetItemPrefab("PortalMagicFluid").GetComponent<ItemDrop>(),
				m_recover = true
			});
			requirements.Add(new Piece.Requirement
			{
				m_amount = 2,
				m_resItem = ObjectDB.instance.GetItemPrefab("Stone").GetComponent<ItemDrop>(),
				m_recover = true
			});
			requirements.Add(new Piece.Requirement
			{
				m_amount = 2,
				m_resItem = ObjectDB.instance.GetItemPrefab("Wood").GetComponent<ItemDrop>(),
				m_recover = true
			});

			paul.GetComponent<Piece>().m_resources = requirements.ToArray();

		}





		/*
		 	foreach (string req in data.reqs)
			{
				string[] array = req.Split(':');
				list.Add(new Piece.Requirement
				{
					m_resItem = ObjectDB.instance.GetItemPrefab(array[0]).GetComponent<ItemDrop>(),
					m_amount = int.Parse(array[1]),
					m_amountPerLevel = int.Parse(array[2]),
					m_recover = (array[3].ToLower() == "true")
				});
			}
            List<Piece.Requirement> requirements = new List<Piece.Requirement>();
            requirements.Add(new Piece.Requirement
            {
                m_amount = 1,
                m_resItem = ObjectDB.instance.GetItemPrefab("Crystal").GetComponent<ItemDrop>(),
                m_recover = true
            });
            requirements.Add(new Piece.Requirement
            {
                m_amount = 2,
                m_resItem = ObjectDB.instance.GetItemPrefab("Obsidian").GetComponent<ItemDrop>(),
                m_recover = true
            });
            requirements.Add(new Piece.Requirement
            {
                m_amount = 2,
                m_resItem = ObjectDB.instance.GetItemPrefab("Bronze").GetComponent<ItemDrop>(),
                m_recover = true
            });
            recipe.m_resources = requirements.ToArray();
 
            ObjectDB.instance.m_recipes.Add(recipe);
		
		[HarmonyPatch(typeof(Console), "InputText")]
		private static class InputText_Patch
		{
			private static bool Prefix(Console __instance)
			{
				if (true) // mod enabled always
				{
					return true;
				}
				string text = __instance.m_input.text;
				if (text.ToLower().Equals(typeof(MagicPortalFluid).Namespace.ToLower() + " reset"))
				{
					((BaseUnityPlugin)context).get_Config().Reload();
					((BaseUnityPlugin)context).get_Config().Save();
					Traverse.Create((object)__instance).Method("AddString", new object[1] { text }).GetValue();
					Traverse.Create((object)__instance).Method("AddString", new object[1] { ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " config reloaded" }).GetValue();
					return false;
				}
				if (text.ToLower().Equals(typeof(MagicPortalFluid).Namespace.ToLower() + " reload"))
				{
					GetRecipeDataFromFiles();
					if ((bool)ObjectDB.instance)
					{
						LoadAllRecipeData(reload: true);
					}
					Traverse.Create((object)__instance).Method("AddString", new object[1] { text }).GetValue();
					Traverse.Create((object)__instance).Method("AddString", new object[1] { ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " reloaded recipes from files" }).GetValue();
					return false;
				}
				if (text.ToLower().StartsWith(typeof(MagicPortalFluid).Namespace.ToLower() + " save "))
				{
					string[] array = text.Split(' ');
					string text2 = array[array.Length - 1];
					RecipeData recipeDataByName = GetRecipeDataByName(text2);
					if (recipeDataByName == null)
					{
						return false;
					}
					CheckModFolder();
					File.WriteAllText(Path.Combine(assetPath, recipeDataByName.name + ".json"), JsonUtility.ToJson((object)recipeDataByName));
					Traverse.Create((object)__instance).Method("AddString", new object[1] { text }).GetValue();
					Traverse.Create((object)__instance).Method("AddString", new object[1] { ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " saved recipe data to " + text2 + ".json" }).GetValue();
					return false;
				}
				if (text.ToLower().StartsWith(typeof(MagicPortalFluid).Namespace.ToLower() + " dump "))
				{
					string[] array2 = text.Split(' ');
					string text3 = array2[array2.Length - 1];
					RecipeData recipeDataByName2 = GetRecipeDataByName(text3);
					if (recipeDataByName2 == null)
					{
						return false;
					}
					Dbgl(JsonUtility.ToJson((object)recipeDataByName2));
					Traverse.Create((object)__instance).Method("AddString", new object[1] { text }).GetValue();
					Traverse.Create((object)__instance).Method("AddString", new object[1] { ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " dumped " + text3 }).GetValue();
					return false;
				}
				if (text.ToLower().StartsWith(typeof(MagicPortalFluid).Namespace.ToLower() ?? ""))
				{
					string text4 = ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " reset\r\n" + ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " reload\r\n" + ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " dump <ItemName>\r\n" + ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " save <ItemName>";
					Traverse.Create((object)__instance).Method("AddString", new object[1] { text }).GetValue();
					Traverse.Create((object)__instance).Method("AddString", new object[1] { text4 }).GetValue();
					return false;
				}
				return true;
			}
		}
		*/

		private static MagicPortalFluid context;

		public static ConfigEntry<bool> modEnabled;

		public static ConfigEntry<bool> isDebug;

		public static ConfigEntry<int> nexusID;

		public static ConfigEntry<float> globalArmorDurabilityLossMult;

		public static ConfigEntry<float> globalArmorMovementModMult;

		public static ConfigEntry<string> waterModifierName;

		private static List<RecipeData> recipeDatas = new List<RecipeData>();

		private static string assetPath;

		public static void Dbgl(string str = "", bool pref = true)
		{
			if (true) // debug
			{
				Debug.Log((pref ? (typeof(MagicPortalFluid).Namespace + " ") : "") + str);
			}
		}

		public static IEnumerator DelayedLoadRecipes()
		{
			yield return null;
			LoadAllRecipeData(reload: true);
		}

		private static void LoadAllRecipeData(bool reload)
		{
			if (reload)
			{
				PortalChanger();
				GetRecipeDataFromFiles();
			}
			Jotunn.Logger.LogInfo("made it in loader");
			//SetRecipeData();
			foreach (RecipeData recipeData in recipeDatas)
			{
				Jotunn.Logger.LogInfo("made it set recipe data");
				SetRecipeData(recipeData);
			}
		}

		private static void GetRecipeDataFromFiles()
		{
			CheckModFolder();
			recipeDatas.Clear();
			string[] files = Directory.GetFiles(assetPath, "*.json");

			//	RecipeData item = { "name":"portal_wood","craftingStation":"$piece_workbench","minStationLevel":1,"amount":1,"disabled":false,"reqs":["GreydwarfEye:10:1:True","FineWood:20:1:True","SurtlingCore:2:1:True"]}
			//recipeDatas.Add{ "name":"portal_wood","craftingStation":"$piece_workbench","minStationLevel":1,"amount":1,"disabled":false,"reqs":["GreydwarfEye:10:1:True","FineWood:20:1:True","SurtlingCore:2:1:True"]}


			foreach (string path in files)
			{

				RecipeData item = JsonUtility.FromJson<RecipeData>(File.ReadAllText(path));
				recipeDatas.Add(item);
			}
		}

		private static void CheckModFolder()
		{
			if (!Directory.Exists(assetPath))
			{
				Dbgl("Creating mod folder");
				Directory.CreateDirectory(assetPath);
			}
		}
		private static void SetRecipeData()
		{
			Jotunn.Logger.LogInfo("Made it in Recipes2");
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab("portal_wood");
			if (itemPrefab == null)
			{
				SetPieceRecipeData();
				return;
			}
		}

		private static void SetRecipeData(RecipeData data)
		{
			Jotunn.Logger.LogInfo("Made it in Recipes");
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(data.name);
			if (itemPrefab == null)
			{
				SetPieceRecipeData(data);
				return;
			}
			if (itemPrefab.GetComponent<ItemDrop>() == null)
			{
				Dbgl("Item data for " + data.name + " not found!");
				return;
			}
			for (int num = ObjectDB.instance.m_recipes.Count - 1; num > 0; num--)
			{
				if (ObjectDB.instance.m_recipes[num].m_item?.m_itemData.m_shared.m_name == itemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)
				{
					if (data.disabled)
					{
						Dbgl("Removing recipe for " + data.name + " from the game");
						ObjectDB.instance.m_recipes.RemoveAt(num);
						break;
					}
					ObjectDB.instance.m_recipes[num].m_amount = data.amount;
					ObjectDB.instance.m_recipes[num].m_minStationLevel = data.minStationLevel;
					ObjectDB.instance.m_recipes[num].m_craftingStation = GetCraftingStation(data.craftingStation);
					List<Piece.Requirement> list = new List<Piece.Requirement>();
					foreach (string req in data.reqs)
					{
						string[] array = req.Split(':');
						list.Add(new Piece.Requirement
						{
							m_resItem = ObjectDB.instance.GetItemPrefab(array[0]).GetComponent<ItemDrop>(),
							m_amount = int.Parse(array[1]),
							m_amountPerLevel = int.Parse(array[2]),
							m_recover = (array[3].ToLower() == "true")
						});
					}
					ObjectDB.instance.m_recipes[num].m_resources = list.ToArray();
					break;
				}
			}
		}
		private static void SetPieceRecipeData()
		{
			GameObject gameObject = GetPieces().Find((GameObject g) => Utils.GetPrefabName(g) == "portal_wood");
			Jotunn.Logger.LogInfo("Made it in Pieces2");
			PortalChanger();

		}
		private static void SetPieceRecipeData(RecipeData data)
		{
			GameObject gameObject = GetPieces().Find((GameObject g) => Utils.GetPrefabName(g) == data.name);
			if (gameObject == null)
			{
				NewMethod(data);
				return;
			}
			if (gameObject.GetComponent<Piece>() == null)
			{
				Dbgl("Item data for " + data.name + " not found!");
				return;
			}
			if (data.disabled)
			{
				Dbgl("Removing recipe for " + data.name + " from the game");
				ItemDrop itemDrop = ObjectDB.instance.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>();
				if ((bool)itemDrop && itemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(gameObject))
				{
					itemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(gameObject);
					return;
				}
				ItemDrop itemDrop2 = ObjectDB.instance.GetItemPrefab("Hoe")?.GetComponent<ItemDrop>();
				if ((bool)itemDrop2 && itemDrop2.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(gameObject))
				{
					itemDrop2.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(gameObject);
					return;
				}
			}
			gameObject.GetComponent<Piece>().m_craftingStation = GetCraftingStation(data.craftingStation);
			List<Piece.Requirement> list = new List<Piece.Requirement>();
			foreach (string req in data.reqs)
			{
				string[] array = req.Split(':');
				list.Add(new Piece.Requirement
				{
					m_resItem = ObjectDB.instance.GetItemPrefab(array[0]).GetComponent<ItemDrop>(),
					m_amount = int.Parse(array[1]),
					m_amountPerLevel = int.Parse(array[2]),
					m_recover = (array[3].ToLower() == "true")
				});
			}
			gameObject.GetComponent<Piece>().m_resources = list.ToArray();
		}

		private static void NewMethod(RecipeData data)
		{
			Dbgl("Item " + data.name + " not found!");
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
					Dbgl("got crafting station " + name);
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

		private static RecipeData GetRecipeDataByName(string name)
		{
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
			if (itemPrefab == null)
			{
				return GetPieceRecipeByName(name);
			}
			ItemDrop.ItemData itemData = itemPrefab.GetComponent<ItemDrop>().m_itemData;
			if (itemData == null)
			{
				Dbgl("Item data not found!");
				return null;
			}
			Recipe recipe = ObjectDB.instance.GetRecipe(itemData);
			if (!recipe)
			{
				Dbgl("Recipe not found!");
				return null;
			}
			RecipeData recipeData = new RecipeData
			{
				name = name,
				amount = recipe.m_amount,
				craftingStation = (recipe.m_craftingStation?.m_name ?? ""),
				minStationLevel = recipe.m_minStationLevel
			};
			Piece.Requirement[] resources = recipe.m_resources;
			foreach (Piece.Requirement requirement in resources)
			{
				recipeData.reqs.Add($"{Utils.GetPrefabName(requirement.m_resItem.gameObject)}:{requirement.m_amount}:{requirement.m_amountPerLevel}:{requirement.m_recover}");
			}
			return recipeData;
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

		//	public MagicPortalFluid()
		//	: this()
		//{
		//}

	}

	// end of namespace class

}

/*
 *             var data = new RecipeData();
            data.name = "portal_wood";
            //  recipeDatas.Add(item);
            data.reqs.Add("FineWood:30:1:True");
            data.reqs.Add("DeerHide:10:1:True");
            data.reqs.Add("Resin:20:1:True");
            data.reqs.Add("BronzeNails:80:1:True");
*/


// RecipeCustomization.BepInExPlugin
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RecipeCustomization;
using UnityEngine;

[BepInPlugin("aedenthorn.RecipeCustomization", "Recipe Customization", "0.5.0")]
public class BepInExPlugin : BaseUnityPlugin
{
	private enum NewDamageTypes
	{
		Water = 0x400
	}

	[HarmonyPatch(typeof(ZNetScene), "Awake")]
	[HarmonyPriority(0)]
	private static class ZNetScene_Awake_Patch
	{
		private static void Postfix()
		{
			{
				((MonoBehaviour)(object)context).StartCoroutine(DelayedLoadRecipes());
				LoadAllRecipeData(reload: true);
			}
		}
	}
	 

	[HarmonyPatch(typeof(Console), "InputText")]
	private static class InputText_Patch
	{
		private static bool Prefix(Console __instance)
		{
			if (!modEnabled.get_Value())
			{
				return true;
			}
			string text = __instance.m_input.text;
			if (text.ToLower().Equals(typeof(BepInExPlugin).Namespace.ToLower() + " reset"))
			{
				((BaseUnityPlugin)context).get_Config().Reload();
				((BaseUnityPlugin)context).get_Config().Save();
				Traverse.Create((object)__instance).Method("AddString", new object[1] { text }).GetValue();
				Traverse.Create((object)__instance).Method("AddString", new object[1] { ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " config reloaded" }).GetValue();
				return false;
			}
			if (text.ToLower().Equals(typeof(BepInExPlugin).Namespace.ToLower() + " reload"))
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
			if (text.ToLower().StartsWith(typeof(BepInExPlugin).Namespace.ToLower() + " save "))
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
			if (text.ToLower().StartsWith(typeof(BepInExPlugin).Namespace.ToLower() + " dump "))
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
			if (text.ToLower().StartsWith(typeof(BepInExPlugin).Namespace.ToLower() ?? ""))
			{
				string text4 = ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " reset\r\n" + ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " reload\r\n" + ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " dump <ItemName>\r\n" + ((BaseUnityPlugin)context).get_Info().get_Metadata().get_Name() + " save <ItemName>";
				Traverse.Create((object)__instance).Method("AddString", new object[1] { text }).GetValue();
				Traverse.Create((object)__instance).Method("AddString", new object[1] { text4 }).GetValue();
				return false;
			}
			return true;
		}
	}

	private static BepInExPlugin context;

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
		if (isDebug.get_Value())
		{
			Debug.Log((pref ? (typeof(BepInExPlugin).Namespace + " ") : "") + str);
		}
	}

	private void Awake()
	{
		context = this;

		assetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), typeof(BepInExPlugin).Namespace);
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), (string)null);
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
			GetRecipeDataFromFiles();
		}
		foreach (RecipeData recipeData in recipeDatas)
		{
			SetRecipeData(recipeData);
		}
	}

	private static void GetRecipeDataFromFiles()
	{
		CheckModFolder();
		recipeDatas.Clear();
		string[] files = Directory.GetFiles(assetPath, "*.json");
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

	private static void SetRecipeData(RecipeData data)
	{
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

	public BepInExPlugin()
		: this()
	{
	}
}

internal class RecipeData
{
	public string name;

	public string craftingStation;

	public int minStationLevel;

	public int amount;

	public bool disabled;

	public List<string> reqs = new List<string>();
}

   

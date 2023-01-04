using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RareMagicPortal.PortalName;
using UnityEngine;
using YamlDotNet.Serialization;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Bootstrap;
using static UnityEngine.GraphicsBuffer;
using YamlDotNet.Core.Tokens;
using BepInEx.Configuration;
using static Heightmap;
using static RareMagicPortal.PortalColorLogic;

namespace RareMagicPortal
{


    internal class PortalColorLogic
    {

        // setups
        public static readonly ManualLogSource RMP =
            BepInEx.Logging.Logger.CreateLogSource(MagicPortalFluid.ModName);

        static Color m_colorTargetfound = new Color(191f / 255f, 150f / 255f, 0, 25);
        static Color lightcolor = new Color(1f, 100f / 255f, 0, 1f);
        //Material PortalDefMaterial = originalMaterials["portal_small"];
        public static Color flamesstart = new Color(1f, 194f / 255f, 34f / 255f, 1f);
        public static Color flamesend = new Color(1f, 0, 0, 1f);
        public static Color Gold = new Color(1f, 215f / 255f, 0, 1f);
        public static Color Purple = new Color(107f / 255f, 63f / 255f, 160 / 255f, 1f);
        public static Color Tan = new Color(210f / 255f, 180f / 255f, 140f / 255f, 1f);
        public static Color Brown = new Color(193f / 255f, 69f / 255f, 19f / 255f, 1f);

        internal static PortalName PortalN;
        internal static Player player = null; // need to keep it between patches
        private static int waitloop = 0;
        private static int rainbowWait=0;
        private static string currentRainbow = "Yellow";


        internal enum PortalColor // gold - master should always be last or highest int
        {
            Yellow = 1,
            Red =2,
            Green = 3,
            Blue = 4,
            Purple = 5,
            Tan = 6,
            Cyan = 7,
            Orange=8,
            White = 20,
            Black = 21,
            Gold = 22,

        }

        internal static Dictionary<string,(Color HexName, int Pos,bool Enabled, string NextColor, string MessageText) > PortalColors = new Dictionary<string,(Color, int, bool, string, string)>()
        {
            {nameof(PortalColor.Yellow),(Color.yellow,(int)PortalColor.Yellow,    false, nameof(PortalColor.Red),   "Red Portal"  )},
            {nameof(PortalColor.Red), (Color.red,(int)PortalColor.Red,            false, nameof(PortalColor.Green), "Red Portal"  )},
            {nameof(PortalColor.Green), (Color.green,(int)PortalColor.Green,      false, nameof(PortalColor.Blue),  "Red Portal"  )},
            {nameof(PortalColor.Blue), (Color.blue,(int)PortalColor.Blue,         false, nameof(PortalColor.Purple),"Red Portal"  )},
            {nameof(PortalColor.Purple),( Purple,(int)PortalColor.Purple,         false, nameof(PortalColor.Tan),   "Red Portal"  )},
            {nameof(PortalColor.Tan), (Brown,(int)PortalColor.Tan,                false, nameof(PortalColor.Cyan),  "Red Portal"  )},
            {nameof(PortalColor.Cyan), (Color.cyan,(int)PortalColor.Cyan,         false, nameof(PortalColor.Orange),"Red Portal"  )},
            {nameof(PortalColor.Orange),( Color.yellow,(int)PortalColor.Orange,   false, nameof(PortalColor.White), "Red Portal"  )},
            {nameof(PortalColor.White), (Color.white,(int)PortalColor.White,      false, nameof(PortalColor.Black), "Red Portal"  )},
            {nameof(PortalColor.Black), (Color.black,(int)PortalColor.Black,      false, nameof(PortalColor.Gold),  "Red Portal"  )},
            {nameof(PortalColor.Gold), (Gold,(int)PortalColor.Gold,               false, nameof(PortalColor.Yellow),"Red Portal"  )}
       
        };


        internal static Dictionary<string, int> CrystalCount = new Dictionary<string, int>();
        internal static Dictionary<string, int> KeyCount = new Dictionary<string, int>(); 
        
        public static void initRCL ()
        {

            List<string> coloren = MagicPortalFluid.EnabledColors.Value.Split(',').ToList();
            foreach (var temp in coloren) // enabled from config
            {
                var temp2 = PortalColors[temp];
                temp2.Enabled = true;
                PortalColors[temp] = temp2;
            }
            foreach (var cols in PortalColors) // setup for all that don't have a count or crystal/key
            {
                CrystalCount.Add(cols.Key, 0);
                KeyCount.Add(cols.Key, 0);
            }
            foreach (var cols in PortalColors.ToList()) // compute NextColorOrder name fuck this by the way. FUCK this
            {
                if (cols.Value.Enabled)
                {
                    string vl = cols.Key;
                    var copy = cols.Value;
                    PortalColor something = (PortalColor)Enum.Parse(typeof(PortalColor), vl); // this will certainly throw an error if incorrect
                    string nextcheck = something.Next().ToString();
                   // RMP.LogInfo("nextcheck" + nextcheck);
                    bool found = false;
                    int loop = 0;
                    while (!found)
                    {
                        foreach (var cols3 in PortalColors)
                        {
                            //RMP.LogInfo("Keycheck" + cols3.Key);
                            if (cols3.Key == nextcheck)
                            {
                                if (cols3.Value.Enabled)
                                {
                                    found = true;
                                    copy.NextColor = nextcheck;
                                    break;
                                }
                                else // found but not enabled so copy current found color and recalc
                                {
                                    vl = cols3.Key;
                                    something = (PortalColor)Enum.Parse(typeof(PortalColor), vl);
                                    nextcheck = something.Next().ToString();
                                    break;
                                }
                            }// end match check
                        }
                        loop++;
                        if (loop > 30)
                        {
                            RMP.LogWarning("NextColor not found");
                            break;
                        }
                    }
                   // RMP.LogInfo("Key Setting");
                    PortalColors[cols.Key] = copy;
                }
            }
 

        }
        


        

        #region Patches

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
                if (!__instance.m_target_found)
                {
                    // The prefab does not have '_target_found_red' but instead '_target_found'.
                    GameObject targetFoundObject = __instance.gameObject.transform.Find("_target_found").gameObject;

                    // Disable the GameObject first, as adding component EffectFade calls its Awake() before being attached.
                    targetFoundObject.SetActive(false);
                    __instance.m_target_found = targetFoundObject.AddComponent<EffectFade>();
                    targetFoundObject.SetActive(true);
                }
                //RareMagicPortal.LogInfo("Adding Portal Awake for all Portals");

                MagicPortalFluid._teleportWorldDataCache.Add(__instance, new TeleportWorldDataRMP(__instance));
            }



            [HarmonyPostfix]
            [HarmonyPriority(Priority.Low)]
            [HarmonyPatch(nameof(TeleportWorld.UpdatePortal))]
            static void TeleportWorldUpdatePortalPostfixRMP(ref TeleportWorld __instance)
            {
                if (//!ConfigEnableCrystalsNKeys.Value
                      !__instance
                    ||  !__instance.m_nview
                    || __instance.m_nview.m_zdo == null
                    || __instance.m_nview.m_zdo.m_zdoMan == null
                    || __instance.m_nview.m_zdo.m_vec3 == null)
                //|| !__instance.m_nview.m_zdo.m_vec3.ContainsKey(_teleportWorldColorHashCode)) // going to ask for it below, so no reason to get a null
                //|| !_teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData)) // I don't think this will break anything
                {
                    return;
                }
                try
                {
                    //if (!__instance.m_nview.m_zdo.m_vec3.ContainsKey(_teleportWorldColorHashCode))
                    //	__instance.m_nview.m_zdo.Set(_teleportWorldColorHashCode, Utils.ColorToVec3(color));
                    bool isthistrue = MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData);
                    if (Player.m_localPlayer.m_seman.GetStatusEffect("yippeTele") != null)
                    {
                        // override color for teleportanything color
                        if (MagicPortalFluid.PortalDrinkColor.Value == "rainbow")
                        {   if (rainbowWait == 2)
                            {
                                PortalColor currentC = (PortalColor)Enum.Parse(typeof(PortalColor), currentRainbow);
                                rainbowWait = 0;
                            }
                            rainbowWait++;
                        }
                        else {
                            teleportWorldData.TargetColor = PortalColors[MagicPortalFluid.PortalDrinkColor.Value].HexName;
                        }
                        SetTeleportWorldColors(teleportWorldData, false, false);
                    }
                    else
                    {
                        string PortalName = __instance.m_nview.m_zdo.GetString("tag");
                        int colorint = PortalColorLogic.CrystalandKeyLogicColor(out string currentcolor, out Color color, out string nextcolor, PortalName);

                        if (MagicPortalFluid.ConfigUseBiomeColors.Value) // obviously teleportWorldData needs to be set
                        {
                            var Biome = "Meadows"; // in case not set yet. // Should get set on hover
                            if (__instance.m_nview.m_zdo.m_vec3.ContainsKey(MagicPortalFluid._portalBiomeHashCode))
                                Biome = __instance.m_nview.m_zdo.GetString(MagicPortalFluid._portalBiomeHashCode);

                            teleportWorldData.Biome = Biome;

                            string BC = MagicPortalFluid.BiomeRepColors.Value;
                            string[] BCarray = BC.Split(',');
                            var results = Array.FindAll(BCarray, s => s.Equals(Biome));
                            List<string> single = results[0].Split(':').ToList(); // should only be 1

                            foreach (var col in PortalColors)
                            {
                                if (col.Key == single[1])
                                {
                                    teleportWorldData.BiomeColor = col.Key;
                                    color = col.Value.HexName;
                                }
                            }

                            Color BiomeCol = PortalColors[teleportWorldData.BiomeColor].HexName;
                            color = BiomeCol;
                        }

                        if (color != teleportWorldData.OldColor)
                        {  // don't waste resources
                            teleportWorldData.TargetColor = color;
                            SetTeleportWorldColors(teleportWorldData, true);
                        }

                    }
                }
                catch { } // catches beginning errors


                }
            }


            [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Interact))]
            public static class PortalCheckOutside
            {
                internal static bool Prefix(TeleportWorld __instance, Humanoid human, bool hold)

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
                        string PortalName = __instance.m_nview.m_zdo.GetString("tag");


                        if (portal != null && PortalName != "" && PortalName != "Empty tag")
                        {
                            Player closestPlayer = Player.m_localPlayer; //Player.GetClosestPlayer(__instance.m_proximityRoot.position, 5f);
                            bool sameperson = false;
                            if (portal.m_creator == closestPlayer.GetPlayerID())
                                sameperson = true;

                            //RareMagicPortal.LogInfo($"Made it to Map during Portal Interact Past portal check and is admin {isAdmin}");
                            if (Input.GetKey(MagicPortalFluid.portalRMPKEY.Value.MainKey) && MagicPortalFluid.portalRMPKEY.Value.Modifiers.All(Input.GetKey) && (MagicPortalFluid.isAdmin || sameperson && !MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)) // creator can change it if enable crystals is off
                            {
                                MagicPortalFluid.Globaliscreator = sameperson; // set this for yml permissions


                                int colorint = PortalColorLogic.CrystalandKeyLogicColor( out string currentcolor, out Color color, out string nextcolor, PortalName);

                                String BiomeColor = MagicPortalFluid.CrystalKeyDefaultColor.Value;
                                if (MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData))
                                {
                                    teleportWorldData.TargetColor = color;
                                    BiomeColor = teleportWorldData.BiomeColor;
                                    SetTeleportWorldColors(teleportWorldData, true);
                                }
                                __instance.m_nview.m_zdo.Set(MagicPortalFluid._teleportWorldColorHashCode, Utils.ColorToVec3(color));
                                //__instance.m_nview.m_zdo.Set(_teleportWorldColorAlphaHashCode, color);
                                __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);

                                colorint = PortalColors[nextcolor].Pos; // inc 1 color// should loop around for last one

                                updateYmltoColorChange(PortalName, colorint, BiomeColor);

                                return false; // stop interaction on changing name

                            }

                            if (sameperson || !sameperson && !MagicPortalFluid.ConfigCreatorLock.Value || closestPlayer.m_noPlacementCost)
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


                    string PortalName = __instance.m_nview.m_zdo.GetString("tag");
                    int colorint = 1;


                    string currentcolor = "Default";
                    string nextcolor;
                    Color currentcolorHex;
                    colorint = CrystalandKeyLogicColor(out currentcolor, out currentcolorHex, out nextcolor, PortalName);


                    string text;
                    Color color = currentcolorHex;
                    switch (colorint)
                    {
                        case 0:
                            text = "Admin Only";
                            break;
                        case 1:
                            text = "Yellow Crystal Portal";
                            break;
                        case 2:
                            text = "Red Crystal Portal";
                            break;
                        case 3:
                            text = "Green Crystal Portal";
                            break;
                        case 4:
                            text = "Blue Crystal Portal";
                            break;
                        case 5:
                            text = "Purple Crystal Portal";
                            break;
                        case 6:
                            text = "Brown Crystal Portal";
                            break;
                        case 7:
                            text = "Cyan Crystal Portal";
                            break;
                        case 8:
                            text = "Orange Crystal Portal";
                            break;
                        case 20:
                            text = "White Crystal Portal";
                            break;
                        case 21:
                            text = "Black Crystal Portal";
                            break;
                        case 22:
                            text = "Gold Crystal Portal";
                            break;
                        default:
                            text = "";
                            break;

                    }



                    if (PortalName == "" && currentcolor != MagicPortalFluid.CrystalKeyDefaultColor.Value && MagicPortalFluid.JustSent == 0)
                    {
                        if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "None" || MagicPortalFluid.CrystalKeyDefaultColor.Value == "none")
                            colorint = 1;
                        else
                        {
                            try
                            {
                                colorint = (int)((PortalColor)Enum.Parse(typeof(PortalColor), MagicPortalFluid.CrystalKeyDefaultColor.Value));
                            }
                            catch { RMP.LogWarning($"DefaultPortalColor {MagicPortalFluid.CrystalKeyDefaultColor.Value} is not an option,this will cause repeating network traffic on no name portals"); }
                        }
                    }

                    var Biome = closestPlayer.GetCurrentBiome().ToString();
                    if (
                    !__instance.m_nview
                    || __instance.m_nview.m_zdo == null
                    || __instance.m_nview.m_zdo.m_zdoMan == null
                    || __instance.m_nview.m_zdo.m_vec3 == null
                    || !__instance.m_nview.m_zdo.m_vec3.ContainsKey(MagicPortalFluid._portalBiomeHashCode))
                    {
                        RMP.LogInfo("Setting Portal Color For First Time");
                        if (MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData))
                        {
                            teleportWorldData.Biome = Biome;
                            Color newColor = color;
                            if (MagicPortalFluid.ConfigUseBiomeColors.Value)
                            {
                                string BC = MagicPortalFluid.BiomeRepColors.Value;
                                string[] BCarray = BC.Split(',');
                                var results = Array.FindAll(BCarray, s => s.Equals(Biome));
                                List<string> single = results[0].Split(':').ToList(); // should only be 1

                                foreach (var col in PortalColors)
                                {
                                    if (col.Key == single[1])
                                    {
                                        teleportWorldData.BiomeColor = col.Key;
                                        //PortalColorLogic.updateYmltoColorChange("", colorint); // No I shouldn't do it in pairs, needs to be individual
                                        newColor = col.Value.HexName;
                                    }
                                }
                            }

                            teleportWorldData.TargetColor = color;
                            SetTeleportWorldColors(teleportWorldData, true);
                            __instance.m_nview.m_zdo.Set(MagicPortalFluid._teleportWorldColorHashCode, Utils.ColorToVec3(color));
                            __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);
                            __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalBiomeHashCode, Biome);
                            RMP.LogInfo("Setting ZDO Color For First Time");
                        }
                        if (PortalName == "")
                            PortalColorLogic.updateYmltoColorChange("", colorint); // update only once
                    }





                    if (PortalName != "" && PortalName != "Empty tag")
                    {
                        if (MagicPortalFluid.isAdmin || sameperson && !MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)
                        {

                            if (MagicPortalFluid.portalRMPKEY.Value.MainKey is KeyCode.None)
                            {
                                //Shortcut = portalRMPKEY.Value;
                                return; // because it would error out otherwise

                            }
                            // L-ctrl + E instead of _

                            if (MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)
                            {
                                __result =
                                    string.Format(
                                        "{0}\n<size={4}>[<color={5}>{2}</color>] Change <color={7}>Portal</color>[{1}] Crystal to: [<color={8}>{3}</color>]</size>\n<size={4}>{6}</size>",
                                        __result,
                                        currentcolor,
                                        MagicPortalFluid.portalRMPKEY.Value + " + " + "E",//_changePortalReq,
                                        nextcolor,
                                        15,
                                        "Yellow",
                                        text,
                                        color,
                                        PortalColors[nextcolor].HexName
                                        );
                            }
                            else
                            {
                                __result =
                                    string.Format(
                                        "{0}\n<size={4}>[<color={5}>{2}</color>] Change <color={6}>Portal</color>[{1}] Color to: [<color={7}>{3}</color>] </size>",
                                        __result,
                                        currentcolor,
                                        MagicPortalFluid.portalRMPKEY.Value + " + " + "E", //_changePortalReq,
                                        nextcolor,
                                        15,
                                        "Yellow",
                                        color,
                                        PortalColors[nextcolor].HexName
                                        );
                            }
                        }
                        else
                        {
                            if (MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)
                            {
                                __result =
                                    string.Format(
                                        "{0}\n<size={2}><color={5}>{1} Portal</color></size>\n<size={2}>{4}</size>",
                                        __result,
                                        currentcolor,
                                        15,
                                        "Yellow",
                                        text,
                                        color
                                        );
                            }
                            else
                            {
                                __result =
                                    string.Format(
                                        "{0}\n<size={2}><color={4}>{1} Portal</color></size>",
                                        __result,
                                        currentcolor,
                                        15,
                                        "Yellow",
                                        color
                                        );
                            }

                        }
                    }
                    else// name = ""
                    {
                        string jo = "Please name Portal to change from Default";
                        __result =
                                    string.Format(
                                        "{0}\n<size={1}><color={4}>{2}</color></size>",
                                        __result,
                                        15,
                                        jo,
                                        "Yellow",
                                        color
                                        );

                    }
                }
            }

            #endregion
            internal static int CrystalandKeyLogicColor(string PortalName = "")
            {

                int CrystalForPortal = MagicPortalFluid.ConfigCrystalsConsumable.Value;
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
                if (OdinsKin)
                {
                    var currentColor = PortalColor.Black.ToString();;
                    return PortalColors[currentColor].Pos;
                }

                if (Free_Passage)
                {
                    PortalColor som =(PortalColor) Enum.Parse(typeof(PortalColor), MagicPortalFluid.FreePassageColor.Value);
                    var currentColor = som.ToString();
                    return PortalColors[currentColor].Pos;
                }

                if (PortalN.Portals[PortalName].TeleportAnything)
                {
                    var currentColor = PortalColor.White.ToString();
                    return PortalColors[currentColor].Pos;
                }

                foreach (var pc in PortalColors)
                {
                    var name = pc.Key;
                    if (pc.Value.Enabled)
                    {
                        try
                        {
                            if (Portal_Crystal_Cost[name] > 0 || Portal_Key[name])
                            {
                                return pc.Value.Pos;
                            }
                        }
                        catch
                        {
                            //RMP.LogInfo("No Tan in File for Portal, adding one");
                            //WritetoYML(PortalName, "Tan");
                        }// not in file so maybe add?
                    }
                }
                return 0;

            }
            internal static int CrystalandKeyLogicColor(out string currentColor, out Color currentColorHex, out string nextcolor, string PortalName = "")
            {


                int CrystalForPortal = MagicPortalFluid.ConfigCrystalsConsumable.Value;
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
                if (OdinsKin)
                {
                    currentColor = PortalColor.Black.ToString();
                    currentColorHex = PortalColors[currentColor].HexName;
                    //int som = Convert.ToInt32(PortalColor.Yellow); // or PortalColors[currentColor].Pos +1
                    nextcolor = PortalColor.Black.Next().ToString(); //PortalColor.Red.ToString();
                    return PortalColors[currentColor].Pos;
                }

                if (Free_Passage)
                {
                    currentColor = PortalColor.Yellow.ToString();
                    currentColorHex = PortalColors[currentColor].HexName;
                    nextcolor = PortalColor.Yellow.Next().ToString(); //PortalColor.Red.ToString();
                    return PortalColors[currentColor].Pos;
                }

                if (PortalN.Portals[PortalName].TeleportAnything)
                {
                    currentColor = PortalColor.White.ToString();
                    currentColorHex = PortalColors[currentColor].HexName;
                    nextcolor = PortalColor.White.Next().ToString(); //PortalColor.Red.ToString();
                    return PortalColors[currentColor].Pos;
                }

                foreach (var pc in PortalColors)
                {
                    var name = pc.Key;
                    if (pc.Value.Enabled) { 
                        try
                        {
                            if (Portal_Crystal_Cost[name] > 0 || Portal_Key[name])
                            {
                                currentColor = name;
                                currentColorHex = PortalColors[currentColor].HexName;
                                nextcolor = pc.Value.NextColor; //PortalColor.Red.ToString();
                                return pc.Value.Pos;
                            }
                        } catch {
                            //RMP.LogInfo("No Tan in File for Portal, adding one");
                            //WritetoYML(PortalName, "Tan");
                            }// not in file so maybe add?
                    }
                }

                currentColor = "Yellow";
                currentColorHex = PortalColors["Yellow"].HexName;
                nextcolor = "Red";
                return 0;


            }
            internal static void updateYmltoColorChange(string PortalName, int colorint, string BiomeCol = null)
            {
                if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
                {
                    WritetoYML(PortalName);
                }
                if (BiomeCol != null)
                    PortalN.Portals[PortalName].BiomeColor = BiomeCol;

                string currentcolor = "Yellow"; // for reference only

                PortalColor Color =  (PortalColor)colorint;
                string ColorName = Color.ToString();
                RMP.LogWarning("Make sure to remove in release color "+ ColorName);

                //main set loop
                PortalN.Portals[PortalName].TeleportAnything = false;
                PortalN.Portals[PortalName].Admin_only_Access = false;

                foreach ( var col in PortalColors) // reset all to 0 and false
                {
                    PortalN.Portals[PortalName].Portal_Crystal_Cost[col.Key] = 0;
                    PortalN.Portals[PortalName].Portal_Key[col.Key] = false;
                }
                PortalN.Portals[PortalName].Portal_Crystal_Cost[ColorName] = MagicPortalFluid.ConfigCrystalsConsumable.Value; // set to default consume for int
                PortalN.Portals[PortalName].Portal_Key[ColorName] = true; // set to true

                if (MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)
                {
                    if (MagicPortalFluid.AdminColor.Value == ColorName)
                    {
                        PortalN.Portals[PortalName].Admin_only_Access = true;
                        PortalN.Portals[PortalName].TeleportAnything = true;
                    }
                    if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value)
                    {
                        PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = 1;
                        PortalN.Portals[PortalName].Portal_Key["Gold"] = true;
                    }
                    if (MagicPortalFluid.FreePassageColor.Value == ColorName) // with CrystalsandKeys you can't get change color without admin so this should be fine
                    {
                        PortalN.Portals[PortalName].TeleportAnything = true;
                    }
                }

                if (MagicPortalFluid.PortalDrinkColor.Value == ColorName ) // probably doesn't do anything here
                {}

                if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())// only for server 
                {
                    MagicPortalFluid.RareMagicPortal.LogInfo("You are a dedicated Server");
                    var serializer = new SerializerBuilder()
                    .Build();
                    var yamlfull = MagicPortalFluid.WelcomeString + Environment.NewLine + serializer.Serialize(PortalN); // build everytime

                    MagicPortalFluid.JustWrote = 1;
                    File.WriteAllText(MagicPortalFluid.YMLCurrentFile, yamlfull); //overwrite
                    string lines = "";
                    foreach (string line in System.IO.File.ReadLines(MagicPortalFluid.YMLCurrentFile)) // rethrough lines manually and add spaces, stupid
                    {
                        lines += line + Environment.NewLine;
                        if (line.Contains("Admin_only_Access")) // three spaces for non main objects
                        { lines += Environment.NewLine; }
                    }
                    File.WriteAllText(MagicPortalFluid.YMLCurrentFile, lines); //overwrite with extra goodies
                    MagicPortalFluid.JustWrote = 2;
                    MagicPortalFluid.YMLPortalData.Value = yamlfull; // send out to clients from server only
                }
                else
                {
                    if (!ZNet.instance.IsServer())
                    {
                        MagicPortalFluid.RareMagicPortal.LogInfo("You are connect to a Server");
                        functions.ServerZDOymlUpdate(colorint, PortalName);// send to server to update and push yml
                    }
                    else // single client only or Server but not dedicated
                    {
                        MagicPortalFluid.RareMagicPortal.LogInfo("Single client only or Server but not dedicated");
                        var serializer = new SerializerBuilder()
                        .Build();
                        var yamlfull = MagicPortalFluid.WelcomeString + Environment.NewLine + serializer.Serialize(PortalN); // build everytime

                        MagicPortalFluid.JustWrote = 1;
                        File.WriteAllText(MagicPortalFluid.YMLCurrentFile, yamlfull); //overwrite
                        string lines = "";
                        foreach (string line in System.IO.File.ReadLines(MagicPortalFluid.YMLCurrentFile)) // rethrough lines manually and add spaces, stupid
                        {
                            lines += line + Environment.NewLine;
                            if (line.Contains("Admin_only_Access")) // three spaces for non main objects
                            { lines += Environment.NewLine; }
                        }
                        File.WriteAllText(MagicPortalFluid.YMLCurrentFile, lines); //overwrite with extra goodies
                        if (MagicPortalFluid.ConfigEnableYMLLogs.Value)
                            MagicPortalFluid.RareMagicPortal.LogInfo(yamlfull);
                        MagicPortalFluid.JustWrote = 2;
                        if (ZNet.instance.IsServer()) // not just dedicated
                            MagicPortalFluid.YMLPortalData.Value = yamlfull;
                    }
                }

            }

            internal static bool CrystalandKeyLogic(string PortalName)
            {
                int CrystalForPortal = MagicPortalFluid.ConfigCrystalsConsumable.Value;
                bool OdinsKin = false;
                bool Free_Passage = false;

                MagicPortalFluid.RareMagicPortal.LogInfo($"Portal name is {PortalName}");
                if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
                {
                    WritetoYML(PortalName);
                }

                OdinsKin = PortalN.Portals[PortalName].Admin_only_Access;
                Free_Passage = PortalN.Portals[PortalName].Free_Passage;
                var Portal_Crystal_Cost = PortalN.Portals[PortalName].Portal_Crystal_Cost; // rgbG  // 0 means it can't be used, (Keys only) anything greater means the cost. -1 means same as 0
                var Portal_Key = PortalN.Portals[PortalName].Portal_Key; // rgbG
                var TeleportEvery = PortalN.Portals[PortalName].TeleportAnything;


                if (OdinsKin && MagicPortalFluid.isAdmin)
                {
                    player.Message(MessageHud.MessageType.TopLeft, "$rmp_kin_welcome"); // forgot this one
                    return true;
                }

                else if (OdinsKin && !MagicPortalFluid.isAdmin && MagicPortalFluid.ConfigEnableCrystalsNKeys.Value) // If requires admin, but not admin, but only with enable crystals otherwise just a normal portal
                {
                    player.Message(MessageHud.MessageType.Center, "$rmp_kin_only");
                    //Teleporting = false;
                    return false;

                }

                if (TeleportEvery) // if no crystals, then just white, if crystals then free passage
                {
                    player.Message(MessageHud.MessageType.TopLeft, "$rmp_freepassage");
                    if (MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)
                    {
                        player.Message(MessageHud.MessageType.Center, "$rmp_allowsEverything");
                    }
                    return true;
                }

                if (MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)
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


                    CrystalCount[nameof(PortalColor.Gold)] = player.m_inventory.CountItems(MagicPortalFluid.CrystalMaster);
                    CrystalCount[nameof(PortalColor.Red)] = player.m_inventory.CountItems(MagicPortalFluid.CrystalRed);
                    CrystalCount[nameof(PortalColor.Green)] = player.m_inventory.CountItems(MagicPortalFluid.CrystalGreen);
                    CrystalCount[nameof(PortalColor.Blue)] = player.m_inventory.CountItems(MagicPortalFluid.CrystalBlue);
                    CrystalCount[nameof(PortalColor.Purple)] = player.m_inventory.CountItems(MagicPortalFluid.CrystalPurple);
                    CrystalCount[nameof(PortalColor.Tan)] = player.m_inventory.CountItems(MagicPortalFluid.CrystalTan);

                    KeyCount[nameof(PortalColor.Gold)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyGold);
                    KeyCount[nameof(PortalColor.Red)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyRed);
                    KeyCount[nameof(PortalColor.Green)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyGreen);
                    KeyCount[nameof(PortalColor.Blue)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyBlue);
                    KeyCount[nameof(PortalColor.Purple)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyPurple);
                    KeyCount[nameof(PortalColor.Tan)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyTan);


                    int flagCarry = 0; // don't have any keys or crystals
                    int crystalorkey = 0;// 0 is crystal, 1 is key, 2 is both
                    bool foundAccess = false;
                    int lowest = 0;

                    int coun = PortalColors.Count;
                    foreach (var col in PortalColors)
                    {
                        if (CrystalCount[col.Key] > 0 || KeyCount[col.Key] > 0)
                        {
                            if (CrystalCount[col.Key] == 0) 
                                flagCarry = col.Value.Pos;
                            else if (Portal_Crystal_Cost[col.Key] > CrystalCount[col.Key]) // has less than required
                                flagCarry = 100+ col.Value.Pos;
                            else flagCarry = 200+ col.Value.Pos; // has more than required

                            if (Portal_Key[col.Key])
                            {
                                if (Portal_Crystal_Cost[col.Key] == 0)
                                {
                                    crystalorkey = 1;
                                    if (KeyCount[col.Key] > 0)
                                        flagCarry = 300+col.Value.Pos;
                                    else
                                        flagCarry = col.Value.Pos; // no crystal cost, but key cost with no key
                                }
                                else
                                {
                                    if (KeyCount[col.Key] > 0 && flagCarry< 200)
                                        flagCarry = 300+col.Value.Pos;
                                    else
                                        crystalorkey = 2; // yes crystal cost, and key cost with no key, so let user know both is good
                                }
                            }
                        }
                        if (flagCarry > 200)
                            foundAccess = true;
                        if (flagCarry < 200 && lowest == 0)
                            lowest = flagCarry;

                    }// for every color


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
                if (MagicPortalFluid.ConfigMessageLeft.Value)
                    hud = MessageHud.MessageType.TopLeft;

                    switch (flagCarry)
                    {
                        case 1:
                            player.Message(hud, $"$rmp_no_yellow_portal {CorK}"); // yellow maybe change permissions for yellow
                            return false;
                        case 2:
                            player.Message(hud, $"$rmp_no_red_portal {CorK}");
                            return false;
                        case 3:
                            player.Message(hud, $"$rmp_no_green_portal {CorK}");
                            return false;
                        case 4:
                            player.Message(hud, $"$rmp_no_blue_portal {CorK}");
                            return false;
                        case 5:
                            player.Message(hud, $"$rmp_no_purple_portal {CorK}");
                            return false;
                        case 6:
                            player.Message(hud, $"$rmp_no_tan_portal {CorK}");
                            return false;
                        case 7:
                            player.Message(hud, $"$rmp_no_cyan_portal {CorK}");
                            return false;
                        case 8:
                            player.Message(hud, $"$rmp_no_orange_portal {CorK}");
                            return false;
                        case 20:
                            player.Message(hud, $"$rmp_no_white_portal {CorK}");
                            return false;
                        case 21:
                            player.Message(hud, $"$rmp_no_black_portal {CorK}");
                            return false;
                        case 22:
                            player.Message(hud, $"$rmp_no_gold_portal {CorK}");
                            return false;

                        case 101:
                            player.Message(hud, $"{Portal_Crystal_Cost["Yellow"]} $rmp_required_yellow {PortalName}");
                            return false;
                        case 102:
                            player.Message(hud, $"{Portal_Crystal_Cost["Red"]} $rmp_required_red {PortalName}");
                            return false;
                        case 103:
                            player.Message(hud, $"{Portal_Crystal_Cost["Green"]} $rmp_required_green {PortalName}");
                            return false;
                        case 104:
                            player.Message(hud, $"{Portal_Crystal_Cost["Blue"]} $rmp_required_blue {PortalName}");
                            return false;
                        case 105:
                            player.Message(hud, $"{Portal_Crystal_Cost["Purple"]} $rmp_required_purple {PortalName}");
                            return false;
                        case 106:
                            player.Message(hud, $"{Portal_Crystal_Cost["Tan"]} $rmp_required_tan {PortalName}");
                            return false;
                        case 107:
                            player.Message(hud, $"{Portal_Crystal_Cost["Cyan"]} $rmp_required_cyan {PortalName}");
                            return false;
                        case 108:
                            player.Message(hud, $"{Portal_Crystal_Cost["Orange"]} $rmp_required_orange {PortalName}");
                            return false;
                        case 120:
                            player.Message(hud, $"{Portal_Crystal_Cost["White"]} $rmp_required_white {PortalName}");
                            return false;
                        case 121:
                            player.Message(hud, $"{Portal_Crystal_Cost["Black"]} $rmp_required_black {PortalName}");
                            return false;
                        case 122:
                            player.Message(hud, $"{Portal_Crystal_Cost["Gold"]} $rmp_required_gold {PortalName}");
                            return false;

                        case 201:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Yellow"]} $rmp_consumed_yellow");
                            //player.m_inventory.RemoveItem(MagicPortalFluid.Crystal, Portal_Crystal_Cost["Yellow"]);
                            return true;
                        case 202:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Red"]} $rmp_consumed_red");
                            player.m_inventory.RemoveItem(MagicPortalFluid.CrystalRed, Portal_Crystal_Cost["Red"]);
                            return true;
                        case 203:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Green"]} $rmp_consumed_green");
                            player.m_inventory.RemoveItem(MagicPortalFluid.CrystalGreen, Portal_Crystal_Cost["Green"]);
                            return true;
                        case 204:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Blue"]} $rmp_consumed_blue");
                            player.m_inventory.RemoveItem(MagicPortalFluid.CrystalBlue, Portal_Crystal_Cost["Blue"]);
                            return true;
                        case 205:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Purple"]} $rmp_consumed_purple");
                            player.m_inventory.RemoveItem(MagicPortalFluid.CrystalPurple, Portal_Crystal_Cost["Purple"]);
                            return true;
                        case 206:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Tan"]} $rmp_consumed_tan");
                            player.m_inventory.RemoveItem(MagicPortalFluid.CrystalTan, Portal_Crystal_Cost["Tan"]);
                            return true;
                        case 207:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Cyan"]} $rmp_consumed_cyan");
                            //player.m_inventory.RemoveItem(MagicPortalFluid.CrystalMaster, Portal_Crystal_Cost["Cyan"]);
                            return true;
                        case 208:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Orange"]} $rmp_consumed_orange");
                            //player.m_inventory.RemoveItem(MagicPortalFluid.CrystalMaster, Portal_Crystal_Cost["Orange"]);
                            return true;
                        case 220:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["White"]} $rmp_consumed_white");
                            //player.m_inventory.RemoveItem(MagicPortalFluid.CrystalMaster, Portal_Crystal_Cost["White"]);
                            return true;
                        case 221:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Black"]} $rmp_consumed_black");
                            //player.m_inventory.RemoveItem(MagicPortalFluid.CrystalMaster, Portal_Crystal_Cost["Black"]);
                            return true;
                        case 222:
                            player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Gold"]} $rmp_consumed_gold");
                            player.m_inventory.RemoveItem(MagicPortalFluid.CrystalMaster, Portal_Crystal_Cost["Gold"]);
                            return true;

                        case 301:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_yellowKey_access");
                            return true;
                        case 302:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_redKey_access");
                            return true;
                        case 303:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_greenKey_access");
                            return true;
                        case 304:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_blueKey_access");
                            return true;
                        case 305:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_purpleKey_access");
                            return true;
                        case 306:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_tanKey_access");
                            return true;
                        case 307:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_cyanKey_access");
                            return true;
                        case 308:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_orangeKey_access");
                            return true;
                        case 320:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_whiteKey_access");
                            return true;
                        case 321:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_blackKey_access");
                            return true;
                        case 322:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_goldKey_access");
                            return true;

                        case 999:
                            player.Message(MessageHud.MessageType.TopLeft, $"$rmp_noaccess");
                            return false;

                        default:
                            player.Message(hud, $"$rmp_noaccess");
                            return false;


                    }
                }
                return true;

            }

            internal static void WritetoYML(string PortalName, string updateP = null) // this only happens if portal is not in yml file at all
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
                    PortalN.Portals[PortalName].Portal_Crystal_Cost[updateP] = 0;
                    PortalN.Portals[PortalName].Portal_Key[updateP] = false; 
                }
                if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "None" || MagicPortalFluid.CrystalKeyDefaultColor.Value == "none")
                {
                    PortalN.Portals[PortalName].Free_Passage = true;
                    colorint = 1; // yellow
                } else
                {
                    PortalN.Portals[PortalName].Portal_Crystal_Cost[MagicPortalFluid.CrystalKeyDefaultColor.Value] = MagicPortalFluid.ConfigCrystalsConsumable.Value;
                    colorint = PortalColors[MagicPortalFluid.CrystalKeyDefaultColor.Value].Pos;
                 }

                if (MagicPortalFluid.ConfigAddRestricted.Value != "")
                    PortalN.Portals[PortalName].AdditionalProhibitItems = MagicPortalFluid.ConfigAddRestricted.Value.Split(',').ToList(); // one time


                if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())// only for server 
                {
                    RMP.LogInfo("You are a dedicated Server");
                    var serializer = new SerializerBuilder()
                    .Build();
                    var yamlfull = MagicPortalFluid.WelcomeString + Environment.NewLine + serializer.Serialize(PortalN); // build everytime

                    MagicPortalFluid.JustWrote = 1;
                    File.WriteAllText(MagicPortalFluid.YMLCurrentFile, yamlfull); //overwrite
                    string lines = "";
                    foreach (string line in System.IO.File.ReadLines(MagicPortalFluid.YMLCurrentFile)) // rethrough lines manually and add spaces, stupid
                    {
                        lines += line + Environment.NewLine;
                        if (line.Contains("Admin_only_Access")) // three spaces for non main objects
                        { lines += Environment.NewLine; }
                    }
                    File.WriteAllText(MagicPortalFluid.YMLCurrentFile, lines); //overwrite with extra goodies
                    MagicPortalFluid.JustWrote = 2;
                    MagicPortalFluid.YMLPortalData.Value = yamlfull; // send out to clients from server only
                }
                else
                {
                    if (!ZNet.instance.IsServer()) // is not server
                    {
                        RMP.LogInfo("You are connect to a Server");
                        functions.ServerZDOymlUpdate(colorint, PortalName);// send to server to update and push yml
                    }
                    else // single client only or Server but not dedicated
                    {
                        RMP.LogInfo("Single client only or Server but not dedicated");
                        var serializer = new SerializerBuilder()
                        .Build();
                        var yamlfull = MagicPortalFluid.WelcomeString + Environment.NewLine + serializer.Serialize(PortalN); // build everytime

                        MagicPortalFluid.JustWrote = 1;
                        File.WriteAllText(MagicPortalFluid.YMLCurrentFile, yamlfull); //overwrite
                        string lines = "";
                        foreach (string line in System.IO.File.ReadLines(MagicPortalFluid.YMLCurrentFile)) // rethrough lines manually and add spaces, stupid
                        {
                            lines += line + Environment.NewLine;
                            if (line.Contains("Admin_only_Access")) // three spaces for non main objects
                            { lines += Environment.NewLine; }
                        }
                        File.WriteAllText(MagicPortalFluid.YMLCurrentFile, lines); //overwrite with extra goodies
                        if (MagicPortalFluid.ConfigEnableYMLLogs.Value)
                            RMP.LogInfo(yamlfull);
                        MagicPortalFluid.JustWrote = 2;
                        if (ZNet.instance.IsServer())
                            MagicPortalFluid.YMLPortalData.Value = yamlfull; // is coop server so send update to client
                    }
                }

            }

            static void SetTeleportWorldColors(TeleportWorldDataRMP teleportWorldData, bool SetcolorTarget = false, bool SetMaterial = false)
            {

                teleportWorldData.OldColor = teleportWorldData.TargetColor;
                //Color Gold = new Color(1f, 215f / 255f, 0, 1f);
                //Color Cyan = Color.cyan

                if (teleportWorldData.TargetColor == Gold)
                {
                    try
                    {
                        Material mat = MagicPortalFluid.originalMaterials["shaman_prupleball"];
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
                        Material mat = MagicPortalFluid.originalMaterials["silver_necklace"];
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
            Material mat = MagicPortalFluid.originalMaterials["portal_small"];
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
                    teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor * 3;
                }
                else if (teleportWorldData.TargetColor == Color.cyan) // cyan now
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor * 4;
                }
                else
                    teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor * 7; // set color // set intensity very high
            }

        }

        static bool TryGetTeleportWorld(TeleportWorld key, out TeleportWorldDataRMP value)
        {
            if (key)
            {
                return MagicPortalFluid._teleportWorldDataCache.TryGetValue(key, out value);
            }

            value = default;
            return false;
        }

    }

    public static class Extensions
    {

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }
}

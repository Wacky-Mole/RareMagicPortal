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
using Random = System.Random;
using System.Xml.Linq;

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
        public static Color Purple = new Color(107f / 255f, 63f / 255f, 160f / 255f, 1f);
        public static Color Tan = new Color(210f / 255f, 180f / 255f, 140f / 255f, 1f);
        public static Color Brown = new Color(193f / 255f, 69f / 255f, 19f / 255f, 1f);
        public static Color Orange = new Color(204f / 255f, 85f / 255f, 0f, 1f);
        public static Color Cornsilk = new Color(1f, 248f/255f, 220f/255f, 1f);
        public static Color Yellow2 = new Color(139f/255f,128f/255f, 0f, 1f);

        internal static PortalName PortalN;
        internal static Player player = null; // need to keep it between patches
        private static int waitloop = 5;
        private static int rainbowWait=0;
        private static string currentRainbow = "Yellow";
        public static char NameIdentifier = '\u25B2';
        private static string BiomeStringTempHolder ="";
        internal static bool reloaded = false;

        public static List<ParticleSystem> CheatSwordColor { get; } = new List<ParticleSystem>();
        


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
            {nameof(PortalColor.Yellow),(Yellow2,(int)PortalColor.Yellow,         false, nameof(PortalColor.Red),   "Red Crystal Portal"  )},
            {nameof(PortalColor.Red), (Color.red,(int)PortalColor.Red,            false, nameof(PortalColor.Green), "Red Crystal Portal"  )},
            {nameof(PortalColor.Green), (Color.green,(int)PortalColor.Green,      false, nameof(PortalColor.Blue),  "Red Crystal Portal"  )},
            {nameof(PortalColor.Blue), (Color.blue,(int)PortalColor.Blue,         false, nameof(PortalColor.Purple),"Red Crystal Portal"  )},
            {nameof(PortalColor.Purple),( Purple,(int)PortalColor.Purple,         false, nameof(PortalColor.Tan),   "Red Crystal Portal"  )},
            {nameof(PortalColor.Tan), (Cornsilk,(int)PortalColor.Tan,             false, nameof(PortalColor.Cyan),  "Red Crystal Portal"  )},
            {nameof(PortalColor.Cyan), (Color.cyan,(int)PortalColor.Cyan,         false, nameof(PortalColor.Orange),"Red Crystal Portal"  )},
            {nameof(PortalColor.Orange),( Orange,(int)PortalColor.Orange,         false, nameof(PortalColor.White), "Red Crystal Portal"  )},
            {nameof(PortalColor.White), (Color.white,(int)PortalColor.White,      false, nameof(PortalColor.Black), "Red Crystal Portal"  )},
            {nameof(PortalColor.Black), (Color.black,(int)PortalColor.Black,      false, nameof(PortalColor.Gold),  "Red Crystal Portal"  )},
            {nameof(PortalColor.Gold), (Gold,(int)PortalColor.Gold,               false, nameof(PortalColor.Yellow),"Red Crystal Portal"  )}
       
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
            static void TeleportWorldAwakepRfixRMP(ref TeleportWorld __instance)
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
                try
                {
                    if (__instance.m_nview.m_zdo.GetString(MagicPortalFluid._portalBiomeColorHashCode) == "skip")
                    {
                        RMP.LogDebug("Portal BiomeColor skip Awake");
                        MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData);
                        teleportWorldData.BiomeColor = "skip";
                    }
                }
                catch { }

            }




            [HarmonyPostfix]
            [HarmonyPriority(Priority.Low)]
            [HarmonyPatch(nameof(TeleportWorld.UpdatePortal))]
            static void TeleportWorldUpdatePortalPostfixRMP(ref TeleportWorld __instance)
            {
                if (//!ConfigEnableCrystalsNKeys.Value
                      !__instance
                    || !__instance.m_nview
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
                        if (MagicPortalFluid.PortalDrinkColor.Value == "Rainbow_switch")
                        {
                            Color newCol = Color.yellow;// default
                            Random rnd = new Random();
                            PortalColor currentC = (PortalColor)Enum.Parse(typeof(PortalColor), currentRainbow);
                            int pickcolor = rnd.Next(1, 12);
                            var colorna = currentC.Next();
                            for (int i = 1; i < pickcolor; i++)
                            {
                                colorna.Next();
                            }
                            currentRainbow = colorna.ToString();
                            //RMP.LogInfo("rainbow currently is " + colorna.ToString());
                            newCol = PortalColors[colorna.ToString()].HexName;
                            rainbowWait = 0;


                            if (newCol != teleportWorldData.OldColor)
                            {  // don't waste resources
                                teleportWorldData.TargetColor = newCol;
                                SetTeleportWorldColors(teleportWorldData, true);
                            }
                        }
                        else if (MagicPortalFluid.PortalDrinkColor.Value == "Rainbow" || MagicPortalFluid.PortalDrinkColor.Value == "rainbow")
                        {
                            if (CheatSwordColor.Count == 0)
                            {
                                CheatSwordColor.AddRange(ObjectDB.instance.GetItemPrefab("SwordCheat").GetComponentsInChildren<ParticleSystem>().Where(transform => transform.name == "Particle System"));
                            }
                            ParticleSystem.MinMaxGradient holdColor = null;
                            foreach (ParticleSystem system in CheatSwordColor)
                            {
                                ParticleSystem.MainModule main = system.main;
                                holdColor = main.startColor;

                            }

                            foreach (ParticleSystem system in teleportWorldData.Systems)
                            {
                                ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
                                colorOverLifetime.color = holdColor;

                                ParticleSystem.MainModule main = system.main;
                                main.startColor = holdColor;

                                system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["Flame"];
                            }
                        }
                        else

                        {
                            teleportWorldData.TargetColor = PortalColors[MagicPortalFluid.PortalDrinkColor.Value].HexName;
                            SetTeleportWorldColors(teleportWorldData, false, false);
                        }

                    }
                    else
                    {
                        //RMP.LogInfo("Hello you jerk");
                        string PortalName = __instance.m_nview.m_zdo.GetString("tag");

                        int colorint = CrystalandKeyLogicColor(out string currentcolor, out Color color, out string nextcolor, PortalName , __instance);

                        if (MagicPortalFluid.ConfigUseBiomeColors.Value) // obviously teleportWorldData needs to be set
                        {

                            if (PortalName.Contains(NameIdentifier)) // don't remove just remove anything past 1 that slipped through
                            {
                                var MorethanNecCount = PortalName.Count(f => f == NameIdentifier);// count
                                if (MorethanNecCount > 1)
                                {
                                    var index = PortalName.IndexOf(NameIdentifier);
                                    PortalName = PortalName.Substring(0, index);
                                    string newstring = PortalName + NameIdentifier + PortalColors[currentcolor].Pos;
                                    __instance.SetText(newstring);// correct string
                                }
                            }

                            if (teleportWorldData.BiomeColor != "skip" && teleportWorldData.BiomeColor != "")
                            {
                                //RMP.LogInfo("Should use BiomeColor " + currentcolor);
                                //BiomeLogicCheck(out currentcolor, out color, out nextcolor, out colorint, PortalName); // BiomeForce Check


                                /*
                                    var Biome = "Meadows"; // in case not set yet. // Should get set on hover

                                    if (__instance.m_nview.m_zdo.GetString(MagicPortalFluid._portalBiomeHashCode) != "")
                                        Biome = __instance.m_nview.m_zdo.GetString(MagicPortalFluid._portalBiomeHashCode);

                                    teleportWorldData.Biome = Biome;
                                    // RMP.LogInfo("Update Biome call");

                                    string BC = MagicPortalFluid.BiomeRepColors.Value;
                                    string[] BCarray = BC.Split(',');
                                    var results = Array.FindAll(BCarray, s => s.Contains(Biome));
                                    //RMP.LogInfo("Biome is currently " + Biome+ " BCcarry length "+BCarray.Length +" results " + results.Length);
                                    List<string> single = results[0].Split(':').ToList(); // should only be 1
                                    foreach (var col in PortalColors)
                                    {
                                        if (col.Key == single[1])
                                        {
                                            teleportWorldData.BiomeColor = col.Key;
                                            //PortalColorLogic.updateYmltoColorChange("", colorint); // No I shouldn't do it in pairs, needs to be individual
                                            color = col.Value.HexName;
                                            __instance.SetText(PortalName + NameIdentifier + col.Value.Pos);

                                        }
                                    }

                                    Color BiomeCol = PortalColors[teleportWorldData.BiomeColor].HexName;
                                    color = BiomeCol;
                                    */

                                if (color != teleportWorldData.OldColor)
                                {  // don't waste resources
                                    teleportWorldData.TargetColor = color;
                                    SetTeleportWorldColors(teleportWorldData, true);
                                }
                                return;
                            }

                        }
                        if (color != teleportWorldData.LinkColor)
                        {  // don't waste resources
                            teleportWorldData.TargetColor = color;
                            teleportWorldData.LinkColor = color;
                            SetTeleportWorldColors(teleportWorldData, true);
                        }
                    }
                }
                catch { } // catches beginning errors


            }
        }
        /*
        [HarmonyPatch(typeof(TextInput), nameof(TextInput.RequestText))]
        public static class PortalTextBoxOverride
        {
            internal static void Postfix(TextInput __instance, ref __results)
            {

            }

        }*/
        [HarmonyPriority(Priority.High)]
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

                    BiomeStringTempHolder = "";
                    if (PortalName.Contains(NameIdentifier))
                    {
                        BiomeStringTempHolder = PortalName;//PortalName.Substring(PortalName.IndexOf(NameIdentifier));
                        var index = PortalName.IndexOf(NameIdentifier);
                        PortalName = PortalName.Substring(0, index);// deletes
                        __instance.SetText(PortalName);
                    }


                    if (portal != null && PortalName != "" && PortalName != "Empty tag")
                    {
                        Player closestPlayer = Player.m_localPlayer; //Player.GetClosestPlayer(__instance.m_proximityRoot.position, 5f);
                        bool sameperson = false;
                        if (portal.m_creator == closestPlayer.GetPlayerID())
                            sameperson = true;

                        if (Input.GetKey(MagicPortalFluid.portalRMPKEY.Value.MainKey) && MagicPortalFluid.portalRMPKEY.Value.Modifiers.All(Input.GetKey) && (MagicPortalFluid.isAdmin || sameperson && !MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)) // creator can change it if enable crystals is off
                        {
                            MagicPortalFluid.Globaliscreator = sameperson; // set this for yml permissions


                            int colorint = CrystalandKeyLogicColor( out string currentcolorskip, out Color colorskip, out string nextcolorskip, PortalName);
                            colorint = PortalColors[nextcolorskip].Pos; // inc 1 color// should loop around for last one
                            Color setcolor = PortalColors[nextcolorskip].HexName; // inc for hexname


                            if (MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData))
                            {
                                teleportWorldData.TargetColor = setcolor;
                                RMP.LogInfo("setting color " + currentcolorskip+ setcolor);
                                teleportWorldData.BiomeColor = "skip";
                                if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal"))
                                {
                                    BiomeStringTempHolder = ""; // empty no longer relevent -- this will cause issues if a map goes from TargetPortal to normal
                                }
                                teleportWorldData.LinkColor = setcolor;
                                SetTeleportWorldColors(teleportWorldData, true);
                            }
                            __instance.m_nview.m_zdo.Set(MagicPortalFluid._teleportWorldColorHashCode, Utils.ColorToVec3(setcolor));
                            //__instance.m_nview.m_zdo.Set(_teleportWorldColorAlphaHashCode, color);
                            __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);
                            __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalBiomeColorHashCode, "skip");

                            updateYmltoColorChange(PortalName, colorint, teleportWorldData.BiomeColor); // update yaml
                            colorint = CrystalandKeyLogicColor(out string currentcolor, out Color color, out string nextcolor, PortalName, __instance);// Do this again now that it has been updated. 

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

            internal static void Postfix(TeleportWorld __instance, Humanoid human, bool hold)
            {
                if (hold)
                    return;
                if (__instance.m_nview.IsValid())
                {
                    if (BiomeStringTempHolder != "")
                    {
                        __instance.SetText(BiomeStringTempHolder);
                        RMP.LogInfo("BiomeHolder CopiedBack name");
                    }

                }
            }
        }
        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.TargetFound))]
        [HarmonyPriority(Priority.High)]
        public static class TeleportHaveTargetFix
        {
            private static string HoldMeDaddy = "";
            static void Prefix(ref TeleportWorld __instance) {

                string PortalName = __instance.m_nview.m_zdo.GetString("tag");

                HoldMeDaddy = "";
                if (PortalName.Contains(NameIdentifier))
                {
                    HoldMeDaddy = PortalName;
                    var index = PortalName.IndexOf(NameIdentifier);
                    PortalName = PortalName.Substring(0, index);// deletes
                    __instance.SetText(PortalName);
                }

            }
            [HarmonyPriority(Priority.Low)]
            static void Postfix(ref TeleportWorld __instance)
            {
                if(HoldMeDaddy != "")
                    __instance.SetText(HoldMeDaddy);
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
                colorint = CrystalandKeyLogicColor(out currentcolor, out currentcolorHex, out nextcolor, PortalName, __instance);

                string tempholdstring = "";
                if (PortalName.Contains(NameIdentifier))
                {
                    tempholdstring = PortalName.Substring(PortalName.IndexOf(NameIdentifier));
                    int indexremove = __result.IndexOf(tempholdstring);
                    string cleanPath = (indexremove < 0)
                        ? __result
                        : __result = __result.Remove(indexremove, tempholdstring.Length);

                    var index = PortalName.IndexOf(NameIdentifier);
                    PortalName = PortalName.Substring(0, index);
                    //RMP.LogInfo("PortalName " + PortalName);
                }


                string text;
                Color color = currentcolorHex;
                text = currentcolor + " " + "Crystal Portal";

                if (PortalName == "" && currentcolor != MagicPortalFluid.CrystalKeyDefaultColor.Value && MagicPortalFluid.JustSent == 0)
                {
                    if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "None" || MagicPortalFluid.CrystalKeyDefaultColor.Value == "none")
                        colorint = 1;
                    else
                    {
                        try
                        {
                            colorint = PortalColors[ MagicPortalFluid.CrystalKeyDefaultColor.Value].Pos;
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
                || __instance.m_nview.m_zdo.GetString(MagicPortalFluid._portalBiomeHashCode) == "")
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
                            var results = Array.FindAll(BCarray, s => s.Contains(Biome));
                            //RMP.LogInfo("Biome is currently " + Biome+ " BCcarry length "+BCarray.Length +" results " + results.Length);
                            List<string> single = results[0].Split(':').ToList(); // should only be 1
                            foreach (var col in PortalColors)
                            {
                                if (col.Key == single[1])
                                {
                                    teleportWorldData.BiomeColor = col.Key;
                                    //PortalColorLogic.updateYmltoColorChange("", colorint); // No I shouldn't do it in pairs, needs to be individual
                                    newColor = col.Value.HexName;
                                    tempholdstring = ("" + NameIdentifier + col.Value.Pos);  
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
                       updateYmltoColorChange("", colorint); // update only once
                }// end FirstPass
                bool write = false;
                if (MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData2)) // run at every hover unfortunetly
                {
                    if (MagicPortalFluid.ConfigUseBiomeColors.Value && (tempholdstring == "" || reloaded))
                    {  
                        if (teleportWorldData2.BiomeColor == "skip") { }else
                        {
                            //RMP.LogWarning("Calc Biome");

                            string BC = MagicPortalFluid.BiomeRepColors.Value;
                            string[] BCarray = BC.Split(',');
                            var results = Array.FindAll(BCarray, s => s.Contains(Biome));
                            List<string> single = results[0].Split(':').ToList(); // should only be 1
                            foreach (var col in PortalColors)
                            {
                                if (col.Key == single[1])
                                {
                                    teleportWorldData2.BiomeColor = col.Key;
                                    //PortalColorLogic.updateYmltoColorChange("", colorint); // No I shouldn't do it in pairs, needs to be individual
                                    tempholdstring = ("" + NameIdentifier + col.Value.Pos);
                                    write = true;
                                }
                            }
                            

                        }
                    }
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
                                    "#" + ColorUtility.ToHtmlStringRGB(color),
                                    "#" + ColorUtility.ToHtmlStringRGB(PortalColors[nextcolor].HexName)
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
                                    "#"+ColorUtility.ToHtmlStringRGB(color),
                                    "#" + ColorUtility.ToHtmlStringRGB(PortalColors[nextcolor].HexName)
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
                                    "#" + ColorUtility.ToHtmlStringRGB(color)
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
                                    "#" + ColorUtility.ToHtmlStringRGB(color)
                                    );
                        }

                    }
                }
                else// name = ""
                {
                    string jo = "Please name Portal, ";
                    string hi = " Color ";
                    __result =
                                string.Format(
                                    "{0}\n<size={1}><color={3}>{2}{5}</color><color={3}>{6}</color></size>",
                                    __result,
                                    15,
                                    jo,
                                    "Yellow",
                                    "#" + ColorUtility.ToHtmlStringRGB(color),
                                    hi,
                                    currentcolor
                                    );

                }
                
                string comparestring = PortalName + tempholdstring;
                if (__instance.m_nview.m_zdo.GetString("tag") != comparestring && write)
                {
                    __instance.SetText(comparestring);
                }
            }
        }

        #endregion
          
        internal static void BiomeLogicCheck(out string currentColor, out Color currentColorHex, out string nextcolor, out int Pos, string PortalName = "", bool skip = false) // for the ones that don't have an __instance
        {
            string BiomeC = "";
            currentColor = "skip";
            currentColorHex = Gold;
            nextcolor = "White";
            Pos = 0;
            if (PortalName.Contains(NameIdentifier))
            {
                BiomeC = PortalName.Substring(PortalName.IndexOf(NameIdentifier));//
                var index = PortalName.IndexOf(NameIdentifier);
                PortalName = PortalName.Substring(0, index);
            }
            if (BiomeC != "" && MagicPortalFluid.ConfigUseBiomeColors.Value && !skip)
            {
                // RMP.LogInfo("BiomeC info is " + BiomeC);
                BiomeC = BiomeC.Remove(0, 1);
                int intS = Int32.Parse(BiomeC);
                PortalColor pcol = (PortalColor)intS;
                currentColor = pcol.ToString();
                //RMP.LogInfo("BiomeC Colar is " + currentColor);
                currentColorHex = PortalColors[currentColor].HexName;
                nextcolor = PortalColors[currentColor].NextColor;
                Pos = PortalColors[currentColor].Pos;

            }
        }
        internal static int CrystalandKeyLogicColor(out string currentColor, out Color currentColorHex, out string nextcolor, string PortalName = "", TeleportWorld __instance = null, int overrideInt = 0)
        {
            string BiomeC = "";
            if (PortalName.Contains(NameIdentifier))
            {
                BiomeC = PortalName.Substring(PortalName.IndexOf(NameIdentifier));//
                var index = PortalName.IndexOf(NameIdentifier);
                PortalName = PortalName.Substring(0, index);
            }

            int CrystalForPortal = MagicPortalFluid.ConfigCrystalsConsumable.Value;
            bool OdinsKin = false;
            bool Free_Passage = false;
           // RMP.LogInfo("here 1");
            if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
            {
                WritetoYML(PortalName);
            }
            OdinsKin = PortalN.Portals[PortalName].Admin_only_Access;
            Free_Passage = PortalN.Portals[PortalName].Free_Passage;
            var Portal_Crystal_Cost = PortalN.Portals[PortalName].Portal_Crystal_Cost; // rgbG  // 0 means it can't be used, (Keys only) anything greater means the cost. -1 means same as 0
            var Portal_Key = PortalN.Portals[PortalName].Portal_Key; // rgbG
            string BiomeForceColor = PortalN.Portals[PortalName].BiomeColor;
            // So the logic is at start it check is BiomeColor is null or skip, this gets set in hover, it gets overwritten in interact(because admin or owner said no)
            // special cases  if admin set then all portals are admin- don't override, freepassage don't override, teleportanything, don't override
            // We dont' actually want to set the yaml main color, but now we want indivudal portals be be overwritten manually. 
            // we need to only check the zdo or teleportWorldData if it's not skip or color, because other than that we have no reference,
            // this could be a problem for some logic- specificly targetworld data- this can't be pulled in its instance, 
            // I don't know how to fix this for TargetPortal other than changing the portalname to give me info 
            // I could use BiomeColor skip as a sort of guess. If it's not skip then most likly it is default force Biome
            // use the pos of pin. vector3 to identify portal and get Color
            // set an identifier on tagname- yep- sucks, but you got do what got do
            // well that won't work because you need the exact name to do the connect check. I could patch that I guess
            // Fuck... Either I do the name thing with patches or I pretty much remake target Portal in RMP to get the instance of the portals for each icon. 
            // damn name thing still - fuck i finally got it. 
            //RMP.LogInfo("here 2");
            if (PortalName != "" && PortalName != "Empty tag")
            {
                if (OdinsKin && MagicPortalFluid.AdminColor.Value != "none")
                {
                    //RMP.LogInfo("Logic 1");
                    currentColor = MagicPortalFluid.AdminColor.Value;
                    currentColorHex = PortalColors[MagicPortalFluid.AdminColor.Value].HexName;
                    nextcolor = PortalColors[MagicPortalFluid.AdminColor.Value].NextColor;
                    return PortalColors[currentColor].Pos;
                }

                if (Free_Passage && MagicPortalFluid.FreePassageColor.Value != "none")
                {
                    //RMP.LogInfo("Logic 2");
                    currentColor = MagicPortalFluid.FreePassageColor.Value;
                    currentColorHex = PortalColors[MagicPortalFluid.FreePassageColor.Value].HexName;
                    nextcolor = PortalColors[MagicPortalFluid.FreePassageColor.Value].NextColor;
                    return PortalColors[MagicPortalFluid.FreePassageColor.Value].Pos;
                }

                if (PortalN.Portals[PortalName].TeleportAnything && MagicPortalFluid.TelePortAnythingColor.Value != "none")
                {
                    //RMP.LogInfo("Logic 3");
                    currentColor = MagicPortalFluid.PortalDrinkColor.Value;
                    currentColorHex = PortalColors[MagicPortalFluid.PortalDrinkColor.Value].HexName;
                    nextcolor = PortalColors[MagicPortalFluid.PortalDrinkColor.Value].NextColor;
                    return PortalColors[MagicPortalFluid.PortalDrinkColor.Value].Pos;
                }
            }
            //RMP.LogInfo("here 3");
            if (BiomeC != "" && MagicPortalFluid.ConfigUseBiomeColors.Value)
            {
                if (__instance == null)
                {
                    //RMP.LogInfo("BiomeC info is " + BiomeC);
                    BiomeC = BiomeC.Remove(0, 1);
                    int intS = Int32.Parse(BiomeC);
                    PortalColor pcol = (PortalColor)intS;
                    currentColor = pcol.ToString();
                    currentColorHex = PortalColors[currentColor].HexName;
                    nextcolor = PortalColors[currentColor].NextColor;
                    return PortalColors[currentColor].Pos;
                }
                else
                {
                    MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData);
                    if (teleportWorldData.BiomeColor != "skip")
                    {
                        //RMP.LogInfo("BiomeC info is " + BiomeC);
                        BiomeC = BiomeC.Remove(0, 1);
                        int intS = Int32.Parse(BiomeC);
                        PortalColor pcol = (PortalColor)intS;
                        currentColor = pcol.ToString();
                        currentColorHex = PortalColors[currentColor].HexName;
                        nextcolor = PortalColors[currentColor].NextColor;
                        return PortalColors[currentColor].Pos;

                    }
                        

                }
            }
            //RMP.LogInfo("here 4");
            if (PortalName == "") {

                if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "None" || MagicPortalFluid.CrystalKeyDefaultColor.Value == "none")
                {
                    currentColor = MagicPortalFluid.FreePassageColor.Value;
                    currentColorHex = PortalColors[MagicPortalFluid.FreePassageColor.Value].HexName;
                    nextcolor = PortalColors[MagicPortalFluid.FreePassageColor.Value].NextColor;
                    return PortalColors[MagicPortalFluid.FreePassageColor.Value].Pos;
                }
                currentColor = MagicPortalFluid.CrystalKeyDefaultColor.Value;
                currentColorHex = PortalColors[MagicPortalFluid.CrystalKeyDefaultColor.Value].HexName;
                nextcolor = PortalColors[MagicPortalFluid.CrystalKeyDefaultColor.Value].NextColor;
                return PortalColors[MagicPortalFluid.CrystalKeyDefaultColor.Value].Pos;

            }
            //RMP.LogInfo("here 5");
            foreach (var pc in PortalColors)
            {
                var name = pc.Key;
                if (pc.Value.Enabled) {
                    if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value && pc.Key == "Gold")
                        continue;
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
                        //
                        //WritetoYML(PortalName, "Tan");
                        }// not in file so maybe add?
                }
            }
            if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value && (Portal_Crystal_Cost["Gold"] > 0 || Portal_Key["Gold"])) // Gold Check
            {
                currentColor = "Gold";
                currentColorHex = PortalColors[currentColor].HexName;
                nextcolor = PortalColors[currentColor].NextColor; //PortalColor.Red.ToString();
                return PortalColors[currentColor].Pos;
            }

            RMP.LogInfo("Warning- Logic going to default yellow");
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
            //if (BiomeCol != null) Setting BiomeColor doesn't make since when it only tracks a pair of Portals and not each indiv
                // PortalN.Portals[PortalName].BiomeColor = BiomeCol;

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
            if (BiomeCol != null)
             PortalN.Portals[PortalName].BiomeColor = BiomeCol;


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

        internal static bool CrystalandKeyLogic(string PortalName, string BiomeColor="")
        {
            int CrystalForPortal = MagicPortalFluid.ConfigCrystalsConsumable.Value;
            bool OdinsKin = false;
            bool Free_Passage = false;

            string BiomeC = "";
            string currentColor = "";
            var flag = false;
            if (PortalName.Contains(NameIdentifier) )
            {
                BiomeC = PortalName.Substring(PortalName.IndexOf(NameIdentifier)+1);//
                var index = PortalName.IndexOf(NameIdentifier);
                PortalName = PortalName.Substring(0, index);
                if (MagicPortalFluid.ConfigUseBiomeColors.Value && BiomeColor != "skip")
                {
                    flag = true;
                    int intS = Int32.Parse(BiomeC);
                    PortalColor pcol = (PortalColor)intS;
                    currentColor = pcol.ToString();
                }
            }

            MagicPortalFluid.RareMagicPortal.LogInfo($"Portal name is {PortalName}");
            if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
            {
                WritetoYML(PortalName);
            }

            OdinsKin = PortalN.Portals[PortalName].Admin_only_Access;
            Free_Passage = PortalN.Portals[PortalName].Free_Passage;
            var Portal_Crystal_Cost = PortalN.Portals[PortalName].Portal_Crystal_Cost; // rgbG  // 0 means it can't be used, (Keys only) anything greater means the cost. -1 means same as 0
            var Portal_Key_Cost = PortalN.Portals[PortalName].Portal_Key; // rgbG
            var TeleportEvery = PortalN.Portals[PortalName].TeleportAnything;

            

            if (OdinsKin && MagicPortalFluid.isAdmin && !flag || currentColor == MagicPortalFluid.AdminColor.Value)
            {
                player.Message(MessageHud.MessageType.TopLeft, "$rmp_kin_welcome"); // forgot this one
                return true;
            }

            else if (OdinsKin && !MagicPortalFluid.isAdmin && MagicPortalFluid.ConfigEnableCrystalsNKeys.Value && !flag) // If requires admin, but not admin, but only with enable crystals otherwise just a normal portal
            {
                player.Message(MessageHud.MessageType.Center, "$rmp_kin_only");
                //Teleporting = false;
                return false;

            }

            if (TeleportEvery && !flag || currentColor == MagicPortalFluid.TelePortAnythingColor.Value) // if no crystals, then just white, if crystals then free passage
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

                if (Free_Passage && !flag || currentColor == MagicPortalFluid.FreePassageColor.Value)
                {
                    player.Message(MessageHud.MessageType.TopLeft, "$rmp_freepassage");
                    return true;
                }


                CrystalCount[nameof(PortalColor.Gold)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorGold.Value);
                CrystalCount[nameof(PortalColor.Red)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorRed.Value);
                CrystalCount[nameof(PortalColor.Green)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorGreen.Value);
                CrystalCount[nameof(PortalColor.Blue)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorBlue.Value);
                CrystalCount[nameof(PortalColor.Purple)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorPurple.Value);
                CrystalCount[nameof(PortalColor.Tan)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorTan.Value);
                CrystalCount[nameof(PortalColor.Yellow)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorYellow.Value);
                CrystalCount[nameof(PortalColor.White)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorWhite.Value);
                CrystalCount[nameof(PortalColor.Black)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorBlack.Value);
                CrystalCount[nameof(PortalColor.Cyan)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorCyan.Value);
                CrystalCount[nameof(PortalColor.Orange)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorOrange.Value);

                KeyCount[nameof(PortalColor.Gold)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyGold);
                KeyCount[nameof(PortalColor.Red)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyRed);
                KeyCount[nameof(PortalColor.Green)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyGreen);
                KeyCount[nameof(PortalColor.Blue)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyBlue);
                KeyCount[nameof(PortalColor.Purple)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyPurple);
                KeyCount[nameof(PortalColor.Tan)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyTan);
                KeyCount[nameof(PortalColor.White)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyWhite);
                KeyCount[nameof(PortalColor.Yellow)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyYellow);
                KeyCount[nameof(PortalColor.Black)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyBlack);
                KeyCount[nameof(PortalColor.Cyan)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyCyan);
                KeyCount[nameof(PortalColor.Orange)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyOrange);

                if (flag) // override PortalName
                {
                    Portal_Crystal_Cost = new Dictionary<string, int>();
                    Portal_Key_Cost = new Dictionary<string, bool>();

                    Portal_Crystal_Cost.Add(currentColor, MagicPortalFluid.ConfigCrystalsConsumable.Value);
                    Portal_Key_Cost.Add(currentColor, true);

                    if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value)
                    {
                        Portal_Crystal_Cost.Add("Gold", MagicPortalFluid.ConfigCrystalsConsumable.Value);
                        Portal_Key_Cost.Add("Gold", true);
                    }

                }

                int flagCarry = 0; // don't have any keys or crystals
                int crystalorkey = 0;// 0 is crystal, 1 is key, 2 is both
                bool foundAccess = false;
                int lowest = 0;

                int coun = PortalColors.Count;
                foreach (var col in PortalColors)
                {
                    if (!Portal_Crystal_Cost.TryGetValue(col.Key, out int Stuff)) // true then not in there
                        continue;

                    if ((Portal_Crystal_Cost[col.Key] > 0 || Portal_Key_Cost[col.Key]) && !foundAccess)
                    {
                        if (CrystalCount[col.Key] == 0) 
                            flagCarry = col.Value.Pos;
                        else if (Portal_Crystal_Cost[col.Key] > CrystalCount[col.Key]) // has less than required
                            flagCarry = 100+ col.Value.Pos;
                        else flagCarry = 200+ col.Value.Pos; // has more than required

                        if (Portal_Key_Cost[col.Key])
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
                        
                    
                    if (flagCarry > 200)
                        foundAccess = true;
                    if (flagCarry < 200 && lowest == 0)
                        lowest = flagCarry;

                        //RMP.LogInfo("FlagCarry for " + col.Key + " " + flagCarry + " Lowest " + lowest + "FoundAccess " + foundAccess);
                    }

                }// for every color

                //RMP.LogInfo("FlagCarry before " + flagCarry + " Lowest " + lowest + "FoundAccess "+ foundAccess);
            if (flagCarry < 22 && lowest == 0) // not sure what this is for I think it is important though
                lowest = flagCarry;

            if (flagCarry == 22 && lowest != 0) // for gold override 
                flagCarry = lowest;


            string CorK = "$rmp_crystals";
            if (crystalorkey == 1)
                CorK = "$rmp_key";
            if (crystalorkey == 2)
                CorK = "$rmp_crystalorkey";


            var hud = MessageHud.MessageType.Center;
            if (MagicPortalFluid.ConfigMessageLeft.Value)
                hud = MessageHud.MessageType.TopLeft;
                //RMP.LogInfo("FlagCarry " + flagCarry + " Lowest " + lowest);
                switch (flagCarry)
                {
                    case 0:
                        player.Message(hud, $"$rmp_noaccess {CorK}"); // yellow maybe change permissions for yellow
                        return false;
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
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalYellow, Portal_Crystal_Cost["Yellow"]);
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
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalCyan, Portal_Crystal_Cost["Cyan"]);
                        return true;
                    case 208:
                        player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Orange"]} $rmp_consumed_orange");
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalOrange, Portal_Crystal_Cost["Orange"]);
                        return true;
                    case 220:
                        player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["White"]} $rmp_consumed_white");
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalWhite, Portal_Crystal_Cost["White"]);
                        return true;
                    case 221:
                        player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Black"]} $rmp_consumed_black");
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalBlack, Portal_Crystal_Cost["Black"]);
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
            RMP.LogInfo("Writing New YML");
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
            else if (teleportWorldData.TargetColor == Color.white)
            {
                try
                {
                    Material mat = MagicPortalFluid.originalMaterials["crystal_exterior"];
                    foreach (Renderer red in teleportWorldData.MeshRend)
                    {
                        red.material = mat;
                    }
                }
                catch { }
            } */
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
            else {          // sets back to default 
                Material mat = MagicPortalFluid.originalMaterials["portal_small"];
                foreach (Renderer red in teleportWorldData.MeshRend)
                {
                    red.material = mat;
                }
            }

        foreach (Light light in teleportWorldData.Lights)
        {
            /*
            if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
            {
                light.color = lightcolor;
            } */

                light.color = teleportWorldData.TargetColor;
        }


        foreach (ParticleSystem system in teleportWorldData.Sucks)
        {
            ParticleSystem.MainModule main = system.main;
                if (teleportWorldData.TargetColor == Color.white)
                {
                    main.startColor = teleportWorldData.TargetColor;
                    main.maxParticles = 1000;
                }
                else if (teleportWorldData.TargetColor == Orange)
                {
                   main.startColor = teleportWorldData.TargetColor;
                    main.maxParticles = 1000;
                }
                else if (teleportWorldData.TargetColor == Color.black)
                {
                    main.startColor = Color.white;
                    main.maxParticles = 1000;
                }
                else if (teleportWorldData.TargetColor == Yellow2)
                {
                    main.startColor = Yellow2;
                    main.maxParticles = 1000;
                }
                else if (teleportWorldData.TargetColor == Cornsilk)
                {
                    main.startColor = Brown;
                    main.maxParticles = 30;
                }
                else
                { 
                    main.startColor = Color.black;
                    main.maxParticles = 1000;

                }


        }
        

        foreach (ParticleSystem system in teleportWorldData.Systems)
        {
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(teleportWorldData.TargetColor, teleportWorldData.TargetColor);
            /*    
            if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
            {
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(flamesstart, flamesend);
            } */

            ParticleSystem.MainModule main = system.main;

            //system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["Portal_flame"];
            main.startColor = teleportWorldData.TargetColor;
            }


            foreach (Material material in teleportWorldData.Materials) // flames
            {
                
                if (teleportWorldData.TargetColor == Purple) // trying to reset to default
                {
                    material.color = new Color(63f/255f,0f, 231f/255f,1f ); // wierd purple problem
                }
                else if (teleportWorldData.TargetColor == Cornsilk) // trying to reset to default
                {
                    material.color = Brown; 
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
                else if (teleportWorldData.TargetColor == Cornsilk)
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = Brown *3;
                }
                else if (teleportWorldData.TargetColor == Color.cyan) // cyan now
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor * 4;
                }
                else if (teleportWorldData.TargetColor == Color.blue) // cyan now
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor * 7;
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

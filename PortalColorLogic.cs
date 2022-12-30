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


        internal enum PortalColor // gold - master should always be last or highest int
        {
            Yellow = 1,
            Red =2,
            Green = 3,
            Blue = 4,
            Purple = 5,
            Brown = 6,
            Cyan = 7,
            Orange=8,
            White = 20,
            Black = 21,
            Gold = 22,

        }

        internal static Dictionary<(String Name, Color HexName), int> PortalColors = new Dictionary<(String, Color), int>()
        {
            {(nameof(PortalColor.Yellow), Color.yellow),(int)PortalColor.Yellow },
            {(nameof(PortalColor.Red), Color.red),(int)PortalColor.Red },
            {(nameof(PortalColor.Green), Color.green),(int)PortalColor.Green },
            {(nameof(PortalColor.Blue), Color.blue),(int)PortalColor.Blue },
            {(nameof(PortalColor.Purple), Purple),(int)PortalColor.Purple },
            {(nameof(PortalColor.Brown), Brown),(int)PortalColor.Brown },
            {(nameof(PortalColor.Cyan), Color.cyan),(int)PortalColor.Cyan },
            {(nameof(PortalColor.Orange), Color.yellow),(int)PortalColor.Orange },
            {(nameof(PortalColor.White), Color.white),(int)PortalColor.White },
            {(nameof(PortalColor.Black), Color.black),(int)PortalColor.Black },
            {(nameof(PortalColor.Gold), Gold),(int)PortalColor.Gold }
       

            /*
            { "Yellow", (Color.yellow, 1 )},
            { "Red",(Color.red, 2 ) },
            {"Green",(Color.green, 3 ) },
            {"Blue",(Color.blue, 4 ) },
            {"Purple",(Purple, 5 ) },
            {"Brown",(Brown, 6 )  },
            {"Cyan",(Color.cyan, 7 )  },
            {"Orange",(Color.gray, 8 )  },
            { "Gold", (Gold, 20 ) },
            {"White",(Color.white, 21 )  },
            {"Black",(Color.black, 22)  }
            */
        };



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
               // RMP.LogInfo("Update Portal Called and passed");
                try
                {
                    bool isthistrue = MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData);
                    //TryGetTeleportWorld
                    //RMP.LogInfo("Update Portal Called Post 1" + isthistrue);
                    if (Player.m_localPlayer.m_seman.GetStatusEffect("yippeTele") != null)
                    {
                        // override color for white
                        teleportWorldData.TargetColor = Color.white;
                        SetTeleportWorldColors(teleportWorldData, false, false);
                    }
                    else
                    {
                        //string PortalName = __instance.GetText();
                        string PortalName = __instance.m_nview.m_zdo.GetString("tag");
                        //RMP.LogInfo("Update Portal Called Post 3 WITH NAME " + PortalName);
                        int colorint = CrystalandKeyLogicColor(PortalName); // this should sync up portal colors

                        //Color CurrentZDOColor = Utils.Vec3ToColor(__instance.m_nview.m_zdo.m_vec3.GetValueSafe(_teleportWorldColorHashCode));
                       // if (MagicPortalFluid.ConfigUseBiomeColors.Value)
                        //{

                       // }
                        Color color;
                        string currentcolor;
                        switch (colorint)
                        {
                            case 0:
                                currentcolor = "Black";
                                color = Color.black;
                                break;
                            case 1:
                                currentcolor = "Yellow";
                                color = Color.yellow;
                                break;
                            case 2:
                                currentcolor = "Red";
                                color = Color.red;
                                break;
                            case 3:
                                currentcolor = "Green";
                                color = Color.green;
                                break;
                            case 4:
                                currentcolor = "Blue";
                                color = Color.cyan;
                                break;
                            case 5:
                                currentcolor = "Purple";
                                color = Purple;
                                break;
                            case 6:
                                currentcolor = "Tan";
                                color = Tan;
                                break;
                            case 7:
                                currentcolor = "Gold";
                                color = Gold;
                                break;
                            case 8:
                                currentcolor = "White";
                                color = Color.white;
                                break;
                            default:
                                currentcolor = "Yellow";
                                color = Color.yellow;
                                break;

                        }
                        //if (!__instance.m_nview.m_zdo.m_vec3.ContainsKey(_teleportWorldColorHashCode))
                        //	__instance.m_nview.m_zdo.Set(_teleportWorldColorHashCode, Utils.ColorToVec3(color));

                       // RMP.LogInfo("PortalName " + PortalName + "color is " + color + " old color " + teleportWorldData.OldColor);
                        if (color != teleportWorldData.OldColor)
                        {  // don't waste resources
                            teleportWorldData.TargetColor = color;
                            SetTeleportWorldColors(teleportWorldData, true);
                        }

                    }
                    //if (TargetPortalLoaded)
                    //__instance.gameObject.transform.Find("_target_found").gameObject.SetActive(false);


                }
                catch {  } // catches beginning errors


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
                                                          //	RareMagicPortal.LogInfo($"Made it to Map during teleworldcache");

                            int colorint = PortalColorLogic.CrystalandKeyLogicColor(PortalName);
                            string currentcolor = "Default";
                            string nextcolor;
                            string text;
                            Color color = Color.yellow;
                            Color Gold = new Color(1f, 215f / 255f, 0, 1f);

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

                            if (MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData))
                            {
                                teleportWorldData.TargetColor = color;
                                SetTeleportWorldColors(teleportWorldData, true);
                            }
                            __instance.m_nview.m_zdo.Set(MagicPortalFluid._teleportWorldColorHashCode, Utils.ColorToVec3(color));
                            //__instance.m_nview.m_zdo.Set(_teleportWorldColorAlphaHashCode, color);
                            __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);

                            // now need to set the yml file to update with these changes on interact
                            if (colorint == 8) // interate one up
                                colorint = 0;
                            else colorint++;

                            PortalColorLogic.updateYmltoColorChange(PortalName, colorint);

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

                //RMP.LogInfo("Biome Currently in is " + closestPlayer.GetCurrentBiome());


                string PortalName = __instance.m_nview.m_zdo.GetString("tag");
                int colorint = 1;

                colorint = PortalColorLogic.CrystalandKeyLogicColor(PortalName);


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

                if (PortalName == "" && currentcolor != MagicPortalFluid.CrystalKeyDefaultColor.Value && MagicPortalFluid.JustSent == 0)
                {
                    if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "None")
                        colorint = 1;
                    else if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "none")
                        colorint = 1;
                    else if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Red")
                        colorint = 2;
                    else if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Green")
                        colorint = 3;
                    else if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Blue")
                        colorint = 4;
                    else if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Purple")
                        colorint = 5;
                    else if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Tan")
                        colorint = 6;
                    else
                    {
                        RMP.LogWarning($"DefaultPortalColor {MagicPortalFluid.CrystalKeyDefaultColor.Value} is not an option,this will cause repeating network traffic on no name portals");
                    }

                }
                if (
                !__instance.m_nview
                || __instance.m_nview.m_zdo == null
                || __instance.m_nview.m_zdo.m_zdoMan == null
                || __instance.m_nview.m_zdo.m_vec3 == null)
                {
                    RMP.LogInfo("Setting Portal Color For First Time");
                    if (MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData))
                    {
                        teleportWorldData.TargetColor = color;
                        SetTeleportWorldColors(teleportWorldData, true);
                        __instance.m_nview.m_zdo.Set(MagicPortalFluid._teleportWorldColorHashCode, Utils.ColorToVec3(color));
                        __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);
                        RMP.LogInfo("Setting ZDO Color For First Time");
                    }
                    if (PortalName == "")
                        PortalColorLogic.updateYmltoColorChange("", colorint); // update only once
                }



                if (PortalName != "" && PortalName != "Empty tag")
                {
                    if (MagicPortalFluid.isAdmin || sameperson && !MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)
                    {
                        //string Shortcut

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
                                    "{0}\n<size={4}>[<color={5}>{2}</color>] Change <color={1}>Portal</color>[{1}] Crystal to: [<color={3}>{3}</color>]</size>\n<size={4}>{6}</size>",
                                    __result,
                                    currentcolor,
                                    MagicPortalFluid.portalRMPKEY.Value + " + " + "E",//_changePortalReq,
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
                                    MagicPortalFluid.portalRMPKEY.Value + " + " + "E", //_changePortalReq,
                                    nextcolor,
                                    15,
                                    "Yellow"
                                    );
                        }
                    }
                    else
                    {
                        if (MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)
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
                }
                else// name = ""
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
                return 0;
            }

            if (Free_Passage)
                return 1;

            if (PortalN.Portals[PortalName].TeleportAnything)
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
            }
            catch
            {
                RMP.LogInfo("No Purple in File for Portal, adding one");
                WritetoYML(PortalName, "Purple");
            }
            try
            {
                if ((Portal_Crystal_Cost["Tan"] > 0 || Portal_Key["Tan"]))
                    return 6;
            }
            catch
            {
                RMP.LogInfo("No Tan in File for Portal, adding one");
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
        internal static void updateYmltoColorChange(string PortalName, int colorint)
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
                    if (MagicPortalFluid.ConfigEnableCrystalsNKeys.Value)
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
                    PortalN.Portals[PortalName].Admin_only_Access = false;

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
                    if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value)
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
                    if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value)
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
                    if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value)
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
                    if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value)
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
                    if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value)
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
            //Dictionary <string, int> Portal_Crystal_Cost = null; // rgbG
            //Dictionary <string, bool> Portal_Key; //rgbG

            MagicPortalFluid.RareMagicPortal.LogInfo($"Portal name is {PortalName}");
            if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
            {
                WritetoYML(PortalName);
            }


            //CrystalForPortal = MagicPortalFluid.PortalN.Portals[PortalName].Crystal_Cost_Master;
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
                MagicPortalFluid.RareMagicPortal.LogInfo("No Purple in File for Portal, adding one in CrystalKeyLogic");
                WritetoYML(PortalName, "Purple");
            }
            try// keep this for a while for people upgrading
            {
                if ((Portal_Crystal_Cost["Tan"] > 0 || Portal_Key["Tan"])) { }

            }
            catch
            {
                MagicPortalFluid.RareMagicPortal.LogInfo("No Tan in File for Portal, adding one in CrystalKeyLogic");
                WritetoYML(PortalName, "Tan");
            }

            //RareMagicPortal.LogInfo($"Portal IS ADMIN {OdinsKin} and is player admin? {isAdmin}");
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

                Dictionary<string,int> CrystalCount = new Dictionary<string, int>();
                Dictionary<string, int> KeyCount = new Dictionary<string, int>();

                CrystalCount.Add(nameof(PortalColor.Gold), player.m_inventory.CountItems(MagicPortalFluid.CrystalMaster));
                CrystalCount.Add(nameof(PortalColor.Red), player.m_inventory.CountItems(MagicPortalFluid.CrystalRed));
                CrystalCount.Add(nameof(PortalColor.Green), player.m_inventory.CountItems(MagicPortalFluid.CrystalGreen));
                CrystalCount.Add(nameof(PortalColor.Blue), player.m_inventory.CountItems(MagicPortalFluid.CrystalBlue));
                CrystalCount.Add(nameof(PortalColor.Purple), player.m_inventory.CountItems(MagicPortalFluid.CrystalPurple));
                CrystalCount.Add(nameof(PortalColor.Brown), player.m_inventory.CountItems(MagicPortalFluid.CrystalTan));

                KeyCount.Add(nameof(PortalColor.Gold), player.m_inventory.CountItems(MagicPortalFluid.PortalKeyGold));
                KeyCount.Add(nameof(PortalColor.Red), player.m_inventory.CountItems(MagicPortalFluid.PortalKeyRed));
                KeyCount.Add(nameof(PortalColor.Green), player.m_inventory.CountItems(MagicPortalFluid.PortalKeyGreen));
                KeyCount.Add(nameof(PortalColor.Blue), player.m_inventory.CountItems(MagicPortalFluid.PortalKeyBlue));
                KeyCount.Add(nameof(PortalColor.Purple), player.m_inventory.CountItems(MagicPortalFluid.PortalKeyPurple));
                KeyCount.Add(nameof(PortalColor.Brown), player.m_inventory.CountItems(MagicPortalFluid.PortalKeyTan));

                int flagCarry = 0; // don't have any keys or crystals
                int crystalorkey = 0;// 0 is crystal, 1 is key, 2 is both
                bool foundAccess = false;
                int lowest = 0;

                int coun = PortalColors.Count;
                foreach (var col in PortalColors)
                {
                    if (CrystalCount[col.Key.Name] > 0 || KeyCount[col.Key.Name] > 0)
                    {
                        if (CrystalCount[col.Key.Name] == 0) 
                            flagCarry = col.Value;
                        else if (Portal_Crystal_Cost[col.Key.Name] > CrystalCount[col.Key.Name]) // has less than required
                            flagCarry = 100+ col.Value;
                        else flagCarry = 200+ col.Value; // has more than required

                        if (Portal_Key[col.Key.Name])
                        {
                            if (Portal_Crystal_Cost[col.Key.Name] == 0)
                            {
                                crystalorkey = 1;
                                if (KeyCount[col.Key.Name] > 0)
                                    flagCarry = 300+col.Value;
                                else
                                    flagCarry = col.Value; // no crystal cost, but key cost with no key
                            }
                            else
                            {
                                if (KeyCount[col.Key.Name] > 0 && flagCarry< 200)
                                    flagCarry = 300+col.Value;
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
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalRed, Portal_Crystal_Cost["Red"]);
                        return true;
                    case 22:
                        player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Green"]} $rmp_consumed_green");
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalGreen, Portal_Crystal_Cost["Green"]);
                        return true;
                    case 33:
                        player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Blue"]} $rmp_consumed_blue");
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalBlue, Portal_Crystal_Cost["Blue"]);
                        return true;
                    case 44:
                        player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Purple"]} $rmp_consumed_purple");
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalPurple, Portal_Crystal_Cost["Purple"]);
                        return true;
                    case 55:
                        player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Tan"]} $rmp_consumed_tan");
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalTan, Portal_Crystal_Cost["Tan"]);
                        return true;
                    case 99:
                        player.Message(MessageHud.MessageType.Center, $"$rmp_crystalgrants_access");
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {Portal_Crystal_Cost["Gold"]} $rmp_consumed_gold");
                        player.m_inventory.RemoveItem(MagicPortalFluid.CrystalMaster, Portal_Crystal_Cost["Gold"]);
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
                if (updateP == "Purple")
                {
                    PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = 0;
                    PortalN.Portals[PortalName].Portal_Key["Purple"] = false;
                    colorint = 1;
                    RMP.LogWarning($"Your PortalName world has Portals that don't Contain Purple, this portal {PortalName} is being updated to default");
                }
                if (updateP == "Tan")
                {
                    PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = 0;
                    PortalN.Portals[PortalName].Portal_Key["Tan"] = false;
                    colorint = 1;
                    RMP.LogWarning($"Your PortalName world has Portals that don't Contain Tan, this portal {PortalName} is being updated to default");
                }
                else
                {
                    return;
                }
            }

            if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Red")
            {
                PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = MagicPortalFluid.ConfigCrystalsConsumable.Value;
                PortalN.Portals[PortalName].Portal_Key["Red"] = true;
                colorint = 2;

            }
            else
            {
                PortalN.Portals[PortalName].Portal_Crystal_Cost["Red"] = 0;
                PortalN.Portals[PortalName].Portal_Key["Red"] = false;
            }
            if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Green")
            {
                PortalN.Portals[PortalName].Portal_Crystal_Cost["Green"] = MagicPortalFluid.ConfigCrystalsConsumable.Value;
                PortalN.Portals[PortalName].Portal_Key["Green"] = true;
                colorint = 3;
            }
            if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Blue")
            {
                PortalN.Portals[PortalName].Portal_Crystal_Cost["Blue"] = MagicPortalFluid.ConfigCrystalsConsumable.Value;
                PortalN.Portals[PortalName].Portal_Key["Blue"] = true;
                colorint = 4;
            }

            if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Purple")
            {
                PortalN.Portals[PortalName].Portal_Crystal_Cost["Purple"] = MagicPortalFluid.ConfigCrystalsConsumable.Value;
                PortalN.Portals[PortalName].Portal_Key["Purple"] = true;
                colorint = 5;
            }

            if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "Tan")
            {
                PortalN.Portals[PortalName].Portal_Crystal_Cost["Tan"] = MagicPortalFluid.ConfigCrystalsConsumable.Value;
                PortalN.Portals[PortalName].Portal_Key["Tan"] = true;
                colorint = 6;
            }

            if (MagicPortalFluid.CrystalKeyDefaultColor.Value == "None")
            {
                PortalN.Portals[PortalName].Free_Passage = true;
                colorint = 1;
            }
            else
            {
                PortalN.Portals[PortalName].Portal_Crystal_Cost["Gold"] = MagicPortalFluid.ConfigCrystalsConsumable.Value; // by default always unless true
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
}

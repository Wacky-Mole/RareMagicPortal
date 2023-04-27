using System;
using System.Collections.Generic;
using UnityEngine;

namespace RareMagicPortal
{
    internal class functions
    {
        public static void GetAllMaterials()
        {
            Material[] array = Resources.FindObjectsOfTypeAll<Material>();
            MagicPortalFluid.originalMaterials = new Dictionary<string, Material>();
            Material[] array2 = array;
            foreach (Material val in array2)
            {
                // Dbgl($"Material {val.name}" );
                MagicPortalFluid.originalMaterials[val.name] = val;
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

        internal static void ServerZDOymlUpdate(int Colorint, string Portalname) // MESSAGE SENDER
        {
            if (ZNet.instance.IsServer())// && ZNet.instance.IsDedicated()) removed dedicated  // so no singleplayer announcement
                return;
            if (MagicPortalFluid.JustSent > 0)
            {
                MagicPortalFluid.JustSent++;
                return;
            }

            ZPackage pkg = new ZPackage(); // Create ZPackage

            pkg.Write(Portalname + "," + Colorint);
            MagicPortalFluid.RareMagicPortal.LogInfo($"Sending the Server a update for {Portalname} with Color {Colorint}");

            MagicPortalFluid.JustSent = 1;
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestServerAnnouncementRMP", new object[] { pkg });
        }

        public static void RPC_RequestServerAnnouncementRMP(long sender, ZPackage pkg) // MESSAGE RECIEVER
        {
            if (ZNet.instance.IsServer()) //&& ZNet.instance.IsDedicated() ) If any server than prepare to recieved message
            {
                if (pkg != null && pkg.Size() > 0)
                { // Check that our Package is not null, and if it isn't check that it isn't empty.
                    ZNetPeer peer = ZNet.instance.GetPeer(sender);
                    if (peer != null)
                    { // Confirm the peer exists
                      //string peerSteamID = ((ZSteamSocket)peer.m_socket).GetPeerID().m_SteamID.ToString(); no more steam
                        string playername = peer.m_playerName;// playername
                        string msg = pkg.ReadString();
                        string[] msgArray = msg.Split(',');
                        string PortalName = msgArray[0];
                        int Colorint = Convert.ToInt32(msgArray[1]);
                        MagicPortalFluid.RareMagicPortal.LogInfo($"Server has recieved a YML update from {playername} for {PortalName} with Color {Colorint}");

                        PortalColorLogic.updateYmltoColorChange(PortalName, Colorint);

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

        internal static string GetColorHtmlString(Color color)
        {
            return color.a == 1.0f
                ? ColorUtility.ToHtmlStringRGB(color)
                : ColorUtility.ToHtmlStringRGBA(color);
        }

        internal static string HandlePortalClick()
        {
            Minimap instance = Minimap.instance;
            List<Minimap.PinData> paul = instance.m_pins;
            Vector3 pos = instance.ScreenToWorldPoint(Input.mousePosition);
            float radius = instance.m_removeRadius * (instance.m_largeZoom * 2f);

            MagicPortalFluid.checkiftagisPortal = "";
            Minimap.PinData pinData = null;
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
                MagicPortalFluid.checkiftagisPortal = pinData.m_name; // icons name
            if (pinData.m_icon.name == "" || pinData.m_icon.name == "TargetPortalIcon") // only targetPortalIcon now, or not maybe new icons being set don't have name after messing with them
            { // TargetPortals Icons have no name therefore this stupid check weeds out regular icons  // pull request for icon.name  TargetPortalIcon
            }
            else
                MagicPortalFluid.checkiftagisPortal = null;

            if (MagicPortalFluid.checkiftagisPortal.Contains("$hud") || MagicPortalFluid.checkiftagisPortal.Contains("Day "))
                MagicPortalFluid.checkiftagisPortal = null;

            return MagicPortalFluid.checkiftagisPortal;
        }
    }
}
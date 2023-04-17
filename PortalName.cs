using System.Collections.Generic;

namespace RareMagicPortal
{
    public class PortalName
    {
        public Dictionary<string, Portal> Portals { get; set; }

        public class Portal
        {
            public Dictionary<string, int> Portal_Crystal_Cost =
                    new Dictionary<string, int>(){
                                  {"Red", 0},
                                  {"Green", 0},
                                  {"Blue", 0 },
                                  {"Purple", 0 },
                                  {"Tan", 0 },
                                  {"Gold", 0 }};

            public Dictionary<string, bool> Portal_Key =
                    new Dictionary<string, bool>(){
                                  {"Red", false},
                                  {"Green", false },
                                  {"Blue", false },
                                  {"Purple", false },
                                  {"Tan", false },
                                  {"Gold", false } };

            public bool Free_Passage { get; set; } = false;

            public bool TeleportAnything { get; set; } = false;

            public List<string> AdditionalProhibitItems { get; set; } = new List<string>();// { "Blackmetal", "Iron" };

            public string BiomeColor { get; set; }

            public int SpecialMode { get; set; } = 0; // 0 - normal, 1 - rainbow, 2 -password lock, - 3 Manual ID lock, -4 AllowedList is Moderator List
            public List<string> AllowedUsers { get; set; } = new List<string>();// { "SteamID1", "SteamID2" };

            public bool Admin_only_Access { get; set; } = false;
        }
    }
}
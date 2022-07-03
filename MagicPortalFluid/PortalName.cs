using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RareMagicPortal
{
    public class PortalName
    {
        public Dictionary<string, Portal> Portals { get; set; }

        public class Portal
        {
            public Dictionary <string, int> Portal_Crystal_Cost =
                    new Dictionary<string, int>(){
                                  {"Red", 1},
                                  {"Green", 0},
                                  {"Blue", 0 },
                                  {"Gold", 1 }};

            public Dictionary<string, bool> Portal_Key =
                    new Dictionary<string, bool>(){
                                  {"Red", true},
                                  {"Green", false },
                                  {"Blue", false },
                                  {"Gold", true } };

            public bool Free_Passage { get; set; } = false;
            public bool Admin_only_Access { get; set; } = false;

        }

    }
}

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


            public string Crystal_Cost { get; set; } = "1,none,none";

            public string Key_Requirments { get; set; } = "allow,none,none";

            public int Crystal_Cost_Master { get; set; }

            public bool Free_Passage { get; set; } = false;
            public bool Admin_only_Access { get; set; } = false;

        }

    }
}

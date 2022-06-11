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
            public int Crystal_Cost { get; set; }
            public bool Admin_only { get; set; } = false;

        }

    }
}

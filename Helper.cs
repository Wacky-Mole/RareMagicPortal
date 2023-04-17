using System.Collections.Generic;
using System.Linq;

namespace RareMagicPortal
{
    public static class Helper
    {
        public static bool GetAllZDOsWithPrefabIterative(this ZDOMan zdoman, List<string> prefabs, List<ZDO> zdos, ref int index)  // Blax code https://raw.githubusercontent.com/blaxxun-boop/TargetPortal/master/TargetPortal/Helper.cs
        {
            HashSet<int> stableHashCodes = new(prefabs.Select(p => p.GetStableHashCode()));
            if (index >= zdoman.m_objectsBySector.Length)
            {
                zdos.AddRange(zdoman.m_objectsByOutsideSector.Values.SelectMany(v => v).Where(zdo => stableHashCodes.Contains(zdo.m_prefab)));
                zdos.RemoveAll(ZDOMan.InvalidZDO);
                return true;
            }
            int num = 0;
            while (index < zdoman.m_objectsBySector.Length)
            {
                if (zdoman.m_objectsBySector[index] is { } zdoList)
                {
                    zdos.AddRange(zdoList.Where(zdo => stableHashCodes.Contains(zdo.m_prefab)));
                    if (++num > 400)
                    {
                        break;
                    }
                }
                ++index;
            }
            return false;
        }
    }
}
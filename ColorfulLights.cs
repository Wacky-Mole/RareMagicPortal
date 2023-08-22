using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RareMagicPortal // Thank you Redseiko and Comfymods
{
    public class PortalLights
    {
        public List<Light> Lights { get; } = new List<Light>();
        public List<ParticleSystem> Systems { get; } = new List<ParticleSystem>();
        public List<ParticleSystemRenderer> Renderers { get; } = new List<ParticleSystemRenderer>();
        public Color TargetColor { get; set; } = Color.clear;

        public PortalLights(Fireplace fireplace)
        {
            ExtractFireplaceData(fireplace.m_enabledObject);
            ExtractFireplaceData(fireplace.m_enabledObjectHigh);
            ExtractFireplaceData(fireplace.m_enabledObjectLow);
           // ExtractFireplaceData(fireplace.m_fireworks);
        }

        private void ExtractFireplaceData(GameObject targetObject)
        {
            if (targetObject)
            {
                Lights.AddRange(targetObject.GetComponentsInChildren<Light>(includeInactive: true));
                Systems.AddRange(targetObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true));
                Renderers.AddRange(targetObject.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true));
            }
        }
    }

    internal class TeleportWorldDataRMP
    {
        public List<Light> Lights { get; } = new List<Light>();
        public List<ParticleSystem> Systems { get; } = new List<ParticleSystem>();
        public List<ParticleSystem> Sucks { get; } = new List<ParticleSystem>();
        public List<ParticleSystem> BlueFlames { get; } = new List<ParticleSystem>();
        public List<Material> Materials { get; } = new List<Material>();
        public String Biome { get; set; }
        public String BiomeColor { get; set; }

        public Color TargetColor = Color.clear;
        public Color LinkColor = Color.clear;
        public Color OldColor = Color.clear;
        public List<Renderer> MeshRend { get; } = new List<Renderer>();
        public String MaterialPortName { get; set; }
        public TeleportWorld TeleportW { get; }

        public TeleportWorldDataRMP(TeleportWorld teleportWorld)
        {
            Lights.AddRange(teleportWorld.GetComponentsInNamedChild<Light>("Point light"));

            Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("suck particles"));
            Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle System"));
            Sucks.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Black_suck"));
            BlueFlames.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("blue flames"));

            Materials.AddRange(
                teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("blue flames") //
                    .Where(psr => psr.material != null)
                    .Select(psr => psr.material));

            MeshRend.AddRange(teleportWorld.GetComponentsInNamedChild<Renderer>("small_portal"));
            //  .Where(psr => psr.material != null)
            //.Select(psr => psr.material));

            TeleportW = teleportWorld;
        }
    }

    internal static class TeleportWorldExtensionRMP // totally cool
    {
        public static IEnumerable<T> GetComponentsInNamedChild<T>(this TeleportWorld teleportWorld, string childName)
        {
            return teleportWorld.GetComponentsInChildren<Transform>(includeInactive: true)
                .Where(transform => transform.name == childName)
                .Select(transform => transform.GetComponent<T>())
                .Where(component => component != null);
        }
    }
}
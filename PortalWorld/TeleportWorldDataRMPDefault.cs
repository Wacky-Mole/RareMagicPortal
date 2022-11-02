using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



namespace RareMagicPortal.PortalWorld
{


	public enum ClassTypes
	{
		TeleportWorldDataRMPDefault,
		TeleportWorldDataRMPModel1,
		TeleportWorldDataRMPModel2,
		TeleportWorldDataRMPModel3,
		TeleportWorldDataRMPModel4
	}


	// factory method
	abstract class TeleportWorldDataCreator
	{
		public abstract ClassBase FactoryMethod(TeleportWorld teleportWorld);
		//public abstract ClassBase TeleportWorldDataCreator(TeleportWorld teleportWorld);
	}

	class TeleportWorldDataCreatorA : TeleportWorldDataCreator
	{

		public override ClassBase FactoryMethod(TeleportWorld teleportWorld)
		{
			return new TeleportWorldDataRMPDefault(teleportWorld);
		}
	}

	class TeleportWorldDataCreatorB : TeleportWorldDataCreator
	{
		public override ClassBase FactoryMethod(TeleportWorld teleportWorld)
		{
			return new TeleportWorldDataRMPModel1(teleportWorld);
		}
	}
	class TeleportWorldDataCreatorC : TeleportWorldDataCreator
	{
		public override ClassBase FactoryMethod(TeleportWorld teleportWorld)
		{
			return new TeleportWorldDataRMPModel2(teleportWorld);
		}
	}
	class TeleportWorldDataCreatorD : TeleportWorldDataCreator
	{
		public override ClassBase FactoryMethod(TeleportWorld teleportWorld)
		{
			return new TeleportWorldDataRMPModel3(teleportWorld);
		}
	}

	class TeleportWorldDataCreatorE : TeleportWorldDataCreator
	{
		public override ClassBase FactoryMethod(TeleportWorld teleportWorld)
		{
			return new TeleportWorldDataRMPModel4(teleportWorld);
		}
	}





	public abstract class ClassBase
	{

		/// public const string ModelDefault = "small_portal";
		/// public const string Model1 = "Torus_cell.002";
		/// public const string Model2 = "RuneRing";
		/// public const string Model3 = "Gates";
		/// public const string Model4 = "QuadPortal";


		//public Color TargetColor;
		//public Color OldColor;
		public TeleportWorld TeleportW;

		public abstract void SetTeleportWorldColors(Color newcolor, bool SetcolorTarget = false, bool SetMaterial = false);
		public abstract Color GetOldColor();
		public abstract Color GetTargetColor();

		/*
		public static ClassBase Create(ClassTypes classtypes)
		{
			switch (classtypes)
			{
				case ClassTypes.TeleportWorldDataRMPDefault: return new TeleportWorldDataRMPDefault();

				default: throw new ArgumentOutOfRangeException();
			}
		}
		

		public static ClassTypes Pick(string Model)
		{
			switch (Model)
			{
				case ModelDefault: return ClassTypes.TeleportWorldDataRMPDefault;

				default: throw new ArgumentOutOfRangeException();
			}
		}

		public static ClassTypes Pick(string Model, TeleportWorld instance)
		{
			switch (Model)
			{
				case ModelDefault: return TeleportWorldDataRMPDefault.;

				default: throw new ArgumentOutOfRangeException();
			}

		
		}
			*/

	}

	class TeleportWorldDataRMPDefault : ClassBase
    {
        public List<Light> Lights { get; } = new List<Light>();
        public List<ParticleSystem> Systems { get; } = new List<ParticleSystem>();
        public List<Material> Materials { get; } = new List<Material>();
		public Color TargetColor { get; set; } = new Color(0, 0, 1); // just to make sure it has been set and needs reset
		public Color OldColor { get; set; } = new Color(0, 0, 1); // just to make sure it has been set and needs reset
		public List<Renderer> MeshRend { get; } = new List<Renderer>();
        public String MaterialPortName { get; set; }
        public  TeleportWorld TeleportW { get; set; }



		public TeleportWorldDataRMPDefault(TeleportWorld teleportWorld)
        {
            Lights.AddRange(teleportWorld.GetComponentsInNamedChild<Light>("Point light"));

            Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("suck particles"));
            Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle System"));

            Materials.AddRange(
                teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("blue flames")
                    .Where(psr => psr.material != null)
                    .Select(psr => psr.material));

            MeshRend.AddRange(teleportWorld.GetComponentsInNamedChild<Renderer>("small_portal"));
            //  .Where(psr => psr.material != null)
            //.Select(psr => psr.material)); 

            TeleportW = teleportWorld;
        }

        public override Color GetOldColor()
        {
			return this.OldColor;
        }
		public override Color GetTargetColor()
		{
			return this.TargetColor;
		}
		public override void SetTeleportWorldColors(Color newcolor,  bool SetcolorTarget = false, bool SetMaterial = false)
		{
			this.OldColor = this.TargetColor;
			this.TargetColor = newcolor;
			//MagicPortalFluid.RareMagicPortal.LogInfo($"InsideTel color {this.TargetColor}");
			
			//Color Gold = new Color(1f, 215f / 255f, 0, 1f);
			//Color Cyan = Color.cyan

			if (this.TargetColor == RareMagicPortal.Globals.Gold)
			{
				try 
				{
					Material mat = RareMagicPortal.Globals.originalMaterials["shaman_prupleball"];
					foreach (Renderer red in this.MeshRend)
					{
						red.material = mat;
					}
				}
				catch { }
			}
			else if (this.TargetColor == Color.black)
			{
				try
				{
					Material mat = RareMagicPortal.Globals.originalMaterials["silver_necklace"];
					foreach (Renderer red in this.MeshRend)
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
				Material mat = RareMagicPortal.Globals.originalMaterials["portal_small"];
				foreach (Renderer red in this.MeshRend)
				{
					red.material = mat;
				}
			}

			foreach (Light light in this.Lights)
			{
				if (this.TargetColor == Color.yellow) // trying to reset to default
				{
					light.color = Globals.lightcolor;
				}
				else
					light.color = this.TargetColor;
			}

			Color FlamePurple = new Color(191f / 255f, 0f, 191f / 255f, 1);
			foreach (ParticleSystem system in this.Systems)
			{
				ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
				if (this.TargetColor == Color.yellow) // trying to reset to default
				{
					colorOverLifetime.color = new ParticleSystem.MinMaxGradient(Globals.flamesstart, Globals.flamesend);
				}

				ParticleSystem.MainModule main = system.main;
				if (this.TargetColor == Color.yellow) // trying to reset to default
				{
					main.startColor = Globals.flamesstart;
				}
				else
					main.startColor = this.TargetColor;
			}

			//teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor; // set color

			foreach (Material material in this.Materials)
			{
				if (this.TargetColor == Color.yellow) // trying to reset to default
				{
					material.color = Globals.flamesstart;
				}
				else
					material.color = this.TargetColor;
			}

			if (SetcolorTarget)
			{
				if (this.TargetColor == Color.black)
				{
					this.TeleportW.m_colorTargetfound = Color.black * 10;
				}
				else if (this.TargetColor == Color.yellow) // trying to reset to default
				{
					this.TeleportW.m_colorTargetfound = RareMagicPortal.Globals.m_colorTargetfound * 7;
				}
				else if (this.TargetColor == RareMagicPortal.Globals.Gold)
				{
					this.TeleportW.m_colorTargetfound = this.TargetColor;
				}
				else if (this.TargetColor == Globals.Tan)
				{
					this.TeleportW.m_colorTargetfound = this.TargetColor * 3;
				}
				else if (this.TargetColor == Color.cyan) // cyan now
				{
					this.TeleportW.m_colorTargetfound = this.TargetColor * 4;
				}
				else
					this.TeleportW.m_colorTargetfound = this.TargetColor * 7; // set color // set intensity very high
			}

		}
	}

	class TeleportWorldDataRMPModel1 : ClassBase
	{
		public new Color TargetColor = Color.clear;
		public new Color OldColor = Color.clear;
		public List<Light> Lights { get; } = new List<Light>();
		public List<Material> Materials { get; } = new List<Material>();

		private Material DefaultMaterials { get; }
		public List<ParticleSystem> Systems { get; } = new List<ParticleSystem>();
		public List<Material> ParticleMaterials { get; } = new List<Material>();
		public List <Renderer> MeshRend { get; }  = new List<Renderer> ();
		public new TeleportWorld TeleportW { get; }

		public override Color GetOldColor()
		{
			return this.OldColor;
		}
		public override Color GetTargetColor()
		{
			return this.TargetColor;
		}
		public override void SetTeleportWorldColors(Color newcolor,bool SetcolorTarget = false, bool SetMaterial = false)
        {
			this.OldColor = this.TargetColor;
			this.TargetColor = newcolor;

			foreach (Light light in this.Lights)
			{
				light.color = this.TargetColor;
			}

			foreach (Material material in this.ParticleMaterials)
			{
				material.SetColor("_TintColor", this.TargetColor * 2); // actual tint
			} // replaced for systems

			foreach (ParticleSystem system in this.Systems)
			{

				ParticleSystem.MainModule main = system.main;
				main.startColor = this.TargetColor;
			}

			int i = 0;
			foreach (var material in this.MeshRend)
			{
				if (material.material.name == "surtlingcore (Instance)" && this.TargetColor != Color.black)
                {
					material.material = DefaultMaterials;


				}
				if (material.material.name == "Stone pattern 01 (Instance)" && this.TargetColor == Color.black)
                {
					i++;
					material.material = RareMagicPortal.Globals.originalMaterials["surtlingcore"];

				}
					
			}
			RareMagicPortal.MagicPortalFluid.RareMagicPortal.LogInfo($"about of material {i}");
		}

		public TeleportWorldDataRMPModel1(TeleportWorld teleportWorld)
		{
			Lights.AddRange(teleportWorld.GetComponentsInNamedChild<Light>("Point light (2)"));

			/*
			ParticleMaterials.AddRange(
				teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Particle0")
					.Where(psr => psr.material != null)
					.Select(psr => psr.material));

			ParticleMaterials.AddRange(
				teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Particle1")
					.Where(psr => psr.material != null)
					.Select(psr => psr.material));

			ParticleMaterials.AddRange(
				teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Particle2")
					.Where(psr => psr.material != null)
					.Select(psr => psr.material));

			ParticleMaterials.AddRange(
				teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Particle3")
					.Where(psr => psr.material != null)
					.Select(psr => psr.material));

			ParticleMaterials.AddRange(
				teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Particle4")
					.Where(psr => psr.material != null)
					.Select(psr => psr.material));
			*/
			Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle0"));
			Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle1"));
			Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle2"));
			Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle3"));
			Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle4"));

			MeshRend.AddRange(teleportWorld.GetComponentsInChildren<Renderer>(true));
			DefaultMaterials = teleportWorld.GetComponentsInNamedChild<Renderer>("Torus_cell.002").Last().material;




			TeleportW = teleportWorld;
		}

	}

	class TeleportWorldDataRMPModel2 : ClassBase // RuneRing
	{
		public new Color TargetColor = Color.clear;
		public new Color OldColor = Color.clear;
		public List<Light> Lights { get; } = new List<Light>();
		public List<ParticleSystem> Systems { get; } = new List<ParticleSystem>();
		private  List<ParticleSystem.ColorOverLifetimeModule> DefaultGreenSystem { get; } = new List<ParticleSystem.ColorOverLifetimeModule>();
		public List<Material> Materials { get; } = new List<Material>();
		private Material DefaultMaterials { get; }

		public List<Renderer> MeshRend { get; } = new List<Renderer>();
		public new TeleportWorld TeleportW { get; }


		public override Color GetOldColor()
		{
			return this.OldColor;
		}
		public override Color GetTargetColor()
		{
			return this.TargetColor;
		}
		public override void SetTeleportWorldColors(Color newcolor, bool SetcolorTarget = false, bool SetMaterial = false)
		{
			this.OldColor = this.TargetColor;
			this.TargetColor = newcolor;

			foreach (Light light in this.Lights)
			{
					light.color = this.TargetColor;
			}

			foreach (ParticleSystem system in this.Systems)
			{
				ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;

				colorOverLifetime.color = new ParticleSystem.MinMaxGradient(this.TargetColor, this.TargetColor);
				//new ParticleSystem.MinMaxCurve

				ParticleSystem.MainModule main = system.main;
				main.startColor = this.TargetColor;
			}

			if (SetcolorTarget)
			{
				this.TeleportW.m_colorTargetfound = this.TargetColor * 7;
			}

			//Material mat = RareMagicPortal.Globals.originalMaterials["portal_small"];
			foreach (Renderer red in this.MeshRend)
			{
				red.material.color = this.TargetColor; // hue of ring
				if (this.TargetColor == Color.black)
				{
					red.material.SetColor("_EmissionColor", Color.white * 2);
					red.material = RareMagicPortal.Globals.originalMaterials["surtlingcore"];
				}
				else
				{
					red.material.SetColor("_EmissionColor", this.TargetColor * 2); // actual emission
					red.material = DefaultMaterials;
				}
			}

			foreach (Material material in this.Materials)
			{
					material.color = this.TargetColor;
					material.SetColor("_TintColor", this.TargetColor*2); // actual tint
			}

		}

		public TeleportWorldDataRMPModel2(TeleportWorld teleportWorld)
		{
			Lights.AddRange(teleportWorld.GetComponentsInNamedChild<Light>("Point light"));

			Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle System2")); // in shader
			Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle System")); // coloroverlifetime

			DefaultGreenSystem.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem.ColorOverLifetimeModule>("Particle System")); // save default green

			Materials.AddRange(
			teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Particle System2")
				.Where(psr => psr.material != null)
				.Select(psr => psr.material));

			Materials.AddRange(
			teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Distortion") // in tint
				.Where(psr => psr.material != null)
				.Select(psr => psr.material));

			MeshRend.AddRange(teleportWorld.GetComponentsInNamedChild<Renderer>("RuneRing")); // material
			//  .Where(psr => psr.material != null)
			//.Select(psr => psr.material)); 
			DefaultMaterials = teleportWorld.GetComponentsInNamedChild<Renderer>("RuneRing").Last().material;

			TeleportW = teleportWorld;
		}

	}

	class TeleportWorldDataRMPModel3 : ClassBase
	{
		public new Color TargetColor = Color.clear;
		public new Color OldColor = Color.clear;
		public List<Light> Lights { get; } = new List<Light>();
		public List<Material> Materials { get; } = new List<Material>();
		public List<Renderer> MeshRend { get; } = new List<Renderer>();
		public new TeleportWorld TeleportW { get; }

		public override Color GetOldColor()
		{
			return this.OldColor;
		}
		public override Color GetTargetColor()
		{
			return this.TargetColor;
		}

		public override void SetTeleportWorldColors(Color newcolor, bool SetcolorTarget = false, bool SetMaterial = false)
		{
			this.OldColor = this.TargetColor;
			this.TargetColor = newcolor;

			foreach (Light light in this.Lights)
			{
				light.color = this.TargetColor;
			}

			if (SetcolorTarget)
			{
				Color Mod = newcolor;
				Mod = Mod * .4f;
				this.TeleportW.m_colorTargetfound = Mod;
			}

			//Material mat = RareMagicPortal.Globals.originalMaterials["portal_small"];
			foreach (Renderer red in this.MeshRend)
			{
				//red.material.color = this.TargetColor; // hue of gate
				if (this.TargetColor == Color.black)
				{
					red.material.SetColor("_EmissionColor", new Color(82f/255f, 56f/255f, 55f/255f, 1));
					red.material.SetColor("_TintColor", new Color(82f / 255f, 56f / 255f, 55f / 255f, 1));
				}
				else
				{

					red.material.SetColor("_EmissionColor", this.TargetColor); // actual emission
					red.material.SetColor("_TintColor", this.TargetColor); // actual emission
				}
			}

			foreach (Material material in this.Materials)
			{
				material.color = this.TargetColor;
				material.SetColor("_TintColor", this.TargetColor ); // actual tint
			}
		}

		public TeleportWorldDataRMPModel3(TeleportWorld teleportWorld)
		{
			TeleportW = teleportWorld;

			Materials.AddRange(
			teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Particle System") // in tint
				.Where(psr => psr.material != null)
				.Select(psr => psr.material));

			Materials.AddRange(
			teleportWorld.GetComponentsInNamedChild<LineRenderer>("UpperEdge") // in tint
				.Where(psr => psr.material != null)
				.Select(psr => psr.material));

			Lights.AddRange(teleportWorld.GetComponentsInNamedChild<Light>("Point light"));

			MeshRend.AddRange(teleportWorld.GetComponentsInNamedChild<Renderer>("Gates"));
		}

	}

	class TeleportWorldDataRMPModel4 : ClassBase
	{
		public new Color TargetColor = Color.clear;
		public new Color OldColor = Color.clear;
		public List<Light> Lights { get; } = new List<Light>();
		public List<Material> Materials { get; } = new List<Material>();
		private Material DefaultMaterials { get; }

		private Transform CenterAdmin { get; set; }
		public List<Material> Materials2 { get; } = new List<Material>();
		public List<Renderer> MeshRend { get; } = new List<Renderer>();
		public new TeleportWorld TeleportW { get; }

		public override Color GetOldColor()
		{
			return this.OldColor;
		}
		public override Color GetTargetColor()
		{
			return this.TargetColor;
		}
		public override void SetTeleportWorldColors(Color newcolor,bool SetcolorTarget = false, bool SetMaterial = false)
		{
			this.OldColor = this.TargetColor;
			this.TargetColor = newcolor;

			foreach (Light light in this.Lights)
			{
				light.color = this.TargetColor;
			}

			if (SetcolorTarget)
			{
				Color Mod = newcolor;
				Mod = Mod *.6f;
				this.TeleportW.m_colorTargetfound = Mod;
			}

			//Material mat = RareMagicPortal.Globals.originalMaterials["portal_small"];
			foreach (Renderer red in this.MeshRend)
			{
				//red.material.color = this.TargetColor; // hue of gate
				if (this.TargetColor == Color.black)
				{
					//red.material.SetColor("_EmissionColor", new Color(82f / 255f, 56f / 255f, 55f / 255f, 1));
					red.material.SetColor("_TintColor", new Color(82f / 255f, 56f / 255f, 55f / 255f, 1));
					red.material = RareMagicPortal.Globals.originalMaterials["silver_necklace"];
					CenterAdmin.gameObject.SetActive(true);
				}
				else
				{
					red.material = DefaultMaterials;
					//red.material.SetColor("_EmissionColor", this.TargetColor); // actual emission
					red.material.SetColor("_TintColor", this.TargetColor); // actual emission
				}
			}

			foreach (Material material in this.Materials)
			{
				material.color = this.TargetColor;
				material.SetColor("_TintColor", this.TargetColor); // actual tint
			}
			foreach (Material material in this.Materials2)
            {
                Color Col2 = this.TargetColor;
				Col2.r = +.3f;
				material.color = Col2;
				material.SetColor("_TintColor", Col2 ); // actual tint
			}
		}

		public TeleportWorldDataRMPModel4(TeleportWorld teleportWorld)
		{


			Materials.AddRange(
			teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Particle System1") // in tint
				.Where(psr => psr.material != null)
				.Select(psr => psr.material));

			Materials2.AddRange(
			teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("Particle System2") // in tint
				.Where(psr => psr.material != null)
				.Select(psr => psr.material));

			Lights.AddRange(teleportWorld.GetComponentsInNamedChild<Light>("Point light"));

			MeshRend.AddRange(teleportWorld.GetComponentsInNamedChild<Renderer>("QuadPortal"));
			DefaultMaterials = teleportWorld.GetComponentsInNamedChild<Renderer>("QuadPortal").Last().material;

			CenterAdmin = teleportWorld.GetComponentsInNamedChild<Transform>("CenterAdmin").First();
			
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

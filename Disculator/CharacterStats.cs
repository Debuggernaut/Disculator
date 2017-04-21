using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disculator
{
	public class CharacterStats 
	{
		public int intellect;
		public int critRating;
		public int hasteRating;
		public int masteryRating;
		public int verRating;

		public float critPercent;
		public float hastePercent;
		public float masteryPercent;
		public float verPercent;

		public float scaledSpellPower;

		public int artifactTraits;
		public float allDamageBonus;

		public float DarkestShadows = 1.1f; //10/3% Shadow Mend per point
		public float BurstOfLight = 1.2f; //5% PW:Radiance per point
		public float ShieldOfFaith = 1.2f; //5% bonus to PW:Shield per point
		public float Confession = 1.12f; //4% to Penance per point

		public float EdgeOfDarkAndLight = 1.15f; //5% to SWP/PtW per point

		public float Skjoldr = 1.15f; //Legendary wrists, +15% to PW:Shield

		public float AvgAtonements;

		public Spell[] HealSpells;
		public Spell[] DamageSpells;

		public Spell Plea;
		public Spell Smend;
		public Spell SmiteAbsorb;
		public Spell Shield;

		public Spell Smite;
		public Spell CastigatedPenance;
		public Spell RegularPenance;
		public Spell Swp;
		public Spell Ptw;
		public Spell SwpDot;
		public Spell PtwDot;

		public float PtwDPS = 0f;
		public float SwpDPS = 0f;

		public float PenanceCD = 9f;
		public float ShieldCD = 7.5f;
		public float SolaceCD = 12f;

		//Modeling Power of the Dark Side as a 1m cooldown effect
		// rather than trying to actually model procs per minute
		public float PowerOfTheDarkSideCD = 60f;

		public Spell LightsWrath;

		public Spell ShadowfiendSwing;
		public Spell MindbenderSwing;

		public Spell Shadowfiend;
		public Spell Mindbender;

		public int ShadowfiendSwings;

		public CharacterStats clone()
		{
			return (CharacterStats) this.MemberwiseClone();
		}

		public CharacterStats()
		{
			
		}
		
		public void Raycalculate()
		{

			allDamageBonus = 1.0f;
			for (int i = 0; i < artifactTraits; i++)
				allDamageBonus = allDamageBonus * 1.0065f;

			critPercent = (critRating / 400f + 5f) / 100f;
			hastePercent = hasteRating / 375f / 100f;
			masteryPercent = (masteryRating * 3f / 800f + 12f) / 100f;
			verPercent = verRating / 475f / 100f;

			scaledSpellPower = intellect * (1 + verPercent) * 1.05f * 1.1f;

			//Rounds up to the next number of swings
			ShadowfiendSwings = (int)Math.Ceiling(8f * (1 + hastePercent));

			//Scale cooldowns appropriately:
			ShieldCD = 7.5f / (1 + hastePercent);
			PowerOfTheDarkSideCD = 60f / (1 + hastePercent);
			SolaceCD = 12f / (1 + hastePercent);

			Plea = new Spell("Plea (0 Atonements)", 2.25f, 1.5f, 3960, 1.0f, this);
			Smend = new Spell("Shadow Mend (Heavy Incoming Damage)", 7.5f, 1.5f, 30800, DarkestShadows, this);
			SmiteAbsorb = new Spell("Smite Absorb", 2.25f, 1.5f, 11000, 1.0f, this);
			Shield = new Spell("Power Word: Shield", 5.5f, 1.5f, 22000, ShieldOfFaith * Skjoldr, this);
			HealSpells = new Spell[]
			{
				Plea,
				new Spell("Plea (6 Atonements)", 2.25f, 1.5f, 3960*6, 1.0f, this),
				Smend,

				Shield,
				new Spell("Penitent Penance", 9f, 2.0f, 22000, Confession, this),
				new Spell("Clarity of Will", 9f, 2f, 30800, 1.0f, this),

				SmiteAbsorb,

				new Spell("Power Word: Radiance", 2.5f*3, 2.5f, 71500, BurstOfLight, this),
				new Spell("Shadow Covenant (Fully Efficient Somehow)", 4.5f*5, 2.5f, 71500, BurstOfLight, this),
				new Spell("Shadow Covenant", 4.5f*5f/2f, 1.5f, 71500, BurstOfLight, this),
				//new Spell("Power Word: Radiance (each)", 2.5f, 2.5f, 71500, BurstOfLight, this),
				new Spell("Divine Star (6+ targets)", 0.9f*6, 1.5f, 27500, 1, this),
				new Spell("Halo (6+ targets)", 2.87f*6, 1.5f, 39600, 1, this),

				new Spell("Shadow Mend (Grace)", 7.55f, 1.5f, 30800, DarkestShadows * 1.3f, this),
				new Spell("Power Word: Shield (Grace)", 5.5f, 1.5f, 22000, ShieldOfFaith*Skjoldr*1.3f, this),
				new Spell("Penitent Penance (Grace)", 9f, 2.0f, 22000, Confession*1.3f, this),

			};

			float powerWordSolaceKludge = 1.15f; //For some reason, Power Word: Solace does 15% more damage than it seems like it should
			float castigation = 4f / 3f;
			Smite = new Spell("Smite", 2.25f * 1.15f, 1.5f, 11000, allDamageBonus, this);
			CastigatedPenance = new Spell("Penance (Castigated)", 1.9f * 3f, 2.0f, 30800, allDamageBonus * Confession * castigation, this);
			RegularPenance = new Spell("Penance (Standard)", 1.9f * 3f, 2.0f, 30800, allDamageBonus * Confession, this);
			Ptw = new Spell("Purge the Wicked (total)", 4.8f * (1 + hastePercent) + 1f, 1.5f, 22000, allDamageBonus * EdgeOfDarkAndLight, this);
			Swp = new Spell("Shadow Word: Pain (total)", 3.42f * (1 + hastePercent) + 0.38f, 1.5f, 24200, allDamageBonus * EdgeOfDarkAndLight, this);
			PtwDot = new Spell("Purge the Wicked (DoT)", 4.8f * (1 + hastePercent), 1.5f, 22000, allDamageBonus * EdgeOfDarkAndLight, this);
			SwpDot = new Spell("Shadow Word: Pain (DoT)", 3.42f * (1 + hastePercent), 1.5f, 24200, allDamageBonus * EdgeOfDarkAndLight, this);

			PtwDPS = PtwDot.AvgEffect() / 20f;
			SwpDPS = SwpDot.AvgEffect() / 18f;
			LightsWrath = new Spell("Light's Wrath", 7f, 2.5f, 0, allDamageBonus, this);

			MindbenderSwing = new Spell("Mindbender (One Swing)", 1.5f, 12f / 8f / (1 + hastePercent), -5500, allDamageBonus, this);
			ShadowfiendSwing = new Spell("Shadowfiend (One Swing)", 2.0f, 12f / 8f / (1 + hastePercent), -5500, allDamageBonus, this);

			Mindbender = new Spell("Mindbender (Full Duration)", ShadowfiendSwings * 1.5f, 1.5f, -5500 * ShadowfiendSwings, allDamageBonus, this);
			Shadowfiend = new Spell("Shadowfiend (Full Duration)", ShadowfiendSwings * 2.0f, 1.5f, 0, allDamageBonus, this);

			DamageSpells = new Spell[]
			{
				Smite, CastigatedPenance, RegularPenance, Ptw, Swp, LightsWrath,
				PtwDot, SwpDot,
				MindbenderSwing, ShadowfiendSwing,
				Mindbender, Shadowfiend,
				new Spell("Power Word: Solace", powerWordSolaceKludge*3f, 1.5f, -11000, allDamageBonus, this),
				new Spell("Divine Star", 1.45f, 1.5f, 27500, allDamageBonus, this),
				new Spell("Halo", 4.31f, 1.5f, 1, allDamageBonus, this),
				//new Spell("Power Word: Solace", 3f, 1.5f, -11000, allDamageBonus, this),
				//new Spell("Power Word: Solace", 3f, 1.5f, -11000, allDamageBonus, this),
			};
		}
	}
}

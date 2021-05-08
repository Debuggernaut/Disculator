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

		public float bonusCritPercent;
		public float bonusHastePercent;
		public float bonusMasteryPercent;
		public float bonusVerPercent;

		private float critPercent;
		private float hastePercent;
		private float masteryPercent;
		private float verPercent;

		public float scaledSpellPower;

		//public float Doomsayer = 3f; //extra seconds of Rapture per point
		//public float Confession = 1.12f; //4% to Penance per point
		//public float BorrowedTime = 1.2f; //5% Smite/Penance haste after applying Atonement
		//public float ShieldOfFaith = 1.2f; //5% bonus to PW:Shield per point

		//public float EdgeOfDarkAndLight = 1.15f; //5% to SWP/PtW per point
		//public float BurstOfLight = 1.15f; //5% power word: radiance healing per point
		//public float DarkestShadows = 1.1f; //10/3% Shadow Mend per point

		//public float AegisOfWrathAndSkjoldr = 1.15f*1.3f; //Legendary wrists, +15% to PW:Shield

		public Spell[] HealSpells;
		public Spell[] DamageSpells;

		public Spell HolyShock;
		public Spell WordOfGlory;
		public Spell LightOfDawn;
		public Spell HolyLight;
		public Spell FlashOfLight;
		public Spell AshenHallow;
		public Spell ShockBarrier_Perfect;
		public Spell JudgmentOfLight;

		public Spell HolyShockDmg;
		public Spell SotR;
		public Spell CrusaderStrike;
		public Spell Judgment;
		public Spell Consecration;
		public Spell AshenHallowDmg;

		public Spell HammerOfWrath;

		public Spell AvengingWrath;

		public Spell SuperHolyShock;

		public float HealingEffectiveness = 1.0f;
		public float MasteryEffectiveness = 0.8f;

		public float TempScaler = 1.0f;

		public float CritPercent {
			get => critPercent + bonusCritPercent;
			set => critPercent = value; }
		public float HastePercent {
			get => hastePercent + bonusHastePercent;
			set => hastePercent = value; }
		public float VerPercent {
			get => verPercent + bonusVerPercent;
			set => verPercent = value; }
		public float MasteryPercent {
			get => masteryPercent + bonusMasteryPercent;
			set => masteryPercent = value; }

		public CharacterStats clone()
		{
			return (CharacterStats) this.MemberwiseClone();
		}

		public CharacterStats()
		{
			
		}

		public void Raycalculate()
		{
			AvengingWrath = new InstantSpell("Avenging Wrath", 0f, 0f, 0f, this);
			AvengingWrath.BaseCooldown = 120f;
			AvengingWrath.BaseCastTime = 0f;

			CritPercent = (critRating / 35f + 6f) / 100f;
			HastePercent = hasteRating / 33f / 100f;
			MasteryPercent = (masteryRating / 23f + 12f) / 100f;
			VerPercent = verRating / 40f / 100f;
			bonusCritPercent = 0f;
			bonusHastePercent = 0f;
			bonusMasteryPercent = 0f;
			bonusVerPercent = 0f;
			TempScaler = 1.0f;

			scaledSpellPower = intellect * (1 + VerPercent);

			HolyShock				 = new InstantSpell("Holy Shock", 1.55f, 1600, -1.0f, this);
			HolyShock.BonusCritChance = 0.3f;
			HolyShock.BaseCooldown = 7.5f;
			WordOfGlory				= new InstantSpell("Word of Glory", 3.15f, 0f, 3.0f, this);
			LightOfDawn				= new InstantSpell("Light of Dawn (5 targets)", 1.05f*5, 0f, 3.0f, this);
			LightOfDawn.TargetCount = 5;
			HolyLight = new Spell("Holy Light", 2.6f, 2.5f, 1500, 1.0f, this);
			FlashOfLight			 = new Spell("Flash of Light", 1.68f, 1.5f, 2200f, 1.0f, this);
			AshenHallow				 = new Spell("Ashen Hallow (5 targets)", 0.42f*15f*5f, 1.5f, 2000f, 1.0f+HastePercent, this); //needs to scale with haste
			AshenHallow.TargetCount = 5;
			AshenHallow.BaseCooldown = 4f * 60f;
			ShockBarrier_Perfect	= new InstantSpell("Shock Barrier", HolyShock.Scaler * 0.6f, 0, 0, this);
			ShockBarrier_Perfect.BaseCastTime = 0;
			JudgmentOfLight			 = new InstantSpell("Judgment of Light", 0.07f*25f, 300, 0, this);
			JudgmentOfLight.BaseCooldown = 12f;
			JudgmentOfLight.TargetCount = 5;

			SuperHolyShock = new InstantSpell("HS w/ 4G, 3SB, + 1/3 a Light of Dawn", HolyShock.Scaler * (1.6f) + 0.38f * 4f + LightOfDawn.Scaler / 3f, 1600f, -1f, this);
			SuperHolyShock.BaseCastTime *= 1.3f;

			CrusaderStrike = new CrusaderStrike(this);

			HealSpells = new Spell[]
			{
				HolyShock              ,
				WordOfGlory            ,
				LightOfDawn            ,
				HolyLight              ,
				FlashOfLight           ,
				AshenHallow            ,
				ShockBarrier_Perfect   ,
				JudgmentOfLight,
				new InstantSpell("Light of the Martyr", 2.1f, 700, 0f, this),
				new InstantSpell("Holy Prism (1 target)", 1.4f, 1300, 0f, this),
				new InstantSpell("Holy Prism (5 targets)", 0.7f*5f, 1300, 0f, this),
				new InstantSpell("Glimmer of Light (one hit)", 0.38f, 0, 0f, this),
				new InstantSpell("Holy Shock w/ 4 glimmer and 3 shock barrier ticks", HolyShock.Scaler*(1.6f)+0.38f*4f, 1600f, -1f, this),
				SuperHolyShock,
				new Spell("Holy Light w/ Infusion", HolyLight.Scaler, HolyLight.BaseCastTime, HolyLight.Mana, 1.3f, this ),
				new Spell("Flash of Light w/ Infusion", FlashOfLight.Scaler, FlashOfLight.BaseCastTime, FlashOfLight.Mana*2f/3f, 1.0f, this ),
				new Spell("Crusader Strike +1/3rd Light of Dawn", 
					LightOfDawn.Scaler/3.0f, 
					1.5f*4f/3f,
					CrusaderStrike.Mana,
					-1,
					this),
				new Spell("Crusader Strike +1/3rd WoG", WordOfGlory.Scaler/3.0f, 1.5f*4f/3f, CrusaderStrike.Mana, -1, this),
				new Spell("Light's Hammer (6 targets)", 0.25f*7f*6f, 1.5f, 1800f, 1.0f+HastePercent, this),
				new Spell("Bestow Faith", 2.1f, 1.5f, 600, 1.0f, this),
			};

			HammerOfWrath = new InstantSpell("Hammer of Wrath (no damage simulated)", 1f, 0, -1.0f, this);
			HammerOfWrath.BaseCooldown = 7.5f;
			HolyShockDmg = new InstantSpell("Holy Shock (damage)", 0.77f, 1600, -1.0f, this);
			HolyShockDmg.BonusCritChance = 0.3f;
			SotR = new InstantSpell("Shield of the Righteous", 0.44f, 0, 3.0f, this);
			Judgment = new InstantSpell("Judgment", 1.26f, 300f, 0f, this);
			Consecration = new InstantSpell("Consecration (1 target)", 0.97f * (1 + HastePercent), 300f, 0f, this);
			Consecration = new InstantSpell("Consecration (5 targets)", 0.97f * 5f * (1 + HastePercent), 300f, 0f, this);
			Consecration.BaseCooldown = 12f;
			AshenHallowDmg = new Spell("Ashen Hallow Damage (5 targets)", 0.54f * 15f * 5f, 1.5f, 2000f, 1.0f + HastePercent, this);

			DamageSpells = new Spell[]
			{
				HolyShockDmg,
				SotR,
				CrusaderStrike,
				Judgment,
				new InstantSpell("Consecration (1 target)", 0.97f * (1 + HastePercent), 300f, 0f, this),
				Consecration,
				AshenHallowDmg
			};
		}
	}
}

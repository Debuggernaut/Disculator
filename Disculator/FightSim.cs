using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disculator
{
	public class FightSim
	{
		public float AtonementDuration = 18f;

		public int CastNum = 0;
		public float Time = 0f;

		public float TotalHealing = 0f;
		public float TotalDamage = 0f;
		public float TankHealing = 0f;
		public int Atonements = 0;
		public float ManaSpent = 0f;
		public bool DotCast = false;
		public bool StackedAtonement = false;

		public float Deeps = 0f;
		public float Heeps = 0f;
		public float TankHeeps = 0f;

		public float[] AtonementExpirations;
		public float DotExpires = 999f;

		public List<Spell> Casts;

		public Spell HolyShock;

		public float HolyPower = 0f;

		public bool fullLog = false;

		public int HolyShockCasts;
		public int CrusaderStrikeCasts;
		public int FinisherCasts;
		public int Penances;
		public int Shields;
		public int Purges;

		public bool HalfAssedDarkmoonSimulator = false;

		public bool SimShockBarrier = true;
		public bool SimAwakening = true;
		public bool SimCrusadersMight = true;
		public bool SimDivinePurpose = true;

		// Note that the presence of only half an ass prevents this from modeling
		// things such as a simultaneous Bloodlust + Power Infusion or the trinket
		// off Chronomatic Anomaly
		public float TempHasteBuff;
		public float TempHasteBuffExpiration;
		public float AvengersWrathEnds;

		public CharacterStats M;
		public StringBuilder sb;


		public void Reset()
		{
			//AtonementDuration = 15f;

			Casts = new List<Spell>();
			CastNum = 0;
			Time = 0f;

			TotalHealing = 0f;
			TotalDamage = 0f;
			TankHealing = 0f;
			Atonements = 0;
			ManaSpent = 0f;
			DotCast = false;
			StackedAtonement = false;

			HolyPower = 0.0f;
			AvengersWrathEnds = 0f;

			Deeps = 0f;
			Heeps = 0f;
			TankHeeps = 0f;

			DotExpires = 0f;

			TempHasteBuffExpiration = 9001f;
			this.noSpell = new Spell("Waiting", 0, 0, 0, this.M);
		}

		private String F(float input)
		{
			if (input > 30 * 1000f)
			{
				return (input / 1000f).ToString("#,#") + "K";
			}

			if (input < 100)
			{
				return input.ToString("#,#.##");
			}

			return input.ToString("#,#.#");
		}

		private void Log(StringBuilder sb, String s)
		{
			if (fullLog)
				sb.AppendLine(F(Time) + "s: " + F(TotalHealing) + "," + F(TotalDamage)
					+ "," + Atonements.ToString() + s);
		}

		public void UpdateBonuses()
		{
			if (Time >= TempHasteBuffExpiration)
			{
				this.M.bonusHastePercent = 0;
			}

			if (Time >= AvengersWrathEnds)
			{
				this.M.bonusCritPercent = 0f;
				this.M.TempScaler = 1.0f;
			}
		}

		public bool BeginAvengingWrath(bool Awakening)
		{
			if (Awakening && !SimAwakening)
			{
				return false;
			}
			if (M.AvengingWrath.Ready(this) || Awakening)
			{
				if (Awakening)
				{
					AvengersWrathEnds = Time + 10f;
				}
				else
				{
					AvengersWrathEnds = Time + 20f;
					M.AvengingWrath.Cast(this);
				}

				this.M.bonusCritPercent = 0.3f;
				this.M.TempScaler = 1.3f;

				return true;
			}

			return false;
		}

		//10 hours straight of button-mashing, that oughtta get 'er done
		public static float LONGTIME = 60f*60*10f;

		Spell noSpell;
		float totalDeadTime = 0f;
		float currentDeadTime = 0f;
		bool waiting = false;

		public void LogSpell(Spell s)
		{
			if (waiting)
			{
				totalDeadTime += currentDeadTime;
				currentDeadTime = 0f;
				waiting = false;
				LogSpell(noSpell);
			}

			if (LONGTIME > 5f * 60f)
				return;

			sb.AppendLine(F(Time) + ": " + s.Name + ", "
				+ F(s.AvgEffect()) + ", "
				+ this.HolyPower + "HP, Buff:"
				+ F(this.M.TempScaler) + ", "
				+ F(this.M.bonusCritPercent) + "crit, "
				);
		}

		public StringBuilder LongRunEasyRotation(CharacterStats ds)
		{
			Random r = new Random(DateTime.Now.Millisecond);
			sb = new StringBuilder();
			this.M = ds;

			//sb.AppendLine("Maximize Holy Shocks");

			Reset();

			List<Spell> PriorityList = new List<Spell>();
			//PriorityList.Add(M.AshenHallow);
			PriorityList.Add(M.JudgmentOfLight);

			//SimCrusadersMight = false;

			if (!SimCrusadersMight)
			{
				((CrusaderStrike)M.CrusaderStrike).CrusadersMight = false;
				Spell BF = new Spell("Bestow Faith", 2.1f, 1.5f, 600, 1.0f, M);
				BF.BaseCooldown = 12f;
				BF.HolyPowerCost = -1f;
				//PriorityList.Add(BF);

				Spell LH = new Spell("Light's Hammer (warning: haste sim wrong)", 0.25f * 7f * 6f, 1.5f, 1800f, 1.0f + M.HastePercent, M);
				LH.BaseCooldown = 60f;
				PriorityList.Add(LH);

				//PriorityList.Add(M.CrusaderStrike);
			}
			else
			{
				((CrusaderStrike)M.CrusaderStrike).CrusadersMight = true;
			}

			//PriorityList.Add(M.HolyLight);
			//PriorityList.Add(M.Consecration);

			bool divinePurposeAvailable = false;
			float startTime = 0f;
			for (CastNum = 0, Time = 0f; Time < LONGTIME; CastNum++)
			{
				UpdateBonuses();

				if (BeginAvengingWrath(false))
					continue;

				if (AvengersWrathEnds > Time)
				{
					if (M.HammerOfWrath.Ready(this))
					{
						M.HammerOfWrath.Cast(this);
						waiting = false;
						continue;
					}
				}

				if (SimCrusadersMight && M.CrusaderStrike.Ready(this)
					//Save Crusader Strikes when some cooldown reduction would be wasted
					//turns out this doesn't seem to make much difference
					&&
					(M.HolyShock.CooldownRemaining(this) > (M.CrusaderStrike.CastTime() + 1.5f))
					)
				{
					M.CrusaderStrike.Cast(this);
					waiting = false;
					continue;
				}

				if (M.HolyShock.Ready(this))
				{
					M.HolyShock.Cast(this);
					if (SimShockBarrier)
						M.ShockBarrier_Perfect.Cast(this);
					waiting = false;
					continue;
				}

				if (M.LightOfDawn.Ready(this) || divinePurposeAvailable)
				{
					if (divinePurposeAvailable && SimDivinePurpose)
					{
						//half-assed workaround to get a free Light of Dawn
						this.HolyPower += 3;
						divinePurposeAvailable = false;
					}
					M.LightOfDawn.Cast(this);
					if (r.Next(1, 100) <= 15 && SimAwakening)
					{
						BeginAvengingWrath(true);
					}
					if (r.Next(1, 100) <= 15 && SimDivinePurpose)
					{
						divinePurposeAvailable = true;
					}
					continue;
				}

				bool castSomething = false;
				foreach (Spell s in PriorityList)
				{
					if (s.Ready(this))
					{
						s.Cast(this);
						castSomething = true;
						waiting = false;
						break;
					}
				}

				if (castSomething)
					continue;

				//Dead time
				Time += 0.1f;
				currentDeadTime += 0.1f;
				waiting = true;
				//sb.AppendLine("* HS CD: " + F(M.HolyShock.CooldownRemaining(this))
				//	+ "s, CS CD: " + F(M.CrusaderStrike.CooldownRemaining(this)) + "s");
			}

			Dictionary<Spell, float> castCount = new Dictionary<Spell, float>();
			foreach (Spell s in this.Casts)
			{
				float castsSoFar = 0;
				if (!castCount.TryGetValue(s, out castsSoFar))
				{
					castCount.Add(s, 1);
				}
				else
				{
					castCount[s] = castsSoFar + 1;
				}
			}

			foreach (KeyValuePair<Spell, float> kvp in castCount)
			{
				sb.AppendLine(kvp.Key.Name + ": " + kvp.Value + " casts, "
					+ F(kvp.Value / (Time/60f)) + " cpm" );
			}

			Deeps = TotalDamage / Time;
			Heeps = TotalHealing / Time;
			TankHeeps = TankHealing / Time;

			float baseRegen = 400f;
			float mps = ManaSpent / Time;
			mps = mps - baseRegen;
			float oomTime = 0f;
			if (mps > 0f)
				oomTime = 53000f / mps;

			sb.AppendLine("Period: " + F(Time) + "s, " +
					//"DPS: " + F(Deeps) + ", " +
					"HPS: " + F(Heeps) + ", " +
					"Tank HPS: " + F(TankHeeps) + ", " +
					"MPS: " + F(ManaSpent / Time) + ", " +
					"HPM: " + F(TotalHealing / ManaSpent) + ", " +
					"OOM Time: " + F(oomTime) + " seconds"
					);
			if (totalDeadTime > 0f)
			{
				sb.AppendLine("Dead time: " + totalDeadTime + " seconds ( " + F(totalDeadTime*100f / Time) + "% ) ");
			}

			return sb;
		}
		
		public StringBuilder whatIfEverySmiteWerePenance(CharacterStats ds)
		{
			sb = new StringBuilder();

			sb.AppendLine("Maintain 6 Atonements, use Penance and PW:Shield on cooldown, recast plea below 5 atonements");

			return sb;
		}

		public StringBuilder RaptureCombo(CharacterStats ds, int raidSize,
			bool useLightsWrath, bool useShadowfiend, bool useMindbender,
			bool PurgeInsteadOfSwp)
		{
			sb = new StringBuilder();

			//sb.Append("Rapture Combo, LW:" + (useLightsWrath ? "Y" : "N") + ",SF:" + (useShadowfiend ? "Y" : "N") + ":");

			//if (useMindbender)
			//{
			//	throw new Exception("Mindbender not supported yet");
			//}
			//if (!PurgeInsteadOfSwp)
			//{
			//	throw new Exception("Shadow Word: Pain not supported yet");
			//}

			//Reset();

			////Used to track the duration of the most recent loop for the purposes of 
			//// applying SW:P/Fiend damage
			//float startTime = 0f;

			//float raptureExpires = 0f;
			//float fiendExpires = 0f;
			//bool fiendUsed = false;
			//for (CastNum = 0, Time = 0f; Time < LONGTIME; CastNum++)
			//{
			//	if (HalfAssedDarkmoonSimulator)
			//		ManaSpent -= ((2438 - 610) / 2 + 610);

			//	float sinsBonus = 1f + (0.01f * Atonements);

			//	//Apply DoT damage from the last round through here
			//	if (DotExpires > Time)
			//	{
			//		float dotDamageThisRound = ds.PtwDPS * (Time - startTime) * sinsBonus;
			//		TotalDamage += dotDamageThisRound;
			//		TankHealing += dotDamageThisRound * 0.4f * (1 + ds.masteryPercent);
			//		TotalHealing += dotDamageThisRound * 0.4f * (1 + ds.masteryPercent) * Atonements;
			//	}

			//	//Apply Shadow Fiend DPS
			//	if (fiendExpires > Time)
			//	{
			//		float fiendDamageThisRound = ds.Shadowfiend.EffectPerSecond() * sinsBonus * (Time - startTime);
			//		TotalDamage += fiendDamageThisRound;
			//		TankHealing += fiendDamageThisRound * 0.4f * (1 + ds.masteryPercent);
			//		TotalHealing += fiendDamageThisRound * 0.4f * (1 + ds.masteryPercent) * Atonements;
			//	}

			//	ExpireAtonements();
				
			//	startTime = Time;
				
			//	if (raptureExpires == 0f)
			//	{
			//		//Apply Purge the Wicked first
			//		if (ApplyPurge(sb, ds)) continue;

			//		//Cast Rapture
			//		raptureExpires = Time + ds.Doomsayer;
			//	}
			//	else
			//	{
			//		if (raptureExpires < Time)
			//		{
			//			//OK, combo's over, everyone go home
			//			if (Atonements <= 5)
			//				break;
			//		}
			//		else
			//		{
			//			//Reset Power Word: Shield's cooldown
			//			// while Rapture is still up
			//			ShieldReady = 0;
			//		}
			//	}

			//	//Cast one more Power Word: Shield after Rapture ends
			//	// if PW:S has been cast without Rapture at least once,
			//	// ShieldReady time will be greater than 0
			//	if (ShieldReady == 0)
			//	{
			//		if (CastShield(sb, ds)) continue;
			//	}

			//	if (ApplyPurge(sb, ds)) continue;

			//	if (useLightsWrath)
			//	{
			//		DamageSpell(ds.LightsWrath, 1.0f, 1f + (Atonements * 0.1f));

			//		useLightsWrath = false;
			//		continue;
			//	}

			//	//TODO: Mindbender support
			//	if (useShadowfiend && !fiendUsed)
			//	{
			//		//Cast Shadowfiend
			//		fiendUsed = true;
			//		fiendExpires = Time + 12f;
			//	}

			//	if (CastPenance(sb, ds)) continue;

			//	DamageSpell(ds.Smite, BorrowedTimeUp ? ds.BorrowedTime : 1.0f);
			//	BorrowedTimeUp = false;

			//	TankHealing += ds.SmiteAbsorb.AvgEffect();
			//	TotalHealing += ds.SmiteAbsorb.AvgEffect();
			//	Log(sb, ": Casting Smite");

			//	startTime = Time;
			//}

			//Deeps = TotalDamage / Time;
			//Heeps = TotalHealing / Time;
			//TankHeeps = TankHealing / Time;

			sb.AppendLine(" Duration: " + F(Time) + "s, " +
					"DPS: " + F(Deeps) + ", " +
					"HPS: " + F(Heeps) + ", " +
					"Tank HPS: " + F(TankHeeps) + ", " +
					"MPS: " + F(ManaSpent / Time) + ", " +
					"HPM: " + F(TotalHealing / ManaSpent)
					);

			return sb;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disculator
{
	class FightStatus
	{
		public float AtonementDuration = 15f;

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

		public bool fullLog = false;

		public void Reset()
		{
			AtonementDuration = 15f;

			CastNum = 0;
			Time = 0f;

			TotalHealing = 0f;
			TotalDamage = 0f;
			TankHealing = 0f;
			Atonements = 0;
			ManaSpent = 0f;
			DotCast = false;
			StackedAtonement = false;

			Deeps = 0f;
			Heeps = 0f;
			TankHeeps = 0f;

			AtonementExpirations = new float[12];
			for (int i = 0; i < AtonementExpirations.Length; i++)
			{
				AtonementExpirations[i] = 9001f;
			}

			DotExpires = 0f;
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

		private void CastSpell(Spell theSpell)
		{
			Time += theSpell.CastTime();
			ManaSpent += theSpell.Mana;
		}

		private void DamageSpell(Spell theSpell)
		{
			CastSpell(theSpell);

			TotalDamage += theSpell.AvgEffect();
			TankHealing += theSpell.AtonementEffect();
			TotalHealing += Atonements * theSpell.AtonementEffect();
		}

		private void HealSpell(Spell theSpell, int targets)
		{
			CastSpell(theSpell);

			TankHealing += theSpell.AvgEffect() / targets;
			TotalHealing += theSpell.AvgEffect();
		}

		public static float LONGTIME = 500f;

		public StringBuilder LongRunEasyRotation(Disculator ds)
		{

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Let Atonemenet fall off, plea to 6, maintain PtW, spam Smite");

			ds.Raycalculate();
			Reset();

			float PenanceCooldown;

			float startTime = 0f;
			for (CastNum = 0, Time = 0f; Time < LONGTIME; CastNum++)
			{
				//Apply DoT damage from the last round through here
				if (DotExpires > Time)
				{
					TotalDamage += ds.PtwDPS * (Time - startTime);
					TankHealing += ds.PtwDPS * (Time - startTime) * 0.4f * (1 + ds.masteryPercent);
					TankHealing += Atonements * ds.PtwDPS * (Time - startTime) * 0.4f * (1 + ds.masteryPercent);
				}

				//Clean up Atonements
				for (int i = 0; i < AtonementExpirations.Length; i++)
				{
					if (AtonementExpirations[i] < Time)
					{
						AtonementExpirations[i] = 999f;
						Atonements--;
					}
				}

				startTime = Time;

				if ((DotExpires - Time) <= 6f)
				{
					//Purge the Wicked needs some special logic, so
					// use castspell just for time/mana
					CastSpell(ds.Ptw);

					if (DotExpires > Time)
					{
						DotExpires += 20f;
					}
					else
					{
						DotExpires = Time + 20f;
					}

					TotalDamage += ds.Ptw.AvgEffect() - ds.PtwDot.AvgEffect();
					TankHealing += ds.Ptw.AtonementEffect() - ds.PtwDot.AtonementEffect();
					TotalHealing += Atonements * (ds.Ptw.AtonementEffect() - ds.PtwDot.AtonementEffect());
					Log(sb, ": Casting Purge the Wicked");
					
					continue;
				}


				if (Atonements == 0 && StackedAtonement == true)
				{
					StackedAtonement = false;
				}

				if (!StackedAtonement)
				{
					AtonementExpirations[Atonements] = Time + AtonementDuration;
					//CastNum Plea
					ManaSpent += ds.Plea.Mana * (Atonements + 1);
					if (Atonements == 0)
					{
						TankHealing += ds.Plea.AvgEffect();
					}
					TotalHealing += ds.Plea.AvgEffect();

					Atonements++;
					Time += ds.Plea.CastTime();
					StackedAtonement = (Atonements == 6);
					Log(sb, ": Casting Plea");
					continue;
				}

				

				DamageSpell(ds.Smite);
				TankHealing += ds.SmiteAbsorb.AvgEffect();
				TotalHealing += ds.SmiteAbsorb.AvgEffect();
				Log(sb, ": Casting Smite");

			}


			Deeps = TotalDamage / Time;
			Heeps = TotalHealing / Time;
			TankHeeps = TankHealing / Time;

			sb.AppendLine("Period: " + F(Time) + "s, " +
					"DPS: " + F(Deeps) + ", " +
					"HPS: " + F(Heeps) + ", " +
					"Tank HPS: " + F(TankHeeps) + ", " +
					"MPS: " + F(ManaSpent / Time) + ", " +
					"HPM: " + F(TotalHealing / ManaSpent)
					);

			return sb;
		}

		public StringBuilder EasyRotation(Disculator ds)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Let Atonemenet fall off, plea to 6, hit PtW, spam Smite");

			Reset();

			for (CastNum = 0, Time = 0f; Time < (AtonementDuration + ds.Plea.CastTime() * 6); CastNum++)
			{
				if (!StackedAtonement)
				{
					AtonementExpirations[Atonements] = Time + AtonementDuration;
					//CastNum Plea
					ManaSpent += ds.Plea.Mana * (Atonements + 1);
					if (Atonements == 0)
					{
						TankHealing += ds.Plea.AvgEffect();
					}
					TotalHealing += ds.Plea.AvgEffect();

					Atonements++;
					Time += ds.Plea.CastTime();
					StackedAtonement = (Atonements == 6);
					Log(sb, ": Casting Plea");
					continue;
				}

				if (!DotCast)
				{
					//CastNum Purge the Wicked
					Time += ds.Ptw.CastTime();

					DotExpires = Time + 20f;

					TotalDamage += ds.Ptw.AvgEffect() - ds.PtwDot.AvgEffect();
					TankHealing += ds.Ptw.AtonementEffect() - ds.PtwDot.AtonementEffect();
					TotalHealing += Atonements * (ds.Ptw.AtonementEffect() - ds.PtwDot.AtonementEffect());
					ManaSpent += ds.Ptw.Mana;
					Log(sb, ": Casting Purge the Wicked");

					DotCast = true;
					continue;
				}

				for (int i = 0; i < Atonements; i++)
				{
					if (AtonementExpirations[i] < Time)
					{
						AtonementExpirations[i] = 999f;
						Atonements--;
					}
				}


				if (DotExpires > Time)
				{
					TotalDamage += ds.PtwDPS * ds.Smite.CastTime();
					TankHealing += ds.PtwDPS * ds.Smite.CastTime() * 0.4f * (1 + ds.masteryPercent);
					TankHealing += Atonements * ds.PtwDPS * ds.Smite.CastTime() * 0.4f * (1 + ds.masteryPercent);
				}

				DamageSpell(ds.Smite);
				TankHealing += ds.SmiteAbsorb.AvgEffect();
				TotalHealing += ds.SmiteAbsorb.AvgEffect();
				Log(sb, ": Casting Smite");

			}


			Deeps = TotalDamage / Time;
			Heeps = TotalHealing / Time;
			TankHeeps = TankHealing / Time;

			sb.AppendLine("Period: " + F(Time) + "s, " +
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

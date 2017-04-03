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
		
		float PenanceReady = 0f;
		float DarkSideReady = 0f;
		float ShieldReady = 0f;

		bool BurstUp = false;
		bool DarkSideUp = false;

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

			PenanceReady = 0f;
			DarkSideReady = 0f;
			ShieldReady = 0f;
			BurstUp = false;

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

		private void CastSpell(Spell theSpell, float bonusHaste)
		{
			Time += theSpell.CastTime() / bonusHaste;
			ManaSpent += theSpell.Mana;
		}

		private void CastSpell(Spell theSpell)
		{
			CastSpell(theSpell, 1.0f);
		}

		private void DamageSpell(Spell theSpell)
		{
			DamageSpell(theSpell, 1.0f);
		}

		private void DamageSpell(Spell theSpell, float bonusHaste)
		{
			DamageSpell(theSpell, bonusHaste, 1.0f);
		}

		private void DamageSpell(Spell theSpell, float bonusHaste, float bonusDamage)
		{
			CastSpell(theSpell, bonusHaste);

			TotalDamage += theSpell.AvgEffect() * bonusDamage;
			//Assumes we always have an atonement up on at least one tank
			TankHealing += theSpell.AtonementEffect() * bonusDamage;
			TotalHealing += Atonements * theSpell.AtonementEffect() * bonusDamage;
		}

		private void HealSpell(Spell theSpell, int targets)
		{
			CastSpell(theSpell);

			TankHealing += theSpell.AvgEffect() / targets;
			TotalHealing += theSpell.AvgEffect();
		}

		private void AddAtonement()
		{
			for (int i=0; i<AtonementExpirations.Length; i++)
			{
				if (AtonementExpirations[i] == 9001f)
				{
					AtonementExpirations[i] = Time + AtonementDuration;
					break;
				}
			}
			Atonements++;
		}

		private void ExpireAtonements()
		{
			//Clean up Atonements
			for (int i = 0; i < AtonementExpirations.Length; i++)
			{
				if (AtonementExpirations[i] < Time)
				{
					AtonementExpirations[i] = 9001f;
					Atonements--;
				}
			}
		}

		public static float LONGTIME = 500f;

		public StringBuilder LongRunEasyRotation(Disculator ds)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Let Atonement fall off, plea to 6, maintain PtW, spam Smite");

			//ds.Raycalculate(); //Allow the caller to tweak things
			Reset();


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

				ExpireAtonements();

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
					//CastNum Plea
					ManaSpent += ds.Plea.Mana * (Atonements + 1);
					if (Atonements == 0)
					{
						TankHealing += ds.Plea.AvgEffect();
					}
					TotalHealing += ds.Plea.AvgEffect();

					AddAtonement();
					Time += ds.Plea.CastTime();
					StackedAtonement = (Atonements == 6);
					Log(sb, ": Casting Plea");
					continue;
				}
				
				if (BurstUp)
				{
					DamageSpell(ds.Smite, ds.BurstOfLight);
					BurstUp = false;
				}
				else
				{
					DamageSpell(ds.Smite);
				}
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

		public StringBuilder LongRun_PenanceAndShield(Disculator ds)
		{

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Maintain 6 Atonements, use Penance and PW:Shield on cooldown");

			//ds.Raycalculate(); //Allow the caller to tweak things
			Reset();
			
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

				ExpireAtonements();

				if (Time > DarkSideReady && !DarkSideUp)
				{
					DarkSideUp = true;
				}

				//--------- Begin Priority List ------------
				startTime = Time;

				//Cast Purge when it falls off
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

				//Power Word: Shield on cooldown
				if (Time > ShieldReady)
				{
					HealSpell(ds.Shield, 1);
					if (Atonements == 0)
					{
						AddAtonement();
					}
					else
					{
						AtonementExpirations[0] = Time + AtonementDuration;
					}
					BurstUp = true;

					ShieldReady = Time + ds.ShieldCD;
					
					Log(sb, ": Casting Power Word: Shield");
					continue;
				}

				//-------- Stack Atonement to 6 ---------
				if (Atonements < 2 && StackedAtonement == true)
				{
					StackedAtonement = false;
				}
				else
				{
					//Penance so long as you're not restacking Atonements
					if (PenanceReady < Time)
					{
						if (BurstUp)
						{
							DamageSpell(ds.CastigatedPenance, ds.BurstOfLight);
							BurstUp = false;
						}
						else
						{
							DamageSpell(ds.CastigatedPenance);
						}
						PenanceReady = Time + ds.PenanceCD;
						Log(sb, ": Casting Penance");
						continue;
					}
				}

				if (!StackedAtonement)
				{
					ManaSpent += ds.Plea.Mana * (Atonements + 1);
					Time += ds.Plea.CastTime();

					//Cast Plea
					if (Atonements == 0)
					{
						TankHealing += ds.Plea.AvgEffect();
					}
					TotalHealing += ds.Plea.AvgEffect();

					AddAtonement();
					BurstUp = true;
					StackedAtonement = (Atonements >= 6);
					Log(sb, ": Casting Plea");
					continue;
				}

				if (Time > DarkSideReady)
				{
					DarkSideUp = true;
					DarkSideReady = Time + ds.PowerOfTheDarkSideCD;
				}
				
				DamageSpell(ds.Smite, BurstUp ? ds.BurstOfLight : 1.0f, DarkSideUp?1.5f:1.0f);
				BurstUp = false;
				if (DarkSideUp)
				{
					DarkSideReady = Time + ds.PowerOfTheDarkSideCD;
					DarkSideUp = false;
				}

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

			sb.AppendLine("Let Atonement fall off, plea to 6, hit PtW, spam Smite");

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

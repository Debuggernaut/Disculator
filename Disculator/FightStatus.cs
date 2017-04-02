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

		public int CastNum;
		public float Time;

		public float TotalHealing = 0f;
		public float TotalDamage = 0f;
		public float TankHealing = 0f;
		public int Atonements = 0;
		public float ManaSpent = 0f;
		public bool DotCast = false;
		public bool StackedAtonement = false;

		public float[] AtonementExpirations;
		public float DotExpires = 999f;

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
			sb.AppendLine(F(Time) + "s: " + F(TotalHealing) + "," + F(TotalDamage)
				+ "," + Atonements.ToString() + s);
		}

		public StringBuilder Rotate(Disculator ds)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Let Atonemenet fall off, plea to 6, hit PtW, spam Smite");

			AtonementExpirations = new float[12];
			for (int i = 0; i < AtonementExpirations.Length; i++)
			{
				AtonementExpirations[i] = 9001f;
			}

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
					Log(sb, ": CastNuming Plea");
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

				Time += ds.Smite.CastTime();

				if (DotExpires > Time)
				{
					TotalDamage += ds.PtwDPS * ds.Smite.CastTime();
					TankHealing += ds.PtwDPS * ds.Smite.CastTime() * 0.4f * (1 + ds.masteryPercent);
					TankHealing += Atonements * ds.PtwDPS * ds.Smite.CastTime() * 0.4f * (1 + ds.masteryPercent);
				}

				TotalDamage += ds.Smite.AvgEffect();
				TankHealing += ds.Smite.AtonementEffect() + ds.SmiteAbsorb.AvgEffect();
				TotalHealing += Atonements * ds.Smite.AtonementEffect() + ds.SmiteAbsorb.AvgEffect();
				ManaSpent += ds.Smite.Mana;
				Log(sb, ": CastNuming Smite");

			}

			sb.AppendLine("Period: " + F(Time) + "s, " +
				"DPS: " + F(TotalDamage / Time) + ", " +
				"HPS: " + F(TotalHealing / Time) + ", " +
				"Tank HPS: " + F(TankHealing / Time) + ", " +
				"MPS: " + F(ManaSpent / Time) + ", " +
				"HPM: " + F(TotalHealing / ManaSpent)
				);

			return sb;
		}
	}
}

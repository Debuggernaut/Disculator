using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disculator
{
	public class InstantSpell:Spell
	{
		public InstantSpell(
			String name,
			float scaler,
			float mana,
			float holyPower,
			CharacterStats m)
		{
			Name = name;
			Scaler = scaler;
			BaseCastTime = 1.5f;
			Mana = mana;
			ExtraScaler = 1.0f;
			HolyPowerCost = holyPower;
			M = m;
		}
	}

	public class Spell
	{
		public String Name;

		public float BonusCritChance = 0f;
		public float Scaler;
		public float BaseCastTime;
		public float Mana;
		public float ExtraScaler;
		public float HolyPowerCost = 0f;
		public bool CannotCrit = false;
		public float BaseCooldown = 0f;

		public bool Damage = false;

		public const int MAX_CHARGES = 5;
		public int BaseCharges = 1;

		public int TargetCount = 1;

		// Current state data used for fight sim
		private int CurrentCharges = 1;
		private float[] ChargeAvailable = new float[MAX_CHARGES];

		public CharacterStats M;

		public void Reset()
		{
			CurrentCharges = BaseCharges;
			for (int i=0; i<BaseCharges; i++)
			{
				ChargeAvailable[i] = 0f;
			}
		}

		public Spell()
		{
		}

		public Spell(
			String name,
			float scaler,
			float basecasttime,
			float mana,
			float extrascaler,
			CharacterStats m)
		{
			Name = name;
			Scaler = scaler;
			BaseCastTime = basecasttime;
			Mana = mana;
			ExtraScaler = extrascaler;
			M = m;
			HolyPowerCost = 0;
		}

		public Spell(
			String name,
			float scaler,
			float mana,
			float holyPower,
			CharacterStats m)
		{
			Name = name;
			Scaler = scaler;
			BaseCastTime = 1.5f;
			Mana = mana;
			ExtraScaler = 1.0f;
			HolyPowerCost = holyPower;
			M = m;

		}

		public float HasteScaler()
		{
			return 1 + M.HastePercent;
		}

		public float AtonementRate()
		{
			return 0f;
		}

		public float CastTime()
		{
			float x = this.BaseCastTime / HasteScaler();
			if (x < 1.0f && x > 0f)
				return 1.0f;
			else
				return x;
		}

		public float BaseEffect()
		{
			float effect = Scaler
				* M.scaledSpellPower
				* ExtraScaler
				* M.TempScaler;
			if (Damage)
			{
				return effect;
			}
			else
			{
				return effect 
					* (1 + (M.MasteryPercent*M.MasteryEffectiveness)) * M.HealingEffectiveness;
			}
		}

		public float Cooldown()
		{
			if (this.BaseCooldown < 30f)
				return this.BaseCooldown / HasteScaler();
			else
				return this.BaseCooldown;
		}

		public float AvgEffect()
		{
			if (CannotCrit)
				return BaseEffect();

			float critChance = M.CritPercent + this.BonusCritChance;

			if (critChance > 1.0f)
				critChance = 1.0f;

			return BaseEffect() * (1 + critChance);
		}

		public float EffectPerSecond()
		{
			return AvgEffect() / CastTime();
		}

		public float EffectPerMana()
		{
			return AvgEffect() / Mana;
		}

		public float ManaPerSecond()
		{
			return Mana / CastTime();
		}

		public float AtonementEffect()
		{
			return AvgEffect() * AtonementRate();
		}

		public float CooldownRemaining(FightSim sim)
		{
			if (ChargeAvailable[0] < sim.Time)
			{
				return 0f;
			}
			else
			{
				return ChargeAvailable[0] - sim.Time;
			}
		}

		public bool Ready(FightSim sim)
		{
			if (sim.Time >= ChargeAvailable[0])
			{
				if (this.HolyPowerCost > 0f)
				{
					return sim.HolyPower >= this.HolyPowerCost;
				}
				else
					return true;
			}

			return false;
		}

		private void UpdateCharges(float time)
		{
			/*
			 * Charge example:
			 * t=0: Both charges available, 10s cooldown
			 * t=1: Cast spell, available[0] = 0f, available[1] = 10f
			 * t=2: Cast spell, available[0] = 10f, available[1] = 20f
			 * t=12, cast spell,available[0] = 20f, available[1] = 30f
			 * ---
			 * t=45, cast spell, avail[0] = 30f, avail[1]=55f
			 */
			if (BaseCharges > 1)
			{
				for (int i = 0; i < BaseCharges - 1; i++)
				{
					ChargeAvailable[i] = ChargeAvailable[i + 1];
				}

				if (time > ChargeAvailable[BaseCharges - 2])
				{
					ChargeAvailable[BaseCharges - 1] = time + Cooldown();
				}
				else
				{
					ChargeAvailable[BaseCharges - 1] = ChargeAvailable[BaseCharges - 2] + Cooldown();
				}
			}
			else
			{
				ChargeAvailable[0] = time + Cooldown();
			}
		}

		public void AdjustCooldown(float adjustment)
		{
			ChargeAvailable[0] += adjustment;
		}

		public virtual bool Cast(FightSim sim)
		{
			if (!Ready(sim))
				return false;

			UpdateCharges(sim.Time);

			if (Damage)
			{
				float damageThisTime = this.AvgEffect();

				sim.TotalDamage += damageThisTime;
			}
			else
			{
				sim.TankHealing += this.AvgEffect() / TargetCount;
				sim.TotalHealing += this.AvgEffect();
			}

			sim.LogSpell(this);

			sim.Time += this.CastTime();
			sim.ManaSpent += this.Mana;
			sim.Casts.Add(this);
			sim.HolyPower -= this.HolyPowerCost;
			if (sim.HolyPower > 5f)
			{
				sim.HolyPower = 5f;
			}

			return true;
		}
	}

	public class CrusaderStrike : Spell
	{
		public bool CrusadersMight = true;

		public CrusaderStrike(CharacterStats stats)
		{
			Name = "Crusader Strike";
			Scaler = 0.8f;
			BaseCastTime = 1.5f;
			Mana = 900;
			ExtraScaler = 1.0f;
			HolyPowerCost = -1.0f;
			M = stats;
			BaseCooldown = 6f;
		}

		public override bool Cast(FightSim sim)
		{
			if (CrusadersMight)
			{
				if (Ready(sim))
				{
					sim.M.HolyShock.AdjustCooldown(-1.5f);
				}
			}
			return base.Cast(sim);
		}
	}
}

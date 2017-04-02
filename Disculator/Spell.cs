using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disculator
{
	public class Spell
	{
		public String Name;

		public float Scaler;
		public float BaseCastTime;
		public float Mana;
		public float ExtraScaler;

		public Disculator M;

		public Spell()
		{

		}

		public Spell(
			String name,
			float scaler,
			float basecasttime,
			float mana,
			float extrascaler,
			Disculator m)
		{
			Name = name;
			Scaler = scaler;
			BaseCastTime = basecasttime;
			Mana = mana;
			ExtraScaler = extrascaler;
			M = m;

		}

		public float HasteScaler()
		{
			return 1 + M.hastePercent;
		}

		public float AtonementRate()
		{
			return 0.4f * (1 + M.masteryPercent);
		}

		public float CastTime()
		{
			return BaseCastTime / HasteScaler();
		}

		public float BaseEffect()
		{
			return Scaler * M.scaledSpellPower * ExtraScaler;
		}

		public float AvgEffect()
		{
			//TODO: Spells with increased crit chance
			return BaseEffect() * (1 + M.critPercent);
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
	}
}

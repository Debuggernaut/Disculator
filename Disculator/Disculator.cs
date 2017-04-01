using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disculator
{
	class Disculator
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

		public void Raycalculate()
		{

			critPercent = (critRating / 400f + 5f) / 100f;
			hastePercent = hasteRating / 375f / 100f;
			masteryPercent = (masteryRating * 3f / 800f + 12f) / 100f;
			verPercent = verRating / 475f / 100f;

			scaledSpellPower = intellect * (1 + verPercent) * 1.05f * 1.1f;

		}
	}
}

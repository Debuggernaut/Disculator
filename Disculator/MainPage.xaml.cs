using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Disculator
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
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

		public Spell[] HealSpells;
		public Spell[] DamageSpells;

		public MainPage()
		{
			this.InitializeComponent();
			Raycalculate();
		}

		private void Raycalculate()
		{
			String[] spellColumns = new String[]
			{
				"blah"
			};

			HealSpells = new Spell[]
			{
				new Spell("Plea (0 Atonements)", 2.25f, 1.5f, 3960, 1.0f, this),
				new Spell("Plea (3 Atonements)", 2.25f, 1.5f, 3960*3, 1.0f, this),
				new Spell("Plea (6 Atonements)", 2.25f, 1.5f, 3960*6, 1.0f, this),
				new Spell("Shadow Mend (Heavy Incoming Damage)", 7.5f, 1.5f, 8800, 1.2f, this),
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
			};

			intellect = int.Parse(this.intbox.Text);
			critRating = int.Parse(this.critbox.Text);
			hasteRating = int.Parse(this.hastebox.Text);
			masteryRating = int.Parse(this.masterybox.Text);
			verRating = int.Parse(this.verbox.Text);

			critPercent = (critRating / 400f + 5f) / 100f;
			hastePercent = hasteRating / 375f / 100f;
			masteryPercent = (masteryRating * 3f / 800f + 12f) / 100f;
			verPercent = verRating / 475f / 100f;

			scaledSpellPower = intellect * (1 + verPercent) * 1.05f * 1.1f;

			this.critpercentbox.Text = critPercent.ToString("P");
			this.hastepercentbox.Text = hastePercent.ToString("P");
			this.masterypercentbox.Text = masteryPercent.ToString("P");
			this.verpercentbox.Text = verPercent.ToString("P");

			StringBuilder sb = new StringBuilder();

			sb.Append("Spellpower: " + scaledSpellPower.ToString("#,#") + " (int * (1 + versatility) * 1.05 * 1.10)");

			this.outbox.Text = sb.ToString();

			this.SpellGrid.Children.Clear();
		}

		private void recalc_Click(object sender, RoutedEventArgs e)
		{
			Raycalculate();
		}
	}
}

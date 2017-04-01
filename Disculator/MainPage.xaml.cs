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

		String[] HealColumns;
		String[] DamageColumns;
		Spell plea;
		Spell smend;

		public MainPage()
		{
			HealColumns = new String[]
			{
				"Spell Name",
				"Base Heal",
				"Cast Time",
				"HPS",
				"HPM",
				"Average Heal",
				"Mana",
				"MPS"
			};

			DamageColumns = new String[]
			{
				"Spell Name",
				"Base Dmg",
				"Cast Time",
				"DPS",
				"HPM (1)",
				"HPM (5)",
				"HPM (7)",
				"HPM (15)",
				"Mana",
				"DPM",
				"MPS"
			};

			plea = new Spell("Plea (0 Atonements)", 2.25f, 1.5f, 3960, 1.0f, this);
			smend = new Spell("Shadow Mend (Heavy Incoming Damage)", 7.5f, 1.5f, 8800, 1.2f, this);
			HealSpells = new Spell[]
			{
				plea,
				new Spell("Plea (3 Atonements)", 2.25f, 1.5f, 3960*3, 1.0f, this),
				new Spell("Plea (6 Atonements)", 2.25f, 1.5f, 3960*6, 1.0f, this),
				smend,
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
				//new Spell("Plea", 2.25f, 1.5f, 8800, 1.0f, this),
			};

			this.InitializeComponent();
			Raycalculate();
		}

		private String F(float input)
		{
			return input.ToString("#,#.#");
		}

		private void Raycalculate()
		{

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

			PopulateHealGrid();
		}

		private void PopulateHealGrid()
		{
			if (HealGrid.ColumnDefinitions.Count == 0)
			{
				for (int c = 0; c < HealColumns.Length; c++)
					HealGrid.ColumnDefinitions.Add(new ColumnDefinition());

				for (int r = 0; r < HealSpells.Length + 1; r++)
					HealGrid.RowDefinitions.Add(new RowDefinition());

			}

			this.HealGrid.Children.Clear();
			for (int c = 0; c < HealColumns.Length; c++)
			{

				TextBlock t = new TextBlock();
				//t.Style = (Style) Application.Current.Resources["TitularLine"];
				t.Style = (Style)this.Resources["TitularLine"];
				t.Text = HealColumns[c];

				HealGrid.Children.Add(t);

				t.SetValue(Grid.ColumnProperty, c);
				t.SetValue(Grid.RowProperty, 0);
			}

			for (int s = 0; s < HealSpells.Length; s++)
			{
				for (int c = 0; c < HealColumns.Length; c++)
				{

					TextBlock t = new TextBlock();
					switch (c)
					{
						case 0:
							t.Text = HealSpells[s].Name;
							break;
						case 1:
							t.Text = F(HealSpells[s].BaseEffect());
							break;
						case 2:
							t.Text = F(HealSpells[s].CastTime());
							break;
						case 3:
							t.Text = F(HealSpells[s].EffectPerSecond());
							break;
						case 4:
							t.Text = F(HealSpells[s].EffectPerMana());
							break;
						case 5:
							t.Text = F(HealSpells[s].AvgEffect());
							break;
						case 6:
							t.Text = F(HealSpells[s].Mana);
							break;
						case 7:
							t.Text = F(HealSpells[s].ManaPerSecond());
							break;
						default:
							t.Text = "???";
							break;
					}

					t.Style = (Style)this.Resources["Statistic"];
					HealGrid.Children.Add(t);
					t.SetValue(Grid.ColumnProperty, c);
					t.SetValue(Grid.RowProperty, s + 1);
				}
			}
		}

		private void recalc_Click(object sender, RoutedEventArgs e)
		{
			Raycalculate();
		}
	}
}

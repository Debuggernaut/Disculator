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
		String[] HealColumns;
		String[] DamageColumns;

		Disculator ds;

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
				"Cast\r\nTime",
				"DPS",
				"HPS",
				"HPM",
				"Mana",
				"DPM",
				"MPS"
			};
			
			this.InitializeComponent();
			Raycalculate();
		}

		private String F(float input)
		{
			if (input > 30*1000f)
			{
				return (input/1000f).ToString("#,#") + "K";
			}

			if (input < 100)
			{
				return input.ToString("#,#.##");
			}

			return input.ToString("#,#.#");
		}

		private void Raycalculate()
		{
			ds = new Disculator();
			ds.intellect = int.Parse(this.intbox.Text);
			ds.critRating = int.Parse(this.critbox.Text);
			ds.hasteRating = int.Parse(this.hastebox.Text);
			ds.masteryRating = int.Parse(this.masterybox.Text);
			ds.verRating = int.Parse(this.verbox.Text);

			ds.AvgAtonements = int.Parse(this.atonementsbox.Text);

			ds.artifactTraits = int.Parse(this.pointsbox.Text);

			ds.Raycalculate();

			this.critpercentbox.Text = ds.critPercent.ToString("P");
			this.hastepercentbox.Text = ds.hastePercent.ToString("P");
			this.masterypercentbox.Text = ds.masteryPercent.ToString("P");
			this.verpercentbox.Text = ds.verPercent.ToString("P");

			this.artifactDamageBonusPercent.Text = ds.allDamageBonus.ToString("P");

			this.MindbenderSwingsBox.Text = ds.ShadowfiendSwings.ToString();

			StringBuilder sb = new StringBuilder();

			sb.Append("Spellpower: " + ds.scaledSpellPower.ToString("#,#") + " (int * (1 + versatility) * 1.05 * 1.10)");

			this.outbox.Text = sb.ToString();

			PopulateHealGrid();
			PopulateDamageGrid();

			Rotations();
		}

		private void PopulateDamageGrid()
		{
			if (DmgGrid.ColumnDefinitions.Count == 0)
			{
				for (int c = 0; c < DamageColumns.Length; c++)
					DmgGrid.ColumnDefinitions.Add(new ColumnDefinition());

				for (int r = 0; r < ds.DamageSpells.Length + 1; r++)
					DmgGrid.RowDefinitions.Add(new RowDefinition());

			}

			this.DmgGrid.Children.Clear();
			for (int c = 0; c < DamageColumns.Length; c++)
			{

				TextBlock t = new TextBlock();
				//t.Style = (Style) Application.Current.Resources["TitularLine"];
				t.Style = (Style)this.Resources["TitularLine"];
				t.Text = DamageColumns[c];

				DmgGrid.Children.Add(t);

				t.SetValue(Grid.ColumnProperty, c);
				t.SetValue(Grid.RowProperty, 0);
			}

			for (int s = 0; s < ds.DamageSpells.Length; s++)
			{
				for (int c = 0; c < DamageColumns.Length; c++)
				{

					TextBlock t = new TextBlock();
					switch (c)
					{
						case 0:
							t.Text = ds.DamageSpells[s].Name;
							break;
						case 1:
							t.Text = F(ds.DamageSpells[s].BaseEffect());
							break;
						case 2:
							t.Text = F(ds.DamageSpells[s].CastTime());
							break;
						case 3:
							t.Text = F(ds.DamageSpells[s].EffectPerSecond());
							break;
						case 4:
							t.Text = F(ds.DamageSpells[s].AtonementEffect()* ds.AvgAtonements / ds.DamageSpells[s].CastTime());
							break;
						case 5:
							t.Text = F(ds.DamageSpells[s].AtonementEffect() * ds.AvgAtonements / ds.DamageSpells[s].Mana);
							break;
						case 6:
							t.Text = F(ds.DamageSpells[s].Mana);
							break;
						case 7:
							t.Text = F(ds.DamageSpells[s].AvgEffect() / ds.DamageSpells[s].Mana);
							break;
						case 8:
							t.Text = F(ds.DamageSpells[s].ManaPerSecond());
							break;
						default:
							t.Text = "???";
							break;
					}

					t.Style = (Style)this.Resources["Statistic"];
					DmgGrid.Children.Add(t);
					t.SetValue(Grid.ColumnProperty, c);
					t.SetValue(Grid.RowProperty, s + 1);
				}
			}
		}

		private void PopulateHealGrid()
		{
			if (HealGrid.ColumnDefinitions.Count == 0)
			{
				for (int c = 0; c < HealColumns.Length; c++)
					HealGrid.ColumnDefinitions.Add(new ColumnDefinition());

				for (int r = 0; r < ds.HealSpells.Length + 1; r++)
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

			for (int s = 0; s < ds.HealSpells.Length; s++)
			{
				for (int c = 0; c < HealColumns.Length; c++)
				{

					TextBlock t = new TextBlock();
					switch (c)
					{
						case 0:
							t.Text = ds.HealSpells[s].Name;
							break;
						case 1:
							t.Text = F(ds.HealSpells[s].BaseEffect());
							break;
						case 2:
							t.Text = F(ds.HealSpells[s].CastTime());
							break;
						case 3:
							t.Text = F(ds.HealSpells[s].EffectPerSecond());
							break;
						case 4:
							t.Text = F(ds.HealSpells[s].EffectPerMana());
							break;
						case 5:
							t.Text = F(ds.HealSpells[s].AvgEffect());
							break;
						case 6:
							t.Text = F(ds.HealSpells[s].Mana);
							break;
						case 7:
							t.Text = F(ds.HealSpells[s].ManaPerSecond());
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

		private void Rotations()
		{

			FightStatus fs = new FightStatus();
			StringBuilder sb = fs.EasyRotation(ds);

			this.RotationBox.Text = sb.ToString();

			Disculator dOrig = ds.clone();

			Disculator dHasty = dOrig.clone();
			dHasty.hasteRating += 2000;
			fs = new FightStatus();
			sb = fs.EasyRotation(dHasty);
			this.RotationBox.Text += sb.ToString();

			Disculator dCrit = dOrig.clone();
			dCrit.critRating += 2000;
			fs = new FightStatus();
			sb = fs.EasyRotation(dCrit);
			this.RotationBox.Text += sb.ToString();


			dOrig.masteryRating += 2000;
			fs = new FightStatus();
			sb = fs.EasyRotation(dOrig);
			this.RotationBox.Text += sb.ToString();
		}


		private void recalc_Click(object sender, RoutedEventArgs e)
		{
			Raycalculate();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			FightStatus fs = new FightStatus();
			this.RotationBox.Text = fs.LongRunEasyRotation(ds).ToString();
		}
	}
}

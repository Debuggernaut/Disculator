﻿using System;
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

		public int artifactTraits;
		public float allDamageBonus;

		public float DarkestShadows = 1.1f; //10/3% Shadow Mend per point
		public float BurstOfLight = 1.2f; //5% PW:Radiance per point
		public float ShieldOfFaith = 1.2f; //5% bonus to PW:Shield per point
		public float Confession = 1.12f; //4% to Penance per point

		public float EdgeOfDarkAndLight = 1.15f; //5% to SWP/PtW per point

		public float Skjoldr = 1.15f; //Legendary wrists, +15% to PW:Shield

		public float AvgAtonements;

		public Spell[] HealSpells;
		public Spell[] DamageSpells;

		String[] HealColumns;
		String[] DamageColumns;
		Spell Plea;
		Spell Smend;
		Spell SmiteAbsorb;

		Spell Smite;
		Spell CastigatedPenance;
		Spell RegularPenance;
		Spell Swp;
		Spell Ptw;
		Spell SwpDot;
		Spell PtwDot;

		float PtwDPS = 0f;
		float SwpDPS = 0f;

		Spell LightsWrath;

		Spell ShadowfiendSwing;
		Spell MindbenderSwing;

		Spell Shadowfiend;
		Spell Mindbender;

		int ShadowfiendSwings;

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

			intellect = int.Parse(this.intbox.Text);
			critRating = int.Parse(this.critbox.Text);
			hasteRating = int.Parse(this.hastebox.Text);
			masteryRating = int.Parse(this.masterybox.Text);
			verRating = int.Parse(this.verbox.Text);

			AvgAtonements = int.Parse(this.atonementsbox.Text);

			artifactTraits = int.Parse(this.pointsbox.Text);
			allDamageBonus = 1.0f;
			for (int i = 0; i < artifactTraits; i++)
				allDamageBonus = allDamageBonus * 1.0065f;

			critPercent = (critRating / 400f + 5f) / 100f;
			hastePercent = hasteRating / 375f / 100f;
			masteryPercent = (masteryRating * 3f / 800f + 12f) / 100f;
			verPercent = verRating / 475f / 100f;

			scaledSpellPower = intellect * (1 + verPercent) * 1.05f * 1.1f;

			//Rounds up to the next number of swings
			ShadowfiendSwings = (int) Math.Ceiling( 8f*(1+hastePercent));

			Plea = new Spell("Plea (0 Atonements)", 2.25f, 1.5f, 3960, 1.0f, this);
			Smend = new Spell("Shadow Mend (Heavy Incoming Damage)", 7.5f, 1.5f, 30800, DarkestShadows, this);
			SmiteAbsorb = new Spell("Smite Absorb", 2.25f, 1.5f, 11000, 1.0f, this);
			HealSpells = new Spell[]
			{
				Plea,
				new Spell("Plea (6 Atonements)", 2.25f, 1.5f, 3960*6, 1.0f, this),
				Smend,
				
				new Spell("Power Word: Shield", 5.5f, 1.5f, 22000, ShieldOfFaith*Skjoldr, this),
				new Spell("Penitent Penance", 9f, 2.0f, 22000, Confession, this),
				new Spell("Clarity of Will", 9f, 2f, 30800, 1.0f, this),

				SmiteAbsorb,

				new Spell("Power Word: Radiance", 2.5f*3, 2.5f, 71500, BurstOfLight, this),
				new Spell("Shadow Covenant (Fully Efficient Somehow)", 4.5f*5, 2.5f, 71500, BurstOfLight, this),
				new Spell("Shadow Covenant", 4.5f*5f/2f, 1.5f, 71500, BurstOfLight, this),
				//new Spell("Power Word: Radiance (each)", 2.5f, 2.5f, 71500, BurstOfLight, this),
				new Spell("Divine Star (6+ targets)", 0.9f*6, 1.5f, 27500, 1, this),
				new Spell("Halo (6+ targets)", 2.87f*6, 1.5f, 39600, 1, this),

				new Spell("Shadow Mend (Grace)", 7.55f, 1.5f, 30800, DarkestShadows * 1.3f, this),
				new Spell("Power Word: Shield (Grace)", 5.5f, 1.5f, 22000, ShieldOfFaith*Skjoldr*1.3f, this),
				new Spell("Penitent Penance (Grace)", 9f, 2.0f, 22000, Confession*1.3f, this),

			};

			float powerWordSolaceKludge = 1.15f; //For some reason, Power Word: Solace does 15% more damage than it seems like it should
			float castigation = 4f / 3f;
			Smite = new Spell("Smite", 2.88f * 1.15f, 1.5f, 11000, allDamageBonus, this);
			CastigatedPenance = new Spell("Penance (Castigated)", 1.9f * 3f, 2.0f, 30800, allDamageBonus*Confession * castigation, this);
			RegularPenance = new Spell("Penance (Standard)", 1.9f * 3f, 2.0f, 30800, allDamageBonus * Confession, this);
			Ptw = new Spell("Purge the Wicked (total)", 4.8f*(1 + hastePercent) + 1f, 1.5f, 22000, allDamageBonus * EdgeOfDarkAndLight, this);
			Swp = new Spell("Shadow Word: Pain (total)", 3.42f * (1 + hastePercent) + 0.38f, 1.5f, 24200, allDamageBonus*EdgeOfDarkAndLight, this);
			PtwDot = new Spell("Purge the Wicked (DoT)", 4.8f * (1 + hastePercent), 1.5f, 22000, allDamageBonus * EdgeOfDarkAndLight, this);
			SwpDot = new Spell("Shadow Word: Pain (DoT)", 3.42f * (1 + hastePercent), 1.5f, 24200, allDamageBonus * EdgeOfDarkAndLight, this);

			PtwDPS = PtwDot.AvgEffect() / 20f;
			SwpDPS = SwpDot.AvgEffect() / 18f;
			LightsWrath = new Spell("Light's Wrath", 7f, 2.5f, 0, allDamageBonus, this);

			MindbenderSwing = new Spell("Mindbender (One Swing)", 1.5f, 12f / 8f / (1 + hastePercent), -5500, allDamageBonus, this);
			ShadowfiendSwing = new Spell("Shadowfiend (One Swing)", 2.0f, 12f / 8f / (1 + hastePercent), -5500, allDamageBonus, this);

			Mindbender = new Spell("Mindbender (Full Duration)", ShadowfiendSwings * 1.5f, 1.5f, -5500 * ShadowfiendSwings, allDamageBonus, this);
			Shadowfiend = new Spell("Shadowfiend (Full Duration)", ShadowfiendSwings * 2.0f, 1.5f, 0, allDamageBonus, this);

			DamageSpells = new Spell[]
			{
				Smite, CastigatedPenance, RegularPenance, Ptw, Swp, LightsWrath,
				PtwDot, SwpDot,
				MindbenderSwing, ShadowfiendSwing,
				Mindbender, Shadowfiend,
				new Spell("Power Word: Solace", powerWordSolaceKludge*3f, 1.5f, -11000, allDamageBonus, this),
				new Spell("Divine Star", 1.45f, 1.5f, 27500, allDamageBonus, this),
				new Spell("Halo", 4.31f, 1.5f, 1, allDamageBonus, this),
				//new Spell("Power Word: Solace", 3f, 1.5f, -11000, allDamageBonus, this),
				//new Spell("Power Word: Solace", 3f, 1.5f, -11000, allDamageBonus, this),
			};

			this.critpercentbox.Text = critPercent.ToString("P");
			this.hastepercentbox.Text = hastePercent.ToString("P");
			this.masterypercentbox.Text = masteryPercent.ToString("P");
			this.verpercentbox.Text = verPercent.ToString("P");

			this.artifactDamageBonusPercent.Text = allDamageBonus.ToString("P");

			this.MindbenderSwingsBox.Text = ShadowfiendSwings.ToString();

			StringBuilder sb = new StringBuilder();

			sb.Append("Spellpower: " + scaledSpellPower.ToString("#,#") + " (int * (1 + versatility) * 1.05 * 1.10)");

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

				for (int r = 0; r < DamageSpells.Length + 1; r++)
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

			for (int s = 0; s < DamageSpells.Length; s++)
			{
				for (int c = 0; c < DamageColumns.Length; c++)
				{

					TextBlock t = new TextBlock();
					switch (c)
					{
						case 0:
							t.Text = DamageSpells[s].Name;
							break;
						case 1:
							t.Text = F(DamageSpells[s].BaseEffect());
							break;
						case 2:
							t.Text = F(DamageSpells[s].CastTime());
							break;
						case 3:
							t.Text = F(DamageSpells[s].EffectPerSecond());
							break;
						case 4:
							t.Text = F(DamageSpells[s].AtonementEffect()*AvgAtonements / DamageSpells[s].CastTime());
							break;
						case 5:
							t.Text = F(DamageSpells[s].AtonementEffect() * AvgAtonements / DamageSpells[s].Mana);
							break;
						case 6:
							t.Text = F(DamageSpells[s].Mana);
							break;
						case 7:
							t.Text = F(DamageSpells[s].AvgEffect() / DamageSpells[s].Mana);
							break;
						case 8:
							t.Text = F(DamageSpells[s].ManaPerSecond());
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

		private void Rotations()
		{
			StringBuilder sb = new StringBuilder();

			float AtonementDuration = 15f;
			int cast = 0;
			float time = 0f;
			sb.AppendLine("Let Atonemenet fall off, plea to 6, hit PtW, spam Smite");

			float totalHealing = 0f;
			float totalDamage = 0f;
			float tankHealing = 0f;
			int atonements = 0;
			float manaSpent = 0f;
			bool dotCast = false;
			bool stackedAtonement = false;

			float[] atonementExpirations = new float[6];
			float dotExpires = 999f;
			for (cast=0, time=0f; time < (AtonementDuration + Plea.CastTime()*6); cast++)
			{
				if (!stackedAtonement)
				{
					atonementExpirations[atonements] = time + AtonementDuration;
					//Cast Plea
					manaSpent += Plea.Mana * (atonements + 1);
					if (atonements == 0)
					{
						tankHealing += Plea.AvgEffect();
					}
					totalHealing += Plea.AvgEffect();

					atonements++;
					time += Plea.CastTime();
					stackedAtonement = (atonements == 6);
					sb.AppendLine(F(time) + "s: " + F(totalHealing) + "," + F(totalDamage) + "," + atonements.ToString() + 
						": Casting Plea");
					continue;
				}

				if (!dotCast)
				{
					//cast Purge the Wicked
					time += Ptw.CastTime();

					dotExpires = time + 20f;

					totalDamage += Ptw.AvgEffect() - PtwDot.AvgEffect();
					tankHealing += Ptw.AtonementEffect() - PtwDot.AtonementEffect();
					totalHealing += atonements * ( Ptw.AtonementEffect() - PtwDot.AtonementEffect() );
					manaSpent += Ptw.Mana;
					sb.AppendLine(F(time) + "s: " + F(totalHealing) + "," + F(totalDamage) + "," + atonements.ToString() +
						": Casting Purge the Wicked");
					dotCast = true;
					continue;
				}

				for (int i=0; i < atonements; i++)
				{
					if (atonementExpirations[i] < time)
					{
						atonementExpirations[i] = 999f;
						atonements--;
					}
				}

				time += Smite.CastTime();

				if (dotExpires > time)
				{
					totalDamage += PtwDPS * Smite.CastTime();
					tankHealing += PtwDPS * Smite.CastTime() * 0.4f * (1 + this.masteryPercent);
					tankHealing += atonements * PtwDPS * Smite.CastTime() * 0.4f * (1 + this.masteryPercent);
				}

				totalDamage += Smite.AvgEffect();
				tankHealing += Smite.AtonementEffect() + SmiteAbsorb.AvgEffect();
				totalHealing += atonements * Smite.AtonementEffect() + SmiteAbsorb.AvgEffect();
				manaSpent += Smite.Mana;
				sb.AppendLine(F(time) + "s: " + F(totalHealing) + "," + F(totalDamage) + "," + atonements.ToString() +
					": Casting Smite");

			}

			sb.AppendLine("Period: " + F(time) + "s, " +
				"DPS: " + F(totalDamage / time) + ", " +
				"HPS: " + F(totalHealing / time) + ", " +
				"Tank HPS: " + F(tankHealing / time) + ", " +
				"MPS: " + F(manaSpent / time) + ", " +
				"HPM: " + F(totalHealing / manaSpent)
				);

			this.RotationBox.Text = sb.ToString();
		}

		private void recalc_Click(object sender, RoutedEventArgs e)
		{
			Raycalculate();
		}
	}
}

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
		int intellect;
		int critRating;
		int hasteRating;
		int masteryRating;
		int verRating;

		float critPercent;
		float hastePercent;
		float masteryPercent;
		float verPercent;

		float scaledSpellPower;

		public MainPage()
		{
			this.InitializeComponent();

		}

		private void recalc_Click(object sender, RoutedEventArgs e)
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

			scaledSpellPower = intellect * (1 + verPercent ) * 1.05f * 1.1f;

			this.critpercentbox.Text = critPercent.ToString("P");
			this.hastepercentbox.Text = hastePercent.ToString("P");
			this.masterypercentbox.Text = masteryPercent.ToString("P");
			this.verpercentbox.Text = verPercent.ToString("P");

			StringBuilder sb = new StringBuilder();

			sb.Append("Spellpower: " + scaledSpellPower.ToString("#,#") + " (int * (1 + versatility) * 1.05 * 1.10)");

			this.outbox.Text = sb.ToString();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nimaime.SPD.SPD.FormWindow
{
	/// <summary>
	/// AddZBDateADJBill.xaml 的交互逻辑
	/// </summary>
	public partial class AddZBDateADJBill : Window
	{
		private List<Material> Materials = [];
		public AddZBDateADJBill(List<Material> materials)
		{
			InitializeComponent();
			Materials = materials;
		}
	}
}

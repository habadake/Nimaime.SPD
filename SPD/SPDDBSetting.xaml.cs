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
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Nimaime.SPD.SPD
{
	/// <summary>
	/// SPDDBSetting.xaml 的交互逻辑
	/// </summary>
	public partial class SPDDBSetting : Window
	{
		private readonly ConfigService config = new();
		public SPDDBSetting()
		{
			InitializeComponent();
			config.Load();
			if (config.Current.SPDDbConfig.SPDDbAddrs == null || config.Current.SPDDbConfig.SPDDbAddrs.Count == 0)
			{
				config.Update(s =>
				{
					s.SPDDbConfig.SPDDbAddrs = [
						new SPDDbAddr("直连192", "192.168.0.118", "3306"),
					];
				});
			}
			cbDbServer.ItemsSource = config.Current.SPDDbConfig.SPDDbAddrs;
			if (config.Current.SPDDbConfig.SelectedSPDDbAddrIndex >= 0 && config.Current.SPDDbConfig.SelectedSPDDbAddrIndex < config.Current.SPDDbConfig.SPDDbAddrs.Count)
			{
				cbDbServer.SelectedIndex = config.Current.SPDDbConfig.SelectedSPDDbAddrIndex;
			}
			else
			{
				cbDbServer.SelectedIndex = 0;
			}
			if (config.Current.SPDDbConfig.UserName != null)
			{
				txtDbUser.Text = config.Current.SPDDbConfig.UserName;
			}
			if (config.Current.SPDDbConfig.Password != null)
			{
				txtDbPassword.Password = config.Current.SPDDbConfig.Password;
			}
		}

		private void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			config.Update(s =>
			{
				s.SPDDbConfig.SelectedSPDDbAddrIndex = cbDbServer.SelectedIndex;
				s.SPDDbConfig.UserName = txtDbUser.Text;
				s.SPDDbConfig.Password = txtDbPassword.Password;
			});
			Close();
		}

		private async void BtnTest_Click(object sender, RoutedEventArgs e)
		{
			btnTest.IsEnabled = false;
			config.Update(s =>
			{
				s.SPDDbConfig.SelectedSPDDbAddrIndex = cbDbServer.SelectedIndex;
				s.SPDDbConfig.UserName = txtDbUser.Text;
				s.SPDDbConfig.Password = txtDbPassword.Password;
			});
			(bool testResult, string msg) = await config.Current.SPDDbConfig.TestConnection();
			if (testResult)
			{
				MessageBox.Show("连接成功！", "测试结果", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
			{
				MessageBox.Show($"连接失败！请检查配置项是否正确。\n{msg}", "测试结果", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			btnTest.IsEnabled = true;
		}
	}
}

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

namespace Nimaime.SPD.HIS
{
	/// <summary>
	/// HISDBSetting.xaml 的交互逻辑
	/// </summary>
	public partial class HISDBSetting : Window
	{
		private readonly ConfigService config = new();
		public HISDBSetting()
		{
			InitializeComponent();
			config.Load();
			if (config.Current.HISDbConfig.HISDbAddrs == null || config.Current.HISDbConfig.HISDbAddrs.Count == 0)
			{
				config.Update(s =>
				{
					s.HISDbConfig.HISDbAddrs = [
						new HISDbAddr("直连10", "192.168.0.12:1521"),
					];
				});
			}
			cbDbServer.ItemsSource = config.Current.HISDbConfig.HISDbAddrs;
			if (config.Current.HISDbConfig.SelectedHISDbAddrIndex >= 0 && config.Current.HISDbConfig.SelectedHISDbAddrIndex < config.Current.HISDbConfig.HISDbAddrs.Count)
			{
				cbDbServer.SelectedIndex = config.Current.HISDbConfig.SelectedHISDbAddrIndex;
			}
			else
			{
				cbDbServer.SelectedIndex = 0;
			}
			if (config.Current.HISDbConfig.UserName != null)
			{
				txtDbUser.Text = config.Current.HISDbConfig.UserName;
			}
			if (config.Current.HISDbConfig.Password != null)
			{
				txtDbPassword.Password = config.Current.HISDbConfig.Password;
			}
		}

		private void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			config.Update(s =>
			{
				s.HISDbConfig.SelectedHISDbAddrIndex = cbDbServer.SelectedIndex;
				s.HISDbConfig.UserName = txtDbUser.Text;
				s.HISDbConfig.Password = txtDbPassword.Password;
			});
			Close();
		}

		private async void BtnTest_Click(object sender, RoutedEventArgs e)
		{
			btnTest.IsEnabled = false;
			config.Update(s =>
			{
				s.HISDbConfig.SelectedHISDbAddrIndex = cbDbServer.SelectedIndex;
				s.HISDbConfig.UserName = txtDbUser.Text;
				s.HISDbConfig.Password = txtDbPassword.Password;
			});
			(bool testResult, string msg) = await config.Current.HISDbConfig.TestConnection();
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

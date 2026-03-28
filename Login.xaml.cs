using Nimaime.SPD.SPD;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nimaime.SPD
{
	/// <summary>
	/// Login.xaml 的交互逻辑
	/// </summary>
	public partial class Login : Window
	{
		private readonly ConfigService config = new();

		public Login()
		{
			InitializeComponent();
			config.Load();
			if (config.Current.SPDWebAddrs == null || config.Current.SPDWebAddrs.Count == 0)
			{
				config.Update(s =>
				{
					s.SPDWebAddrs = [
						new SPDWebAddr("华新", "http://192.168.31.1:58082/"),
						new SPDWebAddr("本院", "http://192.168.31.1:58081/"),
						new SPDWebAddr("直连10", "http://10.10.1.143:8081/"),
						new SPDWebAddr("直连192", "http://192.168.0.119:8081/"),
					];
				});
			}
			cbSPDURL.ItemsSource = config.Current.SPDWebAddrs;
			if (config.Current.SelectedSPDWebAddrIndex >= 0 && config.Current.SelectedSPDWebAddrIndex < config.Current.SPDWebAddrs.Count)
			{
				cbSPDURL.SelectedIndex = config.Current.SelectedSPDWebAddrIndex;
			}
			else
			{
				cbSPDURL.SelectedIndex = 0;
			}
			if (!string.IsNullOrEmpty(config.Current.Account))
			{
				txtAccount.Text = config.Current.Account;
			}
			if (!string.IsNullOrEmpty(config.Current.Password))
			{
				txtPassword.Password = config.Current.Password;
			}
			chkRememberPassword.IsChecked = config.Current.RememberPassword;
		}

		public LoginResult? Result { get; private set; }

		/// <summary>
		/// 登录按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BtnLogin_Click(object sender, RoutedEventArgs e)
		{
			btnLogin.IsEnabled = false;
			LoginService service = new(Convert.ToString(cbSPDURL.SelectedValue) ?? config.Current.SPDWebAddrs[0].Url);
			if (string.IsNullOrEmpty(txtAccount.Text) || string.IsNullOrEmpty(txtPassword.Password))
			{
				MessageBox.Show("请输入账号和密码");
				btnLogin.IsEnabled = true;
				return;
			}
			Result = await service.LoginAsync(txtAccount.Text, txtPassword.Password);
			btnLogin.IsEnabled = true;
			if (Result == null)
			{
				return;
			}
			config.Update(s =>
			{
				s.LastLoginTime = DateTime.Now;
				s.XUS = Result.XUS;
				s.XAuth = Result.XAuthority;
				s.LoginName = Result.UserName;
				s.SelectedSPDWebAddrIndex = cbSPDURL.SelectedIndex;
				s.RememberPassword = chkRememberPassword.IsChecked == true;
			});
			if (chkRememberPassword.IsChecked == true)
			{
				// 记住账号和密码（不安全，仅供测试使用）
				config.Update(s =>
				{
					s.Account = txtAccount.Text;
					s.Password = txtPassword.Password;
				});
			}
			else
			{
				config.Update(s =>
				{
					s.Account = "";
					s.Password = "";
				});
			}
			Close();
		}
	}
}

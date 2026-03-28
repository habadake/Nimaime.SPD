using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace Nimaime.SPD.SPD
{
	public class SPDHTTP
	{
		readonly ConfigService config = new();
		readonly HttpClient httpClient = new();
		public SPDHTTP()
		{
			config.Load();
			httpClient.DefaultRequestHeaders.Add("X-US", config.Current.XUS);
			httpClient.DefaultRequestHeaders.Add("X-AUTHORITY", config.Current.XAuth);
			httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
			httpClient.DefaultRequestHeaders.Add("X-APP-CODE", "gyqx.spdherp");
			if (string.IsNullOrEmpty(config.Current.SelectedSPDWebAddr.Url))
			{
				MessageBox.Show("请先在设置中选择SPD接口地址", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			httpClient.BaseAddress = new Uri(config.Current.SelectedSPDWebAddr.Url);
			httpClient.Timeout = TimeSpan.FromSeconds(300);
		}

		/// <summary>
		/// 向 SPD 服务器发起GET请求
		/// </summary>
		/// <param name="path">API 路径</param>
		/// <param name="showError">是否弹窗报错</param>
		/// <returns>响应结果</returns>
		public async Task<string> GetSPDWebAddr(string path, bool showError = false)
		{
			try
			{
				using HttpResponseMessage response = await httpClient.GetAsync(path);
				response.EnsureSuccessStatusCode();
				string strResponse = await response.Content.ReadAsStringAsync();
				return strResponse;
			}
			catch (Exception ex)
			{
				if (showError) MessageBox.Show($"请求SPD接口失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return "";
			}
		}

		/// <summary>
		/// 向 SPD 服务器发起POST请求，获取表单数据
		/// </summary>
		/// <param name="path">API 路径</param>
		/// <param name="jsonContent">JSON 内容</param>
		/// <param name="showError">是否弹窗报错</param>
		/// <returns>响应结果</returns>
		public async Task<string> PostSPDWebAddr(string path, string jsonContent, bool showError = false)
		{
			try
			{
				using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");
				using HttpResponseMessage response = await httpClient.PostAsync(path, content);
				response.EnsureSuccessStatusCode();
				string strResponse = await response.Content.ReadAsStringAsync();
				return strResponse;
			}
			catch (Exception ex)
			{
				if (showError) MessageBox.Show($"请求SPD接口失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return "";
			}
		}

		/// <summary>
		/// 向 SPD 服务器发起POST请求，获取字节流（用于下载文件）
		/// </summary>
		/// <param name="path">API 路径</param>
		/// <param name="jsonContent">JSON 内容</param>
		/// <param name="showError">是否弹窗报错</param>
		/// <returns>文件字节内容</returns>
		public async Task<byte[]> PostSPDWebAddrDL(string path, string jsonContent, bool showError = false)
		{
			try
			{
				using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");
				using HttpResponseMessage response = await httpClient.PostAsync(path, content);
				response.EnsureSuccessStatusCode();
				byte[] fileData = await response.Content.ReadAsByteArrayAsync();
				return fileData;
			}
			catch (Exception ex)
			{
				if (showError) MessageBox.Show($"请求SPD接口失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return [];
			}
		}
	}
}

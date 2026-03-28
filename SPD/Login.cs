using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Nimaime.SPD.SPD
{
	public class LoginResult
	{
		public string XUS { get; set; }
		public string XAuthority { get; set; }
		public string UserName { get; set; }
	}

	public class LoginService
	{
		private readonly string baseUrl;

		public LoginService(string baseUrl)
		{
			this.baseUrl = baseUrl.TrimEnd('/');
		}

		/// <summary>
		/// 主登录方法
		/// </summary>
		public async Task<LoginResult?> LoginAsync(string username, string password)
		{
			try
			{
				using var client = new HttpClient();

				// Step 1: 获取 salt 和 randomCode
				var checkUrl = $"{baseUrl}/platformService/sys/login/checkUserLoginCode";

				var checkBody = new
				{
					userLoginCode = username
				};

				var checkResp = await client.PostAsync(
					checkUrl,
					new StringContent(JsonSerializer.Serialize(checkBody), Encoding.UTF8, "application/json")
				);

				checkResp.EnsureSuccessStatusCode();

				var checkJson = await checkResp.Content.ReadAsStringAsync();
				using var checkDoc = JsonDocument.Parse(checkJson);

				var data = checkDoc.RootElement.GetProperty("data");

				string salt = data.GetProperty("salt").GetString();
				string randomCode = data.GetProperty("randomCode").GetString();

				if (string.IsNullOrEmpty(salt))
					MessageBox.Show("获取登录参数失败，请检查用户名是否正确");

				// Step 2: 计算 hash
				string hash = GetLoginHash(password, salt ?? "");

				// Step 3: 登录
				var loginUrl = $"{baseUrl}/platformService/sys/login/login";

				var loginBody = new
				{
					userLoginCode = username,
					hash = hash,
					rcode = randomCode
				};

				var request = new HttpRequestMessage(HttpMethod.Post, loginUrl)
				{
					Content = new StringContent(
						JsonSerializer.Serialize(loginBody),
						Encoding.UTF8,
						"application/json"
					)
				};

				request.Headers.Add("X-JS-LICENSE", "login");
				request.Headers.Add("project-code", "herp");

				var loginResp = await client.SendAsync(request);
				loginResp.EnsureSuccessStatusCode();

				var loginJson = await loginResp.Content.ReadAsStringAsync();
				using var loginDoc = JsonDocument.Parse(loginJson);

				if (loginDoc.RootElement.GetProperty("code").GetInt32() < 0)
				{
					string msg = loginDoc.RootElement.GetProperty("msg").GetString();
					MessageBox.Show(msg);
					return null;
				}

				// Step 4: 生成 X-US 和 X-AUTHORITY
				string xus = loginDoc.RootElement.GetProperty("tag").GetString();
				string xAuthority = GetXAuthority("");
				string loginUser = loginDoc.RootElement.GetProperty("data").GetProperty("ename").ToString();

				return new LoginResult
				{
					UserName = loginUser,
					XUS = xus ?? "",
					XAuthority = xAuthority
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return null;
			}
		}

		/// <summary>
		/// SHA256(password + salt)
		/// </summary>
		private string GetLoginHash(string password, string salt)
		{
			using var sha256 = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(password + salt);
			var hashBytes = sha256.ComputeHash(bytes);

			var sb = new StringBuilder();
			foreach (var b in hashBytes)
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}

		/// <summary>
		/// 生成 X-AUTHORITY
		/// </summary>
		private static string GetXAuthority(string saveMyMenu = "")
		{
			long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			string value = $"{saveMyMenu},{timestamp}";
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
		}
	}
}

using Nimaime.SPD.Common;
using Nimaime.SPD.SPD;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Nimaime.SPD
{
	/// <summary>
	/// 基础设置项 JSON解析类
	/// </summary>
	public class Setting
	{
		/// <summary>
		/// 当前选中的服务器对象
		/// </summary>
		public SPDWebAddr SelectedSPDWebAddr
		{
			get
			{
				if (SelectedSPDWebAddrIndex < SPDWebAddrs.Count && SelectedSPDWebAddrIndex >= 0)
					return SPDWebAddrs[SelectedSPDWebAddrIndex];
				return new("", "");
			}
		}
		/// <summary>
		/// 当前选中的服务器WEB URL
		/// </summary>
		public string BaseUrl { get { return SelectedSPDWebAddr.Url; } }
		/// <summary>
		/// 账号
		/// </summary>
		public required string Account { get; set; }
		/// <summary>
		/// 密码
		/// </summary>
		public required string Password { get; set; }
		/// <summary>
		/// 记住密码
		/// </summary>
		public bool RememberPassword { get; set; }
		/// <summary>
		/// X-US 头
		/// </summary>
		public required string XUS { get; set; }
		/// <summary>
		/// X-AUTH 头
		/// </summary>
		public required string XAuth { get; set; }
		/// <summary>
		/// 当前登录的用户名
		/// </summary>
		public required string LoginName { get; set; }
		/// <summary>
		/// 上次登录时间（用于自动过期）
		/// </summary>
		public DateTime LastLoginTime { get; set; }
		/// <summary>
		/// 可选的服务器列表（名称和URL）
		/// </summary>
		public List<SPDWebAddr> SPDWebAddrs { get; set; } = [];
		private int _selectedSPDWebAddrIndex = 0;
		/// <summary>
		/// 选中的SPD服务器序号（对应SPDWebAddrs列表）【JSON序列化问题 需放置在列表后】
		/// </summary>
		public int SelectedSPDWebAddrIndex
		{
			get
			{
				return _selectedSPDWebAddrIndex;
			}
			set
			{
				if (value < 0 || value >= SPDWebAddrs.Count)
				{
					_selectedSPDWebAddrIndex = 0;
					return;
				}
				_selectedSPDWebAddrIndex = value;
			}
		}
		/// <summary>
		/// HIS数据库配置（包含用户名、密码和可选的数据库地址列表）
		/// </summary>
		public HISDbConfig HISDbConfig { get; set; } = new();

		public Setting()
		{
			Account = "";
			Password = "";
			XUS = "";
			XAuth = "";
			LoginName = "";
			LastLoginTime = DateTime.MinValue;
		}
	}

	/// <summary>
	/// SPD服务器类
	/// </summary>
	/// <param name="name"></param>
	/// <param name="url"></param>
	public class SPDWebAddr(string name, string url)
	{
		/// <summary>
		/// 服务器名称
		/// </summary>
		public string Name { get; set; } = name;
		/// <summary>
		/// WEB URL（必须以/结尾）
		/// </summary>
		public string Url { get; set; } = url;
		public override string ToString()
		{
			return $"{Name} ({Url})";
		}
	}

	/// <summary>
	/// HIS数据库地址
	/// </summary>
	/// <param name="name"></param>
	/// <param name="ip"></param>
	/// <param name="port"></param>
	public class HISDbAddr(string name, string ip, string port = "1521")
	{
		/// <summary>
		/// 服务器名称
		/// </summary>
		public string Name { get; set; } = name;
		/// <summary>
		/// WEB URL（必须以/结尾）
		/// </summary>
		public string IP { get; set; } = ip;
		public string Port { get; set; } = port;
		public override string ToString()
		{
			return $"[{Name}] ({IP}:{Port})";
		}
	}

	/// <summary>
	/// HIS数据库配置类（包含用户名、密码和可选的数据库地址列表）
	/// </summary>
	public class HISDbConfig
	{
		/// <summary>
		/// HIS数据库用户名
		/// </summary>
		public string UserName { get; set; } = "";
		/// <summary>
		/// HIS数据库密码
		/// </summary>
		public string Password { get; set; } = "";
		/// <summary>
		/// 当前选定的HIS数据库地址对象
		/// </summary>
		public HISDbAddr SelectedHISDbAddr
		{
			get
			{
				if (SelectedHISDbAddrIndex < HISDbAddrs.Count && SelectedHISDbAddrIndex >= 0)
				{
					return HISDbAddrs[SelectedHISDbAddrIndex];
				}
				return new HISDbAddr("", "", "");
			}
		}
		/// <summary>
		/// 可用HIS数据库地址列表（名称和URL）
		/// </summary>
		public List<HISDbAddr> HISDbAddrs { get; set; } = [];
		private int _selectedHISDbAddrIndex = 0;
		/// <summary>
		/// 选中的HIS数据库地址序号（对应SPDWebAddrs列表）
		/// </summary>
		public int SelectedHISDbAddrIndex
		{
			get
			{
				return _selectedHISDbAddrIndex;
			}
			set
			{
				if (value < 0 || value >= HISDbAddrs.Count)
				{
					_selectedHISDbAddrIndex = 0;
					return;
				}
				_selectedHISDbAddrIndex = value;
			}
		}
		/// <summary>
		/// 测试HIS数据库连接（使用当前选定的地址、用户名和密码）
		/// 【注意：此方法会阻塞调用线程，建议在异步上下文中调用】
		/// </summary>
		/// <returns></returns>
		public async Task<(bool, string)> TestConnection()
		{
			try
			{
				return await OracleHelper.TestConnectionAsync(this);
			}
			catch (Exception ex)
			{
				return (false, ex.ToString()); // 连接失败
			}
		}
	}

	public class ConfigService(string filePath = "config.json")
	{
		private readonly string _filePath = filePath;
		private readonly object _lock = new();

		public Setting Current { get; private set; } = new()
		{
			Account = "",
			Password = "",
			XUS = "",
			XAuth = "",
			LoginName = "",
		};

		/// <summary>
		/// 加载配置（不存在则创建默认）
		/// </summary>
		public void Load()
		{
			lock (_lock)
			{
				if (!File.Exists(_filePath))
				{
					Current = new Setting()
					{
						Account = "",
						Password = "",
						XUS = "",
						XAuth = "",
						LoginName = "",
						LastLoginTime = DateTime.MinValue,
					};
					Save(); // 自动生成默认配置文件
					return;
				}

				var json = File.ReadAllText(_filePath);

				Current = JsonSerializer.Deserialize<Setting>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}) ?? new Setting()
				{
					Account = "",
					Password = "",
					XUS = "",
					XAuth = "",
					LoginName = "",
					LastLoginTime = DateTime.MinValue,
				}; ;
			}
		}

		/// <summary>
		/// 保存配置
		/// </summary>
		public void Save()
		{
			lock (_lock)
			{
				var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions
				{
					WriteIndented = true
				});

				File.WriteAllText(_filePath, json);
			}
		}

		/// <summary>
		/// 更新配置（同时保存文件）
		/// </summary>
		public void Update(Action<Setting> updateAction)
		{
			lock (_lock)
			{
				updateAction(Current);
				Save();
			}
		}
	}
}

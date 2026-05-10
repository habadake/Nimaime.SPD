using MathNet.Numerics.Statistics;
using MySqlConnector;
using System;
using System.Data;
using System.Windows;

namespace Nimaime.SPD.Common
{
	public class MySQLHelper
	{
		readonly ConfigService config = new();
		public MySQLHelper()
		{
			config.Load();
		}
		public static async Task<(bool Success, string Message)> TestConnectionAsync(SPDDbConfig config)
		{
			try
			{
				MySqlConnectionStringBuilder builder = new()
				{
					Server = config.SelectedSPDDbAddr.IP,
					Port = uint.TryParse(config.SelectedSPDDbAddr.Port, out uint port) ? port : 3306,
					UserID = config.UserName,
					Password = config.Password,
					ConnectionTimeout = 30,
					Pooling = false,
				};

				await using MySqlConnection conn = new(builder.ConnectionString);
				await conn.OpenAsync();

				await using MySqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT 1";
				await cmd.ExecuteScalarAsync();
				return (true, "连接成功");
			}
			catch (Exception ex)
			{
				return (false, ex.Message);
			}
		}

		/// <summary>
		/// 根据SQL语句查询数据库并返回DataTable
		/// </summary>
		/// <param name="query">要执行的SQL查询语句</param>
		/// <returns>返回查询结果的DataTable，如果查询失败则返回null</returns>
		public async Task<DataTable?> GetDataTableAsync(string query)
		{
			try
			{
				MySqlConnectionStringBuilder builder = new()
				{
					Server = config.Current.SPDDbConfig.SelectedSPDDbAddr.IP,
					Port = uint.TryParse(config.Current.SPDDbConfig.SelectedSPDDbAddr.Port, out uint port) ? port : 3306,
					UserID = config.Current.SPDDbConfig.UserName,
					Password = config.Current.SPDDbConfig.Password,
					ConnectionTimeout = 30,
					Pooling = false,
					Database = "spd",
				};
				await using MySqlConnection conn = new(builder.ConnectionString);
				conn.Open();
				await using MySqlCommand cmd = new(query, conn);
				using var reader = cmd.ExecuteReader();
				DataTable dt = new();
				dt.Load(reader);
				return dt;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"查询数据库失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}
	}
}

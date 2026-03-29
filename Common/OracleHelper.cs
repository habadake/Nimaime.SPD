using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nimaime.SPD.Common
{
	public class OracleHelper
	{
		/// <summary>
		/// 测试Oracle连接
		/// </summary>
		public static async Task<(bool Success, string Message)> TestConnectionAsync(
			HISDbConfig hisDB,
			string serviceName = "orcl")
		{
			try
			{
				string connStr = $"User Id={hisDB.UserName};Password={hisDB.Password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={hisDB.SelectedHISDbAddr.IP})(PORT={hisDB.SelectedHISDbAddr.Port}))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={serviceName})));";
				using OracleConnection conn = new(connStr);
				conn.Open();

				// 可选：执行一个简单查询验证
				using OracleCommand cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT sys_context('USERENV','SERVICE_NAME') FROM dual";
				await cmd.ExecuteScalarAsync();

				return (true, "连接成功");
			}
			catch (Exception ex)
			{
				return (false, ex.Message);
			}
		}
	}
}

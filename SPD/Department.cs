using Nimaime.SPD.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Nimaime.SPD.SPD
{
	public class Department
	{
		/// <summary>
		/// 科室ID
		/// </summary>
		public string ID { get; set; }
		/// <summary>
		/// 科室名称
		/// </summary>
		public string EName { get; set; }

		public override string ToString()
		{
			return $"[{ID}]{EName}";
		}
	}

	public class  DepartmentDetail
	{
		/// <summary>
		/// 科室ID
		/// </summary>
		public string ID { get; set; }
		/// <summary>
		/// 科室名称
		/// </summary>
		public string EName { get; set; }
		/// <summary>
		/// 拼音首字母
		/// </summary>
		public string ShortPinyin { get; set; }
		/// <summary>
		/// 科室地址
		/// </summary>
		public string Address { get; set; }
		/// <summary>
		/// ERP编码
		/// </summary>
		public string ErpCode { get; set; }
	}

	/// <summary>
	/// 科室相关方法
	/// </summary>
	public static class DepartmentMethods
	{
		public class ApiResponse<T>
		{
			public int code { get; set; }
			public string msg { get; set; }
			public T data { get; set; }
			public string tag { get; set; }
			public string validateErrors { get; set; }
		}

		/// <summary>
		/// 获取所有科室
		/// </summary>
		/// <returns></returns>
		public async static Task<List<Department>> GetAllDepartments()
		{
			SPDHTTP client = new();
			string strResponse = await client.PostSPDWebAddr("/spdHERPService/SysOrgConfig/getSysOrgList", "{}");
			if (string.IsNullOrEmpty(strResponse))
			{
				return [];
			}
			// 反序列化为带壳结构
			ApiResponse<List<Department>>? result = JsonSerializer.Deserialize<ApiResponse<List<Department>>>(strResponse, JSOptionConverterMaker.Option);
			return result?.data ?? [];
		}

		public async static Task<DepartmentDetail?> GetDepartmentDetail(string deptID)
		{
			SPDHTTP client = new();
			string strResponse = await client.PostSPDWebAddr("/platformService/sys/org/getTheOrg", $"{{\"id\":\"{deptID}\"}}");
			if (string.IsNullOrEmpty(strResponse))
			{
				return null;
			}
			// 反序列化为带壳结构
			ApiResponse<DepartmentDetail>? result = JsonSerializer.Deserialize<ApiResponse<DepartmentDetail>>(strResponse, JSOptionConverterMaker.Option);
			return result?.data;
		}
	}
}

using Nimaime.SPD.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.Json;

namespace Nimaime.SPD.SPD
{
	/// <summary>
	/// 供应商数据类
	/// </summary>
	public class Provider
	{
		public string id { get; set; }
		public string cname { get; set; }
		public string provId { get; set; }
		public string userId { get; set; }
		public int version { get; set; }
		public string hosId { get; set; }
		public DateTime lastUpdateDateTime { get; set; }

		public override string ToString()
		{
			return cname;
		}
	}

	public class ApiResponse<T>
	{
		public int code { get; set; }
		public string msg { get; set; }
		public T data { get; set; }
		public string tag { get; set; }
		public string validateErrors { get; set; }
	}

	/// <summary>
	/// 供应商相关接口
	/// </summary>
	public static class ProviderMethods
	{
		public static async Task<List<Provider>> GetProviders()
		{
			SPDHTTP spdHTTP = new();
			string strResponse = await spdHTTP.PostSPDWebAddr("spdHERPService/myInfo/provHosInfo/getHosCollectorList", "{\"hosId\": \"h00a2\"}");
			if (string.IsNullOrEmpty(strResponse))
			{
				return [];
			}
			// 反序列化为带壳结构
			ApiResponse<List<Provider>>? result = JsonSerializer.Deserialize<ApiResponse<List<Provider>>>(strResponse, JSOptionConverterMaker.Option);

			// 判空 + 返回
			return result?.data ?? [];
		}
	}
}

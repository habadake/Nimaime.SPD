using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Nimaime.SPD.SPD
{
	public class Imm
	{
	}

	public static class ImmMethods
	{
		/// <summary>
		/// 根据入库单号获取退货科室
		/// </summary>
		/// <param name="strRK">入库单号</param>
		/// <returns>退货来源科室ID 科室名称</returns>
		public static async Task<(string, string)> GetDepByRK(string strRK)
		{
			SPDHTTP spdHTTP = new();
			string json = $@"{{""orderBy"":"""",""page"":1,""rows"":1,""queryObject"":{{""id"":""{strRK}"",""inStockKind"":""40"",""inOrgId"":""h00a2"",""branchId"":""h00a2-25""}}}}";
			string strResponse = await spdHTTP.PostSPDWebAddr("lmmService/lmmInStock/listByPage", json);
			using JsonDocument doc = JsonDocument.Parse(strResponse);
			JsonElement first = doc.RootElement.GetProperty("data").GetProperty("data")[0];

			string outDeptId = first.GetProperty("outDeptId").GetString() ?? "";
			string outDeptName = first.GetProperty("outDeptName").GetString() ?? "";
			return (outDeptId, outDeptName);
		}
	}
}

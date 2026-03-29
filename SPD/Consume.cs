using Nimaime.SPD.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Nimaime.SPD.SPD
{
	/// <summary>
	/// 消耗查询相关方法
	/// </summary>
	public static class ConsumeMethods
	{
		/// <summary>
		/// 高值耗材唯一码
		/// </summary>
		public static class EPC
		{
			public static async Task<EPCTrackData?> TrackEPC(string epc)
			{
				if (!epc.ToUpper().StartsWith("E") || epc.Length != 16)
				{
					return null;
				}
				SPDHTTP client = new();
				string result = await client.GetSPDWebAddr($"/spdHERPService/stockPile/epcTracability?epc={epc}");
				if (string.IsNullOrWhiteSpace(result))
				{
					return null;
				}

				try
				{
					EPCTrackResult obj = JsonSerializer.Deserialize<EPCTrackResult>(result, JSOptionConverterMaker.Option);

					// 判断接口是否成功
					if (obj?.Code == 0)
					{
						return obj?.Data;
					}
					return null;
				}
				catch (Exception)
				{
					return null;
				}
			}

			public class EPCTrackResult
			{
				public int Code { get; set; }
				public string Msg { get; set; }
				public string Tag { get; set; }
				public object ValidateErrors { get; set; }
				public EPCTrackData Data { get; set; }
			}

			public class EPCTrackData
			{
				public string GoodsId { get; set; }
				public string GoodsName { get; set; }
				public string GoodsGg { get; set; }
				public string GoodsCode { get; set; }
				public string BatchCode { get; set; }
				public string InSettlement { get; set; }
				public string BatchId { get; set; }
				public string ProvId { get; set; }
				public string ProvName { get; set; }
				public string MfrsId { get; set; }
				public string MfrsName { get; set; }
				public string Made { get; set; }
				public string IcdCode { get; set; }
				public string IcdCode20 { get; set; }
				public string MiCode { get; set; }
				public object SourceData { get; set; }
				public string UseStatus { get; set; }
				public string PatientId { get; set; }
				public string PatientName { get; set; }
				public string PatientInHosId { get; set; }
				public DateTime? UseDate { get; set; }
				public string ExecDeptId { get; set; }
				public string ExecDeptName { get; set; }
				public string ApplyDeptName { get; set; }
				public string Gs1Code { get; set; }
				public string HisDeptName { get; set; }
				public string CurrentStocId { get; set; }
				public string CurrentStocName { get; set; }
				public string StocId { get; set; }
				public string StocName { get; set; }
				public string UniqueCode { get; set; }
				public string BarCodeMng { get; set; }
				public string JfDeptName { get; set; }
				public string KdDeptName { get; set; }
				public string KcDeptName { get; set; }
				public DateTime? ExpdtEndDate { get; set; }
				public DateTime? ProductDate { get; set; }
				public string CertificateCode { get; set; }

				public List<TraceabilityNode> TraceablityNodes { get; set; }
			}

			public class TraceabilityNode
			{
				public string Seq { get; set; }
				public string BillId { get; set; }
				public string InType { get; set; }
				public DateTime? FillDate { get; set; }
				public string RowNum { get; set; }
				public string Filler { get; set; }
				public string FillerName { get; set; }
				public string InOrgName { get; set; }
				public string InDeptName { get; set; }
				public string OutDeptName { get; set; }
				public string OutOrgName { get; set; }
				public string GoodsName { get; set; }
				public string InvoiceCode { get; set; }
				public string InvoiceNumber { get; set; }
				public DateTime? InvoiceDate { get; set; }
				public string Auditor { get; set; }
				public string PreAuditor { get; set; }
				public string DealAuditor { get; set; }
				public string PurMode { get; set; }
				public string ProvName { get; set; }
				public string SubProvName { get; set; }
				public string Remark { get; set; }
			}
		}
	}
}

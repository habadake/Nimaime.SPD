using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Nimaime.SPD.SPD
{
	public class Material
	{

	}

	/// <summary>
	/// 耗材基础数据接口
	/// </summary>
	public static class MaterialMethods
	{
		/// <summary>
		/// 导出SPD耗材目录
		/// </summary>
		/// <param name="filePath">文件保存路径</param>
		/// <param name="isEnabled">导出耗材是否激活 "1"=激活 "0"=停用 ""=全部</param>
		/// <returns></returns>
		public static async Task<bool> ExportSPDMaterial(string filePath, string isEnabled = "1")
		{
			SPDHTTP spdHTTP = new();
			ExportSPDMaterialParameter parameter = new()
			{
				flag = isEnabled,
			};
			string strParamJson = JsonSerializer.Serialize(parameter);
			byte[] fileContent = await spdHTTP.PostSPDWebAddrDL("/spdHERPService/myGoods/hosGoods/export2", strParamJson);
			if (fileContent.Length == 0)
			{
				return false;
			}
			try
			{
				File.WriteAllBytes(filePath, fileContent);
				return true;
			}
			catch (Exception ex)
			{
                MessageBox.Show($"报表下载失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
			}
		}

		/// <summary>
		/// 序列化请求JSON参数类
		/// </summary>
		private class ExportSPDMaterialParameter
		{
			public string hosId { get; set; } = "h00a2";
			public string provProvId { get; set; }
			public string goodsName { get; set; }
			public string goodsGg { get; set; }
			public string lbsx { get; set; }
			public string provId { get; set; }
			/// <summary>
			/// 是否启用 0=停用 1=启用 空=全部
			/// </summary>
			public string flag { get; set; } = "1";
			public string lbsxs { get; set; }
			public string mfrsName { get; set; }
			public string charging { get; set; }
			public string isIntoCost { get; set; }
			public string regCode { get; set; }
			public string temperRequire { get; set; }
			public string kind18Consumer { get; set; }
			public string kindMaterial { get; set; }
			public string sdPurchase { get; set; }
			public string purMode { get; set; }
			public string canPurchase { get; set; }
			public string isHasStock { get; set; }
			public string priceOrder { get; set; }
			public string qtyOrder { get; set; }
			public string dateOrder { get; set; }
			public string fieldCode4 { get; set; }
			public string subPurMode { get; set; }
			public string iscoverPackage { get; set; }
			public string kind18Content { get; set; }
			public string disposable { get; set; }
			public string corollaryEquipment { get; set; }
			public string managerKind { get; set; }
			public string kind18 { get; set; }
			public string purJoint { get; set; }
			public string quantityType { get; set; }
			public string purKind { get; set; }
			public string tempPurchaseProv { get; set; }
			public string hitCode { get; set; }
			public string hasPurCentralId { get; set; }
			public string isSupervisionKind { get; set; }
			public string ygptCode { get; set; }
			public string ygptPrimaryCode { get; set; }
			public string isOnline { get; set; }
			public string isDistrRel { get; set; }
			public string onlineKind { get; set; }
			public string kind22 { get; set; }
			public string miCode { get; set; }
			public string medicalOnlinePay { get; set; }
			/// <summary>
			/// 是否带量 -1/空=全部 1=是 2=否 
			/// </summary>
			public string purchaseContract { get; set; } = "-1";
			public string icdCodeDate { get; set; }
			public string hisCodeState { get; set; }
			public string extInt2 { get; set; }
			public string menuUrl { get; set; } = "productInfo";
			public string buttonName { get; set; } = "导出Excel";
			public string keysupervision { get; set; }
			public string hazardousAttribute { get; set; } = "-1";
			public string existPhoto { get; set; }
			public string hospitalMonitorVarieties { get; set; }
			public string certificateCode { get; set; }
			public string isHasCenterStock { get; set; }
			public string riskLevel { get; set; }
			public string consumableLevel { get; set; }
			public string registrationLevel { get; set; }
			public string goodsProperty { get; set; }
			public string spare3 { get; set; }
			public string isGcp { get; set; }
			public string isSpecialFunds { get; set; }
			public string agentName { get; set; } = "";
			public string showHnzlContract { get; set; } = "Y";
			public string operationName { get; set; } = "产品信息导出";
			public string menuVue { get; set; } = "views/productsInfos/productInfo.vue";
		}
	}
}

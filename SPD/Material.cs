using Nimaime.SPD.Common;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using static Nimaime.Helper.File.CommonFileHelper;

namespace Nimaime.SPD.SPD
{
	public class Material
	{
		// ===== 核心字段（常用） =====
		/// <summary>
		/// SPD ID
		/// </summary>
		public string id { get; set; }
		public int SPDID
		{
			get
			{
				return Convert.ToInt32(id.Split('-')[1]);
			}
		}
		/// <summary>
		/// 品名
		/// </summary>
		public string goodsName { get; set; }
		/// <summary>
		/// 规格
		/// </summary>
		public string goodsGg { get; set; }

		public string generalName { get; set; }

		public string goodsProperty { get; set; }
		/// <summary>
		/// 单位
		/// </summary>
		public string unit { get; set; }
		/// <summary>
		/// 价格
		/// </summary>
		public decimal? price { get; set; }

		public decimal? hisPrice { get; set; }
		/// <summary>
		/// 供应商 ID
		/// </summary>
		public string provId { get; set; }
		/// <summary>
		/// 供应商名称
		/// </summary>
		public string provName { get; set; }
		/// <summary>
		/// 厂家ID
		/// </summary>
		public string mfrsId { get; set; }
		/// <summary>
		/// 厂家名称
		/// </summary>
		public string mfrsName { get; set; }

		public string brand { get; set; }

		public string made { get; set; }
		/// <summary>
		/// HIS编码
		/// </summary>
		public string erpCode { get; set; }

		public string spdGoodsCode { get; set; }

		public string hosId { get; set; }
		/// <summary>
		/// 分类信息
		/// </summary>
		public string hosKindName { get; set; }

		/// <summary>
		/// 启用状态 1=启用 0=停用
		/// </summary>
		public string flag { get; set; }
		/// <summary>
		/// 启停描述字符串
		/// </summary>
		public string flagStr { get; set; }
		/// <summary>
		/// 是否收费
		/// </summary>
		public string charging { get; set; }
		/// <summary>
		/// 是否收费
		/// </summary>
		public string isCharging
		{
			get
			{
				return charging == "1" ? "是" : "否";
			}
		}
		/// <summary>
		/// 是否可采购
		/// </summary>
		public string canPurchase { get; set; }
		public string purchaseStatus
		{
			get
			{
				string result = string.Empty;
				result += tempPurchase == "1" ? "临采" : "常采";
				result += ",";
				result += canPurchase == "1" ? "在采" : "停采";
				return result;
			}
		}
		/// <summary>
		/// 物资类型
		/// 10=低值 20=高值 60=试剂
		/// </summary>
		public string purMode { get; set; }
		public string purType
		{
			get
			{
				return purMode switch
				{
					"10" => "低值",
					"20" => "高值",
					"60" => "试剂",
					_ => "其他"
				};
			}
		}
		/// <summary>
		/// 是否带量
		/// </summary>
		public string purchaseContract { get; set; }
		/// <summary>
		/// 27 位医保编码
		/// </summary>
		public string icdCode { get; set; }

		public string managerKind { get; set; }

		public string kindMaterial { get; set; }

		public string shortPinyin { get; set; }
		/// <summary>
		/// 省平台编码
		/// </summary>
		public string hitCode { get; set; }
		/// <summary>
		/// 注册证号
		/// </summary>
		public string certificateCode { get; set; }

		public string registrationLevel { get; set; }

		public string riskLevel { get; set; }

		public string consumableLevel { get; set; }

		public string hazardousAttribute { get; set; }

		public string isIntoCost { get; set; }

		public string isGcp { get; set; }

		public string isSpecialFunds { get; set; }

		public string isOnline { get; set; }

		public string isDistrRel { get; set; }
		/// <summary>
		/// 临采标识
		/// </summary>
		public string tempPurchase { get; set; }

		public string isHasStock { get; set; }

		public string stockQty { get; set; }

		public decimal? taxRate { get; set; }

		public int? version { get; set; }

		public int? sendPackage { get; set; }
		/// <summary>
		/// 导入人
		/// </summary>
		public string ename { get; set; }
		/// <summary>
		/// 导入日期
		/// </summary>
		public DateTime? fillDate { get; set; }
		/// <summary>
		/// 上次更新日期
		/// </summary>
		public DateTime? lastUpdateDatetime { get; set; }

		// ===== 扩展字段（自动接收所有未定义字段） =====

		[JsonExtensionData]
		public Dictionary<string, JsonElement> Extra { get; set; }
	}

	/// <summary>
	/// 耗材基础数据接口
	/// </summary>
	public static class MaterialMethods
	{
		/// <summary>
		/// 获取指定条件的耗材列表
		/// </summary>
		/// <param name="para"></param>
		/// <returns></returns>
		public static async Task<List<Material>> GetSPDMaterialList(SPDMaterialParameter para)
		{
			GetMaterialRequest request = new()
			{
				queryObject = para
			};
			try
			{
				SPDHTTP spdHTTP = new();
				string json = JsonSerializer.Serialize(request);
				string response = await spdHTTP.PostSPDWebAddr(
					"spdHERPService/myGoods/hosGoods/getHosGoodsByHos",
					json
				);

				ApiResponse<PagedResult<Material>>? result = JsonSerializer.Deserialize<ApiResponse<PagedResult<Material>>>(response, JSOptionConverterMaker.Option);

				return result?.data?.data ?? [];
			}
			catch (Exception ex)
			{
				MessageBox.Show($"加载耗材失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return [];
			}
		}


		/// <summary>
		/// 导出SPD耗材目录（指定条件）
		/// 方法不抛出异常
		/// </summary>
		/// <param name="filePath">文件保存路径</param>
		/// <param name="parameter">加载API JSON参数</param>
		/// <returns>是否下载成功</returns>
		public static async Task<bool> ExportSPDMaterialWithPara(string filePath, SPDMaterialParameter para)
		{
			SPDHTTP spdHTTP = new();
			string strParamJson = JsonSerializer.Serialize(para);
			byte[] fileContent = await spdHTTP.PostSPDWebAddrDL("/spdHERPService/myGoods/hosGoods/export2", strParamJson, FileType.Excel, false);
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

		public class ApiResponse<T>
		{
			public T data { get; set; }
			public int code { get; set; }
			public string msg { get; set; }
		}
		public class PagedResult<T>
		{
			public List<T> data { get; set; }
			public int total { get; set; }
		}

		/// <summary>
		/// 加载耗材使用请求类
		/// </summary>
		public class GetMaterialRequest
		{
			public string orderBy { get; set; } = "";

			public SPDMaterialParameter queryObject { get; set; } = new();

			public int page { get; set; } = 1;
			public int rows { get; set; } = 1000000;

			public string menuUrl { get; set; } = "productInfo";
			public string operationName { get; set; } = "产品信息查询";
			public string menuVue { get; set; } = "views/productsInfos/productInfo.vue";
		}

		/// <summary>
		/// 序列化请求JSON参数类
		/// </summary>
		public class SPDMaterialParameter
		{
			public string hosId { get; set; } = "h00a2";
			public string provProvId { get; set; }
			public string goodsName { get; set; }
			public string goodsGg { get; set; }
			public string lbsx { get; set; }
			public string provId { get; set; }
			/// <summary>
			/// 是否启用
			/// 0=停用 1=启用 空=全部
			/// </summary>
			public string flag { get; set; }
			public string lbsxs { get; set; }
			public string mfrsName { get; set; }
			/// <summary>
			/// 是否收费
			/// 0=不收费 1=收费 空=全部
			/// </summary>
			public string charging { get; set; }
			public string isIntoCost { get; set; }
			public string regCode { get; set; }
			public string temperRequire { get; set; }
			public string kind18Consumer { get; set; }
			public string kindMaterial { get; set; }
			public string sdPurchase { get; set; }
			public string purMode { get; set; }
			/// <summary>
			/// 是否采购
			/// 0=停用 1=启用 空=全部
			/// </summary>
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
			/// <summary>
			/// 是否临采
			/// 0=停用 1=启用 空=全部
			/// </summary>
			public string tempPurchase { get; set; }
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

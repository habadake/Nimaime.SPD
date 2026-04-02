using Nimaime.SPD.Common;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using static Nimaime.Helper.File.CommonFileHelper;

namespace Nimaime.SPD.SPD
{
	public class Material
	{
		// ===== 核心字段（常用） =====
		public string id { get; set; }

		public int SPDID
		{
			get
			{
				return Convert.ToInt32(id.Split('-')[1]);
			}
		}

		public string goodsName { get; set; }
		public string goodsGg { get; set; }
		public string generalName { get; set; }
		public string goodsProperty { get; set; }
		public string unit { get; set; }
		public decimal? price { get; set; }
		public decimal? hisPrice { get; set; }

		public string provId { get; set; }
		public string provName { get; set; }

		public string mfrsId { get; set; }
		public string mfrsName { get; set; }

		public string brand { get; set; }
		public string made { get; set; }

		public string erpCode { get; set; }
		public string spdGoodsCode { get; set; }
		public string hosId { get; set; }

		public string hosKindName { get; set; }

		public string flag { get; set; }
		public string flagStr { get; set; }

		public string charging { get; set; }

		public string isCharging => charging == "1" ? "是" : "否";

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

		public string purchaseContract { get; set; }
		public string icdCode { get; set; }

		public string managerKind { get; set; }
		public string kindMaterial { get; set; }

		public string shortPinyin { get; set; }

		public string hitCode { get; set; }
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

		public string tempPurchase { get; set; }

		public string isHasStock { get; set; }
		public string stockQty { get; set; }

		public decimal? taxRate { get; set; }

		public int? version { get; set; }
		public int? sendPackage { get; set; }

		public string ename { get; set; }
		public DateTime? fillDate { get; set; }
		public DateTime? lastUpdateDatetime { get; set; }


		// ===== 🔽 新增字段（来自第二组JSON补充） =====

		public string code { get; set; }
		public string masterCode { get; set; }
		public string uniqueCodeStrategy { get; set; }

		public decimal? packeage { get; set; }

		public string kindCode { get; set; }
		public string kind68code { get; set; }

		public string fieldCode2 { get; set; }
		public string fieldCode3 { get; set; }
		public string fieldCode4 { get; set; }

		public decimal? hitPrice { get; set; }

		public string lbsx { get; set; }

		public string remark { get; set; }
		public string remarkChg { get; set; }

		public string uxid { get; set; }

		public string miName { get; set; }
		public string miCode { get; set; }

		public string goodsDesc { get; set; }

		public string goodsDescFile { get; set; }
		public string fileName { get; set; }

		public string salemanId { get; set; }
		public string salemanCode { get; set; }

		public string subPurMode { get; set; }

		public string midPackageUnit { get; set; }

		public string useUnit { get; set; }
		public decimal? useUnitCount { get; set; }

		public string icdName { get; set; }
		public string icd20Code { get; set; }

		public string barCodeMng { get; set; }

		public string storageConditions { get; set; }

		public string ygptCode { get; set; }
		public string ygptPrimaryCode { get; set; }

		public string onlineKind { get; set; }

		public string hitId { get; set; }

		public string isSpecialized { get; set; }

		public string miGg { get; set; }
		public string miType { get; set; }

		public string medicalOnlinePay { get; set; }

		public string extInt2 { get; set; }
		public int? extInt3 { get; set; }
		public string extInt5 { get; set; }
		public string extInt6 { get; set; }
		public int? extInt7 { get; set; }
		public string extInt8 { get; set; }

		public string ext1 { get; set; }
		public string ext2 { get; set; }
		public string ext3 { get; set; }

		public string gbm { get; set; }
		public string ext8 { get; set; }

		public DateTime? extDatetime2 { get; set; }

		public string ext12 { get; set; }
		public string ext13 { get; set; }
		public string ext14 { get; set; }

		public string deptId { get; set; }

		public int? imgCount { get; set; }

		public decimal? quantity { get; set; }

		public decimal? sPrice { get; set; }
		public decimal? ePrice { get; set; }

		public bool? hasSubmit { get; set; }

		public string regCode { get; set; }

		public string sdPurchase { get; set; }

		public string diPackage { get; set; }

		public string auditorId { get; set; }
		public string auditorName { get; set; }

		public DateTime? auditDate { get; set; }

		public DateTime? auditorHandleDate { get; set; }

		public string subProvId { get; set; }
		public string subProvName { get; set; }

		public string deviceCode { get; set; }
		public string deviceName { get; set; }

		public string goodsImageFlag { get; set; }

		public string isTrackable { get; set; }

		public bool? trackable { get; set; }
	}

	/// <summary>
	/// 耗材基础数据接口
	/// </summary>
	public static class MaterialMethods
	{
		/// <summary>
		/// 获取指定条件的耗材列表
		/// </summary>
		/// <param name="para">查询参数</param>
		/// <returns>耗材列表</returns>
		public static async Task<(List<Material>, int, int)> GetSPDMaterialList(SPDMaterialParameter para)
		{
			return await GetSPDMaterialList(para, page: 1, rows: 50);
		}

		/// <summary>
		/// 获取指定条件的耗材列表（支持分页）
		/// </summary>
		/// <param name="para">查询参数</param>
		/// <param name="page">页码（从1开始）</param>
		/// <param name="rows">每页行数</param>
		/// <returns>耗材列表、总页数、总记录数</returns>
		public static async Task<(List<Material>, int, int)> GetSPDMaterialList(SPDMaterialParameter para, int page, int rows)
		{
			GetMaterialRequest request = new()
			{
				queryObject = para,
				page = page,
				rows = rows
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
				int total = result?.data?.total ?? 0;
				int pageCount = (int)Math.Ceiling((double)total / rows);
				return (result?.data?.data ?? [], pageCount, total);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"加载耗材失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return ([], 0, 0);
			}
		}

		/// <summary>
		/// 将耗材导入到指定科室
		/// </summary>
		/// <param name="spdID">SPD耗材ID或名称</param>
		/// <param name="lstDept">科室列表</param>
		/// <returns>是否全部导入成功</returns>
		public static async Task<bool> ImportMaterial2Dept(List<Material> materials, List<Department> lstDept)
		{
			int successCount = 0;
			int failCount = 0;
			StringBuilder errorMsg = new();

			// 遍历科室列表，逐个导入
			foreach (Department dept in lstDept)
			{
				try
				{
					// 构建请求对象
					var importRequest = new
					{
						hosId = "h00a2",
						deptId = dept.ID,
						hosName = "",
						deptName = dept.EName,
						hosGoodsInfos = materials,
						targetDeptId4imp = "h00a2org-11899",
						specialized = "0"
					};

					// 序列化请求
					string jsonRequest = JsonSerializer.Serialize(importRequest, JSOptionConverterMaker.Option);

					// 发送POST请求
					SPDHTTP spdHTTP = new();
					string response = await spdHTTP.PostSPDWebAddr(
						"/spdHERPService/deptMgr/deptGoodsInfo/deptGoodsInfoImport",
						jsonRequest,
						showError: false
					);

					// 解析响应
					ApiResponse<object>? result = JsonSerializer.Deserialize<ApiResponse<object>>(response, JSOptionConverterMaker.Option);

					if (result != null && result.code == 0)
					{
						successCount++;
					}
					else
					{
						failCount++;
						errorMsg.AppendLine($"科室 [{dept.EName}] 导入失败：{result?.msg ?? "未知错误"}");
					}
				}
				catch (Exception ex)
				{
					failCount++;
					errorMsg.AppendLine($"科室 [{dept.EName}] 导入异常：{ex.Message}");
				}
			}

			// 显示结果
			if (failCount == 0)
			{
				MessageBox.Show($"成功导入到 {successCount} 个科室", 
					"成功", MessageBoxButton.OK, MessageBoxImage.Information);
				return true;
			}
			else
			{
				string message = $"导入完成：成功 {successCount} 个，失败 {failCount} 个\n\n失败详情：\n{errorMsg}";
				MessageBox.Show(message, "部分失败", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
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

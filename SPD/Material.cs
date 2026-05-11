using Nimaime.SPD.Common;
using System.Data;
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
		public static async Task<(List<Material>, int, int)> GetSPDMaterialList(SPDMaterialParameterByWebAPI para)
		{
			return await GetSPDMaterialListByWebAPI(para, page: 1, rows: 50);
		}

		/// <summary>
		/// 获取指定条件的耗材列表（支持分页）BY WEB
		/// </summary>
		/// <param name="para">查询参数</param>
		/// <param name="page">页码（从1开始）</param>
		/// <param name="rows">每页行数</param>
		/// <returns>耗材列表、总页数、总记录数</returns>
		public static async Task<(List<Material>, int, int)> GetSPDMaterialListByWebAPI(SPDMaterialParameterByWebAPI para, int page, int rows)
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
		public static async Task<bool> ExportSPDMaterialWithPara(string filePath, SPDMaterialParameterByWebAPI para)
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

			public SPDMaterialParameterByWebAPI queryObject { get; set; } = new();

			public int page { get; set; } = 1;
			public int rows { get; set; } = 1000000;

			public string menuUrl { get; set; } = "productInfo";
			public string operationName { get; set; } = "产品信息查询";
			public string menuVue { get; set; } = "views/productsInfos/productInfo.vue";
		}

		/// <summary>
		/// 序列化请求JSON参数类
		/// </summary>
		public class SPDMaterialParameterByWebAPI
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

		public static async Task<DataTable?> GetMaterialByDB(SPDMaterialParameterByDB para)
		{
			try
			{
				string sql = $@"
					select
						/* Columns filter here */
						*
					from (
						select
							cast(SUBSTRING_INDEX(hgi.id, '-', -1) as signed)           as '物资ID',
							hgi.erp_code                                               as 'HIS编码',
							hgi.hit_code                                               as '省平台编码',
							IF(hgi.flag = '1', '是', '否')                             as '启用',
							IF(contract_info.带量合同ID IS NULL, '否', '是')           as '带量',
							IF(hgi.can_purchase = '1', '是', '否')                     as '采购',
							IF(hgi.temp_purchase = '1', '是', '否')                    as '临采',
							hgi.goods_name                                             as '名称',
							hgi.goods_gg                                               as '规格',
							case hgi.pur_mode when 10 then '低值' when 20 then '高值' when 60 then '试剂'
							else '未知'                                               end as '物资类型',
							kc.分类名称                                                as '财务分类',
							0+cast(hgi.price as char)                                  as '单价',
							hgi.unit                                                   as '单位',
							IF(hgi.charging = '1', '是', '否')                         as '是否收费',
							company_info.名称                                          as '供应商',
							hgi.hos_mfrs_name                                          as '厂家',
							pgi_s.注册证号                                             as '注册证号',
							hgi.icd_code                                               as '医保编码(27位)',
							hgie.是否十八类                                            as '是否十八类',
							kind18_dict.十八类具体项                                   as '十八类具体项',
							DATE_FORMAT(hgi.ext_datetime2, '%Y-%m-%d')                 as '院内招标时间',
							DATE_FORMAT(hgi.fill_date, '%Y-%m-%d %H:%i:%S')            as '录入时间',
							DATE_FORMAT(hgi.last_update_datetime, '%Y-%m-%d %H:%i:%S') as '最后更新时间',
							his_code_chg.曾用HIS编码                                    as '曾用HIS编码',
							hgi.remark                                                 as '备注'
						from hos_goods_info hgi
						inner join (
							/* 拼供应商名称 */
							select id as '供应商ID', cname as '名称' from bas_company_info
						) company_info on hgi.prov_id = company_info.供应商ID
						/* The category code should match the code defined in hos_kindcode */
						inner join (
							/* 拼财务分类 */
							select h_kc.id as '分类ID', h_kc.kind_name as '分类名称' from hos_kindcode h_kc
						) as kc on hgi.lbsx = kc.分类ID
						left join (
							/* 拼曾用HIS编码*/
							SELECT
								HOS_GOODS_ID,
								GROUP_CONCAT(OLD_VALUE ORDER BY OLD_VALUE) AS '曾用HIS编码'
							FROM hos_goods_chg_sub hgcs
							WHERE
							hgcs.BILL_ID IN (SELECT hgcm.ID FROM hos_goods_chg_main hgcm WHERE hgcm.MAINTAIN_STATUS = 60 and hgcm.STATUS = 30)
							AND hgcs.OLD_VALUE IS NOT NULL AND hgcs.OLD_VALUE != hgcs.NEW_VALUE
							GROUP BY hgcs.HOS_GOODS_ID
						) as his_code_chg on hgi.id = his_code_chg.HOS_GOODS_ID
						left join (
							/* 拼带量 */
							select hcil.hos_goods_id as '物资ID',
								   hc.id             as '带量合同ID',
								   hc.begin_date     as '合同开始时间',
								   hc.end_date       as '合同结束时间'
							from hos_contract_item_list hcil
							inner join hos_contract hc on hcil.contract_id = hc.id
							where hc.status = '20' and now() >= hc.begin_date
							and (hc.end_date is null or now() <= hc.end_date)
						) as contract_info on hgi.hos_id = contract_info.物资ID
						/* Should be left join here because not all hosGoodId has an extra record */
						left join (
							/* 拼扩展 */
							select hgi_ext.goods_id as '物资ID',
								   IF(hgi_ext.kind18 = '1', '是', '否') as '是否十八类',
								   hgi_ext.kind18_content as '十八类编号'
							from hos_goods_info_ext hgi_ext
						) as hgie on hgi.id = hgie.物资ID
						left join (
							/* 十八类字典 */
							select sdv.dict_id as '字典ID',
								   sdv.val     as '十八类编号',
								   sdv.ename   as '十八类具体项'
							from sys_dict_value as sdv
							/* kind18 dictionary */
							where sdv.dict_id = 'kind18'
						) as kind18_dict on hgie.十八类编号 = kind18_dict.十八类编号
						/* Not every hosGood has a certificate */
						left join (
							/* 注册证 */
							select pgi.erp_code as 'ERP编码',
								   pgi.prov_id as '供应商ID',
								   pgi.certificate_code as '注册证号',
								   pgi.spd_goods_code as '平台物资编码'
							from prov_goods_info pgi
						/* A hosGoodId can match multiple records in prov_goods_info due to each provider has a unique erpCode */
						) as pgi_s on hgi.code = pgi_s.ERP编码 and hgi.prov_id = pgi_s.供应商ID and pgi_s.平台物资编码 = concat('good-', hgi.spd_goods_code)
						/*
						all where clause should be here
						*/
						{para}
					) as final_result
					/* final result filter where clause here */
					order by final_result.物资ID;
				";
				//执行SQL并获取结果
				MySQLHelper spdDBHelper = new();
				return spdDBHelper.GetDataTableAsync(sql).Result;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"查询耗材失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}

		public class SPDMaterialParameterByDB
		{
			public string keyword { get; set; } = "";
			public string provId { get; set; } = "";
			public bool? enabled { get; set; } = true;
			public bool? canPurchase { get; set; } = null;
			public bool? tempPurchase { get; set; } = null;
			public bool? contract { get; set; } = null;
			public bool? charging { get; set; } = null;
			public Enums.MaterialType type { get; set; } = Enums.MaterialType.不区分;

			public override string ToString()
			{
				string sqlWhereClause = "where 1=1";
				if (!string.IsNullOrEmpty(keyword))
				{
					sqlWhereClause += $" and (hgi.id = 'h00a2|hosGood-{provId}' or hgi.erp_code = '{keyword}' or hgi.goods_name like '%{keyword}%')\n";
				}
				if (!string.IsNullOrEmpty(provId))
				{
					sqlWhereClause += $" and hgi.prov_id = '{provId}'\n";
				}
				if (enabled.HasValue)
				{
					sqlWhereClause += $" and hgi.flag = '{(enabled.Value ? "1" : "0")}'\n";
				}
				if (canPurchase.HasValue)
				{
					sqlWhereClause += $" and hgi.can_purchase = '{(canPurchase.Value ? "1" : "0")}'\n";
				}
				if (tempPurchase.HasValue)
				{
					sqlWhereClause += $" and hgi.temp_purchase = {(tempPurchase.Value ? "1" : "0")}\n";
				}
				if (charging.HasValue)
				{
					sqlWhereClause += $" and hgi.charging = '{(charging.Value ? "1" : "0")}'\n";
				}
				return sqlWhereClause;
			}
		}
	}
}
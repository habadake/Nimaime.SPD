using Microsoft.Win32;
using Nimaime.SPD.Common;
using Nimaime.SPD.HIS;
using Nimaime.SPD.SPD;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;
using static Nimaime.SPD.SPD.ConsumeMethods.EPC;
using static Nimaime.SPD.SPD.MaterialMethods;

namespace Nimaime.SPD
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		ConfigService config = new();
		public MainWindow()
		{
			InitializeComponent();
			JSOptionConverterMaker.Option.Converters.Add(new DateTimeConverter());
			string programTitle = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "二枚目国药SPD系统帮助程序";
			//获取版本号
			Title = programTitle + " " + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
			config.Load();
			if (string.IsNullOrEmpty(config.Current.LoginName))
			{
				return;
			}
			UpdateLBL();
		}

		#region 登录行为 系统设置
		/// <summary>
		/// 测试XHeader是否有效（使用当前选定的服务器URL和XHeader进行测试请求）
		/// </summary>
		/// <returns></returns>
		static async Task<bool> TestXHeader()
		{
			string url = "/spdHERPService/SysOrgConfig/getByDeptId/h00a2org-11899";
			SPDHTTP spdHTTP = new();
			string response = await spdHTTP.GetSPDWebAddr(url, showError: false);
			return !string.IsNullOrEmpty(response);
		}

		/// <summary>
		/// 双击状态进行登录操作
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void lblLoginStatus_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			CallLogin();
		}

		/// <summary>
		/// 登录动作
		/// </summary>
		private void CallLogin()
		{
			Login login = new();
			login.ShowDialog();
			// 登录后需要重载配置
			config.Load();
			if (!string.IsNullOrEmpty(config.Current.LoginName))
			{
				UpdateLBL();
			}
		}

		/// <summary>
		/// 更新登录状态标签显示（可选择是否先检查登录状态，默认为true）
		/// </summary>
		private async void UpdateLBL(bool checkLoginStatus = true)
		{
			if (checkLoginStatus && !await TestXHeader())
			{
				//检查状态且登录失败
				config.Update(c => {
					c.LoginName = "";
					c.XUS = "";
					c.XAuth = "";
				});
				lblLoginStatus.Content = $"当前登录用户：未登录(双击此处登录)";
				lblLoginStatus.ToolTip =
					$"X-US：{config.Current.XUS}\n" +
					$"X-AUTH：{config.Current.XAuth}\n" +
					$"SERVER：{config.Current.SelectedSPDWebAddr}\n" +
					$"上次登录时间：{config.Current.LastLoginTime:yyyy-MM-dd HH:mm:ss}";
				CallLogin();
				return;
			}
			// 直接更新显示
			lblLoginStatus.Content = $"当前登录用户：{(string.IsNullOrEmpty(config.Current.LoginName) ? "未登录" : config.Current.LoginName)}(双击此处重新登录)";
			lblLoginStatus.ToolTip =
				$"X-US：{config.Current.XUS}\n" +
				$"X-AUTH：{config.Current.XAuth}\n" +
				$"SERVER：{config.Current.SelectedSPDWebAddr}\n" +
				$"上次登录时间：{config.Current.LastLoginTime:yyyy-MM-dd HH:mm:ss}";
			LoadProviderToCB();
		}

		/// <summary>
		/// 注销用户
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnLogout_Click(object sender, RoutedEventArgs e)
		{
			config.Update(s =>
			{
				s.LoginName = "";
				s.XUS = "";
				s.XAuth = "";
				s.LastLoginTime = DateTime.Now;
			});
			UpdateLBL(checkLoginStatus: false);
		}

		/// <summary>
		/// 标签页变化
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (tabMain.SelectedIndex)
			{
				case 0:
					LoadProviderToCB();
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// HIS数据库设置按钮（打开一个新的窗口进行HIS数据库连接配置，配置完成后会自动保存并更新主窗口显示的登录状态）
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnHISDBSetting_Click(object sender, RoutedEventArgs e)
		{
			HISDBSetting setting = new();
			setting.ShowDialog();
			// 登录后需要重载配置
			config.Load();
		}
		#endregion

		#region TAB 01 耗材管理

		private async void btnLoadMaterial_Click(object sender, RoutedEventArgs e)
		{
			btnLoadMaterial.IsEnabled = false;
			SPDMaterialParameter para = GenMaterialParaByUI();
			List<Material> materials = await MaterialMethods.GetSPDMaterialList(para);
			// TODO 打表
			dgMaterial.ItemsSource = materials;
			btnLoadMaterial.IsEnabled = true;
		}

		/// <summary>
		/// 导出耗材（带参数）按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void btnExportMaterialWithPara_Click(object sender, RoutedEventArgs e)
		{
			btnExportMaterialWithPara.IsEnabled = false;
			SaveFileDialog saveFileDialog = new()
			{
				Title = "保存SPD物资信息",
				Filter = "Excel 97-2003 工作簿 (*.xls)|*.xls",
				FileName = $"医院商品导出表{DateTime.Now:yyyy-MM-dd}.xls"
			};
			if (saveFileDialog.ShowDialog() != true)
			{
				btnExportMaterialWithPara.IsEnabled = true;
				return;
			}
			SPDMaterialParameter exPara = GenMaterialParaByUI();
			// 下载报表文件
			bool result = await MaterialMethods.ExportSPDMaterialWithPara(saveFileDialog.FileName, exPara);
			if (result)
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = saveFileDialog.FileName,
					UseShellExecute = true
				});
			}
			btnExportMaterialWithPara.IsEnabled = true;
		}

		private SPDMaterialParameter GenMaterialParaByUI()
		{
			SPDMaterialParameter para = new()
			{
				goodsName = !string.IsNullOrWhiteSpace(txtKeyword.Text) ? txtKeyword.Text.Trim() : "",
				provId = cbProviderInMaterial.SelectedValue is Provider selectedProvider ? selectedProvider.provId : "",
				mfrsName = !string.IsNullOrWhiteSpace(txtManufacturer.Text) ? txtManufacturer.Text.Trim() : "",
				flag = chkIsMaterialEnabled.IsChecked == true ? "1" : chkIsMaterialEnabled.IsChecked == false ? "0" : "",
				canPurchase = chkIsMaterialProcurement.IsChecked == true ? "1" : chkIsMaterialProcurement.IsChecked == false ? "0" : "",
				tempPurchase = chkIsMaterialTemporaryProcurement.IsChecked == true ? "1" : chkIsMaterialTemporaryProcurement.IsChecked == false ? "0" : "",
				charging = chkIsMaterialCharging.IsChecked == true ? "1" : chkIsMaterialCharging.IsChecked == false ? "0" : "",
				purchaseContract = chkIsMaterialContract.IsChecked == true ? "1" : chkIsMaterialContract.IsChecked == false ? "2" : "",
			};
			return para;
		}

		/// <summary>
		/// 导出耗材按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void btnExportMaterialXLS_Click(object sender, RoutedEventArgs e)
		{
			bool exportDisabled = false;
			bool combine = false;
			btnExportMaterialXLS.IsEnabled = false;
			MessageBoxResult r;
			r = MessageBox.Show("是否导出停用耗材？", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
			if (r == MessageBoxResult.Yes)
			{
				exportDisabled = true;
			}
			if (r == MessageBoxResult.Cancel)
			{
				btnExportMaterialXLS.IsEnabled = true;
				return;
			}
			if (!exportDisabled)
			{
				combine = false;
			}
			else
			{
				r = MessageBox.Show("是否合并为一个Excel文件输出", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
				if (r == MessageBoxResult.Yes)
				{
					combine = true;
				}
				else
				{
					combine = false;
				}
			}
			SaveFileDialog saveFileDialog = new()
			{
				Title = "保存SPD物资信息",
				Filter = "Excel 97-2003 工作簿 (*.xls)|*.xls",
				FileName = $"医院商品导出表{DateTime.Now:yyyy-MM-dd}.xls"
			};
			if (saveFileDialog.ShowDialog() != true)
			{
				btnExportMaterialXLS.IsEnabled = true;
				return;
			}
			string fileDic = Path.GetDirectoryName(saveFileDialog.FileName) ?? "";
			if (!Path.Exists(fileDic))
			{
				btnExportMaterialXLS.IsEnabled = true;
				return;
			}
			string fileNameWithoutExt = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
			string filePathEnabled = Path.Combine(fileDic, fileNameWithoutExt + "_启用" + Path.GetExtension(saveFileDialog.FileName));
			string filePathDisabled = Path.Combine(fileDic, fileNameWithoutExt + "_停用" + Path.GetExtension(saveFileDialog.FileName));
			string filePathCombined = Path.Combine(fileDic, fileNameWithoutExt + "_全量" + ".xlsx");
			SPDMaterialParameter parameter = new()
			{
				flag = "1",
			};
			bool result1 = await MaterialMethods.ExportSPDMaterialWithPara(filePathEnabled, parameter);
			bool result2 = false;
			if (exportDisabled)
			{
				parameter.flag = "0";
				result2 = await MaterialMethods.ExportSPDMaterialWithPara(filePathDisabled, parameter);
			}
			if (result1 && result2 && combine)
			{
				// 合并两个Excel 新增一列是否启用
				MergeExcel(filePathEnabled, filePathDisabled, filePathCombined);
				// 删除单独的启用和停用文件
				try
				{
					File.Delete(filePathEnabled);
					File.Delete(filePathDisabled);
				}
				catch
				{
					// 无论如何都不影响合并文件的使用体验，所以即使删除失败也不提示用户了
				}
			}
			if (result1 && (result2 || !combine))
			{
				MessageBox.Show($"导出成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			btnExportMaterialXLS.IsEnabled = true;
		}

		/// <summary>
		/// 合并耗材记录
		/// </summary>
		/// <param name="enabledPath">启用表</param>
		/// <param name="disabledPath">停用表</param>
		/// <param name="outputPath">输出表</param>
		private static void MergeExcel(string enabledPath, string disabledPath, string outputPath)
		{
			XSSFWorkbook workbook = new();
			ISheet sheet = workbook.CreateSheet("合并数据");

			int rowIndex = 0;

			// 读取并写入
			void AppendFile(string path, string flag, bool writeHeader)
			{
				using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
				HSSFWorkbook wb = new(fs);
				ISheet sh = wb.GetSheetAt(0);

				for (int i = 0; i <= sh.LastRowNum; i++)
				{
					IRow sourceRow = sh.GetRow(i);
					if (sourceRow == null) continue;

					// 表头处理
					if (i == 0)
					{
						if (!writeHeader) continue;

						IRow newRow = sheet.CreateRow(rowIndex++);
						int colIndex = 0;

						for (int j = 0; j < sourceRow.LastCellNum; j++)
						{
							newRow.CreateCell(colIndex++)
								  .SetCellValue(sourceRow.GetCell(j)?.ToString());
						}

						// 新增列
						newRow.CreateCell(colIndex).SetCellValue("是否启用");
						continue;
					}

					// 数据行
					IRow targetRow = sheet.CreateRow(rowIndex++);
					int targetCol = 0;

					for (int j = 0; j < sourceRow.LastCellNum; j++)
					{
						targetRow.CreateCell(targetCol++)
								 .SetCellValue(sourceRow.GetCell(j)?.ToString());
					}

					// 新增列值
					targetRow.CreateCell(targetCol).SetCellValue(flag);
				}
			}

			// 先写启用（带表头）
			AppendFile(enabledPath, "是", true);

			// 再写停用（不写表头）
			if (File.Exists(disabledPath))
				AppendFile(disabledPath, "否", false);

			// 保存
			using FileStream outFs = new(outputPath, FileMode.Create, FileAccess.Write);
			workbook.Write(outFs);
		}
		
		/// <summary>
		/// 加载供应商列表到ComboBox
		/// </summary>
		private async void LoadProviderToCB()
		{
			List<Provider> providers = await ProviderMethods.GetProviders();
			cbProviderInMaterial.ItemsSource = providers;
			return;
		}

		/// <summary>
		/// 防止tabMain_SelectionChanged事件导致的ComboBox刷新时
		/// SelectedIndex变化引发的事件处理逻辑混乱（暂时没有更好的解决方案了）
		/// FUCK MICROSOFT
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cbProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			e.Handled = true;
		}
		#endregion

		#region TAB 02 消耗查询
		#region TAB 02-01 EPC 追溯
		private async void btnTrackEPC_Click(object sender, RoutedEventArgs e)
		{
			string epc = txtEPC.Text;
			if (!epc.ToUpper().StartsWith('E'))
			{
				MessageBox.Show("输入的唯一码格式有误！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			btnTrackEPC.IsEnabled = false;
			EPCTrackData? data = await ConsumeMethods.EPC.TrackEPC(epc);
			btnTrackEPC.IsEnabled = true;
			if (data == null)
			{
				return;
			}
			string msgBoxTitle = $"{data.UseStatus}";
			if (data.UseDate != null)
			{
				msgBoxTitle += $" {data.JfDeptName} [{data.PatientId}]{data.PatientName} {data.UseDate:yyyy-MM-dd HH:mm}";
			}
			string msgBoxContent =
				$"{epc}\n" +
				$"ID：{data.GoodsId.Split('-')[1]} " +
				$"{data.GoodsName}\n" +
				$"型号：{data.GoodsGg}\n" +
				$"批号：{data.BatchCode}\n" +
				$"配送单：{data.BatchId}\n" +
				$"供应商名称：{data.ProvName}\n" +
				$"生产厂商名称：{data.MfrsName}\n" +
				$"库存科室：{data.KcDeptName}\n" +
				$"注册证：{data.CertificateCode}\n\n";
			if (data.TraceablityNodes != null && data.TraceablityNodes.Count > 0)
			{
				msgBoxContent += "追溯信息：\n";
				foreach (var node in data.TraceablityNodes)
				{
					msgBoxContent += $"-【{node.InType}】 {node.FillDate:yyyy-MM-dd HH:mm:ss}\n  {node.OutOrgName + node.OutDeptName} ➡️ {node.InDeptName} \n  制单：{node.FillerName} 审核：{node.Auditor}\n\n";
				}
			}
			MessageBox.Show(msgBoxContent, msgBoxTitle, MessageBoxButton.OK, MessageBoxImage.Information);
		}
		#endregion
		#endregion

		#region TAB 03 后勤物资
		/// <summary>
		/// 补充后勤出入库报表退还单据缺失的科室字段
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void btnFetchDeptByImmRK_Click(object sender, RoutedEventArgs e)
		{
			// 读取用户Excel文件
			OpenFileDialog openFileDialog = new()
			{
				Filter = "Excel 工作簿 (*.xlsx)|*.xlsx|Excel 97-2003 工作簿 (*.xls)|*.xls",
				Title = "请选择后勤出入库报表文件"
			};
			string filePath = "";
			btnFetchDeptByImmRK.IsEnabled = false;
			if (openFileDialog.ShowDialog() != true)
			{
				btnFetchDeptByImmRK.IsEnabled = true;
				return;
			}
			filePath = openFileDialog.FileName;
			if (!File.Exists(filePath))
			{
				btnFetchDeptByImmRK.IsEnabled = true;
				MessageBox.Show("尝试打开的文件不存在！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			try
			{
				IWorkbook workbook;
				using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					if (filePath.EndsWith(".xls"))
						workbook = new HSSFWorkbook(fs);
					else
						workbook = new XSSFWorkbook(fs);
				}

				ISheet sheet = workbook.GetSheetAt(0);

				// 获取表头
				IRow headerRow = sheet.GetRow(0);

				int col单号 = -1;
				int col科室 = -1;
				int col类型 = -1;

				// 查找列索引
				for (int i = 0; i < headerRow.LastCellNum; i++)
				{
					string val = headerRow.GetCell(i)?.ToString();
					if (val == "单号") col单号 = i;
					if (val == "科室") col科室 = i;
					if (val == "类型") col类型 = i;
				}

				if (col单号 == -1 || col科室 == -1 || col类型 == -1)
				{
					btnFetchDeptByImmRK.IsEnabled = true;
					MessageBox.Show("未找到必要的列（单号、科室、类型）！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// 遍历数据行
				for (int i = 1; i <= sheet.LastRowNum; i++)
				{
					var row = sheet.GetRow(i);
					if (row == null) continue;

					string rkID = row.GetCell(col单号)?.ToString() ?? "";
					string type = row.GetCell(col类型)?.ToString() ?? "";
					if (string.IsNullOrWhiteSpace(rkID)) continue;
					if (!rkID.StartsWith("RK")) continue;
					if (type != "退还入库") continue;

					try
					{
						(string? deptID, string? deptName) = await ImmMethods.GetDepByRK(rkID);
						ICell cellName = row.GetCell(col科室) ?? row.CreateCell(col科室);
						cellName.SetCellValue(deptName ?? "");
					}
					catch (Exception)
					{
						// 单行失败不中断
						row.CreateCell(col科室).SetCellValue("获取失败");
					}
				}
				// 保存文件（覆盖原文件）
				using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				{
					workbook.Write(fs);
				}
				MessageBox.Show("处理完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
				btnFetchDeptByImmRK.IsEnabled = true;
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = filePath,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				btnFetchDeptByImmRK.IsEnabled = true;
				MessageBox.Show($"发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion
	}
}
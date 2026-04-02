using Microsoft.Win32;
using Nimaime.Helper.File;
using Nimaime.SPD.Common;
using Nimaime.SPD.HIS;
using Nimaime.SPD.SPD;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
			DataContext = this;
			CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();

			culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
			culture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
			culture.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";

			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;

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
			LoadDeptToCB();
			ctlMaterialBaseData.LoadProviderToCB();
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
		
		#endregion

		#region TAB 02 消耗查询
		#region TAB 02-01 消耗报表
		/// <summary>
		/// 载入科室到ComboBox（用于消耗报表的查询参数选择）
		/// </summary>
		private async void LoadDeptToCB()
		{
			List<Department> depts = await DepartmentMethods.GetAllDepartments();
			cbDeptInConsume.ItemsSource = depts;
		}

		/// <summary>
		/// 一键设置为上一个财务月
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSetLastFinanceMonth_Click(object sender, RoutedEventArgs e)
		{
			(dpStartDate.SelectedDate, dpEndDate.SelectedDate) = DateTimeMethods.GetLastFinancialMonth(DateTime.Now);
		}
		#endregion
		#region TAB 02-02 高值追溯
		/// <summary>
		/// 单个EPC输入追溯
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
				MessageBox.Show("查询时出错，请检查网络连接！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
				$"库存科室：{data.StocName}\n" +
				$"注册证：{data.CertificateCode}\n\n";
			if (data.TraceablityNodes != null && data.TraceablityNodes.Count > 0)
			{
				msgBoxContent += "追溯信息：\n";
				foreach (var node in data.TraceablityNodes)
				{
					msgBoxContent += $"-【{node.InType}】 {node.FillDate:yyyy-MM-dd HH:mm:ss}\n  {node.OutOrgName + node.OutDeptName} → {node.InDeptName} \n  制单：{node.FillerName} ";
					if (!string.IsNullOrWhiteSpace(node.Auditor))
					{
						msgBoxContent += $"审核：{node.Auditor}\n\n";
					}
					else
					{
						msgBoxContent += "\n\n";
					}
				}
			}
			MessageBox.Show(msgBoxContent, msgBoxTitle, MessageBoxButton.OK, MessageBoxImage.Information);
		}

		/// <summary>
		/// 读取Excel表格批量查询
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void btnTrackEPCByExcel_Click(object sender, RoutedEventArgs e)
		{
			// 读取用户Excel文件
			OpenFileDialog openFileDialog = new()
			{
				Filter = "Excel 工作簿 (*.xlsx)|*.xlsx|Excel 97-2003 工作簿 (*.xls)|*.xls",
				Title = "请选择需追溯的EPC报表文件，表格需包含列名为【唯一码】或【EPC】的列"
			};
			if (openFileDialog.ShowDialog() != true)
			{
				return;
			}
			string filePath2Trace = openFileDialog.FileName;
			if (!File.Exists(filePath2Trace))
			{
				MessageBox.Show("尝试打开的文件不存在！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			btnTrackEPCByExcel.IsEnabled = false;
			DataSet ds = ExcelHelper.Excel2DataSet(filePath2Trace) ?? new();
			foreach (DataTable dt in ds.Tables)
			{
				string workingColumn = string.Empty;
				if (dt.Columns.Contains("EPC"))
				{
					workingColumn = "EPC";
				}
				if (dt.Columns.Contains("唯一码"))
				{
					workingColumn = "唯一码";
				}
				if (workingColumn == string.Empty)
				{
					MessageBox.Show("未找到列名为【唯一码】或【EPC】的列！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
					btnTrackEPCByExcel.IsEnabled = true;
					return;
				}
				dt.Columns.Add("消耗状态", typeof(string));
				dt.Columns.Add("库存科室", typeof(string));
				dt.Columns.Add("计费科室", typeof(string));
				dt.Columns.Add("病案号", typeof(string));
				dt.Columns.Add("病人姓名", typeof(string));
				dt.Columns.Add("使用时间", typeof(string));

				string buttonTip;
				int idx_row = 0;
				foreach (DataRow row in dt.Rows)
				{
					buttonTip = $"表[{dt.TableName}]正在处理第{++idx_row}个，共{dt.Rows.Count}个";
					btnTrackEPCByExcel.Content = buttonTip;
					string epc = (row[workingColumn].ToString() ?? "").ToUpper();
					if (!epc.StartsWith('E') || epc.Length != 16)
					{
						continue;
					}
					EPCTrackData? data = await ConsumeMethods.EPC.TrackEPC(epc);
					if (data == null)
					{
						row["消耗状态"] = "查询失败";
						continue;
					}
					row["消耗状态"] = data.UseStatus;
					row["库存科室"] = data.KcDeptName;
					row["计费科室"] = data.JfDeptName;
					row["病案号"] = data.PatientId;
					row["病人姓名"] = data.PatientName;
					row["使用时间"] = data.UseDate?.ToString() ?? "";
				}
			}

			btnTrackEPCByExcel.Content = "表格批量查询";
			btnTrackEPCByExcel.IsEnabled = true;

			SaveFileDialog saveFileDialog = new()
			{
				Title = "保存追溯结果",
				Filter = "Excel 工作簿 (*.xlsx)|*.xlsx",
				FileName = $"EPC追溯结果_{Path.GetFileNameWithoutExtension(filePath2Trace)}_{DateTime.Now:yyyy-MM-dd}.xlsx"
			};

			if (saveFileDialog.ShowDialog() != true)
			{
				return;
			}

			string file2Save = saveFileDialog.FileName;
			ExcelHelper.SaveDataSet2Excel(ds, file2Save);
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
			{
				FileName = file2Save,
				UseShellExecute = true
			});
		}
		#endregion
		#endregion

		#region TAB 03 后勤物资
		/// <summary>
		/// 补充后勤出入库报表退还单据缺失的科室字段
		/// 因需要原样输出Excel
		/// 所以不使用ExcelHelper
		/// 直接使用NPOI进行读写操作
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
			btnFetchDeptByImmRK.IsEnabled = false;
			if (openFileDialog.ShowDialog() != true)
			{
				btnFetchDeptByImmRK.IsEnabled = true;
				return;
			}
			string filePath = openFileDialog.FileName;
			if (!File.Exists(filePath))
			{
				btnFetchDeptByImmRK.IsEnabled = true;
				MessageBox.Show("尝试打开的文件不存在！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			try
			{
				IWorkbook workbook;
				using (var fs = new FileStream("", FileMode.Open, FileAccess.Read))
				{
					if ("".EndsWith(".xls"))
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
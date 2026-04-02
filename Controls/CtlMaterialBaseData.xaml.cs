using Microsoft.Win32;
using Nimaime.SPD.Common;
using Nimaime.SPD.SPD;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.ApplicationModel;
using static Nimaime.SPD.SPD.MaterialMethods;

namespace Nimaime.SPD.Controls
{
	/// <summary>
	/// CtlMaterialBaseData.xaml 的交互逻辑
	/// </summary>
	public partial class CtlMaterialBaseData : UserControl
	{
		public CtlMaterialBaseData()
		{
			InitializeComponent();
			cbMaterialDGPager.ItemsSource = Enum.GetNames<Enums.CountPerPage>();
			cbMaterialDGPager.SelectedIndex = 0;
			LoadProviderToCB();
		}

		private int currentDGMatPage = -1;
		private int currentDGMatPageCount = -1;
		private ContextMenu menuDGMaterial { get; set; }
		/// <summary>
		/// 加载耗材信息到UI DataGrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void btnLoadMaterial_Click(object sender, RoutedEventArgs e)
		{
			UpdateMatPage(1);
		}

		#region 页面控制
		/// <summary>
		/// 首页
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDGMatFirstPage_Click(object sender, RoutedEventArgs e)
		{
			UpdateMatPage(1);
		}

		/// <summary>
		/// 上一页
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDGMatPrevPage_Click(object sender, RoutedEventArgs e)
		{
			UpdateMatPage(currentDGMatPage - 1);
		}

		/// <summary>
		/// 下一页
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDGMatNextPage_Click(object sender, RoutedEventArgs e)
		{
			UpdateMatPage(currentDGMatPage + 1);
		}

		/// <summary>
		/// 尾页
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDGMatLastPage_Click(object sender, RoutedEventArgs e)
		{
			UpdateMatPage(currentDGMatPageCount);
		}

		/// <summary>
		/// 加载指定页面
		/// </summary>
		/// <param name="page2Load"></param>
		private async void UpdateMatPage(int page2Load)
		{
			try
			{
				currentDGMatPage = -1;
				btnLoadMaterial.IsEnabled = false;
				btnDGMatFirstPage.IsEnabled = false;
				btnDGMatLastPage.IsEnabled = false;
				btnDGMatPrevPage.IsEnabled = false;
				btnDGMatNextPage.IsEnabled = false;
				btnLoadMaterial.Content = "加载中...";

				SPDMaterialParameter para = GenMaterialParaByUI();
				Enums.CountPerPage enumPageCount = Enum.Parse<Enums.CountPerPage>(cbMaterialDGPager.SelectedItem.ToString() ?? "每页100个");
				(List<Material> materials, currentDGMatPageCount, int totalCount) = await MaterialMethods.GetSPDMaterialList(para, page2Load, (int)enumPageCount);
				if (currentDGMatPageCount > 0)
				{
					btnDGMatFirstPage.IsEnabled = currentDGMatPageCount > 1 && page2Load != 1;
					btnDGMatLastPage.IsEnabled = currentDGMatPageCount > 1 && page2Load != currentDGMatPageCount;
					btnDGMatPrevPage.IsEnabled = currentDGMatPageCount > 1 && page2Load > 1;
					btnDGMatNextPage.IsEnabled = currentDGMatPageCount > 1 && page2Load < currentDGMatPageCount;
					currentDGMatPage = page2Load;
					lblDGMatPage.Content = $"{currentDGMatPage}/{currentDGMatPageCount}";
				}
				else
				{
					lblDGMatPage.Content = $"{0}/{0}";
				}
				lblDGMatCount.Content = $"共 {totalCount} 条";
				dgMaterial.ItemsSource = materials;

				if (materials.Count == 0)
				{
					MessageBox.Show("未找到符合条件的耗材数据", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"加载耗材失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				btnLoadMaterial.IsEnabled = true;
				btnLoadMaterial.Content = "加载";
			}
		}
		#endregion

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

		/// <summary>
		/// 设置查询参数
		/// </summary>
		/// <returns></returns>
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
		public async void LoadProviderToCB()
		{
			if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
			{
				return; // 设计时跳过依赖解析
			}
			cbProviderInMaterial.ItemsSource = await ProviderMethods.GetProviders();
		}

		/// <summary>
		/// 表格右键菜单
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void dgMaterial_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			menuDGMaterial = new();
			MenuItem dgMaterialContextImport2Dept = new()
			{
				Header = "导入到科室",
			};
			dgMaterialContextImport2Dept.Click += dgMaterialContextImport2Dept_Click;
			menuDGMaterial.Items.Add(dgMaterialContextImport2Dept);
			menuDGMaterial.StaysOpen = true;
			menuDGMaterial.IsOpen = true;
		}

		/// <summary>
		/// 导入到科室菜单点击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void dgMaterialContextImport2Dept_Click(object sender, RoutedEventArgs e)
		{
			List<Material> lstSelectedMat = [.. dgMaterial.SelectedItems.Cast<Material>()];
			ImportMaterial2Dept window = new(lstSelectedMat);
			window.ShowDialog();
		}
	}
}

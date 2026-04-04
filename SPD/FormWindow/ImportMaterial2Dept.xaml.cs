using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nimaime.SPD.SPD.FormWindow
{
	/// <summary>
	/// ImportMaterial2Dept.xaml 的交互逻辑
	/// </summary>
	public partial class ImportMaterial2Dept : Window
	{
		private List<Material> Materials = [];
		public ImportMaterial2Dept(List<Material> materials2Import)
		{
			InitializeComponent();
			Materials = materials2Import;
			lblHint.Content = $"将 {Materials.Count} 个耗材导入 {cbDept2Imp.SelectedItems.Count} 个科室";
			LoadDeptToCombo();
		}

		/// <summary>
		/// 加载科室数据到CB
		/// </summary>
		private async void LoadDeptToCombo()
		{
			try
			{
				List<Department> departments = await DepartmentMethods.GetAllDepartments();
				cbDept2Imp.ItemsSource = departments;
			}
			catch
			{
				MessageBox.Show("获取部门列表失败，请稍后再试");
				Close();
				return;
			}
		}

		/// <summary>
		/// 多选框更新时刷新页面
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cbDept2Imp_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			lblHint.Content = $"将 {Materials.Count} 个耗材导入 {cbDept2Imp.SelectedItems.Count} 个科室";
			if (cbDept2Imp.SelectedItems.Count > 0)
			{
				btnImport2Dept.IsEnabled = true;
			}
			else
			{
				btnImport2Dept.IsEnabled = false;
			}
		}

		/// <summary>
		/// 导入操作
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void btnImport2Dept_Click(object sender, RoutedEventArgs e)
		{
			List<Department> departments = [.. cbDept2Imp.SelectedItems.Cast<Department>()];
			await MaterialMethods.ImportMaterial2Dept(Materials, departments);
		}

		/// <summary>
		/// 按关键词搜索科室并添加到CB已选
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnAddDept2CB_Click(object sender, RoutedEventArgs e)
		{
			string keyword = txtDeptKeyword.Text.Trim();
			if (string.IsNullOrWhiteSpace(keyword))
			{
				return;
			}
			foreach (Department dept in cbDept2Imp.ItemsSource)
			{
				if (dept.ID.Contains(keyword) || dept.EName.Contains(keyword))
				{
					if (!cbDept2Imp.SelectedItems.Contains(dept))
					{
						cbDept2Imp.SelectedItems.Add(dept);
					}
				}
			}
			txtDeptKeyword.Text = string.Empty;
		}
	}
}

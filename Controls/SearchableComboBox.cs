using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Nimaime.SPD.Controls
{
	public class SearchableComboBox : ComboBox
	{
		private ICollectionView _collectionView;
		private TextBox _editableTextBox;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// 获取内部 TextBox（关键）
			_editableTextBox = GetTemplateChild("PART_EditableTextBox") as TextBox;

			if (_editableTextBox != null)
			{
				_editableTextBox.TextChanged -= EditableTextBox_TextChanged;
				_editableTextBox.TextChanged += EditableTextBox_TextChanged;
			}
		}

		public SearchableComboBox()
		{
			IsEditable = true;
			IsTextSearchEnabled = false;
			StaysOpenOnEdit = true;

			Loaded += (s, e) => InitCollectionView();
		}

		protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
			base.OnItemsSourceChanged(oldValue, newValue);
			InitCollectionView();
		}

		private void InitCollectionView()
		{
			if (ItemsSource == null) return;

			_collectionView = CollectionViewSource.GetDefaultView(ItemsSource);
			if (_collectionView != null)
			{
				_collectionView.Filter = FilterItem;
			}
		}

		#region SearchText 依赖属性

		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchableComboBox),
				new PropertyMetadata(string.Empty, OnSearchTextChanged));

		public string SearchText
		{
			get => (string)GetValue(SearchTextProperty);
			set => SetValue(SearchTextProperty, value);
		}

		private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as SearchableComboBox;
			control?._collectionView?.Refresh();

			if (control != null && !control.IsDropDownOpen)
				control.IsDropDownOpen = true;
		}

		#endregion

		private void EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (_editableTextBox != null)
			{
				// 当前输入文本
				var text = _editableTextBox.Text;

				// 🚨 关键：暂时解除 SelectionChanged 影响
				if (SelectedItem != null)
				{
					SelectedItem = null;
				}

				// 更新搜索文本
				SearchText = text;

				// 保持光标位置（防止跳动）
				_editableTextBox.SelectionStart = text.Length;

				// 展开下拉
				if (!IsDropDownOpen)
					IsDropDownOpen = true;
			}
		}

		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			// 如果是用户点击选中 → 正常处理
			if (IsKeyboardFocusWithin && IsDropDownOpen)
			{
				base.OnSelectionChanged(e);
			}
			else
			{
				// 🚨 输入过程中，不允许自动选中
				if (!string.IsNullOrEmpty(SearchText))
				{
					SelectedItem = null;
					return;
				}

				base.OnSelectionChanged(e);
			}
		}

		private bool FilterItem(object obj)
		{
			if (obj == null) return false;

			if (string.IsNullOrWhiteSpace(SearchText))
				return true;

			string text = GetItemText(obj);

			return text?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private string GetItemText(object item)
		{
			if (item == null) return string.Empty;

			if (!string.IsNullOrEmpty(DisplayMemberPath))
			{
				var prop = item.GetType().GetProperty(DisplayMemberPath);
				if (prop != null)
				{
					var value = prop.GetValue(item);
					return value?.ToString() ?? "";
				}
			}

			return item.ToString();
		}
	}
}
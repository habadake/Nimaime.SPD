using System;
using System.Collections.Generic;
using System.Text;

namespace Nimaime.SPD.Common
{
	public static class FileTypeDetect
	{
		/// <summary>
		/// 检测字节流是否是指定格式
		/// </summary>
		/// <param name="data">字节数据</param>
		/// <param name="fileType">期望的文件格式</param>
		/// <returns></returns>
		public static bool IsFileType(byte[] data, FileType fileType)
		{
			switch (fileType)
			{
				case FileType.OTHER:
					return true;
				case FileType.Excel:
					if (data == null || data.Length < 4)
						return false;

					// XLS
					if (data[0] == 0xD0 &&
						data[1] == 0xCF &&
						data[2] == 0x11 &&
						data[3] == 0xE0)
						return true;

					// XLSX (ZIP)
					if (data[0] == 0x50 &&
						data[1] == 0x4B &&
						data[2] == 0x03 &&
						data[3] == 0x04)
						return true;

					return false;
				default:
					return true;
			}
		}

		/// <summary>
		/// 文件类型枚举
		/// </summary>
		public enum FileType
		{
			/// <summary>
			/// 其他未指定格式
			/// </summary>
			OTHER = 0,
			/// <summary>
			/// Excel 工作簿（包括 XLS 和 XLSX）
			/// </summary>
			Excel = 1,
			/// <summary>
			/// PDF 文档
			/// </summary>
			PDF = 2,
			/// <summary>
			/// TXT 文本文档
			/// </summary>
			TXT = 3,
		}
	}
}

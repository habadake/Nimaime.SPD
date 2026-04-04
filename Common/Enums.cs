using System;
using System.Collections.Generic;
using System.Text;

namespace Nimaime.SPD.Common
{
	public class Enums
	{
		/// <summary>
		/// 翻页数量
		/// </summary>
		public enum CountPerPage
		{
			每页100个 = 100,
			每页200个 = 200,
			每页500个 = 500,
			每页1000个 = 1000,
			无视卡机全部加载 = 99999999
		}

		/// <summary>
		/// 耗材管理类型
		/// </summary>
		public enum MaterialType
		{
			不区分 = 0,
			低值 = 10,
			高值 = 20,
			试剂 = 60,
		}
	}
}

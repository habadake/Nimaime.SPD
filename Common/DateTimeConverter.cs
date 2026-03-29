using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nimaime.SPD.Common
{
	/// <summary>
	/// JsonConverter DateTime转换 读取yyyy-MM-dd HH:mm:ss格式的 DateTime
	/// </summary>
	public class DateTimeConverter : JsonConverter<DateTime>
	{
		private const string Format = "yyyy-MM-dd HH:mm:ss";

		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var str = reader.GetString();

			if (string.IsNullOrEmpty(str))
				return default;

			if (DateTime.TryParseExact(str, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
			{
				return dt;
			}

			// 兜底（防止有些接口格式不一致）
			return DateTime.Parse(str);
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString(Format));
		}
	}

	public static class JSOptionConverterMaker
	{
		public readonly static JsonSerializerOptions Option = new()
		{
			PropertyNameCaseInsensitive = true,
		};
	}

	/// <summary>
	/// 有关时间日期的相关方法
	/// </summary>
	public static class DateTimeMethods
	{
		/// <summary>
		/// 获取指定日期的上一个完整财务月的起止日期
		/// </summary>
		/// <param name="day">指定日期</param>
		/// <returns>上一个完整财务月的起止日期</returns>
		public static (DateTime start, DateTime end) GetLastFinancialMonth(DateTime day)
		{
			DateTime currentStart;
			DateTime currentEnd;

			if (day.Day >= 11)
			{
				// 当前财务月：本月11日 ~ 下月10日
				currentStart = new DateTime(day.Year, day.Month, 11);
				currentEnd = currentStart.AddMonths(1).AddDays(-1); // 下月10日
			}
			else
			{
				// 当前财务月：上月11日 ~ 本月10日
				DateTime lastMonth = day.AddMonths(-1);
				currentStart = new DateTime(lastMonth.Year, lastMonth.Month, 11);
				currentEnd = new DateTime(day.Year, day.Month, 10);
			}

			// 上一个完整财务月
			DateTime lastStart = currentStart.AddMonths(-1);
			DateTime lastEnd = currentStart.AddDays(-1);

			return (lastStart, lastEnd);
		}
	}
}

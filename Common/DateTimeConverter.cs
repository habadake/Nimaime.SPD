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
}

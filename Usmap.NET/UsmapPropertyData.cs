using GenericReader;

namespace Usmap.NET
{
	public class UsmapPropertyData
	{
		public EUsmapPropertyType Type { get; }
		public string EnumName { get; set; }
		public string StructType { get; set; }
		public UsmapPropertyData InnerType { get; set; }
		public UsmapPropertyData ValueType { get; set; }

		internal UsmapPropertyData(EUsmapPropertyType type)
		{
			Type = type;
		}

		internal static UsmapPropertyData Deserialize(IGenericReader reader, string[] names)
		{
			var propType = reader.Read<EUsmapPropertyType>();
			var data = new UsmapPropertyData(propType);

			switch (propType)
			{
				case EUsmapPropertyType.EnumProperty:
				{
					data.InnerType = Deserialize(reader, names);
					var idx = reader.Read<uint>();
					data.EnumName = names[idx];
					break;
				}
				case EUsmapPropertyType.StructProperty:
				{
					var idx = reader.Read<uint>();
					data.StructType = names[idx];
					break;
				}
				case EUsmapPropertyType.SetProperty:
				case EUsmapPropertyType.ArrayProperty:
				case EUsmapPropertyType.OptionalProperty:
				{
					data.InnerType = Deserialize(reader, names);
					break;
				}
				case EUsmapPropertyType.MapProperty:
				{
					data.InnerType = Deserialize(reader, names);
					data.ValueType = Deserialize(reader, names);
					break;
				}
			}

			return data;
		}
	}
}
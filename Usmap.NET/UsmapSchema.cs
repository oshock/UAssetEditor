namespace Usmap.NET
{
	public readonly struct UsmapSchema
	{
		public string Name { get; }
		public string SuperType { get; }
		public ushort PropCount { get; }
		public UsmapProperty[] Properties { get; }

		public UsmapSchema(string name, string superType, ushort propCount, UsmapProperty[] properties)
		{
			Name = name;
			SuperType = superType;
			PropCount = propCount;
			Properties = properties;
		}

		public override string ToString()
		{
			return $"{Name}{(SuperType == null ? null : $":{SuperType}")} | {Properties.Length} ({PropCount})";
		}
	}
}
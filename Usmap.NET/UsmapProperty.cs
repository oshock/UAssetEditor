namespace Usmap.NET
{
	public readonly struct UsmapProperty
	{
		public string Name { get; }
		public ushort SchemaIdx { get; }
		public byte ArraySize { get; }
		public UsmapPropertyData Data { get; }

		public UsmapProperty(string name, ushort schemaIdx, byte arraySize, UsmapPropertyData data)
		{
			Name = name;
			SchemaIdx = schemaIdx;
			ArraySize = arraySize;
			Data = data;
		}

		public override string ToString()
		{
			return $"{Name} | {Data.Type}";
		}
	}
}
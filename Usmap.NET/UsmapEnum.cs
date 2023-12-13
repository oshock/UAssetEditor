namespace Usmap.NET
{
	public readonly struct UsmapEnum
	{
		public string Name { get; }
		public string[] Names { get; }

		public UsmapEnum(string name, string[] names)
		{
			Name = name;
			Names = names;
		}

		public override string ToString()
		{
			return $"{Name} | {Names.Length}";
		}
	}
}
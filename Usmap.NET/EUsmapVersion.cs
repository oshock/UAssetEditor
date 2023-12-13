namespace Usmap.NET
{
	public enum EUsmapVersion : byte
	{
		INITIAL,

		LATEST_PLUS_ONE,
		LATEST = LATEST_PLUS_ONE - 1
	}
}
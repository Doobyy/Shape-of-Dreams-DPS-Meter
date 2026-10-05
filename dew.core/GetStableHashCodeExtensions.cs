using System.Text;

public static class GetStableHashCodeExtensions
{
	public static uint GetStableHashCode(this string input)
	{
		uint num = 2166136261u;
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		foreach (byte b in bytes)
		{
			num ^= b;
			num *= 16777619;
		}
		return num;
	}
}

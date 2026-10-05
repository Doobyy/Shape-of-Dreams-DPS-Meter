using System.Collections.Generic;

public static class ShuffleListExtension
{
	public static void Shuffle<T>(this IList<T> list, DewRandom random = null)
	{
		if (random == null)
		{
			random = DewRandom.instance;
		}
		for (int i = 0; i < list.Count; i++)
		{
			T value = list[i];
			int index = random.Range(i, list.Count);
			list[i] = list[index];
			list[index] = value;
		}
	}
}

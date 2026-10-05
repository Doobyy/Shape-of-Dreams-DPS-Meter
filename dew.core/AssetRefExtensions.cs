using UnityEngine;

public static class AssetRefExtensions
{
	public static AssetRef<T> ToAssetRef<T>(this T asset) where T : Object
	{
		return new AssetRef<T>(asset);
	}

	public static AssetRef<T>[] ToAssetRefs<T>(this T[] asset) where T : Object
	{
		AssetRef<T>[] array = new AssetRef<T>[asset.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new AssetRef<T>(asset[i]);
		}
		return array;
	}

	public static T[] Values<T>(this AssetRef<T>[] refs) where T : Object
	{
		if (refs == null)
		{
			return null;
		}
		T[] array = new T[refs.Length];
		for (int i = 0; i < refs.Length; i++)
		{
			array[i] = refs[i];
		}
		return array;
	}
}

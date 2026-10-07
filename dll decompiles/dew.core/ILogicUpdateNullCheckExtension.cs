using UnityEngine;

internal static class ILogicUpdateNullCheckExtension
{
	internal static bool IsNull(this ILogicUpdate logic)
	{
		if (logic is Object obj)
		{
			return obj == null;
		}
		return logic == null;
	}
}

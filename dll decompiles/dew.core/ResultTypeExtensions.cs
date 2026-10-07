public static class ResultTypeExtensions
{
	public static bool IsWin(this DewGameResult.ResultType type)
	{
		return type >= DewGameResult.ResultType.PureWhiteDream;
	}
}

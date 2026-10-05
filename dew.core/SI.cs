using System;
using System.Runtime.InteropServices;

public static class SI
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct DoImmediately
	{
	}

	public struct WaitForSeconds(float seconds)
	{
		internal float _seconds = seconds;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct WaitForNextLogicUpdate
	{
	}

	public struct WaitForCondition(Func<bool> condition)
	{
		internal Func<bool> _condition = condition;
	}
}

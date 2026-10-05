using System;
using NCalc;
using UnityEngine;

public static class NCalcExtensions
{
	public static void ExtraNCalcFunctions(string name, FunctionArgs functionArgs)
	{
		if (name == "Clamp")
		{
			EnsureNumOfParams(3);
			object val = functionArgs.Parameters[0].Evaluate();
			object val2 = functionArgs.Parameters[1].Evaluate();
			object val3 = functionArgs.Parameters[2].Evaluate();
			functionArgs.Result = Mathf.Clamp(ToFloat(val), ToFloat(val2), ToFloat(val3));
		}
		else if (name == "Clamp01")
		{
			EnsureNumOfParams(1);
			object val4 = functionArgs.Parameters[0].Evaluate();
			functionArgs.Result = Mathf.Clamp01(ToFloat(val4));
		}
		void EnsureNumOfParams(int count)
		{
			if (functionArgs.Parameters.Length != count)
			{
				throw new Exception($"Expected {count} parameters, got {functionArgs.Parameters.Length}");
			}
		}
	}

	private static float ToFloat(object val)
	{
		if (val is float)
		{
			return (float)val;
		}
		if (val is double num)
		{
			return (float)num;
		}
		if (val is int num2)
		{
			return num2;
		}
		throw new ArgumentException("val");
	}
}

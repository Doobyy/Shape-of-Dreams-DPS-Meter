using System;
using System.Globalization;
using NCalc;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct Formula
{
	[FormerlySerializedAs("forumla")]
	public string formula;

	private Expression _exp;

	private string _cachedFormula;

	private bool _isSimple;

	private float _coeffA;

	private float _coeffB;

	private float _coeffC;

	private float _lastInput;

	private float _lastAnswer;

	private static readonly char[] InvalidSimpleChars = "abcdefghijklmnopqrstuvwyz()[]{}/%^".ToCharArray();

	public static implicit operator Formula(string str)
	{
		return new Formula
		{
			formula = str
		};
	}

	public void Bake()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected Obj, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected Obj, but got Unknown
		if (formula == _cachedFormula)
		{
			return;
		}
		_cachedFormula = formula;
		_lastInput = float.NaN;
		if (TryParseSimplePolynomial(formula, out _coeffA, out _coeffB, out _coeffC))
		{
			_isSimple = true;
			_exp = null;
			return;
		}
		_isSimple = false;
		try
		{
			_exp = new Expression(formula);
			_exp.EvaluateFunction += NCalcExtensions.ExtraNCalcFunctions;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			_exp = null;
		}
	}

	public float Evaluate(float x)
	{
		Bake();
		if (_lastInput == x)
		{
			return _lastAnswer;
		}
		if (_isSimple)
		{
			_lastAnswer = _coeffA * x * x + _coeffB * x + _coeffC;
		}
		else
		{
			if (_exp == null)
			{
				return 0f;
			}
			_exp.Parameters["x"] = x;
			object obj = _exp.Evaluate();
			if (obj is float lastAnswer)
			{
				_lastAnswer = lastAnswer;
			}
			else if (obj is double num)
			{
				_lastAnswer = (float)num;
			}
			else if (obj is int num2)
			{
				_lastAnswer = num2;
			}
			else if (obj is long num3)
			{
				_lastAnswer = num3;
			}
			else
			{
				if (!(obj is decimal num4))
				{
					throw new ArgumentOutOfRangeException(obj?.GetType().Name ?? "null");
				}
				_lastAnswer = (float)num4;
			}
		}
		return _lastAnswer;
	}

	private bool TryParseSimplePolynomial(string f, out float a, out float b, out float c)
	{
		a = 0f;
		b = 0f;
		c = 0f;
		if (string.IsNullOrWhiteSpace(f))
		{
			c = 0f;
			return true;
		}
		try
		{
			string text = f.Replace(" ", "").Replace("-", "+-");
			if (text.IndexOfAny(InvalidSimpleChars) != -1)
			{
				return false;
			}
			string[] array = text.Split(new char[1] { '+' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text2 in array)
			{
				int num = 0;
				string text3 = text2;
				for (int j = 0; j < text3.Length; j++)
				{
					if (text3[j] == 'x')
					{
						num++;
					}
				}
				if (num > 2)
				{
					return false;
				}
				string text4 = text2.Replace("x", "").Replace("*", "");
				float num2;
				if (text4 == "")
				{
					num2 = 1f;
				}
				else
				{
					num2 = ((!(text4 == "-")) ? float.Parse(text4, CultureInfo.InvariantCulture) : (-1f));
				}
				switch (num)
				{
				case 2:
					a += num2;
					break;
				case 1:
					b += num2;
					break;
				default:
					c += num2;
					break;
				}
			}
			return true;
		}
		catch (FormatException)
		{
			return false;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool IsValid(string f, ref string errorMessage)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected Obj, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Expression val = new Expression(f, (EvaluateOptions)4);
			val.EvaluateFunction += NCalcExtensions.ExtraNCalcFunctions;
			val.Parameters["x"] = 0f;
			val.Evaluate();
			return true;
		}
		catch (Exception ex)
		{
			errorMessage = ex.Message;
			return false;
		}
	}
}

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class DewInputReadableTextExtensions
{
	public static string GetReadableText(this Key k)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if ((int)k == 0)
		{
			return DewLocalization.GetUIValue("Key_None");
		}
		if (DewInput.KeyReadableTextBindings.TryGetValue(k, out var value))
		{
			return value;
		}
		return ((object)k/*cast due to constrained. prefix*/).ToString();
	}

	public static string GetReadableText(this GamepadButton? b, bool dontUseSprite = false)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!b.HasValue)
		{
			return DewLocalization.GetUIValue("Key_None");
		}
		return b.Value.GetReadableText(dontUseSprite);
	}

	public static string GetReadableText(this GamepadButton b, bool dontUseSprite = false)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected I4, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected I4, but got Unknown
		if (dontUseSprite)
		{
			if (DewInput.GamepadButtonReadableTextBindings.TryGetValue((GamepadButtonEx)b, out var value))
			{
				return value;
			}
			return ((object)b/*cast due to constrained. prefix*/).ToString();
		}
		return $"<sprite name=\"{DewInput.controllerButtonType}.{(int)b}\">";
	}

	public static string GetReadableText(this GamepadButtonEx? b, bool dontUseSprite = false)
	{
		if (!b.HasValue)
		{
			return DewLocalization.GetUIValue("Key_None");
		}
		return b.Value.GetReadableText(dontUseSprite);
	}

	public static string GetReadableText(this GamepadButtonEx b, bool dontUseSprite = false)
	{
		if (dontUseSprite)
		{
			if (DewInput.GamepadButtonReadableTextBindings.TryGetValue(b, out var value))
			{
				return value;
			}
			return b.ToString();
		}
		return $"<sprite name=\"{DewInput.controllerButtonType}.{(int)b}\">";
	}

	public static string GetReadableText(this MouseButton b)
	{
		if (b == MouseButton.None)
		{
			return DewLocalization.GetUIValue("Key_None");
		}
		if (DewInput.MouseButtonReadableTextBindings.TryGetValue(b, out var value))
		{
			return value;
		}
		return b.ToString();
	}
}

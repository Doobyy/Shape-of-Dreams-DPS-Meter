using System;
using System.Collections.Generic;
using UnityEngine;

public class DewMessageSettings
{
	[Flags]
	public enum ButtonType
	{
		Ok = 1,
		Yes = 2,
		No = 4,
		Cancel = 8,
		Custom0 = 0x10,
		Custom1 = 0x20,
		Custom2 = 0x40,
		Custom3 = 0x80,
		None = 0x100
	}

	public class CustomButton
	{
		public string label;

		public Action onClick;
	}

	public UnityEngine.Object owner;

	public string rawContent;

	public ButtonType buttons = ButtonType.Ok;

	public string[] customButtonTexts;

	public Action<ButtonType> onClose;

	public bool destructiveConfirm;

	public ButtonType defaultButton = ButtonType.Cancel;

	public List<CustomButton> customButtons;

	public int defaultCustomButtonIndex = -1;

	public int customButtonsColumnCount;

	public bool verticalButtons;

	public Func<bool> validator;

	internal bool IsValid()
	{
		if ((object)owner == null || owner != null)
		{
			if (validator != null)
			{
				return validator();
			}
			return true;
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[Serializable]
public class PCBind : ICloneable
{
	public List<Key> modifiers = new List<Key>();

	public Key key;

	public MouseButton mouse;

	public object Clone()
	{
		PCBind pCBind = (PCBind)MemberwiseClone();
		pCBind.modifiers = new List<Key>(modifiers);
		return pCBind;
	}

	public PCBind()
	{
	}

	public PCBind(Key key, params Key[] modifiers)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		this.key = key;
		this.modifiers.AddRange(modifiers);
	}

	public PCBind(MouseButton mouse, params Key[] modifiers)
	{
		this.mouse = mouse;
		this.modifiers.AddRange(modifiers);
	}

	public static implicit operator PCBind(Key key)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new PCBind
		{
			key = key
		};
	}

	public static implicit operator PCBind(MouseButton mouse)
	{
		return new PCBind
		{
			mouse = mouse
		};
	}
}

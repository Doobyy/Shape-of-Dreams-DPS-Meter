using System;
using UnityEngine;

public class LucidDream : GameEffect
{
	public static readonly Color GoodColor = new Color(200f / 255f, 1f, 168f / 255f);

	public static readonly Color EvilColor = new Color(1f, 165f / 255f, 158f / 255f);

	public static readonly Color ChaoticColor = new Color(181f / 255f, 182f / 255f, 1f);

	public Sprite icon;

	public LucidDreamType type;

	public Color color => type switch
	{
		LucidDreamType.Good => GoodColor, 
		LucidDreamType.Evil => EvilColor, 
		LucidDreamType.Chaotic => ChaoticColor, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public override bool isDestroyedOnRoomChange => false;

	public virtual string GetCustomInGameTooltip()
	{
		return null;
	}

	private void MirrorProcessed()
	{
	}
}

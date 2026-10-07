using System;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Name)]
public class Skin : MonoBehaviour, ICosmetic
{
	public string category;

	public SkinRarity rarity;

	public int requiredLevel;

	public bool generatedFromServer;

	public string[] dlcIds;

	public SkillVisualOverrideItem[] skillVisuals = new SkillVisualOverrideItem[0];

	public Sprite previewImage;

	public Vector3 previewRotationOffset;

	public float previewFovOffset;

	public int stardustPrice => rarity switch
	{
		SkinRarity.Default => 0, 
		SkinRarity.Rare => 100, 
		SkinRarity.Epic => 200, 
		SkinRarity.Legendary => 300, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	bool ICosmetic.generatedFromServer => generatedFromServer;

	string[] ICosmetic.dlcIds => dlcIds;

	public bool IsValidFor(string heroType)
	{
		return name.StartsWith("Skin_" + heroType.Substring(5));
	}

	public static string GetDefaultSkin(string heroType)
	{
		return "Skin_" + heroType.Substring(5) + "_Default";
	}
}

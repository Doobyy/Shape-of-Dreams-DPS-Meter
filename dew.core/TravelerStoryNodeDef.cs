using Sirenix.OdinInspector;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Name)]
[CreateAssetMenu(fileName = "TravelerStoryNodeDef", menuName = "Dew/TravelerNode/TravelerStoryNodeDef")]
public class TravelerStoryNodeDef : SerializedScriptableObject
{
	public enum NodeType
	{
		Episode = 1,
		ShortStory = 2,
		Emote = 4,
		Accessory = 8
	}

	public string heroType;

	public NodeType nodeType;

	public string titleKey;

	public string contentKey;

	public int contentChunkCount = 1;

	public int contentChunkPrice = 10;

	public Sprite[] contentImages;

	public string shortKey;

	public AssetRef<Emote> emote;

	public AssetRef<Accessory> accessory;

	public bool unlocksPolarisEnding;

	public int grantedStardust;

	public string requiredNodeId;

	public bool isUnlockedByStardust = true;

	public int requiredMasteryLevel;

	public int requiredStardust;

	public string Id => ((Object)this).name;
}

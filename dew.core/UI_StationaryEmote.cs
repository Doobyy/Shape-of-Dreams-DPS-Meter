using System;
using UnityEngine;

public class UI_StationaryEmote : MonoBehaviour
{
	public RectTransform customParent;

	[NonSerialized]
	public string currentEmoteName;

	[NonSerialized]
	public AssetRef<Emote> currentEmotePrefab;

	[NonSerialized]
	public Emote currentEmoteInstance;

	public virtual void Setup(string emoteName)
	{
		currentEmoteName = emoteName;
		if (currentEmoteInstance != null)
		{
			UnityEngine.Object.Destroy(currentEmoteInstance.gameObject);
			currentEmoteInstance = null;
		}
		if (string.IsNullOrEmpty(emoteName))
		{
			SetupEmpty();
			return;
		}
		Emote byName = DewResources.GetByName<Emote>(emoteName);
		if (byName == null)
		{
			SetupEmpty();
			return;
		}
		currentEmotePrefab = byName;
		currentEmoteInstance = UnityEngine.Object.Instantiate(byName, (customParent != null) ? customParent : transform);
		currentEmoteInstance.SetupStationary();
		currentEmoteInstance.transform.localPosition = Vector3.zero;
	}

	private void SetupEmpty()
	{
		if (currentEmoteInstance != null)
		{
			UnityEngine.Object.Destroy(currentEmoteInstance.gameObject);
			currentEmoteInstance = null;
		}
	}
}

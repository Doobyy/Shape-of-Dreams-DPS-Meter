using System;

[Serializable]
public struct AbilityAnimationReplacementPair
{
	public AssetRef<DewAnimationClip> from;

	public AssetRef<DewAnimationClip> to;
}

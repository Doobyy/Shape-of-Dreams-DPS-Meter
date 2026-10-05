using UnityEngine;

public class ModItem
{
	public ModItemState state;

	public ModSourceType source;

	public string path;

	public string description;

	public string descriptionRichText;

	public string previewPath;

	public string iconPath;

	public Texture2D icon;

	public ModMetadata metadata;

	public string[] assemblyPaths;

	public ulong publishedFileId;
}

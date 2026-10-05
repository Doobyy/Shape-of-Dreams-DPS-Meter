using System.Collections.Generic;

public class DewItemsData
{
	public List<string> encryptedItems = new List<string>();

	public List<string> redeemedInGameRewards = new List<string>();

	public void Validate()
	{
		if (encryptedItems == null)
		{
			encryptedItems = new List<string>();
		}
		if (redeemedInGameRewards == null)
		{
			redeemedInGameRewards = new List<string>();
		}
	}

	public bool ContainsUsable(DecryptedItemData data)
	{
		foreach (string encryptedItem in encryptedItems)
		{
			DecryptedItemData decryptedItemData = DewItem.GetDecryptedItemData(encryptedItem);
			if (decryptedItemData != null && decryptedItemData.item == data.item && decryptedItemData.owner == data.owner && !decryptedItemData.IsExpired())
			{
				return true;
			}
		}
		return false;
	}
}

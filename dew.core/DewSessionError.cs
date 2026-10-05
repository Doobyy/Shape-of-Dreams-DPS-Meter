using System;
using UnityEngine;

public class DewSessionError
{
	public object[] args;

	public static DewSessionError Error()
	{
		return new DewSessionError();
	}

	public static DewSessionError Error(Exception e)
	{
		DewSessionError dewSessionError = new DewSessionError();
		dewSessionError.args = new object[1] { e };
		return dewSessionError;
	}

	public static void ShowError(bool isFatal = false, bool isGame = true)
	{
		Error().Show(isFatal, isGame: true);
	}

	public static void ShowError(Exception e, bool isFatal = false, bool isGame = true)
	{
		Error(e).Show(isFatal, isGame);
	}

	private string GetTextContent(bool isGame)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		DewNetworkMode networkMode = DewNetworkManager.startSettings.networkMode;
		bool flag = networkMode == DewNetworkMode.Singleplayer || networkMode == DewNetworkMode.MultiplayerHost;
		if (args != null && args[0] is SteamException { errorCode: not null } ex && DewLocalization.TryGetUIValue($"Title_Message_SteamException_{ex.errorCode}", out var value))
		{
			return value;
		}
		if (args != null && args[0] is EOSResultException ex2 && DewLocalization.TryGetUIValue($"Title_Message_EOSResultException_{ex2.result}", out var value2))
		{
			return value2;
		}
		if (args != null && args[0] is DewException { type: DewExceptionType.NoInternetConnection })
		{
			return DewLocalization.GetUIValue("Title_Message_DewException_CannotResolveDestinationHost");
		}
		if (args != null && args[0] is DewException ex4 && DewLocalization.TryGetUIValue($"Title_Message_DewException_{ex4.type}", out var value3))
		{
			if (!string.IsNullOrEmpty(ex4.Message))
			{
				Debug.Log($"[DewSessionError] {ex4.type}: {ex4.Message}");
			}
			return value3;
		}
		if (args != null && args.Length != 0 && args[0] is Exception ex5)
		{
			string text = ex5.ToString();
			if (text.Contains("HTTP/1.1 429"))
			{
				return DewLocalization.GetUIValue("Title_Message_DewException_TooManyRequests");
			}
			if (ex5 is TimeoutException || text.Contains("Request timeout"))
			{
				return DewLocalization.GetUIValue("Title_Message_DewException_Timeout");
			}
			if (text.Contains("Cannot resolve destination host"))
			{
				return DewLocalization.GetUIValue("Title_Message_DewException_CannotResolveDestinationHost");
			}
			Debug.LogException(ex5);
		}
		string uIValue = DewLocalization.GetUIValue(flag ? "Title_Message_GameStartError" : "Title_Message_ConnectionFailed");
		if (!isGame)
		{
			uIValue = DewLocalization.GetUIValue("Title_Message_EOSResultException_UnexpectedError");
		}
		if (args != null)
		{
			object[] array = args;
			foreach (object obj in array)
			{
				if (obj is Exception exception)
				{
					Debug.LogException(exception);
				}
				else if (obj != null)
				{
					Debug.LogError($"[DewSessionError] {obj}");
				}
			}
		}
		return uIValue;
	}

	public void Show(bool isFatal, bool isGame)
	{
		string textContent = GetTextContent(isGame);
		GlobalLogicPackage.CallOnReady(() =>
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				rawContent = textContent,
				onClose = (isFatal ? ((Action<DewMessageSettings.ButtonType>)((DewMessageSettings.ButtonType _) =>
				{
					Dew.QuitApplication();
				})) : null)
			});
		});
	}
}

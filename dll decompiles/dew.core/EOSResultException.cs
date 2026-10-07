using System;
using Epic.OnlineServices;

public class EOSResultException : Exception
{
	public Result result;

	public EOSResultException(Result result)
		: base(((object)result/*cast due to constrained. prefix*/).ToString())
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		this.result = result;
	}
}

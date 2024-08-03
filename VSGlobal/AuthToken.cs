using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VSGlobal
{
	/// <exclude />
	public static class AuthToken
	{
		private static string GetModDataPath(ICoreClientAPI capi)
		{
			return capi.GetOrCreateDataPath(Path.Combine(capi.DataBasePath, "ModData", "VSGlobal"));
		}
		
		private static string GetAuthTokenPath(string modDataPath)
		{
			return Path.Combine(modDataPath, "auth.token");
		}
		
		private static void SetToken(string fullPath, Guid token)
		{
			using(StreamWriter stream = File.CreateText(fullPath))
			{
				stream.Write(token);
			};
		}
		
		private static Guid GetToken(string fullPath)
		{
			if(File.Exists(fullPath))
			{
				return Guid.Parse(File.ReadAllText(fullPath));
			}
			else
			{
				var newToken = Guid.NewGuid();
				SetToken(fullPath, newToken);
				return newToken;
			}
		}

		/// <exclude />
		public static Guid? TryGetAuthToken(ICoreClientAPI capi)
		{
			try
			{
				var modFolderPath = GetModDataPath(capi);
				var modTokenPath = GetAuthTokenPath(modFolderPath);
				
				return GetToken(modTokenPath);
			}
			catch (NullReferenceException e)
			{
				capi.Logger.Error(e.Message);
				return null;
			}
		}
	}
}
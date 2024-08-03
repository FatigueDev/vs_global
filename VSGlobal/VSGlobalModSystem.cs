using System.Text;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.Common;
using Vintagestory.Server;
using VSGlobal.Proto;

namespace VSGlobal
{
	/// <exclude />
	public class VSGlobalModSystem : ModSystem
	{
		public override double ExecuteOrder()
		{
			return 0;
		}
		
		public override void StartClientSide(ICoreClientAPI api)
		{
			VSGlobal.Internals.NetworkInternals._Initialize(api, true);			
			base.StartClientSide(api);
		}
	}
}

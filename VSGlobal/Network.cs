// using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using VSGlobal.EventArguments;
using VSGlobal.Proto;
using WebSocketSharper;

namespace VSGlobal
{
	/// <summary>
	/// Network allow you to send payloads to those who are subscribed to an endpoint (or multiple endpoints):<br/><see cref="Network.Broadcast{T}(T, string)"/><br/><see cref="Network.Subscribe(string)"/>
	/// </summary>
	public static class Network
	{
		/// <summary>
		/// Called when you want to broadcast to one of your subscribed locations.<br/>
		///	<c>You must call VSGlobal.Network.Subscribe("name_of_my_module_or_my_mod_id") prior to using Broadcast. Try not to use "core".</c><br/>
		/// If we are not connected to core, the packet will be dropped safely and the event logged.
		/// <example>
		/// Example with a custom network message:
		/// <code>
		/// 	//First, we define our network packet somewhere like so.
		/// 	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
		/// 	public class CustomNetworkMessage
		/// 	{
		/// 		public bool didSomething;
		/// 		public IClientPlayer sender;
		/// 		public string message = "Default Message";
		/// 	}
		/// 
		/// 	// Later on, in a function body ...
		/// 
		/// 	// All we have to do is call broadcast. It's generic, so you can throw _anything_ in there. string, class, struct- Whatever.
		/// 	VsGlobal.Broadcast(new CustomNetworkMessage()
		///		{
		///			didSomething = true,
		///			sender = api.World.Player,
		///			message = "Grungus"
		///		}, "broadcast", "my_module");
		/// 	// What that will do is send the packet to the server and relay it to others.
		/// 	// Once received, it'll invoke Events.OnPayloadReceived
		/// </code>
		/// </example>
		/// </summary>
		/// <seealso cref="Events.OnPayloadReceived"/>
		public static async Task Broadcast<T>(T packet, string module)
		{
			WebSocket ws = VSGlobal.Internals.NetworkInternals._GetClientWebsocket();
			
			if(ws.IsAlive == false) return;
			
			await ws.SendTaskAsync(new Payload("broadcast", module).Serialize(packet));				
		}
		
		/// <summary>
		/// Subscribe to a module to received broadcasted packets from. Call this from <see cref="Events.OnConnect"/>
		/// </summary>
		/// <param name="module">Our mod's ID, or any underscore separated string. E.g. "global_chat" or "hungry_hungry_hippo"</param>
		/// <returns></returns>
		public static async Task Subscribe(string module)
		{
			WebSocket ws = VSGlobal.Internals.NetworkInternals._GetClientWebsocket();
			
			if(ws.IsAlive == false) return;
			
			await ws.SendTaskAsync(new Payload("subscribe", module).Serialize());
		}
	}

	namespace VSGlobal.Internals
	{
		/// <exclude />
		public static class NetworkInternals
		{
			private static WebSocket webSocket;

			/// <exclude />
			public static WebSocket _GetClientWebsocket()
			{
				return webSocket;
			}

			/// <exclude />
			private static async Task _Connect()
			{
				await webSocket.ConnectTaskAsync();
			}

			/// <exclude />
			public static string _GetConnectionString(ICoreClientAPI api, Guid authToken, bool localConnect)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("?uid=");
				sb.Append(UrlEncoder.Default.Encode(api.Settings.String.Get("playeruid")));
				sb.Append("&sessionkey=");
				sb.Append(UrlEncoder.Default.Encode(api.Settings.String.Get("sessionkey")));
				sb.Append("&auth_token=");
				sb.Append(authToken);
				sb.Append("&module=");
				sb.Append(UrlEncoder.Default.Encode(_GetDefaultModuleName()));
				
				if(localConnect)
				{
					return $"ws://localhost:4000/socket/websocket{sb.ToString()}";
				}
				else
				{
					return $"ws://newnode.tallostudios.com:42490/socket/websocket{sb.ToString()}";
				}
			}

			/// <exclude />
			public static void _Initialize(ICoreClientAPI api, bool localConnect = false)
			{			
				if (AuthToken.TryGetAuthToken(api) is Guid token)
				{
					webSocket = new WebSocket(new Logger(api), _GetConnectionString(api, token, localConnect), true)
					{
						ReconnectDelay = TimeSpan.FromSeconds(15)
					};

					_SetupHandlers(api);
					
					Task.Run(async () =>
					{
						while (!api.PlayerReadyFired)
						{
							await Task.Delay(500);
						}

						_PushMainThreadTask(api, async () => await _Connect());
					});
				}
				else
				{
					api.Logger.Error("VsGlobal couldn't find your Auth Key! Contact the dev!");
					return;
				}
			}

			/// <exclude />
			public static void _SetupHandlers(ICoreClientAPI api)
			{
				webSocket.OnClose += (sender, closeEventArgs) => {				
					_PushMainThreadTask(api, () => api.ShowChatMessage($"VSG disconnect: {closeEventArgs.Reason}"));
					_PushMainThreadTask(api, ()=> api.ShowChatMessage($"Reconnecting in {webSocket.WaitTime.Seconds} seconds..."));
					Events.OnDisconnectHandler(closeEventArgs);
				};

				webSocket.OnOpen += (sender, openEventArgs) => {
					_PushMainThreadTask(api, () => api.ShowChatMessage($"You are now connected to VS Global."));
					Events.OnConnectHandler(new EventArgs());
				};

				webSocket.OnError += (sender, errorEventArgs) => {
					_PushMainThreadTask(api, () => {
						api.Logger.Error(errorEventArgs.Exception);
						api.ShowChatMessage($"VSG has encountered an error. You can find your logs in: {Path.Combine(api.DataBasePath, "Logs", "client-vsglobal.txt")}");
					});
					Events.OnErrorHandler(errorEventArgs);
				};

				webSocket.OnMessage += (sender, msg) => {
					Events.OnPayloadReceivedHandler(new OnPayloadReceivedEventArgs() {
						payload = Payload.Deserialize(msg.RawData, msg.RawData.Count())
					});
				};
			}

			/// <exclude />
			public static string _GetDefaultModuleName()
			{
				return "core";
			}

			private static void _PushMainThreadTask(ICoreClientAPI api, Action callback)
			{
				api.Event.EnqueueMainThreadTask(callback, $"vs_global:{Guid.NewGuid()}");
			}
		}
	}
}

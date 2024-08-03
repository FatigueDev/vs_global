using VSGlobal.EventArguments;
using VSGlobal.Handlers;
using VSGlobal.Proto;
using WebSocketSharper;

namespace VSGlobal
{

	namespace Handlers
	{
		using EventArguments;

		/// <exclude />
		public delegate void OnConnectHandler(EventArgs e);
		/// <exclude />
		public delegate void OnDisconnectHandler(CloseEventArgs e);
		/// <exclude />
		public delegate void OnErrorHandler(WebSocketSharper.ErrorEventArgs e);
		/// <exclude />
		public delegate void OnPayloadReceivedHandler(OnPayloadReceivedEventArgs e);
	}

	namespace EventArguments
	{
		/// <summary>
		/// Provides <see cref="Payload"/>
		/// </summary>
		public class OnPayloadReceivedEventArgs : EventArgs
		{
			public required Payload payload;
		}
	}

	/// <summary>
	/// Events allow you to add callbacks to VSGlobal's events:<br/><see cref="Events.OnConnect"/><br/><see cref="Events.OnDisconnect"/><br/><see cref="Events.OnError"/><br/><see cref="Events.OnPayloadReceived"/>
	/// </summary>
	/// <seealso cref="EventArguments"/>
	public static class Events
	{
		/// <summary>
		/// Invoked when VSGlobal has connected
		/// <example>
		/// Using a lambda:
		/// <code>
		/// 	VSGlobal.Events.OnConnect += (e) =>
		/// 	{
		/// 		// Do stuff on connect
		///			VSGlobal.Subscribe("my_module");
		///			VSGlobal.Broadcast("Hello World!", "my_module"); //&gt; OnPayloadReceived
		/// 	};
		/// </code>
		/// Using a function:
		/// <code>
		/// 	public void MyCoolOnConnect(EventArgs e)
		/// 	{
		/// 		// Do stuff on connect
		/// 	}
		/// 	
		/// 	// Then later, in a function body somewhere we register the handler.
		/// 	VSGlobal.Events.OnConnect += MyCoolOnConnect;
		/// </code>
		/// </example>
		/// </summary>
		public static event OnConnectHandler OnConnect;

		/// <summary>
		/// Invoked when VsGlobal has disconnected (banned, server issue, skill issue)<br/>
		/// <example>
		/// Using a lambda:
		/// <code>
		/// 	Events.OnDisconnect += (e) =>
		/// 	{
		/// 		// Do stuff on disconnect
		/// 	};
		/// </code>
		/// Using a function:
		/// <code>
		/// 	public void MyCoolOnDisconnect(EventArgs e)
		/// 	{
		/// 		// Do stuff on disconnect     
		/// 	}
		/// 	
		/// 	// Then later, in a function body somewhere we register the handler.
		/// 	Events.OnDisconnect += MyCoolOnDisconnect;
		/// </code>
		/// </example>
		/// </summary>
		public static event OnDisconnectHandler OnDisconnect;

		/// <summary>
		/// <code>
		/// 	Events.OnError += (e) =>
		/// 	{
		/// 		// Do stuff on disconnect
		///			Console.WriteLine(e.Exception);
    	///			Console.WriteLine(e.Message);
		///  	};
		/// </code>
		/// Using a function:
		/// <code>
		/// 	public void MyCoolOnDisconnect(WebSocketSharper.ErrorEventArgs e)
		/// 	{
		/// 		// Do stuff on disconnect
		/// 	}
		/// 
		/// 	// Then later, in a function body somewhere we register the handler.
		/// 	Events.OnError += MyCoolOnDisconnect;
		/// </code>
		/// </summary>
		public static event OnErrorHandler OnError;

		/// <summary>
		/// Invoked when VsGlobal receives a payload<br/>
		/// <example>
		/// Using a lambda:
		/// <code>
		/// 	Events.OnPayloadReceived += (e) =>
		/// 	{
		/// 		// This will be called whenever a packet arrives, regardless of module or sender.
		/// 		if(e.payload.Module == "my_module_name")
		/// 		{
		/// 			// Now that we know the payload is for our module, we can try converting it to our expected types.
		/// 			MyCustomClass? myCustomThing = e.payload.DeserializePacket&lt;MyCustomClass&gt;();
		/// 			if(myCustomThing is MyCustomClass packet)
		/// 			{
		/// 				DoSomething(myCustomThing.value);
		/// 			}
		/// 		}
		/// 		else
		/// 		{
		/// 			// It's someone else's packet. Could be handy for extension mods!
		/// 		}
		/// 	};
		/// </code>
		/// Using a function:
		/// <code>
		/// 	public void ReceiveMessagePacket(OnPayloadReceivedEventArgs e)
		/// 	{
		/// 		// Same as the lambda, we have access to any payload coming in here.
		/// 		Message? maybeMessage = e.payload.DeserializePacket&lt;Message&gt;();
		/// 		
		/// 		// We can also be quite cheeky and attempt to deserialize it to our custom type regardless of module.
		/// 		// If it doesn't, it's not ours- So I suppose that's valid as well.
		/// 		if(maybeMessage is Message msg)
		/// 		{
		/// 			// Do something with our received custom message!
		/// 		}
		/// 	}
		/// 	
		/// 	// Ideally, within `public override void StartClientside(ICoreClientAPI)`
		/// 	Events.OnPayloadReceived += ReceiveMessagePacket;
		/// </code>
		/// </example>
		/// </summary>
		/// <seealso cref="OnPayloadReceivedEventArgs"/>
		public static event OnPayloadReceivedHandler OnPayloadReceived;

		/// <exclude />
		public static void OnConnectHandler(EventArgs e)
		{
			OnConnectHandler handler = OnConnect;
			if (handler != null)
			{
				handler(e);
			}
		}

		/// <exclude />
		public static void OnDisconnectHandler(CloseEventArgs e)
		{
			OnDisconnectHandler handler = OnDisconnect;
			if (handler != null)
			{
				handler(e);
			}
		}

		/// <exclude />
		public static void OnErrorHandler(WebSocketSharper.ErrorEventArgs e)
		{
			OnErrorHandler handler = OnError;
			if (handler != null)
			{
				handler(e);
			}
		}

		/// <exclude />
		public static void OnPayloadReceivedHandler(OnPayloadReceivedEventArgs e)
		{
			OnPayloadReceivedHandler handler = OnPayloadReceived;
			if (handler != null)
			{
				handler.Invoke(e);
			}
		}
	}
}
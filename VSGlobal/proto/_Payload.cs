#region Payload
#pragma warning disable CS8618
using ProtoBuf;
using Vintagestory.API.Util;

namespace VSGlobal.Proto
{
	[ProtoContract()]
	public partial class Payload
	{
		[ProtoMember(1)]
		[System.ComponentModel.DefaultValue("")]
		public string Module { get; set; }

		[ProtoMember(2)]
		[System.ComponentModel.DefaultValue(false)]
		public string Event { get; set; }

		[ProtoMember(3)]
		[System.ComponentModel.DefaultValue("")]
		public string PacketType { get; set; } = "";

		[ProtoMember(4)]
		public byte[] PacketValue { get; set; }

		public Payload() : this("", "") { }
		public Payload(string triggerEvent) : this(triggerEvent, "") { }
		public Payload(string triggerEvent, string module) { Module = module; Event = triggerEvent; }

		public static Payload Deserialize(byte[] buffer, int responseSize)
		{
			using (var receivedPacketStream = new MemoryStream(buffer, 0, responseSize))
			{
				return Serializer.Deserialize<Payload>(receivedPacketStream);
			}
		}
	}

	public static class PayloadExtensionMethods
	{
		public static byte[] Serialize<T>(this Payload payload, T packetValue)
		{
			using (var payloadStream = new MemoryStream())
			{
				Serializer.Serialize(payloadStream, packetValue);
				payload.PacketValue = payloadStream.ToArray();

				payloadStream.Clear();

				payload.PacketType = typeof(T).FullName ?? "ERROR; Type was null";

				Serializer.Serialize(payloadStream, payload);
				return payloadStream.ToArray();
			}
		}

		public static byte[] Serialize(this Payload payload)
		{
			using (var payloadStream = new MemoryStream())
			{
				payload.PacketValue = [];
				payload.PacketType = typeof(byte[]).FullName ?? "ERROR; Type was null";
				Serializer.Serialize(payloadStream, payload);
				return payloadStream.ToArray();
			}
		}

		public static T? DeserializePacket<T>(this Payload payload)
		{
			Type? packetType = Type.GetType(payload.PacketType);
			if (packetType == typeof(T))
			{
				using (var packetValueStream = new MemoryStream(payload.PacketValue))
				{
					return Serializer.Deserialize<T>(packetValueStream);
				}
			}
			else
			{
				return default;
			}
		}
	}
}

#pragma warning restore CS8618
#endregion

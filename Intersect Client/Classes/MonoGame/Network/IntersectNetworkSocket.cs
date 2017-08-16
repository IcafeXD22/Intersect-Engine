﻿using System;
using System.Diagnostics;
using System.Reflection;
using Intersect.Logging;
using Intersect.Network;
using Intersect.Network.Crypto;
using Intersect.Network.Crypto.Formats;
using Intersect.Network.Packets.Reflectable;
using IntersectClientExtras.Network;
using Intersect_Client.Classes.General;
using Intersect_Client.Classes.Networking;

namespace Intersect.Client.Classes.MonoGame.Network
{
    public class IntersectNetworkSocket : GameSocket
    {
        public static ClientNetwork ClientLidgrenNetwork;

        public IntersectNetworkSocket()
        {
        }

        public override void Connect(string host, int port)
        {
            if (ClientLidgrenNetwork == null)
            {
                Log.Global.AddOutput(new ConsoleOutput());
                var config = new NetworkConfiguration(Globals.Database.ServerHost,
                    (ushort) Globals.Database.ServerPort);
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("Intersect.Client.public-intersect.bek"))
                {
                    var rsaKey = EncryptionKey.FromStream<RsaKey>(stream);
                    Debug.Assert(rsaKey != null, "rsaKey != null");
                    ClientLidgrenNetwork = new ClientNetwork(config, rsaKey.Parameters);
                }

                if (ClientLidgrenNetwork != null)
                {
                    ClientLidgrenNetwork.Handlers[PacketCode.BinaryPacket] = PacketHandler.HandlePacket;
                    if (!ClientLidgrenNetwork.Connect())
                    {
                        Log.Error("An error occurred while attempting to connect.");
                    }
                }
            }
        }

        public override void SendData(byte[] data)
        {
            if (ClientLidgrenNetwork != null)
            {
                var buffer = new ByteBuffer();
                buffer.WriteBytes(data);
                if (!ClientLidgrenNetwork.Send(new BinaryPacket(null) {Buffer = buffer}))
                {
                    throw new Exception("Beta 4 network send failed.");
                }
            }
        }

        public override void Update()
        {
        }

        public override void Disconnect(string reason)
        {
            ClientLidgrenNetwork?.Disconnect(reason);
        }

        public override void Dispose()
        {
            ClientLidgrenNetwork?.Dispose();
            ClientLidgrenNetwork = null;
        }

        public override bool IsConnected()
        {
            return ClientLidgrenNetwork.IsConnected;
        }

        public override int Ping()
        {
            return ClientLidgrenNetwork?.Ping ?? -1;
        }
    }
}
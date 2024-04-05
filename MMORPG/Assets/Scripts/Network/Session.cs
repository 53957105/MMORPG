using Common.Network;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tool;
using UnityEngine.SocialPlatforms;

namespace Network
{
    public class Session : Connection
    {
        public Session(Socket socket) : base(socket)
        {
            ConnectionClosed += OnConnectionClosed;
            ErrorOccur += OnErrorOccur;
            PacketReceived += OnPacketReceived;
        }

        //TODO ��ˮλ����
        private List<Packet> _receivedPackets = new List<Packet>();
        private TaskCompletionSource<bool> _receivedPacketTSC = new TaskCompletionSource<bool>();

        public async Task<T> ReceiveAsync<T>() where T : class, Google.Protobuf.IMessage
        {
            while (true)
            {
                await _receivedPacketTSC.Task;
                _receivedPacketTSC = new TaskCompletionSource<bool>();
                Packet? packet;
                lock (_receivedPackets)
                {
                    packet = _receivedPackets.Find(packet => { return packet.Message.GetType() == typeof(T); });
                    if (packet != null)
                        _receivedPackets.Remove(packet);
                    else
                        continue;
                }
                var res = packet.Message as T;
                Debug.Assert(res != null);
                return res;
            }
        }

        private void OnPacketReceived(Connection sender, PacketReceivedEventArgs e)
        {
            Global.Logger.Info($"[Channel] �������Է������˵����ݰ�:{e.Packet.Message.GetType()}");
            lock (_receivedPackets)
            {
                _receivedPackets.Add(e.Packet);
            }
            _receivedPacketTSC.TrySetResult(true);
        }

        private void OnErrorOccur(Connection sender, ErrorOccurEventArgs e)
        {
            Global.Logger.Error($"[Channel] �����쳣:{e.Exception}");
        }

        private void OnConnectionClosed(Connection sender, ConnectionClosedEventArgs e)
        {
            if (e.IsManual)
            {
                Global.Logger.Info($"[Channel] �رնԷ������˵�����!");
            }
            else
            {
                Global.Logger.Info($"[Channel] �Զ˹ر�����");
            }
        }
    }
}
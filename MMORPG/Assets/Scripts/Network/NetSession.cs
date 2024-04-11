using Common.Network;
using Common.Tool;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

public class SuddenPacketReceivedEventArgs
{
    public Packet Packet { get; }

    public SuddenPacketReceivedEventArgs(Packet packet)
    {
        Packet = packet;
    }
}

public class NetSession : Connection
{
    /// <summary>
    /// ͻ����Ϣ����
    /// </summary>
    public event EventHandler<SuddenPacketReceivedEventArgs>? SuddenPacketReceived;

    public NetSession(Socket socket) : base(socket)
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

    private void OnPacketReceived(object? sender, PacketReceivedEventArgs e)
    {
        Global.Logger.Debug($"[Channel] �������Է������˵����ݰ�:{e.Packet.Message.GetType()}");
        if (ProtoManager.Instance.IsEmergency(e.Packet.Message.GetType()))
        {
            SuddenPacketReceived?.Invoke(this, new SuddenPacketReceivedEventArgs(e.Packet));
            return;
        }
        lock (_receivedPackets)
        {
            _receivedPackets.Add(e.Packet);
        }
        _receivedPacketTSC.TrySetResult(true);
    }

    private void OnErrorOccur(object? sender, ErrorOccurEventArgs e)
    {
        Global.Logger.Error($"[Channel] �����쳣:{e.Exception}");
    }

    private void OnConnectionClosed(object? sender, ConnectionClosedEventArgs e)
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
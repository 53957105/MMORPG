using Common.Network;
using Common.Proto.Player;
using Common.Tool;
using Google.Protobuf;
using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace MMORPG
{
    public interface INetworkSystem : ISystem
    {
        public IUnRegister ReceiveEvent<TMessage>(Action<TMessage> onReceived) where TMessage : class, IMessage;

        public Task ConnectAsync();
        public void Close();

        public void SendToServer(IMessage msg);
        public Task<T> ReceiveAsync<T>() where T : class, IMessage;
        public Task StartAsync();
    }

    public class NetworkSystem : AbstractSystem, INetworkSystem
    {
        private NetSession _session;
        private Dictionary<Type, Delegate> _eventMsgHandlers = new();

        ////TODO ��ˮλ����
        private LinkedList<IMessage> _messageList = new();

        public async Task<T> ReceiveAsync<T>() where T : class, IMessage
        {
            while (true)
            {
                IMessage msg = null;
                lock (_messageList)
                {
                    var node = _messageList.FindIf(msg => { return msg.GetType() == typeof(T); });
                    if (node != null)
                    {
                        msg = node.Value;
                        //UnityEngine.Debug.Log(typeof(T));
                        _messageList.Remove(node);
                    }
                }
                if (msg == null)
                {
                    await Task.Delay(100);
                    continue;
                }
                var res = msg as T;
                Debug.Assert(res != null);
                return res;
            }
        }

        public void SendToServer(IMessage msg)
        {
            _session.Send(msg);
        }

        public Task StartAsync()
        {
            _session.PacketReceived += OnPacketReceived;
            return _session.StartAsync();
        }

        private void OnPacketReceived(object sender, PacketReceivedEventArgs e)
        {
            var msgType = e.Packet.Message.GetType();
            if (ProtoManager.IsEvent(msgType))
            {
                _eventMsgHandlers[msgType]?.DynamicInvoke(new object[] { e.Packet.Message });
            }
            else
            {
                _messageList.AddLast(e.Packet.Message);
            }
        }

        public async Task ConnectAsync()
        {
            Socket socket;
            var box = this.GetSystem<IBoxSystem>();
            while (true)
            {
                // ��ʾ��ת���ؿ�
                box.ShowSpinner("���ӷ�������......");
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await socket.ConnectAsync(NetConfig.ServerIpAddress, NetConfig.ServerPort);
                    box.CloseSpinner();
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error("Network", ex, $"���ӷ�����ʱ���ִ���:{ex.Message}");
                    box.CloseSpinner();
                    await box.ShowMessageAsync("����", $"���ӷ�����ʧ��:{ex}", "��������");
                    continue;
                }
            }
            _session = new NetSession(socket);
        }

        protected override void OnInit()
        {
        }

        IUnRegister INetworkSystem.ReceiveEvent<TMessage>(Action<TMessage> onReceived)
        {
            var type = typeof(TMessage);
            if (!_eventMsgHandlers.ContainsKey(type))
            {
                _eventMsgHandlers[type] = null;
            }
            _eventMsgHandlers[type] = (_eventMsgHandlers[type] as Action<TMessage>) + onReceived;
            return new CustomUnRegister(() => UnReceiveEvent(onReceived));
        }

        private void UnReceiveEvent<TMessage>(Action<TMessage> onReceived)
        {
            var type = typeof(TMessage);
            Debug.Assert(_eventMsgHandlers.ContainsKey(type));
            _eventMsgHandlers[type] = (_eventMsgHandlers[type] as Action<TMessage>) - onReceived;
        }

        public void Close()
        {
            _session.Close();
        }
    }
}
using Common.Network;
using Common.Tool;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class GameClient : Singleton<GameClient>
{
    private Session _session;

    public Session Session => _session;

    public async Task ConnectAsync() {
        while (true)
        {
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(NetConfig.ServerIpAddress, NetConfig.ServerPort);
                Debug.Log("���ӵ�������");
                _session = new Session(socket);
                await _session.StartAsync();
                break;
            }
            catch (Exception e)
            {
                await MessageBox.ShowInfoAsync($"����������ʧ��!\nԭ��:{e.Message}", buttonText:"��������");
                continue;
            }
        }
    }
    public bool Connected()
    {
        return _session != null;
    }
}

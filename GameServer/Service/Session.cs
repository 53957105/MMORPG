﻿using Common.Network;
using GameServer.Tool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Service
{
    public class Session : Connection
    {
        //TODO 可读性更高的SessionName
        public string SessionName => _socket.RemoteEndPoint.ToString();

        public Session(Socket socket) : base(socket)
        {
            ConnectionClosed += OnConnectionClosed;
            ErrorOccur += OnErrorOccur;
            HighWaterMark += OnHighWaterMark;
            SuccessSent += OnSuccessSent;
            PacketReceived += OnPacketReceived;
        }

        private void OnPacketReceived(object? sender, PacketReceivedEventArgs e)
        {
            Global.Logger.Info($"[Session] 成功接收来自{SessionName}的数据包");
        }

        private void OnSuccessSent(object? sender, SuccessSentEventArgs e)
        {
            Global.Logger.Info($"[Session] 成功发送数据包给{SessionName}");
        }

        private void OnHighWaterMark(object? sender, HighWaterMarkEventArgs e)
        {
            Global.Logger.Error($"[Session] {SessionName}的发送队列超出最高水位!");
        }

        private void OnErrorOccur(object? sender, ErrorOccurEventArgs e)
        {
            Global.Logger.Error($"[Session] {SessionName}出现异常:{e.Exception}");
        }

        private void OnConnectionClosed(object? sender, ConnectionClosedEventArgs e)
        {
            if (e.IsManual)
            {
                Global.Logger.Info($"[Session] 成功关闭对{SessionName}的链接!");
            }
            else
            {
                Global.Logger.Info($"[Session] {SessionName}对端关闭链接");
            }
        }
    }
}
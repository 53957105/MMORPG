using Common.Proto.Base;
using Common.Proto.Player;
using Common.Tool;
using MMORPG;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoginCommand : AbstractCommand
{
    private string _username;
    private string _password;

    public LoginCommand(string username, string password)
    {
        _username = username;
        _password = password;
    }

    protected async override void OnExecute()
    {
        var box = this.GetSystem<IBoxSystem>();
        if (_username.Length < 4 || _username.Length > 12)
        {
            box.ShowNotification("�û������ȱ�����4-12��֮��!");
            return;
        }
        if (_password.Length < 8 || _password.Length > 16)
        {
            box.ShowNotification("���볤�ȱ�����8-16��֮��!");
            return;
        }

        box.ShowSpinner("��¼��......");
        var networkSys = this.GetSystem<INetworkSystem>();
        networkSys.SendToServer(new LoginRequest
        {
            Username = _username,
            Password = _password
        });
        var response = await networkSys.ReceiveAsync<LoginResponse>();
        box.CloseSpinner();

        if (response.Error == NetError.Success)
        {
            //SceneHelper.SwitchScene("EnterScene");
        }
        else
        {
            box.ShowMessage($"��¼ʧ��!\nԭ��:{response.Error.GetInfo().Description}");
        }
    }
}
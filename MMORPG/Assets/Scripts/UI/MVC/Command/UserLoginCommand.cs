using Common.Proto.Base;
using Common.Proto.Player;
using Common.Tool;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UserLoginCommand : AbstractCommand
{
    protected async override void OnExecute()
    {
        var model = this.GetModel<UserLoginModel>();

        if (model.LoginUsername.Length < 4 || model.LoginUsername.Length > 12)
        {
            SceneHelper.CreateNotificationBox(new() { Description = "�û������ȱ�����4-12��֮��!" });
            return;
        }
        if (model.LoginPassword.Length < 8 || model.LoginPassword.Length > 16)
        {
            SceneHelper.CreateNotificationBox(new() { Description = "���볤�ȱ�����8-16��֮��!" });
            return;
        }

        var request = new LoginRequest
        {
            Username = model.LoginUsername,
            Password = model.LoginPassword
        };
        SceneHelper.BeginSpinnerBox(new() { Description = "��¼��......" });
        NetClient.Session.Send(request);
        var response = await NetClient.Session.ReceiveAsync<LoginResponse>();
        SceneHelper.EndSpinnerBox();

        if (response.Error == NetError.Success)
        {
            SceneHelper.SwitchScene("EnterScene");
        }
        else
        {
            SceneHelper.ShowMessageBox(new()
            {
                Description = $"��¼ʧ��!\nԭ��:{response.Error.GetInfo().Description}"
            });
        }
    }
}
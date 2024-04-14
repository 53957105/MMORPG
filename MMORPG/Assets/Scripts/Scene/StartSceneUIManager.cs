using Common.Network;
using Common.Proto;
using Common.Proto.Base;
using Common.Proto.Player;
using Common.Tool;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class StartSceneUIManager : MonoSingleton<StartSceneUIManager>
{
    public GameObject AccountPanel;
    public GameObject LoginPanel;
    public GameObject RegisterPanel;

    public TMP_InputField LoginUsernameInput;
    public TMP_InputField LoginPasswordInput;
    public TMP_InputField RegisterUsernameInput;
    public TMP_InputField RegisterPasswordInput;
    public TMP_InputField RegisterVeriftyPasswordInput;

    async void Start()
    {
        AccountPanel.SetActive(true);
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(false);
        await ConnectServer();
    }

    public void ShowLogin()
    {
        PanelSwitcher.FadeOut(AccountPanel);
        PanelSwitcher.FadeIn(LoginPanel);
    }

    public void CloseLogin()
    {
        PanelSwitcher.FadeIn(AccountPanel);
        PanelSwitcher.FadeOut(LoginPanel);
    }

    public void ShowRegister()
    {
        PanelSwitcher.FadeOut(AccountPanel);
        PanelSwitcher.FadeIn(RegisterPanel);
    }

    public void CloseRegister()
    {
        PanelSwitcher.FadeIn(AccountPanel);
        PanelSwitcher.FadeOut(RegisterPanel);
    }

    public async Task ConnectServer()
    {
        Socket socket;
        while (true)
        {
            // ��ʾ��ת���ؿ�
            SceneHelper.BeginSpinnerBox(new SpinnerBoxConfig() { Description = "���ӷ�������......" });
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(NetConfig.ServerIpAddress, NetConfig.ServerPort);
                SceneHelper.EndSpinnerBox();
                break;
            }
            catch (System.Exception ex)
            {
                SceneHelper.EndSpinnerBox();
                // ��ʾ��Ϣ��
                await SceneHelper.ShowMessageBoxAsync(new MessageBoxConfig()
                {
                    Title = "����",
                    Description = $"���ӷ�����ʧ��:{ex}",
                    ConfirmButtonText = "��������",
                });
                continue;
            }
        }
        // ��ʼ�¼�ѭ��
        await NetClient.StartSessionAsync(socket);
    }

    public async void DoLogin()
    {
        if (LoginUsernameInput.text.Length < 4 || LoginUsernameInput.text.Length > 12)
        {
            SceneHelper.CreateNotificationBox(new() { Description = "�û������ȱ�����4-12��֮��!" });
            return;
        }
        if (LoginPasswordInput.text.Length < 8 || LoginPasswordInput.text.Length > 16)
        {
            SceneHelper.CreateNotificationBox(new() { Description = "���볤�ȱ�����8-16��֮��!" });
            return;
        }

        var request = new LoginRequest
        {
            Username = LoginUsernameInput.text,
            Password = LoginPasswordInput.text
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

    public async void DoRegister()
    {
        if (RegisterUsernameInput.text.Length < 4 || RegisterUsernameInput.text.Length > 12)
        {
            SceneHelper.CreateNotificationBox(new() { Description = "�û������ȱ�����4-12��֮��!" });
            return;
        }
        if (RegisterPasswordInput.text.Length < 8 || RegisterPasswordInput.text.Length > 16)
        {
            SceneHelper.CreateNotificationBox(new() { Description = "���볤�ȱ�����8-16��֮��!" });
            return;
        }
        if (RegisterPasswordInput.text != RegisterVeriftyPasswordInput.text)
        {
            SceneHelper.CreateNotificationBox(new() { Description = "�����������벻��ͬ!" });
            return;
        }

        var request = new RegisterRequest
        {
            Username = RegisterUsernameInput.text,
            Password = RegisterPasswordInput.text
        };
        SceneHelper.BeginSpinnerBox(new() { Description = "ע����......" });
        NetClient.Session.Send(request);
        var response = await NetClient.Session.ReceiveAsync<RegisterResponse>();
        SceneHelper.EndSpinnerBox();

        if (response.Error == NetError.Success)
        {
            SceneHelper.ShowMessageBox(new()
            {
                Description = $"ע��ɹ�!"
            });
        }
        else
        {
            SceneHelper.ShowMessageBox(new()
            {
                Description = $"ע��ʧ��!\nԭ��:{response.Error.GetInfo().Description}"
            });
        }
    }
}

using Common.Network;
using Common.Proto.Base;
using Common.Proto.Player;
using Common.Tool;
using System.Net.Sockets;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class StartService : MonoBehaviour
{
    public TMP_InputField LoginUsernameInput;
    public TMP_InputField LoginPasswordInput;
    public TMP_InputField RegisterUsernameInput;
    public TMP_InputField RegisterPasswordInput;
    public TMP_InputField RegisterVeriftyPasswordInput;

    private async void Start()
    {
        await ConnectServer();
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
        NetClient.Session.Send(request);
        var response = await NetClient.Session.ReceiveAsync<LoginResponse>();
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
        NetClient.Session.Send(request);
        var response = await NetClient.Session.ReceiveAsync<RegisterResponse>();
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
using Common.Proto.Base;
using Common.Proto.Player;
using Common.Tool;
using TMPro;
using UnityEngine;

namespace Serivce {
    public class PlayerService : MonoSingleton<PlayerService>
    {
        public TMP_InputField LoginUsernameInput;
        public TMP_InputField LoginPasswordInput;
        public TMP_InputField RegisterUsernameInput;
        public TMP_InputField RegisterPasswordInput;
        public TMP_InputField RegisterVeriftyPasswordInput;

        public async void TryLogin()
        {
            if (LoginUsernameInput.text.Length < 4 || LoginUsernameInput.text.Length > 12)
            {
                SceneManager.Instance.CreateNotificationBox(new() { Description = "�û������ȱ�����4-12��֮��!" });
                return;
            }
            if (LoginPasswordInput.text.Length < 8 || LoginPasswordInput.text.Length > 16)
            {
                SceneManager.Instance.CreateNotificationBox(new() { Description = "���볤�ȱ�����8-16��֮��!" });
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
                SceneManager.Instance.SwitchScene("EnterScene");
            }
            else
            {
                SceneManager.Instance.ShowMessageBox(new()
                {
                    Description = $"��¼ʧ��!\nԭ��:{response.Error.GetInfo().Description}"
                });
            }
        }

        public async void TryRegister()
        {
            if (RegisterUsernameInput.text.Length < 4 || RegisterUsernameInput.text.Length > 12)
            {
                SceneManager.Instance.CreateNotificationBox(new() { Description = "�û������ȱ�����4-12��֮��!" });
                return;
            }
            if (RegisterPasswordInput.text.Length < 8 || RegisterPasswordInput.text.Length > 16)
            {
                SceneManager.Instance.CreateNotificationBox(new() { Description = "���볤�ȱ�����8-16��֮��!" });
                return;
            }
            if (RegisterPasswordInput.text != RegisterVeriftyPasswordInput.text)
            {
                SceneManager.Instance.CreateNotificationBox(new() { Description = "�����������벻��ͬ!" });
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
                SceneManager.Instance.ShowMessageBox(new()
                {
                    Description = $"ע��ɹ�!"
                });
            }
            else
            {
                SceneManager.Instance.ShowMessageBox(new()
                {
                    Description = $"ע��ʧ��!\nԭ��:{response.Error.GetInfo().Description}"
                });
            }
        }

        public async void TryEnterGame()
        {
            SceneManager.Instance.SwitchScene("GameScene");
        }
    }
}
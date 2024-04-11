using Common.Proto.Base;
using Common.Proto.Player;
using Common.Tool;
using TMPro;
using UnityEngine;

namespace Serivce {
    public class UserService : MonoSingleton<UserService>
    {
        public TMP_InputField LoginUsernameInput;
        public TMP_InputField LoginPasswordInput;
        public TMP_InputField RegisterUsernameInput;
        public TMP_InputField RegisterPasswordInput;
        public TMP_InputField RegisterVeriftyPasswordInput;

        public async void TryLogin()
        {
            //TODO �˺�����淶���
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

            var loginRequest = new LoginRequest
            {
                Username = LoginUsernameInput.text,
                Password = LoginPasswordInput.text
            };
            NetClient.Session.Send(loginRequest);
            var response = await NetClient.Session.ReceiveAsync<LoginResponse>();
            if (response.Error == NetError.Success)
            {
            }
            else
            {
                SceneManager.Instance.ShowMessageBox(new()
                {
                    Description = $"��¼ʧ��!\nԭ��:{response.Error.GetInfo().Description}"
                });
            }
        }
        public void TryRegister()
        {
            //TODO �˺�����淶���
            //GameClient.Instance.Session.SendAsync(new UserRegisterRequest()
            //{
            //    Username = RegisterUsernameInput.text,
            //    Password = RegisterPasswordInput.text,
            //});
        }
    }
}
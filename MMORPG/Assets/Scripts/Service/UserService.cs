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
                SceneManager.Instance.CreateNotificationBox(new NotificationBoxConfig() { Description = "�û������ȱ�����4-12��֮��!" });
                return;
            }
            if (LoginPasswordInput.text.Length < 8 || LoginPasswordInput.text.Length > 16)
            {
                SceneManager.Instance.CreateNotificationBox(new NotificationBoxConfig() { Description = "���볤�ȱ�����8-16��֮��!" });
                return;
            }

            GameClient.Instance.Session.SendAsync(new UserLoginRequest()
            {
                Username = LoginUsernameInput.text,
                Password = LoginPasswordInput.text,
            }, null);

            var res = await GameClient.Instance.Session.ReceiveAsync<UserLoginResponse>();
            if (res.Status == LoginStatus.Error)
            {
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
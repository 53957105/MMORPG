using Common.Proto;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class StartManager : MonoSingleton<StartManager>
{
    public GameObject AccountPanel;
    public GameObject LoginPanel;
    public GameObject RegisterPanel;

    void Start()
    {
        AccountPanel.SetActive(true);
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(false);
        ConnectServer();
    }

    public void ConnectServer()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                SceneManager.Instance.BeginSpinnerBox(new SpinnerBoxConfig() { Description = "���ӷ�������......" });
                try
                {
                    await GameClient.Instance.ConnectAsync();
                    SceneManager.Instance.EndSpinnerBox();
                    break;
                }
                catch (System.Exception ex)
                {
                    SceneManager.Instance.EndSpinnerBox();
                    await SceneManager.Instance.ShowMessageBoxAsync(new MessageBoxConfig()
                    {
                        Title = "����",
                        Description = $"���ӷ�����ʧ��:{ex}",
                        ConfirmButtonText = "��������",
                    });
                    continue;
                }
            }
            //TODO �쳣����
            await GameClient.Instance.StartAsync();
        });
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

}

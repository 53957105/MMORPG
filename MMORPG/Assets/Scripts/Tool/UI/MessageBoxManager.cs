using Michsky.MUIP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum MessageBoxStyle
{
    LongDesc,   // ������ʾ����Ϣ
    ShortDesc   // ������ʾ����Ϣ, �ʺ�ֻ�������ı���
}

public enum MessageBoxResult
{
    Confirm,    // ȷ��
    Cancel      // ȡ��
}

/// <summary>
/// ��Ϣ�������
/// </summary>
public class MessageBoxConfig
{
    public string Title = "��ʾ";
    public string Description = "����һ����Ϣ��";
    public string ConfirmButtonText = "ȷ��";
    public string CancalButtonText = "ȡ��";
    public bool ShowConfirmButton = true;
    public bool ShowCancalButton = false;
    public Action<MessageBoxResult> OnChose;    // ���û�ѡ����"ȷ��"��"ȡ��"������һ����ť
    public Action OnOpen;
    public MessageBoxStyle Style = MessageBoxStyle.LongDesc;
}

public class MessageBoxManager : MonoBehaviour
{
    public ModalWindowManager LongDescModalWindow;
    public ModalWindowManager ShortDescModalWindow;

    public bool IsShowing => GetWindow().isOn;

    public MessageBoxConfig Config { get; set; }

    private void Awake()
    {
        LongDescModalWindow.confirmButton.onClick.AddListener(() => Config.OnChose?.Invoke(MessageBoxResult.Confirm));
        LongDescModalWindow.cancelButton.onClick.AddListener(() => Config.OnChose?.Invoke(MessageBoxResult.Cancel));
        LongDescModalWindow.onOpen.AddListener(() => Config.OnOpen?.Invoke());

        ShortDescModalWindow.confirmButton.onClick.AddListener(() => Config.OnChose?.Invoke(MessageBoxResult.Confirm));
        ShortDescModalWindow.cancelButton.onClick.AddListener(() => Config.OnChose?.Invoke(MessageBoxResult.Cancel));
        ShortDescModalWindow.onOpen.AddListener(() => Config.OnOpen?.Invoke());
    }

    public void Show()
    {
        if (IsShowing)
            throw new Exception("��ǰ����MessageBox������ʾ!");

        var window = GetWindow();
        window.titleText = Config.Title;
        window.descriptionText = Config.Description;
        window.confirmButton.buttonText = Config.ConfirmButtonText;
        window.cancelButton.buttonText = Config.CancalButtonText;
        window.showConfirmButton = Config.ShowConfirmButton;
        window.showCancelButton = Config.ShowCancalButton;

        window.cancelButton.UpdateUI();
        window.confirmButton.UpdateUI();
        window.UpdateUI();
        window.Open();
    }

    private ModalWindowManager GetWindow() => Config.Style switch
    {
        MessageBoxStyle.LongDesc => LongDescModalWindow,
        MessageBoxStyle.ShortDesc => ShortDescModalWindow,
        _ => throw new NotImplementedException()
    };
}
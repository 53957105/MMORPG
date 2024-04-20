using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// ��ת���ؿ������
/// </summary>
public record SpinnerBoxConfig
{
    public string Description = "����һ����ת�������";
    public float DescriptionFontSize = 16;
}

public class SpinnerBoxManager : MonoBehaviour
{
    public GameObject SpinnerBox;
    public TextMeshProUGUI DescriptionText;

    public SpinnerBoxConfig Config { get; set; }

    public bool IsShowing { get; private set; }

    private void Start()
    {
        SpinnerBox.SetActive(false);
    }

    public void Show()
    {
        if (IsShowing)
        {
            Debug.LogWarning("��ǰSpinnerBox������ʾ!");
            return;
        }
        DescriptionText.text = Config.Description;
        DescriptionText.fontSize = Config.DescriptionFontSize;
        IsShowing = true;
        PanelHelper.FadeIn(SpinnerBox);
    }

    public void Close()
    {
        Debug.Assert(IsShowing);
        PanelHelper.FadeOut(SpinnerBox);
        IsShowing = false;
    }
}

using System;
using UnityEngine;
using UnityEngine.Playables;

public enum NotificationBoxPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public enum NotificationBoxStyle
{
    Fading,
    Popup,
    Sliding
}

public record NotificationBoxConfig
{
    public string Title = "֪ͨ";
    public string Description = "����һ��֪ͨ��";
    public NotificationBoxPosition Position = NotificationBoxPosition.TopRight;
    public NotificationBoxStyle Style = NotificationBoxStyle.Sliding;
}

public class NotificationBoxManager : MonoBehaviour
{
    public Michsky.MUIP.NotificationManager FadingNotificationTL;
    public Michsky.MUIP.NotificationManager PopupNotificationTL;
    public Michsky.MUIP.NotificationManager SlidingNotificationTL;
    public Michsky.MUIP.NotificationManager FadingNotificationTR;
    public Michsky.MUIP.NotificationManager PopupNotificationTR;
    public Michsky.MUIP.NotificationManager SlidingNotificationTR;
    public Michsky.MUIP.NotificationManager FadingNotificationBL;
    public Michsky.MUIP.NotificationManager PopupNotificationBL;
    public Michsky.MUIP.NotificationManager SlidingNotificationBL;
    public Michsky.MUIP.NotificationManager FadingNotificationBR;
    public Michsky.MUIP.NotificationManager PopupNotificationBR;
    public Michsky.MUIP.NotificationManager SlidingNotificationBR;

    public NotificationBoxConfig Config { get; set; }

    private Transform _instantiationsGroup;

    private void Awake()
    {
        _instantiationsGroup = new GameObject("Instantiations Group").transform;
        _instantiationsGroup.SetParent(transform);
    }

    public void Create()
    {
        var notification = Instantiate(GetNotification());
        notification.gameObject.transform.SetParent(_instantiationsGroup);
        notification.title = Config.Title;
        notification.description = Config.Description;
        notification.onClose.AddListener(() => Destroy(notification));
        notification.UpdateUI();
        notification.Open();
    }

    private Michsky.MUIP.NotificationManager GetNotification()
    {
        switch (Config.Position)
        {
            case NotificationBoxPosition.TopLeft:
                return Config.Style switch
                {
                    NotificationBoxStyle.Fading => FadingNotificationTL,
                    NotificationBoxStyle.Popup => PopupNotificationTL,
                    NotificationBoxStyle.Sliding => SlidingNotificationTL,
                    _ => throw new NotImplementedException(),
                };
            case NotificationBoxPosition.TopRight:
                return Config.Style switch
                {
                    NotificationBoxStyle.Fading => FadingNotificationTR,
                    NotificationBoxStyle.Popup => PopupNotificationTR,
                    NotificationBoxStyle.Sliding => SlidingNotificationTR,
                    _ => throw new NotImplementedException(),
                };
            case NotificationBoxPosition.BottomLeft:
                return Config.Style switch
                {
                    NotificationBoxStyle.Fading => FadingNotificationBL,
                    NotificationBoxStyle.Popup => PopupNotificationBL,
                    NotificationBoxStyle.Sliding => SlidingNotificationBL,
                    _ => throw new NotImplementedException(),
                };
            case NotificationBoxPosition.BottomRight:
                return Config.Style switch
                {
                    NotificationBoxStyle.Fading => FadingNotificationBR,
                    NotificationBoxStyle.Popup => PopupNotificationBR,
                    NotificationBoxStyle.Sliding => SlidingNotificationBR,
                    _ => throw new NotImplementedException(),
                };
            default:
                throw new NotImplementedException();
        }
    }
}
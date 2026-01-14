using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MessageUI : BaseUIPanel
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField, Range(1, 5)] private float existTime;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        if (_canvasGroup == null)
            return;
        _canvasGroup.DOKill();
        FadeInMessage();
        Invoke(nameof(FadeOutMessage), existTime);
    }

    private void FadeInMessage()
    {
        _canvasGroup.DOFade(1f, 0.2f).SetLink(_canvasGroup.gameObject);
        messageText.CrossFadeAlpha(1f, .2f, true);
    }

    private void FadeOutMessage()
    {
        _canvasGroup.DOFade(0f, .2f).SetLink(_canvasGroup.gameObject);
        messageText.CrossFadeAlpha(0f, .2f, true);
    }

    private void OnDestroy() {
        CancelInvoke();
    }
}
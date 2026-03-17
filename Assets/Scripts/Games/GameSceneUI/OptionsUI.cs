using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour {


    public static OptionsUI Instance { get; private set; }
    
    [SerializeField] private Button closeButton;
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button incisivePassButton;
    [SerializeField] private Button shortPassButton;
    [SerializeField] private Button longPassButton;
    [SerializeField] private Button shootButton;
    [SerializeField] private Button swapdButton;
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI incisivePassButtonText;
    [SerializeField] private TextMeshProUGUI shortPassButtonText;
    [SerializeField] private TextMeshProUGUI longPassButtonText;
    [SerializeField] private TextMeshProUGUI shootButtonText;
    [SerializeField] private TextMeshProUGUI swapdButtonText;
    [SerializeField] private Transform pressToRebindKeyTransform;


    private Action onCloseButtonAction;


    private void Awake() {
        Instance = this;
        closeButton.onClick.AddListener(() => {
            Hide();
            onCloseButtonAction();
        });

        moveUpButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Up); });
        moveDownButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Down); });
        moveLeftButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Left); });
        moveRightButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Right); });
        incisivePassButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.IncisivePass); });
        shortPassButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.ShortPass); });
        longPassButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.LongPass); });
        shootButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Shoot); });
        swapdButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Swap); });
        
        Hide();
    }

    private void Start() {
        PauseManager.Instance.OnGameUnpaused += OnGameUnpaused;

        UpdateVisual();

        HidePressToRebindKey();

    }

    public void OnDestroy() {
        PauseManager.Instance.OnGameUnpaused -= OnGameUnpaused;
    }

    private void OnGameUnpaused() {
        Hide();
    }

    private void UpdateVisual() {
        moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
        moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
        incisivePassButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.IncisivePass);
        shortPassButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.ShortPass);
        longPassButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.LongPass);
        shootButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Shoot);
        swapdButtonText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Swap);
    }

    public void Show(Action onCloseButtonAction) {
        this.onCloseButtonAction = onCloseButtonAction;

        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void ShowPressToRebindKey() {
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }

    private void HidePressToRebindKey() {
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(GameInput.Binding binding) {
        ShowPressToRebindKey();
        GameInput.Instance.RebindBinding(binding, () => {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }

}

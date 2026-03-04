using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI tipText;

    private int _mNowProgress;
    private int _mTargetProgress;
    private AsyncOperation _mLoadSceneAsyncOperation;

    private bool _mRequestSend;
    
    private void Start()
    {   
        tipText.gameObject.SetActive(false);
        _mLoadSceneAsyncOperation = GameInterface.Interface.SceneLoader.LoadGameSceneAsync();
        _mLoadSceneAsyncOperation.allowSceneActivation = false;
        _mRequestSend = false;
    }

    private void Update()
    {
        if (_mLoadSceneAsyncOperation == null)
            return;
        if (_mLoadSceneAsyncOperation.progress < 0.85f)
        {
            _mTargetProgress = (int)(_mLoadSceneAsyncOperation.progress * 100);
        }
        else
        {
            _mTargetProgress = 100;
        }
        
        if (_mNowProgress < _mTargetProgress)
        {
            float speed = (_mTargetProgress - _mNowProgress) > 10 ? 20f : 15f;
            _mNowProgress = (int)Mathf.Min(
                _mNowProgress + speed * Time.deltaTime * 60f,
                _mTargetProgress
            );
        }
        progressSlider.value = _mNowProgress / 100f;
        
        if (_mNowProgress == 100 && !_mRequestSend)
        {
            LoadGameSceneCompleteRequest request =
                GameInterface.Interface.RequestManager.GetRequest<LoadGameSceneCompleteRequest>();
            
            request.SendLoadGameSceneCompleteRequest(onSuccess: OnAllPlayerLoadComplete);
            _mRequestSend = true;

            // StartCoroutine(DelayToComplete());

            tipText.gameObject.SetActive(true);
        }
    }
    private void OnAllPlayerLoadComplete()
    {
        Debug.Log("All player load complete");
        _mLoadSceneAsyncOperation.allowSceneActivation = true;
    }
    // IEnumerator DelayToComplete()
    // {
    //     yield return new WaitForSeconds(0.01f);
    //     _mRequestSend = true;
    //     _mLoadSceneAsyncOperation.allowSceneActivation = true;
    // }

}

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scene 
{
    MainMenuScene,
    RoomScene,
    LoadingScene,
    GameScene,
}

public class SceneLoader
{
    public void LoadScene(Scene scene)
    {
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
    }

    public AsyncOperation LoadGameSceneAsync()
    {
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(Scene.GameScene.ToString(), LoadSceneMode.Single);
        // loadSceneAsync.allowSceneActivation = false;
        return loadSceneAsync;
    }

    public void LoadSceneAsync(Scene scene, Action onComplete = null)
    {
        // OnSceneLoad?.Invoke();

        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);

        loadSceneAsync.completed += _ =>
        {
            // OnSceneLoadComplete?.Invoke();
            onComplete?.Invoke();
        };
    }
}
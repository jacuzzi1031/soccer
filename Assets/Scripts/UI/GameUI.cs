using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public List<CountryDataSo> countryDatabase;
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    [Header("Flags")]
    [SerializeField] private Image homeFlagImage;
    [SerializeField] private Image awayFlagImage;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI goalScorerText;
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI scoreInfoText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;

    private string lastBallCarrier = "";
    private static Dictionary<string, Sprite> flagSpritesDict =
        new Dictionary<string, Sprite>();
    private static readonly Dictionary<string, string> CountryMap =
        new Dictionary<string, string>
        {
            { "FRANCE", "法国队" },
            { "ARGENTINA", "阿根廷队" },
            { "BRAZIL", "巴西队" },
            { "ENGLAND", "英格兰队" },
            { "GERMANY", "德国队" },
            { "ITALY", "意大利队" },
            { "SPAIN", "西班牙队" },
            { "USA", "美国队" },
            { "CANADA", "加拿大队" }
        };
    
    private bool _matchStarted = false;
    public static string ToChinese(string winner)
    {
        if (string.IsNullOrEmpty(winner))
            return string.Empty;

        return CountryMap.TryGetValue(winner, out var cn)
            ? cn
            : winner; // 找不到就原样返回，防止崩
    }
    private void Awake() {
        LoadCountrySprite();
        playerText.text = "";
    }

    public void OnEnable() {
        GameInterface.Interface.EventSystem.Subscribe<OnBallPossessedEvent>(OnBallPossessed);
        GameInterface.Interface.EventSystem.Subscribe<OnBallReleasedEvent>(OnBallReleased);
        GameInterface.Interface.EventSystem.Subscribe<OnScoreChangedEvent>(OnScoreChanged);
        GameInterface.Interface.EventSystem.Subscribe<OnTeamResetEvent>(OnTeamReset);
        GameInterface.Interface.EventSystem.Subscribe<OnGameOverEvent>(OnGameOver);
        
        GameInterface.Interface.GameManager.OnStartMatch+= GameManagerOnOnStartMatch;
    }

    private void GameManagerOnOnStartMatch() {
        _matchStarted = true;
        UpdateScore();
        UpdateFlags();
        UpdateClock();
    }

    private void OnDestroy()
    {
        GameInterface.Interface.EventSystem.Unsubscribe<OnBallPossessedEvent>(OnBallPossessed);
        GameInterface.Interface.EventSystem.Unsubscribe<OnBallReleasedEvent>(OnBallReleased);
        GameInterface.Interface.EventSystem.Unsubscribe<OnScoreChangedEvent>(OnScoreChanged);
        GameInterface.Interface.EventSystem.Unsubscribe<OnTeamResetEvent>(OnTeamReset);
        GameInterface.Interface.EventSystem.Unsubscribe<OnGameOverEvent>(OnGameOver);
    }

    private void Update()
    {
        if (!_matchStarted)
            return;
        UpdateClock();
    }
    void LoadCountrySprite()
    {
        foreach (var data in countryDatabase)
        {
            if (!flagSpritesDict.ContainsKey(data.countryName))
            {
                flagSpritesDict.Add(data.countryName, data.flagSprite);
            }
        }
    }
    public Sprite GetSprite(string country)
    {
        return flagSpritesDict[country];
    }

    private void UpdateScore()
    {
        scoreText.text = GetScoreText(GameInterface.Interface.GameManager.MatchController.currentMatch);
    }
    public string GetScoreText(Match match)
    {   
        return $"{match.goalsHome} - {match.goalsAway}";
    }

    private void UpdateFlags()
    {
        homeFlagImage.sprite =
            GetSprite(GameInterface.Interface.GameManager.MatchController.currentMatch.countryHome);

        awayFlagImage.sprite =
            GetSprite(GameInterface.Interface.GameManager.MatchController.currentMatch.countryAway);
    }

    private void UpdateClock()
    {
        if (GameInterface.Interface.GameManager.MatchController.timeLeft < 0)
            timeText.color = Color.yellow;
        else {
            timeText.color = Color.white;
        }

        timeText.text = GetTimeText(GameInterface.Interface.GameManager.MatchController.timeLeft);
    }
    public static string GetTimeText(float timeLeft)
    {
        if (timeLeft < 0f)
        {
            return "00:00";
        }
        else
        {   
            int totalSeconds = Mathf.CeilToInt(timeLeft);
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds - minutes * 60);

            return $"{minutes:00} : {seconds:00}";
        }
    }

    private void OnBallPossessed(OnBallPossessedEvent obj)
    {
        playerText.text = obj.PlayerName;
        lastBallCarrier = obj.PlayerName;
    }

    private void OnBallReleased(OnBallReleasedEvent obj)
    {
        playerText.text = "";
    }

    private void OnScoreChanged(OnScoreChangedEvent obj)
    {
        if (!GameInterface.Interface.GameManager.MatchController.IsTimeUp())
        {
            goalScorerText.text = $"{lastBallCarrier} 进球!";
            scoreInfoText.text =
                GetCurrentScoreInfo(GameInterface.Interface.GameManager.MatchController.currentMatch);
            animator.Play("GoalAppear");
        }

        UpdateScore();
    }
    public string GetCurrentScoreInfo(Match match)
    {
        if (match.IsTied())
        {
            return $"比分持平 {match.goalsHome} - {match.goalsAway}";
        }
        else
        {
            return $"{ToChinese(match.winner)} 领先 {match.finalScore}";
        }
    }

    private void OnTeamReset(OnTeamResetEvent obj)
    {
        if (GameInterface.Interface.GameManager.MatchController.currentMatch.HasSomeoneScored())
        {
            animator.Play("GoalHide");
        }
    }

    private void OnGameOver(OnGameOverEvent obj)
    {
        scoreInfoText.text =
            GetFinalScoreInfo(GameInterface.Interface.GameManager.MatchController.currentMatch);

        animator.Play("GameOver");
    }
    public string GetFinalScoreInfo(Match match)
    {
        return $"{ToChinese(match.winner)} 获胜 {match.finalScore}";
    }

    public void OnGameOverAnimationComplete() {
        // StartCoroutine(ReturnToMainMenu());
        GameInterface.Interface.GameManager.EndMatch();
    }

    // private IEnumerator ReturnToMainMenu() {
    //     yield return new WaitForSeconds(2.5f);
    //     GameInterface.Interface.SceneLoader.LoadScene(Scene.MainMenuScene);
    // }
}
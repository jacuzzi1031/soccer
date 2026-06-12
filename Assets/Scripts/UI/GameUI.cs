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
    private MatchController _matchController;

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
        
        GameInterface.Interface.EventSystem.Subscribe<MatchStartEvent>(OnMatchStart);
    }

    private void OnMatchStart(MatchStartEvent obj) {
        _matchStarted = true;
        _matchController=GameSceneBootstrap.Instance.MatchController;
        UpdateStartScore();
        UpdateFlags();
        UpdateClock();
    }

    private void OnDestroy() {
        _matchController = null;
        GameInterface.Interface.EventSystem.Unsubscribe<OnBallPossessedEvent>(OnBallPossessed);
        GameInterface.Interface.EventSystem.Unsubscribe<OnBallReleasedEvent>(OnBallReleased);
        GameInterface.Interface.EventSystem.Unsubscribe<OnScoreChangedEvent>(OnScoreChanged);
        GameInterface.Interface.EventSystem.Unsubscribe<OnTeamResetEvent>(OnTeamReset);
        GameInterface.Interface.EventSystem.Unsubscribe<OnGameOverEvent>(OnGameOver);
        
        GameInterface.Interface.EventSystem.Unsubscribe<MatchStartEvent>(OnMatchStart);
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

    private void UpdateStartScore()
    {
        scoreText.text = $"0 - 0";
    }

    public string GetScoreText(Match match)
    {   
        return $"{match.goalsHome} - {match.goalsAway}";
    }

    private void UpdateFlags()
    {
        homeFlagImage.sprite =
            GetSprite(_matchController.countryHome);

        awayFlagImage.sprite =
            GetSprite(_matchController.countryAway);
    }

    private void UpdateClock()
    {
        if (_matchController.FramesLeft <= 0)
        {
            timeText.color = Color.yellow;
        }
        else
        {
            timeText.color = Color.white;
        }

        timeText.text = GetTimeText(_matchController.FramesLeft);
    }
    private const int FPS = 60;

    public static string GetTimeText(int framesLeft)
    {
        if (framesLeft <= 0)
        {
            return "00 : 00";
        }

        int totalSeconds = (framesLeft + FPS - 1) / FPS; // Ceil

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes:00} : {seconds:00}";
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
        var goalHome = _matchController.GoalsHome;
        var goalAway = _matchController.GoalsAway;
        var winner = _matchController.Winner;
        if (!_matchController.IsTimeUp())
        {
            goalScorerText.text = $"{lastBallCarrier} 进球!";

            scoreInfoText.text =
                GetCurrentScoreInfo(goalHome, goalAway,winner);
            animator.Play("GoalAppear");
        }
        scoreText.text = $"{goalHome} - {goalAway}";
    }
    public string GetCurrentScoreInfo(int goalHome,int goalAway,string winner)
    {
        if (goalHome==goalAway)
        {
            return $"比分持平 {goalHome} - {goalAway}";
        }
        else
        {
            return $"{ToChinese(winner)} 领先 {goalHome} - {goalAway}";
        }
    }

    private void OnTeamReset(OnTeamResetEvent obj)
    {
        if (_matchController.HasSomeoneScored())
        {
            animator.Play("GoalHide");
        }
    }

    private void OnGameOver(OnGameOverEvent obj)
    {

        var goalsHome = _matchController.GoalsHome;
        var goalsAway = _matchController.GoalsAway;
        var winner = _matchController.Winner;
        scoreInfoText.text =
            GetFinalScoreInfo(winner, goalsHome, goalsAway);

        animator.Play("GameOver");
        GameSceneBootstrap.Instance.EndMatch();
    }
    public string GetFinalScoreInfo(string winnerName,int goalsHome,int goalsAway)
    {
        return $"{ToChinese(winnerName)} 获胜 {goalsHome} - {goalsAway}";
    }

    // public void OnGameOverAnimationComplete() {
    //     GameSceneBootstrap.Instance.EndMatch();
    // }

    // private IEnumerator ReturnToMainMenu() {
    //     yield return new WaitForSeconds(2.5f);
    //     GameInterface.Interface.SceneLoader.LoadScene(Scene.MainMenuScene);
    // }
}
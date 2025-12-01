using UnityEngine;

public class Goal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TriggerDetection backNetArea;
    [SerializeField] private TriggerDetection scoringArea;
    [SerializeField] private Transform targetsParent;

    private string country = "";
    
    private void Start()
    {
        backNetArea.OnTriggered += OnBallEnterBackNet;
        
        scoringArea.OnTriggered += OnBallEnterScoringArea;
        
        country=GameManager.Instance.currentMatch.countryHome;
    }


    private void OnBallEnterBackNet(Collider2D other)
    {
        Ball ball = other.GetComponentInParent<Ball>();
        if (ball == null) return;

        ball.Stop();
    }
    
    private void OnBallEnterScoringArea(Collider2D other)
    {
        Ball ball = other.GetComponentInParent<Ball>();
        if (ball == null) return;
        Debug.Log("scoring!!!");
        // SoundPlayer.Play(SoundPlayer.Sound.Whistle);
        // GameEvents.TeamScored(country);
    }
    
    public Vector2 GetRandomTargetPosition()
    {
        int count = targetsParent.childCount;
        int index = Random.Range(0, count);
        return targetsParent.GetChild(index).position;
    }

    public Vector2 GetCenterTargetPosition()
    {
        int count = targetsParent.childCount;
        int index = Mathf.FloorToInt(count / 2f);
        return targetsParent.GetChild(index).position;
    }

    public Vector2 GetTopTargetPosition()
    {
        return targetsParent.GetChild(0).position;
    }

    public Vector2 GetBottomTargetPosition()
    {
        int count = targetsParent.childCount;
        return targetsParent.GetChild(count - 1).position;
    }
}

using UnityEngine;

public class Goal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TriggerDetection backNetArea;
    [SerializeField] private TriggerDetection scoringArea;
    [SerializeField] private Transform targetsParent;
    [SerializeField] private Collider2D InvisibleWalls;
    private Collider2D scoringAreaCollider;
    private string country = "";
    
    private void Start()
    {
        backNetArea.OnTriggered += OnBallEnterBackNet;
        
        scoringAreaCollider=scoringArea.GetComponent<Collider2D>();
    }


    private void OnBallEnterBackNet(Collider2D other)
    {
        BallView ballView = other.GetComponentInParent<BallView>();
        if (ballView == null) return;

        ballView.Stop();
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

    public Collider2D GetScoringArea() {
        return scoringAreaCollider;
    }

    public void initialize(string currentMatchCountryHome) {
        country=currentMatchCountryHome;
    }
}

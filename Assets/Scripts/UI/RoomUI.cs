using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class RoomUI : MonoBehaviour
{
    public static RoomUI Instance{get; private set;}
    [SerializeField] private TextMeshProUGUI roomNameText;
    [Header("Data")]
    public List<CountryDataSo> countryDatabase;
    [Header("UI")]
    public Transform gridParent;
    public CountryItem itemPrefab;
    public SelectorCursor selectorPrefab;
    public GameObject CpucursorPrefab;
    private Dictionary<int,SelectorCursor> selectorDict = new Dictionary<int,SelectorCursor>();
    [Header("Layout")]
    public int columns = 4;

    private List<CountryItem> items = new();
    private int currentIndex = 0;
    private Vector2 lastMoveInput;
    private void Awake() {
        Instance = this;
        UpdateRoomName();
    }

    public void OnCountrySelect(int selectorid,int selectIndex) {
        if (selectIndex != currentIndex) {
            currentIndex = selectIndex;
            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.UI_NAV);
        }

        //还是之后交给RoomManager处理
        
        selectorDict[selectorid].MoveTo(items[selectIndex].transform);
        // SoundPlayer.Play("ui_nav");
    }

    private void UpdateRoomName()
    {
        roomNameText.text = GameInterface.Interface.GameManager.currentMatchType switch
        {
            GameManager.MatchType.Training           => "训练模式",
            GameManager.MatchType.TrainingWithEnemy  => "对抗训练",
            GameManager.MatchType.UltimateTeam       => "锦标赛",
            _                                        => "未知模式"
        };
    }
    public void Start()
    {
        //GridLayoutGroup 的执行顺序是 Awake / Start之后Layout Rebuild（UI 系统） Item 的 RectTransform.position 在 Start 时还是旧值 / (0,0)，
        //不能给selector初始化第一个item位置
        BuildGrid();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            gridParent.GetComponent<RectTransform>()
        );
        currentIndex = 0;
        GameInput.Instance.OnShootAction+= SetCountryFromKeyboard;
    }

    void BuildGrid()
    {
        foreach (var data in countryDatabase)
        {
            var item = Instantiate(itemPrefab, gridParent);
            item.Bind(data);
            items.Add(item);
        }
    }
    public void Update()
    {
        Vector2 move = GameInput.Instance.GetMovementVectorNormalized();

        if (move == Vector2.zero || move == lastMoveInput)
            return;
        
        TryMove(move);
        lastMoveInput = move;
    }
    public void LateUpdate()
    {
        if (GameInput.Instance.GetMovementVectorNormalized() == Vector2.zero)
            lastMoveInput = Vector2.zero;
    }
    void TryMove(Vector2 move)
    {
        int row = currentIndex / columns;
        int col = currentIndex % columns;

        int targetIndex = currentIndex;

        if (move.x > 0.5f && col < columns - 1)
            targetIndex++;
        else if (move.x < -0.5f && col > 0)
            targetIndex--;
        else if (move.y > 0.5f && row > 0)
            targetIndex -= columns;
        else if (move.y < -0.5f && currentIndex + columns < items.Count)
            targetIndex += columns;

        if (targetIndex != currentIndex)
        {
            GameInterface.Interface.EventSystem.Publish(new CountrySelectEvent(targetIndex,items[targetIndex].CountryName));
        }
    }

    public void SetIndexFromMouse(int index) {
        if(currentIndex == index) return;
        GameInterface.Interface.EventSystem.Publish(new CountrySelectEvent(index,items[index].CountryName));
    }
    private void SetCountryFromKeyboard(object sender, EventArgs e) {
        GameInterface.Interface.EventSystem.Publish(new CountryConfirmEvent(items[currentIndex].CountryName));
    }
    public void SetCountryFromClick(string countryName) {
        GameInterface.Interface.EventSystem.Publish(new CountryConfirmEvent(countryName));
    }

    public void OnPlayerPositionAvailable(int i,bool isempty) {
        if (!isempty) {
            CreateSelector(i);
        }
        else {
            selectorDict.Remove(i);
        }
    }
    private void CreateSelector(int positionId)
    {
        SelectorCursor selector = Instantiate(selectorPrefab, transform);
        selector.Init(positionId);
        selectorDict.Add(positionId, selector);
        selector.MoveTo(items[currentIndex].transform);
    }

    public void selectCpuCountry(string excludeCountry, Action<string> onSelected)
    {
        var candidates = items
            .Where(item => item.CountryName != excludeCountry)
            .ToList();

        if (candidates.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            var selectedItem = candidates[index];

            GameObject cpuCursor = Instantiate(CpucursorPrefab, transform);
            cpuCursor.transform.position = selectedItem.transform.position;

            // 回调返回 CountryName
            onSelected?.Invoke(selectedItem.CountryName);
        }
    }
}

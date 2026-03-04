using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SocketProtocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class RoomUI : MonoBehaviour
{
    public static RoomUI Instance{get; private set;}
    [SerializeField] private TextMeshProUGUI roomNameText;
    [Header("Data")]
    public List<CountryDataSo> countryDatabase;
    [Header("UI")]
    public Button exitButton;
    public Transform gridParent;
    public CountryItem itemPrefab;
    public SelectorCursor selectorPrefab;
    public GameObject CpucursorPrefab;
    private Dictionary<int,SelectorCursor> selectorDict = new Dictionary<int,SelectorCursor>();
    [Header("Layout")]
    public int columns = 4;

    private List<CountryItem> items = new();
    private int localIndex = 0;
    private Vector2 lastMoveInput;
    
    private RoomPlayerSelectCountryRequest _mRoomPlayerSelectCountryRequest;
    private RoomPlayerConfirmCountryRequest _mRoomPlayerConfirmCountryRequest;
    private void Awake() {
        Instance = this;
        UpdateRoomName();
    }
    public void OnCountrySelect(int selectorid,int selectIndex) {
        if (selectIndex != localIndex) {
            localIndex = selectIndex;
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
            RoomMatchType.Training           => "训练模式",
            RoomMatchType.TrainingWithEnemy  => "对抗训练",
            RoomMatchType.UltimateTeam       => "锦标赛",
            _                            => "未知模式"
        };
    }
    public void Start()
    {
        exitButton.onClick.AddListener(() => {
                QuitRoomRequest quitRoomRequest = GameInterface.Interface.RequestManager.GetRequest<QuitRoomRequest>();
                quitRoomRequest.SendQuitRoomRequest();
            }
            );
        BuildGrid();
        // Start之后Layout Rebuild,所以为了selectCursor需要
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            gridParent.GetComponent<RectTransform>()
        );
        localIndex = 0;
        GameInput.Instance.OnShootAction+= ConfirmCountryFromKeyboard;
        _mRoomPlayerSelectCountryRequest = GameInterface.Interface.RequestManager.GetRequest<RoomPlayerSelectCountryRequest>();
        _mRoomPlayerConfirmCountryRequest = GameInterface.Interface.RequestManager.GetRequest<RoomPlayerConfirmCountryRequest>();
    }

    public void OnDestroy() {
        GameInput.Instance.OnShootAction-= ConfirmCountryFromKeyboard;
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
        int row = localIndex / columns;
        int col = localIndex % columns;

        int targetIndex = localIndex;

        if (move.x > 0.5f && col < columns - 1)
            targetIndex++;
        else if (move.x < -0.5f && col > 0)
            targetIndex--;
        else if (move.y > 0.5f && row > 0)
            targetIndex -= columns;
        else if (move.y < -0.5f && localIndex + columns < items.Count)
            targetIndex += columns;

        if (targetIndex != localIndex)
        {
            _mRoomPlayerSelectCountryRequest.SendSelectCountryRequest(targetIndex, items[targetIndex].CountryName);
        }
    }

    public void SetIndexFromMouse(int index) {
        if(localIndex == index) return;
        _mRoomPlayerSelectCountryRequest.SendSelectCountryRequest(index, items[index].CountryName);
    }
    private void ConfirmCountryFromKeyboard() {
        _mRoomPlayerConfirmCountryRequest.SendRoomPlayerConfirmCountryRequest(items[localIndex].CountryName);
    }
    public void ConfirmCountryFromClick(string countryName) {
        _mRoomPlayerConfirmCountryRequest.SendRoomPlayerConfirmCountryRequest(countryName);
    }
    public void CreateSelector(int positionId)
    {
        SelectorCursor selector = Instantiate(selectorPrefab, transform);
        selector.Init(positionId);
        selectorDict.Add(positionId, selector);
        selector.MoveTo(items[0].transform);
        Debug.Log("CreateSelector in RoomUI");
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

    public void DeleteSelectCursor(int playerIndex) {
        if (selectorDict.TryGetValue(playerIndex, out SelectorCursor selector))
        {
            Destroy(selector.gameObject);
            selectorDict.Remove(playerIndex);
        }
    }
}

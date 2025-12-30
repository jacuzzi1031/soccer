using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : BaseManager
{
    public List<RoomPlayerInfo> RoomPlayerList { get; private set; }

    public RoomManager() {
        RoomPlayerList = new List<RoomPlayerInfo>();
        
    }

    public void OnEnter() {
        GameInterface.Interface.EventSystem.Subscribe<CountryConfirmEvent>(onCountryConfirm);
    }

    public void OnExit() {
        GameInterface.Interface.EventSystem.Unsubscribe<CountryConfirmEvent>(onCountryConfirm);
    }

    public override void OnInit() {
        //先手动设置
        RoomPlayerInfo roomPlayerInfo = new RoomPlayerInfo();
        roomPlayerInfo.id = 0;
        roomPlayerInfo.nickname = "jacuzzi";
        roomPlayerInfo.isHome = true;
        RoomPlayerList.Add(roomPlayerInfo);
        GameInterface.Interface.LocalPlayerInfo=roomPlayerInfo;
        //先手动设置
        
    }
    private void onCountryConfirm(CountryConfirmEvent obj) {
        
        int localId = GameInterface.Interface.LocalPlayerInfo.id;
        RoomPlayerInfo roomPlayerInfo =RoomPlayerList.Find(roomPlayer=>roomPlayer.id == localId);
        if(roomPlayerInfo.comfirmed==true) return;
        
        RoomPlayer roomPlayer = RoomVisual.Instance._mRoomPlayerInfoToRoomPlayerDict[roomPlayerInfo];
        if (roomPlayer.RoomIndex == 0) {
            roomPlayerInfo.isHome = true;
        }
        else {
            roomPlayerInfo.isHome = false;
        }
        
        SoundManager.Instance.Play(SoundManager.Instance.audioRefs.UI_SELECT);
        
        GameInterface.Interface.GameManager.SetMatchCountry(roomPlayer.RoomIndex,obj.Country);
        
        roomPlayerInfo.comfirmed=true;
        RoomVisual.Instance._mRoomPlayerInfoToRoomPlayerDict[roomPlayerInfo].SetConfirmed(true);
        if (RoomPlayerList.Count == 1) {
            RoomVisual.Instance.SpawnCPU(obj.Country,
                (index, country) => {
                    GameInterface.Interface.GameManager.SetMatchCountry(index,country);
                    SoundManager.Instance.Play(SoundManager.Instance.audioRefs.UI_SELECT);
                } );
            //没给GameManager 信息
        }
        
        //下面是服务端确认
        bool allReady = RoomPlayerList.All(item => item.comfirmed);
        if (allReady)
        {
            GameInterface.Interface.StartCoroutine(DelayLoadScene());
        }

        // if (allReady) {
        //      GameInterface.Interface.SceneLoader.LoadGameSceneAsync();
        // }
    }
    IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(1f);
        GameInterface.Interface.SceneLoader.LoadScene(Scene.LoadingScene);
    }
}

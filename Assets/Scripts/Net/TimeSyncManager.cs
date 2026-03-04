
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimeSyncManager:MonoBehaviour
{
    private long _offset;
    public static TimeSyncManager Instance { get;private set; }
    private TimeSyncRequest timeSyncRequest;
    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public long GetServerTimeMs()
    {
        long local = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return local + _offset;
    }
    class SyncSample
    {
        public long Offset;
        public long Rtt;
    }
    private List<SyncSample> _samples = new List<SyncSample>();
    public bool IsSynced { get; private set; }
    public void OnTimeSyncResponse(long clientSendTime, long serverTime)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        long rtt = now - clientSendTime;
        long oneWay = rtt / 2;
        long estimatedServerNow = serverTime + oneWay;

        long offset = estimatedServerNow - now;
        Debug.Log("TimeSyncManager.OnTimeSyncResponse  Rtt:"+rtt+"  Offset:"+offset);
        _samples.Add(new SyncSample
        {
            Offset = offset,
            Rtt = rtt
        });

        if (_samples.Count >= 3)
        {
            ApplyBestSample();
        }
    }
    private void ApplyBestSample()
    {
        var best = _samples.OrderBy(s => s.Rtt).First();
        _offset = best.Offset;

        _samples.Clear();
        IsSynced = true;
    }

    public void EnterGameWhenSynced() {
        StartCoroutine(WaitAndEnterGame());
    }
    private IEnumerator WaitAndEnterGame()
    {
        yield return new WaitUntil(() => IsSynced);

        GameInterface.Interface.SceneLoader.LoadScene(Scene.LoadingScene);
    }
}

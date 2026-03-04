
    using System.Collections;
    using UnityEngine;

    public class StartTimeSyncRoutine:MonoBehaviour {
        private TimeSyncRequest timeSyncRequest;
        private Coroutine _timeSyncCoroutine;

        private void Awake() {
            timeSyncRequest=GameInterface.Interface.RequestManager.GetRequest<TimeSyncRequest>();
        }
        public void Start() {
            _timeSyncCoroutine = StartCoroutine(SyncRoutine());
        }
        private IEnumerator SyncRoutine()
        {
            const int sampleCount = 3;

            for (int i = 0; i < sampleCount; i++)
            {
                timeSyncRequest.SendTimeSyncRequest();
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }
    }

#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_2018_1_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace UnityEngine {
    [ExecuteInEditMode]
    public class MonitorStatus : MonoBehaviour {

        [SerializeField, Range(.1f, 2f)]
        private float updateInterval = 1.0f;
        private string info = "";
        [SerializeField] private bool fpsMode = true;

        private int frames = 0;
        private float duration = 0f;
        private float bestFrames = float.MaxValue;
        private float frameMedia = 0f;
        private float worstPictures = 0f;

        private static MonitorStatus mainMonitorStatus = (MonitorStatus)null;
        public static MonitorStatus MainMonitorStatus => mainMonitorStatus;

        private void Awake() {
            if (mainMonitorStatus == (MonitorStatus)null) {
                mainMonitorStatus = this;
#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                    DontDestroyOnLoad(gameObject);
#else
                DontDestroyOnLoad(gameObject);
#endif
            } else { 
                if (mainMonitorStatus != this)
                    DestroyImmediate(gameObject);
            }
        }

        private void Update() {
            Monitor();
        }

        private void OnGUI() {
            fpsMode = GUI.Toggle(new Rect(0, 0, 130, 25), fpsMode, "FPS mode");
            GUI.Box(new Rect(
                new Vector2(0, 25),
                GUI.skin.box.CalcSize(new GUIContent(info))),
                info);
        }
        //I preferred to put this method inside the update method since it is called less often than the ongui method.
        private void Monitor() {
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            frames++;
            duration += unscaledDeltaTime;

            if (duration >= updateInterval) {
                frameMedia = frames / duration;
                duration = worstPictures = frames = 0;
                bestFrames = float.MaxValue;
            }

            if (unscaledDeltaTime < bestFrames)
                bestFrames = unscaledDeltaTime;

            if (unscaledDeltaTime > worstPictures)
                worstPictures = unscaledDeltaTime;

            float _frames = 1f / unscaledDeltaTime;
            float _bestFrames = 1f / unscaledDeltaTime;
            float _worstPictures = 1f / unscaledDeltaTime;

            _frames = fpsMode ? (int)_frames : 1000f * _frames;
            _bestFrames = fpsMode ? (int)_bestFrames : 1000f * _bestFrames;
            _worstPictures = fpsMode ? (int)_worstPictures : 1000f * _worstPictures;
            float _frameMedia = fpsMode ? (int)frameMedia : 1000f * frameMedia;

            info = string.Format(
                "{0}\nFrames:{1}\nBest Frames:{2}\nFrame Media:{3}\nWorst Pictures:{4}",
                fpsMode ? "FPS" : "MS", _frames, _bestFrames, _frameMedia, _worstPictures);

            info = string.Format(
                "{0}\nMemory:{1}\nGPU Memory:{2}",
                info, SystemInfo.graphicsMemorySize, SystemInfo.systemMemorySize);

            info = string.Format(
                "{0}\nTotalAllocatedMemory:{1}mb\nTotalReservedMemory:{2}mb\nTotalUnusedReservedMemory:{3}mb",
                info,
                Profiler.GetTotalAllocatedMemoryLong() / 1048576,
                Profiler.GetTotalReservedMemoryLong() / 1048576,
                Profiler.GetTotalUnusedReservedMemoryLong() / 1048576);
#if UNITY_EDITOR
            info = string.Format(
                "{0}\nDrawCalls:{1}\nUsed Texture Memory:{2}\nrenderedTextureCount:{3}",
                info,
                UnityStats.drawCalls,
                UnityStats.usedTextureMemorySize / 1048576,
                UnityStats.renderTextureCount);
#endif
        }
    }
}

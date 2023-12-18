using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_IOS
using System.Collections;
using Unity.Collections;
using UnityEngine.XR.ARKit;
#endif

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class ARWorldMapController : MonoBehaviour
    {
        List<string> m_LogMessages;
        ARRaycastManager m_RaycastManager;
        ARAnchorManager m_ARAnchorManager;

        [Tooltip("The ARSession component controlling the session from which to generate ARWorldMaps.")]
        [SerializeField]
        ARSession m_ARSession;

        [SerializeField]
        Animator bubbleAnimator;

        [Tooltip("UI Text component to display error messages")]
        [SerializeField]
        Text m_ErrorText;

        [Tooltip("The UI Text element used to display log messages.")]
        [SerializeField]
        Text m_LogText;

        [Tooltip("The UI Text element used to display the current AR world mapping status.")]
        [SerializeField]
        Text m_MappingStatusText;

        [Tooltip("A UI button component that will generate an ARWorldMap and save it to disk.")]
        [SerializeField]
        Button m_SaveButton;

        [Tooltip("A UI button component that will load a previously saved ARWorldMap from disk and apply it to the current session.")]
        [SerializeField]
        Button m_LoadButton;

        static string path => Path.Combine(Application.persistentDataPath, "my_session.worldmap");
        bool isAnimationPlaying = false;

        bool supported
        {
            get
            {
#if UNITY_IOS
                return m_ARSession.subsystem is ARKitSessionSubsystem && ARKitSessionSubsystem.worldMapSupported;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Create an <c>ARWorldMap</c> and save it to disk.
        /// </summary>
        public void OnSaveButton()
        {
#if UNITY_IOS
            StartCoroutine(Save());
#endif
        }

        /// <summary>
        /// Load an <c>ARWorldMap</c> from disk and apply it to the current session.
        /// </summary>
        public void OnLoadButton()
        {
#if UNITY_IOS
            StartCoroutine(Load());
#endif
        }

        /// <summary>
        /// Reset the <c>ARSession</c>, destroying any existing trackables, such as planes.
        /// Upon loading a saved <c>ARWorldMap</c>, saved trackables will be restored.
        /// </summary>
        public void OnResetButton()
        {
            m_ARSession.Reset();
        }

#if UNITY_IOS
        IEnumerator Save()
        {
            var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
            if (sessionSubsystem == null)
            {
                Log("No session subsystem available. Could not save.");
                yield break;
            }

            var request = sessionSubsystem.GetARWorldMapAsync();

            while (!request.status.IsDone())
                yield return null;

            if (request.status.IsError())
            {
                Log($"Session serialization failed with status {request.status}");
                yield break;
            }

            var worldMap = request.GetWorldMap();
            request.Dispose();

            SaveAndDisposeWorldMap(worldMap);
        }

        void SaveAndDisposeWorldMap(ARWorldMap worldMap)
        {
            Log("Serializing ARWorldMap to byte array...");
            var data = worldMap.Serialize(Allocator.Temp);
            Log($"ARWorldMap has {data.Length} bytes.");

            var file = File.Open(path, FileMode.Create);
            var writer = new BinaryWriter(file);
            writer.Write(data.ToArray());
            writer.Close();
            data.Dispose();
            worldMap.Dispose();
            Log($"ARWorldMap written to {path}");
        }

        IEnumerator Load()
        {
            var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
            if (sessionSubsystem == null)
            {
                Log("No session subsystem available. Could not load.");
                yield break;
            }

            FileStream file;
            try
            {
                file = File.Open(path, FileMode.Open);
            }
            catch (FileNotFoundException)
            {
                Debug.LogError("No ARWorldMap was found. Make sure to save the ARWorldMap before attempting to load it.");
                yield break;
            }

            Log($"Reading {path}...");

            const int bytesPerFrame = 1024 * 10;
            var bytesRemaining = file.Length;
            var binaryReader = new BinaryReader(file);
            var allBytes = new List<byte>();
            while (bytesRemaining > 0)
            {
                var bytes = binaryReader.ReadBytes(bytesPerFrame);
                allBytes.AddRange(bytes);
                bytesRemaining -= bytesPerFrame;
                yield return null;
            }

            var data = new NativeArray<byte>(allBytes.Count, Allocator.Temp);
            data.CopyFrom(allBytes.ToArray());

            Log("Deserializing to ARWorldMap...");
            ARWorldMap worldMap;
            if (ARWorldMap.TryDeserialize(data, out worldMap))
                data.Dispose();

            if (worldMap.valid)
            {
                Log("Deserialized successfully.");
            }
            else
            {
                Debug.LogError("Data is not a valid ARWorldMap.");
                yield break;
            }

            Log("Apply ARWorldMap to the current session.");
            sessionSubsystem.ApplyWorldMap(worldMap);
        }
#endif

        void Awake()
        {
            m_LogMessages = new List<string>();
            m_RaycastManager = GetComponent<ARRaycastManager>();
            m_ARAnchorManager = GetComponent<ARAnchorManager>();
        }

        void Log(string logMessage)
        {
            m_LogMessages.Add(logMessage);
        }

        static void SetActive(Button button, bool active)
        {
            if (button != null)
                button.gameObject.SetActive(active);
        }

        static void SetActive(Text text, bool active)
        {
            if (text != null)
                text.gameObject.SetActive(active);
        }

        static void SetText(Text text, string value)
        {
            if (text != null)
                text.text = value;
        }

        void Update()
        {
            if (supported)
            {
                SetActive(m_ErrorText, false);
                SetActive(m_SaveButton, true);
                SetActive(m_LoadButton, true);
                SetActive(m_MappingStatusText, true);
            }
            else
            {
                SetActive(m_ErrorText, true);
                SetActive(m_SaveButton, false);
                SetActive(m_LoadButton, false);
                SetActive(m_MappingStatusText, false);
            }

#if UNITY_IOS
            var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
            if (sessionSubsystem == null)
                return;

            var numLogsToShow = 20;
            string msg = "";
            for (int i = Mathf.Max(0, m_LogMessages.Count - numLogsToShow); i < m_LogMessages.Count; ++i)
            {
                msg += m_LogMessages[i];
                msg += "\n";
            }
            SetText(m_LogText, msg);
            SetText(m_MappingStatusText, $"Mapping Status: {sessionSubsystem.worldMappingStatus}");

            // Check proximity to the anchor
            if (IsPlayerNearAnchor())
            {
                if (!isAnimationPlaying)
                {
                    // Start the bubble animation
                    bubbleAnimator.SetBool("IsPlaying", true);
                    isAnimationPlaying = true;
                }
            }
            else
            {
                if (isAnimationPlaying)
                {
                    // Stop the bubble animation
                    bubbleAnimator.SetBool("IsPlaying", false);
                    isAnimationPlaying = false;
                }
            }
#endif
        }

        bool IsPlayerNearAnchor()
        {
            // Implement logic to check proximity to the anchor
            // For example, using raycasting to determine distance
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
            {
                float thresholdDistance = 2.0f; // Set your desired threshold distance
                return hit.distance < thresholdDistance;
            }

            return false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using RedRunner.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace RedRunner.UI
{
    public class UIPlayerStatsText : UIText
    {
        public enum StatsDisplayMode
        {
            StartScreen,
            EndScreen
        }

        [SerializeField]
        private StatsDisplayMode m_DisplayMode = StatsDisplayMode.StartScreen;

        private int m_CoinCount;
        private float m_CurrentScore;
        private float m_HighScore;
        private float m_LastScore;

        public void SetDisplayMode(StatsDisplayMode displayMode)
        {
            m_DisplayMode = displayMode;
            RefreshText();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            GameManager.OnScoreChanged += GameManager_OnScoreChanged;
            GameManager gameManager = GameManager.Singleton ?? FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                m_CoinCount = gameManager.CoinCount;
                m_CurrentScore = gameManager.CurrentScore;
                m_HighScore = gameManager.HighScore;
                m_LastScore = gameManager.LastScore;
                gameManager.m_Coin.AddEventAndFire(GameManager_OnCoinChanged, this, true);
            }

            RefreshText();
        }

        protected override void OnDisable()
        {
            GameManager.OnScoreChanged -= GameManager_OnScoreChanged;

            GameManager gameManager = GameManager.Singleton ?? FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.m_Coin.RemoveEvent(this);
            }

            base.OnDisable();
        }

        private void GameManager_OnScoreChanged(float newScore, float highScore, float lastScore)
        {
            m_CurrentScore = newScore;
            m_HighScore = highScore;
            m_LastScore = lastScore;
            RefreshText();
        }

        private void GameManager_OnCoinChanged(int coinCount)
        {
            m_CoinCount = coinCount;
            RefreshText();
        }

        private void RefreshText()
        {
            if (m_DisplayMode == StatsDisplayMode.EndScreen)
            {
                text = string.Format("Run {0}\nBest {1}\nCoins x {2}", m_LastScore.ToLength(), m_HighScore.ToLength(), m_CoinCount);
                return;
            }

            text = string.Format("Best {0}\nCoins x {1}", m_HighScore.ToLength(), m_CoinCount);
        }
    }

    public static class UIPlayerStatsBootstrap
    {
        private const string RuntimeStartStatsName = "Runtime Start Stats Text";
        private const string RuntimeEndStatsName = "Runtime End Stats Text";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateStatsTextsIfMissing()
        {
            CreateStatsText(RuntimeStartStatsName, "Start Screen", UIPlayerStatsText.StatsDisplayMode.StartScreen, new Vector2(-36f, -36f), new Vector2(360f, 96f));
            CreateStatsText(RuntimeEndStatsName, "End Screen", UIPlayerStatsText.StatsDisplayMode.EndScreen, new Vector2(-36f, -36f), new Vector2(380f, 132f));
        }

        private static void CreateStatsText(string objectName, string parentName, UIPlayerStatsText.StatsDisplayMode displayMode, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            if (FindTransformByName(objectName) != null)
            {
                return;
            }

            Transform parent = FindTransformByName(parentName);
            if (parent == null)
            {
                return;
            }

            GameObject statsObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(UIPlayerStatsText), typeof(Outline), typeof(UIScreenVisibilityFollower));
            statsObject.transform.SetParent(parent, false);

            RectTransform rectTransform = statsObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;

            UIPlayerStatsText statsText = statsObject.GetComponent<UIPlayerStatsText>();
            statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (statsText.font == null)
            {
                statsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            statsText.fontSize = 26;
            statsText.alignment = TextAnchor.UpperRight;
            statsText.color = new Color(1f, 0.95f, 0.2f, 1f);
            statsText.raycastTarget = false;
            statsText.horizontalOverflow = HorizontalWrapMode.Overflow;
            statsText.verticalOverflow = VerticalWrapMode.Overflow;
            statsText.SetDisplayMode(displayMode);

            Outline outline = statsObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.1f, 0.1f, 0.55f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private static Transform FindTransformByName(string targetName)
        {
            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == targetName && transforms[i].gameObject.scene.IsValid())
                {
                    return transforms[i];
                }
            }

            return null;
        }
    }
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreenVisibilityFollower : MonoBehaviour
    {
        private UIScreen m_ParentScreen;
        private CanvasGroup m_CanvasGroup;

        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_ParentScreen = GetComponentInParent<UIScreen>();
        }

        private void LateUpdate()
        {
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = GetComponent<CanvasGroup>();
            }

            if (m_ParentScreen == null)
            {
                m_ParentScreen = GetComponentInParent<UIScreen>();
            }

            bool visible = m_ParentScreen == null || m_ParentScreen.IsOpen;
            m_CanvasGroup.alpha = visible ? 1f : 0f;
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.blocksRaycasts = false;
        }
    }
    public class UIAchievementToast : MonoBehaviour
    {
        private readonly Queue<string> m_MessageQueue = new Queue<string>();

        private CanvasGroup m_CanvasGroup;
        private Text m_Text;
        private Coroutine m_ShowRoutine;

        public void Initialize(Text text, CanvasGroup canvasGroup)
        {
            m_Text = text;
            m_CanvasGroup = canvasGroup;
            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 0f;
                m_CanvasGroup.interactable = false;
                m_CanvasGroup.blocksRaycasts = false;
            }
        }

        public void Show(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            m_MessageQueue.Enqueue(message);
            if (m_ShowRoutine == null)
            {
                m_ShowRoutine = StartCoroutine(ShowQueuedMessages());
            }
        }

        private IEnumerator ShowQueuedMessages()
        {
            while (m_MessageQueue.Count > 0)
            {
                string message = m_MessageQueue.Dequeue();
                if (m_Text != null)
                {
                    m_Text.text = "Achievement Unlocked\n" + message;
                }

                yield return FadeTo(1f, 0.18f);
                yield return new WaitForSecondsRealtime(1.8f);
                yield return FadeTo(0f, 0.24f);
                yield return new WaitForSecondsRealtime(0.12f);
            }

            m_ShowRoutine = null;
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            if (m_CanvasGroup == null)
            {
                yield break;
            }

            float startAlpha = m_CanvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                m_CanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            m_CanvasGroup.alpha = targetAlpha;
        }
    }

    public class UIAchievementSystem : MonoBehaviour
    {
        private const string SaveKeyPrefix = "RedRunner.Achievement.";

        [SerializeField]
        private UIAchievementToast m_Toast;

        private GameManager m_GameManager;
        private bool m_Subscribed;

        private IEnumerator Start()
        {
            if (m_Toast == null)
            {
                m_Toast = GetComponent<UIAchievementToast>();
            }

            while (GameManager.Singleton == null)
            {
                yield return null;
            }

            m_GameManager = GameManager.Singleton;
            Subscribe();
        }

        private void OnDestroy()
        {
            if (m_Subscribed)
            {
                GameManager.OnScoreChanged -= GameManager_OnScoreChanged;
                if (m_GameManager != null)
                {
                    m_GameManager.m_Coin.RemoveEvent(this);
                }
            }
        }

        private void Subscribe()
        {
            if (m_GameManager == null || m_Subscribed)
            {
                return;
            }

            GameManager.OnScoreChanged += GameManager_OnScoreChanged;
            m_GameManager.m_Coin.AddEvent(GameManager_OnCoinChanged, this, true);
            m_Subscribed = true;
        }

        private void GameManager_OnScoreChanged(float newScore, float highScore, float lastScore)
        {
            float scoreMeters = newScore * Extensions.modifier;
            if (scoreMeters >= 10f)
            {
                Unlock("score_10", "Runner 10 m");
            }
            if (scoreMeters >= 50f)
            {
                Unlock("score_50", "Runner 50 m");
            }
            if (scoreMeters >= 100f)
            {
                Unlock("score_100", "Runner 100 m");
            }
        }

        private void GameManager_OnCoinChanged(int coinCount)
        {
            if (coinCount >= 1)
            {
                Unlock("coin_1", "First Coin");
            }
            if (coinCount >= 10)
            {
                Unlock("coin_10", "Coin Collector x10");
            }
            if (coinCount >= 50)
            {
                Unlock("coin_50", "Treasure Keeper x50");
            }
        }

        private void Unlock(string id, string title)
        {
            string key = SaveKeyPrefix + id;
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                return;
            }

            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();

            if (m_Toast != null)
            {
                m_Toast.Show(title);
            }
        }
    }

    public static class UIAchievementBootstrap
    {
        private const string RuntimeAchievementSystemName = "Runtime Achievement System";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateAchievementSystemIfMissing()
        {
            if (GameObject.Find(RuntimeAchievementSystemName) != null)
            {
                return;
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            GameObject rootObject = new GameObject(RuntimeAchievementSystemName, typeof(RectTransform), typeof(CanvasGroup), typeof(UIAchievementToast), typeof(UIAchievementSystem));
            rootObject.transform.SetParent(canvas.transform, false);

            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = new Vector2(0f, -34f);
            rootRect.sizeDelta = new Vector2(430f, 82f);

            CanvasGroup canvasGroup = rootObject.GetComponent<CanvasGroup>();

            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backgroundObject.transform.SetParent(rootObject.transform, false);

            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            Image background = backgroundObject.GetComponent<Image>();
            background.color = new Color(0.05f, 0.05f, 0.05f, 0.68f);
            background.raycastTarget = false;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            textObject.transform.SetParent(rootObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18f, 8f);
            textRect.offsetMax = new Vector2(-18f, -8f);

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.95f, 0.25f, 1f);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Outline outline = textObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.65f);
            outline.effectDistance = new Vector2(2f, -2f);

            UIAchievementToast toast = rootObject.GetComponent<UIAchievementToast>();
            toast.Initialize(text, canvasGroup);
        }
    }
    public class UIControlHintText : MonoBehaviour
    {
        [SerializeField]
        private float m_VisibleDuration = 7f;
        [SerializeField]
        private float m_FadeDuration = 0.6f;

        private CanvasGroup m_CanvasGroup;
        private UIScreen m_ParentScreen;
        private float m_Timer;

        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            m_ParentScreen = GetComponentInParent<UIScreen>();


            m_CanvasGroup.alpha = 1f;
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            m_Timer = 0f;
            if (m_CanvasGroup != null)
            {
                m_ParentScreen = GetComponentInParent<UIScreen>();

                m_CanvasGroup.alpha = 1f;
            }
        }

        private void Update()
        {
            if (m_CanvasGroup == null)
            {
                return;
            }

            if (m_ParentScreen == null)
            {
                m_ParentScreen = GetComponentInParent<UIScreen>();
            }

            if (m_ParentScreen != null && !m_ParentScreen.IsOpen)
            {
                m_CanvasGroup.alpha = 0f;
                return;
            }

            m_Timer += Time.unscaledDeltaTime;
            if (m_Timer <= m_VisibleDuration)
            {
                m_ParentScreen = GetComponentInParent<UIScreen>();

                m_CanvasGroup.alpha = 1f;
                return;
            }

            float fadeProgress = Mathf.Clamp01((m_Timer - m_VisibleDuration) / m_FadeDuration);
            m_CanvasGroup.alpha = 1f - fadeProgress;
        }
    }

    public static class UIControlHintBootstrap
    {
        private const string RuntimeControlHintName = "Runtime Control Hint Text";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateControlHintIfMissing()
        {
            if (GameObject.Find(RuntimeControlHintName) != null)
            {
                return;
            }

            Transform parent = FindTransformByName("In-Game Screen");
            if (parent == null)
            {
                return;
            }

            GameObject hintObject = new GameObject(RuntimeControlHintName, typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(Text), typeof(Outline), typeof(UIControlHintText));
            hintObject.transform.SetParent(parent, false);

            RectTransform rectTransform = hintObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0f, 0f);
            rectTransform.anchoredPosition = new Vector2(18f, 24f);
            rectTransform.sizeDelta = new Vector2(620f, 42f);

            Text hintText = hintObject.GetComponent<Text>();
            hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (hintText.font == null)
            {
                hintText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            hintText.text = "A/D Move    Space Jump x2    Left Shift Dash";
            hintText.fontSize = 20;
            hintText.alignment = TextAnchor.MiddleLeft;
            hintText.color = new Color(1f, 1f, 1f, 0.95f);
            hintText.raycastTarget = false;
            hintText.horizontalOverflow = HorizontalWrapMode.Overflow;
            hintText.verticalOverflow = VerticalWrapMode.Overflow;

            Outline outline = hintObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.68f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private static Transform FindTransformByName(string targetName)
        {
            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == targetName && transforms[i].gameObject.scene.IsValid())
                {
                    return transforms[i];
                }
            }

            return null;
        }
    }
}





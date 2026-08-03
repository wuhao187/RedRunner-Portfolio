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
}

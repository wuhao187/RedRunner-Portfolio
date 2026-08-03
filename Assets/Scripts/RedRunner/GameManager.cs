using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


using RedRunner.Characters;
using RedRunner.Collectables;
using RedRunner.TerrainGeneration;

namespace RedRunner
{
    public sealed class GameManager : MonoBehaviour
    {
        public delegate void AudioEnabledHandler(bool active);

        public delegate void ScoreHandler(float newScore, float highScore, float lastScore);

        public delegate void ResetHandler();

        public static event ResetHandler OnReset;
        public static event ScoreHandler OnScoreChanged;
        public static event AudioEnabledHandler OnAudioEnabled;

        private const string CoinSaveKey = "RedRunner.Coin";
        private const string AudioEnabledSaveKey = "RedRunner.AudioEnabled";
        private const string LastScoreSaveKey = "RedRunner.LastScore";
        private const string HighScoreSaveKey = "RedRunner.HighScore";

        private static GameManager m_Singleton;

        public static GameManager Singleton
        {
            get
            {
                return m_Singleton;
            }
        }

        [SerializeField]
        private Character m_MainCharacter;
        [SerializeField]
        [TextArea(3, 30)]
        private string m_ShareText;
        [SerializeField]
        private string m_ShareUrl;
        private float m_StartScoreX = 0f;
        private float m_HighScore = 0f;
        private float m_LastScore = 0f;
        private float m_Score = 0f;

        private bool m_GameStarted = false;
        private bool m_GameRunning = false;
        private bool m_AudioEnabled = true;

        /// <summary>
        /// This is my developed callbacks compoents, because callbacks are so dangerous to use we need something that automate the sub/unsub to functions
        /// with this in-house developed callbacks feature, we garantee that the callback will be removed when we don't need it.
        /// </summary>
        public Property<int> m_Coin = new Property<int>(0);


        #region Getters
        public bool gameStarted
        {
            get
            {
                return m_GameStarted;
            }
        }

        public bool gameRunning
        {
            get
            {
                return m_GameRunning;
            }
        }

        public bool audioEnabled
        {
            get
            {
                return m_AudioEnabled;
            }
        }
        #endregion

        void Awake()
        {
            if (m_Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            m_Singleton = this;
            m_Score = 0f;
            LoadLocalData();
        }

        void UpdateDeathEvent(bool isDead)
        {
            if (isDead)
            {
                StartCoroutine(DeathCrt());
            }
            else
            {
                StopCoroutine("DeathCrt");
            }
        }

        IEnumerator DeathCrt()
        {
            m_LastScore = m_Score;
            if (m_Score > m_HighScore)
            {
                m_HighScore = m_Score;
            }
            SaveLocalData();
            if (OnScoreChanged != null)
            {
                OnScoreChanged(m_Score, m_HighScore, m_LastScore);
            }

            yield return new WaitForSecondsRealtime(1.5f);

            EndGame();
            var endScreen = UIManager.Singleton.UISCREENS.Find(el => el.ScreenInfo == UIScreenInfo.END_SCREEN);
            UIManager.Singleton.OpenScreen(endScreen);
        }

        private void Start()
        {
            m_MainCharacter.IsDead.AddEventAndFire(UpdateDeathEvent, this);
            m_StartScoreX = m_MainCharacter.transform.position.x;
            Init();
        }

        public void Init()
        {
            EndGame();
            UIManager.Singleton.Init();
            NotifyScoreChanged();
            StartCoroutine(Load());
        }

        void Update()
        {
            if (m_GameRunning)
            {
                if (m_MainCharacter.transform.position.x > m_StartScoreX && m_MainCharacter.transform.position.x > m_Score)
                {
                    m_Score = m_MainCharacter.transform.position.x;
                    if (OnScoreChanged != null)
                    {
                        OnScoreChanged(m_Score, m_HighScore, m_LastScore);
                    }
                }
            }
        }

        void NotifyScoreChanged()
        {
            if (OnScoreChanged != null)
            {
                OnScoreChanged(m_Score, m_HighScore, m_LastScore);
            }
        }
        IEnumerator Load()
        {
            var startScreen = UIManager.Singleton.UISCREENS.Find(el => el.ScreenInfo == UIScreenInfo.START_SCREEN);
            yield return new WaitForSecondsRealtime(3f);
            UIManager.Singleton.OpenScreen(startScreen);
        }

        void OnApplicationQuit()
        {
            m_LastScore = m_Score;
            if (m_Score > m_HighScore)
            {
                m_HighScore = m_Score;
            }
            SaveLocalData();
        }

        void LoadLocalData()
        {
            m_Coin.Value = PlayerPrefs.GetInt(CoinSaveKey, 0);
            m_AudioEnabled = PlayerPrefs.GetInt(AudioEnabledSaveKey, 1) == 1;
            AudioListener.volume = m_AudioEnabled ? 1f : 0f;
            m_LastScore = PlayerPrefs.GetFloat(LastScoreSaveKey, 0f);
            m_HighScore = PlayerPrefs.GetFloat(HighScoreSaveKey, 0f);
        }

        public void SaveLocalData()
        {
            PlayerPrefs.SetInt(CoinSaveKey, m_Coin.Value);
            PlayerPrefs.SetInt(AudioEnabledSaveKey, m_AudioEnabled ? 1 : 0);
            PlayerPrefs.SetFloat(LastScoreSaveKey, m_LastScore);
            PlayerPrefs.SetFloat(HighScoreSaveKey, m_HighScore);
            PlayerPrefs.Save();
        }

        public int CoinCount
        {
            get
            {
                return m_Coin.Value;
            }
        }

        public float CurrentScore
        {
            get
            {
                return m_Score;
            }
        }

        public float HighScore
        {
            get
            {
                return m_HighScore;
            }
        }

        public float LastScore
        {
            get
            {
                return m_LastScore;
            }
        }

        public void AddCoin(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            m_Coin.Value += amount;
            SaveLocalData();
        }

        public void ClearLocalData()
        {
            m_Coin.Value = 0;
            m_LastScore = 0f;
            m_HighScore = 0f;
            m_Score = 0f;
            m_AudioEnabled = true;
            AudioListener.volume = 1f;

            PlayerPrefs.DeleteKey(CoinSaveKey);
            PlayerPrefs.DeleteKey(AudioEnabledSaveKey);
            PlayerPrefs.DeleteKey(LastScoreSaveKey);
            PlayerPrefs.DeleteKey(HighScoreSaveKey);
            SaveLocalData();
            NotifyScoreChanged();

            if (OnAudioEnabled != null)
            {
                OnAudioEnabled(m_AudioEnabled);
            }
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void ToggleAudioEnabled()
        {
            SetAudioEnabled(!m_AudioEnabled);
        }

        public void SetAudioEnabled(bool active)
        {
            m_AudioEnabled = active;
            AudioListener.volume = active ? 1f : 0f;
            SaveLocalData();
            if (OnAudioEnabled != null)
            {
                OnAudioEnabled(active);
            }
        }

        public void StartGame()
        {
            m_GameStarted = true;
            ResumeGame();
        }

        public void StopGame()
        {
            m_GameRunning = false;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            m_GameRunning = true;
            Time.timeScale = 1f;
        }

        public void EndGame()
        {
            m_GameStarted = false;
            StopGame();
        }

        public void RespawnMainCharacter()
        {
            RespawnCharacter(m_MainCharacter);
        }

        public void RespawnCharacter(Character character)
        {
            Block block = TerrainGenerator.Singleton.GetCharacterBlock();
            if (block != null)
            {
                Vector3 position = block.transform.position;
                position.y += 2.56f;
                position.x += 1.28f;
                character.transform.position = position;
                character.Reset();
            }
        }

        public void Reset()
        {
            m_Score = 0f;
            if (OnReset != null)
            {
                OnReset();
            }
        }

        public void ShareOnTwitter()
        {
            Share("https://twitter.com/intent/tweet?text={0}&url={1}");
        }

        public void ShareOnGooglePlus()
        {
            Share("https://plus.google.com/share?text={0}&href={1}");
        }

        public void ShareOnFacebook()
        {
            Share("https://www.facebook.com/sharer/sharer.php?u={1}");
        }

        public void Share(string url)
        {
            Application.OpenURL(string.Format(url, m_ShareText, m_ShareUrl));
        }

        [System.Serializable]
        public class LoadEvent : UnityEvent
        {

        }

    }

}


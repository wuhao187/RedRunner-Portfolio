using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedRunner.UI
{
    public class PauseScreen : UIScreen
    {
        private const string RuntimeClearSaveButtonName = "Runtime Clear Save Button";

        [SerializeField]
        protected Button ResumeButton = null;
        [SerializeField]
        protected Button HomeButton = null;
        [SerializeField]
        protected Button SoundButton = null;
        [SerializeField]
        protected Button ExitButton = null;
        [SerializeField]
        protected Button ClearSaveButton = null;

        private Text m_ClearSaveButtonText;

        private void Start()
        {
            ResumeButton.SetButtonAction(() =>
            {
                var inGameScreen = UIManager.Singleton.UISCREENS.Find(el => el.ScreenInfo == UIScreenInfo.IN_GAME_SCREEN);
                UIManager.Singleton.OpenScreen(inGameScreen);
                GameManager.Singleton.StartGame();
            });

            HomeButton.SetButtonAction(() =>
            {
                GameManager.Singleton.Init();
            });

            CreateClearSaveButtonIfNeeded();
            BindClearSaveButton();

            if (ClearSaveButton != null)
            {
                ClearSaveButton.gameObject.SetActive(IsOpen);
            }
        }

        private void CreateClearSaveButtonIfNeeded()
        {
            if (ClearSaveButton != null)
            {
                return;
            }

            Transform existingButton = transform.Find(RuntimeClearSaveButtonName);
            if (existingButton != null)
            {
                ClearSaveButton = existingButton.GetComponent<Button>();
                m_ClearSaveButtonText = existingButton.GetComponentInChildren<Text>();
                return;
            }

            GameObject buttonObject = new GameObject(RuntimeClearSaveButtonName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UIButton));
            buttonObject.transform.SetParent(transform, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0f, -220f);
            buttonRect.sizeDelta = new Vector2(260f, 58f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.9f, 0.25f, 0.22f, 0.92f);

            ClearSaveButton = buttonObject.GetComponent<UIButton>();

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            m_ClearSaveButtonText = textObject.GetComponent<Text>();
            m_ClearSaveButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (m_ClearSaveButtonText.font == null)
            {
                m_ClearSaveButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            m_ClearSaveButtonText.text = "Reset Save";
            m_ClearSaveButtonText.fontSize = 26;
            m_ClearSaveButtonText.alignment = TextAnchor.MiddleCenter;
            m_ClearSaveButtonText.color = Color.white;
            m_ClearSaveButtonText.raycastTarget = false;
        }

        private void BindClearSaveButton()
        {
            if (ClearSaveButton == null)
            {
                return;
            }

            ClearSaveButton.SetButtonAction(() =>
            {
                GameManager.Singleton.ClearLocalData();
                if (m_ClearSaveButtonText != null)
                {
                    m_ClearSaveButtonText.text = "Save Reset";
                }
            });
        }

        public override void UpdateScreenStatus(bool open)
        {
            base.UpdateScreenStatus(open);
            if (ClearSaveButton != null)
            {
                ClearSaveButton.gameObject.SetActive(open);
            }

            if (open && m_ClearSaveButtonText != null)
            {
                m_ClearSaveButtonText.text = "Reset Save";
            }
        }
    }
}

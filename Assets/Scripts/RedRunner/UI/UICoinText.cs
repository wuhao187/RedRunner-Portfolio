using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using RedRunner.Collectables;
using System;

namespace RedRunner.UI
{
	public class UICoinText : UIText
	{
		[SerializeField]
		protected string m_CoinTextFormat = "x {0}";

		protected override void Awake ()
		{
			base.Awake ();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			RefreshCoinsText();
		}

		protected override void Start()
		{
			var gm = GameManager.Singleton ?? FindFirstObjectByType<GameManager>();
			if (gm == null)
			{
				Debug.LogError("UICoinText: GameManager not found in scene; coin UI will be disabled.");
				enabled = false;
				return;
			}

			gm.m_Coin.AddEventAndFire(UpdateCoinsText, this);
		}

		public void SetCoinTextFormat(string coinTextFormat)
		{
			m_CoinTextFormat = coinTextFormat;
			RefreshCoinsText();
		}

		private void RefreshCoinsText()
		{
			var gm = GameManager.Singleton ?? FindFirstObjectByType<GameManager>();
			if (gm == null)
			{
				return;
			}

			SetCoinsText(gm.CoinCount);
		}

		private void UpdateCoinsText(int newCoinValue)
		{
			var animator = GetComponent<Animator>();
			if (animator != null)
			{
				animator.SetTrigger("Collect");
			}

			SetCoinsText(newCoinValue);
		}

		private void SetCoinsText(int coinValue)
		{
			text = string.Format(m_CoinTextFormat, coinValue);
		}
	}

	public static class UICoinHudBootstrap
	{
		private const string RuntimeCoinTextName = "Runtime Coin Text";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void CreateCoinTextIfMissing()
		{
			if (GameObject.Find(RuntimeCoinTextName) != null)
			{
				return;
			}

			Transform parent = FindTransformByName("In-Game Screen");
			if (parent == null)
			{
				Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
				if (canvas == null)
				{
					return;
				}

				parent = canvas.transform;
			}

			GameObject coinTextObject = new GameObject(RuntimeCoinTextName, typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(UIScreenVisibilityFollower));
			coinTextObject.transform.SetParent(parent, false);

			RectTransform rectTransform = coinTextObject.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0f, 1f);
			rectTransform.anchorMax = new Vector2(0f, 1f);
			rectTransform.pivot = new Vector2(0f, 1f);
			rectTransform.anchoredPosition = new Vector2(24f, -88f);
			rectTransform.sizeDelta = new Vector2(240f, 48f);

			UICoinText coinText = coinTextObject.AddComponent<UICoinText>();
			coinText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			if (coinText.font == null)
			{
				coinText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			}

			coinText.fontSize = 28;
			coinText.alignment = TextAnchor.MiddleLeft;
			coinText.color = new Color(1f, 0.92f, 0.15f, 1f);
			coinText.raycastTarget = false;
			coinText.SetCoinTextFormat("Coins x {0}");
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

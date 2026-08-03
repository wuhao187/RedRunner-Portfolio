using UnityEngine;
using UnityEngine.UI;

using RedRunner.Characters;

namespace RedRunner.UI
{

	public class UIDashCooldownText : MonoBehaviour
	{
		#region Fields

		[SerializeField]
		protected RedCharacter m_Character;
		[SerializeField]
		protected Text m_Text;
		[SerializeField]
		protected string m_ReadyText = "Dash Ready";
		[SerializeField]
		protected string m_CooldownFormat = "Dash {0:0.0}s";
		[SerializeField]
		protected Color m_ReadyColor = new Color ( 0.3f, 1f, 0.45f, 1f );
		[SerializeField]
		protected Color m_CooldownColor = new Color ( 1f, 0.85f, 0.25f, 1f );

		#endregion

		#region MonoBehaviour Messages

		void Awake ()
		{
			if ( m_Text == null )
			{
				m_Text = GetComponent<Text> ();
			}

			if ( m_Character == null )
			{
				m_Character = FindFirstObjectByType<RedCharacter> ();
			}
		}

		void Update ()
		{
			if ( m_Text == null )
			{
				m_Text = GetComponent<Text> ();
			}

			if ( m_Text == null )
			{
				return;
			}

			if ( m_Character == null )
			{
				m_Character = FindFirstObjectByType<RedCharacter> ();
			}

			if ( m_Character == null )
			{
				SetReadyState ();
				return;
			}

			float remaining = m_Character.DashCooldownRemaining;
			if ( remaining <= 0f )
			{
				SetReadyState ();
			}
			else
			{
				SetCooldownState ( remaining );
			}
		}

		#endregion

		#region Private Methods

		void SetReadyState ()
		{
			m_Text.text = m_ReadyText;
			m_Text.color = m_ReadyColor;
		}

		void SetCooldownState ( float remaining )
		{
			m_Text.text = string.Format ( m_CooldownFormat, remaining );
			m_Text.color = m_CooldownColor;
		}

		#endregion
	}
}
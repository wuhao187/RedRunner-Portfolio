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
				m_Text.text = m_ReadyText;
				return;
			}

			float remaining = m_Character.DashCooldownRemaining;
			if ( remaining <= 0f )
			{
				m_Text.text = m_ReadyText;
			}
			else
			{
				m_Text.text = string.Format ( m_CooldownFormat, remaining );
			}
		}

		#endregion
	}
}
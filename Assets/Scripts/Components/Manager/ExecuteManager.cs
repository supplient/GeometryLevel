using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{
	public class ExecuteManager : MonoBehaviour
	{
		public static ExecuteManager instance {
			get
			{
				var gameObject = GameObject.FindGameObjectWithTag(Utility.TAG_GAME_CONTROLLER);
				return gameObject.GetComponent<ExecuteManager>();
			}
		}

		// ======== Exectute Control ==========

		private bool m_isPaused = false;
		private float m_timeScale_backup = 1.0f;

		public bool isPaused
		{
			get
			{
				return m_isPaused;
			}
		}

		public void PauseOrResumeGame()
		{
			if(m_isPaused)
				ResumeGame();
			else
				PauseGame();
		}

		/// <summary>
		/// Prevent Time.time from increasing.
		/// FixedUpdate will be slient after paused.
		/// Update will stay alive.
		/// </summary>
		public void PauseGame()
		{
			if(m_isPaused)
				return;

			// Pause game's time
			m_timeScale_backup = Time.timeScale;
			Time.timeScale = 0.0f;

			// Popup Pause Menu
			UIManager.instance.PopupPauseMenu();

			// Update state & Log
			m_isPaused = true;
			Debug.Log("Game Paused.");
		}

		public void ResumeGame()
		{
			if(!m_isPaused)
				return;

			// Resume game's time
			Time.timeScale = m_timeScale_backup;

			// Remove Pause Menu
			UIManager.instance.RemovePauseMenu();

			// Update state & Log
			m_isPaused = false;
			Debug.Log("Game Resume.");
		}

		public void QuitGame()
		{
			Debug.Log("Game Quit.");

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif // UNITY_EDTIOR
			Application.Quit(0);
		}

	}
}

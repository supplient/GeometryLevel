using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace geo_level
{
	public class UIController: MonoBehaviour
	{
		public static UIController instance {
			get
			{
				var gameObject = GameObject.FindGameObjectWithTag("GameController");
				return gameObject.GetComponent<UIController>();
			}
		}
		private static GameObject canvas
		{
			get
			{
				return GameObject.FindGameObjectWithTag("UICanvas");
			}
		}

		// ======== Properties =========
		public GameObject m_pauseMenu = null;

		// ======== Status =========
		private GameObject mi_pauseMenu = null;

		public void PopupPauseMenu() {
			if(mi_pauseMenu)
			{
				Debug.LogWarning("Pause Menu already exists.");
				return;
			}
			mi_pauseMenu = GameObject.Instantiate(m_pauseMenu);

			// Transform
			var menuTrans = mi_pauseMenu.GetComponent<RectTransform>();
			menuTrans.SetParent(canvas.transform);
			menuTrans.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

			// Attach Events
			menuTrans.Find("Resume").GetComponent<Button>()
				.onClick.AddListener(GameController.instance.ResumeGame);
			menuTrans.Find("Quit").GetComponent<Button>()
				.onClick.AddListener(GameController.instance.QuitGame);
		}
		public void RemovePauseMenu()
		{
			if(!mi_pauseMenu)
			{
				Debug.LogWarning("Pause Menu not exists.");
				return;
			}
			mi_pauseMenu.SetActive(false);
			GameObject.Destroy(mi_pauseMenu, 1.0f);
			mi_pauseMenu = null;
		}
	}
}

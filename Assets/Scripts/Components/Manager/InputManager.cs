using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace geo_level
{
	public class InputManager: MonoBehaviour
	{
		public static InputManager instance {
			get
			{
				var gameObject = GameObject.FindGameObjectWithTag(Utility.TAG_GAME_CONTROLLER);
				return gameObject.GetComponent<InputManager>();
			}
		}

		// ======== Global Keyboard Input =========
		private void FixedUpdate()
		{
			// Forward to GameObject
			if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
				Forward2PhysicsController();
		}

		private void Update()
		{
			// Global Process, mainly system concerned
			if(Input.GetKeyDown(KeyCode.Escape))
				ExecuteManager.instance.PauseOrResumeGame();
		}

		private void Forward2PhysicsController()
		{
			float input = 0.0f;
			if(Input.GetKey(KeyCode.A))
				input += -1.0f;
			if(Input.GetKey(KeyCode.D))
				input += 1.0f;
			PhysicsController.instance.input = input;
		}
	}
}

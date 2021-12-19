using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace geo_level
{
	class LevelManager: MonoBehaviour
	{
		public static LevelManager instance
		{
			get
			{
				var gameObject = GameObject.FindGameObjectWithTag(Utility.TAG_GAME_CONTROLLER);
				return gameObject.GetComponent<LevelManager>();
			}
		}
		
		public void CompleteLevel()
		{
			Debug.Log("Level Completed.");
			// TODO
		}
	}
}

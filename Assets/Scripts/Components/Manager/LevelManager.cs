using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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

		private AsyncOperation m_asyncLoadForNextLevel = null;

		private void Start()
		{
			var nowScene = SceneManager.GetActiveScene();
			int nowIndex = nowScene.buildIndex;
			int nextIndex = (nowIndex + 1) % SceneManager.sceneCountInBuildSettings;
			LoadLevel(nextIndex);
		}

		private void LoadLevel(int buildIndex)
		{

			Debug.Log(String.Format("Start Loading Level {0}", buildIndex));

			m_asyncLoadForNextLevel = SceneManager.LoadSceneAsync(buildIndex);
			m_asyncLoadForNextLevel.allowSceneActivation = false;
		}
		
		public void CompleteLevel()
		{
			Debug.Log("Level Completed.");
			var nowScene = SceneManager.GetActiveScene();
			int nowIndex = nowScene.buildIndex;

			// We wait by a corotinue to await long-time no response.
			m_asyncLoadForNextLevel.allowSceneActivation = true;
			StartCoroutine(WaitLoadingNextLevel(nowIndex));
		}

		private IEnumerator WaitLoadingNextLevel(int nowIndex)
		{
			while(!m_asyncLoadForNextLevel.isDone)
				yield return null;

			SceneManager.UnloadSceneAsync(nowIndex);
		}

		public void CompleteGame()
		{
			Debug.Log("Game Completed.");
			// TODO
		}
	}
}

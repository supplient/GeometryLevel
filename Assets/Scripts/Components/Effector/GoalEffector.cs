using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{
	public class GoalEffector : MonoBehaviour
	{
		private void OnTriggerEnter2D(Collider2D collision)
		{
			var triggerObj = collision.gameObject;

			// Check if it is a valid trigger.
			// Player or Mirrored are valid triggers.
			if(!Utility.IsPlayer(triggerObj) && !Utility.IsMirrored(triggerObj))
				return;

			LevelManager.instance.CompleteLevel();
		}
	}
}

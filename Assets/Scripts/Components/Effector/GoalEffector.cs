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
			var marker = triggerObj.GetComponent<Marker>();
			if(!marker.canTriggerGoal)
				return;

			LevelManager.instance.CompleteLevel();
		}
	}
}

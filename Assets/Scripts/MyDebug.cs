using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace geo_level
{
	class MyDebug
	{
		public static void TriggerLog(GameObject trigger, string triggerType, GameObject target)
		{
			Debug.Log(string.Format("{0}'s {1} is triggered by {2}.", trigger.name, triggerType, target.name));
		}
	}
}

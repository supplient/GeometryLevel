using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace geo_level
{
	public class Utility
	{
		public static string TAG_PLAYER = "Player";
		public static string TAG_GAME_CONTROLLER = "GameController";
		public static string TAG_UI_CANVAS = "UICanvas";

		public static bool IsPlayer(GameObject obj) { return obj.CompareTag(Utility.TAG_PLAYER); }
		public static bool IsMirrored(GameObject obj) { return obj.GetComponent<MirroredWorker>(); }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{
	public class PolygonColliderModifier : MonoBehaviour, ISpriteChangeReceiver
	{
        /// <summary>
        /// Colldier will be udpated to match the Sprite's new outline.
        /// </summary>
		public void OnSpriteChanged(Sprite sprite)
        {
            if (!isActiveAndEnabled)
                return;

            // Get Rendering Texture
			Texture2D tex = sprite.texture;

            // Trace the texture to get outline for collider
            List<List<int>> pathsInPixel = Potrace.Trace(tex);

            // Convert pathsInPixel into pathsInUnit: pixel->unit & move (0,0) to pivot
            List<List<Vector2>> pathsInUnit = new List<List<Vector2>>();
            foreach (List<int> pathInPixel in pathsInPixel)
            {
                Debug.Assert(pathInPixel.Count % 2 == 0);

                List<Vector2> pathInUnit = new List<Vector2>(pathInPixel.Count / 2);
                for (int i = 0; 2 * i < pathInPixel.Count; i++)
                {
                    int x = pathInPixel[2 * i];
                    int y = pathInPixel[2 * i + 1];
                    float ux = (x - sprite.pivot.x) / sprite.pixelsPerUnit;
                    float uy = (y - sprite.pivot.y) / sprite.pixelsPerUnit;
                    pathInUnit.Add(new Vector2(ux, uy));
                }
                pathsInUnit.Add(pathInUnit);
            }

            // Update Collider
            PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
            collider.pathCount = pathsInUnit.Count;
            for (int i = 0; i < pathsInUnit.Count; i++)
                collider.SetPath(i, pathsInUnit[i]);
        }

	}

}

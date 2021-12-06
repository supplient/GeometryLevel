using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{
	public class MirroredWorker : MonoBehaviour
	{
		public GameObject m_origin;
		public GameObject m_mirrorArea;
		public GameObject m_mirrorTrigger;

		private void UpdateTranform()
		{
			/*
			 * TODO: if the mirrorArea & the mirrorEffector's scales are not uniform,
			 *	Tranform will be strange.
			 *	e.g.
			 *		mirrorEffect's	scale: 4,1,1
			 *		mirrorArea's	scale: 8,1,1 
			 *		(If mirrorArea's scale is 4,1,1, it will be ok)
			 */

			// Cal mirrored's TRS matrix
			Matrix4x4 trigger_w2l	= m_mirrorTrigger.transform.worldToLocalMatrix;
			Matrix4x4 area_l2w		= m_mirrorArea.transform.localToWorldMatrix;
			Matrix4x4 m_origin_mat	= m_origin.transform.localToWorldMatrix;
			Matrix4x4 mirrored_mat	= area_l2w * trigger_w2l * m_origin_mat;

			// Extract TRS from the matrix
			// TODO: note we assume mirrored has no parent, so localScale==lossyScale
			Vector3 pos = new Vector3();
			pos.x = mirrored_mat[0, 3];
			pos.y = mirrored_mat[1, 3];
			pos.z = mirrored_mat[2, 3];
			Quaternion rot = mirrored_mat.rotation;
			Vector3 scale = mirrored_mat.lossyScale;

			transform.position		= pos;
			transform.rotation		= rot;
			transform.localScale	= scale;
		}


		private delegate Vector3 TransFunc(Vector3 p);
		private void UpdateTexture()
		{
			// Get m_origin's tex
			SpriteGenerator m_originSpriteGenerator = m_origin.GetComponent<SpriteGenerator>();
			if(!m_originSpriteGenerator)
			{
				Debug.LogError("SpriteGenertor not found.", m_origin);
				return;
			}
			Texture2D m_originTex = m_originSpriteGenerator.m_tex;

			// Get mirrored's tex
			SpriteGenerator mirroredSpriteGenertor = GetComponent<SpriteGenerator>();
			if(!mirroredSpriteGenertor)
			{
				Debug.LogError("SpriteGenertor not found.", gameObject);
				return;
			}
			Texture2D mirroredTex = mirroredSpriteGenertor.m_tex;

			// Get pivot & transformFunction to transform the point from local-coord to world-coord
			Vector2 pivot = m_originSpriteGenerator.m_pivot;
			float pixelsPerUnit = m_originSpriteGenerator.m_pixelsPerUnit;
			TransFunc transFunc_l2w = m_origin.transform.TransformPoint;

			// Get the mirror trigger's collider
			var collider = m_mirrorTrigger.GetComponent<Collider2D>();
			if(!collider)
			{
				Debug.LogError("Collider2D not found.");
				return;
			}

			/* Check each pixel in m_originTex.
			*	If the pixel is in the mirror trigger,
			*	it will be filled into mirrorTex.
			*	Otherwise, the pixel will be ignored.
			*/
			for(int y=0; y < m_originTex.height; y++)
			{
				for(int x=0; x < m_originTex.width; x++)
				{
					// Transform the pixel: tex-coord -> local-coord -> world-coord
					float x_local = ((float)x / m_originTex.width  - pivot.x) * m_originTex.width  / pixelsPerUnit;
					float y_local = ((float)y / m_originTex.height - pivot.y) * m_originTex.height / pixelsPerUnit;
					var p_local = new Vector3(x_local, y_local, 0.0f);
					var p_world = transFunc_l2w(p_local);
					
					// Check if in the mirror trigger
					if(collider.OverlapPoint(new Vector2(p_world.x, p_world.y)))
					{
						// If in, fill the mirrorTex
						Color32 color = m_originTex.GetPixel(x, y);
						mirroredTex.SetPixel(x, y, color);
					}
					else
					{
						// Otherwise, set transparent
						mirroredTex.SetPixel(x, y, new Color32(0, 0, 0, 0));
					}
				}
			}

			// Update the mirrored's Sprite
			SpriteGenerator spriteGenerator = GetComponent<SpriteGenerator>();
			if(!spriteGenerator)
			{
				Debug.LogError("SpriteGenerator not found.");
				return;
			}
			spriteGenerator.UpdateSprite();
		}

		private void FixedUpdate()
		{
			if(!m_origin)
			{
				Debug.LogError("m_origin not found.", gameObject);
				return;
			}
			if(!m_mirrorArea)
			{
				Debug.LogError("m_mirrorArea not found.", gameObject);
				return;
			}
			if(!m_mirrorTrigger)
			{
				Debug.LogError("m_mirrorTrigger not found.", gameObject);
				return;
			}

			UpdateTranform();
			UpdateTexture();
		}
	}

}

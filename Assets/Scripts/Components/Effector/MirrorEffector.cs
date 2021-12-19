using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{

	public class MirrorEffector : MonoBehaviour
	{
		[Tooltip(@"The pair mirror area object. 
	The mirrored target will be the child of the area object.
	So the area object's Transform will be applied to the mirrored target."
		)]
		public GameObject m_mirrorArea;

		private Dictionary<GameObject, GameObject> target2mirrored = new Dictionary<GameObject, GameObject>();

		private void OnTriggerEnter2D(Collider2D collision)
		{
			GameObject target = collision.gameObject;

			// Check if mirror area assigned
			if(!m_mirrorArea)
			{
				Debug.LogWarning("The mirror area not assigend.");
				return;
			}

			// Check if the mirrored exists
			GameObject mirrored;
			if(target2mirrored.TryGetValue(target, out mirrored))
			{
				Debug.LogWarning("A mirror object exists.", target);
				return;
			}

			// Log
			MyDebug.TriggerLog(gameObject, GetType().Name, target);

			// Create mirrored
			// TODO: use a mirror prefab
			mirrored = Instantiate(target);
			Destroy(mirrored.GetComponent<PhysicsController>());
			mirrored.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
			{
				var targetTex = target.GetComponent<SpriteGenerator>().m_tex;
				mirrored.GetComponent<SpriteGenerator>().m_tex = new Texture2D(
					targetTex.width, targetTex.height, 
					TextureFormat.RGBA32, 
					false, false
				);

				// TODO: now just clear the texture
				//		Maybe a better faster solution
				for(int x=0; x<targetTex.width; x++)
				{
					for(int y=0; y<targetTex.height; y++)
					{
						mirrored.GetComponent<SpriteGenerator>().m_tex.SetPixel(x, y, new Color(0, 0, 0, 0));
					}
				}
			}

			// Attach the mirrored
			var worker = mirrored.AddComponent<MirroredWorker>();
			worker.m_origin = target;
			worker.m_mirrorArea = m_mirrorArea;
			worker.m_mirrorTrigger = gameObject;

			// Memo the mirrored
			target2mirrored[target] = mirrored;
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			GameObject target = collision.gameObject;

			// Check if exists
			GameObject mirrored;
			if(!target2mirrored.TryGetValue(target, out mirrored))
			{
				Debug.LogWarning("The mirror object does not exist.", target);
				return;
			}

			// Destroy & Erase
			mirrored.SetActive(false);
			Destroy(mirrored, 0.1f);
			target2mirrored.Remove(target);
		}

		/*
		private void OnTriggerStay2D(Collider2D collision)
		{
			GameObject target = collision.gameObject;

			// Check if exists
			GameObject mirrored;
			if(!target2mirrored.TryGetValue(target, out mirrored))
			{
				Debug.LogWarning("The mirror object does not exist.", target);
				return;
			}

			// Update the local transform
			UpdateMirroredTransform(target, mirrored);

			// Update the sprite
			UpdateSprite(target, mirrored);
		}

		private delegate Vector3 TransFunc(Vector3 p);
		private void UpdateSprite(GameObject target, GameObject mirrored)
		{
			// Get target's tex
			SpriteGenerator targetSpriteGenerator = target.GetComponent<SpriteGenerator>();
			if(!targetSpriteGenerator)
			{
				Debug.LogError("SpriteGenertor not found.", target);
				return;
			}
			Texture2D targetTex = targetSpriteGenerator.m_tex;

			// Get mirrored's tex
			SpriteGenerator mirroredSpriteGenertor = mirrored.GetComponent<SpriteGenerator>();
			if(!mirroredSpriteGenertor)
			{
				Debug.LogError("SpriteGenertor not found.", mirrored);
				return;
			}
			Texture2D mirroredTex = mirroredSpriteGenertor.m_tex;

			// Get pivot & transformFunction to transform the point from local-coord to world-coord
			Vector2 pivot = targetSpriteGenerator.m_pivot;
			float pixelsPerUnit = targetSpriteGenerator.m_pixelsPerUnit;
			TransFunc transFunc_l2w = target.transform.TransformPoint;

			// Get the mirror trigger's collider
			var collider = GetComponent<Collider2D>();
			if(!collider)
			{
				Debug.LogError("Collider2D not found.");
				return;
			}
			for(int y=0; y <= targetTex.height; y++)
			{
				for(int x=0; x <= targetTex.width; x++)
				{
					// Transform the pixel: tex-coord -> local-coord -> world-coord
					float x_local = ((float)x / targetTex.width  - pivot.x) * targetTex.width  / pixelsPerUnit;
					float y_local = ((float)y / targetTex.height - pivot.y) * targetTex.height / pixelsPerUnit;
					var p_local = new Vector3(x_local, y_local, 0.0f);
					var p_world = transFunc_l2w(p_local);
					
					// Check if in the mirror trigger
					if(collider.OverlapPoint(new Vector2(p_world.x, p_world.y)))
					{
						// If in, fill the mirrorTex
						Color32 color = targetTex.GetPixel(x, y);
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
			SpriteGenerator spriteGenerator = mirrored.GetComponent<SpriteGenerator>();
			if(!spriteGenerator)
			{
				Debug.LogError("SpriteGenerator not found.");
				return;
			}
			spriteGenerator.UpdateSprite();
		}

		private void UpdateMirroredTransform(GameObject target, GameObject mirrored)
		{

			// Cal mirrored's TRS matrix
			Matrix4x4 trigger_w2l = transform.worldToLocalMatrix;
			Matrix4x4 area_l2w = m_mirrorArea.transform.localToWorldMatrix;
			Matrix4x4 target_mat = target.transform.localToWorldMatrix;
			Matrix4x4 mirrored_mat = area_l2w * trigger_w2l * target_mat;

			// Extract TRS from the matrix
			// TODO: note we assume mirrored has no parent, so localScale==lossyScale
			Vector3 pos = new Vector3();
			pos.x = mirrored_mat[0, 3];
			pos.y = mirrored_mat[1, 3];
			pos.z = mirrored_mat[2, 3];
			Quaternion rot = mirrored_mat.rotation;
			Vector3 scale = mirrored_mat.lossyScale;

			mirrored.transform.position		= pos;
			mirrored.transform.rotation		= rot;
			mirrored.transform.localScale	= scale;
		}
		 */
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{

	public class ShapeEffector : MonoBehaviour
	{
		public Texture2D m_targetTex;

		public bool m_changePivot = false;
		public Vector2 m_pivot;
		public bool m_changePixelsPerUnit = false;
		public float m_pixelsPerUnit;

		[Tooltip("Whether to reset the rigidbody's rotation before changing shape. This may avoid some penerate problem.")]
		public bool m_resetRotation = false;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			// Self Check
			if(!isActiveAndEnabled)
				return;

			if(!m_targetTex)
			{
				Debug.LogWarning("m_targetTex not set.");
				return;
			}

			// Check if is a valid trigger
			GameObject target = collision.gameObject;
			if(!target.GetComponent<Marker>() || !target.GetComponent<Marker>().canTriggerShape)
				return;

			// Log
			MyDebug.TriggerLog(gameObject, GetType().Name, target);

			// Get Its origin
			target = Utility.GetOriginObject(target);
			SpriteGenerator spriteGenerator = target.GetComponent<SpriteGenerator>();
			if(!spriteGenerator)
			{
				Debug.LogWarning("Sprite Generator not found.", target);
				return;
			}

			// Reset Rigidbody's Rotation
			if(m_resetRotation)
			{
				Rigidbody2D rigidbody = target.GetComponent<Rigidbody2D>();
				if(!rigidbody)
				{
					Debug.LogWarning("Rigidbody not found.");
					return;
				}
				rigidbody.SetRotation(0.0f);
			}

			// Change Target's Sprite config
			if(m_changePivot)
				spriteGenerator.m_pivot = m_pivot;
			if(m_changePixelsPerUnit)
				spriteGenerator.m_pixelsPerUnit = m_pixelsPerUnit;

			// Change Target's Texture
			spriteGenerator.SetNewTexture(m_targetTex);
			spriteGenerator.UpdateSprite();
		}
	}



}

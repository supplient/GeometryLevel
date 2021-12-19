using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{

	public class ShapeEffector : MonoBehaviour
	{
		public Texture2D m_targetTex;
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

			// Target Check
			GameObject target = collision.gameObject;
			SpriteGenerator spriteGenerator = target.GetComponent<SpriteGenerator>();
			if(!spriteGenerator)
				return;

			// Log
			MyDebug.TriggerLog(gameObject, GetType().Name, target);

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

			// Change Target's Texture
			spriteGenerator.SetNewTexture(m_targetTex);
			spriteGenerator.UpdateSprite();
		}
	}



}

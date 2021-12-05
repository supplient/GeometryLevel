using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{

	public class ShapeEffector : MonoBehaviour
	{
		public Texture2D m_targetTex;

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
			Debug.Log(string.Format("{0}'s {1} is triggered by {2}.", name, GetType().Name, target.name));

			// Change Target's Texture
			spriteGenerator.SetNewTexture(m_targetTex);
			spriteGenerator.UpdateSprite();
		}
	}



}

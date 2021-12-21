using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace geo_level
{
	public class SpriteGenerator : MonoBehaviour
	{
		// Const Parameters
		public const int DEFAULT_TEXTURE_WIDTH  = 100;
		public const int DEFAULT_TEXTURE_HEIGHT = 100;

		// Property
		public Texture2D m_tex;
		/// <summary>
		/// Pivot, in [0,1].
		/// </summary>
		public Vector2 m_pivot = new Vector2(0.5f, 0.5f);
		public float m_pixelsPerUnit = 30.0f;

		// Private State
		private Sprite m_sprite;


		// Start is called before the first frame update
		void Start()
		{
			// Init Texture Set
			SetNewTexture(m_tex);

			// Update Sprite
			UpdateSprite();
		}

		/// <summary>
		/// Replace the texture in the sprite.
		/// </summary>
		/// <param name="tex">New texture. If null, a square will be created.</param>
		public void SetNewTexture(Texture2D tex)
        {
			// Default texture is just a square
			if (m_tex == null)
            {
				Debug.Log("tex is null, so using default square.");
				m_tex = new Texture2D(DEFAULT_TEXTURE_WIDTH, DEFAULT_TEXTURE_HEIGHT, TextureFormat.RGBA32, false, false);
				for(int x=0; x<DEFAULT_TEXTURE_WIDTH; x++)
					for(int y=0; y<DEFAULT_TEXTURE_HEIGHT; y++)
						m_tex.SetPixel(x, y, new Color(1, 1, 1, 1));
            }
			// Or texture is specified
			else
            {
				m_tex = tex;
            }
        }

		/// <summary>
		/// Call this when Sprite should be updated.
		/// SpriteChangeReceiver will receive this change message.
		/// </summary>
		public void UpdateSprite()
        {
			// TODO: this maybe useless
			// Upload m_tex to GPU
			m_tex.Apply();

            // Create Sprite & Bind Texture to Sprite
            m_sprite = Sprite.Create(m_tex, new Rect(0, 0, m_tex.width, m_tex.height), m_pivot, m_pixelsPerUnit);

			// Bind Sprite to SpriteRender
			SpriteRenderer render = GetComponent<SpriteRenderer>();
			if(render)
				render.sprite = m_sprite;

			// Call all SpriteChangeReceiver
			ISpriteChangeReceiver[] recvs = gameObject.GetComponents<ISpriteChangeReceiver>();
			foreach(ISpriteChangeReceiver recv in recvs)
				recv.OnSpriteChanged(m_sprite);
        }

    }
}

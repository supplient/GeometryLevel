using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace geo_level
{
	public class SpriteGenerator : MonoBehaviour
	{
		// Const Parameters
		public const int TEXTURE_WIDTH  = 180;
		public const int TEXTURE_HEIGHT = 180;

		// Property
		public Texture2D m_tex;
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
				m_tex = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBA32, false, false);
				for(int x=0; x<TEXTURE_WIDTH; x++)
					for(int y=0; y<TEXTURE_HEIGHT; y++)
						m_tex.SetPixel(x, y, new Color32(255, 255, 255, 255));
            }
			// Or texture is specified, we check if it is valid
			else
            {
				Debug.Assert(m_tex.width  == TEXTURE_WIDTH );
				Debug.Assert(m_tex.height == TEXTURE_HEIGHT);
				m_tex = tex;
            }

			// TODO: this maybe useless
			// Upload m_tex to GPU
			m_tex.Apply();
        }

		/// <summary>
		/// Call this when Sprite should be updated.
		/// SpriteChangeReceiver will receive this change message.
		/// </summary>
		public void UpdateSprite()
        {
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

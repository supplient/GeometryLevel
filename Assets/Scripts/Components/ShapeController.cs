using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{


    public class ShapeController : MonoBehaviour
    {
        // Property
        public float m_pixelsPerUnit = 3.0f;
        public Vector2 m_pivot = new Vector2(0.5f, 0.5f);

        // Dependent State: states not change spontaneously
        // Dependent State - Pixel
        private Texture2D m_originTex;
        private int m_texWidth;
        private int m_texHeight;
        private Texture2D m_tex;

        // Dependent State - Geometry
        private Sprite m_sprite;

        // Volatile State: states may suddenly change in runtime
        // Volatile State - Pixel
        private List<List<bool>> m_mask;
        private List<List<int>> m_pathsInPixel;

        // Component ref
        private SpriteRenderer m_spriteRender;
        private PolygonCollider2D m_collider;

        private void OnMaskChange()
        {
            UpdateTexture();
            UpdateCollider();
        }

        private void UpdateTexture()
        {
            // Update m_tex: m_tex[y][x] = m_mask[y][x] ? m_originTex[y][x] : empty
            for (int x = 0; x < m_texWidth; x++)
            {
                for (int y = 0; y < m_texHeight; y++)
                {
                    Color32 color;
                    if (m_mask[y][x])
                        color = m_originTex.GetPixel(x, y);
                    else
                        color = new Color32(0, 0, 0, 0);
                    m_tex.SetPixel(x, y, color);
                }
            }

            // Upload m_tex to GPU
            m_tex.Apply();
        }

        private void UpdateCollider()
        {
            // Trace the texture to get outline for collider
            m_pathsInPixel = Potrace.Trace(m_tex);

            // Convert pathsInPixel into pathsInUnit: pixel->unit & move (0,0) to pivot
            List<List<Vector2>> pathsInUnit = new List<List<Vector2>>();
            float centerx = m_texWidth * m_pivot.x;
            float centery = m_texWidth * m_pivot.y;
            foreach (List<int> pathInPixel in m_pathsInPixel)
            {
                Debug.Assert(pathInPixel.Count % 2 == 0);

                List<Vector2> pathInUnit = new List<Vector2>(pathInPixel.Count / 2);
                for (int i = 0; 2 * i < pathInPixel.Count; i++)
                {
                    int x = pathInPixel[2 * i];
                    int y = pathInPixel[2 * i + 1];
                    float ux = (x - centerx) / m_pixelsPerUnit;
                    float uy = (y - centery) / m_pixelsPerUnit;
                    pathInUnit.Add(new Vector2(ux, uy));
                }
                pathsInUnit.Add(pathInUnit);
            }

            // Update Collider
            m_collider.pathCount = pathsInUnit.Count;
            for (int i = 0; i < pathsInUnit.Count; i++)
                m_collider.SetPath(i, pathsInUnit[i]);
        }

        // Start is called before the first frame update
        void Start()
        {
            /* Texutre Init */
            // Load m_originTex
            {
                m_texWidth = 8;
                m_texHeight = 8;
                m_originTex = new Texture2D(m_texWidth, m_texHeight, TextureFormat.RGBA32, false, false);
                for (int x = 0; x < m_texWidth; x++)
                    for (int y = 0; y < m_texHeight; y++)
                        m_originTex.SetPixel(x, y, new Color32(255, 255, 255, 255));
            }

            // Create m_tex
            m_tex = new Texture2D(m_texWidth, m_texHeight, TextureFormat.RGBA32, false, false);


            /* Sprite Init */
            // Create Sprite & Bind Texture to Sprite
            m_sprite = Sprite.Create(m_tex, new Rect(0, 0, m_tex.width, m_tex.height), m_pivot, m_pixelsPerUnit);

            // Bind Sprite to SpriteRender
            m_spriteRender = GetComponent<SpriteRenderer>();
            m_spriteRender.sprite = m_sprite;


            /* Collider Init */
            // Load Collider
            m_collider = GetComponent<PolygonCollider2D>();

            // Create m_pathsInPixel
            m_pathsInPixel = new List<List<int>>();



            /* Mask Init */
            // Create m_mask
            m_mask = new List<List<bool>>();

            // DEBUG Now just fill mask
            {
                int[,] mask = new int[8, 8]
                {
               { 0, 0, 0, 0, 0, 0, 0, 0, },
               { 0, 1, 0, 0, 0, 0, 1, 0, },
               { 0, 1, 0, 0, 0, 0, 1, 0, },
               { 0, 1, 0, 0, 0, 0, 1, 0, },
               { 0, 1, 0, 0, 0, 0, 1, 0, },
               { 0, 1, 0, 0, 0, 0, 1, 0, },
               { 0, 1, 1, 1, 1, 1, 1, 0, },
               { 0, 0, 0, 0, 0, 0, 0, 0, },
                };

                m_mask.Clear();
                for (int y = 0; y < m_texHeight; y++)
                {
                    List<bool> row = new List<bool>();
                    for (int x = 0; x < m_texWidth; x++)
                    {
                        row.Add(mask[y, x] == 1);
                    }
                    m_mask.Add(row);
                }
            }

            // Update Texutre&Collider to respond mask's change
            OnMaskChange();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.name != "Circle")
                return;

            /* Mask Update */
            // DEBUG Now just fill mask
            {
                int[,] mask = new int[8, 8]
                {
               { 0, 0, 0, 0, 0, 0, 0, 0, },
               { 0, 1, 1, 1, 1, 1, 1, 0, },
               { 0, 1, 0, 0, 0, 0, 1, 0, },
               { 0, 1, 0, 0, 0, 0, 1, 0, },
               { 0, 1, 0, 0, 0, 0, 1, 0, },
               { 0, 1, 0, 0, 0, 0, 1, 0, },
               { 0, 1, 1, 1, 1, 1, 1, 0, },
               { 0, 0, 0, 0, 0, 0, 0, 0, },
                };

                m_mask.Clear();
                for (int y = 0; y < m_texHeight; y++)
                {
                    List<bool> row = new List<bool>();
                    for (int x = 0; x < m_texWidth; x++)
                    {
                        row.Add(mask[y, x] == 1);
                    }
                    m_mask.Add(row);
                }
            }

            // Update Texture&Collider to respond mask's change
            OnMaskChange();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }



}

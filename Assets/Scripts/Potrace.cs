using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace geo_level
{
    public class Potrace
    {
        [DllImport("Potrace_Debug")]
        private static extern unsafe int trace_onlypolygon_wrap(byte* inarray, int inw, int inh, int* outarray, uint outMaxLen);

        public const int TEX_MAX_SIZE = 1000 * 1000;
        private static unsafe byte[] s_texbuf = new byte[TEX_MAX_SIZE];
        public const int PATHS_MAX_LEN = 2 * 5000;
        private static unsafe int[] s_pathbuf = new int[PATHS_MAX_LEN];
        
        public static List<List<int>> Trace(Texture2D tex)
        {
            if (tex.width * tex.height > TEX_MAX_SIZE)
                Debug.LogError(String.Format("Texture is too large of {0} size, while allowed max size is {1}.", tex.width * tex.height, TEX_MAX_SIZE));

            // Fill texbuf for input by tex
            Color32[] colors = tex.GetPixels32();
            for(int y=0; y<tex.height; y++)
            {
                for(int x=0; x<tex.width; x++)
                {
                    int i = y * tex.width + x;
                    s_texbuf[i] = colors[i].a;
                }
            }

            // Trace by call cpp dll, result will be filled into s_pathbuf
            unsafe
            {
                fixed(int* p_pathbuf = s_pathbuf)
                {
                    fixed(byte* p_texbuf = s_texbuf)
                    {
                        if (trace_onlypolygon_wrap(p_texbuf, tex.width, tex.height, p_pathbuf, PATHS_MAX_LEN) != 0)
                            Debug.LogError("Something when calling potrace.");
                    }
                }
            }

            // Load paths from s_pathbuf
            int INVALID = tex.width + 1;
            List<List<int>> paths = new List<List<int>>();
            int oi = 0;
            while(s_pathbuf[oi] != INVALID)
            {
                List<int> path = new List<int>();
                while(s_pathbuf[oi] != INVALID)
                {
                    path.Add(s_pathbuf[oi]);
                    path.Add(s_pathbuf[oi+1]);
                    oi += 2;
                }
                oi++;
                paths.Add(path);
            }

            return paths;
        }
    }
}

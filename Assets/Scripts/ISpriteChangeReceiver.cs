using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace geo_level
{
    interface ISpriteChangeReceiver
    {
        /// <summary>
        /// Should be called when the Sprite is changed.
        /// </summary>
        void OnSpriteChanged(Sprite sprite);
    }
}

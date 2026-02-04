using UnityEngine;

namespace CodeSketch.Core.Extensions
{
    public static class CSKExtensionsColor
    {
        public static float Magnitude(this Color color)
        {
            return color.r + color.g + color.b + color.a;
        }
    }
}

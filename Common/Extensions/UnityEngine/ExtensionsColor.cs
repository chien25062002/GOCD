using UnityEngine;

namespace GOCD.Framework
{
    public static class ExtensionsColor
    {
        public static float Magnitude(this Color color)
        {
            return color.r + color.g + color.b + color.a;
        }
    }
}

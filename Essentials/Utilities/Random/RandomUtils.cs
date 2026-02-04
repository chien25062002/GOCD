using UnityEngine;

namespace CFramework
{
    public static class RandomUtils
    {
        public static Direction2D Direction2D()
        {
            return Random.Range(0, 2) == 0 ? CFramework.Direction2D.Left : CFramework.Direction2D.Right;
        }
        
        public static Direction4D Direction4D()
        {
            int rdValue = Random.Range(1, 5);
            switch (rdValue)
            {
                case 1:
                    return CFramework.Direction4D.Left;
                case 2:
                    return CFramework.Direction4D.Right;
                case 3:
                    return CFramework.Direction4D.Forward;
                case 4:
                    return CFramework.Direction4D.Backward;
            }
            
            return CFramework.Direction4D.Left;
        }
    }
}

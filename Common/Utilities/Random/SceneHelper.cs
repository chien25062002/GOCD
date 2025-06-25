using UnityEngine.SceneManagement;

namespace CFramework
{
    public static class SceneHelper
    {
        public static bool IsScene(string name)
        {
           return SceneManager.GetActiveScene().name.Equals(name);
        }
    }
}

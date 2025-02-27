using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Scene
{
    public class SceneLoader 
    {
        public enum Scene
        {
            Menu,
            Lobby,
            Game
        }

        public static void Load(Scene targetScene)
        {
            SceneManager.LoadScene(targetScene.ToString());
        }
        public static void LoadNetwork(Scene targetScene)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
        }
    }
}

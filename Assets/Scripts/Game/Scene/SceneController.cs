using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Manager
{
    public class SceneController : MonoBehaviour
    {
        public void GameStart()
        {
            SceneManager.LoadScene("Game");
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ToBeContinued : MonoBehaviour
{
    private void Start()
    {
       
        if (SceneManager.GetActiveScene().buildIndex == 5)
        {
            StartCoroutine(RestartGame(3f));
        }
       
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(LoadTBCAfterDelay(3f)); 
        }
    }

    private IEnumerator LoadTBCAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    private IEnumerator RestartGame(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reload the current scene
        SceneManager.LoadScene(0);
    }
}
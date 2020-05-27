using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Resets the scene by loading it again
 */
public class ResetScript : MonoBehaviour
{
    public void ResetScene()
    {
        SceneManager.LoadScene("InteriorDesign");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * CustomDropDown is used to create a menu of the items in echoAR console.
 * Allows user to select one of these items to instantiate next.
 */
public class CustomDropDown : MonoBehaviour
{
    private Dropdown dropdown;
    
    private void Awake()
    {
        dropdown = GetComponent<Dropdown>();
        dropdown.GetComponent<Dropdown>().ClearOptions();
    }

    /*
     * Adds options to the dropdown menu
     * Options are read from CustomTouchBehavior script's consoleObjects list.
     * This is done only once, but must be called in Update() because it takes time for echoAR models to be loaded in.
     */
    void PopulateDropDownOptions()
    {
        if (dropdown.options.Count == 0)
        {
            GameObject arSessionOrigin = GameObject.Find("AR Session Origin");
            CustomTouchBehavior touchScript = arSessionOrigin.GetComponent<CustomTouchBehavior>();
            List<string> options = new List<string>();
            foreach (string option in touchScript.consoleObjects)
            {
                options.Add(option);
            }
            dropdown.AddOptions(options);
        }
    }

    /*
     * Called every frame
     */
    void Update()
    {
        PopulateDropDownOptions();
    }

    /*
     * This method is called on dropdown value change
     * The "current" field in the CustomtouchBehavior script is updated
     * current denotes the echoAR model that will be instantiated next
     */
    public void updateCurrent()
    {
        GameObject arSessionOrigin = GameObject.Find("AR Session Origin");
        CustomTouchBehavior touchScript = arSessionOrigin.GetComponent<CustomTouchBehavior>();
        touchScript.current = dropdown.value;
    }
}

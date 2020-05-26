using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * CustomSlider is used to create a slider that rotates the most recently grabbed object.
 * Issues:
 * When echoAR objects have been shifted in position using x,y,z keys in the console,
 * the pivot or origin of each object does not change. Rather, only the relative position changes.
 * Therefore, when rotating the echoAR object with a simple rotation, it does not rotate around the correct pivot.
 */
public class CustomSlider : MonoBehaviour
{
    private Slider slider;
    private GameObject toRotate;


    private void Awake()
    {
        slider = GetComponent<Slider>();
    }


    /*
     * Gets the most recently grabbed object from CustomTouchBehavior script
     */
    private void getMostRecentlyGrabbedObject()
    {
        GameObject arSessionOrigin = GameObject.Find("AR Session Origin");
        CustomTouchBehavior touchScript = arSessionOrigin.GetComponent<CustomTouchBehavior>();
        toRotate = touchScript.grabbedObject;
    }


    /*
     * A simple rotation transformation depending on the value of the slider
     */
    public void RotatingObject(){
        getMostRecentlyGrabbedObject();
        if(toRotate != null)
        {
            toRotate.transform.rotation = Quaternion.Euler(toRotate.transform.rotation.x, toRotate.transform.rotation.y + slider.value, toRotate.transform.rotation.z);
            toRotate.name = slider.value.ToString();
        }
    }


    /*
     * Sets the slider object components to be inactive/active
     * This is used to deactivate the slider before any object has been grabbed and activate it again once an object is first grabbed.
     */
    private void recursivelyToggle(Transform root, bool value)
    {
        foreach (Transform t in root)
        {
            t.gameObject.SetActive(value);
            recursivelyToggle(t, value);
        }
    }

    
    /*
     * Called every frame
     */
    private void Update()
    {
        //Check to see if any object has been grabbed
        if (toRotate == null) 
        {
            getMostRecentlyGrabbedObject(); 
        }

        
        if(toRotate == null)
        {
            //If still no object has been grabbed, deactivate the slider
            slider.interactable = false;
            recursivelyToggle(slider.transform, false);
        } else
        {
            //If an object has been grabbed, reactivate the slider
            slider.interactable = true;
            recursivelyToggle(slider.transform, true);

            //Change the text displayed next to the slider (Rotation: [angle])
            Debug.Log(slider.value.ToString());
            GameObject sliderText = GameObject.Find("Slider Text");
            Text texts = sliderText.GetComponent<Text>();
            texts.text = "Rotation: " + slider.value.ToString() + "\u00B0";
        }
    }
}

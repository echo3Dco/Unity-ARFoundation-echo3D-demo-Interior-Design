using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * CustomSlider is used to create a slider that rotates the most recently grabbed object.
 * Note:
 * When echoAR objects have been shifted in position using x,y,z keys in the console,
 * the pivot or origin of each object does not change. Rather, only the relative position changes.
 * Therefore, when rotating the echoAR object with a simple rotation, it does not rotate around the correct pivot.
 */
public class CustomSlider : MonoBehaviour
{
    private Slider slider;
    private GameObject toRotate;
    private bool sliderToggle = false;
    private float prevSliderVal = 0.0f;


    private void Awake()
    {
        slider = GetComponent<Slider>();

        //Initial text
        GameObject sliderText = GameObject.Find("Slider Text");
        Text texts = sliderText.GetComponent<Text>();
        texts.text = "Rotation: " + slider.value.ToString() + "\u00B0";

        //deactivate the slider at the start because no object is grabbed
        slider.interactable = sliderToggle;
        recursivelyToggle(slider.transform, sliderToggle);
    }


    /*
     * Gets the most recently grabbed object from CustomTouchBehavior script
     */
    private GameObject getMostRecentlyGrabbedObject()
    {
        GameObject arSessionOrigin = GameObject.Find("AR Session Origin");
        CustomTouchBehavior touchScript = arSessionOrigin.GetComponent<CustomTouchBehavior>();
        return touchScript.grabbedObject;
    }


    /*
     * A simple rotation transformation depending on the value of the slider
     * Triggered on slider value change
     */
    public void RotatingObject(){
        toRotate.transform.localEulerAngles = new Vector3(toRotate.transform.localEulerAngles.x, toRotate.transform.localEulerAngles.y + slider.value - prevSliderVal, toRotate.transform.localEulerAngles.z);
        prevSliderVal = slider.value;

        //Change the text displayed next to the slider (Rotation: [angle])
        GameObject sliderText = GameObject.Find("Slider Text");
        Text texts = sliderText.GetComponent<Text>();
        texts.text = "Rotation: " + slider.value.ToString() + "\u00B0";
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
        //Resets the slider whenever a new object is grabbed
        GameObject newToRotate = getMostRecentlyGrabbedObject();
        if(toRotate != newToRotate)
        {
            toRotate = newToRotate;
            prevSliderVal = 0;
            slider.value = 0;
        }

        //If its the very first time an object got grabbed, turn on the slider
        if (sliderToggle == false && toRotate != null)
        {
            //If an object has been grabbed, reactivate the slider
            sliderToggle = true;
            slider.interactable = sliderToggle;
            recursivelyToggle(slider.transform, sliderToggle);
        }
    }
}

/**********te****************************************************************
* Copyright (C) echoAR, Inc. 2018-2020.                                   *
* echoAR, Inc. proprietary and confidential.                              *
*                                                                         *
* Use subject to the terms of the Terms of Service available at           *
* https://www.echoar.xyz/terms, or another agreement                      *
* between echoAR, Inc. and you, your company or other organization.       *
***************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
 * CustomBehavior script is run for each model in the echoAR console whenever an echoAR object is instantiated
 */
public class CustomBehaviour : MonoBehaviour
{
    [HideInInspector]
    public Entry entry;


    void Start()
    {
        // Add RemoteTransformations script to object and set its entry
        this.gameObject.AddComponent<RemoteTransformations>().entry = entry;
        
        //Get "current" from CustomTouchBehavior script
        GameObject arSessionOrigin = GameObject.Find("AR Session Origin");
        string identifier = this.gameObject.name;
        CustomTouchBehavior touchScript = arSessionOrigin.GetComponent<CustomTouchBehavior>();
        int count = touchScript.consoleObjects.Count;
        int index = touchScript.current;

        /*
         * Destroy this object if it is not the "current" object
         * If this is done for every model in the echoAR console, only the current object will remain
         */
        if (count != 0 && index >= 0 && index < count && identifier != touchScript.consoleObjects[index])
        {
            Destroy(this.gameObject);
        }
    }

    
    // Update is called once per frame
    void Update()
    {
     
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class CustomTouchBehavior : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    [SerializeField]
    private Camera arCamera; //Used for Raycasting

    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    public GameObject spawnedObject { get; private set; } 
    private string placedTag = "placed"; //Used for easily identifying instantiated echoAR objects

    private RaycastHit hitObject;
    private Pose hitPose;

    private bool grabbed = false;
    public GameObject grabbedObject; //The most recently grabbed object
    private Vector3 offset; //Translation offset
    private bool grabbedWithTwo;
    private Quaternion grabbedRotation;

    public List<string> consoleObjects = new List<string>(); //List of objects placed in echoAR console
    public int current = -1; //Index of object to instantiate next
  


    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();

        //spawn an echoAR object to get data from the console. (Will be destroyed immediately)
        spawnedObject = Instantiate(m_PlacedPrefab); 
    }


    /*
     * Instantiate an object, recursively tag it, and recursively add MeshCollider
     */
    void placeObjectWithTagAndCollider(Pose pose, string theTag) //
    {
        spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
        recursivelyTag(spawnedObject, theTag);
        recursivelyCollide(spawnedObject);
    }


    /*
     * Tag the object and its children (recursive)
     */
    void recursivelyTag(GameObject toTag, string theTag)
    {
        toTag.tag = theTag;
        foreach (Transform t in toTag.transform)
        {
            recursivelyTag(t.gameObject, theTag);
        }
    }


    /*
     * Attach MeshCollider to object and its children (recursive)
     * Needed for raycasting. All children need meshcolliders because a 3D model might be made of many children.
     */
    void recursivelyCollide(GameObject toAttach)
    {
        if(toAttach.GetComponent<MeshCollider>() == null)
        {
            toAttach.AddComponent<MeshCollider>();
        }
        
        foreach (Transform t in toAttach.transform)
        {
            recursivelyCollide(t.gameObject);
        }
    }


    /*
     * Find the "root" of a child object and set to grabbedObject
     * If a component of a 3D model is grabbed, the whole 3D model must be grabbed, not just the component.
     */
    void findParent(GameObject childObject)
    {
        /*
        Transform t = childObject.transform;
        while(t.parent.parent != null)
        {
            if(t.parent.parent.CompareTag(placedTag))
            {
                grabbedObject = t.parent.gameObject;
                return;
            }
            t = t.parent.transform;
        }
        */
        grabbedObject = childObject.transform.parent.gameObject;
    }


    /*
     * Removes shadows from object and its children (recursive)
     * Shadows are arbitrary in AR since light sources will change depending on real-world settings
     */
    void recursivelyRemoveShadows(GameObject toChange)
    {
        if (toChange.GetComponent<MeshRenderer>() != null)
        {
            toChange.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            toChange.GetComponent<MeshRenderer>().receiveShadows = false;
        }

        foreach (Transform t in toChange.transform)
        {
            recursivelyRemoveShadows(t.gameObject);
        }
    }


    /*
     * Retrieves the names of all objects in the echoAR console
     * Populates the consoleList with these names
     * Destroys the spawnedObject that was instantiated in Awake() because it is only used to populate consoleList 
     * This method will only make changes to consoleObjects if it's the first time trying to populate it
     */
    void populateConsoleList()
    {
        if(consoleObjects.Count == 0) // or current == -1
        {
            foreach (Entry entry in echoAR.dbObject.getEntries())
            {
                if (entry.getHologram().getType().Equals(Hologram.hologramType.MODEL_HOLOGRAM))
                {
                    ModelHologram modelHologram = (ModelHologram)entry.getHologram();
                    consoleObjects.Add(modelHologram.getFilename());
                }
            }
            Destroy(spawnedObject);
            current = 0;
        }
    }
    

    /*
     * Detects if a touch is over UI
     * Without this, a UI touch could register as a touch on a plane from AR Plane Manager
     */
    bool isOverUI(Vector2 touchPosition)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        PointerEventData eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = new Vector2(touchPosition.x, touchPosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);

        return results.Count > 0;
    }


    bool controllingUI = false;
    
    /*
     * Called every frame
     */
    void Update()
    {
        populateConsoleList();

        if (spawnedObject != null)
        {
            recursivelyTag(spawnedObject, placedTag);
            recursivelyCollide(spawnedObject);
            recursivelyRemoveShadows(spawnedObject);
        }
        
        if (Input.touchCount > 0) //If there's a touch
        {
            Touch touch = Input.GetTouch(0);

            if (!grabbed)
            {
                if (!controllingUI)
                {
                    controllingUI = isOverUI(touch.position); //Only check again if controllingUI is false. If controllingUI is true, continue controlling the UI
                }
                if (controllingUI)
                {
                    return; //return, so that no transformation is done to any objects and no new objects are created
                }
            }
            
            if (touch.phase == TouchPhase.Began) //If first touch
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out hitObject)) //If the touch hit something i.e. anything with collider (including Planes)
                {
                    if (hitObject.transform.gameObject.CompareTag(placedTag)) //If a placed object was touched
                    {
                        findParent(hitObject.transform.gameObject); //Grab the object that was touched and its parent
                        grabbed = true;
                        m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon);
                        offset = grabbedObject.transform.position - s_Hits[0].pose.position; //An offset vector used for smoother translation interaction
                    }
                    else if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon)) //If a plane was touched
                    {
                        hitPose = s_Hits[0].pose;
                        placeObjectWithTagAndCollider(hitPose, placedTag);
                        Physics.Raycast(ray, out hitObject);
                        grabbedObject = spawnedObject;
                        grabbed = true;
                        offset = new Vector3(0,0,0);
                    }
                }else
                {
                    grabbed = false;
                }
            }

            else //If not first touch
            {
                if(grabbed) //If grabbing right now, translate the grabbed object 
                {
                    if(Input.touchCount == 1 && grabbedWithTwo == true)
                    {
                        grabbedWithTwo = false;
                    }

                    if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon)) //Raycast to the planes
                    {
                        hitPose = s_Hits[0].pose;
                        grabbedObject.transform.position = hitPose.position + offset; //Translate grabbedObject to wherever the touch hits a plane
                    }

                    if(Input.touchCount == 2)
                    {
                        if(grabbedWithTwo == false)
                        {
                            grabbedRotation = grabbedObject.transform.rotation;
                            grabbedWithTwo = true;
                        }


                        /*
                         * Scale transformation:
                         * Code is from Unity AR Foundation examples
                         */
                        Touch touchZero = Input.GetTouch(0);
                        Touch touchOne = Input.GetTouch(1);
                        // Calculate previous position
                        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                        // Find the magnitude of the vector (the distance) between the touches in each frame.
                        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                        // Find the difference in the distances between each frame.
                        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                        float pinchAmount = deltaMagnitudeDiff * 0.02f * Time.deltaTime;
                        grabbedObject.transform.localScale -= new Vector3(pinchAmount, pinchAmount, pinchAmount);
                        // Clamp scale
                        float Min = 0.005f;
                        float Max = 3f;
                        grabbedObject.transform.localScale = new Vector3(
                            Mathf.Clamp(grabbedObject.transform.localScale.x, Min, Max),
                            Mathf.Clamp(grabbedObject.transform.localScale.y, Min, Max),
                            Mathf.Clamp(grabbedObject.transform.localScale.z, Min, Max)
                        );
                    }
                }
            }
        } else
        {
            controllingUI = false;
            grabbed = false;
        }     
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    ARRaycastManager m_RaycastManager;
}
using System;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class UIDocumentWorldSpace : MonoBehaviour
{
    public enum VerHor
    {
        Vertical,
        Horizontal
    }

    //Inspector Variables

    [Space(10, order = 0)]
    [Header("Document", order = 1)]

    //Document settings
    [SerializeField] VisualTreeAsset treeAsset;
    [SerializeField] PanelSettings panelSettings;
    public int sortOrder = 0;

    [Space(10, order = 2)] 
    [Header("Transform Settings", order = 3)]

    //Scale/size settings
    public Camera targetCamera;
    public VerHor referenceDimension;
    public float unitSize;

    [Space(10, order = 4)]
    [Header("Draw In Editor", order = 5)]

    [SerializeField] bool drawInEditor;

    //=================

    UIDocument    document;
    VisualElement panelElement;
    PanelSettings instanceSettings;
    PanelSettings lastPanelSettings;

    string  hashString;
    VerHor  LastRefDimension;
    Vector3 lastScale;
    float   lastUnitSize;

    float refDimensionOrthoSize;
    float refDimensionMax;
    float refDimensionValue;
    int   panelScaleValue;

    bool runTest  = false;
    bool validate = false;

    private void Start()
    {
        hashString = getHashString();
    }


    /// <summary>
    /// Updates instance PanelSettings to scale with reference unit size, camera orthographic size, local scale, and panel dimensions.
    /// </summary>
    void SetScaleByRef()
    {
        //Set scale by reference to unit size
        instanceSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;

        //Vertical Scale
        if (referenceDimension == VerHor.Vertical)
        {
            refDimensionOrthoSize = targetCamera.orthographicSize * 2f;
            refDimensionValue = panelElement.resolvedStyle.height;

            //Match vertical
            instanceSettings.match = 1f;
        }
        //Horizontal Scale
        else
        {
            refDimensionOrthoSize = targetCamera.orthographicSize * 2f * targetCamera.aspect;
            refDimensionValue = panelElement.resolvedStyle.width;

            //Match horizontal
            instanceSettings.match = 0f;
        }

        //Modify resolution of PanelSettings instance
        refDimensionMax = refDimensionOrthoSize * refDimensionValue;
        panelScaleValue = Mathf.RoundToInt(refDimensionMax / (unitSize * transform.localScale.z));
        instanceSettings.referenceResolution = new Vector2Int(panelScaleValue, panelScaleValue);

    }


    /// <summary>
    /// Draws bounding box for panel using Debug.Line, taking into account rotation.
    /// </summary>
    void DrawDimensions()
    {
        //Get aspect ratio of panel dimensions, set width/height
        float panelAspect = panelElement.resolvedStyle.width / panelElement.resolvedStyle.height;
        float x = unitSize * transform.localScale.z;
        float y = x;

        // X = Y * Aspect ratio, Y = X / Aspect ratio
        if (referenceDimension == VerHor.Horizontal) 
            y /= panelAspect;
        else 
            x *= panelAspect;

        //Get rotation, clamp/mod between 0 and 360
        
        float offsetX = transform.position.x; //TO-DO (anchorPoint.x * (x / 2f) * -1f) + transform.position.x;
        float offsetY = transform.position.y; //TO-DO (anchorPoint.y * (y / 2f) * -1f) + transform.position.y;

        //Half of full width/height + direction (via +/-)
        float tlX = -(x / 2f) + offsetX;
        float tlY =  (y / 2f) + offsetY;

        float blX = -(x / 2f) + offsetX;
        float blY = -(y / 2f) + offsetY;

        float trX =  (x / 2f) + offsetX;
        float trY =  (y / 2f) + offsetY;

        float brX =  (x / 2f) + offsetX;
        float brY = -(y / 2f) + offsetY;

        //Each corner position is (Rotation * the direction (corner - center)) + transform.position
        //Draw debug lines between each corner, and between each corner and center

        Debug.DrawLine(transform.position, (transform.rotation * (new Vector3(tlX, tlY, 0f) - transform.position)) + transform.position, Color.blue, Time.deltaTime);
        Debug.DrawLine(transform.position, (transform.rotation * (new Vector3(trX, trY, 0f) - transform.position)) + transform.position, Color.blue, Time.deltaTime);
        Debug.DrawLine(transform.position, (transform.rotation * (new Vector3(blX, blY, 0f) - transform.position)) + transform.position, Color.blue, Time.deltaTime);
        Debug.DrawLine(transform.position, (transform.rotation * (new Vector3(brX, brY, 0f) - transform.position)) + transform.position, Color.blue, Time.deltaTime);

        Debug.DrawLine(
            (transform.rotation * (new Vector3(tlX, tlY, 0f) - transform.position)) + transform.position, 
            (transform.rotation * (new Vector3(blX, blY, 0f) - transform.position)) + transform.position,
            Color.red, Time.deltaTime
        );
        Debug.DrawLine(
            (transform.rotation * (new Vector3(brX, brY, 0f) - transform.position)) + transform.position, 
            (transform.rotation * (new Vector3(blX, blY, 0f) - transform.position)) + transform.position,
            Color.red, Time.deltaTime
        );
        Debug.DrawLine(
            (transform.rotation * (new Vector3(brX, brY, 0f) - transform.position)) + transform.position, 
            (transform.rotation * (new Vector3(trX, trY, 0f) - transform.position)) + transform.position,
            Color.red, Time.deltaTime
        );
        Debug.DrawLine(
            (transform.rotation * (new Vector3(tlX, tlY, 0f) - transform.position)) + transform.position, 
            (transform.rotation * (new Vector3(trX, trY, 0f) - transform.position)) + transform.position,
            Color.red, Time.deltaTime
        );
    }


    /// <summary>
    /// Updates position and scale in relation to reference settings.
    /// </summary>
    private void UpdateTransform()
    {
        SetPosition();
        SetScaleByRef();
    }


    /// <summary>
    /// Updates position and scale in relation to reference settings.
    /// </summary>
    /// <returns>Returns a string of the combined hashes from required components/variables</returns>
    string getHashString()
    {
        string hashstr = "";
        if (treeAsset != null) hashstr += treeAsset.GetHashCode().ToString();
        else hashstr += "nullTA";
        if (panelSettings != null) hashstr += panelSettings.GetHashCode().ToString();
        else hashstr += "nullPS";
        hashstr += sortOrder.ToString();
        return hashstr;
    }


    /// <summary>
    /// Checks if the desired scale, reference unit size, or reference dimension has been changed.
    /// </summary>
    /// <returns>Returns true if any of the checked values has changed.</returns>
    bool scaleChanged()
    {
        bool haschanged = lastScale != transform.localScale;
        haschanged = haschanged || lastUnitSize != unitSize;
        haschanged = haschanged || LastRefDimension != referenceDimension;

        if (haschanged)
        {
            lastScale = transform.localScale;
            lastUnitSize = unitSize;
            LastRefDimension = referenceDimension;
        }

        return haschanged;
    }


    /// <summary>
    /// Checks if the combined hash string for the panel/settings/components has changed.
    /// </summary>
    /// <returns>Returns true if any of the checked values has changed.</returns>
    bool settingsChanged()
    {
        string currentHashString = getHashString();
        if (hashString != currentHashString)
        {
            hashString = currentHashString;
            return true;
        }
        return false;
    }


    /// <summary>
    /// Initializes member objects/components and manages existing components/initiated components.
    /// </summary>
    void InitializeMembers()
    {
        UIDocument lastDocument = null;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (document != null)
        {
            lastDocument = document;
        }
        else
        {
            if (GetComponent<UIDocument>() != null)
            {
                DestroyImmediate(GetComponent<UIDocument>());
            }
            document = gameObject.AddComponent<UIDocument>();
            document.hideFlags = HideFlags.HideInInspector;
        }

        if (instanceSettings == null || document.visualTreeAsset == null)
        {
            instanceSettings = PanelSettings.Instantiate<PanelSettings>(panelSettings);
            document.visualTreeAsset = treeAsset;
            document.sortingOrder = sortOrder;
            document.panelSettings = instanceSettings;
        }

        if (panelElement == null || panelElement.panel == null)
        {
            try
            {
                panelElement = GetComponent<UIDocument>().rootVisualElement.Q("Container");
            }
            catch
            {
                panelElement = GetComponent<UIDocument>().rootVisualElement;
            }
        }
        UpdateTransform();
    }


    /// <summary>
    /// Updates the panel element's position and rotation in relation to attached gameObject.
    /// </summary>
    public void SetPosition()
    {
        Vector2 newPosition = RuntimePanelUtils.CameraTransformWorldToPanel(
            panelElement.panel, 
            transform.position,
            targetCamera
        );

        newPosition.x = (newPosition.x - panelElement.layout.width / 2);
        newPosition.y = (newPosition.y - panelElement.layout.height / 2);
        panelElement.transform.position = newPosition;
        panelElement.transform.rotation = transform.rotation;
    }

    void OnValidate()
    {
        validate = true;
    }


    private void LateUpdate()
    {
        if (treeAsset != null && panelSettings != null)
        {
            try
            {
                if (settingsChanged() || scaleChanged() || validate)
                {
                    InitializeMembers();
                    validate = false;
                }
                SetScaleByRef();
                UpdateTransform();
                if (drawInEditor) DrawDimensions();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
                InitializeMembers();
            }
        }
    }

}

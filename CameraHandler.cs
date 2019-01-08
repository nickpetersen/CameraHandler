using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CameraHandler : MonoBehaviour {

    private static readonly float zoomDamping = 10;
    private float desiredZoomValue;
    private static readonly float PanSpeed = 80f;
    private static readonly float ZoomSpeedTouch = .8f;
    private static readonly float ZoomSpeedMouse = 10f;
    
    private static readonly float[] BoundsX = new float[]{-19000f, 60000f};
    private static readonly float[] BoundsZ = new float[]{-15000f, 15000f};
    private static readonly float[] ZoomBounds = new float[]{20f, 60f};
    
    private Camera cam;
    
    private Vector3 lastPanPosition;
    private int panFingerId; // Touch mode only
    
    private bool wasZoomingLastFrame; // Touch mode only
    private Vector2[] lastZoomPositions; // Touch mode only

    void Awake() {
        cam = GetComponent<Camera>();
        desiredZoomValue = cam.fieldOfView;
    }
    
    void Update() {
        // If it's not event system then handle mouse
        if(!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer) {
                HandleTouch();
            } else {
                HandleMouse();
            }
            if(desiredZoomValue != cam.fieldOfView) {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredZoomValue, Time.deltaTime * zoomDamping);
            }
        }
    }

    void ZoomCamera(float offset, float speed)
    {
    if (offset == 0)
    {
    return;
    }

    desiredZoomValue = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
    }
    
    void HandleTouch() {
        // If it's not event system then handle mouse
        if(!EventSystem.current.IsPointerOverGameObject()) {

            switch(Input.touchCount) {
        
            case 1: // Panning
                wasZoomingLastFrame = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                } else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved) {
                    PanCamera(touch.position);
                }
                break;
        
            case 2: // Zooming
                Vector2[] newPositions = new Vector2[]{Input.GetTouch(0).position, Input.GetTouch(1).position};
                if (!wasZoomingLastFrame) {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                } else {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;
        
                    ZoomCamera(offset, ZoomSpeedTouch);
        
                    lastZoomPositions = newPositions;
                }
                break;
                
            default: 
                wasZoomingLastFrame = false;
                break;
            }
        }
    }
    
    void HandleMouse() {
        // If it's not event system then handle mouse
        if(!EventSystem.current.IsPointerOverGameObject()) {
        // On mouse down, capture it's position.
        // Otherwise, if the mouse is still down, pan the camera.
        if (Input.GetMouseButtonDown(0)) {
            lastPanPosition = Input.mousePosition;
        } else if (Input.GetMouseButton(0)) {
            PanCamera(Input.mousePosition);
        }
        // Check for scrolling to zoom the camera
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, ZoomSpeedMouse);
        }
    }
    
    void PanCamera(Vector3 newPanPosition) {
        // Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);
        
        // Perform the movement
        transform.Translate(move, Space.World);  
        
        // Ensure the camera remains within bounds.
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);
        transform.position = pos;
    
        // Cache the position
        lastPanPosition = newPanPosition;
    }
}
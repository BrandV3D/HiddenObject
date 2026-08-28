//-----------------------------------------------------------------------------------------------------	
// Script allows to pan and zoom(pinch) camera to have access to small details in scene 
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("Scripts/Jigsaw Puzzle/Camera Controller")]
public class CameraController : MonoBehaviour 
{
	[Header("Zoom")]

	[Range (0f, 5f)]
	public float zoomSpeed = 0.5f;					// Zoom changing speed
	public Vector2 zoomLimits = new Vector2(3, -3); // Camera orthographicSize changing limits
	public bool doubleClickZooming = true;			// Enable/Disable Zooming by double-click/tap
	public bool disableZooming;				 		// Disable Zooming functionality


	[Header("Movement")]

	[Range (0f, 5f)]
	public float panSpeed = 0.5f;					// Panning speed
	public Vector2 panLimits = new Vector2(10, 10); // Camera x,y  position changing limits				
	public bool disablePanning;						// Disable Panning functionality


	// Important internal variables - please don't change them blindly
	float initialZoom;	
	float doubleClickMaxDelay = 0.3f;
	float doubleClickDelay;
	Vector2 initialPosition;
	Vector3 cameraNewPosition = new Vector3();



	//=======================================================================================================================================================================
	// Get initial data
	void Start () 
	{
		initialZoom = Camera.main.orthographicSize;      
		initialPosition = Camera.main.transform.position;
		cameraNewPosition = Camera.main.transform.position;
	}

	//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	// Process Zoom and Pan
	void LateUpdate ()
	{
		cameraNewPosition.z = Camera.main.transform.position.z;

		// Zooming
		if (!disableZooming) 
		{ 
			#if UNITY_EDITOR  ||  UNITY_STANDALONE  ||  UNITY_WEBPLAYER 
			// Mouse scroll zoom
				Camera.main.orthographicSize -= zoomSpeed * Input.GetAxis ("Mouse ScrollWheel");

			#else  // For touch-devices:  Pinch-zoom
				if (Input.touchCount > 1) 
				{
					// If there are two touches on the device... Store both touches.
					Touch touchZero = Input.GetTouch (0);
					Touch touchOne 	= Input.GetTouch (1);

					// Find the position in the previous frame of each touch.
					Vector3 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
					Vector3 touchOnePrevPos  = touchOne.position - touchOne.deltaPosition;

					// Find the magnitude of the vector (the distance) between the touches in each frame.
					float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
					float touchDeltaMag 	= (touchZero.position - touchOne.position).magnitude;

					// Find the difference in the distances between each frame.
					float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

					// Change the orthographic size based on the change in distance between the touches.
					Camera.main.orthographicSize += deltaMagnitudeDiff * zoomSpeed / 5;
				}
			#endif



			//Double-click(tap) zoom
			if (doubleClickZooming  &&  Input.GetMouseButtonUp (0)  &&  Time.timeScale > 0)
				if (doubleClickDelay > Time.time) 
				{
					if (Camera.main.orthographicSize < initialZoom)
						Camera.main.orthographicSize = initialZoom;
					else
						Camera.main.orthographicSize = initialZoom + zoomLimits.y;

					cameraNewPosition.x = Camera.main.ScreenToWorldPoint (Input.mousePosition).x;
					cameraNewPosition.y = Camera.main.ScreenToWorldPoint (Input.mousePosition).y;					

					doubleClickDelay = 0;
				} 
				else
					doubleClickDelay = Time.time + doubleClickMaxDelay;  



			// Check if Camera orthographicSize(zoom) is still within zoomLimits
			if (Camera.main.orthographicSize > initialZoom + zoomLimits.x)
				Camera.main.orthographicSize = initialZoom + zoomLimits.x;
			else 
				if (Camera.main.orthographicSize < initialZoom + zoomLimits.y)
					Camera.main.orthographicSize = initialZoom + zoomLimits.y;

		}    



		// Panning camera      
		if (!disablePanning) 
		{           
			float zoomModifier = Camera.main.orthographicSize / initialZoom;

			#if UNITY_EDITOR  ||  UNITY_STANDALONE  ||  UNITY_WEBPLAYER 
				if(Input.GetMouseButton(0))
				{
					float h = panSpeed * Input.GetAxis ("Mouse X");
					float v = panSpeed * Input.GetAxis ("Mouse Y");

					if (Mathf.Abs(h) > 0.1) 
						cameraNewPosition.x = Mathf.Clamp (Camera.main.transform.position.x - h*zoomModifier, initialPosition.x - panLimits.x, initialPosition.x + panLimits.x);

					if (Mathf.Abs(v) > 0.1) 
						cameraNewPosition.y = Mathf.Clamp (Camera.main.transform.position.y - v*zoomModifier, initialPosition.y - panLimits.y, initialPosition.y + panLimits.y);
				}
			#else  // For touch-devices  
				if (Input.touchCount == 1  &&  Input.GetTouch (0).phase == TouchPhase.Moved) 
				{
					// Get movement of the finger since last frame
					Vector2 touchDeltaPosition = Input.GetTouch (0).deltaPosition * panSpeed * 100;

					cameraNewPosition.x = Mathf.Clamp (Camera.main.transform.position.x - (touchDeltaPosition.x / Screen.width) * zoomModifier, initialPosition.x - panLimits.x, initialPosition.x + panLimits.x);
					cameraNewPosition.y = Mathf.Clamp (Camera.main.transform.position.y - (touchDeltaPosition.y / Screen.height) * zoomModifier, initialPosition.y - panLimits.y, initialPosition.y + panLimits.y);
				}
			#endif
		}   

		// Set new Camera position (if changed)
		Camera.main.transform.position = cameraNewPosition;

	}

	//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
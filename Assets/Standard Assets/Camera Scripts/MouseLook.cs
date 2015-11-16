using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
//using UnityEngineInternal;
//using AOT;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add a rigid body to the capsule
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSWalker script to the capsule

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {


	[DllImport ("UniWii")]
	private static extern void wiimote_start();
	[DllImport ("UniWii")]
	private static extern void wiimote_stop();

	[DllImport ("UniWii")]
	private static extern int wiimote_count();
	[DllImport ("UniWii")]
	private static extern bool wiimote_available( int which );
	[DllImport ("UniWii")]
	private static extern bool wiimote_isIRenabled( int which );
	[DllImport ("UniWii")]	
	private static extern bool wiimote_enableIR( int which );
	[DllImport ("UniWii")]
	private static extern bool wiimote_isExpansionPortEnabled( int which );
	[DllImport ("UniWii")]
	private static extern void wiimote_rumble( int which, float duration);
	[DllImport ("UniWii")]
	private static extern double wiimote_getBatteryLevel( int which );

	[DllImport ("UniWii")]
	private static extern byte wiimote_getAccX(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getAccY(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getAccZ(int which);

	[DllImport ("UniWii")]
	private static extern float wiimote_getIrX(int which);
	[DllImport ("UniWii")]
	private static extern float wiimote_getIrY(int which);
	[DllImport ("UniWii")]
	private static extern float wiimote_getRoll(int which);
	[DllImport ("UniWii")]
	private static extern float wiimote_getPitch(int which);
	[DllImport ("UniWii")]
	private static extern float wiimote_getYaw(int which);

	[DllImport ("UniWii")]
	private static extern byte wiimote_getNunchuckStickX(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getNunchuckStickY(int which);

	[DllImport ("UniWii")]
	private static extern byte wiimote_getNunchuckAccX(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getNunchuckAccZ(int which);

	[DllImport ("UniWii")]
	private static extern bool wiimote_getButtonA(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getButtonB(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getButtonUp(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getButtonLeft(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getButtonRight(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getButtonDown(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getButton1(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getButton2(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getButtonNunchuckC(int which);
	[DllImport ("UniWii")]
	private static extern byte wiimote_getButtonNunchuckZ(int which);
	[DllImport ("UniWii")]
	private static extern bool wiimote_getButtonPlus(int which);
	[DllImport ("UniWii")]
	private static extern bool wiimote_getButtonMinus(int which);


	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = .1F;//15F;0.1
	public float sensitivityY = .1F;//15F;0.2

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationX = 0F;
	float rotationY = 0F;
	
	Quaternion originalRotation;
	private string display = "Press the 1 and 2 buttons on the Wiimote to connect.";
	
	private const float deadZoneBegin = -.10F;
	private const float deadZoneEnd = .10F;
	
	private float lastIrMouseX = -1, lastIrMouseY = -1;
	
	public int nStickX;
	public int nStickY;
	
	void Update ()
	{
		int c = wiimote_count()-1;
		if (c>=0) {
			float ir_x = wiimote_getIrX(c);
			float ir_y = wiimote_getIrY(c);
		    if ( (ir_x != -100) && (ir_y != -100) ) {
			    	float temp_x = ((ir_x + .5F) /* / 1.5F */) * (float) Screen.width;
			    	float temp_y = /*(float) Screen.height - */(((ir_y + (float) 1.0)/ (float)2.0) * (float) Screen.height);
			    	temp_x = Mathf.RoundToInt(temp_x);
			    	temp_y = Mathf.RoundToInt(temp_y);
			    	
					/*
					if( ir_x >= deadZoneBegin && ir_x <= deadZoneEnd ) 
					   ir_x = 0F;
					else
					{
					   if( ir_x > 0 )
					      ir_x = ( ir_x - deadZoneBegin )  / ( 1.0F - deadZoneBegin );
					   else
					      ir_x = ( ir_x + deadZoneBegin ) / ( 1.0F - deadZoneBegin );
					}
					if( ir_y >= deadZoneBegin && ir_y <= deadZoneEnd ) 
					   ir_y = 0F;
					else
					{
					   if( ir_y > 0 )
					      ir_y = ( ir_y - deadZoneBegin )  / ( 1.0F - deadZoneBegin );
					   else
					      ir_y = ( ir_y + deadZoneBegin ) / ( 1.0F - deadZoneBegin );
					}
					*/
					if (lastIrMouseX == -1) lastIrMouseX = temp_x;
					if (lastIrMouseY == -1) lastIrMouseY = temp_y;
					if(ir_x >=0.4 ){
						rotationX+= 0.18F;
					}else if( ir_x <= -0.4){
						rotationX+= -0.18F;
					}
						rotationX += (temp_x - lastIrMouseX) * sensitivityX;
					//if(temp_x< lastIrMouseX  || temp_y> lastIrMouseY )
					if(temp_y - lastIrMouseY> 0.1 || temp_y - lastIrMouseY < 0.1){
						rotationY += (temp_y - lastIrMouseY) * sensitivityY;
						lastIrMouseY = temp_y;
					}
						
					if(temp_x - lastIrMouseX> 0.1 || temp_x - lastIrMouseX < 0.1)
					lastIrMouseX = temp_x;
					
		
					rotationX = ClampAngle (rotationX, minimumX, maximumX);
					rotationY = ClampAngle (rotationY, minimumY, maximumY);
					
					nStickX = (int) wiimote_getNunchuckStickX(c);
					nStickY = (int) wiimote_getNunchuckStickY(c);
					byte nAccX = wiimote_getNunchuckAccX(c);
					byte nAccZ = wiimote_getNunchuckAccZ(c);
					
					//display = "Wiimote " + c + " IR X: " + ir_x + " IR Y: " + ir_y + " " + temp_x + " " + temp_y + " " + lastIrMouseX + " " + lastIrMouseY + " nunchuck: " + nStickX + " " + nStickY + " " + nAccX + " " + nAccZ;
					display = string.Format("Wiimote {0} IR values: ({1,6:G2}, {2,6:G2}) Maps to screen: ({3,4:G4}, {4,4:G4}) Nunchuck stick: {5,5:G4}, {6,5:G4}, rotationX: {7,7:G3}", c, ir_x, ir_y, temp_x, temp_y, nStickX, nStickY, rotationX);
					for(int i=0; i<10000; i++){}
					if ( !float.IsNaN(ir_x) && !float.IsNaN(ir_y) ) {
					Quaternion xQuaternion = Quaternion.AxisAngle (Vector3.up, Mathf.Deg2Rad * rotationX);
					Quaternion yQuaternion = Quaternion.AxisAngle (Vector3.left, Mathf.Deg2Rad * rotationY);
					
					if (axes == RotationAxes.MouseXAndY)
						transform.localRotation = originalRotation * xQuaternion * yQuaternion;
					else if (axes == RotationAxes.MouseX)
						transform.localRotation = originalRotation * xQuaternion;
					else transform.localRotation = originalRotation * yQuaternion;
					}
					if (nStickX != 0) BroadcastMessage("nunchuckDataX", nStickX, SendMessageOptions.DontRequireReceiver);	
					if (nStickY != 0) BroadcastMessage("nunchuckDataY", nStickY, SendMessageOptions.DontRequireReceiver);	
					//MonoBehaviour s = (MonoBehaviour) gameObject.GetComponent("FPSWalker");
				//yield return new WaitForSeconds(0.3);
					//s.nunchuckData( nStickX, nStickY);
			//	StartCoroutine(Delay());
			//	StopCoroutine(Delay());
				//for(int i=0; i<1000; i++)
				//	i++;
			}
			if (wiimote_getButtonB(c)==1) 
				BroadcastMessage("Fire");
			if (wiimote_getButton1(c)==1)
				BroadcastMessage("SelectFirstWeapon");
			if (wiimote_getButton2(c)==1)
				BroadcastMessage("SelectSecondWeapon");
			if (wiimote_getButtonA(c)==true)
				BroadcastMessage("Jump");
			}
			if (wiimote_getButtonPlus(c)==true) {
				sensitivityX += .1f;
				sensitivityY += .2f;
			}
			if (wiimote_getButtonMinus(c)==true) {
				sensitivityX -= .1f;
				sensitivityY -= .2f;
			}
			
				
	 {
		if (axes == RotationAxes.MouseXAndY)
			{
				// Read the mouse input axis
				rotationX += Input.GetAxis("Mouse X") * sensitivityX;
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
	
				rotationX = ClampAngle (rotationX, minimumX, maximumX);
				rotationY = ClampAngle (rotationY, minimumY, maximumY);
				
				Quaternion xQuaternion = Quaternion.AxisAngle (Vector3.up, Mathf.Deg2Rad * rotationX);
				Quaternion yQuaternion = Quaternion.AxisAngle (Vector3.left, Mathf.Deg2Rad * rotationY);
				
				transform.localRotation = originalRotation * xQuaternion * yQuaternion;
			}
			else if (axes == RotationAxes.MouseX)
			{
				rotationX += Input.GetAxis("Mouse X") * sensitivityX;
				rotationX = ClampAngle (rotationX, minimumX, maximumX);
	
				Quaternion xQuaternion = Quaternion.AxisAngle (Vector3.up, Mathf.Deg2Rad * rotationX);
				transform.localRotation = originalRotation * xQuaternion;
			}
			else
			{
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = ClampAngle (rotationY, minimumY, maximumY);
	
				Quaternion yQuaternion = Quaternion.AxisAngle (Vector3.left, Mathf.Deg2Rad * rotationY);
				transform.localRotation = originalRotation * yQuaternion;
			}
		}

	}
	
	void Start ()
	{
		wiimote_start();
		
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
		originalRotation = transform.localRotation;
	}

	void OnApplicationQuit() {
		wiimote_stop();
	}

	void OnGUI() {
		GUI.Label( new Rect(10,10, 500, 100), display);
		GUI.Box (new Rect(lastIrMouseX,lastIrMouseY,50,50), "IR");
	}
	
	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}
}
/*
    IEnumerator Delay(){
	
	yield return new WaitForSeconds(0.2F);
}*/
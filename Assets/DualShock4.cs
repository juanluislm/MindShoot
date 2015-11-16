using UnityEngine;
using System.Collections;

public class DualShock4 : MonoBehaviour {
	public int joyIndex;
	
	void Start () {
		joyIndex = 0;
	}
	
	void Update () {
	
	}
	
	void OnGUI() {
		int i = 0;
		foreach ( string joystick in Input.GetJoystickNames() ) {
			GUI.TextArea( new Rect( 60, 60 + ( i * 110 ) , 600, 100 ), joystick );
			i++;
		}
	}
}

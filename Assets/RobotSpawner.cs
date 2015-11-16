using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotSpawner : MonoBehaviour {
	public GameObject robot;
	List<GameObject> robotList;
	bool tickTock;
	public int meditation;
	private int interval;
	
	void Start () {
		robotList = new List<GameObject>();
		tickTock = false;
		//meditation = 100;
	}
	
	void Update () {
		meditation = GameObject.Find("NeuroSky").GetComponent<ThinkGearController>().meditation;
		
		if ( meditation <= 40 ) {
			interval = 8;
		} else if ( meditation > 40 && meditation <= 60 ) {
			interval = 5;
		} else if ( meditation > 60 && meditation <= 100 ) {
			interval = 3;
		}
		
		if ( (int)Time.time % interval == 0 && tickTock == false /*Input.GetKeyDown( KeyCode.P )*/ ) {
			Instantiate( robot );
			robot.transform.position = this.transform.position;
			robotList.Add( robot );
			tickTock = true;
		}
		if ( (int)Time.time % interval != 0 ) {
			tickTock = false;
		}
	}
	
	void OnGUI() {
		GUI.TextArea( new Rect( 60, 60, 100, 30 ), Time.time.ToString() );
	}
}

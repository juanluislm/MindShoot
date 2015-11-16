using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class StressTracker : MonoBehaviour {
	public List<int> stressList;
	int meditation;
	bool tickTock;
	public int kills;
	
	void Start () {
		stressList = new List<int>();
		tickTock = false;
		//GameObject.Find("StressTracker").SetActive(false);
	}
	
	void Update () {
		meditation = GameObject.Find("NeuroSky").GetComponent<ThinkGearController>().meditation;
		
		if ( (int)Time.time % 2 == 0 && tickTock == false /*Input.GetKeyDown( KeyCode.P )*/ ) {
			if ( meditation <= 40 ) {
				stressList.Add(3);
			} else if ( meditation > 40 && meditation <= 60 ) {
				stressList.Add(2);
			} else if ( meditation > 60 && meditation <= 100 ) {
				stressList.Add(1);
			}
			tickTock = true;
		}
		if ( (int)Time.time % 2 != 0 ) {
			tickTock = false;
		}
		if ( (int)Time.time % 240 == 0 ) {
			File.Delete(@"UserStressData.txt");
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"UserStressData.txt")) {
				foreach ( int thisInt in stressList ) {
					file.WriteLine(thisInt);
				}
			}
		}
	}
}

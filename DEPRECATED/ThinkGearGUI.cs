using UnityEngine;
using System.Collections;

public class ThinkGearGUI : MonoBehaviour {
	enum AppState {
		Disconnected = 0,
		Connecting,
		Connected
	}
	
	string portName;
	
	private bool showErrorWindow = false;
	private bool showConnectedWindow = false;
	private bool showDisconnectedWindow = false;
	private AppState state = AppState.Disconnected;
	private Hashtable headsetValues;
	private Rect windowRect = new Rect(100, 100, 150, 100);
	private int xres = Screen.width;
	private int yres = Screen.height;
	
	/*function Start () {
	  var xres : int = Screen.width;
	  var yres : int = Screen.height;
	}*/
	
	void OnGUI(){
		GUILayout.BeginArea(new Rect(5, yres-80, 100, 200));
		//GUILayout.BeginHorizontal();
		
		switch(state){
		  case AppState.Disconnected:
			// display UI for the user to enter in the port name and connect
			GUILayout.Label("Port name:");                       
			portName = GUILayout.TextField(portName, GUILayout.Width(150));
		 
			if(GUILayout.Button("Connect")){
				state = AppState.Connecting;
				SendMessage("OnHeadsetConnectionRequest", portName);
			}
			break;
		
		  case AppState.Connecting:
			GUILayout.Label("Connecting...");
			break;
		  
		  case AppState.Connected:
			// display UI to allow a user to disconnect
			GUILayout.Label("Connected");
			
			if(GUILayout.Button("Disconnect")) {
				SendMessage("OnHeadsetDisconnectionRequest");
			}
		  	break;
		}
		
		// only output the headset data if the headset is
		// connected and transmitting data
		if ( state == AppState.Connected && headsetValues.Count > 0 ){
			foreach (string key in headsetValues.Keys) {
				float value = (float)headsetValues[key];
				GUILayout.Label(key + ": " + value);
			}
		}
		
		Rect messageRect = new Rect(20, 20, 120, 50);
		
		if(showErrorWindow) {
			messageRect = GUILayout.Window(0, windowRect, ErrorWindow, "Error");
		}
		
		if(showConnectedWindow) {
			messageRect = GUILayout.Window(0, windowRect, ConnectedWindow, "Connected");
		}
		
		if(showDisconnectedWindow) {
			messageRect = GUILayout.Window(0, windowRect, DisconnectedWindow, "Disconnected");
		}
		
		GUILayout.EndArea();
	}
	
	/*
	 * Event listeners
	 */
	
	void OnHeadsetConnected(){
	  showConnectedWindow = true;
	  state = AppState.Connected;
	}
	
	void OnHeadsetConnectionError(){
	  showErrorWindow = true;
	  state = AppState.Disconnected;
	}
	
	void OnHeadsetDisconnected(){
	  showDisconnectedWindow = true;
	  state = AppState.Disconnected;
	}
	
	void OnHeadsetDataReceived(Hashtable values){
	  headsetValues = values;
	}
	
	/**
	 * Disconnect the headset when the application quits.
	 */
	void OnApplicationQuit(){
	  SendMessage("OnHeadsetDisconnectionRequest");
	}
	
	/*
	 * Status windows
	 */
	
	void ErrorWindow(){
		GUILayout.Label("There was a connection error.");
	  
		if(GUILayout.Button("Close")) {
			showErrorWindow = false;
		}
	}
	
	void ConnectedWindow(){
		GUILayout.Label("The headset has been successfully connected.");
	
		if(GUILayout.Button("Okay")) {
			showConnectedWindow = false;
		}
	}
	
	void DisconnectedWindow(){
		GUILayout.Label("The headset has been disconnected.");
	
		if(GUILayout.Button("Okay")) {
			showDisconnectedWindow = false;
		}
	}
}
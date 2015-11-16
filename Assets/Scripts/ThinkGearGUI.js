enum AppState {
  Disconnected = 0,
  Connecting,
  Connected
}

var portName : String;

private var showErrorWindow : boolean = false;
private var showConnectedWindow : boolean = false;
private var showDisconnectedWindow : boolean = false;
private var state : AppState = AppState.Disconnected;
private var headsetValues : Hashtable;
private var windowRect : Rect = new Rect(100, 100, 150, 100);
private var xres : int = Screen.width;
private var yres : int = Screen.height;

/*function Start () {
  var xres : int = Screen.width;
  var yres : int = Screen.height;
}*/

function OnGUI(){
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

      if(GUILayout.Button("Disconnect"))
        SendMessage("OnHeadsetDisconnectionRequest");

      break;
  }

  // only output the headset data if the headset is
  // connected and transmitting data
  if(state == AppState.Connected && headsetValues){
    for(var key : String in headsetValues.Keys){
      var value : float = headsetValues[key];
      GUILayout.Label(key + ": " + value);
      //GameObject.Find("RobotSpawner").GetComponent("RobotSpawner")
    }
  }

  if(showErrorWindow)
    GUILayout.Window(0, windowRect, ErrorWindow, "Error");

  if(showConnectedWindow) {
    GUILayout.Window(0, windowRect, ConnectedWindow, "Connected");
  }

  if(showDisconnectedWindow)
    GUILayout.Window(0, windowRect, DisconnectedWindow, "Disconnected");
    
  GUILayout.EndArea();
}

/*
 * Event listeners
 */

function OnHeadsetConnected(){
  showConnectedWindow = true;
  state = AppState.Connected;
}

function OnHeadsetConnectionError(){
  showErrorWindow = true;
  state = AppState.Disconnected;
}

function OnHeadsetDisconnected(){
  showDisconnectedWindow = true;
  state = AppState.Disconnected;
}

function OnHeadsetDataReceived(values : Hashtable){
  headsetValues = values;
}

/**
 * Disconnect the headset when the application quits.
 */
function OnApplicationQuit(){
  SendMessage("OnHeadsetDisconnectionRequest");
}

/*
 * Status windows
 */

function ErrorWindow(){
  GUILayout.Label("There was a connection error.");
  
  if(GUILayout.Button("Close"))
    showErrorWindow = false;
}

function ConnectedWindow(){
  GUILayout.Label("The headset has been successfully connected.");

  if(GUILayout.Button("Okay"))
    showConnectedWindow = false;
}

function DisconnectedWindow(){
  GUILayout.Label("The headset has been disconnected.");

  if(GUILayout.Button("Okay"))
    showDisconnectedWindow = false;
}

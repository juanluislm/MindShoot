import System;
import System.Text;
import System.Security.Cryptography;

enum algorith { MD4, SHA1 }

//enum state { idle, displayInfo, displayDialog }
private var currentState : String;

private var salt = "1234123412341234"; 

private var username;
private var email;
private var code;

//
// Unity callbacks
//
// This will display a nag screen for unregistered users, 
// or the valid registration info for unregistered users.
//

function Update () {
}

function OnGUI() {
	switch (currentState) {
		case "idle" : break;
		case "displayInfo":
			GUI.Label(Rect(20,20,240,120), "Nag screen");
		case "displayDialog":
			var wr = Rect(20,20,240,120);
			wr = GUILayout.Window(0, wr, doRegWindow, "Register");
			break;
    } 
}

function doRegWindow(windowID : int) 
{ 
	GUILayout.Label("Name:");
	username = GUILayout.TextField("");
	GUILayout.Label("Email address:");
	email = GUILayout.TextField("");
	GUILayout.Label("Registration code:");
	code = GUILayout.TextField("");
	
   if (GUILayout.Button("Register")) { }
   GUI.DragWindow(); //make the window dragable 
} 

// Useful functions so you don't have to write them... modify as you wish


function showRegistrationDialog() {
}

function hideRegistrationDialog() {
}

function showNag() {
}

function hideNag() {
}



//

function setKey() {
}

function checkForValidity( username: String, email: String, hash: String ) {
	// checks the username, email and hash 
}

function isRegistered() {
}

function register() {
}

function checkBlacklist( hash : String) {
	// calls to web site, which checks to see if this hash value has been blacklisted
}

function unregister() {
	// removes registration information; for testing only
}


// md5 function from the wiki; a SHA1 variant is below.

static function Md5(strToEncrypt)
{
	var encoding = UTF8Encoding();
	var bytes = encoding.GetBytes(strToEncrypt);
 
	// encrypt bytes
	var SHA1 = SHA1CryptoServiceProvider();
	var hashBytes = SHA1.ComputeHash(bytes);
 
	// Convert the encrypted bytes back to a string (base 16)
	var hashString = "";
 
	for (var i = 0; i < hashBytes.Length; i++)
	{
		hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, "0"[0]);
	}
 
	return hashString.PadLeft(32, "0"[0]);
}

 
static function SHA1(strToEncrypt)
{
	var encoding = UTF8Encoding();
	var bytes = encoding.GetBytes(strToEncrypt);
 
	// encrypt bytes
	var md5 = MD5CryptoServiceProvider();
	var hashBytes = md5.ComputeHash(bytes);
 
	// Convert the encrypted bytes back to a string (base 16)
	var hashString = "";
 
	for (var i = 0; i < hashBytes.Length; i++)
	{
		hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, "0"[0]);
	}
 
	return hashString.PadLeft(32, "0"[0]);
}

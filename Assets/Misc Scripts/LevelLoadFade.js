/*
	Usage:

	// Load my level	
	LevelLoadFade.FadeAndLoadLevel("mylevel", Color.white, 0.5);

	// Reset the current level
	LevelLoadFade.FadeAndLoadLevel(Application.loadedLevel, Color.white, 0.5);
*/

static function FadeAndLoadLevel (level, fadeTexture : Texture2D, fadeLength : float)
{
	if (fadeTexture == null)
		FadeAndLoadLevel(level, Color.white, fadeLength);

	var fade = new GameObject ("Fade");
	fade.AddComponent(LevelLoadFade);
	fade.AddComponent(GUITexture);
	fade.transform.position = Vector3 (0.5, 0.5, 1000);
	fade.guiElement.texture = fadeTexture;
	fade.GetComponent(LevelLoadFade).DoFade(level, fadeLength, false);
}

static function FadeAndLoadLevel (level, color : Color, fadeLength : float)
{
	var fadeTexture = new Texture2D (1, 1);
	fadeTexture.SetPixel(0, 0, color);
	fadeTexture.Apply();
	
	var fade = new GameObject ("Fade");
	fade.AddComponent(LevelLoadFade);
	fade.AddComponent(GUITexture);
	fade.transform.position = Vector3 (0.5, 0.5, 1000);
	fade.guiElement.texture = fadeTexture;

	DontDestroyOnLoad(fadeTexture);
	fade.GetComponent(LevelLoadFade).DoFade(level, fadeLength, true);
}

function DoFade (level, fadeLength : float, destroyTexture : boolean)
{
	// Dont destroy the fade game object during level load
	DontDestroyOnLoad(gameObject);

	// Fadeout to start with
	guiElement.color.a = 0;
	
	// Fade texture in
	var time = 0.0;
	while (time < fadeLength)
	{
		time += Time.deltaTime;
		guiElement.color.a = Mathf.InverseLerp(0.0, fadeLength, time);
		yield;
	}
	guiElement.color.a = 1;
	yield;

	// Complete the fade out (Load a level or reset player position)
	Application.LoadLevel(level);
	
	// Fade texture out
	time = 0.0;
	while (time < fadeLength)
	{
		time += Time.deltaTime;
		guiElement.color.a = Mathf.InverseLerp(fadeLength, 0.0, time);
		yield;
	}
	guiElement.color.a = 0;
	yield;

	Destroy (gameObject);

	// If we created the texture from code we used DontDestroyOnLoad,
	// which means we have to clean it up manually to avoid leaks
	if (destroyTexture)
		Destroy (guiElement.texture);
}
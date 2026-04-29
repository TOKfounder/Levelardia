using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
	
	public AudioClip mainTheme;
	public AudioClip menuTheme;

	string sceneName;

	void Start()
	{
		OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		CancelInvoke(nameof(PlayMusic));
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == sceneName)
		{
			return;
		}

		sceneName = scene.name;
		CancelInvoke(nameof(PlayMusic));
		Invoke(nameof(PlayMusic), .2f);
	}

	void PlayMusic()
	{
		AudioClip clipToPlay = null;

		if (sceneName == "Menu")
		{
			clipToPlay = menuTheme;
		} else if (sceneName == "Games")
		{
			clipToPlay = mainTheme;
		}

		if (clipToPlay != null)
		{
			AudioManager.instance.PlayMusic(clipToPlay, 2);
			CancelInvoke(nameof(PlayMusic));
			Invoke(nameof(PlayMusic), clipToPlay.length);
		}
	}

}

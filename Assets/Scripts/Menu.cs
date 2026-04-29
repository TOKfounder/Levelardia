using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	const string ScreenResIndexKey = "screen res index";
	const string FullscreenKey = "fullscreen";

	public GameObject mainMenuHolder;
	public GameObject optionsMenuHolder;

	public Slider[] volumeSliders;
	public Toggle[] resolutionToggles;
	public Toggle fullscreenToggle;
	public int[] screenWidths;
	int activeScreenResIndex;

	void Start()
	{
		activeScreenResIndex = PlayerPrefs.GetInt(ScreenResIndexKey);
		bool isFullScreen = PlayerPrefs.GetInt(FullscreenKey) == 1;

		volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
		volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
		volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;

		for (int i = 0; i < resolutionToggles.Length; i++)
		{
			resolutionToggles[i].isOn = i == activeScreenResIndex;
		}

		fullscreenToggle.isOn = isFullScreen;
	}

	public void Play(){
		SceneManager.LoadScene("Games");
	}

	public void Quit(){
		print("Вышли из игры!");
		Application.Quit();
	}

	public void OptionsMenu(){
		mainMenuHolder.SetActive(false);
		optionsMenuHolder.SetActive(true);
	}

	public void MainMenu(){
		mainMenuHolder.SetActive(true);
		optionsMenuHolder.SetActive(false);
	}

	public void SetScreenResolution(int i){
		if (resolutionToggles[i].isOn)
		{
			activeScreenResIndex = i;
			float aspectRatio = 16/9f;
			Screen.SetResolution(screenWidths[i], (int) (screenWidths[i]/aspectRatio), false);
			PlayerPrefs.SetInt(ScreenResIndexKey, activeScreenResIndex);
			PlayerPrefs.Save();	
		}
	}

	public void SetFullscreen(bool isFullScreen){
		for (int i = 0; i < resolutionToggles.Length; i++)
		{
			resolutionToggles[i].interactable = !isFullScreen;
		}

		if (isFullScreen)
		{
			Resolution[] allResolutions = Screen.resolutions;
			Resolution maxResolution = allResolutions[allResolutions.Length - 1];
			Screen.SetResolution(maxResolution.width, maxResolution.height, true);
		} else {
			SetScreenResolution(activeScreenResIndex);
		}

		PlayerPrefs.SetInt(FullscreenKey, (isFullScreen)? 1 : 0);
		PlayerPrefs.Save();
	}

	public void SetMasterVolume(float value){
		AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
	}

	public void SetMusicVolume(float value){
		AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
	}

	public void SetSfxVolume(float value){
		AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Sfx);
	}
	
}

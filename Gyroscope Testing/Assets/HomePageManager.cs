using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HomePageManager : MonoBehaviour
{
    public TextMeshProUGUI mainText;
    public GameObject scenario1Button;
    public GameObject scenario2Button;
    public GameObject scenario3Button;
    public GameObject scenario4Button;
    public GameObject proceedButton;
    public GameObject backButton;

    private string defaultText = "Welcome to VR RCC Project!\n\nChoose a scenario:";
    private string scenario1Instructions = "Find a way to log in to the computer.\n\nSee what you can do to objects in the room.";
    private string scenario2Instructions = "Scenario 2 instructions.";
    private string scenario3Instructions = "Scenario 3 instructions. ";
    private string scenario4Instructions = "Scenario 4 instructions. ";

    private string selectedScenario = ""; // Track which scenario was clicked

    private GameObject player;

    void Start()
    {
        ShowHome();
        player = GameObject.Find("Player");
    }

    public void OnScenario1Clicked()
    {
        selectedScenario = "Scenario1";
        mainText.text = scenario1Instructions;
        ToggleScenarioButtons(false);
        ShowProceedAndBack(true);
    }

    public void OnScenario2Clicked()
    {
        selectedScenario = "Scenario2";
        mainText.text = scenario2Instructions;
        ToggleScenarioButtons(false);
        ShowProceedAndBack(true);
    }

    public void OnScenario3Clicked()
    {
        selectedScenario = "Scenario3";
        mainText.text = scenario3Instructions;
        ToggleScenarioButtons(false);
        ShowProceedAndBack(true);
    }
    public void OnScenario4Clicked()
    {
        selectedScenario = "Scenario4";
        mainText.text = scenario4Instructions;
        ToggleScenarioButtons(false);
        ShowProceedAndBack(true);
    }

    public void OnBackClicked()
    {
        ShowHome();
    }

    public void OnProceedClicked()
    {
        if (!string.IsNullOrEmpty(selectedScenario))
        {
            player.GetComponentInChildren<PlayerMovement>().TransitionArea();
            StartCoroutine(LoadScene(selectedScenario));
        }
        else
        {
            Debug.LogWarning("No scenario selected!");
        }
    }

    private void ShowHome()
    {
        mainText.text = defaultText;
        ToggleScenarioButtons(true);
        ShowProceedAndBack(false);
        selectedScenario = "";
    }

    private void ToggleScenarioButtons(bool state)
    {
        scenario1Button.SetActive(state);
        // Uncomment when scenarios are ready
        scenario2Button.SetActive(state);
        scenario3Button.SetActive(state);
        scenario4Button.SetActive(state);
    }

    private void ShowProceedAndBack(bool state)
    {
        proceedButton.SetActive(state);
        backButton.SetActive(state);
    }

    IEnumerator LoadScene(String sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(GameObject.Find("EventSystem"));
        Scene currentScene = SceneManager.GetActiveScene();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (!newScene.IsValid())
        {
            Debug.Log("Scene is not Valid");
            yield break;
        }

        foreach (var rootObject in newScene.GetRootGameObjects())
        {
            if (rootObject.name == "Player")
            {
                player.transform.position = rootObject.transform.position;
                player.transform.rotation = rootObject.transform.rotation;
                Destroy(rootObject);
                break;
            }
        }

        if (currentScene.IsValid())
        {
            SceneManager.MoveGameObjectToScene(player, newScene);
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentScene);
            asyncUnload.completed += OnSceneUnloaded;
        }
        else
        {
            // An error occurred
            Debug.Log("Scene is not Valid");
        }
    }

    private void OnSceneUnloaded(AsyncOperation obj)
    {
        FadeOutSquare_Static.setPhase(null, GameEnums.FadeOutSquare_PhaseEnum.FadeOut);
    }

    public void OnPointerEnter()
    {
    }
}
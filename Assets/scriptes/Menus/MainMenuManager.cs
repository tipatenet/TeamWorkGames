using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private enum states { Start, Settings, quit, Default, inSettings };
    public TMP_Text text;

    [SerializeField]
    private Camera defaultCamera;

    [SerializeField]
    private Camera MenuCamera;

    [SerializeField]
    private Camera SettingsCamera;

    [SerializeField]
    private GameObject defaultCanvas;

    [SerializeField]
    private float transitionTime = 0.5f;

    [SerializeField]
    private states currentState = states.Default;

    private Camera currentActiveCamera;
    private bool canInteract = true;

    // Stocke les positions/rotations d'origine de chaque camera
    private Dictionary<Camera, (Vector3 pos, Quaternion rot)> originalTransforms;

    private void Start()
    {
        // Sauvegarde les transforms d'origine AVANT toute manipulation
        originalTransforms = new Dictionary<Camera, (Vector3 pos, Quaternion rot)>
        {
            { defaultCamera, (defaultCamera.transform.position, defaultCamera.transform.rotation) },
            { MenuCamera, (MenuCamera.transform.position, MenuCamera.transform.rotation) },
            { SettingsCamera, (SettingsCamera.transform.position, SettingsCamera.transform.rotation) }
        };

        currentActiveCamera = defaultCamera;
        UpdateText();
        UpdateDefaultCanvas();
        SwitchCamera(GetCameraForState(currentState), false);
    }

    private void Update()
    {
        // Si on est sur Default et qu'une touche quelconque est pressee, on passe sur Start
        if (currentState == states.Default && canInteract && Input.anyKeyDown)
        {
            EnterMenu();
        }
    }

    public void EnterMenu()
    {
        if (!canInteract) return;
        if (currentState != states.Default) return;

        currentState = states.Start;
        UpdateText();
        UpdateDefaultCanvas();
        SwitchCamera(GetCameraForState(currentState), true);
    }

    public void NextState()
    {
        if (!canInteract) return;

        if (currentState == states.Start)
        {
            currentState = states.Settings;
        }
        else if (currentState == states.Settings)
        {
            currentState = states.quit;
        }
        else if (currentState == states.quit)
        {
            currentState = states.Start;
        }
        UpdateText();
        SwitchCamera(GetCameraForState(currentState), true);
    }

    public void PrevState()
    {
        if (!canInteract) return;

        if (currentState == states.Start)
        {
            currentState = states.quit;
        }
        else if (currentState == states.Settings)
        {
            currentState = states.Start;
        }
        else if (currentState == states.quit)
        {
            currentState = states.Settings;
        }
        UpdateText();
        SwitchCamera(GetCameraForState(currentState), true);
    }

    public void MenuAction()
    {
        switch (currentState)
        {
            case states.Start:
                Debug.Log("Start selected");
                break;
            case states.Settings:
                EnterSettings();
                break;
            case states.quit:
                Application.Quit();
                break;
        }
    }

    public void EnterSettings()
    {
        if (!canInteract) return;
        if (currentState != states.Settings) return;

        currentState = states.inSettings;
        SwitchCamera(GetCameraForState(currentState), true);
    }

    public void ExitSettings()
    {
        Debug.Log("cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc");
        if (!canInteract) return;
        if (currentState != states.inSettings) return;

        currentState = states.Settings;
        UpdateText();
        SwitchCamera(GetCameraForState(currentState), true);
    }

    public void GoToDefault()
    {
        if (!canInteract) return;

        currentState = states.Default;
        UpdateDefaultCanvas();
        SwitchCamera(GetCameraForState(currentState), true);
    }

    private Camera GetCameraForState(states state)
    {
        switch (state)
        {
            case states.Start:
            case states.Settings:
            case states.quit:
                return MenuCamera;
            case states.inSettings:
                return SettingsCamera;
            case states.Default:
                return defaultCamera;
            default:
                return defaultCamera;
        }
    }

    private void UpdateDefaultCanvas()
    {
        if (defaultCanvas != null)
            defaultCanvas.SetActive(currentState == states.Default);
    }

    private void SwitchCamera(Camera targetCamera, bool animated)
    {
        if (targetCamera == null || targetCamera == currentActiveCamera) return;

        Camera previousCamera = currentActiveCamera;

        targetCamera.gameObject.SetActive(true);
        targetCamera.depth = previousCamera.depth + 1;

        if (previousCamera.GetComponent<AudioListener>() != null)
            previousCamera.GetComponent<AudioListener>().enabled = false;

        if (targetCamera.GetComponent<AudioListener>() != null)
            targetCamera.GetComponent<AudioListener>().enabled = true;

        currentActiveCamera = targetCamera;

        if (animated)
        {
            StartCoroutine(CameraTransition(targetCamera, previousCamera));
        }
        else
        {
            previousCamera.gameObject.SetActive(false);
        }
    }

    private IEnumerator CameraTransition(Camera targetCamera, Camera previousCamera)
    {
        canInteract = false;

        Vector3 startPos = previousCamera.transform.position;
        Quaternion startRot = previousCamera.transform.rotation;

        Vector3 endPos = originalTransforms[targetCamera].pos;
        Quaternion endRot = originalTransforms[targetCamera].rot;

        targetCamera.transform.position = startPos;
        targetCamera.transform.rotation = startRot;

        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            targetCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            targetCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        targetCamera.transform.position = endPos;
        targetCamera.transform.rotation = endRot;

        previousCamera.gameObject.SetActive(false);
        canInteract = true;
    }

    private void UpdateText()
    {
        switch (currentState)
        {
            case states.Start:
                text.text = "Start";
                break;
            case states.Settings:
                text.text = "Settings";
                break;
            case states.quit:
                text.text = "Quit";
                break;
            case states.Default:
                text.text = "Start";
                break;
            case states.inSettings:
                text.text = "Settings";
                break;
        }
    }
}
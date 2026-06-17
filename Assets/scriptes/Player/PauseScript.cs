using UnityEngine;

public class PauseScript : MonoBehaviour
{
    public GameObject poseMenuUI;

    private bool isPosed = false;

    void Start()
    {
        PlayerInputHandler.Instance.OnPosePressed += TogglePose;
    }

    void OnDestroy()
    {
        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.OnPosePressed -= TogglePose;
    }

    private void TogglePose()
    {
        if (!isPosed)
            EnterPose();
        else
            ExitPose();
    }

    public void EnterPose()
    {
        isPosed = true;

        if (poseMenuUI != null)
            poseMenuUI.SetActive(true);

        PlayerInputHandler.Instance.LockGameplayInputsForPose(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitPose()
    {
        isPosed = false;

        if (poseMenuUI != null)
            poseMenuUI.SetActive(false);

        PlayerInputHandler.Instance.LockGameplayInputsForPose(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
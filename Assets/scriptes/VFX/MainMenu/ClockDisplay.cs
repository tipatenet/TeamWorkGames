using TMPro;
using UnityEngine;

public class ClockDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;

    [Tooltip("Nombre de secondes réelles pour 1 minute de jeu")]
    [SerializeField] private float realSecondsPerGameMinute = 1f;

    private float currentMinutes = 0f; // 00:00
    private const float MaxMinutes = 180f; // 3h = 180 minutes

    private void Update()
    {
        currentMinutes += Time.deltaTime / realSecondsPerGameMinute;

        if (currentMinutes >= MaxMinutes)
        {
            currentMinutes %= MaxMinutes;
        }

        int hours = Mathf.FloorToInt(currentMinutes / 60);
        int minutes = Mathf.FloorToInt(currentMinutes % 60);

        timeText.text = $"{hours:00}:{minutes:00}";
    }
}
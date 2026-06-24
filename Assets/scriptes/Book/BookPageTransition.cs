using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BookPageTransition : MonoBehaviour
{
    [System.Serializable]
    public class PageTransitionRule
    {
        [Header("Condition")]
        public int requiredPageCount; // 🔥 nombre de pages requis pour déclencher
        public List<string> requiredScenes;

        [Header("Action")]
        public string targetScene;
    }

    public List<PageTransitionRule> rules = new List<PageTransitionRule>();

    private BookSystem bookSystem;
    private int lastPageCount = -1;

    void Start()
    {
        bookSystem = GetComponent<BookSystem>();
    }

    void Update()
    {
        if (bookSystem == null) return;

        // Détecte changement du nombre de pages
        if (bookSystem.nbPages == lastPageCount) return;
        lastPageCount = bookSystem.nbPages;

        CheckTransitionRules();
    }

    void CheckTransitionRules()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        foreach (PageTransitionRule rule in rules)
        {
            // 🔥 condition basée sur le nombre de pages
            if (bookSystem.nbPages != rule.requiredPageCount)
                continue;

            // vérification scène
            if (rule.requiredScenes != null &&
                rule.requiredScenes.Count > 0 &&
                !rule.requiredScenes.Contains(currentScene))
                continue;

            // transition
            if (!string.IsNullOrEmpty(rule.targetScene))
            {
                Debug.Log($"[BOOK] nbPages = {bookSystem.nbPages} → TP vers {rule.targetScene}");
                SceneTransitionManager.Instance.GoToScene(rule.targetScene);
                return;
            }
        }
    }
}
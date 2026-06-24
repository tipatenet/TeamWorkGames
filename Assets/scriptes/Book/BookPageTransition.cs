using System.Collections.Generic;
using UnityEngine;

public class BookPageTransition : MonoBehaviour
{
    [System.Serializable]
    public class PageTransitionRule
    {
        [Header("Condition")]
        public int pageIndex;           // index de la page qui déclenche la transition
        public List<string> requiredScenes; // scènes où cette règle est active (vide = toutes les scènes)

        [Header("Action")]
        public string targetScene;      // scène vers laquelle on tp
    }

    public List<PageTransitionRule> rules = new List<PageTransitionRule>();
    private BookSystem bookSystem;
    private int lastPageIndex = -1;

    void Start()
    {
        bookSystem = GetComponent<BookSystem>();
    }

    void Update()
    {
        if (bookSystem == null) return;

        // Détecte un changement de page
        if (bookSystem.selectedIndex == lastPageIndex) return;
        lastPageIndex = bookSystem.selectedIndex;

        CheckTransitionRules();
    }

    void CheckTransitionRules()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        foreach (PageTransitionRule rule in rules)
        {
            // Vérifie que c'est la bonne page
            if (bookSystem.selectedIndex != rule.pageIndex) continue;

            // Vérifie qu'on est dans la bonne scène (si la liste est vide = toutes les scènes)
            if (rule.requiredScenes.Count > 0 && !rule.requiredScenes.Contains(currentScene)) continue;

            // Déclenche la transition
            if (!string.IsNullOrEmpty(rule.targetScene))
            {
                Debug.Log($"[BOOK] Page {rule.pageIndex} dans '{currentScene}' → tp vers '{rule.targetScene}'");
                SceneTransitionManager.Instance.GoToScene(rule.targetScene);
                return;
            }
        }
    }
}
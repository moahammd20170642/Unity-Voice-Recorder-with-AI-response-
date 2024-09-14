using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LanguageSelector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI languageText;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;

    private string[] languages = { "English", "Spanish", "French", "German", "Chinese" };
    private int currentIndex = 0;

    private void Start()
    {
        // Set initial language
        UpdateLanguageText();

        // Add listeners to arrow buttons
        leftArrowButton.onClick.AddListener(SelectPreviousLanguage);
        rightArrowButton.onClick.AddListener(SelectNextLanguage);
    }

    private void UpdateLanguageText()
    {
        // Update the text to show the current language
        languageText.text = languages[currentIndex];
    }

    private void SelectPreviousLanguage()
    {
        // Decrease index and wrap around if needed
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = languages.Length - 1;
        }

        UpdateLanguageText();
    }

    private void SelectNextLanguage()
    {
        // Increase index and wrap around if needed
        currentIndex++;
        if (currentIndex >= languages.Length)
        {
            currentIndex = 0;
        }

        UpdateLanguageText();
    }
}

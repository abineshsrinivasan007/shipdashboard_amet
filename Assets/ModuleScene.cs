using UnityEngine;
using TMPro;

public class ModuleSceneManager : MonoBehaviour
{
    public TextMeshProUGUI welcomeText;

    void Start()
    {
        string studentName = PlayerPrefs.GetString("student_name", "Student");
        welcomeText.text = "Welcome, " + studentName + "!";
    }
}

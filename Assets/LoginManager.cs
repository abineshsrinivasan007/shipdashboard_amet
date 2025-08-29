using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
[System.Serializable]
public class LoginResponse
{
    public int id;
    public string name;
    public string email;
    public string message;
}

public class LoginManager : MonoBehaviour
{
    public TMP_InputField vpCodeInput;      // Input field in your scene
    public TextMeshProUGUI resultText;      // Text to show messages

    private string vpCode;

    public void Login()
    {
        vpCode = vpCodeInput.text.Trim();
        Debug.Log("VP Code entered: " + vpCode); // Check this

        if (string.IsNullOrEmpty(vpCode))
        {
            resultText.text = "Please enter your VP code.";
            return;
        }

        // Start login process
        StartCoroutine(SendLoginRequest(vpCode));
    }

    IEnumerator SendLoginRequest(string vpCode)
    {
        string url = "http://192.168.1.160:8000/login/"; // Your Django server URL (with trailing slash!)

        WWWForm form = new WWWForm();
        form.AddField("vp_code", vpCode);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Login Error: " + request.error);
            resultText.text = "Network error: " + request.error;
        }
        else
        {
            string json = request.downloadHandler.text;
            Debug.Log("Login Response: " + json);

            try
            {
                LoginResponse res = JsonUtility.FromJson<LoginResponse>(json);

                if (res != null && res.name != null)
                {
                    PlayerPrefs.SetInt("student_id", res.id);
                    PlayerPrefs.SetString("student_name", res.name);
                    PlayerPrefs.Save();

                    resultText.text = "Welcome, " + res.name + "!";

                    Debug.Log("✅ Condition passed, trying to load ModuleScene...");
                    SceneManager.LoadScene("ModuleScene"); // <-- Try to load scene
                }
                else
                {
                    resultText.text = "Invalid VP code.";
                }
            }
            catch
            {
                resultText.text = "Error parsing server response.";
                if (resultText == null)
                {
                    Debug.LogError("❌ resultText is NOT assigned in the Inspector!");
                }

                Debug.LogError("JSON Parse Error: " + json);
            }
        }
    }

    // 🔽 Add this test function at the END of the class
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("🔄 Manually loading ModuleScene...");
            SceneManager.LoadScene("ModuleScene");
        }
    }
}

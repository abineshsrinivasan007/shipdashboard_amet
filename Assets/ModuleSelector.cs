using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ModuleData
{
    public int id;
    public string name;
}

[System.Serializable]
public class SessionResponse
{
    public int session_id;
    public string message;
}

public class ModuleSelector : MonoBehaviour
{
    public TMP_Text welcomeText;
    public TMP_Dropdown moduleDropdown;
    public TMP_Text resultText;

    private int studentId;
    private string studentName;

    private List<ModuleData> modulesList;

    void Start()
    {
        studentId = PlayerPrefs.GetInt("student_id");
        studentName = PlayerPrefs.GetString("student_name");
        welcomeText.text = "Welcome, " + studentName;

        StartCoroutine(GetModulesFromAPI());
    }

    IEnumerator GetModulesFromAPI()
    {
        string url = "http://127.0.0.1:8000/modules/";


        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Module API Response: " + json);

            modulesList = new List<ModuleData>(JsonHelper.FromJson<ModuleData>(json));

            List<string> moduleNames = new List<string>();
            foreach (var module in modulesList)
            {
                moduleNames.Add(module.name);
            }

            moduleDropdown.ClearOptions();
            moduleDropdown.AddOptions(moduleNames);
            Debug.Log("Dropdown loaded with " + moduleNames.Count + " modules.");
        }
        else
        {
            resultText.text = "Failed to load modules: " + request.error;
            Debug.LogError("Module API failed: " + request.error);
        }
    }

    public void StartModule()
    {
        int selectedIndex = moduleDropdown.value;
        if (modulesList != null && selectedIndex < modulesList.Count)
        {
            int selectedModuleId = modulesList[selectedIndex].id;
            StartCoroutine(SendSessionRequest(studentId, selectedModuleId));
        }
        else
        {
            resultText.text = "Please wait for modules to load.";
        }
    }

    IEnumerator SendSessionRequest(int studentId, int moduleId)
    {
        string baseUrl = "http://192.168.1.160:8000/start-session/";


        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);
        form.AddField("module_id", moduleId);

        UnityWebRequest request = UnityWebRequest.Post(baseUrl, form);
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Session Start Response: " + json);

            SessionResponse response = JsonUtility.FromJson<SessionResponse>(json);
            PlayerPrefs.SetInt("session_id", response.session_id);

            SceneManager.LoadScene("TrainingScene");
        }
        else
        {
            resultText.text = "Error: " + request.downloadHandler.text;
            Debug.LogError("Session Request Failed: " + request.downloadHandler.text);
        }
    }
}

// Helper for JSON arrays
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

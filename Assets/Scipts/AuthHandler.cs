using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthHandler : MonoBehaviour
{

    public string ApiUrl = "https://sid-restapi.onrender.com/api/";

    TMP_InputField UsernameInputField;
    TMP_InputField PasswordInputField;
    [SerializeField] TMP_InputField ScoreInputField;
    [SerializeField] TMP_InputField UserScoreInputField;
    [SerializeField] RawImage UserStatus;

    public List<TextMeshProUGUI> scoreList;

    private string Token;
    private string Username;

    void Start()
    {
        Token = PlayerPrefs.GetString("token");

        if(string.IsNullOrEmpty(Token) )
        {
            Debug.Log("No hay TOKEN almacenado");
        }
        else
        {
            Username = PlayerPrefs.GetString("username");
            StartCoroutine(GetProfile(Username));
        }

        UsernameInputField = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>();
        PasswordInputField = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>();
    }

    public void Register()
    {
        AuthData authData = new AuthData();
        authData.username = UsernameInputField.text;
        authData.password = PasswordInputField.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendRegister(json));
    }
    public void Login()
    {
        AuthData authData = new AuthData();
        authData.username = UsernameInputField.text;
        authData.password = PasswordInputField.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendLogin(json));
    }
    public void GettingUserData()
    {
        StartCoroutine(GetUsers());
    }
    public void PatchingScore()
    {
        ScoreSend scoreSend = new ScoreSend();
        scoreSend.data = new DataUser();

        scoreSend.username = Username;
        scoreSend.data.score = int.Parse(ScoreInputField.text);

        string json = JsonUtility.ToJson(scoreSend);
        StartCoroutine(PatchScore(json));
    }
    IEnumerator GetUsers()
    {
        UnityWebRequest request = UnityWebRequest.Get(ApiUrl + "usuarios");
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                var descendingUsers = data.usuarios.OrderByDescending(u => u.data.score).ToArray();

                for (int i = 0; i < scoreList.Count; i++)
                {
                    scoreList[i].text = descendingUsers[i].username + " Puntaje: " + descendingUsers[i].data.score;
                }
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator PatchScore(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(ApiUrl + "usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "PATCH";
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                Debug.Log("El nuevo puntaje es: " + data.usuario.data.score);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator GetProfile(string username)
    {
        UnityWebRequest request = UnityWebRequest.Get(ApiUrl + "usuarios/" + username);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                Debug.Log("Sesion activa del usuario " + data.usuario.username);
                Debug.Log("Su puntaje es " + data.usuario.data.score);
                UserStatus.color = new Color(0, 255, 0);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(ApiUrl + "usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if(request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                Debug.Log("Se registro el usuario con id " + data.usuario._id);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator SendLogin(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(ApiUrl + "auth/login", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                Debug.Log("Inicio de sesion por el usuario " + data.usuario.username);

                PlayerPrefs.SetString("token", data.token);
                PlayerPrefs.SetString("username", data.usuario.username);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
}

[System.Serializable]
public class AuthData
{
    public string username;
    public string password;
    public User usuario;
    public string token;
    public User[] usuarios;
}

[System.Serializable]
public class User
{
    public string _id;
    public string username;
    public bool estado;
    public DataUser data;
}

[System.Serializable]
public class DataUser
{
    public int score;
}

[System.Serializable]
public class ScoreSend
{
    public string username;
    public DataUser data;
}

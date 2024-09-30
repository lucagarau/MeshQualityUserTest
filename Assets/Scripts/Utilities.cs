using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class Utilities : MonoBehaviour
{
    private static float _lastDownloadTime;
    
    class DownloadData
    {
        public string type = "csv";
        public string id;
    }
    
    [System.Serializable]
    public class ServerResponse
    {
        public string csv_name;
        public int index;
    }
    
    public static IEnumerator DownloadFile(string file, string url, string internalPath, Action<string> callback = null, string another = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        stopwatch.Start();

        if (!File.Exists(internalPath + file))
        {
            var drcUrl = url + file;

            using (UnityWebRequest request = UnityWebRequest.Get(drcUrl))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    HandleDownloadError(request);
                    yield break;
                }

                SaveDownloadedFile(file, request.downloadHandler.data, internalPath);
                stopwatch.Stop();
                _lastDownloadTime = stopwatch.ElapsedMilliseconds;
                
            }
        }
        
        if (another != null)
        {
            if (!File.Exists(internalPath + another))
            {
                var drcUrl = url + another;

                using (UnityWebRequest request = UnityWebRequest.Get(drcUrl))
                {
                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        HandleDownloadError(request);
                        yield break;
                    }

                    SaveDownloadedFile(another, request.downloadHandler.data, internalPath);
                }
            }
        }
        
        stopwatch.Stop();
        _lastDownloadTime = stopwatch.ElapsedMilliseconds;

        // Esegui il callback per ulteriori azioni
        callback?.Invoke(file);
    }

    public static IEnumerator RequestCsv(string url, string internalPath, TestManager instance,Action<string> callback)
    {
        //serialize the data
        var data = new DownloadData();
        data.id = TestManager.ID;
        string json = JsonUtility.ToJson(data);

        //create a POST request
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        //send the request
        yield return request.SendWebRequest();

        //check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            // Leggi la risposta JSON
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);

            // Deserializza la risposta
            ServerResponse response = JsonUtility.FromJson<ServerResponse>(jsonResponse);
            Debug.Log("CSV: " + response.csv_name);
            Debug.Log("Index: " + response.index);

            instance.csvName = response.csv_name;
            instance.meshIndex = response.index;
            
            callback?.Invoke(null);
        }
    }

    public static IEnumerator SendResults(string url)
    {
        //serialize the data
        string json = JsonUtility.ToJson(TestManager.currentModel);
        
        //create a POST request
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        
        //send the request
        yield return request.SendWebRequest();
        
        //check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
    }
    public static IEnumerator SendResultsCategory(string url)
    {
        //serialize the data
        string json = JsonUtility.ToJson(TestManager.CurrentCategoryAnswer);
        
        //create a POST request
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        
        //send the request
        yield return request.SendWebRequest();
        
        //check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
    }
    
    

    // Metodo per gestire errori di download
    private static void HandleDownloadError(UnityWebRequest request)
    {
        Debug.LogError("Errore durante il download del file: " + request.error);
    }

    // Metodo per salvare il file scaricato
    private static void SaveDownloadedFile(string file, byte[] data, string internalPath)
    {
        string filePath = internalPath + file;
        
        var folder = Path.GetDirectoryName(filePath);
        if(!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        File.WriteAllBytes(filePath, data);
        //Debug.Log(filePath);
    }
    
    
    public static Material LoadMTL(string path)
    {
        Material material = new Material(Shader.Find("Standard"));

        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            string[] tokens = line.Split(' ');
            if (tokens.Length < 2) continue;

            switch (tokens[0])
            {
                case "newmtl":
                    material.name = tokens[1];
                    break;
                case "Kd":
                    material.color = new Color(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                    break;
                case "Ks":
                    material.SetColor("_SpecColor", new Color(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3])));
                    break;
                case "Ka":
                    material.SetColor("_EmissionColor", new Color(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3])));
                    break;
                case "d":
                    material.SetFloat("_Mode", float.Parse(tokens[1]));
                    break;
                case "Ns":
                    material.SetFloat("_Glossiness", float.Parse(tokens[1]));
                    break;
                case "illum":
                    material.SetFloat("_Mode", float.Parse(tokens[1]));
                    break;
                case "map_Kd":
                    material.mainTexture = new Texture2D(2, 2);
                    var texturePath = Path.Combine(Path.GetDirectoryName(path), tokens[1]);
                    var data = File.ReadAllBytes(texturePath);
                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(data);
                    texture.Apply();
                    material.mainTexture = texture;
                    break; 
            }
        }

        return material;
    }
    
    

    
}

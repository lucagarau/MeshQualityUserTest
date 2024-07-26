using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = System.Random;


public class MeshData
{
    public string drc { get; set; }
    public string texture { get; set; }
}
public class TestManager : MonoBehaviour
{
    public GameObject objectToMove;
    private DracoMeshManager _dracoMeshManager;
    private MovementScripts movementScript;
    
    private int _meshIndex = -1;
    private List<MeshData> models = new List<MeshData>();
    public string ip = "192.168.172.42";
    public string port = "8080";
    private string _url;
    private string _meshPath;
    
    private Random indexGenerator = new Random();
    private void Start()
    {
        if (objectToMove != null)
        {
            _dracoMeshManager = objectToMove.GetComponentInChildren<DracoMeshManager>();
            movementScript = objectToMove.GetComponent<MovementScripts>();
        }
        _url = "http://" + ip + ":" + port + "/";
        
        // Pulisce la cache locale
        _meshPath = Application.temporaryCachePath + "/";
        ClearCache();
        updateList();
    }

    private void updateList()
    {
        StartCoroutine(Utilities.DownloadFile("mesh_list.txt", _url, _meshPath, readMeshList));
    }
    
    // Metodo per pulire la cache locale
    void ClearCache()
    {
        DirectoryInfo di = new DirectoryInfo(_meshPath);
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }

        foreach (var subfolder in di.GetDirectories())
        {
            subfolder.Delete(true);
        }
        
    }
    
    private void readMeshList(string path)
    {
        path = _meshPath + path;
        if (File.Exists(path))
        {
            try
            {
                string[] lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    string[] data = line.Split('|');
                    models.Add(new MeshData { drc = data[0], texture = data[1] });
                }
                Debug.Log("Lette " + models.Count + " mesh");
            }
            catch (Exception e)
            {
                Debug.LogError("Errore durante la lettura del file: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("File non trovato: " + path);
        }
    }
    
    public void LoadNextMesh()
    {
        //blocco il movimento e resetto le variabili di controllo del movimento
        movementScript.meshReady = false;
        movementScript.textureReady = false;
        
        
        _meshIndex = indexGenerator.Next(0,models.Count);
        if (_meshIndex >= models.Count)
        {
            Debug.Log("Fine lista");
        }
        var drc = models[_meshIndex].drc;
        var texture = models[_meshIndex].texture;
        StartCoroutine(Utilities.DownloadFile(drc, _url, _meshPath, changeMesh));
        StartCoroutine(Utilities.DownloadFile(texture, _url, _meshPath, changeTexture));
        
        movementScript.StartMoving();
    }
    
    private void changeMesh(string mesh)
    {
        Debug.Log("Cambio la mesh corrente con: " + mesh);
        _dracoMeshManager.ChangeMesh(mesh);
    }
    
    private void changeTexture(string texture)
    {
        Debug.Log("Cambio la texture corrente con: " + texture);
        _dracoMeshManager.ChangeTexture(texture);
    }
    
}

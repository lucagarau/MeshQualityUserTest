using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;


public class MeshData
{
    public string drcPath { get; set; }
    public string texturePath { get; set; }
    public int distance { get; set; }
    public string category { get; set; }
    
    public MeshData(string category, string drcPath, string texturePath, int distance)
    {
        this.category = category;
        this.drcPath = drcPath;
        this.texturePath = texturePath;
        this.distance = distance;
    }
}

public class Model
{
    public string name = "";
    public string category = "";
    public int lod = 0;
    public string texture_resolution = "";
    public int distance = 0;
    public int quality = -1 ;

    public  void PrintInformation()
    {
        Debug.Log( "Modello: " + name + " - Categoria: " + category + " - LOD: " + lod + " - Texture Resolution: " + texture_resolution + " - Distance: " + distance + " - Quality: " + quality);
    }
    
    
}
public class TestManager : MonoBehaviour
{
    public GameObject objectToMove;
    [FormerlySerializedAs("_avantiPanel")] [SerializeField] private GameObject avantiPanel;
    [FormerlySerializedAs("_oto")] [SerializeField] private TextMeshProUGUI voto;
    private DracoMeshManager _dracoMeshManager;
    private MovementScripts movementScript;

    public static Model currentModel;
    
    private List<MeshData> modelsList = new List<MeshData>();
    private int _meshIndex;
    
    public string ip = "192.168.172.42";
    public string port = "8080";
    private string _url;
    private string _meshPath;
    
    [SerializeField] private bool keepCache = false;
    
    
    private void Start()
    {
        //check components principali
        if (objectToMove != null)
        {
            _dracoMeshManager = objectToMove.GetComponentInChildren<DracoMeshManager>();
            movementScript = objectToMove.GetComponent<MovementScripts>();
        }
    
        //inizializzo la lista dei modelli
        modelsList = new List<MeshData>();
        
        //Controllo se il pannello è stato assegnato, altrementi mando un errore
        if (avantiPanel == null)
        {
            Debug.LogError("Pannello non assegnato");
            return;
        }

        //Assegnamento url per la connessione al server
        _url = "http://" + ip + ":" + port + "/";
        
        // Pulisce la cache locale se keepCache è false
        _meshPath = Application.temporaryCachePath + "/";
        if (!keepCache)
            ClearCache();
        
        //carica la lista dei modelli disponibili
        updateList();
    }

    private void updateList()
    {
        StartCoroutine(Utilities.DownloadFile("mesh_list.csv", _url, _meshPath, readMeshList));
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
    
    /**
     * Metodo per leggere la lista dei modelli disponibili nel server
     * @param path: percorso del file da leggere
     */
    private void readMeshList(string path)
    {
        path = _meshPath + path;
        if (File.Exists(path))
        {
            try
            {
                //leggo tutti i modelli disponibili
                string[] lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    string[] data = line.Split(';');
                    if (data.Length == 4)
                    {
                        modelsList.Add(new MeshData(data[0], data[1], data[2], int.Parse(data[3])));
                    }

                }
                
                //todo: da cambiare con la richiesta al server dell'ultimo modello valutato
                _meshIndex = 0; //indice del modello corrente
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
        
        TestManager.currentModel = new Model();
        //blocco il movimento e resetto le variabili di controllo del movimento
        movementScript.meshReady = false;
        movementScript.textureReady = false;
        
        //todo: creare un pannello di fine test
        //Se non ho modello disponibile, esco
        if(modelsList.Count == 0)
        {
            Debug.Log("Nessun modello disponibile, fine test");
            return;
        }
        
        //recupera il prossimo modello da valutare
        var drc = modelsList[_meshIndex].drcPath;
        var texture = modelsList[_meshIndex].texturePath;
       
        
        
        //cambia separatore in base al sistema operativo
        var separator = Path.DirectorySeparatorChar;
        
        //aggiorno le informazioni del modello corrente da inviare al server dopo che l'utente ha valutato il modello
        var drcSplitted = drc.Split("_");
        var LODTmp = ((drcSplitted[drcSplitted.Length - 1].Split("."))[0]).Split("/");
        
        var textureSplitted = texture.Split(separator);
        var textureTmp = textureSplitted[textureSplitted.Length - 1].Split("_");
        currentModel.texture_resolution = textureTmp[0];
        
        currentModel.name = drc.Remove(drc.LastIndexOf("_") + 1);
        switch (LODTmp[LODTmp.Length - 1])
        {
            case "LOD1":
                currentModel.lod = 1;
                break;
            case "LOD2":
                currentModel.lod = 2;
                break;
            default:
                currentModel.lod = 0;
                break;
        }
        
        currentModel.category = modelsList[_meshIndex].category;
        currentModel.distance = modelsList[_meshIndex].distance;
        
        //scarico il modello e la texture dal server e li carico nella scena
        StartCoroutine(Utilities.DownloadFile(drc, _url, _meshPath, changeMesh));
        StartCoroutine(Utilities.DownloadFile(texture, _url, _meshPath, changeTexture));
        movementScript.StartMoving(modelsList[_meshIndex].distance);
        
        //aggiorno il contatore del modello corrente
        _meshIndex++;
        
    }
    
    private void changeMesh(string mesh)
    {
//        Debug.Log("Cambio la mesh corrente con: " + mesh);
        _dracoMeshManager.ChangeMesh(mesh);
    }
    
    private void changeTexture(string texture)
    {
        //Debug.Log("Cambio la texture corrente con: " + texture);
        _dracoMeshManager.ChangeTexture(texture);
    }
    
    public void SetResultQuality(int quality)
    {
        currentModel.quality = quality;
        voto.text = "Valutazione: " + quality.ToString();
    }
    
    public  void dubugPoho(string poho)
    {
        Debug.Log(poho);
    }
    
    public void submitResult()
    {
        if(currentModel.quality == -1)
        {
            Debug.Log("Valutazione non effettuata");
            return;
        }
        StartCoroutine( Utilities.SendResults(_url));
        voto.text = "Valutazione: Da Inserire";
        avantiPanel.SetActive(false);
        LoadNextMesh();

    }

    
}

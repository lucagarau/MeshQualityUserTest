using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = System.Random;


public class MeshData
{
    public string drc { get; set; }
    public string texture { get; set; }
}

public class Model
{
    public string name = "";
    public string category = "";
    public int lod = 0;
    public string texture_resolution = "";
    public int distance = 0;
    public int quality = 0;

    public  void PrintInformation()
    {
        Debug.Log( "Modello: " + name + " - Categoria: " + category + " - LOD: " + lod + " - Texture Resolution: " + texture_resolution + " - Distance: " + distance + " - Quality: " + quality);
    }
    
    
}
public class TestManager : MonoBehaviour
{
    public GameObject objectToMove;
    private DracoMeshManager _dracoMeshManager;
    private MovementScripts movementScript;

    public static Model currentModel;
    //private List<MeshData> models = new List<MeshData>();
    public string ip = "192.168.172.42";
    public string port = "8080";
    private string _url;
    private string _meshPath;
    
    private Random indexGenerator = new Random();
    private Dictionary<String,List<MeshData>> _models = new Dictionary<string, List<MeshData>>();
    private Dictionary<String,int> _categoryCounter = new Dictionary<string, int>();
    private int _meshIndex = -1;

    private List<String> _categories = new List<string>();
    
    [SerializeField]
    private int _modelsForCategory = 1;
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
                    var drc = data[0];
                    var texture = data[1];
                    var category = data[2];
                    if (!_models.ContainsKey(category))
                    {
                        _models.Add(category, new List<MeshData>());
                    }
                    _models[category].Add(new MeshData {drc = drc, texture = texture});
                }
                _categories.AddRange(_models.Keys);
                foreach (var cat in _categories)
                {
                    _categoryCounter.Add(cat, _modelsForCategory);
                }
                
                Debug.Log("Lista modelli aggiornata");
                Debug.Log("Categorie disponibili: " + string.Join(", ", _categories));
                Debug.Log("Modelli per categoria: " + _categoryCounter);
                
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
        
        //Se non ho categorie disponibili, esco
        if(_categories.Count == 0)
        {
            Debug.Log("Nessun modello disponibile");
            return;
        }
        
        //genera casualmente una categoria tra quelle disponibili
        string cat;
        do
        {
            cat = _categories[indexGenerator.Next(0, _categories.Count)];
        } while (_categoryCounter[cat] == 0);
        
        
        //recupera un modello casuale dalla categoria scelta e lo carico in scena
        _meshIndex = indexGenerator.Next(0, _models[cat].Count);
        var drc = _models[cat][_meshIndex].drc;
        var texture = _models[cat][_meshIndex].texture;
        
        //recupero i dati sul modello corrente: nome, categoria, lod, texture_resolution
        /*var tmp = drc.Split("_");
        currentModel.name = drc.Remove(drc.LastIndexOf("_") + 1);
        var tmpLod = tmp[tmp.Length - 1].Split(".");
        currentModel.lod = tmpLod[0];
        currentModel.category = cat;
        currentModel.texture_resolution = texture.Split("_")[0];*/
        
        
        //cambia separatore in base al sistema operativo
        var separator = Path.DirectorySeparatorChar;
        
        var drcSplitted = drc.Split("_");
        var LODTmp = ((drcSplitted[drcSplitted.Length - 1].Split("."))[0]).Split("/");
        var textureTmp = texture.Split(separator);
        textureTmp = textureTmp[textureTmp.Length - 1].Split("_");
        currentModel.texture_resolution = textureTmp[0] + " x " + textureTmp[0];
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

        
        currentModel.category = cat;
        
        
        //currentModel.PrintInformation();
        
        StartCoroutine(Utilities.DownloadFile(drc, _url, _meshPath, changeMesh));
        StartCoroutine(Utilities.DownloadFile(texture, _url, _meshPath, changeTexture));
        movementScript.StartMoving();
        
        //aggiorno il contatore della categoria, se arrivo a 0 rimuovo la categoria dalla lista
        _categoryCounter[cat]--;
        if(_categoryCounter[cat] == 0)
            _categories.Remove(cat);
        
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
    }
    
    public  void dubugPoho(string poho)
    {
        Debug.Log(poho);
    }
    
    public void submitResult()
    {
        StartCoroutine( Utilities.SendResults(_url));

    }

    
}

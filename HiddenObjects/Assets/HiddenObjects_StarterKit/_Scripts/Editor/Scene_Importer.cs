//-----------------------------------------------------------------------------------------------------	
// Editor script - Creates scene
// Generates atlas and textures, create/arrange/place objects according to info in Data-file
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine.Rendering;


public class Scene_Importer : EditorWindow 
{
	//=====================================================================================================	
	public class LayerData
	{
		public string name;
		public Vector3 offset;
		public Vector3 size;
	}



	//=====================================================================================================
	public string overlayLabel = "_overlay";	// Label for overlay objects
	public string shadowLabel = "_shadow";		// Label for shadow objects
	public string hintLabel = "_hint";			// Label for hint objects
	public int numOfSkippedLabels;				// Number of layers to separate
	public string[] separateLabels;				// List of labels for objects to separate
	public int maxTextureSize = 2048;			// Max Atlas size
	public string inputFile;					// File with data about objects
	public string localDirectory;				// Directory with .png files


	// Important internal variables - please don't change them blindly
	string[] lines;
	LayerData[] layers;
	LayerData[] separatedLayers;
	int layersLength = 0;
	int separatedLayersLength = 0;
	Texture2D[] textureArray;
	Texture2D[] separatedTextureArray;
	Material[] materials;
	Rect[] atlasRects; 
	Material atlasMaterial;


	[MenuItem ("Assets/Import HOG Scene...")]
	//-----------------------------------------------------------------------------------------------------	
	// Init editor interface window
	public static void Init()
	{
		EditorWindow.GetWindow(typeof(Scene_Importer)).Show();
	}

	//-----------------------------------------------------------------------------------------------------
	// Create UI
	void OnGUI() 
	{
		maxTextureSize = EditorGUILayout.IntField("Max texture size:", maxTextureSize);

		EditorGUILayout.TextField("Hint-object label: ", hintLabel);
		EditorGUILayout.TextField("Shadow-object label: ", shadowLabel);
		EditorGUILayout.TextField("Overlay-object label: ", overlayLabel);

		numOfSkippedLabels = EditorGUILayout.IntSlider(numOfSkippedLabels, 0, 10);
		if (separateLabels == null  ||  separateLabels.Length != numOfSkippedLabels) 
			separateLabels = new string[numOfSkippedLabels];


		for (int j = 0; j < numOfSkippedLabels; j++)
			separateLabels[j] = EditorGUILayout.TextField("Separate with label " + j.ToString(), separateLabels[j]);


		if(localDirectory != "") 
			EditorGUILayout.LabelField("Data-directory: ", localDirectory);
		
		if(inputFile != "") 
			EditorGUILayout.LabelField("Data-file: ", Path.GetFileName(inputFile));
		
		if(layersLength > 0)
			EditorGUILayout.LabelField("Number of layers: ", layersLength.ToString());	
		
		if(separatedLayersLength > 0)
			EditorGUILayout.LabelField("Number of separated layers: ", separatedLayersLength.ToString());	



		if(GUILayout.Button("Open data-file"))
		{ 
			// Open and parse  file with data about objects
			inputFile = EditorUtility.OpenFilePanel("Choose Layers data File to Import", Application.dataPath, "txt");
            if (inputFile == "") return;
            localDirectory  = Path.GetDirectoryName(inputFile);            

            localDirectory += localDirectory[2];
            localDirectory = localDirectory.Substring(localDirectory.IndexOf("Assets"));

			var sr = new StreamReader(inputFile);
			var fileContents = sr.ReadToEnd();
			sr.Close();
			lines = fileContents.Split("\n"[0]);

			layers = new LayerData[lines.Length/6];
			for (int i = 0; i < layers.Length; i++)  
				layers[i] = new LayerData();

			layersLength = 0;
			separatedLayersLength = 0;

			ParseTextFile ();   

		}


		if (inputFile != null  &&  inputFile.Length > 1)  
			if (GUILayout.Button("CREATE SCENE"))
			{ 
				// Generate objects and compose scene
				if( (inputFile != "") && inputFile.StartsWith(Application.dataPath))
				{ 
					Directory.CreateDirectory(localDirectory+"_Generated\\");
					Directory.CreateDirectory(localDirectory+"_Generated\\_Separated\\");

					textureArray = new Texture2D[layersLength];
					separatedTextureArray = new Texture2D[separatedLayersLength];
					materials = new Material[separatedLayersLength+1];

					ImportTextures (localDirectory);    
					GenerateGameObjects (localDirectory);   
				}

			}


		this.Repaint();
	}

	//-----------------------------------------------------------------------------------------------------
	// Parse TextFile  with data about objects
	// Info about all layer(each as separated .png ) saved with structure:
	//   Layer_3.png - layer/file name 
	//   723         - X position of layer in the image
	//   790         - Y position of layer in the image
	//   75          - X size of layer (width)
	//   91          - Y size of layer (height)
	//   ---------   - separator-string before information about next layer 
	void ParseTextFile () 
	{
		int layerID = 0;
		int stringNum = 0;


		foreach (var line in lines)
		{
			if (layerID < layers.Length)
				switch (stringNum)
				{
					case 0: 
						layers[layerID].name = line;
						stringNum++;

						if (separateLabels.Length == 0) 
							layersLength++;
						else
						{
							bool separate = false;
							for (int j = 0; j < separateLabels.Length; j++)  
								if (separateLabels[j] != ""  &&  layers[layerID].name.Contains(separateLabels[j]))
								{
									separate = true;
									break;
								}

							if (separate) 
								separatedLayersLength++;
							else 
								layersLength++;
						}
						break;

					case 1: 
						layers[layerID].offset.x = int.Parse(line);
						stringNum++;
						break;

					case 2: 
						layers[layerID].offset.y = int.Parse(line);
						stringNum++;
						break;

					case 3: 
						layers[layerID].size.x = int.Parse(line);
						stringNum++;
						break;

					case 4: 
						layers[layerID].size.y = int.Parse(line);
						stringNum++;
						break;

					case 5: 
						layers[layerID].offset.z = layerID; 
						stringNum = 0;
						layerID++;
						break;
				}

			EditorUtility.DisplayProgressBar( "Preparation",	"Parsing data-file...", layerID/(layers.Length-1.1f));
		}

		EditorUtility.ClearProgressBar();
	}

	//-----------------------------------------------------------------------------------------------------
	// Import and pack Textures, separating(using tag) them if needed
	void ImportTextures (string baseDirectory) 
	{
		int sepTexturesNum = 0;
		int TexturesNum = 0;


			// Import all textures to textureArray
		for (int i = 0; i < layers.Length; i++)  
		{
			string texturePathName = baseDirectory + layers[i].name;
			Texture2D inputTexture = AssetDatabase.LoadAssetAtPath(texturePathName, typeof(Texture2D)) as Texture2D;

			// modify the importer settings
			TextureImporter textureImporter = AssetImporter.GetAtPath(texturePathName) as TextureImporter;
			textureImporter.mipmapEnabled = false;
			textureImporter.isReadable = true;
			textureImporter.npotScale = TextureImporterNPOTScale.None;
			textureImporter.wrapMode = TextureWrapMode.Clamp;
			textureImporter.filterMode = FilterMode.Trilinear;
			textureImporter.textureType = TextureImporterType.GUI;

			AssetDatabase.WriteImportSettingsIfDirty (texturePathName);
			AssetDatabase.ImportAsset(texturePathName);

			//separating textures (using tag) them if needed
			bool separate = false;
			for (int j = 0; j < separateLabels.Length; j++)  
				if (layers[i].name.Contains(separateLabels[j]))
				{
					separate = true;
					break;
				}


			if (separate)
			{ 
				separatedTextureArray[sepTexturesNum] = new Texture2D ((int)layers[i].size.x, (int)layers[i].size.y);
				separatedTextureArray[sepTexturesNum] = inputTexture;
				sepTexturesNum++;
			}
			else
				{
					textureArray[TexturesNum] = new Texture2D ((int)layers[i].size.x, (int)layers[i].size.y);
					textureArray[TexturesNum] = inputTexture;
					TexturesNum++;
				}

		}


		// make assembled material
		string materialPath =  baseDirectory + "_Generated\\" + "_Assembled_Material.mat";
		string texturePath;

		if(File.Exists(materialPath) == true)
			File.Delete(materialPath);


		materials[0] = new Material (Shader.Find("Mobile/Particles/Alpha Blended"));
		AssetDatabase.CreateAsset(materials[0], materialPath);
		AssetDatabase.Refresh();
		materials[0] = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;


		// make materials and copy textures	
		for (int i = 1; i < materials.Length; i++)  
		{
			materialPath = baseDirectory + "_Generated\\_Separated\\"  + separatedTextureArray[i-1] + i.ToString() +".mat";
			texturePath =  baseDirectory + "_Generated\\_Separated\\"  + separatedTextureArray[i-1] + i.ToString()+ ".png";


			if(File.Exists(materialPath) == true)
				File.Delete(materialPath);


			materials[i] = new Material (Shader.Find("Mobile/Particles/Alpha Blended"));
			AssetDatabase.CreateAsset(materials[i], materialPath);
			AssetDatabase.CopyAsset(baseDirectory + separatedTextureArray[i-1].name + ".png", texturePath);
			AssetDatabase.Refresh();

			materials[i] = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
			materials[i].mainTexture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
		}


		// make a new atlas texture
		texturePath = baseDirectory + "_Generated\\" + "_Atlas.png";
		Texture2D atlas = new Texture2D(maxTextureSize, maxTextureSize);
		atlasRects = atlas.PackTextures(textureArray, 0, maxTextureSize);

		byte[] atlasPng = atlas.EncodeToPNG();

		if(File.Exists(texturePath) == true)
			File.Delete(texturePath);
		

		File.WriteAllBytes(texturePath, atlasPng);
		AssetDatabase.Refresh();

		// modify the importer settings
		TextureImporter atlasTextureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;

		atlasTextureImporter.mipmapEnabled = false;
		atlasTextureImporter.maxTextureSize = maxTextureSize;
		atlasTextureImporter.wrapMode = TextureWrapMode.Clamp;
		atlasTextureImporter.filterMode = FilterMode.Trilinear;
		atlasTextureImporter.textureType = TextureImporterType.GUI;


		AssetDatabase.WriteImportSettingsIfDirty(texturePath);
		AssetDatabase.ImportAsset(texturePath);
		atlas = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;

		AssetDatabase.Refresh();

		// be sure atlas is linked to material
		materials[0].mainTexture = atlas;

	}

	//-----------------------------------------------------------------------------------------------------
	// Generate GameObjects
	void GenerateGameObjects (string baseDirectory) 
	{
			// create a root game object for the images
		GameObject rootLayerGo = new GameObject("Root_Layer");
		GameObject rootLayerSparatedGo = new GameObject("_SeparatedObjects");
		GameObject rootLayerOverlayGo = new GameObject("_OverlayObjects");
		GameObject rootLayerActiveGo = new GameObject("_ActiveObjects");

		rootLayerGo.transform.position = Vector3.zero;
		rootLayerGo.AddComponent<ObjectsInitializer> ();

		rootLayerSparatedGo.transform.parent = rootLayerGo.transform;
		rootLayerSparatedGo.transform.position = Vector3.zero;

		rootLayerOverlayGo.transform.parent = rootLayerGo.transform;
		rootLayerOverlayGo.transform.position = Vector3.zero;  

		rootLayerActiveGo.transform.parent = rootLayerGo.transform;
		rootLayerActiveGo.transform.position = Vector3.zero;  


		int sepTexturesNum = 0;
		int TexturesNum = 0;

		for (int i = 0; i < layers.Length; i++)  
		{
			// setup the game object
			GameObject layerGo = new GameObject(Path.GetFileNameWithoutExtension(layers[i].name));


			bool separate = false;
			for (int j = 0; j < separateLabels.Length; j++)  
				if (layers[i].name.Contains(separateLabels[j]))
				{
					separate = true;
					break;
				}

			// separate them if needed
			if (separate)
			{
				CreateMeshes(layerGo, separatedTextureArray[sepTexturesNum], materials[sepTexturesNum+1], new Rect(0,0,1,1), baseDirectory + "_Generated\\_Separated\\"+ "mesh_separated_" + layers[i].name + ".asset");

				layerGo.transform.parent = rootLayerSparatedGo.transform;
				layerGo.transform.position = new Vector3 (
															layers[i].offset.x + separatedTextureArray[sepTexturesNum].width  * 0.5f,
															-layers[i].offset.y - separatedTextureArray[sepTexturesNum].height * 0.5f,
															layers[i].offset.z
														 );

				sepTexturesNum++;
			}
			else
				{
					CreateMeshes(layerGo, textureArray[TexturesNum], materials[0], atlasRects[TexturesNum], baseDirectory + "_Generated\\"+ "mesh_" + layers[i].name + ".asset");

					if (layers[i].name.Contains(overlayLabel)) 
						layerGo.transform.parent = rootLayerOverlayGo.transform;
					else	
						layerGo.transform.parent = rootLayerActiveGo.transform;

					layerGo.transform.position = new Vector3 (
																layers [i].offset.x + textureArray [TexturesNum].width * 0.5f,
																-layers [i].offset.y - textureArray [TexturesNum].height * 0.5f,
																layers [i].offset.z
															 );
					TexturesNum++;
				}


		}

		ArrangeObjects (rootLayerActiveGo); 

	}

	//-----------------------------------------------------------------------------------------------------
	// Create mesh 
	void CreateMeshes (GameObject go, Texture2D texture, Material material, Rect uvRect, string meshPath)
	{
		// create meshFilter if new
		MeshFilter meshFilter = go.GetComponent<MeshFilter>();
		if(meshFilter == null)	
			meshFilter = go.AddComponent<MeshFilter>();


		// create mesh if new
		Mesh mesh = meshFilter.sharedMesh;
		if(mesh == null)  
			mesh = new Mesh();
		mesh.Clear();

		// setup rendering
		MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
		if(meshRenderer == null)  
			meshRenderer = go.AddComponent<MeshRenderer>();

		meshRenderer.GetComponent<Renderer>().material = material;
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;


		// create the mesh geometry
		// Unity winding order is counter-clockwise when viewed from behind and facing forward (away)
		// Unity winding order is clockwise when viewed from behind and facing behind
		// 1---2
		// |  /|
		// | / |
		// 0---3
		Vector3[] newVertices;
		int[] newTriangles;
		Vector2[] uvs;

		float hExtent = texture.width * 0.5f;
		float vExtent = texture.height * 0.5f;

		newVertices = new Vector3[4];
		newVertices[0] = new Vector3(-hExtent, -vExtent, 0);
		newVertices[1] = new Vector3(-hExtent, vExtent, 0);
		newVertices[2] = new Vector3(hExtent, vExtent, 0);
		newVertices[3] = new Vector3(hExtent, -vExtent, 0);

		newTriangles = new int[6];  
		newTriangles[0] = 0; 
		newTriangles[1] = 1; 
		newTriangles[2] = 2; 
		newTriangles[3] = 0; 
		newTriangles[4] = 2; 
		newTriangles[5] = 3;

		uvs = new Vector2[4];
		uvs[0] = new Vector2(uvRect.x, uvRect.y);
		uvs[1] = new Vector2(uvRect.x, uvRect.y + uvRect.height);
		uvs[2] = new Vector2(uvRect.x + uvRect.width, uvRect.y + uvRect.height);
		uvs[3] = new Vector2(uvRect.x + uvRect.width, uvRect.y);

		Color[] vertColors = new Color[4];
		vertColors[0] = Color.white;
		vertColors[1] = Color.white;
		vertColors[2] = Color.white;
		vertColors[3] = Color.white;

		// update the mesh and generate some some normals for the mesh
		mesh.vertices = newVertices; 
		mesh.colors = vertColors;
		mesh.uv = uvs; 
		mesh.triangles = newTriangles;
		mesh.normals = new Vector3[4];
		mesh.RecalculateNormals();

		if(File.Exists(meshPath) == true)
			File.Delete(meshPath);
		

		AssetDatabase.CreateAsset(mesh, meshPath);
		AssetDatabase.Refresh();

		meshFilter.sharedMesh = mesh;

		//go.AddComponent<MeshCollider>();
	}


	//-----------------------------------------------------------------------------------------------------
	// Arange objects by order (z-position) using tags and add related scripts/components
	void ArrangeObjects (GameObject parent) 
	{
		Transform[] transforms;
		List<GameObject> shadows = new List<GameObject>();
		List<GameObject> objects = new List<GameObject>();


		transforms = parent.GetComponentsInChildren<Transform>();

		// Get all objects and separate Shadows from ActiveObjects
		foreach (Transform childTransform in transforms) 
			if (!childTransform.gameObject.name.Contains(shadowLabel)) 
				objects.Add(childTransform.gameObject);
			else
				shadows.Add(childTransform.gameObject);

			// Arrange objects according to their type
		foreach (GameObject tmpObject in objects)
		{
			// Set shadows as child
			foreach (GameObject tmpShadow in shadows)
				if (tmpShadow.name.Contains(tmpObject.name) ) 
					tmpShadow.transform.parent = tmpObject.transform;

			// HintObject and ActiveObject   
			tmpObject.AddComponent<BoxCollider2D>();

			if (tmpObject.name.Contains(hintLabel))  tmpObject.AddComponent<HintObject>();
			else
				tmpObject.AddComponent<ActiveObject>(); 
		}

		DestroyImmediate(parent.GetComponent("ActiveObject"));
	}

 //-----------------------------------------------------------------------------------------------------
}
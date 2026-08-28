//-----------------------------------------------------------------------------------------------------	
// Editor script
// Pack bunch of .png files to atlas and create Animation from them as frames
// Also creates plane(sprite) with animated material
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityEngine.Rendering;


public class Animation_Importer : EditorWindow  
{
	public int maxTextureSize = 2048;		// Max Atlas size
	public string localDirectory;			// Directory with frames (.png files)
	public Vector3 objectPosition;			// Position of created object
	public float animationSpeed;			// Animation speed (time  beetween frames)
	public GameObject rootObject;			// Optional parent object


	// Important internal variables - please don't change them blindly
	List<string> files = new List <string> ();
	Texture2D[] textureArray;
	Material material;
	Rect[] atlasRects; 
	Material atlasMaterial;


	[MenuItem ("Assets/Import Animation...")]
	//-----------------------------------------------------------------------------------------------------	
	// Init editor interface window
	public static void Init()
	{
		EditorWindow.GetWindow(typeof(Animation_Importer)).Show();
	}

    //-----------------------------------------------------------------------------------------------------
    // Create UI
    void OnGUI()
    {
        maxTextureSize = EditorGUILayout.IntField("Max atlas size:", maxTextureSize);
        objectPosition = EditorGUILayout.Vector3Field("Position:", objectPosition);
        animationSpeed = EditorGUILayout.FloatField("Animation Speed:", animationSpeed);


        EditorGUILayout.PrefixLabel("Root Object:");
        rootObject = EditorGUILayout.ObjectField(rootObject, typeof(GameObject), true) as GameObject;

        if (localDirectory != "")
            EditorGUILayout.LabelField("Data-directory: ", localDirectory);

        if (files != null && files.Count > 0)
            EditorGUILayout.LabelField("Number of frames: ", files.Count.ToString());


        if (GUILayout.Button("Open Data-directory"))
        {
            files.Clear();

            localDirectory = EditorUtility.OpenFolderPanel("Load png Textures of Directory", Application.dataPath, "");
            if (localDirectory == "") return;

            localDirectory += localDirectory[2];
            localDirectory = localDirectory.Substring(localDirectory.IndexOf("Assets"));


            string[] tmpFiles  = Directory.GetFiles(localDirectory);

			// Get all files/frames
			for (int i = 0; i < tmpFiles.Length; i++)  
				if(tmpFiles[i].EndsWith(".png")) 
					files.Add (Path.GetFileName(tmpFiles[i]));

		}


		if(localDirectory != null  &&  localDirectory.Length > 1)  
			if(GUILayout.Button("CREATE ANIMATION"))
			{ 
				// Generate animation etc.	
				Directory.CreateDirectory(localDirectory+"_Generated\\");

				textureArray = new Texture2D[files.Count];

				ImportTextures ();    
				GenerateGameObject ();   
			}


		this.Repaint();
	}

	//-----------------------------------------------------------------------------------------------------
	// Import and pack Textures
	void ImportTextures () 
	{
		// Import all textures to textureArray
		for (int i = 0; i < files.Count; i++)  
		{
			string texturePathName = localDirectory + files[i];
			Texture2D inputTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePathName);

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

			textureArray[i] = new Texture2D(inputTexture.width, inputTexture.height);
			textureArray[i] = inputTexture;

		}


		// make assembled material
		string materialPath =  localDirectory + "_Generated\\" + "_Assembled_Material.mat";
		string texturePath;

		if(File.Exists(materialPath) == true)
		{
			File.Delete(materialPath);
			AssetDatabase.Refresh();
		}
		material = new Material (Shader.Find("Mobile/Particles/Alpha Blended"));
		AssetDatabase.CreateAsset(material, materialPath);
		AssetDatabase.Refresh();
		material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);


		// make a new atlas texture
		texturePath = localDirectory + "_Generated\\" + "_Atlas.png";
		Texture2D atlas = new Texture2D(maxTextureSize, maxTextureSize);
		atlasRects = atlas.PackTextures(textureArray, 0, maxTextureSize);

		byte[] atlasPng = atlas.EncodeToPNG();

		if(File.Exists(texturePath) == true)
		{
			File.Delete(texturePath);
			AssetDatabase.Refresh();
		}

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
		atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

		AssetDatabase.Refresh();


		// be sure atlas is linked to material
		material.mainTexture = atlas;

	}

	//-----------------------------------------------------------------------------------------------------
	// Create a root game object for the images
	void GenerateGameObject () 
	{
		// setup the game object
		GameObject layerGo = new GameObject(Path.GetFileNameWithoutExtension(files[0] as String));


		CreateMesh(layerGo, textureArray[0], material, atlasRects[0], localDirectory + "_Generated\\"+ "mesh_" + files[0] + ".asset");

		if (rootObject)
		{
			rootObject.transform.position = Vector3.zero;
			layerGo.transform.parent = rootObject.transform;
		}

		layerGo.transform.position =  new Vector3 (
													objectPosition.x + textureArray[0].width  * 0.5f, 
													-objectPosition.y - textureArray[0].height * 0.5f,
													objectPosition.z
													);

		AnimationClip animationClip = CreateAnimation();

		layerGo.AddComponent<Animation>();
		layerGo.GetComponent<Animation>().AddClip (animationClip, animationClip.name);
		layerGo.GetComponent<Animation>().clip = animationClip;

	}

	//-----------------------------------------------------------------------------------------------------
	// Create mesh
	void CreateMesh(GameObject go, Texture2D texture, Material material, Rect uvRect, string meshPath)
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
		{
			File.Delete(meshPath);
			AssetDatabase.Refresh();
		}

		AssetDatabase.CreateAsset(mesh, meshPath);
		AssetDatabase.Refresh();

		meshFilter.sharedMesh = mesh;

		//go.AddComponent<MeshCollider>();
	}

	//-----------------------------------------------------------------------------------------------------
	// Create Animation clip
	AnimationClip CreateAnimation () 
	{
		// Setup animation
		Keyframe[] ksX = new Keyframe[atlasRects.Length];
		Keyframe[] ksY = new Keyframe[atlasRects.Length];
		AnimationClip clip = new AnimationClip();
		clip.legacy = true;

		if (animationSpeed <= 0) animationSpeed = 1;

		for(int i = 0; i < atlasRects.Length ; i++)
		{
			ksX[i] =  new Keyframe(i/animationSpeed, atlasRects[i].x - atlasRects[0].x);   
			ksY[i] =  new Keyframe(i/animationSpeed, atlasRects[i].y - atlasRects[0].y);  

		}	

	
		AnimationCurve animX = new AnimationCurve(ksX);
		AnimationCurve animY = new AnimationCurve(ksY);

		clip.name = files[0] as string;
		clip.SetCurve("", typeof(Material), "_MainTex.offset.x", animX);
		clip.SetCurve ("", typeof(Material), "_MainTex.offset.y", animY);
		clip.wrapMode = WrapMode.Loop;


		AssetDatabase.CreateAsset(clip, localDirectory + "_Generated\\"+ "clip_" + files[0] + ".asset");
		AssetDatabase.WriteImportSettingsIfDirty(localDirectory + "_Generated\\"+ "clip_" + files[0] + ".asset");
		AssetDatabase.ImportAsset(localDirectory + "_Generated\\"+ "clip_" + files[0] + ".asset");
		AssetDatabase.Refresh();

		return clip;
	}

	//-----------------------------------------------------------------------------------------------------
}
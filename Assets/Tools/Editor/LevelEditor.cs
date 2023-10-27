using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;

public class LevelEditor : EditorWindow
{

    Vector2 scrollPosition;
    float prefabAreaWidth;
    float prefabAreaHeight;

    GameObject selectedPrefab;


    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow() // Show the Level Editor Window
    {
        EditorWindow.GetWindow<LevelEditor>();
    }

    void OnEnable()
    {
        // so that the script can access click events on the scene view
        SceneView.duringSceneGui += OnSceneGUI; 
    }

    private void OnDisable()
    {
        // removes the sript access to the scene view
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event eventCurrent = Event.current;

        if (eventCurrent.type == EventType.MouseDown && eventCurrent.button == 0)
        {
            //cast a ray to check where to place the object in the worldspace
            Ray ray = HandleUtility.GUIPointToWorldRay(eventCurrent.mousePosition);
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(ray, out hitInfo);
            if (selectedPrefab != null)
            {
                // instantiate the prefab into the world
                GameObject newObject = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
                Transform transform = newObject.transform;

                //records the object so that it can be undone and sets it as dirty (so that unity saves it)
                Undo.RegisterCreatedObjectUndo(newObject, "Place Object");
                EditorUtility.SetDirty(newObject);

                MeshRenderer renderer = newObject.GetComponent<MeshRenderer>();
                float meshHeight = renderer.bounds.size.y;

                if (hit) // if the prefab was placed on another GameObject
                {
                    transform.position = hitInfo.point;
                    transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y + meshHeight/2, Mathf.Round(transform.position.z));
                }
                else // if the prefab wasn't placed on anything
                {
                    float t = -ray.origin.y / ray.direction.y; // to calculate the distance along the ray where it intersects the 0 plane
                    transform.position = ray.origin + t * ray.direction; // this represents the intersection point of the ray and the y plane
                    transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y + meshHeight / 2, Mathf.Round(transform.position.z));
                }
            }

        Event.current.Use();
        }
   }

    void OnGUI() 
    {

        // BEGIN PREFAB AREA
        prefabAreaWidth = GetWindow<LevelEditor>().position.width - 10;
        prefabAreaHeight = GetWindow<LevelEditor>().position.height / 2;

        GUILayout.BeginArea(new Rect(10, 10, prefabAreaWidth, prefabAreaHeight));

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        int selected = 0;
        string[] folderOptions = new string[] // will automatically add list of folders within prefab folder eventually
        {
            "option 1", "option 2", "option 3"
        };

        // used to select a folder, nonfunctional at the moment
        selected = EditorGUILayout.Popup("Prefab Folder", selected, folderOptions);

        int buttonSize = 80; // Adjust the size of the buttons as needed

        // Getting a list of all prefabs in a specific folder
        string folderPath = "Assets/Prefabs"; // Change this to the path of your folder
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        GUILayout.BeginHorizontal();
        //creates buttons with texture of prefab
        foreach (string prefabGuid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Texture2D prefabImage = AssetPreview.GetAssetPreview(prefab);

            if (prefabImage != null)
            {
                if (GUILayout.Button(prefabImage, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
                {
                    selectedPrefab = prefab; // on mouse click input, instantiate the prefab in the world.
                    Debug.Log("Selected " + selectedPrefab);
                }
            }
        }


        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }
}
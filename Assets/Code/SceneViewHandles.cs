using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

internal static class SceneViewHandles
{
    private const float UPDATE_TIME = 5f;	// Amount of time between periodic re-scanning of the scene for newly changed components.
    private static float t = 0f;
    private static List<MonoAttributeCollection> attributes;
    private static Dictionary<Type, Dictionary<Type, SVHandleDisplay>> handleDisplays;
    private static List<Type> preCheckedTypes;
    private static List<MonoBehaviour> activeSceneBuffer;

    [InitializeOnLoadMethod]
    private static void Init()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        EditorApplication.update += Update;
        EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
        EditorApplication.playModeStateChanged += OnPlayStateChanged;

        // Load the types via reflection, as this was the best way to get a smooth end-user experience.
        LoadDisplaysViaReflection();
        PreCheckTypesViaReflection();

        LoadPrefs();
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (attributes == null)
            SweepForComponents();

        Color cachedHandlesColor = Handles.color;

        for(int i = attributes.Count; i >= 0; i--)
        {
            MonoAttributeCollection attribute = attributes[i];

            // If there is no mono instance, assume our component has been removed.
            if (attribute.MonoInstance == null)
            {
                attributes.Remove(attribute);
                continue;
            }

            if(displayMode == DisplayMode.SelectedObject && !Selection.Contains(attribute.MonoInstance.gameObject))
                    continue;
			
            // Draw our SVDebugs
            foreach (var kvPair in attribute.SVHandles)
            {
                object value = kvPair.Key.GetValue(attribute.MonoInstance);
                Type type = kvPair.Key.FieldType;

                if (value == null)
                {
                    attributes.Remove(attribute);
                    break;
                }

                if (!handleDisplays.ContainsKey(type))
                {
                    Debug.LogWarning("Attempt to draw debug for a type which doesn't have a ITypeDisplay.\nPerhaps you should add one?");
                    continue;
                }

                SVArgs args = new SVArgs(value, attribute.MonoInstance);

                Handles.color = attribute.SVHandles[kvPair.Key].Color;

                EditorGUI.BeginChangeCheck();
                {
                    // If the attribute has a specific Display defined, use that.
                    if (kvPair.Value.Type != null)
                        handleDisplays[type][kvPair.Value.Type].Draw(args, ref value);
                    else
                    {
                        SVHandleDisplay currentDisp = null;
                        using (var e = handleDisplays[type].Values.GetEnumerator())
                        {
                            while (e.MoveNext())
                            {
                                if (currentDisp == null || e.Current.Priority > currentDisp.Priority)
                                    currentDisp = e.Current;
                                e.MoveNext();
                            }
                        }

                        if (currentDisp != null)
                            currentDisp.Draw(args, ref value);
                    }
                }
                if (EditorGUI.EndChangeCheck())
                    kvPair.Key.SetValue(attribute.MonoInstance, value);
            }
        }

        Handles.color = cachedHandlesColor;
	}

    private static void Update()
    {
        // Do the logic for our periodic update.
        // This is a requirement because from time to time Unity doesn't seem to trigger callbacks correctly
        t += Time.unscaledDeltaTime;
        if (t > UPDATE_TIME)
        {
            t = 0f;
            SweepForComponents();
        }
    }

    private static void OnHierarchyChanged()
    {
        SweepForComponents();
    }

    /// <summary>
    /// Triggered when the play state changes.
    /// </summary>
    /// <remarks>
    /// We need this method because without it our code glitches when exiting play mode. The solution to this is to
    /// fully reset our window whenever we exit playmode, effectively faking our own 'recompile'.
    /// (It'd be nice if Unity recompiled the project or atleast re-called InitializeOnLoad methods when exiting playmode,
    ///     but oh well. :/ )
    /// </remarks>
    /// <param name="change"></param>
    private static void OnPlayStateChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredEditMode) return;
        // Force refresh our components
        attributes.Clear();
        handleDisplays.Clear();
        SweepForComponents();
        LoadDisplaysViaReflection();
    }

    /// <summary>
    /// Sweeps through the current scene for any MonoBehaviour objects, then checks each for any 
    /// fields with the [DrawDebug] attribute, then finally stores this information in our Attributes list.
    /// </summary>
    private static void SweepForComponents()
    {
        if (attributes == null)
            attributes = new List<MonoAttributeCollection>();
        if (activeSceneBuffer == null)
	        activeSceneBuffer = new List<MonoBehaviour>();

        activeSceneBuffer.Clear();

        foreach (var type in preCheckedTypes)
	        activeSceneBuffer.AddRange(Object.FindObjectsOfType(type).AsEnumerable() as IEnumerable<MonoBehaviour>);

        foreach (MonoBehaviour mono in activeSceneBuffer)
        {
            Type monoType = mono.GetType();

            // See if this component has any fields with our attribute
            FieldInfo[] fields = monoType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            bool hasHandleAttribute = fields.Any(field => Attribute.GetCustomAttribute(field, typeof(SVHandleAttribute)) is SVHandleAttribute);
            if (!hasHandleAttribute)
                continue;

            // If we haven't exited by this point, we know there is atleast one field with our attribute.
            MonoAttributeCollection attrib = GetOrCreateAttributeCollection(mono);

            foreach (FieldInfo field in fields)
            {
                SVHandleAttribute svHandleAttributeAttrib = Attribute.GetCustomAttribute(field, typeof(SVHandleAttribute)) as SVHandleAttribute;

				if (svHandleAttributeAttrib == null)
					continue;
				if (attrib.SVHandles.ContainsKey(field))
					continue;
				attrib.SVHandles.Add(field, svHandleAttributeAttrib);
			}
        }
    }

    /// <summary>
    /// Searches through out attribute list to see if one exists with the given mono instance.
    /// If none exist, it creates a new AttributeInstance with the given mono instance and adds it to Attributes.
    /// </summary>
    /// <param name="mono"></param>
    /// <returns></returns>
    private static MonoAttributeCollection GetOrCreateAttributeCollection(MonoBehaviour mono)
    {
		foreach (var attribute in attributes)
		{
			if (attribute.MonoInstance == mono)
				return attribute;
		}

		var attributeCollection = new MonoAttributeCollection(mono);
        attributes.Add(attributeCollection);
        return attributeCollection;
    }

    /// <summary>
    /// Simple class that holds all of the info for a monobehaviour instance of our attribute.
    /// We could split this further into an instance per-field, but I feel that is unnecessary,
    /// considering they would all reference the same Mono instance anyways.
    /// </summary>
    private class MonoAttributeCollection
    {
        public MonoBehaviour MonoInstance;
        public Dictionary<FieldInfo, SVHandleAttribute> SVHandles;

        public MonoAttributeCollection(MonoBehaviour mono)
        {
            MonoInstance = mono;
            SVHandles = new Dictionary<FieldInfo, SVHandleAttribute>();
        }
    }

    /// <summary>
    /// Loops through all of the current assemblies, finding every class that inherits ITypeDebugDisplay or ITypeHandleDisplay.
    /// Then uses Activator to create a new instance of the Type, and adds it's definition to the corresponding dictionary.
    /// </summary>
    private static void LoadDisplaysViaReflection()
    {
        if (handleDisplays == null)
            handleDisplays = new Dictionary<Type, Dictionary<Type, SVHandleDisplay>>();
        else
            handleDisplays.Clear();

		// TODO - Convert to alternative as LINQ isn't necessary here.
        IEnumerable<SVHandleDisplay> hDisplays= AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(t => t.GetTypes())
            .Where(t => t.IsClass && typeof(SVHandleDisplay).IsAssignableFrom(t) && t != typeof(SVHandleDisplay))
            .Select(t => (SVHandleDisplay) Activator.CreateInstance(t));

        foreach (var hd in hDisplays)
        {
            if (handleDisplays.ContainsKey(hd.ExecutingType))
                handleDisplays[hd.ExecutingType].Add(hd.GetType(), hd);
            else
                handleDisplays.Add(hd.ExecutingType, new Dictionary<Type, SVHandleDisplay>(){{hd.GetType(), hd}});
        }
    }

    /// <summary>
    /// Loops through all classes in all assemblies, checking to see if there are any
    /// variables which have an [SVHandle] attribute. If so, add the type to a
    /// list. This way we don't pull all MonoBehaviour instances, but instead only
    /// pull from any known type to have our attribute.
    /// </summary>
    private static void PreCheckTypesViaReflection()
    {
        if (preCheckedTypes == null)
            preCheckedTypes = new List<Type>();
        else
            preCheckedTypes.Clear();

        IEnumerable<Type> kAT = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(t => t.GetTypes())
            .Where(t => t.IsClass && typeof(MonoBehaviour).IsAssignableFrom(t))
            .Where(t => t.GetFields().Any(f => Attribute.GetCustomAttribute(f, typeof(SVHandleAttribute)) is SVHandleAttribute));

        preCheckedTypes = kAT.ToList();
    }

    #region Utilities

    /// <summary>
    /// Does fading automatically using Handles.color.
    /// </summary>
    /// <param name="position"></param>
    /// <returns>If the object is completely faded out, if so it is recommended that you just return.</returns>
    public static bool DoFade(Vector3 position)
	{
		return false;
        Color c = Handles.color;
        float actualDist = Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, position);
        float value = actualDist / fadeDistance;
        c.a = 1-value;
        c.a = Mathf.Max(0, c.a);
        Handles.color = c;
        return c.a <= 0.05f;
    }
    #endregion

    #region Config

    private static bool prefsLoaded = false;

    private static float fadeDistance = 25f;
    private static DisplayMode displayMode;

    [PreferenceItem("Scene View Handles")]
    private static void ConfigGUI()
    {
        if (!prefsLoaded)
        {
            LoadPrefs();
        }

        fadeDistance = EditorGUILayout.Slider("Fade Distance", fadeDistance, 0f, 50f);

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Display Type:");
            displayMode = (DisplayMode) GUILayout.SelectionGrid((int) displayMode, new string[] {"Selected Object", "All Objects"}, 2);
        }
        GUILayout.EndHorizontal();

        if (GUI.changed)
            SavePrefs();
    }

    private static void LoadPrefs()
    {
        fadeDistance = EditorPrefs.GetFloat("SV_FadeDistance");
        displayMode = (DisplayMode) EditorPrefs.GetInt("SV_DisplayMode");
		
		prefsLoaded = true;
    }

    private static void SavePrefs()
    {
        EditorPrefs.SetFloat("SV_FadeDistance", fadeDistance);
        EditorPrefs.SetInt("SV_DisplayMode", (int)displayMode);
    }

    private enum DisplayMode
    {
        SelectedObject, All
    }
    #endregion
}
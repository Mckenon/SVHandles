using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

internal static class SceneViewHandles
{
    private const float UPDATE_TIME = 5f;                               // Value used to periodically update our attributes.
    private static float t = 0f;                                        // Value used to keep track of time between updates.
    private static List<AttributeInstance> attributes;                  // A list of attribute instances we keep cached for re-use.
    private static Dictionary<Type, ITypeDebugDisplay> debugDisplays;   // Dictionary full of display methods for various types, loaded via reflection.
    private static Dictionary<Type, ITypeHandleDisplay> handleDisplays; // Dictionary full of handle methods for various types

    [InitializeOnLoadMethod]
    private static void Init()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        EditorApplication.update += Update;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        EditorApplication.playModeStateChanged += OnPlayStateChanged;

        // Load the types via reflection, because I'm a lazy dev who doesn't like typing out stuff.
        // (Not to mention it makes the end-user experience smooth as hell)
        LoadDisplaysViaReflection();
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (attributes == null)
            SweepForComponents();

        for(int i = attributes.Count; i --> 0;)
        {
            var attrib = attributes[i];

            // If there is no mono instance, assume our component has been removed.
            if (attrib.MonoInstance == null)
            {
                attributes.Remove(attrib);
                continue;
            }

            // TODO - Do something to make this not be mostly two copied code blocks.

            Color cachedColor = Handles.color;

            // Draw our SVDebugs
            foreach (var kvPair in attrib.SVDebugs)
            {
                object value = kvPair.Key.GetValue(attrib.MonoInstance);
                Type type = kvPair.Key.FieldType;

                if (value == null)
                {
                    attributes.Remove(attrib);
                    break;
                }

                if (!debugDisplays.ContainsKey(type))
                {
                    Debug.LogWarning("Attempt to draw debug for a type which doesn't have a ITypeDisplay.\nPerhaps you should add one?");
                    continue;
                }

                SVArgs args = new SVArgs(value, attrib.MonoInstance);

                Handles.color = attrib.SVDebugs[kvPair.Key].Color;

                // Run a display method from an instance of ITypeDisplay, using our type as an index in the dictionary.
                debugDisplays[type].Draw(args);
            }

            Handles.color = cachedColor;

            // Draw our SVDebugs
            foreach (var kvPair in attrib.SVHandles)
            {
                object value = kvPair.Key.GetValue(attrib.MonoInstance);
                Type type = kvPair.Key.FieldType;

                if (value == null)
                {
                    attributes.Remove(attrib);
                    break;
                }

                if (!handleDisplays.ContainsKey(type))
                {
                    Debug.LogWarning("Attempt to draw debug for a type which doesn't have a ITypeDisplay.\nPerhaps you should add one?");
                    continue;
                }

                SVArgs args = new SVArgs(value, attrib.MonoInstance);

                Handles.color = attrib.SVHandles[kvPair.Key].Color;

                // Run a display method from an instance of ITypeDisplay, using our type as an index in the dictionary.
                object outValue = handleDisplays[type].Draw(args);

                // We allow for null checking here so that you can micro-optimize away the call to SetValue if there are no changes.
                if (outValue != null)
                    kvPair.Key.SetValue(attrib.MonoInstance, outValue);
            }

            Handles.color = cachedColor;
        }
    }

    private static void Update()
    {
        // Do the logic for our periodic update.
        // This update isn't really necessary, and is only here as a precaution in case something bad happens.
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
        debugDisplays.Clear();
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
            attributes = new List<AttributeInstance>();

        MonoBehaviour[] activeScene = Object.FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour mono in activeScene)
        {
            Type monoType = mono.GetType();

            // See if this component has any fields with our attribute
            FieldInfo[] fields = monoType.GetFields(BindingFlags.Instance | BindingFlags.Public);

            bool hasDebugAttrib = fields.Any(field => Attribute.GetCustomAttribute(field, typeof(SVDebug)) is SVDebug || Attribute.GetCustomAttribute(field, typeof(SVHandle)) is SVHandle);
            if (!hasDebugAttrib)
                continue;

            // If we haven't exited by this point, we know there is atleast one field with our attribute.
            AttributeInstance attrib = GetOrCreateAttributeInstance(mono);

            foreach (var field in fields)
            {
                SVDebug svDebugAttrib = Attribute.GetCustomAttribute(field, typeof(SVDebug)) as SVDebug;
                SVHandle svHandleAttrib = Attribute.GetCustomAttribute(field, typeof(SVHandle)) as SVHandle;

                if (svDebugAttrib != null)
                    if (!attrib.SVDebugs.ContainsKey(field))
                        attrib.SVDebugs.Add(field, svDebugAttrib);
                if(svHandleAttrib != null)
                    if (!attrib.SVHandles.ContainsKey(field))
                        attrib.SVHandles.Add(field, svHandleAttrib);
            }
        }
    }

    /// <summary>
    /// Searches through out attribute list to see if one exists with the given mono instance.
    /// If none exist, it creates a new AttributeInstance with the given mono instance and adds it to Attributes.
    /// </summary>
    /// <param name="mono"></param>
    /// <returns></returns>
    private static AttributeInstance GetOrCreateAttributeInstance(MonoBehaviour mono)
    {
        foreach(var attrib in attributes)
            if (attrib.MonoInstance == mono)
                return attrib;
        var newAttrib = new AttributeInstance(mono);
        attributes.Add(newAttrib);
        return newAttrib;
    }

    /// <summary>
    /// Simple class that holds all of the info for a monobehaviour instance of our attribute.
    /// We could split this further into an instance per-field, but I feel that is unnecessary,
    /// considering they would all reference the same Mono instance anyways.
    /// </summary>
    private class AttributeInstance
    {
        public MonoBehaviour MonoInstance;
        public Dictionary<FieldInfo, SVDebug> SVDebugs;
        public Dictionary<FieldInfo, SVHandle> SVHandles;

        public AttributeInstance(MonoBehaviour mono)
        {
            MonoInstance = mono;
            SVDebugs = new Dictionary<FieldInfo, SVDebug>();
            SVHandles = new Dictionary<FieldInfo, SVHandle>();
        }
    }

    /// <summary>
    /// Loops through all of the current assemblies, finding every class that inherits ITypeDebugDisplay or ITypeHandleDisplay.
    /// Then uses Activator to create a new instance of the Type, and adds it's definition to the corresponding dictionary.
    /// </summary>
    private static void LoadDisplaysViaReflection()
    {
        debugDisplays = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(t => t.GetTypes())
            .Where(t => t.IsClass && typeof(ITypeDebugDisplay).IsAssignableFrom(t))
            .Select(t => (ITypeDebugDisplay) Activator.CreateInstance(t))
            .ToDictionary(t => t.ExecutingType, t => t);
        handleDisplays = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(t => t.GetTypes())
            .Where(t => t.IsClass && typeof(ITypeHandleDisplay).IsAssignableFrom(t))
            .Select(t => (ITypeHandleDisplay)Activator.CreateInstance(t))
            .ToDictionary(t => t.ExecutingType, t => t);
    }

    #region Utilities

    /// <summary>
    /// Does fading automatically using Handles.color.
    /// </summary>
    /// <param name="position"></param>
    /// <returns>If the object is completely faded out, if so it is recommended that you just return.</returns>
    public static bool DoFade(Vector3 position)
    {
        Color c = Handles.color;
        float actualDist = Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, position);
        float value = actualDist / fadeDistance;
        c.a = 1-value;
        c.a = Mathf.Max(0, c.a);
        Handles.color = c;
        return c.a == 0;
    }
    #endregion

    #region Config

    private static bool prefsLoaded = false;

    private static float fadeDistance = 25f;

    [PreferenceItem("Scene View Handles")]
    private static void ConfigGUI()
    {
        if (!prefsLoaded)
        {
            LoadPrefs();
            prefsLoaded = true;
        }

        fadeDistance = EditorGUILayout.Slider("Fade Distance", fadeDistance, 0f, 50f);

        if (GUI.changed)
            SavePrefs();
    }

    private static void LoadPrefs()
    {
        fadeDistance = EditorPrefs.GetFloat("SV_FadeDistance");
    }

    private static void SavePrefs()
    {
        EditorPrefs.SetFloat("SV_FadeDistance", fadeDistance);
    }
    #endregion
}
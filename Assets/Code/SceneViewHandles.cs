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
    private const float UPDATE_TIME = 5f;                   // Value used to periodically update our attributes.
    private static float t = 0f;                            // Value used to keep track of time between updates.
    private static List<AttributeInstance> Attributes;      // A list of attribute instances we keep cached for re-use.
    private static Dictionary<Type, ITypeDisplay> Displays; // Dictionary full of display methods for various types, loaded via reflection.

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
        if (Attributes == null)
            SweepForComponents();

        for(int i = Attributes.Count; i --> 0;)
        {
            var attrib = Attributes[i];

            // If there is no mono instance, assume our component has been removed.
            if (attrib.MonoInstance == null)
            {
                Attributes.Remove(attrib);
                continue;
            }

            // Go through each field, and draw whatever visual the type requires.
            foreach (var field in attrib.Fields)
            {
                object value = field.GetValue(attrib.MonoInstance);
                Type type = field.FieldType;

                if (value == null)
                {
                    Attributes.Remove(attrib);
                    break;
                }

                if (!Displays.ContainsKey(type))
                {
                    Debug.LogWarning("Attempt to draw debug for a type which doesn't have a ITypeDisplay.\nPerhaps you should add one?");
                    continue;
                }

                DrawDebugArgs args = new DrawDebugArgs(value, attrib.MonoInstance);

                Handles.color = attrib.SVDebugAttributes[attrib.Fields.IndexOf(field)].Color;

                // Run a display method from an instance of ITypeDisplay, using our type as an index in the dictionary.
                Displays[type].Draw(args);
            }
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
        Attributes.Clear();
        Displays.Clear();
        SweepForComponents();
        LoadDisplaysViaReflection();
    }

    /// <summary>
    /// Sweeps through the current scene for any MonoBehaviour objects, then checks each for any 
    /// fields with the [DrawDebug] attribute, then finally stores this information in our Attributes list.
    /// </summary>
    private static void SweepForComponents()
    {
        if (Attributes == null)
            Attributes = new List<AttributeInstance>();

        MonoBehaviour[] activeScene = Object.FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour mono in activeScene)
        {
            Type monoType = mono.GetType();

            // See if this component has any fields with our attribute
            FieldInfo[] fields = monoType.GetFields(BindingFlags.Instance | BindingFlags.Public);

            bool hasDebugAttrib = fields.Any(field => Attribute.GetCustomAttribute(field, typeof(SVDebug)) is SVDebug);
            if (!hasDebugAttrib)
                continue;

            // If we haven't exited by this point, we know there is atleast one field with our attribute.
            AttributeInstance attrib = GetOrCreateAttributeInstance(mono);

            foreach (var field in fields)
            {
                SVDebug svDebugAttrib = Attribute.GetCustomAttribute(field, typeof(SVDebug)) as SVDebug;

                if (svDebugAttrib == null)
                    continue;

                if (!attrib.Fields.Contains(field))
                    attrib.Fields.Add(field);
                if (!attrib.SVDebugAttributes.Contains(svDebugAttrib))
                    attrib.SVDebugAttributes.Add(svDebugAttrib);
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
        foreach(var attrib in Attributes)
            if (attrib.MonoInstance == mono)
                return attrib;
        var newAttrib = new AttributeInstance(mono);
        Attributes.Add(newAttrib);
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
        public List<FieldInfo> Fields;
        public List<SVDebug> SVDebugAttributes;

        public AttributeInstance(MonoBehaviour mono)
        {
            MonoInstance = mono;
            Fields = new List<FieldInfo>();
            SVDebugAttributes = new List<SVDebug>();
        }
    }

    /// <summary>
    /// Loops through all of the current assemblies, finding every class that inherits ITypeDisplay.
    /// Then uses Activator to create a new instance of the Type, and adds it's definition to Displays.
    /// </summary>
    private static void LoadDisplaysViaReflection()
    {
        Type[] displayTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(t => t.GetTypes())
            .Where(t => t.IsClass && typeof(ITypeDisplay).IsAssignableFrom(t)).ToArray();
        Displays = new Dictionary<Type, ITypeDisplay>();
        foreach (var dispType in displayTypes)
        {
            ITypeDisplay displayInstance = (ITypeDisplay) Activator.CreateInstance(dispType);
            Displays.Add(displayInstance.ExecutingType, displayInstance);
        }
    }
}
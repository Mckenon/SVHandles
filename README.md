# SceneViewHandles
SceneViewHandles is a lightweight extension for Unity which allows you to quickly visualize debug information such as raycasts and points in space by simply adding one attribute to the variable, and nothing else. Say you had a point in space in your code that you'd like to debug. Having to go through and add an ``OnDrawGizmos()`` method, then finally add your code is a long process in comparison to just adding ``[DrawDebug]`` right over your variable. SceneViewHandles allows you to easily preview types within Unity, and even allows for you to extend it with your own custom types.

## Example
```cs
public class TestInstance : MonoBehaviour
{
    [DrawDebug(0f, 1f, 0f)]
    public Vector3 MyPoint = new Vector3(0, 10, 0);

    [DrawDebug(1f, 0f, 0f)]
    public Vector3 TestDynamic = new Vector3(0, 10, 10);

    [DrawDebug]
    public Ray TestRay = new Ray(new Vector3(0, 0, 0), Vector3.forward);

    private float t = 0f;
    private void Update()
    {
        t += Time.deltaTime;
        TestDynamic = new Vector3(Mathf.Cos(t) * 1f, 0, Mathf.Sin(t) * 1f);
        TestRay.direction = (TestDynamic).normalized;
    }
}
```
This code example produces the following visual:  
![Image Example](https://i.imgur.com/nwTu0fe.png)  
and, animated: https://streamable.com/q1v66

## Extendability
So let's say you had some custom type you use often, that you would like to debug with this as well. That's easy, all you need to do is have a class in your project which implements ``ITypeDisplay``. For reference, here is how the code looks for drawing a ``Ray`` in ``DrawDebug``.
```cs
    public class RayDisplay : ITypeDisplay
    {
        public Type ExecutingType
        {
            get { return typeof(Ray); }
        }

        public void Draw(DrawDebugArgs args)
        {
            Ray? ray = args.Value as Ray?;

            if (ray == null)
                return;

            Handles.ArrowHandleCap(0, ray.Value.origin, Quaternion.LookRotation(ray.Value.direction), 1f, EventType.Repaint);
        }
    }
```

## Installation
To install SVHandles, simply go to the releases tab above, download the .dll, and place it within your project! If you would like, you can also clone the entire repository for personal development purposes, or if you happen to have a childhood fear of .dll's.

## Side-Note
If you do decide to extend SceneViewHandles, it would be greatly appreciated if you could copy your display code into a PullRequest to make things better for everyone, I'm only one person, so I can't think of everything! (Same goes for posting issues if you see something wrong, we're all here to improve.)

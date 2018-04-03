# Scene View Handles
SVHandles is a lightweight extension for Unity which allows you to quickly visualize and modify variables such as Vector3 and Bounds by simply adding ``[SVHandle]`` to the variable, and nothing else.

## SVHandle Example
The Following code produces this visual:
```cs
    [SVHandle]
    public Vector3 MyPoint = new Vector3(0, 10, 0);
```
![Handle Example](https://i.imgur.com/rhgZPXB.gif)

## Extendability
So let's say you had some custom type you use often, that you would like to debug with this as well. That's easy, all you need to do is have a class in your project which inherits ``SVHandleDisplay``. For reference, here is how the code looks for drawing a ``Ray`` in  an ``SVHandle``.
```cs
public class RayDisplay : SVHandleDisplay
{
	public override Type ExecutingType
	{
		get { return typeof(Ray); }
	}

	public override void Draw(SVDebugArgs args, ref object value)
	{
		Ray? ray = value as Ray?;

		Handles.ArrowHandleCap(0, ray.Value.origin, Quaternion.LookRotation(ray.Value.direction), 1f, EventType.Repaint);
	}
}
```
If you need to know more, soon there will be a wiki with all of the information you need!

## Installation
To install SVHandles, simply click this [link](https://github.com/DeathGameDev/SVHandles/raw/master/Library/ScriptAssemblies/SceneViewHandles.dll) to download the DLL, and put it in your unity project. You can also clone the entire repository if you would like to further customize the extension.

## Side-Note
If you do decide to extend SVHandles, it would be greatly appreciated if you could copy your display code into a PullRequest to make things better for everyone, I'm only one person, so I can't think of everything! (Same goes for posting issues if you see something wrong, we're all here to improve.)

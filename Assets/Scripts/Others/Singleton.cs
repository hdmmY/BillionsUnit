using System;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public bool Persistence
	{
		get;
		set;
	}

	public static bool Instantiated
	{
		get;
		private set;
	}

	public static T Instance
	{
		get
		{
			if (!Instantiated)
			{
				CreateInstance ();
			}
			return _instance;
		}
	}

	private static void CreateInstance ()
	{
		var type = typeof (T);
		var objects = FindObjectsOfType<T> ();

		if (objects.Length > 0)
		{
			if (objects.Length > 1)
			{
				for (int i = 1; i < objects.Length; i++)
					Destroy (objects[i].gameObject);
			}

			_instance = objects[0];
			_instance.gameObject.SetActive (true);
			Instantiated = true;

			return;
		}

		string prefabName;
		GameObject gameObject;

		var attribute = Attribute.GetCustomAttribute (type, typeof (PrefabAttribute)) as PrefabAttribute;
		if (attribute == null || string.IsNullOrEmpty (attribute.Name))
		{
			prefabName = type.ToString ();
			gameObject = new GameObject ();
		}
		else
		{
			prefabName = attribute.Name;
			gameObject = Instantiate (Resources.Load<GameObject> (prefabName));

			if (gameObject == null)
			{
				throw new Exception ("Could not find Prefab \"" + prefabName +
					"\" on Resources for Singleton of type \"" + type + "\".");
			}
		}

		gameObject.name = prefabName;

		if (_instance == null)
		{
			_instance = gameObject.GetComponent<T> () ?? gameObject.AddComponent<T> ();
		}
		Instantiated = true;
	}

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	protected virtual void Awake ()
	{
		if (_instance == null)
		{
			CreateInstance ();
			if (Persistence)
			{
				DontDestroyOnLoad (gameObject);
			}
			return;
		}

		if (Persistence) DontDestroyOnLoad (gameObject);

		if (GetInstanceID () != _instance.GetInstanceID ()) Destroy (gameObject);
	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	private void OnDestroy ()
	{
		Instantiated = false;
	}
}
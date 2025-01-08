using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainGeneration;

public class Loading : MonoBehaviour
{

	public bool logTaskLoadTimes;
	public bool logTotalLoadTime;

	[Header("References")]
	public TerrainHeightSettings heightSettings;
	public TerrainHeightProcessor heightProcessor;
	public WorldLookup worldLookup;
	public CoverMapLoader coverMapLoader;
	public LodMeshLoader terrainLoader;
	public MeshLoader earthLoader;
	public MeshLoader oceanLoader;
	public MeshLoader countryOutlineLoader;

	// Called before all other scripts (defined in script execution order settings)
	void Awake()
	{
		Load();
	}

	LoadTask[] GetTasks()
	{
		List<LoadTask> tasks = new List<LoadTask>();
		AddTask(() => heightProcessor.ProcessHeightMap(), "Processing Height Map");
		AddTask(() => worldLookup.Init(heightProcessor.processedHeightMap), "Initializing World Lookup");
		AddTask(() => coverMapLoader.Load(), "Loading Cover map");
		AddTask(() => terrainLoader.Load(), "Loading Terrain Mesh");
		AddTask(() => earthLoader.Load(), "Loading Earth Mesh");
		AddTask(() => oceanLoader.Load(), "Loading Ocean Mesh");
		AddTask(() => countryOutlineLoader.Load(), "Loading Country Outlines");

		void AddTask(System.Action task, string name)
		{
			tasks.Add(new LoadTask(task, name));
		}

		return tasks.ToArray();
	}



	void Load()
	{
		var loadTimer = System.Diagnostics.Stopwatch.StartNew();
		OnLoadStart();
		LoadTask[] tasks = GetTasks();

		foreach (LoadTask task in tasks)
		{
			long taskTime = task.Execute();
			if (logTaskLoadTimes)
			{
				Debug.Log($"{task.taskName}: {taskTime} ms.");
			}
		}

		OnLoadFinish();
		if (logTotalLoadTime)
		{
			Debug.Log($"Total load duration: {loadTimer.ElapsedMilliseconds} ms.");
		}
	}



	void OnLoadStart()
	{
	}

	void OnLoadFinish()
	{
		Resources.UnloadUnusedAssets();
	}

	private class LoadTask
	{
		public System.Action task;
		public string taskName;

		public LoadTask(System.Action task, string name)
		{
			this.task = task;
			this.taskName = name;
		}

		public long Execute(bool log = true)
		{
			if (log)
			{
				Debug.Log($"Task: {taskName} started");
			}
			var sw = System.Diagnostics.Stopwatch.StartNew();
			task.Invoke();

			if (log)
			{
				Debug.Log($"Task: {taskName} finished in {sw.ElapsedMilliseconds}ms");
			}
			return sw.ElapsedMilliseconds;
		}
	}

}

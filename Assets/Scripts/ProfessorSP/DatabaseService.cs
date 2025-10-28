using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeographicalAdventures.ProfessorSP
{
public class DatabaseService
{
	private const string MONGO_URI = "mongodb+srv://21678145:21678145@cluster0.zemxy3i.mongodb.net";
	private const string DATABASE_NAME = "ProfessorSP";

	private MongoClient client;
	private IMongoDatabase db;
	public DatabaseService()
	{
		string customUri = Environment.GetEnvironmentVariable("ProfessorSP_URI");
		if (customUri == null)
		{
			customUri = MONGO_URI;
		}
		client = new MongoClient(customUri);
		string customDb = Environment.GetEnvironmentVariable("ProfessorSP_DB");
		if (customDb == null)
		{
			customDb = DATABASE_NAME;
		}
		db = client.GetDatabase(customDb);
	}
	public void GetLatestData(MonoBehaviour mono, Action<List<ForecastData>> callback)
	{
		mono.StartCoroutine(LoadForecastData(1, callback));
	}
	public void GetForecastData(MonoBehaviour mono, int count, Action<List<ForecastData>> callback)
	{
		mono.StartCoroutine(LoadForecastData(count, callback));
	}

	IEnumerator LoadForecastData(int count, Action<List<ForecastData>> callback)
	{
		var colForecast = db.GetCollection<ForecastData>("Forecast");
		var sortBuilder = Builders<ForecastData>.Sort;
		var sort = sortBuilder.Descending(f => f.updatedAt);
		var filterBuilder = Builders<ForecastData>.Filter;
		var filter = filterBuilder.Ne(f => f.settings.impactPrompt, null);
		var data = colForecast.Find(filter).Sort(sort).Limit(count).ToList();
		yield return null;
		callback(data);
	}

	public void GetImpactContent(MonoBehaviour mono,string id, Action<BsonDocument> callback)
	{
		mono.StartCoroutine(LoadImpacts(id, callback));
	}

	IEnumerator LoadImpacts(string id, Action<BsonDocument> callback)
	{
		var colImpact = db.GetCollection<ImpactData>("impacts");
		var filter = Builders<ImpactData>.Filter.Eq(f => f._id, id);
		var impact = colImpact.Find(filter).FirstOrDefault();
		yield return null;
		callback(impact.content);
	}
}
}
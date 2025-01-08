using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GeographicalAdventures.ProfessorSP
{
	public class ForecastData
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		[BsonElement("createdAt")]
		public DateTime createdAt { get; set; }
		[BsonElement("updatedAt")]
		public DateTime updatedAt { get; set; }
		[BsonElement("emails")]
		public List<BsonDocument> emails { get; set; }

		[BsonElement("sources")]
		public List<BsonDocument> sources { get; set; }
		[BsonElement("settings")]
		public Settings settings { get; set; }
		[BsonElement("public")]
		public bool publicValue { get; set; }
		[BsonElement("extraInfo")]
		public BsonDocument extraInfo { get; set; }
		[BsonElement("messages")]
		public List<Message> messages { get; set; }
	}

	public class ImpactData
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		[BsonElement("createdAt")]
		public DateTime createdAt { get; set; }
		[BsonElement("updatedAt")]
		public DateTime updatedAt { get; set; }
		[BsonElement("content")]
		public BsonDocument content { get; set; }
	}

	public class Message
	{
		[BsonElement("content")]
		public string content { get; set; }
		[BsonElement("role")]
		public string role { get; set; }
	}

	public class Settings
	{
		[BsonElement("model")]
		public string model { get; set; }
		[BsonElement("plannerPrompt")]
		public string plannerPrompt { get; set; }
		[BsonElement("publisherPrompt")]
		public string publisherPrompt { get; set; }
		[BsonElement("impactPrompt")]
		public string impactPrompt { get; set; }
		[BsonElement("breadth")]
		public int breadth { get; set; }
	}

}
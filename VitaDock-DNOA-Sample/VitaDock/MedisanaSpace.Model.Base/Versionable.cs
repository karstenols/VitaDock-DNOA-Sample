namespace MedisanaSpace.Model.Base
{
	using Newtonsoft.Json;
	using System;

	/**
		*/

	public abstract class Versionable
	{
		public Versionable()
		{
			this.Id = Guid.NewGuid().ToString();
			this.Active = true;
			DateTime date = DateTime.Now;
			this.UpdatedDate = date;
		}

		public string Id { get; set; }

		public bool Active { get; set; }

		[JsonIgnore]
		public DateTime UpdatedDate { get; set; }

		[JsonProperty(PropertyName = "updatedDate",
			ItemConverterType = typeof(EpochMillisecondsConverter))]
		[JsonConverter(typeof(EpochMillisecondsConverter))]
		public DateTime MeasurementDate { get; set; }

		public int Version { get; set; }
	}
}
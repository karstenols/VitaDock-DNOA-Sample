namespace MedisanaSpace.Model
{
	using MedisanaSpace.Model.Base;

	public class Thermodock : BaseModelWithoutMeal
	{
		public const int MinBodyTemperature = 38;

		public const int MaxBodyTemerature = 39;

		public const int MinBodyTemperatureTargetMin = 34;

		public const int MaxBodyTemperatureTargetMin = 42;

		public const int MinBodyTemperatureTargetMax = 34;

		public const int MaxBodyTemperatureTargetMax = 42;

		public float BodyTemperature { get; set; }

		public float BodyTemperatureTargetMin { get; set; }

		public float BodyTemperatureTargetMax { get; set; }

		public string ModuleSerialId { get; set; }
	}
}
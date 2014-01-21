namespace MedisanaSpace.Model.Base
{
	/// <summary>
	/// Holds basic info about a VitaDock reading
	/// </summary>
	public abstract class BaseModelWithoutMeal : Versionable
	{
		public const int MinActivityStatus = 0;
		public const int MaxActivityStatus = 3;
		public const int MinMood = 0;
		public const int MaxMood = 2;
		public const int MaxNoteLength = 512;

		public int ActivityStatus { get; set; }

		public int Mood { get; set; }

		public string Note { get; set; }
	}
}
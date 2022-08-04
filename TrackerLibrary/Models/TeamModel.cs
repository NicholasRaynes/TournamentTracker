using System.Collections.Generic;

namespace TrackerLibrary.Models
{
	/// <summary>
	/// Represents a team within a tournament.
        /// </summary>
	public class TeamModel
	{
		/// <summary>
		/// The unique identifier for each team.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// A list of members for each team.
		/// </summary>
		public List<PersonModel> TeamMembers { get; set; } = new List<PersonModel>();
		
		/// <summary>
		/// The team name for each team.
		/// </summary>
		public string TeamName { get; set; }
	}
}

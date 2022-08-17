using TrackerLibrary.Models;

namespace TrackerUI
{
	/// <summary>
    	/// This interface represents a team requester for the application.
    	/// </summary>
	public interface ITeamRequester
	{
		void TeamComplete(TeamModel model);
	}
}

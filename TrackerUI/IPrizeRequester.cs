using TrackerLibrary.Models;

namespace TrackerUI
{
	/// <summary>
    	/// This interface represents a prize requester for the application.
    	/// </summary>
	public interface IPrizeRequester
	{
		void PrizeComplete(PrizeModel model);
	}
}

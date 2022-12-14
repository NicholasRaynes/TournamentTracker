using System.Collections.Generic;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
	/// <summary>
    	/// This class represents a create tournament form for the application.
    	/// </summary>
	public partial class CreateTournamentForm : Form, IPrizeRequester, ITeamRequester
	{
		List<TeamModel> availableTeams = GlobalConfig.Connection.GetTeam_All();
		List<TeamModel> selectedTeams = new List<TeamModel>();
		List<PrizeModel> selectedPrizes = new List<PrizeModel>();

		/// <summary>
        	/// Initializes an instance of the CreateTournamentForm class.
		/// </summary>
		public CreateTournamentForm()
		{
			InitializeComponent();

			WireUpLists();
		}

		/// <summary>
        	/// Reponsible for data binding the controls to their corresponding lists.
        	/// </summary>
		private void WireUpLists()
		{
			selectTeamDropDown.DataSource = null;
			selectTeamDropDown.DataSource = availableTeams;
			selectTeamDropDown.DisplayMember = "TeamName";

			tournamentTeamsListBox.DataSource = null;
			tournamentTeamsListBox.DataSource = selectedTeams;
			tournamentTeamsListBox.DisplayMember = "TeamName";

			prizesListBox.DataSource = null;
			prizesListBox.DataSource = selectedPrizes;
			prizesListBox.DisplayMember = "PlaceName";
		}

		/// <summary>
        	/// Handles the click event for the add team button.
        	/// </summary>
		private void AddTeamButton_Click(object sender, System.EventArgs e)
		{
			TeamModel t = (TeamModel)selectTeamDropDown.SelectedItem;

			if (t != null)
			{
				availableTeams.Remove(t);
				selectedTeams.Add(t);

				WireUpLists();
			}
		}

		/// <summary>
        	/// Handles the click event for the create prize button.
        	/// </summary>
		private void CreatePrizeButton_Click(object sender, System.EventArgs e)
		{
			// Call the CreatePrizeForm
			CreatePrizeForm frm = new CreatePrizeForm(this);
			
			frm.Show();
		}

		/// <summary>
        	/// Responsible for adding a prize to the selected prizes list.
		/// </summary>
		/// <param name="model">A prize.</param>
		public void PrizeComplete(PrizeModel model)
		{
			// Get back from the form a PrizeModel
			// Take the PrizeModel and put it into our list of selected prizes
			selectedPrizes.Add(model);
			WireUpLists();
		}

		
		/// <summary>
        	/// Responsible for adding a team to the selected teams list.
		/// </summary>
		/// <param name="model">A team.</param>
		public void TeamComplete(TeamModel model)
		{
			selectedTeams.Add(model);
			WireUpLists();
		}

		/// <summary>
        	/// Handles the link clicked event for the create new team link.
        	/// </summary>
		private void CreateNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			CreateTeamForm frm = new CreateTeamForm(this);
			frm.Show();
		}

		/// <summary>
        	/// Handles the click event for the remove selected players button.
        	/// </summary>
		private void RemoveSelectedPlayersButton_Click(object sender, System.EventArgs e)
		{
			TeamModel t = (TeamModel)tournamentTeamsListBox.SelectedItem;

			if (t != null)
			{
				selectedTeams.Remove(t);
				availableTeams.Add(t);

				WireUpLists();
			}
		}

		/// <summary>
        	/// Handles the click event for the remove selected prizes button.
        	/// </summary>
		private void RemoveSelectedPrizesButton_Click(object sender, System.EventArgs e)
		{
			PrizeModel p = (PrizeModel)prizesListBox.SelectedItem;

			if (p != null)
			{
				selectedPrizes.Remove(p);

				WireUpLists();
			}
		}

		/// <summary>
        	/// Handles the click event for the create tournament button.
        	/// </summary>
		private void CreateTournamentButton_Click(object sender, System.EventArgs e)
		{
			// Validate data
			decimal fee = 0;

			bool feeAcceptable = decimal.TryParse(entryFeeValue.Text, out fee);

			if (!feeAcceptable)
			{
				MessageBox.Show("You need to enter a valid Entry Fee.", 
					"Invalid Fee", 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Error);
				return;
			}

			// Create our Tournament
			TournamentModel tm = new TournamentModel
			{
				TournamnetName = tournamentNameValue.Text,
				EntryFee = fee,
				Prizes = selectedPrizes,
				EnteredTeams = selectedTeams
			};

			// Wire our matchups
			TournamentLogic.CreateRounds(tm);

			// Create Tournament entry
			// Create all of the Prize entries
			// Create all of the Team entries
			GlobalConfig.Connection.CreateTournament(tm);

			tm.AlertUsersToNewRound();

			TournamentViewerForm frm = new TournamentViewerForm(tm);
			frm.Show();
			this.Close();
		}
	}
}

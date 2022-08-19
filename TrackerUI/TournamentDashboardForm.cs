using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
	/// <summary>
    	/// This class represents a tournament dashboard form for the application.
    	/// </summary>
	public partial class TournamentDashboardForm : Form
	{
		List<TournamentModel> tournaments = GlobalConfig.Connection.GetTournament_All();

		/// <summary>
        	/// Initializes a new instance of the TournamentDashboardForm class.
		/// </summary>
		public TournamentDashboardForm()
		{
			InitializeComponent();

			WireUpLists();
		}

		/// <summary>
        	/// Reponsible for data binding the controls to their corresponding lists.
        	/// </summary>
		private void WireUpLists()
		{
			loadExistingTournamentDropDown.DataSource = tournaments;
			loadExistingTournamentDropDown.DisplayMember = "TournamentName";
		}

		/// <summary>
        	/// Handles the click event for the create tournament button.
        	/// </summary>
		private void CreateTournamentButton_Click(object sender, EventArgs e)
		{
			CreateTournamentForm frm = new CreateTournamentForm();
			frm.Show();
		}

		/// <summary>
        	/// Handles the click event for the load tournament button.
        	/// </summary>
		private void LoadTournamentButton_Click(object sender, EventArgs e)
		{
			TournamentModel tm = (TournamentModel)loadExistingTournamentDropDown.SelectedItem;
			TournamentViewerForm frm = new TournamentViewerForm(tm);
			frm.Show();
		}
	}
}

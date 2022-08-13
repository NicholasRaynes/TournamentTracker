using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
	/// <summary>
    	/// This class represents a create team form for the application.
    	/// </summary>
	public partial class CreateTeamForm : Form
	{
		private List<PersonModel> availableTeamMembers = GlobalConfig.Connection.GetPerson_All();
		private List<PersonModel> selectedTeamMembers = new List<PersonModel>();
		private ITeamRequester callingForm;

		/// <summary>
        	/// Initializes an instance of the CreateTeamForm class, with an ITeamRequester parameter.
		/// </summary>
		/// <param name="caller">The team caller.</param>
		public CreateTeamForm(ITeamRequester caller)
		{
			InitializeComponent();

			callingForm = caller;

			//CreateSampleData();

			WireUpLists();
		}

		/// <summary>
        	/// Responsible for creating sample data to test the application during development.
        	/// </summary>
		private void CreateSampleData()
		{
			availableTeamMembers.Add(new PersonModel { FirstName = "Tim", LastName = "Corey" });
			availableTeamMembers.Add(new PersonModel { FirstName = "Sue", LastName = "Storm" });

			selectedTeamMembers.Add(new PersonModel { FirstName = "Jane", LastName = "Smith" });
			selectedTeamMembers.Add(new PersonModel { FirstName = "Bill", LastName = "Jones" });
		}

		/// <summary>
        	/// Reponsible for data binding the controls to their corresponding lists.
        	/// </summary>
		private void WireUpLists()
		{
			selectTeamMemberDropDown.DataSource = null;

			selectTeamMemberDropDown.DataSource = availableTeamMembers;
			selectTeamMemberDropDown.DisplayMember = "FullName";

			teamMembersListBox.DataSource = null;

			teamMembersListBox.DataSource = selectedTeamMembers;
			teamMembersListBox.DisplayMember = "FullName";
		}

		
		/// <summary>
        	/// Handles the click event for the create member button.
        	/// </summary>
		private void CreateMemberButton_Click(object sender, EventArgs e)
		{
			if (ValidateForm())
			{
				PersonModel p = new PersonModel();

				p.FirstName = firstNameValue.Text;
				p.LastName = lastNameValue.Text;
				p.EmailAddress = emailValue.Text;
				p.CellPhoneNumber = cellphoneValue.Text;

				GlobalConfig.Connection.CreatePerson(p);

				selectedTeamMembers.Add(p);

				WireUpLists();

				firstNameValue.Text = string.Empty;
				lastNameValue.Text = string.Empty;
				emailValue.Text = string.Empty;
				cellphoneValue.Text = string.Empty;
			}
			else
			{ 
				MessageBox.Show("You need to fill in all of the fields.");
			}
		}

		/// <summary>
		/// Responsible for validating the form.
		/// </summary>
		/// <returns>Whether the form is valid or not.</returns>
		private bool ValidateForm()
		{
			if (firstNameValue.Text.Length == 0)
			{
				return false;
			}

			if (lastNameValue.Text.Length == 0)
			{
				return false;
			}

			if (emailValue.Text.Length == 0)
			{
				return false;
			}

			if (cellphoneValue.Text.Length == 0)
			{
				return false;
			}

			return true;
		}

		/// <summary>
        	/// Handles the click event for the add member button.
        	/// </summary>
		private void AddMemberButton_Click(object sender, EventArgs e)
		{
			PersonModel p = (PersonModel)selectTeamMemberDropDown.SelectedItem;

			if (p != null)
			{
				availableTeamMembers.Remove(p);
				selectedTeamMembers.Add(p);

				WireUpLists();
			}
		}

		/// <summary>
        	/// Handles the click event for the remove selected member button.
        	/// </summary>
		private void RemoveSelectedMemberButton_Click(object sender, EventArgs e)
		{
			PersonModel p = (PersonModel)teamMembersListBox.SelectedItem;

			if (p != null)
			{
				selectedTeamMembers.Remove(p);
				availableTeamMembers.Add(p);

				WireUpLists(); 
			}
		}

		/// <summary>
        	/// Handles the click event for the create team button.
        	/// </summary>
		private void CreateTeamButton_Click(object sender, EventArgs e)
		{
			TeamModel t = new TeamModel();

			t.TeamName = teamNameValue.Text;
			t.TeamMembers = selectedTeamMembers;

			GlobalConfig.Connection.CreateTeam(t);

			callingForm.TeamComplete(t);

			this.Close();
		}
	}
}

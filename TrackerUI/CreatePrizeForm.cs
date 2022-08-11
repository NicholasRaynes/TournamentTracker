using System;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
	/// <summary>
    	/// This class represents a create prize form for the application.
    	/// </summary>
	public partial class CreatePrizeForm : Form
	{
		IPrizeRequester callingForm;
		
		/// <summary>
        	/// Initializes an instance of the CreatePrizeForm class, with an IPrizeRequester parameter.
		/// </summary>
		public CreatePrizeForm(IPrizeRequester caller)
		{
			InitializeComponent();

			callingForm = caller;
		}

		/// <summary>
        	/// Handles the click event for create prize button.
        	/// </summary>
		private void CreatePrizeButton_Click(object sender, EventArgs e)
		{
			if (ValidateForm())
			{
				PrizeModel model = new PrizeModel(
					placeNameValue.Text,
					placeNumberValue.Text,
					prizeAmountValue.Text,
					prizePercentageValue.Text);

				GlobalConfig.Connection.CreatePrize(model);

				callingForm.PrizeComplete(model);

				this.Close();

				//placeNameValue.Text = String.Empty;
				//placeNumberValue.Text = String.Empty;
				//prizeAmountValue.Text = "0";
				//prizePercentageValue.Text = "0";
			}
			else
			{
				MessageBox.Show("This form has invalid information. Please check it and try again.");
			}
		}

		/// <summary>
        	/// Responsible for validating the form.
        	/// </summary>
		private bool ValidateForm()
		{
			bool output = true;
			bool placeNumberValidNumber = int.TryParse(placeNumberValue.Text, out int placeNumber);

			if (!placeNumberValidNumber)
			{
				output = false;
			}

			if (placeNumber < 1)
			{
				output = false;
			}

			if (placeNameValue.Text.Length == 0)
			{
				output = false;
			}

			bool prizeAmountValid = decimal.TryParse(prizeAmountValue.Text, out decimal prizeAmount);
			bool prizePercentageValid = int.TryParse(prizePercentageValue.Text, out int prizePercentage);

			if (!prizeAmountValid || !prizePercentageValid)
			{
				output = false;
			}

			if (prizeAmount <= 0 && prizePercentage <= 0)
			{
				output = false;
			}

			if (prizePercentage < 0 || prizePercentage > 100)
			{
				output = false;
			}

			return output;
		}
	}
}

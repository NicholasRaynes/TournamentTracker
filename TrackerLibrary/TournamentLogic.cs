using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
	/// <summary>
    	/// This class represents the logic/functionality for a tournament.
    	/// </summary>
	public static class TournamentLogic
	{
		/// <summary>
		/// Responsible for creating the rounds for a tournament.
		/// </summary>
		/// <param name="model">A tournament model.</param>
		public static void CreateRounds(TournamentModel model)
		{
			List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.EnteredTeams);
			int rounds = FindNumberOfRounds(randomizedTeams.Count);
			int byes = NumberOfByes(rounds, randomizedTeams.Count);

			model.Rounds.Add(CreateFirstround(byes, randomizedTeams));

			CreateOtherRounds(model, rounds);

			UpdateTournamentResults(model);
		}

		/// <summary>
		/// Responsible updating the results for a tournament.
		/// </summary>
		/// <param name="model">A tournament model.</param>
		public static void UpdateTournamentResults(TournamentModel model)
		{
			int startingRound = model.CheckCurrentRound();
			List<MatchupModel> toScore = new List<MatchupModel>();

			foreach (List<MatchupModel> round in model.Rounds)
			{
				foreach (MatchupModel rm in round)
				{
					if (rm.Winner == null && (rm.Entries.Any(x => x.Score != 0) || rm.Entries.Count == 1))
					{
						toScore.Add(rm);
					}
				}
			}

			MarkWinnersInMatchups(toScore);

			AdvancedWinners(toScore, model);

			toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
			
			int endingRound = model.CheckCurrentRound();

			if (endingRound > startingRound)
			{
				model.AlertUsersToNewRound();
			}
		}

		/// <summary>
		/// Responsible for alerting users to a new round.
		/// </summary>
		/// <param name="model">A tournament model.</param>
		public static void AlertUsersToNewRound(this TournamentModel model)
		{
			int currentRoundNumber = model.CheckCurrentRound();
			List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First();

			foreach (MatchupModel matchup in currentRound)
			{
				foreach (MatchupEntryModel me in matchup.Entries)
				{
					foreach (PersonModel p in me.TeamCompeting.TeamMembers)
					{
						AlertPersonToNewRound(p, me.TeamCompeting.TeamName, matchup.Entries.Where(x => x.TeamCompeting != me.TeamCompeting).FirstOrDefault());
					}
				}
			}
		}

		/// <summary>
		/// Responsible for alerting a person to a new round.
		/// </summary>
		/// <param name="p">A person model.</param>
		/// <param name="teamName">The name of a team.</param>
		/// <param name="competitor">The new matchup competitor for the round.</param>
		private static void AlertPersonToNewRound(PersonModel p, string teamName, MatchupEntryModel competitor)
		{
			if (p.EmailAddress.Length == 0)
			{
				return;
			}

			string toAddress = "";
			string subject = "";
			StringBuilder body = new StringBuilder();

			if (competitor != null)
			{
				subject = $"You have a new matchup with { competitor.TeamCompeting.TeamName }";

				body.AppendLine("<h1>You have a new matchup</h1>");
				body.Append("<strong>Competitor: </strong>");
				body.AppendLine(competitor.TeamCompeting.TeamName);
				body.AppendLine();
				body.AppendLine();
				body.AppendLine("Have a great time!");
				body.AppendLine("~Tournament Tracker");
			}
			else
			{
				subject = "You have a bye week this round";

				body.AppendLine("Enjoy your round off!");
				body.AppendLine("~Tournament Tracker");
			}

			toAddress = p.EmailAddress;

			EmailLogic.SendEmail(toAddress, subject, body.ToString());
		}

		/// <summary>
		/// Responsible for checking the current round.
		/// </summary>
		/// <param name="model">A tournament model.</param>
		/// <returns>The number corresponding to the current round.</returns>
		private static int CheckCurrentRound(this TournamentModel model)
		{
			int output = 1;

			foreach (List<MatchupModel> rounds in model.Rounds)
			{
				if (rounds.All(x => x.Winner != null))
				{
					output += 1;
				}
				else
				{
					return output;
				}
			}

			CompleteTournament(model);

			return output - 1;
		}

		/// <summary>
		/// Responsible for completing a tournament.
		/// </summary>
		/// <param name="model">A tournament model.</param>
		private static void CompleteTournament(TournamentModel model)
		{
			GlobalConfig.Connection.CompleteTournament(model);
			TeamModel winners = model.Rounds.Last().First().Winner;
			TeamModel runnerUp = model.Rounds.Last().First().Entries.Where(x => x.TeamCompeting != winners).First().TeamCompeting;

			decimal winnerPrize = 0;
			decimal runnerUpPrize = 0;

			if (model.Prizes.Count > 0)
			{
				decimal totalIncome = model.EnteredTeams.Count * model.EntryFee;

				PrizeModel firstPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
				PrizeModel secondPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();

				if (firstPlacePrize != null)
				{
					winnerPrize = firstPlacePrize.CalculatePrizePayout(totalIncome);
				}

				if (secondPlacePrize != null)
				{
					runnerUpPrize = secondPlacePrize.CalculatePrizePayout(totalIncome);
				}
			}

			// Send Email to all tournament
			string subject = "";
			StringBuilder body = new StringBuilder();

			subject = $"In { model.TournamnetName }, { winners.TeamName } has won!";

			body.AppendLine("<h1>We have a WINNER!</h1>");
			body.AppendLine("<p>Congratulations to our winner on a great tournament.</p>");
			body.AppendLine("</br>");

			if (winnerPrize > 0)
			{
				body.AppendLine($"<p>{ winners.TeamName } will receive ${ winnerPrize }");
			}

			if (runnerUpPrize > 0)
			{
				body.AppendLine($"<p>{ runnerUp.TeamName } will receive ${ runnerUpPrize }");
			}

			body.AppendLine("<p>Thanks for a great tournament everyone!</p>");
			body.AppendLine("~Tournament Tracker");

			List<string> bccAddresses = new List<string>();
			foreach (TeamModel t in model.EnteredTeams)
			{
				foreach (PersonModel p in t.TeamMembers)
				{
					if (p.EmailAddress.Length > 0)
					{
						bccAddresses.Add(p.EmailAddress);
					}
				}
			}

			EmailLogic.SendEmail(new List<string>(), bccAddresses, subject, body.ToString());

			// Complete Tournament
			model.CompleteTournament();
		}

		/// <summary>
		/// Responsible for calculating the prize payout.
		/// </summary>
		/// <param name="prize">The prize.</param>
		/// <param name="totalIncome">The total income.</param>
		/// <returns>The amount of a prize payout.</returns>
		private static decimal CalculatePrizePayout(this PrizeModel prize, decimal totalIncome)
		{
			decimal output = 0;

			if (prize.PrizeAmount > 0)
			{
				output = prize.PrizeAmount;
			}
			else
			{
				output = Decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizeAmount / 100));
			}

			return output;
		}

		/// <summary>
		/// Responsible for advancing winners.
		/// </summary>
		/// <param name="models">A list of matchups.</param>
		/// <param name="tournament">A tournament model.</param>
		private static void AdvancedWinners(List<MatchupModel> models, TournamentModel tournament)
		{
			foreach (MatchupModel m in models)
			{
				foreach (List<MatchupModel> round in tournament.Rounds)
				{
					foreach (MatchupModel rm in round)
					{
						foreach (MatchupEntryModel me in rm.Entries)
						{
							if (me.ParentMatchup != null)
							{
								if (me.ParentMatchup.Id == m.Id)
								{
									me.TeamCompeting = m.Winner;
									GlobalConfig.Connection.UpdateMatchup(rm);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Responsible for marking the winners of each matchup.
		/// </summary>
		/// <param name="models">A list of matchups.</param>
		private static void MarkWinnersInMatchups(List<MatchupModel> models)
		{
			// greater or lesser
			string greaterWins =  ConfigurationManager.AppSettings["greaterWins"];

			foreach (MatchupModel m in models)
			{
				// Checks for bye week entry
				if (m.Entries.Count == 1)
				{
					m.Winner = m.Entries[0].TeamCompeting;
					continue;
				}

				// 0 means false, or low score wins
				if (greaterWins == "0")
				{
					if (m.Entries[0].Score < m.Entries[1].Score)
					{
						m.Winner = m.Entries[0].TeamCompeting;
					}
					else if (m.Entries[1].Score < m.Entries[0].Score)
					{
						m.Winner = m.Entries[1].TeamCompeting;
					}
					else
					{
						throw new Exception("We do not allow ties in this application.");
					}
				}
				else
				{
					// 1 means true, or high score wins
					if (m.Entries[0].Score > m.Entries[1].Score)
					{
						m.Winner = m.Entries[0].TeamCompeting;
					}
					else if (m.Entries[1].Score > m.Entries[0].Score)
					{
						m.Winner = m.Entries[1].TeamCompeting;
					}
					else
					{
						throw new Exception("We do not allow ties in this application.");
					}
				}
			}
		}

		/// <summary>
		/// Responsible for creating other rounds.
		/// </summary>
		/// <param name="model">A tournament model.</param>
		/// <param name="rounds">The amount of rounds.</param>
		private static void CreateOtherRounds(TournamentModel model, int rounds)
		{
			int round = 2;
			List<MatchupModel> previousRound = model.Rounds[0];
			List<MatchupModel> currRound = new List<MatchupModel>();
			MatchupModel currMatchup = new MatchupModel();

			while (round <= rounds)
			{
				foreach (MatchupModel match in previousRound)
				{
					currMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });

					if (currMatchup.Entries.Count > 1)
					{
						currMatchup.MatchupRound = round;
						currRound.Add(currMatchup);
						currMatchup = new MatchupModel();
					}
				}

				model.Rounds.Add(currRound);
				previousRound = currRound;

				currRound = new List<MatchupModel>();
				round++;
			}
		}

		/// <summary>
		/// Responsible for creating the first round.
		/// </summary>
		/// <param name="byes">The amount of teams with a bye.</param>
		/// <param name="teams">The list of teams.</param>
		/// <returns>The list of matchups for the first round.</returns>
		private static List<MatchupModel> CreateFirstround(int byes, List<TeamModel> teams)
		{
			List<MatchupModel> output = new List<MatchupModel>();
			MatchupModel curr = new MatchupModel();

			foreach (TeamModel team in teams)
			{
				curr.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

				if (byes > 0 || curr.Entries.Count > 1)
				{
					curr.MatchupRound = 1;
					output.Add(curr);
					curr = new MatchupModel();

					if (byes > 0)
					{
						byes--;
					}
				}
			}

			return output;
		}
		
		/// <summary>
		/// Responsible for obtaining the number of byes.
		/// </summary>
		/// <param name="rounds">The amount of rounds.</param>
		/// <param name="numberOfTeams">The number of teams.</param>
		/// <returns>The number of byes for a tournament.</returns>
		private static int NumberOfByes(int rounds, int numberOfTeams)
		{
			int output = 0;
			int totalTeams = 1;

			for (int i = 1; i < rounds; i++)
			{
				totalTeams *= 2;
			}

			output = totalTeams - numberOfTeams;

			return output;
		}

		/// <summary>
		/// Responsible for obtaining the number of rounds.
		/// </summary>
		/// <param name="teamCount">The amount of teams.</param>
		/// <returns>The number of rounds.</returns>
		private static int FindNumberOfRounds(int teamCount)
		{
			int output = 1;
			int val = 2;

			while (val < teamCount)
			{
				output++;
				val *= 2;
			}

			return output;
		}

		/// <summary>
		/// Responsible for randomizing the team order.
		/// </summary>
		/// <param name="teams">A list of teams for a tournament.</param>
		/// <returns>A randomized list of teams for a tournament.</returns>
		private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
		{
			return teams.OrderBy(x => Guid.NewGuid()).ToList();
		}
	}
}

using System.Configuration;
using TrackerLibrary.DataAccess;

namespace TrackerLibrary
{
	/// <summary>
	/// Responsible for establishing a connection between the data file(s), and application.
	/// </summary>
	public static class GlobalConfig
	{
		public const string PrizesFile = "PrizeModel.csv";
		public const string PeopleFile = "PersonModel.csv";
		public const string TeamFile = "TeamModel.csv";
		public const string TournamentFile = "TournamentModel.csv";
		public const string MatchupFile = "MatchupModel.csv";
		public const string MatchupEntryFile = "MatchupEntryModel.csv";

		/// <summary>
		/// The data connection.
		/// </summary>
		public static IDataConnection Connection { get; private set; }

		/// <summary>
		/// Responsible for intizalizing the connection.
		/// </summary>
		/// <param name="db">The database type.</param>
		public static void InitializeConnection(DatabaseType db)
		{
			switch (db)
			{
				case DatabaseType.Sql:				
					SqlConnector sql = new SqlConnector();
					Connection = sql;
					break;
				case DatabaseType.TextFile:
					TextConnector text = new TextConnector();
					Connection = text;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Responsible for returning the connection string.
		/// </summary>
		/// <param name="name">The connection string name.</param>
		public static string CnnString(string name)
		{
			return ConfigurationManager.ConnectionStrings[name].ConnectionString;
		}

		/// <summary>
		/// Responsible for looking up the app key.
		/// </summary>
		/// <param name="key">The app key.</param>
		public static string AppKeyLookup(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}
	}
}

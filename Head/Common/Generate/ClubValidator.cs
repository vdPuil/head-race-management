using System;
using System.Collections.Generic;
using Head.Common.Interfaces.Utils;
using Head.Common.Csv;
using Common.Logging;
using Head.Common.Domain;
using Head.Common.BritishRowing;
using Head.Common.Internal.Overrides;
using System.Linq;
using Head.Common.Internal.Categories;
using Head.Common.Interfaces.Enums;
using System.Text;

namespace Head.Common.Generate
{
	public class ClubValidator : IValidation<IEnumerable<IClub>>
	{
		#region IValidation implementation

		public bool Validate (IEnumerable<IClub> clubs)
		{
			ILog logger = LogManager.GetCurrentClassLogger ();
			bool valid = true;

			foreach (var club in clubs) 
			{
				if (club.Index.StartsWith ("Z") && String.IsNullOrEmpty (club.Country)) 
				{
					logger.WarnFormat ("Need a country for club: {0} - {1}", club.Index, club.Name);
					valid = false;
				}
			}

			return valid;
		}

		#endregion
	}

	public class AthleteValidator : IValidation<IEnumerable<IAthlete>>
	{
		#region IValidation implementation
		public bool Validate (IEnumerable<IAthlete> athletes)
		{
			var originalathletes = new AthleteCreator ().SetRawPath ("Resources/competitorexport-close.csv").Create ();
		
			ILog logger = LogManager.GetCurrentClassLogger ();

			// chris - magic number 
			DateTime raceday = new DateTime (2014, 3, 30);
			var sb = new StringBuilder ();
			sb.AppendLine("Age report:");
			foreach (var athlete in athletes.Where(a => a.DateOfBirth >= raceday.AddYears(-16) || a.DateOfBirth <= raceday.AddYears(-75)).OrderBy(a => a.DateOfBirth))
				sb.AppendFormat ("{0}, {1}, #{2}, {6}, {7} => {4} years ({3}){5}", 
					athlete.Name, athlete.Crew.Name, athlete.Crew.StartNumber, 
					athlete.DateOfBirth.ToShortDateString (), 
					Math.Floor(raceday.Subtract (athlete.DateOfBirth).TotalDays / 365), Environment.NewLine, athlete.Crew.BoatingLocation.Name, athlete.Crew.SubmittingEmail);
			logger.Info (sb.ToString ());

			// TODO - highlight when it's a cox that's changed, but that shouldn't count for the subs calculation 
			logger.Info ("Change report:");
			IList<Tuple<IAthlete, IAthlete>> changes = new List<Tuple<IAthlete, IAthlete>> ();
			foreach (var athlete in athletes) {
				var originally = originalathletes.FirstOrDefault (a => a.Licence == athlete.Licence);
				if (originally == null) {
					changes.Add (new Tuple<IAthlete, IAthlete> (athlete, null));
					logger.InfoFormat ("{2}: {1}: {0} is new", athlete.Name, athlete.Crew.Name, athlete.Crew.StartNumber);
					continue;
				}
				if (athlete.CrewId != originally.CrewId) {
					changes.Add (new Tuple<IAthlete, IAthlete> (athlete, originally));
					logger.InfoFormat ("{2}: {1}: {0} has moved crew (from {3})", athlete.Name, athlete.Crew.Name, athlete.Crew.StartNumber, originally.CrewId);
				}
			}

			foreach (var grouping in changes.GroupBy(ch => ch.Item1.Crew.StartNumber).OrderBy(gr => gr.Key)) {
				logger.InfoFormat ("Crew {0} has made {1} changes: ", grouping.Key, grouping.Count ());
			}

			return true;
		}
		#endregion
	}
}
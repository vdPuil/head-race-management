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
using System.Configuration;

namespace Head.Common.Generate.Validators
{

	public class AthleteValidator : IValidation<IEnumerable<IAthlete>>
	{
		#region IValidation implementation
		public bool Validate (IEnumerable<IAthlete> athletes)
		{
			var originalathletes = new AthleteCreator ().SetRawPath ("Resources/competitorexport-close.csv").Create ();
		
			ILog logger = LogManager.GetCurrentClassLogger ();

			DateTime raceday = DateTime.MinValue;
			if(!DateTime.TryParse(ConfigurationManager.AppSettings["racedate"].ToString(), out raceday))
				raceday = DateTime.MinValue;

			var sb = new StringBuilder ();
			sb.AppendLine("Age report:");
			foreach (var athlete in athletes.Where(a => a.DateOfBirth >= raceday.AddYears(-16) || a.DateOfBirth <= raceday.AddYears(-75)).OrderBy(a => a.DateOfBirth))
				sb.AppendFormat ("{0}, {1}, #{2}, {6}, {7} => {4} years ({3}){5}", 
					athlete.Name, athlete.Crew.Name, athlete.Crew.StartNumber, 
					athlete.DateOfBirth.ToShortDateString (), 
					Math.Floor(raceday.Subtract (athlete.DateOfBirth).TotalDays / 365), Environment.NewLine, athlete.Crew.BoatingLocation.Name, athlete.Crew.SubmittingEmail);
			logger.Info (sb.ToString ());

			logger.Info ("Change report:");
			IList<Tuple<IAthlete, IAthlete>> changes = new List<Tuple<IAthlete, IAthlete>> ();
			foreach (var athlete in athletes.OrderBy(a => a.Crew.StartNumber)) {
				// the substring here ensures that we're ignoring the expiry date, so we're not counting renewals. 
				var originally = originalathletes.FirstOrDefault (a => a.Licence.Substring(6) == athlete.Licence.Substring(6));
				if (originally == null) {
					if(!athlete.IsCox)
						changes.Add (new Tuple<IAthlete, IAthlete> (athlete, null));
					logger.InfoFormat ("{2}: {1}: {0} is new [cox? {3}]", athlete.Name, athlete.Crew.Name, athlete.Crew.StartNumber, athlete.IsCox);
					continue;
				}
				if (athlete.CrewId != originally.CrewId) {
					if(!athlete.IsCox)
						changes.Add (new Tuple<IAthlete, IAthlete> (athlete, originally));
					logger.InfoFormat ("{2}: {1}: {0} has moved crew (from {3}) [cox? {4}]", athlete.Name, athlete.Crew.Name, athlete.Crew.StartNumber, originally.CrewId, athlete.IsCox);
				}
			}

			foreach (var grouping in changes.GroupBy(ch => ch.Item1.Crew.StartNumber).OrderBy(gr => gr.Key)) {
				var crew = grouping.First().Item1.Crew;
				int ch = grouping.Count ();
				string msg = ch >= 4 ? string.Format ("{0} / {1}", crew.Name, crew.SubmittingEmail) : string.Empty;
				logger.InfoFormat ("Crew {0} has made {1} changes. {2}", grouping.Key, grouping.Count (), msg);
			}

			return true;
		}
		#endregion
	}
}
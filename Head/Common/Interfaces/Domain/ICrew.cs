using System;
using System.Collections.Generic;
using Head.Common.Interfaces.Enums;
using Head.Common.Internal.Categories;
using Logic.Domain;

namespace Head.Common.Domain
{
	public interface ICrew
	{
		bool IsTimeOnly { get;} 
		Gender Gender { get; } 
		EventCategory EventCategory { get; } 
		bool IsForeign { get; } 
		bool IsMasters { get; } 
		void IncludeInCategory(ICategory category);
		IEnumerable<ICategory> Categories { get;} 
		string Name { get; }
		IClub BoatingLocation { get; } 
		int CrewId { get; } 
		int StartNumber { get; }
		int? PreviousYear { get; }
		string BoatingLocationContact { get; }
		bool IsScratched { get; } 
		bool IsPaid { get; } 
		bool IsNovice { get; } 
		int CategoryPosition (ICategory category);

		void SetCategoryOrder (ICategory category, int order); 

		IEnumerable<IAthlete> Athletes { get; } 
		string SubmittingEmail { get; } 

		void SetTimeStamps (IEnumerable<DateTime> starts, IEnumerable<DateTime> finishes);
		string QueryReason { get;} 
		// TODO - make the adjustment 
		void SetAdjusted(TimeSpan adjustment);
		TimeSpan Elapsed { get; }
		// TODO - adjusted categories should acquire the adjusted time - should be negative 
		TimeSpan Adjusted { get; } 
		FinishType FinishType { get; } 
		// TODO - set the penalty, if required, and then output it in the results - amount should be positive 
		void SetPenalty(TimeSpan penalty, string citation);
		// TODO - disqualify any crew that needs it 
		void Disqualify(string citation);
		string Citation { get; }
	}

}

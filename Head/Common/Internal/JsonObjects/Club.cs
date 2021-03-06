using System;
using Head.Common.Domain;
using Head.Common.Internal.Overrides;
using System.Collections.Generic;
using Common.Logging;

namespace Head.Common.Internal.JsonObjects
{
	public class AthleteClub : IClub, IEquatable<IClub>, IEquatable<AthleteClub>
	{
		readonly string _index;
		readonly string _name;

		public AthleteClub(string index, string name)
		{
			_index = index;
			_name = name;
		}

		public string Index { get { return _index; } } 
		public string Name { get { return _name; } } 
		public string Country { get { return String.Empty; } }
		public bool IsBoatingLocation { get { return false; } } 

		public void AddBoatingCrew (ICrew crew)
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<ICrew> BoatingCrews { get { return new List<ICrew> (); } } 
	
		#region IEquatable implementation
		public bool Equals(IClub other)
		{
			if (ReferenceEquals(this, other))
				return true;
			if (other == null)
				return false;
			return this.Index == other.Index;

		}
		public bool Equals(AthleteClub other)
		{
			if (ReferenceEquals(this, other))
				return true;
			if (other == null)
				return false;
			return this.Index == other.Index;

		}
		#endregion

		public override int GetHashCode()
		{
			return Index.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if(obj is Club) return Equals((IClub)obj);
			if(obj is AthleteClub) return Equals((IClub)obj);
			return false;
		}
	}
	public class Club : IClub, IEquatable<Club>, IEquatable<IClub>
    {
        readonly ClubDetails _clubDetails; 
		readonly IList<ICrew> _boatingCrews;

		public Club(ClubDetails clubdetails)
        {
            _clubDetails = clubdetails;
			_boatingCrews = new List<ICrew> ();
        }

		public string Index { get  { return _clubDetails.Index; } } 
        public string Name { 
            get 
            { 
                return String.Format("{0}{1}", 
					 _clubDetails.Name,
                        String.IsNullOrEmpty(Country) ? String.Empty : " (" + Country + ")");                                  
            } 
        } 
		public void SetName(string name)
		{
			if(String.IsNullOrEmpty(_clubDetails.Name))
				_clubDetails.Name = name;
		}
        public string Country { get { return _clubDetails.Country; } } 
		public bool IsBoatingLocation { get { return _clubDetails.IsBoatingLocation; } } 
		public IEnumerable<ICrew> BoatingCrews { get { return _boatingCrews; } }
		public void AddBoatingCrew(ICrew crew)
		{
			_boatingCrews.Add (crew);
		}

        #region IEquatable implementation
        public bool Equals(Club other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;
            return this.Index == other.Index;

        }

		public bool Equals(IClub other)
		{
			if (ReferenceEquals(this, other))
				return true;
			if (other == null)
				return false;
			return this.Index == other.Index;

		}

        #endregion

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj is Club) return Equals((Club)obj);
            return false;
        }

        public override string ToString()
        {
			return string.Format("[{4}: Index={0}, Name={1}, Country={2}, IsBoatingLocation={3}{4}]", 
				Index, Name, 
				Country, IsBoatingLocation, 
				GetType().Name, 
				IsBoatingLocation 
				? _boatingCrews.Count.ToString()
					: String.Empty);
        }
    }
}


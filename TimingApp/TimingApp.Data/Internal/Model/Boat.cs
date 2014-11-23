using System;
using TimingApp.Data.Interfaces;
using System.Collections.Generic;
using TimingApp.Data.Internal;

namespace TimingApp.Data.Internal.Model
{
	class Boat : BaseNotifyPropertyChanged, IBoat 
	{
		readonly string _name;
		readonly int _number;
		readonly string _category;

		public Boat(int number, string name, string category)
		{
			_number = number;
			_name = name;
			_category = category;
		}

		#region IEquatable implementation

		public bool Equals(IBoat other)
		{
			return Number == other.Number;
		}

		#endregion

		#region IBoat implementation

		public int Number { get { return _number; } }
		public string Name { get { return _name; } }
		public string Category { get { return _category; } }

		public string PrettyName {
			get
			{
				return string.Format("{0} / {1} / {2}", Number, Name, Category);
			}
		}


		#endregion

	}	

}

using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using TimingApp.ApplicationLayer;
using TimingApp.Portable.DataLayer;
using TimingApp.Portable.Model;
using System.Drawing;
using MonoTouch.CoreGraphics;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace TimingApp
{
	public class Boat 
	{
		public int StartNumber { get; set; } 
		public string Name { get; set; } 
	}

	public class CrewSelectionElement : CheckboxElement
	{
		readonly int _index;
		readonly string _name;

		public CrewSelectionElement(int index, string name, bool value) : base(string.Format("{0}: {1}", index, name), value)
		{
			_index = index;
			_name = name;
		}

		public int Index { get { return _index; } } 
		public string Name { get { return _name; } } 
	}

	class MyRootElement : RootElement {
		public string ShortName { get; set; }

		public MyRootElement (string caption, string shortName)
			: base (caption)
		{
			ShortName = shortName;
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);
			cell.TextLabel.Text = ShortName;
			return cell;
		}
	}

	public class TimingDetailViewController : DialogViewController
	{
		public Action<TimingItem> ItemAdded;

		readonly IList<Section> _sections = new List<Section>();
		CrewsDialogViewController _popover;

		public TimingDetailViewController() : base (UITableViewStyle.Plain, null)
		{
			Initialize ();
		}


		protected void Initialize()
		{
			var crews = Create ().ToDictionary(b => b.StartNumber, b => b.Name);
			if(crews.Count == 0)
				crews = Enumerable.Range (1, 220).ToDictionary (i => i, i => "crew " + i);

			_popover = new CrewsDialogViewController(crews);
			UIPopoverController myPopOver = new UIPopoverController(_popover); 
			_popover.Changed += () => 
			{
				PopulateTable();
			};
		
			NavigationItem.RightBarButtonItem = new UIBarButtonItem("Crews", UIBarButtonItemStyle.Plain, null);
			NavigationItem.RightBarButtonItem.Clicked += (sender, e) => { myPopOver.PopoverContentSize = new SizeF(450f, 420f);
				myPopOver.PresentFromBarButtonItem (NavigationItem.RightBarButtonItem, UIPopoverArrowDirection.Up, true); };
		}

		// TODO - update the caption 
//		public void UpdateCaption(string str)
//		{
//			if (Root == null)
//				return;
//			Root.Caption = str;
//			ReloadData ();
//		}

		// chris - euch. 
		public void Reset ()
		{
			var crews = Create ().ToDictionary(b => b.StartNumber, b => b.Name);
			if(crews.Count == 0)
				crews = Enumerable.Range (1, 220).ToDictionary (i => i, i => "crew " + i);

			_popover.Reset (crews);
		}

		public IList<Boat> Create()
		{
			try{
				// chris - this needs to be strengthened and whatnot 
				const string path = "Json/VetsHead2014.json";

				bool exists = File.Exists(path);
				if(exists)
				{
					string json = File.ReadAllText(path);
									
					var intermediate = JsonConvert.DeserializeObject<List<Boat>>(json) as IList<Boat>;

					return intermediate;
				}
			}
			catch(Exception ex)
			{

			}

			return new List<Boat>();
		}

		Section AddAnotherItem(string heading)
		{
			var notes = new EntryElement ("Notes", "identifying marks", string.Empty);
			var button = new StyledStringElement("Insert mystery finisher");
			button.TextColor = UIColor.LightTextColor;
			button.DetailColor = UIColor.LightTextColor;
			button.BackgroundColor = UIColor.DarkTextColor;

			var newRoot = new Section (heading) { notes, button };
			button.Tapped += () => {
				ItemAdded (new TimingItem (Race, Location, Coordinates, OurLittleSecret, -1, DateTime.Now, notes.Value));
				notes.Value = string.Empty;
			};
			return newRoot;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			PopulateTable();			
		}

		void PopulateTable()
		{
			_sections.Clear ();
			// TODO - have a ticking clock in the title bar 
			foreach (var kvp in _popover.Selected) 
			{
				var ce = new CrewElement (kvp.Key, kvp.Value);
				var s = new Section(kvp.Key.ToString ()) { ce };
				_sections.Add (s);
			}
			Root = new RootElement ("Forthcoming crews") { _sections };
			Root.Add (AddAnotherItem ("Unidentified finisher"));
		}

		public override void Selected (NSIndexPath indexPath)
		{
			if (indexPath.Section < _sections.Count) 
			{
				// TODO - shouldn't need to parse something we put in there 
				int crew = int.Parse (_sections [indexPath.Section].Caption);
				Remove (crew);
				// chris - magic numbers! 
				ItemAdded (new TimingItem (Race, Location, Coordinates, OurLittleSecret, crew, DateTime.Now, String.Empty));
				PopulateTable ();
			}
			else
				base.Selected (indexPath);
		}
		// chris - magic number 
		public string Race { get { return "Vets Head 2014"; } } 
		public string Location { get; set; }
		public string OurLittleSecret { get; set; }
		// chris - get the GPS if available
		public string Coordinates { get { return "Where are we?"; } } 

		public void Remove(int crew)
		{
			_popover.Remove (crew);
		}
	}

	public class CrewElement : OwnerDrawnElement
	{
		readonly string _text; 

		// TODO - consider club specific colours 
		// TODO - add a notes box 
		public CrewElement (int number, string name) : base(UITableViewCellStyle.Default, "crewElement")
		{
			_text  = String.Format("Crew {0}: {1}", number, name);
		}

		public override void Draw (RectangleF bounds, CGContext context, UIView view)
		{
			UIColor.DarkGray.SetFill();
			context.FillRect(bounds);

			UIColor.Yellow.SetColor();   
			view.DrawString(_text, new RectangleF(10, 15, bounds.Width - 20, bounds.Height - 30), UIFont.BoldSystemFontOfSize(14.0f), UILineBreakMode.TailTruncation);
		}

		public override float Height (RectangleF bounds)
		{
			return 44.0f;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using TimingApp.ApplicationLayer;

namespace TimingApp
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.

	public class TimingSplitViewController : UISplitViewController
	{
		TimingDetailViewController _detailViewController;
		TimingMasterViewController _masterViewController;

		public TimingSplitViewController () : base()
		{
			// create our master and detail views
			_detailViewController = new TimingDetailViewController ();
			var detailNavigationController = new UINavigationController (_detailViewController);
			_masterViewController = new TimingMasterViewController (_detailViewController);
			var masterNavigationController = new UINavigationController (_masterViewController);

			ShouldHideViewController = (svc, vc, orientation) => {
				return false;
			};

			_detailViewController.ItemAdded += (item) => _masterViewController.AddItem(item);
		

			//			Li stFiles ("https://www.dropbox.com/sh/o0h70ccg9t3ssql/itnWVUH6K6");

			// ALWAYS SET THIS LAST (since iOS5.1)
			// https://bugzilla.xamarin.com/show_bug.cgi?id=3803
			// http://spouliot.wordpress.com/2012/03/26/events-vs-objective-c-delegates/
			// create an array of controllers from them and then assign it to the 
			// controllers property
			ViewControllers = new UIViewController[] { masterNavigationController, detailNavigationController };
		}

		public void Initialise(string location, string secret)
		{
			_masterViewController.Go (location, secret);
		}

		// TODO - pass through to update the time caption 
//		public void UpdateCaption(string str)
//		{
//			_detailViewController.UpdateCaption (str);
//		}
		// TODO - grab the race details from the circulated shareed JSON file - https://www.dropbox.com/sh/o0h70ccg9t3ssql/YGkhw06FKJ/race.json

//		void ListFiles (string link)
//		{
//			var dbc = new DBChooser ("").
//
//			DBError error;
//			DBPath path = new DBPath ();
//			var contents = DBFilesystem.SharedFilesystem.ListFolder (path, out error);
//			foreach (DBFileInfo info in contents) {
//				Console.WriteLine (info.Path);
//			}    
//		}
	}
}

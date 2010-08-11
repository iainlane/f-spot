/*
 * FSpot.UI.Dialog.SelectionRatioDialog.cs
 *
 * Author(s):
 *	Stephane Delcroix  <stephane@delcroix.org>
 *
 * This is free software. See COPYING fro details.
 */

using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

using Gtk;

using Mono.Unix;
using Hyena;

namespace FSpot.UI.Dialog {
	public class SelectionRatioDialog : GladeDialog
	{
		[Serializable]
		public struct SelectionConstraint {
			private string label;
			public string Label {
				get { return label; }
				set { label = value; }
			}
			private double ratio;
			public double XyRatio {
				get { return ratio; }
				set { ratio = value; }
			}

			public SelectionConstraint (string label, double ratio)
			{
				this.label = label;
				this.ratio = ratio;
			}
		}

		[Glade.Widget] Button close_button;
		[Glade.Widget] Button add_button;
		[Glade.Widget] Button delete_button;
		[Glade.Widget] Button up_button;
		[Glade.Widget] Button down_button;
		[Glade.Widget] TreeView content_treeview;
		private ListStore constraints_store;

		public SelectionRatioDialog () : base ("customratio_dialog")
		{
			close_button.Clicked += delegate (object o, EventArgs e) {SavePrefs (); this.Dialog.Destroy (); };
			add_button.Clicked += delegate (object o, EventArgs e) {constraints_store.AppendValues ("New Selection", 1.0);};
			delete_button.Clicked += DeleteSelectedRows;
			up_button.Clicked += MoveUp;
			down_button.Clicked += MoveDown;
			CellRendererText text_renderer = new CellRendererText ();
			text_renderer.Editable = true;
			text_renderer.Edited += HandleLabelEdited;
			content_treeview.AppendColumn (Catalog.GetString ("Label"), text_renderer, "text", 0);
			text_renderer = new CellRendererText ();
			text_renderer.Editable = true;
			text_renderer.Edited += HandleRatioEdited;
			content_treeview.AppendColumn (Catalog.GetString ("Ratio"), text_renderer, "text", 1);

			LoadPreference (Preferences.CUSTOM_CROP_RATIOS);
			Preferences.SettingChanged += OnPreferencesChanged;
		}

		private void Populate ()
		{
			constraints_store = new ListStore (typeof (string), typeof (double));
			content_treeview.Model = constraints_store;
			XmlSerializer serializer = new XmlSerializer (typeof(SelectionConstraint));
			string [] vals = Preferences.Get<string []> (Preferences.CUSTOM_CROP_RATIOS);
			if (vals != null)
				foreach (string xml in vals) {
					SelectionConstraint constraint = (SelectionConstraint)serializer.Deserialize (new StringReader (xml));
					constraints_store.AppendValues (constraint.Label, constraint.XyRatio);
				}
		}

		private void OnPreferencesChanged (object sender, NotifyEventArgs args)
		{
			LoadPreference (args.Key);
		}

		private void LoadPreference (String key)
		{
			switch (key) {
			case Preferences.CUSTOM_CROP_RATIOS:
				Populate ();
				break;
			}
		}

		private void SavePrefs ()
		{
			List<string> prefs = new List<string> ();
			XmlSerializer serializer = new XmlSerializer (typeof (SelectionConstraint));
			foreach (object[] row in constraints_store) {
				StringWriter sw = new StringWriter ();
				serializer.Serialize (sw, new SelectionConstraint ((string)row[0], (double)row[1]));
				sw.Close ();
				prefs.Add (sw.ToString ());
			}

#if !GCONF_SHARP_2_18
			if (prefs.Count != 0)
#endif
				Preferences.Set (Preferences.CUSTOM_CROP_RATIOS, prefs.ToArray());
#if !GCONF_SHARP_2_18
			else
				Preferences.Set (Preferences.CUSTOM_CROP_RATIOS, -1);
#endif
		}

		public void HandleLabelEdited (object sender, EditedArgs args)
		{
			args.RetVal = false;
			TreeIter iter;
			if (!constraints_store.GetIterFromString (out iter, args.Path))
				return;

			using (GLib.Value val = new GLib.Value (args.NewText))
				constraints_store.SetValue (iter, 0, val);

			args.RetVal = true;
		}

		public void HandleRatioEdited (object sender, EditedArgs args)
		{
			args.RetVal = false;
			TreeIter iter;
			if (!constraints_store.GetIterFromString (out iter, args.Path))
				return;

			double ratio;
			try {
				ratio = ParseRatio (args.NewText);
			} catch (FormatException fe) {
				Log.Exception (fe);
				return;
			}
			if (ratio < 1.0)
				ratio = 1.0 / ratio;

			using (GLib.Value val = new GLib.Value (ratio))
				constraints_store.SetValue (iter, 1, val);

			args.RetVal = true;
		}

		private double ParseRatio (string text)
		{
			try {
				return Convert.ToDouble (text);
			} catch (FormatException) {
				char [] separators = {'/', ':'};
				foreach (char c in separators) {
					if (text.IndexOf (c) != -1) {
						double ratio = Convert.ToDouble (text.Substring (0, text.IndexOf (c)));
						ratio /= Convert.ToDouble (text.Substring (text.IndexOf (c) + 1));
						return ratio;
					}
				}
				throw new FormatException (String.Format ("unable to parse {0}", text));
			}
		}

		private void DeleteSelectedRows (object o, EventArgs e)
		{
			TreeIter iter;
			TreeModel model;
			if (content_treeview.Selection.GetSelected (out model, out iter))
				(model as ListStore).Remove (ref iter);
		}

		private void MoveUp (object o, EventArgs e)
		{
			TreeIter selected;
			TreeModel model;
			if (content_treeview.Selection.GetSelected (out model, out selected)) {
				//no IterPrev :(
				TreeIter prev;
				TreePath path = model.GetPath (selected);
				if (path.Prev ())
					if (model.GetIter (out prev, path))
						(model as ListStore).Swap (prev, selected);
			}
		}

		private void MoveDown (object o, EventArgs e)
		{
			TreeIter current;
			TreeModel model;
			if (content_treeview.Selection.GetSelected (out model, out current)) {
				TreeIter next = current;
				if ((model as ListStore).IterNext (ref next))
					(model as ListStore).Swap (current, next);
			}
		}
	}
}

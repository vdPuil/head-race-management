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
using Head.Common.Utils;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Head.Common.Internal.JsonObjects;

namespace Head.Common.Generate
{
	public class ResultsPrinter
	{
		public static void Dump(IEnumerable<ICrew> crews) 
		{
			string raceDetails = "Vets Head - 30 March 2014 - Provisional Results";
			string updated = "Updated: \t" + DateTime.Now.ToShortTimeString () + " " + DateTime.Now.ToShortDateString ();
			StringBuilder sb = new StringBuilder ();
			sb.AppendLine (updated);

			using(var fs = new FileStream("Vets Head 2014 Results.pdf", FileMode.Create)){
				using(Document document = new Document(PageSize.A4.Rotate())){

					// 					BaseFont bf = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
					Font font = new Font(Font.FontFamily.HELVETICA, 7f, Font.NORMAL);
					Font italic = new Font(Font.FontFamily.HELVETICA, 7f, Font.ITALIC);
					Font bold = new Font(Font.FontFamily.HELVETICA, 7f, Font.BOLD);

					// step 2:
					// we create a writer that listens to the document and directs a PDF-stream to a file            
					PdfWriter.GetInstance(document, fs);

					// step 3: we open the document
					document.Open();

					// entitle the document 
					document.Add(new Paragraph(raceDetails));
					document.AddSubject(raceDetails);

					// grab the header and seed the table 

					float[] widths = new float[] { 1f, 1f, 5f, 1f, 2.5f, 1f, 1f, 1f, 3f };
					PdfPTable table = new PdfPTable(widths.Count()) 
					{
						TotalWidth = 800f,
						LockedWidth = true,                    
						HorizontalAlignment = 0,
						SpacingBefore = 20f,
						SpacingAfter = 30f,
					};
					table.SetWidths(widths);

					// TODO - categories, notes, penalties, etc. 
					foreach(var h in new List<string> { "Overall", "Start", "Crew", "Elapsed", "Category", "Category Pos", "Gender Pos", "Foreign Pos", "Notes" })
					{
						table.AddCell(new PdfPCell(new Phrase(h)) { Border = 1, HorizontalAlignment = 2, Rotation = 90 } );
						sb.AppendFormat ("{0}\t", h);
					}
					sb.AppendLine ();
					foreach (var crew in crews.OrderBy(cr => cr.FinishType).ThenBy(cr => cr.Elapsed)) 
					{
						ICategory primary;
						StringBuilder extras = new StringBuilder ();
						string overallpos = string.Empty;
						string categorypos = string.Empty;
						string genderpos = string.Empty;
						string foreignpos = string.Empty;
						if (crew is UnidentifiedCrew)
							primary = new TimeOnlyCategory ();
						else
							if (crew.Categories.Any (c => c is TimeOnlyCategory)) 
							{ 
								primary = crew.Categories.First (c => c is TimeOnlyCategory);
							} 
							else 
							{
								primary = crew.Categories.First (c => c is EventCategory);
								if (crew.FinishType == FinishType.Finished) 
								{
									overallpos = CategoryNotes(crew, c => c is OverallCategory, false, extras);
									categorypos = CategoryNotes(crew, c => c == primary, false, extras); 
									genderpos = CategoryNotes(crew, c => c.EventType == EventType.MastersHandicapped, false, extras); 
									foreignpos =  CategoryNotes(crew, c => c.EventType == EventType.Foreign, true, extras); 
								}
							}

						if (!string.IsNullOrEmpty (crew.QueryReason))
							extras.Append (crew.QueryReason);

						if (!string.IsNullOrEmpty (crew.Citation))
							extras.Append (crew.Citation);


						var objects = new List<Tuple<string, Font>> { 
							new Tuple<string, Font> (overallpos, font),
							new Tuple<string, Font> (crew.StartNumber.ToString (), font),
							new Tuple<string, Font> (crew.Name, font),
							new Tuple<string, Font> ((crew.FinishType == FinishType.Finished || crew.FinishType == FinishType.TimeOnly) ? crew.Elapsed.ToString().Substring(3).Substring(0,8) : crew.FinishType.ToString(), font),
							new Tuple<string, Font> (primary.Name, primary.Offered ? font : italic),
							new Tuple<string, Font> (categorypos, font ),
							new Tuple<string, Font> (genderpos, font ),
							new Tuple<string, Font> (foreignpos, font ),
							new Tuple<string, Font> (extras.ToString(), italic ),
						};
						// sb.AppendFormat ("{0}\t{1}\t{2}\t{3}\t{4}{5}", objects[0].Item1, objects[1].Item1, objects[2].Item1, objects[3].Item1, objects[4].Item1, Environment.NewLine);

						// TODO - actual category, for the purposes of adjustment 
						// chris - if multiple crews from the same club in the same category put the stroke's name - currently being overridden after manual observation 
						foreach (var l in objects)
							table.AddCell (new PdfPCell (new Phrase (l.Item1.TrimEnd (), l.Item2)) { Border = 0 }); 
					}
					using (System.IO.StreamWriter file = new System.IO.StreamWriter("vetshead14-results.txt"))
					{
						file.Write(sb.ToString());
					}

					document.Add(table);
					document.Add (new Paragraph ("Categories shown in italics have not attracted sufficient entries to qualify for prizes.", italic));
					document.Add (new Paragraph ("The adjusted and foreign prizes are open to all indicated crews and will be awarded based on adjusted times as calculated according to the tables in the Rules of Racing", font));
					document.Add (new Paragraph (updated, font));
					document.AddTitle("Designed by vrc.org.uk");
					document.AddAuthor("Chris Harrison, VH Timing and Results");
					document.AddKeywords("Vets Head, 2014, Results");

					document.Close();
				}
			}
		}

		static string CategoryNotes(ICrew crew, Func<ICategory, bool> predicate, bool useDefault, StringBuilder stringBuilder)
		{
			ICategory cat = useDefault ? crew.Categories.FirstOrDefault (c => predicate (c)) : crew.Categories.First (c => predicate (c));
			if(cat == null) 
				return string.Empty;

			int position = crew.CategoryPosition (cat);
			if (position == 1)
				stringBuilder.AppendFormat ("{0} winner. ", cat.Name);
			return position.ToString ();
		}

	}
	
}

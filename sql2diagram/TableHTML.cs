//
// TableHTML.cs
//
// Author:
//   Timotheus Pokorra (timotheus.pokorra@solidcharity.com)
//
// Copyright (C) 2015 Timotheus Pokorra
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Collections.Generic;

namespace sql2diagram
{
	public class TableHTML: Table
	{
		string strHTMLTableHeader = 
			"<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 3.2 Final//EN\">" + Environment.NewLine + 
			"<HTML>" + Environment.NewLine + 
			"<HEAD>" + Environment.NewLine + 
			"<LINK REL=\"stylesheet\" HREF=\"../table-doc.css\" TYPE=\"text/css\"/>" + Environment.NewLine + 
			"<script type=\"text/javascript\" src=\"../table-doc-sub.js\"></script>" + Environment.NewLine + 
			"</HEAD>" + Environment.NewLine + 
			"<BODY class=\"table\" onload=\"popupInit()\">" + Environment.NewLine + 
			"<div id=\"divDescription\"> <!--Empty div--> </div>" + Environment.NewLine;
		string strHTMLFooter =
			"</BODY>" + Environment.NewLine +
			"</HTML>" + Environment.NewLine;

		public TableHTML(Table t)
			:base(t)
		{
		}

		public TableHTML(string pName)
			:base(pName)
		{
		}

		public void prepareDisplay(DataBase db, string module, bool repeatedRun)
		{
			foreach (Constraint itc in constraints) {
				if (itc.getType() == eType.eForeignKey) {
					db.addToCurrentDiagram(itc);
				}
			}

			outHtml();
		}

		/// <summary>
		/// get all files img_*.html
		/// search for "table=<tablename>&"
		/// if yes, print a link to the diagram here
		/// </summary>
		public void printDiagramLinks(StreamWriter file) {
			string needle = "\"?";
			needle += name;
			needle += "\"";

			if (Directory.Exists ("img")) {
				string[] files = Directory.GetFiles ("img", "img_*.html");
				foreach (string filename in files) {
					StreamReader sr = new StreamReader (filename);
					if (sr.ReadToEnd ().Contains (needle)) {
						file.WriteLine ("<a href=\"../img/" + Path.GetFileName (filename) + "?" +
							name + "\" target=\"_top\">Diagram " +
							Path.GetFileNameWithoutExtension (filename).Substring(4) + "</a>&nbsp;");

					}
					sr.Close ();
				}
			} else {
				Console.WriteLine ("Couldn't find the directory img.");
			}
		}

		public int outHtml()
		{
			int nrLines = 0;

			if (!Directory.Exists ("tables")) {
				Directory.CreateDirectory ("tables");
			}
			if (!Directory.Exists ("img")) {
				Directory.CreateDirectory ("img");
			}

			string strTableDocFileName = "tables/" + name + ".html";

			StreamWriter dbTableDoc = new StreamWriter(strTableDocFileName);
			dbTableDoc.WriteLine (strHTMLTableHeader);
			dbTableDoc.WriteLine ("<!-- Table: " + name + " -->");
			dbTableDoc.WriteLine ("<DIV CLASS=\"tab\" ID=\"tab_" + name + "\">");
			dbTableDoc.WriteLine ("<table class=\"name\">");
			dbTableDoc.WriteLine ("<tr><td>");
			dbTableDoc.WriteLine ("Table <a href='../index.html?table=" + name + "&group=" + group + "' target='_top'>" + name + "</a>");
			dbTableDoc.WriteLine ("</td></tr>");
			dbTableDoc.WriteLine ("</table>");
			printDiagramLinks(dbTableDoc);

			dbTableDoc.WriteLine ("<table class=\"content\">");
			dbTableDoc.WriteLine ("<tr><td>");

			if (comment != "")
			{
				dbTableDoc.WriteLine ("Description:<br>");
				dbTableDoc.WriteLine (comment + "<br>");
			}
			dbTableDoc.WriteLine ("</td></tr></table>");
			dbTableDoc.WriteLine ("<table class=\"fields\"><COL id=\"field\"><COL id=\"descr\"><COL id=\"foreignkey\">");

			if (primary >= 1) {
				dbTableDoc.WriteLine ("<tr><td colspan=3><font class=\"PrimaryKey\">PRIMARY KEY</font></td></tr>");

				foreach (Attribute it in attributes) {
					AttributeHTML att = new AttributeHTML(it);
					if (isKey(att, eType.ePrimaryKey)) {
						att.outHtml(ref dbTableDoc, this);
					}
				}

				dbTableDoc.WriteLine ( "<tr><td colspan = '3'><hr/></td></tr>");
			}

			if (unique >= 1) {
				dbTableDoc.WriteLine ( "<tr><td colspan=3><font class=\"Unique\">UNIQUE KEY</font></td></tr>");

				foreach (Attribute it in attributes) {
					AttributeHTML att = new AttributeHTML(it);
					if (isKey(att, eType.eUnique)) {
						att.outHtml(ref dbTableDoc, this);
					}
				}

				dbTableDoc.WriteLine ("<tr><td colspan = '3'><hr/></td></tr>");
			}

			foreach (Attribute it in attributes) {
				AttributeHTML att = new AttributeHTML(it);
				if (!isKey(att, eType.ePrimaryKey) && !isKey(att, eType.eUnique))
					att.outHtml(ref dbTableDoc, this);
			}

			if ( foreign > 0) {
				dbTableDoc.WriteLine ("<tr><td colspan = '3'><hr/><br/>");
				dbTableDoc.WriteLine ("<font class=\"ForeignKey\">FOREIGN KEY</font><br/>");

				List<Constraint> constraints = getConstraints();
				foreach (Constraint it in constraints)
					if (it.getType() == eType.eForeignKey) {
						dbTableDoc.Write (it.getName() + ": ");
						dbTableDoc.Write (it.getLocalAttributesString());
						string strRemoteFields  = it.getRemoteAttributesString();
						dbTableDoc.WriteLine (
							" => <a href=\"" + it.getRemoteTableName() +
							".html#" + ((strRemoteFields.Length > 0)?it.getFirstRemoteAttribute():"top") +
							"\" target=\"table-info\" onMouseOver=\"popup( '" + strRemoteFields +
							"');\" onMouseOut=\"popout()\">" +
							it.getRemoteTableName() + "</a><br/>");
					}
				dbTableDoc.WriteLine ("</td></tr>");
			}
			if ( referenced.Count != 0) {
				dbTableDoc.WriteLine ("<tr><td colspan = '3'><hr/><br/>" +
					"<font class=\"ReferencedBy\">REFERENCED BY</font><br/>");

				int count = 0;
				foreach (Constraint it in referenced) {
					if (count > 0) {
						dbTableDoc.WriteLine(",");
					}
					if (count % 3 == 0 && count != 0) {
						dbTableDoc.WriteLine("<br/>");
					}

					dbTableDoc.Write (
						"<a href=\"" + it.getParentTableName() + ".html#top\" target=\"table-info\">" +
						it.getParentTableName() + "</a>");
					count++;
				}
				dbTableDoc.WriteLine ("</td><tr/>");
			}
			dbTableDoc.WriteLine ("</table><!-- end of fields -->");
			dbTableDoc.WriteLine ("<!-- end of table " + name + " -->");
			dbTableDoc.WriteLine ("</DIV>");
			dbTableDoc.WriteLine (strHTMLFooter);

			dbTableDoc.Close ();

			return nrLines;
		}
	}
}


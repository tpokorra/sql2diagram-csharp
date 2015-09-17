//
// DataBaseHTML.cs
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
	public class DataBaseHTML: DataBase
	{
		private string strHTMLMenuHeader =
			"<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 3.2 Final//EN\">" + Environment.NewLine +
			"<HTML>" + Environment.NewLine +
			"<HEAD>" + Environment.NewLine +
			"<LINK REL=\"stylesheet\" HREF=\"table-doc.css\" TYPE=\"text/css\"/>" + Environment.NewLine +
			"</HEAD>" + Environment.NewLine +
			"<BODY CLASS=\"menu\">" + Environment.NewLine;

		static string strHTMLGroupHeader =
			"<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 3.2 Final//EN\">" + Environment.NewLine +
			"<HTML>" + Environment.NewLine +
			"<HEAD>" + Environment.NewLine +
			"<LINK REL=\"stylesheet\" HREF=\"table-doc.css\" TYPE=\"text/css\"/>" + Environment.NewLine +
			"</HEAD>" + Environment.NewLine +
			"<BODY class=\"group\">" + Environment.NewLine;

		static string strHTMLFooter =
			"</BODY>" + Environment.NewLine +
			"</HTML>" + Environment.NewLine;

		public DataBaseHTML(DataBase db): base(db)
		{
		}

		public void prepareDisplay(string module, bool repeatedRun)
		{
			m_module = module;
			tables.Clear();
			foreach ( Table it in allTables) {
				tables.Add(it);
			}

			Table.resetColumn(tables.Count);
			foreach ( Table it in tables) {
				TableHTML tab = new TableHTML(it);
				tab.prepareDisplay(this, m_module, repeatedRun);
			}

			// merge Positions of associations
			foreach ( PosAssociation at in posAssociations) {
				Table src = getFromId(at.connections[0]);
				if (src.getName().Length != 0) {
					src.setPositionConstraint( at.connectionPoints[0],
						getFromId(at.connections[1]),
						at.obj_pos, at.obj_bb, at.orth_orient, at.orth_points);
				}
			}

			tables.Sort ();
		}

		public void outHtmlMap(string name, string title)
		// call writeDiagram before!
		{
			string diagramname = name.Substring(0, name.IndexOf("."));
			string filename = "img/img_" + diagramname + ".html";
			StreamWriter htmlfile = new StreamWriter(filename);
			//TestFileOpen( &htmlfile, filename);
			Console.WriteLine ("writing html map to file " + filename);

			Decimal left, top, right, bottom;
			getCornersOfDiagram(out left, out top, out right, out bottom);
			Decimal xfactor = 20, yfactor = 20;

			htmlfile.WriteLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">");

			htmlfile.WriteLine(Environment.NewLine + "<html><head><title>" + title + "</title></head>");
			htmlfile.WriteLine(Environment.NewLine + "<script type=\"text/javascript\">\n<!--");
			htmlfile.WriteLine("function scroll(tablename) {");
			htmlfile.WriteLine("\tswitch (tablename) {");
			foreach (Table it in tables) {
				if (it.isVisible()) {
					htmlfile.WriteLine("\t\tcase \"?" + it.getName() + "\": window.scrollTo("
						+ Convert.ToInt16((it.getPosition().x-left)*xfactor-200).ToString() + ","
						+ Convert.ToInt16((it.getPosition().y-top)*yfactor-200).ToString() + "); break;");
				}
			}
			htmlfile.WriteLine("\t}\n}");

			htmlfile.WriteLine("//-->\n</script>");

			htmlfile.WriteLine("<body BGCOLOR=\"#FFFFFF\" onload=\"scroll(location.search)\">");
			htmlfile.WriteLine("<base target=\"_top\">");

			htmlfile.WriteLine("\n<map name=\"PETRAtables\">");

			foreach (Table it in tables)
				if (it.isVisible())
				{
					string href;
					href = "index.html?table=";
					href += it.getName();
					href += "&group=";
					href += it.getGroup();

					htmlfile.WriteLine("<area shape=\"rect\" coords=\""
						+ Convert.ToInt16((it.getPosition().x-left)*xfactor).ToString() + ", "
						+ Convert.ToInt16((it.getPosition().y-top)*yfactor).ToString() + ", "
						+ Convert.ToInt16((it.getPosition().x-left+it.getWidth())*xfactor).ToString() + ","
						+ Convert.ToInt16((it.getPosition().y-top+it.getHeight())*yfactor+30).ToString()
						+ "\" href=\"../" + href + "\" target=\"_top\" alt=\""
						+ it.getName() + "\">");
				}
			htmlfile.WriteLine("</map>");

			htmlfile.WriteLine("\n<div align=\"center\">");
			htmlfile.WriteLine("<IMG SRC=\"" + diagramname + ".png\" ALT=\""
				+ diagramname + "\" BORDER=\"0\" usemap=\"#PETRAtables\">");
			htmlfile.WriteLine("</div>");

			htmlfile.WriteLine("\n</body>\n</html>");
			htmlfile.Close();
		}

		public void outHtml(StreamWriter file, string module)
		{
			file.WriteLine ("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 3.2 Final//EN\">");
			file.WriteLine ("<HTML>");
			file.WriteLine ("<HEAD>");
			file.WriteLine ("<TITLE>Module " + module + "</TITLE>");
			file.WriteLine ("<META NAME=\"Generator\" CONTENT=\"sql2dia\">");
			file.WriteLine ("<META NAME=\"Author\" CONTENT=\"?\">");
			file.WriteLine ("<LINK REL=\"stylesheet\" HREF=\"table-doc.css\" TYPE=\"text/css\"/>");
			file.WriteLine ("</HEAD><BODY class=\"group\">");
			file.Write ("<DIV id=\"" + module + "\" class=\"links-tab\"><FONT class=\"tablenames\">");

			List<string> names = new List<string>();
			foreach (Table it in tables) {
				if ( Table.cmpModule( it.getModule(), module)) {
					names.Add(it.getName());
				}
			}
			names.Sort();
			int row;
			for ( row = 0; row < names.Count; row++) {
				file.WriteLine("\t\t<a href='tables/" + names[row] + ".html#top' target='table-info'>\" + names[row] + \"</a><br/>");
			}
			file.WriteLine("</DIV></FONT>");
			file.WriteLine ("</BODY>");
			file.WriteLine("</HTML>");

			foreach (Table it in tables) {
				if (Table.cmpModule(it.getModule(), module)) {
					TableHTML tab = (TableHTML)it;
					tab.outHtml();
				}
			}
		}

		public void writeTableGroup( string group) {

			string strGroupDocFileName = "table-doc-tables-" + group + ".html";
			StreamWriter dbGroupDoc = new StreamWriter( strGroupDocFileName);
			List<string> tablenames = getTableNamesInGroup(group);

			// Setup new DIV
			dbGroupDoc.WriteLine(strHTMLGroupHeader);
			dbGroupDoc.WriteLine("\t<DIV id=\"ts_" + group + "\" class=\"links-tab\"><FONT class=\"tablenames\">");

			foreach (string tableit in tablenames) {
				dbGroupDoc.WriteLine("\t\t" + AttributeHTML.getHRef( tableit, true) + tableit + "</a><br/>");
			}
			tablenames.Clear();

			dbGroupDoc.WriteLine ("\t</FONT></DIV>");
			dbGroupDoc.WriteLine (strHTMLFooter);
			dbGroupDoc.Close();
		}

		public void writeMenus() {
			// Make a list of tables into groups
			string szGroupsFile = "table-doc-groups.html";
			StreamWriter dbMenuDoc = new StreamWriter( szGroupsFile);
			dbMenuDoc.WriteLine(strHTMLMenuHeader);
			dbMenuDoc.WriteLine("<DIV class=\"links\">");

			List<string> groups = getGroupNames();
			if (groups.Count == 0)
			{
				//find all groups by searching for the dia files;
				string[] files = Directory.GetFiles ("", "*.dia");
				foreach (string filename in files) {

					dbMenuDoc.WriteLine("\t<a href=\"table-doc-tables-" + Path.GetFileNameWithoutExtension(filename)
						+ ".html\" target=\"tables\"\">" + Path.GetFileNameWithoutExtension(filename) + "</a>");
				}
			}
			else
			{
				// use the group names that are assigned to each table
				foreach (string itGroup in groups)
				{
					dbMenuDoc.WriteLine("\t<a href=\"table-doc-tables-" + itGroup +
						".html\" target=\"tables\"\">" + itGroup + "</a>");
					writeTableGroup(itGroup);
				}
			}
			dbMenuDoc.WriteLine("</DIV>");
			dbMenuDoc.WriteLine(strHTMLFooter);
			dbMenuDoc.Close();
		}
	}
}


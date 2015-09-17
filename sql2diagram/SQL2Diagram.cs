//
// SQL2Diagram.cs
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
using System.Collections.Generic;
using System.Xml;

namespace sql2diagram
{
	public class SQL2Diagram
	{
		public static void Usage( int exit_val) {
			Console.WriteLine("Usage: ");
			Console.WriteLine("  " + System.AppDomain.CurrentDomain.FriendlyName + " <file with sql create script> <prefix of tables>");
			Console.WriteLine("                                   prefix can be: all, ie. ");
			Console.WriteLine ("                                   all tables are selected, no prefixes required");
			Console.WriteLine("  " + System.AppDomain.CurrentDomain.FriendlyName + " -p project-file | --dump sql-file(s)");
			Console.WriteLine("  " + System.AppDomain.CurrentDomain.FriendlyName + " -g -f sql-file");
			Console.WriteLine ();
			Console.WriteLine("Output format:");
			Console.WriteLine("\t[-d|--dump]    Generate a sample project file with given source file(s)");
			Console.WriteLine("\t[-g|--dumpgroups] Generate project file by group names with given source file");
			Console.WriteLine("\t[-p|--project] Create/update diagrams according to given project file (requires -f|--sqlfile)");
			Console.WriteLine ();
			Console.WriteLine("Options:");
			Console.WriteLine("\t[-f|--sqlfile]  - which sql file to use");
			Console.WriteLine("\t[-o|--diaoutputpath]  - where to read/write the dia file");
			Console.WriteLine("\t[-v|--verbose]  - make it verbose");
			Console.WriteLine("\t[-h|-?|--help]  - show this usage message");
			Console.WriteLine ();
			Environment.Exit( exit_val);
		}

		static void DumpProjectAllTables( string sqlfilename) {
			// Create a sample project file for the given source file(s)
			Console.WriteLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\" ?>");
			Console.WriteLine();
			Console.WriteLine("<database>");
			ParserSQL sql = new ParserSQL();

			Console.WriteLine("\t<group name=\"all\">");
			if ( !sql.readSQL( sqlfilename)) {
				Console.WriteLine("Problem reading sql create script file: " + sqlfilename);
				Environment.Exit( -1);
			}
			DataBase db = sql.getDatabase();
			// For all tables read, dump their names in this group
			Console.Write(db.ToString());
			// End of this group
			Console.WriteLine("\t</group>");
			Console.WriteLine("</database>");
		}

		/// <summary>
		/// Create a project file for the given source file by group comment that is with each table: 
		///   for example: 
		/// -- my table description  
		/// -- GROUP: account
		/// CREATE TABLE a_ledger ...
		/// </summary>
		static void DumpProjectAllTablesByGroup( string sqlfilename) {
			ParserSQL sql = new ParserSQL();

			if ( sql.readSQL( sqlfilename )) {
				DataBaseHTML db = new DataBaseHTML(sql.getDatabase());
				List<string> groups = db.getGroupNames();

				Console.WriteLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\" ?>");
				Console.WriteLine();
				Console.WriteLine("<database>");

				foreach (string itGroup in groups)
				{    
					Console.WriteLine("\t<group name=\"" + itGroup + "\">");
					// For all tables in that group dump their names here
					List<string> tableNamesInGroup = db.getTableNamesInGroup(itGroup);
					foreach (string itTable in tableNamesInGroup) 
					{
						Console.WriteLine("\t\t<tablename name=\"" + itTable + "\"/>");
					}
					// End of this group
					Console.WriteLine("\t</group>");
				}

				Console.WriteLine("</database>");

				db.writeMenus();
			}
		}

		static void RunProject( string szProject, string szDBSqlfile, string strPathDiaOutput) 
		{
			// Open the project file and check the structure
			XmlDocument doc = new XmlDocument();
			doc.Load (szProject);
			if (doc.DocumentElement.Name != "database")
			{
				Console.WriteLine("document " + szProject + " of the wrong type, root node != database but " + doc.DocumentElement.FirstChild.Name);
				Environment.Exit (-3);
			}

			// Read all the source-files
			Console.WriteLine("Reading source-files...");
			ParserSQL sql = new ParserSQL();

			Console.WriteLine("Source: " + szDBSqlfile);
			if ( !sql.readSQL( szDBSqlfile )) {
				Console.WriteLine("\tProblem reading sql file: " + szDBSqlfile);
				Environment.Exit (-3);
			}

			// Group the tables.
			Console.WriteLine("Grouping tables...");
			DataBaseHTML db = new DataBaseHTML(sql.getDatabase());
			db.prepareLinks();
			db.prepareDisplay( "", false);
			XmlNode cur = doc.DocumentElement;
			// on the diagram, tables that don't have all links displayed on that diagram get a special color marking
			// but sometimes, one table is referenced by each table; this table can be excluded (eg. s_user); use format [s_user],[s_other]
			string strColorTableIgnoreReferencedTables;
			if (((XmlElement)cur).HasAttribute("ColorIgnoreReferencedTables")) {
				strColorTableIgnoreReferencedTables = cur.Attributes["ColorIgnoreReferencedTables"].Value;
			}
			cur = cur.FirstChild;
			while ( cur != null) {
				if ( cur.Name == "group") {
					Console.WriteLine ("Group: " + cur.Attributes["name"].Value);
					string strGroup = cur.Attributes["name"].Value;
					XmlNode tables = cur.FirstChild;
					// Run over tables...
					string strTableList = String.Empty;
					while ( tables != null) {
						if ( tables.Name == "tablename") {
							if ( strTableList.Length > 0) {
								strTableList += ",";
							}
							strTableList += "[" + tables.Attributes["name"].Value + "]";
						}
						tables = tables.NextSibling;
					}

					// TODO processDia( strPathDiaOutput, strGroup, strTableList, strColorTableIgnoreReferencedTables);
				}
				cur = cur.NextSibling;
			}
		}

		public static int Main(string[] args)
		{
			bool bDoProject = false;
			bool bDoDump = false;
			bool bDoDumpGroups = false;
			string szProjectFile = string.Empty;
			string szSqlFile = string.Empty;
			string szDiaOutputPath = Environment.CurrentDirectory;

			int argsCount = 0;
			while (args.Length > argsCount) {
				if (args [argsCount] == "-d") {
					bDoDump = true;
				} else if (args [argsCount] == "-g") {
					bDoDumpGroups = true;
				} else if (args [argsCount] == "-p") {
					bDoProject = true;
					if (args.Length > argsCount + 1) {
						argsCount++;
						szProjectFile = args [argsCount];
					} else {
						SQL2Diagram.Usage (-1);
					}
				} else if (args [argsCount] == "-o") {
					if (args.Length > argsCount + 1) {
						argsCount++;
						szDiaOutputPath = args [argsCount];
					} else {
						SQL2Diagram.Usage (-1);
					}
				} else if (args [argsCount] == "-f") {
					if (args.Length > argsCount + 1) {
						argsCount++;
						szSqlFile = args [argsCount];
					} else {
						SQL2Diagram.Usage (-1);
					}
				}
				argsCount++;
			}

			if ( bDoDump) {
				if (szSqlFile == String.Empty) {
					SQL2Diagram.Usage (-1);
				}
				DumpProjectAllTables(szSqlFile);
			} else if ( bDoDumpGroups) {
				if (szSqlFile == String.Empty) {
					SQL2Diagram.Usage (-1);
				}
				DumpProjectAllTablesByGroup(szSqlFile);
			} else if ( bDoProject) {
				if (szSqlFile == String.Empty) {
					SQL2Diagram.Usage (-1);
				}
				RunProject( szProjectFile, szSqlFile, szDiaOutputPath);
			} else {
				SQL2Diagram.Usage (-1);
			}

			return 0;
		}
	}
}


//
// DataBase.cs
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

namespace sql2diagram
{
	public class PosAssociation
	{
		public string id, obj_pos, obj_bb, orth_points, orth_orient;
		public string[] connections = { "", ""};
		public int[] connectionPoints = { -1, -1};
	};

	public class DataBase
	{
		protected string m_module;
		protected List<Table> tables = new List<Table>();
		protected List<Table> allTables = new List<Table>();
		protected List<PosAssociation> posAssociations = new List<PosAssociation>();

		public DataBase()
		{
		}

		public DataBase(DataBase db)
		{
			m_module = db.m_module;
			tables = db.tables;
			allTables = db.allTables;
			posAssociations = db.posAssociations;
		}

		public void addLink(Constraint foreignkey)
		{
			string destTable = foreignkey.getRemoteTableName();
			Table tab = getAllTable(destTable);
			if (tab.getName() == destTable) {
				tab.addLink(foreignkey);
			}
		}
		public void prepareLinks()
		{
			foreach (Table it in allTables) {
				it.prepareLinks(this);
			}
		}

		public void addToCurrentDiagram(Constraint foreignkey)
		{
			string destTable = foreignkey.getRemoteTableName();
			Table tab = getTable(destTable);
			if ( tab.getName() != destTable) { // not already there
				tab = getAllTable(destTable);
				if (tab.getName().Length != 0) {
					tables.Add(tab);
				}
			}
		}

		public void resetSizePosition()
		{
			foreach (Table it in allTables) {
				it.setPosition(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, true);
			}
		}

		public void setPosition(string id, string name, string obj_pos, string obj_bb,
			string elem_corner, string elem_width, string elem_height, bool visible)
		{
			foreach ( Table it in allTables) {
				if (it.getName() == name) {
					it.setPosition(id, obj_pos, obj_bb, elem_corner, elem_width, elem_height, visible);
				}
			}
		}

		public void setPosAssociation(string id, string obj_pos, string obj_bb,
			string orth_points, string orth_orient,
			string[] connections, int[] connectionPoints)
		{
			PosAssociation p = new PosAssociation();
			p.id = id;
			p.obj_pos = obj_pos;
			p.obj_bb = obj_bb;
			p.orth_orient = orth_orient;
			p.orth_points = orth_points;
			p.connectionPoints[0] = connectionPoints[0];
			p.connections[0] = connections[0];
			p.connectionPoints[1] = connectionPoints[1];
			p.connections[1] = connections[1];
			posAssociations.Add(p);
		}

		public Table getFromId(string id)
		{
			foreach ( Table it in tables) {
				if (it.getId() == id) {
					return it;
				}
			}
			return new Table("");
		}

		public Table getTable(string name)
		{
			foreach ( Table it in tables) {
				if ( it.getName() == name) {
					return it;
				}
			}
			return new Table("");
		}

		public Table getAllTable(string name)
		{
			foreach ( Table it in allTables) {
				if ( it.getName() == name) {
					return it;
				}
			}
			return new Table("");
		}

		public List<string> getGroupNames()
		{
			List<string> result = new List<string>();
			foreach ( Table it in allTables) {
				bool groupFound = false;
				foreach (string group in result)
				{
					if (group == it.getGroup())
					{
						groupFound = true;
					}
				}
				if ( !groupFound ) {
					result.Add(it.getGroup());
				}
			}
			return result;
		}

		public List<string> getTableNamesInGroup(string groupname)
		{
			List<string> result = new List<string>();
			foreach ( Table it in allTables) {
				if (it.getGroup() == groupname)
				{
					result.Add(it.getName());
				}
			}
			result.Sort();
			return result;
		}

		public string getModule()
		{
			return m_module;
		}

		public static string getModuleFile( string table_name) {
			return table_name.Substring( 0, 2);
		}

		public Table addTable(string tablename)
		{
			Table newTable = new Table (tablename);
			allTables.Add(newTable);
			return newTable;
		}

		public void displayNonDisplayedTables()
		{
			foreach ( Table it in allTables) {
				if ( !it.isDisplayedAlready()) {
					Console.WriteLine( it.getName());
				}
			}
		}

		public bool isDisplayedOnCurrentDiagram(string table)
		{
			foreach (Table it in tables) {
				if (it.getName() == table) {
					return it.isVisible();
				}
			}
			return false;
		}

		public bool inTableList(Table tab, string strTableList)
		{
			return inTableList(tab.getName(), strTableList);
		}

		public bool inTableList(string tab, string strTableList)
		{
			return strTableList.Contains( "[" + tab + "]");
		}

		public override string ToString()
		{
			string o = String.Empty;
			foreach (Table it in allTables) {
				o += "\t\t<tablename name=\"" + it.getName() + "\"/>" + Environment.NewLine;
			}
			return o;
		}

		public void getCornersOfDiagram(out Decimal left, out Decimal top, out Decimal right, out Decimal bottom) {
			left = 0;
			top = 0;
			right = 0;
			bottom = 0;
/* TODO
			// get the upper left corner and the lower right corner of the image

			vector<Table>::iterator it;
			right = left = tables.begin()->getPosition().x;
			bottom = top = tables.begin()->getPosition().y;
			for (it = tables.begin(); it != tables.end(); it++)
				if (it->isVisible())
				{
					if (left > it->getPosition().x)
						left = it->getPosition().x;
					if (right < it->getPosition().x+it->getWidth())
						right = it->getPosition().x+it->getWidth();
					if (top > it->getPosition().y)
						top = it->getPosition().y;
					if (bottom < it->getPosition().y+it->getHeight())
						bottom = it->getPosition().y+it->getHeight();
				}

			vector<PosAssociation>::iterator at;
			for (at = posAssociations.begin(); at != posAssociations.end(); at++) {
				vector<string> vpoints;
				vector<string>::iterator pointsit;
				string s = at->orth_points;
				while (s.length()>0) {
					string::size_type posSemicolon = s.find(";");
					if (posSemicolon != string::npos) {
						vpoints.push_back(s.substr(0, posSemicolon));
						s = s.substr(posSemicolon+1);
					}
					else {
						vpoints.push_back(s);
						s = "";
					}
				}
				for (pointsit = vpoints.begin(); pointsit != vpoints.end(); pointsit++) {
					vector<string> vcoord;
					string s = *pointsit;
					while (s.length()>0) {
						string::size_type posSemicolon = s.find(";");
						if (posSemicolon != string::npos) {
							vcoord.push_back(s.substr(0, posSemicolon));
							s = s.substr(posSemicolon+1);
						}
						else {
							vcoord.push_back(s);
							s = "";
						}
					}
					if (vcoord.size() == 2) { // x and y
						float x = atof(vcoord.begin()->c_str());
						float y = atof((vcoord.begin()+1)->c_str());
						if (x > right)
							right = x+1;
						if (x < left)
							left = x;
						if (y > bottom)
							bottom = y+1;
						if (y < top)
							top = y;
					}
				}
			}
						*/
		}
	}
}


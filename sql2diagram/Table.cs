//
// Table.cs
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
	public class Table : IComparable<Table>
	{
		protected static int column = 0;
		protected static int line = 0;
		protected static int lines = 0;
		protected string name, comment, group;
		protected bool m_visible;
		protected List<Attribute> attributes = new List<Attribute>();
		protected List<Constraint> constraints = new List<Constraint>();

		protected int primary;
		protected int unique;
		protected int foreign;
		protected List<Constraint> referenced = new List<Constraint>();

		protected Point position;
		protected string m_obj_bb;
		protected string m_elem_corner, m_id;
		protected Decimal m_elem_width, m_elem_height;

		protected bool displayed_already, displayed_already_inModule;

		public Table ()
		{
		}

		public Table(string pName)
		{
			m_id = "";
			m_obj_bb = "";
			m_elem_corner = "";
			m_elem_width = 0.0m;
			m_elem_height = 0.0m;
			position = new Point(-1,-1);
			name = pName;
			group = "undefined";
			m_visible = false;
			displayed_already = false;
			displayed_already_inModule = false;
		}

		public Table(Table tab)
		{
			attributes = tab.attributes;
			constraints = tab.constraints;
			position = tab.position;
			foreign = tab.foreign;
			m_elem_corner = tab.m_elem_corner;
			m_elem_height = tab.m_elem_height;
			m_elem_width = tab.m_elem_width;
			m_obj_bb = tab.m_obj_bb;
			m_id = tab.m_id;
			name = tab.name;
			comment = tab.comment;
			group = tab.group;
			primary = tab.primary;
			unique = tab.unique;
			referenced = tab.referenced;
			m_visible = tab.m_visible;
			displayed_already = tab.displayed_already;
			displayed_already_inModule = tab.displayed_already_inModule;
		}

		static public bool cmpModule(string m1, string m2)
		{
			// for scenarios without any prefix at all
			if (m2.CompareTo("all") == 0)
			{
				return true;
			}

			string item1, item2, list1, list2;
			list1 = m1;
			while ( ( item1 = Utilities.getNextCSV(ref list1)) != "") {
				list2 = m2;
				while ((item2 = Utilities.getNextCSV(ref list2)) != "") {
					if (item1 == item2) {
						return true;
					}
				}
			}

			list1 = m1;
			// for scenarios, where no underscore is used
			while ( ( item1 = Utilities.getNextCSV(ref list1)) != "") {
				list2 = m2;
				while ((item2 = Utilities.getNextCSV(ref list2)) != "") {
					if ((item1.IndexOf(item2) == 0) || (item2.IndexOf(item1) == 0)) {
						return true;
					}
				}
			}
			return false;
		}

		public void setComment(string s)
		{
			if (s.Contains("GROUP: "))
			{
				comment = s.Substring(0, s.IndexOf("GROUP: "));
				group = s.Substring(s.IndexOf("GROUP: ") + "GROUP: ".Length);
			}
			else
			{
				comment = s;
			}
		}

		public string getName()
		{
			return name;
		}

		public string getId()
		{
			return m_id;
		}

		public string getModule()
		{
			int pos = name.IndexOf("_");
			if ( pos> 0) {
				return name.Substring(0, pos);
			}
			return name;
		}

		public string getGroup()
		{
			return group;
		}

		public Attribute addAttribute(string name)
		{
			Attribute newAttr = new Attribute (name);
			attributes.Add(newAttr);
			return newAttr;
		}

		public Constraint addConstraint(string name, string type)
		{
			Constraint newConstr = new Constraint (name, type);
			constraints.Add(newConstr);
			return newConstr;
		}

		public void setPosition(string id, string obj_pos, string obj_bb,
				string elem_corner, string elem_width, string elem_height, bool visible)
		{
			if (obj_pos.Length != 0) {
				position.x = Convert.ToDecimal(obj_pos.Substring(0, obj_pos.IndexOf(",")));
				position.y = Convert.ToDecimal(obj_pos.Substring(obj_pos.IndexOf(",")+1, obj_pos.Length - obj_pos.IndexOf(",")));
			}
			m_obj_bb = obj_bb;
			m_id = id;
			m_elem_corner = elem_corner;
			m_elem_width = Convert.ToDecimal(elem_width);
			m_elem_height = Convert.ToDecimal(elem_height);
			m_visible = visible;
		}

		public void setPositionConstraint(int nrAttr, Table foreignTable,
				string obj_pos, string obj_bb, string orth_orient,
				string orth_points)
		{
			if ( nrAttr >= 0) {
				foreach (Constraint it in constraints) {
					if ( it.getFirstLocalAttribute() == getNameAttribute( nrAttr)
						&&   it.getRemoteTableName() == foreignTable.getName()) {
						it.setPosition( obj_pos, obj_bb, orth_orient, orth_points);
					}
				}
			}
		}

		public bool isVisible()
		{
			return m_visible;
		}

		public void setVisible(bool visible)
		{
			m_visible = visible;
		}

		public bool isDisplayedAlready()
		{
			return displayed_already;
		}

		public void SetDisplayedAlready(bool displayedAlready, string module)
		{
			displayed_already = displayedAlready;
			if ( cmpModule( getModule(), module)) {
				displayed_already_inModule = true;
			}
		}

		public bool isKey(Attribute attr, eType key)
		{
			foreach ( Constraint it in constraints) {
				if ( ( it.getType() == key)
					&&   ( it.isInLocalAttr( attr.getName()) != -1)) {
					return true;
				}
			}
			return false;
		}

		public int getPosAttribute(string keyname)
		{
			foreach ( Constraint it in constraints) {
				if (it.getName() == keyname) {
					string attrname= it.getFirstLocalAttribute();
					foreach (Attribute it2 in attributes) {
						if (it2.getName() == attrname) {
							return it2.getLineNr();
						}
					}
				}
			}
			return 0;
		}

		public bool getConstraints(List<Constraint> pconstraints)
		{
			pconstraints = getConstraints ();
			return true;
		}

		public List<Constraint> getConstraints()
		{
			List<Constraint> pconstraints = new List<Constraint> ();
			foreach (Constraint it in constraints)
			{
				pconstraints.Add(it);
			}
			return pconstraints;
		}

		public bool getAttributes(List<string> attributeNames)
		{
			foreach (Attribute it in attributes)
			{
				attributeNames.Add(it.getName());
			}
			return attributes.Count != 0;

		}

		public bool setAttributeComment(string attr, string comment)
		{
			foreach (Attribute it2 in attributes)
			{
				if (it2.getName() == attr) {
					it2.setComment(comment);
					return true;
				}
			}
			return false;
		}

		public bool setAttributeCheck(string attr, string check)
		{
			foreach (Attribute it2 in attributes)
			{
				if (it2.getName() == attr) {
					it2.setCheck(check);
					return true;
				}
			}
			return false;
		}

		public string getNameAttribute(int pos)
		{
			foreach (Attribute it2 in attributes)
			{
				if (it2.getLineNr() == pos) {
					return it2.getName();
				}
			}
			return "";
		}


		public int CompareTo(Table tab)
		{
			return name.CompareTo(tab.name);
		}

		public int getImportance()
		{
			return referenced.Count*2+foreign;
		}

		public int getCountReferences(DataBase db, string strLocTableList)
		{
			int count = 0;
			foreach ( Constraint c in referenced)
			{
				if (db.inTableList(c.getParentTableName() , strLocTableList))
				{
					count++;
				}
			}
			return count;
		}

		public void addLink(Constraint foreignKey)
		{
			referenced.Add(foreignKey);
		}

		public void prepareLinks(DataBase db)
		{
			primary = foreign = unique = 0;

			foreach ( Constraint itc in constraints) {
				if( itc.getType()== eType.eForeignKey) {
					db.addLink(itc);
				}
				switch( itc.getType()) {
				case eType.ePrimaryKey: primary ++; break;
				case eType.eForeignKey: foreign ++; break;
				case eType.eKey: break;		  
				case eType.eUnique: unique ++; break;
				}
			}
		}

		public void prepareDisplay(DataBase db, string module)
		{
			foreach (Constraint itc in constraints) {
				if (itc.getType() == eType.eForeignKey) {
					if ( cmpModule( getModule(), module)) {
						db.addToCurrentDiagram(itc);
					}
				}
			}
		}

		public Point getPosition()
		{
			return position;
		}

		public void setPosition(Decimal x, Decimal y)
		{
			position = new Point(x, y);
		}

		public Decimal getHeight()
		{
			return m_elem_height;
		}

		public Decimal getWidth()
		{
			return m_elem_width;
		}

		public static void resetColumn( int totalNumber)
		{
			column = 0;
			line = 0;
			if (totalNumber > 0) {
				lines = totalNumber/4+1;
			}
		}
	}
}


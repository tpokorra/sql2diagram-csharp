//
// Constraint.cs
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
	public enum eType {ePrimaryKey, eForeignKey, eKey, eUnique} ;

	public class Constraint
	{
		protected string sName;
		protected eType type;
		protected List<string> localAttributes = new List<string>();
		protected List<PointerAttribute> remoteAttributes = new List<PointerAttribute>();
		protected PointerTable parentTable, remoteTable;
		protected string orth_orient, orth_points, obj_bb, obj_pos;

		public Constraint(string pName, string ptype)
		{
			parentTable = new PointerTable("");
			remoteTable = new PointerTable("");
			sName = pName;
			if (ptype.ToUpper() == "FOREIGN KEY") {
				type = eType.eForeignKey;
			} else if (ptype.ToUpper() == "PRIMARY KEY") {
				type = eType.ePrimaryKey;
			} else if (ptype.ToUpper() == "KEY") {
				type = eType.eKey;
			} else if (ptype.ToUpper() == "UNIQUE") {
				type = eType.eUnique;
			}
		}

		public Constraint(Constraint c)
		{
			localAttributes = c.localAttributes;
			remoteAttributes = c.remoteAttributes;
			parentTable = c.parentTable;
			remoteTable = c.remoteTable;
			sName = c.sName;
			type =c.type;
			obj_bb = c.obj_bb;
			obj_pos = c.obj_pos;
			orth_points = c.orth_points;
			orth_orient = c.orth_orient;
		}

		public string getFirstLocalAttribute()
		{
			if (localAttributes.Count > 0) {
				return localAttributes[0];
			}
			return "";
		}

		public void setPosition(string obj_pos, string obj_bb, string orth_orient, string orth_points)
		{
			this.obj_pos = obj_pos;
			this.obj_bb = obj_bb;
			this.orth_orient = orth_orient;
			this.orth_points = orth_points;
		}

		public void getPosition(ref string obj_pos, ref string obj_bb, ref string orth_orient, ref string orth_points)
		{
			obj_pos = this.obj_pos;
			obj_bb = this.obj_bb;
			orth_orient = this.orth_orient;
			orth_points = this.orth_points;
		}

		public string getName()
		{
			return sName;
		}

		public eType getType()
		{
			return type;
		}

		public int isInLocalAttr(string attr)
		{
			int pos = 0;
			foreach (string it in localAttributes) {
				if (it == attr) {
					return pos;
				}
				pos++;
			}

			return -1;
		}

		public PointerAttribute getRemoteAttributes(int pos)
		{
			return remoteAttributes[pos];
		}

		public string getRemoteAttributesString()
		{
			string s = String.Empty;
			foreach (PointerAttribute it in remoteAttributes) {
				if (s != "") s += ";";
				s += it.getAttributeName();
			}
			return s;
		}

		public string getFirstRemoteAttribute()
		{
			if (remoteAttributes.Count == 0) {
				return "";
			}
			else {
				return remoteAttributes[0].getAttributeName();
			}
		}

		public string getLocalAttributesString()
		{
			string s = String.Empty;
			foreach (string it in localAttributes) {
				if (s != "") s += ";";
				s += it;
			}
			return s;
		}

		public string getRemoteTableName()
		{
			return remoteTable.getTableName();
		}

		public string getParentTableName()
		{
			return parentTable.getTableName();
		}

		public void setLocalColumns(List<string> columns)
		{
			localAttributes = columns;
		}

		public void setRemoteColumns(string ParentTable, string RemoteTable, List<string> columns)
		{
			remoteAttributes.Clear();
			parentTable = new PointerTable(ParentTable);
			remoteTable = new PointerTable(RemoteTable);
			foreach (string it in columns) {
				remoteAttributes.Add(new PointerAttribute(RemoteTable, it));
			}
		}		
	}
}


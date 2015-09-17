//
// Attribute.cs
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
	public class Attribute
	{
		protected string sName;
		protected string sComment = String.Empty;
		protected string sType = String.Empty;
		protected string sCheck = String.Empty;
		protected List<string> typeParam = new List<string>();
		protected string sConstraint = String.Empty;
		protected string sDefault = String.Empty;
		protected int lineNr;

		public Attribute(string name)
		{
			sName = name;
		}

		public Attribute(Attribute attr)
		{
			sType = attr.sType;
			typeParam = attr.typeParam;
			sName = attr.sName;
			sType = attr.sType;
			sCheck = attr.sCheck;
			typeParam = attr.typeParam;
			sConstraint = attr.sConstraint;
			sDefault = attr.sDefault;
			lineNr = attr.lineNr;
			sComment = attr.sComment;
		}

		public void setComment(string s)
		{
			sComment = s;
		}

		public void setType(string type)
		{
			sType = type;
		}

		public void setType(List<string> type)
		{
			typeParam = type;
		}

		public void setConstraint(string constraint)
		{
			sConstraint = constraint;
		}

		public void setDefault(string def)
		{
			sDefault = def;
		}

		public void setCheck(string check)
		{
			sCheck = check;
		}

		public string getCheck()
		{
			return sCheck;
		}

		public string getName()
		{
			return sName;
		}

		public int getLineNr()
		{
			return lineNr;
		}

		public string getType()
		{
			string type = sType;
			if ( typeParam.Count != 0) {
				type += "(";
				bool first = true;
				foreach (string it in typeParam) {
					if (!first) {
						type += ", ";
					}
					first = false;
					type += it;
				}
				type += ")";
			}
			return type;
		}		
	}
}


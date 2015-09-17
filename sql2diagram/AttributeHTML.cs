//
// AttributeHTML.cs
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
	public class AttributeHTML: Attribute
	{
		public AttributeHTML(Attribute attr) : base(attr)
		{
		}

		public static string getHRef( string strTable, bool bWithOffset = false, string strTarget = "table-info", string strExtraTags = "", string anchor = "top") {
			// TODO: maybe add parameter stuff...
			return "<a href=\"" + ( bWithOffset ? "tables/" : "") +
				"" + strTable + ".html#" + anchor + "\" target=\"" + strTarget + "\"" +
				strExtraTags + ">";
		}

		public int outHtml(ref StreamWriter pdbDoc, TableHTML table) {
			pdbDoc.WriteLine("<tr class=\"field\"><td width=\"35%\">");
			pdbDoc.WriteLine("<a name=\"" + sName + "\">");
			pdbDoc.WriteLine("<b><a href='../index.html?table=" + table.getName() + "&group=" + table.getGroup() + "#" + sName + "' target='_top'>" + sName + "</b></a>");
			pdbDoc.WriteLine("<div style=\"margin-left: 20px;\">" + sType);

			if (typeParam.Count > 0) {
				pdbDoc.Write(" (");
				bool first = true;
				foreach (string it in typeParam) {
					if (!first) pdbDoc.Write(", ");
					first = false;
					pdbDoc.Write(it);
				}
				pdbDoc.WriteLine(")");
			}
			if (sDefault.Length > 0) {
				pdbDoc.WriteLine(" <font class=\"DefaultFieldValue\">default: " + sDefault + "</font>");
			}

			if (sCheck.Length > 0) {
				pdbDoc.WriteLine(" <font class=\"CheckField\">CHECK (" + sCheck + ")</font>");
			}

			if (sConstraint.Length > 0) {
				pdbDoc.WriteLine(" <font class=\"FieldConstraints\">"+ sConstraint + "</font>");
			}

			pdbDoc.Write("</div></td><td>");
			if ( sComment != "") {
				pdbDoc.WriteLine(sComment);
			}
			pdbDoc.Write("</td><td>");

			List<Constraint> constraints = table.getConstraints();
			int count =0;
			foreach (Constraint it in constraints) {

				if (it.getType() == eType.eForeignKey
					&& (it.isInLocalAttr(sName)) != -1) {
					if (count > 0) {
						pdbDoc.Write(", ");
					}
					count++;
					string remoteTable = it.getRemoteTableName();
					string strRemoteFields = it.getRemoteAttributesString();
					pdbDoc.Write(getHRef( remoteTable, false, "table-info", " onMouseOver=\"popup( '" + strRemoteFields + "');\" onMouseOut=\"popout()\"",
						(strRemoteFields.Length>0)?strRemoteFields:"top")
						+ remoteTable + "</a>");

				}
			}
			pdbDoc.WriteLine("</td></tr>");
			return 0;
		}
	}
}


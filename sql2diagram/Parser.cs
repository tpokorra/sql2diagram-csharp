//
// Parser.cs
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

namespace sql2diagram
{
	public class Parser
	{
		protected StreamReader hFile;
		protected string line;
		protected string token;
		protected string current;
		protected string comment;

		public bool open(string name)
		{
			hFile = new StreamReader(name);
			current = String.Empty;
			return !hFile.EndOfStream;
		}

		public void close()
		{
			hFile.Close ();
		}

		protected string trimQuotes(string s)
		{
			if (s[0] == s[s.Length-1]) {
				if ((s[0] == '\'')
					|| (s[0] == '"')
					|| (s[0] == '`')) {
					return s.Substring (1, s.Length - 2);
				}
			}
			return s;	
		}

		public string getComment()
		{
			return comment;
		}

		public string goBackToken(string current, string token)
		{
			return current + " " + token;
		}

		public string getNextToken(string current, ref string token)
		{
			int i = 0;

			do {
				current = current.Trim();
				token = current;

				while (i < token.Length
					&& token[i] != ' '
					&& token[i] != ')'
					&& token[i] != '('
					&& token[i] != ','
					&& token[i] != '.'
					&& token[i] != ';'
					&& token[i] != '\t' ) {
					if ( token[i] == '\'') {
						i++;
						while (i < token.Length && token[i] != '\'') {
							i++;
						}
						i++;
					}
					else if (token[i] == '"') {
						i++;
						while (i < token.Length && token[i] != '"') {
							i++;
						}
						i++;
					}
					else if (token[i] == '`') {
						i++;
						while (i < token.Length && token[i] != '`') {
							i++;
						}
						i++;
					} else {
						i++;
					}
				}

				if (i == 0 && token.Length > 0) {
					i++;
				} else if (i == 0) {
					current = goToNextLine(current);
				}

				token = token.Substring(0, i);
			} while ( (i == 0 || token == " ") && !hFile.EndOfStream);

			return current.Substring(token.Length);
		}

		public string getNextTokenWithoutQuotes(string current, string token)
		{
			getNextToken(current, ref token);
			return trimQuotes(token);
		}

		public string goToNextLine(string current)
		{
			comment = "";
			do {
				line = hFile.ReadLine().Trim();
				current = line;
				if (current.Length >= 2 && current.Substring(0,2) == "--") {
					comment += "\n ";
					comment += current.Substring(2);
				}
			} while ( !hFile.EndOfStream
				&& ( current.Length == 0 || (current.Length >= 2 && current.Substring(0, 2) == "--")));
			return current;
		}

		public string readToken(string current, ref string token, string expected)
		{
			current = getNextToken(current, ref token);
			if (token.ToLower().CompareTo(expected.ToLower()) != 0) {
				Console.WriteLine (Environment.NewLine + "error: " + line);
				return string.Empty;
			}
			return current;
		}
	}
}


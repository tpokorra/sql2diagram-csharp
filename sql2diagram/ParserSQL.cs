//
// ParserSQL.cs
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
using System.Collections;
using System.Collections.Generic;

namespace sql2diagram
{
	public class ParserSQL: Parser
	{
		private DataBase db = new DataBase();

		public ParserSQL()
		{
		}

		public List<string> readList(bool withBracketOpen=true)
		{
			List<string> list = new List<string> ();

			if ( withBracketOpen) {
				current = readToken(current, ref token, "(");
			}
			current = getNextToken(current, ref token);

			list.Add(trimQuotes(token));
			current = getNextToken(current, ref token);
			while (token != ")") {
				current = getNextToken(current, ref token);
				list.Add(trimQuotes(token));
				current = getNextToken(current, ref token);
			}

			return list;
		}

		public void readKey(Table tab)
		{
			string constraintName;
			if ( token.ToUpper() == "PRIMARY") {
				current = readToken(current, ref token, "KEY");
				Constraint cons = tab.addConstraint("", "primary key");
				List<string> list = readList();
				cons.setLocalColumns(list);
				current = getNextToken(current, ref token);
			}
			else if ( token.ToUpper() == "KEY") {
				current = getNextToken(current, ref token);
				constraintName = trimQuotes(token);
				Constraint cons = tab.addConstraint(constraintName, "key");
				List<string> list = readList();
				cons.setLocalColumns(list);
				current = getNextToken(current, ref token);
			}
		}

		public void readConstraint(Table tab)
		{
			current = getNextToken(current, ref token);
			string constraintName = trimQuotes(token);
			current = getNextToken(current, ref token);
			if ( token.ToUpper() == "PRIMARY") {
				current = readToken(current, ref token, "KEY");
				Constraint cons = tab.addConstraint(constraintName, "primary key");
				List<string> list = readList();
				cons.setLocalColumns(list);
				current = getNextToken(current, ref token);
			} else if (token.ToUpper() == "FOREIGN") {
				current = readToken(current, ref token, "KEY");
				Constraint cons = tab.addConstraint(constraintName, "foreign key");
				string table;
				List<string> list = readList();
				cons.setLocalColumns(list);
				current = readToken(current, ref token, "REFERENCES");
				current = getNextToken(current, ref token); // table
				table = trimQuotes(token);
				list = readList();
				current = getNextToken(current, ref token);
				cons.setRemoteColumns(tab.getName(), table, list);
				if (token.ToUpper() == "ON") {
					current = getNextToken(current, ref token); // update
					current = getNextToken(current, ref token); // cascade 
					current = getNextToken(current, ref token); 
				}

			} else if (token.ToUpper() == "KEY") {
				Constraint cons = tab.addConstraint(constraintName, "key");
				List<string> list = readList();
				cons.setLocalColumns(list);
				current = getNextToken(current, ref token);
			} else if (token.ToUpper() == "UNIQUE") {
				Constraint cons = tab.addConstraint(constraintName, "unique");
				List<string> list = readList();
				cons.setLocalColumns(list);
				current = getNextToken(current, ref token);
			}
			if (token.ToUpper() == "USING") {
				// USING INDEX TABLESPACE INDX
				while (token[0] != ')' && token[0] != ',') {
					current = getNextToken(current, ref token);
				}
				if (token.ToUpper() == "INDEX") {
					current = getNextToken(current, ref token);
				}
			}
		}

		public void readCheck(Table tab)
		{
			current = getNextToken(current, ref token);
			if (token == "(") {
				string check = String.Empty;
				current = getNextToken(current, ref token);
				string attr = token;
				int openBrackets = 1;
				while (openBrackets > 0 && current.Length != 0 && !hFile.EndOfStream) {
					check += " " + token;
					current = getNextToken(current, ref token);
					if (token == ")") {
						openBrackets --;
					}
					if (token == "(") {
						openBrackets ++;
					}
				}

				if (!tab.setAttributeCheck(attr, check)) {
					Console.WriteLine("could not find attribute " + attr + " in table " + tab.getName() + " to add the check.");
				}
				current = getNextToken(current, ref token);
			}
		}

		public void readColumnNULL(Attribute att)
		{
			if (token.ToUpper() == "NOT") {
				current = getNextToken(current, ref token);
				if (token.ToUpper() == "NULL") {
					current = getNextToken(current, ref token);
					att.setConstraint("NOT NULL");
				}
			}
			if (token.ToUpper() == "NULL") {
				current = getNextToken(current, ref token);
				att.setConstraint("NULL");
			}
		}

		public void readColumn(Table tab)
		{
			Attribute att = tab.addAttribute(trimQuotes(token));
			att.setComment(getComment());
			current = getNextToken(current, ref token);
			string type = token;
			if (token.ToUpper() == "CHARACTER") {
				current = getNextToken(current, ref token);
				if (token.ToUpper() == "VARYING") {
					type = type + " " + token; // Varying
				} else {
					current = goBackToken(current, token);
				}
			}
			if ( token.ToUpper() == "TIMESTAMP") {
				current = getNextToken(current, ref token);
				if ( token.ToUpper() == "WITH" || token.ToUpper() == "WITHOUT") {
					type = type + " " + token;
					current = getNextToken(current, ref token);
					if (token.ToUpper() == "TIME") {
						type = type + " " + token;
						current = getNextToken(current, ref token);
						if (token.ToUpper() == "ZONE") {
							type = type + " " + token;
						}
					}
				} else {
					current = goBackToken(current, token);
				}

			}

			att.setType(type);

			current = getNextToken(current, ref token);
			// parameter of type
			if (token == "(") {
				List<string> typeParam = readList(false);
				att.setType (typeParam);
				current = getNextToken(current, ref token);
			}

			if (token.ToUpper() == "UNSIGNED") {
				current = getNextToken(current, ref token);
			}

			readColumnNULL(att);

			if (token.ToUpper() == "DEFAULT") {
				string default_token;
				current = getNextToken(current, ref token);
				default_token = "";
				do {
					default_token += token;
					if (token == "(") {
						while (token != ")") {
							current = getNextToken(current, ref token);
							default_token += token;
						}
						current = getNextToken(current, ref token);
					}
					else {
						current = getNextToken(current, ref token);
						if (token == "(") {
							default_token += token;
							while (token != ")") {
								current = getNextToken(current, ref token);
								default_token += token;
							}
							current = getNextToken(current, ref token);
						}
					}
				} while (token[0] == ':');
				if ( (token.ToUpper() == "WITH") || (token.ToUpper() == "WITHOUT")) {
					default_token = default_token + " " + token;
					current = getNextToken(current, ref token);
					if (token.ToUpper() == "TIME") {
						default_token = default_token + " " + token;
						current = getNextToken(current, ref token);
						if (token.ToUpper() == "ZONE") {
							default_token = default_token + " " + token;
							current = getNextToken(current, ref token);
						}
					}
				}
				if (token.ToUpper() == "VARYING") {
					default_token = default_token + " " + token; // Varying
					current = getNextToken(current, ref token);
				}
				att.setDefault(default_token);
			}

			readColumnNULL(att);

			if (token.ToUpper() == "CHECK") {
				current = getNextToken(current, ref token);
				if (token == "(") {
					string check = string.Empty;
					current = getNextToken(current, ref token);
					int openBrackets = 1;
					while (openBrackets > 0 && current.Length != 0 && !hFile.EndOfStream) {
						check += " " + token;
						current = getNextToken(current, ref token);
						if (token == ")") {
							openBrackets --;
						}
						if (token == "(") {
							openBrackets ++;
						}
					}
					att.setCheck(check);
					current = getNextToken(current, ref token);
				}

			}
			while ( token != ","
				&& token != ")"
				&& current.Length != 0 && !hFile.EndOfStream) {
				current = getNextToken(current, ref token);
			}
			if ( current.Trim().Length >= 2 && current.Trim().Substring(0, 2) == "--") {
				att.setComment(current.Trim().Substring(2));
				current = goToNextLine(current);
			}
		}

		public void readAlterTable()
		{
			/* ALTER TABLE ONLY di_template_pos
  			   ADD CONSTRAINT di_template_pos_pk PRIMARY KEY (person_key, role, field_key);
			*/
			current = getNextToken(current, ref token);
			if (token.ToUpper() != "TABLE") {
				return;
			}
			current = getNextToken(current, ref token);
			if (token.ToUpper() == "ONLY") {
				current = getNextToken(current, ref token);
			}
			Table tab = db.getAllTable(token);
			if (tab.getName() != token) {
				Console.WriteLine("could not create a constraint, table " + token + " not found ");
				return;
			}
			current = getNextToken(current, ref token);
			if ( token.ToUpper() != "ADD") {
				return;
			}
			current = getNextToken(current, ref token);
			if (token.ToUpper() != "CONSTRAINT") {
				return;
			}
			readConstraint(tab);
		}

		public void readTable()
		{
			current = getNextToken(current, ref token);

			Table tab = db.addTable(trimQuotes(token));
			//	printf("read %s ... ", tab.getName().c_str());
			tab.setComment(getComment());
			//printf("Table Name: %s\n", tab.getName().c_str());
			current = getNextToken(current, ref token); // '('
			do {
				current = getNextToken(current, ref token);
				if (token.ToUpper() == "CONSTRAINT") {
					readConstraint(tab);
				} else if (token.ToUpper() == "PRIMARY") {
					readKey(tab);
				} else if (token.ToUpper() == "KEY") {
					readKey(tab);
				} else if (token.ToUpper() == "CHECK") {
					readCheck(tab);
				}
				else {
					readColumn(tab);
				}
			} while ( token != ")" && !hFile.EndOfStream);

			while (token != ";"
				&& !hFile.EndOfStream) {
				current = getNextToken(current, ref token);
			}
			//	printf(" done\n");
		}

		public void readComment()
		{
			//COMMENT ON TABLE AD_Attribute IS 'User Defined Attributes'
			//COMMENT ON COLUMN T_Aging.DueDate IS 'Base Date for Aging calculation'
			current = getNextToken(current, ref token);
			if (token.ToUpper() != "ON") {
				return;
			}

			current = getNextToken(current, ref token);
			if (token.ToUpper() == "TABLE") {

				current = getNextToken(current, ref token);
				Table tab = db.getAllTable(token);
				if (tab.getName() != token) {
					Console.WriteLine("could not add a comment, table " + token + " not found ");
					return;
				}

				current = getNextToken(current, ref token); // IS

				current = getNextToken(current, ref token);
				// without quotes
				tab.setComment(token.Substring(1, token.Length - 2));

			} else if (token.ToUpper() == "COLUMN") {

				current = getNextToken(current, ref token);
				Table tab = db.getAllTable(token);
				if (tab.getName() != token) {
					Console.WriteLine("could not add a comment to a field, table " + token + " not found ");
					return;
				}
				current = getNextToken(current, ref token); // .

				current = getNextToken(current, ref token);
				string attr = token;
				current = getNextToken(current, ref token); // IS

				current = getNextToken(current, ref token);
				// without quotes
				token = token.Substring(0, token.Length - 2);
				if (!tab.setAttributeComment(attr, token.Substring(1))) {
					Console.WriteLine("could not add a comment to a field, field " + attr + "in table " + tab.getName() + " not found ");
					return;
				}
			}

		}

		public bool readSQL(string filename)
		{
			int count = 0;

			if (!open(filename)) {
				return false;
			}

			current = getNextToken("", ref token);
			while (!hFile.EndOfStream){
				if (token.ToUpper() == "CREATE") {
					current = getNextToken(current, ref token);
					if (token.ToUpper() == "TABLE") {
						readTable();
					}
				} else if (token.ToUpper() == "ALTER") {
					readAlterTable();
				} else if (token.ToUpper() == "COMMENT") {
					readComment();
				} else {
					current = getNextToken(current, ref token);
				}
				count ++;
			}
			close();
			return true;
		}

		public DataBase getDatabase()
		{
			return db;
		}
	}
}


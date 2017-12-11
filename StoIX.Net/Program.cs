using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text.RegularExpressions;
using System.Xml;
using CommandLine;

namespace StoIX.Net
{
	class Program
	{
		static void Main(string[] args)
		{
			Regex g = new Regex(@"<xs:element name=""(\w+)"">");
			DataSet ds = new DataSet();
			IList<string> listElements = new List<string>();

			using (StreamReader sr = new StreamReader(@"Galaktika.HCM.Demo2.HAT.ClearVersion.xml"))
			{
				Regex pattern = new Regex(@"<xs:element name=""(\w+)"">");

				while (Regex.IsMatch(sr.ReadLine(), "<xs"))
				{
					
				}
			}

			if (args.Length > 0)
			{
				var options = Parser.Default.ParseArguments<Options>(args);

				using (StreamReader r = new StreamReader(options.Value.Filename))
				{
					string line;

					while ((line = r.ReadLine()) != null)
					{
						Match m = g.Match(line);
						if (m.Success)
						{
							listElements.Add(m.Groups[1].Value);
						}
					}
				}

				string sConnectionString = String.Format("User ID={0};password={1};Initial Catalog={2};Data Source={3}",
								options.Value.Username, options.Value.Password, options.Value.Database, options.Value.DataSource);
				using (SqlConnection objConn = new SqlConnection(sConnectionString))
				{
					objConn.Open();
					string sSQL = "Select TABLE_NAME FROM INFORMATION_SCHEMA.TABLES" +
						" Where TABLE_TYPE = 'BASE TABLE'" +
						" Order By TABLE_NAME;";
					DataSet dataSet = new DataSet();

					using (objConn)
					{
						DataSet listTables = new DataSet();
						SqlDataAdapter adapter = new SqlDataAdapter(sSQL, objConn);
						adapter.Fill(listTables);
						Console.WriteLine("Table names list obtained");
						foreach (DataTable table in listTables.Tables)
						{
							int i = 0;
							string getDataTablesSql = "USE [" + options.Value.Database + @"]\nGO\n\nSELECT";
							using (StreamWriter file = new StreamWriter(@"outFile.sql"))
							{
								file.WriteLine(getDataTablesSql);
							
								foreach (DataRow dataRow in table.Rows)
								{
									if (listElements.Contains(dataRow.ItemArray[0].ToString()))
									{
										i += 1;

										var rowType = dataRow.ItemArray[0].ToString() == "BirthdaysItem" || dataRow.ItemArray[0].ToString() == "RetireesItem" ? "BINARY BASE64)" : "TYPE)";
										getDataTablesSql += String.Format("(SELECT * FROM dbo.{0} FOR XML PATH('{0}'), " + rowType, dataRow.ItemArray[0], i);

										var fullSt = String.Format("{-4, 0}", getDataTablesSql);

//										print(fullSt if i == len(rows) else fullSt + ',', file = f)
//										print("FOR XML PATH('XpoDataSet')", file = f)
	//									SqlDataAdapter getTableAdapter = new SqlDataAdapter(String.Format(
	//										"SELECT * FROM dbo.{0}", dataRow.ItemArray[0]), objConn);
	//									getTableAdapter.Fill(ds);
	//									ds.WriteXml(@"D:\OutputXmlFile.xml");

									}
								}
							}
						}
					}

//					ds.WriteXml(@"D:\OutputXmlFile.xml");
				}
				//				XmlTextReader reader = new XmlTextReader(connParams.Value.Filename);
				//
				//				//				foreach (var VARIABLE in reader.ReadString())
				//				//				{
				//				//					
				//				//				}
				//
				//				while (reader.Read())
				//				{
				//					var str = reader.ReadInnerXml();
				//					Console.WriteLine(reader.Name);
				//				}
				//				string sConnectionString = String.Format("User ID={0};password={1};Initial Catalog={2};Data Source={3}",
				//					connParams.Value.Username, connParams.Value.Password, connParams.Value.Database, connParams.Value.DataSource);
				//				SqlConnection objConn = new SqlConnection(sConnectionString);
				//				objConn.Open();
				//				DataTable schemaTable = objConn.GetSchema();
				//				// Fill the DataTables.
				//				foreach (DataRow dataTableRow in schemaTable.Rows)
				//				{
				//					string tableName = dataTableRow["Table_Name"].ToString();
				//					// I seem to get an extra table starting with ~. I can't seem to screen it out based on information in schemaTable,
				//					// hence this hacky check.
				//					if (!tableName.StartsWith("~", StringComparison.InvariantCultureIgnoreCase))
				//					{
				//						//						FillTable(dataSet, conn, tableName);
				//					}
				//				}
				//				string sSQL = "Select TABLE_NAME FROM INFORMATION_SCHEMA.TABLES" + 
				//					" Where TABLE_TYPE = 'BASE TABLE'" + 
				//					" Order By TABLE_NAME;";
				//				DataSet dataSet = new DataSet();
				//				using (objConn)	
				//				{
				//					SqlDataAdapter adapter = new SqlDataAdapter(sSQL, objConn);
				//					adapter.Fill(dataSet);
				//					Console.WriteLine("Table names list obtained");
				//					foreach (DataTable table in dataSet.Tables)
				//					{
				//						string getDataTablesSql = "USE [" + connParams.Value.Database + "";
				//						foreach (DataRow dataRow in table.Rows)
				//						{
				//							Console.WriteLine(dataRow.ItemArray.GetValue(0));
				//						}
				//					}
				//				}

			}
			else
			{
				Console.WriteLine("Please, enter params for connection string");
			}
		}
	}
}

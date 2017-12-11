using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CommandLine;

namespace StoIX.Net
{
	class Program
	{
		static void Main(string[] args)
		{
			DataSet ds = new DataSet();
			IList<string> listElements = new List<string>();

			if (args.Length > 0)
			{
				var options = Parser.Default.ParseArguments<Options>(args);

				string sConnectionString = String.Format("User ID={0};password={1};Initial Catalog={2};Data Source={3}",
								options.Value.Username, options.Value.Password, options.Value.Database, options.Value.DataSource);
				using (SqlConnection connection = new SqlConnection(sConnectionString))
				{
					connection.Open();
					DataTable table = connection.GetSchema("Tables");
					DisplayData(ds, table, connection);
					Console.WriteLine("Press any key to continue.");
//					string sSQL = "Select TABLE_NAME FROM INFORMATION_SCHEMA.TABLES" +
//						" Where TABLE_TYPE = 'BASE TABLE'" +
//						" Order By TABLE_NAME;";
//
//					DataSet listTables = new DataSet();
//					SqlDataAdapter adapter = new SqlDataAdapter(sSQL, connection);
//					adapter.Fill(listTables);
//					Console.WriteLine("Table names list obtained");
//					foreach (DataTable table in listTables.Tables)
//					{
//						int i = 0;
//					}
//					DataTable schemaTable = connection.GetSchema();
//					// Fill the DataTables.
//					foreach (DataRow dataTableRow in schemaTable.Rows)
//					{
//						string tableName = dataTableRow["Table_Name"].ToString();
//						// I seem to get an extra table starting with ~. I can't seem to screen it out based on information in schemaTable,
//						// hence this hacky check.
//						if (!tableName.StartsWith("~", StringComparison.InvariantCultureIgnoreCase))
//						{
//							//						FillTable(dataSet, conn, tableName);
//						}
//					}
					ds.WriteXmlSchema(@"D:\OutputXmlFile.xml");
					ds.WriteXml(@"D:\OutputXmlFile.xml");
				}
			}
			else
			{
				Console.WriteLine("Please, enter params for connection string");
			}
		}

		private static void DisplayData(DataSet ds, DataTable table, SqlConnection conn)
		{
			foreach (DataRow row in table.Rows)
			{
				foreach (DataColumn col in table.Columns)
				{
					if (col.ColumnName == "TABLE_NAME")
					{
						SqlDataAdapter adapter = new SqlDataAdapter(String.Format("SELECT * FROM {0}", row[col]), conn);
						adapter.Fill(ds);
					}
					Console.WriteLine("{0} = {1}", col.ColumnName, row[col]);
				}
				Console.WriteLine("============================");
			}
		}
	}
}

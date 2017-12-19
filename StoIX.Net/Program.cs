using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CommandLine;

namespace StoIX.Net
{
	class Program
	{
		public static DataSet ds = new DataSet("XpoDataSet");
		static void Main(string[] args)
		{
			
			IList<string> listElements = new List<string>();

			if (args.Length > 0)
			{
				var options = Parser.Default.ParseArguments<Options>(args);

				string sConnectionString = String.Format("User ID={0};password={1};Initial Catalog={2};Data Source={3}",
								options.Value.Username, options.Value.Password, options.Value.Database, options.Value.DataSource);

				List<string> tabNames = LoadTablesToDataSet(sConnectionString);
				LoadRelationsToDataSet(sConnectionString, tabNames);

				ds.WriteXml(@"D:\OutputXmlFile.xml", XmlWriteMode.WriteSchema);
			}
			else
			{
				Console.WriteLine("Please, enter params for connection string");
			}
		}

		private static DataSet LoadRelationsToDataSet(string sConnectionString, List<string> tabNames)
		{
			DataSet dsRelaitons = new DataSet();
			using (SqlConnection connection = new SqlConnection(sConnectionString))
			{
				using (SqlCommand relationsSql = new SqlCommand(@"
SELECT
    fk.name 'FK Name',
    tp.name 'Parent table',
    cp.name, cp.column_id,
    tr.name 'Referenced table',
    cr.name, cr.column_id
FROM 
    sys.foreign_keys fk
INNER JOIN 
    sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN 
    sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN 
    sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN 
    sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN 
    sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
ORDER BY
    tp.name, cp.column_id", connection))
				{
					using (SqlDataAdapter relAdapter = new SqlDataAdapter(relationsSql))
					{
						DataTable relationsDt = new DataTable();
						relAdapter.Fill(relationsDt);
						foreach (var tableName in tabNames)
						{
							List<DataRow> childs =
								relationsDt.AsEnumerable().
								Where(pc => pc.Field<string>("Parent table") == tableName).
								ToList();
							List<DataColumn> parentColumns = new List<DataColumn>();
							List<DataColumn> childColumns = new List<DataColumn>();
							foreach (var childTableName in childs)
							{
								var childTbName = childTableName["Referenced table"].ToString();
								string parentColumnName = childTableName["name"].ToString();
								parentColumns.Add(ds.Tables[tableName].Columns[parentColumnName]);
								childColumns.Add(ds.Tables[childTbName].Columns["Oid"]);
								
								if (childColumns.Count > 0 && ds.Tables[tableName].Rows.Count > 0)
								{
									DataRelation dr = new DataRelation(
										String.Format("FK_{0}_{1}", tableName, parentColumnName),
										ds.Tables[tableName].Columns[parentColumnName],
										ds.Tables[childTbName].Columns["Oid"],
										false);
									ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint(
										String.Format("FK_{0}_{1}", tableName, parentColumnName), 
										ds.Tables[tableName].Columns[parentColumnName],
										ds.Tables[childTbName].Columns["Oid"]
										);
//									foreignKeyConstraint.DeleteRule = Rule.None;
									//									ds.Tables[tableName].ChildRelations.Add(dr);
//									CreateConstraint(ds, tableName, childTbName, parentColumnName, "Oid");
									ds.Relations.Add(dr);
									ds.Tables[childTbName].Constraints.Add(foreignKeyConstraint);
//									ds.EnforceConstraints = true;
								}
							}
						}

					}
				}
			}

			return dsRelaitons;
		}

		private static void CreateConstraint(DataSet dataSet,
		string table1, string table2, string column1, string column2)
		{
			// Declare parent column and child column variables.
			DataColumn parentColumn;
			DataColumn childColumn;
			ForeignKeyConstraint foreignKeyConstraint;

			// Set parent and child column variables.
			parentColumn = dataSet.Tables[table1].Columns[column1];
			childColumn = dataSet.Tables[table2].Columns[column2];
			foreignKeyConstraint = new ForeignKeyConstraint
				 (String.Format("FK_{0}_{1}", table1, column1), parentColumn, childColumn);

			// Set null values when a value is deleted.
			foreignKeyConstraint.DeleteRule = Rule.SetNull;
			foreignKeyConstraint.UpdateRule = Rule.Cascade;
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;

			// Add the constraint, and set EnforceConstraints to true.
			dataSet.Tables[table1].Constraints.Add(foreignKeyConstraint);
			dataSet.EnforceConstraints = true;
		}

		private static List<string> LoadTablesToDataSet(string connString)
		{
			List<string> tablesName = new List<string>();

			try
			{
				using (SqlConnection conn = new SqlConnection(connString)) // Opening the connection
				{
					conn.Open();
					DataTable dt = conn.GetSchema("Tables");// Getting all user tables present in the Database (Msys* tables are system thus useless for us)
					tablesName = dt.AsEnumerable().Select(dr => dr.Field<string>("TABLE_NAME")).Where(dr => !dr.StartsWith("MSys")).ToList();

					// Getting the data for every user tables
					foreach (string tableName in tablesName)
					{
						if (!string.IsNullOrEmpty(tableName))
						{
							using (SqlCommand cmd = new SqlCommand(string.Format("SELECT * FROM [{0}]", tableName), conn))
							{
								using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
								{
									// Saving all tables in DataSet.
									DataTable buf = new DataTable(tableName); //create empty table
//									adapter.FillSchema(buf, SchemaType.Source);
									adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
									adapter.Fill(buf); //fill table with data using the adapter
									ds.Tables.Add(buf);  //' Add table to the destination dataset
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error message: {0}", ex);
			}

			return tablesName;
		}
	}
}

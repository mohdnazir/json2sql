using json2sql.sqlpersister;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace json2sql.converter
{
    class JsonToTableConverter
    {
        JsonDataTable rootTable = new JsonDataTable("root");

        void AddDataRow(IEnumerable<JToken> arl, JsonDataTable table, int parentRowId, string parentTableName)
        {
            foreach (var item in arl)
            {
                if (item is IDictionary<string, JToken> childRecords)
                {
                    AddDataRow(childRecords, table, parentRowId, parentTableName);
                }
                else if (item.HasValues)
                {
                    // is IEnumerable<JToken> arlist
                    //Don't know what to do here????
                }
                else if (item != null)  //For plain array
                {
                    JsonDataRow row = new JsonDataRow(parentRowId);
                    row.Fields.Add(new Field() { FieldName = "Column1", FieldValue = item });
                    table.Rows.Add(row);
                    table.AddFieldForMetadata("_Id", row._Id, true);
                    table.AddFieldForMetadata(parentTableName + "_Id", row._ParentId, true);
                    table.AddFieldForMetadata("Column1", item, false);
                }
            }
        }

        void AddDataRow(IDictionary<string, JToken> root, JsonDataTable mainTable, int parentRowId, string parentTableName)
        {
            JsonDataRow row = new JsonDataRow(parentRowId);
            mainTable.Rows.Add(row);
            mainTable.AddFieldForMetadata("_Id", row._Id, true);
            mainTable.AddFieldForMetadata(parentTableName + "_Id", row._ParentId, true);

            foreach (var item in root)
            {
                if (item.Value is IDictionary<string, JToken> childDic)
                {
                    JsonDataTable childTable = AddNodeTable(childDic, item.Key, row._Id, mainTable.TableName);                 
                    mainTable.AddTable(item.Key, childTable);
                }
                else if (item.Value.HasValues)
                {
                    IEnumerable<JToken> childAry = item.Value;
                    JsonDataTable childTable = AddNodeTable(childAry, item.Key, row._Id, mainTable.TableName);
                    mainTable.AddTable(item.Key, childTable);
                }
                else
                {
                    mainTable.AddFieldForMetadata(item.Key, item.Value, false);
                    row.Fields.Add(new Field() { FieldName = item.Key, FieldValue = item.Value });
                }
            }
        }

        JsonDataTable AddNodeTable(IDictionary<string, JToken> childRows, string tableName, int parentRowId, string parentTableName)
        {
            JsonDataTable nodeTable = new JsonDataTable(tableName);
            nodeTable.ParentTableName = parentTableName;
            AddDataRow(childRows, nodeTable, parentRowId, parentTableName);
            return nodeTable;
        }

        JsonDataTable AddNodeTable(IEnumerable<JToken> childArray, string tableName, int parentRowId, string parentTableName)
        {
            JsonDataTable nodeTable = new JsonDataTable(tableName);
            nodeTable.ParentTableName = parentTableName;
            AddDataRow(childArray, nodeTable, parentRowId, parentTableName);
            return nodeTable;
        }
        void RemoveEmptyRootTable()
        {
            if (rootTable.Rows.DataRows.Count > 0
                && rootTable.Rows.DataRows[0].Fields.Count == 0
                && (rootTable.Tables.Count > 0
                    && rootTable.Tables.ContainsKey("root"))) //means root table is empty
            {
                rootTable = rootTable.Tables["root"];
                foreach (var row in rootTable.Rows.DataRows)
                {
                    row._ParentId = 0;  //reset parent id becuse parent table is removed
                }

                foreach (var fmd in rootTable.FieldMetaData)
                {
                    if (fmd.FieldName.ToLower() == "root_id")
                    {
                        fmd.FieldName = "Parent_Id";
                        break;
                    }
                } 
            }
        }
        public void Parse(IDictionary<string, JToken> root)
        {
            AddDataRow(root, rootTable, 0, "Parent");
            RemoveEmptyRootTable();
        }

        /// <summary>
        /// Return sql scripts in string
        /// </summary>
        /// <param name="sqlPersister">SQL stratagy</param>
        /// <returns></returns>
        public StringBuilder ToSqlScript(ISqlPersister sqlPersister)
        {
            return sqlPersister.ToSqlScript(rootTable);
        }

        /// <summary>
        /// Write SQL script to file and return file path
        /// </summary>
        /// <param name="sqlPersister">SQL Persisting</param>
        /// <param name="filePath">File path where output is written</param>
        /// <returns></returns>
        public string ToSqlScript(ISqlPersister sqlPersister, FilePath filePath)
        {
            return sqlPersister.ToSqlScript(rootTable, filePath);
        }
    }
}

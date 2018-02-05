using json2sql.converter;
using System;
using System.Linq;
using System.Text;

namespace json2sql.sqlpersister
{
    class SqlitePersister : SQLPersister
    {
        JsonDataTable jsonDataTable = null;
        public override string GetSchema()
        {
            throw new NotImplementedException();
        }
        public override StringBuilder ToSqlScript(JsonDataTable jsonDataTable)
        {
            this.jsonDataTable = jsonDataTable;
            StringBuilder stringBuilder = new StringBuilder();
            GenerateCreateTableScript(jsonDataTable, stringBuilder);
            GenerateInsertScript(jsonDataTable, stringBuilder);
            return stringBuilder;
        }

        public override string ToSqlScript(JsonDataTable jsonDataTable, FilePath filePath)
        {
            throw new NotImplementedException();
        }

        void GenerateCreateTableScript(JsonDataTable dt, StringBuilder sbScript)
        { 
            sbScript.AppendFormat("create table if not exists [{0}](", dt.TableName);
            sbScript.AppendLine();
            foreach (var fieldData in dt.FieldMetaData)
            {
                sbScript.AppendFormat("\t[{0}] {1} NULL,", fieldData.FieldName, SqlDataType(fieldData.DataType));
                sbScript.AppendLine();
            }
            sbScript.Length -= 3;   //remove last comma and \r\n
            sbScript.AppendLine();
            sbScript.Append(")");
            if (dt.Tables.Count > 0)
            {
                foreach (var table in dt.Tables)
                {
                    sbScript.AppendLine();
                    sbScript.AppendFormat("------------ Table : \"{0}\" ------------", table.Key);
                    sbScript.AppendLine();
                    GenerateCreateTableScript(table.Value, sbScript);
                }
            }
            sbScript.AppendLine();
        }

        void GenerateInsertScript(JsonDataTable dt, StringBuilder sbScript)
        {
            sbScript.AppendFormat("------------ Begin Table \"{0}\" Rows ({1}) ------------", dt.TableName, dt.Rows.Count);
            sbScript.AppendLine();

            foreach (var row in dt.Rows.DataRows)
            {
                sbScript.AppendFormat("insert into [{0}](", dt.TableName);
                foreach (var fieldData in dt.FieldMetaData)
                {
                    sbScript.AppendFormat("[{0}],", fieldData.FieldName);
                }
                sbScript.Length -= 1;
                sbScript.Append(")").AppendLine();
                sbScript.Append("\tvalues(");

                sbScript.AppendFormat("{0},", row._Id);
                sbScript.AppendFormat("{0},", row._ParentId);

                var flds = from m in dt.FieldMetaData
                           join r in row.Fields on m.FieldName equals r.FieldName into ps
                           from r in ps.DefaultIfEmpty()
                           where m.IsIdentityField == false
                           select new
                           {
                               FieldName = m.FieldName,
                               DataType = m.DataType,
                               DateCulture = m.DateCulture,
                               FieldValue = r?.FieldValue   //use null propogation
                           };

                foreach (var field in flds)
                {
                    var val = DecideFieldValue(
                        new Field() { FieldName = field.FieldName, FieldValue = field.FieldValue },
                        dt.FieldMetaData);
                    sbScript.AppendFormat("{1}{0}{1},", val, GetFieldDataTypeDelimit(
                        new Field() { FieldName = field.FieldName, FieldValue = field.FieldValue },
                        dt.FieldMetaData));
                }
                sbScript.Length -= 1;   //remove last comma
                sbScript.Append(")").AppendLine();
            }
            sbScript.AppendFormat("------------ End Table \"{0}\" Rows ({1}) ------------", dt.TableName, dt.Rows.Count);
            sbScript.AppendLine().AppendLine();

            if (dt.Tables.Count > 0)    //Iterate into child tables
            {
                foreach (var table in dt.Tables)
                {
                    GenerateInsertScript(table.Value, sbScript);
                }
            }
        }
        string SqlDataType(FieldDataType fdt)
        {
            switch (fdt)
            {
                case FieldDataType.Integer:
                    return "INTEGER";
                case FieldDataType.Double:
                    return "REAL";
                case FieldDataType.DateTime:
                case FieldDataType.String:
                    return "TEXT";
                default:
                    return "TEXT";
            }
        }

        string SqlDataDelimit(FieldDataType fdt)
        {
            switch (fdt)
            {
                case FieldDataType.Integer:
                case FieldDataType.Double:
                    return "";
                case FieldDataType.DateTime:
                case FieldDataType.String:
                    return "'";
                default:
                    return "'";
            }
        }

        string GetFieldDataTypeDelimit(Field field, FieldMetaData[] fieldMetaData)
        {
            if (field.FieldValue == null) return "";
            var fmd = fieldMetaData.Where(a => a.FieldName == field.FieldName).FirstOrDefault();
            return SqlDataDelimit(fmd.DataType);
        }

        string DecideFieldValue(Field field, FieldMetaData[] fieldMetaData)
        {
            if (field.FieldValue == null)
                return "null";

            var val = field.FieldValue.ToString();            
            var fmd = fieldMetaData.Where(a => a.FieldName == field.FieldName).FirstOrDefault();
            if (val == string.Empty && (fmd.DataType == FieldDataType.Integer || fmd.DataType == FieldDataType.Double))
                val = "0";
            else
                val = val.Replace("'", "''");

            return val;
        }

    }
}

using json2sql.converter;
using System;
using System.Linq;
using System.Text;

namespace json2sql.sqlpersister
{
    class OraclePersister : SQLPersister
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

        private void GenerateCreateTableScript(JsonDataTable dt, StringBuilder sbScript)
        {
            sbScript.AppendFormat("CREATE TABLE {0}(", dt.TableName.Trim().Replace(' ', '_'));
            sbScript.AppendLine();
            foreach (var fieldData in dt.FieldMetaData)
            {
                sbScript.AppendFormat("\t{0} {1} NULL,", DecideFieldName(fieldData), OracleDataType(fieldData.DataType));
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
        private void GenerateInsertScript(JsonDataTable dt, StringBuilder sbScript)
        {
            sbScript.AppendFormat("------------ Begin Table \"{0}\" Rows ({1}) ------------", dt.TableName, dt.Rows.Count);
            sbScript.AppendLine();

            foreach (var row in dt.Rows.DataRows)
            {
                sbScript.AppendFormat("INSERT INTO {0}(", dt.TableName.Trim().Replace(' ', '_'));
                foreach (var fieldData in dt.FieldMetaData)
                {
                    sbScript.AppendFormat("{0},", DecideFieldName(fieldData));
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
        string OracleDataType(FieldDataType fdt)
        {
            switch (fdt)
            {
                case FieldDataType.Integer:
                    return "NUMBER";
                case FieldDataType.Double:
                    return "LONG";
                case FieldDataType.DateTime:
                    return "DATE";
                case FieldDataType.String:
                    return "VARCHAR2(4000)";
                default:
                    return "VARCHAR2(4000)";
            }
        }

        string OracleDataDelimit(FieldDataType fdt)
        {
            switch (fdt)
            {
                case FieldDataType.Integer:
                case FieldDataType.Double:
                case FieldDataType.DateTime:
                    return "";
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
            return OracleDataDelimit(fmd.DataType);
        }
        string DecideFieldValue(Field field, FieldMetaData[] fieldMetaData)
        {
            if (field.FieldValue == null)
                return "null";

            var val = field.FieldValue.ToString();
            var fmd = fieldMetaData.Where(a => a.FieldName == field.FieldName).FirstOrDefault();
            if (val == string.Empty && (fmd.DataType == FieldDataType.Integer || fmd.DataType == FieldDataType.Double))
                val = "0";
            else if (fmd.DataType == FieldDataType.DateTime)
                val = string.Format("TO_DATE('{0}','YYYY-MM-DD HH24:MI:SS')", field.FieldValue);
            else
                val = val.Replace("'", "''");

            return val;
        }
        string DecideFieldName(FieldMetaData field)
        {
            var fldName = field.FieldName.StartsWith('_') ? "C" + field.FieldName : field.FieldName;
            fldName = field.FieldName.ToUpper() == "DATE" ? "C" + fldName : fldName;
            return fldName.Trim().Replace(' ', '_');
        }
    }
}
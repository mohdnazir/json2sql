using System;
using System.Collections.Generic;
using System.Linq;

namespace json2sql.converter
{
    class JsonDataTable
    {
        List<FieldMetaData> fieldList = new List<FieldMetaData>();
        Dictionary<string, JsonDataTable> tables;
        public JsonDataTable()
        {
            Rows = new JsonDataRowCollection();
            tables = new Dictionary<string, JsonDataTable>();
            fieldList = new List<FieldMetaData>();
            ParentTableName = "Parent";
        }

        public JsonDataTable(string name) : this()
        {
            TableName = name;
        }
        public string TableName { get; set; }
        public JsonDataRowCollection Rows { get; set; }
        public Dictionary<string, JsonDataTable> Tables { get { return tables; } }
        public void AddTable(string tableName, JsonDataTable table)
        {
            if (tables.ContainsKey(tableName))
            {
                var existingTable = tables[tableName];
                foreach (var row in table.Rows.DataRows)
                {
                    row.IsAddedInCollection = false;
                    existingTable.Rows.Add(row);
                }
                foreach (var meta in table.FieldMetaData)
                {
                    var missingfield = existingTable.FieldMetaData.FirstOrDefault(a => meta.FieldName == a.FieldName);
                    if (missingfield == null)
                    {
                        existingTable.AddFieldForMetadata(meta.FieldName, meta.FieldValue, meta.IsIdentityField);
                    }
                }
            }
            else
                tables.Add(tableName, table);
        }
        public void AddFieldForMetadata(string fieldName, object fieldValue, bool isIdentityField)
        {
            FieldMetaData fieldMetaData = fieldList.Where(field => field.FieldName == fieldName).FirstOrDefault();
            if (fieldMetaData == null)
            {
                fieldMetaData = new FieldMetaData(fieldName);
                fieldMetaData.IsIdentityField = isIdentityField;
                fieldList.Add(fieldMetaData);
            }
            fieldMetaData.Value(fieldValue);
        }
        public FieldMetaData[] FieldMetaData
        {
            get { return fieldList.ToArray(); }
        }
        public string ParentTableName { get; set; }
    }

    class JsonDataRowCollection
    {
        List<JsonDataRow> rows;
        int _rowid = 0;
        public JsonDataRowCollection()
        {
            rows = new List<JsonDataRow>();
        }
        public void Add(JsonDataRow row)
        {
            if (!row.IsAddedInCollection)
            {
                _rowid++;
                row._Id = _rowid;
                rows.Add(row);
                row.IsAddedInCollection = true;
            }
        }
        public List<JsonDataRow> DataRows { get { return rows; } }
        public int Count { get { return rows.Count; } }

    }
    class JsonDataRow
    {
        public JsonDataRow()
        {
            Fields = new List<Field>();
        }

        public JsonDataRow(int parentRowId) : this()
        {
            _ParentId = parentRowId;
        }

        public int _Id { get; set; }
        public int _ParentId { get; set; }
        public List<Field> Fields { get; set; }
        public bool IsAddedInCollection { get; set; }
    }

    class Field
    {
        public Field()
        {

        }
        public Field(string name)
        {
            FieldName = name;
        }
        public string FieldName { get; set; }

        public object FieldValue { get; set; }
    }

    enum FieldDataType
    {
        Integer, Double, DateTime, String
    }

    class FieldMetaData
    {
        int enGBDateTypeCount;
        int enUSDateTypeCount;
        int numberTypeCount;
        int stringTypeCount;
        object fieldValue;
        int dateTypeCount;
        bool isDecimal;

        public FieldMetaData()
        {

        }
        public FieldMetaData(string fieldName)
        {
            FieldName = fieldName;
        }
        public string FieldName { get; set; }
        public object FieldValue { get { return fieldValue; } }
        public bool IsIdentityField { get; set; }
        public FieldDataType DataType { get { return GetDataType(); } }
        public string DateCulture
        {
            get
            {
                return enGBDateTypeCount > enUSDateTypeCount
                    ? "en-GB"
                    : enUSDateTypeCount > enGBDateTypeCount
                        ? "en-US"
                        : "other";
            }
        }
        public void Value(object fieldValue)
        {
            string value = Convert.ToString(fieldValue);
            this.fieldValue = fieldValue;

            Boolean nParsed = Decimal.TryParse(value, out decimal nData);
            bool dGBFound = false, dUSFound = false;

            if (nParsed)
            {
                numberTypeCount++;
                if (!isDecimal && value.Contains("."))
                    isDecimal = true;
            }
            if (DateTime.TryParse(value, System.Globalization.CultureInfo.GetCultureInfo("en-GB").DateTimeFormat, System.Globalization.DateTimeStyles.None, out DateTime date2))
            {
                enGBDateTypeCount++;
                dGBFound = true;
            }

            if (DateTime.TryParse(value, System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat, System.Globalization.DateTimeStyles.None, out date2))
            {
                enUSDateTypeCount++;
                dUSFound = true;
            }

            if (dGBFound || dUSFound)
            {
                dateTypeCount++;
            }
            else if (!nParsed)
            {
                // Consider string (by default)
                if (value.Trim().Length > 0)
                {
                    stringTypeCount++;
                }
            }
        }

        FieldDataType GetDataType()
        {
            FieldDataType dataType = FieldDataType.String;
            if (stringTypeCount > 0)
            {
                dataType = FieldDataType.String;
            }

            //if occurance of date found more than any other data type then consider it date because it is not possible 
            //to convert all occurance of date to datetime type bedause of format privded
            if (dateTypeCount > numberTypeCount && dateTypeCount > stringTypeCount)
                dataType = FieldDataType.DateTime;
            else
            {
                if (numberTypeCount > 0 && dateTypeCount > 0)
                    dataType = FieldDataType.Integer;
                else
                {
                    if (numberTypeCount > 0 && dateTypeCount == 0 && stringTypeCount == 0)
                        dataType = isDecimal ? FieldDataType.Double : FieldDataType.Integer;
                    else
                        dataType = FieldDataType.String;
                }
            }

            return dataType;
        }
    }
}


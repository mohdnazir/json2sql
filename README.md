# json2sql
JSON to SQL converter, written in C# dot.net core, to create relational model and create SQL scripts for multiple databases. 

This is capable to determine the datatype from JSON and create appropriate data model in sql. It suports the conversion into 
following SQL Script:

## Suported SQL Scripts ##
1. MSSQL
1. MYSQL
1. Oracle
1. SQLite

## Implementation ##

**Uploaded file or refer file in a folder**
```C#
  Converter("FileUpload", "C:\json2sql\SampleJSON\sales.json", Convertertype.mssql);
```

** JSON Data **
```C#
  var json = "[ 100, 500, 300, 200, 400 ]";
  Converter("RawData", "json", Convertertype.mssql);
```

** URL **
```C#
  Converter("URL", "http://mywebsite.com/data.json", Convertertype.mssql);
```

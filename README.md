# json2sql
JSON to SQL converter, written in C# and dot.net core, creates relational model and create SQL scripts for multiple databases. 

This is capable to determine the datatype from JSON and create appropriate data model in sql. It suports the conversion into 
following SQL Script:

## Suported SQL Scripts ##
1. MSSQL
1. MYSQL
1. Oracle
1. SQLite

## Implementation ##

**Uploaded file to a folder than refer uploaded file**
```C#
  Converter("FileUpload", "C:\json2sql\SampleJSON\sales.json", Convertertype.mssql);
```

**JSON Data**
```C#
  var json = "[ 100, 500, 300, 200, 400 ]";
  Converter("RawData", json, Convertertype.mssql);
```

**URL**
```C#
  Converter("URL", "http://mywebsite.com/data.json", Convertertype.mssql);
```

### website link ###
An online implementation is available at [Converter](http://itarchitectman.com/)

## Output ##
```SQL
--SQL Table Script
if not exists(select * from sysobjects where name = 'root' and xtype = 'U')
	create table [root](
		[_Id] int NULL,
		[Parent_Id] int NULL,
		[DATE] datetime NULL,
		[OrderId] varchar(max) NULL,
		[OrderItemID] varchar(max) NULL,
		[SKUCode] varchar(max) NULL,
		[BuyerName] varchar(max) NULL,
		[PinCode] int NULL,
		[whid] varchar(max) NULL,
		[total_items] int NULL,
		[grand_total] real NULL,
		[sale_status] varchar(max) NULL,
		[sellerId] varchar(max) NULL
	)
------------ Begin Table "root" Rows (3) ------------
insert into [root]([_Id],[Parent_Id],[DATE],[OrderId],[OrderItemID],[SKUCode],[BuyerName],[PinCode],[whid],[total_items],[grand_total],[sale_status],[sellerId])
	values(1,0,'1970-01-01 00:00:00','','','','',0,'',1,0.00,'','A2O1YUSSUS')
insert into [root]([_Id],[Parent_Id],[DATE],[OrderId],[OrderItemID],[SKUCode],[BuyerName],[PinCode],[whid],[total_items],[grand_total],[sale_status],[sellerId])
	values(2,0,'2017-05-31 19:33:53','404-5146680-2910706','404-5146680-2910706-MICROMAX156','MICROMAX156','',201010,'',1,0.00,'Cancelled','A2O1YUSSUS')
insert into [root]([_Id],[Parent_Id],[DATE],[OrderId],[OrderItemID],[SKUCode],[BuyerName],[PinCode],[whid],[total_items],[grand_total],[sale_status],[sellerId])
	values(3,0,'2017-05-31 18:34:38','403-0345266-3534761','55791338810427','ZEBU725','Lei Bramley',560066,'QSIP',1,1998.00,'Shipped','A2O1YUSSUS')
------------ End Table "root" Rows (3) ------------
```

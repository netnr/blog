#if Full || DataKit

namespace Netnr.SharedDataKit
{
    /// <summary>
    /// 数据交互脚本
    /// </summary>
    public partial class DataKitScript
    {
        /// <summary>
        /// 获取库名
        /// </summary>
        /// <param name="tdb"></param>
        /// <returns></returns>
        public static string GetDatabaseName(SharedEnum.TypeDB tdb)
        {
            string result = null;

            switch (tdb)
            {
                case SharedEnum.TypeDB.SQLite:
                    result = "PRAGMA database_list";
                    break;
                case SharedEnum.TypeDB.MySQL:
                case SharedEnum.TypeDB.MariaDB:
                    result = "SELECT SCHEMA_NAME AS DatabaseName FROM information_schema.schemata ORDER BY SCHEMA_NAME";
                    break;
                case SharedEnum.TypeDB.Oracle:
                    result = "SELECT USERNAME AS DatabaseName FROM ALL_USERS ORDER BY USERNAME";
                    break;
                case SharedEnum.TypeDB.SQLServer:
                    result = "SELECT name AS DatabaseName FROM sys.databases ORDER BY name";
                    break;
                case SharedEnum.TypeDB.PostgreSQL:
                    result = "SELECT datname AS DatabaseName FROM pg_database ORDER BY datname";
                    break;
            }

            return result;
        }

        /// <summary>
        /// 获取库
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="listDatabaseName"></param>
        /// <returns></returns>
        public static string GetDatabase(SharedEnum.TypeDB tdb, IList<string> listDatabaseName = null)
        {
            string result = null;

            string where = string.Empty;
            if (listDatabaseName?.Count > 0)
            {
                where = "AND {0} IN ('" + string.Join("','", listDatabaseName) + "')";
            }

            switch (tdb)
            {
                case SharedEnum.TypeDB.SQLite:
                    result = "PRAGMA database_list;PRAGMA encoding";
                    break;
                case SharedEnum.TypeDB.MySQL:
                case SharedEnum.TypeDB.MariaDB:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "t1.SCHEMA_NAME");
                    }

                    result = $@"
SELECT
  t1.SCHEMA_NAME AS DatabaseName,
  t1.DEFAULT_CHARACTER_SET_NAME AS DatabaseCharset,
  t1.DEFAULT_COLLATION_NAME AS DatabaseCollation,
  @@datadir AS DatabasePath,
  t2.DatabaseDataLength,
  t2.DatabaseIndexLength,
  t2.DatabaseCreateTime
FROM
  information_schema.schemata t1
  LEFT JOIN (
    SELECT
      TABLE_SCHEMA,
      SUM(DATA_LENGTH) AS DatabaseDataLength,
      SUM(INDEX_LENGTH) AS DatabaseIndexLength,
      MIN(CREATE_TIME) AS DatabaseCreateTime
    FROM
      information_schema.tables
    GROUP BY
      TABLE_SCHEMA
  ) t2 ON t1.SCHEMA_NAME = t2.TABLE_SCHEMA
WHERE
  1 = 1 {where}
";
                    break;
                case SharedEnum.TypeDB.Oracle:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "t1.USERNAME");
                    }

                    result = $@"
SELECT
  t1.USERNAME AS DatabaseName,
  t1.USERNAME AS DatabaseOwner,
  t1.DEFAULT_TABLESPACE AS DatabaseSpace,
  dp1.value AS DatabaseCharset,
  dp2.value AS DatabaseCollation,
  f1.FILE_NAME AS DatabasePath,
  s1.DatabaseDataLength,
  S2.DatabaseIndexLength,
  t1.CREATED AS DatabaseCreateTime
FROM
  DBA_USERS t1
  LEFT JOIN nls_database_parameters dp1 ON dp1.parameter = 'NLS_CHARACTERSET'
  LEFT JOIN nls_database_parameters dp2 ON dp2.parameter = 'NLS_SORT'
  LEFT JOIN DBA_DATA_FILES f1 ON f1.TABLESPACE_NAME = t1.DEFAULT_TABLESPACE
  LEFT JOIN (
    SELECT
      OWNER,
      SUM(BYTES) AS DatabaseDataLength
    FROM
      DBA_SEGMENTS
    WHERE
      SEGMENT_TYPE IN('TABLE', 'NESTED TABLE', 'LOBSEGMENT')
    GROUP BY
      OWNER
  ) s1 ON s1.OWNER = t1.USERNAME
  LEFT JOIN (
    SELECT
      OWNER,
      SUM(BYTES) AS DatabaseIndexLength
    FROM
      DBA_SEGMENTS
    WHERE
      SEGMENT_TYPE NOT IN('TABLE', 'NESTED TABLE', 'LOBSEGMENT')
    GROUP BY
      OWNER
  ) s2 ON s2.OWNER = t1.USERNAME
WHERE
  1 = 1 {where}
ORDER BY
  t1.USERNAME
";
                    break;
                case SharedEnum.TypeDB.SQLServer:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "t1.name");
                    }

                    result = $@"
SELECT
  t1.name AS DatabaseName,
  t4.name AS DatabaseOwner,
  t1.collation_name AS DatabaseCollation,
  t2.physical_name AS DatabasePath,
  t3.physical_name AS DatabaseLogPath,
  (
    SELECT
      sum(CONVERT(bigint, f0.[size])) * 8 * 1024
    FROM
      sys.master_files f0
    WHERE
      f0.database_id = t1.database_id
      AND f0.[type] = 0
  ) AS DatabaseDataLength,
  (
    SELECT
      sum(CONVERT(bigint, f1.[size])) * 8 * 1024
    FROM
      sys.master_files f1
    WHERE
      f1.database_id = t1.database_id
      AND f1.[type] = 1
  ) AS DatabaseLogLength,
  t1.create_date AS DatabaseCreateTime
FROM
  sys.databases t1
  LEFT JOIN sys.master_files t2 ON t2.database_id = t1.database_id
  LEFT JOIN sys.master_files t3 ON t3.database_id = t1.database_id
  left join sys.server_principals t4 on t1.owner_sid = t4.sid
WHERE
  t2.[type] = 0
  AND t3.[type] = 1 {where}
ORDER BY
  t1.name
            ";
                    break;
                case SharedEnum.TypeDB.PostgreSQL:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "t1.datname");
                    }

                    result = $@"
SELECT
  t1.datname AS DatabaseName,
  pg_get_userbyid(t1.datdba) AS DatabaseOwner,
  t2.spcname AS DatabaseSpace,
  pg_encoding_to_char(t1.encoding) AS DatabaseCharset,
  t1.datcollate AS DatabaseCollation,
  (
    SELECT
      setting
    FROM
      pg_settings
    WHERE
      NAME = 'data_directory'
  ) AS DatabasePath,
  pg_catalog.pg_database_size(t1.oid) AS DatabaseDataLength
FROM
  pg_database t1
  LEFT JOIN pg_tablespace t2 ON t1.dattablespace = t2.oid
WHERE
  1 = 1 {where}
ORDER BY
  t1.datname
";
                    break;
            }

            return result;
        }

        /// <summary>
        /// 获取表
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="databaseName">数据库名</param>
        /// <param name="schemaName">模式名</param>
        /// <returns></returns>
        public static string GetTable(SharedEnum.TypeDB tdb, string databaseName, string schemaName)
        {
            string result = null;

            string where = string.Empty;
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                where = "AND {0} = '" + schemaName.OfSql() + "'";
            }

            switch (tdb)
            {
                case SharedEnum.TypeDB.SQLite:
                    result = $@"SELECT tbl_name AS TableName FROM {databaseName}.sqlite_master WHERE type = 'table' ORDER BY tbl_name"; ;
                    break;
                case SharedEnum.TypeDB.MySQL:
                case SharedEnum.TypeDB.MariaDB:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "TABLE_SCHEMA");
                    }

                    result = $@"
SELECT
  TABLE_NAME AS TableName,
  TABLE_SCHEMA AS SchemaName,
  TABLE_TYPE AS TableType,
  ENGINE AS TableEngine,
  TABLE_ROWS AS TableRows,
  DATA_LENGTH AS TableDataLength,
  INDEX_LENGTH AS TableIndexLength,
  CREATE_TIME AS TableCreateTime,
  TABLE_COLLATION AS TableCollation,
  TABLE_COMMENT AS TableComment
FROM
  information_schema.tables
WHERE
  TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA = '{databaseName}' {where}
ORDER BY
  TABLE_NAME
";
                    break;
                case SharedEnum.TypeDB.Oracle:
                    result = $@"
SELECT
  t1.TABLE_NAME AS TableName,
  t1.OWNER AS TableOwner,
  t1.TABLESPACE_NAME AS TableSpace,
  t2.TABLE_TYPE AS TableType,
  t1.NUM_ROWS AS TableRows,
  t4.BYTES AS TableDataLength,
  t5.TableIndexLength,
  t3.CREATED AS TableCreateTime,
  COALESCE(m3.TIMESTAMP, t3.CREATED) AS TableModifyTime,
  t2.COMMENTS AS TableComment
FROM
  ALL_TABLES t1
  LEFT JOIN ALL_TAB_COMMENTS t2 ON t1.OWNER = t2.OWNER
  AND t1.TABLE_NAME = t2.TABLE_NAME
  LEFT JOIN ALL_OBJECTS t3 ON t1.OWNER = t3.OWNER
  AND t3.OBJECT_TYPE = t2.TABLE_TYPE
  AND t3.OBJECT_NAME = t1.TABLE_NAME
  LEFT JOIN all_tab_modifications m3 ON t1.OWNER = m3.TABLE_OWNER
  AND m3.TABLE_NAME = t1.TABLE_NAME
  LEFT JOIN DBA_SEGMENTS t4 ON t1.OWNER = t4.OWNER
  AND t1.TABLE_NAME = t4.SEGMENT_NAME
  AND t4.SEGMENT_TYPE = 'TABLE'
  LEFT JOIN (
    SELECT
      p1.OWNER,
      p1.TABLE_NAME,
      SUM(p2.BYTES) TableIndexLength
    FROM
      DBA_INDEXES p1
      LEFT JOIN DBA_SEGMENTS p2 ON p1.OWNER = p2.OWNER
      AND p1.INDEX_NAME = p2.SEGMENT_NAME
    GROUP BY
      P1.OWNER,
      p1.TABLE_NAME
  ) t5 ON t1.OWNER = t5.OWNER
  AND t1.TABLE_NAME = t5.TABLE_NAME
WHERE
  1 = 1 AND t1.OWNER = '{databaseName}'
ORDER BY
  t1.TABLE_NAME
";
                    break;
                case SharedEnum.TypeDB.SQLServer:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "SCHEMA_NAME(o.schema_id)");
                    }

                    result = $@"
USE [{databaseName}];
SELECT
  o.name AS TableName,
  (
    select
      sp.name
    from
      sys.databases db
      left join sys.server_principals sp on db.owner_sid = sp.sid
    where
      db.name = '{databaseName}'
  ) AS TableOwner,
  SCHEMA_NAME(o.schema_id) AS SchemaName,
  CASE
    o.type
    WHEN 'U' THEN 'BASE TABLE'
    WHEN 'V' THEN 'VIEW'
    ELSE o.type
  END AS TableType,
  m1.TableRows,
  m1.TableDataLength,
  m2.TableIndexLength,
  o.create_date AS TableCreateTime,
  o.modify_date AS TableModifyTime,
  ep.value AS TableComment
FROM
  sys.objects o
  LEFT JOIN sys.extended_properties ep ON ep.major_id = o.object_id
  AND ep.minor_id = 0
  LEFT JOIN (
    SELECT
      t.object_id,
      p.rows AS TableRows,
      SUM(a.total_pages) * 8 * 1024 AS TableDataLength
    FROM
      sys.tables t
      INNER JOIN sys.indexes i ON t.object_id = i.object_id
      INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID
      AND i.index_id = p.index_id
      INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
    GROUP BY
      t.object_id,
      p.rows
  ) m1 ON o.object_id = m1.object_id
  LEFT JOIN (
    SELECT
      object_id,
      SUM([used_page_count]) * 8 * 1024 AS TableIndexLength
    FROM
      sys.dm_db_partition_stats
    GROUP BY
      object_id
  ) m2 ON o.object_id = m2.object_id
WHERE
  o.type IN ('U', 'V') {where}
ORDER BY
  SCHEMA_NAME(o.schema_id),
  o.name
            ";
                    break;
                case SharedEnum.TypeDB.PostgreSQL:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "t1.table_schema");
                    }

                    result = $@"
SELECT
  t1.table_name AS TableName,
  t1.table_schema AS SchemaName,
  t2.tableowner AS TableOwner,
  t2.tablespace AS TableSpace,
  t1.table_type AS TableType,
  t4.reltuples AS TableRows,
  pg_relation_size(t4.oid) AS TableDataLength,
  pg_indexes_size(t4.oid) AS TableIndexLength,
  obj_description(t4.oid) AS TableComment
FROM
  information_schema.tables t1
  LEFT JOIN pg_tables t2 ON t1.table_name = t2.tablename
  AND t1.table_schema = t2.schemaname
  LEFT JOIN pg_namespace t3 ON t1.table_schema = t3.nspname
  LEFT JOIN pg_class t4 ON t3.oid = t4.relnamespace
  AND t1.table_name = t4.relname
WHERE
  t1.table_type = 'BASE TABLE'
  AND t1.table_schema NOT IN('pg_catalog', 'information_schema') {where}
ORDER BY
  t1.table_schema,
  t1.table_name
";
                    break;
            }

            return result;
        }

        /// <summary>
        /// 表DLL
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GetTableDDL(SharedEnum.TypeDB tdb, string databaseName, string schemaName, string tableName)
        {
            string result = null;

            switch (tdb)
            {
                case SharedEnum.TypeDB.SQLite:
                    result = $@"SELECT type, tbl_name, sql FROM {databaseName}.sqlite_master WHERE tbl_name = '{tableName}' ORDER BY type DESC";
                    break;
                case SharedEnum.TypeDB.MySQL:
                case SharedEnum.TypeDB.MariaDB:
                    result = $"SHOW CREATE TABLE `{databaseName}`.`{tableName}`";
                    break;
                case SharedEnum.TypeDB.Oracle:
                    {
                        var list = new List<string>
                        {
                            $"SELECT DBMS_METADATA.GET_DDL('TABLE', '{tableName}','{databaseName}') AS ddl_table FROM DUAL",
                            $"SELECT DBMS_METADATA.GET_DDL('INDEX', INDEX_NAME, '{databaseName}') AS ddl_index FROM ALL_INDEXES WHERE OWNER = '{databaseName}' AND TABLE_NAME = '{tableName}'",
                            $"SELECT DBMS_METADATA.GET_DDL('CONSTRAINT', CONSTRAINT_NAME) AS ddl_check FROM ALL_CONSTRAINTS WHERE OWNER = '{databaseName}' AND TABLE_NAME = '{tableName}' ORDER BY CONSTRAINT_TYPE DESC",
                            $"SELECT COMMENTS FROM ALL_TAB_COMMENTS WHERE OWNER = '{databaseName}' AND TABLE_NAME = '{tableName}'",
                            $"SELECT COLUMN_NAME, COMMENTS FROM ALL_COL_COMMENTS WHERE OWNER = '{databaseName}' AND TABLE_NAME = '{tableName}'"
                        };

                        var sql = "BEGIN\n";
                        for (int i = 0; i < list.Count; i++)
                        {
                            sql += $"\tOPEN :o{i} FOR {list[i]};\n";
                        }
                        sql += "END;";

                        result = sql;
                    }
                    break;
                case SharedEnum.TypeDB.SQLServer:
                    // http://www.stormrage.com/SQLStuff/sp_GetDDL_Latest.txt
                    result = $@"
USE [{databaseName}];

DECLARE @TBL VARCHAR(255) = '{schemaName}.[{tableName}]'

DECLARE @TBLNAME varchar(200),
        @SCHEMANAME varchar(255),
        @STRINGLEN int,
        @TABLE_ID int,
        @FINALSQL varchar(max),
        @CONSTRAINTSQLS varchar(max),
        @CHECKCONSTSQLS varchar(max),
        @RULESCONSTSQLS varchar(max),
        @FKSQLS varchar(max),
        @TRIGGERSTATEMENT varchar(max),
        @EXTENDEDPROPERTIES varchar(max),
        @INDEXSQLS varchar(max),
        @MARKSYSTEMOBJECT varchar(max),
        @vbCrLf char(2),
        @ISSYSTEMOBJECT int,
        @input varchar(max),
        @ObjectTypeFound varchar(255),
        @ObjectDataTypeLen int;
-- ####################
-- INITIALIZE
-- ####################
SET @input = '';
--does the tablename contain a schema?
SET @vbCrLf = CHAR(10);
SELECT
  @SCHEMANAME = ISNULL(PARSENAME(@TBL, 2), 'dbo'),
  @TBLNAME = PARSENAME(@TBL, 1);
SELECT
  @TBLNAME = [OBJS].[name],
  @TABLE_ID = [OBJS].[object_id]
FROM [sys].[objects] [OBJS]
WHERE [OBJS].[type] IN ('S', 'U')
AND [OBJS].[name] <> 'dtproperties'
AND [OBJS].[name] = @TBLNAME
AND [OBJS].[schema_id] = SCHEMA_ID(@SCHEMANAME);
SELECT
  @ObjectDataTypeLen = MAX(LEN([name]))
FROM [sys].[types];
-- ####################
-- Valid Table, Continue Processing
-- ####################
SELECT
  @FINALSQL
  = 'IF OBJECT_ID(''' + QUOTENAME(@SCHEMANAME) + '.' + QUOTENAME(@TBLNAME) + ''') IS NOT NULL ' + @vbcrlf
  + 'DROP TABLE ' + QUOTENAME(@SCHEMANAME) + '.' + QUOTENAME(@TBLNAME) + ';' + @vbcrlf + @vbcrlf
  + 'CREATE TABLE ' + QUOTENAME(@SCHEMANAME) + '.' + QUOTENAME(@TBLNAME) + ' ( ';
--removed invalid code here which potentially selected wrong table--thanks David Grifiths @SSC!
SELECT
  @STRINGLEN = MAX(LEN([COLS].[name])) + 1
FROM [sys].[objects] [OBJS]
INNER JOIN [sys].[columns] [COLS]
  ON [OBJS].[object_id] = [COLS].[object_id]
  AND [OBJS].[object_id] = @TABLE_ID;
-- ####################
--Get the columns, their definitions and defaults.
-- ####################
SELECT
  @FINALSQL
  = @FINALSQL
  + CASE
    WHEN [COLS].[is_computed] = 1 THEN @vbCrLf + QUOTENAME([COLS].[name]) + ' AS '
      + ISNULL([CALC].[definition], '') + CASE
        WHEN [CALC].[is_persisted] = 1 THEN ' PERSISTED'
        ELSE ''
      END
    ELSE @vbCrLf + QUOTENAME([COLS].[name]) + ' ' + SPACE(@STRINGLEN - LEN([COLS].[name]))
      + UPPER(TYPE_NAME([COLS].[user_type_id]))
      + CASE
        -- data types with precision and scale  IE DECIMAL(18,3), NUMERIC(10,2)
        WHEN TYPE_NAME([COLS].[user_type_id]) IN ('decimal', 'numeric') THEN '(' + CONVERT(varchar, [COLS].[precision]) + ',' + CONVERT(varchar, [COLS].[scale]) + ') '
          + CASE
            WHEN COLUMNPROPERTY(@TABLE_ID, [COLS].[name], 'IsIdentity') = 0 THEN ''
            ELSE ' IDENTITY(' + CONVERT(varchar, ISNULL(IDENT_SEED(@TBLNAME), 1)) + ','
              + CONVERT(varchar, ISNULL(IDENT_INCR(@TBLNAME), 1)) + ')'
          END
          + CASE
            WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
            ELSE ' '
          END + CASE
            WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
            ELSE ' NULL'
          END -- data types with scale  IE datetime2(7),TIME(7)
        WHEN TYPE_NAME([COLS].[user_type_id]) IN ('datetime2', 'datetimeoffset', 'time') THEN CASE
            WHEN [COLS].[scale] < 7 THEN '(' + CONVERT(varchar, [COLS].[scale]) + ') '
            ELSE ' '
          END
          + CASE
            WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
            ELSE ' '
          END + CASE
            WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
            ELSE ' NULL'
          END --data types with no/precision/scale,IE  FLOAT
        WHEN TYPE_NAME([COLS].[user_type_id]) IN ('float') --,'real')
        THEN --addition: if 53, no need to specifically say (53), otherwise display it
          CASE
            WHEN [COLS].[precision] = 53 THEN CASE
                WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
                ELSE ' '
              END + CASE
                WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
                ELSE ' NULL'
              END
            ELSE '(' + CONVERT(varchar, [COLS].[precision]) + ') '
              + CASE
                WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
                ELSE ' '
              END + CASE
                WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
                ELSE ' NULL'
              END
          END --data type with max_length  	ie CHAR (44), VARCHAR(40), BINARY(5000),
        -- ####################
        -- COLLATE STATEMENTS
        -- personally i do not like collation statements,
        -- but included here to make it easy on those who do
        -- ####################
        WHEN TYPE_NAME([COLS].[user_type_id]) IN ('char', 'varchar', 'binary', 'varbinary') THEN CASE
            WHEN [COLS].[max_length] = -1 THEN '(max)' --collate to comment out when not desired
              + CASE
                WHEN COLS.collation_name IS NULL THEN ''
                ELSE ' COLLATE ' + COLS.collation_name
              END
              + CASE
                WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
                ELSE ' '
              END + CASE
                WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
                ELSE ' NULL'
              END
            ELSE '(' + CONVERT(varchar, [COLS].[max_length]) + ') ' --collate to comment out when not desired
              + CASE
                WHEN COLS.collation_name IS NULL THEN ''
                ELSE ' COLLATE ' + COLS.collation_name
              END
              + CASE
                WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
                ELSE ' '
              END + CASE
                WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
                ELSE ' NULL'
              END
          END --data type with max_length ( BUT DOUBLED) ie NCHAR(33), NVARCHAR(40)
        WHEN TYPE_NAME([COLS].[user_type_id]) IN ('nchar', 'nvarchar') THEN CASE
            WHEN [COLS].[max_length] = -1 THEN '(max)' --collate to comment out when not desired
              + CASE
                WHEN COLS.collation_name IS NULL THEN ''
                ELSE ' COLLATE ' + COLS.collation_name
              END
              + CASE
                WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
                ELSE ' '
              END + CASE
                WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
                ELSE ' NULL'
              END
            ELSE '(' + CONVERT(varchar, ([COLS].[max_length] / 2)) + ') '
              + CASE
                WHEN COLS.collation_name IS NULL THEN ''
                ELSE ' COLLATE ' + COLS.collation_name
              END
              + CASE
                WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
                ELSE ' '
              END + CASE
                WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
                ELSE ' NULL'
              END
          END
        WHEN TYPE_NAME([COLS].[user_type_id]) IN ('datetime', 'money', 'text', 'image', 'real') THEN ' '
          + CASE
            WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
            ELSE ' '
          END + CASE
            WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
            ELSE ' NULL'
          END --  other data type 	IE INT, DATETIME, MONEY, CUSTOM DATA TYPE,...
        ELSE ' '
          + CASE
            WHEN COLUMNPROPERTY(@TABLE_ID, [COLS].[name], 'IsIdentity') = 0 THEN ' '
            ELSE ' IDENTITY(' + CONVERT(varchar, ISNULL(IDENT_SEED(@TBLNAME), 1)) + ','
              + CONVERT(varchar, ISNULL(IDENT_INCR(@TBLNAME), 1)) + ')'
          END
          + CASE
            WHEN [COLS].[is_sparse] = 1 THEN ' sparse'
            ELSE ' '
          END + CASE
            WHEN [COLS].[is_nullable] = 0 THEN ' NOT NULL'
            ELSE ' NULL'
          END
      END
      + CASE
        WHEN [COLS].[default_object_id] = 0 THEN '' --ELSE ' DEFAULT '  + ISNULL(def.[definition] ,'')
        --optional section in case NAMED default constraints are needed:
        ELSE '  CONSTRAINT ' + QUOTENAME([DEF].[name]) + ' DEFAULT ' + ISNULL([DEF].[definition], '') --i thought it needed to be handled differently! NOT!
      END --CASE cdefault
  END --iscomputed
  + ','
FROM [sys].[columns] [COLS]
LEFT OUTER JOIN [sys].[default_constraints] [DEF]
  ON [COLS].[default_object_id] = [DEF].[object_id]
LEFT OUTER JOIN [sys].[computed_columns] [CALC]
  ON [COLS].[object_id] = [CALC].[object_id]
  AND [COLS].[column_id] = [CALC].[column_id]
WHERE [COLS].[object_id] = @TABLE_ID
ORDER BY [COLS].[column_id];
-- ####################
--used for formatting the rest of the constraints:
-- ####################
SELECT
  @STRINGLEN = MAX(LEN([OBJS].[name])) + 1
FROM [sys].[objects] [OBJS];
-- ####################
--PK/Unique Constraints and Indexes, using the 2005/08 INCLUDE syntax
-- ####################
DECLARE @Results TABLE (
  [SCHEMA_ID] int,
  [SCHEMA_NAME] varchar(255),
  [OBJECT_ID] int,
  [OBJECT_NAME] varchar(255),
  [index_id] int,
  [index_name] varchar(255),
  [ROWS] bigint,
  [SizeMB] decimal(19, 3),
  [IndexDepth] int,
  [TYPE] int,
  [type_desc] varchar(30),
  [fill_factor] int,
  [is_unique] int,
  [is_primary_key] int,
  [is_unique_constraint] int,
  [index_columns_key] varchar(max),
  [index_columns_include] varchar(max),
  [has_filter] bit,
  [filter_definition] varchar(max),
  [currentFilegroupName] varchar(128),
  [CurrentCompression] varchar(128)
);
INSERT INTO @Results
  SELECT
    [SCH].[schema_id],
    [SCH].[name] AS [SCHEMA_NAME],
    [OBJS].[object_id],
    [OBJS].[name] AS [OBJECT_NAME],
    [IDX].[index_id],
    ISNULL([IDX].[name], '---') AS [index_name],
    [partitions].[ROWS],
    [partitions].[SizeMB],
    INDEXPROPERTY([OBJS].[object_id], [IDX].[name], 'IndexDepth') AS [IndexDepth],
    [IDX].[type],
    [IDX].[type_desc],
    [IDX].[fill_factor],
    [IDX].[is_unique],
    [IDX].[is_primary_key],
    [IDX].[is_unique_constraint],
    ISNULL([Index_Columns].[index_columns_key], '---') AS [index_columns_key],
    ISNULL([Index_Columns].[index_columns_include], '---') AS [index_columns_include],
    [IDX].[has_filter],
    [IDX].[filter_definition],
    [filz].[name],
    ISNULL([p].[data_compression_desc], '')
  FROM [sys].[objects] [OBJS]
  INNER JOIN [sys].[schemas] [SCH]
    ON [OBJS].[schema_id] = [SCH].[schema_id]
  INNER JOIN [sys].[indexes] [IDX]
    ON [OBJS].[object_id] = [IDX].[object_id]
  INNER JOIN [sys].[filegroups] [filz]
    ON [IDX].[data_space_id] = [filz].[data_space_id]
  INNER JOIN [sys].[partitions] [p]
    ON [IDX].[object_id] = [p].[object_id]
    AND [IDX].[index_id] = [p].[index_id]
  INNER JOIN (SELECT
    [STATS].[object_id],
    [STATS].[index_id],
    SUM([STATS].[row_count]) AS [ROWS],
    CONVERT(
    numeric(19, 3),
    CONVERT(
    numeric(19, 3),
    SUM([STATS].[in_row_reserved_page_count] + [STATS].[lob_reserved_page_count]
    + [STATS].[row_overflow_reserved_page_count])) / CONVERT(numeric(19, 3), 128)) AS [SizeMB]
  FROM [sys].[dm_db_partition_stats] [STATS]
  GROUP BY [STATS].[object_id],
           [STATS].[index_id]) AS [partitions]
    ON [IDX].[object_id] = [partitions].[OBJECT_ID]
    AND [IDX].[index_id] = [partitions].[index_id]
  CROSS APPLY (SELECT
    LEFT([Index_Columns].[index_columns_key], LEN([Index_Columns].[index_columns_key]) - 1) AS [index_columns_key],
    LEFT([Index_Columns].[index_columns_include], LEN([Index_Columns].[index_columns_include]) - 1) AS [index_columns_include]
  FROM (SELECT (SELECT
                 QUOTENAME([COLS].[name])
                 + CASE
                   WHEN [IXCOLS].[is_descending_key] = 0 THEN ' asc'
                   ELSE ' desc'
                 END + ',' + ' '
               FROM [sys].[index_columns] [IXCOLS]
               INNER JOIN [sys].[columns] [COLS]
                 ON [IXCOLS].[column_id] = [COLS].[column_id]
                 AND [IXCOLS].[object_id] = [COLS].[object_id]
               WHERE [IXCOLS].[is_included_column] = 0
               AND [IDX].[object_id] = [IXCOLS].[object_id]
               AND [IDX].[index_id] = [IXCOLS].[index_id]
               ORDER BY [IXCOLS].[key_ordinal]
               FOR xml PATH (''))
               AS [index_columns_key],
               (SELECT
                 QUOTENAME([COLS].[name]) + ',' + ' '
               FROM [sys].[index_columns] [IXCOLS]
               INNER JOIN [sys].[columns] [COLS]
                 ON [IXCOLS].[column_id] = [COLS].[column_id]
                 AND [IXCOLS].[object_id] = [COLS].[object_id]
               WHERE [IXCOLS].[is_included_column] = 1
               AND [IDX].[object_id] = [IXCOLS].[object_id]
               AND [IDX].[index_id] = [IXCOLS].[index_id]
               ORDER BY [IXCOLS].[index_column_id]
               FOR xml PATH (''))
               AS [index_columns_include]) AS [Index_Columns]) AS [Index_Columns]
  WHERE [SCH].[name] LIKE CASE
    WHEN @SCHEMANAME = '' COLLATE Chinese_PRC_CI_AS THEN [SCH].[name]
    ELSE @SCHEMANAME
  END
  AND [OBJS].[name] LIKE CASE
    WHEN @TBLNAME = '' COLLATE Chinese_PRC_CI_AS THEN [OBJS].[name]
    ELSE @TBLNAME
  END
  ORDER BY [SCH].[name],
  [OBJS].[name],
  [IDX].[name];
--@Results table has both PK,s Uniques and indexes in thme...pull them out for adding to funal results:
SET @CONSTRAINTSQLS = '';
SET @INDEXSQLS = '';
-- ####################
--constriants
-- ####################
SELECT
  @CONSTRAINTSQLS
  = @CONSTRAINTSQLS
  + CASE
    WHEN [is_primary_key] = 1 OR
      [is_unique] = 1 THEN @vbCrLf + 'CONSTRAINT   ' COLLATE Chinese_PRC_CI_AS + QUOTENAME([index_name]) + ' '
      + CASE
        WHEN [is_primary_key] = 1 THEN ' PRIMARY KEY '
        ELSE CASE
            WHEN [is_unique] = 1 THEN ' UNIQUE      '
            ELSE ''
          END
      END + [type_desc] + CASE
        WHEN [type_desc] = 'NONCLUSTERED' THEN ''
        ELSE '   '
      END + ' (' + [index_columns_key]
      + ')' + CASE
        WHEN [index_columns_include] <> '---' THEN ' INCLUDE (' + [index_columns_include] + ')'
        ELSE ''
      END + CASE
        WHEN [has_filter] = 1 THEN ' ' -- + [filter_definition]
        ELSE ' '
      END
      + CASE
        WHEN [fill_factor] <> 0 OR
          [CurrentCompression] <> 'NONE' THEN ' WITH ('
          + CASE
            WHEN [fill_factor] <> 0 THEN 'FILLFACTOR = ' + CONVERT(varchar(30), [fill_factor])
            ELSE ''
          END
          + CASE
            WHEN [fill_factor] <> 0 AND
              [CurrentCompression] <> 'NONE' THEN ',DATA_COMPRESSION = ' + [CurrentCompression] + ' '
            WHEN [fill_factor] <> 0 AND
              [CurrentCompression] = 'NONE' THEN ''
            WHEN [fill_factor] = 0 AND
              [CurrentCompression] <> 'NONE' THEN 'DATA_COMPRESSION = ' + [CurrentCompression] + ' '
            ELSE ''
          END + ')'
        ELSE ''
      END
    ELSE ''
  END + ','
FROM @RESULTS
WHERE [type_desc] != 'HEAP'
AND [is_primary_key] = 1
OR [is_unique] = 1
ORDER BY [is_primary_key] DESC,
[is_unique] DESC;
-- ####################
--indexes
-- ####################
SELECT
  @INDEXSQLS
  = @INDEXSQLS
  + CASE
    WHEN [is_primary_key] = 0 OR
      [is_unique] = 0 THEN @vbCrLf + 'CREATE ' COLLATE Chinese_PRC_CI_AS + [type_desc] + ' INDEX ' COLLATE Chinese_PRC_CI_AS
      + QUOTENAME([index_name]) + ' ' + @vbCrLf + '   ON ' COLLATE Chinese_PRC_CI_AS
      + QUOTENAME([schema_name]) + '.' + QUOTENAME([OBJECT_NAME])
      + CASE
        WHEN [CurrentCompression] = 'COLUMNSTORE' COLLATE Chinese_PRC_CI_AS THEN ' (' + [index_columns_include] + ')'
        ELSE ' (' + [index_columns_key] + ')'
      END
      + CASE
        WHEN [CurrentCompression] = 'COLUMNSTORE' COLLATE Chinese_PRC_CI_AS THEN '' COLLATE Chinese_PRC_CI_AS
        ELSE CASE
            WHEN [index_columns_include] <> '---' THEN @vbCrLf + '   INCLUDE (' COLLATE Chinese_PRC_CI_AS + [index_columns_include]
              + ')' COLLATE Chinese_PRC_CI_AS
            ELSE '' COLLATE Chinese_PRC_CI_AS
          END
      END --2008 filtered indexes syntax
      + CASE
        WHEN [has_filter] = 1 THEN @vbCrLf + '   WHERE ' COLLATE Chinese_PRC_CI_AS + [filter_definition]
        ELSE ''
      END
      + CASE
        WHEN [fill_factor] <> 0 OR
          [CurrentCompression] <> 'NONE' COLLATE Chinese_PRC_CI_AS THEN ' WITH (' COLLATE Chinese_PRC_CI_AS
          + CASE
            WHEN [fill_factor] <> 0 THEN 'FILLFACTOR = ' COLLATE Chinese_PRC_CI_AS + CONVERT(varchar(30), [fill_factor])
            ELSE ''
          END
          + CASE
            WHEN [fill_factor] <> 0 AND
              [CurrentCompression] <> 'NONE' THEN ',DATA_COMPRESSION = ' + [CurrentCompression] + ' '
            WHEN [fill_factor] <> 0 AND
              [CurrentCompression] = 'NONE' THEN ''
            WHEN [fill_factor] = 0 AND
              [CurrentCompression] <> 'NONE' THEN 'DATA_COMPRESSION = ' + [CurrentCompression] + ' '
            ELSE ''
          END + ')'
        ELSE ''
      END
  END
FROM @RESULTS
WHERE [type_desc] != 'HEAP'
AND [is_primary_key] = 0
AND [is_unique] = 0
ORDER BY [is_primary_key] DESC,
[is_unique] DESC;
IF @INDEXSQLS <> '' COLLATE Chinese_PRC_CI_AS
  SET @INDEXSQLS = @vbCrLf + ';' COLLATE Chinese_PRC_CI_AS + @vbCrLf + @INDEXSQLS;
-- ####################
--CHECK Constraints
-- ####################
SET @CHECKCONSTSQLS = '' COLLATE Chinese_PRC_CI_AS;
SELECT
  @CHECKCONSTSQLS
  = @CHECKCONSTSQLS + @vbCrLf
  + ISNULL(
  'CONSTRAINT ' + QUOTENAME([OBJS].[name]) + ' CHECK '
  + ISNULL([CHECKS].[definition], '') + ',',
  '')
FROM [sys].[objects] [OBJS]
INNER JOIN [sys].[check_constraints] [CHECKS]
  ON [OBJS].[object_id] = [CHECKS].[object_id]
WHERE [OBJS].[type] = 'C'
AND [OBJS].[parent_object_id] = @TABLE_ID;
-- ####################
--FOREIGN KEYS
-- ####################
SET @FKSQLS = '';
SELECT
  @FKSQLS = @FKSQLS + @vbCrLf + [MyAlias].[Command]
FROM (SELECT DISTINCT --FK must be added AFTER the PK/unique constraints are added back.
  850 AS [ExecutionOrder],
  'CONSTRAINT ' + QUOTENAME([conz].[name]) + ' FOREIGN KEY (' + [ChildCollection].[ChildColumns]
  + ') REFERENCES ' + QUOTENAME(SCHEMA_NAME([conz].[schema_id])) + '.'
  + QUOTENAME(OBJECT_NAME([conz].[referenced_object_id])) + ' (' + [ParentCollection].[ParentColumns]
  + ') ' + CASE [conz].[update_referential_action]
    WHEN 0 THEN '' --' ON UPDATE NO ACTION '
    WHEN 1 THEN ' ON UPDATE CASCADE '
    WHEN 2 THEN ' ON UPDATE SET NULL '
    ELSE ' ON UPDATE SET DEFAULT '
  END + CASE [conz].[delete_referential_action]
    WHEN 0 THEN '' --' ON DELETE NO ACTION '
    WHEN 1 THEN ' ON DELETE CASCADE '
    WHEN 2 THEN ' ON DELETE SET NULL '
    ELSE ' ON DELETE SET DEFAULT '
  END
  + CASE [conz].[is_not_for_replication]
    WHEN 1 THEN ' NOT FOR REPLICATION '
    ELSE ''
  END + ',' AS [Command]
FROM [sys].[foreign_keys] [conz]
INNER JOIN [sys].[foreign_key_columns] [colz]
  ON [conz].[object_id] = [colz].[constraint_object_id]
INNER JOIN (
--gets my child tables column names
SELECT
  [conz].[name],
  --technically, FK's can contain up to 16 columns, but real life is often a single column. coding here is for all columns
  [ChildColumns] = STUFF((SELECT
    ',' + QUOTENAME([REFZ].[name])
  FROM [sys].[foreign_key_columns] [fkcolz]
  INNER JOIN [sys].[columns] [REFZ]
    ON [fkcolz].[parent_object_id] = [REFZ].[object_id]
    AND [fkcolz].[parent_column_id] = [REFZ].[column_id]
  WHERE [fkcolz].[parent_object_id] = [conz].[parent_object_id]
  AND [fkcolz].[constraint_object_id] = [conz].[object_id]
  ORDER BY [fkcolz].[constraint_column_id]
  FOR xml PATH (''), TYPE)
  .[value]('.', 'varchar(max)'),
  1,
  1,
  '')
FROM [sys].[foreign_keys] [conz]
INNER JOIN [sys].[foreign_key_columns] [colz]
  ON [conz].[object_id] = [colz].[constraint_object_id]
WHERE [conz].[parent_object_id] = @TABLE_ID
GROUP BY [conz].[name],
         [conz].[parent_object_id],
         --- without GROUP BY multiple rows are returned
         [conz].[object_id]) [ChildCollection]
  ON [conz].[name] = [ChildCollection].[name]
INNER JOIN (
--gets the parent tables column names for the FK reference
SELECT
  [conz].[name],
  [ParentColumns] = STUFF((SELECT
    ',' + [REFZ].[name]
  FROM [sys].[foreign_key_columns] [fkcolz]
  INNER JOIN [sys].[columns] [REFZ]
    ON [fkcolz].[referenced_object_id] = [REFZ].[object_id]
    AND [fkcolz].[referenced_column_id] = [REFZ].[column_id]
  WHERE [fkcolz].[referenced_object_id] = [conz].[referenced_object_id]
  AND [fkcolz].[constraint_object_id] = [conz].[object_id]
  ORDER BY [fkcolz].[constraint_column_id]
  FOR xml PATH (''), TYPE)
  .[value]('.', 'varchar(max)'),
  1,
  1,
  '')
FROM [sys].[foreign_keys] [conz]
INNER JOIN [sys].[foreign_key_columns] [colz]
  ON [conz].[object_id] = [colz].[constraint_object_id] -- AND colz.parent_column_id
GROUP BY [conz].[name],
         [conz].[referenced_object_id],
         --- without GROUP BY multiple rows are returned
         [conz].[object_id]) [ParentCollection]
  ON [conz].[name] = [ParentCollection].[name]) [MyAlias];
-- ####################
--RULES
-- ####################
SET @RULESCONSTSQLS = '';
SELECT
  @RULESCONSTSQLS
  = @RULESCONSTSQLS
  + ISNULL(
  @vbCrLf
  + 'if not exists(SELECT [name] FROM sys.objects WHERE TYPE=''R'' AND schema_id = ' COLLATE Chinese_PRC_CI_AS
  + CONVERT(varchar(30), [OBJS].[schema_id]) + ' AND [name] = ''' COLLATE Chinese_PRC_CI_AS
  + QUOTENAME(OBJECT_NAME([COLS].[rule_object_id])) + ''')' COLLATE Chinese_PRC_CI_AS + @vbCrLf
  + [MODS].[definition] + @vbCrLf + ';' COLLATE Chinese_PRC_CI_AS + @vbCrLf + 'EXEC sp_binderule  '
  + QUOTENAME([OBJS].[name]) + ', ''' + QUOTENAME(OBJECT_NAME([COLS].[object_id])) + '.'
  + QUOTENAME([COLS].[name]) + '''' COLLATE Chinese_PRC_CI_AS + @vbCrLf + ';' COLLATE Chinese_PRC_CI_AS,
  '')
FROM [sys].[columns] [COLS]
INNER JOIN [sys].[objects] [OBJS]
  ON [OBJS].[object_id] = [COLS].[object_id]
INNER JOIN [sys].[sql_modules] [MODS]
  ON [COLS].[rule_object_id] = [MODS].[object_id]
WHERE [COLS].[rule_object_id] <> 0
AND [COLS].[object_id] = @TABLE_ID;
-- ####################
--TRIGGERS
-- ####################
SET @TRIGGERSTATEMENT = '';
SELECT
  @TRIGGERSTATEMENT = @TRIGGERSTATEMENT + @vbCrLf + [MODS].[definition] + @vbCrLf + ';'
FROM [sys].[sql_modules] [MODS]
WHERE [MODS].[object_id] IN (SELECT
  [OBJS].[object_id]
FROM [sys].[objects] [OBJS]
WHERE [OBJS].[type] = 'TR'
AND [OBJS].[parent_object_id] = @TABLE_ID);
IF @TRIGGERSTATEMENT <> '' COLLATE Chinese_PRC_CI_AS
  SET @TRIGGERSTATEMENT = @vbCrLf + ';' COLLATE Chinese_PRC_CI_AS + @vbCrLf + @TRIGGERSTATEMENT;
-- ####################
--NEW SECTION QUERY ALL EXTENDED PROPERTIES
-- ####################
SET @EXTENDEDPROPERTIES = '';
SELECT
  @EXTENDEDPROPERTIES
  = @EXTENDEDPROPERTIES + @vbCrLf + 'EXEC sys.sp_addextendedproperty @name = N''' COLLATE Chinese_PRC_CI_AS + [name] + ''', @value = N''' COLLATE Chinese_PRC_CI_AS
  + REPLACE(CONVERT(varchar(max), [VALUE]), '''', '''''')
  + ''', @level0type = N''SCHEMA'', @level0name = ' COLLATE Chinese_PRC_CI_AS + QUOTENAME(@SCHEMANAME)
  + ', @level1type = N''TABLE'', @level1name = ' COLLATE Chinese_PRC_CI_AS + QUOTENAME(@TBLNAME) + ';' + @vbCrLf
FROM [sys].[fn_listextendedproperty](NULL, 'schema', @SCHEMANAME, 'table', @TBLNAME, NULL, NULL);

WITH [obj]
AS (SELECT
  [split].[a].[value]('.', 'VARCHAR(20)') AS [name]
FROM (SELECT
  CAST('<M>' + REPLACE('column,constraint,index,trigger,parameter', ',', '</M><M>') + '</M>' AS xml) AS [data]) AS [A]
CROSS APPLY [data].[nodes]('/M') AS [split] ([a]))
SELECT
  @EXTENDEDPROPERTIES
  = @EXTENDEDPROPERTIES + @vbCrLf + 'EXEC sys.sp_addextendedproperty @name = N''' COLLATE Chinese_PRC_CI_AS + [lep].[name] + ''', @value = N''' COLLATE Chinese_PRC_CI_AS
  + REPLACE(CONVERT(varchar(max), [lep].[value]), '''', '''''')
  + ''', @level0type = N''SCHEMA'', @level0name = ' COLLATE Chinese_PRC_CI_AS + QUOTENAME(@SCHEMANAME)
  + ', @level1type = N''TABLE'', @level1name = ' COLLATE Chinese_PRC_CI_AS + QUOTENAME(@TBLNAME)
  + ', @level2type = N''' COLLATE Chinese_PRC_CI_AS + UPPER([obj].[name])
  + ''', @level2name = ' COLLATE Chinese_PRC_CI_AS + QUOTENAME([lep].[objname]) + ';' COLLATE Chinese_PRC_CI_AS --SELECT objtype, objname, name, value
FROM [obj]
CROSS APPLY [sys].[fn_listextendedproperty](NULL, 'schema', @SCHEMANAME, 'table', @TBLNAME, [obj].[name], NULL) AS [lep];
IF @EXTENDEDPROPERTIES <> '' COLLATE Chinese_PRC_CI_AS
  SET @EXTENDEDPROPERTIES = @vbCrLf + ';' COLLATE Chinese_PRC_CI_AS + @vbCrLf + @EXTENDEDPROPERTIES;
-- ####################
--FINAL CLEANUP AND PRESENTATION
-- ####################
--at this point, there is a trailing comma, or it blank
SELECT
  @FINALSQL = @FINALSQL + @CONSTRAINTSQLS + @CHECKCONSTSQLS + @FKSQLS;
--note that this trims the trailing comma from the end of the statements
SET @FINALSQL = SUBSTRING(@FINALSQL, 1, LEN(@FINALSQL) - 1);
SET @FINALSQL = @FINALSQL + ')' COLLATE Chinese_PRC_CI_AS;
SET @input = ' ' + @FINALSQL + @INDEXSQLS + @RULESCONSTSQLS + @TRIGGERSTATEMENT + @EXTENDEDPROPERTIES;

SELECT
  @TBLNAME AS [Table],
  @input AS [Create Table];
";
                    break;
                case SharedEnum.TypeDB.PostgreSQL:
                    // https://stackoverflow.com/questions/2593803
                    result = $@"
DO $$
DECLARE
  in_schema_name VARCHAR := '{schemaName}';
  in_table_name VARCHAR := '{tableName}';
  newline VARCHAR := E'\n';
  -- the ddl we're building
  v_table_ddl TEXT;
  -- data about the target table
  v_table_oid INT;
  -- records for looping
  v_column_record record;
  v_constraint_record record;
  v_index_record record;
  v_table_comment TEXT;
  v_column_comment TEXT;
BEGIN
  SELECT
    c.oid INTO v_table_oid
  FROM
    pg_catalog.pg_class c
    LEFT JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
  WHERE
    1 = 1
    AND c.relkind = 'r'
    AND c.relname = in_table_name
    AND n.nspname = in_schema_name;
  -- the schema
  -- throw an error if table was not found
  IF (v_table_oid IS NULL) THEN
    RAISE EXCEPTION 'table(%) does not exist',in_table_name;
  END IF;
  -- start the create definition
  v_table_ddl := concat(
    'DROP TABLE IF EXISTS ',
    in_schema_name,
    '.""',
    in_table_name,
    '"";',
    newline,
    'CREATE TABLE ',
    in_schema_name,
    '.""',
    in_table_name,
    '"" (',
    newline
  );
  -- define all of the columns in the table; https://stackoverflow.com/a/8153081/3068233
  FOR v_column_record IN
  SELECT
    c.table_schema,
    c.table_name,
    c.column_name,
    c.data_type,
    c.character_maximum_length,
    c.is_nullable,
    c.column_default,
    col_description(
      format('%s.""%s""', c.table_schema, c.table_name) :: regclass :: oid,
      c.ordinal_position
    ) AS column_comment
  FROM
    information_schema.columns c
  WHERE
    (table_schema, table_name) = (in_schema_name, in_table_name)
  ORDER BY
    ordinal_position LOOP v_table_ddl := concat(
      v_table_ddl,
      '  ""',
      v_column_record.column_name,
      '"" ',
      v_column_record.data_type,
      CASE
        WHEN v_column_record.character_maximum_length IS NOT NULL THEN concat('(', v_column_record.character_maximum_length, ')')
        ELSE ''
      END,
      ' ',
      CASE
        WHEN v_column_record.is_nullable = 'NO' THEN 'NOT NULL'
        ELSE 'NULL'
      END,
      CASE
        WHEN v_column_record.column_default IS NOT NULL THEN concat(' DEFAULT ', v_column_record.column_default)
        ELSE ''
      END,
      ',',
      newline
    );
  -- column comment
  v_column_comment := concat(
    v_column_comment,    
    newline,
    'COMMENT ON COLUMN ',
    v_column_record.table_schema,
    '.""',
    v_column_record.table_name,
    '"".""',
    v_column_record.column_name,
    '"" IS ''',
    v_column_record.column_comment,
    ''';'
  );
  END LOOP;
  -- define all the constraints in the; https://dba.stackexchange.com/a/214877/75296
  FOR v_constraint_record IN
  SELECT
    con.conname AS constraint_name,
    con.contype AS constraint_type,
    CASE
      WHEN con.contype = 'p' THEN 1 -- primary key constraint
      WHEN con.contype = 'u' THEN 2 -- unique constraint
      WHEN con.contype = 'f' THEN 3 -- foreign key constraint
      WHEN con.contype = 'c' THEN 4
      ELSE 5
    END AS type_rank,
    pg_get_constraintdef(con.oid) AS constraint_definition
  FROM
    pg_catalog.pg_constraint con
    JOIN pg_catalog.pg_class rel ON rel.oid = con.conrelid
    JOIN pg_catalog.pg_namespace nsp ON nsp.oid = connamespace
  WHERE
    nsp.nspname = in_schema_name
    AND rel.relname = in_table_name
  ORDER BY
    type_rank LOOP v_table_ddl := concat(
      v_table_ddl,
      '  ',
      'CONSTRAINT ""',
      v_constraint_record.constraint_name,
      '"" ',
      v_constraint_record.constraint_definition,
      ',',
      newline
    );
  END LOOP;
  -- drop the last comma before ending the create statement
  v_table_ddl = concat(
    substr(v_table_ddl, 0, length(v_table_ddl) - 1),
    newline
  );
  -- end the create definition
  v_table_ddl := concat(v_table_ddl, ');', newline, newline);
  -- suffix create statement with all of the indexes on the table
  FOR v_index_record IN
  SELECT
    indexdef
  FROM
    pg_indexes
  WHERE
    (schemaname, tablename) = (in_schema_name, in_table_name)
    AND indexname NOT IN (
      SELECT
        conname
      FROM
        pg_catalog.pg_constraint
      WHERE
        contype = 'p'
    ) LOOP v_table_ddl := concat(v_table_ddl, v_index_record.indexdef, ';', newline);
  END LOOP;
  -- table comment
  SELECT
    concat(
      'COMMENT ON TABLE ',
      schemaname,
      '.""',
      relname,
      '""',
      ' IS ''',
      REPLACE(obj_description(relid), '''', ''''''),
      ''';',
      newline
    ) INTO v_table_comment
  FROM
    pg_stat_user_tables
  WHERE
    schemaname = in_schema_name
    AND relname = in_table_name;
  -- comment
  v_table_ddl := concat(
    v_table_ddl,
    newline,
    v_table_comment,
    v_column_comment
  );
  -- return the ddl
  RAISE NOTICE '%', REPLACE(v_table_ddl, concat(newline, newline, newline), newline);
END
$$;
";
                    break;
            }

            return result;
        }

        /// <summary>
        /// 获取列
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="databaseName">数据库名</param>
        /// <param name="listSchemaNameTableName">过滤 模式名、表名集合</param>
        /// <returns></returns>
        public static string GetColumn(SharedEnum.TypeDB tdb, string databaseName, List<Tuple<string, string>> listSchemaNameTableName = null)
        {
            string result = null;

            string where = string.Empty;
            if (listSchemaNameTableName?.Count > 0)
            {
                var orList = new List<string>();
                listSchemaNameTableName.ForEach(item =>
                {
                    var andList = new List<string>();

                    //TableName
                    if (!string.IsNullOrWhiteSpace(item.Item2))
                    {
                        andList.Add("{0}='" + item.Item2 + "'");
                    }
                    //SchemaName
                    if (!string.IsNullOrWhiteSpace(item.Item1))
                    {
                        andList.Add("{1}='" + item.Item1 + "'");
                    }
                    orList.Add($"({string.Join(" AND ", andList)})");
                });

                where = $"AND ({string.Join(" OR ", orList)})";
            }

            switch (tdb)
            {
                case SharedEnum.TypeDB.SQLite:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "m.name");
                    }

                    result = $@"
SELECT
  '' AS TableName,
  '' AS ColumnName,
  '' AS ColumnType,
  '' AS DataType,
  0 AS DataLength,
  0 AS DataScale,
  0 AS ColumnOrder,
  0 AS PrimaryKey,
  0 AS AutoIncr,
  1 AS IsNullable,
  '' AS ColumnDefault
UNION ALL
SELECT
  m.name AS TableName,
  p.name AS ColumnName,
  p.type AS ColumnType,
  CASE
    WHEN instr(p.type, '(') = 0 THEN p.type
    ELSE substr(p.type, 0, instr(p.type, '('))
  END AS DataType,
  CASE
    WHEN instr(p.type, ',') <> 0 THEN substr(
      p.type,
      instr(p.type, '(') + 1,
      instr(p.type, ',') - instr(p.type, '(') -1
    )
    WHEN instr(p.type, '(') <> 0 THEN substr(
      p.type,
      instr(p.type, '(') + 1,
      LENGTH(p.type) - instr(p.type, '(') -1
    )
    ELSE NULL
  END AS DataLength,
  CASE
    WHEN instr(p.type, ',') <> 0 THEN substr(
      p.type,
      instr(p.type, ',') + 1,
      LENGTH(p.type) - instr(p.type, ',') -1
    )
    ELSE NULL
  END AS DataScale,
  p.cid+1 AS ColumnOrder,
  p.pk AS PrimaryKey,
  0 AS AutoIncr,
  CASE
    WHEN p.[notnull] = 1 THEN 0
    ELSE 1
  END AS IsNullable,
  p.dflt_value AS ColumnDefault
FROM
  {databaseName}.sqlite_master m
  LEFT OUTER JOIN pragma_table_info (m.name) p ON m.name <> p.name
WHERE
  m.type = 'table' {where}
ORDER BY
  TableName,
  ColumnOrder;

SELECT name, sql FROM {databaseName}.sqlite_master m WHERE 1=1 {where}
";
                    break;
                case SharedEnum.TypeDB.MySQL:
                case SharedEnum.TypeDB.MariaDB:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "t1.TABLE_NAME", "t1.TABLE_SCHEMA");
                    }

                    result = $@"
SELECT
  t1.TABLE_NAME AS TableName,
  t1.TABLE_SCHEMA AS SchemaName,
  t2.TABLE_COMMENT AS TableComment,
  t1.COLUMN_NAME AS ColumnName,
  t1.COLUMN_TYPE AS ColumnType,
  t1.DATA_TYPE AS DataType,
  COALESCE(t1.CHARACTER_MAXIMUM_LENGTH, t1.NUMERIC_PRECISION) AS DataLength,
  t1.NUMERIC_SCALE AS DataScale,
  t1.ORDINAL_POSITION AS ColumnOrder,
  t3.ORDINAL_POSITION AS PrimaryKey,
  CASE
    WHEN t1.EXTRA = 'auto_increment' THEN 1
    ELSE 0
  END AS AutoIncr,
  CASE
    WHEN t1.IS_NULLABLE = 'YES' THEN 1
    ELSE 0
  END AS IsNullable,
  t1.COLUMN_DEFAULT AS ColumnDefault,
  t1.COLUMN_COMMENT AS ColumnComment
FROM
  information_schema.columns t1
  LEFT JOIN information_schema.tables t2 ON t1.TABLE_SCHEMA = t2.TABLE_SCHEMA
  AND t1.TABLE_NAME = t2.TABLE_NAME
  LEFT JOIN information_schema.key_column_usage t3 ON t3.TABLE_SCHEMA = t1.TABLE_SCHEMA
  AND t3.TABLE_NAME = t1.TABLE_NAME
  AND t3.COLUMN_NAME = t1.COLUMN_NAME
WHERE
  t2.TABLE_TYPE = 'BASE TABLE'
  AND t2.TABLE_SCHEMA = '{databaseName}' {where}
ORDER BY
  t1.TABLE_NAME,
  t1.ORDINAL_POSITION
";
                    break;
                case SharedEnum.TypeDB.Oracle:
                    if (!string.IsNullOrEmpty(where))
                    {
                        where = string.Format(where, "t1.TABLE_NAME");
                    }

                    result = $@"
SELECT
  t1.TABLE_NAME AS TableName,
  t2.COMMENTS AS TableComment,
  t3.COLUMN_NAME AS ColumnName,
  CASE
    WHEN t3.DATA_PRECISION IS NOT NULL THEN t3.DATA_TYPE || '(' || t3.DATA_PRECISION || ',' || t3.DATA_SCALE || ')'
    WHEN t3.CHAR_COL_DECL_LENGTH IS NOT NULL THEN t3.DATA_TYPE || '(' || t3.CHAR_COL_DECL_LENGTH || ')'
    ELSE t3.DATA_TYPE
  END AS ColumnType,
  t3.DATA_TYPE AS DataType,
  COALESCE(t3.DATA_PRECISION, t3.CHAR_COL_DECL_LENGTH) AS DataLength,
  t3.DATA_SCALE AS DataScale,
  t3.COLUMN_ID AS ColumnOrder,
  t5.POSITION AS PrimaryKey,
  DECODE(t3.NULLABLE, 'N', 0, 1) AS IsNullable,
  t3.DATA_DEFAULT AS ColumnDefault,
  t4.COMMENTS AS ColumnComment
FROM
  ALL_TABLES t1
  LEFT JOIN ALL_TAB_COMMENTS t2 ON t1.OWNER = t2.OWNER
  AND t1.TABLE_NAME = t2.TABLE_NAME
  LEFT JOIN ALL_TAB_COLUMNS t3 ON t1.OWNER = t3.OWNER
  AND t1.TABLE_NAME = t3.TABLE_NAME
  LEFT JOIN ALL_COL_COMMENTS t4 ON t1.OWNER = t4.OWNER
  AND t1.TABLE_NAME = t4.TABLE_NAME
  AND t3.COLUMN_NAME = t4.COLUMN_NAME
  LEFT JOIN ALL_CONS_COLUMNS t5 ON t1.OWNER = t5.OWNER
  AND t1.TABLE_NAME = t5.TABLE_NAME
  AND t3.COLUMN_NAME = t5.COLUMN_NAME
  AND t5.POSITION IS NOT NULL
WHERE
  t1.OWNER = '{databaseName}' {where}
ORDER BY
  t1.TABLE_NAME,
  t3.COLUMN_ID
";
                    break;
                case SharedEnum.TypeDB.SQLServer:
                    if (listSchemaNameTableName?.Count > 0)
                    {
                        where = string.Format(where, "o.name", "SCHEMA_NAME(o.schema_id)");
                    }

                    result = $@"
USE [{databaseName}];
SELECT
  o.name AS TableName,
  SCHEMA_NAME(o.schema_id) AS SchemaName,
  ep1.value AS TableComment,
  c.name AS ColumnName,
  CASE
    -- decimal/numeric
    WHEN c.system_type_id IN (106, 108) THEN CONCAT(t.name, '(', c.precision, ',', c.scale, ')')
    -- int/real/float/money
    WHEN c.system_type_id IN (48, 52, 56, 59, 60, 62, 122, 127) THEN t.name
    -- datetime/smalldatetime
    WHEN c.system_type_id IN (40, 41, 42, 43, 58, 61) THEN t.name
    ELSE CONCAT(t.name, '(', COLUMNPROPERTY(c.object_id, c.name, 'charmaxlen'), ')')
  END AS ColumnType,
  t.name AS DataType,
  CASE
    WHEN c.system_type_id IN (48, 52, 56, 59, 60, 62, 106, 108, 122, 127) THEN c.precision
    ELSE COLUMNPROPERTY(c.object_id, c.name, 'charmaxlen')
  END AS DataLength,
  CASE
    WHEN c.system_type_id IN (40, 41, 42, 43, 58, 61) THEN NULL
    ELSE ODBCSCALE(c.system_type_id, c.scale)
  END AS DataScale,
  c.column_id AS ColumnOrder,
  k.key_ordinal AS PrimaryKey,
  c.is_identity AS AutoIncr,
  c.is_nullable AS IsNullable,
  OBJECT_DEFINITION(c.default_object_id) AS ColumnDefault,
  ep2.value AS ColumnComment
FROM sys.objects o
JOIN sys.columns c
  ON c.object_id = o.object_id
LEFT JOIN sys.types t
  ON c.user_type_id = t.user_type_id
LEFT JOIN (SELECT
  idx.object_id,
  ic1.key_ordinal,
  ic1.column_id
FROM sys.indexes AS idx
INNER JOIN sys.index_columns AS ic1
  ON idx.object_id = ic1.object_id
  AND idx.index_id = ic1.index_id
WHERE idx.is_primary_key = 1) k
  ON c.object_id = k.object_id
  AND c.column_id = k.column_id
LEFT JOIN sys.extended_properties ep1
  ON c.object_id = ep1.major_id
  AND ep1.minor_id = 0
LEFT JOIN sys.extended_properties ep2
  ON ep2.major_id = c.object_id
  AND ep2.minor_id = c.column_id
WHERE o.type IN ('U', 'V') {where}
ORDER BY SCHEMA_NAME(o.schema_id), o.name, c.column_id
            ";
                    break;
                case SharedEnum.TypeDB.PostgreSQL:
                    if (listSchemaNameTableName?.Count > 0)
                    {
                        where = string.Format(where, "t1.table_name", "t1.table_schema");
                    }

                    result = $@"
SELECT
  t1.table_name AS TableName,
  t1.table_schema AS SchemaName,
  obj_description (
    format ('%s.""%s""', t1.table_schema, t1.table_name) :: regclass :: oid,
    'pg_class'
  ) AS TableComment,
  t1.column_name AS ColumnName,
  CASE
    WHEN t1.character_maximum_length IS NULL THEN t1.udt_name
    ELSE CONCAT(t1.udt_name, '(', t1.character_maximum_length, ')')
  END AS ColumnType,
  t1.udt_name AS DataType,
  COALESCE(t1.character_maximum_length, numeric_precision) AS DataLength,
  numeric_scale AS DataScale,
  t1.ordinal_position AS ColumnOrder,
  CASE
    t1.is_identity
    WHEN 'YES' THEN 1
    ELSE 0
  END AS AutoIncr,
  t2.ordinal_position AS PrimaryKey,
  CASE
    t1.is_nullable
    WHEN 'NO' THEN 0
    ELSE 1
  END AS IsNullable,
  t1.column_default AS ColumnDefault,
  col_description (
    format ('%s.""%s""', t1.table_schema, t1.table_name) :: regclass :: oid,
    t1.ordinal_position
  ) AS ColumnComment
FROM
  information_schema.columns t1
  LEFT JOIN information_schema.key_column_usage t2 ON t1.table_name = t2.table_name
  AND t1.table_schema = t2.table_schema
  AND t1.column_name = t2.column_name
WHERE
  t1.table_schema NOT IN('pg_catalog', 'information_schema') {where}
ORDER BY
  t1.table_schema,
  t1.table_name,
  t1.ordinal_position
";
                    break;
            }

            return result;
        }

        /// <summary>
        /// 设置表注释
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="databaseName">数据库名</param>
        /// <param name="tableSchema">模式</param>
        /// <param name="tableName">表名</param>
        /// <param name="tableComment">表注释</param>
        /// <returns></returns>
        public static string SetTableComment(SharedEnum.TypeDB tdb, string databaseName, string tableSchema, string tableName, string tableComment)
        {
            string result = null;

            tableComment = tableComment.OfSql();
            switch (tdb)
            {
                case SharedEnum.TypeDB.MySQL:
                case SharedEnum.TypeDB.MariaDB:
                    result = $"ALTER TABLE `{databaseName}`.`{tableName}` COMMENT '{tableComment}'";
                    break;
                case SharedEnum.TypeDB.Oracle:
                    result = $"COMMENT ON TABLE \"{databaseName}\".\"{tableName}\" IS '{tableComment}'";
                    break;
                case SharedEnum.TypeDB.SQLServer:
                    tableSchema ??= "dbo";

                    result = $@"
USE [{databaseName}];
IF NOT EXISTS (
  SELECT
    A.name,
    C.value
  FROM
    sys.tables A
    INNER JOIN sys.extended_properties C ON C.major_id = A.object_id
    AND minor_id = 0
  WHERE
    A.name = N'{tableName}'
) EXEC sys.sp_addextendedproperty @name = N'MS_Description',
@value = N'{tableComment}',
@level0type = N'SCHEMA',
@level0name = N'{tableSchema}',
@level1type = N'TABLE',
@level1name = N'{tableName}';
EXEC sp_updateextendedproperty @name = N'MS_Description',
@value = N'{tableComment}',
@level0type = N'SCHEMA',
@level0name = N'{tableSchema}',
@level1type = N'TABLE',
@level1name = N'{tableName}';
            ";
                    break;
                case SharedEnum.TypeDB.PostgreSQL:
                    tableSchema ??= "public";

                    result = $"COMMENT ON TABLE {tableSchema}.\"{tableName}\" IS '{tableComment.OfSql()}'";
                    break;
            }

            return result;
        }

        /// <summary>
        /// 设置列注释
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="databaseName">数据库名</param>
        /// <param name="tableSchema">模式</param>
        /// <param name="tableName">表名</param>
        /// <param name="columnName">列名</param>
        /// <param name="columnComment">列注释</param>
        /// <returns></returns>
        public static string SetColumnComment(SharedEnum.TypeDB tdb, string databaseName, string tableSchema, string tableName, string columnName, string columnComment)
        {
            string result = null;

            columnComment = columnComment.OfSql();
            switch (tdb)
            {
                case SharedEnum.TypeDB.SQLite:
                    result = "";
                    break;
                case SharedEnum.TypeDB.MySQL:
                case SharedEnum.TypeDB.MariaDB:
                    result = "";
                    break;
                case SharedEnum.TypeDB.Oracle:
                    result = "";
                    break;
                case SharedEnum.TypeDB.SQLServer:
                    tableSchema ??= "dbo";

                    result = $@"
USE [{databaseName}];
IF NOT EXISTS (
  SELECT
    C.value AS column_description
  FROM
    sys.tables A
    INNER JOIN sys.columns B ON B.object_id = A.object_id
    INNER JOIN sys.extended_properties C ON C.major_id = B.object_id
    AND C.minor_id = B.column_id
  WHERE
    A.name = N'{tableName}'
    AND B.name = N'{columnName}'
) EXEC sys.sp_addextendedproperty @name = N'MS_Description',
@value = N'{columnComment}',
@level0type = N'SCHEMA',
@level0name = N'{tableSchema}',
@level1type = N'TABLE',
@level1name = N'{tableName}',
@level2type = N'COLUMN',
@level2name = N'{columnName}';
EXEC sp_updateextendedproperty @name = N'MS_Description',
@value = N'{columnComment}',
@level0type = N'SCHEMA',
@level0name = N'{tableSchema}',
@level1type = N'TABLE',
@level1name = N'{tableName}',
@level2type = N'COLUMN',
@level2name = N'{columnName}';
            ";
                    break;
                case SharedEnum.TypeDB.PostgreSQL:
                    result = "";
                    break;
            }

            return result;
        }
    }
}

#endif
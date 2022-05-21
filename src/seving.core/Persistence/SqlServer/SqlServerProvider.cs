using Dapper;
using Lendsum.Crosscutting.Common;
using Microsoft.Extensions.Options;
using seving.core.Locks;
using seving.core.Utils.Extensions;
using seving.core.Utils.Serializer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;


namespace seving.core.Persistence.SqlServer
{
    /// <summary>
    /// Persistence provider using sql server as storage
    /// </summary>
    public class SqlServerProvider : IPersistenceProvider
    {
        private HashSet<string> sequencesDetected = new HashSet<string>();
        private HashSet<string> existingTablesCache = new HashSet<string>();
        private ITextSerializer serializer;
        private static int initialized = 0;
        private string connectionString;
        private SqlTransaction? transaction { get; set; }
        private IOptions<SqlServerProviderConfig> config;
        private Random random;

        /// <summary>
        /// The last character
        /// </summary>
        public string LastCharacter => @"ÿ";

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerProvider"/> class.
        /// </summary>
        public SqlServerProvider(ITextSerializer serializer, IOptions<SqlServerProviderConfig> config)
        {
            this.config = config;
            this.random = new Random(DateTime.UtcNow.Millisecond);
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.connectionString = config?.Value?.SevingConnectionString ?? throw new ArgumentNullException("there is no conneciton string with name seving");
            InitDatabase();
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public async Task<PersistenceResultEnum> Delete(IPersistable item)
        {
            string table = item.Partition;
            CreateIfNeeded(table);

            Func<SqlConnection, Task<PersistenceResultEnum>> func = async (conn) =>
            {
                var rows = await conn.ExecuteAsync("DELETE FROM [seving].[$TableName$] Where DocumentKey=@param1 "
                        .Replace("$TableName$", ValidateTableName(table))
                    , new { param1 = item.Keys.Key }, this.transaction);
                return rows <= 1 ? PersistenceResultEnum.Success : PersistenceResultEnum.DocumentOutOfDate;
            };

            return await this.RunQuery(func);
        }

        /// <summary>
        /// Deletes the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        public async Task<PersistenceResultEnum> Delete(IEnumerable<IPersistable> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (!items.Any()) return PersistenceResultEnum.Success;

            if (items.Select(x => x.Partition).Distinct().Count() != 1)
            {
                throw new ArgumentException("All items must be contained in the same table", nameof(items));
            }

            var table = items.First().Partition;
            CreateIfNeeded(items.First().Partition);

            Func<SqlConnection, Task<PersistenceResultEnum>> func = async (conn) =>
            {
                var rows = await conn.ExecuteAsync("DELETE FROM [seving].[$TableName$] Where DocumentKey in @param1 "
                        .Replace("$TableName$", table)
                    , new { param1 = items.Select(x => x.Keys.Key) }, this.transaction);
                return rows <= items.Count() ? PersistenceResultEnum.Success : PersistenceResultEnum.DocumentOutOfDate;
            };

            return await this.RunQuery(func);
        }

        /// <summary>
        /// Deletes all keys and values from the persistence layer.
        /// </summary>
        public async Task DeleteAll()
        {
            Func<SqlConnection, Task<IEnumerable<string>>> getTables = async (conn) =>
                await conn.QueryAsync<string>("SELECT TABLE_NAME FROM information_schema.tables WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA = 'seving'", null, this.transaction);

            var tables = await this.RunQuery(getTables);

            Func<SqlConnection, Task<PersistenceResultEnum>> func = async (conn) =>
            {
                List<Task> tasks = new List<Task>();
                foreach (var table in tables)
                {
                    tasks.Add(conn.ExecuteAsync("DELETE FROM [seving].[" + table + "]", null, this.transaction));
                }

                await Task.WhenAll(tasks.ToArray());
                return PersistenceResultEnum.Success;
            };

            await this.RunQuery(func);
        }

        /// <summary>
        /// Gets the value stored in the key provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public async Task<T?> GetValue<T>(IPersistable key) where T : class
        {
            var table = key.Partition;
            CreateIfNeeded(table);

            Func<SqlConnection, Task<IEnumerable<string>>> func = async (conn) => await conn.QueryAsync<string>(
                "select JsonDecompressed from [seving].[$TableName$] Where DocumentKey = @param1 "
                    .Replace("$TableName$", ValidateTableName(table)),
                new { param1 = key.Keys.Key },
                this.transaction
            );

            var list = await this.RunQuery(func);
            if (list.Count() > 1)
                throw new SevingException("A key should only have an associated value, but several were found");

            if (!list.Any())
                return default(T);

            string raw = list.First();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return default(T);
            }

            var result = (T?)(this.serializer.Deserialize<T>(raw) as IPersistable);
            return result;
        }

        /// <summary>
        /// Gets the values stored in the keys provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetValues<T>(IEnumerable<IPersistable> keys) where T : class
        {

            var partitions = keys.Select(x => x.Partition).Distinct();
            if (partitions.Count() != 1) throw new SevingException("The partition must be the same for all keys");
            var table = partitions.First();
            CreateIfNeeded(table);
            string[] keysToSelect = keys.Select(x => x.Keys.Key).ToArray();

            Func<SqlConnection, Task<IEnumerable<string>>> func = async (conn) => await conn.QueryAsync<string>(
                "select JsonDecompressed from [seving].[$TableName$] Where DocumentKey in @param1 "
                    .Replace("$TableName$", ValidateTableName(table)),
                new { param1 = keysToSelect },
                this.transaction
            );

            var list = await this.RunQuery(func);

            if (!list.Any())
                return Enumerable.Empty<T>();

            IEnumerable<T> result = list.Select(x => (this.serializer.Deserialize<T>(x))).ToArray();
            return result;
        }

        public async Task<BatchQuery<T>> GetByKeyPattern<T>(IPersistable startItem, IPersistable endItem, int? limit = null, bool includeKeys = false, bool asc = true) where T : IPersistable
        {
            string table = startItem.Partition;
            if (table != endItem.Partition)
            {
                throw new ArgumentException("The startItem and endItem must have the same partition");
            }

            BatchQuery<T> result = new BatchQuery<T>()
            {
                Ascendent = asc,
                IncludeKeys = includeKeys,
                Limit = limit,
                EndKey = endItem.Keys.Key,
                StartKey = startItem.Keys.Key,
                LastKey = String.Empty,
                Items = Enumerable.Empty<T>(),
                Partition = table
            };

            result = await this.GetByKeyPattern(result);
            return result;
        }

        public async Task<BatchQuery<T>> GetByKeyPattern<T>(BatchQuery<T> batchQuery) where T : IPersistable
        {
            string table = batchQuery.Partition;

            CreateIfNeeded(table);

            string moreThan = batchQuery.IncludeKeys ? ">=" : ">";
            string lessThan = batchQuery.IncludeKeys ? "<=" : "<";

            string query = "SELECT ";
            if (batchQuery.Limit != null) query = query + S.Invariant($"TOP {batchQuery.Limit}");
            query = query + @" JsonDecompressed
                from  [seving].[$TableName$]
                where DocumentKey" + moreThan + "@param1 and DocumentKey" + lessThan + "@param2";
            if (batchQuery.Ascendent == false)
            {
                query = query + " order by DocumentKey Desc";
            }

            query = query.Replace("$TableName$", table);
            string startKey = string.IsNullOrWhiteSpace(batchQuery.LastKey) ? batchQuery.StartKey : batchQuery.LastKey;
            string endKey = batchQuery.EndKey;

            Func<SqlConnection, Task<IEnumerable<string>>> func = async (conn) =>
            {
                var result = await conn.QueryAsync<string>(query, new { param1 = startKey, param2 = endKey }, this.transaction);
                return result;
            };

            var raws = await this.RunQuery(func);
            var resultItems = raws.Select(x => serializer.Deserialize<T>(x)).ToArray();

            var result = batchQuery.Advance(resultItems);
            return result;
        }

        /// <summary>
        /// Inserts the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public async Task<PersistenceResultEnum> Insert(IPersistable item)
        {
            string table = item.Partition;
            CreateIfNeeded(table);

            if (string.IsNullOrWhiteSpace(item.Cas)) item.Cas = this.BuildNewCas;

            var raw = this.serializer.Serialize(item);
            try
            {
                Func<SqlConnection, Task<PersistenceResultEnum>> func = async (conn) =>
               {
                   await conn.ExecuteAsync(@"INSERT INTO [seving].[$TableName$]
                   ([DocumentKey]
                   ,[CAS]
                   ,[CreatedDate]
                   ,[LastDateModified]
                   ,[JsonCompressed]) VALUES (
                    @param1,
                    @param2,
                    GETUTCDATE(),
                    GETUTCDATE(),
                    COMPRESS(@param3))".Replace("$TableName$", ValidateTableName(table)),
                     new { param1 = item.Keys.Key, param2 = item.Cas, param3 = raw },
                     transaction: this.transaction);

                   return PersistenceResultEnum.Success;
               };

                return await this.RunQuery(func);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    return await Task.FromResult(PersistenceResultEnum.KeyAlreadyExist);
                }

                throw;
            }
        }


        /// <summary>
        /// Reserves the counter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public async Task<ulong> ReserveCounter(string key)
        {
            await this.CreateSequenceIfNotExists(key);

            Func<SqlConnection, Task<ulong>> func = async (conn) =>
            {
                return await conn.ExecuteScalarAsync<ulong>(S.Invariant($"SELECT NEXT VALUE FOR seving.{key};"), null, this.transaction);
            };

            return await this.RunQuery(func);
        }

        /// <summary>
        /// Update hte value in the current key or in the key related to the persistable item if the current key is null.
        /// Use current key to replace the document key with the new one contained in the item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="currentKey"></param>
        /// <returns></returns>
        /// <exception cref="SevingException"></exception>
        public async Task<PersistenceResultEnum> Update(IPersistable item, ComposedKey? currentKey = null)
        {

            string table = item.Partition;
            CreateIfNeeded(table);
            string currentDocumentKey = currentKey?.Key ?? item.Keys.Key;

            Func<SqlConnection, Task<PersistenceResultEnum>> func;
            item.Cas = BuildNewCas;
            var raw = this.serializer.Serialize(item);
            var command = @"
                update [seving].[$TableName$]
                SET [DocumentKey]=@documentKey,
                    [LastDateModified]=GETUTCDATE,
                    [CAS]=@cas,
                    [JsonCompressed]=COMPRESS(@jsonValue)
                WHERE 
                    [DocumentKey]=@currentDocumentKey";

            if (!string.IsNullOrWhiteSpace(item.Cas))
            {
                command = command + " and [CAS]=@oldCas";
            }

            func = async (conn) =>
            {
                var queryResult = await conn.QueryAsync(command,
               new
               {
                   documentKey = item.Keys.Key,
                   cas = item.Cas,
                   jsonValue = raw,
                   currentDocumentKey = currentDocumentKey
               },
               this.transaction
               );

                switch (queryResult.Count())
                {
                    case 1:
                        return PersistenceResultEnum.Success;
                    case 0:
                        return PersistenceResultEnum.DocumentOutOfDate;
                    default:
                        return PersistenceResultEnum.NonDefinedError;
                }
            };

            try
            {
                return await this.RunQuery(func);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    return await Task.FromResult(PersistenceResultEnum.KeyAlreadyExist);
                }

                throw;
            }
        }

        /// <summary>
        /// Gets the lock.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="maxExecutingInSeconds">The maximum executing in seconds.</param>
        /// <returns>The locker info, check the InProgress value to see if the locker has been adquired.</returns>
        /// <exception cref="EventSourcingException">The key already exists</exception>
        public async Task<bool> GetLock(string itemName, int maxExecutingInSeconds)
        {
            var mergeCommand = @"
                merge [seving].[Lockers] as target
                using (select @param1 as ItemName, @param2 as MaxSeconds) as source
                on (target.ItemName=Source.ItemName)
                When matched and (target.InProgress = 0 or DATEADD(s,MaxSeconds,target.LockInitDate) < GETUTCDATE() )  then
                    Update Set
                    target.LockInitDate= GETUTCDATE(),
                    InProgress = 1
                When not matched by target then
                    insert (ItemName, LockInitDate, InProgress) values (source.ItemName, GETUTCDATE(), 1)
                output
                    $action, inserted.ItemName, inserted.InProgress;
                ";
            Func<SqlConnection, Task<bool>> func = async (conn) =>
            {
                var queryResult = await conn.QueryAsync(mergeCommand,
                    new
                    {
                        param1 = itemName,
                        param2 = maxExecutingInSeconds
                    },
                    this.transaction
                );

                switch (queryResult.Count())
                {
                    case 1:
                        var itemOutput = queryResult.First();
                        return itemOutput.InProgress;

                    case 0:
                        return false;
                    default:
                        throw new SevingException("Can not be different number of actions than 0 or 1");
                }
            };
            try
            {
                var lockerInfo = await this.RunQuery(func);
                return lockerInfo;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    return false;
                }

                throw;
            }
        }

        /// <summary>
        /// Release the lock
        /// </summary>
        /// <param name="itemName">The name of the locker.</param>
        public async Task ReleaseLock(string itemName)
        {
            var mergeCommand = @"
                merge [seving].[Lockers] as target
                using (select @param1 as ItemName) as source
                on (target.ItemName=Source.ItemName)
                When matched and target.InProgress = 1 then
                    Update Set
                    InProgress = 0
                output
                    $action, inserted.ItemName;
                ";
            Func<SqlConnection, Task<PersistenceResultEnum>> func;

            func = async (conn) =>
            {
                var queryResult = await conn.QueryAsync(mergeCommand,
                                   new
                                   {
                                       param1 = itemName
                                   },
                                   this.transaction
                                   );

                switch (queryResult.Count())
                {
                    case 1:
                        return PersistenceResultEnum.Success;
                    case 0:
                        return PersistenceResultEnum.DocumentOutOfDate;
                    default:
                        return PersistenceResultEnum.NonDefinedError;
                }
            };
            try
            {
                await this.RunQuery(func);
            }
            catch (SqlException)
            {
                throw;
            }
        }
        private void InitDatabase()
        {
            if (initialized != 0) return;
            if (Interlocked.Exchange(ref initialized, 1) != 0) return;

            using (var conn = this.GetNewConnection().Result)
            {

                var tableCount = conn.ExecuteScalar<int>("select count(TABLE_NAME) from information_schema.tables where TABLE_NAME='Lockers'");
                if (tableCount == 0)
                {
                    try
                    {
                        conn.Execute(createScheme);
                        conn.Execute(createLockerTable);
                    }
                    catch { }
                }
            }
        }

        private async Task<TResult> RunQuery<TResult>(Func<SqlConnection, Task<TResult>> func)
        {
            if (this.transaction != null)
            {
                return await func(this.transaction.Connection);
            }
            else
            {
                using (var conn = await this.GetNewConnection())
                {
                    return await func(conn);
                }
            }
        }

        private async Task<SqlConnection> GetNewConnection()
        {
            var connection = new SqlConnection(this.connectionString);
            await connection.OpenAsync();
            return connection;
        }

        private async Task<IEnumerable<string>> DetectSequences(string name)
        {
            using (var conn = await this.GetNewConnection())
            {
                var sequences = await conn.QueryAsync<string>("SELECT seq.name as name from sys.sequences as seq where name=@param1", new { param1 = name });
                foreach (var sequence in sequences)
                {
                    lock (this.sequencesDetected)
                    {
                        this.sequencesDetected.Add(sequence);
                    }
                }

                return sequences;
            }
        }

        private async Task CreateSequenceIfNotExists(string name, int tries = 0)
        {
            if (this.sequencesDetected.Contains(name)) return;
            await this.DetectSequences(name);
            if (this.sequencesDetected.Contains(name)) return;

            try
            {
                using (var conn = await this.GetNewConnection())
                {
                    await conn.ExecuteAsync(createSequence.Replace("%name%", name));
                }
            }
            catch
            {
                if (tries <= 10)
                {
                    await CreateSequenceIfNotExists(name, tries + 1);
                }
                else
                {
                    throw;
                }
            }

            lock (this.sequencesDetected)
            {
                this.sequencesDetected.Add(name);
            }
        }

        public async Task<IPersistenceProvider> BeginScope()
        {
            var result = new SqlServerProvider(this.serializer, this.config);
            var connection = await this.GetNewConnection();
            result.transaction = connection.BeginTransaction();
            return result;
        }

        /// <summary>
        /// Commits this instance.
        /// </summary>
        public async Task Commit()
        {
            if (this.transaction == null) throw new SevingException("The transaction is not set");

            var connection = this.transaction?.Connection;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            await this.transaction.CommitAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            this.transaction?.Connection?.Dispose();
            connection?.Dispose();
            this.transaction = null;
        }

        /// <summary>
        /// Rollbacks this instance.
        /// </summary>
        public async Task Rollback()
        {
            if (this.transaction == null) throw new SevingException("The transaction is not set");
            var connection = this.transaction?.Connection;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            await this.transaction.RollbackAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            this.transaction.Connection?.Dispose();
            connection?.Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.transaction != null)
            {
                var connection = this.transaction?.Connection;
                this.transaction?.Connection?.Dispose();
                this.transaction?.Dispose();
                connection?.Dispose();
            }
        }


        // Creates an events table if one does not exist. This method cannot be awaitable
        private void CreateIfNeeded(string tableName)
        {
            lock (existingTablesCache)
            {
                if (existingTablesCache.Contains(tableName)) return;

                using (var c = this.GetNewConnection().Result)
                {
                    switch (c.ExecuteScalar<int>(this._existsTable, new { table = tableName }))
                    {
                        // Can another thread create the table while we check if it exists? Extra concurrency control may be needed.
                        case 0:
                            c.Execute(this.createTable.Replace("$TableName$", ValidateTableName(tableName)));
                            this.existingTablesCache.Add(tableName);
                            return;

                        case 1:
                            this.existingTablesCache.Add(tableName);
                            return;

                        default:
                            throw new SevingException("Found several tables with same name: " + tableName);
                    }
                }
            }
        }

        // Just in case, the world is dangerous, and SQL inyections are scary.
        private static string ValidateTableName(String tablename)
        {
            if (!Regex.IsMatch(tablename, "[A-Za-z0-9_]+")) throw new SevingException("Invalid table name (DocumentType): " + tablename);
            return tablename;
        }

        private string createScheme = @"
            CREATE SCHEMA seving;";

        private string createTable = @"
            CREATE TABLE [seving].[$TableName$](
            [DocumentKey] [nvarchar](200) NOT NULL,
            [CreatedDate] [datetime] NOT NULL,
            [LastDateModified] [datetime] NOT NULL,
            [CAS] [bigint] NOT NULL,
            [JsonCompressed] [varbinary](max) NOT NULL,
            [JsonDecompressed] AS CAST(DECOMPRESS(JsonCompressed) AS nvarchar(max))
            CONSTRAINT [PK_$TableName$] PRIMARY KEY CLUSTERED
            (
                [DocumentKey] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
            ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
            ";

        private string createLockerTable = @"
            CREATE TABLE [seving].[Lockers](
            [ItemName] [nvarchar](200) NOT NULL,
            [LockInitDate][datetime] NOT NULL,
            [InProgress]  [bit] NOT NULL

            CONSTRAINT [PK_Lockers] PRIMARY KEY CLUSTERED
            (
                [ItemName] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
            ) ON [PRIMARY];
            ";

        private string createSequence = @"
            CREATE SEQUENCE seving.%name% start with 1 increment by 1;";

        private string _existsTable = @" select count (*) 
                                        from information_schema.tables 
                                        where table_name = @table";
        private string BuildNewCas => this.random.Next(0, 9999999).ToString(CultureInfo.InvariantCulture);
    }
}
namespace PokerTell.DatabaseSetup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using log4net;

    using NHibernate.Tool.hbm2ddl;

    using PokerTell.Infrastructure;
    using PokerTell.Infrastructure.Interfaces.DatabaseSetup;

    using Properties;

    public class EmbeddedManagedDatabase : IManagedDatabase
    {
        const string FileExtension = "db3";

        static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly string _databaseDirectory;

        readonly IDataProvider _dataProvider;

        readonly IDataProviderInfo _dataProviderInfo;

        public EmbeddedManagedDatabase(IDataProvider dataProvider, IDataProviderInfo dataProviderInfo)
        {
            _dataProviderInfo = dataProviderInfo;
            _dataProvider = dataProvider;

            _databaseDirectory = string.Format("{0}\\{1}\\", Files.LocalUserAppDataPath, Files.DataFolder);

            ValidateDatabaseDirectoryAndRecreateIfNeccessary();
        }

        public string ConnectionString { get; private set; }

        public IDataProviderInfo DataProviderInfo
        {
            get { return _dataProviderInfo; }
        }

        public IManagedDatabase ChooseDatabase(string databaseName)
        {
            ConnectionString = "data source = " + FullPathFor(databaseName);

            _dataProvider.Connect(ConnectionString, _dataProviderInfo);

            return this;
        }

        public IManagedDatabase CreateDatabase(string databaseName)
        {
            return ChooseDatabase(databaseName);
        }

        public IManagedDatabase CreateTables()
        {
            _dataProvider.BuildSessionFactory();

            new SchemaExport(_dataProvider.NHibernateConfiguration)
                .Execute(false, true, false, _dataProvider.Connection, null);

            return this;
        }

        public IManagedDatabase VersionDatabase(string databaseName)
        {
            _dataProvider.ExecuteNonQuery(string.Format(Resources.Sql_Queries_Embedded_InsertVersionNumber, ApplicationProperties.VersionNumber));
            return this;
        }

        public bool DatabaseExists(string databaseName)
        {
            return File.Exists(FullPathFor(databaseName));
        }

        public IManagedDatabase DeleteDatabase(string databaseName)
        {
            File.Delete(FullPathFor(databaseName));

            return this;
        }

        public IEnumerable<string> GetAllPokerTellDatabaseNames()
        {
            var dirInfo = new DirectoryInfo(_databaseDirectory);

            return dirInfo.GetFiles("*." + FileExtension).Select(fi => GetDatabaseNameFromPath(fi.FullName));
        }

        public string GetNameFor(string databaseInUse)
        {
             return GetDatabaseNameFromPath(databaseInUse);
        }

        static string DatabaseNameWithAddedFileExtension(string databaseName)
        {
            return databaseName + "." + FileExtension;
        }

        static string GetDatabaseNameFromPath(string fullPath)
        {
            string fileName = GetFileNameFrom(fullPath);
            return RemoveFileExtensionFrom(fileName);
        }

        static string GetFileNameFrom(string fullPath)
        {
            bool fileExists = File.Exists(fullPath);

            if (fileExists)
            {
                var fileInfo = new FileInfo(fullPath);
                return fileInfo.Name;
            }

            throw new FormatException("Full Path of database file: " + fullPath);
        }

        static string RemoveFileExtensionFrom(string theFileName)
        {
            int indexOflastDot = theFileName.LastIndexOf('.');
            bool foundLastDot = indexOflastDot > 0;

            if (foundLastDot)
            {
                return theFileName.Substring(0, indexOflastDot);
            }

            throw new FormatException("FileName: " + theFileName);
        }

        string FullPathFor(string databaseName)
        {
            return _databaseDirectory + DatabaseNameWithAddedFileExtension(databaseName);
        }

        void ValidateDatabaseDirectoryAndRecreateIfNeccessary()
        {
            if (!Directory.Exists(_databaseDirectory))
            {
                Log.Debug(_databaseDirectory + " was not found and therefore was created.");
                Directory.CreateDirectory(_databaseDirectory);
            }
        }
    }
}
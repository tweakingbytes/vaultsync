// Copyright © 2019-2023 Simon Knight
// This file is part of VaultSync.

// VaultSync is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.

// VaultSync is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with VaultSync.  If not, see<https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.SQLite;
using System.IO;


namespace VaultSync
{

    public class FileDetail
    {
        // Data class returned by a number of database queries
        public FileDetail(string path, string encryptedName, Int64 size, Int64 createDate, Int64 modDate, Int64 parent, string host)
        {
            Path = path;
            EncryptedName = encryptedName;
            Size = size;
            CreateDate = createDate;
            ModDate = modDate;
            Parent = parent;
            Host = host;
        }


        public string Path { get; set; }
        public string EncryptedName { get; set ; }
        public long Size { get; set ; }
        public long CreateDate { get; set; }
        public long ModDate { get; set; }
        public long Parent { get; set; }
        public string Host { get; set; }

        public bool IsFolder { get => string.IsNullOrEmpty(EncryptedName); }
    }

    // Handle potential nested tansactions
    public class NestedTransaction
    {
        private readonly SQLiteTransaction tran = null;

        public static NestedTransaction Create(SQLiteConnection db) {
            return new NestedTransaction(db);
        }

        private NestedTransaction(SQLiteConnection db) { 
            try
            {
                tran = db.BeginTransaction();
            }
            catch (InvalidOperationException)
            {
                // Do nothing, we are already in a transaction
            }
        }
         
        public void Commit()
        {
            if (tran != null)
            {
                tran.Commit();
            }
        }

        public void Rollback()
        {
            if (tran != null)
            {
                tran.Rollback();
            }
        }

    }

    // Wrap all database operations in this DataConnector class
    public class DataConnector
    {
        public enum SyncType { Folder, File } // Type for file syncing, either a folder or a file

        private readonly string connectionString;
        SQLiteConnection db; //Instance of SQLite

        public bool DataChanged { get; set; } // Track database changes so the DB is not copied and re-encrypted unneccessarily

        public DataConnector(string dbPath)
        {
            connectionString = "Data Source=" + dbPath;
            db = new SQLiteConnection(connectionString);
            db.Open();
            DataChanged = false;
        }

        // Check if a query can be run against the open database
        // If the decryption failed then a database will have been created and opened but won't be valid
        public bool IsValid()
        {
            try
            {
                var cmd = db.CreateCommand();

                cmd.CommandText = "select id from folder limit 1";
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                }
            }
            catch (SQLiteException)
            {
                return false;
            }
            return true;
        }

        // The database file is copied to its encrypted form in the vault but SQLite has to have released it first, which isn't as simple as it should be.
        public void Close()
        {
            if (db != null)
            {
                db.Close();
                db = null;
                GC.Collect(); // There are known problems with the file being released. Stack overflow also recommends this
                GC.WaitForPendingFinalizers();
            }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        public void CreateDBIfNecessary()
        {
            NestedTransaction tran = NestedTransaction.Create(db);
            var cmd = db.CreateCommand();

            // See if there are any tables in the master database
            cmd.CommandText = "select * from sqlite_master limit 1";
            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                {
                    reader.Close();
                    // Create the tables and starting data
                    cmd.CommandText = @"
                create table directory (
                path varchar(260) not null,
                host varchar(50) not null,
                encryptedname varchar(40) not null,
                size long not null,
                moddate long not null,
                parent integer not null,
                primary key(path, host)
                );

                create index if not exists dir_parent_idx on directory (parent);

                create table sync (
                host varchar(50) not null,
                path varchar(260) not null,
                type int not null,
                primary key (host, path)
                );

                create table folder (
                id integer not null primary key,
                name varchar(260) not null,
                parent integer not null,
                unique (name, parent)
                );

                create table params (
                name varchar(30) not null primary key,
                value varchar(100) not null
                );

                ";
                    cmd.ExecuteNonQuery();
                }
            }
            tran.Commit();
        }

        // Fix up the schema with items that may not be in place when the database was created
        public void UpdateSchema()
        {
            lock (db)
            {
                NestedTransaction tran = NestedTransaction.Create(db);
                try
                {
                    int version = 0;
                    string versionStr = GetParameter("version");
                    if (versionStr != null)
                    {
                        version = Int32.Parse(versionStr);
                    }

                    bool change = false;
                    if (version < 1) {
                        var cmd = db.CreateCommand();

                        cmd.CommandText = @"
                        create index if not exists dir_parent_idx on directory (parent);
                        create index if not exists folder_parent_idx on folder (parent);
                        create table if not exists ignore ( pattern varchar(260) not null primary key ); 
                        alter table directory add column createdate integer;
                        ";
                        cmd.ExecuteNonQuery();
                        change = true;
                    }
                    else if (version < 2)
                    {
                    }

                    if (change)
                    {
                        UpdateParameter("version", "1");
                    }

                    tran.Commit();
                }
                catch (SQLiteException)
                {
                    tran.Rollback();
                }
            }
        }

        // Get the details of the named file 
        public FileDetail GetFileDetails(string path)
        {
            lock (db)
            {
                FileDetail result = null;
                try
                {
                    var cmd = db.CreateCommand();

                    cmd.CommandText = "select path, encryptedname, size, ifnull(createdate, 0) as createdate, moddate, parent, host from directory where path = :path";
                    cmd.Parameters.AddWithValue("path", path);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            result = GenFileDetails(reader);
                        }
                    }
                }
                catch (SQLiteException)
                {
                }
                return result;
            }
        }

        // Pull the directory parts out of the file path and link them in a linked folder hierachy
        // If the folders don't exist then create them
        // Return the db identifier of the direct parent of the file
        public Int64 FindOrCreatParent(string path)
        {
            string[] parts = path.Split(Path.DirectorySeparatorChar); // Split the directory on the directory separator

            Int64 parentId = 0; // Root directory has an id of 0
            for (int i = 0; i < parts.Length - 1; ++i) // skip the last part, its the file name
            {
                // Walk the directory structure following parent links
                Int64? nextParentId = GetFolderId(parts[i], parentId);
                if (nextParentId.HasValue)
                {
                    parentId = nextParentId.Value;
                }
                else
                {
                    parentId = CreateFolder(parts[i], parentId);
                }
            }

            return parentId;
        }

        // Fetch the identifer for the named folder with the given parent
        private Int64? GetFolderId(string folder, Int64 parentId)
        {
            lock (db)
            {
                var cmd = db.CreateCommand();

                cmd.CommandText = "select id from folder where name = :name and parent = :parent";
                cmd.Parameters.AddWithValue("name", folder);
                cmd.Parameters.AddWithValue("parent", parentId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt64(0);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        // Create a folder entry with the given name and parent
        // Return the identifier of the new folder
        private Int64 CreateFolder(string folder, Int64 parentId)
        {
            lock (db)
            {

                var cmd = db.CreateCommand();

                cmd.CommandText = "insert into folder (name, parent) values (:name, :parent); select id from folder where name = :name and parent = :parent";
                cmd.Parameters.AddWithValue("name", folder);
                cmd.Parameters.AddWithValue("parent", parentId);

                using (var reader = cmd.ExecuteReader())
                {
                    DataChanged = true;

                    // read the new identifier
                    if (reader.Read())
                    {
                        return reader.GetInt64(0);
                    }
                    throw new Exception(Strings.FolderCreateFailed);
                }
            }
        }

        // Unpack the SQL result into the file detail structure
        private static FileDetail GenFileDetails(SQLiteDataReader reader)
        {
            return new FileDetail(reader.GetString(0), reader.GetString(1), reader.GetInt64(2), reader.GetInt64(3), reader.GetInt64(4), reader.GetInt64(5), reader.GetString(6));
        }

        public void UpdateFileDetail(FileDetail detail)
        {
            lock (db)
            {
                var cmd = db.CreateCommand();

                cmd.CommandText = "update directory set size = :size, createdate = :createDate, moddate = :modDate where path = :path";
                cmd.Parameters.AddWithValue("path", detail.Path);
                cmd.Parameters.AddWithValue("size", detail.Size);
                cmd.Parameters.AddWithValue("createDate", detail.CreateDate);
                cmd.Parameters.AddWithValue("modDate", detail.ModDate);
                cmd.ExecuteNonQuery();
                DataChanged = true;
            }
        }

        public bool IndexExists(string encryptedName)
        {
            lock (db)
            {
                bool result = true; // fail safe so file isn't accidentally deleted
                try
                {
                    var cmd = db.CreateCommand();

                    cmd.CommandText = "select encryptedname from directory where encryptedname = :name";
                    cmd.Parameters.AddWithValue("name", encryptedName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        result = reader.Read(); // true if something read
                    }
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                }
                return result;
            }
        }

        public void InsertFileDetail(FileDetail detail)
        {
            lock (db)
            {
                var cmd = db.CreateCommand();

                cmd.CommandText = "insert into directory (path, encryptedname, size, createdate, moddate, parent, host) values (:path, :fn, :size, :createDate, :modDate, :parent, :host)";
                cmd.Parameters.AddWithValue("path", detail.Path);
                cmd.Parameters.AddWithValue("fn", detail.EncryptedName);
                cmd.Parameters.AddWithValue("size", detail.Size);
                cmd.Parameters.AddWithValue("createDate", detail.CreateDate);
                cmd.Parameters.AddWithValue("modDate", detail.ModDate);
                cmd.Parameters.AddWithValue("parent", detail.Parent);
                cmd.Parameters.AddWithValue("host", detail.Host);
                cmd.ExecuteNonQuery();
                DataChanged = true;
            }
        }
   
        public bool DeleteIgnorePattern(string pattern)
        {
            lock (db)
            {
                NestedTransaction tran = NestedTransaction.Create(db);
                try
                {
                    var cmd = db.CreateCommand();

                    cmd.CommandText = "delete from ignore where pattern = :pattern";
                    cmd.Parameters.AddWithValue("pattern", pattern);
                    cmd.ExecuteNonQuery();
                    tran.Commit();
                    DataChanged = true;
                    return true;
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                    tran.Rollback();
                    return false;
                }
            }
        }

        public bool InsertIgnoreItem(string pattern)
        {
            lock (db)
            {

                NestedTransaction tran = NestedTransaction.Create(db);
                try
                {
                    var cmd = db.CreateCommand();

                    cmd.CommandText = "insert into ignore (pattern) values (:pattern)";
                    cmd.Parameters.AddWithValue("pattern", pattern);
                    cmd.ExecuteNonQuery();
                    tran.Commit();
                    DataChanged = true;
                    return true;
                }
                catch (SQLiteException e)
                {
                    if (e.ResultCode != SQLiteErrorCode.Constraint) // Ignore duplicate unique keys
                    {
                        MessageBoxForm.Show(e.Message);
                    }
                    tran.Rollback();
                    return false;
                }
            }
        }

        // Peform an action on each item in the ignore list
        public void ListIgnoreItems(System.Action<string> action)
        {
            lock (db)
            {
                try
                {
                    var cmd = db.CreateCommand();

                    cmd.CommandText = "select pattern from ignore";
                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            string pattern = reader.GetString(0);
                            action(pattern);
                        }
                    }
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                }
            }
        }

        // Perform an action on each item in the sync list passing it the sync details and a sub-job
        public void ListSyncPoints(System.Action<string, string, SyncType, Action<string>> action, Action<string> job)
        {
            lock (db)
            {
                try
                {
                    var cmd = db.CreateCommand();

                    cmd.CommandText = "select path, type, host from sync";
                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            string path = reader.GetString(0);
                            int type = reader.GetInt16(1);
                            string host = reader.GetString(2);

                            action(host, path, type == 0 ? SyncType.Folder : SyncType.File, job);
                        }
                    }
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                }
            }
        }

        public bool InsertSyncPoint(string host, string path, SyncType type)
        {
            lock (db)
            {
                bool result = true;
                NestedTransaction tran = NestedTransaction.Create(db);
                try
                {
                    var cmd = db.CreateCommand();

                    cmd.CommandText = "insert into sync (host, path, type) values (:host, :path, :type)";
                    cmd.Parameters.AddWithValue("path", path);
                    cmd.Parameters.AddWithValue("host", host);
                    cmd.Parameters.AddWithValue("type", type == SyncType.Folder ? 0 : 1);
                    cmd.ExecuteNonQuery();
                    tran.Commit();
                    DataChanged = true;
                }
                catch (SQLiteException e)
                {
                    if (e.ResultCode != SQLiteErrorCode.Constraint) // Ignore duplicate unique keys
                    {
                        MessageBoxForm.Show(e.Message);
                    }
                    tran.Rollback();
                    result = false;
                }
                return result;
            }
        }

        public void DeleteSyncPoint(string host, string path)
        {
            lock (db)
            {
                NestedTransaction tran = NestedTransaction.Create(db);
                try
                {
                    var cmd = db.CreateCommand();

                    cmd.CommandText = "delete from sync where host = :host and path = :path";
                    cmd.Parameters.AddWithValue("path", path);
                    cmd.Parameters.AddWithValue("host", host);
                    cmd.ExecuteNonQuery();

                    tran.Commit();
                    DataChanged = true;
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                    tran.Rollback();
                }
            }
        }

        public void DeleteFile(string path, string host)
        {
            lock (db)
            {

                NestedTransaction tran = NestedTransaction.Create(db);
                try
                {
                    var cmd = db.CreateCommand();

                    cmd.CommandText = "delete from directory where path = :path and host = :host";
                    cmd.Parameters.AddWithValue("path", path);
                    cmd.Parameters.AddWithValue("host", host);
                    cmd.ExecuteNonQuery();

                    tran.Commit();
                    DataChanged = true;
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                    tran.Rollback();
                }
            }
        }

        // When a file is deleted it may leave folder entries behind that have no associated files.
        // This method cleans out all the childless folders so they don't clutter the file display.
        // When deleted, folders also leave childless folders.
        public void DeleteChildlessFolders()
        {
            lock (db)
            {

                NestedTransaction tran = NestedTransaction.Create(db);
                try
                {
                    var cmd = db.CreateCommand();

                    while (ChildlessFoldersExist())
                    {
                        // delete folders that don't have any children
                        cmd.CommandText = @"
                    delete from folder
                    where id in (select id from folder f 
                        left outer join directory d on d.parent = f.id 
                        where not exists (select * from folder f2 where f2.parent = f.id) and path is null)";
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                    DataChanged = true;
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                    tran.Rollback();
                }
            }
        }

        private bool ChildlessFoldersExist()
        {
            lock (db)
            {

                var cmd = db.CreateCommand();
                cmd.CommandText = @"
                select id from folder f 
                left outer join directory d on d.parent = f.id 
                where not exists (select * from folder f2 where f2.parent = f.id) and path is null limit 1";

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read();
                }
            }
        }

        // Perform an action on each item in the sub-directory identified by the folder identifier
        // The current sub-directory is provided as a string
        public void ListContents(System.Action<FileDetail> action, Int64 folderId, string currentText)
        {
            lock (db)
            {
                try
                {
                    ListFilesInFolder(action, folderId, 0, 0, "", "path", "asc");
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                }
            }
        }


        // Perform an action on each item in the sub-directory identified by the folder identifier
        // The current sub-directory is provided as a string
        public void ListFiles(System.Action<FileDetail> action, Int64 folderId, int limit, int offset, string text, string sortCol, string order)
        {
            lock (db)
            {
                try
                {
                    ListFilesInFolder(action, folderId, limit, offset, text, sortCol, order);
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                }
            }
        }

        public void MoveHost(string oldHost, string newHost)
        {
            lock (db)
            {
                var cmd = db.CreateCommand();

                cmd.CommandText = "update directory set host = :newhost where host = :oldhost";
                cmd.Parameters.AddWithValue("newhost", newHost);
                cmd.Parameters.AddWithValue("oldhost", oldHost);
                cmd.ExecuteNonQuery();
                DataChanged = true;
            }
        }

        public void ListFilesInFolder(Action<FileDetail> action, Int64 folderId, int limit, int offset, string text, string sortCol, string order)
        {
            lock (db)
            {

                var cmd = db.CreateCommand();

                cmd.CommandText = @"
                select path, encryptedname, size, createdate, moddate, parent, host
                from (select path, encryptedname, size, ifnull(createdate, 0) as createdate, moddate, parent, host
                from directory 
                where parent = :parent
                union all
                select (case when :txt = '' then name else :txt || '\' || name end) as path, '' as encryptedname, 0 as size, 0 as createdate, 0 as moddate, id as parent, '' as host
                from folder 
                where parent = :parent)";

                if (!string.IsNullOrEmpty(sortCol))
                {
                    cmd.CommandText += string.Format(" order by {0} {1}", sortCol, order);
                }

                if (limit != 0)
                {
                    cmd.CommandText += string.Format(" limit {0} offset {1}", limit, offset);
                }

                cmd.Parameters.AddWithValue("parent", folderId);
                cmd.Parameters.AddWithValue("txt", text);
                using (var reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        action(GenFileDetails(reader));
                    }
                }
            }
        }

        // Count each item in the sub-directory identified by the folder identifier
        public int CountContents(Int64 folderId)
        {
            lock (db)
            {
                int result = 0;
                try
                {
                    var cmd = db.CreateCommand();
                    cmd.CommandText = @"
                    select count(*) 
                    from directory 
                    where parent = :parent
                    union all
                    select count(*)
                    from folder
                    where parent = :parent";
                    cmd.Parameters.AddWithValue("parent", folderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result += reader.GetInt32(0);
                        }
                    }
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                }
                return result;
            }
        }

        private bool InsertParameterNoLock(string name, string value)
        {
            NestedTransaction tran = NestedTransaction.Create(db);
            try
            {
                var cmd = db.CreateCommand();

                cmd.CommandText = "insert into params (name, value) values (:name, :value)";
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("value", value);
                cmd.ExecuteNonQuery();
                tran.Commit();
                DataChanged = true;
                return true;
            }
            catch (SQLiteException)
            {
                tran.Rollback();
                return false;
            }
        }

        private bool UpdateParameterNoLock(string name, string value)
        {
            NestedTransaction tran = NestedTransaction.Create(db);
            try
            {
                var cmd = db.CreateCommand();

                cmd.CommandText = "update params set value = :value where name = :name";
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("value", value);
                cmd.ExecuteNonQuery();
                tran.Commit();
                DataChanged = true;
                return true;
            }
            catch (SQLiteException)
            {
                tran.Rollback();
                return false;
            }
        }

        public bool UpdateParameter(string name, string value)
        {
            lock (db)
            {
                string p = GetParameter(name);
                if (p == null)
                {
                    return InsertParameterNoLock(name, value);
                }
                else
                {
                    return UpdateParameterNoLock(name, value);
                }
            }
        }

        public string GetParameter(string name)
        {
            lock (db)
            {
                string result = null;
                try
                {
                    var cmd = db.CreateCommand();
                    cmd.CommandText = @"select value from params where name = :name";
                    cmd.Parameters.AddWithValue("name", name);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = reader.GetString(0);
                        }
                    }
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                }
                return result;
            }
        }

        public void Vacuum()
        {
            lock (db)
            {
                if (!DataChanged)
                {
                    // Don't bother with a vacuum if the data hasn't changed
                    return;
                }

                try
                {
                    var cmd = db.CreateCommand();
                    cmd.CommandText = @"vacuum";
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    MessageBoxForm.Show(e.Message);
                }
            }
        }
    }
}
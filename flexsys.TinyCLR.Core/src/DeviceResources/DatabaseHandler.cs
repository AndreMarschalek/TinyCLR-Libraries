using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Data.SQLite;

namespace flexsys.TinyCLR.Core
{
    public sealed partial class DeviceResources
    {
        public sealed class DatabaseHandler : ServiceExtensions
        {
            public static DatabaseHandler Instance { get; } = new DatabaseHandler();

            private static SQLiteDatabase _Database;
            private FileHandler _FH;

            public DatabaseHandler()
            {
            }

            public void Dispose()
            {
                _Database.Dispose();
                _FH.Dispose();
            }

            public void Database_Open(string file)
            {
                _FH = new FileHandler();
                _Database = new SQLiteDatabase(file);
                //_Database.Open(@"\SD\system\database.db3");
                DeviceResources.Instance.DatabaseReady.Set();
            }

            public ResultSet ExecuteQuery(string sql)
            {
                try
                {
                    if (sql[sql.Length] != ';')
                    {
                        sql += ";";
                    }
                    DebugPrint(this, "\r\n\tSQL : " + sql);
                    return _Database.ExecuteQuery(sql);
                }
                catch (Exception ex)
                {
                    ErrorPrint(this, ex.Message);
                    return null;
                }
            }

            public bool ExecuteNonQuery(string sql)
            {
                try
                {
                    if (sql[sql.Length] != ';')
                    {
                        sql += ";";
                    }
                    DebugPrint(this, "\r\n\tSQL : " + sql);
                    _Database.ExecuteNonQuery(sql);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorPrint(this, ex.Message);
                    return false;
                }
            }
        }
    }
}
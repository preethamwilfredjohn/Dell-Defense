using System.IO;
using System.Data.SqlClient;
using log4net;

namespace DellDefenseCore
{
    class FindingFiles
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void loadDB(string path, SqlConnection con1)
        {
            Hashing hash = new Hashing();
            foreach (string file in Directory.GetFiles(path))
            {
                try
                {
                    string hashedContent = Hashing.BytesToString(hash.GetHashSha256(file));
                    SqlCommand sc = new SqlCommand("INSERT into DellDefenseDB values(@fileName,@fileContent)", con1);
                    sc.Parameters.AddWithValue("fileName", file);
                    sc.Parameters.AddWithValue("fileContent", hashedContent);
                    log.Info("Inserting file details into database " + file);
                    sc.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    log.Error("Error while writing to database" + ex);
                }
            }
            foreach (string directory in Directory.GetDirectories(path))
            {
                loadDB(directory, con1);
            }
        }
    }
}

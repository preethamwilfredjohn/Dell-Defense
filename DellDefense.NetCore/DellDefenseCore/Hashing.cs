using System.IO;
using System.Security.Cryptography;

namespace DellDefenseCore
{
    class Hashing
    {
        //Hashing the contents of the file
        private SHA256 Sha256 = SHA256.Create();
        public byte[] GetHashSha256(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                return Sha256.ComputeHash(stream);
            }
        }

        //converting bytes to string
        public static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }
    }
}

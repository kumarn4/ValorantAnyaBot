using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ValorantAnyaBot.Data.Cryptography;

namespace ValorantAnyaBot.Data
{
    public class UserService
    {
        public UsersData Users;

        public UserService()
        {
            FileInfo userfile = new FileInfo("user.json");
            if (!userfile.Exists)
            {
                Users = new UsersData();
                Users.un_iv = RandomizedStringGenerateService.Generate(16);
                Users.pw_iv = RandomizedStringGenerateService.Generate(16);
                Users.un_ky = RandomizedStringGenerateService.Generate(32);
                Users.pw_ky = RandomizedStringGenerateService.Generate(32);

                File.Create(userfile.FullName).Close();

                File.WriteAllText(
                    userfile.FullName,
                    JsonSerializer.Serialize(
                        Users,
                        new JsonSerializerOptions()
                        {
                            IncludeFields = true
                        }));
            }

            Users = JsonSerializer.Deserialize<UsersData>(
                File.ReadAllText(userfile.FullName));
        }

        public string GetEncryptedUsername(string username)
        {
            return Encrypt(username, Users.un_iv, Users.un_ky);
        }
        public string GetEncryptedPassword(string password)
        {
            return Encrypt(password, Users.pw_iv, Users.pw_ky);
        }
        public string GetDecryptedUsername(string crypt_username)
        {
            return Decrypt(crypt_username, Users.un_iv, Users.un_ky);
        }
        public string GetDecryptedPassword(string crypt_password)
        {
            return Decrypt(crypt_password, Users.pw_iv, Users.pw_ky);
        }

        public UserData Get(ulong id)
        {
            return Users.datas.Find(x => x.id == id);
        }
        
        public bool Set(ulong id, string username, string password)
        {
            if (Get(id) != null) return false;
            UserData user = new UserData();
            user.id = id;
            user.encrypted_username = GetEncryptedUsername(username);
            user.encrypted_password = GetEncryptedPassword(password);
            Users.datas.Add(user);
            Save();
            return true;
        }

        public void Save()
        {
            Clear();
            File.WriteAllText(
                "user.json",
                JsonSerializer.Serialize(
                    Users,
                    new JsonSerializerOptions()
                    {
                        IncludeFields = true
                    }));
        }

        private void Clear()
        {
            using (FileStream fs = new FileInfo("user.json").Open(FileMode.Open))
            {
                fs.SetLength(0);
                fs.Flush();
            }
        }

        private string Encrypt(string text, string iv, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Key = Encoding.UTF8.GetBytes(key);

                ICryptoTransform e = aes.CreateEncryptor();
                byte[] dst;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(
                        ms, e, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(text);
                        }
                        dst = ms.ToArray();
                    }
                }
                return Convert.ToBase64String(dst);
            }
        }

        private string Decrypt(string crypt, string iv, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Key = Encoding.UTF8.GetBytes(key);

                ICryptoTransform e = aes.CreateDecryptor(aes.Key, aes.IV);
                string dst = "";
                using (MemoryStream ms = new MemoryStream(
                    Convert.FromBase64String(crypt)))
                {
                    using (CryptoStream cs = new CryptoStream(
                        ms, e, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            dst = sr.ReadLine();
                        }
                    }
                }
                return dst;
            }
        }

        public class UsersData
        {
            public List<UserData> datas { get; set; } = new List<UserData>();
            public string un_iv { get; set; }
            public string un_ky { get; set; }
            public string pw_iv { get; set; }
            public string pw_ky { get; set; }
        }
        public class UserData
        {
            public ulong id { get; set; }
            public string encrypted_username { get; set; }
            public string encrypted_password { get; set; }

        }
    }
}

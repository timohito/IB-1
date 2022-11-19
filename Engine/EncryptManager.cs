using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Security.Cryptography;

namespace IB1.Engine
{
    public class EncryptManager
    {
        //временный файл и зашифрованный
        private static readonly string UserStorageName = "..\\users.txt";
        private static readonly string UserStorageNameCrypted = "..\\usersEncrypt.txt";
        private List<User> users;
        private DESCryptoServiceProvider dESCryptoServiceProvider;

        //для кофига ключ и вектор инициализации в строковом представлении
        private static readonly string Key = "abcdefgh";
        private static readonly string IV = "hgfedcba";

        public EncryptManager()
        {
            //конфиг шифр-класса
            dESCryptoServiceProvider = new DESCryptoServiceProvider();
            dESCryptoServiceProvider.Mode = CipherMode.CFB;
            dESCryptoServiceProvider.Padding = PaddingMode.PKCS7;

            //проверка на наличие зашифрованного файла
            if (File.Exists(UserStorageNameCrypted))
            {
                //считываем данные из зашифрованного файла во временный, используя ключ и вектор инициализации
                byte[] bytes = File.ReadAllBytes(UserStorageNameCrypted);
                byte[] encr = dESCryptoServiceProvider.CreateDecryptor(Encoding.UTF8.GetBytes(Key), Encoding.UTF8.GetBytes(IV)).TransformFinalBlock(bytes, 0, bytes.Length);
                File.WriteAllBytes(UserStorageName, encr);

                string usersText = File.ReadAllText(UserStorageName);
                users = usersText.SafeDeserialize<List<User>>(null);

                //при исключении создаем админа без пароля
                if (users == null || users.Count == 0)
                {
                    users = new List<User>() { new User()
                    {
                        Login = "admin",
                        Blocked = false,
                        Password = "",
                        RestrictionPassword = false
                    } };
                }
            }
            else
            {
                users = new List<User>() { new User()
                {
                    Login = "admin",
                    Blocked = false,
                    Password = "",
                    RestrictionPassword = false
                } };
                //записываем во временный файл админа без пароля
                using (var sw = new StreamWriter(UserStorageName))
                {
                    sw.WriteLine(users.SafeSerialize(null));
                    sw.Close();
                }

            }
        }
        public void EncryptUsers()
        {
            // считываем данные из временного файла в зашифрованный, используя ключ и вектор инициализации
            byte[] bytes = File.ReadAllBytes(UserStorageName);
            byte[] eBytes = dESCryptoServiceProvider.CreateEncryptor(Encoding.UTF8.GetBytes(Key), 
                Encoding.UTF8.GetBytes(IV)).TransformFinalBlock(bytes, 0, bytes.Length);
            File.WriteAllBytes(UserStorageNameCrypted, eBytes);
            //удаляем временный файл
            File.Delete(UserStorageName);
        }
        //сохраняем пользователей во временный файл
        public List<User> GetUsers()
        {
            return users;
        }
        private void SaveUsers()
        {
            try
            {
                using (var sw = new StreamWriter(UserStorageName, false))
                {
                    sw.WriteLine(users.SafeSerialize(null));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void AddUser(User newUser)
        {
            try
            {
                users.Add(newUser);
                SaveUsers();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void EditUser(User editUser)
        {
            try
            {
                //находим и изменяем все поля
                foreach(var user in users)
                {    
                    if(user.Login.Equals(editUser.Login))
                    {
                        user.Blocked = editUser.Blocked;
                        user.Password = editUser.Password;  
                        user.RestrictionPassword = editUser.RestrictionPassword;
                    }
                }
                SaveUsers();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}

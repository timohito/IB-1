using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IB1.Engine;
using Unity;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;
using System.Security.Cryptography;

namespace IB1.Engine
{
    public class UserLogic
    {
        //Unity IoC контейнер, зависимость
        [Dependency]
        public IUnityContainer Container { get; set; }

        private List<User> users = new List<User>();
        private EncryptManager em;
        private SHA256CryptoServiceProvider hashProvider;

        /// <summary>
        /// соль для хеширования
        /// </summary>
        byte[] salt = Encoding.UTF8.GetBytes("qwerabcn");

        /// <summary>
        /// строка - регулярное выражение для ограничения на пароль
        /// </summary>
        private string regex = @"^\w+$";

        private User Admin;

        //в конструкторе также получаем всех пользователей
        public UserLogic(EncryptManager _em)
        {
            em = _em;
            hashProvider = new SHA256CryptoServiceProvider();
            LoadData();
        }
        public bool IsAdmin(User user)
        {
            if (user == null) 
            {
                return false;
            }

            return user.Login.Equals(Admin.Login) && HashPass(user.Password).Equals(Admin.Password);
        }

        public bool IsEmptyPass(string login)
        {
            foreach(var u in users)
            {
                if (login.Equals(u.Login) && u.Password.Equals(""))
                {
                    return true;
                }
                else if (login.Equals(u.Login) && !u.Password.Equals(""))
                {
                    return false;
                }
            }
            return false;
        }

        public bool ExistsUser(string login)
        {
            foreach (var u in users)
            {
                if (u.Login.Equals(login))
                {
                    return true;
                }
            }
            return false;
        }

        private User GetUserByLogin(string login)
        {
            foreach (var u in users)
            {
                if (u.Login.Equals(login))
                {
                    return u;
                }
            }
            return null;
        }

        public User ChangePassword(User user)
        {
            User u = GetUserByLogin(user.Login);

            if (!u.RestrictionPassword)
            {
                EditUser(user);
                return GetUserByLogin(user.Login);
            }
            else
            {
                if (!Regex.IsMatch(user.Password, regex))
                {
                    return null;                }
                else
                {
                    EditUser(user);
                    return GetUserByLogin(user.Login);
                }
            }
            return null;
        }

        // метод для сравнивания паролей
        public User MatchPassword(User user)
        {
            User u = GetUserByLogin(user.Login);
            if(u == null)
            {
                throw new Exception("Нет пользователя с таким логином");
            }
            if(u.Blocked)
            {
                throw new Exception("Функции заблокированы");
            }
            else
            {
                //если пароля не было и нет ограничений - редактируем пользователя
                if (string.IsNullOrEmpty(u.Password) && !u.RestrictionPassword)
                {
                    user.RestrictionPassword = u.RestrictionPassword;
                    user.Blocked = u.Blocked;
                    EditUser(user);
                    return user;
                }
                //если пароля не было и есть ограничения
                else if (string.IsNullOrEmpty(u.Password) && u.RestrictionPassword)
                {
                    if (!Regex.IsMatch(user.Password, regex))
                    {
                        throw new RegexException("Пароль должен состоять из букв или цифр");
                    }
                    else {
                        user.RestrictionPassword = u.RestrictionPassword;
                        user.Blocked = u.Blocked;
                        EditUser(user);
                        return user;
                    }
                }
                else
                {
                    //проверяем хешированные пароли
                    return u.Password.Equals(HashPass(user.Password)) ? user : null; 
                }  
                
            }
            return null;
        }

        //получение всех пользователей без админа
        public List<User> GetUsers()
        {
            return users.Where(x => !x.Login.Equals("admin")).ToList();
        }

        public void AddUser(User user)
        {
            if (user == null)
            {
                throw new Exception("Нет пользователя для добавления");
            }
            User existUser = GetUserByLogin(user.Login);
            if(existUser != null)
            {
                throw new Exception("Пользователь с таким именем уже существует");
            }
            em.AddUser(user);
            LoadData();
        }

        public void EditUser(User user, bool IsChahgePassword = true)
        {
            if(user == null)
            {
                throw new Exception("Нет пользователя для редактирования");
            }
            if (string.IsNullOrEmpty(user.Password))
            {
                em.EditUser(user);
            }
            else if (!IsChahgePassword)
            {
                em.EditUser(user);
            }
            else
            {
                em.EditUser(HashPass(user));
            }
            LoadData();
        }

        private void LoadData()
        {
            users = em.GetUsers();

            foreach(var user in users)
            {
                if(user.Login == "admin")
                {
                    Admin = user;
                }
            }
        }

        //методы для хеширования паролей
        private User HashPass(User user)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(user.Password);

            byte[] saltedInput = new byte[salt.Length + passwordBytes.Length];
            salt.CopyTo(saltedInput, 0);
            passwordBytes.CopyTo(saltedInput, salt.Length);

            byte[] hashedBytes = hashProvider.ComputeHash(saltedInput);

            user.Password = BitConverter.ToString(hashedBytes);

            return user;
        }

        //получаем байтовое представление пароля
        //получаем "соль" и совмещаем массивы байтов
        //возвращаем строковое представление
        private string HashPass(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            byte[] saltedInput = new byte[salt.Length + passwordBytes.Length];
            salt.CopyTo(saltedInput, 0);
            passwordBytes.CopyTo(saltedInput, salt.Length);

            byte[] hashedBytes = hashProvider.ComputeHash(saltedInput);

            return BitConverter.ToString(hashedBytes);
        }
        //деструктор при уничтожении класса зашифрует всех пользователей
        ~UserLogic()
        {
            em.EncryptUsers();
        }
        
    }
}

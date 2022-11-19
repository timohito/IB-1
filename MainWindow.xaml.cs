using IB1.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unity;

namespace IB1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //зависимость IoC - контейнера
        [Dependency]
        public IUnityContainer Container { get; set; }
        private readonly UserLogic _userLogic;
        public User user { set { User = value; } }
        private User User;
        public MainWindow(UserLogic userLogic)
        {
            InitializeComponent();
            _userLogic = userLogic;
        }


        private void buttonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxOldPassword.Password))
            {
                MessageBox.Show("Заполните старый пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(TextBoxNewPassword.Password))
            {
                MessageBox.Show("Введите новый пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //проверка в бизнес логике, на то, что хеши сходятся
            var userMatch= _userLogic.MatchPassword(new User
            {
                Login = User.Login,
                Blocked = User.Blocked,
                RestrictionPassword = User.RestrictionPassword,
                Password = TextBoxOldPassword.Password
            });
            if (userMatch == null) {
                MessageBox.Show("Старый пароль неверен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                User changePassword = new User
                {
                    Login = User.Login,
                    Password = TextBoxNewPassword.Password,
                    Blocked = User.Blocked,
                    RestrictionPassword = User.RestrictionPassword
                };
                var user = _userLogic.ChangePassword(changePassword);
                if (user != null)
                {
                    User = user;
                    TextBoxOldPassword.Clear();
                    TextBoxNewPassword.Clear();
                    MessageBox.Show("Пароль успешно изменен", "Сообщение",
              MessageBoxButton.OK, MessageBoxImage.Information);
                } 
                else
                {
                    MessageBox.Show("Пароль должен содержать буквы и цифры", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItemRef_Click(object sender, RoutedEventArgs e)
        {
            var window = Container.Resolve<RefWindow>();
            window.ShowDialog();
        }
        private void MenuItemUsers_Click(object sender, RoutedEventArgs e)
        {
            var window = Container.Resolve<UsersWindow>();
            window.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            labelUser.Content = User.Login;
            if (_userLogic.IsAdmin(User))
            {
                MenuItemUsers.Visibility = Visibility.Visible;
            }
            else
            {
                MenuItemUsers.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
              Close();
        }
    }
}

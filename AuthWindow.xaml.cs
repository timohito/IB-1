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
using System.Windows.Shapes;
using Unity;

namespace IB1
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        //зависимость IoC - контейнера
        [Dependency]
        public IUnityContainer Container { get; set; }
        private readonly UserLogic _userLogic;
        private int TryingInputPassword;
        public AuthWindow(UserLogic userLogic)
        {
            InitializeComponent();
            _userLogic = userLogic;
            TryingInputPassword = 0;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text))
            {
                MessageBox.Show("Заполните логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(!_userLogic.ExistsUser(TextBoxLogin.Text))
            {
                MessageBox.Show("Нет пользователя с таким логином", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(TextBoxPassword.Password))
            {
                MessageBox.Show("Заполните пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(TextBoxPasswordRetype.Visibility == Visibility.Visible && string.IsNullOrEmpty(TextBoxPasswordRetype.Password))
            {
                MessageBox.Show("Повторите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (TextBoxPasswordRetype.Visibility == Visibility.Visible && !TextBoxPasswordRetype.Password.Equals(TextBoxPassword.Password))
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                var user = new User
                {
                    Login = TextBoxLogin.Text,
                    Password = TextBoxPassword.Password
                };
                var matchUser = _userLogic.MatchPassword(user);
                if (matchUser != null)
                {
                    var window = Container.Resolve<MainWindow>();
                    window.user = matchUser;
                    window.Show();
                    TextBoxLogin.Clear();
                    TextBoxPassword.Clear();
                    TextBoxPasswordRetype.Clear();
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин/пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    TryingInputPassword++;
                    if (TryingInputPassword >= 3)
                    {
                        DialogResult = false;
                        Close();
                    }
                }
            }
            catch (RegexException ex)
            {
                var mb = MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                if(mb.Equals(MessageBoxResult.Cancel))
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
                             
        //при потере фокуса проверяем, заходит пользователь первый раз или нет
        private void TextBoxLogin_LostFocus(object sender, RoutedEventArgs e)
        {
            string login = TextBoxLogin.Text;
            if(!string.IsNullOrEmpty(login) && _userLogic.IsEmptyPass(login))
            {

                TextBoxPasswordRetype.Visibility = Visibility.Visible;
                LabelPasswordRetype.Visibility = Visibility.Visible;
            }
            else
            {
                TextBoxPasswordRetype.Visibility = Visibility.Collapsed;
                LabelPasswordRetype.Visibility = Visibility.Collapsed;
            }
        }
    }
}

using IB1.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Логика взаимодействия для AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        //зависимость IoC - контейнера
        [Dependency]
        public IUnityContainer Container { get; set; }
        private readonly UserLogic _userLogic;
        public AddUserWindow(UserLogic userLogic)
        {
            InitializeComponent();
            _userLogic = userLogic;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text))
            {
                MessageBox.Show("Заполните Имя пользователя", "Ошибка", MessageBoxButton.OK,
               MessageBoxImage.Error);
                return;
            }
            try
            {
                _userLogic.AddUser(new User()
                {
                   Login = TextBoxLogin.Text,
                   Password = "",
                });

                MessageBox.Show("Сохранение прошло успешно", "Сообщение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK,
               MessageBoxImage.Error);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}

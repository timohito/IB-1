using IB1.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
    /// Логика взаимодействия для UsersWindow.xaml
    /// </summary>
    public partial class UsersWindow : Window
    {
        //зависимость IoC - контейнера
        [Dependency]
        public IUnityContainer Container { get; set; }
        private readonly UserLogic _userLogic;
        public UsersWindow(UserLogic userLogic)
        {
            InitializeComponent();
            _userLogic = userLogic;
        }

        private void buttonBlocked_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridUsers.SelectedCells.Count != 0)
            {
                var result = MessageBox.Show("Заблокировать пользователя", "Вопрос", MessageBoxButton.YesNo,
               MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var cellInfo = dataGridUsers.SelectedCells[0];
                        User content = (User)(cellInfo.Item);
                        content.Blocked = true;
                        _userLogic.EditUser(content, false);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK,
                       MessageBoxImage.Error);
                    }
                    LoadData();
                }
            }
        }

        private void buttonEnableRestrict_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridUsers.SelectedCells.Count != 0)
            {
                var result = MessageBox.Show("Включить ограничения на пароль", "Вопрос", MessageBoxButton.YesNo,
               MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var cellInfo = dataGridUsers.SelectedCells[0];
                        User content = (User)(cellInfo.Item);
                        content.RestrictionPassword = true;
                        _userLogic.EditUser(content, false);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK,
                       MessageBoxImage.Error);
                    }
                    LoadData();
                }
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = Container.Resolve<AddUserWindow>();
            if (window.ShowDialog().Value)
            {
                LoadData();
            }
        }
        private void LoadData()
        {
            var list = _userLogic.GetUsers();
            if (list != null)
            {
                dataGridUsers.ItemsSource = list;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        /// <summary>
        /// Данные для привязки DisplayName к названиям столбцов в DataGrid
        /// </summary>
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string displayName = GetPropertyDisplayName(e.PropertyDescriptor);
            if (!string.IsNullOrEmpty(displayName))
            {
                e.Column.Header = displayName;
            }
            new DataGridLength(1, DataGridLengthUnitType.Star); 

        }
        /// <summary>
        /// метод привязки DisplayName к названию столбца
        /// </summary>
        public static string GetPropertyDisplayName(object descriptor)
        {

            PropertyDescriptor pd = descriptor as PropertyDescriptor;
            if (pd != null)
            {
                // Check for DisplayName attribute and set the column header accordingly
                DisplayNameAttribute displayName = pd.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
                if (displayName != null && displayName != DisplayNameAttribute.Default)
                {
                    return displayName.DisplayName;
                }

            }
            else
            {
                PropertyInfo pi = descriptor as PropertyInfo;
                if (pi != null)
                {
                    // Check for DisplayName attribute and set the column header accordingly
                    Object[] attributes = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    for (int i = 0; i < attributes.Length; ++i)
                    {
                        DisplayNameAttribute displayName = attributes[i] as DisplayNameAttribute;
                        if (displayName != null && displayName != DisplayNameAttribute.Default)
                        {
                            return displayName.DisplayName;
                        }
                    }
                }
            }
            return null;
        }
    }
}

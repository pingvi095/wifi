using System.Windows;

namespace wifi
{
    /// <summary>
    /// Окно выбора роли пользователя при запуске приложения.
    /// Позволяет выбрать режим "Пользователь" или "Администратор".
    /// </summary>
    public partial class RoleSelectionWindow : Window
    {
        /// <summary>
        /// Конструктор окна.
        /// Инициализирует компоненты интерфейса.
        /// </summary>
        public RoleSelectionWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Пользователь".
        /// Открывает главное окно приложения в обычном режиме.
        /// </summary>
        private void User_Click(object sender, RoutedEventArgs e)
        {
            var main = new MainWindow(); // создаём главное окно
            main.Show();                // показываем его
            Close();                     // закрываем окно выбора роли
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Администратор".
        /// Открывает окно авторизации администратора.
        /// При успешной авторизации открывает главное окно с правами администратора.
        /// </summary>
        private void Admin_Click(object sender, RoutedEventArgs e)
        {
            var login = new AdminLoginWindow(); // создаём окно логина для администратора
            login.ShowDialog();                 // показываем его модально

            // Проверяем, авторизовался ли администратор
            if (login.IsAdminAuthorized)
            {
                var main = new MainWindow(); // создаём главное окно
                main.SetAdmin();             // переводим его в режим администратора
                main.Show();                 // показываем главное окно
                Close();                     // закрываем окно выбора роли
            }
            else
            {
                // Сообщение об ошибке, если вход не удался
                MessageBox.Show("Не удалось войти как администратор!");
            }
        }
    }
}

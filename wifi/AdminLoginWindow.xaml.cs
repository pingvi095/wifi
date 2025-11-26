using System.Windows;

namespace wifi
{
    /// <summary>
    /// Окно авторизации администратора.
    /// Позволяет получить доступ к административным функциям программы
    /// после успешного ввода логина и пароля.
    /// </summary>
    public partial class AdminLoginWindow : Window
    {
        /// <summary>
        /// Флаг, указывающий, авторизован ли администратор.
        /// Значение true — если вход выполнен успешно.
        /// </summary>
        public bool IsAdminAuthorized { get; private set; } = false;

        /// <summary>
        /// Конструктор окна. Инициализирует компоненты интерфейса.
        /// </summary>
        public AdminLoginWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик кнопки входа.
        /// Проверяет введённые логин и пароль администратора.
        /// Если данные верны (admin/admin), устанавливает флаг авторизации и закрывает окно.
        /// </summary>
        /// <param name="sender">Источник события (кнопка).</param>
        /// <param name="e">Аргументы события.</param>
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (LoginBox.Text == "admin" && PasswordBox.Password == "admin")
            {
                IsAdminAuthorized = true;
                Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.");
            }
        }

        /// <summary>
        /// Обработчик кнопки отмены.
        /// Закрывает окно без авторизации.
        /// </summary>
        /// <param name="sender">Источник события (кнопка).</param>
        /// <param name="e">Аргументы события.</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

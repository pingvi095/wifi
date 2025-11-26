using System;
using System.Collections.Generic;
using System.Windows;
using wifi.Helpers;

namespace wifi
{
    /// <summary>
    /// Окно регистрации нового администратора.
    /// Позволяет создать нового пользователя с правами администратора
    /// и сохранить его данные в базе данных.
    /// </summary>
    public partial class AdminRegisterWindow : Window
    {
        /// <summary>
        /// Конструктор окна регистрации администратора.
        /// Инициализирует визуальные компоненты окна.
        /// </summary>
        public AdminRegisterWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Зарегистрировать".
        /// Проверяет корректность введённых данных (логин, пароль, подтверждение пароля),
        /// проверяет наличие администратора в базе данных и,
        /// если всё верно, добавляет нового администратора.
        /// </summary>
        /// <param name="sender">Источник события (кнопка регистрации).</param>
        /// <param name="e">Аргументы события.</param>
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string pass = PasswordBox.Password.Trim();
            string confirm = ConfirmBox.Password.Trim();

            // Проверка заполнения полей
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            // Проверка совпадения паролей
            if (pass != confirm)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            try
            {
                // Проверка, существует ли уже администратор с таким логином
                string checkQuery = "SELECT COUNT(*) FROM admins WHERE username=@u";
                var checkParams = new Dictionary<string, object> { { "@u", username } };
                long exists = Convert.ToInt64(DatabaseHelper.ExecuteScalar(checkQuery, checkParams));

                if (exists > 0)
                {
                    MessageBox.Show("Такой администратор уже существует!");
                    return;
                }

                // Хеширование пароля и добавление нового администратора в базу
                string hash = PasswordHelper.GetHash(pass);
                string insertQuery = "INSERT INTO admins (username, password) VALUES (@u,@p)";
                var insertParams = new Dictionary<string, object> { { "@u", username }, { "@p", hash } };

                DatabaseHelper.ExecuteNonQuery(insertQuery, insertParams);
                MessageBox.Show("Регистрация успешна!");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }
        }
    }
}

namespace wifi.Models
{
    /// <summary>
    /// Модель, представляющая администратора приложения.
    /// Содержит данные для авторизации.
    /// </summary>
    public class Admin
    {
        /// <summary>
        /// Имя пользователя администратора (логин).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Пароль администратора.
        /// </summary>
        public string Password { get; set; }
    }
}

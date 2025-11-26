using System;
using System.Security.Cryptography;
using System.Text;

namespace wifi.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с паролями.
    /// Предоставляет методы для хеширования и проверки паролей.
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Возвращает SHA256-хеш переданной строки в нижнем регистре.
        /// Используется для безопасного хранения паролей.
        /// </summary>
        /// <param name="input">Исходная строка (пароль), которую нужно захешировать.</param>
        /// <returns>Хеш строки в виде шестнадцатеричного текста в нижнем регистре.</returns>
        public static string GetHash(string input)
        {
            // Создаём объект SHA256 для вычисления хеша
            using SHA256 sha = SHA256.Create();

            // Преобразуем строку в массив байт и вычисляем хеш
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Преобразуем байты в шестнадцатеричное представление и приводим к нижнему регистру
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Проверяет, соответствует ли введённый пароль сохранённому хешу.
        /// </summary>
        /// <param name="plainTextPassword">Введённый пользователем пароль в открытом виде.</param>
        /// <param name="hashedPassword">Сохранённый хеш пароля для сравнения.</param>
        /// <returns>True, если пароли совпадают, иначе False.</returns>
        public static bool VerifyPassword(string plainTextPassword, string hashedPassword)
        {
            // Хешируем введённый пароль и сравниваем с хешем из базы
            return GetHash(plainTextPassword) == hashedPassword;
        }
    }
}

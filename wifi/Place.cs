// Models/Place.cs
namespace wifi.Models
{
    /// <summary>
    /// Модель, представляющая место (кафе, библиотеку, лаунж-зону и т.д.).
    /// Используется для отображения информации в основном окне и деталях.
    /// </summary>
    public class Place
    {
        /// <summary>
        /// Уникальный идентификатор места.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название места.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Тип места (например, "Кафе", "Библиотека", "Лаунж-зона").
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Адрес места.
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// Качество Wi-Fi (например, "Плохо", "Средне", "Хорошо").
        /// </summary>
        public string WiFiQuality { get; set; } = "";

        /// <summary>
        /// Часы работы в текстовом виде (например, "9:00–23:00", "Круглосуточно").
        /// </summary>
        public string WorkHours { get; set; } = "";

        /// <summary>
        /// Описание места (краткая информация, особенности и т.п.).
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Путь к изображению места (локальный или URL).
        /// </summary>
        public string PhotoPath { get; set; } = "";

        /// <summary>
        /// Контактная информация (телефон или email).
        /// </summary>
        public string Contact { get; set; } = "";

        /// <summary>
        /// Средняя оценка места (например, на основе отзывов пользователей).
        /// </summary>
        public double Rating { get; set; } = 0.0;
    }
}

// Models/Review.cs
namespace wifi.Models
{
    /// <summary>
    /// Модель, представляющая отзыв о месте.
    /// Связывается с конкретным Place через PlaceId.
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Уникальный идентификатор отзыва.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор места, к которому относится отзыв.
        /// </summary>
        public int PlaceId { get; set; }

        /// <summary>
        /// Автор отзыва (имя пользователя).
        /// </summary>
        public string Author { get; set; } = "";

        /// <summary>
        /// Количество звёзд (1..5) за место.
        /// </summary>
        public int Stars { get; set; }

        /// <summary>
        /// Текст отзыва.
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// Дата и время создания отзыва. По умолчанию — текущий момент.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using wifi.Helpers;
using wifi.Models;

namespace wifi
{
    /// <summary>
    /// Окно с детальной информацией о выбранном месте и его отзывах.
    /// Позволяет просматривать описание, рейтинг, фото, а также добавлять отзывы.
    /// </summary>
    public partial class DetailsWindow : Window
    {
        /// <summary>
        /// Идентификатор выбранного места.
        /// </summary>
        private readonly int placeId;

        /// <summary>
        /// Коллекция отзывов, отображаемая в интерфейсе.
        /// </summary>
        private readonly ObservableCollection<Review> reviews = new();

        /// <summary>
        /// Конструктор окна деталей.
        /// Загружает информацию о месте и все отзывы при инициализации.
        /// </summary>
        /// <param name="id">ID выбранного места в базе данных.</param>
        public DetailsWindow(int id)
        {
            InitializeComponent();
            placeId = id;
            ReviewsList.ItemsSource = reviews;
            LoadDetails();
            LoadReviews();
        }

        /// <summary>
        /// Загружает основную информацию о месте (название, адрес, рейтинг, описание, фото и др.)
        /// из таблицы <c>places</c>.
        /// </summary>
        private void LoadDetails()
        {
            try
            {
                string query = "SELECT * FROM places WHERE id=@id LIMIT 1";
                var parameters = new Dictionary<string, object> { { "@id", placeId } };
                var dt = DatabaseHelper.ExecuteSelect(query, parameters);

                if (dt.Rows.Count == 0) return;
                var row = dt.Rows[0];

                NameText.Text = row["name"].ToString();
                ContactText.Text = row["contact"].ToString();
                HoursText.Text = row["work_hours"].ToString();
                WiFiText.Text = row["wifi_quality"].ToString();
                RatingText.Text = Convert.ToDouble(row["rating"] == DBNull.Value ? 0.0 : row["rating"]).ToString("N1");
                DescText.Text = row["description"].ToString();

                // Загрузка изображения, если оно указано и существует
                var path = row["photo_path"].ToString();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.UriSource = new Uri(path, UriKind.Absolute);
                        bmp.EndInit();
                        PhotoImg.Source = bmp;
                    }
                    catch
                    {
                        PhotoImg.Source = null;
                    }
                }
                else
                {
                    PhotoImg.Source = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке деталей: " + ex.Message);
            }
        }

        /// <summary>
        /// Загружает список отзывов для данного места из таблицы <c>reviews</c>.
        /// </summary>
        private void LoadReviews()
        {
            reviews.Clear();
            try
            {
                string query = "SELECT id, place_id, author, stars, comment, created_at FROM reviews WHERE place_id=@pid ORDER BY created_at DESC";
                var parameters = new Dictionary<string, object> { { "@pid", placeId } };
                var dt = DatabaseHelper.ExecuteSelect(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    reviews.Add(new Review
                    {
                        Id = Convert.ToInt32(row["id"]),
                        PlaceId = Convert.ToInt32(row["place_id"]),
                        Author = row["author"].ToString(),
                        Stars = Convert.ToInt32(row["stars"]),
                        Comment = row["comment"].ToString(),
                        CreatedAt = Convert.ToDateTime(row["created_at"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке отзывов: " + ex.Message);
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Отправить отзыв".
        /// Добавляет новый отзыв, пересчитывает средний рейтинг и обновляет интерфейс.
        /// </summary>
        private void SendReview_Click(object sender, RoutedEventArgs e)
        {
            var author = string.IsNullOrWhiteSpace(AuthorBox.Text) ? "Аноним" : AuthorBox.Text.Trim();
            if (!int.TryParse(((System.Windows.Controls.ComboBoxItem)StarsBox.SelectedItem).Content.ToString(), out int stars))
                stars = 3;
            var comment = CommentBox.Text?.Trim() ?? "";

            try
            {
                // Добавление нового отзыва
                string insert = "INSERT INTO reviews (place_id, author, stars, comment) VALUES (@pid, @a, @s, @c)";
                var insertParams = new Dictionary<string, object>
                {
                    { "@pid", placeId },
                    { "@a", author },
                    { "@s", stars },
                    { "@c", comment }
                };
                DatabaseHelper.ExecuteNonQuery(insert, insertParams);

                // Пересчёт среднего рейтинга по всем отзывам
                string avgQuery = "SELECT AVG(stars) FROM reviews WHERE place_id=@pid";
                var avgParams = new Dictionary<string, object> { { "@pid", placeId } };
                var avgObj = DatabaseHelper.ExecuteScalar(avgQuery, avgParams);

                double avg = (avgObj != null && avgObj != DBNull.Value) ? Convert.ToDouble(avgObj) : 0.0;

                // Обновление рейтинга в таблице мест
                string update = "UPDATE places SET rating=@r WHERE id=@id";
                var updateParams = new Dictionary<string, object> { { "@r", avg }, { "@id", placeId } };
                DatabaseHelper.ExecuteNonQuery(update, updateParams);

                // Очистка полей и обновление интерфейса
                AuthorBox.Text = "";
                CommentBox.Text = "";
                StarsBox.SelectedIndex = 2;

                LoadDetails();
                LoadReviews();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении отзыва: " + ex.Message);
            }
        }

        /// <summary>
        /// Закрывает текущее окно.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

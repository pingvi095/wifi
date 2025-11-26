using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using wifi.Helpers;
using wifi.Models;

namespace wifi
{
    /// <summary>
    /// Окно для редактирования существующего места в базе данных.
    /// Позволяет изменить основную информацию — тип, адрес, описание, качество Wi-Fi и другие поля.
    /// </summary>
    public partial class EditPlaceWindow : Window
    {
        /// <summary>
        /// Объект выбранного места, переданный из главного окна.
        /// </summary>
        private readonly Place place;

        /// <summary>
        /// Конструктор окна редактирования.
        /// Инициализирует интерфейс и заполняет поля текущими данными выбранного места.
        /// </summary>
        /// <param name="selectedPlace">Объект места, выбранного для редактирования.</param>
        public EditPlaceWindow(Place selectedPlace)
        {
            InitializeComponent();
            place = selectedPlace;
            LoadData();
        }

        /// <summary>
        /// Проверяет корректность формата часов работы.
        /// Допускается формат "00:00-00:00" или значение "Круглосуточно".
        /// </summary>
        /// <param name="input">Введённая строка с часами работы.</param>
        /// <returns><see langword="true"/>, если формат корректный; иначе <see langword="false"/>.</returns>
        private bool ValidateWorkHours(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            if (Regex.IsMatch(input, @"^круглосуточно$", RegexOptions.IgnoreCase))
                return true;

            if (Regex.IsMatch(input, @"^\d{2}:\d{2}-\d{2}:\d{2}$"))
            {
                string[] parts = input.Split('-', ':');
                int h1 = int.Parse(parts[0]), m1 = int.Parse(parts[1]);
                int h2 = int.Parse(parts[2]), m2 = int.Parse(parts[3]);
                return h1 >= 0 && h1 <= 23 && h2 >= 0 && h2 <= 23 && m1 >= 0 && m1 <= 59 && m2 >= 0 && m2 <= 59;
            }
            return false;
        }

        /// <summary>
        /// Загружает данные выбранного места в соответствующие поля интерфейса.
        /// </summary>
        private void LoadData()
        {
            NameBox.Text = place.Name;
            AddressBox.Text = place.Address;
            WorkBox.Text = place.WorkHours;
            ContactBox.Text = place.Contact;
            

            foreach (ComboBoxItem item in TypeBox.Items)
                if (item.Content.ToString() == place.Type) { TypeBox.SelectedItem = item; break; }

            foreach (ComboBoxItem item in WiFiBox.Items)
                if (item.Content.ToString() == place.WiFiQuality) { WiFiBox.SelectedItem = item; break; }
        }

        /// <summary>
        /// Сохраняет внесённые изменения в базу данных.
        /// Выполняет валидацию данных и формирует запрос <c>UPDATE</c> к таблице <c>places</c>.
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text.Trim();
            var type = (TypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.Trim() ?? "";
            var address = AddressBox.Text.Trim();
            var wifi = (WiFiBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.Trim() ?? "";
            var hours = WorkBox.Text.Trim();
            var contact = ContactBox.Text.Trim();
            

            // Проверка на заполнение всех полей
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type) ||
                string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(wifi) ||
                string.IsNullOrWhiteSpace(hours) || string.IsNullOrWhiteSpace(contact))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            // Проверка формата часов работы
            if (!ValidateWorkHours(hours))
            {
                MessageBox.Show("Неверный формат часов работы!");
                return;
            }

            // Приведение к единому формату
            if (Regex.IsMatch(hours, @"^круглосуточно$", RegexOptions.IgnoreCase))
                hours = "Круглосуточно";

            // Подготовка SQL-запроса
            string query = @"UPDATE places SET 
                                name=@n,
                                type=@t,
                                address=@a,
                                wifi_quality=@w,
                                work_hours=@h,
                                contact=@c,
                                description=@d
                             WHERE id=@id";

            var parameters = new Dictionary<string, object>
            {
                { "@n", name },
                { "@t", type },
                { "@a", address },
                { "@w", wifi },
                { "@h", hours },
                { "@c", contact },
                { "@id", place.Id }
            };

            // Выполнение запроса обновления
            DatabaseHelper.ExecuteNonQuery(query, parameters);
            MessageBox.Show("Изменения успешно сохранены!");
            Close();
        }
    }
}

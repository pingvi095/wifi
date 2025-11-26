using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using wifi.Helpers;
using wifi.Models;

namespace wifi
{
    /// <summary>
    /// Главное окно приложения.
    /// Отображает список мест, позволяет фильтровать, искать, сортировать
    /// и открывать подробности о каждом месте.  
    /// При авторизации администратора открывает доступ к добавлению, редактированию и удалению записей.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Коллекция мест, отображаемая в DataGrid.
        /// </summary>
        private readonly ObservableCollection<Place> places = new();

        /// <summary>
        /// Флаг, указывающий, вошёл ли администратор.
        /// </summary>
        private bool isAdmin = false;

        /// <summary>
        /// Конструктор главного окна.
        /// Инициализирует интерфейс и загружает список мест из базы данных.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            PlacesGrid.ItemsSource = places;
            LoadPlaces();
        }

        /// <summary>
        /// Переводит приложение в режим администратора,
        /// открывая дополнительные функции (добавление, редактирование, удаление).
        /// </summary>
        public void SetAdmin()
        {
            isAdmin = true;
            try
            {
               
                AddButton.Visibility = Visibility.Visible;
                EditButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
                Admin4ik.Visibility = Visibility.Collapsed;

            }
            catch
            {
                // Безопасно игнорируем, если кнопка отсутствует
            }
        }

        /// <summary>
        /// Метод вызывается при загрузке основного контейнера окна (например, Grid).
        /// Устанавливает стандартные значения фильтров и подгружает список мест.
        /// </summary>
        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            InitDefaultSelections();
            LoadPlaces();
        }

        /// <summary>
        /// Устанавливает значения фильтров и сортировки по умолчанию.
        /// </summary>
        private void InitDefaultSelections()
        {
            if (TypeFilter != null && TypeFilter.SelectedIndex < 0) TypeFilter.SelectedIndex = 0;
            if (WiFiFilter != null && WiFiFilter.SelectedIndex < 0) WiFiFilter.SelectedIndex = 0;
            if (HoursFilter != null && HoursFilter.SelectedIndex < 0) HoursFilter.SelectedIndex = 0;
            if (SortBox != null && SortBox.SelectedIndex < 0) SortBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Загружает список мест из базы данных с учётом фильтров и поиска.
        /// </summary>
        /// <param name="search">Строка поиска (по названию, адресу или описанию).</param>
        
            private void LoadPlaces(string search = "")
        {
            if (TypeFilter == null || WiFiFilter == null || HoursFilter == null || SortBox == null || PlacesGrid == null)
                return;

            places.Clear();
            var whereParts = new List<string>();
            var parameters = new Dictionary<string, object>();

            // Фильтр по поисковой строке
            if (!string.IsNullOrWhiteSpace(search) && search != "Поиск...")
            {
                whereParts.Add("(name LIKE @q OR address LIKE @q)");
                parameters["@q"] = "%" + search.Trim() + "%";
            }

            // Фильтр по типу
            string typeSel = (TypeFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все типы";
            if (typeSel != "Все типы")
            {
                whereParts.Add("TRIM(LOWER(type)) = TRIM(LOWER(@type))");
                parameters["@type"] = typeSel;
            }

            // Фильтр по Wi-Fi
            string wifiSel = (WiFiFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Любое";
            if (wifiSel != "Любое")
            {
                whereParts.Add("TRIM(LOWER(wifi_quality)) = TRIM(LOWER(@wifi))");
                parameters["@wifi"] = wifiSel;
            }

            // Фильтр по часам работы
            string hoursSel = (HoursFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Любые часы";
            if (hoursSel != "Любые часы")
            {
                if (hoursSel == "Круглосуточно")
                    whereParts.Add("(LOWER(work_hours) LIKE '%24%' OR LOWER(work_hours) LIKE '%круглосуточ%' OR LOWER(work_hours) LIKE '%24/7%' OR LOWER(work_hours) LIKE '%24 часа%')");
                else if (hoursSel == "До 23:00")
                    whereParts.Add("(work_hours LIKE '%23%' OR LOWER(work_hours) LIKE '%до 23%' OR LOWER(work_hours) REGEXP '[^0-9]23(:00)?')");
                else if (hoursSel == "До 20:00")
                    whereParts.Add("(work_hours LIKE '%20%' OR LOWER(work_hours) LIKE '%до 20%' OR LOWER(work_hours) REGEXP '[^0-9]20(:00)?')");
                else
                {
                    whereParts.Add("work_hours LIKE @hours");
                    parameters["@hours"] = "%" + hoursSel + "%";
                }
            }
            
        

        // Формирование SQL-запроса
        string query = "SELECT * FROM places";
            if (whereParts.Count > 0)
                query += " WHERE " + string.Join(" AND ", whereParts);

            // Сортировка
            string sortSel = (SortBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "По умолчанию";
            switch (sortSel)
            {
                case "По рейтингу (убывание)":
                    query += " ORDER BY rating DESC";
                    break;
                case "По рейтингу (возрастание)":
                    query += " ORDER BY rating ASC";
                    break;
                case "По названию (А→Я)":
                    query += " ORDER BY name ASC";
                    break;
                case "По названию (Я→А)":
                    query += " ORDER BY name DESC";
                    break;

            }

            try
            {
                DataTable dt = DatabaseHelper.ExecuteSelect(query, parameters);
                foreach (DataRow row in dt.Rows)
                {
                    places.Add(new Place
                    {
                        Id = Convert.ToInt32(row["id"]),
                        Name = row["name"]?.ToString() ?? "",
                        Type = row["type"]?.ToString() ?? "",
                        Address = row["address"]?.ToString() ?? "",
                        WiFiQuality = row["wifi_quality"]?.ToString() ?? "",
                        WorkHours = row["work_hours"]?.ToString() ?? "",
                        Description = row["description"]?.ToString() ?? "",
                        PhotoPath = row["photo_path"]?.ToString() ?? "",
                        Contact = row["contact"]?.ToString() ?? "",
                        Rating = row["rating"] == DBNull.Value ? 0.0 : Convert.ToDouble(row["rating"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        /// <summary>
        /// Сбрасывает все фильтры и сортировки к значениям по умолчанию.
        /// </summary>
        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            TypeFilter.SelectedIndex = 0;
            WiFiFilter.SelectedIndex = 0;
            HoursFilter.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
            SearchBox.Text = "Поиск...";
            SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            LoadPlaces("");
        }

        /// <summary>
        /// Очищает поле поиска при фокусе.
        /// </summary>
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        /// <summary>
        /// Восстанавливает подсказку, если поле поиска пустое.
        /// </summary>
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск...";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        /// <summary>
        /// Обновляет список мест при вводе текста в поиске.
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text != "Поиск...")
                LoadPlaces(SearchBox.Text);
        }

        /// <summary>
        /// Обновляет список при изменении любого фильтра.
        /// </summary>
        private void Filters_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadPlaces(SearchBox.Text == "Поиск..." ? "" : SearchBox.Text);
        }

        /// <summary>
        /// Быстрый фильтр — отображает только круглосуточные заведения.
        /// </summary>
        private void Quick24_Click(object sender, RoutedEventArgs e)
        {
            HoursFilter.SelectedIndex = 1;
        }

        /// <summary>
        /// Быстрый фильтр — кафе с хорошим Wi-Fi.
        /// </summary>
        private void QuickCafeGood_Click(object sender, RoutedEventArgs e)
        {
            TypeFilter.SelectedIndex = 1;
            WiFiFilter.SelectedIndex = 4;
        }

        /// <summary>
        /// Открывает окно авторизации администратора.
        /// При успешном входе включает админский режим.
        /// </summary>
        private void Admin_Click(object sender, RoutedEventArgs e)
        {
            var login = new AdminLoginWindow();
            login.ShowDialog();
            if (login.IsAdminAuthorized) SetAdmin();
        }

        /// <summary>
        /// Добавление нового места (доступно только администратору).
        /// </summary>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
          
            var w = new AddPlaceWindow();
            w.ShowDialog();
            LoadPlaces(SearchBox.Text == "Поиск..." ? "" : SearchBox.Text);
        }

        /// <summary>
        /// Редактирование выбранного места (только для администратора).
        /// </summary>
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            
            if (PlacesGrid.SelectedItem is not Place p)
            {
                MessageBox.Show("Выберите запись для редактирования.");
                return;
            }
            var w = new EditPlaceWindow(p);
            w.ShowDialog();
            LoadPlaces(SearchBox.Text == "Поиск..." ? "" : SearchBox.Text);
        }

        /// <summary>
        /// Удаляет выбранное место после подтверждения.
        /// </summary>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
           
            if (PlacesGrid.SelectedItem is not Place p)
            {
                MessageBox.Show("Выберите запись для удаления.");
                return;
            }
            if (MessageBox.Show("Удалить выбранное место?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string query = "DELETE FROM places WHERE id=@id";
                var parameters = new Dictionary<string, object> { { "@id", p.Id } };
                DatabaseHelper.ExecuteNonQuery(query, parameters);
                LoadPlaces(SearchBox.Text == "Поиск..." ? "" : SearchBox.Text);
            }
        }

        /// <summary>
        /// Открывает окно с подробной информацией о выбранном месте.
        /// </summary>
        private void OpenDetails_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Place p)
            {
                var details = new DetailsWindow(p.Id);
                details.ShowDialog();
                LoadPlaces(SearchBox.Text == "Поиск..." ? "" : SearchBox.Text);
            }
        }

        /// <summary>
        /// Возврат к окну выбора роли (пользователь/администратор).
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var roleWindow = new RoleSelectionWindow();
            roleWindow.Show();
            Close();
        }

       
    }
}

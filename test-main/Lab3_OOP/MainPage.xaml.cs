using System.Collections.ObjectModel;
using System.Reflection;

namespace Lab3_OOP
{
    public partial class MainPage : ContentPage
    {
        //посилання на початковий файл джсон с інформацією про наукові роботи
        private string _filePath = "";
        //файл для запису результатів фільтрації
        private string _resultsFilePath = "";
        private bool _isError = false;
        //структура с назвами пікерів-фільтрів та списками з даними для їх заповнення
        private Dictionary<string, List<string>> _pickersData = new Dictionary<string, List<string>>
        {
            { "AuthorName", new List<string>() },
            { "Faculty", new List<string>() },
            { "CustomerName", new List<string>() },
            { "Branch", new List<string>() },
        };
        //колекція з десереалізованими даними 
        private ObservableCollection<ScientificWork> _deserializedData = new ObservableCollection<ScientificWork>();
        //колекція з даними які ми показуємо користувачу
        private ObservableCollection<ScientificWork> _dataToShow = new ObservableCollection<ScientificWork>();
        //об'єкт який відповідає за пошук-фільтрацію за допомогою LINQ
        private LinqSearch _analizator = new LinqSearch();
        //створюємо критерію пошуку
        SearchCriteria criteria = new SearchCriteria();

        public MainPage()
        {
            InitializeComponent();
            ResultsListView.ItemsSource = _dataToShow;
        }

        //допоміжний метод для заповнення пікерів-фільтрів даними 
        private void AddPickerValue(ScientificWork work)
        {
            string[] selectedProperties = { "AuthorName", "Faculty", "CustomerName", "Branch" };

            foreach (string propertyName in selectedProperties)
            {
                PropertyInfo property = work.GetType().GetProperty(propertyName);
                var pickerList = _pickersData[propertyName];

                if (property != null)
                {
                    string propertyValue = property.GetValue(work) as string;
                    if (!string.IsNullOrEmpty(propertyValue) && !pickerList.Contains(propertyValue))
                        //додаємо дані в пікер лише в тому випадку, якщо вони раніше не додані
                    {
                       pickerList.Add(propertyValue);
                    }
                }
            }
        }

        //метод для очищення даних критеріїв пошуку (пікерів)
        private void ClearCriterias()
        {
            foreach (var list in _pickersData.Values)
            {
                list.Clear();
            }
        }

        //допоміжний метод для відв'язання джерела даних пікерів 
        private void ClearPickersValues()
        {
            authorPicker.ItemsSource = null;
            facultyPicker.ItemsSource = null;
            custNamePicker.ItemsSource = null;
            branchPicker.ItemsSource = null;
        }

        //сортування даних в пікерах-фільтрах
        private void SortPickersValues()
        {
            foreach (var list in _pickersData.Values)
            {
                list.Sort();
            }
        }

       //метод для встановлення джерела даних для пікерів, попередньо ци дані фільтруємо
        private void AddItemSourses()
        {
            SortPickersValues();
            authorPicker.ItemsSource = _pickersData["AuthorName"];
            facultyPicker.ItemsSource = _pickersData["Faculty"];
            custNamePicker.ItemsSource = _pickersData["CustomerName"];
            branchPicker.ItemsSource = _pickersData["Branch"];
        }

        //метод для заповнення пікерів-фільтрів даними з наданого джсон файлу
        private void FillPickers()
        {
            ClearCriterias();
           
            foreach(var work in _deserializedData)
            {
                AddPickerValue(work);
            }

            //спочатку відв'язуємось від джерела даних, а потім заново прив'язуємось (це необхідно якщо обрали наприклад спочатку один файл, а потім зовсім інший)
            ClearPickersValues();
            AddItemSourses();
        }

        //метод який очищує все (обрані фільтри, список з даними які показуємо)
        private void UpdateFilters()
        {
            authorPicker.SelectedItem = null;
            facultyPicker.SelectedItem = null;
            custNamePicker.SelectedItem = null;
            branchPicker.SelectedItem = null;
            _dataToShow.Clear();
            notFoundLabel.IsVisible = false;
        }

        //метод який відповідає за обрання файлу та обробку помилок які можуть під час цього виникнути
        private async Task<string> PickFile()
        {
            _isError = false;
            try
            {
                var result = await FilePicker.PickAsync();
                if (result != null)
                {
                    string resultPath = result.FullPath;

                    if (File.Exists(resultPath))
                    {
                        string extension = Path.GetExtension(resultPath);
                        //перевіяємо чи правильне розширення файлу
                        if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                        {
                            return resultPath;
                        }
                        else
                        {
                            _isError = true;
                            await DisplayAlert("Помилка", "Обраний файл не є JSON-файлом.", "ОК");
                        }
                    }
                    else
                    {
                        _isError = true;
                        await DisplayAlert("Помилка", "Обраного файлу не існує.", "ОК");
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _isError = true;
                await DisplayAlert("Помилка", $"{ex.Message}", "ОК");
                return string.Empty;
            }
        }

        //метод-обробка кнопки Обрати файл
        private async void OnPickFileClicked(object sender, EventArgs e)
        {
            //обираємо файл
            _filePath = await PickFile();

            if (!string.IsNullOrEmpty(_filePath) && !_isError)
            {
                FileInfo fileInfo = new FileInfo(_filePath);

                if(fileInfo.Length > 0)
                {
                    try
                    {
                        //якщо з файлом все ок, то десереалізуємо дані з нього
                        _deserializedData = JsonProcessor.Deserialize(_filePath);
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Помилка", ex.Message, "ОК");
                    }

                    //заповнюємо фільтри даними 
                    UpdateFilters();
                    FillPickers();
                    filters.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Помилка", "Файл пустий.", "ОК");
                }
            }
        }

        //метод-обробник кнопки Зберегти дані в Джсон-файл
        private async void SaveJsonBtnClicked(object sender, EventArgs e)
        {
            //обираємо файл куди зберігати
            _resultsFilePath = await PickFile();

            if (!string.IsNullOrEmpty(_resultsFilePath) && !_isError)
            {
                try
                {
                    //сереалізуємо відфільтровані дані в обраний файл
                    JsonProcessor.Serialize(_resultsFilePath, _dataToShow);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Помилка", ex.Message, "ОК");
                }
                
                await DisplayAlert("Інформація", "Результати збережені!", "ОК");
            }
        }

        //метод обробник кнопки Очистити
        private void OnCleanBtnClicked(object sender, EventArgs e)
        {
            UpdateFilters();
        }

        //метод формування критерії пошуку з даних обраних в пікерах-фільтрах
        private SearchCriteria FormCriteria()
        {
            SearchCriteria newCriteria = new SearchCriteria();

            //якщо обране значення пікера-фільтра, то закидуємо це обране значення в критерію, інакше закидуємо пусту строку 
            newCriteria.AuthorName = authorPicker.SelectedItem != null ? authorPicker.SelectedItem as string : string.Empty;
            newCriteria.Faculty = facultyPicker.SelectedItem != null ? facultyPicker.SelectedItem as string : string.Empty;
            newCriteria.CustomerName = custNamePicker.SelectedItem != null ? custNamePicker.SelectedItem as string : string.Empty;
            newCriteria.Branch = branchPicker.SelectedItem != null ? branchPicker.SelectedItem as string : string.Empty;

            return newCriteria;
        }

        //метод-обробник кнопки Пошук
        private async void OnSearchBtnClicked(object sender, EventArgs e)
        {
            //формуємо критерію пошуку
            criteria = FormCriteria();

            try
            {
                //шукаємо дані за допомогою LINQ
                _analizator.Search(criteria, _deserializedData, _dataToShow);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"{ex.Message}", "ОК");
            }

            //якщо список відфільтрованих даних не пустий, то показуємо список даних, інакше теекст що нічого не знайдено
            if (_dataToShow.Count > 0 && !string.IsNullOrEmpty(_filePath))
            {
                ResultsContainer.IsVisible = true;
                notFoundLabel.IsVisible = false;
            }
            else
            {
                ResultsContainer.IsVisible = false;
                if (!string.IsNullOrEmpty(_filePath))
                {
                    notFoundLabel.IsVisible = true;
                }
            }

        }

        //метод-обробник видалення наукової роботи зі списку
        private void DeleteButtonClicked(object sender, EventArgs e)
        {
            //отримуємо кнопку яка надіслала запит на видалення
            Button button = (Button)sender;
            //отримуємо наукову роботу яку треба видалити
            ScientificWork workToDel = (ScientificWork)button.BindingContext;
            //видаляємо її з списку який показуємо і загального списку десереалізованих даних
            _dataToShow.Remove(workToDel);
            _deserializedData.Remove(workToDel);

            //оновлюємо значення даних в пікерах-фільтрах
            FillPickers();

            //закидуємо оновлені дані в Джсон-файл
            try
            {
                JsonProcessor.Serialize(_filePath, _deserializedData);
            }
            catch (Exception ex)
            {
                DisplayAlert("Помилка", ex.Message, "ОК");
            }
            
        }

        //метод-обробник редагування наукової роботи
        private async void OnChangeBtnClicked(object sender, EventArgs e)
        {
            //отримуємо кнопку яка надіслала запит на редагування
            Button button = (Button)sender;
            //отримуємо наукову роботу яку треба редагувати
            ScientificWork selectedItem = (ScientificWork)button.BindingContext;
            if(selectedItem != null)
            {
                //створюємо сторінку для редагування
                var secondPage = new EditPage(selectedItem);
                //підписуємось на подію яка повідомляє про те що дані наукової роботи були змінені на сторінці редагування
                secondPage.DataModified += (s, modifiedData) =>
                {
                    if (modifiedData != null)
                    {
                        ResultsListView.ItemsSource = null;

                        int idxInShowData = _dataToShow.IndexOf(selectedItem);
                        int idxInAllData = _dataToShow.IndexOf(selectedItem);

                        //оновлюємо відредаговану роботу в обох списках
                        _dataToShow[idxInShowData] = modifiedData;
                        _deserializedData[idxInAllData] = modifiedData;

                        ResultsListView.ItemsSource = _dataToShow;
                        DisplayAlert("Інформація", "Зміни успішно внесені!", "ОК");

                        //оновлюємо значення пікерів-фільтрів
                        FillPickers();

                        try
                        {
                            //оновлюємо інформацію в Джсон-файлі
                            JsonProcessor.Serialize(_filePath, _deserializedData);
                        }
                        catch (Exception ex)
                        {
                            DisplayAlert("Помилка", ex.Message, "ОК");
                        }
                    }
                    else
                    {
                        DisplayAlert("Помилка", "Внести зміни не вдалося.", "ОК");
                    }
                };
                //навігація на сторінку-редагування наукової роботи
                await Navigation.PushModalAsync(secondPage);
            }
            else
            {
                await DisplayAlert("Помилка", "Сталася помилка!.", "ОК");
            }
        }

        //метод обробник додавання нової наукової роботи 
        private async void OnAddElemBtnClicked(object sender, EventArgs e)
        {
            //створюємо нову наукову роботу
            ScientificWork newWork = new ScientificWork();

            //створюємо нову сторінку для заповнення даних
            var secondPage = new EditPage(newWork);
            //підписуємось на подію що дані були заповнені
            secondPage.DataModified += (s, modifiedData) =>
            {
                if (modifiedData != null)
                {
                    //додаємо нову роботу в обидва списки
                    _deserializedData.Insert(0, modifiedData);
                    _analizator.Search(criteria, _deserializedData, _dataToShow);

                    DisplayAlert("Інформація", "Нова наукова робота додана!", "ОК");
                    //оновлюємо дані пікерів-фільтрів
                    FillPickers();

                    try
                    {
                        //оновлюємо дані в Джсон-файлі з доданою новою роботою
                        JsonProcessor.Serialize(_filePath, _deserializedData);
                    }
                    catch (Exception ex)
                    {
                        DisplayAlert("Помилка", ex.Message, "ОК");
                    }
                }
                else
                {
                    DisplayAlert("Помилка", "Внести зміни не вдалося.", "ОК");
                }
            };

            //навігація на сторінку редагування
            await Navigation.PushModalAsync(secondPage);
        }
    }
}
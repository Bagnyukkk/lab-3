using System.Collections.ObjectModel;

namespace Lab3_OOP
{
    //сторінка редагування/створення наукової роботи
    public partial class EditPage : ContentPage
    {
        private ScientificWork _selectedItem;
        public event EventHandler<ScientificWork> DataModified;

        //заповнюємо поля введення початковими даними, які прийшли нам з обраною роботою
        private void FillInputs()
        {
            nameInput.Text = _selectedItem.Name;
            authorNameInput.Text = _selectedItem.AuthorName;
            facultyInput.Text = _selectedItem.Faculty;
            departInput.Text = _selectedItem.Department;
            labInput.Text = _selectedItem.Labaratory;
            posInput.Text = _selectedItem.AuthorPosition;
            startOnInput.Text = _selectedItem.StartOnPosition;
            lastOnInput.Text = _selectedItem.LastonPosition;
            custNameInput.Text = _selectedItem.CustomerName;
            custAdrInput.Text = _selectedItem.CustomerAdress;
            submInput.Text = _selectedItem.Submission;
            branchInput.Text = _selectedItem.Branch;
        }
        public EditPage(ScientificWork selected)
        {
            InitializeComponent();

            //записуємо обрану користувачем роботу
            _selectedItem = selected;
            FillInputs();
        }

        //перевірка чи правильний рік введений
        private bool ValidateYear(string value)
        {
            if (int.TryParse(value, out int year))
            {
                if (year >0 && year <= 2023)
                {
                    return true;
                }     
            }
            return false;
        }
        
        //перевірка чи поле введення не пусте
        private bool IsEmpty(string value) 
        {
            return value == string.Empty;
        }

        //валідуємо всі поля
        private bool ValidateAll()
        {
            return (
                !IsEmpty(nameInput.Text) &&
                !IsEmpty(authorNameInput.Text)&&
                !IsEmpty(facultyInput.Text)&&
                !IsEmpty(departInput.Text)&&
                !IsEmpty(labInput.Text)&&
                !IsEmpty(posInput.Text)&&
                !IsEmpty(startOnInput.Text)&&
                ValidateYear(startOnInput.Text)&&
                ValidateYear(lastOnInput.Text)&&
                !IsEmpty(lastOnInput.Text)&&
                !IsEmpty(custNameInput.Text)&&
                !IsEmpty(custAdrInput.Text)&&
                !IsEmpty(submInput.Text)&&
                !IsEmpty(branchInput.Text)
                );
        }
         // оновлюємо значення полів обраної наукової роботи заповненими даними 
        private void UpdateSelected()
        {
            _selectedItem.Name = nameInput.Text;
            _selectedItem.AuthorName = authorNameInput.Text;
            _selectedItem.Faculty = facultyInput.Text;
            _selectedItem.Department = departInput.Text;
            _selectedItem.Labaratory = labInput.Text;
            _selectedItem.AuthorPosition = posInput.Text;
            _selectedItem.StartOnPosition = startOnInput.Text;
            _selectedItem.LastonPosition = lastOnInput.Text;
            _selectedItem.CustomerName = custNameInput.Text;
            _selectedItem.CustomerAdress = custAdrInput.Text;
            _selectedItem.Submission = submInput.Text;
            _selectedItem.Branch = branchInput.Text;
        }

        //метод-обробник кнопки Зберегти зміни
        private void SaveButtonClicked(object sender, EventArgs e)
        {
            //валідуємо всі поля введення
            if (ValidateAll())
            {
                //якщо все ок, то оновлюєм наукову роботу
                UpdateSelected();
                //викликаємо подію що дані були оновлені
                DataModified?.Invoke(this, _selectedItem);
                //повертаємось на початкову сторінку
                Navigation.PopModalAsync();
            }
            else
            {
                DisplayAlert("Помилка", "Деякі введення не валідні.", "ОК");
            }
        }

        //метод обробник кнопки Скасувати, тобто повернення на почткову сторінку
        private void ReturnButtonClicked(object sender, EventArgs e)
        {
                Navigation.PopModalAsync();
        }
    }
}

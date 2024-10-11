using MewingLab.Classes;
using System.Windows;
using MewingLab.DB;
using System.Linq;
using System.Data.SqlTypes;
using System.Windows.Input;

namespace MewingLab.Forms
{
    public partial class CreateWorkerWindow : Window
    {
        MewingLabEntities4 db = new MewingLabEntities4();
        public CreateWorkerWindow()
        {
            InitializeComponent();
        }

        /*
            По нажатию кнопки проводится валидация всех полей.
            Если всё хорошо, то добавляем нового работника
         */
        private void createWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на пустоту имени
            if (workerNameTB.Text == "")
            {
                Helper.Message("Поле ФИО должно быть заполнено!", "error");
                return;
            }

            // Проверка на пустоту телефона и его длина 11 символов
            if (workerPhoneTB.Text == "" || workerPhoneTB.Text.Length != 11 || !workerPhoneTB.Text.All(char.IsDigit))
            {
                Helper.Message("Поле номер телефона должно быть заполнено и длина составлять 11 символов!!", "error");
                return;
            }

            // Проверка на пустоту почты
            if (workerEmailTB.Text == "")
            {
                Helper.Message("Поле почты должно быть заполнено!", "error");
                return;
            }

            // Проверка на пустоту почты
            if (workerLoginTB.Text == "")
            {
                Helper.Message("Поле логина должно быть заполнено!", "error");
                return;
            }

            // Проверка на пустоту пароля
            if (workerPasswordTB.Text == "")
            {
                Helper.Message("Поле пароля должно быть заполнено!", "error");
                return;
            }

            // Проверка на корректность почты
            if (!Mail_LIB.Validation.checkEmail(workerEmailTB.Text))
            {
                Helper.Message("Почта должно быть валидно!", "error");
                return;
            }

            // Проверка на корректность логина
            if (!Mail_LIB.Validation.checkLogin(workerLoginTB.Text))
            {
                Helper.Message("Логин должен быть валидным!", "error");
                return;
            }

            // Проверка на корректность пароля
            if (!Mail_LIB.Validation.checkPassword(workerPasswordTB.Text))
            {
                Helper.Message("Пароль должен быть валидным!", "error");
                return;
            }

            // Проверка на то, что ФИО написано в три слова
            string[] workerName = workerNameTB.Text.Split(' ');
            if (workerName.Length != 3 || workerName[0].Length > 100 || workerName[1].Length > 100 || workerName[2].Length > 100)
            {
                Helper.Message("ФИО должно быть в 3 слова! Каждое слово не больше 100 символов!", "error");
                return;
            }

            // Проверка на то, какой RadioButton выбран
            int typeWorker = 0;
            string avatarPath = "";

            if (labRB.IsChecked == true)
            {
                typeWorker = 1;
                avatarPath = "lab1.jpeg";
            }

            if (labResRB.IsChecked == true)
            {
                typeWorker = 2;
                avatarPath = "lab2.png";
            }

            if (buhRB.IsChecked == true)
            {
                typeWorker = 3;
                avatarPath = "buh.jpeg";
            }

            if (adminRB.IsChecked == true)
            {
                typeWorker = 4;
                avatarPath = "admin.png";
            }

            users checkLogin = db.users.Where(u => u.login == workerLoginTB.Text).FirstOrDefault();
            users checkPhone = db.users.Where(u => u.phone_number == workerPhoneTB.Text).FirstOrDefault();

            if (checkLogin != null)
            {
                Helper.Message("Логин такой занят!", "error");
                return;
            }

            if (checkPhone != null)
            {
                Helper.Message("Телефон такой занят!", "error");
                return;
            }

            // Добавление в БД нового пользователя
            users addNewUser = new users
            {
                name = workerName[0],
                second_name = workerName[1],
                last_name = workerName[2],
                login = workerLoginTB.Text,
                password = workerPasswordTB.Text,
                id_user_type = typeWorker,
                avatar_path = avatarPath,
                phone_number = workerPhoneTB.Text,
            };

            db.users.Add(addNewUser);
            db.SaveChanges();

            Helper.Message("Новый сотрудник добавлен!", "good");
            
            AddServicesWorkerWindow addServicesWorkerWindow = new AddServicesWorkerWindow();
            addServicesWorkerWindow.ShowDialog();

            // Закрываемся
            Close();
        }

        private void workerPhoneTB_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true; // отменяем вставку
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true; // отмена пробела
            }
        }

        private void workerEmailTB_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true; // отменяем вставку
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true; // отмена пробела
            }
        }

        private void workerLoginTB_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true; // отменяем вставку
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true; // отмена пробела
            }
        }

        private void workerPasswordTB_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true; // отменяем вставку
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true; // отмена пробела
            }
        }
    }
}
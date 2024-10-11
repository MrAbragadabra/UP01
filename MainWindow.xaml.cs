using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MewingLab.Classes;
using MewingLab.Forms;
using Microsoft.Win32;
using MewingLab.DB;

namespace MewingLab
{
    public partial class MainWindow : Window
    {
        /*
            db - хранит в себе все модели
            blockedTime - хранит в себе время блокировки
            passwordIsOpen - хранит состояние пароля (скрыт/открыт)
            captchaIsOpen - хранит состоние капчи (скрыта/открыта)
         */
        private MewingLabEntities4 db = new MewingLabEntities4();
        private DateTime blockedTime;
        private bool passwordIsOpen = false;
        private bool captchaIsOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            GetRegTimeAndBlock();       // Получение из реестра времени блокировки
        }

        /*
            Событие на нажатие кнопки, которая открывает пароль.
            Вначале идёт проверка на то, что пароль открыт, используя
            переменную passwordIsOpen, если пароль открыт, то 
            скрываем TextBox и показываем PasswordBox, иначе в обратном порядке
         */
        private void openPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (!passwordIsOpen)
            {
                passwordTB.Visibility = Visibility.Visible;
                hiddenPasswordPB.Visibility = Visibility.Collapsed;
                passwordIsOpen = true;
            }   
            else
            {
                hiddenPasswordPB.Visibility = Visibility.Visible;
                passwordTB.Visibility = Visibility.Collapsed;
                passwordIsOpen = false;
            }
        }

        /*
            Событие на нажатие клавиши в PasswordBox пароля, чтобы синхронизировать
            с TextBox
         */
        private void hiddenPasswordPB_KeyUp(object sender, KeyEventArgs e)
        {
            passwordTB.Text = hiddenPasswordPB.Password;
        }

        /*
            Событие на нажатие клавиши в TextBox пароля, чтобы синхронизировать
            с PasswordBox
         */
        private void passwordTB_KeyUp(object sender, KeyEventArgs e)
        {
            hiddenPasswordPB.Password = passwordTB.Text;
        }

        /*
            Событие на нажатие кнопки "Войти", по нажатию кнопки происходит
            валидация всех полей, чтобы они соответствовали требованиям
            как только все поля будут корректны, то начнётся проверка на 
            капчу, если открыта, то проверяем логин и пароль вместе с капчей, 
            если пользователь косячит и не вводит правильный пароль или капчу,
            то прилетает блокировка на 10 секунд. Если всё будет хорошо, то пользователь
            буде авторизован
         */
        private async void loginButton_Click_1(object sender, RoutedEventArgs e)
        {
            // Проверка на пустоту логина
            if (loginTB.Text == "")
            {
                Helper.Message("Поле логин не может быть пустым!", "error");
                return;
            }

            // Проверка на пустоту пароля
            if (passwordTB.Text == "")
            {
                Helper.Message("Поле пароля не может быть пустым!", "error");
                return;
            }

            // Проверка на количество пробелов, используя Split
            string[] wordsInLogin = loginTB.Text.Split(' ');
            string[] wordsInPassword = passwordTB.Text.Split(' ');

            // Если слов больше, чем 1, то говорим, что надо писать без пробелов
            if (wordsInLogin.Length > 1 || wordsInPassword.Length > 1)
            {
                Helper.Message("В полях не может быть пробелов!", "error");
                return;
            }

            // В отдельные переменные записываем логин и пароль гостя (пользователя, который ещё на этапе входа)
            string guestLogin = loginTB.Text;
            string guestPassword = passwordTB.Text;

            // Поиск пользователя в базе по логину
            users authorizedUser = db.users.Where(u => u.login == guestLogin).FirstOrDefault();

            // Если пользователь такой не найден, то говорим об этом пользователю
            if (authorizedUser == null)
            {
                Helper.Message("Пользователь не найден!", "error");
                return;
            }

            // Проверка на то, что капча открыта
            if (captchaIsOpen)
            {
                // Сверка пароля и капчи от пользователя, если всё хорошо, то авторизуем пользователя
                if (authorizedUser.password == guestPassword && userCaptchaTB.Text == captchaText.Text)
                {
                    AuthenticateUser(authorizedUser);
                }
                else
                {
                    // Заново генерируем капчу и выводим сообщение
                    captchaText.Text = Helper.GenerateCaptcha(6);
                    Helper.Message("Вы не прошли капчу! Блокировка на 10 секунд!", "error");

                    // Сохраняем в истории неудачный вход
                    addLoginToHistory(authorizedUser, 2);

                    // Очищаем все поля, кроме логина
                    passwordTB.Text = "";
                    hiddenPasswordPB.Password = "";
                    userCaptchaTB.Text = "";

                    // Блокируем вход на 10 секунд
                    await BlockLogin(10);
                }
            }
            else
            {
                // Если капчи нет, то сверяем пароль, если всё хорошо, то авторизуем
                if (authorizedUser.password == guestPassword)
                {
                    AuthenticateUser(authorizedUser);
                }
                else
                {
                    Helper.Message("Неправильный пароль! Введите капчу для входа", "error");

                    // Запись в БД об неудачной попытке
                    addLoginToHistory(authorizedUser, 2);

                    // Очистка пароля
                    passwordTB.Text = "";
                    hiddenPasswordPB.Password = "";

                    // Генерируем капчу
                    captchaText.Text = Helper.GenerateCaptcha(6);

                    // Увеличиваем окно и показываем поля с капчей
                    loginWindow.Height = 450;
                    labelCaptcha.Visibility = Visibility.Visible;
                    userCaptchaTB.Visibility = Visibility.Visible;
                    gridCaptcha.Visibility = Visibility.Visible;

                    // Указываем, что капча открыта
                    captchaIsOpen = true;

                    return;
                }
            }
        }

        /*
            Функция принимает пользователя, который авторизуется, выводит сообщение.
            Прячет окно с авторизацией от пользователя, далее проверяет кто пользователь по роли
            и открывает окно, которое соответствует роли пользователя
         */
        private void AuthenticateUser(users currentUser)
        {
            Helper.Message("Авторизация прошла успешно!", "good");

            Hide();

            switch (currentUser.id_user_type)
            {
                case 1:
                    addLoginToHistory(currentUser, 1);

                    LaboratorianWindow laboratorianWindow = new LaboratorianWindow(currentUser);
                    laboratorianWindow.Show();
                    break;
                case 2:
                    addLoginToHistory(currentUser, 1);

                    LaboratorianResearcherWindow laboratorianResearcherWindow = new LaboratorianResearcherWindow(currentUser);
                    laboratorianResearcherWindow.Show();
                    break;
                case 3:
                    addLoginToHistory(currentUser, 1);

                    AccountantWindow accountantWindow = new AccountantWindow(currentUser);
                    accountantWindow.Show();
                    break;
                case 4:
                    addLoginToHistory(currentUser, 1);

                    AdministratorWindow administratorWindow = new AdministratorWindow(currentUser);
                    administratorWindow.Show();
                    break;
                default:
                    Helper.Message("Ошибка входа по данных учётным данным! Обратитесь к системному администратору!", "error");
                    break;
            }
        }

        /*
            Асинхронный Task, который принимает длину блокировки в секундах
            Блокирует все поля на то время, которое передадим, по окончанию времени
            просто все элементы разблокируются
         */
        private async Task BlockLogin(double seconds)
        {
            loginButton.IsEnabled = false;
            loginTB.IsEnabled = false;
            passwordTB.IsEnabled = false;
            hiddenPasswordPB.IsEnabled = false;
            openPasswordButton.IsEnabled = false;
            userCaptchaTB.IsEnabled = false;
            regenerateCaptcha.IsEnabled = false;

            await Task.Delay(TimeSpan.FromSeconds(seconds));

            loginButton.IsEnabled = true;
            loginTB.IsEnabled = true;
            passwordTB.IsEnabled = true;
            hiddenPasswordPB.IsEnabled = true;
            openPasswordButton.IsEnabled = true;
            userCaptchaTB.IsEnabled = true;
            regenerateCaptcha.IsEnabled = true;
        }

        // Событие на кнопку регенерации капчи
        private void regenerateCaptcha_Click(object sender, RoutedEventArgs e)
        {
            captchaText.Text = Helper.GenerateCaptcha(6);
        }

        // Функция, которая записывает в историю входа данные пользователя (его id)
        // и сохраняет в БД время и статус
        private void addLoginToHistory(users currentUser, int status)
        {
            users_history addHistory = new users_history
            {
                id_user = currentUser.id,
                id_login_status = status,
                time = DateTime.Now
            };

            db.users_history.Add(addHistory);
            db.SaveChanges();
        }

        // Функция, которая получает из реестра время блокировки и блокирует на оставшееся время
        private async void GetRegTimeAndBlock()
        {
            RegistryKey registryKey = Registry.CurrentUser;
            RegistryKey timeKey = registryKey.OpenSubKey("MewingLaba");

            if (timeKey != null)
            {
                blockedTime = DateTime.Parse(timeKey.GetValue("time").ToString());
                timeKey.Close();
            }

            if ((DateTime.Now - blockedTime).TotalSeconds < 1800) 
            {
                await BlockLogin(1800 - (DateTime.Now - blockedTime).TotalSeconds);
            }
        }

        private async void loginWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Получение всех окон приложения
            List<Window> windowsToClose = new List<Window>(Application.Current.Windows.Cast<Window>());

            // Из списка удаляем текущее окно
            windowsToClose.Remove(this);

            // Перебираем и асинхронно закрываем окна
            foreach (Window window in windowsToClose)
            {
                await CloseWindowAsync(window);
            }
        }

        // Закрываем окна с задержкой в 1мс
        private async Task CloseWindowAsync(Window window)
        {
            await Task.Delay(1);
            window.Close();
        }

        private void loginTB_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void passwordTB_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void hiddenPasswordPB_PreviewKeyDown(object sender, KeyEventArgs e)
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
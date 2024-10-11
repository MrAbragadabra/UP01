using MewingLab.Classes;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MewingLab.DB;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Input;

namespace MewingLab.Forms
{
    public partial class LaboratorianWindow : Window
    {
        /*
            currentUser - текущий пользователь в формате модели из БД
            biomaterialList - список всех биоматериалов
            megaTimer - таймер
            sessionTime - время посиделок (сессии)
            remainingTime - оставшееся время
            userInformed - проинформирован пользователь или нет
         */
        private MewingLabEntities4 db = new MewingLabEntities4();
        private users currentUser;
        private List<biomaterials> biomaterialsList = new List<biomaterials>();
        private List<patients> patientsList = new List<patients>();
        private DispatcherTimer megaTimer;
        private TimeSpan sessionTime = TimeSpan.FromMinutes(150);
        private TimeSpan remainingTime;
        private bool userInformed = false;
        public LaboratorianWindow(users _currentUser)
        {
            InitializeComponent();
            currentUser = _currentUser;

            // Получение биоматериалов и пациентов в список 
            biomaterialsList = db.biomaterials.ToList();
            patientsList = db.patients.ToList();

            // Привязка биоматериалов и пациентов в список 
            biomaterialsLV.ItemsSource = biomaterialsList;
            patientsCMB.ItemsSource = patientsList;

            // Запись в поле кода последнего +1 номера
            orders lastOrder = db.orders.OrderByDescending(o => o.id).FirstOrDefault();
            int newNumber = lastOrder.number + 1;
            codeTB.Text = newNumber.ToString();

            megaTimer = new DispatcherTimer();               // Экземпляр DispatcherTimer
            megaTimer.Interval = TimeSpan.FromSeconds(1);    // Установка длины интервала таймера в 1сек
            megaTimer.Tick += Timer_Tick;                    // Подписка на событие

            // Начинаем посиделки
            StartSession();
        }

        private void StartSession()
        {
            // Запуск таймера
            remainingTime = sessionTime;
            megaTimer.Start();
        }

        /*
            Событие на каждый Тик таймера (раз в секунду)
         */
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Вычитаем 1 секунду от оставшегося времени
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));

            // Если времени осталось меньше 0, то блокируемся и закрываем окно
            if (remainingTime.TotalSeconds <= 0)
            {
                // Останавливаем таймер
                megaTimer.Stop();

                // Сохраняем текущее время в реестре
                Helper.SaveBlockTime();

                // Закрываем окно
                Close();

                // Открываем вход
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                return;
            }

            // Обновляем время
            UpdateTime();
        }

        /*
            Функция обновления времени, которая также выводит за 15 минут до блокировки сообщение
         */
        private void UpdateTime()
        {
            // Вывод оставшегося времени в формате hh\:mm\:ss
            TimeSpan displayTime = remainingTime;
            timer.Text = displayTime.ToString(@"hh\:mm\:ss");
            
            // Если времени осталось меньше 15 минут и мы не проинформированы, то выводим сообщение
            if (displayTime.TotalSeconds <= 900 && userInformed == false)
            {
                Helper.Message("Осталось 15 минут!", "good");
                userInformed = true;
            }
        }

        /*
            При закрытии текущего окна открывается окно входа
         */
        private void laboratorianWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        /*
            При загрузке формы строится путь до фото, а также выводится информация
            о пользователе
         */
        private void laboratorianWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Построение пути до фото
            string savePath = System.IO.Path.GetFullPath(@"..\Avatars");
            savePath = savePath + "\\" + currentUser.avatar_path;

            // Генерация Source для фото
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(savePath);
            bitmap.EndInit();
            userAvatar.Source = bitmap;

            // Вывод информации из базы
            userText.Text = $"Лаборант {currentUser.second_name}. Логин: {currentUser.login}";
        }

        private void biomaterialsLV_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // При выборе биоматериала включаем элементы формы
            codeTB.IsEnabled = true;
            patientsCMB.IsEnabled = true;
            addPatientButton.IsEnabled = true;
            updatePatientsButton.IsEnabled = true;
        }

        private void updatePatientsButton_Click(object sender, RoutedEventArgs e)
        {
            // Обновление списка пациентов
            patientsList = db.patients.ToList();
            patientsCMB.ItemsSource = null;
            patientsCMB.ItemsSource = patientsList;
        }

        private void addPatientButton_Click(object sender, RoutedEventArgs e)
        {
            // Открытие окна добавления пациента
            AddPatientWindow addPatientWindow = new AddPatientWindow();
            addPatientWindow.ShowDialog();
        }

        private void addServicesToOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на то, что поле не пустое, имеет 6 символов и только цифры
            int userNumber = 0;
            if (codeTB.Text == "" || !codeTB.Text.All(char.IsDigit) || codeTB.Text.Length != 6)
            {
                Helper.Message("Поле кода должно быть только из цифр и шесть символов!", "error");
                return;
            }

            try
            {
                // Если не получиться конвертировать в int, то косячник юзер
                userNumber = int.Parse(codeTB.Text);
            }
            catch
            {
                Helper.Message("Ошибка в поле кода! Он не должен начинаться с нуля!", "error");
                return;
            }

            // Ищем по номеру, есть ли такой номер
            orders checkOrder = db.orders.Where(o => o.number == userNumber).FirstOrDefault();

            // Если такой номер есть, то кричим на пользователя
            if (checkOrder != null)
            {
                Helper.Message("Данный код занят! Попробуйте другой!", "error");
                return;
            }

            // Запись в отдельные переменные пациента и биоматериала
            patients selectedPatient = (patients)patientsCMB.SelectedItem;
            biomaterials selectedBiomaterial = (biomaterials)biomaterialsLV.SelectedItem;

            // Добавление в БД запись
            orders addOrder = new orders
            {
                id_order_status = 1,
                creation_date = DateTime.Now,
                id_user = currentUser.id,
                number = int.Parse(codeTB.Text),
                id_patient = selectedPatient.id,
                id_biomaterial = selectedBiomaterial.id,
            };

            // Сохранение в БД
            db.orders.Add(addOrder);
            db.SaveChanges();

            // Получение последней записи заказа
            orders lastOrder = db.orders.OrderByDescending(o => o.id).FirstOrDefault();

            // Формирование кода id + дата + уникальный код
            string code = $"{lastOrder.id}{lastOrder.creation_date.ToShortDateString()}{lastOrder.number}";

            // Удаление из строки всех знаков пунктуации, так как дата идёт с точками
            code = new string(code.Where(c => !Char.IsPunctuation(c)).ToArray());

            // Переменная, которая в будущем будет в себе хранить штрих-код
            System.Drawing.Image barcodeImage = null;

            // Создание экземпляра класса Barcode
            BarcodeLib.Barcode barcode = new BarcodeLib.Barcode();

            // Задача цвета фона и шрифта
            barcode.BackColor = System.Drawing.Color.White;
            barcode.ForeColor = System.Drawing.Color.Black;

            // Включаем подпись
            barcode.IncludeLabel = true;

            // Расположение штрих кода и подписи
            barcode.Alignment = BarcodeLib.AlignmentPositions.CENTER;
            barcode.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;

            // Формат, в котором будет генерироваться штрихкод
            barcode.ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;

            // Установка шрифта для подписи
            System.Drawing.Font font = new System.Drawing.Font("verdana", 10f);
            barcode.LabelFont = font;

            // Установка высоты и ширины штрих кода
            barcode.Height = 100;
            barcode.Width = 200;

            // Генерация штрих кода и запись в image
            barcodeImage = barcode.Encode(BarcodeLib.TYPE.CODE128C, code);

            // Построение пути папки со штрих кодами
            string savePath = System.IO.Path.GetFullPath(@"..\Barcodes");
            savePath = savePath + "\\" + $"{code}.pdf";

            // Формирование документа по размеру штрих кода
            Document document = new Document(new iTextSharp.text.Rectangle(barcodeImage.Width, barcodeImage.Height));

            // Сохранение PDF по пути в режиме Create
            PdfWriter.GetInstance(document, new FileStream(savePath, FileMode.Create));

            // Открываем документик
            document.Open();

            // Генерируем изображение для PDF
            iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(barcodeImage, ImageFormat.Jpeg);

            // Фото по середине
            pdfImage.SetAbsolutePosition((document.PageSize.Width - pdfImage.ScaledWidth) / 2, (document.PageSize.Height - pdfImage.ScaledHeight) / 2);

            // Помещаем это изображение в PDF
            document.Add(pdfImage);

            // Закрываем документа
            document.Close();

            // Запись в поле кода последнего +1 номера
            int newNumber = lastOrder.number + 1;
            codeTB.Text = newNumber.ToString();

            // Открываем окно с добавление услуг к заказу
            AddServicesToOrder addServicesToOrder = new AddServicesToOrder(savePath);
            addServicesToOrder.ShowDialog();

            // Блокируем все поля
            codeTB.IsEnabled = false;
            patientsCMB.IsEnabled = false;
            addPatientButton.IsEnabled = false;
            updatePatientsButton.IsEnabled = false;
            addServicesToOrderButton.IsEnabled = false;
        }

        private void patientsCMB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // При выборе пациента включается кнопка добавления заказа
            addServicesToOrderButton.IsEnabled = true;
        }

        private void codeTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
        }

        private void codeTB_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
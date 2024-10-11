using MewingLab.Classes;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MewingLab.DB;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using SkiaSharp;
using System.Data.Entity.Migrations;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;

namespace MewingLab.Forms
{

    public partial class LaboratorianResearcherWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool LockWorkStation();
        /*
            currentUser - текущий пользователь в формате модели из БД
            megaTimer - таймер
            sessionTime - время посиделок (сессии)
            remainingTime - оставшееся время
            userInformed - проинформирован пользователь или нет
         */
        private users currentUser;
        private DispatcherTimer megaTimer;
        private DispatcherTimer AnalizatorTimer;
        private TimeSpan sessionTime = TimeSpan.FromMinutes(150);
        private TimeSpan remainingTime;
        private bool userInformed = false;
        private List<services_in_order> allServiceInOrder;
        private List<services_in_order> allServiceInWork = new List<services_in_order>();
        private MewingLabEntities4 db = new MewingLabEntities4();
        private services_in_order selectedSerivesForTimer;
        private GetAnalizator AnalizatorResults;
        public LaboratorianResearcherWindow(users _currentUser)
        {
            InitializeComponent();
            currentUser = _currentUser;

            megaTimer = new DispatcherTimer();              // Экземпляр DispatcherTimer
            megaTimer.Interval = TimeSpan.FromSeconds(1);   // Установка длины интервала таймера в 1сек
            megaTimer.Tick += Timer_Tick;                   // Подписка на событие

            AnalizatorTimer = new DispatcherTimer();
            AnalizatorTimer.Interval = TimeSpan.FromSeconds(5);
            AnalizatorTimer.Tick += AnalizatorTimer_Tick;

             

            // Начинаем посиделки
            StartSession();
        }

        private void AnalizatorTimer_Tick(object sender, EventArgs e)
        {
            AnalizatorResults = GetResultsFromAnalizator(selectedSerivesForTimer);
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
        private void laboratorianResearcherWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void laboratorianResearcherWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Построение пути до фото
            string savePath = System.IO.Path.GetFullPath(@"..\Avatars");
            savePath = savePath + "\\" + currentUser.avatar_path;

            // Генерация Source для фото
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new System.Uri(savePath);
            bitmap.EndInit();
            userAvatar.Source = bitmap;

            // Вывод информации из базы
            userText.Text = $"Лаборант-исследователь {currentUser.second_name}. Логин: {currentUser.login}";
        }

        private void servicesInOrderList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            sendToAnalyzeButton.IsEnabled = true;
        }

        private async void sendToAnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            services_in_order selectedService = (services_in_order)servicesInOrderList.SelectedItem;
            MessageBoxResult result = MessageBox.Show("Вы точно хотите отправить данный материал в анализатор?", "Подтверждение", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // Создание экземпляра класса Services
                Services analyzedService = new Services();
                analyzedService.serviceCode = selectedService.id_service;

                // Добавление в список услуги
                List<Services> services = new List<Services> { analyzedService };

                // Запись в строку id пациента
                string patient = selectedService.orders.id_patient.ToString();

                try
                {
                    // Создание ссылки запроса на API
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{selectedService.services.analyzer.analyzer_name}");

                    // Указание типа запроса и типа контента
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";

                    // Создание JSON и его отправка
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = new JavaScriptSerializer().Serialize(new
                        {
                            patient,
                            services
                        });

                        streamWriter.Write(json);
                    }

                    // Получение ответа
                    HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    // Если всё хорошо то хорошо
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        selectedSerivesForTimer = selectedService;

                        // Изменение статуса в базе данных
                        selectedService.id_status = 4;
                        db.services_in_order.AddOrUpdate(selectedService);
                        db.SaveChanges();

                        // Обновление списка открытых заказов
                        allServiceInOrder.Clear();
                        servicesInOrderList.ItemsSource = null;

                        allServiceInOrder = db.services_in_order.Where(s => s.id_status == 1 || s.id_status == 3).ToList();
                        servicesInOrderList.ItemsSource = allServiceInOrder;

                        //Блокировка кнопки на отправку в анализатор
                        servicesInOrderList.IsEnabled = false;
                        sendToAnalyzeButton.IsEnabled = false;

                        MessageBox.Show("Услуги успешно отправлены! Ожидайте 5 секунд!");

                        // Начало таймера и sleep
                        AnalizatorTimer.Start();
                        await Task.Delay(6000);

                        if (AnalizatorResults.services != null)
                        {
                            // Стоп таймера
                            AnalizatorTimer.Stop();

                            MessageBoxResult analizatorResult = MessageBox.Show($"Тест №{selectedSerivesForTimer.id} на {selectedSerivesForTimer.services.name} получен с результатом {AnalizatorResults.services[0].result}. \n Вы принимаете данный результат?", "Результат теста", MessageBoxButton.YesNo);

                            if (analizatorResult == MessageBoxResult.Yes)
                            {
                                // Добавление результата в БД
                                results newResult = new results
                                {
                                    patient_id = selectedSerivesForTimer.orders.patients.id,
                                    services_id = selectedSerivesForTimer.id_service,
                                    result = AnalizatorResults.services[0].result,
                                    analyzer_id = selectedSerivesForTimer.services.analyzer_id,
                                    order_id = selectedSerivesForTimer.orders.id,
                                };

                                // Сохранение в БД
                                db.results.Add(newResult);
                                db.SaveChanges();

                                // Разблокировка
                                servicesInOrderList.IsEnabled = true;
                                sendToAnalyzeButton.IsEnabled = true;

                                MessageBox.Show("Результат исследования записан в базу данных!");
                            }
                            else
                            {
                                // Изменение статуса заказа
                                selectedSerivesForTimer.id_status = 3;
                                db.services_in_order.AddOrUpdate(selectedSerivesForTimer);
                                db.SaveChanges();

                                allServiceInOrder = db.services_in_order.Where(s => s.id_status == 1 || s.id_status == 3).ToList();
                                servicesInOrderList.ItemsSource = allServiceInOrder;

                                servicesInOrderList.IsEnabled = true;
                                sendToAnalyzeButton.IsEnabled = true;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ну всё, запрос сломался!");
                    }
                }
                catch
                {
                    MessageBox.Show("Ошибка подключения к анализатору!");
                }
                
            }
            else
            {
                LockWorkStation();
            }
        }

        private GetAnalizator GetResultsFromAnalizator(services_in_order selectedService)
        {
            GetAnalizator getAnalizators = new GetAnalizator();

            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{selectedService.services.analyzer.analyzer_name}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        string json = reader.ReadToEnd();
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        getAnalizators = serializer.Deserialize<GetAnalizator>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return getAnalizators;
        }
    }


    public class Services
    {
        public int serviceCode { get; set; }
        public string result { get; set; }
    }

    public class GetAnalizator
    {
        public string patient { get; set; }
        public List<Services> services { get; set; }
        public int progress { get; set; }
    }
}

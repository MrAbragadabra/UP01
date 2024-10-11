using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using MewingLab.Classes;
using MewingLab.DB;
namespace MewingLab.Forms
{
    public partial class AddServicesToOrder : Window
    {
        private MewingLabEntities4 db = new MewingLabEntities4();
        private List<services> servicesList = new List<services>();
        private List<services> SelectedServicesList = new List<services>();
        private string barcodePath;
        public AddServicesToOrder(string _barcodePath)
        {
            InitializeComponent();

            // Добавление в список всех услуг из БД
            servicesList = db.services.ToList();
            barcodePath = _barcodePath;
        }

        private void addServicesToOrder_Loaded(object sender, RoutedEventArgs e)
        {
            // Привязка к ComboBox списка
            servicesCMB.ItemsSource = servicesList;
        }

        private void servicesCMB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // При нажатии на элемент из списка услуг, проходимся по
            // Списку выбранных услуг, если такой есть уже, то ничего не делаем
            // Если нет, то добавляем
            for (int i = 0; i < SelectedServicesList.Count; i++)
            {
                if (SelectedServicesList[i] == (services)servicesCMB.SelectedItem)
                {
                    SelectedServicesList.Remove((services)servicesCMB.SelectedItem);
                }
            }

            SelectedServicesList.Add((services)servicesCMB.SelectedItem);

            // Обновление списка
            selectedServicesCMB.ItemsSource = null;
            selectedServicesCMB.ItemsSource = SelectedServicesList;
        }

        private void selectedServicesCMB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedServicesList.Remove((services)selectedServicesCMB.SelectedItem);
            selectedServicesCMB.ItemsSource = null;
            selectedServicesCMB.ItemsSource = SelectedServicesList;
        }

        private void addServicesToOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Если услуги есть в списке, то добавляем в базу все услуги в пользователя
            if (SelectedServicesList.Count != 0)
            {
                // Получение последней записи в заказах
                orders last = db.orders.OrderByDescending(u => u.id).FirstOrDefault();

                // Добавление услуг в заказ
                foreach (services i in selectedServicesCMB.Items)
                {
                    services_in_order add = new services_in_order
                    {
                        id_order = last.id,
                        id_service = i.id,
                        id_status = 1
                    };

                    db.services_in_order.Add(add);
                }
                db.SaveChanges();

                List<services_in_order> allServicesInCurrentOrder = new List<services_in_order>();
                allServicesInCurrentOrder = db.services_in_order.Where(o => o.id_order == last.id).ToList();

                int sumDays = 0;
                double sumPrice = 0;

                // Добавление в заказ суммы дней и суммы денег
                foreach (services_in_order i in allServicesInCurrentOrder)
                {
                    services currentServices = db.services.Where(s => s.id == i.id_service).FirstOrDefault();

                    sumDays = sumDays + currentServices.days;
                    sumPrice = sumPrice + currentServices.price;
                }

                last.day_in_work = sumDays;
                last.summ = sumPrice;
                db.SaveChanges();

                Helper.Message("Заказ оформлен!", "good");
                Process.Start(barcodePath);

                Close();
            }
            else
            {
                Helper.Message("Вы не выбрали услуги!!!", "error");
            }
        }
    }
}

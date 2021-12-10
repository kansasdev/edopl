
using PieEatingNinjas.EIdReader.UWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Proximity;
using Windows.Security.Cryptography.Certificates;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace edopl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private int selectedReader;
        private List<SmartCard> lstCard;
        private List<Certificate> lstCertificate;
        public MainPage()
        {
            this.InitializeComponent();
            selectedReader = 0;
            lstCard = new List<SmartCard>();
            lstCertificate = new List<Certificate>();
            InitSmartcard().GetAwaiter().GetResult();

        }

        private static async Task InitSmartcard()
        {
           


            await Task.CompletedTask;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                cmbReaders.Items.Clear();
                SetProgress(true);
                
                string selector = SmartCardReader.GetDeviceSelector();
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);
                int counter = 0;
                foreach (DeviceInformation di in devices)
                {
                    SmartCardReader rdr = await SmartCardReader.FromIdAsync(di.Id);

                    cmbReaders.Items.Add(rdr.Name);

                    IReadOnlyList<SmartCard> cards = await rdr.FindAllCardsAsync();

                    if(cards.Count>0)
                    {
                        selectedReader = counter;
                        foreach(SmartCard card in cards)
                        {
                            
                            lstCard.Add(card);
                        }
                        counter++;
                    }
                    else
                    {
                        counter++;
                        continue;
                    }
                    
                }
                if(devices.Count==0)
                {
                    throw new Exception("Brak odpowiedniego czytnika kart. Musisz mieć zainstalowany bezstykowy czytnik kart inteligentnych");
                }
                else
                {
                    cmbReaders.SelectedIndex = selectedReader;
                }
            }
            catch (Exception ex)
            {
                ContentDialog cd = new ContentDialog()
                {
                    Title = "Błąd wykonywania",
                    Content = ex.Message,
                    CloseButtonText = "OK"
                };
                await cd.ShowAsync();
            }
            finally
            {
                SetProgress(false);
            }
        }

        private async void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void cmbReaders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void SetProgress(bool isStart)
        {
            if (isStart)
            {
                pbProgress.Visibility = Visibility.Visible;
                btnAbout.IsEnabled = false;
                btnInitialize.IsEnabled = false;
                btnOdczytajCertyfikaty.IsEnabled = false;
                cmbReaders.IsEnabled = false;
            }
            else
            {
                pbProgress.Visibility = Visibility.Collapsed;
                btnAbout.IsEnabled = true;
                btnInitialize.IsEnabled=true;
                btnOdczytajCertyfikaty.IsEnabled = true;
                cmbReaders.IsEnabled=true;
            }
        }

        private async void btnOdczytajCertyfikaty_Click(object sender, RoutedEventArgs e)
        {
            if (lstCard.Count > 0)
            {
                SmartCard card = lstCard[0];

                CertificateQuery cq = new CertificateQuery();
                //cq.HardwareOnly = true;
                cq.StoreName = "MY";

                IReadOnlyList<Certificate> iCert = await CertificateStores.FindAllAsync(cq);

                foreach (Certificate cert in iCert)
                {
                    if (cert.Issuer.Contains("pl.ID"))
                    {
                        lstCertificate.Add(cert);
                    }
                }
                if (lstCertificate.Count > 0)
                {

                }
                else
                {
                    ContentDialog cd = new ContentDialog()
                    {
                        Title = "Brak certyfikatów",
                        Content = "Czy masz zainstalowane oprogramowanie pośredniczące? Jeżeli nie, zainstaluj je z: https://www.gov.pl/pliki/edowod/e-dowod-4.2.2.2.exe"+Environment.NewLine+
                        "Czy położyłeś dokument na czytniku? Czy podałeś CAN?",
                        CloseButtonText = "OK"
                    };
                    await cd.ShowAsync();
                }
            }
            else
            {
                ContentDialog cd = new ContentDialog()
                {
                    Title = "Błąd wykonywania",
                    Content = "Nie wykryto kart w czytniku.Zainicjalizowałeś czytnik?",
                    CloseButtonText = "OK"
                };
                await cd.ShowAsync();
            }
        }
    }
}

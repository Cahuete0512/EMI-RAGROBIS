﻿using EMI_RA.API.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EMI_RA.WPF
{
    /// <summary>
    /// Logique d'interaction pour AjouterAdhérent.xaml
    /// </summary>
    public partial class AjouterAdhérent : Page
    {
        String Societe = "";
        String CiviliteContact = "";
        String NomContact = "";
        String PrenomContact = "";
        String Email = "";
        String Adresse = "";

        public AjouterAdhérent()
        {
            InitializeComponent();
            

        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Societe = societe.Text;
            CiviliteContact = civilite.Text;
            NomContact = nom.Text;
            PrenomContact = prenom.Text;
            Email = email.Text;
            Adresse = adresse.Text;


            var clientApi = new Client("https://localhost:44313/", new HttpClient());

            var adherentDTO = new Adherents_DTO()
            {

                Societe = Societe,
                CiviliteContact = CiviliteContact,
                NomContact = NomContact,
                PrenomContact = PrenomContact,
                Email = Email,
                Adresse = Adresse

            };
            var adherent = await clientApi.AdherentsPOSTAsync(adherentDTO);
            MessageBox.Show("L'adhérent a été enregistré");


        }

      
    }
}

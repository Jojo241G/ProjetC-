using System;

namespace Pressing_Wpf
{
    public class Commande
    {
        public string Code { get; set; }
        public string NomClient { get; set; }
        public string Telephone { get; set; }
        public double PoidsKg { get; set; }       // Poids (2 décimales)
        public int NombreArticles { get; set; }   // NOUVEAU : nombre d'articles
        public ServiceType TypeService { get; set; }
        public double Montant { get; set; }
        public DateTime DateDepot { get; set; }
        public DateTime DateRetrait { get; set; }
        public bool EstRecuperee { get; set; }

        // Pour affichage (DataGrid, etc.)
        public string StatutTexte => EstRecuperee ? "Récupérée" : "En attente";

        // Constructeur pour les nouvelles commandes
        public Commande(string nom, string telephone, double poids, int nbArticles, ServiceType type)
        {
            NomClient = nom;
            Telephone = telephone;

            poids = Math.Round(poids, 2);
            if (poids < 1)
                throw new Exception("Le poids doit être supérieur ou égal à 1 Kg.");
            PoidsKg = poids;

            if (nbArticles <= 0)
                throw new Exception("Le nombre d'articles doit être supérieur à 0.");
            NombreArticles = nbArticles;

            TypeService = type;
            DateDepot = DateTime.Now;
            EstRecuperee = false;

            Code = GenererCode();
            CalculerMontant();
            CalculerDateRetrait();
        }

        // Constructeur pour la lecture depuis SQLite
        public Commande(string code, string nom, string tel,
                        double poids, int nbArticles, ServiceType type,
                        double montant, DateTime depot, DateTime retrait, bool recuperee)
        {
            Code = code;
            NomClient = nom;
            Telephone = tel;
            PoidsKg = Math.Round(poids, 2);
            NombreArticles = nbArticles;
            TypeService = type;
            Montant = montant;
            DateDepot = depot;
            DateRetrait = retrait;
            EstRecuperee = recuperee;
        }

        private string GenererCode()
        {
            return "PR-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        }

        private void CalculerMontant()
        {
            double prixKg = (TypeService == ServiceType.Standar) ? 500 : 1000;
            double montantBrut = prixKg * PoidsKg;

            // Arrondi au multiple supérieur de 25
            Montant = ArrondirMultiple25(montantBrut);
        }

        private double ArrondirMultiple25(double m)
        {
            return Math.Ceiling(m / 25.0) * 25;
        }

        private void CalculerDateRetrait()
        {
            DateRetrait = (TypeService == ServiceType.Standar)
                ? DateDepot.AddHours(48)
                : DateDepot.AddHours(24);
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;

namespace Pressing_Wpf
{
    public partial class MainWindow : Window
    {
        private bool MenuOpen = false;
        private readonly PressingManager _pressing = new PressingManager();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuOpen = !MenuOpen;

            Storyboard sb = MenuOpen
                ? (Storyboard)FindResource("SlideMenuIn")
                : (Storyboard)FindResource("SlideMenuOut");

            sb.Begin(SideMenu);
        }

        private TextBlock MakeLabel(string txt) =>
            new TextBlock { Text = txt, Foreground = Brushes.White, FontSize = 16, Margin = new Thickness(0, 6, 0, 2) };

        // ========= ENREGISTRER =========
        private void OpenRegister(object sender, RoutedEventArgs e)
        {
            TitleBlock.Text = "Enregistrer une commande";
            ContentPanel.Children.Clear();

            StackPanel panel = new() { Margin = new Thickness(0, 10, 0, 0) };

            TextBox txtNom = new() { Padding = new Thickness(8) };
            TextBox txtTel = new() { Padding = new Thickness(8) };
            TextBox txtPoids = new() { Padding = new Thickness(8) };
            TextBox txtArticles = new() { Padding = new Thickness(8) };

            ComboBox cbService = new() { Padding = new Thickness(6) };
            cbService.Items.Add("Standar");
            cbService.Items.Add("Express");
            cbService.SelectedIndex = 0;

            panel.Children.Add(MakeLabel("Nom du client"));
            panel.Children.Add(txtNom);

            panel.Children.Add(MakeLabel("Téléphone"));
            panel.Children.Add(txtTel);

            panel.Children.Add(MakeLabel("Poids (Kg, ex : 2.30)"));
            panel.Children.Add(txtPoids);

            panel.Children.Add(MakeLabel("Nombre d'articles"));
            panel.Children.Add(txtArticles);

            panel.Children.Add(MakeLabel("Type de service"));
            panel.Children.Add(cbService);

            TextBlock lblInfo = new()
            {
                Foreground = Brushes.LightGreen,
                Margin = new Thickness(0, 15, 0, 0),
                FontSize = 14
            };
            panel.Children.Add(lblInfo);

            Button btnSave = new()
            {
                Content = "✔ Enregistrer + Générer le ticket PDF",
                Style = (Style)FindResource("ModernButton"),
                Margin = new Thickness(0, 15, 0, 0)
            };

            btnSave.Click += (s, a) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtNom.Text) ||
                        string.IsNullOrWhiteSpace(txtTel.Text) ||
                        string.IsNullOrWhiteSpace(txtPoids.Text) ||
                        string.IsNullOrWhiteSpace(txtArticles.Text))
                    {
                        MessageBox.Show("Merci de remplir tous les champs.");
                        return;
                    }

                    double p = double.Parse(txtPoids.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                    int articles = int.Parse(txtArticles.Text);
                    ServiceType type = cbService.SelectedIndex == 0 ? ServiceType.Standar : ServiceType.Express;

                    var cmd = _pressing.EnregistrerCommande(txtNom.Text.Trim(), txtTel.Text.Trim(), p, articles, type);
                    TicketGenerator.GenererTicket(cmd);

                    string message =
                        $"Commande enregistrée avec succès !\n\n" +
                        $"Code : {cmd.Code}\n" +
                        $"Client : {cmd.NomClient}\n" +
                        $"Articles : {cmd.NombreArticles}\n" +
                        $"Montant : {cmd.Montant} FCFA\n" +
                        $"Retrait prévu : {cmd.DateRetrait}\n\n" +
                        $"Un ticket PDF a été généré dans le dossier 'Tickets'.";

                    MessageBox.Show(message, "Commande enregistrée", MessageBoxButton.OK, MessageBoxImage.Information);

                    lblInfo.Text = $"Dernier code généré : {cmd.Code}  |  Articles : {cmd.NombreArticles}  |  Montant : {cmd.Montant} FCFA";

                    txtPoids.Clear();
                    txtArticles.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message);
                }
            };

            panel.Children.Add(btnSave);
            ContentPanel.Children.Add(panel);
        }

        // ========= RECHERCHE PAR CODE =========
        private void OpenSearch(object sender, RoutedEventArgs e)
        {
            TitleBlock.Text = "Rechercher une commande (par code)";
            ContentPanel.Children.Clear();

            StackPanel panel = new();

            panel.Children.Add(MakeLabel("Code de la commande (ex : PR-XXXXXX)"));
            TextBox input = new() { Padding = new Thickness(8) };
            panel.Children.Add(input);

            Button btnSearch = new()
            {
                Content = "🔍 Rechercher",
                Style = (Style)FindResource("ModernButton")
            };

            TextBox result = new()
            {
                Background = Brushes.Black,
                Foreground = Brushes.White,
                Padding = new Thickness(10),
                Height = 280,
                Margin = new Thickness(0, 15, 0, 0),
                IsReadOnly = true,
                AcceptsReturn = true
            };

            btnSearch.Click += (s, a) =>
            {
                var c = _pressing.RechercherParCode(input.Text.Trim());

                if (c == null)
                {
                    result.Text = "Aucune commande trouvée pour ce code.";
                }
                else
                {
                    result.Text =
                        $"Code : {c.Code}\n" +
                        $"Client : {c.NomClient}\n" +
                        $"Téléphone : {c.Telephone}\n" +
                        $"Poids : {c.PoidsKg:0.00} Kg\n" +
                        $"Articles : {c.NombreArticles}\n" +
                        $"Service : {c.TypeService}\n" +
                        $"Montant : {c.Montant} FCFA\n" +
                        $"Déposé le : {c.DateDepot}\n" +
                        $"Retrait prévu : {c.DateRetrait}\n" +
                        $"Statut : {c.StatutTexte}";
                }
            };

            panel.Children.Add(btnSearch);
            panel.Children.Add(result);
            ContentPanel.Children.Add(panel);
        }

        // ========= RECHERCHE PAR NOM / TÉL =========
        private void OpenSearchByClient(object sender, RoutedEventArgs e)
        {
            TitleBlock.Text = "Rechercher un code (nom / téléphone)";
            ContentPanel.Children.Clear();

            StackPanel panel = new();

            panel.Children.Add(MakeLabel("Nom du client ou numéro de téléphone"));
            TextBox input = new() { Padding = new Thickness(8) };
            panel.Children.Add(input);

            Button btnSearch = new()
            {
                Content = "🔎 Rechercher les commandes du client",
                Style = (Style)FindResource("ModernButton")
            };

            TextBox result = new()
            {
                Background = Brushes.Black,
                Foreground = Brushes.White,
                Padding = new Thickness(10),
                Height = 280,
                Margin = new Thickness(0, 15, 0, 0),
                IsReadOnly = true,
                AcceptsReturn = true
            };

            btnSearch.Click += (s, a) =>
            {
                var list = _pressing.RechercherParNomOuTel(input.Text.Trim());

                if (list.Count == 0)
                {
                    result.Text = "Aucune commande trouvée pour ce client.";
                    return;
                }

                result.Clear();
                foreach (var c in list)
                {
                    result.AppendText(
                        $"Code : {c.Code}\n" +
                        $"Client : {c.NomClient}\n" +
                        $"Téléphone : {c.Telephone}\n" +
                        $"Articles : {c.NombreArticles}\n" +
                        $"Montant : {c.Montant} FCFA\n" +
                        $"Retrait : {c.DateRetrait}\n" +
                        $"Statut : {c.StatutTexte}\n" +
                        "--------------------------------------\n");
                }
            };

            panel.Children.Add(btnSearch);
            panel.Children.Add(result);
            ContentPanel.Children.Add(panel);
        }

        // ========= LISTE DES COMMANDES (DataGrid) =========
        private void OpenList(object sender, RoutedEventArgs e)
        {
            TitleBlock.Text = "Liste des commandes";
            ContentPanel.Children.Clear();

            var data = _pressing.ObtenirToutesCommandes();

            DataGrid dg = new()
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Margin = new Thickness(0, 10, 0, 0),
                Height = 400
            };

            dg.Columns.Add(new DataGridTextColumn { Header = "Code", Binding = new Binding("Code") });
            dg.Columns.Add(new DataGridTextColumn { Header = "Client", Binding = new Binding("NomClient") });
            dg.Columns.Add(new DataGridTextColumn { Header = "Téléphone", Binding = new Binding("Telephone") });
            dg.Columns.Add(new DataGridTextColumn { Header = "Poids (Kg)", Binding = new Binding("PoidsKg") { StringFormat = "0.00" } });
            dg.Columns.Add(new DataGridTextColumn { Header = "Articles", Binding = new Binding("NombreArticles") });
            dg.Columns.Add(new DataGridTextColumn { Header = "Service", Binding = new Binding("TypeService") });
            dg.Columns.Add(new DataGridTextColumn { Header = "Montant (FCFA)", Binding = new Binding("Montant") });
            dg.Columns.Add(new DataGridTextColumn { Header = "Déposé le", Binding = new Binding("DateDepot") });
            dg.Columns.Add(new DataGridTextColumn { Header = "Retrait le", Binding = new Binding("DateRetrait") });
            dg.Columns.Add(new DataGridTextColumn { Header = "Statut", Binding = new Binding("StatutTexte") });

            dg.ItemsSource = data;

            ContentPanel.Children.Add(dg);
        }

        // ========= RÉCUPÉRER =========
        private void OpenRecover(object sender, RoutedEventArgs e)
        {
            TitleBlock.Text = "Récupérer une commande";
            ContentPanel.Children.Clear();

            StackPanel panel = new();
            panel.Children.Add(MakeLabel("Code de la commande"));
            TextBox input = new() { Padding = new Thickness(8) };
            panel.Children.Add(input);

            Button btn = new()
            {
                Content = "✔ Marquer comme récupérée",
                Style = (Style)FindResource("ModernButton")
            };

            btn.Click += (s, a) =>
            {
                if (_pressing.RecupererCommande(input.Text.Trim()))
                    MessageBox.Show("Commande marquée comme récupérée.");
                else
                    MessageBox.Show("Commande introuvable.");
            };

            panel.Children.Add(btn);
            ContentPanel.Children.Add(panel);
        }

        // ========= ANNULER =========
        private void OpenCancel(object sender, RoutedEventArgs e)
        {
            TitleBlock.Text = "Annuler une commande";
            ContentPanel.Children.Clear();

            StackPanel panel = new();
            panel.Children.Add(MakeLabel("Code de la commande à annuler"));
            TextBox input = new() { Padding = new Thickness(8) };
            panel.Children.Add(input);

            Button btn = new()
            {
                Content = "❌ Annuler la commande",
                Style = (Style)FindResource("ModernButton")
            };

            btn.Click += (s, a) =>
            {
                if (_pressing.AnnulerCommande(input.Text.Trim()))
                    MessageBox.Show("Commande annulée.");
                else
                    MessageBox.Show("Commande introuvable.");
            };

            panel.Children.Add(btn);
            ContentPanel.Children.Add(panel);
        }

        // ========= STATISTIQUES =========
        private void OpenStats(object sender, RoutedEventArgs e)
        {
            TitleBlock.Text = "Statistiques du Pressing";
            ContentPanel.Children.Clear();

            TextBlock stats = new()
            {
                Foreground = Brushes.White,
                FontSize = 18
            };

            stats.Text =
                $"Total commandes : {_pressing.NombreTotalCommandes()}\n" +
                $"Récupérées : {_pressing.NombreCommandesRecuperees()}\n" +
                $"Non récupérées : {_pressing.NombreCommandesNonRecuperees()}\n\n" +
                $"Chiffre d'affaires total : {_pressing.ChiffreAffairesTotal()} FCFA\n" +
                $"Standar : {_pressing.ChiffreAffairesEco()} FCFA\n" +
                $"Express : {_pressing.ChiffreAffairesExpress()} FCFA\n\n" +
                $"Poids total traité : {_pressing.PoidsTotalTraite():0.00} Kg";

            ContentPanel.Children.Add(stats);
        }
    }
}

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace Pressing_Wpf
{
    public class PressingManager
    {
        private readonly string _cnx = "Data Source=pressing.db";

        public PressingManager()
        {
            InitialiserBase();
        }

        private void InitialiserBase()
        {
            using var cn = new SqliteConnection(_cnx);
            cn.Open();

            string sql = @"
            CREATE TABLE IF NOT EXISTS Commandes (
                Code TEXT PRIMARY KEY,
                NomClient TEXT NOT NULL,
                Telephone TEXT NOT NULL,
                PoidsKg REAL NOT NULL,
                NombreArticles INTEGER NOT NULL,
                TypeService INTEGER NOT NULL,
                Montant REAL NOT NULL,
                DateDepot TEXT NOT NULL,
                DateRetrait TEXT NOT NULL,
                EstRecuperee INTEGER NOT NULL
            );";

            using var cmd = new SqliteCommand(sql, cn);
            cmd.ExecuteNonQuery();
        }

        public Commande EnregistrerCommande(string nom, string tel, double poids, int nbArticles, ServiceType type)
        {
            var cmdObj = new Commande(nom, tel, poids, nbArticles, type);

            using var cn = new SqliteConnection(_cnx);
            cn.Open();

            string sql = @"
            INSERT INTO Commandes 
            (Code, NomClient, Telephone, PoidsKg, NombreArticles, TypeService, Montant, DateDepot, DateRetrait, EstRecuperee)
            VALUES (@c, @n, @t, @p, @a, @s, @m, @d, @r, @e)";

            using var cmd = new SqliteCommand(sql, cn);

            cmd.Parameters.AddWithValue("@c", cmdObj.Code);
            cmd.Parameters.AddWithValue("@n", cmdObj.NomClient);
            cmd.Parameters.AddWithValue("@t", cmdObj.Telephone);
            cmd.Parameters.AddWithValue("@p", cmdObj.PoidsKg);
            cmd.Parameters.AddWithValue("@a", cmdObj.NombreArticles);
            cmd.Parameters.AddWithValue("@s", (int)cmdObj.TypeService);
            cmd.Parameters.AddWithValue("@m", cmdObj.Montant);
            cmd.Parameters.AddWithValue("@d", cmdObj.DateDepot.ToString("o"));
            cmd.Parameters.AddWithValue("@r", cmdObj.DateRetrait.ToString("o"));
            cmd.Parameters.AddWithValue("@e", cmdObj.EstRecuperee ? 1 : 0);

            cmd.ExecuteNonQuery();

            return cmdObj;
        }

        private Commande Lire(SqliteDataReader r)
        {
            // Indices en fonction de la création de la table
            return new Commande(
                r.GetString(0),                  // Code
                r.GetString(1),                  // NomClient
                r.GetString(2),                  // Telephone
                r.GetDouble(3),                  // PoidsKg
                r.GetInt32(4),                   // NombreArticles
                (ServiceType)r.GetInt32(5),      // TypeService
                r.GetDouble(6),                  // Montant
                DateTime.Parse(r.GetString(7)),  // DateDepot
                DateTime.Parse(r.GetString(8)),  // DateRetrait
                r.GetInt32(9) == 1               // EstRecuperee
            );
        }

        public Commande RechercherParCode(string code)
        {
            using var cn = new SqliteConnection(_cnx);
            cn.Open();

            string sql = "SELECT * FROM Commandes WHERE Code=@c";

            using var cmd = new SqliteCommand(sql, cn);
            cmd.Parameters.AddWithValue("@c", code);

            using var r = cmd.ExecuteReader();
            return r.Read() ? Lire(r) : null;
        }

        public List<Commande> RechercherParNomOuTel(string info)
        {
            var liste = new List<Commande>();

            using var cn = new SqliteConnection(_cnx);
            cn.Open();

            string sql = "SELECT * FROM Commandes WHERE NomClient LIKE @i OR Telephone LIKE @i";

            using var cmd = new SqliteCommand(sql, cn);
            cmd.Parameters.AddWithValue("@i", "%" + info + "%");

            using var r = cmd.ExecuteReader();
            while (r.Read())
                liste.Add(Lire(r));

            return liste;
        }

        public bool RecupererCommande(string code)
        {
            using var cn = new SqliteConnection(_cnx);
            cn.Open();

            string sql = "UPDATE Commandes SET EstRecuperee=1 WHERE Code=@c";

            using var cmd = new SqliteCommand(sql, cn);
            cmd.Parameters.AddWithValue("@c", code);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool AnnulerCommande(string code)
        {
            using var cn = new SqliteConnection(_cnx);
            cn.Open();

            string sql = "DELETE FROM Commandes WHERE Code=@c";

            using var cmd = new SqliteCommand(sql, cn);
            cmd.Parameters.AddWithValue("@c", code);

            return cmd.ExecuteNonQuery() > 0;
        }

        // NOUVEAU : obtenir toutes les commandes (pour la liste)
        public List<Commande> ObtenirToutesCommandes()
        {
            var liste = new List<Commande>();

            using var cn = new SqliteConnection(_cnx);
            cn.Open();

            string sql = "SELECT * FROM Commandes ORDER BY DateDepot DESC";

            using var cmd = new SqliteCommand(sql, cn);
            using var r = cmd.ExecuteReader();

            while (r.Read())
                liste.Add(Lire(r));

            return liste;
        }

        // ---------- STATISTIQUES ----------
        private int GetInt(string sql)
        {
            using var cn = new SqliteConnection(_cnx);
            cn.Open();
            using var cmd = new SqliteCommand(sql, cn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private double GetDouble(string sql)
        {
            using var cn = new SqliteConnection(_cnx);
            cn.Open();
            using var cmd = new SqliteCommand(sql, cn);
            return Convert.ToDouble(cmd.ExecuteScalar());
        }

        public int NombreTotalCommandes() => GetInt("SELECT COUNT(*) FROM Commandes");
        public int NombreCommandesRecuperees() => GetInt("SELECT COUNT(*) FROM Commandes WHERE EstRecuperee=1");
        public int NombreCommandesNonRecuperees() => GetInt("SELECT COUNT(*) FROM Commandes WHERE EstRecuperee=0");

        public double ChiffreAffairesTotal() => GetDouble("SELECT IFNULL(SUM(Montant),0) FROM Commandes");
        public double ChiffreAffairesEco() => GetDouble("SELECT IFNULL(SUM(Montant),0) FROM Commandes WHERE TypeService=0");
        public double ChiffreAffairesExpress() => GetDouble("SELECT IFNULL(SUM(Montant),0) FROM Commandes WHERE TypeService=1");
        public double PoidsTotalTraite() => GetDouble("SELECT IFNULL(SUM(PoidsKg),0) FROM Commandes");
    }
}

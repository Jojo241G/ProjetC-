using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace Pressing_Wpf
{
    public static class TicketGenerator
    {
        static TicketGenerator()
        {
            Settings.License = LicenseType.Community;
        }

        public static void GenererTicket(Commande cmd)
        {
            string dossier = "Tickets";
            if (!Directory.Exists(dossier))
                Directory.CreateDirectory(dossier);

            string chemin = Path.Combine(dossier, $"Ticket_{cmd.Code}.pdf");

            var document = new TicketDocument(cmd);
            document.GeneratePdf(chemin);
        }
    }

    public class TicketDocument : IDocument
    {
        private readonly Commande _cmd;

        public TicketDocument(Commande cmd)
        {
            _cmd = cmd;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata { Title = "Ticket Pressing" };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A6);
                page.Margin(20);
                page.PageColor(Colors.White);

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("TICKET PRESSING")
                              .FontSize(18)
                              .Bold()
                              .AlignCenter();

                    col.Item().Text($"Code        : {_cmd.Code}");
                    col.Item().Text($"Client      : {_cmd.NomClient}");
                    col.Item().Text($"Téléphone   : {_cmd.Telephone}");
                    col.Item().Text($"Poids       : {_cmd.PoidsKg:0.00} Kg");
                    col.Item().Text($"Articles    : {_cmd.NombreArticles}");
                    col.Item().Text($"Service     : {_cmd.TypeService}");
                    col.Item().Text($"Montant     : {_cmd.Montant} FCFA");
                    col.Item().Text($"Déposé le   : {_cmd.DateDepot}");
                    col.Item().Text($"Retrait le  : {_cmd.DateRetrait}");

                    col.Item().PaddingTop(15)
                              .Text("Merci pour votre confiance.")
                              .Italic()
                              .AlignCenter();
                });
            });
        }
    }
}

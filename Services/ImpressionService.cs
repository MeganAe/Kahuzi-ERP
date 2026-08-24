using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using GestionCommerciale.Models;

namespace GestionCommerciale.Services;

public static class ImpressionService
{
    public static void ImprimerFacture(Vente vente)
    {
        var doc = CreerDocumentFacture(vente);
        var printDialog = new PrintDialog();

        if (printDialog.ShowDialog() == true)
        {
            IDocumentPaginatorSource paginator = doc;
            printDialog.PrintDocument(paginator.DocumentPaginator, $"Facture_{vente.Numero}");
        }
    }

    public static FlowDocument CreerDocumentFacture(Vente vente)
    {
        var doc = new FlowDocument
        {
            PagePadding = new Thickness(40),
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 13,
            ColumnWidth = double.PositiveInfinity
        };

        // En-tête
        var headerTable = new Table();
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(300) });
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

        var rowGroup = new TableRowGroup();
        var rowHeader = new TableRow();

        // Bloc entreprise
        var pEnt = new Paragraph();
        pEnt.Inlines.Add(new Bold(new Run("KAHUZI ERP\n")) { FontSize = 18, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F766E")) });
        pEnt.Inlines.Add(new Run("Gestion Commerciale d'Entreprise\n"));
        pEnt.Inlines.Add(new Run("Bukavu, Sud-Kivu, RDC\n"));
        pEnt.Inlines.Add(new Run("Contact : contact@kahuzierp.cd\n"));
        rowHeader.Cells.Add(new TableCell(pEnt));

        // Bloc Facture / Numéro
        var pFact = new Paragraph { TextAlignment = TextAlignment.Right };
        pFact.Inlines.Add(new Bold(new Run("FACTURE / REÇU\n")) { FontSize = 16 });
        pFact.Inlines.Add(new Run($"N° : {vente.Numero}\n"));
        pFact.Inlines.Add(new Run($"Date : {vente.DateVente:dd/MM/yyyy HH:mm}\n"));
        pFact.Inlines.Add(new Run($"Paiement : {vente.ModePaiement}\n"));
        pFact.Inlines.Add(new Run($"Statut : {vente.Statut}\n"));
        rowHeader.Cells.Add(new TableCell(pFact));

        rowGroup.Rows.Add(rowHeader);
        headerTable.RowGroups.Add(rowGroup);
        doc.Blocks.Add(headerTable);

        // Ligne de séparation
        doc.Blocks.Add(new BlockUIContainer(new Separator { Margin = new Thickness(0, 15, 0, 15), Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F766E")) }));

        // Client
        var pClient = new Paragraph();
        pClient.Inlines.Add(new Bold(new Run("Client : ")) { FontSize = 14 });
        pClient.Inlines.Add(new Run(vente.Client?.NomComplet ?? "Client Comptant / Occasionnel"));
        if (!string.IsNullOrWhiteSpace(vente.Client?.Telephone))
        {
            pClient.Inlines.Add(new Run($"  |  Tél : {vente.Client.Telephone}"));
        }
        if (!string.IsNullOrWhiteSpace(vente.Client?.Adresse))
        {
            pClient.Inlines.Add(new Run($"  |  Adresse : {vente.Client.Adresse}"));
        }
        doc.Blocks.Add(pClient);

        // Tableau des articles
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 15, 0, 15) };
        table.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // Article
        table.Columns.Add(new TableColumn { Width = new GridLength(80) }); // Qté
        table.Columns.Add(new TableColumn { Width = new GridLength(120) }); // Prix unitaire
        table.Columns.Add(new TableColumn { Width = new GridLength(130) }); // Total

        var tableRowGroup = new TableRowGroup();

        // En-têtes du tableau
        var thRow = new TableRow { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9")) };
        thRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Désignation / Article")))) { Padding = new Thickness(8, 6, 8, 6) });
        thRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Quantité"))) { TextAlignment = TextAlignment.Center }) { Padding = new Thickness(8, 6, 8, 6) });
        thRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Prix Unit."))) { TextAlignment = TextAlignment.Right }) { Padding = new Thickness(8, 6, 8, 6) });
        thRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Sous-Total"))) { TextAlignment = TextAlignment.Right }) { Padding = new Thickness(8, 6, 8, 6) });
        tableRowGroup.Rows.Add(thRow);

        // Lignes
        if (vente.Lignes != null)
        {
            foreach (var ligne in vente.Lignes)
            {
                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(ligne.NomProduit))) { Padding = new Thickness(8, 6, 8, 6) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(ligne.Quantite.ToString()))) { Padding = new Thickness(8, 6, 8, 6), TextAlignment = TextAlignment.Center });
                row.Cells.Add(new TableCell(new Paragraph(new Run($"{ligne.PrixUnitaire:N0} FC"))) { Padding = new Thickness(8, 6, 8, 6), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run($"{ligne.SousTotal:N0} FC"))) { Padding = new Thickness(8, 6, 8, 6), TextAlignment = TextAlignment.Right });
                tableRowGroup.Rows.Add(row);
            }
        }

        table.RowGroups.Add(tableRowGroup);
        doc.Blocks.Add(table);

        // Total
        var pTotal = new Paragraph { TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 10, 0, 20) };
        pTotal.Inlines.Add(new Bold(new Run($"TOTAL À PAYER : {vente.Total:N0} FC\n")) { FontSize = 16, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F766E")) });
        doc.Blocks.Add(pTotal);

        // Pied de page
        var pFooter = new Paragraph { TextAlignment = TextAlignment.Center, FontStyle = FontStyles.Italic, Margin = new Thickness(0, 30, 0, 0) };
        pFooter.Inlines.Add(new Run("Merci de votre confiance. — Document généré par Kahuzi ERP (Bukavu)"));
        doc.Blocks.Add(pFooter);

        return doc;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
using System.Diagnostics.Eventing.Reader;
using static System.Formats.Asn1.AsnWriter;

namespace CompilationMiniJeux
{
    internal class MiniJeuEcrireVite
    {
        public static readonly string[] TABMOTSPOSSIBLES = { "haribo", "jaaj" };
        public static readonly int DUREEMINIJEU = 5;
        public static readonly int NBBONBONSGAGNES = 10;

        public string motATaper;

        private Label labMotADeviner, labelAfficherTemps;
        private TextBox saisie;
        private Random random = new Random();

        private MainWindow mainWindow;

        public MiniJeuEcrireVite(MainWindow fenetreParent)
        {
            mainWindow = fenetreParent;
        }

        public void InitMiniJeuEcrireVite()
        {
            mainWindow.timer.Stop();
            mainWindow.chronometre.Stop();

            mainWindow.CanvasPrincipal.Children.Clear();

            mainWindow.tempsRestant = DUREEMINIJEU;

            labMotADeviner = new Label
            {
                Name = "labMotADeviner",
                Content = "Mot à taper",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(labMotADeviner, 175);
            Canvas.SetTop(labMotADeviner, 143);

            Console.WriteLine(TABMOTSPOSSIBLES.Length);
            motATaper = TABMOTSPOSSIBLES[random.Next(TABMOTSPOSSIBLES.Length)];
            labMotADeviner.Content = motATaper;

            mainWindow.CanvasPrincipal.Children.Add(labMotADeviner);

            // Création du TextBox
            saisie = new TextBox
            {
                Name = "saisie",
                Width = 450,
                FontFamily = new System.Windows.Media.FontFamily("Comic Sans MS"),
                FontSize = 72,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Canvas.SetLeft(saisie, 175);
            Canvas.SetTop(saisie, 217);

            // Ajout de l'événement TextChanged
            saisie.TextChanged += JouerMiniJeuEcrireVite;

            // Ajout du TextBox au Canvas
            mainWindow.CanvasPrincipal.Children.Add(saisie);

            labelAfficherTemps = new Label
            {
                Content = mainWindow.tempsRestant,
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(labMotADeviner, 175);
            Canvas.SetTop(labMotADeviner, 143);

            labMotADeviner.Content = motATaper;

            mainWindow.CanvasPrincipal.Children.Add(labelAfficherTemps);

            mainWindow.timer.Start();
            mainWindow.chronometre.Start();
        }

        public void VerifierTemps()
        {
            if (mainWindow.tempsRestant <= 0)
                mainWindow.ecranIntermediaire.InitEcranIntermediaire(0, false);

            labelAfficherTemps.Content = mainWindow.tempsRestant;
        }

        public void JouerMiniJeuEcrireVite(object sender, TextChangedEventArgs e)
        {
            bool ok = true;

            if (saisie.Text.Length == motATaper.Length)
            {
                for (int i = 0; i < motATaper.Length; i++)
                {
                    if (motATaper[i] != saisie.Text[i])
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                    mainWindow.ecranIntermediaire.InitEcranIntermediaire(NBBONBONSGAGNES, true);
            }
            else
            {
                for (int i = 0; i < saisie.Text.Length; i++)
                {
                    if (motATaper[i] != saisie.Text[i])
                    {
                        mainWindow.ecranIntermediaire.InitEcranIntermediaire(0, false);
                    }
                }
            }
        }
    }
}

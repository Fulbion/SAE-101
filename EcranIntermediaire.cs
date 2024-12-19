using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace CompilationMiniJeux
{
    public class EcranIntermediaire
    {
        // Tableau de messages d'encouragement pour les victoires
        private static readonly string[] MESSAGESENCOURAGEMENT = { "On lache rien", "Courage !", "Victoire !", "Encore un mini-jeu ?", "On ne t’arrête pas !", "Au top !", "Bien joué !", "Bravo !", "Aller, on continue !", "Une victoire de légende !", "Même pas peur !", "C'est ça du talent !", "Respect !", "Inarrêtable !" };

        // Tableau de messages de défaite pour les échecs
        private static readonly string[] MESSAGESDEFAITE = { "On y retourne ?\n Tu peux le faire !", "Dommage", "Accroche-toi,\ntu vas y arriver !", "On ne lâche rien !", "On ne baisse pas les bras !", "C'est raté...", "Presque ! Dommage..." };

        // Délai avant de commencer un nouveau mini-jeu
        private static readonly int DUREEAVANTCOMMENCERNOUVEAUMINIJEU = 2;

        // Paramètres de temps pour le décrément de vie
        private static readonly int ATTENTEMAXAVANTDECREMENTER = 40, ATTENTEMAXENTREDECREMENTATIONS = 1;

        // Tailles des différents éléments de l'interface
        private static readonly int TAILLEMESSAGEPRINCIPAL = 50, TAILLEMESSAGEACCELERATION = 35, TAILLEMESSAGEVIE = 35, TAILLEBONBON = 70, TAILLECOEURS = 35, TAILLECOMPTEURBONBONS = 30;

        // Position Y du message principal
        private static readonly int YPOSMESSAGEPRINCIPAL = 100;

        // Décalages pour le placement des éléments
        private static readonly int DECALAGEXVIELABEL = 50, DECALAGEXBONBON = 40, DECALAGEYBONBON = 40, DECALAGEYVIELABEL = 30, DECALAGEYCOEURS = 45, DELECAGEYCALCULLABEL = 45;
        private static readonly int ECARTEMENTENTRECOEURS = 50;

        private MainWindow mainWindow;

        private Random random = new Random();

        // Labels pour afficher les messages et les informations
        private Label calculLabel, labelDecompte;
        private Label labelMessagePrincipal, messageAccelerationJeu, vieLabel;

        // Images pour les éléments visuels
        private static BitmapImage imageCoeur;
        private static BitmapImage imageBonbon;

        // Variables de synchronisation pour le décrément de vie
        private int attenteAvantDecrementer, attenteentredecrementations;

        public EcranIntermediaire(MainWindow fenetreParent)
        {
            mainWindow = fenetreParent;
            InitImages();
        }

        // Initialise les images des bonbons et des cœurs
        public static void InitImages()
        {
            imageBonbon = new BitmapImage(new Uri($"pack://application:,,,/img/EcranIntermediaire/bonbonecraninter.png"));
            imageCoeur = new BitmapImage(new Uri($"pack://application:,,,/img/EcranIntermediaire/coeur.png"));
        }

        // Initialise l'écran intermédiaire avec les informations de jeu
        public void InitEcranIntermediaire(int bonbonsGagnes, bool gagne)
        {
            double vieLabelX;

            mainWindow.CanvasPrincipal.Children.Clear();

            mainWindow.timer.Stop();
            mainWindow.chronometre.Stop();

            // Réinitialise les paramètres
            mainWindow.tempsRestant = DUREEAVANTCOMMENCERNOUVEAUMINIJEU;
            mainWindow.nbBonbonsGagnes = bonbonsGagnes;

            attenteAvantDecrementer = ATTENTEMAXAVANTDECREMENTER;
            attenteentredecrementations = ATTENTEMAXENTREDECREMENTATIONS;

            // Affiche le message principal
            MessagePrincipal(gagne);

            // Affiche le message d'accélération du jeu
            MessageAccelerationJeu();

            // Affiche la vie du joueur
            AfficherVie(out vieLabelX);

            // Affiche le compteur de bonbons
            AfficherCompteurBonbons(vieLabelX);

            // Redémarre le timer principal
            mainWindow.timer.Start();
        }

        // Affiche le message principal en fonction du résultat de la partie
        private void MessagePrincipal(bool gagne)
        {
            if (!gagne)
            {
                // Diminue les points de vie si le joueur a perdu
                mainWindow.nbPV--;
                labelMessagePrincipal = new Label
                {
                    FontSize = TAILLEMESSAGEPRINCIPAL,
                    FontFamily = mainWindow.policeEcriturePersonalisee,
                    Content = MESSAGESDEFAITE[random.Next(0, MESSAGESDEFAITE.Length)]
                };
            }
            else
            {
                // Crée un label coloré pour les victoires
                labelMessagePrincipal = CreerLabelCouleurArcEnCiel(MESSAGESENCOURAGEMENT[random.Next(0, MESSAGESENCOURAGEMENT.Length)]);
            }

            // Ajoute le label au Canvas principal
            mainWindow.CanvasPrincipal.Children.Add(labelMessagePrincipal);
            mainWindow.CanvasPrincipal.UpdateLayout(); // Forcer le Canvas à se mettre à jour pour que le Label ait une largeur calculée

            // Centre le message principal horizontalement
            Canvas.SetLeft(labelMessagePrincipal, (mainWindow.CanvasPrincipal.ActualWidth - labelMessagePrincipal.ActualWidth) / 2);
            // Positionne le message à une hauteur fixe
            Canvas.SetTop(labelMessagePrincipal, YPOSMESSAGEPRINCIPAL);
        }

        private void MessageAccelerationJeu()
        {
            messageAccelerationJeu = new Label { FontSize = TAILLEMESSAGEACCELERATION, FontFamily = mainWindow.policeEcriturePersonalisee };

            // Vérifie si l'indice actuel est proche de la fin des mini-jeux
            if (mainWindow.i > mainWindow.indicesMiniJeux.Length - 2)
                messageAccelerationJeu.Content = "Attention, le jeu s'accèlere !";

            mainWindow.CanvasPrincipal.Children.Add(messageAccelerationJeu);

            // Met à jour le layout pour s'assurer que les modifications sont prises en compte
            mainWindow.CanvasPrincipal.UpdateLayout();

            Canvas.SetLeft(messageAccelerationJeu, (mainWindow.CanvasPrincipal.ActualWidth - messageAccelerationJeu.ActualWidth) / 2);
            Canvas.SetTop(messageAccelerationJeu, Canvas.GetTop(labelMessagePrincipal) + labelMessagePrincipal.ActualHeight);
        }

        private void AfficherVie(out double vieLabelX)
        {
            vieLabel = new Label { FontSize = TAILLEMESSAGEVIE, FontFamily = mainWindow.policeEcriturePersonalisee, Content = "VIE :" };

            mainWindow.CanvasPrincipal.Children.Add(vieLabel);

            // Met à jour le layout pour recalculer les positions des éléments
            mainWindow.CanvasPrincipal.UpdateLayout();

            vieLabelX = (mainWindow.Width / 2) - vieLabel.ActualWidth - DECALAGEXVIELABEL;
            Canvas.SetLeft(vieLabel, vieLabelX);
            Canvas.SetTop(vieLabel, Canvas.GetTop(messageAccelerationJeu) + messageAccelerationJeu.ActualHeight + DECALAGEYVIELABEL);

            // Boucle pour afficher les coeurs en fonction du nombre de points de vie (nbPV)
            for (int i = 0; i < mainWindow.nbPV; i++)
            {
                // Crée une image de coeur
                Image coeur = new Image { Width = TAILLECOEURS, Height = TAILLECOEURS, Source = imageCoeur };

                // Ajoute l'image du coeur au Canvas principal
                mainWindow.CanvasPrincipal.Children.Add(coeur);

                // Positionne chaque coeur en fonction de l'espacement défini
                Canvas.SetLeft(coeur, vieLabelX + vieLabel.ActualWidth + ECARTEMENTENTRECOEURS * i);
                Canvas.SetTop(coeur, Canvas.GetTop(messageAccelerationJeu) + messageAccelerationJeu.ActualHeight + DECALAGEYCOEURS);
            }
        }

        private void AfficherCompteurBonbons(double vieLabelX)
        {
            Image bonbon = new Image
            {
                Width = TAILLEBONBON,
                Height = TAILLEBONBON,
                Source = imageBonbon
            };

            mainWindow.CanvasPrincipal.Children.Add(bonbon);

            // Positionne le bonbon sous le label de vie
            Canvas.SetLeft(bonbon, vieLabelX + (vieLabel.ActualWidth / 2) - DECALAGEXBONBON);
            Canvas.SetTop(bonbon, Canvas.GetTop(vieLabel) + vieLabel.ActualHeight + DECALAGEYBONBON);

            calculLabel = new Label
            {
                FontSize = TAILLECOMPTEURBONBONS,
                FontFamily = mainWindow.policeEcriturePersonalisee,
                Content = $": {mainWindow.nbBonbons} + {mainWindow.nbBonbonsGagnes}"
            };

            mainWindow.CanvasPrincipal.Children.Add(calculLabel);

            // Met à jour le layout pour recalculer les positions
            mainWindow.CanvasPrincipal.UpdateLayout();

            // Positionne le label du compteur de bonbons à côté du bonbon
            Canvas.SetLeft(calculLabel, Canvas.GetLeft(bonbon) + (calculLabel.ActualWidth / 2));
            Canvas.SetTop(calculLabel, Canvas.GetTop(vieLabel) + vieLabel.ActualHeight + DELECAGEYCALCULLABEL);
        }

        private Label CreerLabelCouleurArcEnCiel(string texte)
        {
            // Créer un tableau de couleurs pour l'effet arc-en-ciel
            Brush[] couleursArcEnCiel = new Brush[] { Brushes.Red, Brushes.Orange, new SolidColorBrush(Color.FromRgb(255, 215, 0)), Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Violet };

            // Initialise un TextBlock pour contenir le texte avec des styles personnalisés
            TextBlock blocDeTexte = new TextBlock { FontSize = TAILLEMESSAGEPRINCIPAL, FontWeight = FontWeights.Bold, FontFamily = mainWindow.policeEcriturePersonalisee };

            // Ajoute un effet d'ombre au texte
            blocDeTexte.Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 0, ShadowDepth = 0, Opacity = 1 };

            // Applique les couleurs arc-en-ciel à chaque caractère du texte
            for (int i = 0; i < texte.Length; i++)
            {
                Run run = new Run(texte[i].ToString()) { Foreground = couleursArcEnCiel[i % couleursArcEnCiel.Length] };
                blocDeTexte.Inlines.Add(run);
            }

            return new Label { Content = blocDeTexte, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center };
        }


        public void ResterSurEcranIntermediaire()
        {
            if (attenteAvantDecrementer > 0)
                attenteAvantDecrementer--;

            // vérifie si le nombre de bonbons gagnés est supérieur à zéro
            else if (mainWindow.nbBonbonsGagnes > 0)
            {
                // Si le délai entre les décrémentations est supérieur à zéro, le décrémente
                if (attenteentredecrementations > 0)
                    attenteentredecrementations--;
                else
                {
                    // Réinitialise le délai entre les décrémentations
                    attenteentredecrementations = ATTENTEMAXENTREDECREMENTATIONS;

                    // Décrémente le nombre de bonbons gagnés et incrémente le nombre de bonbons
                    mainWindow.nbBonbonsGagnes--;
                    mainWindow.nbBonbons++;

                    calculLabel.Content = $": {mainWindow.nbBonbons} + {mainWindow.nbBonbonsGagnes}";
                }
            }

            // Si aucun bonbon n'est gagné, gère la fin du mini-jeu
            else
                mainWindow.GererFinMiniJeu();
        }
    }
}
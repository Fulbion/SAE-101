using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Media;

namespace CompilationMiniJeux
{
    class MiniJeuTapette : IMiniJeu
    {
        // Constantes de vitesse et de gravité
        private static readonly double VITESSEMAX = 5;
        private static readonly double VITESSEBONBONVOLANTINITIALISATION = 2, VITESSECHUTEMIN = 0.25, VITESSECHUTEMAX = 20, COEFGRAVITE = 1.1;
        private static readonly double OPACITEIMPACT = 0.5;

        // Constantes de configuration du jeu
        public static readonly int NOMBREBONBONSVOLANTSINITIALISEES = 10, MARGEENDEHORSECRAN = 220, AUDESSUS = 10, ENDESSOUS = -10, DUREEMINIJEU = 20;
        private static readonly int DECALAGEYBOITECOLLISIONTAPETTE = 30;
        private static readonly int MAXCOMPTEURRAFRAICHIRBONBONS = 3;
        private static readonly int TAILLETAPETTE = 60;
        private static readonly int TAILLEMOUCHE = 60;
        private static readonly int TAILLEIMPACT = 50;
        private static readonly int TAILLEAFFICHAGECHRONO = 35;
        private static readonly int POSYLABELAFFICHECHRONO = 0;

        // Listes pour suivre les bonbons volants et leurs propriétés
        private List<Image> bonbonsVolants = new List<Image>();
        private List<double[]> vitesseBonbonsVolants = new List<double[]>();
        private List<bool> bonbonsVolantsEcrases = new List<bool>();
        private List<int> indexsImagesBonbonVolant = new List<int>();

        // Tableaux d'images pour les animations des bonbons volants
        private BitmapImage[] animationsBonbonVolant = new BitmapImage[4];

        private Random random = new Random();
        private DispatcherTimer timer, chronometre;

        // Images pour la tapette et l'impact visuel
        private Image tapette, impact;
        private BitmapImage imageTapette, imageImpact;

        // Label pour afficher le chronomètre
        private Label label;
        private MainWindow mainWindow;

        // Variables pour suivre l'état du jeu
        private int nombreBonbonsVolantsEnVie;
        private int indexImageBonbonVolant;
        private int compteurRafraichirImageBonbons;

        public MiniJeuTapette(MainWindow parentWindow)
        {
            mainWindow = parentWindow;
            InitImages();
        }

        // Méthode pour initialiser le jeu de tapette
        public void Init()
        {
            // Arrête les timers existants
            mainWindow.timer.Stop();
            mainWindow.chronometre.Stop();

            // Nettoie le canvas et réinitialise les listes
            mainWindow.CanvasPrincipal.Children.Clear();
            bonbonsVolants.Clear();
            vitesseBonbonsVolants.Clear();
            indexsImagesBonbonVolant.Clear();

            // Initialise le temps de jeu et le nombre de bonbons
            mainWindow.tempsRestant = DUREEMINIJEU;
            nombreBonbonsVolantsEnVie = NOMBREBONBONSVOLANTSINITIALISEES;

            // Fait apparaître les bonbons volants
            FaireApparaitreBonbonsVolants();

            indexImageBonbonVolant = 0;
            compteurRafraichirImageBonbons = 0;

            // Crée et configure le label du chronomètre
            label = new Label { Content = DUREEMINIJEU, FontSize = TAILLEAFFICHAGECHRONO, FontFamily = mainWindow.policeEcriturePersonalisee };
            Canvas.SetLeft(label, mainWindow.Width / 2 - label.ActualWidth / 2);
            Canvas.SetTop(label, POSYLABELAFFICHECHRONO);

            // Initialise l'image de la tapette
            tapette = new Image
            {
                Source = imageTapette,
                Width = TAILLETAPETTE,
                Height = TAILLETAPETTE
            };

            Panel.SetZIndex(tapette, AUDESSUS);

            // Ajoute les éléments au canvas principal
            mainWindow.CanvasPrincipal.Children.Add(tapette);
            mainWindow.CanvasPrincipal.Children.Add(label);

            // Démarre les timers du jeu
            mainWindow.timer.Start();
            mainWindow.chronometre.Start();
        }

        // Méthode pour initialiser les images du jeu
        private void InitImages()
        {
            // Charge les images pour l'animation des bonbons volants
            for (int i = 0; i < animationsBonbonVolant.Length; i++)
                animationsBonbonVolant[i] = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuTapette/bonbonvolant{i}.png"));

            // Charge les images pour la tapette et l'impact
            imageTapette = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuTapette/tapette.png"));
            imageImpact = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuTapette/imageimpact.png"));
        }
        private void FaireApparaitreBonbonsVolants()
        {
            double coordXApparition, coordYApparition;

            // Boucle pour initialiser et ajouter le nombre de bonbons volants requis
            for (int i = 0; i < NOMBREBONBONSVOLANTSINITIALISEES; i++)
            {
                // Création d'une image pour le bonbon volant
                var mouche = new Image
                {
                    Width = TAILLEMOUCHE,
                    Height = TAILLEMOUCHE,
                };

                // Ajouter le bonbon volant aux listes correspondantes
                bonbonsVolants.Add(mouche);
                vitesseBonbonsVolants.Add(new double[3] { VITESSEBONBONVOLANTINITIALISATION, VITESSEBONBONVOLANTINITIALISATION, VITESSECHUTEMIN });
                bonbonsVolantsEcrases.Add(false);
                indexsImagesBonbonVolant.Add(0);

                // Détermination des coordonnées d'apparition du bonbon volant
                if (random.Next(0, 2) == 0)
                {
                    coordXApparition = random.Next(0, (int)(mainWindow.CanvasPrincipal.ActualWidth - mouche.Width));
                    coordYApparition = new int[2] { random.Next(-MARGEENDEHORSECRAN, -(int)mouche.Height), random.Next((int)mainWindow.CanvasPrincipal.ActualHeight, (int)mainWindow.CanvasPrincipal.ActualHeight + MARGEENDEHORSECRAN) }[random.Next(0, 2)];
                }
                else
                {
                    coordXApparition = new int[2] { random.Next(-MARGEENDEHORSECRAN, -(int)mouche.Width), random.Next((int)mainWindow.CanvasPrincipal.ActualWidth, (int)mainWindow.CanvasPrincipal.ActualWidth + MARGEENDEHORSECRAN) }[random.Next(0, 2)];
                    coordYApparition = random.Next(0, (int)(mainWindow.CanvasPrincipal.ActualHeight - mouche.Height));
                }

                Canvas.SetLeft(mouche, coordXApparition);
                Canvas.SetTop(mouche, coordYApparition);

                bonbonsVolants[i].Source = animationsBonbonVolant[indexsImagesBonbonVolant[0]];

                mainWindow.CanvasPrincipal.Children.Add(mouche);
            }
        }

        public void Jouer()
        {
            bool toutesLesMouchesEnDehorsEcran;

            label.Content = mainWindow.tempsRestant;

            BougerTapette();
            MouvementsBonbonsVolants();

            // Rafraîchir l'image des bonbons à intervalles réguliers
            compteurRafraichirImageBonbons = (compteurRafraichirImageBonbons + 1) % MAXCOMPTEURRAFRAICHIRBONBONS;

            // Vérification de la fin du mini-jeu
            if (nombreBonbonsVolantsEnVie <= 1)
            {
                toutesLesMouchesEnDehorsEcran = true;

                // Vérifier si toutes les mouches sont en dehors de l'écran
                for (int i = 0; i < bonbonsVolants.Count; i++)
                {
                    if (!EstEnDehorsEcran(bonbonsVolants[i]))
                    {
                        toutesLesMouchesEnDehorsEcran = false;
                        break;
                    }
                }

                // Si toutes les mouches sont hors de l'écran, afficher l'écran intermédiaire
                if (toutesLesMouchesEnDehorsEcran)
                    mainWindow.ecranIntermediaire.InitEcranIntermediaire(NOMBREBONBONSVOLANTSINITIALISEES, true);
            }
            else if (mainWindow.tempsRestant <= 0)
            {
                // Temps écoulé, afficher aussi l'écran intermédiaire
                mainWindow.ecranIntermediaire.InitEcranIntermediaire(0, false);
            }
        }

        private void BougerTapette()
        {
            // Déplacer la tapette à la position de la souris
            Point positionMouse = Mouse.GetPosition(null);
            Canvas.SetLeft(tapette, positionMouse.X - tapette.ActualWidth);
            Canvas.SetTop(tapette, positionMouse.Y - tapette.ActualHeight);
        }

        private void MouvementsBonbonsVolants()
        {
            for (int i = 0; i < bonbonsVolants.Count; i++)
            {
                if (bonbonsVolantsEcrases[i])
                {
                    // Gérer l'état d'un bonbon volant écrasé
                    EtatBonbonVolantEcrase(i);
                }
                else
                {
                    // Mettre à jour l'image du bonbon volant si le compteur de rafraîchissement est à zéro
                    if (compteurRafraichirImageBonbons <= 0)
                    {
                        bonbonsVolants[i].Source = animationsBonbonVolant[indexsImagesBonbonVolant[i]];
                        indexsImagesBonbonVolant[i] = (indexsImagesBonbonVolant[i] + 1) % animationsBonbonVolant.Length;
                    }

                    // Gérer le cas où le bonbon est en dehors de l'écran
                    if (EstEnDehorsEcran(bonbonsVolants[i]))
                        EtatBonbonVolantHorsEcran(i);
                    else
                        EtatBonbonVolantEnVieEtDansEcran(i);
                }
            }
        }

        private void EtatBonbonVolantEcrase(int IDMouche)
        {
            double y = Canvas.GetTop(bonbonsVolants[IDMouche]);

            // Si le bonbon volant est en dehors de l'écran, le supprimer des listes
            if (EstEnDehorsEcran(bonbonsVolants[IDMouche]))
            {
                mainWindow.CanvasPrincipal.Children.Remove(bonbonsVolants[IDMouche]);
                indexsImagesBonbonVolant.RemoveAt(bonbonsVolants.IndexOf(bonbonsVolants[IDMouche]));
                vitesseBonbonsVolants.RemoveAt(bonbonsVolants.IndexOf(bonbonsVolants[IDMouche]));
                bonbonsVolantsEcrases.RemoveAt(bonbonsVolants.IndexOf(bonbonsVolants[IDMouche]));
                bonbonsVolants.RemoveAt(bonbonsVolants.IndexOf(bonbonsVolants[IDMouche]));
            }
            else
            {
                // Continuer la chute du bonbon volant écrasé
                bonbonsVolants[IDMouche].Source = animationsBonbonVolant[0];
                y += vitesseBonbonsVolants[IDMouche][2];
                vitesseBonbonsVolants[IDMouche][2] = Math.Min(VITESSECHUTEMAX, vitesseBonbonsVolants[IDMouche][2] * COEFGRAVITE);
                Canvas.SetTop(bonbonsVolants[IDMouche], y);
            }
        }


        private void EtatBonbonVolantHorsEcran(int IDMouche)
        {
            double normeVecteur, dx, dy, x = Canvas.GetLeft(bonbonsVolants[IDMouche]), y = Canvas.GetTop(bonbonsVolants[IDMouche]);

            // Calculer un vecteur directionnel vers le centre du Canvas
            dx = (mainWindow.CanvasPrincipal.ActualWidth / 2) - x;
            dy = (mainWindow.CanvasPrincipal.ActualHeight / 2) - y;
            normeVecteur = Math.Sqrt(dx * dx + dy * dy);

            //normaliser le vecteur et appliquer la vitesse max
            x += (dx / normeVecteur) * VITESSEMAX;
            y += (dy / normeVecteur) * VITESSEMAX;

            // Appliquer la position de la mouche
            Canvas.SetLeft(bonbonsVolants[IDMouche], x);
            Canvas.SetTop(bonbonsVolants[IDMouche], y);
        }

        private void EtatBonbonVolantEnVieEtDansEcran(int IDMouche)
        {
            double normeVecteur, x = Canvas.GetLeft(bonbonsVolants[IDMouche]), y = Canvas.GetTop(bonbonsVolants[IDMouche]);

            // Mise à jour de la position
            x += vitesseBonbonsVolants[IDMouche][0];
            y += vitesseBonbonsVolants[IDMouche][1];

            // Variation aléatoire de la vitesse entre -1 et 1
            vitesseBonbonsVolants[IDMouche][0] += random.NextDouble() * 2 - 1;
            vitesseBonbonsVolants[IDMouche][1] += random.NextDouble() * 2 - 1;

            // empecher la mouche d'aller plus vite que sa vitesse max
            normeVecteur = Math.Sqrt(vitesseBonbonsVolants[IDMouche][0] * vitesseBonbonsVolants[IDMouche][0] + vitesseBonbonsVolants[IDMouche][1] * vitesseBonbonsVolants[IDMouche][1]);
            if (normeVecteur > VITESSEMAX)
            {
                vitesseBonbonsVolants[IDMouche][0] = (vitesseBonbonsVolants[IDMouche][0] / normeVecteur) * VITESSEMAX;
                vitesseBonbonsVolants[IDMouche][1] = (vitesseBonbonsVolants[IDMouche][1] / normeVecteur) * VITESSEMAX;
            }

            // Rebonds sur les bords du Canvas
            if (x < 0 || x > mainWindow.CanvasPrincipal.ActualWidth - bonbonsVolants[IDMouche].Width)
            {
                vitesseBonbonsVolants[IDMouche][0] = -vitesseBonbonsVolants[IDMouche][0];
                x = Math.Clamp(x, 0, Math.Abs(mainWindow.CanvasPrincipal.ActualWidth - bonbonsVolants[IDMouche].Width)); // Force la position en x de la mouche à rester entre le bord gauche et le bord droit du canvas
            }
            if (y < 0 || y > mainWindow.CanvasPrincipal.ActualHeight - bonbonsVolants[IDMouche].Height)
            {
                vitesseBonbonsVolants[IDMouche][1] = -vitesseBonbonsVolants[IDMouche][1];
                y = Math.Clamp(y, 0, mainWindow.CanvasPrincipal.ActualHeight - bonbonsVolants[IDMouche].Height);
            }

            Canvas.SetLeft(bonbonsVolants[IDMouche], x);
            Canvas.SetTop(bonbonsVolants[IDMouche], y);
        }

        // Méthode pour détecter une collision entre deux images
        private bool Collision(Image mouche, Image tapette)
        {
            // Vérifie si les bords des images se chevauchent horizontalement et verticalement
            return Canvas.GetLeft(mouche) < Canvas.GetLeft(tapette) + tapette.Width &&
                   Canvas.GetLeft(mouche) + mouche.Width > Canvas.GetLeft(tapette) &&
                   Canvas.GetTop(mouche) < Canvas.GetTop(tapette) + tapette.Height - DECALAGEYBOITECOLLISIONTAPETTE &&
                   Canvas.GetTop(mouche) + mouche.Height > Canvas.GetTop(tapette) - DECALAGEYBOITECOLLISIONTAPETTE;
        }

        // Méthode pour vérifier si une image (sprite) est en dehors des limites de l'écran
        private bool EstEnDehorsEcran(Image sprite)
        {
            double gauche = Canvas.GetLeft(sprite), haut = Canvas.GetTop(sprite), droite = gauche + sprite.Width, bas = haut + sprite.Height;

            return (droite < 0 || gauche > mainWindow.CanvasPrincipal.ActualWidth || bas < 0 || haut > mainWindow.CanvasPrincipal.ActualHeight);
        }

        // Méthode pour créer un impact visuel à une position spécifique (x, y)
        private void CreerImpact(double x, double y)
        {
            // Initialise une nouvelle image d'impact
            impact = new Image
            {
                Source = imageImpact,
                Width = TAILLEIMPACT,
                Height = TAILLEIMPACT,
                Opacity = OPACITEIMPACT
            };

            // Centre l'impact autour des coordonnées
            Canvas.SetLeft(impact, x - impact.Width / 2);
            Canvas.SetTop(impact, y - impact.Height / 2);

            Panel.SetZIndex(impact, ENDESSOUS);

            // Applique une rotation aléatoire à l'impact
            impact.RenderTransform = new RotateTransform(random.Next(0, 360), impact.Width / 2, impact.Height / 2);

            mainWindow.CanvasPrincipal.Children.Add(impact);
        }

        // Méthode appelée lors du relâchement du bouton de la souris sur la tapette
        public void MouseUpTapette(object sender, MouseButtonEventArgs e)
        {
            // Parcourt la liste des bonbons volants pour vérifier les collisions
            for (int i = 0; i < bonbonsVolants.Count; i++)
            {
                if (Mouse.LeftButton == MouseButtonState.Released) {


                    if (Collision(bonbonsVolants[i], tapette) && !bonbonsVolantsEcrases[i])
                    {

                        // Marque le bonbon comme écrasé
                        bonbonsVolantsEcrases[i] = true;
                        nombreBonbonsVolantsEnVie--;

                        CreerImpact(Canvas.GetLeft(bonbonsVolants[i]) + bonbonsVolants[i].Width / 2, Canvas.GetTop(bonbonsVolants[i]) + bonbonsVolants[i].Height / 2);
                    }
                }
            }
        }

        public void OnKeyDown(object sender, KeyEventArgs e) { }
        public void OnKeyUp(object sender, KeyEventArgs e) { }
    }
}
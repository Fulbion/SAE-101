using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CompilationMiniJeux
{
    public class MiniJeuBonbon : IMiniJeu
    {
        // Constantes pour les vitesses du joueur et des objets
        private static readonly double VITESSEMAXJOUEUR = 10, MINVITESSEJOUEUR = 1;
        private static readonly double VITESSECHUTEOBJETS = 5;
        private static readonly double COEFACCELERATION = 1.2;

        // Constantes pour la durée du jeu et les positions
        private static readonly int DUREEMINIJEU = 10;
        private static readonly int POSITIONYLAPLUSELOIGNE = -400;
        private static readonly int AUDESSUS = 10;
        private static readonly int TAILLECARTON = 175;
        private static readonly int TAILLELABELAFFICHECHRONO = 35;
        private static readonly int POSYLABELAFFICHECHRONO = 0;
        private static readonly int POSXCARTON = 0;
        private static readonly int TAILLEOBJETSTOMBANTS = 75;
        private static readonly int NOMBREBONBONSECRAN = 8;
        private static readonly int NOMBREPIQUESECRAN = 4;
        public static readonly double MINCOEFACCELERATIONCHUTE = 1;

        // Variables pour le mouvement du joueur
        private bool droite, gauche;

        // Listes pour les bonbons et les piques
        private List<Image> bonbons = new List<Image>();
        private List<Image> piques = new List<Image>();

        // Label pour afficher le chrono
        private Label labelAfficheChrono;
        private MainWindow mainWindow;

        // Vitesse initiale du joueur et score
        private double vitesseJoueur = 1;
        private int score;

        // Coefficient d'accélération de la chute des objets
        public double coefAccelerationChute = MINCOEFACCELERATIONCHUTE;

        // Images pour les objets et le carton
        private BitmapImage imageBonbon, imagePique, imageCarton;
        private Image carton;

        // Constructeur du jeu
        public MiniJeuBonbon(MainWindow parentWindow)
        {
            mainWindow = parentWindow;
            InitImages();
        }

        // Initialise les images des bonbons, piques et du carton
        private void InitImages()
        {
            imageBonbon = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuBonbons/bonbon_parachute.png"));
            imagePique = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuBonbons/boule_piques_parachute.png"));
            imageCarton = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuBonbons/carton.png"));
        }

        // Initialise la pluie de bonbons et l'état du jeu
        public void Init()
        {
            // Arrête les chronomètres avant de redémarrer
            mainWindow.chronometre.Stop();
            mainWindow.timer.Stop();

            gauche = false;
            droite = false;

            // Efface le canvas et réinitialise les listes
            mainWindow.CanvasPrincipal.Children.Clear();
            bonbons.Clear();
            piques.Clear();

            // Réinitialise le score et le temps restant
            score = 0;
            mainWindow.tempsRestant = DUREEMINIJEU;

            // Crée l'image du carton
            carton = new Image { Width = TAILLECARTON, Height = TAILLECARTON, Source = imageCarton };

            // Initialise le label pour le chrono
            labelAfficheChrono = new Label { FontSize = TAILLELABELAFFICHECHRONO, Content = DUREEMINIJEU, FontFamily = mainWindow.policeEcriturePersonalisee };

            // Positionne le label et le carton sur le canvas
            Canvas.SetLeft(labelAfficheChrono, mainWindow.ActualWidth / 2);
            Canvas.SetTop(labelAfficheChrono, POSYLABELAFFICHECHRONO);

            Canvas.SetLeft(carton, POSXCARTON);
            Canvas.SetTop(carton, mainWindow.Height - carton.Height);
            Panel.SetZIndex(carton, AUDESSUS);

            // Ajoute le carton et le label au canvas
            mainWindow.CanvasPrincipal.Children.Add(carton);
            mainWindow.CanvasPrincipal.Children.Add(labelAfficheChrono);

            // Redémarre les chronomètres
            mainWindow.chronometre.Start();
            mainWindow.timer.Start();
        }

        // Fait apparaître un bonbon ou une pique aléatoirement
        private void FaireApparaitreBonbonOuPique(string typeObjet, Brush couleur)
        {
            Random random = new Random();

            Image bonbonOuPiques = new Image
            {
                Width = TAILLEOBJETSTOMBANTS,
                Height = TAILLEOBJETSTOMBANTS,
            };

            Canvas.SetTop(bonbonOuPiques, random.Next(POSITIONYLAPLUSELOIGNE, -(int)bonbonOuPiques.Height));
            Canvas.SetLeft(bonbonOuPiques, random.Next(0, Math.Abs((int)mainWindow.CanvasPrincipal.ActualWidth - (int)bonbonOuPiques.Width)));

            // Définit l'image selon le type d'objet (bonbon ou pique)
            if (typeObjet == "bonbon")
            {
                bonbons.Add(bonbonOuPiques);
                bonbonOuPiques.Source = imageBonbon;
            }
            else
            {
                piques.Add(bonbonOuPiques);
                bonbonOuPiques.Source = imagePique;
            }

            mainWindow.CanvasPrincipal.Children.Add(bonbonOuPiques);
        }

        // Gère le jeu pendant la pluie de bonbons
        public void Jouer()
        {
            // Met à jour le chrono affiché
            labelAfficheChrono.Content = mainWindow.tempsRestant;

            // Augmente progressivement la vitesse du joueur jusqu'à la limite maximale
            vitesseJoueur = Math.Min(vitesseJoueur * COEFACCELERATION, VITESSEMAXJOUEUR);

            // Ajoute de nouveaux bonbons si le nombre est insuffisant
            if (bonbons.Count < NOMBREBONBONSECRAN)
                FaireApparaitreBonbonOuPique("bonbon", Brushes.Blue);

            // Ajoute de nouvelles piques si le nombre est insuffisant
            if (piques.Count < NOMBREPIQUESECRAN)
                FaireApparaitreBonbonOuPique("pique", Brushes.Red);

            // Gère le mouvement du carton selon les touches pressées
            if (droite && Canvas.GetLeft(carton) + carton.Width < mainWindow.CanvasPrincipal.ActualWidth)
                Canvas.SetLeft(carton, Canvas.GetLeft(carton) + vitesseJoueur);
            else if (gauche && Canvas.GetLeft(carton) > 0)
                Canvas.SetLeft(carton, Canvas.GetLeft(carton) - vitesseJoueur);
            else
                vitesseJoueur = MINVITESSEJOUEUR;

            // Fait tomber les bonbons et les piques
            Bouger(bonbons, "bonbons");
            Bouger(piques, "piques");

            // Termine le jeu si le temps est écoulé
            if (mainWindow.tempsRestant <= 0 && mainWindow.jeuInitialise)
                mainWindow.ecranIntermediaire.InitEcranIntermediaire(score, true);
        }


        // Méthode pour déplacer les objets qui tombent
        private void Bouger(List<Image> objets, string typeObjet)
        {
            // Liste pour stocker les éléments à supprimer après collision
            List<Image> elementsASupprimer = new List<Image>();

            // Parcours de la liste des objets en ordre inverse pour permettre la suppression sans décalage d'index
            for (int i = objets.Count - 1; i >= 0; i--)
            {
                // Déplacer l'objet vers le bas en appliquant une vitesse de chute et un coefficient d'accélération
                Canvas.SetTop(objets[i], Canvas.GetTop(objets[i]) + VITESSECHUTEOBJETS * coefAccelerationChute);

                if (Collision(objets[i], carton, typeObjet))
                {
                    // Si l'objet est un bonbon, on le supprime et on augmente le score
                    if (typeObjet == "bonbons")
                    {
                        mainWindow.CanvasPrincipal.Children.Remove(objets[i]);
                        elementsASupprimer.Add(objets[i]);
                        score++;
                    }
                    // Sinon, déclencher l'écran intermédiaire et arrêter le processus
                    else
                    {
                        mainWindow.ecranIntermediaire.InitEcranIntermediaire(0, false);
                        return;
                    }
                }
                // Si l'objet tombe en dehors de l'écran, le supprimer de l'affichage et de la liste
                else if (Canvas.GetTop(objets[i]) > mainWindow.CanvasPrincipal.ActualHeight)
                {
                    mainWindow.CanvasPrincipal.Children.Remove(objets[i]);
                    objets.RemoveAt(i);
                }
            }

            for (int i = 0; i < elementsASupprimer.Count; i++)
                objets.Remove(elementsASupprimer[i]);
        }

        // Méthode pour vérifier la collision entre un objet tombant et le joueur
        private bool Collision(Image objetTombant, Image joueur, string typeObjet)
        {
            // Retourne vrai si les coordonnées des deux objets se chevauchent
            return
                (Canvas.GetLeft(objetTombant) + objetTombant.Width >= Canvas.GetLeft(joueur) - 10) &&
                (Canvas.GetLeft(objetTombant) <= Canvas.GetLeft(joueur) + joueur.Width - 10) &&
                (Canvas.GetTop(objetTombant) >= Canvas.GetTop(joueur)) &&
                (Canvas.GetTop(objetTombant) <= Canvas.GetTop(joueur) + 10);
        }

        public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Left)
                gauche = true;

            if (e.Key == System.Windows.Input.Key.Right)
                droite = true;

            // Empêcher le mouvement si les deux touches sont pressées simultanément
            if (e.Key == System.Windows.Input.Key.Left && e.Key == System.Windows.Input.Key.Right)
            {
                droite = false;
                gauche = false;
            }
        }

        // Méthode appelée lorsqu'une touche est relâchée pour arrêter le mouvement horizontal
        public void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Left)
                gauche = false;

            if (e.Key == System.Windows.Input.Key.Right)
                droite = false;
        }
    }
}
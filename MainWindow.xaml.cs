using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CompilationMiniJeux
{
    public partial class MainWindow : Window
    {
        public static readonly int FREQUENCERAFRAICHISSEMENT = 16;

        private static readonly int NBPVMAX = 5;

        // Indices pour différents mini-jeux
        private static readonly int INDICEMINIJEUBONBONS = 1,
                                     INDICEMINIJEUTAPETTE = 2,
                                     INDICEMINIJEURUNNER = 3,
                                     INDICEMINIJEUQTEPIGNATA = 4,
                                     INDICEMINIJEUQTEAVION = 5,
                                     INDICEMINIJEUQTEBONNETEAU = 6,
                                     INDICEMINIJEUECRIREVITE = 7,
                                     INDICEMINIJEUPACMAN = 8;

        // Coefficient de réduction de vitesse du chronomètre
        private static readonly double COEFACCELERATIONCHRONO = 0.95;

        private static readonly double MINVITESSECHRONO = 1;

        // Valeur par défaut pour les bonbons gagnés
        private static readonly int VALEURDEFAUTNBBONBONSGAGNES = -1;

        // Accélération par défaut pour les éléments de jeu (vitesses scroll et vitesse de chutte des objets)
        private static readonly double VALEURACCELERATION = 0.1;

        // Vitesse actuelle du chronomètre
        private double vitesseChrono = MINVITESSECHRONO;

        public int nbBonbonsGagnes;
        public int nbBonbons; 
        public int nbPV;
        public int i;
        public int tempsRestant;

        public double volume;

        public double vitesseScroll;
        public double coefAccelerationVitesseScroll;
        public double coefAccelerationChute;

        public int[] indicesMiniJeux = new int[] { INDICEMINIJEURUNNER, INDICEMINIJEUBONBONS, INDICEMINIJEUPACMAN, INDICEMINIJEUQTEAVION, INDICEMINIJEUQTEBONNETEAU, INDICEMINIJEUQTEPIGNATA, INDICEMINIJEUTAPETTE };

        public bool jeuInitialise;

        public DispatcherTimer timer;
        public DispatcherTimer chronometre;

        // Police définie pour le texte
        public FontFamily policeEcriturePersonalisee;

        // Ecrans titre, de transition et de game over
        public EcranTitre ecranTitre;
        public EcranGameOver ecranGameOver;
        public EcranIntermediaire ecranIntermediaire;

        // Instances des différents mini-jeux
        private MiniJeuBonbon miniJeuBonbon;
        private MiniJeuTapette miniJeuTapette;
        private MiniJeuRunner miniJeuRunner;
        private MiniJeuQTEPignata miniJeuQTEPignata;
        private MiniJeuAvion miniJeuAvion;
        private MiniJeuBonneteau miniJeuBonneteau;
        //private MiniJeuEcrireVite miniJeuEcrireVite;
        private MiniJeuPacMan.MiniJeuPacMan miniJeuPacMan;
        // Polymorphisme pour les mini-jeux
        private List<IMiniJeu> miniJeux;

        private MediaPlayer musique;

        //------------------------------------------------------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void InitMusique()
        {
            musique = new MediaPlayer();
            musique.Open(new Uri("pack://application:,,,/sons/musiqueJeu.wav"));
            musique.MediaEnded += RelanceMusique;
            musique.Volume = volume / 100;
            musique.Play();

        }
        private void RelanceMusique(object? sender, EventArgs e)
        {
            musique.Position = TimeSpan.Zero;
            musique.Play();
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Initialisation de la liste
            miniJeux = new List<IMiniJeu>();

            // Initialisation des écrans et mini-jeux
            ecranIntermediaire = new EcranIntermediaire(this);
            miniJeuBonbon = new MiniJeuBonbon(this);
            miniJeuTapette = new MiniJeuTapette(this);
            miniJeuRunner = new MiniJeuRunner(this);
            miniJeuQTEPignata = new MiniJeuQTEPignata(this);
            miniJeuAvion = new MiniJeuAvion(this);
            miniJeuBonneteau = new MiniJeuBonneteau(this);
            //miniJeuEcrireVite = new MiniJeuEcrireVite(this);
            miniJeuPacMan = new MiniJeuPacMan.MiniJeuPacMan(this);

            // Ajout des mini-jeux à la liste
            miniJeux.Add(miniJeuBonbon);
            miniJeux.Add(miniJeuTapette);
            miniJeux.Add(miniJeuRunner);
            miniJeux.Add(miniJeuQTEPignata);
            miniJeux.Add(miniJeuAvion);
            miniJeux.Add(miniJeuBonneteau);
            //miniJeux.Add(miniJeuEcrireVite);
            miniJeux.Add(miniJeuPacMan);

            // Mélange des indices des mini-jeux
            MelangerIndicesMiniJeux(indicesMiniJeux);

            // Chargement de la police personnalisée
            policeEcriturePersonalisee = new FontFamily(new Uri("pack://application:,,,/"), "./PoliceEcriture/#Pixel Sans Serif");

            chronometre = new DispatcherTimer();
            chronometre.Interval = TimeSpan.FromSeconds(vitesseChrono);
            chronometre.Tick += MettreAJourChronometre;
            chronometre.Start();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(FREQUENCERAFRAICHISSEMENT);
            timer.Tick += Jeu;

            ecranTitre = new EcranTitre(this);

            InitMusique();
            ResetJeu();
        }

        // Méthode pour mettre à jour le chronomètre à chaque tick
        private void MettreAJourChronometre(object sender, EventArgs e)
        {
            tempsRestant = Math.Max(0, tempsRestant - 1);
        }

        //------------------------------------------------------------------------------------------------

        // Méthode pour réinitialiser le jeu à son état initial
        public void ResetJeu()
        {
            if (!ecranTitre.active)
            {
                this.CanvasPrincipal.Children.Clear();
                jeuInitialise = false;
                i = 0;
                nbBonbons = 0;
                nbBonbonsGagnes = VALEURDEFAUTNBBONBONSGAGNES;
                nbPV = NBPVMAX;
                vitesseChrono = MINVITESSECHRONO;
                volume = ecranTitre.volume;
                musique.Volume = volume / 100;
                // Réinitialisation des paramètres des mini-jeux
                vitesseScroll = MiniJeuRunner.MINVITESSESCROLL;
                coefAccelerationVitesseScroll = MiniJeuAvion.MINCOEFACCELERATIONVITESSESCROLL;
                coefAccelerationChute = MiniJeuBonbon.MINCOEFACCELERATIONCHUTE;

                if (!timer.IsEnabled) timer.Start();
            }
        }

        private void Jeu(object sender, EventArgs e)
        {
            musique.Volume = ecranTitre.volume;
            // Vérifie si le nombre de points de vie est épuisé.
            if (nbPV <= 0)
            {
                ecranGameOver = new EcranGameOver(this);
                ecranGameOver.InitEcranFin();
            }
            // Vérifie si le nombre de bonbons gagnés dépasse la valeur par défaut.
            else if (nbBonbonsGagnes > VALEURDEFAUTNBBONBONSGAGNES)
            {
                ecranIntermediaire.ResterSurEcranIntermediaire();
            }
            // Gère le mini-jeu de pluie de bonbons.
            else
                Jouer(miniJeux[i]);
        }

        private void Jouer(IMiniJeu miniJeu)
        {
            if (!jeuInitialise)
            {
                miniJeu.Init();
                jeuInitialise = true;
            }
            miniJeu.Jouer();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            miniJeux[i].OnKeyDown(sender, e);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            miniJeux[i].OnKeyUp(sender, e);
        }

        // Méthode appelée lorsqu'un bouton de la souris est relâché.
        private void MouseUp_MainWindow(object sender, MouseButtonEventArgs e)
        {
            if (miniJeux.IndexOf(miniJeuTapette) == i) miniJeuTapette.MouseUpTapette(sender, e);
        }


        //------------------------------------------------------------------------------------------------

        // Mélange les indices des mini-jeux pour les présenter dans un ordre aléatoire.
        public static void MelangerIndicesMiniJeux(int[] indicesMiniJeux)
        {
            // Crée une instance de Random pour générer des nombres aléatoires.
            Random random = new Random();
            int j, dernierMiniJeuJoue = indicesMiniJeux[indicesMiniJeux.Length - 1];

            for (int i = indicesMiniJeux.Length - 1; i > 0; i--)
            {
                // Sélectionne un indice aléatoire entre 0 et i (inclus).
                j = random.Next(0, i + 1);

                // Échange les éléments à la position i et j.
                (indicesMiniJeux[i], indicesMiniJeux[j]) = (indicesMiniJeux[j], indicesMiniJeux[i]);
            }

            // Assure que le dernier mini-jeu joué ne soit pas le premier du nouveau mélange.
            if (dernierMiniJeuJoue == indicesMiniJeux[0])
            {
                j = random.Next(1, indicesMiniJeux.Length);
                (indicesMiniJeux[0], indicesMiniJeux[j]) = (indicesMiniJeux[j], indicesMiniJeux[0]);
            }
        }

        // Gère la fin d'un mini-jeu et prépare le prochain mini-jeu ou un nouveau cycle.
        public void GererFinMiniJeu()
        {
            if (!chronometre.IsEnabled)
                chronometre.Start();

            // Vérifie si le temps est écoulé pour le mini-jeu actuel.
            if (tempsRestant <= 0)
            {
                // Réinitialise le nombre de bonbons gagnés à sa valeur par défaut.
                nbBonbonsGagnes = VALEURDEFAUTNBBONBONSGAGNES;
                i++; // Passe au mini-jeu suivant.

                // Si tous les mini-jeux ont été joués, mélange les indices pour un nouveau cycle.
                if (i > indicesMiniJeux.Length - 1)
                {
                    MelangerIndicesMiniJeux(indicesMiniJeux); 
                    i = 0; 

                    // Accélère le chronomètre pour augmenter la difficulté.
                    vitesseChrono *= COEFACCELERATIONCHRONO;
                    chronometre.Interval = TimeSpan.FromSeconds(vitesseChrono);

                    // Accélère les paramètres spécifiques des différents mini-jeux pour augmenter la difficulté.
                    vitesseScroll *= MiniJeuRunner.COEFAACCELERATIONVITESSESCROLL;
                    coefAccelerationVitesseScroll += VALEURACCELERATION;
                    coefAccelerationChute += VALEURACCELERATION;
                }

                jeuInitialise = false;
            }
        }
    }
}
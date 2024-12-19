using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Media;

namespace CompilationMiniJeux
{
    internal class MiniJeuRunner : IMiniJeu
    {
        // Constantes pour la gravité et le saut
        private static readonly double GRAVITENORMALE = 1;
        private static readonly double GRAVITEQUANDTOMBE = 1.5;
        private static readonly double FORCESAUT = -15;

        // Constantes pour le défilement horizontal
        public static readonly double MINVITESSESCROLL = 7;
        public static readonly double COEFAACCELERATIONVITESSESCROLL = 1.1;      // Coefficient d'accélération du défilement

        // Constantes pour la taille des éléments
        private static readonly double TAILLEPICS = 12.5;
        private static readonly double TAILLEBLOCS = 50;
        private static readonly double TAILLEOBJETDOUBLESAUT = 50;
        private static readonly double TAILLEOBJETBONBON = 35;

        // Constantes pour les blocs et le joueur
        private static readonly int NBPICSBLOC = 4;
        private static readonly int AUDESSUS = 10;
        private static readonly int NBTYPEBLOCS = 15;
        private static readonly int NBFRAMESANIMATIONJOUEUR = 3;
        private static readonly int VALEURMAXCOMPTEUR = 2;
        private static readonly int XPOSDEPARTJOUEUR = 100;
        private static readonly int YPOSDEPARTJOUEUR = 600;
        private static readonly int TAILLEJOUEUR = 50;

        // Dictionnaire des symboles pour les différents objets et blocs
        private static readonly Dictionary<string, char> SYMBOLES = new Dictionary<string, char>
                                                                    {
                                                                        { "DOUBLESAUT", 'J' },   // Symbole pour l'objet double saut
                                                                        { "BOMBON", 'B' },       // Symbole pour l'objet bonbon
                                                                        { "BLOCSANSPICS", '#' }, // Symbole pour un bloc sans pics
                                                                        { "BLOCAVECPICS", 'P' }, // Symbole pour un bloc avec pics
                                                                        { "BLOCFIN", '|' }       // Symbole pour la ligne d'arrivée
                                                                    };

        // Niveaux du jeu représentés sous forme de cartes
        private static readonly string[] CARTE1 =
        {
            "################################################################|",
            "#...............................................................|",
            "#...............................................................|",
            "#...............................................................|",
            "#...............................................................|",
            "#...........................................................B...|",
            "#.........................J.....................................|",
            "#............................B...B....BBBB...JJJ...BBBBB.P....P#|",
            "#......................BB....P..P#...P#####.......P#######.....P|",
            "#..................B.P####...P...P...P#####.......P#######.....P|",
            "#.............BB..P#######...P...P...P#####.......P#######.....P|",
            "#........BBB.P############...P...P...P#####.......P#######.....P|",
            "#......P##################...P...P...P#####.......P#######.....P|",
            "################################################################|"
        };

        private static readonly string[] CARTE2 =
        {
            "################################################################|",
            "#....................................................P..........|",
            "#....................................................P..........|",
            "#....................................................P..........|",
            "#....................................................P..........|",
            "#....................................B...............P..........|",
            "#..................................J...BB...B....BB.............|",
            "#..........................BBB........P##..PP..PP##.............|",
            "#.....................BBB..P#####.....P##...P...P##.........BBB.|",
            "#.................BB...P#########.....P##...P...P##..BB....P####|",
            "#............BB...P#####################################...P####|",
            "#........B..P###################################################|",
            "#.....B.P#######################################################|",
            "################################################################|"
        };

        private static readonly string[] CARTE3 =
        {
            "################################################################|",
            "#...............................................................|",
            "#...............................................................|",
            "#.......................P.......................................|",
            "#.......................P.......................................|",
            "#.......................P....................P..................|",
            "#.......................P....................P.............BBB..|",
            "#.....................................BB............BB...P######|",
            "#....................BB..............P###....B..J..P###..P######|",
            "#......................JJ.....J.BBBB.P###....P.....P###..P######|",
            "#...............BB..P#.....P#############....P.....P###..P######|",
            "#.........BBBB.P######.....P#############....P.....P############|",
            "#........P############.....P#############....P.....P############|",
            "################################################################|"
        };

        private static readonly string[] CARTE4 =
        {
            "################################################################|",
            "#...............................................................|",
            "#...............................................................|",
            "#...............................................................|",
            "#...............................................................|",
            "#...............................................................|",
            "#...............................................................|",
            "#...............................................................|",
            "#...............................................................|",
            "#...............................................................|",
            "#....................................................BBBBB......|",
            "#........B.........B.........P.................B....P#####......|",
            "#........P..BBBBB..P.....P####.BBBBBBB..P......P................|",
            "################################################################|"
        };

        // Tableau regroupant toutes les cartes disponibles
        private static readonly string[][] TABCARTES = { CARTE1, CARTE2, CARTE3, CARTE4 };

        // Variables pour la gestion du saut
        private bool saute;
        private bool doubleSautDisponible;
        private bool enDoubleSaut;
        private double vitesseSaut = 0;

        // Vitesse de défilement du jeu
        public double vitesseScroll;

        // Listes d'images pour les obstacles, blocs finaux, objets de saut et bonbons
        private List<Image> obstacles = new List<Image>();
        private List<Image> blocsFin = new List<Image>();
        private Image personnage;

        private List<Image> objetsSaut = new List<Image>();
        private List<Image> bonbons = new List<Image>();
        private List<Polygon> pics = new List<Polygon>();

        // Images pour les blocs et animations du joueur
        private BitmapImage[] imagesBlocs = new BitmapImage[NBTYPEBLOCS];
        private BitmapImage[] imagesAnimationMarcheJoueur = new BitmapImage[NBFRAMESANIMATIONJOUEUR];
        private BitmapImage imageAnimationSautJoueur;
        private BitmapImage imageObjDoubleSaut;
        private BitmapImage imageBonbon;
        private BitmapImage imageLigneArrivee;

        private Random random = new Random();
        private MainWindow mainWindow;

        private int bonbonsCollectes;

        private int compteur;
        private int indexImageAnimatonMarche;

        //----------------------------------------------------------------------------------------------------------------------------------

        public MiniJeuRunner(MainWindow fenetreParent)
        {
            mainWindow = fenetreParent;
            InitImages();
        }

        // Méthode pour initialiser les images utilisées dans le jeu
        private void InitImages()
        {
            // Chargement des images des blocs
            for (int i = 0; i < NBTYPEBLOCS; i++)
                imagesBlocs[i] = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuRunner/Blocs/bloc{i}.png"));

            // Chargement des images pour l'animation de marche du joueur
            for (int i = 0; i < NBFRAMESANIMATIONJOUEUR; i++)
                imagesAnimationMarcheJoueur[i] = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuRunner/Joueur/animationmarche{i}.png"));

            // Chargement des autres images nécessaires pour le jeu
            imageAnimationSautJoueur = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuRunner/Joueur/animationsaut.png"));
            imageObjDoubleSaut = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuRunner/objetdoublesaut.png"));
            imageBonbon = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuRunner/bonbon.png"));
            imageLigneArrivee = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuRunner/Blocs/lignearrivee.png"));
        }

        //----------------------------------------------------------------------------------------------------------------------------------

        // Méthode pour initialiser et démarrer le jeu
        public void Init()
        {
            // Arrête le timer principal pour éviter toute mise à jour pendant l'initialisation
            mainWindow.timer.Stop();

            // Efface tous les enfants du Canvas principal pour une réinitialisation complète
            mainWindow.CanvasPrincipal.Children.Clear();

            // Réinitialise les listes d'obstacles, blocs finaux, bonbons, objets de double saut et pics
            obstacles.Clear();
            blocsFin.Clear();
            bonbons.Clear();
            objetsSaut.Clear();
            pics.Clear();

            // Initialise l'image du personnage avec sa taille et son image par défaut
            personnage = new Image
            {
                Width = TAILLEJOUEUR,
                Height = TAILLEJOUEUR,
                Source = imagesAnimationMarcheJoueur[0]
            };

            mainWindow.CanvasPrincipal.Children.Add(personnage);
            Canvas.SetLeft(personnage, XPOSDEPARTJOUEUR);
            Canvas.SetTop(personnage, YPOSDEPARTJOUEUR);
            Panel.SetZIndex(personnage, AUDESSUS);

            // Réinitialise les états 
            saute = false;
            doubleSautDisponible = false;
            enDoubleSaut = false;
            vitesseSaut = 0;
            bonbonsCollectes = 0;
            compteur = VALEURMAXCOMPTEUR;
            indexImageAnimatonMarche = 0;

            // Initialise la carte avec une de 4 configurations
            InitCarte(TABCARTES[random.Next(TABCARTES.Length)]);

            // Redémarre le timer principal pour relancer le jeu
            mainWindow.timer.Start();
        }

        // Méthode pour initialiser les éléments de la carte
        private void InitCarte(string[] carte)
        {
            for (int i = 0; i < carte.Length; i++)
            {
                for (int j = 0; j < carte[0].Length; j++)
                {
                    if (carte[i][j] == SYMBOLES["BLOCAVECPICS"])
                    {
                        InitBlocs(i, j, obstacles);
                        InitPicsBloc(i, j);
                    }
                    else if (carte[i][j] == SYMBOLES["BLOCSANSPICS"])
                        InitBlocs(i, j, obstacles);
                    else if (carte[i][j] == SYMBOLES["BLOCFIN"])
                        InitBlocs(i, j, blocsFin);
                    else if (carte[i][j] == SYMBOLES["BOMBON"])
                        InitObjetCollectable(i, j, TAILLEOBJETBONBON, bonbons);
                    else if (carte[i][j] == SYMBOLES["DOUBLESAUT"])
                        InitObjetCollectable(i, j, TAILLEOBJETDOUBLESAUT, objetsSaut);
                }
            }
        }

        // Méthode pour initialiser un bloc à une position donnée et l'ajouter à une liste spécifiée
        private void InitBlocs(int i, int j, List<Image> listeOuAjouterBloc)
        {
            // Crée une nouvelle image pour le bloc avec les dimensions appropriées
            Image bloc = new Image { Width = TAILLEBLOCS, Height = TAILLEBLOCS };

            // Si le bloc est une ligne d'arrivée, utilise l'image correspondante
            if (listeOuAjouterBloc == blocsFin)
                bloc.Source = imageLigneArrivee;
            else
                bloc.Source = imagesBlocs[random.Next(imagesBlocs.Length)];

            Canvas.SetLeft(bloc, j * TAILLEBLOCS);
            Canvas.SetTop(bloc, i * TAILLEBLOCS);

            mainWindow.CanvasPrincipal.Children.Add(bloc);
            listeOuAjouterBloc.Add(bloc);
        }

        // Méthode pour initialiser les pics associés à un bloc à une position donnée
        private void InitPicsBloc(int i, int j)
        {
            for (int k = 0; k < NBPICSBLOC; k++)
            {
                // Crée un polygone représentant un pic
                Polygon pic = new Polygon
                {
                    Points = new PointCollection
                    {
                        new Point(0, k * TAILLEPICS),
                        new Point(-TAILLEPICS, k * TAILLEPICS + TAILLEPICS / 2),
                        new Point(0, (k + 1) * TAILLEPICS)
                    },
                    Fill = Brushes.Black
                };

                mainWindow.CanvasPrincipal.Children.Add(pic);
                Canvas.SetLeft(pic, j * TAILLEBLOCS);
                Canvas.SetTop(pic, i * TAILLEBLOCS);

                pics.Add(pic);
            }
        }

        // Initialise un objet collectable et l'ajoute au canevas principal
        private void InitObjetCollectable(int i, int j, double taille, List<Image> listeObjetCollectable)
        {
            // Création d'une image pour l'objet collectable avec la taille spécifiée
            Image collectable = new Image
            {
                Width = taille,
                Height = taille
            };

            // Définit la source de l'image en fonction du type d'objet collectable
            if (listeObjetCollectable == objetsSaut)
                collectable.Source = imageObjDoubleSaut;
            else
                collectable.Source = imageBonbon;

            mainWindow.CanvasPrincipal.Children.Add(collectable);

            // Positionne l'objet collectable à la position (i, j) avec ajustement pour le centrage
            Canvas.SetLeft(collectable, j * TAILLEBLOCS + (TAILLEBLOCS - taille) / 2);
            Canvas.SetTop(collectable, i * TAILLEBLOCS + (TAILLEBLOCS - taille) / 2);

            listeObjetCollectable.Add(collectable);
        }

        //----------------------------------------------------------------------------------------------------------------------------------

        // Boucle Principale
        public void Jouer()
        {
            // Gère l'animation de saut si le personnage saute
            if (saute)
            {
                personnage.Source = imageAnimationSautJoueur;
                indexImageAnimatonMarche = 0;
                compteur = VALEURMAXCOMPTEUR;
            }
            else
            {
                // Gère l'animation de marche 
                compteur--;
                if (compteur <= 0)
                {
                    indexImageAnimatonMarche = (indexImageAnimatonMarche + 1) % NBFRAMESANIMATIONJOUEUR;
                    compteur = VALEURMAXCOMPTEUR;
                }
                personnage.Source = imagesAnimationMarcheJoueur[indexImageAnimatonMarche];
            }

            AppliquerGravite();
            DefilerElements();
            VerifierCollisions();
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (!saute)
                {
                    saute = true;
                    vitesseSaut = FORCESAUT;
                }
                else if (doubleSautDisponible && !enDoubleSaut)
                {
                    vitesseSaut = FORCESAUT;
                    enDoubleSaut = true;
                    doubleSautDisponible = false;
                }
            }
        }
        public void OnKeyUp(object sender, KeyEventArgs e) { }

        // Applique la gravité au personnage
        private void AppliquerGravite()
        {
            // Ajuste la vitesse de chute en fonction de la direction du saut
            if (vitesseSaut >= 0)
                vitesseSaut += GRAVITEQUANDTOMBE;
            else
                vitesseSaut += GRAVITENORMALE;

            Canvas.SetTop(personnage, Canvas.GetTop(personnage) + vitesseSaut);
        }

        // Défile les éléments à l'écran en fonction de la vitesse de défilement
        private void DefilerElements()
        {
            foreach (Image obstacle in obstacles)
                Canvas.SetLeft(obstacle, Canvas.GetLeft(obstacle) - vitesseScroll);

            foreach (Image blocFin in blocsFin)
                Canvas.SetLeft(blocFin, Canvas.GetLeft(blocFin) - vitesseScroll);

            foreach (Image bonbon in bonbons)
                Canvas.SetLeft(bonbon, Canvas.GetLeft(bonbon) - vitesseScroll);

            foreach (Image obj in objetsSaut)
                Canvas.SetLeft(obj, Canvas.GetLeft(obj) - vitesseScroll);

            foreach (Polygon pic in pics)
                Canvas.SetLeft(pic, Canvas.GetLeft(pic) - vitesseScroll);
        }

        //----------------------------------------------------------------------------------------------------------------------------------

        // Vérifie les collisions avec différents types d'objets
        private void VerifierCollisions()
        {
            GererCollisionsBombons();
            GererCollisionsObjetsSaut();
            GererCollisionsMurs();
            GererCollisionsTriangles();
            GererCollisionsBlocsFin();
        }

        // Gère les collisions avec les obstacles
        private void GererCollisionsMurs()
        {
            bool collisionDetectee = false;

            // Crée le rectangle de collision du personnage
            System.Drawing.Rectangle joueurRect = new System.Drawing.Rectangle((int)Canvas.GetLeft(personnage), (int)Canvas.GetTop(personnage), (int)personnage.Width, (int)personnage.Height);

            // Vérifie les collisions avec chaque obstacle
            foreach (Image obstacle in obstacles)
            {
                System.Drawing.Rectangle obstacleRect = new System.Drawing.Rectangle((int)Canvas.GetLeft(obstacle), (int)Canvas.GetTop(obstacle), (int)obstacle.Width, (int)obstacle.Height);

                if (joueurRect.IntersectsWith(obstacleRect))
                {
                    // Gère la réaction à la collision (arrête le saut)
                    if (vitesseSaut < 0)
                        Canvas.SetTop(personnage, Canvas.GetTop(obstacle) + obstacle.Height);
                    else
                        Canvas.SetTop(personnage, Canvas.GetTop(obstacle) - personnage.Height);

                    vitesseSaut = 0;
                    collisionDetectee = true;
                    enDoubleSaut = false;
                    break;
                }
            }

            // Met à jour l'état de saut en fonction de la collision
            if (collisionDetectee) saute = false;
            else saute = true;
        }

        // Gère les collisions avec les blocs de fin du niveau
        private void GererCollisionsBlocsFin()
        {
            System.Drawing.Rectangle joueurRect = new System.Drawing.Rectangle((int)Canvas.GetLeft(personnage), (int)Canvas.GetTop(personnage), (int)personnage.Width, (int)personnage.Height);

            foreach (Image blocFin in blocsFin)
            {
                System.Drawing.Rectangle blocFinRectangle = new System.Drawing.Rectangle((int)Canvas.GetLeft(blocFin), (int)Canvas.GetTop(blocFin), (int)blocFin.Width, (int)blocFin.Height);

                if (joueurRect.IntersectsWith(blocFinRectangle))
                {
                    // Affiche l'écran de fin de niveau
                    mainWindow.ecranIntermediaire.InitEcranIntermediaire(bonbonsCollectes, true);
                    return;
                }
            }
        }

        /// Gère les collisions entre le personnage et les pics.
        public void GererCollisionsTriangles()
        {
            // Initialise le rectangle de collision pour le personnage
            int i = 0;
            System.Drawing.Rectangle joueurRect = new System.Drawing.Rectangle((int)Canvas.GetLeft(personnage), (int)Canvas.GetTop(personnage), (int)personnage.Width, (int)personnage.Height);

            // Parcourt tous les triangles (pics) présents dans la liste
            for (int k = 0; k < pics.Count; k++)
            {
                // Crée un rectangle de collision pour chaque triangle avec un décalage calculé
                System.Drawing.Rectangle boiteCollision = new System.Drawing.Rectangle((int)(Canvas.GetLeft(pics[k]) - TAILLEPICS / 2), (int)(Canvas.GetTop(pics[k]) + (i * TAILLEPICS)), (int)TAILLEPICS / 2, (int)TAILLEPICS / 2);

                i = (i + 1) % NBPICSBLOC;

                if (joueurRect.IntersectsWith(boiteCollision))
                {
                    mainWindow.ecranIntermediaire.InitEcranIntermediaire(0, false);
                    return;
                }
            }
        }

        /// Gère les collisions entre le personnage et les bonbons.
        private void GererCollisionsBombons()
        {
            // Initialise le rectangle de collision pour le personnage
            System.Drawing.Rectangle joueurRect = new System.Drawing.Rectangle((int)Canvas.GetLeft(personnage), (int)Canvas.GetTop(personnage), (int)personnage.Width, (int)personnage.Height);

            // Parcourt tous les bonbons de la liste, en sens inverse pour permettre la suppression
            for (int i = bonbons.Count - 1; i >= 0; i--)
            {
                // Crée un rectangle de collision pour chaque bonbon
                System.Drawing.Rectangle bombonRect = new System.Drawing.Rectangle((int)Canvas.GetLeft(bonbons[i]), (int)Canvas.GetTop(bonbons[i]), (int)bonbons[i].Width, (int)bonbons[i].Height);

                // Si une collision est détectée, retire le bonbon du canvas et de la liste
                if (joueurRect.IntersectsWith(bombonRect))
                {
                    mainWindow.CanvasPrincipal.Children.Remove(bonbons[i]);
                    bonbons.RemoveAt(i);
                    bonbonsCollectes++;
                }
            }
        }

        /// Gère les collisions entre le personnage et les objets permettant le double saut.
        private void GererCollisionsObjetsSaut()
        {
            System.Drawing.Rectangle joueurRect = new System.Drawing.Rectangle((int)Canvas.GetLeft(personnage), (int)Canvas.GetTop(personnage), (int)personnage.Width, (int)personnage.Height);

            for (int i = objetsSaut.Count - 1; i >= 0; i--)
            {
                System.Drawing.Rectangle objetSautRect = new System.Drawing.Rectangle((int)Canvas.GetLeft(objetsSaut[i]), (int)Canvas.GetTop(objetsSaut[i]), (int)objetsSaut[i].Width, (int)objetsSaut[i].Height);

                // Si une collision est détectée, retire l'objet du canvas et active le double saut
                if (joueurRect.IntersectsWith(objetSautRect))
                {
                    mainWindow.CanvasPrincipal.Children.Remove(objetsSaut[i]);
                    objetsSaut.RemoveAt(i);
                    doubleSautDisponible = true;
                }
            }
        }
    }
}
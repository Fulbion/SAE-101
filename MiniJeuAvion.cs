using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CompilationMiniJeux
{
    internal class MiniJeuAvion : IMiniJeu
    {
        public static readonly double MINCOEFACCELERATIONVITESSESCROLL = 1;
        private static readonly double VITESSESCROLL = 100;
        private static readonly double TAILLEBLOCS = 50;
        private static readonly double TAILLEOBJETBONBON = 35;

        private bool toucheHaut;
        private double accelerationY;

        public double coefAccelerationVitesseScroll;

        public static string[] carte3 =
        {
            "##################################################################|",
            "............###########################.................##########|",
            "............###########################.................##########|",
            "............###########################.................##########|",
            "............#######.......#############.................##########|",
            "-------.....#######.......................B.B.B.........##########|",
            "#######.............BBBB.................B.B.B....................|",
            "#######.............................###..........##...............|",
            "#######........................########..........##...............|",
            "#######............######....##########..........##....BBBBBB.....|",
            "#######............####################..........##...............|",
            "#######################################..........##...............|",
            "#######################################..........##...............|",
            "##################################################################|"
        };


        public static string[] carte2 =
        {
            "#########################################################################|",
            ".................###.......##......B.B......###..........................|",
            ".................###.......##.....B.B.B.....###..............B.....B.....|",
            ".................###.......##....#######.....##..........................|",
            ".................###.......##....#######......#..........................|",
            "----------.......###.......................BB................B.....B.....|",
            "##########.......###...BB................................................|",
            "##########............BB...##...B.......B..##########....################|",
            "##########.................##....B.....B...#........#...........B.B.B....|",
            "##########.......###.......#################........#...........B.B.B....|",
            "##########.......###########################........#####################|",
            "##########.......###########################........#####################|",
            "##########.......###########################........#####################|",
            "#########################################################################|"
        };


        public string[] carte;

        private Image player;
        private Image arrierePlan;
        private BitmapImage playerSprite = new BitmapImage(new Uri("pack://application:,,,/img/avion/tile0.png"));
        private DispatcherTimer timerAnimation;
        private int indexFrame = 0;
        private Rectangle[,] elements;
        private List<Image> bonbons = new List<Image>();
        private int nbBonbons = 0;

        private MainWindow mainWindow;

        public MiniJeuAvion(MainWindow fenetreParent)
        {
            mainWindow = fenetreParent;
        }

        public void Init()
        {
            mainWindow.timer.Stop();
            mainWindow.chronometre.Stop();

            mainWindow.CanvasPrincipal.Children.Clear();

            // Choix aléatoire de la carte
            Random random = new Random();
            switch (random.Next(0, 2))
            {
                case 0:
                    carte = carte2;
                    break;
                case 1:
                    carte = carte3;
                    break;
            }

            // Création du joueur
            player = new Image
            {
                Width = 40,
                Height = 35
            };

            // Création de l'arrière-plan
            arrierePlan = new Image
            {
                Width = mainWindow.Width * 2,
                Height = mainWindow.Height * 2
            };

            arrierePlan.Source = new BitmapImage(new Uri("pack://application:,,,/images/arr-plan/ciel.jpg"));
            arrierePlan.Stretch = Stretch.Fill;

            player.Source = playerSprite;
            player.Stretch = Stretch.Fill;
            indexFrame = 0;
            timerAnimation = new DispatcherTimer();
            timerAnimation.Interval = TimeSpan.FromMilliseconds(100);
            timerAnimation.Tick += (sender, e) =>
            {
                indexFrame = (indexFrame + 1) % 12;
                player.Source = new BitmapImage(new Uri("pack://application:,,,/images/avion/tile" + indexFrame + ".png"));
            };

            // Positionnement du joueur sur le Canvas
            Canvas.SetLeft(player, 205);
            Canvas.SetTop(player, 205);

            // Positionnement de l'arrière-plan sur le Canvas
            Canvas.SetLeft(arrierePlan, 0);
            Canvas.SetTop(arrierePlan, 0);

            // Ajout du joueur au Canvas
            mainWindow.CanvasPrincipal.Children.Add(arrierePlan);
            mainWindow.CanvasPrincipal.Children.Add(player);

            InitCarte();

            toucheHaut = false;
            accelerationY = 0;

            timerAnimation.Start();
            mainWindow.timer.Start();
            mainWindow.chronometre.Start();
        }

        private void InitCarte()
        {
            double xArrPlan = Canvas.GetLeft(arrierePlan);
            xArrPlan -= VITESSESCROLL * coefAccelerationVitesseScroll;
            Canvas.SetLeft(arrierePlan, xArrPlan);

            elements = new Rectangle[carte.Length, carte[0].Length];

            for (int i = 0; i < elements.GetLength(0); i++)
            {
                for (int j = 0; j < elements.GetLength(1); j++)
                {
                    if (carte[i][j] == '#' || carte[i][j] == '|' || carte[i][j] == '-')
                    {
                        elements[i, j] = new Rectangle();
                        elements[i, j].Width = TAILLEBLOCS;
                        elements[i, j].Height = TAILLEBLOCS;
                        if (carte[i][j] == '#') elements[i, j].Fill = Brushes.Red;
                        else if (carte[i][j] == '|') elements[i, j].Fill = Brushes.Lime;
                        else if (carte[i][j] == '-') elements[i, j].Fill = Brushes.Blue;
                        else if (carte[i][j] == 'B') InitObjetCollectable(i, j, TAILLEOBJETBONBON, bonbons);
                        mainWindow.CanvasPrincipal.Children.Add(elements[i, j]);
                        Canvas.SetLeft(elements[i, j], j * elements[i, j].Width + mainWindow.vitesseScroll * mainWindow.coefAccelerationVitesseScroll);
                        Canvas.SetTop(elements[i, j], i * elements[i, j].Height);
                    }
                    else if (carte[i][j] == 'B') InitObjetCollectable(i, j, TAILLEOBJETBONBON, bonbons);
                }
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

            collectable.Source = new BitmapImage(new Uri("pack://application:,,,/images/MiniJeuRunner/bonbon.png"));

            mainWindow.CanvasPrincipal.Children.Add(collectable);

            // Positionne l'objet collectable à la position (i, j) avec ajustement pour le centrage
            Canvas.SetLeft(collectable, j * TAILLEBLOCS + (TAILLEBLOCS - taille) / 2);
            Canvas.SetTop(collectable, i * TAILLEBLOCS + (TAILLEBLOCS - taille) / 2);

            listeObjetCollectable.Add(collectable);
        }


        private void GestionTouches()
        {
            if (toucheHaut)
            {
                accelerationY -= 0.5;
            }
            else
            {
                accelerationY += 0.5;
            }
        }

        public void Jouer()
        {
            GestionTouches();

            double y = Canvas.GetTop(player);
            if (y + accelerationY > 0 && y + accelerationY < mainWindow.Height - player.Height)
            {
                y += accelerationY;
            }
            else
            {
                accelerationY = 0;
            }


            for (int i = 0; i < elements.GetLength(0); i++)
            {
                for (int j = 0; j < elements.GetLength(1); j++)
                {
                    if (carte[i][j] == '#' || carte[i][j] == '|' || carte[i][j] == '-')
                    {
                        Canvas.SetLeft(elements[i, j], Canvas.GetLeft(elements[i, j]) - mainWindow.vitesseScroll * mainWindow.coefAccelerationVitesseScroll);

                        System.Drawing.Rectangle playerHitbox = new System.Drawing.Rectangle((int)Canvas.GetLeft(player), (int)Canvas.GetTop(player), (int)player.Width, (int)player.Height);
                        System.Drawing.Rectangle hitbox = new System.Drawing.Rectangle((int)Canvas.GetLeft(elements[i, j]), (int)Canvas.GetTop(elements[i, j]), (int)elements[i, j].Width, (int)elements[i, j].Height);
                        if (playerHitbox.IntersectsWith(hitbox))
                        {
                            if (carte[i][j] == '#')
                                mainWindow.ecranIntermediaire.InitEcranIntermediaire(0, false);
                            else if (carte[i][j] == '|')
                                mainWindow.ecranIntermediaire.InitEcranIntermediaire(nbBonbons, true);
                            else if (carte[i][j] == '-')
                            {
                                y -= accelerationY;
                                accelerationY = 0;
                            }
                        }
                    }
                }
            }

            foreach (Image bonbon in bonbons)
            {
                Canvas.SetLeft(bonbon, Canvas.GetLeft(bonbon) - mainWindow.vitesseScroll * mainWindow.coefAccelerationVitesseScroll);
            }

            GererCollisionsBonbons();

            Canvas.SetTop(player, y);
        }
        private void GererCollisionsBonbons()
        {
            // Initialise le rectangle de collision pour le personnage
            System.Drawing.Rectangle joueurRect = new System.Drawing.Rectangle((int)Canvas.GetLeft(player), (int)Canvas.GetTop(player), (int)player.Width, (int)player.Height);

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
                    nbBonbons++;
                }
            }
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                toucheHaut = true;
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                toucheHaut = false;
        }
    }
}
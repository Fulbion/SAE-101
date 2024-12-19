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
    internal class MiniJeuQTEPignata : IMiniJeu
    {
        private static readonly double MAXIMPACTFORCE = 0.4;
        private static readonly double GRAVITE = 9.8;
        private static readonly double LONGUEURFIL = 350, EPAISSEURFIL = 2;
        private static readonly double AMORTISSEMENTROTATION = 0.95;
        private static readonly double ACCELERATIONVITESSEFRAPPE = 1;
        private static readonly double ANGLEROTATIONBATON = 45;
        private static readonly double AUGMENTATIONFORCEIMPACTEBATONPIGNATA = 0.01;
        private static readonly double MINVITESSEBATON = 2;
        private static readonly double POSLIGNEY = -10;

        private static readonly int DROITE = 1, GAUCHE = -1, VITESSERETOURBATON = 10;
        private static readonly int DUREEMINIJEU = 10;
        private static readonly int NBONBONSMINGAGNE = 10, NBONBONSMAXGAGNE = 15;
        private static readonly int NBTOUCHESAPRESSER = 6;
        private static readonly int VALEURMAXCOMPTEUR = 5;
        private static readonly int NBFRAMESPIGNATADESTRUCTION = 5;
        private static readonly int TAILLEPIGNATA = 300;
        private static readonly int TAILLERECTANGLECOLLISION = 15;

        private static readonly Point MILIEUBASBATON = new Point(0.5, 1);

        private static readonly Key[] TOUCHESPIGNATA = { Key.Up, Key.Down, Key.Right, Key.Left, Key.A, Key.Z };
        private static readonly double[] POSBATON = { 590, 300 };

        private static readonly Dictionary<string, int> INFOSLABELS = new Dictionary<string, int>
                                                                                    {
                                                                                        { "POSXLABELAFFICHERTOUCHEAPRESSER", 20 },
                                                                                        { "POSXLABELAFFICHERCHRONO", 1150},
                                                                                        { "POSY", 20 },
                                                                                        { "TAILLEPOLICE", 25 },
                                                                                    };

        private static readonly Dictionary<string, int> INFOSBATON = new Dictionary<string, int>
                                                                                    {
                                                                                        { "LARGEUR", 30 },
                                                                                        { "HAUTEUR", 240 },
                                                                                        { "ARRONDISSEMENTBORDS", 30 },
                                                                                    };

        private Key[] suiteTouchesAPresser;

        private double angle;
        private double vitesseAngulaire;
        private double vitesseBaton;
        private double angleActuelBaton = -ANGLEROTATIONBATON;
        private double forceImpactBatonPignata;

        private int directionBaton;
        private int nbBonbonsGagnesDansMiniJeu;
        private int index;
        private int indexImagePignataDesintegration;
        private int compteur;

        private bool gagne;

        private Line ligne;
        private Rectangle baton;
        private Label afficherToucheAPressser, labelAfficheChrono;
        private MainWindow mainWindow;
        private Random random = new Random();

        private DispatcherTimer animationBatonTimer;

        private BitmapImage imagePignata;
        private BitmapImage[] animationsBonbonPignataExplose = new BitmapImage[NBFRAMESPIGNATADESTRUCTION];

        private Image pignata;

        public MiniJeuQTEPignata(MainWindow fenetreParent)
        {
            mainWindow = fenetreParent;

            animationBatonTimer = new DispatcherTimer();
            animationBatonTimer.Interval = TimeSpan.FromMilliseconds(MainWindow.FREQUENCERAFRAICHISSEMENT);
            animationBatonTimer.Tick += animerBaton;

            InitImages();
        }

        private void InitImages()
        {
            imagePignata = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuPignata/pignata.png"));
            for (int i = 0; i < animationsBonbonPignataExplose.Length; i++)
                animationsBonbonPignataExplose[i] = new BitmapImage(new Uri($"pack://application:,,,/img/MiniJeuPignata/destructionpignata{i}.png"));
        }

        public void Init()
        {
            mainWindow.timer.Stop();
            mainWindow.chronometre.Stop();

            mainWindow.CanvasPrincipal.Children.Clear();

            labelAfficheChrono = new Label {
                Content = DUREEMINIJEU,
                FontFamily = mainWindow.policeEcriturePersonalisee,
                FontSize = INFOSLABELS["TAILLEPOLICE"],
            };

            Canvas.SetLeft(labelAfficheChrono, INFOSLABELS["POSXLABELAFFICHERCHRONO"]);
            Canvas.SetTop(labelAfficheChrono, INFOSLABELS["POSY"]);
            mainWindow.CanvasPrincipal.Children.Add(labelAfficheChrono);

            afficherToucheAPressser = new Label
            {
                Content = "Touche à presser : ",
                FontSize = INFOSLABELS["TAILLEPOLICE"],
                FontFamily = mainWindow.policeEcriturePersonalisee,
                Foreground = System.Windows.Media.Brushes.Black
            };

            Canvas.SetLeft(afficherToucheAPressser, INFOSLABELS["POSXLABELAFFICHERTOUCHEAPRESSER"]);
            Canvas.SetTop(afficherToucheAPressser, INFOSLABELS["POSY"]);
            mainWindow.CanvasPrincipal.Children.Add(afficherToucheAPressser);

            baton = new Rectangle
            {
                Width = INFOSBATON["LARGEUR"],
                Height = INFOSBATON["HAUTEUR"],
                RadiusX = INFOSBATON["ARRONDISSEMENTBORDS"],
                RadiusY = INFOSBATON["ARRONDISSEMENTBORDS"],
                Stroke = Brushes.Black,
                Fill = Brushes.Brown,
                RenderTransformOrigin = MILIEUBASBATON
            };

            baton.RenderTransform = new RotateTransform(angleActuelBaton); //appliquer une rotation initiale au bâton

            Canvas.SetLeft(baton, POSBATON[0]);
            Canvas.SetTop(baton, POSBATON[1]);
            mainWindow.CanvasPrincipal.Children.Add(baton);

            ligne = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = EPAISSEURFIL,
                X1 = mainWindow.Width/2,
                Y1 = POSLIGNEY
            };
            mainWindow.CanvasPrincipal.Children.Add(ligne);

            pignata = new Image
            {
                Width = TAILLEPIGNATA,
                Height = TAILLEPIGNATA,
                Source = imagePignata,
            };
            mainWindow.CanvasPrincipal.Children.Add(pignata);

            mainWindow.tempsRestant = DUREEMINIJEU;

            nbBonbonsGagnesDansMiniJeu = random.Next(NBONBONSMINGAGNE, NBONBONSMAXGAGNE + 1);
            gagne = false;
            index = 0;
            indexImagePignataDesintegration = 0;
            compteur = VALEURMAXCOMPTEUR;

            GenererSuiteTouchesAleatoire();

            mainWindow.timer.Start();
            mainWindow.chronometre.Start();
        }

        private void GenererSuiteTouchesAleatoire()
        {
            Key toucheTemporaire;
            suiteTouchesAPresser = new Key[NBTOUCHESAPRESSER];

            for (int i = 0; i < suiteTouchesAPresser.Length; i++)
            {
                toucheTemporaire = TOUCHESPIGNATA[random.Next(TOUCHESPIGNATA.Length)];

                while (i > 0 && toucheTemporaire == suiteTouchesAPresser[i - 1])
                    toucheTemporaire = TOUCHESPIGNATA[random.Next(TOUCHESPIGNATA.Length)];
  
                suiteTouchesAPresser[i] = toucheTemporaire;
            }
            MettreAJourLabelAfficherToucheAPressser();
        }

        public void Jouer()
        {
            double x, y;

            labelAfficheChrono.Content = mainWindow.tempsRestant;

            if (gagne)
            {
                pignata.Source = animationsBonbonPignataExplose[indexImagePignataDesintegration];

                if (compteur <= 0)
                    indexImagePignataDesintegration = Math.Min(indexImagePignataDesintegration + 1, animationsBonbonPignataExplose.Length - 1);

                compteur--;
                if (compteur < 0)
                    compteur = VALEURMAXCOMPTEUR;

                if (!animationBatonTimer.IsEnabled && indexImagePignataDesintegration >= animationsBonbonPignataExplose.Length - 1)
                {
                    mainWindow.ecranIntermediaire.InitEcranIntermediaire(nbBonbonsGagnesDansMiniJeu, true);
                    return;
                }
            }
            else if (mainWindow.tempsRestant <= 0)
            {
                mainWindow.ecranIntermediaire.InitEcranIntermediaire(0, false);
                return;
            }

            vitesseAngulaire += -(GRAVITE / LONGUEURFIL) * Math.Sin(angle);
            vitesseAngulaire *= AMORTISSEMENTROTATION;
            angle += vitesseAngulaire;

            x = (mainWindow.Width/2) + LONGUEURFIL * Math.Sin(angle);
            y = POSLIGNEY + LONGUEURFIL * Math.Cos(angle);

            ligne.X2 = x;
            ligne.Y2 = y;

            Canvas.SetLeft(pignata, x - pignata.Width/2);
            Canvas.SetTop(pignata, y - pignata.Height/2);
        }

        private void MettreAJourLabelAfficherToucheAPressser()
        {
            if (index >= NBTOUCHESAPRESSER - 1)
                afficherToucheAPressser.Content = "\n-";

            else if (suiteTouchesAPresser[index] == Key.Left)
                afficherToucheAPressser.Content = "\n←";

            else if (suiteTouchesAPresser[index] == Key.Right)
                afficherToucheAPressser.Content = "\n→";

            else if (suiteTouchesAPresser[index] == Key.Up)
                afficherToucheAPressser.Content = "\n↑";

            else if (suiteTouchesAPresser[index] == Key.Down)
                afficherToucheAPressser.Content = "\n↓";
            else
                afficherToucheAPressser.Content = $"\n{suiteTouchesAPresser[index]}";
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == suiteTouchesAPresser[index])
            {
                if (!gagne)
                {
                    directionBaton = DROITE;

                    if (!animationBatonTimer.IsEnabled)
                        animationBatonTimer.Start();
                }

                index = Math.Min(index+1, NBTOUCHESAPRESSER-1); 
                MettreAJourLabelAfficherToucheAPressser();
            }
        }

        public void OnKeyUp(object sender, KeyEventArgs e) { }

        private void animerBaton(object sender, EventArgs e)
        {
            if (baton.RenderTransform is RotateTransform rotateTransform)
            {
                if (directionBaton == DROITE)
                {
                    vitesseBaton += ACCELERATIONVITESSEFRAPPE;
                    angleActuelBaton += vitesseBaton;

                    forceImpactBatonPignata = Math.Min(forceImpactBatonPignata + AUGMENTATIONFORCEIMPACTEBATONPIGNATA, MAXIMPACTFORCE);

                    if (Collision(pignata, baton))
                    {
                        vitesseAngulaire = forceImpactBatonPignata;

                        if (index >= NBTOUCHESAPRESSER-1)
                            gagne = true;
                    }

                    if (angleActuelBaton >= ANGLEROTATIONBATON)
                    {
                        forceImpactBatonPignata = 0;
                        directionBaton = GAUCHE;
                        vitesseBaton = MINVITESSEBATON;
                    }
                }
                else
                {
                    angleActuelBaton -= VITESSERETOURBATON;

                    if (angleActuelBaton <= -ANGLEROTATIONBATON)
                        animationBatonTimer.Stop();
                }
                rotateTransform.Angle = angleActuelBaton;
            }
        }

        // Calcule le rectangle englobant d'un rectangle après application d'une rotation.
        private Rect RectangleApresRotation(Rectangle rect)
        {
            // Crée un rectangle de collision initial basé sur la position du rectangle sur le Canvas.
            Rect rectangleCollision = new Rect(Canvas.GetLeft(rect), Canvas.GetTop(rect), TAILLERECTANGLECOLLISION, TAILLERECTANGLECOLLISION);

            // Déclaration des quatre coins du rectangle après rotation.
            Point topLeft, topRight, bottomLeft, bottomRight;

            // Variables pour stocker les coordonnées minimales et maximales après transformation.
            double minX, maxX, minY, maxY;

            // Vérifie si une transformation de rotation est appliquée au rectangle.
            if (rect.RenderTransform is RotateTransform rotateTransform)
            {
                // Définis une nouveau centre de rotation
                RotateTransform rotation = new RotateTransform(rotateTransform.Angle, Canvas.GetLeft(rect) + rect.RenderTransformOrigin.X * rect.Width, Canvas.GetTop(rect) + rect.RenderTransformOrigin.Y * rect.Height);

                // Applique la rotation aux quatre coins du rectangle de collision.
                topLeft = rotation.Transform(new Point(rectangleCollision.Left, rectangleCollision.Top));
                topRight = rotation.Transform(new Point(rectangleCollision.Right, rectangleCollision.Top));
                bottomLeft = rotation.Transform(new Point(rectangleCollision.Left, rectangleCollision.Bottom));
                bottomRight = rotation.Transform(new Point(rectangleCollision.Right, rectangleCollision.Bottom));

                // Calcule la plus petite/grande valeur de X parmi les quatre points transformés.
                //minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
                //maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));

                // Calcule la plus petite/grande valeur de Y parmi les quatre points transformés.
                minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
                maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

                // Retourne un rectangle qui contient tous les points après la rotation.
                return new Rect(topLeft.X, topLeft.Y, topRight.X - topLeft.X, maxY - minY); //Rect(minX, minY, maxX - minX, maxY - minY);
            }

            // Si aucune transformation de rotation n'est appliquée, retourne le rectangle de collision initial.
            return rectangleCollision;
        }

        private bool Collision(Image objA, Rectangle objB)
        {
            Rect rectA = new Rect(Canvas.GetLeft(objA), Canvas.GetTop(objA), objA.Width, objA.Width);
            Rect rectB = RectangleApresRotation(objB); //retourne un rectangle qui tient compte de la rotation appliquée à ObjB

            return rectA.IntersectsWith(rectB);
        }
    }
}
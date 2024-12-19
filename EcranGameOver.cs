using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace CompilationMiniJeux
{
    public class EcranGameOver
    {
        public static readonly int DECALAGEX = 280, WIDTHBTN=100 , HEIGHTBTN=50, TAILLEMESSAGE=48;

        private MainWindow mainWindow;
        private Label labelGameOver;
        private Button btnRejouer;
        private Button btnQuitter;

        private int attenteAvantDecrementer, attenteentredecrementations;

        public EcranGameOver(MainWindow fenetreParent)
        {
            mainWindow = fenetreParent;
        }

        public void InitEcranFin()
        {
            mainWindow.CanvasPrincipal.Children.Clear();

            mainWindow.timer.Stop();
            mainWindow.chronometre.Stop();

            Rectangle rectangleRouge = new Rectangle
            {
                Fill = Brushes.Red,
                Width = mainWindow.CanvasPrincipal.ActualWidth,
                Height = mainWindow.CanvasPrincipal.ActualHeight
            };
            mainWindow.CanvasPrincipal.Children.Add(rectangleRouge);

            labelGameOver = new Label();
            labelGameOver.Content = "Partie terminée !";
            labelGameOver.FontFamily = mainWindow.policeEcriturePersonalisee;
            labelGameOver.Foreground = Brushes.White;
            labelGameOver.FontSize = TAILLEMESSAGE;
            Canvas.SetLeft(labelGameOver, (mainWindow.CanvasPrincipal.ActualWidth)/2 - DECALAGEX);
            Canvas.SetTop(labelGameOver, mainWindow.CanvasPrincipal.ActualHeight / 10);

            btnRejouer = new Button();
            btnRejouer.Width = WIDTHBTN;
            btnRejouer.Height = HEIGHTBTN;
            btnRejouer.Content = "Rejouer";
            btnRejouer.Click += Rejouer_OnClick;
            Canvas.SetLeft(btnRejouer, (mainWindow.CanvasPrincipal.ActualWidth - btnRejouer.ActualWidth) / 2);
            Canvas.SetTop(btnRejouer, 5 * mainWindow.CanvasPrincipal.ActualHeight / 10);

            btnQuitter = new Button();
            btnQuitter.Width = WIDTHBTN;
            btnQuitter.Height = HEIGHTBTN;
            btnQuitter.Content = "Quitter";
            btnQuitter.Click += Quitter_OnClick;
            Canvas.SetLeft(btnQuitter, (mainWindow.CanvasPrincipal.ActualWidth - btnRejouer.ActualWidth) / 2);
            Canvas.SetTop(btnQuitter, 6 * mainWindow.CanvasPrincipal.ActualHeight / 10);

            mainWindow.CanvasPrincipal.Children.Add(labelGameOver);
            mainWindow.CanvasPrincipal.Children.Add(btnRejouer);
            mainWindow.CanvasPrincipal.Children.Add(btnQuitter);
        }

        public void Rejouer_OnClick(object sender, RoutedEventArgs e)
        {
            mainWindow.ResetJeu();
        }
        public void Quitter_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
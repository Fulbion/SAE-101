using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media.Imaging;

namespace CompilationMiniJeux
{
    public class EcranTitre
    {
        private MainWindow mainWindow;
        private Label labelTitre;
        private Button buttonJouer;
        private Button buttonParam;
        private Button buttonQuitter;
        public bool active = true;

        public double volume = 0.5;

        public static readonly double MAXVAL = 10, MINVAL = 0;

        public EcranTitre(MainWindow parent)
        {
            mainWindow = parent;

            Image backgroundImage = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/img/fondecranaccueil.png")),
                Stretch = Stretch.Fill
            };
            mainWindow.CanvasPrincipal.Children.Add(backgroundImage);

            labelTitre = new Label
            {
                Content = "Bonbon Mania",
                FontSize = 40,
                FontFamily = mainWindow.policeEcriturePersonalisee
            };
            mainWindow.CanvasPrincipal.Children.Add(labelTitre);
            labelTitre.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(labelTitre, (mainWindow.ActualWidth / 2) - (labelTitre.DesiredSize.Width / 2));

            buttonJouer = new Button{
                Content = "Jouer",
                Width = 200,
                Height = 48,
                FontFamily = mainWindow.policeEcriturePersonalisee,
                FontSize = 15
            };

            buttonJouer.Click += Jouer_Click;
            mainWindow.CanvasPrincipal.Children.Add(buttonJouer);
            Canvas.SetLeft(buttonJouer, (mainWindow.Width / 2) - (buttonJouer.Width / 2));
            Canvas.SetTop(buttonJouer, 100);

            buttonParam = new Button
            {
                Content = "Paramètres",
                Width = 200,
                Height = 48,
                FontFamily = mainWindow.policeEcriturePersonalisee,
                FontSize = 15
            };
            buttonParam.Click += Param_Click;
            mainWindow.CanvasPrincipal.Children.Add(buttonParam);
            Canvas.SetLeft(buttonParam, (mainWindow.Width / 2) - (buttonParam.Width / 2));
            Canvas.SetTop(buttonParam, 150);

            buttonQuitter = new Button
            {
                Content = "Quitter",
                Width = 200,
                Height = 48,
                FontFamily = mainWindow.policeEcriturePersonalisee,
                FontSize = 15
            };
            buttonQuitter.Click += Quitter_Click;
            mainWindow.CanvasPrincipal.Children.Add(buttonQuitter);
            Canvas.SetLeft(buttonQuitter, (mainWindow.Width / 2) - (buttonQuitter.Width / 2));
            Canvas.SetTop(buttonQuitter, 200);

        }

        private void Jouer_Click(object sender, EventArgs e)
        {
            active = false;
            mainWindow.ResetJeu();
        }

        private void Param_Click(object sender, EventArgs e)
        {
            Parametres parametres = new Parametres();
            parametres.ShowDialog();
            volume = parametres.volume;
        }

        private void Quitter_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

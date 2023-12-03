using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace Csharpagotchi
{
    // Componentes
    public class PositionComponent
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class VelocityComponent
    {
        public double SpeedX { get; set; }
        public double SpeedY { get; set; }
    }

    public class SpriteComponent
    {
        public Uri Sprite { get; set; }
        public UIElement UIElement { get; set; }
    }

    // Classe para o áudio
    public class AudioManager
    {
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();
        private readonly Random random = new Random();

        public void PlayRandomSound(string folderPath)
        {
            // Obtém a lista de arquivos de áudio na pasta
            string[] audioFiles = Directory.GetFiles(folderPath, "../assets/slime/sounds/*.wav");

            if (audioFiles.Length > 0)
            {
                // Escolhe aleatoriamente um arquivo de áudio
                int randomIndex = random.Next(audioFiles.Length);
                string randomAudioFile = audioFiles[randomIndex];

                // Define o caminho do arquivo de áudio no MediaPlayer
                mediaPlayer.Open(new Uri(randomAudioFile, UriKind.RelativeOrAbsolute));

                // Reproduz o áudio
                mediaPlayer.Play();
            }
            // Se não houver arquivos de áudio na pasta, não faz nada
        }
    }

    // Entidade
    public class Entity
    {
        public List<object> Components { get; set; } = new List<object>();
    }

    public class EntityFactory
    {
        public Entity CreateSlime(double x, double y)
        {
            Entity slime = new Entity();

            slime.Components.Add(new PositionComponent { X = x, Y = y });
            slime.Components.Add(new VelocityComponent { SpeedX = 0, SpeedY = 0 });
            slime.Components.Add(new SpriteComponent { Sprite = new Uri("../assets/slime/default.png", UriKind.Relative), UIElement = new Image() });

            return slime;
        }
    }   

    // Sistema
    public class MovementSystem
    {
        private readonly AudioManager audioManager = new AudioManager();
        
        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is PositionComponent) is PositionComponent positionComponent && entity.Components.FirstOrDefault(c => c is VelocityComponent) is VelocityComponent velocityComponent)
                {
                    // Atualiza a posição com base na velocidade
                    positionComponent.X += velocityComponent.SpeedX;
                    positionComponent.Y += velocityComponent.SpeedY;

                    // Tocar audio de movimento
                    audioManager.PlayRandomSound("../assets/slime/sounds/");

                    // Aqui você poderia adicionar lógica de colisão, limites da tela, etc.
                }
            }
        }
    }

    public class RenderingSystem
    {
        public void Start(List<Entity> entities, Canvas canvas)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is SpriteComponent) is SpriteComponent spriteComponent)
                {
                    // Adiciona o sprite ao canvas
                    canvas.Children.Add(spriteComponent.UIElement);
                }
            }
        }

        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is PositionComponent) is PositionComponent positionComponent && entity.Components.FirstOrDefault(c => c is SpriteComponent) is SpriteComponent spriteComponent)
                {
                    // Pega a posição do componente e atualiza a posição do sprite
                    Canvas.SetLeft(spriteComponent.UIElement, positionComponent.X);
                    Canvas.SetTop(spriteComponent.UIElement, positionComponent.Y);

                    // Aqui você poderia adicionar lógica de animação, etc.
                }
            }
        }
    }   

    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer gameTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Obtém as dimensões da tela principal
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // Define o tamanho da janela para ocupar toda a tela
            Width = screenWidth;
            Height = screenHeight;

            // Define a posição inicial da janela como centralizada
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Maximized;
            Topmost = true;

            // Inicializa o DispatcherTimer
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000 / 60) // Define o intervalo para aproximadamente 60 FPS
            };
            gameTimer.Tick += GameLoop; // Define o método a ser chamado a cada tick
            gameTimer.Start(); // Inicia o temporizador
        }

        private readonly List<Entity> entities = new List<Entity>(); // Lista para armazenar os slimes massas
        private readonly EntityFactory entityFactory = new EntityFactory(); // Fábrica de entidades

        private void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            // Obtém a posição atual do cursor do mouse
            Point mousePosition = e.GetPosition(this);

            // "Spawna" um Slime na posição do cursor do mouse
            entities.Add(entityFactory.CreateSlime(mousePosition.X, mousePosition.Y));
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Lógica do jogo - atualizações, movimentos, cálculos, etc.
            // Chamadas de métodos para atualizar a lógica do jogo

            // Por exemplo:
            AtualizarPosicoes();
            VerificarColisoes();

            // Atualização da interface gráfica, se necessário
            // Atualizações de elementos visuais com base nos estados do jogo

            // Por exemplo:
            AtualizarInterface();
        }

        // Métodos de exemplo para atualizar a lógica do jogo e a interface gráfica
        private void AtualizarPosicoes()
        {
            // Lógica para atualizar as posições dos elementos do jogo
        }

        private void VerificarColisoes()
        {
            // Lógica para verificar colisões entre objetos do jogo
        }

        private void AtualizarInterface()
        {
            // Lógica para atualizar a interface gráfica com base no estado do jogo
        }
    }
}

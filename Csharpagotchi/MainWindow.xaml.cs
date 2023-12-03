using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

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
        public double DistanceToChangeDirection { get; set; }
    }

    public class SpriteComponent
    {
        // Objeto contendo as Uri dos sprites do personagem parado e andando
        public Uri Sprite { get; set; }
        public Image Image { get; set; }
        public bool IsMoving { get; set; }
        public bool PreviousIsMoving { get; set; }
    }

    // Classe para o áudio
    public class AudioManager
    {
        public readonly MediaPlayer mediaPlayer = new MediaPlayer();
        private readonly Random random = new Random();

        private static string GetAbsolutePathFromRelativeUri(Uri relativeUri)
        {
            // Verificar se esse código está executando no Visual Studio ou em um executável
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Se estiver executando no Visual Studio, retorna o caminho absoluto com duas pastas acima
                return Path.GetFullPath("../../" + relativeUri.ToString());
            }
            else
            {
                // Se estiver não estiver executando no Visual Studio, retorna o caminho absoluto
                return Path.GetFullPath(relativeUri.ToString());
            }
        }

        public void PlayRandomSound(Uri folderPath)
        {
            // Obtém a lista de arquivos de áudio na pasta
            string[] audioFiles = Directory.GetFiles(GetAbsolutePathFromRelativeUri(folderPath), "*.wav");

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
        private static string GetAbsolutePathFromRelativeUri(string relativeUri)
        {
            // Verificar se esse código está executando no Visual Studio ou em um executável
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Se estiver executando no Visual Studio, retorna o caminho absoluto com duas pastas acima
                return Path.GetFullPath("../../" + relativeUri);
            }
            else
            {
                // Se estiver não estiver executando no Visual Studio, retorna o caminho absoluto
                return Path.GetFullPath(relativeUri);
            }
        }

        public Entity CreateSlime(double x, double y)
        {
            Entity slime = new Entity();

            slime.Components.Add(new PositionComponent { X = x, Y = y });
            slime.Components.Add(new VelocityComponent { SpeedX = 0, SpeedY = 0 });
            slime.Components.Add(new SpriteComponent { Sprite = new Uri(GetAbsolutePathFromRelativeUri("./assets/slime/sprites/walk150.gif")) });

            return slime;
        }
    }

    // Sistema
    interface ISystem
    {
        void Start(List<Entity> entities, Canvas canvas);
        void Update(List<Entity> entities);
    }

    public class MovementSystem : ISystem
    {
        private readonly AudioManager audioManager = new AudioManager();
        private readonly Random random = new Random();
        private Canvas canvas;

        public void Start(List<Entity> entities, Canvas _canvas)
        {
            canvas = _canvas;
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is PositionComponent) is PositionComponent positionComponent)
                {
                    if (entity.Components.FirstOrDefault(c => c is VelocityComponent) is VelocityComponent velocityComponent)
                    {
                        // Define a velocidade inicial ao iniciar o jogo
                        velocityComponent.DistanceToChangeDirection = 0;
                        SetRandomVelocity(velocityComponent);
                    }
                }
            }
        }

        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is PositionComponent) is PositionComponent positionComponent && entity.Components.FirstOrDefault(c => c is VelocityComponent) is VelocityComponent velocityComponent)
                {
                    // Atualiza a posição com base na velocidade
                    positionComponent.X += velocityComponent.SpeedX;
                    positionComponent.Y += velocityComponent.SpeedY;

                    // Toca áudio aleatório de movimento
                    bool isPlaying = audioManager.mediaPlayer.Position < audioManager.mediaPlayer.NaturalDuration;
                    if (!isPlaying)
                    {
                        audioManager.PlayRandomSound(new Uri("./assets/slime/sounds/", UriKind.Relative));
                    }

                    // Verifica se o slime está prestes a sair da tela
                    CheckScreenBounds(positionComponent, velocityComponent, canvas);

                    // Atualiza a velocidade aleatoriamente
                    SetRandomVelocity(velocityComponent);

                    // Verificar se o slime está parado ou andando
                    if (velocityComponent.SpeedX == 0 && velocityComponent.SpeedY == 0)
                    {
                        ((SpriteComponent)entity.Components.FirstOrDefault(c => c is SpriteComponent)).IsMoving = false;
                    }
                    else
                    {
                        ((SpriteComponent)entity.Components.FirstOrDefault(c => c is SpriteComponent)).IsMoving = true;
                    }
                }
            }
        }

        private void CheckScreenBounds(PositionComponent positionComponent, VelocityComponent velocityComponent, Canvas canvas)
        {
            // Obtém as dimensões da tela
            double screenWidth = canvas.ActualWidth;
            double screenHeight = canvas.ActualHeight;

            // Obtém as dimensões do sprite
            double spriteWidth = 98;
            double spriteHeight = 98;

            // Verifica se o slime está fora dos limites da tela e ajusta a posição
            if (positionComponent.X < 0)
            {
                positionComponent.X = 0;
                velocityComponent.SpeedX = Math.Abs(velocityComponent.SpeedX); // Inverte a direção
            }
            else if (positionComponent.X + spriteWidth > screenWidth)
            {
                positionComponent.X = screenWidth - spriteWidth;
                velocityComponent.SpeedX = -Math.Abs(velocityComponent.SpeedX); // Inverte a direção
            }

            if (positionComponent.Y < 0)
            {
                positionComponent.Y = 0;
                velocityComponent.SpeedY = Math.Abs(velocityComponent.SpeedY); // Inverte a direção
            }
            else if (positionComponent.Y + spriteHeight > screenHeight)
            {
                positionComponent.Y = screenHeight - spriteHeight;
                velocityComponent.SpeedY = -Math.Abs(velocityComponent.SpeedY); // Inverte a direção
            }
        }

        private void SetRandomVelocity(VelocityComponent velocityComponent)
        {
            if (velocityComponent.DistanceToChangeDirection <= 0)
            {
                // Gera velocidades aleatórias
                velocityComponent.SpeedX = random.NextDouble() * 4 - 1; // Valor entre -1 e 1
                velocityComponent.SpeedY = random.NextDouble() * 4 - 1; // Valor entre -1 e 1

                // Gera distância aleatória para mudar a direção
                velocityComponent.DistanceToChangeDirection = random.NextDouble() * 1000;
            }
            else
            {
                velocityComponent.DistanceToChangeDirection -= Math.Abs(velocityComponent.SpeedX) < Math.Abs(velocityComponent.SpeedY) ? Math.Abs(velocityComponent.SpeedY) : Math.Abs(velocityComponent.SpeedX);
            }
        }

    }

    public class RenderingSystem : ISystem
    {
        private static string GetAbsolutePathFromRelativeUri(string relativeUri)
        {
            // Verificar se esse código está executando no Visual Studio ou em um executável
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Se estiver executando no Visual Studio, retorna o caminho absoluto com duas pastas acima
                return Path.GetFullPath("../../" + relativeUri);
            }
            else
            {
                // Se estiver não estiver executando no Visual Studio, retorna o caminho absoluto
                return Path.GetFullPath(relativeUri);
            }
        }

        public void Start(List<Entity> entities, Canvas canvas)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is SpriteComponent) is SpriteComponent spriteComponent)
                {
                    // Adiciona o sprite ao canvas
                    spriteComponent.Image = new Image
                    {
                        Source = new BitmapImage(spriteComponent.Sprite),
                        Height = 98,
                        Width = 98
                    };
                    canvas.Children.Add(spriteComponent.Image);
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
                    Canvas.SetLeft(spriteComponent.Image, positionComponent.X);
                    Canvas.SetTop(spriteComponent.Image, positionComponent.Y);

                    // Verifica se o estado de movimento mudou
                    if (spriteComponent.IsMoving != spriteComponent.PreviousIsMoving)
                    {
                        // Se mudou, atualiza o estado anterior
                        spriteComponent.PreviousIsMoving = spriteComponent.IsMoving;

                        // Verifica se o slime está parado ou andando
                        if (spriteComponent.IsMoving)
                        {
                            Console.WriteLine("Andando");
                            BitmapImage image = new BitmapImage(); // Cria um objeto BitmapImage
                            image.BeginInit();
                            image.UriSource = new Uri(GetAbsolutePathFromRelativeUri("./assets/slime/sprites/walk150.gif"), UriKind.Absolute);
                            image.EndInit();
                            ImageBehavior.SetAnimatedSource(spriteComponent.Image, image);
                            ImageBehavior.SetRepeatBehavior(spriteComponent.Image, RepeatBehavior.Forever);
                        }
                        else
                        {
                            Console.WriteLine("Parado");
                            // Se o slime estiver andando, define o sprite andando
                            spriteComponent.Sprite = new Uri("./assets/slime/sprites/walk150.gif", UriKind.Relative);
                        }
                    }

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

            // Inicializa o jogo
            StartGame();
        }

        // Lista de entidades do jogo
        private readonly List<Entity> entities = new List<Entity>(); // Lista para armazenar os slimes massas
        private readonly EntityFactory entityFactory = new EntityFactory(); // Fábrica de entidades

        // Lista de sistemas do jogo
        private readonly List<object> systems = new List<object>(); // Lista para armazenar os sistemas do jogo

        // Método inicial para o Game
        private void StartGame()
        {
            // Cria um Slime na posição (0, 0)
            entities.Add(entityFactory.CreateSlime(0, 0));

            // Inicializa os sistemas
            MovementSystem movementSystem = new MovementSystem();
            RenderingSystem renderingSystem = new RenderingSystem();

            // Adiciona os sistemas à lista de sistemas
            systems.Add(movementSystem);
            systems.Add(renderingSystem);

            // Inicializa os sistemas
            systems.ForEach(system => system.GetType().GetMethod("Start").Invoke(system, new object[] { entities, (Canvas)FindName("Game") }));
        }

        // Método chamado a cada tick do DispatcherTimer
        private void GameLoop(object sender, EventArgs e)
        {
            // Printar FPS no terminal
            //Console.WriteLine("FPS: " + 1000 / gameTimer.Interval.TotalMilliseconds);

            // Lógica do jogo - atualizações, movimentos, cálculos, etc.
            // Chamadas de métodos para atualizar a lógica do jogo
            systems.ForEach(system => system.GetType().GetMethod("Update").Invoke(system, new object[] { entities }));
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

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

    public class MovementComponent
    {
        public Vector Position { get; set; } = new Vector(0, 0);
        public Vector Speed { get; set; } = new Vector(0, 0);
        public double DistanceToChangeDirection { get; set; }
        public int SecondsToChangeDirection { get; set; }
        public bool IsMoving { get; set; } = false;
    }

    public class SpriteComponent
    {
        // Objeto contendo as Uri dos sprites do personagem parado e andando
        public Uri IdleSprite { get; set; }
        public BitmapImage IdleSpriteBitmap { get; set; }
        public Uri WalkSprite { get; set; }
        public BitmapImage WalkSpriteBitmap { get; set; }
        public Image Image { get; set; }
        public bool PreviousIsMoving { get; set; } = false;
    }

    public class AudioComponent
    {
        public string WalkingAudioFolder { get; set; }
        public MediaPlayer MediaPlayer { get; set; }
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

            Console.WriteLine(GetAbsolutePathFromRelativeUri("./assets/slime/sounds/walking"));
            slime.Components.Add(new MovementComponent { Position = new Vector(x, y) });
            slime.Components.Add(new SpriteComponent { IdleSprite = new Uri(GetAbsolutePathFromRelativeUri("./assets/slime/sprites/walk150.gif")), WalkSprite = new Uri(GetAbsolutePathFromRelativeUri("./assets/slime/sprites/walk150.gif")) });
            slime.Components.Add(new AudioComponent { WalkingAudioFolder = GetAbsolutePathFromRelativeUri("./assets/slime/sounds/walking") });

            return slime;
        }
    }

    // Sistema
    interface ISystem
    {
        void Start(List<Entity> entities, Canvas canvas);
        void Update(List<Entity> entities);
        void UpdatePerSecond(List<Entity> entities);
    }

    public class AudioSystem : ISystem
    {
        private readonly Random random = new Random();

        private void PlayRandomSound(string folderPath, MediaPlayer mediaPlayer)
        {
            // Obtém a lista de arquivos de áudio na pasta
            string[] audioFiles = Directory.GetFiles(folderPath, "*.wav");

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

        public void Start(List<Entity> entities, Canvas canvas)
        {
            // Criar um MediaPlayer para cada entidade que possuir um AudioComponent
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is AudioComponent) is AudioComponent audioComponent)
                {
                    audioComponent.MediaPlayer = new MediaPlayer();
                }
            }
        }

        public void Update(List<Entity> entities)
        {
            // Lógica de atualização, se necessário
        }

        public void UpdatePerSecond(List<Entity> entities)
        {
            // Tocar um som de movimento aleatório para cada entidade que possuir um AudioComponent e estiver se movendo
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is AudioComponent) is AudioComponent audioComponent && entity.Components.FirstOrDefault(c => c is MovementComponent) is MovementComponent movementComponent && movementComponent.IsMoving)
                {
                    if (audioComponent.MediaPlayer != null && movementComponent.IsMoving)
                    {
                        PlayRandomSound(audioComponent.WalkingAudioFolder, audioComponent.MediaPlayer);
                    }
                }
            }

        }
    }

    public class MovementSystem : ISystem
    {
        private readonly Random random = new Random();
        private Canvas canvas;

        public void Start(List<Entity> entities, Canvas _canvas)
        {
            canvas = _canvas;
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is MovementComponent) is MovementComponent movementComponent)
                {
                    // Define a velocidade inicial ao iniciar o jogo
                    movementComponent.DistanceToChangeDirection = 0;
                    movementComponent.SecondsToChangeDirection = 0;
                    SetRandomMovement(movementComponent);
                }
            }
        }

        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is MovementComponent) is MovementComponent movementComponent)
                {
                    // Atualiza a posição com base na velocidade
                    if (movementComponent.IsMoving) {
                        movementComponent.Position += movementComponent.Speed;
                    }

                    // Verifica se o slime está prestes a sair da tela
                    CheckScreenBounds(movementComponent, canvas);

                    // Atualiza a velocidade aleatoriamente
                    SetRandomMovement(movementComponent);
                }
            }
        }

        public void UpdatePerSecond(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is MovementComponent) is MovementComponent movementComponent && movementComponent.DistanceToChangeDirection <= 0 && movementComponent.SecondsToChangeDirection > 0)
                {
                    // Diminui o tempo para mudar a direção
                    movementComponent.SecondsToChangeDirection--;
                }
            }
        }

        private void CheckScreenBounds(MovementComponent movementComponent, Canvas canvas)
        {
            // Obtém as dimensões da tela
            double screenWidth = canvas.ActualWidth;
            double screenHeight = canvas.ActualHeight;

            // Obtém as dimensões do sprite
            double spriteWidth = 98;
            double spriteHeight = 98;

            // Verifica se o slime está fora dos limites da tela e ajusta a posição
            if (movementComponent.Position.X < 0)
            {
                movementComponent.Position = new Vector(0, movementComponent.Position.Y);
                movementComponent.Speed = new Vector(Math.Abs(movementComponent.Speed.X), movementComponent.Speed.Y); // Inverte a direção
            }
            else if (movementComponent.Position.X + spriteWidth > screenWidth)
            {
                movementComponent.Position = new Vector(screenWidth - spriteWidth, movementComponent.Position.Y);
                movementComponent.Speed = new Vector(-Math.Abs(movementComponent.Speed.X), movementComponent.Speed.Y); // Inverte a direção
            }

            if (movementComponent.Position.Y < 0)
            {
                movementComponent.Position = new Vector(movementComponent.Position.X, 0);
                movementComponent.Speed = new Vector(movementComponent.Speed.X, Math.Abs(movementComponent.Speed.Y)); // Inverte a direção
            }
            else if (movementComponent.Position.Y + spriteHeight > screenHeight)
            {
                movementComponent.Position = new Vector(movementComponent.Position.X, screenHeight - spriteHeight);
                movementComponent.Speed = new Vector(movementComponent.Speed.X, -Math.Abs(movementComponent.Speed.Y)); // Inverte a direção
            }
        }

        private void SetRandomMovement(MovementComponent movementComponent)
    {
            if (movementComponent.DistanceToChangeDirection <= 0 && movementComponent.SecondsToChangeDirection <= 0)
            {
                // Gera velocidades aleatórias
                movementComponent.Speed = new Vector(RandomizeSign(random.Next(1, 3)), RandomizeSign(random.Next(1, 3)));

                // Gera distância aleatória para mudar a direção
                movementComponent.DistanceToChangeDirection = random.Next(100, 600);
                movementComponent.SecondsToChangeDirection = random.Next(10, 15);
            }
            else if (movementComponent.SecondsToChangeDirection > 0 && movementComponent.DistanceToChangeDirection > 0)
            {
                movementComponent.IsMoving = true;
                movementComponent.DistanceToChangeDirection -= Math.Abs(movementComponent.Speed.X) < Math.Abs(movementComponent.Speed.Y) ? Math.Abs(movementComponent.Speed.Y) : Math.Abs(movementComponent.Speed.X);
            }
            else
            {
                movementComponent.IsMoving = false;
            }
        }

        // Receber número e aleatorizar se é negativo ou positivo
        private int RandomizeSign(int number)
        {
            return random.Next(2) == 0 ? Math.Abs(number) : -Math.Abs(number);
        }
    }

    public class RenderingSystem : ISystem
    {
        public void Start(List<Entity> entities, Canvas canvas)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is SpriteComponent) is SpriteComponent spriteComponent)
                {
                    // Define os BitmapImage dos sprites
                    spriteComponent.IdleSpriteBitmap = new BitmapImage(spriteComponent.IdleSprite);
                    BitmapImage walkingBitmap = new BitmapImage();
                    walkingBitmap.BeginInit();
                    walkingBitmap.UriSource = spriteComponent.WalkSprite;
                    walkingBitmap.EndInit();
                    spriteComponent.WalkSpriteBitmap = walkingBitmap;

                    // Adiciona o sprite ao canvas
                    spriteComponent.Image = new Image
                    {
                        Source = spriteComponent.IdleSpriteBitmap,
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
                if (entity.Components.FirstOrDefault(c => c is MovementComponent) is MovementComponent movementComponent && entity.Components.FirstOrDefault(c => c is SpriteComponent) is SpriteComponent spriteComponent)
                {
                    // Pega a posição do componente e atualiza a posição do sprite
                    Canvas.SetLeft(spriteComponent.Image, movementComponent.Position.X);
                    Canvas.SetTop(spriteComponent.Image, movementComponent.Position.Y);

                    // Verifica se o estado de movimento mudou
                    if (movementComponent.IsMoving != spriteComponent.PreviousIsMoving)
                    {
                        // Se mudou, atualiza o estado anterior
                        spriteComponent.PreviousIsMoving = movementComponent.IsMoving;

                        // Verifica se o slime está parado ou andando
                        if (movementComponent.IsMoving)
                        {
                            ImageBehavior.SetAutoStart(spriteComponent.Image, true);
                            ImageBehavior.SetAnimatedSource(spriteComponent.Image, spriteComponent.WalkSpriteBitmap);
                        }
                        else
                        {
                            ImageBehavior.SetAutoStart(spriteComponent.Image, false);
                            ImageBehavior.SetAnimatedSource(spriteComponent.Image, spriteComponent.IdleSpriteBitmap);
                        }
                        ImageBehavior.SetRepeatBehavior(spriteComponent.Image, RepeatBehavior.Forever);
                    }

                    // Aqui você poderia adicionar lógica de animação, etc.
                }
            }
        }

        public void UpdatePerSecond(List<Entity> entities)
        {
            // Lógica para atualizar a lógica do jogo por segundo
        }
    }

    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer gameTimer;
        private readonly DispatcherTimer gameTimerSeconds;

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

            // Inicializa o DispatcherTimer de segundos
            gameTimerSeconds = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000) // Define o intervalo para 1 segundo
            };

            gameTimerSeconds.Tick += GameLoopPerSecond; // Define o método a ser chamado a cada tick
            gameTimerSeconds.Start(); // Inicia o temporizador

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
            AudioSystem audioSystem = new AudioSystem();

            // Adiciona os sistemas à lista de sistemas
            systems.Add(movementSystem);
            systems.Add(renderingSystem);
            systems.Add(audioSystem);

            // Inicializa os sistemas
            systems.ForEach(system => system.GetType().GetMethod("Start").Invoke(system, new object[] { entities, (Canvas)FindName("Game") }));
        }

        // Método chamado a cada tick do DispatcherTimer de segundos
        private void GameLoopPerSecond(object sender, EventArgs e)
        {
            // Lógica do jogo - atualizações, movimentos, cálculos, etc.
            // Chamadas de métodos para atualizar a lógica do jogo
            systems.ForEach(system => system.GetType().GetMethod("UpdatePerSecond").Invoke(system, new object[] { entities }));
        }

        // Método chamado a cada tick do DispatcherTimer
        private void GameLoop(object sender, EventArgs e)
        {
            // Lógica do jogo - atualizações, movimentos, cálculos, etc.
            // Chamadas de métodos para atualizar a lógica do jogo
            systems.ForEach(system => system.GetType().GetMethod("Update").Invoke(system, new object[] { entities }));
        }
    }
}

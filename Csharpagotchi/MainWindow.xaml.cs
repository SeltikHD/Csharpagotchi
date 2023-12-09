using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public bool AutoWalk { get; set; } = false;
        public double Desacceleration { get; set; } = 0.0;
        public bool Enabled { get; set; } = true;
    }

    public class PhysicsComponent
    {
        // Objeto contendo as propriedades físicas do personagem
        public bool Enabled { get; set; } = false;
        public double Direction { get; set; } // Ângulo em graus
        public double Smoothness { get; set; } = 0.2;
        public double SmoothDirection { get; set; } = 0;
        public Vector Deslocation { get; set; } = new Vector(0, 0);
        public double TimeInDeslocation { get; set; } = 0;
    }

    public class InputComponent
    {
        // Objeto contendo as propriedades de input do personagem
        public bool IsGrabbing { get; set; } = false;
        public bool IsDraggable { get; set; } = false;
        public Vector LastMousePosition { get; set; } = new Vector(0, 0);
    }

    public class SpriteComponent
    {
        // Objeto contendo as Uri dos sprites do personagem parado e andando
        public int Width { get; set; }
        public int Height { get; set; }
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

            slime.Components.Add(new MovementComponent { Position = new Vector(x, y), AutoWalk = true });
            slime.Components.Add(new SpriteComponent { Width = 98, Height = 98, IdleSprite = new Uri(GetAbsolutePathFromRelativeUri("./assets/slime/sprites/walk150.gif")), WalkSprite = new Uri(GetAbsolutePathFromRelativeUri("./assets/slime/sprites/walk150.gif")) });
            slime.Components.Add(new AudioComponent { WalkingAudioFolder = GetAbsolutePathFromRelativeUri("./assets/slime/sounds/walking") });
            slime.Components.Add(new PhysicsComponent());
            slime.Components.Add(new InputComponent{ IsDraggable = true});

            return slime;
        }
    }

    // Sistema
    interface ISystem
    {
        // Entidades; Canvas; Função para setar o cursor
        void Start(List<Entity> entities, Canvas canvas);
        void Update(List<Entity> entities);
        void UpdatePerSecond(List<Entity> entities);
    }

    // Sistema de física
    public class PhysicsSystem : ISystem
    {
        private Vector lastMousePosition = new Vector(0, 0);

        public void Start(List<Entity> entities, Canvas canvas)
        {
            // Lógica de inicialização, se necessário
        }

        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.Find(c => c is PhysicsComponent) is PhysicsComponent physicsComponent)
                {
                    // Atualiza o SmoothDirection
                    physicsComponent.SmoothDirection += (physicsComponent.Direction - physicsComponent.SmoothDirection) * physicsComponent.Smoothness;

                    // Verifica se o componente está habilitado
                    if (physicsComponent.Enabled && entity.Components.Find(c => c is InputComponent) is InputComponent inputComponent)
                    {
                        if (inputComponent.IsGrabbing)
                        {
                            // Verifica se a posição do mouse é 0, 0 e se o vetor está definido
                            if (lastMousePosition == new Vector(0, 0))
                            {
                                lastMousePosition = inputComponent.LastMousePosition;
                            }

                            // Verifica se a posição do mouse mudou e se os vetores estão definidos e maior que 0
                            if (inputComponent.LastMousePosition != lastMousePosition && lastMousePosition != new Vector(0, 0) && inputComponent.LastMousePosition != new Vector(0, 0))
                            {
                                // Calcula o deslocamento
                                physicsComponent.Deslocation += new Vector(inputComponent.LastMousePosition.X - lastMousePosition.X, inputComponent.LastMousePosition.Y - lastMousePosition.Y);
                            }

                            physicsComponent.TimeInDeslocation += 1 / 60.0;
                            lastMousePosition = inputComponent.LastMousePosition;
                        }
                        else if (entity.Components.Find(c => c is MovementComponent) is MovementComponent movementComponent)
                        {
                            // Verifica se o deslocamento é maior que 0
                            if (physicsComponent.Deslocation != new Vector(0, 0) && physicsComponent.TimeInDeslocation > 0)
                            {
                                // Calcula a velocidade
                                Vector speed = new Vector(physicsComponent.Deslocation.X / 2.5 / physicsComponent.TimeInDeslocation, physicsComponent.Deslocation.Y / 2.5 / physicsComponent.TimeInDeslocation);
                                int signX = speed.X > 0 ? 1 : -1;
                                int signY = speed.Y > 0 ? 1 : -1;
                                movementComponent.Speed = new Vector((Math.Abs(speed.X) > 5000 ? signX * 5000 : speed.X) / 60, (Math.Abs(speed.Y) > 5000 ? signY * 5000 : speed.Y) / 60);

                                Console.WriteLine("Speed: " + movementComponent.Speed);

                                // Zera o deslocamento e o tempo de deslocamento
                                physicsComponent.Deslocation = new Vector(0, 0);
                                physicsComponent.TimeInDeslocation = 0;
                                lastMousePosition = new Vector(0, 0);
                                inputComponent.LastMousePosition = new Vector(0, 0);
                            } else
                            {
                                // Define que o slime não está se movendo
                                movementComponent.AutoWalk = false;
                                movementComponent.IsMoving = true;
                                movementComponent.Desacceleration = 0.98;
                                physicsComponent.Enabled = false;
                            }
                        }
                    }
                }
            }
        }

        public void UpdatePerSecond(List<Entity> entities)
        {
            // Lógica de atualização por segundo, se necessário
        }
    }

    // Sistema de áudio
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
                if (entity.Components.Find(c => c is AudioComponent) is AudioComponent audioComponent)
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
                if (entity.Components.Find(c => c is AudioComponent) is AudioComponent audioComponent && entity.Components.Find(c => c is MovementComponent) is MovementComponent movementComponent && audioComponent.MediaPlayer != null && movementComponent.IsMoving)
                {
                    PlayRandomSound(audioComponent.WalkingAudioFolder, audioComponent.MediaPlayer);
                }
            }
        }
    }

    // Sistema de movimento
    public class MovementSystem : ISystem
    {
        private readonly Random random = new Random();
        private Canvas mainCanvas;

        public void Start(List<Entity> entities, Canvas canvas)
        {
            mainCanvas = canvas;
            foreach (var entity in entities)
            {
                if (entity.Components.Find(c => c is MovementComponent) is MovementComponent movementComponent)
                {
                    // Define a velocidade inicial ao iniciar o jogo
                    movementComponent.DistanceToChangeDirection = 0;
                    movementComponent.SecondsToChangeDirection = 0;

                    if (movementComponent.AutoWalk)
                    {
                        SetRandomMovement(movementComponent);
                    }
                }
            }
        }

        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.Find(c => c is MovementComponent) is MovementComponent movementComponent)
                {
                    SpriteComponent spriteComponent = (SpriteComponent) entity.Components.Find(c => c is SpriteComponent);

                    if (movementComponent.Enabled)
                    {
                        InputComponent inputComponent = (InputComponent) entity.Components.Find(c => c is InputComponent);

                        if (inputComponent.IsGrabbing)
                        {
                            if (spriteComponent != null)
                            {
                                movementComponent.Position = new Vector(inputComponent.LastMousePosition.X - spriteComponent.Width / 2, inputComponent.LastMousePosition.Y - spriteComponent.Height / 2);
                            }

                            movementComponent.IsMoving = false;
                        }
                        else
                        {
                            // Atualiza a posição com base na velocidade
                            if (movementComponent.IsMoving)
                            {
                                movementComponent.Position += movementComponent.Speed;
                            }

                            if (movementComponent.Desacceleration > 0.0)
                            {
                                movementComponent.Speed = new Vector(movementComponent.Speed.X * movementComponent.Desacceleration, movementComponent.Speed.Y * movementComponent.Desacceleration);
                            }

                            if (Math.Abs(movementComponent.Speed.X) < 2 && Math.Abs(movementComponent.Speed.Y) < 2 && !movementComponent.AutoWalk)
                            {
                                movementComponent.Desacceleration = 0;
                                movementComponent.AutoWalk = true;
                                movementComponent.DistanceToChangeDirection = 0;
                            }

                            if (movementComponent.AutoWalk)
                            {
                                // Atualiza a velocidade aleatoriamente
                                SetRandomMovement(movementComponent);
                            }
                        }
                    }
                    else
                    {
                        movementComponent.IsMoving = false;
                    }

                    if (spriteComponent != null)
                    {
                        // Verifica se o slime está prestes a sair da tela
                        CheckScreenBounds(movementComponent, spriteComponent);
                    }
                }
            }
        }

        public void UpdatePerSecond(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.Find(c => c is MovementComponent) is MovementComponent movementComponent && movementComponent.DistanceToChangeDirection <= 0 && movementComponent.SecondsToChangeDirection > 0)
                {
                    // Diminui o tempo para mudar a direção
                    movementComponent.SecondsToChangeDirection--;
                }
            }
        }

        private void CheckScreenBounds(MovementComponent movementComponent, SpriteComponent spriteComponent)
        {
            // Obtém as dimensões da tela
            double screenWidth = mainCanvas.ActualWidth;
            double screenHeight = mainCanvas.ActualHeight;

            // Verifica se o slime está fora dos limites da tela e ajusta a posição
            if (movementComponent.Position.X < 0)
            {
                movementComponent.Position = new Vector(0, movementComponent.Position.Y);
                movementComponent.Speed = new Vector(Math.Abs(movementComponent.Speed.X), movementComponent.Speed.Y); // Inverte a direção
            }
            else if (movementComponent.Position.X + spriteComponent.Width > screenWidth)
            {
                movementComponent.Position = new Vector(screenWidth - spriteComponent.Width, movementComponent.Position.Y);
                movementComponent.Speed = new Vector(-Math.Abs(movementComponent.Speed.X), movementComponent.Speed.Y); // Inverte a direção
            }

            if (movementComponent.Position.Y < 0)
            {
                movementComponent.Position = new Vector(movementComponent.Position.X, 0);
                movementComponent.Speed = new Vector(movementComponent.Speed.X, Math.Abs(movementComponent.Speed.Y)); // Inverte a direção
            }
            else if (movementComponent.Position.Y + spriteComponent.Height > screenHeight)
            {
                movementComponent.Position = new Vector(movementComponent.Position.X, screenHeight - spriteComponent.Height);
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

    // Sistema de input
    public class InputSystem : ISystem
    {
        private Vector lastMousePosition;

        public void Start(List<Entity> entities, Canvas canvas)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.Find(c => c is InputComponent) is InputComponent inputComponent)
                {
                    // Define o evento de movimento do mouse no canvas
                    canvas.MouseMove += (sender, e) =>
                    {
                        Vector actualPosition = (Vector)e.GetPosition(canvas);
                        lastMousePosition = actualPosition;

                        // Verifica se a entidade está sendo arrastada
                        if (e.LeftButton == MouseButtonState.Pressed && inputComponent.IsGrabbing && entity.Components.Find(c => c is PhysicsComponent) is PhysicsComponent physicsComponent)
                        {
                            // Define que a entidade está se movendo
                            if (entity.Components.Find(c => c is MovementComponent) is MovementComponent movementComponent)
                            {
                                movementComponent.IsMoving = true;
                            }

                            // Define a direção do slime
                            double scale = 100;
                            Vector direction = actualPosition - inputComponent.LastMousePosition;
                            physicsComponent.Direction = Math.Atan2(direction.X * scale, direction.Y * scale * - 1) * (180.0 / Math.PI);
                        }

                        // Obtém a posição do mouse
                        inputComponent.LastMousePosition = actualPosition;
                    };


                    if (entity.Components.Find(c => c is SpriteComponent) is SpriteComponent spriteComponent)
                    {
                        spriteComponent.Image.MouseMove += (sender, e) =>
                        {
                            // Verifica se a entidade está sendo arrastada
                            if (e.LeftButton == MouseButtonState.Pressed && inputComponent.IsDraggable)
                            {
                                if (entity.Components.Find(c => c is PhysicsComponent) is PhysicsComponent physicsComponent)
                                {
                                    physicsComponent.Enabled = true;
                                }

                                canvas.CaptureMouse();

                                Mouse.OverrideCursor = Cursors.SizeAll;
                                canvas.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));

                                inputComponent.IsGrabbing = true;
                            }
                            else
                            {
                                Mouse.OverrideCursor = Cursors.Hand;
                            }
                        };
                    }

                    canvas.MouseUp += (sender, e) =>
                    {
                        // Verifica se a entidade está sendo arrastada, se 
                        if (inputComponent.IsGrabbing)
                        {
                            if (entity.Components.Find(c => c is PhysicsComponent) is PhysicsComponent physicsComponent)
                            {
                                physicsComponent.Direction = 0;
                            }

                            inputComponent.IsGrabbing = false;
                   
                            canvas.ReleaseMouseCapture();
                            canvas.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                        }
                    };
                }
            }
        }

        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.Find(c => c is InputComponent) is InputComponent inputComponent && entity.Components.Find(c => c is SpriteComponent) is SpriteComponent spriteComponent && entity.Components.Find(c => c is PhysicsComponent) is PhysicsComponent physicsComponent)
                {
                    if (inputComponent.LastMousePosition == lastMousePosition && inputComponent.IsGrabbing)
                    {
                        physicsComponent.Direction = 0;

                        if (entity.Components.Find(c => c is MovementComponent) is MovementComponent movementComponent)
                        {
                            movementComponent.DistanceToChangeDirection = 0;
                        }
                    }

                    RotateTransform rotateTransform = new RotateTransform(physicsComponent.SmoothDirection, spriteComponent.Width / 2, spriteComponent.Height / 2);
                    spriteComponent.Image.RenderTransform = rotateTransform;
                }
            }
        }

        public void UpdatePerSecond(List<Entity> entities)
        {
            // Lógica de atualização por segundo, se necessário
        }
    }

    // Sistema de renderização
    public class RenderingSystem : ISystem
    {
        public void Start(List<Entity> entities, Canvas canvas)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.Find(c => c is SpriteComponent) is SpriteComponent spriteComponent)
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
                        Height = spriteComponent.Height,
                        Width = spriteComponent.Width
                    };

                    canvas.Children.Add(spriteComponent.Image);
                }
            }
        }

        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.Find(c => c is SpriteComponent) is SpriteComponent spriteComponent && entity.Components.Find(c => c is MovementComponent) is MovementComponent movementComponent)
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
                        if (movementComponent.IsMoving && movementComponent.AutoWalk)
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
                }
            }
        }

        public void UpdatePerSecond(List<Entity> entities)
        {
            // Lógica para atualizar a lógica do jogo por segundo
        }
    }

    /// <summary>
    /// Interação lógica para MainWindow.xaml
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
                Interval = TimeSpan.FromMilliseconds(1000.0 / 60) // Define o intervalo para aproximadamente 60 FPS
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
            PhysicsSystem physicsSystem = new PhysicsSystem();
            InputSystem inputSystem = new InputSystem();

            // Adiciona os sistemas à lista de sistemas
            systems.Add(movementSystem);
            systems.Add(renderingSystem);
            systems.Add(audioSystem);
            systems.Add(physicsSystem);
            systems.Add(inputSystem);

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

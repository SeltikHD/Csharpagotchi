using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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

    // Entidade
    public class Entity
    {
        public List<object> Components { get; set; } = new List<object>();
    }

    // Sistema
    public class MovementSystem
    {
        public void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Components.FirstOrDefault(c => c is PositionComponent) is PositionComponent positionComponent && entity.Components.FirstOrDefault(c => c is VelocityComponent) is VelocityComponent velocityComponent)
                {
                    // Atualiza a posição com base na velocidade
                    positionComponent.X += velocityComponent.SpeedX;
                    positionComponent.Y += velocityComponent.SpeedY;

                    // Aqui você poderia adicionar lógica de colisão, limites da tela, etc.
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

            // Inicializa o DispatcherTimer
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000 / 60) // Define o intervalo para aproximadamente 60 FPS
            };
            gameTimer.Tick += GameLoop; // Define o método a ser chamado a cada tick
            gameTimer.Start(); // Inicia o temporizador
        }

        private List<Entity> entities = new List<Entity>(); // Lista para armazenar os slimes massas

        // Método para criar e adicionar um Slime à lista de entidades
        private void SpawnSlime(double mouseX, double mouseY)
        {
            Entity slime = new Entity(); // Cria uma nova entidade (Slime)

            // Adiciona componentes à entidade
            slime.Components.Add(new PositionComponent { X = mouseX, Y = mouseY });
            slime.Components.Add(new VelocityComponent { SpeedX = 0, SpeedY = 0 });

            // Adiciona o Slime à lista de entidades
            entities.Add(slime);
        }

        private void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            // Obtém a posição atual do cursor do mouse
            Point mousePosition = e.GetPosition(this);

            // "Spawna" um Slime na posição do cursor do mouse
            SpawnSlime(mousePosition.X, mousePosition.Y);
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

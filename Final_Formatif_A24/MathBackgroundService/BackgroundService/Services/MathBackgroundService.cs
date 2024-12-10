using BackgroundServiceVote.Data;
using BackgroundServiceVote.DTOs;
using BackgroundServiceVote.Hubs;
using BackgroundServiceVote.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundServiceVote.Services
{
    public class UserData
    {
        public int Choice { get; set; } = -1;
        public int NbConnections { get; set; } = 0;
    }

    public class MathBackgroundService : BackgroundService
    {
        public const int DELAY = 20 * 1000;

        private Dictionary<string, UserData> _data = new();

        private IHubContext<MathQuestionsHub> _mathQuestionHub;

        private MathQuestion? _currentQuestion;

        public MathQuestion? CurrentQuestion => _currentQuestion;

        private MathQuestionsService _mathQuestionsService;
        // TODO: Scope
        private IServiceScopeFactory _serviceScopeFactory;


        public MathBackgroundService(IHubContext<MathQuestionsHub> mathQuestionHub, MathQuestionsService mathQuestionsService, IServiceScopeFactory serviceScopeFactory) // Scope
        {
            _mathQuestionHub = mathQuestionHub;
            _mathQuestionsService = mathQuestionsService;
            //TODO: Scope
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void AddUser(string userId)
        {
            if (!_data.ContainsKey(userId))
            { 
                _data[userId] = new UserData();
            }
            _data[userId].NbConnections++;
        }

        public void RemoveUser(string userId)
        {
            if (!_data.ContainsKey(userId))
            {
                _data[userId].NbConnections--;
                if(_data[userId].NbConnections <= 0)
                    _data.Remove(userId);
            }
        }

        public async void SelectChoice(string userId, int choice)
        {
            if (_currentQuestion == null)
                return;

            UserData userData = _data[userId];
            
            if (userData.Choice != -1)
                throw new Exception("A user cannot change is choice!");

            userData.Choice = choice;

            _currentQuestion.PlayerChoices[choice]++;

            // TODO 01 (DONE): Notifier les clients qu'un joueur a choisi une réponse
            await _mathQuestionHub.Clients.All.SendAsync("IncreasePlayersChoices", choice);
        }

        private async Task EvaluateChoices()
        {
            // TODO 02 (DONE): La méthode va avoir besoin d'un scope
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                BackgroundServiceContext dbContext = scope.ServiceProvider.GetRequiredService<BackgroundServiceContext>();

                // On peut maintenant utiliser le dbContext normalement
                // On peut également faire un SaveChanges.
                foreach (var userId in _data.Keys)
                {
                    Player player = await dbContext.Player.Where(p => p.UserId == userId).FirstAsync();

                    var userData = _data[userId];
                    // TODO 02.1 (DONE): Notifier les clients pour les bonnes et mauvaises réponses
                    // TODO 02.2 (DONE): Modifier et sauvegarder le NbRightAnswers des joueurs qui ont la bonne réponse
                    // Vérifier la réponse du joueur
                    if (userData.Choice == _currentQuestion!.RightAnswerIndex)
                    {
                        // Bonne réponse
                        player.NbRightAnswers++;
                        await dbContext.SaveChangesAsync();

                        PlayerInfoDTO playerInfo = new PlayerInfoDTO
                        {
                            NbRightAnswers = player.NbRightAnswers
                        };

                        // Notifier le joueur de la bonne réponse
                        await _mathQuestionHub.Clients.User(userId).SendAsync("GoodAnswer", playerInfo);
                    }
                    else
                    {
                        // Mauvaise réponse ou pas de réponse
                        var rightAnswer = _currentQuestion!.Answers[_currentQuestion.RightAnswerIndex];

                        // Notifier le joueur de la mauvaise réponse
                        await _mathQuestionHub.Clients.User(userId).SendAsync("BadAnswer", rightAnswer);
                    }

                }


            }
            // Une fois que l'on va sortir du "using", le scope va être détruit et le dbContext associé au scope va également être détruit!
            // Reset
            foreach (var key in _data.Keys)
            {
                _data[key].Choice = -1;
            }
        }

        private async Task Update(CancellationToken stoppingToken)
        {
            if (_currentQuestion != null)
            {
                await EvaluateChoices();
            }

            _currentQuestion = _mathQuestionsService.CreateQuestion();

            await _mathQuestionHub.Clients.All.SendAsync("CurrentQuestion", _currentQuestion);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Update(stoppingToken);
                await Task.Delay(DELAY, stoppingToken);
            }
        }

        // TODO 03 (DONE):	Mettre à jour le nombre de bonnes réponses (NbRightAnswers) des joueurs qui ont eu la bonne réponse dans la BD.
        // (Vérifier que la donnée est encore bonne après un refresh de la page)
        public async Task<PlayerInfoDTO?> GetPlayerInfo(string userId)
        {
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                BackgroundServiceContext dbContext = scope.ServiceProvider.GetRequiredService<BackgroundServiceContext>();
                var player = await dbContext.Player.FirstOrDefaultAsync(p => p.UserId == userId);

                if (player == null)
                    return null;

                return new PlayerInfoDTO
                {
                    NbRightAnswers = player.NbRightAnswers
                };
            }
        }
    }
}

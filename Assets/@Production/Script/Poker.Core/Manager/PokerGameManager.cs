using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using UnityEngine.Profiling;
using Unity.Collections.LowLevel.Unsafe;
using System.Linq;
using Pker.Combination;
using System.Threading.Tasks;

namespace Pker
{
    public class PokerGameManager : MonoBehaviour
    {
        [SerializeField]
        [NaughtyAttributes.ReadOnly]
        GameState gameState = GameState.Prepare;

        [SerializeField]
        PokerPlayer[] pokerPlayers;
        public IReadOnlyList<PokerPlayer> PokerPlayers => pokerPlayers;

        public CardCombination LastCard { get; private set; }
        public int LastGiveTurn { get; private set; }
        public int Turn { get; private set; }
        public int PlayerCount { get; private set; }
        public int LastWinner { get; private set; }
        public long BetPerCard { get; private set; }


        //Start Game
        public UnityEvent OnNewCardShared { get; set; } = new UnityEvent();

        //OnGamePlay
        public UnityEvent<int> OnTurnChange { get; set; } = new UnityEvent<int>();
        public UnityEvent<int, PlayerAction> OnPlayerAction { get; set; } = new UnityEvent<int, PlayerAction>();
        public UnityEvent<CardCombination> OnCardPlayed { get; set; } = new UnityEvent<CardCombination>();
        public UnityEvent<int> OnPlayerWinSession { get; set; } = new UnityEvent<int>();
        public UnityEvent<int> OnPlayerWin { get; set; } = new UnityEvent<int>();
        
        //OnLose
        public UnityEvent<int> OnPlayerChanged { get; set; } = new UnityEvent<int>();

        public async Task WaitUntilGameStart() 
        {
            while (gameState == GameState.Prepare)
            { 
                await Task.Delay(100);
            }
        }

        [Button]
        public void StartGame()
        {
            StartGame(new long[] { 100000 , 100000 , 100000 , 100000 } , 1000, 0);
        }

        private NativeArray<Card> GetShuffledCard() 
        {
            var allDeck = PokerHelper.GetFullDeck(Allocator.TempJob);

            Profiler.BeginSample("Shuffle Array");
            Shuffle(allDeck, (uint)Random.Range(1, 9999999));
            Profiler.EndSample();

            return allDeck;
        }

        public void StartGame(long[] money , long betPerCard, int firstTurn = 0)
        {
            if (gameState != GameState.Prepare)
            {
                return;
            }

            int totalPlayer = money.Length;
            if (totalPlayer < 2 || totalPlayer > 4)
            {
                Debug.LogError("Capsa player are limited to 2-4 player");
                return;
            }

            Profiler.BeginSample("Starting Game");
            var allDeck = GetShuffledCard();

            LastCard = default;
            BetPerCard = betPerCard;
            PlayerCount = totalPlayer;
            Turn = firstTurn % totalPlayer;
            pokerPlayers = new PokerPlayer[totalPlayer];
            for (int i = 0; i < totalPlayer; i++)
            {
                pokerPlayers[i] = new PokerPlayer(this, money[i]);
                pokerPlayers[i].UpdateCard(allDeck.GetSubArray(i*13, 13));
            }
            allDeck.Dispose();

            UpdateAllPlayerCombination();
            gameState = GameState.Play;
            OnNewCardShared.Invoke();
            OnTurnChange.Invoke(Turn);
            Profiler.EndSample();
        }

        [Button]
        public void NextGame()
        {
            if (gameState != GameState.EndGame)
            {
                return;
            }

            Profiler.BeginSample("Starting Game");
            var allDeck = GetShuffledCard();

            LastCard = default;
            Turn = LastWinner;
            OnCardPlayed.Invoke(LastCard);

            for (int i = 0; i < pokerPlayers.Length; i++)
            {
                pokerPlayers[i].UpdateCard(allDeck.GetSubArray(i * 13, 13));
            }
            allDeck.Dispose();

            UpdateAllPlayerCombination();
            gameState = GameState.Play;
            OnNewCardShared.Invoke();
            OnTurnChange.Invoke(Turn);
            Debug.Log("Next Game, Turn : "+Turn);
            Profiler.EndSample();
        }

        public void NextTurn(bool triggerplayerAction= true) 
        {
            if (gameState != GameState.Play) return;

            if (triggerplayerAction)
            {
                OnPlayerAction.Invoke(Turn, PlayerAction.Skip);
            }

            Turn = (Turn + 1) % PlayerCount;
            OnTurnChange.Invoke(Turn);

            if (LastGiveTurn == Turn)
            {
                EndSession();
            }
        }
        public bool PlayCard(CardCombination combination)
        {
            if (gameState != GameState.Play) return false;

            bool isValid = combination.IsHigherThan(LastCard);
            if (isValid && PokerPlayers[Turn].IsPlayerHasCard(combination, out var indexes))
            {
                LastCard = combination;
                PokerPlayers[Turn].UseCard(indexes);

                OnPlayerAction.Invoke(Turn, PlayerAction.Play);
                OnCardPlayed.Invoke(combination);

                if (!CheckWinner(Turn))
                {
                    LastGiveTurn = Turn;
                    NextTurn(false);
                }
                else
                {
                    EndGame();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void EndSession()
        {
            LastCard = default;
            OnCardPlayed.Invoke(LastCard);
            OnPlayerWinSession.Invoke(Turn);
        }

        [Button]
        private void EndGame() 
        {
            gameState = GameState.EndGame;
            
            LastWinner = Turn;

            long rewards = 0;
            for (int i = 0; i < pokerPlayers.Length; i++)
            {
                if (LastWinner == i) continue;

                var loseMoney = GetLoseMoney(i);
                rewards += loseMoney;
                pokerPlayers[i].Money -= loseMoney;

                if (pokerPlayers[i].Money == 0)
                {
                    pokerPlayers[i] = new PokerPlayer(this, 100000);// change bankrupt player
                    OnPlayerChanged.Invoke(i);
                }
            }

            pokerPlayers[LastWinner].Money += rewards;
            OnPlayerWin.Invoke(Turn);
        }

        private bool CheckWinner(int playerId) 
        {
            return PokerPlayers[playerId].Cards.Count == 0;
        }

        private long GetLoseMoney(int playerId)
        {
            long loseMoney = PokerPlayers[playerId].Cards.Count * BetPerCard;
            if (pokerPlayers[playerId].Money < loseMoney)
            {
                loseMoney = pokerPlayers[playerId].Money;
            }

            return loseMoney;
        }

        List<GetCombinationResult> getCombinationsCache = new List<GetCombinationResult>(4);
        List<NativeArray<Card>> allcardCache = new List<NativeArray<Card>>(4);
        bool inProcess = false;

        [Button]
        public void UpdateAllPlayerCombination() 
        {
            if (inProcess) return;

            inProcess = true;
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(pokerPlayers.Length , Allocator.Temp);
            for (int i = 0; i < PokerPlayers.Count; i++)
            {
                NativeArray<Card> allCard = new NativeArray<Card>(pokerPlayers[i].Cards.ToArray(), Allocator.TempJob);
                var jobHandle = PokerHelper.GetAllCombinations(allCard, out var result);
                jobHandles[i] = jobHandle;
                getCombinationsCache.Add(result);
                allcardCache.Add(allCard);
            }

            JobHelper.AddScheduledJob(default, JobHandle.CombineDependencies(jobHandles), oncomplete =>
            {
                for (int i = 0; i < PokerPlayers.Count; i++)
                {
                    pokerPlayers[i].SetCombinations(getCombinationsCache[i]);
                    allcardCache[i].Dispose();
                }
                getCombinationsCache.Clear();
                allcardCache.Clear();
                inProcess = false;
                UnityEngine.Debug.Log("Job Complete : " + oncomplete.Duration);
            });            
        }

        [BurstCompile]
        public static void Shuffle(NativeArray<Card> array, uint seed)
        {
            
            Unity.Mathematics.Random rand = new Unity.Mathematics.Random(seed);
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rand.NextInt(0, i + 1);

                // Swap array[i] and array[j]
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }
}
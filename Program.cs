
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

class Program
{
    static void Main(string[] args)
    {
        HouseEdgeCalculator calculator = new HouseEdgeCalculator();
        double totalEdge = 0.0;
        for (int i = 1; i <= 10; i++)
        {
            for (int j = 1; j <= 10; j++)
            {
                for (int k = 1; k <= 10; k++)
                {
                    double odds = ((double)calculator.shoe[i] / calculator.total) * ((double)calculator.shoe[j] / calculator.total) * ((double)calculator.shoe[k] / calculator.total);
                    Hand player = new Hand();
                    Hand dealer = new Hand();
                    player.AddCard(i); player.AddCard(j);
                    dealer.AddCard(k);
                    double[] result = calculator.CalculateHouseEdge(player, dealer);
                    double edge = result[0];
                    double choice = result[1];
                    totalEdge += odds * edge;

                    Console.WriteLine($"{i}-{j} vs {k}\t{edge}\t{choice}");
                }
            }
        }
        Console.WriteLine($"Total: {totalEdge}\t{calculator.total}");
        //Hand player = new Hand(16, false, 2);
        //Hand dealer = new Hand(6, false, 1);
        //calculator.shoe[8]--; calculator.shoe[8]--;
        //calculator.shoe[6]--;
        //calculator.total -= 3;

        //double stand = calculator.CalculateHouseEdgeStand(player, dealer);
        //double hit = calculator.CalculateHouseEdgeHit(player, dealer);
        //double doubled = calculator.CalculateHouseEdgeDouble(player, dealer);
        //double split = calculator.CalculateHouseEdgeSplit(player, dealer);

        //Console.WriteLine($"Stand: {stand}\tHit: {hit}\tDouble: {doubled}\tSplit: {split}");
    }
}

class HouseEdgeCalculator
{
    public Dictionary<int, int> shoe = new Dictionary<int, int>()
    {
        { 1, 32 },
        { 2, 32 },
        { 3, 32 },
        { 4, 32 },
        { 5, 32 },
        { 6, 32 },
        { 7, 32 },
        { 8, 32 },
        { 9, 32 },
        { 10, 128 },
    };
    public int total = 8 * 52;

    public double[] CalculateHouseEdge(Hand playerHand, Hand dealerHand)
    {
        if (playerHand.NaturalBJ())
        {
            return new[] { NaturalBJHouseEdge(dealerHand), -1.0 };
        }
        List<Double> moveOdds = new List<double>
        {
            CalculateHouseEdgeStand(playerHand, dealerHand),
            CalculateHouseEdgeHit(playerHand, dealerHand),
            CalculateHouseEdgeDouble(playerHand, dealerHand)
        };
        if (playerHand.CanSplit)
        {
            moveOdds.Add(CalculateHouseEdgeSplit(playerHand, dealerHand));
        }
        double maxValue = moveOdds.Max();
        return new[] { maxValue, moveOdds.IndexOf(maxValue) };
    }

    public double CalculateHouseEdgeStand(Hand playerHand, Hand dealerHand)
    {
        return CalculateHouseEdgeStandRec(playerHand, dealerHand);
    }

    private double CalculateHouseEdgeStandRec(Hand playerHand, Hand dealerHand)
    {
        if (playerHand.TooMany())
        {
            return -1;
        }
        if (dealerHand.TooMany())
        {
            return 1;
        }
        if (dealerHand.Score > 16)
        {
            if (dealerHand.Score > playerHand.Score)
            {
                return -1;
            }
            else if (dealerHand.Score < playerHand.Score)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        double houseEdge = 0.0;
        // if ((dealerHand.Score == 10 || (dealerHand.Score == 11 && dealerHand.Soft)) )
        if (dealerHand.Cards == 1 && (dealerHand.Score == 10 || dealerHand.Score == 11))
        {
            return HandleDealerBJ(playerHand, dealerHand);
        }
        for (int i = 1; i <= 10; i++)
        {
            double cardOdds = (double)shoe[i] / total;
            shoe[i]--; total--;
            Hand newDealerHand = new Hand(dealerHand);
            newDealerHand.AddCard(i);
            houseEdge += cardOdds * CalculateHouseEdgeStandRec(playerHand, newDealerHand);
            shoe[i]++; total++;
        }
        return houseEdge;
    }

    private double HandleDealerBJ(Hand playerHand, Hand dealerHand)
    {
        if (dealerHand.Score == 11)
        {
            int shoeTotalNo10s = total - shoe[10];
            double houseEdge = 0.0;
            for (int i = 1; i <= 9; i++)
            {
                double cardOdds = (double)shoe[i] / shoeTotalNo10s;
                shoe[i]--; total--;
                Hand newDealerHand = new Hand(dealerHand);
                newDealerHand.AddCard(i);
                houseEdge += cardOdds * CalculateHouseEdgeStandRec(playerHand, newDealerHand);
                shoe[i]++; total++;
            }
            return houseEdge;
        }
        else
        {
            int shoeTotalNoAces = total - shoe[1];
            double houseEdge = 0.0;
            for (int i = 2; i <= 10; i++)
            {
                double cardOdds = (double)shoe[i] / shoeTotalNoAces;
                shoe[i]--; total--;
                Hand newDealerHand = new Hand(dealerHand);
                newDealerHand.AddCard(i);
                houseEdge += cardOdds * CalculateHouseEdgeStandRec(playerHand, newDealerHand);
                shoe[i]++; total++;
            }
            return houseEdge;
        }
    }

    public double CalculateHouseEdgeHit(Hand playerHand, Hand dealerHand)
    {
        double houseEdge = 0.0;
        for (int i = 1; i <= 10; i++)
        {
            double cardOdds = (double)shoe[i] / total;
            shoe[i]--; total--;
            Hand newPlayerHand = new Hand(playerHand);
            newPlayerHand.AddCard(i);
            houseEdge += cardOdds * CalculateHouseEdgeHitRec(newPlayerHand, dealerHand);
            shoe[i]++; total++;
        }
        return houseEdge;
    }

    private double CalculateHouseEdgeHitRec(Hand playerHand, Hand dealerHand, int depth = 2)
    {
        if (depth < 0)  // Avoid testing unlikely scenarios where player needs 7 cards or more
        {               // For one of the worst case (player 6, dealer 2), this reduces player edge computation
            return 0;   // from ~20s to ~2.5s with an error of ~0.004%
        }
        if (playerHand.TooMany())
        {
            return -1;
        }

        double standHouseEdge = CalculateHouseEdgeStand(playerHand, dealerHand);
        double hitHouseEdge = 0.0;
        for (int i = 1; i <= 10; i++)
        {
            double cardOdds = (double)shoe[i] / total;
            shoe[i]--; total--;
            Hand newPlayerHand = new Hand(playerHand);
            newPlayerHand.AddCard(i);
            hitHouseEdge += cardOdds * CalculateHouseEdgeHitRec(newPlayerHand, dealerHand, depth - 1);
            shoe[i]++; total++;
        }
        return Math.Max(standHouseEdge, hitHouseEdge);
    }

    public double CalculateHouseEdgeDouble(Hand playerHand, Hand dealerHand)
    {
        double houseEdge = 0.0;
        for (int i = 1; i <= 10; i++)
        {
            double cardOdds = (double)shoe[i] / total;
            shoe[i]--; total--;
            Hand newPlayerHand = new Hand(playerHand);
            newPlayerHand.AddCard(i);
            houseEdge += cardOdds * 2 * CalculateHouseEdgeStand(newPlayerHand, dealerHand);
            shoe[i]++; total++;
        }
        return houseEdge;
    }

    public double CalculateHouseEdgeSplit(Hand playerHand, Hand dealerHand)
    {
        bool acePair = playerHand.Score == 12 && playerHand.Soft;
        int newPlayerScore = acePair ? 11 : playerHand.Score / 2;
        Hand newPlayerHand = new Hand(newPlayerScore, acePair, 1);
        return 2* CalculateHouseEdge(newPlayerHand, dealerHand)[0];

    }

    private double NaturalBJHouseEdge(Hand dealerHand)
    {
        if (dealerHand.Cards == 1)
        {
            if (dealerHand.Score == 10)
            {
                double aceOdds = (double)shoe[1] / total;
                return 1.5 * (1 - aceOdds);
            }
            if (dealerHand.Score == 11)
            {
                double tenOdds = (double)shoe[10] / total;
                return 1.5 * (1 - tenOdds);
            }
        }
        return 1.5;
    }
}

public class Hand
{
    public Hand(int score = 0, bool soft = false, int cards = 0, bool canSplit = false)
    {
        Score = score;
        Soft = soft;
        Cards = cards;
        CanSplit = canSplit;
        Debug.Assert(Score >= 11 || !Soft);
    }
    public Hand(Hand other)
    {
        Score = other.Score;
        Soft = other.Soft;
        Cards = other.Cards;
        CanSplit = other.CanSplit;
    }
    public int Score { get; set; }
    public bool Soft { get; set; }
    public int Cards { get; set; }
    public bool CanSplit { get; set; }
    public bool TooMany()
    {
        return Score > 21;
    }
    public bool NaturalBJ()
    {
        return Cards == 2 && Score == 21;
    }
    public void AddCard(int card)
    {
        Cards++;
        if (Cards == 2 && (card == Score || (card == 1 && Score == 11)))
        {
            CanSplit = true;
        }
        else
        {
            CanSplit = false;
        }
        if (card == 1)
        {
            if (Score < 11)
            {
                Score += 11;
                Soft = true;
            }
            else
            {
                Score++;
            }
        }
        else
        {
            if (Soft && Score + card > 21)
            {
                Score = Score + card - 10;
                Soft = false;
            }
            else
            {
                Score += card;
            }
        }
    }
}
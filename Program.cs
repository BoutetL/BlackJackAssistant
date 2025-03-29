
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

class Program
{
    static void Main(string[] args)
    {
        Hand player = new Hand(6, false);
        Hand dealer = new Hand(2, false);
        HouseEdgeCalculator calculator = new HouseEdgeCalculator();
        double stand = calculator.CalculateHouseEdgeStand(player, dealer);
        double hit = calculator.CalculateHouseEdgeHit(player, dealer);

        Console.WriteLine(stand + "   " + hit);
    }
}

class HouseEdgeCalculator
{
    Dictionary<int, int> shoe = new Dictionary<int, int>()
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
    int total = 8 * 52;

    public double CalculateHouseEdgeStand(Hand playerHand, Hand dealerHand)
    {
        return CalculateHouseEdgeStandRec(playerHand, dealerHand);
    }

    private double CalculateHouseEdgeStandRec(Hand playerHand, Hand dealerHand)
    {
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
        if ((dealerHand.Score == 11 && dealerHand.Soft) || dealerHand.Score == 10)
        {
            return HandleDealerBJ(playerHand, dealerHand);
        }
        for (int i = 1; i <= 10; i++)
        {
            double cardOdds = (double)shoe[i] / total;
            Hand newDealerHand = new Hand(dealerHand);
            newDealerHand.AddCard(i);
            houseEdge += cardOdds * CalculateHouseEdgeStandRec(playerHand, newDealerHand);
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
                Hand newDealerHand = new Hand(dealerHand);
                newDealerHand.AddCard(i);
                houseEdge += cardOdds * CalculateHouseEdgeStandRec(playerHand, newDealerHand);
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
                Hand newDealerHand = new Hand(dealerHand);
                newDealerHand.AddCard(i);
                houseEdge += cardOdds * CalculateHouseEdgeStandRec(playerHand, newDealerHand);
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
            Hand newPlayerHand = new Hand(playerHand);
            newPlayerHand.AddCard(i);
            houseEdge += cardOdds * CalculateHouseEdgeHitRec(newPlayerHand, dealerHand, 0);
            Console.Write(i + " ");
        }
        return houseEdge;
    }

    private double CalculateHouseEdgeHitRec(Hand playerHand, Hand dealerHand, int depth)
    {
        if (depth > 3)  // Avoid testing unlikely scenarios where player needs 7 cards or more
        {               // For one of the worst case (player 6, dealer 2), this reduces player edge computation
            return 0;   // from ~20s to ~2.5s
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
            Hand newPlayerHand = new Hand(playerHand);
            newPlayerHand.AddCard(i);
            hitHouseEdge += cardOdds * CalculateHouseEdgeHitRec(newPlayerHand, dealerHand, depth + 1);
        }
        return Math.Max(standHouseEdge, hitHouseEdge);
    }
}

public class Hand
{
    public Hand (int score, bool soft = false)
    {
        Score = score;
        Soft = soft;
        Debug.Assert(Score >= 11 || !Soft);
    }
    public Hand(Hand other)
    {
        Score = other.Score;
        Soft = other.Soft;
    }
    public int Score { get; set; }
    public bool Soft { get; set; }
    public bool TooMany()
    {
        return Score > 21;
    }
    public void AddCard(int card)
    {
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame1
{
    public class Player
    {
        public Game1 myGame;
        //number - player order
        public int number;
        //hand - cards in a players hand, must be less than 10, hidden from other players
        public Section hand = null;
        //Victory points earned from chips
        public int chipPoints = 0;
        //total victory points from chips+buildings in tableau- ONly calculated at end?
        public int totalPoints = 0;
        //buildings/planets played by players, game ends when one is size 12
        public Section tableau = null;
        //each time card is added/removed to tableau, special abilities need to be upated
        public bool isDirty = false; 
        //Action card played by this player
        public string phaseSelected = "";
        public string errorMessage = "";
        //Explore powers
        public int exploreDraw = 0;
        public int exploreKeep = 0;
        //Develop powers
        public int developDraw = 0;
        public int developReduce = 0;
        public int developDrawAfter = 0;
        public List<string> settlePowers = new List<string>();
        public int settleReduce = 0;
        public int totalMilitary = 0;
        public int cardsWithTempMilitary = 0;
        public int tempMilitary = 0;
        public int settleDrawAfter = 0;
        public bool payForMilitary = false;
        public bool freeWorld = false; //Only lasts one turn
        public int rebelHelp = 0;
        public Dictionary<string, int> settleSpecificCost = new Dictionary<string,int>(4);
        public Dictionary<string, int> settleSpecificMilitary = new Dictionary<string, int>(4);
        public List<string> tradePowers = new List<string>();
        //Trade Powers
        public int tradeDraw = 0;
        public Dictionary<string, int> tradeSpecific = new Dictionary<string, int>(4);
        //Consume Powers
        public List<ConsumePowers> myConsumePowers = new List<ConsumePowers>();
        public bool lucky = false; //player names a cost/defense, flips up top card, keeps if correct
        public bool all = false; //trade all remaining goods for VPs-1
        public int consumeDraw= 0;
        //Produce powers
        public int produceDrawAfter = 0;
        public Dictionary<string, int> produceGoods = new Dictionary<string, int>(4); //one for each color
        public Dictionary<string, int> produceWindfalls = new Dictionary<string, int>(5); //one for each color and one for ANY
        public Dictionary<string, int> produceDraw = new Dictionary<string, int>(6); //one for each color, one for different, one for NONE
        public Dictionary<string, int> produceMost = new Dictionary<string, int>(4); //one for each color
        public Dictionary<string, int> produceFromTableau = new Dictionary<string, int>(4); //one for each color

        public Player(int num, Game1 game)
        {
            this.number = num;
            settleSpecificCost["brown"] = 0;
            settleSpecificCost["blue"] = 0;
            settleSpecificCost["green"] = 0;
            settleSpecificCost["yellow"] = 0;
            settleSpecificMilitary["brown"] = 0;
            settleSpecificMilitary["blue"] = 0;
            settleSpecificMilitary["green"] = 0;
            settleSpecificMilitary["yellow"] = 0;
            hand = new Section(6, 6, game);
            tableau = new Section(5, 12, game);
            myGame = game;
            
        }

        public void addCardToHand(Card card)
        {
            card.owner = this;
            this.hand.Add(card);
        }

        public void addCardToTableau(Card card)
        {
            card.owner = this;
            card.curSection = tableau;
            this.tableau.Add(card);
            //update special abilities
            //each power for a given phase is separated by a comma
            foreach (string str in card.explore.Split(','))
            {
                parseExplore(str);
            }
            foreach (string str in card.develop.Split(','))
            {
                parseDevelop(str);
            }
            foreach (string str in card.settle.Split(','))
            {
                parseSettle(str);
                card.parseSettle(str);
            }
            foreach (string str in card.trade.Split(','))
            {
                parseTrade(str);
            }
            foreach (string str in card.consume.Split(','))
            {
                parseConsume(str);
            }
            foreach (string str in card.consume.Split(','))
            {
                parseProduce(str);
            }
            //special stuff
            if (card.tempMilitary > 0) cardsWithTempMilitary++;
            //dirtyBit = false;
        }

        public void parseExplore(string str)
        {
            string[] split = str.Split(':');
            if (split[0].Equals("keep")) exploreKeep += Int16.Parse(split[1]);
            else if (split[0].Equals("draw")) exploreDraw += Int16.Parse(split[1]);
        }

        public void parseDevelop(string str)
        {
            string[] split = str.Split(':');
            if (split[0].Equals("draw")) developDraw += Int16.Parse(split[1]);
            else if (split[0].Equals("reduce")) developReduce += Int16.Parse(split[1]);
            else if (split[0].Equals("after")) developDrawAfter += Int16.Parse(split[1]);
        }

        public void parseSettle(string str)
        {
            string[] split = str.Split(':');
            if (split[0].Equals("reduce")) settleReduce += Int16.Parse(split[1]);
            else if (split[0].Equals("military")) totalMilitary += Int16.Parse(split[1]);
            else if (split[0].Equals("after")) settleDrawAfter += Int16.Parse(split[1]);
            else if (split[0].Equals("payForMilitary")) payForMilitary = true;
            else if (split[0].Equals("rebel")) rebelHelp += Int16.Parse(split[1]);
            else if (split[0].Equals("specific"))
            {
                if (split[1].Equals("C")) settleSpecificCost[split[2]] += Int16.Parse(split[3]);
                else if (split[1].Equals("M")) settleSpecificMilitary[split[2]] += Int16.Parse(split[3]);
            }

            //freeWorld and tempMilitary are done for one round only so don't parse them here -lies
        }

        public void parseTrade(string str)
        {
            if (str.Contains("draw")) tradeDraw += Int16.Parse(str.Split(':')[1]);
            else if (str.Contains("specific")) tradeSpecific[str.Split(':')[1]] += Int16.Parse(str.Split(':')[2]);
            //'THIS' keyowrd will be handled when discarding a good
        }

        public void parseConsume(string str)
        {
            if (str.Split(':')[0].Contains("lucky")) lucky = true;
            else if (str.Split(':')[0].Contains("all")) all = true; //EVALUATE EACH ROUND?
            else if (str.Split(':')[0].Contains("draw")) consumeDraw += Int16.Parse(str.Split(':')[1]);

            //consume power should be evaluated each round? 
        }

        public void parseProduce(string str)
        {
            string[] split = str.Split(':');
            if (split[0].Equals("after")) produceDrawAfter += Int16.Parse(split[1]);
            else if (split[0].Equals("produce")) produceGoods[split[1]]++;
            else if (split[0].Equals("windfall")) produceWindfalls[split[1]]++;
            else if (split[0].Equals("draw"))
            {
                string[] split2 = split[1].Split('_');
                if (split2[0].Equals("tableau")) produceFromTableau[split2[1]] += Int16.Parse(split[2]);
                else if (split2[0].Equals("most")) produceMost[split2[1]] += Int16.Parse(split[2]);
                else produceDraw[split[1]] += Int16.Parse(split[2]);
            }

            //'DRAW IF PRODUCE ON THIS' NEEDS TO BE EVALUATED PER CARD EACH ROUND
        }
    }

    public class ConsumePowers
    {
        public string typeOfGood;
        public int numOfGoods;
        public int timesToDoIt;
        public int cardReward;
        public int vpReward;

        public ConsumePowers(string type, int num, int times, int card, int vp)
        {
            typeOfGood = type;
            numOfGoods = num;
            timesToDoIt = times;
            cardReward = card;
            vpReward = vp;
        }
    }
}

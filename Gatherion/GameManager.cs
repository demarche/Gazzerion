using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherion
{
    class GameManager
    {
        public List<Card>[] deck;
        public List<Card>[] handCard;
        public int skillPt_1p = 0;
        public int skillPt_2p = 0;

        //現在の手番
        public int now_Player = 0;
        //最大プレイヤー人数
        public int max_Player = 2;
        int cardNum;

        //手札枚数
        public int handCardNum;

        public bool is1P
        {
            get
            {
                return now_Player == 0;
            }
        }

        //現在のプレイヤーの手札
        public List<Card> nowHandCard
        {
            get
            {
                return handCard[now_Player];
            }
        }

        //現在のプレイヤーの山札
        public List<Card> nowDeck
        {
            get
            {
                return deck[now_Player];
            }
        }

        //手番交代
        public void next()
        {
            now_Player = (now_Player + 1) % max_Player;
        }

        public bool[] initiation;

        public Size fieldSize;
        public Size cardSize;
        public Field[,] field;
        public List<string> messageList;

        //CPUの再帰用コンストラクタ
        public GameManager(GameManager game, List<Card>[] handCard)
        {
            myConstractor(game.cardNum, game.handCardNum, game.fieldSize, game.cardSize);
            for(int x = 0; x < game.fieldSize.Width; x++)
            {
                for (int y = 0; y < game.fieldSize.Height; y++)
                {
                    field[x, y] = new Field(game.field[x, y]);
                }
            }
            initiation = (bool[])game.initiation.Clone();
            now_Player = game.now_Player;
            for(int i = 0; i < handCard.Count(); i++)
            {
                this.handCard[i] = new List<Card>(handCard[i]);
            }

        }

        //ゲーム開始用
        public GameManager(int cardNum, int handCardNum, Size fieldSize, Size cardSize, string[] deckIndexes = null)
        {
            myConstractor(cardNum, handCardNum, fieldSize, cardSize, deckIndexes);
        }

        //初期化処理群
        void myConstractor(int cardNum, int handCardNum, Size fieldSize, Size cardSize, string[] deckIndexes = null)
        {
            this.fieldSize = fieldSize;
            this.cardSize = cardSize;
            this.cardNum = cardNum;
            this.handCardNum = handCardNum;
            messageList = new List<string>();
            //フィールド初期化
            field = new Field[fieldSize.Width, fieldSize.Height];
            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    field[x, y] = new Field();
                }
            }
            //カード初期化
            deck = new List<Card>[max_Player];
            handCard = new List<Card>[max_Player];
            //山札を作成
            if (deckIndexes == null) deckIndexes = new string[max_Player];
            foreach (var deckIndex in deckIndexes.Select((v, i) => new { v, i }))
            {
                if (deckIndex.v == "" || deckIndex.v == null)
                {
                    deck[deckIndex.i] = Enumerable.Range(0, cardNum).Select(t => Card.RandomCardGenerator()).ToList();
                }
                else
                {
                    deck[deckIndex.i] = new List<Card>(Card.deckList[deckIndex.v].Select(t => new Card(((int[])t.elems.ToArray().Clone()).ToList(), t.imgPath)));
                }

                //山札をシャッフル
                Card.suffleCards(ref deck[deckIndex.i]);
            }
            initiation = new bool[max_Player];
            for(int i = 0; i < max_Player; i++)
            {
                //手札を作成
                handCard[i] = new List<Card>();
                //イニシエーション
                initiation[i] = true;
            }
            //Card.SerializeCards(card_1p);

            //ドロー
            for (int i = 0; i < handCardNum; i++)
            {
                for (int j = 0; j < max_Player; j++)
                {
                    draw();
                    next();
                }
            }
        }

        //手札からフィールドに
        public bool handToField(Point fieldPt, int handCardIndex)
        {
            Card card = nowHandCard[handCardIndex];
            if (!Field.putCard(this, fieldPt, card, cardSize, initiation)) return false;

            //手札を消す
            nowHandCard.RemoveAt(handCardIndex);

            return true;
        }

        //フィールドのクリア
        public void clearField()
        {
            field = new Field[fieldSize.Width, fieldSize.Height];
            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    field[x, y] = new Field();
                }
            }
            initiation = new bool[2] { true, true };
        }

        //バースト
        public void burst(int group)
        {
            int skillpt = Field.Burst(this, cardSize, group);
            if (is1P) skillPt_2p += skillpt;
            else skillPt_1p += skillpt;

            initiation[group] = true;
        }

        //手札番号更新
        public void refleshHandcardID()
        {
            var notContainsIDs = Enumerable.Range(0, nowHandCard.Count()).Where(t => !nowHandCard.Select(u => u.handCardID).Contains(t)).ToList();
            for (int i = 0; i < nowHandCard.Count(); i++)
            {
                if (nowHandCard[i].handCardID != -1) continue;
                nowHandCard[i].handCardID = notContainsIDs.First();
                notContainsIDs.RemoveAt(0);
            }
        }

        //山札から手札へドロー
        public bool draw()
        {
            if (nowHandCard.Count() >= handCardNum) return false;
            if (nowDeck.Count() == 0) return false;

            nowHandCard.Add(deck[now_Player].First());
            nowDeck.RemoveAt(0);

            //手札番号更新
            refleshHandcardID();

            return true;
        }

        //メッセージを挿入
        public void insertInfo(string info)
        {
            messageList.Add(messageList.Count().ToString()+" "+ (now_Player + 1).ToString() + "P:" + info);
        }
    }
}

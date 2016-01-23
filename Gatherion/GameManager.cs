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
        public List<Card> card_1p;
        public List<Card> card_2p;
        public List<Card> handCard_1p = new List<Card>();
        public List<Card> handCard_2p = new List<Card>();
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

        //手番交代
        public void next()
        {
            now_Player = (now_Player + 1) % max_Player;
        }

        public bool[] initiation = new bool[2] { true, true };

        public Size fieldSize;
        public Size cardSize;
        public Field[,] field;
        public List<string> messageList;

        //CPUの再帰用コンストラクタ
        public GameManager(GameManager game, List<Card> handCard_1p, List<Card> handCard_2p)
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
            this.handCard_1p = new List<Card>(handCard_1p);
            this.handCard_2p = new List<Card>(handCard_2p);

        }

        //ゲーム開始用
        public GameManager(int cardNum, int handCardNum, Size fieldSize, Size cardSize, string deckIndex_1p="", string deckIndex_2p="")
        {
            myConstractor(cardNum, handCardNum, fieldSize, cardSize, deckIndex_1p,deckIndex_2p);
        }

        //初期化処理群
        void myConstractor(int cardNum, int handCardNum, Size fieldSize, Size cardSize, string deckIndex_1p = "", string deckIndex_2p = "")
        {
            this.fieldSize = fieldSize;
            this.cardSize = cardSize;
            this.cardNum = cardNum;
            this.handCardNum = handCardNum;
            messageList = new List<string>();
            field = new Field[fieldSize.Width, fieldSize.Height];
            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    field[x, y] = new Field();
                }
            }

            if (deckIndex_1p == "")
                card_1p = Enumerable.Range(0, cardNum).Select(t => Card.RandomCardGenerator()).ToList();
            else
                card_1p = new List<Card>(Card.deckList[deckIndex_1p].Select(t => new Card(((int[])t.elems.ToArray().Clone()).ToList(), t.imgPath)));
            if (deckIndex_2p == "")
                card_2p = Enumerable.Range(0, cardNum).Select(t => Card.RandomCardGenerator()).ToList();
            else
                card_2p = new List<Card>(Card.deckList[deckIndex_2p].Select(t => new Card(((int[])t.elems.ToArray().Clone()).ToList(), t.imgPath)));
            Card.suffleCards(ref card_1p);
            Card.suffleCards(ref card_2p);
            Card.SerializeCards(card_1p);

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
            Card card = is1P ? handCard_1p[handCardIndex] : handCard_2p[handCardIndex];
            if (!Field.putCard(this, fieldPt, card, cardSize, initiation)) return false;

            if (is1P) handCard_1p.RemoveAt(handCardIndex);
            else handCard_2p.RemoveAt(handCardIndex);
            refleshHandcardID();

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

            for (int i = 0; i < (is1P ? handCard_1p : handCard_2p).Count(); i++)
            {
                (is1P ? handCard_1p : handCard_2p)[i].handCardID = i;
            }
        }

        //ドロー
        public bool draw()
        {
            if (is1P && handCard_1p.Count() >= handCardNum || !is1P && handCard_2p.Count() >= handCardNum) return false;
            if (is1P && card_1p.Count() == 0 || !is1P && card_2p.Count() == 0) return false;

            if (is1P)
            {
                handCard_1p.Add(card_1p.First());
                card_1p.RemoveAt(0);
            }
            else
            {
                handCard_2p.Add(card_2p.First());
                card_2p.RemoveAt(0);
            }

            //手札番号更新
            refleshHandcardID();

            return true;
        }

        //情報を挿入
        public void insertInfo(string info)
        {
            messageList.Add(messageList.Count().ToString()+" "+ (now_Player + 1).ToString() + "P:" + info);
        }
    }
}

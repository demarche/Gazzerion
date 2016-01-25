using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherion
{
    class Field
    {
        public Card card = null;//フィールドの左上にカード配置
        public int connector = -1;//コネクター
        public int group = -1;

        static int connector_group = -1;

        public Field(Field field)
        {
            card = field.card;
            connector = field.connector;
            group = field.group;
        }

        public Field() { }

        //衝突判定
        static bool collision(Field[,] field, Point putAt, Size cardSize)
        {
            Size fieldSize = new Size(field.GetLength(0), field.GetLength(1));
            if (putAt.X > fieldSize.Width - cardSize.Width || putAt.Y > fieldSize.Height - cardSize.Height) return true;
            
            for (int x = 0; x < cardSize.Width; x++)
            {
                for (int y = 0; y < cardSize.Height; y++)
                {
                    if (field[putAt.X + x, putAt.Y + y].group != -1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// コネクターに連結可能か判定
        /// card.TestCard==trueの場合はコネクターの属性を無視
        /// </summary>
        /// <param name="field"></param>
        /// <param name="putAt"></param>
        /// <param name="card"></param>
        /// <param name="mycardSize">回転に合わせたカードサイズ</param>
        /// <returns></returns>
        static bool canConnect(Field[,] field, Point putAt, Card card, Size mycardSize)
        {
            Size fieldSize = new Size(field.GetLength(0), field.GetLength(1));

            List<int> elems = card.elems;
            int turn = card.turn;
            for (int i = 0; i < 4; i++)
            {
                if (elems[i] == -1 && card.testCard == -1) continue;
                int sx, sy;
                switch ((i + turn) % 4)
                {
                    case 0:
                        sx = putAt.X + mycardSize.Width / 2 - 1;
                        sy = putAt.Y - 1;
                        if (sy < 0) continue;
                        connector_group = field[sx, sy].group;
                        if (card.testCard == -1 &&
                            field[sx, sy].connector == elems[i] &&
                            field[sx + 1, sy].connector == elems[i] ||
                            card.testCard != -1 &&
                            field[sx, sy].group == card.testCard &&//テストカード
                            field[sx + 1, sy].group == card.testCard &&
                            field[sx, sy].connector != -1 &&
                            field[sx + 1, sy].connector != -1)
                            return true;
                        break;
                    case 1:
                        sx = putAt.X + mycardSize.Width;
                        sy = putAt.Y + mycardSize.Height / 2;
                        if (sx >= fieldSize.Width) continue;
                        connector_group = field[sx, sy].group;
                        if (card.testCard == -1 &&
                            field[sx, sy].connector == elems[i] &&
                            field[sx, sy - 1].connector == elems[i] ||
                            card.testCard != -1 &&
                            field[sx, sy].group == card.testCard &&//テストカード
                            field[sx, sy - 1].group == card.testCard &&
                            field[sx, sy].connector != -1 &&
                            field[sx, sy - 1].connector != -1)
                            return true;
                        break;
                    case 2:
                        sx = putAt.X + mycardSize.Width / 2 - 1;
                        sy = putAt.Y + mycardSize.Height;
                        if (sy >= fieldSize.Height) continue;
                        connector_group = field[sx, sy].group;
                        if (card.testCard == -1 &&
                            field[sx, sy].connector == elems[i] &&
                            field[sx + 1, sy].connector == elems[i] ||
                            card.testCard != -1 &&
                            field[sx, sy].group == card.testCard &&//テストカード
                            field[sx + 1, sy].group == card.testCard &&
                            field[sx, sy].connector != -1 &&
                            field[sx + 1, sy].connector != -1)
                            return true;
                        break;
                    case 3:
                        sx = putAt.X - 1;
                        sy = putAt.Y + mycardSize.Height / 2 - 1;
                        if (sx < 0) continue;
                        connector_group = field[sx, sy].group;
                        if (card.testCard == -1 &&
                            field[sx, sy].connector == elems[i] &&
                            field[sx, sy + 1].connector == elems[i] ||
                            card.testCard != -1 &&
                            field[sx, sy].group == card.testCard &&//テストカード
                            field[sx, sy + 1].group == card.testCard &&
                            field[sx, sy].connector != -1 &&
                            field[sx, sy + 1].connector != -1)
                            return true;
                        break;
                }
            }
            return false;
        }

        //配置可能判定
        public static bool canPut(Field[,] field, Point putAt, Card card, Size mycardSize, int now_Player, bool[] initiation, bool isJudge = false)
        {
            Size fieldSize = new Size(field.GetLength(0), field.GetLength(1));

            //衝突判定
            if (collision(field, putAt, mycardSize)) return false;

            //イニシエーション可能
            if (initiation[now_Player] && (now_Player % 2 == 0 && putAt.Y == fieldSize.Height - mycardSize.Height || now_Player % 2 == 1 && putAt.Y == 0))
            {
                if (!isJudge) initiation[now_Player] = false;
                connector_group = now_Player;
                return true;
            }

            //属性連結可能
            if (canConnect(field, putAt, card, mycardSize)) return true;

            return false;
        }

        //バーストチェック
        public static bool isBurst(GameManager game, Size cardSize, int group)
        {
            Size fieldSize = game.fieldSize;

            Card card = new Card(group);
            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    for (int turn = 0; turn < 2; turn++)
                    {
                        Size mycardSize = cardSize;
                        if (turn % 2 == 1)
                        {
                            mycardSize = new Size(cardSize.Height, cardSize.Width);
                        }
                        card.turn = turn;
                        if (canPut(game.field, new Point(x, y), card, mycardSize, group, game.initiation, true))
                        {
                            card.turn = 0;
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        //バーストさせる
        public static int Burst(GameManager game, Size cardSize, int group)
        {
            int groupSum = 0;
            Size fieldSize = game.fieldSize;

            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    if (game.field[x, y].group == group)
                    {
                        game.field[x, y] = new Field();
                        groupSum++;
                    }
                }
            }
            int cardProduct = cardSize.Width * cardSize.Height;
            int groupCardNum = groupSum / cardProduct;
            return groupCardNum;
        }

        /// <summary>
        /// フィールドにカードを配置し、イニシエーションの由来を記録
        /// </summary>
        /// <param name="field"></param>
        /// <param name="putAt"></param>
        /// <param name="card"></param>
        /// <param name="mycardSize"></param>
        /// <returns></returns>
        public static int fillCard(Field[,] field, Point putAt, Card card, Size mycardSize)
        {
            if (connector_group == -1) throw new Exception("connector group is -1");

            List<int> elems = card.elems;
            int turn = card.turn;
            //コネクター配置
            for (int i = 0; i < elems.Count(); i++)
            {
                if (elems[i] == -1) continue;
                int sx, sy;
                switch ((i + turn) % 4)
                {
                    case 0:
                        sx = putAt.X + mycardSize.Width / 2 - 1;
                        sy = putAt.Y;
                        field[sx, sy].connector = elems[i];
                        field[sx + 1, sy].connector = elems[i];
                        break;
                    case 1:
                        sx = putAt.X + mycardSize.Width - 1;
                        sy = putAt.Y + mycardSize.Height / 2;
                        field[sx, sy].connector = elems[i];
                        field[sx, sy - 1].connector = elems[i];
                        break;
                    case 2:
                        sx = putAt.X + mycardSize.Width / 2 - 1;
                        sy = putAt.Y + mycardSize.Height;
                        field[sx, sy - 1].connector = elems[i];
                        field[sx + 1, sy - 1].connector = elems[i];
                        break;
                    case 3:
                        sx = putAt.X;
                        sy = putAt.Y + mycardSize.Height / 2 - 1;
                        field[sx, sy].connector = elems[i];
                        field[sx, sy + 1].connector = elems[i];
                        break;
                }
            }

            //グループ化
            for (int x = 0; x < mycardSize.Width; x++)
            {
                for (int y = 0; y < mycardSize.Height; y++)
                {
                    field[putAt.X + x, putAt.Y + y].group = connector_group;
                }
            }

            //カード配置
            field[putAt.X, putAt.Y].card = card;

            int tmpConnector = connector_group;
            connector_group = -1;

            return tmpConnector;
        }

        /// <summary>
        /// フィールドに配置したカードを除去
        /// </summary>
        /// <param name="field"></param>
        /// <param name="putAt"></param>
        /// <param name="mycardSize"></param>
        static void unfillCard(Field[,] field, Point putAt, Size mycardSize)
        {
            for (int x = 0; x < mycardSize.Width; x++)
            {
                for (int y = 0; y < mycardSize.Height; y++)
                {
                    field[putAt.X + x, putAt.Y + y].group = -1;
                    field[putAt.X + x, putAt.Y + y].connector = -1;
                }
            }
            field[putAt.X, putAt.Y].card = null;
        }

        //バースト時のカード枚数取得
        public static int getSheetsNumber(GameManager game, int group)
        {
            int groupSum = 0;
            Size fieldSize = game.fieldSize;
            Size cardSize = game.cardSize;

            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    if (game.field[x, y].group == group)
                        groupSum++;
                }
            }
            int cardProduct = cardSize.Width * cardSize.Height;
            int groupCardNum = groupSum / cardProduct;

            return groupCardNum;
        }
        
        //低バーストチェック
        public static bool isLowBurst(GameManager game, Point putAt, Card card, Size cardSize)
        {
            //一時的に埋める
            int tmpConnect = fillCard(game.field, putAt, card, cardSize);
            bool tmpInit = game.initiation[tmpConnect];
            game.initiation[tmpConnect] = false;

            for (int i = 0; i < game.max_Player; i++)
            {
                //バーストチェック
                if (!isBurst(game, cardSize, i))
                {
                    continue;
                }

                //バースト枚数取得
                int groupCardNum = getSheetsNumber(game, i);

                if (groupCardNum < 3)
                {
                    //埋めを解除
                    unfillCard(game.field, putAt, cardSize);

                    //判定コネクターを戻す
                    connector_group = tmpConnect;
                    game.initiation[tmpConnect] = tmpInit;
                    return true;
                }
            }
            //埋めを解除
            unfillCard(game.field, putAt, cardSize);

            //判定コネクターを戻す
            connector_group = tmpConnect;
            game.initiation[tmpConnect] = tmpInit;
            return false;
        }

        /// <summary>
        /// 配置可能場所リスト生成
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static List<Card> getCandidates(GameManager game)
        {
            //候補
            List<Card> candidates = new List<Card>();
            
            Size cardSize = game.cardSize;
            Size fieldSize = game.fieldSize;

            foreach (var card in game.nowHandCard)
            {
                if (card == null) continue;
                card.available = false;
                for (int x = 0; x < fieldSize.Width; x++)
                {
                    for (int y = 0; y < fieldSize.Height; y++)
                    {
                        for (int turn = 0; turn < 4; turn++)
                        {
                            Size mycardSize = cardSize;
                            if (turn % 2 == 1)
                            {
                                mycardSize = new Size(cardSize.Height, cardSize.Width);
                            }
                            card.turn = turn;
                            if (canPut(game.field, new Point(x, y), card, mycardSize, game.now_Player, game.initiation, true))
                            {
                                //低バーストでもない場合は候補に追加
                                if (!isLowBurst(game, new Point(x, y), card, mycardSize))
                                {
                                    Card cand = new Card(card);
                                    cand.point = new Point(x, y);
                                    candidates.Add(cand);
                                    card.available = true;
                                }
                            }
                        }
                    }
                }
                card.turn = 0;
            }

            return candidates;
        }

        //カード配置
        public static bool putCard(GameManager game, Point putAt, Card card, Size cardSize, bool[] initiation)
        {
            Field[,] field = game.field;
            Size fieldSize = game.fieldSize;
            int turn = card.turn;

            //カードサイズをターンにあわせる
            Size mycardSize = cardSize;
            if (turn % 2 == 1)
            {
                mycardSize = new Size(cardSize.Height, cardSize.Width);
            }

            //前回のイニシエーション保存
            bool[] oldInitiation = new bool[] { initiation[0], initiation[1] };

            //設置判定
            if (!canPut(field, putAt, card, mycardSize, game.now_Player, initiation)) return false;

            if (isLowBurst(game, putAt, card, mycardSize))
            {
                //イニシエーションの場合は取り消し
                int init_group = game.now_Player;
                if (initiation[init_group] != oldInitiation[init_group])
                    initiation[init_group] = true;
                return false;
            }

            //カード配置
            fillCard(game.field, putAt, card, mycardSize);

            return true;
        }
    }
}

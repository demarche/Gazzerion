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
        public class CanPutInfo
        {
            public int turn;
            public int cardIndex;
            public CanPutInfo(int turn, int cardIndex) { this.turn = turn; this.cardIndex = cardIndex; }
        }
        public List<CanPutInfo> canPutInfo = new List<CanPutInfo>();//配置可能カード情報

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
        public static bool canPut(Field[,] field, Point putAt, Card card, Size mycardSize, bool is1P, bool[] initiation, bool isJudge = false)
        {
            Size fieldSize = new Size(field.GetLength(0), field.GetLength(1));

            //衝突判定
            if (collision(field, putAt, mycardSize)) return false;

            //イニシエーション可能
            if (is1P && initiation[0] && putAt.Y == fieldSize.Height - mycardSize.Height || !is1P && initiation[1] && putAt.Y == 0)
            {
                if (!isJudge) initiation[is1P ? 0 : 1] = false;
                connector_group = is1P ? 0 : 1;
                return true;
            }

            //属性連結可能
            if (canConnect(field, putAt, card, mycardSize)) return true;

            return false;
        }

        //バーストチェック
        public static bool isBurst(GameManager game, Size cardSize, int group)
        {
            Size fieldSize = new Size(game.field.GetLength(0), game.field.GetLength(1));

            Card card = new Card(new List<int> { -1, -1, -1, -1 });
            card.testCard = group;
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
                        if (canPut(game.field, new Point(x, y), card, mycardSize, group == 0, game.initiation, true))
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
            Size fieldSize = new Size(game.field.GetLength(0), game.field.GetLength(1));

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

        //低バーストチェック
        public static bool isLowBurst(GameManager game, Point putAt, Card card, Size cardSize)
        {
            //一時的に埋める
            int tmpConnect = fillCard(game.field, putAt, card, cardSize);
            bool tmpInit = game.initiation[tmpConnect];
            game.initiation[tmpConnect] = false;

            for (int i = 0; i < 2; i++)
            {
                //バーストチェック
                if (!isBurst(game, cardSize, i))
                {
                    continue;
                }

                int groupSum = 0;
                Size fieldSize = new Size(game.field.GetLength(0), game.field.GetLength(1));

                for (int x = 0; x < fieldSize.Width; x++)
                {
                    for (int y = 0; y < fieldSize.Height; y++)
                    {
                        if (game.field[x, y].group == i)
                            groupSum++;
                    }
                }
                int cardProduct = cardSize.Width * cardSize.Height;
                int groupCardNum = groupSum / cardProduct;

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
        /// 詰みチェック
        /// 兼置ける場所の記録
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cardSize"></param>
        /// <returns></returns>
        public static bool isCheckmate(GameManager game, Size cardSize)
        {
            bool is1P = game.is1P;
            Size fieldSize = new Size(game.field.GetLength(0), game.field.GetLength(1));

            //前回のアシスト情報クリア
            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    game.field[x, y].canPutInfo = new List<CanPutInfo>();
                }
            }
            foreach (var card in (is1P ? game.handCard_1p : game.handCard_2p).Select((v, i) => new { v, i }))
            {
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
                            card.v.turn = turn;
                            if (canPut(game.field, new Point(x, y), card.v, mycardSize, is1P, game.initiation, true))
                            {
                                //低バーストでもない場合は詰みでない
                                if (!isLowBurst(game, new Point(x, y), card.v, mycardSize))
                                {/*
                                    card.turn = 0;
                                    return false;*/
                                    game.field[x, y].canPutInfo.Add(new CanPutInfo(card.v.turn, card.i));
                                }
                            }
                        }
                    }
                }
                card.v.turn = 0;
            }

            //カードが置ける場所の候補がない場合false
            return Enumerable.Range(0, fieldSize.Width).Select(x => Enumerable.Range(0, fieldSize.Height).Select(y =>
            game.field[x, y].canPutInfo.Count())//アシストの個数
            .Count(t => t != 0))//アシストの個数が0でないもの
            .Count(t => t != 0)//アシストの個数が0でないものの個数
            == 0;
        }

        //カード配置
        public static bool putCard(GameManager game, Point putAt, Card card, Size cardSize, bool is1P, bool[] initiation)
        {
            Field[,] field = game.field;
            Size fieldSize = new Size(field.GetLength(0), field.GetLength(1));
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
            if (!canPut(field, putAt, card, mycardSize, is1P, initiation)) return false;

            if (isLowBurst(game, putAt, card, mycardSize))
            {
                //イニシエーションの場合は取り消し
                int init_group = is1P ? 0 : 1;
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

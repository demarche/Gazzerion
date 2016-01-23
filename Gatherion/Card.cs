using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;
using DxLibDLL;

namespace Gatherion
{
    public class Card
    {
        //属性
        public List<int> elems;
        //画像のパス
        public string imgPath = "";
        //フィールド上の位置
        [XmlIgnore]
        public Point point = new Point(-1, -1);
        //回転
        [XmlIgnore]
        public int turn = 0;
        //手札のID
        [XmlIgnore]
        public int handCardID = -1;
        //テスト用カード（配置可能検証用）
        [XmlIgnore]
        public int testCard = -1;

        public static Dictionary<string, List<Card>> deckList = new Dictionary<string, List<Card>>();

        static Random rnd = new Random();
        public static Dictionary<string, int> CardGraphDic = new Dictionary<string, int>();

        public Card(List<int> elements)
        {
            elems = elements;
        }

        public Card(List<int> elements, string imgPath)
        {
            elems = elements;
            this.imgPath = imgPath;
        }

        //カードのクローン
        public Card(Card card)
        {
            elems = new List<int>(card.elems);
            imgPath = card.imgPath;
            point = new Point(card.point.X, card.point.Y);
            turn = card.turn;
            handCardID = card.handCardID;
            testCard = card.testCard;
        }

        public Card() { }

        //シリアライザ
        public static void SerializeCards(List<Card> cards)
        {
            XmlSerializer serializer1 = new XmlSerializer(typeof(List<Card>));
            StreamWriter sw = new StreamWriter(@"deck.xml", false, new UTF8Encoding(false));
            serializer1.Serialize(sw, cards);
            sw.Close();
        }

        //デシリアライザ
        static List<Card> DeserializeCards(string path)
        {
            XmlSerializer serializer2 = new XmlSerializer(typeof(List<Card>));
            StreamReader sr = new StreamReader(path, new UTF8Encoding(false));
            List<Card> cards = (List<Card>)serializer2.Deserialize(sr);
            sr.Close();
            return cards;
        }

        //デッキ読み込み
        public static void LoadDeck()
        {
            string[] folders = Directory.GetDirectories("deck");
            foreach(string deckFolderPath in folders)
            {
                string xmlName = Path.Combine(deckFolderPath, "deck.xml");
                if (File.Exists(xmlName))
                {
                    List<Card> deck = DeserializeCards(xmlName);

                    //画像読み込み
                    foreach(Card card in deck)
                    {
                        if (card.imgPath!=""&&!CardGraphDic.ContainsKey(card.imgPath))
                        {
                            string imgPath = Path.GetFullPath(Path.Combine(deckFolderPath, card.imgPath));
                            CardGraphDic[card.imgPath] = DX.LoadGraph(imgPath);
                        }
                    }

                    deckList[Path.GetFileName(deckFolderPath)] = deck;
                }
            }
        }

        //ランダムなカード生成
        public static Card RandomCardGenerator()
        {
            List<int> elements = new List<int>();
            do {
                elements = new List<int>();
                for (int i = 0; i < 4; i++)
                {
                    if (rnd.Next() % 2 == 0) elements.Add(-1);
                    else elements.Add(rnd.Next(5));
                }
            }while (elements.Count(t => t == -1) == 4) ;

            return new Card(elements);
        }

        //カードのシャッフル
        public static void suffleCards(ref List<Card> cards)
        {
            List<int> permutation = Enumerable.Range(0, cards.Count()).ToList();
            for (int i = 0; i < cards.Count(); i++)
            {
                int t = rnd.Next(cards.Count());
                if (t != i)
                {
                    int tmp = permutation[t];
                    permutation[t] = permutation[i];
                    permutation[i] = tmp;
                }
            }

            var cardsTmp = new List<Card>(cards);
            cards = Enumerable.Range(0,cards.Count()).Select(t=>cardsTmp[permutation[t]]).ToList();
        }

    }
}

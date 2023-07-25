using KZHub.CardGenerationService.DTOs.Card;
using KZHub.CardGenerationService.Services.CardProcessing.Interfaces;
using System.Drawing;
using System.Runtime.Versioning;

namespace KZHub.CardGenerationService.Services.CardProcessing
{
    public class CardGenerator : ICardGenerator
    {
        private static SolidBrush drawBrush = new SolidBrush(Color.Black);
        private static StringFormat drawFormat = new StringFormat();

        //Rectangles
        static RectangleF zastepRect = new RectangleF(220, 145, 405, 55);
        static RectangleF dateRect = new RectangleF(80, 275, 500, 50);
        static RectangleF placeRect = new RectangleF(580, 275, 1000, 50);
        static RectangleF requiredItemsRect = new RectangleF(80, 1045, 1000, 280);

        public Bitmap GenerateCard(CreateCardDTO cardDTO)
        {
            Bitmap card = new Bitmap(Path.Combine(Environment.CurrentDirectory, @"Resources\", "karta.png"));

            if (!string.IsNullOrEmpty(cardDTO.Zastep)) DrawZastep(card, cardDTO.Zastep);

            DrawDate(card, cardDTO.Date);

            if (!string.IsNullOrEmpty(cardDTO.Place)) DrawPlace(card, cardDTO.Place);

            if(cardDTO.Points.Count != 0) DrawPoints(card, cardDTO.Points);

            if(!string.IsNullOrEmpty(cardDTO.RequiredItems)) DrawRequiredItems(card, cardDTO.RequiredItems);

            return card;
        }

        private void DrawZastep(Bitmap card, string zastep)
        {
            Font zastepFont = new Font("Arial", 38, FontStyle.Bold);

            if (zastep.Length > 10)
            {
                zastepFont = new Font("Arial", 25, FontStyle.Bold);
            }

            Graphics.FromImage(card).DrawString(zastep, zastepFont, drawBrush, zastepRect, drawFormat);
        }

        private void DrawDate(Bitmap card, DateTime date)
        {
            Font datumFont = new Font("Arial", 25, FontStyle.Bold);

            string month = GetMonthString(date.Month);

            Graphics.FromImage(card).DrawString($"{date.Day} {month} {date.Year}", datumFont, drawBrush, dateRect, drawFormat);
        }

        private void DrawPlace(Bitmap card, string place)
        {
            Font placeFont = new Font("Arial", 30, FontStyle.Bold);

            Graphics.FromImage(card).DrawString(place, placeFont, drawBrush, placeRect);
        }

        private void DrawPoints(Bitmap card, List<CreatePointDTO> points)
        {
            Font pointsFont = new Font("Arial", 20, FontStyle.Bold);

            float height = 585 / points.Count;

            RectangleF pointRect = new RectangleF(220, 400, 515, height);
            RectangleF timeRect = new RectangleF(80, 400, 135, height);
            RectangleF whoRect = new RectangleF(735, 400, 220, height);

            //Draw czas
            DrawTime(card, pointsFont, timeRect, points);

            //Draw coRobimy
            DrawPointTitle(card, pointsFont, pointRect, points);

            //Draw kto
            DrawZastepMember(card, pointsFont, whoRect, points);
        }

        private void DrawZastepMember(Bitmap card, Font pointsFont, RectangleF whoRect, List<CreatePointDTO> points)
        {
            foreach (CreatePointDTO p in points)
            {
                Graphics.FromImage(card).DrawString(p.ZastepMember, pointsFont, drawBrush, whoRect);
                whoRect.Y += whoRect.Height;
            }
        }

        private void DrawPointTitle(Bitmap card, Font pointsFont, RectangleF pointRect, List<CreatePointDTO> points)
        {
            foreach (CreatePointDTO p in points)
            {
                Graphics.FromImage(card).DrawString(p.Title, pointsFont, drawBrush, pointRect);
                pointRect.Y += pointRect.Height;
            }
        }

        private void DrawTime(Bitmap card, Font pointsFont, RectangleF timeRect, List<CreatePointDTO> points)
        {
            foreach (CreatePointDTO p in points)
            {
                Graphics.FromImage(card).DrawString(p.Time.ToString("HH:mm"), pointsFont, drawBrush, timeRect);
                timeRect.Y += timeRect.Height;
            }
        }

        public void DrawRequiredItems(Bitmap card, string? requiredItems)
        {
            Font requiredItemsFont = new Font("Arial", 30, FontStyle.Bold);

            Graphics.FromImage(card).DrawString(requiredItems, requiredItemsFont, drawBrush, requiredItemsRect);
        }

        private string GetMonthString(int month)
        {
            switch (month)
            {
                case 1:
                    return "Stycznia";
                case 2:
                    return "Lutego";
                case 3:
                    return "Marca";
                case 4:
                    return "Kwietnia";
                case 5:
                    return "Maja";
                case 6:
                    return "Czerwca";
                case 7:
                    return "Lipca";
                case 8:
                    return "Sierpnia";
                case 9:
                    return "Września";
                case 10:
                    return "Października";
                case 11:
                    return "Listopada";
                case 12:
                    return "Grudnia";
                default:
                    throw new ArgumentOutOfRangeException("month", $"Cannot convert month name for month: {month} ");
            }
        }
    }
}

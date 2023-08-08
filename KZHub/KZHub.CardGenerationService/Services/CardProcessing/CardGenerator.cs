using KZHub.CardGenerationService.DTOs.Card;
using KZHub.CardGenerationService.Services.CardProcessing.Interfaces;
using SkiaSharp;

namespace KZHub.CardGenerationService.Services.CardProcessing
{
    public class CardGenerator : ICardGenerator
    {
        //Rectangles
        static SKRect zastepRect = new SKRect() { Location = new SKPoint(220, 190), Size = new SKSize(405, 55) };
        static SKRect dateRect = new SKRect() { Location = new SKPoint(85, 305), Size = new SKSize(500, 50) };
        static SKRect placeRect = new SKRect() { Location = new SKPoint(590, 305), Size = new SKSize(1000, 50)};
        static SKRect requiredItemsRect = new SKRect() { Location = new SKPoint(85, 1075), Size = new SKSize(1000, 280)};

        public SKBitmap GenerateCard(CreateCardDTO cardDTO)
        {
            Console.WriteLine("--> Generating card...");
            SKBitmap card;
            using (var fileStream = new FileStream(Path.Combine(Environment.CurrentDirectory, @"Resources", "karta.png"), FileMode.Open))
            {
                 card = SKBitmap.Decode(fileStream);
            }
            Console.WriteLine(card.Width + " " + card.Height + " " + card.ToString());

            using(SKCanvas canvas = new SKCanvas(card))
            {
                if (!string.IsNullOrEmpty(cardDTO.Zastep)) DrawZastep(canvas, cardDTO.Zastep);

                DrawDate(canvas, cardDTO.Date);

                if (!string.IsNullOrEmpty(cardDTO.Place)) DrawPlace(canvas, cardDTO.Place);

                if(cardDTO.Points.Count != 0) DrawPoints(canvas, cardDTO.Points);

                if (!string.IsNullOrEmpty(cardDTO.RequiredItems)) DrawRequiredItems(canvas, cardDTO.RequiredItems);

                //using (var data = card.Encode(SKEncodedImageFormat.Png, 80))
                //{
                //    using (var stream = File.OpenWrite(Path.Combine("C:\\data\\KartaZbiorkiMaker\\Karty", "1.png")))
                //    {
                //        data.SaveTo(stream);
                //    }
                //}
            }

            Console.WriteLine("--> Card was generated!");

            return card;
        }

        private void DrawZastep(SKCanvas card, string zastep)
        {
            SKPaint zastepFont = new SKPaint() { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), TextSize = 38 };

            if (zastep.Length > 10)
            {
                zastepFont.TextSize = 25;
            }

            card.DrawText(zastep, zastepRect.Location, zastepFont);
        }

        private void DrawDate(SKCanvas card, DateTime date)
        {
            SKPaint datumFont = new SKPaint() { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), TextSize = 25 };

            string month = GetMonthString(date.Month);

            card.DrawText($"{date.Day} {month} {date.Year}", dateRect.Location, datumFont);
        }

        private void DrawPlace(SKCanvas card, string place)
        {
            SKPaint placeFont = new SKPaint() { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), TextSize = 30 };

            card.DrawText(place, placeRect.Location, placeFont);
        }

        private void DrawPoints(SKCanvas card, List<CreatePointDTO> points)
        {
            SKPaint pointsFont = new SKPaint() { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), TextSize = 20 };

            float height = 585 / points.Count;

            SKRect pointRect = new SKRect() { Location = new SKPoint(225, 420), Size = new SKSize(515, height)};
            SKRect timeRect = new SKRect() { Location = new SKPoint(85, 420), Size = new SKSize(135, height)};
            SKRect whoRect = new SKRect() { Location = new SKPoint(740, 420), Size = new SKSize(220, height)};

            //Draw czas
            DrawTime(card, pointsFont, timeRect, points);

            //Draw coRobimy
            DrawPointTitle(card, pointsFont, pointRect, points);

            //Draw kto
            DrawZastepMember(card, pointsFont, whoRect, points);
        }

        private void DrawZastepMember(SKCanvas card, SKPaint pointsFont, SKRect whoRect, List<CreatePointDTO> points)
        {
            var top = whoRect.Top;
            var height = whoRect.Height;
            foreach (CreatePointDTO p in points)
            {
                card.DrawText(p.ZastepMember, new SKPoint(whoRect.Left, top), pointsFont);
                top += height;
            }
        }

        private void DrawPointTitle(SKCanvas card, SKPaint pointsFont, SKRect pointRect, List<CreatePointDTO> points)
        {
            var top = pointRect.Top;
            var height = pointRect.Height;
            foreach (CreatePointDTO p in points)
            {
                card.DrawText(p.Title, new SKPoint(pointRect.Left, top), pointsFont);
                top += height;
            }
        }

        private void DrawTime(SKCanvas card, SKPaint pointsFont, SKRect timeRect, List<CreatePointDTO> points)
        {
            var top = timeRect.Top;
            var height = timeRect.Height;
            foreach (CreatePointDTO p in points)
            {
                card.DrawText(p.Time.ToString("HH:mm"), new SKPoint(timeRect.Left, top), pointsFont);
                top += height;
            }
        }

        public void DrawRequiredItems(SKCanvas card, string? requiredItems)
        {
            SKPaint requiredItemsFont = new SKPaint() { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), TextSize = 30 };

            WrapLines(requiredItems, requiredItemsRect.Width, card, requiredItemsFont, requiredItemsRect.Left, requiredItemsRect.Top);
        }

        void WrapLines(string longLine, float lineLengthLimit, SKCanvas canvas, SKPaint defPaint, float x, float y)
        {
            var wrappedLines = new List<string>();
            var lineLength = 0f;
            var line = "";
            foreach (var word in longLine.Split(' '))
            {
                var wordWithSpace = word + " ";
                var wordWithSpaceLength = defPaint.MeasureText(wordWithSpace);
                if (lineLength + wordWithSpaceLength > lineLengthLimit)
                {
                    wrappedLines.Add(line);
                    line = "" + wordWithSpace;
                    lineLength = wordWithSpaceLength;
                }
                else
                {
                    line += wordWithSpace;
                    lineLength += wordWithSpaceLength;
                }
            }

            wrappedLines.Add(line);

            foreach (var wrappedLine in wrappedLines)
            {
                canvas.DrawText(wrappedLine, x, y, defPaint);
                y += defPaint.FontSpacing;
            }
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

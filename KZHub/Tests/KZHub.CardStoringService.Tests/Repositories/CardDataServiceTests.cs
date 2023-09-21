using KZHub.CardStoringService.Data;
using KZHub.CardStoringService.Models;
using KZHub.CardStoringService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KZHub.CardStoringService.Tests.Repositories
{
    public class CardDataServiceTests
    {
        #region Setup / Constructor

        private readonly DataContext _context;

        public CardDataServiceTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("InMemTest")
                .Options;

            _context = new DataContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        #endregion

        [Fact]
        public async Task SaveCard_WithValidCard_ShouldSave()
        {
            var cardDataService = new CardDataService(_context);

            Card expected = new Card()
            {
                Id = 1,
                Zastep = "Wilk",
                Date = DateTime.Now,
                Place = "Kaptur",
                Points = new List<Point>()
                {
                    new Point()
                    {
                        Id = 1,
                        Time = DateTime.Now,
                        Title = "Budowanie Igloo",
                        ZastepMember = "Maciek"
                    }
                },
                RequiredItems = "Nóż, jedzenie"
            };

            await cardDataService.SaveCard(expected);

            var actual = await cardDataService.GetCard(1);
            Assert.NotNull(actual);
            Assert.Equal(1, actual.Id);
            Assert.Equal(expected.Zastep, actual.Zastep);
            Assert.Equal(expected.Date, actual.Date);
            Assert.Equal(expected.Place, actual.Place);
            Assert.NotEmpty(actual.Points);
            
            for(int i=0; i<expected.Points.Count; i++)
            {
                Assert.NotNull(actual.Points[i]);
                Assert.Equal(expected.Points[i].Id, actual.Points[i].Id);
                Assert.Equal(expected.Points[i].Time, actual.Points[i].Time);
                Assert.Equal(expected.Points[i].Title, actual.Points[i].Title);
                Assert.Equal(expected.Points[i].ZastepMember, actual.Points[i].ZastepMember);
            }

            Assert.Equal(expected.RequiredItems, actual.RequiredItems);
        }

        [Fact]
        public async Task SaveCard_WithoutPoints_ShouldSave()
        {
            var cardDataService = new CardDataService(_context);

            Card expected = new Card()
            {
                Id = 1,
                Zastep = "Wilk",
                Date = DateTime.Now,
                Place = "Kaptur",
                RequiredItems = "Nóż, jedzenie"
            };

            await cardDataService.SaveCard(expected);

            var actual = await cardDataService.GetCard(1);
            Assert.NotNull(actual);
            Assert.Equal(1, actual.Id);
            Assert.Equal(expected.Zastep, actual.Zastep);
            Assert.Equal(expected.Date, actual.Date);
            Assert.Equal(expected.Place, actual.Place);
            Assert.Empty(actual.Points);

            Assert.Equal(expected.RequiredItems, actual.RequiredItems);
        }

        [Fact]
        public async Task SaveCard_WithoutPlace_ShouldSave()
        {
            var cardDataService = new CardDataService(_context);

            Card expected = new Card()
            {
                Id = 1,
                Zastep = "Wilk",
                Date = DateTime.Now,
                Points = new List<Point>()
                {
                    new Point()
                    {
                        Id = 1,
                        Time = DateTime.Now,
                        Title = "Budowanie Igloo",
                        ZastepMember = "Maciek"
                    }
                },
                RequiredItems = "Nóż, jedzenie"
            };

            await cardDataService.SaveCard(expected);

            var actual = await cardDataService.GetCard(1);
            Assert.NotNull(actual);
            Assert.Equal(1, actual.Id);
            Assert.Equal(expected.Zastep, actual.Zastep);
            Assert.Equal(expected.Date, actual.Date);
            Assert.Empty(actual.Place);
            Assert.NotEmpty(actual.Points);

            for (int i = 0; i < expected.Points.Count; i++)
            {
                Assert.NotNull(actual.Points[i]);
                Assert.Equal(expected.Points[i].Id, actual.Points[i].Id);
                Assert.Equal(expected.Points[i].Time, actual.Points[i].Time);
                Assert.Equal(expected.Points[i].Title, actual.Points[i].Title);
                Assert.Equal(expected.Points[i].ZastepMember, actual.Points[i].ZastepMember);
            }

            Assert.Equal(expected.RequiredItems, actual.RequiredItems);
        }

        [Fact]
        public async Task SaveCard_WithEmptyCard_ShouldNotSave()
        {
            var cardDataService = new CardDataService(_context);

            Card expected = new Card();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await cardDataService.SaveCard(expected));
        }

        #region Disposable

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            GC.SuppressFinalize(this);
        } 

        #endregion
    }
}

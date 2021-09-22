using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private readonly IBookingManager bookingManager;
        private readonly Mock<IRepository<Booking>> bookingRepository;
        private readonly Mock<IRepository<Room>> roomRepository;

        public BookingManagerTests(){

            bookingRepository = new Mock<IRepository<Booking>>();
            roomRepository = new Mock<IRepository<Room>>();
            bookingManager = new BookingManager(bookingRepository.Object, roomRepository.Object);

            bookingRepository = new Mock<IRepository<Booking>>();

            var fakeBookings = new List<Booking>
            {
                new()
                {
                    Id = 10,
                    CustomerId = 1,
                    RoomId = 1,
                    IsActive = true,
                    StartDate = DateTime.Now.AddMonths(1),
                    EndDate = DateTime.Now.AddMonths(2)
                },
                new()
                {
                    Id = 20,
                    CustomerId = 2,
                    RoomId = 22,
                    IsActive = true,
                    StartDate = DateTime.Now.AddMonths(1),
                    EndDate = DateTime.Now.AddMonths(2)
                }
            };

            bookingRepository.Setup(bookings => bookings.GetAll()).Returns(fakeBookings);

            var fakeRooms = new List<Room>
            {
                new()
                {
                    Description = "Room description",
                    Id = 1,
                },
                new()
                {
                    Description = "Room description",
                    Id = 22
                }
            };

            roomRepository.Setup(fakeRooms => fakeRooms.GetAll()).Returns(fakeRooms);
            
            bookingManager = new BookingManager(bookingRepository.Object, roomRepository.Object);
        }
        
        public static IEnumerable<object[]> GetLocalData()
        {
            DateTime startDate = DateTime.Today.AddMonths(1);
            DateTime endDate = DateTime.Today.AddMonths(2);


            var data = new List<object[]>
            {
                new object[] { startDate.AddMonths(2), endDate.AddMonths(3), true },
                new object[] { startDate, endDate, false },

            };

            return data;
        }

        [Fact]
        public void FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            DateTime date = DateTime.Today;
            Assert.Throws<ArgumentException>(() => bookingManager.FindAvailableRoom(date, date));
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public void GetFullyOccupiedDates_EndDateBeforeStartDate_ThrowsException_WithCorrectMessage()
        {
            // Arange
            var startDate = DateTime.Now.AddDays(10);
            var endDate = DateTime.Now.AddDays(7);
            
            // Act
            var exception = Assert.Throws<ArgumentException>(() => bookingManager.GetFullyOccupiedDates(startDate, endDate));
            
            // Assert
            Assert.Equal(exception.Message, ("The start date cannot be later than the end date."));
        }
        
        [Fact]
        public void GetFullyOccupiedDates_EndDateBeforeStartDate_ThrowsException()
        {
            // Arange
            var startDate = DateTime.Now.AddDays(10);
            var endDate = DateTime.Now.AddDays(7);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => bookingManager.GetFullyOccupiedDates(startDate, endDate));
        }

        [Theory]
        [MemberData(nameof(GetLocalData))]
        public void CreatingABooking_AllRoomsOccupied_ReturnsCorrectBool(DateTime startDate, DateTime endDate, bool expectedResult)
        {
            // Arange
            var booking = new Booking
            {
                CustomerId = 1,
                StartDate = startDate,
                EndDate = endDate
            };
            
            // Act
            bool createBooking = bookingManager.CreateBooking(booking);
            
            // Assert
            Assert.Equal(expectedResult, createBooking);
            //Assert.False(createBooking);
        }
        
        [Fact]
        public void CreatingABooking_RoomsAvailable_ReturnsTrue()
        {
            // Arange
            var booking = new Booking
            {
                Id = 20,
                CustomerId = 2,
                RoomId = 22,
                IsActive = true,
                StartDate = DateTime.Now.AddMonths(2),
                EndDate = DateTime.Now.AddMonths(3)
            };
            
            // Act
            bool createBooking = bookingManager.CreateBooking(booking);
            
            // Assert
            Assert.True(createBooking);
        }

        [Fact]
        public void CreateBooking_Verify_AddBooking()
        {
            // Arange
            var booking = new Booking
            {
                Id = 20,
                CustomerId = 2,
                RoomId = 22,
                IsActive = true,
                StartDate = DateTime.Now.AddMonths(2),
                EndDate = DateTime.Now.AddMonths(3)
            };

            bookingRepository.Setup(bookings => bookings.Add(It.IsAny<Booking>()));

            // Act
            bool createBooking = bookingManager.CreateBooking(booking);
            
            // Verify
            bookingRepository.Verify(x=>x.Add(It.Is<Booking>(_ => _.Id == booking.Id)), Times.Once());
        }
    }
}

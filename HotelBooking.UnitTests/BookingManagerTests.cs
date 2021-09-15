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
        private IBookingManager bookingManager;
        Mock<IRepository<Booking>> fakeBookingRepository;
        Mock<IRepository<Room>> fakeRoomRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            IRepository<Booking> bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);

            fakeBookingRepository = new Mock<IRepository<Booking>>();

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

            fakeBookingRepository.Setup(bookings => bookings.GetAll()).Returns(fakeBookings);

            fakeRoomRepository = new Mock<IRepository<Room>>();

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

            fakeRoomRepository.Setup(fakeRooms => fakeRooms.GetAll()).Returns(fakeRooms);
            
            bookingManager = new BookingManager(fakeBookingRepository.Object, fakeRoomRepository.Object);
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

        [Fact]
        public void CreatingABooking_AllRoomsOccupied_ReturnsFalse()
        {
            // Arange
            var booking = new Booking
            {
                Id = 20,
                CustomerId = 2,
                RoomId = 22,
                IsActive = true,
                StartDate = DateTime.Now.AddMonths(1),
                EndDate = DateTime.Now.AddMonths(2)
            };
            
            // Act
            bool createBooking = bookingManager.CreateBooking(booking);
            
            // Assert
            Assert.False(createBooking);
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
    }
}

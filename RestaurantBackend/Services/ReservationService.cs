using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace RestaurantBackend.Services
{
    public interface IReservationService
    {
        Task<Reservation?> CreateReservationAsync(ReservationVM model);
        Task<Reservation?> UpdateReservationAsync(ReservationVM model);
        Task<bool> CancelReservationAsync(int reservationId);
        Task<Reservation?> GetReservationByIdAsync(int reservationId);
        Task<IEnumerable<Reservation>> GetReservationsByEmailAsync(string email);
        Task<IEnumerable<Reservation>> GetAllReservationsAsync();
        Task SendConfirmationEmail(Customer customer, Reservation reservation);
        Task<IEnumerable<string>> GetAvailableSlotsAsync(DateTime date);
    }

    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Customer> _userManager;
        private readonly ILogger<ReservationService> _logger;
        private readonly IConfiguration _configuration;

        public ReservationService(ApplicationDbContext context, UserManager<Customer> userManager,
            ILogger<ReservationService> logger, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Reservation?> CreateReservationAsync(ReservationVM model)
        {
            _logger.LogInformation("Checking user existence by email: {Email}", model.Email);
            var customer = await _userManager.FindByEmailAsync(model.Email);

            if (customer == null)
            {
                customer = new Customer
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(customer);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create a new user for the email: {Email}. Errors: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }
            }

            DateTime reservationDateTime;
            if (!TryParseReservationDateTime(model.ReservationDate, model.ReservationTime, out reservationDateTime))
            {
                return null;
            }

            if (!await IsReservationSlotAvailableAsync(reservationDateTime))
            {
                _logger.LogWarning("Requested reservation slot is unavailable: {ReservationDateTime}", reservationDateTime);
                return null;
            }

            var reservation = new Reservation
            {
                UserId = customer.Id,
                ReservationDateTime = reservationDateTime,
                NumberOfGuests = model.NumberOfGuests,
                SpecialRequests = string.IsNullOrEmpty(model.SpecialRequests) ? "No special requests" : model.SpecialRequests
            };

            try
            {
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                await SendConfirmationEmail(customer, reservation);
                return reservation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving the reservation.");
                return null;
            }
        }

        public async Task<Reservation?> UpdateReservationAsync(ReservationVM model)
        {
            if (model.ReservationId == 0)
            {
                _logger.LogError("Reservation ID is zero or invalid.");
                return null;
            }

            DateTime reservationDateTime;
            if (!TryParseReservationDateTime(model.ReservationDate, model.ReservationTime, out reservationDateTime))
            {
                return null;
            }

            var existingReservation = await _context.Reservations.Include(r => r.User).FirstOrDefaultAsync(r => r.ReservationId == model.ReservationId);
            if (existingReservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found for update.", model.ReservationId);
                return null;
            }

            var customer = existingReservation.User;
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for reservation ID {ReservationId}.", model.ReservationId);
                return null;
            }

            UpdateCustomerDetails(customer, model);
            await _userManager.UpdateAsync(customer);

            existingReservation.ReservationDateTime = reservationDateTime;
            existingReservation.NumberOfGuests = model.NumberOfGuests;
            existingReservation.SpecialRequests = model.SpecialRequests;

            _context.Reservations.Update(existingReservation);
            await _context.SaveChangesAsync();

            await SendUpdateEmail(customer, existingReservation);
            return existingReservation;
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {id} not found for cancellation.", reservationId);
                return false;
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cancelled reservation ID {id}", reservationId);
            return true;
        }

        public async Task<Reservation?> GetReservationByIdAsync(int reservationId)
        {
            return await _context.Reservations.Include(r => r.User).FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByEmailAsync(string email)
        {
            var customer = await _userManager.FindByEmailAsync(email);
            if (customer == null)
            {
                return Enumerable.Empty<Reservation>();
            }

            return await _context.Reservations.Where(r => r.UserId == customer.Id).ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
        {
            try
            {
                return await _context.Reservations.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all reservations.");
                throw;
            }
        }

        public async Task<bool> IsReservationSlotAvailableAsync(DateTime desiredReservationTime)
        {
            return !await _context.Reservations.AnyAsync(r => r.ReservationDateTime == desiredReservationTime);
        }

        public async Task SendConfirmationEmail(Customer customer, Reservation reservation)
        {
            if (customer == null || string.IsNullOrEmpty(customer.Email))
            {
                _logger.LogError("Invalid customer data. Email is missing.");
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(customer);

            string frontendBaseUrl = _configuration["FrontendBaseUrl"];
            var tokenEncoded = WebUtility.UrlEncode(token);
            var emailEncoded = WebUtility.UrlEncode(customer.Email);
            var manageReservationLink = $"{frontendBaseUrl}/reservation-details?token={tokenEncoded}&email={emailEncoded}";

            _logger.LogInformation($"Manage reservation link: {manageReservationLink}");

            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_configuration["EmailSettings:EmailAddress"], _configuration["EmailSettings:Password"]),
                    EnableSsl = true,
                };

                using var message = new MailMessage(_configuration["EmailSettings:EmailAddress"], customer.Email)
                {
                    Subject = "Reservation Confirmation",
                    Body = $@"
                        <html>
                            <body>
                                <p>Hello {customer.FirstName},</p>
                                <p>Your reservation has been confirmed. Here are the details:</p>
                                <p><strong>Reservation DateTime:</strong> {reservation.ReservationDateTime}</p>
                                <p><strong>Number of Guests:</strong> {reservation.NumberOfGuests}</p>
                                <p>If you have any questions, please contact us.</p>
                                <p>If you want to manage your reservation, click on this link:</p>
                                <p><a href='{manageReservationLink}'>Manage Reservation</a></p>                                
                            </body>
                        </html>",
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation("Sent confirmation email to {Email}", customer.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", customer.Email);
            }
        }

        public async Task SendUpdateEmail(Customer customer, Reservation reservation)
        {
            if (customer == null || string.IsNullOrEmpty(customer.Email))
            {
                _logger.LogError("Invalid customer data. Email is missing.");
                return;
            }

            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_configuration["EmailSettings:EmailAddress"], _configuration["EmailSettings:Password"]),
                    EnableSsl = true,
                };

                using var message = new MailMessage(_configuration["EmailSettings:EmailAddress"], customer.Email)
                {
                    Subject = "Reservation Update",
                    Body = $@"
                        <html>
                            <body>
                                <p>Hello {customer.FirstName},</p>
                                <p>Your reservation has been updated successfully. Here are the updated details:</p>
                                <p><strong>Reservation DateTime:</strong> {reservation.ReservationDateTime}</p>
                                <p><strong>Number of Guests:</strong> {reservation.NumberOfGuests}</p>
                                <p>If you have any questions, please contact us.</p>
                            </body>
                        </html>",
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation("Sent update confirmation email to {Email}", customer.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send update confirmation email to {Email}", customer.Email);
            }
        }

        private bool TryParseReservationDateTime(string reservationDate, string reservationTime, out DateTime reservationDateTime)
        {
            reservationDateTime = DateTime.MinValue;
            if (!DateTime.TryParseExact(reservationDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime date))
            {
                _logger.LogError("Invalid reservation date format: {ReservationDate}", reservationDate);
                return false;
            }

            string timeStr = reservationTime.Contains("T") ? DateTime.Parse(reservationTime).ToString("HH:mm") : reservationTime;

            if (!DateTime.TryParseExact(timeStr, "HH:mm", null, DateTimeStyles.None, out DateTime time))
            {
                _logger.LogError("Invalid reservation time format: {ReservationTime}", timeStr);
                return false;
            }

            reservationDateTime = date.Add(time.TimeOfDay);
            return true;
        }

        private void UpdateCustomerDetails(Customer customer, ReservationVM model)
        {
            if (customer.FirstName != model.FirstName || customer.LastName != model.LastName || customer.Email != model.Email || customer.PhoneNumber != model.PhoneNumber)
            {
                customer.FirstName = model.FirstName;
                customer.LastName = model.LastName;
                customer.Email = model.Email;
                customer.PhoneNumber = model.PhoneNumber;
            }
        }

        public async Task<IEnumerable<string>> GetAvailableSlotsAsync(DateTime date)
        {
            return await _context.Reservations
                .Where(r => r.ReservationDateTime.Date == date.Date)
                .Select(r => r.ReservationDateTime.ToString("HH:mm"))
                .ToListAsync();
        }
    }
}

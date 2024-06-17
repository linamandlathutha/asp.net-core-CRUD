using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using WebApplication10.Models;
using Microsoft.Azure.Cosmos;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using SendGrid.Helpers.Mail;

namespace WebApplication10.Controllers
{
    public class StudentController : Controller
    {
        private readonly StudentRepository _studentRepository;

        public StudentController(CosmosClient cosmosClient)
        {
            _studentRepository = new StudentRepository(cosmosClient, "futureville", "container1");
        }


        public async Task<IActionResult> Index(string searchTerm)
        {
            IEnumerable<Student> students;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                students = await _studentRepository.SearchStudentsByNameAsync(searchTerm);
            }
            else
            {
                students = await _studentRepository.GetAllStudentsAsync();
            }

            return View(students);
        }

        public IActionResult Home()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult LogIn()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            await _studentRepository.CreateStudentAsync(student);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(string id)
        {
            return View(await _studentRepository.GetStudentByIdAsync(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Student student)
        {
            if (student.id == null)
            {
                return NotFound("The Client Posted a A Student Model with a null id");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _studentRepository.UpdateStudentAsync(student);
                }
                catch (Exception ex)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        public IActionResult Delete()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound("student with the specified id could not be found because the Id is null");
            }

            var student = await _studentRepository.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound("student with the specified id could not be found");
            }

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var stud = await _studentRepository.GetStudentByIdAsync(id);
            if (stud == null)
                throw new Exception($"Could not find Item with id: {id}");

            await _studentRepository.DeleteStudentAsync(stud);
            return RedirectToAction(nameof(Index));
        }

        private async Task SendStudentDetailsByEmail(Student student)
        {
            
            try
            {
                var smtpClient = new MailKit.Net.Smtp.SmtpClient();

                await smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync("tradingviewmendo@gmail.com", "zvbdjiryrivqnrem");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Futureville", "tradingviewmendo@gmail.com"));
                message.To.Add(new MailboxAddress(student.Name, student.EmailAddress));
                message.Subject = "Student Details";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
            <h1>Student Details</h1>
            <p>Student ID: {student.StudentID}</p>
            <p>Name: {student.Name}</p>
            <p>Surname: {student.Surname}</p>
            <p>Email Address: {student.EmailAddress}</p>
            <p>Mobile Number: {student.MobileNumber}</p>
            <p>Active: {(student.IsActive ? "Yes" : "No")}</p>
        ";

                message.Body = bodyBuilder.ToMessageBody();

                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error sending email: {ex.Message}");
                // Handle exception as needed
                throw;
            }






        }

            











        [HttpGet]
        public async Task<IActionResult> Display(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _studentRepository.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            await SendStudentDetailsByEmail(student);

            return View(student);
        }

        
        [HttpPost]
        public async Task<IActionResult> SendEmail(string studentId)
        {
            // Retrieve the student details from the repository using the studentId
            var student = await _studentRepository.GetStudentByIdAsync(studentId);

            if (student == null)
            {
                // Handle the case where the student with the given ID is not found
                return NotFound();
            }

            // Send email containing student details
            await SendStudentDetailsByEmail(student);

            // Redirect back to the Display page with the student ID
            return RedirectToAction("Display", new { id = studentId });
        }


    }
}

﻿using NUnit.Framework;
using MvcContrib.TestHelper;
using Northwind.Controllers;
using SharpArch.Core.PersistenceSupport;
using Northwind.Core;
using Rhino.Mocks;
using NUnit.Framework.SyntaxHelpers;
using System.Web.Mvc;
using System.Collections.Generic;
using SharpArch.Testing;

namespace Tests.Northwind.Controllers
{
    [TestFixture]
    public class EmployeesControllerTests
    {
        [Test]
        public void CanListEmployees() {
            EmployeesController controller = new EmployeesController(CreateMockEmployeeRepository());
            ViewResult result = controller.Index().AssertViewRendered();

            Assert.That(result.ViewData.Model as List<Employee>, Is.Not.Null);
            Assert.That((result.ViewData.Model as List<Employee>).Count, Is.EqualTo(2));
        }

        [Test]
        public void CanShowEmployee() {
            EmployeesController controller = new EmployeesController(CreateMockEmployeeRepository());
            ViewResult result = controller.Show(1).AssertViewRendered();

            Assert.That(result.ViewData.Model as Employee, Is.Not.Null);
            Assert.That((result.ViewData.Model as Employee).ID, Is.EqualTo(1));
        }

        [Test]
        public void CanInitEmployeeCreation() {
            EmployeesController controller = new EmployeesController(CreateMockEmployeeRepository());
            ViewResult result = controller.Create().AssertViewRendered();

            Assert.That(result.ViewData.Model as Employee, Is.Null);
        }

        [Test]
        public void CanEnsureEmployeeCreationIsValid() {
            EmployeesController controller = new EmployeesController(CreateMockEmployeeRepository());
            Employee employeeFromForm = new Employee();
            ViewResult result = controller.Create(employeeFromForm).AssertViewRendered();

            Assert.That(result.ViewData.Model as Employee, Is.Null);
            Assert.That(result.ViewData.ModelState.Count, Is.EqualTo(3));
            Assert.That(result.ViewData.ModelState["Employee.FirstName"].Errors[0].ErrorMessage, Is.Not.Empty);
            Assert.That(result.ViewData.ModelState["Employee.LastName"].Errors[0].ErrorMessage, Is.Not.Empty);
            Assert.That(result.ViewData.ModelState["Employee.PhoneExtension"].Errors[0].ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void CanCreateEmployee() {
            EmployeesController controller = new EmployeesController(CreateMockEmployeeRepository());
            Employee employeeFromForm = new Employee() {
                FirstName = "Jackie",
                LastName = "Daniels",
                PhoneExtension = 350
            };
            RedirectToRouteResult redirectResult = controller.Create(employeeFromForm)
                .AssertActionRedirect().ToAction("Index");
            Assert.That(controller.TempData["message"], Is.EqualTo("Daniels, Jackie was successfully created."));
        }

        [Test]
        public void CanInitEmployeeEdit() {
            EmployeesController controller = new EmployeesController(CreateMockEmployeeRepository());
            ViewResult result = controller.Edit(1).AssertViewRendered();

            Assert.That(result.ViewData.Model as Employee, Is.Not.Null);
            Assert.That((result.ViewData.Model as Employee).ID, Is.EqualTo(1));
        }

        [Test]
        public void CanUpdateEmployee() {
            EmployeesController controller = new EmployeesController(CreateMockEmployeeRepository());
            Employee employeeFromForm = new Employee() {
                FirstName = "Jackie",
                LastName = "Daniels",
                PhoneExtension = 350
            };
            RedirectToRouteResult redirectResult = controller.Edit(1, employeeFromForm)
                .AssertActionRedirect().ToAction("Index");
            Assert.That(controller.TempData["message"], Is.EqualTo("Daniels, Jackie was successfully updated."));
        }

        [Test]
        public void CanDeleteEmployee() {
            EmployeesController controller = new EmployeesController(CreateMockEmployeeRepository());
            RedirectToRouteResult redirectResult = controller.Delete(1)
                .AssertActionRedirect().ToAction("Index");
            Assert.That(controller.TempData["message"], Is.EqualTo("The employee was successfully deleted."));
        }

        private IRepository<Employee> CreateMockEmployeeRepository() {
            MockRepository mocks = new MockRepository();

            IRepository<Employee> mockedRepository = mocks.StrictMock<IRepository<Employee>>();
            Expect.Call(mockedRepository.GetAll())
                .Return(CreateEmployees());
            Expect.Call(mockedRepository.Get(1)).IgnoreArguments()
                .Return(CreateEmployee());
            Expect.Call(mockedRepository.SaveOrUpdate(null)).IgnoreArguments()
                .Return(CreateEmployee());
            Expect.Call(delegate { mockedRepository.Delete(null); }).IgnoreArguments();

            IDbContext mockedDbContext = mocks.StrictMock<IDbContext>();
            Expect.Call(delegate { mockedDbContext.CommitChanges(); });
            SetupResult.For(mockedRepository.DbContext).Return(mockedDbContext);
            
            mocks.Replay(mockedRepository);

            return mockedRepository;
        }

        private Employee CreateEmployee() {
            Employee employee = new Employee("Johnny", "Appleseed");
            PersistentObjectIdSetter<int>.SetIdOf(employee, 1);
            return employee;
        }

        private List<Employee> CreateEmployees() {
            List<Employee> employees = new List<Employee>();

            employees.Add(new Employee("John", "Wayne"));
            employees.Add(new Employee("Joe", "Bradshaw"));

            return employees;
        }
    }
}

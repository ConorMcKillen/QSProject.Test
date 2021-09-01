using System;
using Xunit;
using QSProject.Data.Services;
using QSProject.Data.Models;

namespace QSProject.Test
{
    public class PatientServiceTests
    {
        private readonly IMedicineService svc;

        public PatientServiceTests()
        {
            svc = new MedicineServiceDb();

            svc.Initialise();
        }

        [Fact]
        public void Patient_GetPatientsWhenNone_ShouldReturnNone()
        {
            var patients = svc.GetPatients();
            var count = patients.Count;

            Assert.Equal(0, count);
        }

        [Fact]
        public void Patient_AddPatientUnique_ShouldSetAllProperties()
        {
            var o = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");
            var p = svc.GetPatient(o.Id);

            Assert.NotNull(p);
            Assert.Equal(p.Id, p.Id);
            Assert.Equal("John", p.Name);
            Assert.Equal("john@email.com", p.Email);
            Assert.Equal(30, p.Age);
            Assert.Equal("https://photo.com", p.PhotoUrl);
        }

        [Fact]
        public void Patient_AddWhenDuplicateEmail_ShouldReturnNull()
        {
            var p1 = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");

            // add duplicate

            var p2 = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");

            // assert
            Assert.NotNull(p1);
            Assert.Null(p2);
        }

        [Fact]
        public void Patient_UpdateWhenExists_ShouldSetAllProperties()
        {
            // arrange - create test patient
            var o = svc.AddPatient("Pat", 50, "pat@email.com", "https://photo.com");

            // act - update test patient
            o.Name = "John";
            o.Email = "john@email.com";
            o.Age = 30;
            o.PhotoUrl = "https://zzz.com";

            o = svc.UpdatePatient(o);

            // assert

            Assert.NotNull(o);
            Assert.Equal("John", o.Name);
            Assert.Equal("john@email.com", o.Email);
            Assert.Equal(30, o.Age);
            Assert.Equal("https://zzz.com", o.PhotoUrl);
        }

        [Fact]
        public void Patient_GetPatientsWhenTwoAdded_ShouldReturnTwo()
        {
            // arrange
            var p1 = svc.AddPatient("Pat", 50, "pat@email.com", "https://photo.com");
            var p2 = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");

            // act
            var patients = svc.GetPatients();
            var count = patients.Count;

            // assert
            Assert.Equal(2, count);
        }

        [Fact]
        public void Patient_GetPatientWhenNone_ShouldReturnNull()
        {
            // arrange

            // act
            var patient = svc.GetPatient(1); // non existent patient

            // assert
            Assert.Null(patient);
        }

        [Fact]
        public void Patient_GetPatientThatExists_ShouldReturnPatient()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");

            // act
            var s = svc.GetPatient(p.Id);

            // assert
            Assert.NotNull(s);
            Assert.Equal(p.Id, s.Id);
        }

        [Fact]
        public void Patient_DeletePatientThatExists_ShouldReturnTrue()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");

            // act
            var deleted = svc.DeletePatient(p.Id);
            var p1 = svc.GetPatient(p.Id); // try to retrieve deleted patient

            // assert
            Assert.True(deleted); // delete patient should return true
            Assert.Null(p1); // p1 should be null
        }

        [Fact]
        public void Patient_DeletePatientThatExists_ShouldReducePatientCountByOne()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");

            // act
            var deleted = svc.DeletePatient(p.Id);
            var patients = svc.GetPatients();

            // assert
            Assert.Equal(0, patients.Count); // should be 0 patients
        }

        [Fact]
        public void Patient_DeletePatientThatDoesntExist_ShouldNotChangePatientCount()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");

            // act
            svc.DeletePatient(0); // delete non existent patient
            var patients = svc.GetPatients(); // retrieve list of patients

            // assert
            Assert.Equal(1, patients.Count); // should be 1 patient
        }

        // -------------------------- Medicine Request Test ---------------------------- //

        [Fact]
        public void Medicine_CreateMedicineRequestForExistingPatient_ShouldBeActive()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");

            // act
            var m = svc.CreateMedicineRequest(p.Id, "Request");

            // assert
            Assert.True(m.Active);
        }

        [Fact]
        public void Medicine_GetOpenMedicineRequestWhenTwoAdded_ShouldReturnTwo()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");
            var m1 = svc.CreateMedicineRequest(p.Id, "Request 1");
            var m2 = svc.CreateMedicineRequest(p.Id, "Request 2");

            // act
            var open = svc.GetOpenMedicineRequests();

            // assert
            Assert.Equal(2, open.Count);
        }

        [Fact]
        public void Medicine_CloseMedicineRequestWhenOpen_ShouldReturnMedicineRequest()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");
            var m = svc.CreateMedicineRequest(p.Id, "Request");

            // act
            var r = svc.CloseMedicineRequest(m.Id, "Resolved");

            // assert
            Assert.NotNull(r); // verify closed medicine request returned
            Assert.False(r.Active); // verify its closed
            Assert.Equal("Resolved", m.Resolution); // verify the resolution
            Assert.NotEqual(DateTime.MinValue, r.ResolvedOn);
        }

        [Fact]
        public void Medicine_CloseMedicineRequestWhenAlreadyClosed_ShouldReturnNull()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");
            var m = svc.CreateMedicineRequest(p.Id, "Request");

            // act
            var closed = svc.CloseMedicineRequest(m.Id, "Resolved"); // close active medicine request
            closed = svc.CloseMedicineRequest(m.Id); // close non active ticket

            // assert
            Assert.Null(closed); // no ticket returned as already closed
        }

        [Fact]
        public void Medicine_GetAllMedicineRequestsWhenOneOpenAndOneClosed_ShouldReturnTwo()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");

            var m1 = svc.CreateMedicineRequest(p.Id, "Request 1");
            var m2 = svc.CreateMedicineRequest(p.Id, "Request 2");
            var closed = svc.CloseMedicineRequest(m1.Id, "Resolved"); // closed one medicine request

            // act
            var medicines = svc.GetAllMedicineRequests(); // get all medicine requests

            // assert
            Assert.Equal(2, medicines.Count);
        }

        [Fact]
        public void Medicine_SearchMedicineRequestsWhenTwoResultsAvailableInAllMedicineRequests_ShouldReturnTwo()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");
            var m1 = svc.CreateMedicineRequest(p.Id, "Request 1");
            var m2 = svc.CreateMedicineRequest(p.Id, "Request 2");
            var closed = svc.CloseMedicineRequest(m1.Id, "Resolved"); // close one medicine request

            // act
            var medicines = svc.SearchMedicineRequests(MedicineRange.ALL, "Request"); // search all medicine requests

            // assert
            Assert.Equal(2, medicines.Count);
        }

        [Fact]
        public void Medicine_SearchMedicineRequestsWhenOneResultAvailableInOpenMedicineRequests_ShouldReturnOne()
        {
            // arrange
            var p = svc.AddPatient("John", 30, "john@email.com", "https://photo.com");
            var m1 = svc.CreateMedicineRequest(p.Id, "Request 1");
            var m2 = svc.CreateMedicineRequest(p.Id, "Request 2");
            var closed = svc.CloseMedicineRequest(m1.Id, "Resolved"); // close one medicine request

            // act
            var medicines = svc.SearchMedicineRequests(MedicineRange.OPEN, "Request"); // search open medicine requests

            // assert
            Assert.Equal(1, medicines.Count);
        }

        // ------------------------ User Tests -------------------------- //

        [Fact] // Register Valid User Test
        public void User_Register_WhenValid_ShoudlReturnUser()
        {
            // arrange
            var reg = svc.Register("John","john@email.com", "staff", Role.staff);

            // act
            var user = svc.GetUserByEmail(reg.Email);

            // assert
            Assert.NotNull(reg);
            Assert.NotNull(user);
        }

        [Fact] // Register Duplicate Test
        public void User_Register_WhenDuplicateEmail_ShouldReturnNull()
        {
            // arrange
            var p1 = svc.Register("John", "john@email.com", "staff", Role.staff);

            // act
            var p2 = svc.Register("John", "john@email.com", "staff", Role.staff);

            // assert
            Assert.NotNull(p1);
            Assert.Null(p2);
        }

        [Fact] // Authenticate Invalid Test
        public void User_Authenticate_WhenInValidCredentials_ShouldReturnNull()
        {
            // arrange
            var p = svc.Register("John", "john@email.com", "staff", Role.staff);

            // act
            var user = svc.Authenticate("john@email.com", "patient");

            // assert
            Assert.Null(user);
        }

        [Fact] // Authenticate Valid Test
        public void User_Authenticate_WhenValidCredentials_ShouldReturnUser()
        {
            // arrange
            var p = svc.Register("John", "john@email.com", "staff", Role.staff);

            // act
            var user = svc.Authenticate("john@email.com", "staff");

            // assert
            Assert.NotNull(user);
        }


    }
}

using ContactsConsoleAPI.Business;
using ContactsConsoleAPI.Business.Contracts;
using ContactsConsoleAPI.Data.Models;
using ContactsConsoleAPI.DataAccess;
using ContactsConsoleAPI.DataAccess.Contrackts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestContactDbContext dbContext; //TestC....e basa danni sk. m. d. si vzaimodestwame
        private IContactManager contactManager; // IContactMa...e neshtoto koeto shte testvame

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestContactDbContext();
            this.contactManager = new ContactManager(new ContactRepository(this.dbContext)); // tova indikira che tezi niqkolko modula sa indikirani da rabotiat zaedno
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();   //do tuk v Teardown ni kazva che sled vseki test se ztriva cilata baz ot danni i pri vseki test se re initializira , za da si garantirame nikakava isolatsia pri run-vane!!!!
        }


        //positive test
        [Test]
        public async Task AddContactAsync_ShouldAddNewContact()
        {  
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            // Act
            await contactManager.AddAsync(newContact);

            // Assert

            var dbContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID);// tyrsi dali tozi kontakt e vliazal v bazata ot Danni dbContext.Contacts i sledva ako e vliazla i ne e Null proveriavame vsiako edno ot negovite propartite dali  esashtoto kato tova koeto sme podali pri izvikvaneto na AddAsunc komandata

            Assert.NotNull(dbContact);
            Assert.AreEqual(newContact.FirstName, dbContact.FirstName);
            Assert.AreEqual(newContact.LastName, dbContact.LastName);
            Assert.AreEqual(newContact.Phone, dbContact.Phone);
            Assert.AreEqual(newContact.Email, dbContact.Email);
            Assert.AreEqual(newContact.Address, dbContact.Address);
            Assert.AreEqual(newContact.Contact_ULID, dbContact.Contact_ULID);
        }

        //Negative test s nakoi nevalidno property v sluchaya email
        [Test]
        public async Task AddContactAsync_TryToAddContactWithInvalidCredentials_ShouldThrowException()
        {   
            //Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "invalid_Mail", // tova e invalid email
                Gender = "Male",
                Phone = "0889933779"
            };
             //Act and Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await contactManager.AddAsync(newContact)); // tuk proveriavame dali hvarlia greshka 
            var actual = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID); //tuk proveriavame che neshto ne e propusnato ot pbaza danni  

            Assert.IsNull(actual);
            Assert.That(ex?.Message, Is.EqualTo("Invalid contact!"));

        }

        [Test]
        public async Task DeleteContactAsync_WithValidULID_ShouldRemoveContactFromDb()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            
            await contactManager.AddAsync(newContact); //sega shte dobavim v baza danni posredstwvom KOMPONENTA kojto testvame taj katotoj ni predostavi dopalnitelna validacia za tova che dannite koito slagame v bazata danni vsichko e nared s tiah!!!!!!! NO moje i directno da go slojim v bazta danni s  dbContext.Contacts.Add();

            // Act 
            await contactManager.DeleteAsync(newContact.Contact_ULID); // poneje triem i ne ni vrashta nishto , t.e. toj evoid prosto si go await-vame

            
            
            await contactManager.DeleteAsync(newContact.Contact_ULID);
            //Assert  -----proveriavame dali tozi Contact go niama veche v bazata danni chrez kato ni namira kontakta (x=>x.Contact_ULID) po tozi ULID
            var contactInDb = await dbContext.Contacts.FirstOrDefaultAsync(x => x.Contact_ULID == newContact.Contact_ULID);  
            Assert.IsNull(contactInDb); //zashtot nie smen go iztrili 
            //Assert.Inconclusive("Test not implemented yet.");
        }

        [Test]
        public async Task DeleteContactAsync_TryToDeleteWithNullOrWhiteSpaceULID_ShouldThrowException()
        {
            //    // Arrange
            //    var newContact = new Contact()
            //    {
            //        FirstName = "TestFirstName",
            //        LastName = "TestLastName",
            //        Address = "Anything for testing address",
            //        Contact_ULID = " ", //must be minimum 10 symbols - numbers or Upper case letters
            //        Email = "test@gmail.com",
            //        Gender = "Male",
            //        Phone = "0889933779"
            //    };


            //    await contactManager.DeleteAsync(newContact_ULID);
            //    //Act and Assert
            //    var ex = Assert.ThrowsAsync<ValidationException>(async () => await contactManager.DeleteAsync(newContact_ULID)); // tuk proveriavame dali hvarlia greshka 
            //    var actual = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID); //tuk proveriavame che neshto ne e propusnato ot pbaza danni  

            //    Assert.IsNull(actual);
            //    Assert.That(ex?.Message, Is.EqualTo("Invalid contact!"));

            //    // Assert  TUK SAMO AKT AND ASSERT !!!!!!! SPORED TEO GORNOTO E IZLISHNO ZASHTO HVARLIA GRESHKA ( PTOVERIAVAME |SI KAKVA GRESHKA HVARLYA FUNKCIQta \\\\\\\\     DELETEASYNC  v CONTACT MANAGER  !!!!!!  ULID cannot be empty.
            //    Assert.Inconclusive("Test not implemented yet.");
            Assert.ThrowsAsync<ArgumentException>(() => contactManager.DeleteAsync(null));
            Assert.ThrowsAsync<ArgumentException>(() => contactManager.DeleteAsync(" "));

            // drug nachin e da se zapishe taak spodawane na niakolko parametara
            // [TestCase("")]
            // [TestCase("    ")]
            // [TestCase(null)
            // public async Task DeleteContactAsync_TryToDeleteWithNullOrWhiteSpaceULID_ShouldThrowException(string invalidULID) or invalidUlid)
            // Act  and Assert ===> moje da si vzemem mesega koito se hvarlia pri greshka
            // var exception = Assert.ThrowsAsync<ArgumentException>(() => contactManager.DeleteAsync(invalidUlid));
            // Assert.That(ex?.Message, IsEqualTo("ULID cannot be empty."); taka assertvame samiq mwessage za greshka !!!

        }

        [Test]
        public async Task GetAllAsync_WhenContactsExist_ShouldReturnAllContacts()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };
            var secondNewContact = new Contact() 
            {
                FirstName = "SecondTestFirstName",
                LastName = "SecondTestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "2ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933778"
            };

            await contactManager.AddAsync(newContact);
            await contactManager.AddAsync(secondNewContact); //sega  dobaviame i dvata v baza danni posredstwvom KOMPONENTA kojto testvame taj katotoj ni predostavi dopalnitelna validacia za tova che dannite koito slagame v bazata danni vsichko e nared s tiah!!!!!!! NO moje i directno da go slojim v bazta danni s  dbContext.Contacts.Add();
            // Act
            var result = await contactManager.GetAllAsync();

            // Assert
           Assert.That(result.Count(), Is.EqualTo(2));

            var firstContact = result.First();
            Assert.That(firstContact.Address, Is.EqualTo(newContact.Address));
            Assert.That(firstContact.Contact_ULID, Is.EqualTo(newContact.Contact_ULID));
            Assert.That(firstContact.Gender, Is.EqualTo(newContact.Gender));
            Assert.That(firstContact.Phone, Is.EqualTo(newContact.Phone));
            Assert.That(firstContact.FirstName, Is.EqualTo(newContact.FirstName));
            Assert.That(firstContact.LastName, Is.EqualTo(newContact.LastName));
            Assert.That(firstContact.Email, Is.EqualTo(newContact.Email));
        }

        [Test]
        public async Task GetAllAsync_WhenNoContactsExist_ShouldThrowKeyNotFoundException()
        {
        
            // Act  and Assert
           var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.GetAllAsync());
            Assert.That(exception.Message, Is.EqualTo("No contact found."));
            
        }

        [Test]
        public async Task SearchByFirstNameAsync_WithExistingFirstName_ShouldReturnMatchingContacts()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", 
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };
            var secondNewContact = new Contact()
            {
                FirstName = "SecondTestFirstName",
                LastName = "SecondTestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "2ABC23456HH", 
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933778"
            };

            await contactManager.AddAsync(newContact);
            await contactManager.AddAsync(secondNewContact);
            // Act
            var result = await contactManager.SearchByFirstNameAsync(secondNewContact.FirstName);
            // Assert
           Assert.That(result.Count, Is.EqualTo(1)); 
            var itemInTheDb = result.First();
            Assert.That(itemInTheDb.LastName, Is.EqualTo(secondNewContact.LastName));
            Assert.That(itemInTheDb.FirstName, Is.EqualTo(secondNewContact.FirstName));
            Assert.That(itemInTheDb.Address, Is.EqualTo(secondNewContact.Address));
            Assert.That(itemInTheDb.Email, Is.EqualTo(secondNewContact.Email));
            Assert.That(itemInTheDb.Contact_ULID, Is.EqualTo(secondNewContact.Contact_ULID));
            Assert.That(itemInTheDb.Gender, Is.EqualTo(secondNewContact.Gender));
            Assert.That(itemInTheDb.Phone, Is.EqualTo(secondNewContact.Phone));



        }

        [Test]
        public async Task SearchByFirstNameAsync_WithNonExistingFirstName_ShouldThrowKeyNotFoundException()
        {
            // Act and  Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByFirstNameAsync("NO SUCH KEY"));
            Assert.That(exception.Message, Is.EqualTo("No contact found with the given first name."));
        }

        [Test]
        public async Task SearchByLastNameAsync_WithExistingLastName_ShouldReturnMatchingContacts()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };
            var secondNewContact = new Contact()
            {
                FirstName = "SecondTestFirstName",
                LastName = "SecondTestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "2ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933778"
            };

            await contactManager.AddAsync(newContact);
            await contactManager.AddAsync(secondNewContact);
            // Act
            var result = await contactManager.SearchByLastNameAsync(secondNewContact.LastName);
            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            var itemInTheDb = result.First();
            Assert.That(itemInTheDb.FirstName, Is.EqualTo(secondNewContact.FirstName));
            Assert.That(itemInTheDb.FirstName, Is.EqualTo(secondNewContact.FirstName));
            Assert.That(itemInTheDb.Address, Is.EqualTo(secondNewContact.Address));
            Assert.That(itemInTheDb.Email, Is.EqualTo(secondNewContact.Email));
            Assert.That(itemInTheDb.Contact_ULID, Is.EqualTo(secondNewContact.Contact_ULID));
            Assert.That(itemInTheDb.Gender, Is.EqualTo(secondNewContact.Gender));
            Assert.That(itemInTheDb.Phone, Is.EqualTo(secondNewContact.Phone));

        }

        [Test]
        public async Task SearchByLastNameAsync_WithNonExistingLastName_ShouldThrowKeyNotFoundException()
        {
            // Act and  Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByLastNameAsync("NON_EXISTING_NAME"));
            Assert.That(exception.Message, Is.EqualTo("No contact found with the given last name."));
        }

        [Test]
        public async Task GetSpecificAsync_WithValidULID_ShouldReturnContact()
        {
            // Arrange
            var newContacts = new List<Contact>() 
            {

               new Contact()
               {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
               },
               new Contact()
               {
                FirstName = "SecondTestFirstName",
                LastName = "SecondTestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "2ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933778"
               }
            };
            

           foreach( var contact in newContacts)
            {
                await  contactManager.AddAsync(contact);
            }

           //Act 
          var result = await contactManager.GetSpecificAsync(newContacts[1].Contact_ULID); // realno vtoria contact
           Assert.NotNull(result);
            Assert.That(result.FirstName, Is.EqualTo(newContacts[1].FirstName));  // it passes with [0] too

                    
        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidULID_ShouldThrowKeyNotFoundException()
        {
            // Act and  Assert
            const string invalidUlid = "NON_VALID_ID";

            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.GetSpecificAsync(invalidUlid));
            Assert.That(exception.Message, Is.EqualTo($"No contact found with ULID: {invalidUlid}"));
        }

        [Test]
        public async Task UpdateAsync_WithValidContact_ShouldUpdateContact()
        {
            // Arrange   da si napalnia bazata danni
            var newContacts = new List<Contact>()
            {

               new Contact()
               {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
               },
               new Contact()
               {
                FirstName = "SecondTestFirstName",
                LastName = "SecondTestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "2ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933778"
               }
            };


            foreach (var contact in newContacts)
            {
                await contactManager.AddAsync(contact);
            }
            var modifiedContact = newContacts[0];
            modifiedContact.FirstName = "UPDATED!";

            //Act 
            await contactManager.UpdateAsync(modifiedContact);// ne  vrashta nishto
           
            // Assert
            var itemInDb = await dbContext.Contacts.FirstOrDefaultAsync(x => x.Contact_ULID == modifiedContact.Contact_ULID);
            Assert.That(itemInDb.FirstName, Is.EqualTo(modifiedContact.FirstName)); 
            Assert.NotNull(itemInDb);
            Assert.That(itemInDb.LastName, Is.EqualTo(modifiedContact.LastName));
            Assert.That(itemInDb.Address, Is.EqualTo(modifiedContact.Address));
            Assert.That(itemInDb.Gender, Is.EqualTo(modifiedContact.Gender));
            Assert.That(itemInDb.Email, Is.EqualTo(modifiedContact.Email));
            Assert.That(itemInDb.Phone, Is.EqualTo(modifiedContact.Phone));
            Assert.That(itemInDb.Contact_ULID, Is.EqualTo(modifiedContact.Contact_ULID));
        }

        [Test]
        public async Task UpdateAsync_WithInvalidContact_ShouldThrowValidationException()
        {

            // Act and Assert
            var exception = Assert.ThrowsAsync<ValidationException>(() => contactManager.UpdateAsync(new Contact()));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Invalid contact!"));
            
        }
    }
}

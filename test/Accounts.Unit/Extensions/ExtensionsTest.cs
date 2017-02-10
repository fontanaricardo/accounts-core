using System.Linq;
using Accounts.Models;
using System;
using Xunit;

namespace Accounts.Unit.Extensions
{
    public class ExtensionsTest
    {
        [FactAttribute]
        public void ShouldGetDisplayName()
        {
            Person oldPerson = new Person {
                Name = "A",
                RG = "456"
            };

            Person newPerson = new Person {
                Name = "A",
                RG = "123"
            };

            var diff = newPerson.Diff<Person>(oldPerson);

            Assert.Equal(diff.First().Item1, "Identidade");
        }

        [FactAttribute]
        public void ShouldGetPropertyName()
        {
            var oldApplicationUser = new ApplicationUser {
                FullUserName = "bla"
            };

            var newApplicationUser = new ApplicationUser {
                FullUserName = "ble"
            };

            var diff = oldApplicationUser.Diff<ApplicationUser>(newApplicationUser);

            Assert.Equal(diff.First().Item1, "FullUserName");
        }

        [FactAttribute]
        public void ShouldGetCorrectValues()
        {
            Person oldPerson = new Person {
                Name = "A",
                RG = "456"
            };

            Person newPerson = new Person {
                Name = "A",
                RG = "123"
            };

            var diff = newPerson.Diff<Person>(oldPerson);

            Assert.Equal(diff.First().Item2, "456");
            Assert.Equal(diff.First().Item3, "123");
        }

        [FactAttribute]
        public void ShouldIgnoreKey()
        {
            Person oldPerson = new Person {
                PersonID = 1,
                Name = "A",
                RG = "456"
            };

            Person newPerson = new Person {
                PersonID = 2,
                Name = "A",
                RG = "123"
            };

            var diff = newPerson.Diff<Person>(oldPerson);

            Assert.False(diff.Any(d => d.Item1 == "PersonID"));
        }

        [FactAttribute]
        public void ShouldNotIgnoreKey()
        {
            Person oldPerson = new Person {
                PersonID = 1,
                Name = "A",
                RG = "456"
            };

            Person newPerson = new Person {
                PersonID = 2,
                Name = "A",
                RG = "123"
            };

            var diff = newPerson.Diff<Person>(oldPerson, false);

            Assert.True(diff.Any(d => d.Item1 == "PersonID"));
        }
    }
}